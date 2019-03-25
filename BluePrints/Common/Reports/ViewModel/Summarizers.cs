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

namespace BluePrints.Common.ViewModel.Reporting
{
    public interface IStatsSummarizer
    {
        void Build(bool showLoadingScreen = true, bool isCosts = false, decimal weightingPortion = 1, bool earnOnly = false, bool useProductivity = false);
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

        public virtual void Build(bool showLoadingScreen = true, bool isCosts = false, decimal weightingPortion = 1, bool earnOnly = false, bool useProductivity = false)
        {
            if(showLoadingScreen)
                LoadingScreenManager.ShowLoadingScreen(GetAllMaxProgress());

            if(!earnOnly)
            {
                SetBudgetDataPoints(weightingPortion);
                SetCurrentDataPoints(weightingPortion);
            }

            SetEarnedDataPoints(weightingPortion);

            if(!earnOnly)
                SetRemainingDataPoints(weightingPortion, useProductivity);

            Summarize();

            if (showLoadingScreen)
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
        public abstract void SetRemainingDataPoints(decimal weightingPortion = 1, bool useProductivity = false);

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

        public void BuildBudgetedOnly(decimal weightingPortion = 1, decimal unitsPerQty = 1)
        {
            SetBudgetDataPoints(weightingPortion);
            SetCurrentDataPoints(weightingPortion);
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
                List<StoredProcedure_PlannedDataPoint> plannedLateDataPoints = bluePrintDataContext.QueryDeliverablePlannedLateDataPointsByProject(this.projectNumber);

                foreach (IReportable reportableObject in ((SummaryStats)this.SummaryStats).Reportables)
                {
                    ReportablesDisplay reportablesDisplay = reportableObject as ReportablesDisplay;
                    if(reportablesDisplay != null)
                    {
                        IReportable_Group reportable_Group = reportablesDisplay.ProgressItem as IReportable_Group;
                        if(reportable_Group != null)
                        {
                            List<StoredProcedure_PlannedDataPoint> currentGroupDeliverableDataPoints = new List<StoredProcedure_PlannedDataPoint>();
                            List<StoredProcedure_PlannedDataPoint> currentGroupLateDeliverableDataPoints = new List<StoredProcedure_PlannedDataPoint>();
                            foreach (IReportable reportable in reportable_Group.Reportables)
                            {
                                IReportable_Quantity quantityReportable = reportable as IReportable_Quantity;
                                IEnumerable<StoredProcedure_PlannedDataPoint> currentReportablePlannedDataPoints = plannedDataPoints.Where(x => x.Original_Guid == reportable.OriginalEntityKey);

                                double qtyPerUnit = 1;
                                if (quantityReportable != null)
                                    qtyPerUnit = quantityReportable.Total_Units == 0 ? 0 : Convert.ToDouble(quantityReportable.Total_Quantity / quantityReportable.Total_Units);

                                foreach (StoredProcedure_PlannedDataPoint dataPoint in currentReportablePlannedDataPoints)
                                {
                                    dataPoint.PeriodPlannedQuantity = dataPoint.PeriodPlannedUnits * qtyPerUnit;
                                }

                                reportable.Stats.Budgeted.SetPlannedData(currentReportablePlannedDataPoints);
                                reportable.Stats.BudgetedLate.SetPlannedData(currentReportablePlannedDataPoints);
                                reportable.Update();
                                currentGroupDeliverableDataPoints.AddRange(currentReportablePlannedDataPoints);
                                currentGroupLateDeliverableDataPoints.AddRange(currentReportablePlannedDataPoints);
                            }

                            reportable_Group.Stats.Budgeted.SetPlannedData(currentGroupDeliverableDataPoints);
                            reportable_Group.Stats.BudgetedLate.SetPlannedData(currentGroupLateDeliverableDataPoints);
                            reportable_Group.Update();
                            continue;
                        }
                        else
                        {
                            IEnumerable<StoredProcedure_PlannedDataPoint> currentReportablePlannedDataPoints = plannedDataPoints.Where(x => x.Original_Guid == reportableObject.OriginalEntityKey);
                            double qtyPerUnit = reportableObject.Total_Units == 0 ? 0 :Convert.ToDouble(reportableObject.Total_Quantity / reportableObject.Total_Units);

                            foreach (StoredProcedure_PlannedDataPoint dataPoint in currentReportablePlannedDataPoints)
                            {
                                dataPoint.PeriodPlannedQuantity = dataPoint.PeriodPlannedUnits * qtyPerUnit;
                            }

                            reportablesDisplay.Stats.Budgeted.SetPlannedData(currentReportablePlannedDataPoints);
                            reportablesDisplay.Stats.BudgetedLate.SetPlannedData(currentReportablePlannedDataPoints);
                            reportablesDisplay.Update();
                        }
                    }
                    else
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
                                StoredProcedure_PlannedDataPoint weightedPlannedDataPoint = new StoredProcedure_PlannedDataPoint();
                                DataUtils.ShallowCopy(weightedPlannedDataPoint, plannedDataPoint);
                                weightedPlannedDataPoint.PeriodPlannedQuantity = weightedPlannedDataPoint.PeriodPlannedUnits * qtyPerUnit;
                                weightedPlannedDataPoints.Add(weightedPlannedDataPoint);
                            }
                        }

                        reportableObject.Stats.Budgeted.SetPlannedData(weightedPlannedDataPoints);

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
                                StoredProcedure_PlannedDataPoint weightedPlannedLateDataPoint = new StoredProcedure_PlannedDataPoint();
                                DataUtils.ShallowCopy(weightedPlannedLateDataPoint, plannedLateDataPoint);
                                weightedPlannedLateDataPoint.PeriodPlannedQuantity = weightedPlannedLateDataPoint.PeriodPlannedUnits * qtyPerUnit;
                                weightedPlannedLateDataPoints.Add(weightedPlannedLateDataPoint);
                            }
                        }

