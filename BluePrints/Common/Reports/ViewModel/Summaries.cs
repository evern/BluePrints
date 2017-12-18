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

        public DeliverableSummaryStats(IEnumerable<BASELINE_ITEMProgress> progressItem, DateTime reporting_data_date, TimeSpan reporting_interval, DateTime first_aligned_data_date, IEnumerable<VariationAdjustment> projectVariationAdjustments)
            : base(progressItem, reporting_data_date, reporting_interval, first_aligned_data_date, projectVariationAdjustments)
        {
            ProjectionHelpers.Initialize_Stats(progressItem, projectVariationAdjustments, reporting_data_date, reporting_interval, first_aligned_data_date, false);
        }
    }

    public class ProjectSummaryStats : SummaryStats
    {
        #region Progress Error Log
        readonly DateTime reporting_data_date;
        readonly TimeSpan reporting_interval;
        readonly DateTime first_aligned_data_date;
        public List<SUBJOB> ExoMissingSUBJOBS { get; private set; }
        #endregion

        public ProjectSummaryStats(IEnumerable<IReportable> progressItem, DateTime reporting_data_date, TimeSpan reporting_interval, DateTime first_aligned_data_date, IEnumerable<VariationAdjustment> projectVariationAdjustments)
            : base(progressItem, reporting_data_date, reporting_interval, first_aligned_data_date, projectVariationAdjustments)
        {
            this.reporting_data_date = reporting_data_date;
            this.reporting_interval = reporting_interval;
            this.first_aligned_data_date = first_aligned_data_date;
            ExoMissingSUBJOBS = new List<SUBJOB>();
            ProjectionHelpers.Initialize_Stats(progressItem, projectVariationAdjustments, reporting_data_date, reporting_interval, first_aligned_data_date, false);
        }

        public IEnumerable<ExoDataPoint> GetBurnedDataPoints()
        {
            return this.Burned.GetData().Select(x => (ExoDataPoint)x);
        }

        public void AddMissingExoSubjob(SUBJOB SUBJOB)
        {
            if(!ExoMissingSUBJOBS.Any(x => x.INTERNAL_NAME1 == SUBJOB.INTERNAL_NAME1))
                ExoMissingSUBJOBS.Add(SUBJOB);
        }
    }

    public static class SummaryStatsHelpers
    {
        public static SummaryStats Group_Summary_Stats(SummaryStats summary_stats, Func<IReportable, bool> reportable_predicate, Func<ExoDataPoint, bool> predicate)
        {
            if (summary_stats == null || summary_stats.Reportables.Count() == 0)
                return null;

            //set budgeted, current and earned
            IEnumerable<IReportable> grouped_reportables = summary_stats.Reportables.Where(reportable_predicate);

            DateTime reporting_data_date = summary_stats.ReportingDataDate;
            TimeSpan reporting_interval = summary_stats.ReportingInterval;
            DateTime first_aligned_data_date = summary_stats.FirstAlignedDataDate;

            List<VariationAdjustment> grouped_variation_adjustments = grouped_reportables.SelectMany(x => x.Stats.VariationAdjustments).ToList();
            SummaryStats grouped_summary_stats = new SummaryStats(grouped_reportables, reporting_data_date, reporting_interval, first_aligned_data_date, grouped_variation_adjustments);
            grouped_summary_stats.GenerateSummary();

            IEnumerable<ExoDataPoint> burned_data_points = summary_stats.Burned.GetData().Select(x => (ExoDataPoint)x);
            IEnumerable<ExoDataPoint> actual_data_points = summary_stats.Actual.GetData().Select(x => (ExoDataPoint)x);
            List<ExoDataPoint> burnedRawDataPoints = burned_data_points.Where(predicate).ToList();
            grouped_summary_stats.Burned.SetData(burnedRawDataPoints);
            List<ExoDataPoint> actualRawDataPoints = actual_data_points.Where(predicate).ToList();
            grouped_summary_stats.Actual.SetData(actualRawDataPoints);
            grouped_summary_stats.RecalculateStats(false);

            if (grouped_reportables.Count() == 0 && burnedRawDataPoints.Count() == 0 && actualRawDataPoints.Count() == 0)
                return null;

            return grouped_summary_stats;
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
        public SummaryStats(IEnumerable<IReportable> progressItem, DateTime reporting_data_date, TimeSpan reporting_interval, DateTime first_aligned_data_date, IEnumerable<VariationAdjustment> projectVariationAdjustments)
            : base(reporting_data_date, reporting_interval, first_aligned_data_date, progressItem.Sum(x => x.Estimated_Units), progressItem.Sum(x => x.Total_Units), progressItem.Sum(x => x.Estimated_Costs), progressItem.Sum(x => x.Total_Costs), projectVariationAdjustments)
        {
            Reportables = progressItem;

            //Since this is only used by subjob to rolldown from project, progress already have stats
            ProjectionHelpers.Initialize_Stats(progressItem, projectVariationAdjustments, reporting_data_date, reporting_interval, first_aligned_data_date, true);
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

        public decimal TotalUnits
        {
            get { return totalUnits; }
        }

        public decimal BudgetedUnits
        {
            get { return budgetedUnits; }
        }

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

        public ProgressStats(DateTime reporting_data_date, TimeSpan reporting_interval, DateTime first_aligned_data_date, decimal budgetedUnits, decimal totalUnits, decimal budgetedCosts, decimal totalCosts, IEnumerable<VariationAdjustment> variationAdjustments)
        {
            this.ReportingDataDate = reporting_data_date;
            this.ReportingInterval = reporting_interval;
            this.FirstAlignedDataDate = first_aligned_data_date;

            this.budgetedUnits = budgetedUnits;
            this.totalUnits = totalUnits;
            this.budgetedCosts = budgetedCosts;
            this.totalCosts = totalCosts;
            this.VariationAdjustments = variationAdjustments.ToList();

            //Budgeted = new Stats(ReportingDataDate, budgetedUnits, totalUnits, budgetedCosts, totalCosts, FirstAlignedDataDate, ReportingInterval, variationAdjustments, false, true);
            Budgeted = new Stats(ReportingDataDate, budgetedUnits, budgetedUnits, budgetedCosts, budgetedCosts, FirstAlignedDataDate, ReportingInterval, variationAdjustments, false, true);
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

            //Budgeted = new Stats(ReportingDataDate, budgetedUnits, totalUnits, BudgetedCosts, totalCosts, FirstAlignedDataDate, ReportingInterval, VariationAdjustments, false, true);
            Budgeted = new Stats(ReportingDataDate, budgetedUnits, budgetedUnits, budgetedCosts, budgetedCosts, FirstAlignedDataDate, ReportingInterval, VariationAdjustments, false, true);
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