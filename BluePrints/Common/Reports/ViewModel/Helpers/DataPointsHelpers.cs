using BaseModel.Data.Helpers;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static BluePrints.Data.BluePrintsEntities;

namespace BluePrints.Common.ViewModel.Reporting
{
    public static class DataPointsHelpers
    {
        #region Cumulative Calculation Methods
        /// <summary>
        /// Populate summary data point for reporting S-Curve
        /// </summary>
        public static ObservableCollection<DataPoint> GroupDataPointsByPeriod(
            IEnumerable<DataPoint> rawDataPoints, decimal budgetedUnits,
            decimal budgetedCosts, decimal qtyPerUnit, DateTime firstAlignedDataDate, TimeSpan progressInterval, Guid aggregateGuid, 
            IEnumerable<VariationAdjustment> rawVariationAdjustments = null, DateTime? overrideLastPeriodDate = null, bool isRemaining = false)
                        {
            if (rawDataPoints == null || rawDataPoints.Count() == 0)
                return null;

            List<DataPoint> filteredRawDataPoints;
            if (isRemaining)
                filteredRawDataPoints = rawDataPoints.Where(x => x.IsRemaining).ToList();
            else
                filteredRawDataPoints = rawDataPoints.ToList();

            var summaryDataPoints = new ObservableCollection<DataPoint>();
            if (filteredRawDataPoints.Count() == 0)
                return summaryDataPoints;

            Guid deliverableGuid = filteredRawDataPoints.First().DeliverableGuid;
            DateTime progressLastDataDate;
            //In progress distribution we want to generate data points even if P6 or subjob says it's finished. i.e. 100% all the way until data date
            if (overrideLastPeriodDate != null)
                progressLastDataDate = (DateTime)overrideLastPeriodDate;
            else
                progressLastDataDate = filteredRawDataPoints.Max(dataPoint => dataPoint.ProgressDate);

            DateTime zeroUnitsDataDate = firstAlignedDataDate.AddDays(-1 * progressInterval.Days);
            //Add zero UOM data point so that line graph starts at 0%
            summaryDataPoints.Add(new DataPoint()
            {
                DeliverableGuid = deliverableGuid,
                TotalUnits = budgetedUnits,
                 TotalCosts = budgetedCosts,
                BudgetedUnits = budgetedUnits,
                BudgetedCosts = budgetedCosts,
                Units = 0,
                Costs = 0,
                Quantity = 0,
                ProgressDate = zeroUnitsDataDate.AddDays(-1 * progressInterval.Days),
                IsRemaining = isRemaining
            });

            //Start going through each progress items to retrieve cumulative data point per period
            var scanDate = zeroUnitsDataDate;
            decimal cumulativeUnits = 0;
            decimal cumulativeCosts = 0;
            decimal cumulativeAdjustmentUnits = 0;
            decimal cumulativeAdjustmentCosts = 0;
            while (scanDate < progressLastDataDate)
            {
                List<DataPoint> currentPeriodDataPoints;
                List<VariationAdjustment> currentPeriodVariationAdjustments;

                DateTime floorDate = scanDate;
                DateTime ceilingDate = scanDate.AddDays(progressInterval.Days);
                if (floorDate == zeroUnitsDataDate)
                {
                    currentPeriodDataPoints =
                        filteredRawDataPoints.Where(
                            DataPoint => DataPoint.ProgressDate <= ceilingDate).ToList();
                    currentPeriodVariationAdjustments = rawVariationAdjustments == null
                        ? new List<VariationAdjustment>()
                        : rawVariationAdjustments.Where(
                            Adjustment => Adjustment.AdjustmentDate <= ceilingDate).ToList();
                }
                else
                {
                    currentPeriodDataPoints =
                        filteredRawDataPoints.Where(
                            DataPoint =>
                                DataPoint.ProgressDate > floorDate &&
                                DataPoint.ProgressDate <= ceilingDate).ToList();
                    currentPeriodVariationAdjustments = rawVariationAdjustments == null
                        ? new List<VariationAdjustment>()
                        : rawVariationAdjustments.Where(Adjustment => Adjustment.AdjustmentDate > floorDate &&
                                Adjustment.AdjustmentDate <= ceilingDate).ToList();
                }

                decimal currentPeriodUnits = currentPeriodDataPoints.Sum(dataPoint => dataPoint.Units);
                decimal currentPeriodCosts = currentPeriodDataPoints.Sum(dataPoint => dataPoint.Costs);
                decimal currentPeriodAdjustmentUnits = currentPeriodVariationAdjustments.Sum(adjustment => adjustment.AdjustmentUnits);
                decimal currentPeriodAdjustmentCosts = currentPeriodVariationAdjustments.Sum(adjustment => adjustment.AdjustmentNativeCosts);

                cumulativeUnits += currentPeriodUnits;
                cumulativeCosts += currentPeriodCosts;

                cumulativeAdjustmentUnits += currentPeriodAdjustmentUnits;
                cumulativeAdjustmentCosts += currentPeriodAdjustmentCosts;
                    
                summaryDataPoints.Add(new DataPoint()
                {
                    DeliverableGuid = deliverableGuid,
                    TotalUnits = budgetedUnits + cumulativeAdjustmentUnits,
                    TotalCosts = budgetedCosts + cumulativeAdjustmentCosts,
                    BudgetedUnits = budgetedUnits + cumulativeAdjustmentUnits,
                    BudgetedCosts = budgetedCosts + cumulativeAdjustmentCosts,
                    Units = cumulativeUnits,
                    Costs = cumulativeCosts,
                    Quantity = cumulativeUnits * qtyPerUnit,
                    ProgressDate = ceilingDate,
                    IsRemaining = isRemaining
                });

                if (currentPeriodAdjustmentUnits > 0)
                {
                    DataPoint lastDataPoint = summaryDataPoints.Last();
                    if (lastDataPoint != null)
                    {
                        //for sharktooth effect, add dip on a day before the adjustment's week ending occurs
                        //this means that a day before, the percentage would be higher if it weren't for the variation adjustments
                        DataPoint newDataPoint = new DataPoint();
                        DataUtils.ShallowCopy(newDataPoint, lastDataPoint);
                        newDataPoint.ProgressDate = scanDate.AddDays(-1);
                        newDataPoint.BudgetedUnits -= currentPeriodAdjustmentUnits;
                        newDataPoint.BudgetedCosts -= currentPeriodAdjustmentCosts;
                        summaryDataPoints.Add(newDataPoint);
                    }
                }

                scanDate = ceilingDate;
            }

            return summaryDataPoints;
        }

