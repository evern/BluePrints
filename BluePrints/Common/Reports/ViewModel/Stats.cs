using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        public readonly decimal BudgetedQty;
        public readonly decimal TotalQty;
        public readonly decimal BudgetedCosts;
        public readonly decimal TotalCosts;
        readonly IEnumerable<VariationAdjustment> rawVariationAdjustments;
        readonly bool hideDataPointsBeforeDataDate;
        readonly bool alwaysBenchmarkAgainstBudgeted;
        readonly DateTime firstAlignedDataDate;
        readonly TimeSpan reportInterval;
        readonly DateTime? extrapolateDate;
        readonly bool isDebug;
        public bool FromP6 { get; private set; }

        /// <summary>
        /// Used For Subjob Summary Stats
        /// </summary>
        /// <param name="summaryStats">Project Summary Stats</param>
        public Stats(SummaryStats summaryStats)
        {
            this.reportingDataDate = summaryStats.ReportingDataDate;
            this.BudgetedUnits = summaryStats.BudgetedUnits;
            this.BudgetedCosts = summaryStats.BudgetedCosts;
            this.TotalUnits = summaryStats.totalUnits;
            this.TotalCosts = summaryStats.totalCosts;
            this.BudgetedQty = summaryStats.BudgetedQty;
            this.TotalQty = summaryStats.TotalQty;
            this.firstAlignedDataDate = summaryStats.FirstAlignedDataDate;
            this.reportInterval = summaryStats.ReportingInterval;
            //Always use weekly
            //this.reportInterval = new TimeSpan(1, 0, 0, 0);
            this.rawVariationAdjustments = summaryStats.VariationAdjustments;
        }

        public Stats(DateTime reportingDataDate, decimal budgetedUnits, decimal totalUnits, decimal budgetedQty, decimal totalQty, decimal budgetedCosts, decimal totalCosts, DateTime firstAlignedDataDate, TimeSpan reportInterval, IEnumerable<VariationAdjustment> rawVariationAdjustments = null, bool hideDataPointsBeforeDataDate = false, bool alwaysBenchmarkAgainstBudgeted = false, DateTime? extrapolateDate = null, bool isDebug = false)
        {
            this.reportingDataDate = reportingDataDate;
            this.BudgetedUnits = budgetedUnits;
            this.TotalUnits = totalUnits;
            this.BudgetedQty = budgetedQty;
            this.TotalQty = totalQty;
            this.BudgetedCosts = budgetedCosts;
            this.TotalCosts = totalCosts;
            this.firstAlignedDataDate = firstAlignedDataDate;
            this.reportInterval = reportInterval;
            this.extrapolateDate = extrapolateDate;
            //Always use weekly
            //this.reportInterval = new TimeSpan(1, 0, 0, 0);
            this.rawVariationAdjustments = rawVariationAdjustments;
            this.hideDataPointsBeforeDataDate = hideDataPointsBeforeDataDate;
            this.alwaysBenchmarkAgainstBudgeted = alwaysBenchmarkAgainstBudgeted;
            this.isDebug = isDebug;
        }

        public void SetData(IEnumerable<DataPoint> rawDataPoints)
        {
            this.rawDataPoints = rawDataPoints;
        }

        public decimal GetApplicableProductivityCalculationBudgetedDuration()
        {
            if (this.rawDataPoints == null || this.rawDataPoints.Count() == 0)
                return 0;

            return this.rawDataPoints.Where(x => x.ProgressDate <= reportingDataDate).Where(x => x.RemainingDuration != null).Sum(x => (decimal)x.RemainingDuration);
        }

        public decimal GetApplicableProductivityCalculationRemainingDuration()
        {
            if (this.rawDataPoints == null || this.rawDataPoints.Count() == 0)
                return 0;

            return this.rawDataPoints.Where(x => x.RemainingDuration != null && x.RemainingDuration > 0).Sum(x => (decimal)x.RemainingDuration);
        }

        public decimal GetApplicableRemainingProductivityCalculationBudgetedUnits()
        {
            if (this.rawDataPoints == null || this.rawDataPoints.Count() == 0)
                return 0;

            return this.rawDataPoints.Where(x => x.ProgressDate <= reportingDataDate).Where(x => x.RemainingDuration != null).Sum(x => (decimal)x.Units);
        }

        public decimal GetApplicableRemainingProductivityCalculationRemainingUnits()
        {
            if (this.rawDataPoints == null || this.rawDataPoints.Count() == 0)
                return 0;

            return this.rawDataPoints.Where(x => x.RemainingDuration != null && x.RemainingDuration > 0).Sum(x => (decimal)x.Units);
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

            if (earnedDataPoints != null)
                convertedDataPoints.AddRange(earnedDataPoints.ToList());

            if (convertedDataPoints.All(x => x.IsFromP6))
                SetFromP6();

            this.rawDataPoints = convertedDataPoints;
        }

        public void SetRemainingActualData(IEnumerable<DataPoint> remainingDataPoints, IEnumerable<DataPoint> burnedDataPoints)
        {
            List<DataPoint> convertedDataPoints = remainingDataPoints.Where(x => x.IsRemaining).ToList();

            if (burnedDataPoints != null)
                convertedDataPoints.AddRange(burnedDataPoints.ToList());

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
                    decimal qtyPerUnit = TotalUnits == 0 ? 0 : TotalQty / TotalUnits;
                    //Budgeted units are always used because variation adjustment will be added on if alwaysBenchmarkAgainstBudgeted is false and rawVariationAdjustments is not null
                    cumulativeDataPoints = DataPointsHelpers.GroupDataPointsByPeriod(rawDataPoints, BudgetedUnits, BudgetedCosts, qtyPerUnit, firstAlignedDataDate, reportInterval, Guid.Empty, alwaysBenchmarkAgainstBudgeted ? null : rawVariationAdjustments, extrapolateDate);
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
                    decimal qtyPerUnit = this.TotalUnits == 0 ? 0 : TotalQty / TotalUnits;
                    DateTime? plotStartdate = hideDataPointsBeforeDataDate ? reportingDataDate : (DateTime?)null;
                    dataPoints = DataPointsHelpers.ConvertCumulativeToPeriodDataPoint(CumulativeDataPoints, qtyPerUnit, plotStartdate);
                    //dataPoints = DataPointsHelpers.ConvertCumulativeToPeriodDataPoint(CumulativeDataPoints);
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
                    currentPeriodDataPoint = DataPointsHelpers.FindPeriodDataPointInCumulativeDataPoints(CumulativeDataPoints, reportingDataDate.Date);

                return currentPeriodDataPoint;
            }
        }

        private DataPoint currentPeriodCumulativeDataPoint { get; set; }
        public DataPoint CurrentPeriodCumulativeDataPoint
        {
            get
            {
                if (currentPeriodCumulativeDataPoint == null && CumulativeDataPoints != null && CumulativeDataPoints.Count > 0 && reportingDataDate != null)
                    currentPeriodCumulativeDataPoint = DataPointsHelpers.FindPeriodDataPointInDataPoints(CumulativeDataPoints, reportingDataDate.Date);
                return currentPeriodCumulativeDataPoint;
            }
        }

        public decimal Remaining_Units
        {
            get
            {
                if (rawDataPoints != null && rawDataPoints.Count() > 0)
                    return rawDataPoints.Where(x => x.IsRemaining).Sum(x => x.Units);
                else
                    return 0;
            }
        }

        public decimal Remaining_Costs
        {
            get
            {
                if (rawDataPoints != null && rawDataPoints.Count() > 0)
                    return rawDataPoints.Where(x => x.IsRemaining).Sum(x => x.Costs);
                else
                    return 0;
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
        public string Subjob_Name { get; set; }
        public string ResourceName { get; set; }
        public string Role { get; set; }
        public string CostGroup { get; set; }
        public string CostType { get; set; }

        public string PhaseCode
        {
            get
            {
                if (Subjob_Name == string.Empty)
                    return string.Empty;
                else if (Subjob_Name.Length < 15)
                    return string.Empty;
                
                return Subjob_Name.Substring(13, 2);
            }
        }

        public string Department_Code
        {
            get
            {
                if (Role == null || Role == string.Empty || Role.Length < 2)
                    return string.Empty;
                else if (Role.Length < 2)
                    return string.Empty;

                return Role.Substring(0, 2);
            }
        }

        public string Discipline_Code
        {
            get
            {
                if (CostGroup == null || CostGroup == string.Empty || CostGroup.Length < 4)
                    return string.Empty;
                else if (CostGroup.Length < 4)
                    return string.Empty;

                return CostGroup.Substring(0, 4);
            }
        }

        public string AreaCode
        {
            get
            {
                if (Subjob_Name == string.Empty)
                    return string.Empty;
                else if (Subjob_Name.Length < 9)
                    return string.Empty;

                return Subjob_Name.Substring(6, 3);
            }
        }

        public string SubAreaCode
        {
            get
            {
                if (Subjob_Name == string.Empty)
                    return string.Empty;
                else if (Subjob_Name.Length < 12)
                    return string.Empty;

                return Subjob_Name.Substring(10, 2);
            }
        }

        public string Commodity_Code
        {
            get
            {
                if (CostType == null || CostType == string.Empty || CostType.Length < 3)
                    return string.Empty;
                else if (CostType.Length < 3)
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
        public decimal? RemainingDuration { get; set; }
        public decimal Quantity { get; set; }

        public Guid DeliverableGuid { get; set; }
        public bool IsFromP6 { get; set; }
        public bool IsRemaining { get; set; }

        public bool DoNotPlot { get; set; }

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