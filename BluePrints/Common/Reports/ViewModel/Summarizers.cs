using BluePrints.Common.Projections;
using BluePrints.Data;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using static BluePrints.Data.BluePrintsEntities;
using System;
using System.Diagnostics;
using BaseModel.Data.Helpers;

namespace BluePrints.Common.ViewModel.Reporting
{
    public interface IStatsSummarizer
    {
        void Build(bool showLoadingScreen = true, bool isCosts = false, decimal weightingPortion = 1);
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

        public virtual void Build(bool showLoadingScreen = true, bool isCosts = false, decimal weightingPortion = 1)
        {
            if(showLoadingScreen)
                LoadingScreenManager.ShowLoadingScreen(GetAllMaxProgress());

            SetBudgetDataPoints(weightingPortion);
            SetCurrentDataPoints(weightingPortion);
            SetEarnedDataPoints(weightingPortion);
            SetRemainingDataPoints(weightingPortion);
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
        public abstract void SetBudgetDataPoints(decimal weightingPortion = 1);

        public abstract int SetCurrentDataPointsProgress();
        public abstract void SetCurrentDataPoints(decimal weightingPortion = 1);

        public abstract int SetEarnedDataPointsProgress();
        public abstract void SetEarnedDataPoints(decimal weightingPortion = 1);

        public abstract int SetRemainingDataPointsProgress();
        public abstract void SetRemainingDataPoints(decimal weightingPortion = 1);

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

        public void BuildBudgetedOnly(decimal weightingPortion = 1)
        {
            SetBudgetDataPoints(weightingPortion);
        }

        public void BuildEarnedAndRemaining(decimal weightingPortion = 1)
        {
            SetEarnedDataPoints(weightingPortion);
            SetRemainingDataPoints(weightingPortion);
            Summarize();
        }

        public override int SetBudgetDataPointsProgress()
        {
            return ((SummaryStats)this.SummaryStats).Reportables.Count();
        }

        public override void SetBudgetDataPoints(decimal weightingPortion = 1)
        {
            using (BluePrintsEntities bluePrintDataContext = new BluePrintsEntities())
            {
                List<StoredProcedure_PlannedDataPoint> plannedDataPoints = bluePrintDataContext.QueryDeliverablePlannedDataPointsByProject(this.projectNumber);
                foreach (IReportable reportableObject in ((SummaryStats)this.SummaryStats).Reportables)
                {
                    ReportablesDisplay reportablesDisplay = reportableObject as ReportablesDisplay;
                    if(reportablesDisplay != null)
                    {
                        IReportable_Group reportable_Group = reportablesDisplay.ProgressItem as IReportable_Group;
                        if(reportable_Group != null)
                        {
                            List<StoredProcedure_PlannedDataPoint> currentGroupDeliverableDataPoints = new List<StoredProcedure_PlannedDataPoint>();
                            foreach (IReportable reportable in reportable_Group.Reportables)
                            {
                                reportable.Stats.Budgeted.SetPlannedData(plannedDataPoints.Where(x => x.Original_Guid == reportable.OriginalEntityKey));
                                reportable.Update();
                                currentGroupDeliverableDataPoints.AddRange(plannedDataPoints.Where(x => x.Original_Guid == reportable.OriginalEntityKey));
                            }

                            reportable_Group.Stats.Budgeted.SetPlannedData(currentGroupDeliverableDataPoints);
                            reportable_Group.Update();
                            continue;
                        }
                        else
                        {
                            reportablesDisplay.Stats.Budgeted.SetPlannedData(plannedDataPoints.Where(x => x.Original_Guid == reportableObject.OriginalEntityKey));
                            reportablesDisplay.Update();
                        }
                    }
                    else
                    {
                        List<StoredProcedure_PlannedDataPoint> weightedPlannedDataPoints = new List<StoredProcedure_PlannedDataPoint>();
                        foreach(StoredProcedure_PlannedDataPoint plannedDataPoint in plannedDataPoints.Where(x => x.Original_Guid == reportableObject.OriginalEntityKey))
                        {
                            if(reportableObject.AssignedUsers.Count() > 0)
                            {
                                foreach (User_Weight user in reportableObject.AssignedUsers)
                                {
                                    StoredProcedure_PlannedDataPoint weightedPlannedDataPoint = new StoredProcedure_PlannedDataPoint();
                                    DataUtils.ShallowCopy(weightedPlannedDataPoint, plannedDataPoint);
                                    weightedPlannedDataPoint.PeriodPlannedUnits *= user.AggregateWeightDbl;
                                    weightedPlannedDataPoint.PeriodPlannedPrice *= user.AggregateWeightDbl;
                                    weightedPlannedDataPoints.Add(weightedPlannedDataPoint);
                                }
                            }
                            else
                            {
                                StoredProcedure_PlannedDataPoint weightedPlannedDataPoint = new StoredProcedure_PlannedDataPoint();
                                DataUtils.ShallowCopy(weightedPlannedDataPoint, plannedDataPoint);
                                weightedPlannedDataPoints.Add(weightedPlannedDataPoint);
                            }
                        }

                        reportableObject.Stats.Budgeted.SetPlannedData(weightedPlannedDataPoints);
                        reportableObject.Update();
                    }

                    LoadingScreenManager.Progress();
                }
            }
        }

        public override int SetCurrentDataPointsProgress()
        {
            return 0;
        }

        public override void SetCurrentDataPoints(decimal weightingPortion = 1)
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
        public override void SetEarnedDataPoints(decimal weightingPortion = 1)
        {
            foreach (IReportable progressItemStat in ((SummaryStats)this.SummaryStats).Reportables)
            {
                partialStatsBuilder.BuildEarnedDataPoints(progressItemStat);
                LoadingScreenManager.Progress();
            }
        }

        public override int SetRemainingDataPointsProgress()
        {
            return ((SummaryStats)this.SummaryStats).Reportables.Count();
        }

        public override void SetRemainingDataPoints(decimal weightingPortion = 1)
        {
            using (BluePrintsEntities bluePrintDataContext = new BluePrintsEntities())
            {
                List<StoredProcedure_RemainingDataPoint> remainingDataPoints = bluePrintDataContext.QueryDeliverableRemainingDataPointsByProject(this.projectNumber);
                //double sumRemaining = remainingDataPoints.Sum(x => x.PeriodRemainingUnits);
                //string s = sumRemaining.ToString();

                //foreach (var remainingDataPoint in remainingDataPoints)
                //{
                //    IEnumerable<IReportable> reportableObjects = ((SummaryStats)this.SummaryStats).Reportables;
                //    if (reportableObjects.Any(x => x.OriginalEntityKey == remainingDataPoint.Original_Guid))
                //    {
                //        Debug.Print(remainingDataPoint.PeriodRemainingUnits.ToString());
                //    }
                //}

                foreach (IReportable reportableObject in ((SummaryStats)this.SummaryStats).Reportables)
                {
                    ReportablesDisplay reportablesDisplay = reportableObject as ReportablesDisplay;
                    if (reportablesDisplay != null)
                    {
                        IReportable_Group reportable_Group = reportablesDisplay.ProgressItem as IReportable_Group;
                        if (reportable_Group != null)
                        {
                            List<StoredProcedure_RemainingDataPoint> currentGroupDeliverableDataPoints = new List<StoredProcedure_RemainingDataPoint>();
                            foreach (IReportable reportable in reportable_Group.Reportables)
                            {
                                reportable.Stats.Remaining.SetRemainingData(remainingDataPoints.Where(x => x.Original_Guid == reportable.OriginalEntityKey), reportable.Stats.Earned.DataPoints);
                                reportable.Update();
                                currentGroupDeliverableDataPoints.AddRange(remainingDataPoints.Where(x => x.Original_Guid == reportable.OriginalEntityKey));
                            }

                            reportable_Group.Stats.Remaining.SetRemainingData(currentGroupDeliverableDataPoints, reportable_Group.Stats.Earned.DataPoints);
                            reportable_Group.Update();
                            continue;
                        }
                        else
                        {
                            reportablesDisplay.Stats.Remaining.SetRemainingData(remainingDataPoints.Where(x => x.Original_Guid == reportableObject.OriginalEntityKey), reportableObject.Stats.Earned.DataPoints);
                            reportablesDisplay.Update();
                        }
                    }
                    else
                    {
                        reportableObject.Stats.Remaining.SetRemainingData(remainingDataPoints.Where(x => x.Original_Guid == reportableObject.OriginalEntityKey), reportableObject.Stats.Earned.DataPoints);
                        //if (reportableObject.Stats.Remaining.DataPoints != null)
                        //    Debug.Print(reportableObject.Stats.Remaining.DataPoints.Sum(x => x.Units).ToString());
                        //else
                        //    Debug.Print(reportableObject.OriginalEntityKey.ToString());

                        reportableObject.Update();
                    }

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

        public FullSummarizer(ProjectSummaryStats summaryStats, FullStatsBuilder fullStatsBuilder, string projectNumber)
            : base(summaryStats, fullStatsBuilder, projectNumber)
        {
            FullStatsBuilder = fullStatsBuilder;
        }

        public void BuildBurnedDataPoints(ExoBurnedFilterType filterType)
        {
            ProjectSummaryStats projectSummaryStats = this.SummaryStats as ProjectSummaryStats;
            if (projectSummaryStats != null)
                FullStatsBuilder.BuildExoDataPoints(projectSummaryStats, filterType);
        }

        public void RecalculateStats(bool isCosts)
        {
            ((SummaryStats)this.SummaryStats).RecalculateStats(isCosts);
        }
    }

    public class SingleObjectSummarizer : StatsSummarizer
    {
        readonly IReportable progressItem;

        PartialStatsBuilder partialStatsBuilder;
        public PartialStatsBuilder PartialStatsBuilder
        {
            get { return partialStatsBuilder; }
            set { partialStatsBuilder = value; }
        }

        public SingleObjectSummarizer(IReportable progressItem, PartialStatsBuilder partialStatsBuilder)
        {
            SummaryStats = progressItem.Stats;
            this.progressItem = progressItem;
            PartialStatsBuilder = partialStatsBuilder;
        }

        public override int SetBudgetDataPointsProgress()
        {
            return 1;
        }

        public override void SetBudgetDataPoints(decimal weightingPortion = 1)
        {
            PartialStatsBuilder.BuildPlannedDataPointsFromQuery(this.progressItem, weightingPortion);
            LoadingScreenManager.Progress();
        }

        public override int SetCurrentDataPointsProgress()
        {
            return 0;
        }

        public override void SetCurrentDataPoints(decimal weightingPortion = 1)
        {

        }

        public override int SetEarnedDataPointsProgress()
        {
            return 1;
        }

        public override void SetEarnedDataPoints(decimal weightingPortion = 1)
        {
            PartialStatsBuilder.BuildEarnedDataPoints(progressItem);
            LoadingScreenManager.Progress();
        }

        public override int SetRemainingDataPointsProgress()
        {
            return 1;
        }

        public override void SetRemainingDataPoints(decimal weightingPortion = 1)
        {
            PartialStatsBuilder.BuildRemainingDataPointsFromQuery(progressItem, weightingPortion);
            LoadingScreenManager.Progress();
        }
    }
}
