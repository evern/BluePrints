using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Projections;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using BluePrints.View;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static BluePrints.Data.BluePrintsEntities;

namespace BluePrints.Common.ViewModel.Reporting
{
    public abstract class StatsSummarizer
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

            SetP6Parameters();
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
            maxProgress += SetP6ParametersProgress();
            maxProgress += SetBudgetDataPointsProgress();
            maxProgress += SetCurrentDataPointsProgress();
            maxProgress += SetEarnedDataPointsProgress();
            maxProgress += SetRemainingDataPointsProgress();

            return maxProgress;
        }

        public abstract int SetP6ParametersProgress();
        public abstract void SetP6Parameters();

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
        readonly PartialStatsBuilder PartialStatsBuilder;

        public PartialSummarizer(SummaryStats summarizableObject, PartialStatsBuilder partialStatsBuilder)
        {
            this.SummaryStats = summarizableObject;
            this.PartialStatsBuilder = partialStatsBuilder;
        }

        public void BuildBudgetedOnly()
        {
            SetP6Parameters();
            SetPlannedDataPoints(true);
        }

        public override int SetP6ParametersProgress()
        {
            return 1;
        }

        public override void SetP6Parameters()
        {
            ProjectionHelpers.SetWorkpackAssignmentStartUnit(((SummaryStats)this.SummaryStats).Deliverable);
            LoadingScreenManager.Progress();
        }

        public override int SetBudgetDataPointsProgress()
        {
            return ((SummaryStats)this.SummaryStats).Deliverable.Count();
        }

        public override void SetBudgetDataPoints()
        {
            //PlannedDataPointsBuilderFromDatabase(CURRENTPROJECT.NUMBER, false);
            SetPlannedDataPoints(true);
        }

        public override int SetCurrentDataPointsProgress()
        {
            return ((SummaryStats)this.SummaryStats).Deliverable.Count();
        }

        public override void SetCurrentDataPoints()
        {
            //PlannedDataPointsBuilderFromDatabase(CURRENTPROJECT.NUMBER, true);
            SetPlannedDataPoints(false);
        }

        private void SummarizePlannedDataPointsFromDatabase(string ProjectNumber, bool isOriginal)
        {
            BluePrintsEntities bluePrintDataContext = new BluePrintsEntities();
            ObjectResult<StoredProcedure_DeliverablesDataPoints> deliverablesDataPoints = bluePrintDataContext.GetDataPointsByProject(ProjectNumber, isOriginal);
            SetPlannedDataPoints(isOriginal, deliverablesDataPoints.ToList());
        }

        private void SetPlannedDataPoints(bool isOriginal, IEnumerable<StoredProcedure_DeliverablesDataPoints> DataPointsCollection = null)
        {
            ReportingEnum.AssignmentLoadType assignmentLoadType = isOriginal == true ? ReportingEnum.AssignmentLoadType.Original : assignmentLoadType = ReportingEnum.AssignmentLoadType.Modified;

            foreach (PROGRESS_ITEMProjection reportableObject in ((SummaryStats)this.SummaryStats).Deliverable)
            {
                PartialStatsBuilder.BuildPlannedDataPoints(reportableObject, assignmentLoadType);
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
                PartialStatsBuilder.BuildEarnedDataPoints(progressItemStat);
                LoadingScreenManager.Progress();
            }
        }

        public override int SetRemainingDataPointsProgress()
        {
            return ((SummaryStats)this.SummaryStats).Deliverable.Count();
        }

        public override void SetRemainingDataPoints()
        {
            //BuildProductivity();
            foreach (PROGRESS_ITEMProjection progressItemStat in ((SummaryStats)this.SummaryStats).Deliverable)
            {
                PartialStatsBuilder.BuildRemainingDataPoints(progressItemStat);
                LoadingScreenManager.Progress();
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

        public FullSummarizer(ProjectSummaryStats summaryStats, FullStatsBuilder fullStatsBuilder)
            : base(summaryStats, fullStatsBuilder)
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
            SetP6Parameters();
            SetBudgetDataPoints();
            SetCurrentDataPoints();
            SetEarnedDataPoints();
            SetRemainingDataPoints();
            Summarize();
        }

        public void BuildBudgetedOnly()
        {

            SetPlannedDataPoints(true);
        }

        public override int SetP6ParametersProgress()
        {
            return 0;
        }

        public override void SetP6Parameters()
        {
            //Needs to be populated before using summarizer
        }

        public override int SetBudgetDataPointsProgress()
        {
            return 1;
        }

        public override void SetBudgetDataPoints()
        {
            //PlannedDataPointsBuilderFromDatabase(CURRENTPROJECT.NUMBER, false);
            SetPlannedDataPoints(true);
        }

        public override int SetCurrentDataPointsProgress()
        {
            return 1;
        }

        public override void SetCurrentDataPoints()
        {
            //PlannedDataPointsBuilderFromDatabase(CURRENTPROJECT.NUMBER, true);
            SetPlannedDataPoints(false);
        }

        private void SummarizePlannedDataPointsFromDatabase(string ProjectNumber, bool isOriginal)
        {
            BluePrintsEntities bluePrintDataContext = new BluePrintsEntities();
            ObjectResult<StoredProcedure_DeliverablesDataPoints> deliverablesDataPoints = bluePrintDataContext.GetDataPointsByProject(ProjectNumber, isOriginal);
            SetPlannedDataPoints(isOriginal, deliverablesDataPoints.ToList());
        }

        private void SetPlannedDataPoints(bool isOriginal, IEnumerable<StoredProcedure_DeliverablesDataPoints> DataPointsCollection = null)
        {
            ReportingEnum.AssignmentLoadType assignmentLoadType = isOriginal == true ? ReportingEnum.AssignmentLoadType.Original : assignmentLoadType = ReportingEnum.AssignmentLoadType.Modified;

            PartialStatsBuilder.BuildPlannedDataPoints(progressItem, assignmentLoadType);
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
            PartialStatsBuilder.BuildRemainingDataPoints(progressItem);
        }
    }
}
