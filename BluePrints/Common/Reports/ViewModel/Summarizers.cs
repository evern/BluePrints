using BluePrints.Common.Projections;
using BluePrints.Data;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using static BluePrints.Data.BluePrintsEntities;
using System;
using System.Diagnostics;
using BaseModel.Data.Helpers;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;

namespace BluePrints.Common.ViewModel.Reporting
{
    public interface IStatsSummarizer
    {
        void Build(bool showLoadingScreen = true, bool isCosts = false, decimal weightingPortion = 1, List<StatsCalculationType> calcTypes = null, bool useProductivity = false);
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

        public virtual void Build(bool showLoadingScreen = true, bool isCosts = false, decimal weightingPortion = 1, List<StatsCalculationType> calcTypes = null, bool useProductivity = false)
        {
            if(showLoadingScreen)
                LoadingScreenManager.ShowLoadingScreen(GetAllMaxProgress(calcTypes));

            if (calcTypes == null)
                calcTypes = BluePrintsDataUtils.AllCalcTypes;

            if(calcTypes.Contains(StatsCalculationType.Planned))
            {
                if (showLoadingScreen)
                    LoadingScreenManager.SetMessage("Retrieving Planned Data...");

                SetBudgetDataPoints(weightingPortion);
                SetCurrentDataPoints(weightingPortion);
            }

            if(calcTypes.Contains(StatsCalculationType.Earned))
            {
                if (showLoadingScreen)
                    LoadingScreenManager.SetMessage("Retrieving Earned Data...");

                SetEarnedDataPoints(weightingPortion);
            }

            if (calcTypes.Contains(StatsCalculationType.Remaining))
            {
                if (showLoadingScreen)
                    LoadingScreenManager.SetMessage("Retrieving Remaining Data...");

                SetRemainingDataPoints(weightingPortion, useProductivity);
            }

            if (calcTypes.Contains(StatsCalculationType.Forecast))
            {
                if (showLoadingScreen)
                    LoadingScreenManager.SetMessage("Retrieving Forecast Data...");

                SetBudgetDataPoints(weightingPortion, true);
                SetRemainingDataPoints(weightingPortion, useProductivity, true);
            }

            Summarize();

            if (showLoadingScreen)
                LoadingScreenManager.CloseLoadingScreen();
        }

        protected int GetAllMaxProgress(List<StatsCalculationType> calcTypes)
        {
            int maxProgress = 0;
            if (calcTypes == null)
            {
                maxProgress += SetBudgetDataPointsProgress();
                maxProgress += SetCurrentDataPointsProgress();
                maxProgress += SetEarnedDataPointsProgress();
                maxProgress += SetRemainingDataPointsProgress();
                return maxProgress;
            }

            if (calcTypes.Contains(StatsCalculationType.Planned))
            {
                maxProgress += SetBudgetDataPointsProgress();
                maxProgress += SetCurrentDataPointsProgress();
            }

            if (calcTypes.Contains(StatsCalculationType.Earned))
                maxProgress += SetEarnedDataPointsProgress();

            if (calcTypes.Contains(StatsCalculationType.Remaining) || calcTypes.Contains(StatsCalculationType.Forecast))
                maxProgress += SetRemainingDataPointsProgress();

            return maxProgress;
        }
        
        public abstract int SetBudgetDataPointsProgress();
        public abstract void SetBudgetDataPoints(decimal weightingPortion = 1, bool isForecast = false, bool buildLate = true);

        public abstract int SetCurrentDataPointsProgress();
        public abstract void SetCurrentDataPoints(decimal weightingPortion = 1);

        public abstract int SetEarnedDataPointsProgress();
        public abstract void SetEarnedDataPoints(decimal weightingPortion = 1);

        public abstract int SetRemainingDataPointsProgress();
        public abstract void SetRemainingDataPoints(decimal weightingPortion = 1, bool useProductivity = false, bool isForecast = false);

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

        public void BuildBudgeted(decimal weightingPortion = 1, decimal unitsPerQty = 1, bool buildCurrent = true, bool buildLate = true)
        {
            SetBudgetDataPoints(weightingPortion, false, buildLate);
            if(buildCurrent)
                SetCurrentDataPoints(weightingPortion);
        }

        public void BuildEarned(decimal weightingPortion = 1)
        {
            SetEarnedDataPoints(weightingPortion);
        }

        public void BuildRemaining(decimal weightingPortion = 1, bool isForecast = false)
        {
            SetRemainingDataPoints(weightingPortion, isForecast);
        }

        public override int SetBudgetDataPointsProgress()
        {
            return ((SummaryStats)this.SummaryStats).Reportables.Count();
        }

