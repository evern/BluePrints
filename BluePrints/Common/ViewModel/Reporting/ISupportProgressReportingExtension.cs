using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Mvvm.POCO;
using BluePrints.Common.Projections;

namespace BluePrints.Common.ViewModel.Reporting
{
    /// <summary>
    /// Provides a set of extension methods to perform commonly used operations with ISupportProgressReporting.
    /// </summary>
    public static class ISupportProgressReportingExtensions
    {
        /// <summary>
        /// Default exception periods, may be replaced by calendar settings
        /// </summary>
        public static List<Period> NonWorkingPeriods
        {
            get
            {
                var XmasStart = new DateTime(2015, 12, 21);
                var XmasEnd = new DateTime(2016, 1, 3);
                var nonworkingperiod = new List<Period>();
                nonworkingperiod.Add(new Period(XmasStart.Date, XmasEnd.Date));
                return nonworkingperiod;
            }
        }

        #region Reportables Parameter Calculation

        public static void InitializePROGRESS_ITEMStats(IEnumerable<PROGRESS_ITEMProjection> PROGRESS_ITEMS, IEnumerable<VariationAdjustment> variationAdjustments, PROGRESS livePROGRESS, bool progressHaveStats)
        {
            foreach (PROGRESS_ITEMProjection progressItem in PROGRESS_ITEMS)
            {
                List<VariationAdjustment> currentProgressItemAdjustments = variationAdjustments.Where(x => x.DeliverableOriginalGuid == progressItem.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_ORIGINAL).ToList();
                if(!progressHaveStats)
                    progressItem.Stats = new ProgressStats(livePROGRESS, progressItem.BASELINE_ITEMJoinRATE.BASELINE_ITEM.ESTIMATED_HOURS, progressItem.BASELINE_ITEMJoinRATE.BASELINE_ITEM.TOTAL_HOURS, progressItem.BASELINE_ITEMJoinRATE.ESTIMATED_COSTS, progressItem.BASELINE_ITEMJoinRATE.TOTAL_COSTS, currentProgressItemAdjustments);
            }
        }

        public static List<VariationAdjustment> BuildProjectVariationAdjustments(IQueryable<VARIATION> VARIATION, IEnumerable<BASELINE_ITEMProjection> BASELINE_ITEM)
        {
            List<VariationAdjustment> variationAdjustments = new List<VariationAdjustment>();
            IQueryable<VARIATION> ApprovedVARIATION = VARIATION.Where(x => x.APPROVED != null && x.TYPE == VariationType.External);
            foreach (VARIATION variation in ApprovedVARIATION)
            {
                IEnumerable<VARIATION_ITEM> applicableVariation_Item = variation.VARIATION_ITEM.Where(x => x.ACTION == VariationAction.Add || x.ACTION == VariationAction.Append);
                foreach (VARIATION_ITEM variation_item in applicableVariation_Item)
                {
                    BASELINE_ITEMProjection findBASELINE_ITEM = BASELINE_ITEM.FirstOrDefault(x => x.BASELINE_ITEM.GUID_ORIGINAL == variation_item.GUID_ORIBASEITEM);
                    if (findBASELINE_ITEM != null)
                    {
                        variationAdjustments.Add(new VariationAdjustment(variation_item.GUID_ORIBASEITEM) { AdjustmentDate = (DateTime)variation.APPROVED, AdjustmentUnits = variation_item.VARIATION_UNITS, AdjustmentRate = findBASELINE_ITEM.ITEMRATE });
                    }
                }
            }

            return variationAdjustments;
        }
        
        /// <summary>
        /// Calculates productivity
        /// </summary>
        /// <param name="workingPeriod">Period to calculate the productivity</param>
        /// <param name="exceptionPeriod">Periods to not calculate the productivity</param>
        /// <param name="totalUnits">Total units to spread across the period</param>
        /// <returns>Calculated productivity</returns>
        public static decimal CalculatePlannedProductivity(Period workingPeriod, IEnumerable<Period> exceptionPeriod,
            decimal totalUnits, decimal exceptionProductivity = 0)
        {
            if (workingPeriod.StartDate.Date > workingPeriod.EndDate.Date || totalUnits == 0)
                return exceptionProductivity;

            var countDate = workingPeriod.StartDate.Date;
            decimal workingPeriodInDays = 0;
            do
            {
                //if dates are not between
                if (
                    !exceptionPeriod.Any(
                        dates => dates.StartDate.Date <= countDate.Date && countDate.Date <= dates.EndDate.Date))
                    workingPeriodInDays += 1;

                countDate = countDate.AddDays(1);
            } while (countDate <= workingPeriod.EndDate.Date);

            if (workingPeriodInDays == 0)
                return 0;

            var earnedHours = workingPeriodInDays * int.Parse(CommonResources.ProgressReporting_DefaultHoursADay);
            var productivity = totalUnits / earnedHours;

            return productivity;
        }