        /// <summary>
        /// Populate remaining summary data point for reporting S-Curve
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<DataPoint> PopulateCumulativeRemainingSummaryDataPoints(
            ObservableCollection<DataPoint> flatDataPointsBeforeDataDate,
            ObservableCollection<VariationAdjustment> variationAdjustments,
            ObservableCollection<DataPoint> flatDataPointsAfterDataDate, decimal totalBudgetedUnits,
            decimal totalBudgetedCosts, DateTime firstAlignedDataDate, TimeSpan progressInterval, Guid aggregateGuid)
        {
            if (flatDataPointsBeforeDataDate == null || flatDataPointsBeforeDataDate.Count == 0 ||
                flatDataPointsAfterDataDate == null || flatDataPointsAfterDataDate.Count == 0)
                return null;

            var collectionEarnedUnits = flatDataPointsBeforeDataDate.Sum(obj => obj.Units);
            var collectionEarnedUnitsAfter = flatDataPointsAfterDataDate.Sum(obj => obj.Units);

            var summaryDataPoints = new ObservableCollection<DataPoint>();
            var remainingFirstDataDate = flatDataPointsAfterDataDate.Min(dataPoint => dataPoint.ProgressDate);
            var remainingLastDataDate = flatDataPointsAfterDataDate.Max(dataPoint => dataPoint.ProgressDate);

            //Add zero UOM data point so that line graph starts at 0%
            summaryDataPoints.Add(new DataPoint()
            {
                BudgetedUnits = totalBudgetedUnits,
                BudgetedCosts = totalBudgetedCosts,
                Units = 0,
                Costs = 0,
                ProgressDate = firstAlignedDataDate.AddDays(-1 * progressInterval.Days)
            });

            //Start going through each progress items to retrieve cumulative data point per period
            var scanDate = firstAlignedDataDate;
            decimal individualPeriodCumulativeUnits = 0;
            decimal individualPeriodCumulativeCosts = 0;
            decimal cumulativeAdjustmentUnits = 0;
            decimal cumulativeAdjustmentCosts = 0;
            var executedDataDatePoints = true;

            while (scanDate.Date <= remainingLastDataDate)
            {
                List<DataPoint> progressItemScanDateDataPoints;
                List<VariationAdjustment> progressItemAdjustments;

                if (scanDate == firstAlignedDataDate)
                {
                    progressItemScanDateDataPoints =
                        flatDataPointsBeforeDataDate.Where(
                            DataPoint => DataPoint.ProgressDate < scanDate.AddDays(progressInterval.Days)).ToList();
                    progressItemAdjustments = variationAdjustments == null
                        ? new List<VariationAdjustment>()
                        : variationAdjustments.Where(
                            adjustment => adjustment.AdjustmentDate < scanDate.AddDays(progressInterval.Days)).ToList();
                }
                else if (scanDate >= remainingFirstDataDate)
                {
                    progressItemScanDateDataPoints =
                        flatDataPointsAfterDataDate.Where(
                            DataPoint =>
                                !executedDataDatePoints && DataPoint.ProgressDate.Date == remainingFirstDataDate.Date ||
                                DataPoint.ProgressDate >= scanDate &&
                                DataPoint.ProgressDate < scanDate.AddDays(progressInterval.Days)).ToList();
                    progressItemAdjustments = variationAdjustments == null
                        ? new List<VariationAdjustment>()
                        : variationAdjustments.Where(
                            adjustment =>
                                !executedDataDatePoints &&
                                adjustment.AdjustmentDate.Date == remainingFirstDataDate.Date ||
                                adjustment.AdjustmentDate >= scanDate &&
                                adjustment.AdjustmentDate < scanDate.AddDays(progressInterval.Days)).ToList();
                    executedDataDatePoints = true;
                }
                else
                {
                    progressItemScanDateDataPoints =
                        flatDataPointsBeforeDataDate.Where(
                            DataPoint =>
                                DataPoint.ProgressDate >= scanDate &&
                                DataPoint.ProgressDate < scanDate.AddDays(progressInterval.Days)).ToList();
                    progressItemAdjustments = variationAdjustments == null
                        ? new List<VariationAdjustment>()
                        : variationAdjustments.Where(adjustment => adjustment.AdjustmentDate >= scanDate &&
                                adjustment.AdjustmentDate < scanDate.AddDays(progressInterval.Days)).ToList();
                }

                var incrementUnits = progressItemScanDateDataPoints.Sum(dataPoint => dataPoint.Units);
                var variationUnits = progressItemAdjustments.Sum(adjustment => adjustment.AdjustmentUnits);
                var variationCosts = progressItemAdjustments.Sum(adjustment => adjustment.AdjustmentNativeCosts);
                if (variationUnits > 0)
                {
                    cumulativeAdjustmentUnits += variationUnits;
                    cumulativeAdjustmentCosts += variationCosts;

                    //for sharktooth effect, add dip on a day after the previous period
                    summaryDataPoints.Add(new DataPoint()
                    {
                        BudgetedUnits = totalBudgetedUnits + cumulativeAdjustmentUnits,
                        BudgetedCosts = totalBudgetedCosts + cumulativeAdjustmentCosts,
                        Units = individualPeriodCumulativeUnits,
                        Costs = individualPeriodCumulativeCosts,
                        ProgressDate = scanDate.AddDays(-1 * progressInterval.Days).AddDays(1)
                    });
                }

                if (incrementUnits > 0)
                {
                    individualPeriodCumulativeUnits += progressItemScanDateDataPoints.Sum(dataPoint => dataPoint.Units);
                    individualPeriodCumulativeCosts += progressItemScanDateDataPoints.Sum(dataPoint => dataPoint.Costs);

                    summaryDataPoints.Add(new DataPoint()
                    {
                        BudgetedUnits = totalBudgetedUnits + cumulativeAdjustmentUnits,
                        BudgetedCosts = totalBudgetedCosts + cumulativeAdjustmentCosts,
                        Units = individualPeriodCumulativeUnits,
                        Costs = individualPeriodCumulativeCosts,
                        ProgressDate = scanDate
                    });
                }

                scanDate = scanDate.AddDays(progressInterval.Days);
            }

            return summaryDataPoints;
        }
        #endregion

