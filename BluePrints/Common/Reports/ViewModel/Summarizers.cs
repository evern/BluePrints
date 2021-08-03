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
using BluePrints.Common.Misc;

namespace BluePrints.Common.ViewModel.Reporting
{
    public interface IStatsSummarizer
    {
        void Build(bool showLoadingScreen = true, decimal weightingPortion = 1, List<StatsCalculationType> calcTypes = null, bool useProductivity = false);
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

        public virtual void Build(bool showLoadingScreen = true, decimal weightingPortion = 1, List<StatsCalculationType> calcTypes = null, bool useProductivity = false)
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
        public abstract void SetBudgetDataPoints(decimal weightingPortion = 1, bool isForecast = false, bool buildLate = true, bool isVariationSeparated = false);

        public abstract int SetCurrentDataPointsProgress();
        public abstract void SetCurrentDataPoints(decimal weightingPortion = 1, bool isVariationSeparated = false);

        public abstract int SetEarnedDataPointsProgress();
        public abstract void SetEarnedDataPoints(decimal weightingPortion = 1, bool isVariationSeparated = false);

        public abstract int SetRemainingDataPointsProgress();
        public abstract void SetRemainingDataPoints(decimal weightingPortion = 1, bool useProductivity = false, bool isForecast = false, bool isVariationSeparated = false);

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
        readonly bool isSummariseByWBS;

        public PartialSummarizer(SummaryStats summarizableObject, PartialStatsBuilder partialStatsBuilder, string projectNumber)
        {
            if (summarizableObject is WBSSummary WBSSummarisable)
                isSummariseByWBS = true;

            SummaryStats = summarizableObject;
            this.partialStatsBuilder = partialStatsBuilder;
            this.projectNumber = projectNumber;
        }

        public void BuildBudgeted(decimal weightingPortion = 1, decimal unitsPerQty = 1, bool buildCurrent = true, bool buildLate = true, bool isVariationSeparated = false)
        {
            SetBudgetDataPoints(weightingPortion, false, buildLate, isVariationSeparated);
            if(buildCurrent)
                SetCurrentDataPoints(weightingPortion);
        }

        public void BuildEarned(decimal weightingPortion = 1, bool isVariationSeparated = false)
        {
            SetEarnedDataPoints(weightingPortion, isVariationSeparated);
        }

        public void BuildRemaining(decimal weightingPortion = 1, bool isForecast = false, bool isVariationSeparated = false)
        {
            SetRemainingDataPoints(weightingPortion, isForecast, isVariationSeparated);
        }

        public override int SetBudgetDataPointsProgress()
        {
            if (isSummariseByWBS)
                return ((WBSSummary)this.SummaryStats).WBSReportables.Count;
            else
                return ((SummaryStats)this.SummaryStats).Reportables.Count();
        }

        public override void SetBudgetDataPoints(decimal weightingPortion = 1, bool isForecast = false, bool buildLate = true, bool isVariationSeparated = false)
        {
            using (BluePrintsEntities bluePrintDataContext = new BluePrintsEntities())
            {
                if(isSummariseByWBS)
                {
                    List<DataPointsGroup> plannedDataPointsGroups = bluePrintDataContext.QueryDeliverablePlannedDataPointsGroupByProject(this.projectNumber, true, false, isForecast, false, isVariationSeparated);
                    foreach (WBSReportable reportableObject in ((WBSSummary)SummaryStats).WBSReportables)
                    {
                        reportableObject.AssignWBSReportableData(x => x.Budgeted.SetPlannedData, plannedDataPointsGroups, isVariationSeparated);
                        if(buildLate)
                        {
                            List<DataPointsGroup> plannedLateDataPointsGroups = bluePrintDataContext.QueryDeliverablePlannedDataPointsGroupByProject(this.projectNumber, true, true, isForecast, false, isVariationSeparated);
                            reportableObject.AssignWBSReportableData(x => x.BudgetedLate.SetPlannedData, plannedLateDataPointsGroups, isVariationSeparated);
                        }
                    }
                }
                else
                {
                    List<Data.DataPoint> plannedDataPoints = bluePrintDataContext.QueryDeliverablePlannedDataPointsByProject(this.projectNumber, isForecast);
                    List<Data.DataPoint> plannedLateDataPoints = null;
                    if (buildLate)
                        plannedLateDataPoints = bluePrintDataContext.QueryDeliverablePlannedLateDataPointsByProject(this.projectNumber);

                    foreach (IReportable reportableObject in ((SummaryStats)this.SummaryStats).Reportables)
                    {
                        assignDataPointsByGuid(reportableObject, x => x.Stats.Budgeted, plannedDataPoints, reportableObject.OriginalEntityKey);
                        if (buildLate)
                            assignDataPointsByGuid(reportableObject, x => x.Stats.BudgetedLate, plannedDataPoints, reportableObject.OriginalEntityKey);

                        reportableObject.Update();
                        LoadingScreenManager.Progress();
                    }
                }
            }
        }

