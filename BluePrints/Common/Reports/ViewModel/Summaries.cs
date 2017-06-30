using BluePrints.Common.Projections;
using BluePrints.Data;
using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.Common.ViewModel.Reporting
{
    public class ProjectSummaryStats : SummaryStats
    {
        #region Progress Error Log
        readonly PROGRESS progress;
        public List<WORKPACK> ExoMissingWORKPACKS { get; private set; }
        #endregion

        public ProjectSummaryStats(IEnumerable<IReportable> progressItem, PROGRESS livePROGRESS, IEnumerable<VariationAdjustment> projectVariationAdjustments)
            : base(progressItem, livePROGRESS, projectVariationAdjustments)
        {
            progress = livePROGRESS;
            ExoMissingWORKPACKS = new List<WORKPACK>();
            ProjectionHelpers.InitializePROGRESS_ITEMStats(progressItem, projectVariationAdjustments, livePROGRESS, false);
        }

        public SummaryStats GroupStatsByWorkpack(WORKPACK workpack, bool isLegacyProject = true)
        {
            //set budgeted, current and earned
            IEnumerable<IReportable> progressItemStatsByWorkpack = Reportables.Where(x => x.Deliverable.Workpack_Guid == workpack.GUID);

            DateTime progressItemReportingDataDate = this.ReportingDataDate;
            List<VariationAdjustment> workpackVariationAdjustments = progressItemStatsByWorkpack.SelectMany(x => x.Stats.VariationAdjustments).ToList();
            SummaryStats workpackSummaryStats = new SummaryStats(progressItemStatsByWorkpack, progress, workpackVariationAdjustments);
            //Since workpackSummaryStats is already initialized with workpack specific deliverables, summary will aggreagate deliverable stats
            workpackSummaryStats.GenerateSummary();

            IEnumerable<ExoDataPoint> burnedDataPoints = this.Burned.GetData().Select(x => (ExoDataPoint)x);
            IEnumerable<ExoDataPoint> actualDataPoints = this.Actual.GetData().Select(x => (ExoDataPoint)x);
            List<ExoDataPoint> burnedRawDataPoints = burnedDataPoints.Where(x => x.WorkpackName == workpack.INTERNAL_NAME1).ToList();
            workpackSummaryStats.Burned.SetData(burnedRawDataPoints);
            List<ExoDataPoint> actualRawDataPoints = actualDataPoints.Where(x => x.WorkpackName == workpack.INTERNAL_NAME1).ToList();
            workpackSummaryStats.Actual.SetData(actualRawDataPoints);
            workpackSummaryStats.RecalculateStats(false);

            return workpackSummaryStats;
        }

        public SummaryStats GroupStatsByStockCode(SummaryStats progressItemStatsByWorkpack, string stockCode)
        {
            IEnumerable<IReportable> progressItemStatsByStockCode = progressItemStatsByWorkpack.Reportables.Where(x => x.Deliverable.Stock_Code == stockCode);
            List<VariationAdjustment> stockCodeVariationAdjustments = progressItemStatsByStockCode.SelectMany(x => x.Stats.VariationAdjustments).ToList();
            SummaryStats stockCodeSummary = new SummaryStats(progressItemStatsByStockCode, progress, stockCodeVariationAdjustments);
            stockCodeSummary.GenerateSummary();

            IEnumerable<ExoDataPoint> workpackBurnedDataPoints = progressItemStatsByWorkpack.Burned.GetData().Select(x => (ExoDataPoint)x);
            IEnumerable<ExoDataPoint> workpackActualDataPoints = progressItemStatsByWorkpack.Actual.GetData().Select(x => (ExoDataPoint)x);
            IEnumerable<ExoDataPoint> burnedRawDataPoints = workpackBurnedDataPoints.Where(x => x.StockCode == stockCode);
            stockCodeSummary.Burned.SetData(burnedRawDataPoints);
            IEnumerable<ExoDataPoint> actualRawDataPoints = workpackActualDataPoints.Where(x => x.StockCode == stockCode);
            stockCodeSummary.Actual.SetData(actualRawDataPoints);
            stockCodeSummary.RecalculateStats(false);

            if (progressItemStatsByStockCode.Count() == 0 && burnedRawDataPoints.Count() == 0 && actualRawDataPoints.Count() == 0)
                return null;

            return stockCodeSummary;
        }

        public IEnumerable<ExoDataPoint> GetBurnedDataPoints()
        {
            return this.Burned.GetData().Select(x => (ExoDataPoint)x);
        }

        public SummaryStats GroupStatsByCommodityCode(SummaryStats progressItemStatsByStockCode, string commodityCode)
        {
            IEnumerable<IReportable> progressItemStatsByCommodityCode = progressItemStatsByStockCode.Reportables.Where(x => x.Deliverable.Commodity_Code == commodityCode);
            List<VariationAdjustment> commodityCodeVariationAdjustments = progressItemStatsByCommodityCode.SelectMany(x => x.Stats.VariationAdjustments).ToList();
            SummaryStats commodityCodeSummary = new SummaryStats(progressItemStatsByCommodityCode, progress, commodityCodeVariationAdjustments);
            commodityCodeSummary.GenerateSummary();

            IEnumerable<ExoDataPoint> stockCodeBurnedDataPoints = progressItemStatsByStockCode.Burned.GetData().Select(x => (ExoDataPoint)x);
            IEnumerable<ExoDataPoint> stockCodeActualDataPoints = progressItemStatsByStockCode.Actual.GetData().Select(x => (ExoDataPoint)x);
            IEnumerable<ExoDataPoint> burnedRawDataPoints = stockCodeBurnedDataPoints.Where(x => x.CommodityCode == commodityCode);
            commodityCodeSummary.Burned.SetData(burnedRawDataPoints);
            IEnumerable<ExoDataPoint> actualRawDataPoints = stockCodeActualDataPoints.Where(x => x.CommodityCode == commodityCode);
            commodityCodeSummary.Actual.SetData(actualRawDataPoints);
            commodityCodeSummary.RecalculateStats(false);

            if (progressItemStatsByCommodityCode.Count() == 0 && burnedRawDataPoints.Count() == 0 && actualRawDataPoints.Count() == 0)
                return null;

            return commodityCodeSummary;
        }

        public void AddMissingExoWorkpack(WORKPACK WORKPACK)
        {
            ExoMissingWORKPACKS.Add(WORKPACK);
        }
    }

    public class SummaryStats : ProgressStats
    {
        #region Compulsory Parameters
        public IEnumerable<IReportable> Reportables { get; private set; }
        #endregion

        #region Local Variables
        public Stats Burned { get; set; }
        public Stats Actual { get; set; } 
        #endregion

        #region Progress Stats Summary
        public decimal GrossProfit { get; set; }
        public decimal EfficiencyRatio { get; set; }
        public decimal ProgressRatio { get; set; }
        #endregion

        /// <summary>
        /// Initializes a standard summary from a collection of deliverable projection with progress
        /// </summary>
        /// <param name="progressItem">Deliverable projection with progress</param>
        /// <param name="livePROGRESS">Live progress for reporting data date, generating first aligned data date and interval</param>
        /// <param name="projectVariationAdjustments">Project variation adjustments that will be matched against each deliverable projection</param>
        /// <param name="progressItemHaveStats">Deliverable projection stats area already generated</param>
        public SummaryStats(IEnumerable<IReportable> progressItem, PROGRESS livePROGRESS, IEnumerable<VariationAdjustment> projectVariationAdjustments)
            : base(livePROGRESS, progressItem.Sum(x => x.Deliverable.EstimatedHours), progressItem.Sum(x => x.Deliverable.TotalHours), progressItem.Sum(x => x.Deliverable.EstimatedCosts), progressItem.Sum(x => x.Deliverable.TotalCosts), projectVariationAdjustments)
        {
            Reportables = progressItem;

            //Since this is only used by workpack to rolldown from project, progress already have stats
            ProjectionHelpers.InitializePROGRESS_ITEMStats(progressItem, projectVariationAdjustments,livePROGRESS, true);
            Burned = new Stats(ReportingDataDate, BudgetedUnits, totalUnits, BudgetedCosts, totalCosts, FirstAlignedDataDate, ReportingInterval, VariationAdjustments);
            Actual = new Stats(ReportingDataDate, BudgetedUnits, totalUnits, BudgetedCosts, totalCosts, FirstAlignedDataDate, ReportingInterval, VariationAdjustments);
        }

        /// <summary>
        /// Initializes an aggregate summary from a collection of summary stats
        /// </summary>
        /// <param name="summaryStats">Collection of summary stats to be summed</param>
        public SummaryStats(IEnumerable<SummaryStats> summaryStats)
            : base(summaryStats)
        {
            IEnumerable<SummaryStats> cleanSummaryStats = summaryStats.Where(x => x != null);
            Reportables = cleanSummaryStats.Where(x => x != null).SelectMany(x => x.Reportables).ToList();

            Burned = new Stats(ReportingDataDate, BudgetedUnits, totalUnits, BudgetedCosts, totalCosts, FirstAlignedDataDate, ReportingInterval, VariationAdjustments);
            Burned.SetData(cleanSummaryStats.Where(x => x.Burned != null && x.Burned.DataPoints != null).SelectMany(x => x.Burned.DataPoints).ToList());

            Actual = new Stats(ReportingDataDate, BudgetedUnits, totalUnits, BudgetedCosts, totalCosts, FirstAlignedDataDate, ReportingInterval, VariationAdjustments);
            Actual.SetData(cleanSummaryStats.Where(x => x.Actual != null && x.Actual.DataPoints != null).SelectMany(x => x.Actual.DataPoints).ToList());
        }

        public void GenerateSummary()
        {
            this.Budgeted.SetData(Reportables.SelectMany(x => x.Stats.Budgeted.GetData()));
            this.Current.SetData(Reportables.SelectMany(x => x.Stats.Current.GetData()));
            this.Earned.SetData(Reportables.SelectMany(x => x.Stats.Earned.GetData()));
            this.Remaining.SetData(Reportables.SelectMany(x => x.Stats.Remaining.GetData()));
        }

        public void RecalculateStats(bool isCost = false)
        {
            if (Earned.CurrentPeriodDataPoint != null && Actual != null && Actual.CurrentPeriodDataPoint != null)
            {
                var totalEarnedCost = Earned.CurrentPeriodDataPoint.Costs;
                var totalActualCost = Actual.CurrentPeriodDataPoint.Costs;

                GrossProfit = totalEarnedCost == 0 || totalActualCost == 0
                    ? 0
                    : (totalEarnedCost - totalActualCost) / totalEarnedCost;
            }

            decimal totalEarnedUOM = 0;
            decimal totalBurnedUOM = 0;
            decimal totalPlannedUOM = 0;

            if (isCost)
            {
                if (Earned.CurrentPeriodDataPoint != null)
                    totalEarnedUOM = Earned.CurrentPeriodDataPoint.Costs;
                if (Burned.CurrentPeriodDataPoint != null)
                    totalBurnedUOM = Burned.CurrentPeriodDataPoint.Costs;
                if (Budgeted.CurrentPeriodDataPoint != null)
                    totalPlannedUOM = Budgeted.CurrentPeriodDataPoint.Costs;
            }
            else
            {
                if (Earned.CurrentPeriodDataPoint != null)
                    totalEarnedUOM = Earned.CurrentPeriodDataPoint.Units;
                if (Burned.CurrentPeriodDataPoint != null)
                    totalBurnedUOM = Burned.CurrentPeriodDataPoint.Units;
                if (Budgeted.CurrentPeriodDataPoint != null)
                    totalPlannedUOM = Budgeted.CurrentPeriodDataPoint.Units;
            }

            EfficiencyRatio = totalEarnedUOM == 0 || totalBurnedUOM == 0
                ? 0
                : (totalEarnedUOM - totalBurnedUOM) / totalBurnedUOM;
            ProgressRatio = totalEarnedUOM == 0 || totalPlannedUOM == 0
                ? 0
                : (totalEarnedUOM - totalPlannedUOM) / totalPlannedUOM;
        }
    }

    public class ProgressStats : BindableBase
    {
        #region Compulsory Parameters
        public readonly DateTime ReportingDataDate;
        public readonly TimeSpan ReportingInterval;
        public readonly DateTime FirstAlignedDataDate;
        public Stats Budgeted
        {
            get { return GetProperty(() => Budgeted); }
            set { SetProperty(() => Budgeted, value); }
        }

        public Stats Current
        {
            get { return GetProperty(() => Current); }
            set { SetProperty(() => Current, value); }
        }

        public Stats Earned
        {
            get { return GetProperty(() => Earned); }
            set { SetProperty(() => Earned, value); }
        }

        public Stats Remaining
        {
            get { return GetProperty(() => Remaining); }
            set { SetProperty(() => Remaining, value); }
        }

        readonly decimal budgetedUnits;
        public readonly decimal totalUnits;
        public readonly decimal budgetedCosts;
        public readonly decimal totalCosts;
        public readonly List<VariationAdjustment> VariationAdjustments;
        #endregion

        public decimal BudgetedUnits
        {
            get { return budgetedUnits; }
        }

        public decimal BudgetedCosts
        {
            get { return budgetedCosts; }
        }

        public ProgressStats(PROGRESS livePROGRESS, decimal budgetedUnits, decimal totalUnits, decimal budgetedCosts, decimal totalCosts, IEnumerable<VariationAdjustment> variationAdjustments)
        {
            this.ReportingDataDate = livePROGRESS.DATA_DATE;
            this.ReportingInterval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(livePROGRESS);
            this.FirstAlignedDataDate = ChronologicalHelpers.GenerateFirstAlignedDataDate(livePROGRESS);

            this.budgetedUnits = budgetedUnits;
            this.totalUnits = totalUnits;
            this.budgetedCosts = budgetedCosts;
            this.totalCosts = totalCosts;
            this.VariationAdjustments = variationAdjustments.ToList();

            Budgeted = new Stats(ReportingDataDate, budgetedUnits, totalUnits, budgetedCosts, totalCosts, FirstAlignedDataDate, ReportingInterval, variationAdjustments, false, true);
            Current = new Stats(ReportingDataDate, budgetedUnits, totalUnits, budgetedCosts, totalCosts, FirstAlignedDataDate, ReportingInterval, variationAdjustments);
            Earned = new Stats(ReportingDataDate, budgetedUnits, totalUnits, budgetedCosts, totalCosts, FirstAlignedDataDate, ReportingInterval, variationAdjustments);
            Remaining = new Stats(ReportingDataDate, budgetedUnits, totalUnits, budgetedCosts, totalCosts, FirstAlignedDataDate, ReportingInterval, variationAdjustments, true);
        }

        public ProgressStats(IEnumerable<ProgressStats> progressStats)
        {
            IEnumerable<ProgressStats> cleanProgressStats = progressStats.Where(x => x != null);
            if (cleanProgressStats.Count() == 0)
                return;

            this.ReportingDataDate = cleanProgressStats.Where(x => x.ReportingDataDate != null).Min(x => x.ReportingDataDate);
            this.ReportingInterval = cleanProgressStats.First().ReportingInterval;
            this.FirstAlignedDataDate = cleanProgressStats.Min(x => x.FirstAlignedDataDate);

            budgetedUnits = cleanProgressStats.Sum(x => x.budgetedUnits);
            totalUnits = cleanProgressStats.Sum(x => x.totalUnits);
            this.budgetedCosts = cleanProgressStats.Sum(x => x.BudgetedCosts);
            totalCosts = cleanProgressStats.Sum(x => x.totalCosts);
            this.VariationAdjustments = cleanProgressStats.SelectMany(x => x.VariationAdjustments).ToList();

            Budgeted = new Stats(ReportingDataDate, budgetedUnits, totalUnits, BudgetedCosts, totalCosts, FirstAlignedDataDate, ReportingInterval, VariationAdjustments, false, true);
            Budgeted.SetData(cleanProgressStats.Where(x => x.Budgeted != null && x.Budgeted.DataPoints != null).SelectMany(x => x.Budgeted.GetData()).ToList());

            Current = new Stats(ReportingDataDate, budgetedUnits, totalUnits, BudgetedCosts, totalCosts, FirstAlignedDataDate, ReportingInterval, VariationAdjustments);
            Current.SetData(cleanProgressStats.Where(x => x.Current != null && x.Current.DataPoints != null).SelectMany(x => x.Current.GetData()).ToList());

            Earned = new Stats(ReportingDataDate, budgetedUnits, totalUnits, BudgetedCosts, totalCosts, FirstAlignedDataDate, ReportingInterval, VariationAdjustments);
            Earned.SetData(cleanProgressStats.Where(x => x.Earned != null && x.Earned.DataPoints != null).SelectMany(x => x.Earned.GetData()).ToList());

            Remaining = new Stats(ReportingDataDate, budgetedUnits, totalUnits, BudgetedCosts, totalCosts, FirstAlignedDataDate, ReportingInterval, VariationAdjustments, true);
            Remaining.SetData(cleanProgressStats.Where(x => x.Remaining != null && x.Remaining.DataPoints != null).SelectMany(x => x.Remaining.GetData()).ToList());
        }
    }
}