        /// <summary>
        /// Search for specific datapoint within a collection of data points by specific date
        /// </summary>
        /// <param name="progressDataPoints">Data points collection to search</param>
        /// <param name="dataPointDate">Date to retrieve data point</param>
        /// <returns>Data point on the particular date</returns>
        public static DataPoint FindPeriodDataPointInDataPoints(ObservableCollection<DataPoint> progressDataPoints,
            DateTime dataPointDate)
        {
            if (progressDataPoints == null || progressDataPoints.Count == 0)
                return nullProgressDataPoint;

            //use last of default because during Earned list generation the current earned is added to the end of the list
            //this is necessary for the first period when earned contains a zero value data point and an actual value data point
            var specificDateDataPoint = progressDataPoints.OrderBy(x => x.ProgressDate).LastOrDefault(obj => obj.ProgressDate.Date <= dataPointDate.Date);

            return specificDateDataPoint;
        }

        /// <summary>
        /// Searches for the period percentage on a collection of data points by specific date
        /// </summary>
        /// <param name="progressDataPoints">Data points collection to search</param>
        /// <param name="dataPointDate">Date to retrieve data point</param>
        /// <returns>Period period datapoint on the particular date</returns>
        public static DataPoint FindPeriodDataPointInCumulativeDataPoints(
            ObservableCollection<DataPoint> progressDataPoints, DateTime dataPointDate)
        {
            if (progressDataPoints == null || progressDataPoints.Count == 0)
                return nullProgressDataPoint;

            //use last or default because there will be an extra data point added after data date for variation adjustments
            var CumulativeProgressOnDataDate = progressDataPoints.LastOrDefault(obj => obj.ProgressDate.Date == dataPointDate.Date);

            if (CumulativeProgressOnDataDate != null)
            {
                if (!CumulativeProgressOnDataDate.DoNotPlot)
                {
                    var CurrentPeriodIndex = progressDataPoints.IndexOf(CumulativeProgressOnDataDate);
                    if (CurrentPeriodIndex == 0)
                    {
                        return nullProgressDataPoint;
                    }
                    else
                    {
                        var PreviousPeriodIndex = CurrentPeriodIndex - 1;
                        var CumulativeProgressOnDataDatePrevious = progressDataPoints[PreviousPeriodIndex];
                        return new DataPoint()
                        {
                            BudgetedUnits = CumulativeProgressOnDataDate.BudgetedUnits,
                            BudgetedCosts = CumulativeProgressOnDataDate.BudgetedCosts,
                            TotalUnits = CumulativeProgressOnDataDate.TotalUnits,
                            TotalCosts = CumulativeProgressOnDataDate.TotalCosts,
                            Units = CumulativeProgressOnDataDate.Units - CumulativeProgressOnDataDatePrevious.Units,
                            Costs = CumulativeProgressOnDataDate.Costs - CumulativeProgressOnDataDatePrevious.Costs,
                            Quantity = CumulativeProgressOnDataDate.Quantity - CumulativeProgressOnDataDatePrevious.Quantity,
                            ProgressDate = CumulativeProgressOnDataDate.ProgressDate
                        };
                    }
                }
                else
                    return nullProgressDataPoint;

            }
            else
            {
                return nullProgressDataPoint;
            }
        }

