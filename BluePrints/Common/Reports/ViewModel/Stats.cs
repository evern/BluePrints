using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static BluePrints.Data.BluePrintsEntities;

namespace BluePrints.Common.ViewModel.Reporting
{
    public class Stats : BindableBase
    {
        IEnumerable<DataPoint> rawDataPoints
        {
            get { return GetProperty(() => rawDataPoints); }
            set { SetProperty(() => rawDataPoints, value, OnrawDataPointsChanged); }
        }

        void OnrawDataPointsChanged()
        {
            cumulativeDataPoints = null;
            dataPoints = null;
            currentPeriodDataPoint = null;
            currentPeriodCumulativeDataPoint = null;
            RaisePropertyChanged(() => CumulativeDataPoints);
            RaisePropertyChanged(() => DataPoints);
            RaisePropertyChanged(() => CurrentPeriodDataPoint);
            RaisePropertyChanged(() => CurrentPeriodCumulativeDataPoint);
        }

        readonly DateTime reportingDataDate;
        public readonly decimal BudgetedUnits;
        public readonly decimal TotalUnits;
        public readonly decimal BudgetedCosts;
        public readonly decimal TotalCosts;
        readonly IEnumerable<VariationAdjustment> rawVariationAdjustments;
        readonly bool hideDataPointsBeforeDataDate;
        readonly bool alwaysBenchmarkAgainstBudgeted;
        readonly DateTime firstAlignedDataDate;
        readonly TimeSpan reportInterval;

        public bool FromP6 { get; private set; }

        /// <summary>
        /// Used For Workpack Summary Stats
        /// </summary>
        /// <param name="summaryStats">Project Summary Stats</param>
        public Stats(SummaryStats summaryStats)
        {
            this.reportingDataDate = summaryStats.ReportingDataDate;
            this.BudgetedUnits = summaryStats.BudgetedUnits;
            this.BudgetedCosts = summaryStats.BudgetedCosts;
            this.TotalUnits = summaryStats.totalUnits;
            this.TotalCosts = summaryStats.totalCosts;
            this.firstAlignedDataDate = summaryStats.FirstAlignedDataDate;
            this.reportInterval = summaryStats.ReportingInterval;
            //Always use weekly
            //this.reportInterval = new TimeSpan(1, 0, 0, 0);
            this.rawVariationAdjustments = summaryStats.VariationAdjustments;
        }

        public Stats(DateTime reportingDataDate, decimal budgetedUnits, decimal totalUnits, decimal budgetedCosts, decimal totalCosts, DateTime firstAlignedDataDate, TimeSpan reportInterval, IEnumerable<VariationAdjustment> rawVariationAdjustments = null, bool hideDataPointsBeforeDataDate = false, bool alwaysBenchmarkAgainstBudgeted = false)
        {
            this.reportingDataDate = reportingDataDate;
            this.BudgetedUnits = budgetedUnits;
            this.TotalUnits = totalUnits;
            this.BudgetedCosts = budgetedCosts;
            this.TotalCosts = totalCosts;
            this.firstAlignedDataDate = firstAlignedDataDate;
            this.reportInterval = reportInterval;
            //Always use weekly
            //this.reportInterval = new TimeSpan(1, 0, 0, 0);
            this.rawVariationAdjustments = rawVariationAdjustments;
            this.hideDataPointsBeforeDataDate = hideDataPointsBeforeDataDate;
            this.alwaysBenchmarkAgainstBudgeted = alwaysBenchmarkAgainstBudgeted;
        }

        public void SetData(IEnumerable<DataPoint> rawDataPoints)
        {
            this.rawDataPoints = rawDataPoints;
        }

        public void SetPlannedData(IEnumerable<StoredProcedure_PlannedDataPoint> rawStoredProcedureDataPoints)
        {
            List<DataPoint> convertedDataPoints = DataPointsHelpers.ConvertStoredProcedurePlannedDataPointToDataPoints(rawStoredProcedureDataPoints).ToList();

            if (convertedDataPoints.All(x => x.IsFromP6))
                SetFromP6();

            this.rawDataPoints = convertedDataPoints;
        }

        public void SetRemainingData(IEnumerable<StoredProcedure_RemainingDataPoint> rawStoredProcedureDataPoints, IEnumerable<DataPoint> earnedDataPoints)
        {
            List<DataPoint> convertedDataPoints = DataPointsHelpers.ConvertStoredProcedureRemainingDataPointToDataPoints(rawStoredProcedureDataPoints).ToList();

            if(earnedDataPoints != null)
                convertedDataPoints.AddRange(earnedDataPoints.ToList());

            if (convertedDataPoints.All(x => x.IsFromP6))
                SetFromP6();

            this.rawDataPoints = convertedDataPoints;
        }

