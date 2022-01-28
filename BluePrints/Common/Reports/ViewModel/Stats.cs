using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BluePrints.Common.Misc;
using BluePrints.Common.Resources;
using BluePrints.Common.Utils;
using BluePrints.Data;
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
        public bool StatsBuilt { get; set; }
        readonly bool hideDataPointsBeforeDataDate;
        readonly bool forceRetrieveAllRemaining;
        readonly bool alwaysBenchmarkAgainstBudgeted;
        readonly DateTime firstAlignedDataDate;
        readonly TimeSpan reportInterval;
        readonly DateTime? extrapolateDate;
        public bool FromP6 { get; private set; }

        /// <summary>
        /// Used For Subjob Summary Stats
        /// </summary>
        /// <param name="summaryStats">Project Summary Stats</param>
        public Stats(SummaryStats summaryStats, bool hideDataPointsBeforeDataDate = false, bool forceRetrieveAllRemaining = false)
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
            this.hideDataPointsBeforeDataDate = hideDataPointsBeforeDataDate;
            this.forceRetrieveAllRemaining = forceRetrieveAllRemaining;
        }

        public Stats(DateTime reportingDataDate, decimal budgetedUnits, decimal totalUnits, decimal budgetedQty, decimal totalQty, decimal budgetedCosts, decimal totalCosts, DateTime firstAlignedDataDate, TimeSpan reportInterval, bool hideDataPointsBeforeDataDate = false, bool alwaysBenchmarkAgainstBudgeted = false, DateTime? extrapolateDate = null, bool forceRetrieveAllRemaining = false)
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
            this.hideDataPointsBeforeDataDate = hideDataPointsBeforeDataDate;
            this.forceRetrieveAllRemaining = forceRetrieveAllRemaining;

            this.alwaysBenchmarkAgainstBudgeted = alwaysBenchmarkAgainstBudgeted;
        }

        public void SetData(IEnumerable<DataPoint> rawDataPoints)
        {
            this.rawDataPoints = rawDataPoints;
        }

        public void AppendData(IEnumerable<DataPoint> rawDataPoints)
        {
            List<DataPoint> currentDataPoints = new List<DataPoint>();
            currentDataPoints.AddRange(rawDataPoints);
            currentDataPoints.AddRange(this.rawDataPoints);

            this.rawDataPoints = currentDataPoints;
        }

        public void SetAlignedDateData(IEnumerable<DataPoint> rawDataPoints)
        {
            List<DataPoint> dataPoints = new List<DataPoint>(rawDataPoints);

            //deduct a second so that GroupDataPointsByPeriod uses < ceilingDate when grouping data points into periods and aligned date on points are ceiling dates
            dataPoints.ForEach(x => x.ProgressDate = x.ProgressDate.AddSeconds(-1));

            this.rawDataPoints = dataPoints;
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

        public void SetPlannedData(IEnumerable<Data.DataPoint> rawStoredProcedureDataPoints)
        {
            List<DataPoint> convertedDataPoints = DataPointsHelpers.ConvertDbPlannedDataPointToReportingDataPoints(rawStoredProcedureDataPoints).ToList();

            if (convertedDataPoints.All(x => x.IsFromP6))
                SetFromP6();

            this.rawDataPoints = convertedDataPoints;
            this.StatsBuilt = true;
        }

        public void SetPlannedData(IEnumerable<X_WBS_GROUPED_DATAPOINT> rawStoredProcedureDataPoints)
        {
            List<DataPoint> convertedDataPoints = DataPointsHelpers.ConvertDbDataPointToReportingDataPoints(rawStoredProcedureDataPoints).ToList();

            this.rawDataPoints = convertedDataPoints;
            this.StatsBuilt = true;
        }

        public void SetRemainingData(IEnumerable<Data.DataPoint> rawStoredProcedureDataPoints, IEnumerable<DataPoint> earnedDataPoints)
        {
            List<DataPoint> convertedDataPoints;
            if (rawStoredProcedureDataPoints == null || rawStoredProcedureDataPoints.Count() == 0)
                convertedDataPoints = new List<DataPoint>();
            else
                convertedDataPoints = DataPointsHelpers.ConvertDbRemainingDataPointToReportingDataPoints(rawStoredProcedureDataPoints).ToList();

            if (earnedDataPoints != null)
                convertedDataPoints.AddRange(earnedDataPoints);

            if (convertedDataPoints.All(x => x.IsFromP6))
                SetFromP6();
            
            this.rawDataPoints = convertedDataPoints;
            this.StatsBuilt = true;
        }

        public void SetRemainingData(IEnumerable<X_WBS_GROUPED_DATAPOINT> dbDataPoints, IEnumerable<DataPoint> earnedDataPoints)
        {
            List<DataPoint> convertedDataPoints;
            if (dbDataPoints == null || dbDataPoints.Count() == 0)
                convertedDataPoints = new List<DataPoint>();
            else
                convertedDataPoints = DataPointsHelpers.ConvertDbDataPointToReportingDataPoints(dbDataPoints, true).ToList();

            if (earnedDataPoints != null)
                convertedDataPoints.AddRange(earnedDataPoints);

            if (convertedDataPoints.All(x => x.IsFromP6))
                SetFromP6();

            this.rawDataPoints = convertedDataPoints;
            this.StatsBuilt = true;
        }

        //used to collect stock override productivity and earned units for weighted productivity calculation at commodity level
        //because burned units cannot be retrieved at stock level
        //use stock level productivity if deliverable has earned but not burned
        public class StockProductivity
        {
            public decimal? OverrideProductivity { get; set; }
            public decimal EarnedUnits { get; set; }
            public decimal TotalUnits { get; set; }
        }

        public void SetRemainingActualData(IEnumerable<IReportable> groupedReportables, IEnumerable<DataPoint> burnedDataPoints)
        {
            //establish remaining data points
            DateTime? lastBurnedDate = burnedDataPoints.Count() == 0 ? (DateTime?)null : burnedDataPoints.Max(x => x.ProgressDate);
            List<DataPoint> remainingDataPoints = new List<DataPoint>();
            List<StockProductivity> stockProductivities = new List<StockProductivity>();
            decimal defaultProductivity = decimal.Parse(BluePrintsResources.Default_Productivity);
            //gather stock productivities
            foreach(IReportable reportable in groupedReportables)
            {
                if (reportable.Stats == null)
                    continue;

                if (reportable.Stats.Remaining == null)
                    continue;

                IEnumerable<DataPoint> earnedDataPoints = reportable.Stats.Earned.GetData();
                decimal earnedUnits = earnedDataPoints.Count() == 0 ? 0 : earnedDataPoints.Sum(x => x.Units);
                StockProductivity stockProductivity = new StockProductivity() { OverrideProductivity = reportable.Override_Productivity, EarnedUnits = earnedUnits, TotalUnits = reportable.Total_Units };
                stockProductivities.Add(stockProductivity);
            }

            decimal totalBurnedUnits = burnedDataPoints.Count() == 0 ? 0 : burnedDataPoints.Sum(x => x.Units);
            decimal totalEarnedUnits = stockProductivities.Sum(x => x.EarnedUnits);
            decimal groupLevelProductivity = BluePrintsDataUtils.GetProductivity(totalEarnedUnits, totalBurnedUnits);
            decimal totalStockUnits = stockProductivities.Sum(x => x.TotalUnits);
            decimal totalWeightedProductivity = 0;

            //use default remaining units if productivity is 0 to result in total units/costs of 0
            if (totalStockUnits == 0)
                totalWeightedProductivity = 1;
            else
            {
                foreach (StockProductivity stockProductivity in stockProductivities)
                {
                    decimal productivity = 0;
                    decimal weightProRate = stockProductivity.TotalUnits / totalStockUnits;
                    //when group level productivity is zero fallback on stock level productivity, also applies when user inputs an override
                    if (stockProductivity.OverrideProductivity != null)
                        productivity = (decimal)stockProductivity.OverrideProductivity;
                    else
                        productivity = groupLevelProductivity;

                    totalWeightedProductivity += productivity * weightProRate;
                }
            }

            //adjust remaining data points by weighted productivity
            foreach (IReportable reportable in groupedReportables)
            {
                if (reportable.Stats == null)
                    continue;

                if (reportable.Stats.Remaining == null)
                    continue;

                List<DataPoint> baselineRemainingDataPoints = reportable.Stats.Remaining.GetData().Where(x => x.IsRemaining).ToList();
                if (lastBurnedDate != null)
                    baselineRemainingDataPoints = baselineRemainingDataPoints.Where(x => x.ProgressDate > lastBurnedDate).ToList();

                List<DataPoint> remainingAdjustDataPoints = new List<DataPoint>();
                foreach (DataPoint remainingDataPoint in baselineRemainingDataPoints.Where(x => !x.IsProductivityInflated))
                {
                    DataPoint remainingAdjustDataPoint = new DataPoint();
                    DataUtils.ShallowCopy(remainingAdjustDataPoint, remainingDataPoint);
                    remainingAdjustDataPoint.Units = remainingAdjustDataPoint.Units / totalWeightedProductivity;
                    remainingAdjustDataPoint.Costs = remainingAdjustDataPoint.Costs / totalWeightedProductivity;
                    remainingAdjustDataPoint.IsProductivityInflated = true;
                    remainingAdjustDataPoints.Add(remainingAdjustDataPoint);
                }

                remainingDataPoints.AddRange(remainingAdjustDataPoints);
            }

            //burned data points will be plotted before the data date
            if (burnedDataPoints != null)
                remainingDataPoints.AddRange(burnedDataPoints.ToList());

            if (remainingDataPoints.All(x => x.IsFromP6))
                SetFromP6();

            this.rawDataPoints = remainingDataPoints;
            this.StatsBuilt = true;
        }

        public void SetRemainingActualData(WBSReportable WBSReportables, IEnumerable<DataPoint> burnedDataPoints)
        {
            //establish remaining data points
            DateTime? lastBurnedDate = burnedDataPoints.Count() == 0 ? (DateTime?)null : burnedDataPoints.Max(x => x.ProgressDate);
            IEnumerable<DataPoint> earnedDataPoints = WBSReportables.Earned.GetData();
            IEnumerable<DataPoint> remainingDataPoints = WBSReportables.Remaining.GetData();
            decimal totalEarnedUnits = earnedDataPoints.Count() == 0 ? 0 : earnedDataPoints.Sum(x => x.Units);
            decimal totalBurnedUnits = burnedDataPoints.Count() == 0 ? 0 : burnedDataPoints.Sum(x => x.Units);
            decimal productivity = totalEarnedUnits == 0 ? 1 : totalBurnedUnits / totalEarnedUnits;

            //adjust remaining data points by productivity
            List<DataPoint> baselineRemainingDataPoints = remainingDataPoints.Where(x => x.IsRemaining).ToList();
            if (lastBurnedDate != null)
                baselineRemainingDataPoints = baselineRemainingDataPoints.Where(x => x.ProgressDate > lastBurnedDate).ToList();

            List<DataPoint> remainingAdjustDataPoints = new List<DataPoint>();
            foreach (DataPoint remainingDataPoint in baselineRemainingDataPoints.Where(x => !x.IsProductivityInflated))
            {
                DataPoint remainingAdjustDataPoint = new DataPoint();
                DataUtils.ShallowCopy(remainingAdjustDataPoint, remainingDataPoint);
                remainingAdjustDataPoint.Units = remainingAdjustDataPoint.Units / productivity;
                remainingAdjustDataPoint.Costs = remainingAdjustDataPoint.Costs / productivity;
                remainingAdjustDataPoint.IsProductivityInflated = true;
                remainingAdjustDataPoints.Add(remainingAdjustDataPoint);
            }

            //burned data points will be plotted before the data date
            if (burnedDataPoints != null)
                remainingAdjustDataPoints.AddRange(burnedDataPoints.ToList());

            if (remainingDataPoints.All(x => x.IsFromP6))
                SetFromP6();

            this.rawDataPoints = remainingAdjustDataPoints;
            this.StatsBuilt = true;
        }

        public List<ExoDataPoint> ExoDataPoints
        {
            get
            {
                if (this.rawDataPoints == null)
                    return new List<ExoDataPoint>();

                IEnumerable<ExoDataPoint> exoDataPoints = this.rawDataPoints as IEnumerable<ExoDataPoint>;
                if (exoDataPoints != null)
                {
                    exoDataPoints = exoDataPoints.OrderBy(x => x.ProgressDate);
                    return exoDataPoints.ToList();
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

                    //variation adjustment implementation
                    //cumulativeDataPoints = DataPointsHelpers.GroupDataPointsByPeriod(rawDataPoints, BudgetedUnits, BudgetedCosts, qtyPerUnit, firstAlignedDataDate, reportInterval, Guid.Empty, alwaysBenchmarkAgainstBudgeted ? null : rawVariationAdjustments, extrapolateDate);
                    DateTime firstDataDate = firstAlignedDataDate;

                    //total units at start
                    cumulativeDataPoints = DataPointsHelpers.GroupDataPointsByPeriod(rawDataPoints, BudgetedUnits, BudgetedCosts, TotalUnits, TotalCosts, qtyPerUnit, firstDataDate, reportInterval, Guid.Empty,  null, extrapolateDate);
                }

                return cumulativeDataPoints;
            }
        }

        //because cumulative datapoint includes earned data points as well
        private ObservableCollection<DataPoint> remainingOnlyCumulativeDataPoints { get; set; }
        public ObservableCollection<DataPoint> RemainingOnlyCumulativeDataPoints
        {
            get
            {
                if (remainingOnlyCumulativeDataPoints == null && rawDataPoints != null && rawDataPoints.Count() > 0 && firstAlignedDataDate != null)
                {
                    decimal qtyPerUnit = TotalUnits == 0 ? 0 : TotalQty / TotalUnits;
                    //Budgeted units are always used because variation adjustment will be added on if alwaysBenchmarkAgainstBudgeted is false and rawVariationAdjustments is not null

                    //variation adjustment implementation
                    //cumulativeDataPoints = DataPointsHelpers.GroupDataPointsByPeriod(rawDataPoints, BudgetedUnits, BudgetedCosts, qtyPerUnit, firstAlignedDataDate, reportInterval, Guid.Empty, alwaysBenchmarkAgainstBudgeted ? null : rawVariationAdjustments, extrapolateDate);
                    DateTime firstDataDate = firstAlignedDataDate;

                    //total units at start
                    remainingOnlyCumulativeDataPoints = DataPointsHelpers.GroupDataPointsByPeriod(rawDataPoints, BudgetedUnits, BudgetedCosts, TotalUnits, TotalCosts, qtyPerUnit, firstDataDate, reportInterval, Guid.Empty, null, extrapolateDate, true);
                }

                return remainingOnlyCumulativeDataPoints;
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
                    dataPoints = DataPointsHelpers.ConvertCumulativeToPeriodDataPoint(CumulativeDataPoints, qtyPerUnit, plotStartdate, ExoDataPoints);
                    //dataPoints = DataPointsHelpers.ConvertCumulativeToPeriodDataPoint(CumulativeDataPoints);
                }

                return dataPoints;
            }
        }

        //because cumulative datapoint includes earned data points as well
        private ObservableCollection<DataPoint> remainingOnlyDataPoints { get; set; }
        public ObservableCollection<DataPoint> RemainingOnlyDataPoints
        {
            get
            {
                if (remainingOnlyDataPoints == null && RemainingOnlyCumulativeDataPoints != null && RemainingOnlyCumulativeDataPoints.Count() > 0 && reportingDataDate != null)
                {
                    decimal qtyPerUnit = this.TotalUnits == 0 ? 0 : TotalQty / TotalUnits;
                    DateTime? plotStartdate = hideDataPointsBeforeDataDate ? reportingDataDate : (DateTime?)null;
                    remainingOnlyDataPoints = DataPointsHelpers.ConvertCumulativeToPeriodDataPoint(RemainingOnlyCumulativeDataPoints, qtyPerUnit, plotStartdate, ExoDataPoints);
                    //dataPoints = DataPointsHelpers.ConvertCumulativeToPeriodDataPoint(CumulativeDataPoints);
                }

                return remainingOnlyDataPoints;
            }
        }

        private DataPoint currentPeriodDataPoint { get; set; }
        public DataPoint CurrentPeriodDataPoint
        {
            get
            {
                if (currentPeriodDataPoint == null && reportingDataDate != null && CumulativeDataPoints != null && CumulativeDataPoints.Count() > 0 && reportingDataDate != null)
                {
                    currentPeriodDataPoint = DataPointsHelpers.FindPeriodDataPointInCumulativeDataPoints(CumulativeDataPoints, reportingDataDate.Date);
                }

                return currentPeriodDataPoint;
            }
        }

        private DataPoint currentPeriodCumulativeDataPoint { get; set; }
        public DataPoint CurrentPeriodCumulativeDataPoint
        {
            get
            {
                if (currentPeriodCumulativeDataPoint == null && CumulativeDataPoints != null && CumulativeDataPoints.Count > 0 && reportingDataDate != null)
                {
                    currentPeriodCumulativeDataPoint = DataPointsHelpers.FindPeriodDataPointInDataPoints(CumulativeDataPoints, reportingDataDate.Date);
                }
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

        public DateTime? StartDate
        {
            get
            {
                if (DataPoints == null || DataPoints.Where(x => x.Units > 0).Count() == 0)
                    return null;

                return DataPoints.Where(x => x.Units > 0).Min(x => x.ProgressDate);
            }
        }

        public DateTime? EndDate
        {
            get
            {
                if (DataPoints == null || DataPoints.Count == 0)
                    return null;

                return DataPoints.Max(x => x.ProgressDate);
            }
        }
    }

    public class ExoDataPoint : DataPoint, ICanUpdate, IHaveDisciplineDesc
    {
        public ExoDataPoint()
        {
            Variation_Code = string.Empty;
        }

        public string Subjob_Name { get; set; }
        public string ResourceName { get; set; }
        public string Role { get; set; }
        public string CostGroup { get; set; }
        public string CostType { get; set; }
        public string StockCode { get; set; }
        public decimal CostPerQty { get; set; }
        public string Description { get; set; }
        public string Narrative { get; set; }
        public string Supplier { get; set; }
        public string InvoiceNo { get; set; }
        public decimal InvoiceAmount { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string Purchase_GLName { get; set; }
        public string Cost_GLName { get; set; }
        public string Variation_Code { get; set; }
        public bool IsPO { get; set; }
        public string PONumber { get; set; }
        public decimal POOrderQty { get; set; }
        public decimal POSuppliedQty { get; set; }
        public int? POStatus { get; set; }
        public string OfficeName { get; set; }

        public decimal DisplayActualCost => IsPO ? 0 : Costs;
        public decimal DisplayRemainingCost => IsPO ? Costs : 0;
        public decimal DisplayReceiptCost => IsPO ? POSuppliedQty * CostPerQty : 0;

        public decimal DisplayActualQty => IsPO ? 0 : Quantity;
        public decimal DisplayRemainingQty => IsPO ? Quantity : 0;

        public string PhaseCode => BluePrintsDataUtils.GetPhaseCode(Subjob_Name);
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

        public string Discipline_Code { get; set; }

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

        public string Commodity_Code { get; set; }

        #region IHaveDisciplineDesc
        public string DisciplineCode => Discipline_Code;

        public string DisciplineDesc { get; set; }
        #endregion
    }

    public class EarnedQueriesGroup
    {
        public EarnedQueriesGroup(string subJobCode, string disciplineCode, string commodityCode, string variationCode, IEnumerable<X_EARNED_QUERY> earnedQueries)
        {
            SubJobCode = subJobCode;
            DisciplineCode = disciplineCode;
            CommodityCode = commodityCode;
            VariationCode = variationCode;
            EarnedQueries = earnedQueries.ToList();
        }

        public string SubJobCode { get; set; }
        public string DisciplineCode { get; set; }
        public string CommodityCode { get; set; }
        public string VariationCode { get; set; }

        public List<X_EARNED_QUERY> EarnedQueries { get; set; }
    }

    public class ExoDataPointsGroup
    {
        public ExoDataPointsGroup(string subJobCode, string disciplineCode, string commodityCode, string variationCode, IEnumerable<ExoDataPoint> exoDataPoints)
        {
            SubJobCode = subJobCode;
            DisciplineCode = disciplineCode;
            CommodityCode = commodityCode;
            VariationCode = variationCode;
            ExoDataPoints = exoDataPoints.ToList();
        }

        public string SubJobCode { get; set; }
        public string DisciplineCode { get; set; }
        public string CommodityCode { get; set; }
        public string VariationCode { get; set; }

        public List<ExoDataPoint> ExoDataPoints { get; set; }
    }
    
    public class DataPointsGroup
    {
        public DataPointsGroup(string subJobCode, string disciplineCode, string commodityCode, string variationCode, IEnumerable<Data.DataPoint> dataPoints)
        {
            SubJobCode = subJobCode;
            DisciplineCode = disciplineCode;
            CommodityCode = commodityCode;
            VariationCode = variationCode;
            DataPoints = dataPoints.ToList();
        }

        public string SubJobCode { get; set; }
        public string DisciplineCode { get; set; }
        public string CommodityCode { get; set; }
        public string VariationCode { get; set; }

        public List<Data.DataPoint> DataPoints { get; set; }
    }

    public class DataPoint : EntityBase
    {
        public DateTime ProgressDate { get; set; }
        public DateTime ActualDate { get; set; }
        public DateTime? PURCHORD_HDRLastUpdated { get; set; }
        public decimal TotalUnits { get; set; }
        public decimal TotalCosts { get; set; }
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
        public bool IsProductivityInflated { get; set; }
        public bool DoNotPlot { get; set; }

        public List<ExoDataPoint> RawExoData { get; set; }

        public bool IsReportBudgetPercentage { get; set; }

        public decimal BudgetUnitsPercentage
        {
            get
            {
                if (BudgetedUnits == 0 || Units == 0)
                    return 0;
                else
                {
                    decimal budgetUnitsPercentage = Units / BudgetedUnits;

                    //fix rounding issue where it goes beyond 100%
                    if (budgetUnitsPercentage > 1)
                        budgetUnitsPercentage = 1;

                    return budgetUnitsPercentage;
                }
            }
        }

        public decimal UnitsPercentage
        {
            get
            {
                if (TotalUnits == 0 || Units == 0)
                    return 0;
                else
                {
                    decimal totalUnitsPercentage = Units / TotalUnits;

                    //fix rounding issue where it goes beyond 100%
                    if (totalUnitsPercentage > 1)
                        totalUnitsPercentage = 1;

                    return totalUnitsPercentage;
                }
            }
        }

        public decimal BudgetCostsPercentage
        {
            get
            {
                if (BudgetedCosts == 0 || Costs == 0)
                    return 0;
                else
                {
                    decimal budgetCostsPercentage = Costs / BudgetedCosts;

                    //fix rounding issue where it goes beyond 100%
                    if (budgetCostsPercentage > 1)
                        budgetCostsPercentage = 1;

                    return budgetCostsPercentage;
                }
            }
        }

        public decimal CostsPercentage
        {
            get
            {
                if (TotalCosts == 0 || Costs == 0)
                    return 0;
                else
                {
                    decimal costsPercentage = Costs / TotalCosts;

                    //fix rounding issue where it goes beyond 100%
                    if (costsPercentage > 1)
                        costsPercentage = 1;

                    return costsPercentage;
                }
            }
        }

        public decimal ReportUnitsPercentage
        {
            get
            {
                if (IsReportBudgetPercentage)
                    return BudgetUnitsPercentage;

                return UnitsPercentage;
            }
        }

        public decimal ReportCostsPercentage
        {
            get
            {
                if (IsReportBudgetPercentage)
                    return BudgetCostsPercentage;

                return CostsPercentage;
            }
        }
    }
}