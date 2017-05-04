using BluePrints.Common.Projections;
using BluePrints.Data;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using static BluePrints.Data.BluePrintsEntities;

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

        public PartialSummarizer(SummaryStats summarizableObject, PartialStatsBuilder partialStatsBuilder, string projectNumber = "")
        {
            this.SummaryStats = summarizableObject;
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
            return ((SummaryStats)this.SummaryStats).Deliverable.Count();
        }

        public override void SetBudgetDataPoints()
        {
            if (projectNumber == string.Empty)
                SetPlannedDataPoints(true);
            else
                SummarizePlannedDataPointsFromStoredProcedure(this.projectNumber);
        }

        public override int SetCurrentDataPointsProgress()
        {
            if (projectNumber == string.Empty)
                return ((SummaryStats)this.SummaryStats).Deliverable.Count();
            else
                return 0;
        }

        public override void SetCurrentDataPoints()
        {
            if (projectNumber == string.Empty)
                SetPlannedDataPoints(false);
        }

        private void SummarizePlannedDataPointsFromStoredProcedure(string ProjectNumber)
        {
            BluePrintsEntities bluePrintDataContext = new BluePrintsEntities();
            ObjectResult<StoredProcedure_PlannedDataPoint> deliverablesDataPoints = bluePrintDataContext.GetDeliverablesPlannedDataPointsByProject(ProjectNumber);

            List<StoredProcedure_PlannedDataPoint> plannedDataPoints = new List<StoredProcedure_PlannedDataPoint>();

            //circumvent EF issue when ObjectResult is null
            try
            {
                plannedDataPoints.AddRange(deliverablesDataPoints);
            }
            catch
            {
                return;
            }

            foreach (PROGRESS_ITEMProjection reportableObject in ((SummaryStats)this.SummaryStats).Deliverable)
            {
                List<StoredProcedure_PlannedDataPoint> currentDeliverableDataPoints = new List<StoredProcedure_PlannedDataPoint>();

                currentDeliverableDataPoints.AddRange(plannedDataPoints.Where(x => x.Deliverable_Guid == reportableObject.Entity.EntityKey));

                reportableObject.Stats.Budgeted.SetPlannedData(currentDeliverableDataPoints);
                reportableObject.Stats.Current.SetPlannedData(currentDeliverableDataPoints);
                reportableObject.Update();
                //reportableObject.SetBudgeted(currentDeliverableDataPoints.ToList());
                //reportableObject.SetCurrent(currentDeliverableDataPoints.ToList());

                LoadingScreenManager.Progress();
            }
        }

        private void SummarizeRemainingDataPointsFromStoredProcedure(string ProjectNumber)
        {
            BluePrintsEntities bluePrintDataContext = new BluePrintsEntities();
            ObjectResult<StoredProcedure_RemainingDataPoint> deliverablesDataPoints = bluePrintDataContext.GetDeliverablesRemainingDataPointsByProject(ProjectNumber);

            List<StoredProcedure_RemainingDataPoint> remainingDataPoints = new List<StoredProcedure_RemainingDataPoint>();
            
            //circumvent EF issue when ObjectResult is null
            try
            {
                remainingDataPoints.AddRange(deliverablesDataPoints);
            }
            catch
            {
                return;
            }

            foreach (PROGRESS_ITEMProjection reportableObject in ((SummaryStats)this.SummaryStats).Deliverable)
            {
                List<StoredProcedure_RemainingDataPoint> currentDeliverableDataPoints = new List<StoredProcedure_RemainingDataPoint>();

                currentDeliverableDataPoints.AddRange(remainingDataPoints.Where(x => x.Deliverable_Guid == reportableObject.Entity.EntityKey));

                reportableObject.Stats.Remaining.SetRemainingData(currentDeliverableDataPoints, reportableObject.Stats.Earned.DataPoints);
                reportableObject.Update();

                //if (reportableObject.Stats.Remaining != null && reportableObject.Stats.Remaining.DataPoints != null)
                //    Debug.Print(reportableObject.Entity.Entity.INTERNAL_NUM + "|" + reportableObject.Stats.totalUnits + "|" + reportableObject.Stats.Remaining.DataPoints.Sum(x => x.Units));

                LoadingScreenManager.Progress();
            }
        }

        private void SetPlannedDataPoints(bool isOriginal, IEnumerable<StoredProcedure_DeliverablesDataPoints> DataPointsCollection = null)
        {
            ReportingEnum.AssignmentLoadType assignmentLoadType = isOriginal == true ? ReportingEnum.AssignmentLoadType.Original : assignmentLoadType = ReportingEnum.AssignmentLoadType.Modified;

            foreach (PROGRESS_ITEMProjection reportableObject in ((SummaryStats)this.SummaryStats).Deliverable)
            {
                partialStatsBuilder.BuildPlannedDataPoints(reportableObject, assignmentLoadType);
                LoadingScreenManager.Progress();
            }
        }

        public override int SetEarnedDataPointsProgress()
        {
            return ((SummaryStats)this.SummaryStats).Deliverable.Count();
        }

        /// <summary>
        /// Calculates each baselineItem earned data point while populating aggregate non cumulative earned data points
        /// </summary>
        /// <returns>Non cumulative earned progress data points</returns>
        public override void SetEarnedDataPoints()
        {
            foreach (PROGRESS_ITEMProjection progressItemStat in ((SummaryStats)this.SummaryStats).Deliverable)
            {
                partialStatsBuilder.BuildEarnedDataPoints(progressItemStat);
                LoadingScreenManager.Progress();
            }
        }

        public override int SetRemainingDataPointsProgress()
        {
            return ((SummaryStats)this.SummaryStats).Deliverable.Count();
        }

        public override void SetRemainingDataPoints()
        {
            if(this.projectNumber == "")
            {
                foreach (PROGRESS_ITEMProjection progressItemStat in ((SummaryStats)this.SummaryStats).Deliverable)
                {
                    partialStatsBuilder.BuildRemainingDataPoints(progressItemStat);
                    LoadingScreenManager.Progress();
                }
            }
            else
            {
                SummarizeRemainingDataPointsFromStoredProcedure(this.projectNumber);
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
            FullStatsBuilder.BuildExoDataPoints(summaryStats);
        }

        public void RecalculateStats(bool isCosts)
        {
            ((SummaryStats)this.SummaryStats).RecalculateStats(isCosts);
        }
    }

    public class SingleObjectSummarizer : StatsSummarizer
    {
        readonly PROGRESS_ITEMProjection progressItem;

        PartialStatsBuilder partialStatsBuilder;
        public PartialStatsBuilder PartialStatsBuilder
        {
            get { return partialStatsBuilder; }
            set { partialStatsBuilder = value; }
        }

        public SingleObjectSummarizer(PROGRESS_ITEMProjection progressItem, PartialStatsBuilder partialStatsBuilder)
        {
            this.SummaryStats = progressItem.Stats;
            this.progressItem = progressItem;
            PartialStatsBuilder = partialStatsBuilder;
        }

        public override void Build(bool showLoadingScreen = true, bool isCosts = false)
        {
            SetBudgetDataPoints();
            SetCurrentDataPoints();
            SetEarnedDataPoints();
            SetRemainingDataPoints();
            Summarize();
        }

        public void BuildBudgetedOnly()
        {
            SetPlannedDataPointsFromStoredProcedure();
            //SetPlannedDataPoints(true);
        }

        public override int SetBudgetDataPointsProgress()
        {
            return 1;
        }

        public override void SetBudgetDataPoints()
        {
            //PlannedDataPointsBuilderFromDatabase(CURRENTPROJECT.NUMBER, false);
            SetPlannedDataPointsFromStoredProcedure();
            //SetPlannedDataPoints(true);
        }

        public override int SetCurrentDataPointsProgress()
        {
            return 0;
        }

        public override void SetCurrentDataPoints()
        {
            //PlannedDataPointsBuilderFromDatabase(CURRENTPROJECT.NUMBER, true);
            //SetPlannedDataPoints(false);
        }

        private void SetPlannedDataPoints(bool isOriginal, IEnumerable<StoredProcedure_DeliverablesDataPoints> DataPointsCollection = null)
        {
            ReportingEnum.AssignmentLoadType assignmentLoadType = isOriginal == true ? ReportingEnum.AssignmentLoadType.Original : assignmentLoadType = ReportingEnum.AssignmentLoadType.Modified;

            PartialStatsBuilder.BuildPlannedDataPoints(progressItem, assignmentLoadType);
            LoadingScreenManager.Progress();
        }

        private void SetPlannedDataPointsFromStoredProcedure()
        {
            PartialStatsBuilder.BuildPlannedDataPointsFromStoredProcedure(this.progressItem);
            LoadingScreenManager.Progress();
        }

        public override int SetEarnedDataPointsProgress()
        {
            return 1;
        }

        /// <summary>
        /// Calculates each baselineItem earned data point while populating aggregate non cumulative earned data points
        /// </summary>
        /// <returns>Non cumulative earned progress data points</returns>
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
            PartialStatsBuilder.BuildRemainingDataPointsFromStoredProcedure(progressItem);
            LoadingScreenManager.Progress();
            //PartialStatsBuilder.BuildRemainingDataPoints(progressItem);
        }
    }
}