        private void assignDataPointsByGuid(IReportable reportableObject, Func<IReportable, Stats> getProgressStatsFunc, IEnumerable<Data.DataPoint> dataPoints, Guid originalGuid)
        {
            double qtyPerUnit = reportableObject.Total_Units == 0 ? 0 : Convert.ToDouble(reportableObject.Total_Quantity / reportableObject.Total_Units);
            List<Data.DataPoint> weightedDataPoints = new List<Data.DataPoint>();
            foreach (Data.DataPoint dataPoint in dataPoints.Where(x => x.Original_Guid == originalGuid))
            {
                if (reportableObject.AssignedUsers.Count() > 0)
                {
                    foreach (User_Weight user in reportableObject.AssignedUsers)
                    {
                        Data.DataPoint weightedPlannedLateDataPoint = new Data.DataPoint();
                        DataUtils.ShallowCopy(weightedPlannedLateDataPoint, dataPoint);
                        weightedPlannedLateDataPoint.PeriodUnits *= user.AggregateWeightDbl;
                        weightedPlannedLateDataPoint.PeriodPrice *= user.AggregateWeightDbl;
                        weightedPlannedLateDataPoint.PeriodQuantity = weightedPlannedLateDataPoint.PeriodUnits * qtyPerUnit;
                        weightedDataPoints.Add(weightedPlannedLateDataPoint);
                    }
                }
                else
                {
                    dataPoint.PeriodQuantity = dataPoint.PeriodUnits * qtyPerUnit;
                    weightedDataPoints.Add(dataPoint);
                }
            }

            getProgressStatsFunc(reportableObject).SetPlannedData(weightedDataPoints);
        }

        public override int SetCurrentDataPointsProgress()
        {
            return 0;
        }

        public override void SetCurrentDataPoints(decimal weightingPortion = 1, bool isVariationSeparated = false)
        {
            using (BluePrintsEntities bluePrintDataContext = new BluePrintsEntities())
            {
                if(isSummariseByWBS)
                {
                    List<DataPointsGroup> currentDataPointsGroups = bluePrintDataContext.QueryDeliverablePlannedDataPointsGroupByProject(this.projectNumber, true, false, true, false, isVariationSeparated);
                    foreach (WBSReportable reportableObject in ((WBSSummary)SummaryStats).WBSReportables)
                    {
                        reportableObject.AssignWBSReportableData(x => x.Current.SetPlannedData, currentDataPointsGroups, isVariationSeparated);
                        LoadingScreenManager.Progress();
                    }
                }
                else
                {
                    List<Data.DataPoint> currentDataPoints = bluePrintDataContext.QueryDeliverableCurrentDataPointsByProject(this.projectNumber);
                    foreach (IReportable reportableObject in ((SummaryStats)this.SummaryStats).Reportables)
                    {
                        assignDataPointsByGuid(reportableObject, x => x.Stats.Current, currentDataPoints, reportableObject.OriginalEntityKey);
                        reportableObject.Update();
                        LoadingScreenManager.Progress();
                    }
                }
            }
        }

        public override int SetEarnedDataPointsProgress()
        {
            if (isSummariseByWBS)
                return ((WBSSummary)this.SummaryStats).WBSReportables.Count;
            else
                return ((SummaryStats)this.SummaryStats).Reportables.Count();
        }

