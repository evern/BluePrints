using BluePrints.Common.Projections;
using BluePrints.Data;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using static BluePrints.Data.BluePrintsEntities;
using System;

namespace BluePrints.Common.ViewModel.Reporting
{
    public interface IStatsSummarizer
    {
        void Build(bool showLoadingScreen = true, bool isCosts = false);
        void Summarize();
    }

    public abstract class StatsSummarizer : IStatsSummarizer
    {
        ProgressStats summaryObject;
        public ProgressStats SummaryStats
        {
            get { return summaryObject; }
            set { summaryObject = value; }
        }

        public virtual void Build(bool showLoadingScreen = true, bool isCosts = false)
        {
            if(showLoadingScreen)
                LoadingScreenManager.ShowLoadingScreen(GetAllMaxProgress());
            
            SetBudgetDataPoints();
            SetCurrentDataPoints();
            SetEarnedDataPoints();
            SetRemainingDataPoints();
            Summarize();
            LoadingScreenManager.CloseLoadingScreen();
        }

        protected int GetAllMaxProgress()
        {
            int maxProgress = 0;
            maxProgress += SetBudgetDataPointsProgress();
            maxProgress += SetCurrentDataPointsProgress();
            maxProgress += SetEarnedDataPointsProgress();
            maxProgress += SetRemainingDataPointsProgress();

            return maxProgress;
        }
        
        public abstract int SetBudgetDataPointsProgress();
        public abstract void SetBudgetDataPoints();

        public abstract int SetCurrentDataPointsProgress();
        public abstract void SetCurrentDataPoints();

        public abstract int SetEarnedDataPointsProgress();
        public abstract void SetEarnedDataPoints();

        public abstract int SetRemainingDataPointsProgress();
        public abstract void SetRemainingDataPoints();

        public virtual void Summarize()
        {
            SummaryStats summaryStats = SummaryStats as SummaryStats;
            if(summaryStats != null)
                summaryStats.GenerateSummary();
        }
    }

    public class PartialSummarizer : StatsSummarizer
    {
        readonly PartialStatsBuilder partialStatsBuilder;
        readonly string projectNumber;

        public PartialSummarizer(SummaryStats summarizableObject, PartialStatsBuilder partialStatsBuilder, string projectNumber)
        {
            SummaryStats = summarizableObject;
            this.partialStatsBuilder = partialStatsBuilder;
            this.projectNumber = projectNumber;
        }

        public void BuildBudgetedOnly()
        {
            SetBudgetDataPoints();
        }

        public void BuildEarnedAndRemaining()
        {
            SetEarnedDataPoints();
            SetRemainingDataPoints();
            Summarize();
        }

        public override int SetBudgetDataPointsProgress()
        {
            return ((SummaryStats)this.SummaryStats).Reportables.Count();
        }

        public override void SetBudgetDataPoints()
        {
            using (BluePrintsEntities bluePrintDataContext = new BluePrintsEntities())
            {
                List<StoredProcedure_PlannedDataPoint> plannedDataPoints = bluePrintDataContext.QueryDeliverablePlannedDataPointsByProject(this.projectNumber);
                foreach (IReportableStats reportableObject in ((SummaryStats)this.SummaryStats).Reportables)
                {
                    List<StoredProcedure_PlannedDataPoint> currentDeliverableDataPoints = new List<StoredProcedure_PlannedDataPoint>();

                    currentDeliverableDataPoints.AddRange(plannedDataPoints.Where(x => x.Deliverable_Guid == reportableObject.EntityKey));

                    reportableObject.Stats.Budgeted.SetPlannedData(currentDeliverableDataPoints);
                    reportableObject.Stats.Current.SetPlannedData(currentDeliverableDataPoints);
                    reportableObject.Update();

                    LoadingScreenManager.Progress();
                }
            }
        }

        public override int SetCurrentDataPointsProgress()
        {
            return 0;
        }

        public override void SetCurrentDataPoints()
        {
        }

        private void SummarizeRemainingDataPointsFromQuery(string ProjectNumber)
        {

        }

        public override int SetEarnedDataPointsProgress()
        {
            return ((SummaryStats)this.SummaryStats).Reportables.Count();
        }