        /// <summary>
        /// Convert cumulative summary collection to period summary for bar histogram construction
        /// </summary>
        public static ObservableCollection<DataPoint> ConvertCumulativeToPeriodDataPoint(
            ObservableCollection<DataPoint> CumulativeDataPointCollection, decimal qtyPerUnit, DateTime? plotStartDate = null, List<ExoDataPoint> exoDataPoints = null)
        {
            decimal periodUnits = 0;
            decimal periodCosts = 0;

            var PeriodDataPointCollection = new ObservableCollection<DataPoint>();

            if (CumulativeDataPointCollection != null)
                for (var i = 0; i < CumulativeDataPointCollection.Count; i++)
                {
                    DateTime? exoStartDateRange = null;
                    DateTime? exoEndDateRange = null;

                    if (plotStartDate != null)
                        if(CumulativeDataPointCollection[i].ProgressDate <= plotStartDate)
                            continue;

                    if (i == 0)
                    {
                        periodUnits = CumulativeDataPointCollection[i].Units;
                        periodCosts = CumulativeDataPointCollection[i].Costs;
                        exoStartDateRange = CumulativeDataPointCollection[i].ProgressDate;
                    }
                    else
                    {
                        periodUnits = CumulativeDataPointCollection[i].Units - CumulativeDataPointCollection[i - 1].Units;
                        periodCosts = CumulativeDataPointCollection[i].Costs - CumulativeDataPointCollection[i - 1].Costs;

                        exoStartDateRange = CumulativeDataPointCollection[i - 1].ProgressDate;
                        exoEndDateRange = CumulativeDataPointCollection[i].ProgressDate;
                    }

                    List<ExoDataPoint> groupedExoDataPoints = new List<ExoDataPoint>();
                    if(exoDataPoints.Count > 0)
                    {
                        groupedExoDataPoints.AddRange(exoDataPoints.Where(x => x.ActualDate > exoStartDateRange && x.ActualDate <= exoEndDateRange));
                    }

                    PeriodDataPointCollection.Add(new DataPoint
                    {
                        BudgetedCosts = CumulativeDataPointCollection[i].BudgetedCosts,
                        BudgetedUnits = CumulativeDataPointCollection[i].BudgetedUnits,
                        Costs = periodCosts,
                        Units = periodUnits,
                        Quantity = periodUnits * qtyPerUnit,
                        //Costs = periodCosts < 0 ? 0 : periodCosts,
                        //Units = periodUnits < 0 ? 0 : periodUnits,
                        ProgressDate = CumulativeDataPointCollection[i].ProgressDate,
                        RawExoData = groupedExoDataPoints
                    });
                }

            return PeriodDataPointCollection;
        }
        public static IEnumerable<DataPoint> ConvertDbPlannedDataPointToReportingDataPoints(IEnumerable<Data.DataPoint> deliverablesDataPoints)
        {
            List<DataPoint> progressInfoConversion = new List<DataPoint>();
            foreach (Data.DataPoint deliverablesDataPoint in deliverablesDataPoints)
            {
                progressInfoConversion.Add(new DataPoint
                {
                    DeliverableGuid = deliverablesDataPoint.Deliverable_Guid,
                    BudgetedUnits = 0,
                    BudgetedCosts = 0,
                    Costs = Convert.ToDecimal(deliverablesDataPoint.PeriodPrice),
                    Units = Convert.ToDecimal(deliverablesDataPoint.PeriodUnits),
                    Quantity = Convert.ToDecimal(deliverablesDataPoint.PeriodQuantity),

                    ProgressDate = deliverablesDataPoint.UniversalPeriodEndDate,
                    IsFromP6 = deliverablesDataPoint.IsFromP6,
                    RemainingDuration = deliverablesDataPoint.RemainingDuration == null ? (decimal?)null : Convert.ToDecimal(deliverablesDataPoint.RemainingDuration)
                });
            }

            return progressInfoConversion;
        }

