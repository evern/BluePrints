using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Projections;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BluePrints.Common.ViewModel.Reporting
{
    public abstract class SummaryBuilder
    {
        private SummarizableObject summaryObject;

        public SummarizableObject SummaryObject
        {
            get { return summaryObject; }
            set { summaryObject = value; }
        }

        public abstract void BuildVariationDataPoints();
        public abstract void BuildOriginalPlannedDataPoints();
        public abstract void BuildModifiedPlannedDataPoints();
        public abstract void BuildEarnedDataPoints();
        public abstract void BuildBurnedDataPoints();
        public abstract void BuildRemainingDataPoints();
        public abstract void BuildActualDataPoints();
        public abstract void SummarizeDataPoints();
        public abstract void SummarizeNestedSummaryObjectDataPoints();

        public void RecalculateStats(bool isCosts = false)
        {
            SummaryObject.RecalculateStats(isCosts);
        }
    }

    public class UnpackPROJECTSummary : SummaryBuilder
    {
        public UnpackPROJECTSummary(WORKPACK_Dashboard WORKPACKDashboard, PROJECT_Dashboard PROJECTDashboard)
        {
            WORKPACKDashboard.ReportableObjects =
                PROJECTDashboard.ReportableObjects.Where(
                    x => x.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_WORKPACK == WORKPACKDashboard.GUID);
            string activeWORKPACKName;
            if (PROJECTDashboard.PROJECT.USELEGACYWORKPACK)
                activeWORKPACKName = WORKPACKDashboard.WORKPACK.INTERNAL_NAME1;
            else
                activeWORKPACKName = WORKPACKDashboard.WORKPACK.INTERNAL_NAME2;

            IEnumerable<ProgressInfo> workpackBurnedDataPoints =
                PROJECTDashboard.NonCumulative_BurnedDataPoints.Where(x => x.WorkpackName == activeWORKPACKName)
                    .OrderByDescending(x => x.ProgressDate);
            IEnumerable<ProgressInfo> workpackActualDataPoints =
                PROJECTDashboard.NonCumulative_ActualDataPoints.Where(x => x.WorkpackName == activeWORKPACKName)
                    .OrderByDescending(x => x.ProgressDate);
            WORKPACKDashboard.NonCumulative_BurnedDataPoints =
                new ObservableCollection<ProgressInfo>(workpackBurnedDataPoints);
            WORKPACKDashboard.NonCumulative_ActualDataPoints =
                new ObservableCollection<ProgressInfo>(workpackActualDataPoints);
            WORKPACKDashboard.FirstAlignedDataDate = PROJECTDashboard.FirstAlignedDataDate;
            WORKPACKDashboard.LiveBASELINE = PROJECTDashboard.LiveBASELINE;
            WORKPACKDashboard.LivePROGRESS = PROJECTDashboard.LivePROGRESS;
            WORKPACKDashboard.ReportingDataDate = PROJECTDashboard.ReportingDataDate;
            WORKPACKDashboard.IntervalPeriod = PROJECTDashboard.IntervalPeriod;
            SummaryObject = WORKPACKDashboard;
        }

        public override void BuildVariationDataPoints()
        {
            SummaryObject.NonCumulative_VariationAdjustments =
                new ObservableCollection<VariationAdjustment>(
                    SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_VariationAdjustments));
        }

        public override void BuildOriginalPlannedDataPoints()
        {
            SummaryObject.NonCumulative_OriginalDataPoints =
                new ObservableCollection<ProgressInfo>(
                    SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_OriginalDataPoints));
        }

        public override void BuildModifiedPlannedDataPoints()
        {
            SummaryObject.NonCumulative_PlannedDataPoints =
                new ObservableCollection<ProgressInfo>(
                    SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_PlannedDataPoints));
        }

        public override void BuildEarnedDataPoints()
        {
            SummaryObject.NonCumulative_EarnedDataPoints =
                new ObservableCollection<ProgressInfo>(
                    SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_EarnedDataPoints));
        }

        public override void BuildActualDataPoints()
        {
            throw new InvalidOperationException(
                "there is no need to roll up non cumulative actual data points from ReportableObjects.");
        }

        public override void BuildBurnedDataPoints()
        {
            throw new InvalidOperationException(
                "there is no need to roll up non cumulative burned data points from ReportableObjects.");
        }

        public override void BuildRemainingDataPoints()
        {
            SummaryObject.NonCumulative_RemainingCurrentDataPoints =
                new ObservableCollection<ProgressInfo>(
                    SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_RemainingCurrentDataPoints));
            SummaryObject.NonCumulative_RemainingPlannedDataPoints =
                new ObservableCollection<ProgressInfo>(
                    SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_RemainingPlannedDataPoints));
        }

        public override void SummarizeDataPoints()
        {
            ISupportProgressReportingExtensions.GenerateCumulativeSummaryDataPoints(SummaryObject);
        }

        public override void SummarizeNestedSummaryObjectDataPoints()
        {
            throw new InvalidOperationException("there is no need to summarize progress data points.");
        }
    }

    public class ReportableObjectRollUp : SummaryBuilder
    {
        public ReportableObjectRollUp(SummarizableObject summaryObject, WORKPACK workpack,
            SummarizableObject projectSummary)
        {
            summaryObject.ReportableObjects =
                projectSummary.ReportableObjects.Where(
                    x => x.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_WORKPACK == workpack.GUID).ToArray().ToList();
            var nonCumulativeBurnedList =
                projectSummary.NonCumulative_BurnedDataPoints.Where(x => x.WorkpackName == workpack.INTERNAL_NAME1)
                    .OrderByDescending(x => x.ProgressDate)
                    .ToArray()
                    .ToList();
            var nonCumulativeActualList =
                projectSummary.NonCumulative_ActualDataPoints.Where(x => x.WorkpackName == workpack.INTERNAL_NAME1)
                    .OrderByDescending(x => x.ProgressDate)
                    .ToArray()
                    .ToList();
            summaryObject.NonCumulative_BurnedDataPoints =
                new ObservableCollection<ProgressInfo>(nonCumulativeBurnedList);
            summaryObject.NonCumulative_ActualDataPoints =
                new ObservableCollection<ProgressInfo>(nonCumulativeActualList);
            summaryObject.FirstAlignedDataDate = projectSummary.FirstAlignedDataDate;
            summaryObject.LivePROGRESS = projectSummary.LivePROGRESS;
            summaryObject.LiveBASELINE = projectSummary.LiveBASELINE;
            summaryObject.ReportingDataDate = projectSummary.LivePROGRESS.DATA_DATE;
            summaryObject.IntervalPeriod = projectSummary.IntervalPeriod;
            SummaryObject = summaryObject;
        }

        public override void BuildVariationDataPoints()
        {
            SummaryObject.NonCumulative_VariationAdjustments =
                new ObservableCollection<VariationAdjustment>(
                    SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_VariationAdjustments));
        }

        public override void BuildOriginalPlannedDataPoints()
        {
            SummaryObject.NonCumulative_OriginalDataPoints =
                new ObservableCollection<ProgressInfo>(
                    SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_OriginalDataPoints));
        }

        public override void BuildModifiedPlannedDataPoints()
        {
            SummaryObject.NonCumulative_PlannedDataPoints =
                new ObservableCollection<ProgressInfo>(
                    SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_PlannedDataPoints));
        }

        public override void BuildEarnedDataPoints()
        {
            SummaryObject.NonCumulative_EarnedDataPoints =
                new ObservableCollection<ProgressInfo>(
                    SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_EarnedDataPoints));
        }

        public override void BuildActualDataPoints()
        {
            throw new InvalidOperationException(
                "there is no need to roll up non cumulative actual data points from ReportableObjects.");
        }

        public override void BuildBurnedDataPoints()
        {
            throw new InvalidOperationException(
                "there is no need to roll up non cumulative burned data points from ReportableObjects.");
        }

        public override void BuildRemainingDataPoints()
        {
            SummaryObject.NonCumulative_RemainingCurrentDataPoints =
                new ObservableCollection<ProgressInfo>(
                    SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_RemainingCurrentDataPoints));
            SummaryObject.NonCumulative_RemainingPlannedDataPoints =
                new ObservableCollection<ProgressInfo>(
                    SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_RemainingPlannedDataPoints));
        }

        public override void SummarizeDataPoints()
        {
            ISupportProgressReportingExtensions.GenerateCumulativeSummaryDataPoints(SummaryObject);
        }

        public override void SummarizeNestedSummaryObjectDataPoints()
        {
            throw new NotImplementedException();
        }
    }

    public class PROJECTSummaryBuilder : SummaryBuilder
    {
        private IBluePrintsEntitiesUnitOfWork BluePrintsUnitOfWork { get; set; }
        private IP6EntitiesUnitOfWork P6UnitOfWork { get; set; }
        private decimal CurrencyConversion { get; set; }
        private IEnumerable<TASK> PROGRESS_TASKS { get; set; }
        private P6Data.PROJECT PROGRESS_PROJECT = null;

        public PROJECTSummaryBuilder(SummarizableObject summaryObject,
            IBluePrintsEntitiesUnitOfWork BluePrintsUOW = null, IP6EntitiesUnitOfWork P6UOW = null)
        {
            if (summaryObject.LivePROGRESS == null || summaryObject.LiveBASELINE == null)
                return;

            CurrencyConversion = summaryObject.LiveBASELINE.PROJECT.CURRENCYCONVERSION;
            SummaryObject = summaryObject;
            SummaryObject.ReportingDataDate = SummaryObject.LivePROGRESS.DATA_DATE;

            if (BluePrintsUOW == null)
                BluePrintsUOW = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            else
                BluePrintsUnitOfWork = BluePrintsUOW;

            if (P6UOW == null)
                P6UnitOfWork = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            else
                P6UnitOfWork = P6UOW;

            SummaryObject.IntervalPeriod =
                ISupportProgressReportingExtensions.ConvertProgressIntervalToPeriod(SummaryObject.LivePROGRESS);
            SummaryObject.FirstAlignedDataDate =
                ISupportProgressReportingExtensions.GenerateFirstAlignedDataDate(SummaryObject.LivePROGRESS);
        }

        public override void SummarizeDataPoints()
        {
            ISupportProgressReportingExtensions.GenerateCumulativeSummaryDataPoints(SummaryObject);
        }

        public override void SummarizeNestedSummaryObjectDataPoints()
        {
            foreach (var reportableObject in SummaryObject.ReportableObjects)
                ISupportProgressReportingExtensions.GenerateCumulativeSummaryDataPoints(reportableObject,
                    SummaryObject.FirstAlignedDataDate, SummaryObject.IntervalPeriod);
        }

        public override void BuildOriginalPlannedDataPoints()
        {
            PlannedDataPointsBuilder(true);
        }

        public override void BuildModifiedPlannedDataPoints()
        {
            PlannedDataPointsBuilder(false);
        }

        private void PlannedDataPointsBuilder(bool fromOriginalBaseline)
        {
            foreach (var ReportableObject in SummaryObject.ReportableObjects)
            {
                //Populate the progressItem variation adjustments
                ReportableObject.NonCumulative_VariationAdjustments =
                    new ObservableCollection<VariationAdjustment>(
                        SummaryObject.NonCumulative_VariationAdjustments.Where(
                            adjustment =>
                                adjustment.BaselineItemGuid ==
                                ReportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_ORIGINAL).ToList());
                ReportableObject.Cumulative_VariationAdjustments =
                    ISupportProgressReportingExtensions.PopulateCumulativeVariationAdjustments(
                        ReportableObject.NonCumulative_VariationAdjustments, SummaryObject.FirstAlignedDataDate,
                        SummaryObject.IntervalPeriod);

                //Assign the report date for stats display
                ReportableObject.ReportingDataDate = SummaryObject.ReportingDataDate;

                var currentBASELINE_ITEM = ReportableObject.BASELINE_ITEMJoinRATE;
                var currentWORKPACK = currentBASELINE_ITEM.BASELINE_ITEM.WORKPACK;
                if (currentWORKPACK == null)
                    continue;

                List<ProgressInfo> progressItemP6DataPoints;
                var assignmentLoadType = WorkpackAssignmentLoadType.Original;
                if (!fromOriginalBaseline)
                    assignmentLoadType = WorkpackAssignmentLoadType.Modified;

                IEnumerable<TASK> BASELINE_TASKS = null;
                P6Data.PROJECT BASELINE_P6PROJECT = null;
                if (fromOriginalBaseline)
                {
                    if (SummaryObject.LiveBASELINE.P6BASELINE_NAME != null &&
                        SummaryObject.LiveBASELINE.P6BASELINE_NAME != string.Empty)
                        BASELINE_TASKS = GetP6ScheduleTasks(SummaryObject.LiveBASELINE.P6BASELINE_NAME,
                            out BASELINE_P6PROJECT);
                }
                else
                {
                    if (SummaryObject.LiveBASELINE.P6MODBASELINE_NAME != null &&
                        SummaryObject.LiveBASELINE.P6MODBASELINE_NAME != string.Empty)
                        BASELINE_TASKS = GetP6ScheduleTasks(SummaryObject.LiveBASELINE.P6MODBASELINE_NAME,
                            out BASELINE_P6PROJECT);
                }

                if (SummaryObject.LivePROGRESS.P6PROGRESS_NAME != null &&
                    TryBuildP6DataPoints(BASELINE_P6PROJECT, BASELINE_TASKS, ReportableObject, DataPointsType.Planned,
                        assignmentLoadType, out progressItemP6DataPoints))
                {
                    if (fromOriginalBaseline)
                        ReportableObject.NonCumulative_OriginalDataPoints =
                            new ObservableCollection<ProgressInfo>(progressItemP6DataPoints);
                    else
                        ReportableObject.NonCumulative_PlannedDataPoints =
                            new ObservableCollection<ProgressInfo>(progressItemP6DataPoints);
                }
                else
                {
                    var workpackSuspensionPeriod = new List<Period>();

                    workpackSuspensionPeriod.Add(new Period((DateTime) currentWORKPACK.REVIEWSTARTDATE,
                        (DateTime) currentWORKPACK.REVIEWENDDATE));

                    var BaselineItemBaseUnits = currentBASELINE_ITEM.BASELINE_ITEM.ESTIMATED_HOURS;
                    var BaselineItemBaseCosts = currentBASELINE_ITEM.ESTIMATED_COSTS;
                    var BaselineItemTotalUnits = currentBASELINE_ITEM.BASELINE_ITEM.TOTAL_HOURS;
                    var BaselineItemTotalCosts = currentBASELINE_ITEM.TOTAL_COSTS;

                    List<ProgressInfo> plannedDataPoints;
                    if (fromOriginalBaseline) //if it's generating from original baseline ignore variation
                    {
                        var workingBaseTimeSpan = (DateTime) currentWORKPACK.ENDDATE -
                                                       (DateTime) currentWORKPACK.STARTDATE;
                        plannedDataPoints = ISupportProgressReportingExtensions.DataPointsGenerator(SummaryObject,
                            workingBaseTimeSpan, BaselineItemBaseUnits, BaselineItemBaseCosts,
                            (DateTime) currentWORKPACK.STARTDATE, currentBASELINE_ITEM.BASELINE_ITEM.GUID_ORIGINAL,
                            CurrencyConversion, workpackSuspensionPeriod, BaselineItemTotalUnits,
                            BaselineItemTotalCosts);
                        ReportableObject.NonCumulative_OriginalDataPoints =
                            new ObservableCollection<ProgressInfo>(plannedDataPoints);
                    }
                    else
                    {
                        var modifiedEndDateToUse = (DateTime) currentWORKPACK.ENDDATE;
                        if (currentWORKPACK.FORECASTENDDATE != null)
                            modifiedEndDateToUse = (DateTime) currentWORKPACK.FORECASTENDDATE;

                        var workingModifiedTimeSpan = modifiedEndDateToUse - (DateTime) currentWORKPACK.STARTDATE;
                        if (currentWORKPACK.FORECASTSTARTDATE != null &&
                            (DateTime) currentWORKPACK.FORECASTSTARTDATE > currentWORKPACK.ENDDATE)
                            workpackSuspensionPeriod.Add(new Period(((DateTime) currentWORKPACK.ENDDATE).AddDays(1),
                                (DateTime) currentWORKPACK.FORECASTSTARTDATE));

                        //Used to show sharktooth on variation
                        plannedDataPoints = ISupportProgressReportingExtensions.DataPointsGenerator(SummaryObject,
                            workingModifiedTimeSpan, BaselineItemBaseUnits, BaselineItemBaseCosts,
                            (DateTime) currentWORKPACK.STARTDATE, currentBASELINE_ITEM.BASELINE_ITEM.GUID_ORIGINAL,
                            CurrencyConversion, workpackSuspensionPeriod, null, null,
                            ReportableObject.Cumulative_VariationAdjustments);

                        //Used to show normalized variation
                        //plannedDataPoints = DataPointsGenerator(WorkingPeriod, progressInterval, BaselineItemTotalUnits, BaselineItemTotalCosts, this.CurrencyConversion, baselineItem.WORKPACK.STARTDATE, firstAlignedDataDate, baselineItem.GUID_ORIGINAL);
                        ReportableObject.NonCumulative_PlannedDataPoints =
                            new ObservableCollection<ProgressInfo>(plannedDataPoints);
                    }
                }
            }

            if (fromOriginalBaseline)
                SummaryObject.NonCumulative_OriginalDataPoints =
                    new ObservableCollection<ProgressInfo>(
                        SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_OriginalDataPoints));
            else
                SummaryObject.NonCumulative_PlannedDataPoints =
                    new ObservableCollection<ProgressInfo>(
                        SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_PlannedDataPoints));
        }

        /// <summary>
        /// Try to generate non-cumulative data points from P6 TASKs repository
        /// </summary>
        /// <param name="progressItem">current progress item to generate against, also populate progressItem nonCumulative datapoints collection</param>
        /// <param name="p6ScheduleTasks">context P6 tasks</param>
        /// <param name="firstAlignedDataDate">universal chart first aligned data date</param>
        /// <param name="progressInterval">period iteration interval</param>
        /// <param name="this.CurrencyConversion">currency conversion factor</param>
        /// <param name="nonCumulativeP6DataPoints">current progress item non cumulative data points</param>
        /// <returns>is generation success</returns>
        private bool TryBuildP6DataPoints(P6Data.PROJECT P6PROJECT, IEnumerable<TASK> P6TASKS,
            ReportableObject reportableObject, DataPointsType processingType,
            WorkpackAssignmentLoadType assignmentLoadType, out List<ProgressInfo> nonCumulativeP6DataPoints)
        {
            nonCumulativeP6DataPoints = new List<ProgressInfo>();
            if (reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.WORKPACK == null)
                return false;

            IEnumerable<WORKPACK_ASSIGNMENT> FilteredWORKPACK_ASSIGNMENTS;
            if (assignmentLoadType == WorkpackAssignmentLoadType.Modified)
            {
                FilteredWORKPACK_ASSIGNMENTS =
                    reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.WORKPACK.WORKPACK_ASSIGNMENT.Where(
                        assignment => assignment.ISMODIFIEDBASELINE == true);
                if (FilteredWORKPACK_ASSIGNMENTS.Count() == 0)
                    FilteredWORKPACK_ASSIGNMENTS =
                        reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.WORKPACK.WORKPACK_ASSIGNMENT;
                        //try to get original if modified is empty
            }
            else
            {
                FilteredWORKPACK_ASSIGNMENTS =
                    reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.WORKPACK.WORKPACK_ASSIGNMENT.Where(
                        assignment => assignment.ISMODIFIEDBASELINE == false);
            }

            if (P6PROJECT != null && FilteredWORKPACK_ASSIGNMENTS != null && P6TASKS != null &&
                FilteredWORKPACK_ASSIGNMENTS.Count() != 0 && P6TASKS.Count() != 0)
            {
                var lastRecalcDate = P6PROJECT.last_recalc_date;
                var currentBASELINE_ITEM = reportableObject.BASELINE_ITEMJoinRATE;
                var progressItemTotalHours = currentBASELINE_ITEM.BASELINE_ITEM.TOTAL_HOURS;
                var progressItemTotalCosts = currentBASELINE_ITEM.TOTAL_COSTS;

                foreach (var WORKPACK_ASSIGNMENTS in FilteredWORKPACK_ASSIGNMENTS)
                {
                    decimal CurrentAssignmentUnits;
                    decimal CurrentAssignmentCosts;
                    decimal CurrentAssignmentMaxUnits;
                    decimal CurrentAssignmentMinUnits;
                    DateTime CurrentAssignmentStartDate;
                    TimeSpan CurrentAssignmentWorkingPeriod;
                    var currentAssignmentTASK =
                        P6TASKS.FirstOrDefault(task => task.task_code == WORKPACK_ASSIGNMENTS.P6_ACTIVITYID);

                    if (processingType == DataPointsType.Planned)
                    {
                        //routine failed so report to revert to workpack dates calculation
                        if (currentAssignmentTASK == null || currentAssignmentTASK.target_start_date == null)
                            return false;

                        CurrentAssignmentStartDate = (DateTime) currentAssignmentTASK.target_start_date;
                        CurrentAssignmentWorkingPeriod = (DateTime) currentAssignmentTASK.target_end_date -
                                                         (DateTime) currentAssignmentTASK.target_start_date;
                        CurrentAssignmentMaxUnits = progressItemTotalHours;
                        CurrentAssignmentMinUnits = WORKPACK_ASSIGNMENTS.LOW_VALUE;
                    }
                    else if (processingType == DataPointsType.Earned)
                    {
                        CurrentAssignmentMaxUnits = reportableObject.TOTAL_EARNED_UNITS;
                        CurrentAssignmentMinUnits = WORKPACK_ASSIGNMENTS.LOW_VALUE;
                        if (WORKPACK_ASSIGNMENTS.LOW_VALUE > CurrentAssignmentMaxUnits)
                            continue;

                        if (currentAssignmentTASK.act_work_qty == null || currentAssignmentTASK.act_start_date == null ||
                            currentAssignmentTASK.act_end_date == null && lastRecalcDate == null)
                            return false;

                        CurrentAssignmentStartDate = (DateTime) currentAssignmentTASK.act_start_date;
                        if (currentAssignmentTASK.act_end_date == null)
                            CurrentAssignmentWorkingPeriod = (DateTime) lastRecalcDate -
                                                             (DateTime) currentAssignmentTASK.act_start_date;
                        else
                            CurrentAssignmentWorkingPeriod = (DateTime) currentAssignmentTASK.act_end_date -
                                                             (DateTime) currentAssignmentTASK.act_start_date;
                    }
                    else
                    {
                        if (currentAssignmentTASK.early_start_date == null ||
                            currentAssignmentTASK.early_end_date == null)
                            return false;

                        if (WORKPACK_ASSIGNMENTS.HIGH_VALUE < reportableObject.TOTAL_EARNED_UNITS)
                            continue;

                        CurrentAssignmentMaxUnits = progressItemTotalHours;
                        var earnedUnits = reportableObject.TOTAL_EARNED_UNITS;
                        if (earnedUnits > WORKPACK_ASSIGNMENTS.LOW_VALUE)
                            CurrentAssignmentMinUnits = earnedUnits + 1;
                        else
                            CurrentAssignmentMinUnits = WORKPACK_ASSIGNMENTS.LOW_VALUE;

                        CurrentAssignmentStartDate = (DateTime) currentAssignmentTASK.early_start_date;
                        CurrentAssignmentWorkingPeriod = (DateTime) currentAssignmentTASK.early_end_date -
                                                         (DateTime) currentAssignmentTASK.early_start_date;
                    }

                    if (WORKPACK_ASSIGNMENTS.HIGH_VALUE > CurrentAssignmentMaxUnits)
                        CurrentAssignmentUnits = CurrentAssignmentMaxUnits - CurrentAssignmentMinUnits + 1;
                    else
                        CurrentAssignmentUnits = WORKPACK_ASSIGNMENTS.HIGH_VALUE - CurrentAssignmentMinUnits + 1;

                    //use assignment units instead of estimated units because we are working on a subset of total units, also, this cost will be processed by conversion later
                    CurrentAssignmentCosts = CurrentAssignmentUnits * reportableObject.BASELINE_ITEMJoinRATE.ITEMRATE;
                    nonCumulativeP6DataPoints.AddRange(
                        ISupportProgressReportingExtensions.DataPointsGenerator(SummaryObject,
                            CurrentAssignmentWorkingPeriod, CurrentAssignmentUnits, CurrentAssignmentCosts,
                            CurrentAssignmentStartDate, currentBASELINE_ITEM.BASELINE_ITEM.GUID_ORIGINAL,
                            CurrencyConversion, null, null, null));
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public override void BuildVariationDataPoints()
        {
            if (SummaryObject.VARIATIONS == null)
                return;

            var approvedVariation =
                new ObservableCollection<VariationAdjustment>();
            foreach (var VARIATION in SummaryObject.VARIATIONS)
            {
                var approvedDate = VARIATION.APPROVED;
                if (VARIATION.GUID_PROJECT == SummaryObject.LivePROGRESS.GUID_PROJECT && approvedDate != null &&
                    VARIATION.BASELINE1 != null && VARIATION.BASELINE != null)
                {
                    var contextBASELINE_ITEMS =
                        SummaryObject.ReportableObjects.Select(x => x.BASELINE_ITEMJoinRATE);

                    foreach (var VARIATION_ITEM in VARIATION.VARIATION_ITEM)
                    {
                        if (VARIATION_ITEM.ACTION != VariationAction.Add &&
                            VARIATION_ITEM.ACTION != VariationAction.Append)
                            continue;

                        var contextBASELINE_ITEM =
                            contextBASELINE_ITEMS.FirstOrDefault(
                                x => x.BASELINE_ITEM.GUID_ORIGINAL == VARIATION_ITEM.GUID_ORIBASEITEM);
                        if (contextBASELINE_ITEM != null)
                            approvedVariation.Add(new VariationAdjustment()
                            {
                                AdjustmentDate = (DateTime) approvedDate,
                                AdjustmentUnits = VARIATION_ITEM.VARIATION_UNITS,
                                AdjustmentRate = contextBASELINE_ITEM.ITEMRATE,
                                BaselineItemGuid = contextBASELINE_ITEM.BASELINE_ITEM.GUID_ORIGINAL
                            });
                    }
                }
            }

            foreach (var ReportableObject in SummaryObject.ReportableObjects)
                ReportableObject.NonCumulative_VariationAdjustments =
                    new ObservableCollection<VariationAdjustment>(
                        approvedVariation.Where(
                            variation =>
                                variation.BaselineItemGuid ==
                                ReportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_ORIGINAL).ToList());

            SummaryObject.NonCumulative_VariationAdjustments =
                new ObservableCollection<VariationAdjustment>(approvedVariation);
        }

        /// <summary>
        /// Calculates each baselineItem earned data point while populating aggregate non cumulative earned data points
        /// </summary>
        /// <returns>Non cumulative earned progress data points</returns>
        public override void BuildEarnedDataPoints()
        {
            foreach (var reportableObject in SummaryObject.ReportableObjects)
            {
                //Assign the report date for stats display
                reportableObject.ReportingDataDate = SummaryObject.ReportingDataDate;
                List<ProgressInfo> progressItemP6DataPoints;
                if (SummaryObject.LivePROGRESS.P6PROGRESS_NAME != null &&
                    SummaryObject.LivePROGRESS.P6PROGRESS_NAME != string.Empty)
                    PROGRESS_TASKS = GetP6ScheduleTasks(SummaryObject.LivePROGRESS.P6PROGRESS_NAME,
                        out PROGRESS_PROJECT);

                var isProgressDataDateMatch = PROGRESS_PROJECT != null &&
                                               PROGRESS_PROJECT.last_recalc_date != null &&
                                               ((DateTime) PROGRESS_PROJECT.last_recalc_date).Date ==
                                               SummaryObject.LivePROGRESS.DATA_DATE;

                if (isProgressDataDateMatch &&
                    TryBuildP6DataPoints(PROGRESS_PROJECT, PROGRESS_TASKS, reportableObject,
                        DataPointsType.Earned, WorkpackAssignmentLoadType.Modified, out progressItemP6DataPoints))
                {
                    reportableObject.NonCumulative_EarnedDataPoints =
                        new ObservableCollection<ProgressInfo>(progressItemP6DataPoints);
                    reportableObject.isDataPointsGeneratedFromP6 = true;
                }

                else
                {
                    var progressItemEarnedDataPoints =
                        reportableObject.PROGRESS_ITEMSUpToCurrentDate.Select(x => new ProgressInfo()
                        {
                            BudgetedUnits = reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.TOTAL_HOURS,
                            BudgetedCosts = reportableObject.BASELINE_ITEMJoinRATE.TOTAL_COSTS * CurrencyConversion,
                            BaselineItemGuid = reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_ORIGINAL,
                            Units = x.EARNED_UNITS,
                            Costs =
                                x.EARNED_UNITS * reportableObject.BASELINE_ITEMJoinRATE.ITEMRATE *
                                CurrencyConversion,
                            ProgressDate = x.EARNED_DATE,
                        }).AsQueryable();

                    reportableObject.NonCumulative_EarnedDataPoints =
                        new ObservableCollection<ProgressInfo>(progressItemEarnedDataPoints);
                    var s = string.Empty;
                }
            }

            SummaryObject.NonCumulative_EarnedDataPoints =
                new ObservableCollection<ProgressInfo>(
                    SummaryObject.ReportableObjects.SelectMany(
                        progressItem => progressItem.NonCumulative_EarnedDataPoints));
        }

        public override void BuildRemainingDataPoints()
        {
            BuildProductivity();
            //Establishing aligned week ending dates
            var alignedWeekEndingDates =
                ISupportProgressReportingExtensions.GenerateAlignedDatesCollection(SummaryObject.FirstAlignedDataDate,
                    SummaryObject.FirstAlignedDataDate.AddYears(1), SummaryObject.IntervalPeriod);

            var progressItemsEarnedDataPointsBeforeDataDate =
                SummaryObject.ReportableObjects.SelectMany(
                        progressItem =>
                            progressItem.NonCumulative_EarnedDataPoints.Where(
                                dataPoint => dataPoint.ProgressDate.Date <= SummaryObject.ReportingDataDate.Date))
                    .AsQueryable();
            var totalEarnedUnits = progressItemsEarnedDataPointsBeforeDataDate.Sum(dataPoint => dataPoint.Units);
            if (totalEarnedUnits == 0)
                return;

            var exceptionPeriods = new List<Period>();
            exceptionPeriods.AddRange(ISupportProgressReportingExtensions.NonWorkingPeriods);

            foreach (var reportableObject in SummaryObject.ReportableObjects)
                if (reportableObject.RemainingUnitsAfterDataDate > 0)
                {
                    List<ProgressInfo> progressItemP6DataPoints;
                    if (reportableObject.isDataPointsGeneratedFromP6 &&
                        TryBuildP6DataPoints(PROGRESS_PROJECT, PROGRESS_TASKS, reportableObject,
                            DataPointsType.Remaining, WorkpackAssignmentLoadType.Modified, out progressItemP6DataPoints))
                    {
                        reportableObject.NonCumulative_RemainingPlannedDataPoints =
                            new ObservableCollection<ProgressInfo>(progressItemP6DataPoints);
                    }
                    else
                    {
                        DateTime startDateToUse;
                        DateTime firstAlignedWeekEndingDataDate;
                        decimal firstPeriodProRate;

                        if (reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.WORKPACK.FORECASTSTARTDATE != null)
                            startDateToUse =
                                (DateTime)
                                reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.WORKPACK.FORECASTSTARTDATE;
                        else
                            startDateToUse =
                                (DateTime) reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.WORKPACK.STARTDATE;

                        //when workpack dates are later than data date use workpack dates but have a prorate value ready for first period
                        if (startDateToUse > SummaryObject.LivePROGRESS.DATA_DATE)
                        {
                            firstAlignedWeekEndingDataDate =
                                alignedWeekEndingDates.FirstOrDefault(dates => dates.Date >= startDateToUse);
                            firstPeriodProRate =
                                Convert.ToDecimal(
                                    (firstAlignedWeekEndingDataDate.AddSeconds(1) - startDateToUse).TotalDays /
                                    SummaryObject.IntervalPeriod.TotalDays);
                        }
                        else
                        {
                            firstAlignedWeekEndingDataDate =
                                SummaryObject.LivePROGRESS.DATA_DATE.AddDays(SummaryObject.IntervalPeriod.Days);
                            firstPeriodProRate = 1;
                        }

                        var maxInefficiency = 0.5M;
                        decimal currentEfficiency = 0;
                        if (reportableObject.ActualProductivity != 0 && reportableObject.BaselineProductivity != 0)
                            currentEfficiency = reportableObject.ActualProductivity /
                                                reportableObject.BaselineProductivity;

                        reportableObject.NonCumulative_RemainingPlannedDataPoints =
                            ISupportProgressReportingExtensions.RemainingDataPointsGenerator(SummaryObject,
                                reportableObject, firstAlignedWeekEndingDataDate, exceptionPeriods,
                                reportableObject.RemainingUnitsAfterDataDate, reportableObject.BaselineProductivity,
                                CurrencyConversion, firstPeriodProRate);

                        //if there's a planned finish date based on baseline productivity, inflate periodic units/costs
                        var plannedLimitDate = reportableObject.NonCumulative_RemainingPlannedDataPoints == null ||
                                                     reportableObject.NonCumulative_RemainingPlannedDataPoints.Count ==
                                                     0
                            ? (DateTime?) null
                            : reportableObject.NonCumulative_RemainingPlannedDataPoints.Last().ProgressDate;

                        if (currentEfficiency < maxInefficiency)
                            currentEfficiency = maxInefficiency;

                        var inflatedInefficientUnits = currentEfficiency > 0
                            ? reportableObject.RemainingUnitsAfterDataDate / currentEfficiency
                            : reportableObject.RemainingUnitsAfterDataDate;
                        reportableObject.NonCumulative_RemainingCurrentDataPoints =
                            ISupportProgressReportingExtensions.RemainingDataPointsGenerator(SummaryObject,
                                reportableObject, firstAlignedWeekEndingDataDate, exceptionPeriods,
                                inflatedInefficientUnits, reportableObject.ActualProductivity, CurrencyConversion,
                                firstPeriodProRate, plannedLimitDate);
                    }
                }

            //extract all data points out to be used as an overall summary
            SummaryObject.NonCumulative_RemainingPlannedDataPoints =
                new ObservableCollection<ProgressInfo>(
                    SummaryObject.ReportableObjects.SelectMany(
                        progressItem => progressItem.NonCumulative_RemainingPlannedDataPoints));
            SummaryObject.NonCumulative_RemainingCurrentDataPoints =
                new ObservableCollection<ProgressInfo>(
                    SummaryObject.ReportableObjects.SelectMany(
                        progressItem => progressItem.NonCumulative_RemainingCurrentDataPoints));
        }

        /// <summary>
        /// Populate all progress item productivity
        /// </summary>
        private void BuildProductivity()
        {
            //Establish exception periods
            var exceptionPeriods = new List<Period>();
            exceptionPeriods.AddRange(ISupportProgressReportingExtensions.NonWorkingPeriods);
            foreach (var reportableItem in SummaryObject.ReportableObjects)
                if (reportableItem.RemainingUnitsAfterDataDate > 0)
                    BuildReportableObjectProductivity(reportableItem, exceptionPeriods);
        }

        private void BuildReportableObjectProductivity(ReportableObject reportableObject, List<Period> exceptionPeriods)
        {
            //When productivity is below this threshold, escalate to workpack or project
            var minimumProductivityBeforeEscalating = 0.001M;

            if (reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.WORKPACK == null)
                return;

            //establish dates for productivity assessment
            var workpackStart = (DateTime) reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.WORKPACK.STARTDATE;
            var workpackEnd = (DateTime) reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.WORKPACK.ENDDATE;
            var workpackForecastStart =
                reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.WORKPACK.FORECASTSTARTDATE;
            var workpackForecastEnd =
                reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.WORKPACK.FORECASTENDDATE;

            DateTime assessmentStartDate;
            DateTime assessmentEndDate;

            if (workpackForecastStart != null)
                assessmentStartDate = (DateTime) workpackForecastStart;
            else
                assessmentStartDate = workpackStart;

            if (workpackForecastEnd != null)
                assessmentEndDate = (DateTime) workpackForecastEnd;
            else
                assessmentEndDate = workpackEnd;

            if (reportableObject.ReportingDataDate > assessmentStartDate)
                assessmentStartDate = reportableObject.ReportingDataDate;
            if (reportableObject.ReportingDataDate > assessmentEndDate)
                assessmentEndDate = reportableObject.ReportingDataDate;

            var assessmentPeriod = new Period(assessmentStartDate.Date, assessmentEndDate.Date);

            //establish workpack productivity to be used when deliverable productivity is too low
            var currentWORKPACK = reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.WORKPACK;

            //calculate deliverable productivity
            reportableObject.VariationProductivity =
                ISupportProgressReportingExtensions.CalculatePlannedProductivity(assessmentPeriod, exceptionPeriods,
                    reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.TOTAL_HOURS);
            //progressItem.ProgressItem_CurrentProductivity = UnifiedCalculationMethods.CalculateProductivity(assessmentPeriod)

            decimal workpackVarProductivity = 0;
            if (reportableObject.VariationProductivity < minimumProductivityBeforeEscalating)
            {
                var WorkpackBASELINE_ITEMJoinRATES =
                    SummaryObject.ReportableObjects.Where(
                            x => x.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_WORKPACK == currentWORKPACK.GUID)
                        .Select(x => x.BASELINE_ITEMJoinRATE);

                //not checking for progressItemWorkpack null because all progress item should have workpacks assigned if the user 
                var totalWorkpackUnits = WorkpackBASELINE_ITEMJoinRATES.Sum(x => x.BASELINE_ITEM.TOTAL_HOURS);

                workpackVarProductivity =
                    ISupportProgressReportingExtensions.CalculatePlannedProductivity(assessmentPeriod, exceptionPeriods,
                        totalWorkpackUnits);
                if (workpackVarProductivity > 0)
                    reportableObject.VariationProductivity = workpackVarProductivity;
            }

            decimal workpackBaseProductivity = 0;
            //not checking for progressItemWorkpack null because all progress item should have workpacks assigned if the user 
            var totalWorkpackBudgetedUnits = currentWORKPACK == null || currentWORKPACK.BASELINE_ITEM == null
                ? 0
                : currentWORKPACK.BASELINE_ITEM.Sum(pItem => pItem.ESTIMATED_HOURS);
            workpackBaseProductivity = ISupportProgressReportingExtensions.CalculatePlannedProductivity(
                assessmentPeriod, exceptionPeriods, reportableObject.RemainingUnitsAfterDataDate);

            if (reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.ESTIMATED_HOURS == 0)
            {
                reportableObject.BaselineProductivity = workpackBaseProductivity;
            }
            else
            {
                reportableObject.BaselineProductivity =
                    ISupportProgressReportingExtensions.CalculatePlannedProductivity(assessmentPeriod, exceptionPeriods,
                        reportableObject.RemainingUnitsAfterDataDate);
                //apply normalized productivity for unusually low calculated productivity
                if (reportableObject.BaselineProductivity < minimumProductivityBeforeEscalating)
                    if (workpackBaseProductivity > 0)
                        reportableObject.BaselineProductivity = workpackBaseProductivity;
            }

            var deliverablePlannedDataPointsOnOrBeforeDataDate =
                reportableObject.NonCumulative_PlannedDataPoints.Where(
                    dataPoint => dataPoint.ProgressDate <= SummaryObject.LivePROGRESS.DATA_DATE).ToList();
            decimal currentEfficiency = 0;
            if (reportableObject.TOTAL_EARNED_UNITS != 0 && deliverablePlannedDataPointsOnOrBeforeDataDate.Count() > 0)
            {
                var deliverablePlannedUnitsOnOrBeforeDataDate =
                    deliverablePlannedDataPointsOnOrBeforeDataDate.Sum(dataPoint => dataPoint.Units);
                if (deliverablePlannedUnitsOnOrBeforeDataDate > 0)
                    currentEfficiency = reportableObject.TOTAL_EARNED_UNITS / deliverablePlannedUnitsOnOrBeforeDataDate;

                reportableObject.ActualProductivity = reportableObject.BaselineProductivity * currentEfficiency;
            }
            else
            {
                reportableObject.ActualProductivity = reportableObject.BaselineProductivity;
            }
            //assume productivity of 1 because there are no units to measure against

            if (reportableObject.ActualProductivity < minimumProductivityBeforeEscalating)
                reportableObject.ActualProductivity = reportableObject.BaselineProductivity;
        }

        /// <summary>
        /// Calculates each baselineItem burned/actual data point while populating aggregate non cumulative burned/actual data points
        /// </summary>
        /// <returns>Non cumulative earned progress data points</returns>
        public override void BuildBurnedDataPoints()
        {
            var nonCumulative_BurnedDataPoints = new ObservableCollection<ProgressInfo>();
            var firstAlignedDataDate = SummaryObject.FirstAlignedDataDate;
            var progressInterval = SummaryObject.IntervalPeriod;
            var loopDate = firstAlignedDataDate;

            IEnumerable<WORKPACK> WORKPACKS = SummaryObject.LiveBASELINE.PROJECT.WORKPACK;
            var qualifiedWorkpack = WORKPACKS == null
                ? new List<string>()
                : WORKPACKS.Select(x => x.INTERNAL_NAME1);
            var PrimeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            var jobTransactions = from JOBTRANS in PrimeroUnitOfWork.JOB_TRANSACTIONS
                join JOBCOST_HDR2 in PrimeroUnitOfWork.JOBCOST_HDR
                on JOBTRANS.MASTER_JOBNO equals JOBCOST_HDR2.JOBNO
                join JOBCOST_HDR1 in PrimeroUnitOfWork.JOBCOST_HDR
                on JOBTRANS.JOBNO equals JOBCOST_HDR1.JOBNO
                join JOBCOST_RESOURCE in PrimeroUnitOfWork.JOBCOST_RESOURCE
                on JOBTRANS.STAFFNO equals JOBCOST_RESOURCE.SEQNO
                where JOBCOST_HDR2.JOBCODE == SummaryObject.LiveBASELINE.PROJECT.NUMBER && JOBTRANS.TRANSTYPE == "T"
                select
                new
                {
                    JOBCOST_HDR1.JOBCODE,
                    JOBTRANS.QUANTITY,
                    JOBTRANS.LINETOTAL,
                    JOBTRANS.LINECOST,
                    JOBTRANS.TRANSDATE,
                    JOBCOST_RESOURCE.RESOURCENAME
                };

            var jobTransactionsList = jobTransactions.ToList();
            if (jobTransactionsList.Count == 0)
                return;

            var alignedDataDates =
                ISupportProgressReportingExtensions.GenerateAlignedDatesCollection(firstAlignedDataDate,
                    jobTransactionsList.Max(x => x.TRANSDATE).Value, progressInterval);
            foreach (var jobTransaction in jobTransactionsList)
                if (qualifiedWorkpack.Contains(jobTransaction.JOBCODE))
                    nonCumulative_BurnedDataPoints.Add(new ProgressInfo()
                    {
                        BudgetedUnits = 0,
                        BudgetedCosts = 0,
                        Units = (decimal) jobTransaction.QUANTITY,
                        Costs = (decimal) jobTransaction.LINETOTAL * CurrencyConversion,
                        Actuals = (decimal) jobTransaction.LINECOST,
                        ProgressDate = alignedDataDates.FirstOrDefault(dates => dates.Date >= jobTransaction.TRANSDATE),
                        BaselineItemGuid = Guid.Empty,
                        WorkpackName = jobTransaction.JOBCODE,
                        ResourceName = jobTransaction.RESOURCENAME,
                        Quantity = (decimal) jobTransaction.QUANTITY
                    });

            SummaryObject.NonCumulative_BurnedDataPoints = nonCumulative_BurnedDataPoints;
        }

        public override void BuildActualDataPoints()
        {
            var convertBurnedToActualDataPoints = new List<ProgressInfo>();
            SummaryObject.NonCumulative_BurnedDataPoints.ToList()
                .ForEach(dataPoint => convertBurnedToActualDataPoints.Add(new ProgressInfo()
                {
                    BudgetedCosts = dataPoint.BudgetedCosts,
                    BudgetedUnits = dataPoint.BudgetedUnits,
                    Costs = dataPoint.Actuals,
                    Actuals = dataPoint.Actuals,
                    ProgressDate = dataPoint.ProgressDate,
                    BaselineItemGuid = dataPoint.BaselineItemGuid,
                    Units = dataPoint.Units,
                    WorkpackGuid = dataPoint.WorkpackGuid,
                    WorkpackName = dataPoint.WorkpackName,
                    ResourceName = dataPoint.ResourceName,
                    Quantity = dataPoint.Quantity
                }));

            SummaryObject.NonCumulative_ActualDataPoints =
                new ObservableCollection<ProgressInfo>(convertBurnedToActualDataPoints);
        }

        private IEnumerable<TASK> GetP6ScheduleTasks(string shortName, out P6Data.PROJECT P6Schedule)
        {
            if (shortName != string.Empty)
            {
                var PROJECTRepository = P6UnitOfWork.PROJECT;
                P6Schedule = PROJECTRepository.FirstOrDefault(x => x.proj_short_name == shortName);

                if (P6Schedule != null)
                    return P6Schedule.TASK;
            }
            else
            {
                P6Schedule = null;
            }

            return null;
        }

        private enum DataPointsType
        {
            Planned = 0,
            Earned = 1,
            Remaining = 2
        }

        private enum WorkpackAssignmentLoadType
        {
            Original,
            Modified,
            Both
        }
    }
}