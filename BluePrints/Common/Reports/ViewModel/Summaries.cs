using BluePrints.Common.Misc;
using BluePrints.Common.Projections;
using BluePrints.Data;
using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace BluePrints.Common.ViewModel.Reporting
{
    /// <summary>
    /// IReportable wrapper so that baseline_itemProgress properties can be retrieved for reporting purpose
    /// </summary>
    public class DeliverableSummaryStats : ProjectSummaryStats
    {
        public IEnumerable<BASELINE_ITEMProgress> Deliverables
        {
            get { return (IEnumerable<BASELINE_ITEMProgress>)this.Reportables; }
        }

        public DeliverableSummaryStats(IEnumerable<BASELINE_ITEMProgress> progressItem, DateTime reporting_data_date, TimeSpan reporting_interval, DateTime first_aligned_data_date)
            : base(progressItem, reporting_data_date, reporting_interval, first_aligned_data_date)
        {
            ProjectionHelpers.Initialize_Stats(progressItem, reporting_data_date, reporting_interval, first_aligned_data_date, false);
        }
    }

    public class ProjectSummaryStats : WBSSummary
    {
        #region Progress Error Log
        readonly DateTime reporting_data_date;
        readonly TimeSpan reporting_interval;
        readonly DateTime first_aligned_data_date;
        public List<SUBJOB> ExoMissingSUBJOBS { get; private set; }
        #endregion

        public ProjectSummaryStats(IEnumerable<IReportable> progressItem, DateTime reporting_data_date, TimeSpan reporting_interval, DateTime first_aligned_data_date, bool forceRetrieveRemainingDataPoints = false, bool allowPercentageOnZeroTotalUnits = false)
            : base(progressItem, reporting_data_date, reporting_interval, first_aligned_data_date, forceRetrieveRemainingDataPoints, allowPercentageOnZeroTotalUnits)
        {
            this.reporting_data_date = reporting_data_date;
            this.reporting_interval = reporting_interval;
            this.first_aligned_data_date = first_aligned_data_date;
            ExoMissingSUBJOBS = new List<SUBJOB>();
        }

        public ProjectSummaryStats(List<WBSReportable> WBSReportables, DateTime reporting_data_date, TimeSpan reporting_interval, DateTime first_aligned_data_date, bool forceRetrieveRemainingDataPoints = false, bool allowPercentageOnZeroTotalUnits = false)
            : base(WBSReportables, reporting_data_date, reporting_interval, first_aligned_data_date, forceRetrieveRemainingDataPoints, allowPercentageOnZeroTotalUnits)
        {
            this.reporting_data_date = reporting_data_date;
            this.reporting_interval = reporting_interval;
            this.first_aligned_data_date = first_aligned_data_date;
            ExoMissingSUBJOBS = new List<SUBJOB>();
        }

        public IEnumerable<ExoDataPoint> GetBurnedDataPoints()
        {
            return this.Burned.GetData().Select(x => (ExoDataPoint)x);
        }

        public IEnumerable<ExoDataPoint> GetMaterialDataPoints()
        {
            return this.Material.GetData().Select(x => (ExoDataPoint)x);
        }

        public IEnumerable<ExoDataPoint> GetPODataPoints()
        {
            return this.PO.GetData().Select(x => (ExoDataPoint)x);
        }

        public void AddMissingExoSubjob(SUBJOB SUBJOB)
        {
            if(!ExoMissingSUBJOBS.Any(x => x.INTERNAL_NAME1 == SUBJOB.INTERNAL_NAME1))
                ExoMissingSUBJOBS.Add(SUBJOB);
        }

        public override void GenerateSummary()
        {
            this.Budgeted.SetData(WBSReportables.SelectMany(x => x.Budgeted.GetData()));
            this.BudgetedLate.SetData(WBSReportables.SelectMany(x => x.BudgetedLate.GetData()));
            this.Current.SetData(WBSReportables.SelectMany(x => x.Current.GetData()));
            this.Earned.SetData(WBSReportables.SelectMany(x => x.Earned.GetData()));
            this.TenderEarned.SetData(WBSReportables.SelectMany(x => x.TenderEarned.GetData()));
            this.Remaining.SetData(WBSReportables.SelectMany(x => x.Remaining.GetData()));
        }
    }

    public static class SummaryStatsHelpers
    {
        public static SummaryStats Group_Summary_Stats(SummaryStats summary_stats, Func<IReportable, bool> reportable_predicate, Func<ExoDataPoint, bool> predicate, bool forceRetrieveRemainingDataPoints = false)
        {
            if (summary_stats == null)
                return null;

            if (summary_stats.Reportables.Count() == 0)
                summary_stats.Reportables = new List<IReportable>();

            //set budgeted, current and earned
            IEnumerable<IReportable> grouped_reportables = summary_stats.Reportables.Where(reportable_predicate);

            DateTime reporting_data_date = summary_stats.ReportingDataDate;
            TimeSpan reporting_interval = summary_stats.ReportingInterval;
            DateTime first_aligned_data_date = summary_stats.FirstAlignedDataDate;

            SummaryStats grouped_summary_stats = new SummaryStats(grouped_reportables, reporting_data_date, reporting_interval, first_aligned_data_date, forceRetrieveRemainingDataPoints);
            grouped_summary_stats.GenerateSummary();

            IEnumerable<ExoDataPoint> burned_data_points = summary_stats.Burned.GetData().Select(x => (ExoDataPoint)x);
            IEnumerable<ExoDataPoint> actual_data_points = summary_stats.Actual.GetData().Select(x => (ExoDataPoint)x);
            IEnumerable<ExoDataPoint> material_data_points = summary_stats.Material.GetData().Select(x => (ExoDataPoint)x);
            IEnumerable<ExoDataPoint> po_data_points = summary_stats.PO.GetData().Select(x => (ExoDataPoint)x);
            IEnumerable<ExoDataPoint> previousPO_data_points = summary_stats.PreviousPO.GetData().Select(x => (ExoDataPoint)x);
            //IEnumerable<DataPoint> remaining_actual_data_points = summary_stats.RemainingActual.GetData();

            List<ExoDataPoint> burnedRawDataPoints = burned_data_points.Where(predicate).ToList();
            grouped_summary_stats.Burned.SetData(burnedRawDataPoints);
            List<ExoDataPoint> actualRawDataPoints = actual_data_points.Where(predicate).ToList();
            grouped_summary_stats.Actual.SetData(actualRawDataPoints);
            List<ExoDataPoint> materialRawDataPoints = material_data_points.Where(predicate).ToList();
            grouped_summary_stats.Material.SetData(materialRawDataPoints);
            List<ExoDataPoint> poRawDataPoints = po_data_points.Where(predicate).ToList();
            grouped_summary_stats.PO.SetData(poRawDataPoints);
            List<ExoDataPoint> poPreviousRawDataPoints = previousPO_data_points.Where(predicate).ToList();
            grouped_summary_stats.PreviousPO.SetData(poPreviousRawDataPoints);

            //Cannot uset setdata on remaining actual because there's no Func<DataPoint, bool> predicate to apply filter on data
            //grouped_summary_stats.RemainingActual.SetData(remaining_actual_data_points);
            grouped_summary_stats.RemainingActual.SetRemainingActualData(grouped_summary_stats.Reportables, grouped_summary_stats.Burned.GetData());
            grouped_summary_stats.RecalculateStats(false);

            //if (grouped_reportables.Count() == 0 && burnedRawDataPoints.Count() == 0 && actualRawDataPoints.Count() == 0)
            //    return null;

            return grouped_summary_stats;
        }
    }

    public class SummaryStats : ProgressStats
    {
        #region Compulsory Parameters
        public IEnumerable<IReportable> Reportables { get; set; }
        #endregion

        #region Local Variables
        public Stats Actual { get; set; }
        public Stats Material { get; set; }
        public Stats PO { get; set; }
        public Stats PreviousPO { get; set; }
        #endregion

        #region Progress Stats Summary
        public decimal GrossProfit { get; set; }
        public decimal EfficiencyRatio { get; set; }
        public decimal ProgressRatio { get; set; }
        #endregion

        #region Metrics
        public decimal CumulativeEarned_Units => (Earned == null || Earned.CurrentPeriodCumulativeDataPoint == null) ? 0 : Earned.CurrentPeriodCumulativeDataPoint.Units;
        public decimal PeriodEarned_Units => (Earned == null || Earned.CurrentPeriodDataPoint == null) ? 0 : Earned.CurrentPeriodDataPoint.Units;
        public decimal CumulativeBurned_Units => (Burned == null || Burned.CurrentPeriodCumulativeDataPoint == null) ? 0 : Burned.CurrentPeriodCumulativeDataPoint.Units;
        public decimal PeriodBurned_Units => (Burned == null || Burned.CurrentPeriodDataPoint == null) ? 0 : Burned.CurrentPeriodDataPoint.Units;
        public decimal CumulativeEarned_Quantity => (Earned == null || Earned.CurrentPeriodCumulativeDataPoint == null) ? 0 : Earned.CurrentPeriodCumulativeDataPoint.Quantity;

        public decimal CumulativeEarned_Costs => (Earned == null || Earned.CurrentPeriodCumulativeDataPoint == null) ? 0 : Earned.CurrentPeriodCumulativeDataPoint.Costs;
        public decimal PeriodEarned_Costs => (Earned == null || Earned.CurrentPeriodDataPoint == null) ? 0 : Earned.CurrentPeriodDataPoint.Costs;
        public decimal CumulativeBurned_Costs => (Burned == null || Burned.CurrentPeriodCumulativeDataPoint == null) ? 0 : Burned.CurrentPeriodCumulativeDataPoint.Costs;
        public decimal PeriodBurned_Costs => (Burned == null || Burned.CurrentPeriodDataPoint == null) ? 0 : Burned.CurrentPeriodDataPoint.Costs;
        public decimal PeriodEarned_Quantity => (Earned == null || Earned.CurrentPeriodDataPoint == null) ? 0 : Earned.CurrentPeriodDataPoint.Quantity;


        public decimal CumulativeEarnedVsBurned_Units => CumulativeEarned_Units - CumulativeBurned_Units;
        public decimal CumulativeEarnedVsBurned_Costs => CumulativeEarned_Costs - CumulativeBurned_Costs;

        public decimal PeriodEarnedVsBurned_Units => PeriodEarned_Units - PeriodBurned_Units;
        public decimal PeriodEarnedVsBurned_Costs => PeriodEarned_Costs - PeriodBurned_Costs;

        public decimal CumulativePerformanceRatio_Units => CumulativeBurned_Units == 0 ? 1 : CumulativeEarned_Units / CumulativeBurned_Units;
        public decimal CumulativePerformanceRatio_Costs => CumulativeBurned_Costs == 0 ? 1 : CumulativeEarned_Costs / CumulativeBurned_Costs;

        public decimal PeriodPerformanceRatio_Units => PeriodBurned_Units == 0 ? 1 : PeriodEarned_Units / PeriodBurned_Units;
        public decimal PeriodPerformanceRatio_Costs => PeriodBurned_Costs == 0 ? 1 : PeriodEarned_Costs / PeriodBurned_Costs;

        public decimal Remaining_Quantity => TotalQty - CumulativeEarned_Quantity;
        public decimal Remaining_Units => TotalUnits - CumulativeEarned_Units;
        public decimal Remaining_Costs => TotalCosts - CumulativeEarned_Costs;

        public decimal AdjustedRemaining_Units => PeriodPerformanceRatio_Units * Remaining_Units;
        public decimal AdjustedRemaining_Costs => PeriodPerformanceRatio_Costs * Remaining_Costs;

        public decimal AdjustedDifference_Units => Remaining_Units - AdjustedRemaining_Units;
        public decimal AdjustedDifference_Costs => Remaining_Costs - AdjustedRemaining_Costs;
        #endregion

        /// <summary>
        /// Initializes a standard summary from a collection of deliverable projection with progress
        /// </summary>
        /// <param name="progressItem">Deliverable projection with progress</param>
        /// <param name="livePROGRESS">Live progress for reporting data date, generating first aligned data date and interval</param>
        /// <param name="projectVariationAdjustments">Project variation adjustments that will be matched against each deliverable projection</param>
        /// <param name="progressItemHaveStats">Deliverable projection stats area already generated</param>
        public SummaryStats(IEnumerable<IReportable> progressItem, DateTime reporting_data_date, TimeSpan reporting_interval, DateTime first_aligned_data_date, bool forceRetrieveRemainingDataPoints = false, bool allowPercentageOnZeroTotalUnits = false)
            : this(reporting_data_date, reporting_interval, first_aligned_data_date, progressItem.Sum(x => x.Budget_Units), progressItem.Sum(x => x.Total_Units), progressItem.Sum(x => x.Budget_Quantity), progressItem.Sum(x => x.Total_Quantity), progressItem.Sum(x => x.Budget_Costs), progressItem.Sum(x => x.Total_Costs), forceRetrieveRemainingDataPoints, allowPercentageOnZeroTotalUnits)
        {
            Reportables = progressItem;
        }

        /// <summary>
        /// Initializes a standard summary from a collection of deliverable projection with progress
        /// </summary>
        /// <param name="progressItem">Deliverable projection with progress</param>
        /// <param name="livePROGRESS">Live progress for reporting data date, generating first aligned data date and interval</param>
        /// <param name="projectVariationAdjustments">Project variation adjustments that will be matched against each deliverable projection</param>
        /// <param name="progressItemHaveStats">Deliverable projection stats area already generated</param>
        public SummaryStats(DateTime reporting_data_date, TimeSpan reporting_interval, DateTime first_aligned_data_date, decimal budgetedUnits, decimal totalUnits, decimal budgetedQty, decimal totalQty, decimal budgetedCosts, decimal totalCosts, bool forceRetrieveRemainingDataPoints = false, bool allowPercentageOnZeroTotalUnits = false)
            : base(reporting_data_date, reporting_interval, first_aligned_data_date, budgetedUnits, totalUnits, budgetedQty, totalQty, budgetedCosts, totalCosts, null, forceRetrieveRemainingDataPoints, allowPercentageOnZeroTotalUnits)
        {
            Reportables = new List<IReportable>();
            Actual = new Stats(reporting_data_date, budgetedUnits, totalUnits, budgetedQty, totalQty, budgetedCosts, totalCosts, first_aligned_data_date, reporting_interval);
            Material = new Stats(reporting_data_date, budgetedUnits, totalUnits, budgetedQty, totalQty, budgetedCosts, totalCosts, first_aligned_data_date, reporting_interval);
            PO = new Stats(reporting_data_date, budgetedUnits, totalUnits, budgetedQty, totalQty, budgetedCosts, totalCosts, first_aligned_data_date, reporting_interval);
            PreviousPO = new Stats(reporting_data_date, budgetedUnits, totalUnits, budgetedQty, totalQty, budgetedCosts, totalCosts, first_aligned_data_date, reporting_interval);
            RemainingActual = new Stats(reporting_data_date, budgetedUnits, totalUnits, budgetedQty, totalQty, budgetedCosts, totalCosts, first_aligned_data_date, reporting_interval, !forceRetrieveRemainingDataPoints, false, null, forceRetrieveRemainingDataPoints);
        }

        /// <summary>
        /// Initializes an aggregate summary from a collection of summary stats
        /// </summary>
        /// <param name="summaryStats">Collection of summary stats to be summed</param>
        public SummaryStats(IEnumerable<SummaryStats> summaryStats)
            : base(summaryStats)
        {
            IEnumerable<SummaryStats> cleanSummaryStats = summaryStats.Where(x => x != null);
            Actual = new Stats(ReportingDataDate, BudgetedUnits, TotalUnits, BudgetedQty, TotalQty, BudgetedCosts, TotalCosts, FirstAlignedDataDate, ReportingInterval);
            Actual.SetData(cleanSummaryStats.Where(x => x.Actual != null && x.Actual.DataPoints != null).SelectMany(x => x.Actual.DataPoints).ToList());

            Material = new Stats(ReportingDataDate, BudgetedUnits, TotalUnits, BudgetedQty, TotalQty, BudgetedCosts, TotalCosts, FirstAlignedDataDate, ReportingInterval);
            Material.SetData(cleanSummaryStats.Where(x => x.Material != null && x.Material.DataPoints != null).SelectMany(x => x.Material.DataPoints).ToList());

            PO = new Stats(ReportingDataDate, BudgetedUnits, TotalUnits, BudgetedQty, TotalQty, BudgetedCosts, TotalCosts, FirstAlignedDataDate, ReportingInterval);
            PO.SetData(cleanSummaryStats.Where(x => x.PO != null && x.PO.DataPoints != null).SelectMany(x => x.PO.DataPoints).ToList());

            PreviousPO = new Stats(ReportingDataDate, BudgetedUnits, TotalUnits, BudgetedQty, TotalQty, BudgetedCosts, TotalCosts, FirstAlignedDataDate, ReportingInterval);
            PreviousPO.SetData(cleanSummaryStats.Where(x => x.PreviousPO != null && x.PreviousPO.DataPoints != null).SelectMany(x => x.PreviousPO.DataPoints).ToList());
        }

        /// <summary>
        /// Initializes an aggregate summary from a collection of summary stats with filtered dates
        /// </summary>
        /// <param name="summaryStats">Collection of summary stats to be summed</param>
        public SummaryStats(SummaryStats summaryStats, DateTime endDate)
            : base(summaryStats, endDate)
        {
            if (summaryStats == null)
                return;

            Reportables = summaryStats.Reportables.ToList();

            Burned = new Stats(ReportingDataDate, BudgetedUnits, TotalUnits, BudgetedQty, TotalQty, BudgetedCosts, TotalCosts, FirstAlignedDataDate, ReportingInterval);
            if (summaryStats.Burned != null && summaryStats.Burned.DataPoints != null)
                Budgeted.SetData(summaryStats.Burned.GetData().ToList());

            Actual = new Stats(ReportingDataDate, BudgetedUnits, TotalUnits, BudgetedQty, TotalQty, BudgetedCosts, TotalCosts, FirstAlignedDataDate, ReportingInterval);
            if (summaryStats.Actual != null && summaryStats.Actual.DataPoints != null)
                Actual.SetData(summaryStats.Actual.GetData().ToList());

            Material = new Stats(ReportingDataDate, BudgetedUnits, TotalUnits, BudgetedQty, TotalQty, BudgetedCosts, TotalCosts, FirstAlignedDataDate, ReportingInterval);
            if (summaryStats.Material != null && summaryStats.Material.DataPoints != null)
                Material.SetData(summaryStats.Material.GetData().ToList());

            PO = new Stats(ReportingDataDate, BudgetedUnits, TotalUnits, BudgetedQty, TotalQty, BudgetedCosts, TotalCosts, FirstAlignedDataDate, ReportingInterval);
            if (summaryStats.PO != null && summaryStats.PO.DataPoints != null)
                PO.SetData(summaryStats.PO.GetData().ToList());

            PreviousPO = new Stats(ReportingDataDate, BudgetedUnits, TotalUnits, BudgetedQty, TotalQty, BudgetedCosts, TotalCosts, FirstAlignedDataDate, ReportingInterval);
            if (summaryStats.PreviousPO != null && summaryStats.PreviousPO.DataPoints != null)
                PreviousPO.SetData(summaryStats.PreviousPO.GetData().ToList());

            RemainingActual = new Stats(ReportingDataDate, BudgetedUnits, TotalUnits, BudgetedQty, TotalQty, BudgetedCosts, TotalCosts, FirstAlignedDataDate, ReportingInterval, true);
            if (summaryStats.RemainingActual != null && summaryStats.RemainingActual.DataPoints != null)
                RemainingActual.SetData(summaryStats.RemainingActual.GetData().Where(x => x.ProgressDate < endDate).ToList());
        }

        public virtual void GenerateSummary()
        {
            this.Budgeted.SetData(Reportables.SelectMany(x => x.Stats.Budgeted.GetData()));
            this.BudgetedLate.SetData(Reportables.SelectMany(x => x.Stats.BudgetedLate.GetData()));
            this.Current.SetData(Reportables.SelectMany(x => x.Stats.Current.GetData()));
            this.Earned.SetData(Reportables.SelectMany(x => x.Stats.Earned.GetData()));
            this.TenderEarned.SetData(Reportables.SelectMany(x => x.Stats.TenderEarned.GetData()));
            this.Remaining.SetData(Reportables.SelectMany(x => x.Stats.Remaining.GetData()));
            //Remaining actual canno be summarize here because it contains elements from actuals
            //this.RemainingActual.SetData(Reportables.SelectMany(x => x.Stats.RemainingActual.GetData()));
        }

        public void GenerateActualRemainingSummary()
        {

        }

        public void RecalculateStats(bool isCost = false)
        {
            if (Earned.CurrentPeriodCumulativeDataPoint != null && Actual != null && Actual.CurrentPeriodCumulativeDataPoint != null)
            {
                var totalEarnedCost = Earned.CurrentPeriodCumulativeDataPoint.Costs;
                var totalActualCost = Actual.CurrentPeriodCumulativeDataPoint.Costs;

                GrossProfit = totalEarnedCost == 0 || totalActualCost == 0
                    ? 0
                    : (totalEarnedCost - totalActualCost) / totalEarnedCost;
            }

            decimal totalEarnedUOM = 0;
            decimal totalBurnedUOM = 0;
            decimal totalPlannedUOM = 0;

            if (isCost)
            {
                if (Earned.CurrentPeriodCumulativeDataPoint != null)
                    totalEarnedUOM = Earned.CurrentPeriodCumulativeDataPoint.Costs;
                if (Burned.CurrentPeriodCumulativeDataPoint != null)
                    totalBurnedUOM = Burned.CurrentPeriodCumulativeDataPoint.Costs;
                if (Budgeted.CurrentPeriodCumulativeDataPoint != null)
                    totalPlannedUOM = Budgeted.CurrentPeriodCumulativeDataPoint.Costs;
            }
            else
            {
                if (Earned.CurrentPeriodCumulativeDataPoint != null)
                    totalEarnedUOM = Earned.CurrentPeriodCumulativeDataPoint.Units;
                if (Burned.CurrentPeriodCumulativeDataPoint != null)
                    totalBurnedUOM = Burned.CurrentPeriodCumulativeDataPoint.Units;
                if (Budgeted.CurrentPeriodCumulativeDataPoint != null)
                    totalPlannedUOM = Budgeted.CurrentPeriodCumulativeDataPoint.Units;
            }

            EfficiencyRatio = totalBurnedUOM == 0
                ? 0
                : (totalEarnedUOM - totalBurnedUOM) / totalBurnedUOM;
            ProgressRatio = totalPlannedUOM == 0
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

        public Stats BudgetedLate
        {
            get { return GetProperty(() => BudgetedLate); }
            set { SetProperty(() => BudgetedLate, value); }
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

        public Stats Burned
        {
            get { return GetProperty(() => Burned); }
            set { SetProperty(() => Burned, value); }
        }

        public Stats TenderEarned
        {
            get { return GetProperty(() => TenderEarned); }
            set { SetProperty(() => TenderEarned, value); }
        }

        public Stats Remaining
        {
            get { return GetProperty(() => Remaining); }
            set { SetProperty(() => Remaining, value); }
        }

        public Stats RemainingActual
        {
            get { return GetProperty(() => RemainingActual); }
            set { SetProperty(() => RemainingActual, value); }
        }

        readonly decimal budgetedUnits;
        public readonly decimal totalUnits;
        readonly decimal budgetedQty;
        public readonly decimal totalQty;
        public readonly decimal budgetedCosts;
        public readonly decimal totalCosts;
        public bool AllowPercentageOnZeroTotalUnits { get; set; }
        #endregion

        public decimal ExoBudgetQty { get; set; }
        public decimal ExoBudgetCosts { get; set; }

        public decimal TotalUnits
        {
            get { return totalUnits; }
        }

        public decimal BudgetedUnits
        {
            get { return budgetedUnits; }
        }

        public decimal BudgetedQty => budgetedQty;

        public decimal TotalQty => totalQty;

        public decimal BudgetedCosts
        {
            get { return budgetedCosts; }
        }

        public decimal TotalCosts
        {
            get { return totalCosts; }
        }

        public decimal BaselineProductivity
        {
            get
            {
                if (Budgeted == null || Budgeted.DataPoints == null || Budgeted.DataPoints.Count == 0)
                    return 0;


                decimal budgeted_units = Budgeted.GetApplicableRemainingProductivityCalculationBudgetedUnits();
                decimal budgeted_duration = Budgeted.GetApplicableProductivityCalculationBudgetedDuration();
                if (budgeted_duration == 0)
                    return 0;

                return budgeted_units / budgeted_duration;
            }
        }

        public decimal RemainingProductivity
        {
            get
            {
                if (Remaining == null || Remaining.DataPoints == null || Remaining.DataPoints.Count == 0)
                    return 0;

                decimal remaining_units = Remaining.GetApplicableRemainingProductivityCalculationRemainingUnits();
                decimal remaining_duration = Remaining.GetApplicableProductivityCalculationRemainingDuration();
                if (remaining_duration == 0)
                    return 0;

                return remaining_units / remaining_duration;
            }
        }

        public ProgressStats(DateTime reporting_data_date, TimeSpan reporting_interval, DateTime first_aligned_data_date, decimal budgetedUnits, decimal totalUnits, decimal budgetedQty, decimal totalQty, decimal budgetedCosts, decimal totalCosts, DateTime? extrapolateDate = null, bool forceRetrieveRemainingDataPoints = false, bool allowPercentageOnZeroTotalUnits = false)
        {
            this.ReportingDataDate = reporting_data_date;
            this.ReportingInterval = reporting_interval;
            this.FirstAlignedDataDate = first_aligned_data_date;

            this.budgetedUnits = budgetedUnits;

            if (allowPercentageOnZeroTotalUnits && totalUnits == 0)
                totalUnits = BluePrintsConstants.DurationBasedTotalUnits;

            this.totalUnits = totalUnits;
            this.budgetedQty = budgetedQty;
            this.totalQty = totalQty;
            this.budgetedCosts = budgetedCosts;
            this.totalCosts = totalCosts;
            this.AllowPercentageOnZeroTotalUnits = allowPercentageOnZeroTotalUnits;

            Budgeted = new Stats(reporting_data_date, budgetedUnits, totalUnits, budgetedQty, budgetedQty, budgetedCosts, budgetedCosts, first_aligned_data_date, reporting_interval, false, true, extrapolateDate);
            BudgetedLate = new Stats(reporting_data_date, budgetedUnits, totalUnits, budgetedQty, budgetedQty, budgetedCosts, budgetedCosts, first_aligned_data_date, reporting_interval, false, true, extrapolateDate);
            Current = new Stats(reporting_data_date, budgetedUnits, totalUnits, budgetedQty, totalQty, budgetedCosts, totalCosts, first_aligned_data_date, reporting_interval, false, true, extrapolateDate);
            Earned = new Stats(reporting_data_date, budgetedUnits, totalUnits, budgetedQty, totalQty, budgetedCosts, totalCosts, first_aligned_data_date, reporting_interval, false, false, extrapolateDate);
            TenderEarned = new Stats(reporting_data_date, budgetedUnits, budgetedUnits, budgetedQty, budgetedQty, budgetedCosts, budgetedCosts, first_aligned_data_date, reporting_interval, false, true, extrapolateDate);
            Burned = new Stats(reporting_data_date, budgetedUnits, totalUnits, budgetedQty, totalQty, budgetedCosts, totalCosts, first_aligned_data_date, reporting_interval);
            Remaining = new Stats(reporting_data_date, budgetedUnits, totalUnits, budgetedQty, totalQty, budgetedCosts, totalCosts, first_aligned_data_date, reporting_interval, !forceRetrieveRemainingDataPoints, false, extrapolateDate, forceRetrieveRemainingDataPoints);
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
            budgetedQty = cleanProgressStats.Sum(x => x.budgetedQty);
            totalUnits = cleanProgressStats.Sum(x => x.totalUnits);
            totalQty = cleanProgressStats.Sum(x => x.totalQty);
            this.budgetedCosts = cleanProgressStats.Sum(x => x.BudgetedCosts);
            totalCosts = cleanProgressStats.Sum(x => x.totalCosts);

            Budgeted = new Stats(ReportingDataDate, budgetedUnits, totalUnits, budgetedQty, totalQty, budgetedCosts, totalCosts, FirstAlignedDataDate, ReportingInterval, false, true);
            Budgeted.SetData(cleanProgressStats.Where(x => x.Budgeted != null && x.Budgeted.DataPoints != null).SelectMany(x => x.Budgeted.GetData()).ToList());

            BudgetedLate = new Stats(ReportingDataDate, budgetedUnits, totalUnits, budgetedQty, totalQty, budgetedCosts, totalCosts, FirstAlignedDataDate, ReportingInterval, false, true);
            BudgetedLate.SetData(cleanProgressStats.Where(x => x.BudgetedLate != null && x.BudgetedLate.DataPoints != null).SelectMany(x => x.BudgetedLate.GetData()).ToList());

            Current = new Stats(ReportingDataDate, budgetedUnits, totalUnits, budgetedQty, totalQty, budgetedCosts, totalCosts, FirstAlignedDataDate, ReportingInterval, false, true);
            Current.SetData(cleanProgressStats.Where(x => x.Current != null && x.Current.DataPoints != null).SelectMany(x => x.Current.GetData()).ToList());

            Earned = new Stats(ReportingDataDate, budgetedUnits, totalUnits, budgetedQty, totalQty, budgetedCosts, totalCosts, FirstAlignedDataDate, ReportingInterval, false, false, null);
            Earned.SetData(cleanProgressStats.Where(x => x.Earned != null && x.Earned.DataPoints != null).SelectMany(x => x.Earned.GetData()).ToList());

            Burned = new Stats(ReportingDataDate, BudgetedUnits, totalUnits, budgetedQty, TotalQty, budgetedCosts, totalCosts, FirstAlignedDataDate, ReportingInterval);
            Burned.SetData(cleanProgressStats.Where(x => x.Burned != null && x.Burned.DataPoints != null).SelectMany(x => x.Burned.DataPoints).ToList());

            TenderEarned = new Stats(ReportingDataDate, budgetedUnits, totalUnits, budgetedQty, TotalQty, budgetedCosts, totalCosts, FirstAlignedDataDate, ReportingInterval, false, true);
            TenderEarned.SetData(cleanProgressStats.Where(x => x.TenderEarned != null && x.TenderEarned.DataPoints != null).SelectMany(x => x.TenderEarned.GetData()).ToList());

            Remaining = new Stats(ReportingDataDate, budgetedUnits, totalUnits, budgetedQty, totalQty, budgetedCosts, totalCosts, FirstAlignedDataDate, ReportingInterval, true);
            Remaining.SetData(cleanProgressStats.Where(x => x.Remaining != null && x.Remaining.DataPoints != null).SelectMany(x => x.Remaining.GetData()).ToList());

            RemainingActual = new Stats(ReportingDataDate, budgetedUnits, totalUnits, budgetedQty, totalQty, budgetedCosts, totalCosts, FirstAlignedDataDate, ReportingInterval, true);
            RemainingActual.SetData(cleanProgressStats.Where(x => x.RemainingActual != null && x.RemainingActual.DataPoints != null).SelectMany(x => x.RemainingActual.GetData()).ToList());
        }

        public ProgressStats(ProgressStats progressStat, DateTime endDate)
        {
            if (progressStat == null)
                return;

            this.ReportingDataDate = progressStat.ReportingDataDate;
            this.ReportingInterval = progressStat.ReportingInterval;
            this.FirstAlignedDataDate = progressStat.FirstAlignedDataDate;

            budgetedUnits = progressStat.budgetedUnits;
            budgetedQty = progressStat.budgetedQty;
            totalUnits = progressStat.totalUnits;
            totalQty = progressStat.totalQty;
            this.budgetedCosts = progressStat.BudgetedCosts;
            totalCosts = progressStat.totalCosts;

            Budgeted = new Stats(ReportingDataDate, budgetedUnits, totalUnits, budgetedQty, totalQty, budgetedCosts, totalCosts, FirstAlignedDataDate, ReportingInterval, false, true);
            if(progressStat.Budgeted != null && progressStat.Budgeted.DataPoints != null)
                Budgeted.SetData(progressStat.Budgeted.GetData().ToList());

            BudgetedLate = new Stats(ReportingDataDate, budgetedUnits, totalUnits, budgetedQty, totalQty, budgetedCosts, totalCosts, FirstAlignedDataDate, ReportingInterval, false, true);
            if (progressStat.BudgetedLate != null && progressStat.BudgetedLate.DataPoints != null)
                BudgetedLate.SetData(progressStat.BudgetedLate.GetData().ToList());

            Current = new Stats(ReportingDataDate, budgetedUnits, totalUnits, budgetedQty, totalQty, budgetedCosts, totalCosts, FirstAlignedDataDate, ReportingInterval, false, true);
            if (progressStat.Current != null && progressStat.Current.DataPoints != null)
                Current.SetData(progressStat.Current.GetData().ToList());

            Earned = new Stats(ReportingDataDate, budgetedUnits, totalUnits, budgetedQty, totalQty, budgetedCosts, totalCosts, FirstAlignedDataDate, ReportingInterval);
            if (progressStat.Earned != null && progressStat.Earned.DataPoints != null)
                Earned.SetData(progressStat.Earned.GetData().ToList());

            TenderEarned = new Stats(ReportingDataDate, budgetedUnits, totalUnits, budgetedQty, totalQty, budgetedCosts, totalCosts, FirstAlignedDataDate, ReportingInterval);
            if (progressStat.TenderEarned != null && progressStat.TenderEarned.DataPoints != null)
                TenderEarned.SetData(progressStat.TenderEarned.GetData().ToList());

            Remaining = new Stats(ReportingDataDate, budgetedUnits, totalUnits, budgetedQty, totalQty, budgetedCosts, totalCosts, FirstAlignedDataDate, ReportingInterval, true);
            if (progressStat.Remaining != null && progressStat.Remaining.DataPoints != null)
                Remaining.SetData(progressStat.Remaining.GetData().Where(x => x.ProgressDate < endDate).ToList());
        }
    }
}