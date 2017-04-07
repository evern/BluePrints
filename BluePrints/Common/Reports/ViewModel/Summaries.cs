using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Data;
using BluePrints.P6EntitiesDataModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Mvvm.POCO;
using BluePrints.Common.Projections;
using BluePrints.Data.Attributes;

namespace BluePrints.Common.ViewModel.Reporting
{
    public class ProjectSummaryStats : SummaryStats
    {
        #region Progress Error Log
        readonly PROGRESS progress;
        public List<WORKPACK> ExoMissingWORKPACKS { get; private set; }
        #endregion

        public ProjectSummaryStats(IEnumerable<PROGRESS_ITEMProjection> progressItem, PROGRESS livePROGRESS, IEnumerable<VariationAdjustment> projectVariationAdjustments)
            : base(progressItem, livePROGRESS, projectVariationAdjustments)
        {
            progress = livePROGRESS;
            ExoMissingWORKPACKS = new List<WORKPACK>();
            ProjectionHelpers.InitializePROGRESS_ITEMStats(progressItem, projectVariationAdjustments, livePROGRESS, false);
        }

        public SummaryStats GroupBurnedStatsByWorkpack(WORKPACK workpack)
        {
            IEnumerable<PROGRESS_ITEMProjection> progressItemStatsByWorkpack = Deliverable.Where(x => x.Entity.Entity.GUID_WORKPACK == workpack.GUID);

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

        public void AddMissingExoWorkpack(WORKPACK WORKPACK)
        {
            ExoMissingWORKPACKS.Add(WORKPACK);
        }
    }

    public class SummaryStats : ProgressStats
    {
        #region Compulsory Parameters
        public IEnumerable<PROGRESS_ITEMProjection> Deliverable { get; private set; }
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
        public SummaryStats(IEnumerable<PROGRESS_ITEMProjection> progressItem, PROGRESS livePROGRESS, IEnumerable<VariationAdjustment> projectVariationAdjustments)
            : base(livePROGRESS, progressItem.Sum(x => x.Entity.Entity.ESTIMATED_HOURS), progressItem.Sum(x => x.Entity.Entity.TOTAL_HOURS), progressItem.Sum(x => x.Entity.ESTIMATED_COSTS), progressItem.Sum(x => x.Entity.TOTAL_COSTS), projectVariationAdjustments)
        {
            Deliverable = progressItem;

            //Since this is only used by workpack to rolldown from project, progress already have stats
            ProjectionHelpers.InitializePROGRESS_ITEMStats(progressItem, projectVariationAdjustments,livePROGRESS, true);
            Burned = new Stats(ReportingDataDate, BudgetedUnits, TotalUnits, BudgetedCosts, TotalCosts, FirstAlignedDataDate, ReportingInterval, VariationAdjustments);
            Actual = new Stats(ReportingDataDate, BudgetedUnits, TotalUnits, BudgetedCosts, TotalCosts, FirstAlignedDataDate, ReportingInterval, VariationAdjustments);
        }

        /// <summary>
        /// Initializes an aggregate summary from a collection of summary stats
        /// </summary>
        /// <param name="summaryStats">Collection of summary stats to be summed</param>
        public SummaryStats(IEnumerable<SummaryStats> summaryStats)
            : base(summaryStats)
        {
            IEnumerable<SummaryStats> cleanSummaryStats = summaryStats.Where(x => x != null);
            Deliverable = cleanSummaryStats.Where(x => x != null).SelectMany(x => x.Deliverable).ToList();

            Burned = new Stats(ReportingDataDate, BudgetedUnits, TotalUnits, BudgetedCosts, TotalCosts, FirstAlignedDataDate, ReportingInterval, VariationAdjustments);
            Burned.SetData(cleanSummaryStats.Where(x => x.Burned != null && x.Burned.DataPoints != null).SelectMany(x => x.Burned.DataPoints).ToList());

            Actual = new Stats(ReportingDataDate, BudgetedUnits, TotalUnits, BudgetedCosts, TotalCosts, FirstAlignedDataDate, ReportingInterval, VariationAdjustments);
            Actual.SetData(cleanSummaryStats.Where(x => x.Actual != null && x.Actual.DataPoints != null).SelectMany(x => x.Actual.DataPoints).ToList());
        }