        /// <summary>
        /// Calculates each baselineItem earned data point while populating aggregate non cumulative earned data points
        /// </summary>
        /// <returns>Non cumulative earned progress data points</returns>
        public override void SetEarnedDataPoints(decimal weightingPortion = 1, bool isVariationSeparated = false)
        {
            if(isSummariseByWBS)
            {
                using (BluePrintsEntities bluePrintDataContext = new BluePrintsEntities())
                {
                    List<X_EARNED_QUERY> earnedQueries = bluePrintDataContext.X_EARNED_QUERY.Where(x => x.ProjectNumber == this.projectNumber).ToList();
                    List<EarnedQueriesGroup> earnedQueriesGroups;
                    if(isVariationSeparated)
                        earnedQueriesGroups = earnedQueries.GroupBy(x => new { x.SubJobCode, x.DisciplineCode, x.CommodityCode, x.VariationCode }).Select(g => new EarnedQueriesGroup(g.Key.SubJobCode, g.Key.DisciplineCode, g.Key.CommodityCode, g.Key.VariationCode, g)).ToList();
                    else
                        earnedQueriesGroups = earnedQueries.GroupBy(x => new { x.SubJobCode, x.DisciplineCode, x.CommodityCode, x.VariationCode }).Select(g => new EarnedQueriesGroup(g.Key.SubJobCode, g.Key.DisciplineCode, g.Key.CommodityCode, "", g)).ToList();

                    foreach (WBSReportable reportableObject in ((WBSSummary)SummaryStats).WBSReportables)
                    {
                        decimal qtyPerUnit = reportableObject.TotalUnits == 0 ? 0 : reportableObject.TotalQty / reportableObject.TotalUnits;
                        EarnedQueriesGroup earnedQueriesGroup;
                        if(isVariationSeparated)
                            earnedQueriesGroup = earnedQueriesGroups.FirstOrDefault(x => x.SubJobCode == reportableObject.SUBJOB_CODE && x.DisciplineCode == reportableObject.DISCIPLINE_CODE && x.CommodityCode == reportableObject.COMMODITY_CODE && x.VariationCode == reportableObject.VARIATION_CODE);
                        else
                            earnedQueriesGroup = earnedQueriesGroups.FirstOrDefault(x => x.SubJobCode == reportableObject.SUBJOB_CODE && x.DisciplineCode == reportableObject.DISCIPLINE_CODE && x.CommodityCode == reportableObject.COMMODITY_CODE);

                        if (earnedQueriesGroup != null)
                        {
                            List<DataPoint> progressItemEarnedDataPoints;
                            if (reportableObject.AllowPercentageOnZeroTotalUnits)
                            {
                                progressItemEarnedDataPoints = earnedQueriesGroup.EarnedQueries.Select(x => new DataPoint()
                                {
                                    TotalUnits = reportableObject.TotalUnits == 0 ? BluePrintsConstants.DurationBasedTotalUnits : reportableObject.TotalUnits,
                                    TotalCosts = reportableObject.TotalCosts,
                                    BudgetedUnits = reportableObject.BudgetedUnits,
                                    BudgetedCosts = reportableObject.BudgetedCosts,
                                    Units = x.EARNED_UNITS,
                                    Quantity = x.EARNED_UNITS * qtyPerUnit,
                                    Costs = x.EarnedPrice,
                                    ProgressDate = x.EARNED_DATE,
                                }).ToList();
                            }
                            else
                            {
                                progressItemEarnedDataPoints = earnedQueriesGroup.EarnedQueries.Select(x => new DataPoint()
                                {
                                    TotalUnits = reportableObject.TotalUnits,
                                    TotalCosts = reportableObject.TotalCosts,
                                    BudgetedUnits = reportableObject.BudgetedUnits,
                                    BudgetedCosts = reportableObject.BudgetedCosts,
                                    Units = x.ReportingEarnedUnits,
                                    Quantity = x.ReportingEarnedUnits * qtyPerUnit,
                                    Costs = x.EarnedPrice,
                                    ProgressDate = x.EARNED_DATE,
                                }).ToList();
                            }

                            //adjust set earned data should only be performed at this level (lowest level), summary dashboard entity will just use set data
                            reportableObject.Earned.SetData(progressItemEarnedDataPoints);
                            reportableObject.TenderEarned.SetData(progressItemEarnedDataPoints);
                        }

                        LoadingScreenManager.Progress();
                    }
                }
            }
            else
            {
                foreach (IReportable progressItemStat in ((SummaryStats)this.SummaryStats).Reportables)
                {
                    decimal qtyPerUnit = progressItemStat.Total_Units == 0 ? 0 : progressItemStat.Total_Quantity / progressItemStat.Total_Units;
                    partialStatsBuilder.BuildEarnedDataPoints(progressItemStat, qtyPerUnit);
                    LoadingScreenManager.Progress();
                }
            }
        }

        public override int SetRemainingDataPointsProgress()
        {
            return ((SummaryStats)this.SummaryStats).Reportables.Count();
        }