        public IEnumerable<ExoDataPoint> ExoDataPoints
        {
            get
            {
                if (this.rawDataPoints == null)
                    return new List<ExoDataPoint>();

                IEnumerable<ExoDataPoint> exoDataPoints = this.rawDataPoints as IEnumerable<ExoDataPoint>;
                if (exoDataPoints != null)
                {
                    exoDataPoints = exoDataPoints.OrderBy(x => x.ProgressDate);
                    return exoDataPoints;
                }

                return new List<ExoDataPoint>();
            }
        }

        public List<DataPoint> GetData()
        {
            if (this.rawDataPoints == null)
                return new List<DataPoint>();

            return this.rawDataPoints.ToList();
        }

        public void SetFromP6()
        {
            this.FromP6 = true;
        }

        private ObservableCollection<DataPoint> cumulativeDataPoints { get; set; }
        public ObservableCollection<DataPoint> CumulativeDataPoints
        {
            get
            {
                if (cumulativeDataPoints == null && rawDataPoints != null && rawDataPoints.Count() > 0 && firstAlignedDataDate != null)
                {
                    cumulativeDataPoints = DataPointsHelpers.GroupDataPointsByPeriod(rawDataPoints, BudgetedUnits, BudgetedCosts, firstAlignedDataDate, reportInterval, Guid.Empty, alwaysBenchmarkAgainstBudgeted ? null : rawVariationAdjustments);
                }

                return cumulativeDataPoints;
            }
        }

        private ObservableCollection<DataPoint> dataPoints { get; set; }
        public ObservableCollection<DataPoint> DataPoints
        {
            get
            {
                if (dataPoints == null && CumulativeDataPoints != null && CumulativeDataPoints.Count() > 0 && reportingDataDate != null)
                {
                    DateTime? plotStartdate = hideDataPointsBeforeDataDate ? reportingDataDate : (DateTime ?)null;
                    dataPoints = DataPointsHelpers.ConvertCumulativeToPeriodDataPoint(CumulativeDataPoints, plotStartdate);
                }

                return dataPoints;
            }
        }

        private DataPoint currentPeriodDataPoint { get; set; }
        public DataPoint CurrentPeriodDataPoint
        {
            get
            {
                if (currentPeriodDataPoint == null && reportingDataDate != null && CumulativeDataPoints != null && CumulativeDataPoints.Count() > 0 && reportingDataDate != null)
                    currentPeriodDataPoint = DataPointsHelpers.FindPeriodDataPointInCumulativeDataPoints(
                            CumulativeDataPoints, reportingDataDate.Date);

                return currentPeriodDataPoint;
            }
        }

        private DataPoint currentPeriodCumulativeDataPoint { get; set; }
        public DataPoint CurrentPeriodCumulativeDataPoint
        {
            get
            {
                if (currentPeriodCumulativeDataPoint == null && CumulativeDataPoints != null && CumulativeDataPoints.Count > 0 && reportingDataDate != null)
                    currentPeriodCumulativeDataPoint = DataPointsHelpers.FindPeriodDataPointInDataPoints(CumulativeDataPoints,
                            reportingDataDate.Date);
                return currentPeriodCumulativeDataPoint;
            }
        }

        public DateTime StartDate
        {
            get
            {
                if (DataPoints == null)
                    return new DateTime();

                return DataPoints.Min(x => x.ProgressDate);
            }
        }

        public DateTime EndDate
        {
            get
            {
                if (DataPoints == null)
                    return new DateTime();

                return DataPoints.Max(x => x.ProgressDate);
            }
        }
    }

    public class ExoDataPoint : DataPoint
    {
        public string WorkpackName { get; set; }
        public string ResourceName { get; set; }
        public string CostGroup { get; set; }
        public string CostType { get; set; }

        public string PhaseCode
        {
            get
            {
                if (WorkpackName == string.Empty)
                    return string.Empty;

                return WorkpackName.Substring(13, 2);
            }
        }

        public string DisciplineCode
        {
            get
            {
                if (CostGroup == null || CostGroup == string.Empty || CostGroup.Length < 3)
                    return string.Empty;

                return CostGroup.Substring(0, 3);
            }
        }

        public string CommodityCode
        {
            get
            {
                if (CostType == null || CostType == string.Empty || CostType.Length < 3)
                    return string.Empty;

                return CostType.Substring(0, 3);
            }
        }
    }

    public class DataPoint
    {
        public DateTime ProgressDate { get; set; }
        public decimal Units { get; set; }
        public decimal Costs { get; set; }
        //Used to store actuals while storing burn
        public decimal Actuals { get; set; }
        public decimal BudgetedUnits { get; set; }
        public decimal BudgetedCosts { get; set; }

        public decimal Quantity { get; set; }

        public Guid DeliverableGuid { get; set; }
        public bool IsFromP6 { get; set; }

        public decimal UnitsPercentage
        {
            get
            {
                if (BudgetedUnits == 0 || Units == 0)
                    return 0;
                else
                    return Units / BudgetedUnits;
            }
        }

        public decimal CostsPercentage
        {
            get
            {
                if (BudgetedCosts == 0 || Costs == 0)
                    return 0;
                else
                    return Costs / BudgetedCosts;
            }
        }
    }
}