        public override void SetBudgetDataPoints(decimal weightingPortion = 1, bool isForecast = false, bool buildLate = true)
        {
            using (BluePrintsEntities bluePrintDataContext = new BluePrintsEntities())
            {
                List<StoredProcedure_PlannedDataPoint> plannedDataPoints = bluePrintDataContext.QueryDeliverablePlannedDataPointsByProject(this.projectNumber, isForecast);
                List<StoredProcedure_PlannedDataPoint> plannedLateDataPoints = null;
                if(buildLate)
                    plannedLateDataPoints = bluePrintDataContext.QueryDeliverablePlannedLateDataPointsByProject(this.projectNumber);

                foreach (IReportable reportableObject in ((SummaryStats)this.SummaryStats).Reportables)
                {
                    double qtyPerUnit = reportableObject.Total_Units == 0 ? 0 : Convert.ToDouble(reportableObject.Total_Quantity / reportableObject.Total_Units);
                    List<StoredProcedure_PlannedDataPoint> weightedPlannedDataPoints = new List<StoredProcedure_PlannedDataPoint>();
                    foreach (StoredProcedure_PlannedDataPoint plannedDataPoint in plannedDataPoints.Where(x => x.Original_Guid == reportableObject.OriginalEntityKey))
                    {
                        if(reportableObject.AssignedUsers.Count() > 0)
                        {
                            foreach (User_Weight user in reportableObject.AssignedUsers)
                            {
                                StoredProcedure_PlannedDataPoint weightedPlannedDataPoint = new StoredProcedure_PlannedDataPoint();
                                DataUtils.ShallowCopy(weightedPlannedDataPoint, plannedDataPoint);
                                weightedPlannedDataPoint.PeriodPlannedUnits *= user.AggregateWeightDbl;
                                weightedPlannedDataPoint.PeriodPlannedPrice *= user.AggregateWeightDbl;
                                weightedPlannedDataPoint.PeriodPlannedQuantity = weightedPlannedDataPoint.PeriodPlannedUnits * qtyPerUnit;
                                weightedPlannedDataPoints.Add(weightedPlannedDataPoint);
                            }
                        }
                        else
                        {
                            plannedDataPoint.PeriodPlannedQuantity = plannedDataPoint.PeriodPlannedUnits * qtyPerUnit;
                            weightedPlannedDataPoints.Add(plannedDataPoint);
                        }

                        reportableObject.Stats.Budgeted.SetPlannedData(weightedPlannedDataPoints);

                        if(buildLate)
                        {
                            List<StoredProcedure_PlannedDataPoint> weightedPlannedLateDataPoints = new List<StoredProcedure_PlannedDataPoint>();
                            foreach (StoredProcedure_PlannedDataPoint plannedLateDataPoint in plannedLateDataPoints.Where(x => x.Original_Guid == reportableObject.OriginalEntityKey))
                            {
                                if (reportableObject.AssignedUsers.Count() > 0)
                                {
                                    foreach (User_Weight user in reportableObject.AssignedUsers)
                                    {
                                        StoredProcedure_PlannedDataPoint weightedPlannedLateDataPoint = new StoredProcedure_PlannedDataPoint();
                                        DataUtils.ShallowCopy(weightedPlannedLateDataPoint, plannedLateDataPoint);
                                        weightedPlannedLateDataPoint.PeriodPlannedUnits *= user.AggregateWeightDbl;
                                        weightedPlannedLateDataPoint.PeriodPlannedPrice *= user.AggregateWeightDbl;
                                        weightedPlannedLateDataPoint.PeriodPlannedQuantity = weightedPlannedLateDataPoint.PeriodPlannedUnits * qtyPerUnit;
                                        weightedPlannedLateDataPoints.Add(weightedPlannedLateDataPoint);
                                    }
                                }
                                else
                                {
                                    plannedLateDataPoint.PeriodPlannedQuantity = plannedLateDataPoint.PeriodPlannedUnits * qtyPerUnit;
                                    weightedPlannedLateDataPoints.Add(plannedLateDataPoint);
                                }
                            }

                            reportableObject.Stats.BudgetedLate.SetPlannedData(weightedPlannedLateDataPoints);
                        }

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
            using (BluePrintsEntities bluePrintDataContext = new BluePrintsEntities())
            {
                List<StoredProcedure_PlannedDataPoint> currentDataPoints = bluePrintDataContext.QueryDeliverableCurrentDataPointsByProject(this.projectNumber);

                foreach (IReportable reportableObject in ((SummaryStats)this.SummaryStats).Reportables)
                {
                    List<StoredProcedure_PlannedDataPoint> weightedPlannedDataPoints = new List<StoredProcedure_PlannedDataPoint>();
                    foreach (StoredProcedure_PlannedDataPoint plannedDataPoint in currentDataPoints.Where(x => x.Original_Guid == reportableObject.OriginalEntityKey))
                    {
                        if (reportableObject.AssignedUsers.Count() > 0)
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

                    reportableObject.Stats.Current.SetPlannedData(weightedPlannedDataPoints);
                    reportableObject.Update();

                    LoadingScreenManager.Progress();
                }
            }
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
                decimal qtyPerUnit = progressItemStat.Total_Units == 0 ? 0 : progressItemStat.Total_Quantity / progressItemStat.Total_Units;
                partialStatsBuilder.BuildEarnedDataPoints(progressItemStat, qtyPerUnit);
                LoadingScreenManager.Progress();
            }
        }

        public override int SetRemainingDataPointsProgress()
        {
            return ((SummaryStats)this.SummaryStats).Reportables.Count();
        }

        public override void SetRemainingDataPoints(decimal weightingPortion = 1, bool useProductivity = false, bool isForecast = false)
        {
            using (BluePrintsEntities bluePrintDataContext = new BluePrintsEntities())
            {
                List<StoredProcedure_RemainingDataPoint> remainingDataPoints = bluePrintDataContext.QueryDeliverableRemainingDataPointsByProject(this.projectNumber, isForecast);
                List<PROGRESS_ETC> projectProgressETCs = bluePrintDataContext.QueryProjectProgressETC(this.projectNumber);

                foreach (IReportable reportableObject in ((SummaryStats)this.SummaryStats).Reportables)
                {
                    List<StoredProcedure_RemainingDataPoint> storedProcedure_RemainingDataPoints = remainingDataPoints.Where(x => x.Original_Guid == reportableObject.OriginalEntityKey).ToList();
                    if (useProductivity)
                    {
                        //not using this here but ref is required
                        bool isOverride = false;
                        decimal productivity = BluePrintsDataUtils.GetStockLevelProductivity(reportableObject, ref isOverride);
                        storedProcedure_RemainingDataPoints.ForEach(x => productivityInflation(x, productivity));
                    }

                    reportableObject.Stats.Remaining.SetRemainingData(storedProcedure_RemainingDataPoints, reportableObject.Stats.Earned.GetData());
                    if (isForecast)
                        reportableObject.SetProgressETCs(projectProgressETCs.Where(x => x.GUID_ORIBASEITEM == reportableObject.OriginalEntityKey).ToList());

                    reportableObject.Update();
                }

                LoadingScreenManager.Progress();
            }
        }

        /// <summary>
        /// Inflate datapoint's units and price by a factor
        /// </summary>
        private void productivityInflation(StoredProcedure_RemainingDataPoint dataPoint, decimal productivity)
        {
            double dblProductivity = Convert.ToDouble(productivity);
            dataPoint.PeriodRemainingUnits = dataPoint.PeriodRemainingUnits / dblProductivity;
            dataPoint.PeriodRemainingPrice = dataPoint.PeriodRemainingPrice / dblProductivity;
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

        public void BuildBurnedDataPoints(bool forceRetrieveAllJobs, bool forceRetrieveAllUnits, bool forceRetrieveAllPOs, bool showLoadingScreen = false, bool timeOnly = false)
        {
            ProjectSummaryStats projectSummaryStats = this.SummaryStats as ProjectSummaryStats;

            if (projectSummaryStats != null)
                FullStatsBuilder.BuildExoDataPoints(FullStatsBuilder.PrimeroUOW, projectSummaryStats, forceRetrieveAllJobs, forceRetrieveAllUnits, forceRetrieveAllPOs, showLoadingScreen, timeOnly);
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

        public override void SetBudgetDataPoints(decimal weightingPortion = 1, bool isForecast = false, bool buildLate = true)
        {
            PartialStatsBuilder.BuildPlannedDataPointsFromQuery(this.progressItem, weightingPortion, isForecast);
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
            decimal qtyPerUnit = progressItem.Total_Units == 0 ? 0 : progressItem.Total_Quantity / progressItem.Total_Units;
            PartialStatsBuilder.BuildEarnedDataPoints(progressItem, qtyPerUnit);
            LoadingScreenManager.Progress();
        }

        public override int SetRemainingDataPointsProgress()
        {
            return 1;
        }

        public override void SetRemainingDataPoints(decimal weightingPortion = 1, bool useProductivity = false, bool isForecast = false)
        {
            PartialStatsBuilder.BuildRemainingDataPointsFromQuery(progressItem, weightingPortion, isForecast);
            LoadingScreenManager.Progress();
        }
    }
}