        public void GenerateSummary()
        {
            this.Budgeted.SetData(Deliverable.SelectMany(x => x.Stats.Budgeted.GetData()));
            this.Current.SetData(Deliverable.SelectMany(x => x.Stats.Current.GetData()));
            this.Earned.SetData(Deliverable.SelectMany(x => x.Stats.Earned.GetData()));
            this.Remaining.SetData(Deliverable.SelectMany(x => x.Stats.Remaining.GetData()));
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

    public class ProgressStats
    {
        #region Compulsory Parameters
        public readonly DateTime ReportingDataDate;
        public readonly TimeSpan ReportingInterval;
        public readonly DateTime FirstAlignedDataDate;
        public Stats Budgeted { get; set; }
        public Stats Current { get; set; }
        public Stats Earned { get; set; }
        public Stats Remaining { get; set; }
        readonly decimal budgetedUnits;
        public readonly decimal TotalUnits;
        public readonly decimal budgetedCosts;
        public readonly decimal TotalCosts;
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
            TotalUnits = totalUnits;
            this.budgetedCosts = budgetedCosts;
            TotalCosts = totalCosts;
            this.VariationAdjustments = variationAdjustments.ToList();
            Budgeted = new Stats(ReportingDataDate, budgetedUnits, totalUnits, budgetedCosts, totalCosts, FirstAlignedDataDate, ReportingInterval);
            Current = new Stats(ReportingDataDate, budgetedUnits, totalUnits, budgetedCosts, totalCosts, FirstAlignedDataDate, ReportingInterval, variationAdjustments);
            Earned = new Stats(ReportingDataDate, budgetedUnits, totalUnits, budgetedCosts, totalCosts, FirstAlignedDataDate, ReportingInterval, variationAdjustments);
            Remaining = new Stats(ReportingDataDate, budgetedUnits, totalUnits, budgetedCosts, totalCosts, FirstAlignedDataDate, ReportingInterval, variationAdjustments, true);
        }

        public ProgressStats(IEnumerable<ProgressStats> progressStats)
        {
            IEnumerable<ProgressStats> cleanProgressStats = progressStats.Where(x => x != null);
            this.ReportingDataDate = cleanProgressStats.Where(x => x.ReportingDataDate != null).Min(x => x.ReportingDataDate);
            this.ReportingInterval = cleanProgressStats.First().ReportingInterval;
            this.FirstAlignedDataDate = cleanProgressStats.Min(x => x.FirstAlignedDataDate);

            budgetedUnits = cleanProgressStats.Sum(x => x.budgetedUnits);
            TotalUnits = cleanProgressStats.Sum(x => x.TotalUnits);
            this.budgetedCosts = cleanProgressStats.Sum(x => x.BudgetedCosts);
            TotalCosts = cleanProgressStats.Sum(x => x.TotalCosts);
            this.VariationAdjustments = cleanProgressStats.SelectMany(x => x.VariationAdjustments).ToList();

            Budgeted = new Stats(ReportingDataDate, budgetedUnits, TotalUnits, BudgetedCosts, TotalCosts, FirstAlignedDataDate, ReportingInterval);
            Budgeted.SetData(cleanProgressStats.Where(x => x.Budgeted != null && x.Budgeted.DataPoints != null).SelectMany(x => x.Budgeted.GetData()).ToList());

            Current = new Stats(ReportingDataDate, budgetedUnits, TotalUnits, BudgetedCosts, TotalCosts, FirstAlignedDataDate, ReportingInterval, VariationAdjustments);
            Current.SetData(cleanProgressStats.Where(x => x.Current != null && x.Current.DataPoints != null).SelectMany(x => x.Current.GetData()).ToList());

            Earned = new Stats(ReportingDataDate, budgetedUnits, TotalUnits, BudgetedCosts, TotalCosts, FirstAlignedDataDate, ReportingInterval, VariationAdjustments);
            Earned.SetData(cleanProgressStats.Where(x => x.Earned != null && x.Earned.DataPoints != null).SelectMany(x => x.Earned.GetData()).ToList());

            Remaining = new Stats(ReportingDataDate, budgetedUnits, TotalUnits, BudgetedCosts, TotalCosts, FirstAlignedDataDate, ReportingInterval, VariationAdjustments, true);
            Remaining.SetData(cleanProgressStats.Where(x => x.Remaining != null && x.Remaining.DataPoints != null).SelectMany(x => x.Remaining.GetData()).ToList());
        }
    }
}