        /// <summary>
        /// Calculates each baselineItem earned data point while populating aggregate non cumulative earned data points
        /// </summary>
        /// <returns>Non cumulative earned progress data points</returns>
        public override void SetEarnedDataPoints()
        {
            foreach (IReportableStats progressItemStat in ((SummaryStats)this.SummaryStats).Reportables)
            {
                partialStatsBuilder.BuildEarnedDataPoints(progressItemStat);
                LoadingScreenManager.Progress();
            }
        }

        public override int SetRemainingDataPointsProgress()
        {
            return ((SummaryStats)this.SummaryStats).Reportables.Count();
        }

        public override void SetRemainingDataPoints()
        {
            using (BluePrintsEntities bluePrintDataContext = new BluePrintsEntities())
            {
                List<StoredProcedure_RemainingDataPoint> remainingDataPoints = bluePrintDataContext.QueryDeliverableRemainingDataPointsByProject(this.projectNumber);
                foreach (IReportableStats reportableObject in ((SummaryStats)this.SummaryStats).Reportables)
                {
                    List<StoredProcedure_RemainingDataPoint> currentDeliverableDataPoints = new List<StoredProcedure_RemainingDataPoint>();
                    currentDeliverableDataPoints.AddRange(remainingDataPoints.Where(x => x.Deliverable_Guid == reportableObject.EntityKey));

                    reportableObject.Stats.Remaining.SetRemainingData(currentDeliverableDataPoints, reportableObject.Stats.Earned.DataPoints);
                    reportableObject.Update();

                    LoadingScreenManager.Progress();
                }
            }
        }

        public override void Summarize()
        {
            ((SummaryStats)this.SummaryStats).GenerateSummary();
        }
    }

    public class FullSummarizer : PartialSummarizer
    {
        readonly FullStatsBuilder FullStatsBuilder;

        public FullSummarizer(ProjectSummaryStats summaryStats, FullStatsBuilder fullStatsBuilder, string projectNumber = "")
            : base(summaryStats, fullStatsBuilder, projectNumber)
        {
            FullStatsBuilder = fullStatsBuilder;
        }

        public void BuildBurnedDataPoints()
        {
            ProjectSummaryStats projectSummaryStats = this.SummaryStats as ProjectSummaryStats;
            if (projectSummaryStats != null)
                FullStatsBuilder.BuildExoDataPoints(projectSummaryStats);
        }

        public void RecalculateStats(bool isCosts)
        {
            ((SummaryStats)this.SummaryStats).RecalculateStats(isCosts);
        }
    }

    public class SingleObjectSummarizer : StatsSummarizer
    {
        readonly IReportableStats progressItem;

        PartialStatsBuilder partialStatsBuilder;
        public PartialStatsBuilder PartialStatsBuilder
        {
            get { return partialStatsBuilder; }
            set { partialStatsBuilder = value; }
        }

        public SingleObjectSummarizer(IReportableStats progressItem, PartialStatsBuilder partialStatsBuilder)
        {
            SummaryStats = progressItem.Stats;
            this.progressItem = progressItem;
            PartialStatsBuilder = partialStatsBuilder;
        }

        public override int SetBudgetDataPointsProgress()
        {
            return 1;
        }

        public override void SetBudgetDataPoints()
        {
            PartialStatsBuilder.BuildPlannedDataPointsFromQuery(this.progressItem);
            LoadingScreenManager.Progress();
        }

        public override int SetCurrentDataPointsProgress()
        {
            return 0;
        }

        public override void SetCurrentDataPoints()
        {

        }

        public override int SetEarnedDataPointsProgress()
        {
            return 1;
        }

        public override void SetEarnedDataPoints()
        {
            PartialStatsBuilder.BuildEarnedDataPoints(progressItem);
            LoadingScreenManager.Progress();
        }

        public override int SetRemainingDataPointsProgress()
        {
            return 1;
        }

        public override void SetRemainingDataPoints()
        {
            PartialStatsBuilder.BuildRemainingDataPointsFromQuery(progressItem);
            LoadingScreenManager.Progress();
        }
    }
}