        public static IEnumerable<DataPoint> ConvertDbRemainingDataPointToReportingDataPoints(IEnumerable<Data.DataPoint> deliverablesDataPoints)
        {
            List<DataPoint> progressInfoConversion = new List<DataPoint>();
            foreach (Data.DataPoint deliverablesDataPoint in deliverablesDataPoints)
            {
                progressInfoConversion.Add(new DataPoint
                {
                    DeliverableGuid = deliverablesDataPoint.Deliverable_Guid,
                    BudgetedUnits = 0,
                    BudgetedCosts = 0,
                    Costs = Convert.ToDecimal(deliverablesDataPoint.PeriodPrice),
                    Units = Convert.ToDecimal(deliverablesDataPoint.PeriodUnits),

                    ProgressDate = deliverablesDataPoint.UniversalPeriodEndDate,
                    IsFromP6 = deliverablesDataPoint.IsFromP6,
                    RemainingDuration = deliverablesDataPoint.RemainingDuration == null ? (decimal?)null : Convert.ToDecimal(deliverablesDataPoint.RemainingDuration),
                    IsRemaining = true
                });
            }

            return progressInfoConversion;
        }

        public static DataPoint nullProgressDataPoint = new DataPoint()
        {
            BudgetedCosts = 0,
            BudgetedUnits = 0,
            Costs = 0,
            Units = 0
        };
    }
}