                        reportableObject.Stats.BudgetedLate.SetPlannedData(weightedPlannedLateDataPoints);
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
                    ReportablesDisplay reportablesDisplay = reportableObject as ReportablesDisplay;
                    if (reportablesDisplay != null)
                    {
                        IReportable_Group reportable_Group = reportablesDisplay.ProgressItem as IReportable_Group;
                        if (reportable_Group != null)
                        {
                            List<StoredProcedure_PlannedDataPoint> currentGroupDeliverableDataPoints = new List<StoredProcedure_PlannedDataPoint>();
                            foreach (IReportable reportable in reportable_Group.Reportables)
                            {
                                reportable.Stats.Current.SetPlannedData(currentDataPoints.Where(x => x.Original_Guid == reportable.OriginalEntityKey));
                                reportable.Update();
                                currentGroupDeliverableDataPoints.AddRange(currentDataPoints.Where(x => x.Original_Guid == reportable.OriginalEntityKey));
                            }

                            reportable_Group.Stats.Current.SetPlannedData(currentGroupDeliverableDataPoints);
                            reportable_Group.Update();
                            continue;
                        }
                        else
                        {
                            reportablesDisplay.Stats.Current.SetPlannedData(currentDataPoints.Where(x => x.Original_Guid == reportableObject.OriginalEntityKey));
                            reportablesDisplay.Update();
                        }
                    }
                    else
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
                    }

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

        public override void SetRemainingDataPoints(decimal weightingPortion = 1, bool useProductivity = false)
        {
            using (BluePrintsEntities bluePrintDataContext = new BluePrintsEntities())
            {
                List<StoredProcedure_RemainingDataPoint> remainingDataPoints = bluePrintDataContext.QueryDeliverableRemainingDataPointsByProject(this.projectNumber);
                //List<StoredProcedure_RemainingDataPoint> remainingLateDataPoints = bluePrintDataContext.QueryDeliverableRemainingLateDataPointsByProject(this.projectNumber);
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
                            List<StoredProcedure_RemainingDataPoint> currentGroupDeliverableLateDataPoints = new List<StoredProcedure_RemainingDataPoint>();
                            foreach (IReportable reportable in reportable_Group.Reportables)
                            {
                                reportable.Stats.Remaining.SetRemainingData(remainingDataPoints.Where(x => x.Original_Guid == reportable.OriginalEntityKey), reportable.Stats.Earned.DataPoints);
                                //SummaryStats summaryStats = reportable.Stats as SummaryStats;
                                //if(summaryStats != null)
                                //    reportable.Stats.RemainingActual.SetRemainingData(remainingDataPoints.Where(x => x.Original_Guid == reportable.OriginalEntityKey), summaryStats.Burned.DataPoints);

                                reportable.Update();
                                currentGroupDeliverableDataPoints.AddRange(remainingDataPoints.Where(x => x.Original_Guid == reportable.OriginalEntityKey));
                                currentGroupDeliverableLateDataPoints.AddRange(remainingDataPoints.Where(x => x.Original_Guid == reportable.OriginalEntityKey));
                            }

                            reportable_Group.Stats.Remaining.SetRemainingData(currentGroupDeliverableDataPoints, reportable_Group.Stats.Earned.DataPoints);
                            //SummaryStats groupSummaryStats = reportable_Group.Stats as SummaryStats;
                            //if (groupSummaryStats != null)
                            //    reportable_Group.Stats.RemainingActual.SetRemainingData(currentGroupDeliverableLateDataPoints, groupSummaryStats.Burned.DataPoints);

                            reportable_Group.Update();
                            continue;
                        }
                        else
                        {
                            reportablesDisplay.Stats.Remaining.SetRemainingData(remainingDataPoints.Where(x => x.Original_Guid == reportableObject.OriginalEntityKey), reportableObject.Stats.Earned.DataPoints);
                            //SummaryStats summaryStats = reportablesDisplay.Stats as SummaryStats;
                            //if (summaryStats != null)
                            //    reportablesDisplay.Stats.RemainingActual.SetRemainingData(remainingDataPoints.Where(x => x.Original_Guid == reportableObject.OriginalEntityKey), summaryStats.Burned.DataPoints);

                            reportablesDisplay.Update();
                        }
                    }
                    else
                    {
                        List<StoredProcedure_RemainingDataPoint> storedProcedure_RemainingDataPoints = remainingDataPoints.Where(x => x.Original_Guid == reportableObject.OriginalEntityKey).ToList();
                        if (useProductivity)
                        {
                            //not using this here but ref is required
                            bool isOverride = false;
                            decimal productivity = BluePrintsDataUtils.GetStockLevelProductivity(reportableObject, ref isOverride);
                            storedProcedure_RemainingDataPoints.ForEach(x => productivityInflation(x, productivity));
                        }

                        reportableObject.Stats.Remaining.SetRemainingData(storedProcedure_RemainingDataPoints, reportableObject.Stats.Earned.DataPoints);
                        reportableObject.Update();
                    }

                    LoadingScreenManager.Progress();
                }
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

        public void BuildBurnedDataPoints(bool forceRetrieveAllBurned)
        {
            ProjectSummaryStats projectSummaryStats = this.SummaryStats as ProjectSummaryStats;
            if (projectSummaryStats != null)
                FullStatsBuilder.BuildExoDataPoints(projectSummaryStats, forceRetrieveAllBurned);
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
            decimal qtyPerUnit = progressItem.Total_Units == 0 ? 0 : progressItem.Total_Quantity / progressItem.Total_Units;
            PartialStatsBuilder.BuildEarnedDataPoints(progressItem, qtyPerUnit);
            LoadingScreenManager.Progress();
        }

        public override int SetRemainingDataPointsProgress()
        {
            return 1;
        }

        public override void SetRemainingDataPoints(decimal weightingPortion = 1, bool useProductivity = false)
        {
            PartialStatsBuilder.BuildRemainingDataPointsFromQuery(progressItem, weightingPortion);
            LoadingScreenManager.Progress();
        }
    }
}