        /// <summary>
        /// Calculates the data date backwards to get the first aligned data date as per the project start date
        /// </summary>
        public static DateTime GenerateFirstAlignedDataDate(PROGRESS principalProgress, TimeSpan? periodInterval = null)
        {
            if (periodInterval == null)
                periodInterval = ConvertProgressIntervalToPeriod(principalProgress);

            var intervalPeriod = (TimeSpan) periodInterval;
            var firstProgressDate = principalProgress.DATA_DATE;

            //rewind the first progress date to scan to before the datadate aligned to startdate day of week
            while (firstProgressDate.AddDays(-1 * intervalPeriod.Days) >
                   principalProgress.PROGRESS_START.Date.AddSeconds(-1))
                firstProgressDate = firstProgressDate.AddDays(-1 * intervalPeriod.Days);

            return firstProgressDate;
        }

        /// <summary>
        /// Calculates the data date forward to get the last aligned data date as per the first aligned data date
        /// </summary>
        public static List<DateTime> GenerateAlignedDatesCollection(DateTime firstAlignedDataDate,
            DateTime lastDataPointDate, TimeSpan intervalPeriod)
        {
            var lastProgressDate = firstAlignedDataDate;
            lastDataPointDate = lastDataPointDate.AddDays(intervalPeriod.Days);
            var alignedDataDatesCollection = new List<DateTime>();
            alignedDataDatesCollection.Add(firstAlignedDataDate);
            //forward the first progress date to scan to after the datadate aligned to end day of week
            do
            {
                lastProgressDate = lastProgressDate.AddDays(intervalPeriod.Days);
                alignedDataDatesCollection.Add(lastProgressDate);
            } while (lastProgressDate < lastDataPointDate);

            return alignedDataDatesCollection;
        }

        /// <summary>
        /// Calculates the aligned data date for workpack start date, used for remaining datapoints calculation
        /// </summary>
        public static DateTime GenerateWorkpackAlignedDataDate(DateTime firstAlignedDataDate, DateTime workpackStartDate,
            TimeSpan intervalPeriod)
        {
            var weekEndingAlignedDataDate = firstAlignedDataDate;

            do
            {
                weekEndingAlignedDataDate = weekEndingAlignedDataDate.AddDays(intervalPeriod.Days);
            } while (weekEndingAlignedDataDate <= workpackStartDate);

            return weekEndingAlignedDataDate;
        }

        public static TimeSpan ConvertProgressIntervalToPeriod(PROGRESS PROGRESS)
        {
            int intervalCount = PROGRESS.INTERVAL_COUNT;
            if (intervalCount == 0)
                intervalCount = 1;

            TimeSpan intervalPeriod = TimeSpan.FromDays((int)PROGRESS.INTERVAL_TYPE * intervalCount);
            return intervalPeriod;
        }

        public static void SetWorkpackAssignmentStartUnit(IEnumerable<PROGRESS_ITEMProjection> progressStats)
        {
            Dictionary<Guid, decimal> workpackP6AssignedUnits = new Dictionary<Guid, decimal>();
            progressStats = progressStats.OrderBy(x => x.BASELINE_ITEMJoinRATE.BASELINE_ITEM.INTERNAL_NUM != null);
            foreach(PROGRESS_ITEMProjection progressStat in progressStats)
            {
                Guid? currentWORKPACKGuid = progressStat.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_WORKPACK;
                if (currentWORKPACKGuid == null)
                    continue;

                var assignedWorkpack = workpackP6AssignedUnits.Where(x => x.Key == currentWORKPACKGuid)
                    .Select(e => (KeyValuePair<Guid, decimal>?)e).FirstOrDefault();

                decimal workpackAssignmentStartUnit = 1;
                if (assignedWorkpack != null)
                {
                    workpackAssignmentStartUnit = ((KeyValuePair<Guid, decimal>)assignedWorkpack).Value;
                    workpackP6AssignedUnits.Remove(((KeyValuePair<Guid, decimal>)assignedWorkpack).Key);
                }

                progressStat.SetWorkpackAssignmentStartUnit(workpackAssignmentStartUnit);
                //move assignment start unit by total hours for next start unit assignment
                workpackAssignmentStartUnit += progressStat.BASELINE_ITEMJoinRATE.BASELINE_ITEM.TOTAL_HOURS;
                workpackP6AssignedUnits.Add((Guid)currentWORKPACKGuid, workpackAssignmentStartUnit);
            }
        }
        #endregion