        public override void SetRemainingDataPoints(decimal weightingPortion = 1, bool useProductivity = false, bool isForecast = false, bool isVariationSeparated = false)
        {
            using (BluePrintsEntities bluePrintDataContext = new BluePrintsEntities())
            {
                if(isSummariseByWBS)
                {
                    List<DataPointsGroup> remainingDataPointsGroup = bluePrintDataContext.QueryDeliverablePlannedDataPointsGroupByProject(this.projectNumber, false, false, false, isForecast, isVariationSeparated);
                    foreach (WBSReportable reportableObject in ((WBSSummary)SummaryStats).WBSReportables)
                    {
                        DataPointsGroup dataPointsGroup;
                        if(isVariationSeparated)
                            dataPointsGroup = remainingDataPointsGroup.FirstOrDefault(x => x.SubJobCode == reportableObject.SUBJOB_CODE && x.DisciplineCode == reportableObject.DISCIPLINE_CODE && x.CommodityCode == reportableObject.COMMODITY_CODE && x.VariationCode == reportableObject.VARIATION_CODE);
                        else
                            dataPointsGroup = remainingDataPointsGroup.FirstOrDefault(x => x.SubJobCode == reportableObject.SUBJOB_CODE && x.DisciplineCode == reportableObject.DISCIPLINE_CODE && x.CommodityCode == reportableObject.COMMODITY_CODE);

                        if (dataPointsGroup != null)
                            reportableObject.Remaining.SetRemainingData(dataPointsGroup.DataPoints, reportableObject.Earned.GetData());

                        LoadingScreenManager.Progress();
                    }
                }
                else
                {
                    List<Data.DataPoint> remainingDataPoints = bluePrintDataContext.QueryDeliverableRemainingDataPointsByProject(this.projectNumber, isForecast);
                    List<PROGRESS_ETC> projectProgressETCs = bluePrintDataContext.QueryProjectProgressETC(this.projectNumber);

                    foreach (IReportable reportableObject in ((SummaryStats)this.SummaryStats).Reportables)
                    {
                        List<Data.DataPoint> dataPoints = remainingDataPoints.Where(x => x.Original_Guid == reportableObject.OriginalEntityKey).ToList();
                        if (useProductivity)
                        {
                            //not using this here but ref is required
                            bool isOverride = false;
                            decimal productivity = BluePrintsDataUtils.GetStockLevelProductivity(reportableObject, ref isOverride);
                            dataPoints.ForEach(x => productivityInflation(x, productivity));
                        }

                        reportableObject.Stats.Remaining.SetRemainingData(dataPoints, reportableObject.Stats.Earned.GetData());
                        if (isForecast)
                            reportableObject.SetProgressETCs(projectProgressETCs.Where(x => x.GUID_ORIBASEITEM == reportableObject.OriginalEntityKey).ToList());

                        reportableObject.Update();
                        LoadingScreenManager.Progress();
                    }
                }
            }
        }

        /// <summary>
        /// Inflate datapoint's units and price by a factor
        /// </summary>
        private void productivityInflation(Data.DataPoint dataPoint, decimal productivity)
        {
            double dblProductivity = Convert.ToDouble(productivity);
            dataPoint.PeriodUnits = dataPoint.PeriodUnits / dblProductivity;
            dataPoint.PeriodPrice = dataPoint.PeriodPrice / dblProductivity;
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

        public void BuildBurnedDataPoints(DashboardEXOQueryType dashboardEXOQueryType = DashboardEXOQueryType.TimeAndMaterial, bool isGroupByWBS = false, bool showLoadingScreen = false, bool forceRetrieveAllJobs = false, bool forceRetrieveAllUnits = false, bool forceRetrieveAllPOs = false)
        {
            ProjectSummaryStats projectSummaryStats = this.SummaryStats as ProjectSummaryStats;

            if (projectSummaryStats != null)
                FullStatsBuilder.BuildExoDataPoints(FullStatsBuilder.PrimeroUOW, projectSummaryStats, dashboardEXOQueryType, isGroupByWBS, showLoadingScreen, forceRetrieveAllJobs, forceRetrieveAllUnits, forceRetrieveAllPOs);
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

        public override void SetBudgetDataPoints(decimal weightingPortion = 1, bool isForecast = false, bool buildLate = true, bool isVariationSeparated = false)
        {
            PartialStatsBuilder.BuildPlannedDataPointsFromQuery(this.progressItem, weightingPortion, isForecast);
            LoadingScreenManager.Progress();
        }

        public override int SetCurrentDataPointsProgress()
        {
            return 0;
        }

        public override void SetCurrentDataPoints(decimal weightingPortion = 1, bool isVariationSeparated = false)
        {

        }

        public override int SetEarnedDataPointsProgress()
        {
            return 1;
        }

        public override void SetEarnedDataPoints(decimal weightingPortion = 1, bool isVariationSeparated = false)
        {
            decimal qtyPerUnit = progressItem.Total_Units == 0 ? 0 : progressItem.Total_Quantity / progressItem.Total_Units;
            PartialStatsBuilder.BuildEarnedDataPoints(progressItem, qtyPerUnit);
            LoadingScreenManager.Progress();
        }

        public override int SetRemainingDataPointsProgress()
        {
            return 1;
        }

        public override void SetRemainingDataPoints(decimal weightingPortion = 1, bool useProductivity = false, bool isForecast = false, bool isVariationSeparated = false)
        {
            PartialStatsBuilder.BuildRemainingDataPointsFromQuery(progressItem, weightingPortion, isForecast);
            LoadingScreenManager.Progress();
        }
    }
}