        #region Cumulative Calculation Methods

        /// <summary>
        /// Populate summary data point for reporting S-Curve
        /// </summary>
        public static ObservableCollection<DataPoint> GroupDataPointsByPeriod(
            IEnumerable<DataPoint> rawDataPoints, decimal budgetedUnits,
            decimal budgetedCosts, DateTime firstAlignedDataDate, TimeSpan progressInterval, Guid aggregateGuid,
            IEnumerable<VariationAdjustment> rawVariationAdjustments = null)
        {
            if (rawDataPoints == null || rawDataPoints.Count() == 0)
                return null;

            var summaryDataPoints = new ObservableCollection<DataPoint>();
            var progressLastDataDate = rawDataPoints.Max(dataPoint => dataPoint.ProgressDate);

            //Add zero UOM data point so that line graph starts at 0%
            summaryDataPoints.Add(new DataPoint()
            {
                BudgetedUnits = budgetedUnits,
                BudgetedCosts = budgetedCosts,
                Units = 0,
                Costs = 0,
                ProgressDate = firstAlignedDataDate.AddDays(-1 * progressInterval.Days)
            });

            //Start going through each progress items to retrieve cumulative data point per period
            var scanDate = firstAlignedDataDate;
            decimal cumulativeUnits = 0;
            decimal cumulativeCosts = 0;
            decimal cumulativeAdjustmentUnits = 0;
            decimal cumulativeAdjustmentCosts = 0;
            while (scanDate <= progressLastDataDate)
            {
                List<DataPoint> currentPeriodDataPoints;
                List<VariationAdjustment> currentPeriodVariationAdjustments;

                if (scanDate == firstAlignedDataDate)
                {
                    currentPeriodDataPoints =
                        rawDataPoints.Where(
                            DataPoint => DataPoint.ProgressDate < scanDate.AddDays(progressInterval.Days)).ToList();
                    currentPeriodVariationAdjustments = rawVariationAdjustments == null
                        ? new List<VariationAdjustment>()
                        : rawVariationAdjustments.Where(
                            Adjustment => Adjustment.AdjustmentDate < scanDate.AddDays(progressInterval.Days)).ToList();
                }
                else
                {
                    currentPeriodDataPoints =
                        rawDataPoints.Where(
                            DataPoint =>
                                DataPoint.ProgressDate >= scanDate &&
                                DataPoint.ProgressDate < scanDate.AddDays(progressInterval.Days)).ToList();
                    currentPeriodVariationAdjustments = rawVariationAdjustments == null
                        ? new List<VariationAdjustment>()
                        : rawVariationAdjustments.Where(Adjustment => Adjustment.AdjustmentDate >= scanDate &&
                                Adjustment.AdjustmentDate < scanDate.AddDays(progressInterval.Days)).ToList();
                }

                decimal currentPeriodUnits = currentPeriodDataPoints.Sum(dataPoint => dataPoint.Units);
                decimal currentPeriodCosts = currentPeriodDataPoints.Sum(dataPoint => dataPoint.Costs);
                decimal currentPeriodAdjustmentUnits = currentPeriodVariationAdjustments.Sum(adjustment => adjustment.AdjustmentUnits);
                decimal variationCosts = currentPeriodVariationAdjustments.Sum(adjustment => adjustment.AdjustmentNativeCosts);

                if (currentPeriodAdjustmentUnits > 0)
                {
                    cumulativeAdjustmentUnits += currentPeriodAdjustmentUnits;
                    cumulativeAdjustmentCosts += variationCosts;

                    //for sharktooth effect, add dip on a day after the previous period
                    summaryDataPoints.Add(new DataPoint()
                    {
                        BudgetedUnits = budgetedUnits + cumulativeAdjustmentUnits,
                        BudgetedCosts = budgetedCosts + cumulativeAdjustmentCosts,
                        Units = cumulativeUnits,
                        Costs = cumulativeCosts,
                        ProgressDate = scanDate.AddDays(-1 * progressInterval.Days).AddDays(1)
                    });
                }

                if (currentPeriodUnits > 0)
                {
                    cumulativeUnits += currentPeriodUnits;
                    cumulativeCosts += currentPeriodCosts;

                    summaryDataPoints.Add(new DataPoint()
                    {
                        BudgetedUnits = budgetedUnits + cumulativeAdjustmentUnits,
                        BudgetedCosts = budgetedCosts + cumulativeAdjustmentCosts,
                        Units = cumulativeUnits,
                        Costs = cumulativeCosts,
                        ProgressDate = scanDate
                    });
                }

                scanDate = scanDate.AddDays(progressInterval.Days);
            }

            decimal totalUnits = summaryDataPoints.Sum(x => x.Units);
            string s = totalUnits.ToString();
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

        #region OnDemand Calculation

        public static DataPoint nullProgressDataPoint = new DataPoint()
        {
            BudgetedCosts = 0,
            BudgetedUnits = 0,
            Costs = 0,
            Units = 0
        };

        /// <summary>
        /// Search for specific datapoint within a collection of data points by specific date
        /// </summary>
        /// <param name="progressDataPoints">Data points collection to search</param>
        /// <param name="dataPointDate">Date to retrieve data point</param>
        /// <returns>Data point on the particular date</returns>
        public static DataPoint FindDataPointByDate(ObservableCollection<DataPoint> progressDataPoints,
            DateTime dataPointDate)
        {
            if (progressDataPoints == null || progressDataPoints.Count == 0)
                return nullProgressDataPoint;

            //use last of default because during Earned list generation the current earned is added to the end of the list
            //this is necessary for the first period when earned contains a zero value data point and an actual value data point
            var specificDateDataPoint =
                progressDataPoints.LastOrDefault(obj => obj.ProgressDate.Date <= dataPointDate.Date);

            return specificDateDataPoint;
        }

        /// <summary>
        /// Searches for the period percentage on a collection of data points by specific date
        /// </summary>
        /// <param name="progressDataPoints">Data points collection to search</param>
        /// <param name="dataPointDate">Date to retrieve data point</param>
        /// <returns>Period period datapoint on the particular date</returns>
        public static DataPoint GeneratePeriodDataPointFromCumulative(
            ObservableCollection<DataPoint> progressDataPoints, DateTime dataPointDate)
        {
            if (progressDataPoints == null || progressDataPoints.Count == 0)
                return nullProgressDataPoint;

            var CumulativeProgressOnDataDate =
                progressDataPoints.FirstOrDefault(obj => obj.ProgressDate.Date == dataPointDate.Date);
            if (CumulativeProgressOnDataDate != null)
            {
                var CurrentPeriodIndex = progressDataPoints.IndexOf(CumulativeProgressOnDataDate);
                if (CurrentPeriodIndex == 0)
                {
                    return null;
                }
                else
                {
                    var PreviousPeriodIndex = CurrentPeriodIndex - 1;
                    var CumulativeProgressOnDataDatePrevious = progressDataPoints[PreviousPeriodIndex];
                    return new DataPoint()
                    {
                        BudgetedUnits = CumulativeProgressOnDataDate.BudgetedUnits,
                        BudgetedCosts = CumulativeProgressOnDataDate.BudgetedCosts,
                        Units = CumulativeProgressOnDataDate.Units - CumulativeProgressOnDataDatePrevious.Units,
                        Costs = CumulativeProgressOnDataDate.Costs - CumulativeProgressOnDataDatePrevious.Costs,
                        ProgressDate = CumulativeProgressOnDataDate.ProgressDate
                    };
                }
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
            ObservableCollection<DataPoint> CumulativeDataPointCollection, DateTime? plotStartDate = null)
        {
            decimal periodUnits = 0;
            decimal periodCosts = 0;

            var PeriodDataPointCollection = new ObservableCollection<DataPoint>();

            if (CumulativeDataPointCollection != null)
                for (var i = 0; i < CumulativeDataPointCollection.Count; i++)
                {
                    if (plotStartDate != null && CumulativeDataPointCollection[i].ProgressDate <= plotStartDate)
                        continue;

                    if (i == 0)
                    {
                        periodUnits = CumulativeDataPointCollection[i].Units;
                        periodCosts = CumulativeDataPointCollection[i].Costs;
                    }
                    else
                    {
                        periodUnits = CumulativeDataPointCollection[i].Units -
                                      CumulativeDataPointCollection[i - 1].Units;
                        periodCosts = CumulativeDataPointCollection[i].Costs -
                                      CumulativeDataPointCollection[i - 1].Costs;

                        if (periodUnits < 0)
                            periodUnits = 0;

                        if (periodCosts < 0)
                            periodCosts = 0;
                    }

                    PeriodDataPointCollection.Add(new DataPoint
                    {
                        BudgetedCosts = CumulativeDataPointCollection[i].BudgetedCosts,
                        BudgetedUnits = CumulativeDataPointCollection[i].BudgetedUnits,
                        Costs = periodCosts < 0 ? 0 : periodCosts,
                        Units = periodUnits < 0 ? 0 : periodUnits,
                        ProgressDate = CumulativeDataPointCollection[i].ProgressDate
                    });
                }

            return PeriodDataPointCollection;
        }

        #endregion

        #region Generators

        /// <summary>
        /// Generate datapoint by spreading out units/costs across a specified timespan
        /// </summary>
        /// <param name="workingPeriod">Period to spread units/costs across</param>
        /// <param name="progressInterval">Interval for each period</param>
        /// <param name="totalUnits">Units to be spreaded</param>
        /// <param name="totalCosts">Costs to be spreaded</param>
        /// <param name="currencyConversionFactor">Factor for currency conversion</param>
        /// <param name="plotStartDate">The raw datetime of the entity to plot against</param>
        /// <param name="firstAlignedDataDate">Aligned date to correspond with other data points on chart</param>
        /// <param name="dataPointGuid">The deliverable guid</param>
        /// <param name="totalUnits">Override the total units to produce a different percentage</param>
        /// <param name="totalCosts">Override the total costs to produce a different percentage</param>
        /// <returns></returns>
        public static List<DataPoint> DataPointsGenerator(TimeSpan intervalPeriod, DateTime alignedDataDate, TimeSpan workingPeriod, decimal budgetedUnits, decimal budgetedCosts, DateTime plotStartDate, decimal currencyConversion, IEnumerable<Period> suspensionPeriod, IEnumerable<VariationAdjustment> variationAdjustments = null)
        {
            var returnProgressDataPoints = new List<DataPoint>();
            var progressInterval = intervalPeriod;
            DateTime firstAlignedDataDate = alignedDataDate;

            decimal PeriodCount = 0;
            decimal remainingUnits = budgetedUnits;
            decimal remainingCosts = budgetedCosts;
            PeriodCount = Convert.ToDecimal(workingPeriod.TotalDays) / Convert.ToDecimal(progressInterval.TotalDays);

            //don't use assignment units for total units because it comes from workpack assignment
            //workpack assignment have incomplete units because it sometimes only describe a portion of the total units

            decimal convertedCosts = budgetedCosts * currencyConversion;
            var UnitsPerPeriod = PeriodCount < 1 ? budgetedUnits : budgetedUnits / PeriodCount;
            var CostsPerPeriod = PeriodCount < 1 ? convertedCosts : convertedCosts / PeriodCount;

            //because first progress date is not necessarily the next interval
            DateTime loopDate;
            if (firstAlignedDataDate > plotStartDate)
            {
                loopDate = firstAlignedDataDate;
                do
                {
                    loopDate = loopDate.AddDays(-1 * progressInterval.Days);
                } while (loopDate.Date.AddDays(-1 * progressInterval.Days) > plotStartDate);
            }
            else
            {
                loopDate = firstAlignedDataDate;
                do
                {
                    loopDate = loopDate.AddDays(progressInterval.Days);
                } while (loopDate.Date.AddDays(progressInterval.Days) < plotStartDate);
            }

            var firstProgressPlanned = new DataPoint()
            {
                BudgetedUnits = 0,
                BudgetedCosts = 0,
                Units = 0,
                Costs = 0,
                ProgressDate = plotStartDate
            };

            returnProgressDataPoints.Add(firstProgressPlanned);

            if (loopDate < plotStartDate)
                loopDate = loopDate.AddDays(progressInterval.Days);

            //first aligned progress data point, must be checked for pro-rata
            var proRateTimeSpan = loopDate - plotStartDate;
            var proRataPeriod = Convert.ToDecimal(proRateTimeSpan.TotalDays) /
                                    (Convert.ToDecimal(progressInterval.TotalDays));

            PeriodCount -= proRataPeriod;
            decimal firstPeriodProRateUnits = UnitsPerPeriod * proRataPeriod;
            decimal firstPeriodProRateCosts = CostsPerPeriod * proRataPeriod;

            remainingUnits -= firstPeriodProRateUnits;
            remainingCosts -= firstPeriodProRateCosts;
            var firstPeriodAlignedProgressPlanned = new DataPoint()
            {
                BudgetedUnits = 0,
                BudgetedCosts = 0,
                Units = firstPeriodProRateUnits,
                Costs = firstPeriodProRateCosts,
                ProgressDate = loopDate
            };
            returnProgressDataPoints.Add(firstPeriodAlignedProgressPlanned);

            if (PeriodCount < 1)
            {
                UnitsPerPeriod = remainingUnits; //first period is last period
                CostsPerPeriod = remainingCosts;
            }
            //Reduced units from first period pro rate needs to be distributed across all other periods minus the first period
            else if (proRataPeriod > 0 && PeriodCount > 1)
            {
                UnitsPerPeriod += (UnitsPerPeriod * (1 - proRataPeriod) / (PeriodCount));
                CostsPerPeriod += (CostsPerPeriod * (1 - proRataPeriod) / (PeriodCount));
            }

            decimal loopPeriodCountForException = PeriodCount;
            DateTime loopDateForException = loopDate;

            //Establish exception periods
            List<Period> exceptionPeriods = new List<Period>();
            exceptionPeriods.AddRange(NonWorkingPeriods);
            if (suspensionPeriod != null)
                exceptionPeriods.AddRange(suspensionPeriod);


            //normalize units/costs by non-effective periods
            do
            {
                if (
                    exceptionPeriods.Any(
                        dates =>
                            dates.StartDate.Date <= loopDateForException && loopDateForException <= dates.EndDate.Date))
                {
                    UnitsPerPeriod += UnitsPerPeriod / PeriodCount; //increase all periods productivity
                    CostsPerPeriod += CostsPerPeriod / PeriodCount;
                }

                loopPeriodCountForException -= 1;
                loopDateForException = loopDateForException.AddDays(progressInterval.TotalDays);
            } while (loopPeriodCountForException > 0);

            //first period is already added through pro-rate routine
            loopDate = loopDate.AddDays(progressInterval.TotalDays);
            do
            {
                var CalendarNonWorkingPeriod = false;

                if (exceptionPeriods.Any(dates => dates.StartDate.Date <= loopDate && loopDate <= dates.EndDate.Date))
                    CalendarNonWorkingPeriod = true;

                List<VariationAdjustment> currentPeriodVariationAdjustments = variationAdjustments == null ? new List<VariationAdjustment>() : variationAdjustments.Where(
                        Adjustment => Adjustment.AdjustmentDate >= loopDate && Adjustment.AdjustmentDate < loopDate.AddDays(progressInterval.TotalDays)).ToList();
                decimal additionalUnitsPerPeriod = 0;
                if (currentPeriodVariationAdjustments.Count > 0)
                {
                    decimal currentPeriodAdjustmentUnits = currentPeriodVariationAdjustments.Sum(x => x.AdjustmentUnits);
                    additionalUnitsPerPeriod = currentPeriodAdjustmentUnits / PeriodCount;
                    UnitsPerPeriod += additionalUnitsPerPeriod;
                }

                decimal assignUnits;
                if (CalendarNonWorkingPeriod)
                    assignUnits = 0;
                else if (UnitsPerPeriod > remainingUnits)
                    assignUnits = remainingUnits;
                else
                    assignUnits = UnitsPerPeriod;

                var newProgressPlanned = new DataPoint()
                {
                    BudgetedUnits = 0,
                    BudgetedCosts = 0,
                    Units = assignUnits,
                    Costs = CalendarNonWorkingPeriod ? 0 : CostsPerPeriod > remainingCosts ? remainingCosts : CostsPerPeriod,
                    ProgressDate = loopDate
                };

                remainingUnits -= assignUnits;
                returnProgressDataPoints.Add(newProgressPlanned);
                PeriodCount -= 1;
                loopDate = loopDate.AddDays(progressInterval.TotalDays);
            } while (remainingUnits > 0);

            return returnProgressDataPoints;
        }

        /// <summary>
        /// Generate the remaining data points based on productivity
        /// Prerequisites: this.ISupportProgressReportingCollection.FirstAlignedDataDate, this.ISupportProgressReportingCollection.IntervalPeriod and this.ISupportProgressReportingCollection.UnifiedCalculationMethod must be initialized
        /// </summary>
        public static List<DataPoint> RemainingDataPointsGenerator(TimeSpan progressInterval,
            PROGRESS_ITEMProjection progressItemStats, DateTime firstAlignedWeekEndingDataDate, List<Period> exceptionPeriod,
            decimal remainingUnits, decimal unitsPerHour, decimal firstPeriodProRate, decimal currencyConversion,
            DateTime? limitDate = null)
        {
            var remainingDataPoints = new List<DataPoint>();
            var currentBASELINE_ITEM = progressItemStats.BASELINE_ITEMJoinRATE;
            if (currentBASELINE_ITEM.BASELINE_ITEM.TOTAL_HOURS == 0 || unitsPerHour == 0)
                return remainingDataPoints;

            var unitsPerDay = unitsPerHour * int.Parse(CommonResources.ProgressReporting_DefaultHoursADay);
            decimal unitsPerPeriod;
            TimeSpan intervalPeriod = progressInterval;

            if (limitDate != null)
            {
                var dateLimit = (DateTime) limitDate;
                var remainingCountPeriod =
                    Convert.ToDecimal((dateLimit - firstAlignedWeekEndingDataDate).Days / intervalPeriod.Days);

                if (remainingCountPeriod <= 0)
                    remainingCountPeriod = 1;

                unitsPerPeriod = remainingCountPeriod == remainingUnits ? 1 : remainingUnits / remainingCountPeriod;
            }
            else
            {
                unitsPerPeriod = unitsPerDay * intervalPeriod.Days;
            }

            if (unitsPerPeriod > remainingUnits)
                unitsPerPeriod = remainingUnits;

            //remaining date is moved forward a period to categorize the datapoints as week ending
            var remainingCountDataDate = firstAlignedWeekEndingDataDate;

            do
            {
                if (
                    !exceptionPeriod.Any(
                        dates =>
                            dates.StartDate.Date <= remainingCountDataDate.Date &&
                            remainingCountDataDate.Date <= dates.EndDate.Date))
                {
                    decimal periodUnits;
                    
                    if (firstPeriodProRate > 0)
                    {
                        periodUnits = unitsPerPeriod * firstPeriodProRate;
                        firstPeriodProRate = 0;
                    }
                    else if (remainingUnits < unitsPerPeriod + 1) 
                    //Do units per periods + 1 so that any insignificant amount won't be pushed to the following period (i.e. week)
                        periodUnits = remainingUnits;
                    else
                        periodUnits = unitsPerPeriod;

                    var newDataPoint = new DataPoint()
                    {
                        BudgetedCosts = currentBASELINE_ITEM.TOTAL_COSTS,
                        BudgetedUnits = currentBASELINE_ITEM.BASELINE_ITEM.TOTAL_HOURS,
                        Units = periodUnits,
                        ProgressDate = remainingCountDataDate
                    };

                    newDataPoint.Costs = currentBASELINE_ITEM.ITEMRATE * currencyConversion;
                    remainingDataPoints.Add(newDataPoint);
                    remainingUnits -= periodUnits;
                }
                else
                {
                    var newDataPoint = new DataPoint()
                    {
                        BudgetedCosts = currentBASELINE_ITEM.TOTAL_COSTS,
                        BudgetedUnits = currentBASELINE_ITEM.BASELINE_ITEM.TOTAL_HOURS,
                        Units = 0,
                        Costs = 0,
                        ProgressDate = remainingCountDataDate
                    };

                    remainingDataPoints.Add(newDataPoint);
                }

                remainingCountDataDate = remainingCountDataDate.AddDays(intervalPeriod.Days);
            } while (remainingUnits > 0);

            return remainingDataPoints;
        }

        #endregion
    }
}