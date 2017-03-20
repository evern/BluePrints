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
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static BluePrints.Data.BluePrintsEntities;

namespace BluePrints.Common.ViewModel.Reporting
{
    public abstract class SummaryBuilder
    {
        SummarizableObject summaryObject;
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
            WORKPACKDashboard.ReportableObjects = PROJECTDashboard.ReportableObjects.Where(x => x.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_WORKPACK == WORKPACKDashboard.GUID);
            string activeWORKPACKName;
            if (PROJECTDashboard.PROJECT.USELEGACYWORKPACK)
                activeWORKPACKName = WORKPACKDashboard.WORKPACK.INTERNAL_NAME1;
            else
                activeWORKPACKName = WORKPACKDashboard.WORKPACK.INTERNAL_NAME2;

            IEnumerable<ProgressInfo> workpackBurnedDataPoints = PROJECTDashboard.NonCumulative_BurnedDataPoints.Where(x => x.WorkpackName == activeWORKPACKName).OrderByDescending(x => x.ProgressDate);
            IEnumerable<ProgressInfo> workpackActualDataPoints = PROJECTDashboard.NonCumulative_ActualDataPoints.Where(x => x.WorkpackName == activeWORKPACKName).OrderByDescending(x => x.ProgressDate);
            WORKPACKDashboard.NonCumulative_BurnedDataPoints = new ObservableCollection<ProgressInfo>(workpackBurnedDataPoints);
            WORKPACKDashboard.NonCumulative_ActualDataPoints = new ObservableCollection<ProgressInfo>(workpackActualDataPoints);
            WORKPACKDashboard.FirstAlignedDataDate = PROJECTDashboard.FirstAlignedDataDate;
            WORKPACKDashboard.LiveBASELINE = PROJECTDashboard.LiveBASELINE;
            WORKPACKDashboard.LivePROGRESS = PROJECTDashboard.LivePROGRESS;
            WORKPACKDashboard.ReportingDataDate = PROJECTDashboard.ReportingDataDate;
            WORKPACKDashboard.IntervalPeriod = PROJECTDashboard.IntervalPeriod;
            this.SummaryObject = WORKPACKDashboard;
        }

        public override void BuildVariationDataPoints()
        {
            SummaryObject.NonCumulative_VariationAdjustments = new ObservableCollection<VariationAdjustment>(SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_VariationAdjustments));
        }

        public override void BuildOriginalPlannedDataPoints()
        {
            SummaryObject.NonCumulative_OriginalDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_OriginalDataPoints));
        }

        public override void BuildModifiedPlannedDataPoints()
        {
            SummaryObject.NonCumulative_PlannedDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_PlannedDataPoints));
        }

        public override void BuildEarnedDataPoints()
        {
            SummaryObject.NonCumulative_EarnedDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_EarnedDataPoints));
        }

        public override void BuildActualDataPoints()
        {
            throw new InvalidOperationException("there is no need to roll up non cumulative actual data points from ReportableObjects.");
        }

        public override void BuildBurnedDataPoints()
        {
            throw new InvalidOperationException("there is no need to roll up non cumulative burned data points from ReportableObjects.");
        }

        public override void BuildRemainingDataPoints()
        {
            SummaryObject.NonCumulative_RemainingCurrentDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_RemainingCurrentDataPoints));
            SummaryObject.NonCumulative_RemainingPlannedDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_RemainingPlannedDataPoints));
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
        public ReportableObjectRollUp(SummarizableObject summaryObject, WORKPACK workpack, SummarizableObject projectSummary)
        {
            summaryObject.ReportableObjects = projectSummary.ReportableObjects.Where(x => x.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_WORKPACK == workpack.GUID).ToArray().ToList();
            var nonCumulativeBurnedList = projectSummary.NonCumulative_BurnedDataPoints.Where(x => x.WorkpackName == workpack.INTERNAL_NAME1).OrderByDescending(x => x.ProgressDate).ToArray().ToList();
            var nonCumulativeActualList = projectSummary.NonCumulative_ActualDataPoints.Where(x => x.WorkpackName == workpack.INTERNAL_NAME1).OrderByDescending(x => x.ProgressDate).ToArray().ToList();
            summaryObject.NonCumulative_BurnedDataPoints = new ObservableCollection<ProgressInfo>(nonCumulativeBurnedList);
            summaryObject.NonCumulative_ActualDataPoints = new ObservableCollection<ProgressInfo>(nonCumulativeActualList);
            summaryObject.FirstAlignedDataDate = projectSummary.FirstAlignedDataDate;
            summaryObject.LivePROGRESS = projectSummary.LivePROGRESS;
            summaryObject.LiveBASELINE = projectSummary.LiveBASELINE;
            summaryObject.ReportingDataDate = projectSummary.LivePROGRESS.DATA_DATE;
            summaryObject.IntervalPeriod = projectSummary.IntervalPeriod;
            this.SummaryObject = summaryObject;
        }

        public override void BuildVariationDataPoints()
        {
            SummaryObject.NonCumulative_VariationAdjustments = new ObservableCollection<VariationAdjustment>(SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_VariationAdjustments));
        }

        public override void BuildOriginalPlannedDataPoints()
        {
            SummaryObject.NonCumulative_OriginalDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_OriginalDataPoints));
        }

        public override void BuildModifiedPlannedDataPoints()
        {
            SummaryObject.NonCumulative_PlannedDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_PlannedDataPoints));
        }

        public override void BuildEarnedDataPoints()
        {
            SummaryObject.NonCumulative_EarnedDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_EarnedDataPoints));
        }

        public override void BuildActualDataPoints()
        {
            throw new InvalidOperationException("there is no need to roll up non cumulative actual data points from ReportableObjects.");
        }

        public override void BuildBurnedDataPoints()
        {
            throw new InvalidOperationException("there is no need to roll up non cumulative burned data points from ReportableObjects.");
        }

        public override void BuildRemainingDataPoints()
        {
            SummaryObject.NonCumulative_RemainingCurrentDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_RemainingCurrentDataPoints));
            SummaryObject.NonCumulative_RemainingPlannedDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_RemainingPlannedDataPoints));
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
        IBluePrintsEntitiesUnitOfWork BluePrintsUnitOfWork { get; set; }
        IP6EntitiesUnitOfWork P6UnitOfWork { get; set; }
        decimal CurrencyConversion { get; set; }
        IEnumerable<TASK> PROGRESS_TASKS { get; set; }
        BluePrints.P6Data.PROJECT PROGRESS_PROJECT = null;
        Data.PROJECT CURRENTPROJECT = null;

        public PROJECTSummaryBuilder(SummarizableObject summaryObject, IBluePrintsEntitiesUnitOfWork BluePrintsUOW = null, IP6EntitiesUnitOfWork P6UOW = null, Data.PROJECT currentPROJECT = null)
        {
            this.CURRENTPROJECT = currentPROJECT;

            if (summaryObject.LivePROGRESS == null || summaryObject.LiveBASELINE == null)
                return;
            
            this.CurrencyConversion = summaryObject.LiveBASELINE.PROJECT.CURRENCYCONVERSION;
            this.SummaryObject = summaryObject;
            this.SummaryObject.ReportingDataDate = this.SummaryObject.LivePROGRESS.DATA_DATE;

            if (BluePrintsUOW == null)
                BluePrintsUOW = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            else
                this.BluePrintsUnitOfWork = BluePrintsUOW;

            if (P6UOW == null)
                this.P6UnitOfWork = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            else
                this.P6UnitOfWork = P6UOW;

            this.SummaryObject.IntervalPeriod = ISupportProgressReportingExtensions.ConvertProgressIntervalToPeriod(SummaryObject.LivePROGRESS);
            this.SummaryObject.FirstAlignedDataDate = ISupportProgressReportingExtensions.GenerateFirstAlignedDataDate(SummaryObject.LivePROGRESS);
        }

        public override void SummarizeDataPoints()
        {
            ISupportProgressReportingExtensions.GenerateCumulativeSummaryDataPoints(this.SummaryObject);
        }

        public override void SummarizeNestedSummaryObjectDataPoints()
        {
            foreach (ReportableObject reportableObject in SummaryObject.ReportableObjects)
            {
                ISupportProgressReportingExtensions.GenerateCumulativeSummaryDataPoints(reportableObject, this.SummaryObject.FirstAlignedDataDate, this.SummaryObject.IntervalPeriod);
            }
        }

        public override void BuildOriginalPlannedDataPoints()
        {
            //PlannedDataPointsBuilderFromDatabase(CURRENTPROJECT.NUMBER, false);
            PlannedDataPointsBuilder(true);
        }

        public override void BuildModifiedPlannedDataPoints()
        {
            //PlannedDataPointsBuilderFromDatabase(CURRENTPROJECT.NUMBER, true);
            PlannedDataPointsBuilder(false);
        }

        private void PlannedDataPointsBuilderFromDatabase(string ProjectNumber, bool isOriginal)
        {
            BluePrintsEntities bluePrintDataContext = new BluePrintsEntities();
            ObjectResult<StoredProcedure_DeliverablesDataPoints> deliverablesDataPoints = bluePrintDataContext.GetDataPointsByProject(ProjectNumber, isOriginal);
            PlannedDataPointsBuilder(isOriginal, deliverablesDataPoints.ToList());
        }

        private void PlannedDataPointsBuilder(bool isOriginal, IEnumerable<StoredProcedure_DeliverablesDataPoints> DataPointsCollection = null)
        {
            WorkpackAssignmentLoadType assignmentLoadType = WorkpackAssignmentLoadType.Original;
            if (!isOriginal)
                assignmentLoadType = WorkpackAssignmentLoadType.Modified;

            IEnumerable<TASK> BASELINE_TASKS = null;
            P6Data.PROJECT BASELINE_P6PROJECT = null;
            if (isOriginal)
            {
                if (this.SummaryObject.LiveBASELINE.P6BASELINE_NAME != null && this.SummaryObject.LiveBASELINE.P6BASELINE_NAME != string.Empty)
                    BASELINE_TASKS = GetP6ScheduleTasks(SummaryObject.LiveBASELINE.P6BASELINE_NAME, out BASELINE_P6PROJECT);
            }
            else
            {
                if (this.SummaryObject.LiveBASELINE.P6MODBASELINE_NAME != null && this.SummaryObject.LiveBASELINE.P6MODBASELINE_NAME != string.Empty)
                    BASELINE_TASKS = GetP6ScheduleTasks(SummaryObject.LiveBASELINE.P6MODBASELINE_NAME, out BASELINE_P6PROJECT);
            }

            Dictionary<Guid, decimal> workpackAssignedUnits = new Dictionary<Guid, decimal>();
            foreach (ReportableObject ReportableObject in SummaryObject.ReportableObjects)
            {
                //Populate the progressItem variation adjustments
                ReportableObject.NonCumulative_VariationAdjustments = new ObservableCollection<VariationAdjustment>(SummaryObject.NonCumulative_VariationAdjustments.Where(adjustment => adjustment.BaselineItemGuid == ReportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_ORIGINAL).ToList());
                ReportableObject.Cumulative_VariationAdjustments = ISupportProgressReportingExtensions.PopulateCumulativeVariationAdjustments(ReportableObject.NonCumulative_VariationAdjustments, SummaryObject.FirstAlignedDataDate, SummaryObject.IntervalPeriod);

                //Assign the report date for stats display
                ReportableObject.ReportingDataDate = SummaryObject.ReportingDataDate;

                BASELINE_ITEMProjection currentBASELINE_ITEM = ReportableObject.BASELINE_ITEMJoinRATE;
                WORKPACK currentWORKPACK = currentBASELINE_ITEM.BASELINE_ITEM.WORKPACK;
                if (currentWORKPACK == null)
                    continue;

                List<ProgressInfo> progressItemP6DataPoints;
                if (TryBuildP6DataPoints(BASELINE_P6PROJECT, BASELINE_TASKS, ReportableObject, DataPointsType.Planned, assignmentLoadType, workpackAssignedUnits, out progressItemP6DataPoints))
                {
                    if (isOriginal)
                        ReportableObject.NonCumulative_OriginalDataPoints = new ObservableCollection<ProgressInfo>(progressItemP6DataPoints);
                    else
                        ReportableObject.NonCumulative_PlannedDataPoints = new ObservableCollection<ProgressInfo>(progressItemP6DataPoints);
                }
                else
                {
                    List<Period> workpackSuspensionPeriod = new List<Period>();
                    workpackSuspensionPeriod.Add(new Period((DateTime)currentWORKPACK.REVIEWSTARTDATE, (DateTime)currentWORKPACK.REVIEWENDDATE));

                    decimal BaselineItemBaseUnits = currentBASELINE_ITEM.BASELINE_ITEM.ESTIMATED_HOURS;
                    decimal BaselineItemBaseCosts = currentBASELINE_ITEM.ESTIMATED_COSTS;
                    decimal BaselineItemTotalUnits = currentBASELINE_ITEM.BASELINE_ITEM.TOTAL_HOURS;
                    decimal BaselineItemTotalCosts = currentBASELINE_ITEM.TOTAL_COSTS;

                    List<ProgressInfo> plannedDataPoints;
                    if (isOriginal) //if it's generating from original baseline ignore variation
                    {
                        if(DataPointsCollection != null)
                        {
                            IEnumerable<StoredProcedure_DeliverablesDataPoints> currentDeliverableDataPoints = DataPointsCollection.Where(x => x.GUID_ORIGINAL == ReportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_ORIGINAL).OrderBy(x => x.UniversalPeriodStartDate);
                            ReportableObject.NonCumulative_OriginalDataPoints = new ObservableCollection<ProgressInfo>(ConvertDeliverablesDataPointToProgressInfo(currentDeliverableDataPoints, ReportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.TOTAL_HOURS, ReportableObject.BASELINE_ITEMJoinRATE.TOTAL_COSTS));
                        }
                        else
                        {
                            TimeSpan workingBaseTimeSpan = (DateTime)currentWORKPACK.ENDDATE - (DateTime)currentWORKPACK.STARTDATE;
                            plannedDataPoints = ISupportProgressReportingExtensions.DataPointsGenerator(SummaryObject, workingBaseTimeSpan, BaselineItemBaseUnits, BaselineItemBaseCosts, (DateTime)currentWORKPACK.STARTDATE, currentBASELINE_ITEM.BASELINE_ITEM.GUID_ORIGINAL, this.CurrencyConversion, workpackSuspensionPeriod, BaselineItemTotalUnits, BaselineItemTotalCosts);
                            ReportableObject.NonCumulative_OriginalDataPoints = new ObservableCollection<ProgressInfo>(plannedDataPoints);
                        }
                    }
                    else
                    {
                        if (DataPointsCollection != null)
                        {
                            IEnumerable<StoredProcedure_DeliverablesDataPoints> currentDeliverableDataPoints = DataPointsCollection.Where(x => x.GUID_ORIGINAL == ReportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_ORIGINAL).OrderBy(x => x.UniversalPeriodStartDate);
                            ReportableObject.NonCumulative_PlannedDataPoints = new ObservableCollection<ProgressInfo>(ConvertDeliverablesDataPointToProgressInfo(currentDeliverableDataPoints, ReportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.TOTAL_HOURS, ReportableObject.BASELINE_ITEMJoinRATE.TOTAL_COSTS));
                        }
                        else
                        {
                            DateTime modifiedEndDateToUse = (DateTime)currentWORKPACK.ENDDATE;
                            if (currentWORKPACK.FORECASTENDDATE != null)
                                modifiedEndDateToUse = (DateTime)currentWORKPACK.FORECASTENDDATE;

                            TimeSpan workingModifiedTimeSpan = modifiedEndDateToUse - (DateTime)currentWORKPACK.STARTDATE;
                            if (currentWORKPACK.FORECASTSTARTDATE != null && ((DateTime)currentWORKPACK.FORECASTSTARTDATE) > currentWORKPACK.ENDDATE)
                                workpackSuspensionPeriod.Add(new Period(((DateTime)currentWORKPACK.ENDDATE).AddDays(1), (DateTime)currentWORKPACK.FORECASTSTARTDATE));

                            //Used to show sharktooth on variation
                            plannedDataPoints = ISupportProgressReportingExtensions.DataPointsGenerator(SummaryObject, workingModifiedTimeSpan, BaselineItemBaseUnits, BaselineItemBaseCosts, (DateTime)currentWORKPACK.STARTDATE, currentBASELINE_ITEM.BASELINE_ITEM.GUID_ORIGINAL, this.CurrencyConversion, workpackSuspensionPeriod, null, null, ReportableObject.Cumulative_VariationAdjustments);

                            //Used to show normalized variation
                            //plannedDataPoints = DataPointsGenerator(WorkingPeriod, progressInterval, BaselineItemTotalUnits, BaselineItemTotalCosts, this.CurrencyConversion, baselineItem.WORKPACK.STARTDATE, firstAlignedDataDate, baselineItem.GUID_ORIGINAL);
                            ReportableObject.NonCumulative_PlannedDataPoints = new ObservableCollection<ProgressInfo>(plannedDataPoints);
                        }
                    }
                }
            }

            if (isOriginal)
                SummaryObject.NonCumulative_OriginalDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_OriginalDataPoints));
            else
                SummaryObject.NonCumulative_PlannedDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_PlannedDataPoints));
        }

        private IEnumerable<ProgressInfo> ConvertDeliverablesDataPointToProgressInfo(IEnumerable<StoredProcedure_DeliverablesDataPoints> deliverablesDataPoints, decimal BudgetedUnits, decimal BudgetedCosts )
        {
            List<ProgressInfo> progressInfoConversion = new List<ProgressInfo>();
            foreach (StoredProcedure_DeliverablesDataPoints deliverablesDataPoint in deliverablesDataPoints)
            {
                progressInfoConversion.Add(new ProgressInfo
                {
                    BaselineItemGuid = deliverablesDataPoint.GUID_ORIGINAL, 
                    BudgetedUnits = BudgetedUnits, 
                    BudgetedCosts = BudgetedCosts, 
                    Costs = Convert.ToDecimal(deliverablesDataPoint.PeriodPlannedPrice),
                    Units = Convert.ToDecimal(deliverablesDataPoint.PeriodPlannedUnits), 
                    ProgressDate = deliverablesDataPoint.UniversalPeriodEndDate
                });
            }

            return progressInfoConversion;
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
        private bool TryBuildP6DataPoints(P6Data.PROJECT P6PROJECT, IEnumerable<TASK> P6TASKS, ReportableObject reportableObject, DataPointsType processingType, WorkpackAssignmentLoadType assignmentLoadType, Dictionary<Guid, Decimal> workpackAssignedUnits, out List<ProgressInfo> nonCumulativeP6DataPoints)
        {
            nonCumulativeP6DataPoints = new List<ProgressInfo>();
            if (reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_WORKPACK == null)
                return false;

            IEnumerable<WORKPACK_ASSIGNMENT> currentWORKPACK_ASSIGNMENTS;
            Guid currentWORKPACKGuid = (Guid)reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_WORKPACK;

            if (assignmentLoadType == WorkpackAssignmentLoadType.Modified && reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.WORKPACK.WORKPACK_ASSIGNMENT.Any(assignment => assignment.ISMODIFIEDBASELINE == true))
                currentWORKPACK_ASSIGNMENTS = reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.WORKPACK.WORKPACK_ASSIGNMENT.Where(assignment => assignment.ISMODIFIEDBASELINE == true);
            else
                currentWORKPACK_ASSIGNMENTS = reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.WORKPACK.WORKPACK_ASSIGNMENT.Where(assignment => assignment.ISMODIFIEDBASELINE == false);

            if (P6PROJECT != null && currentWORKPACK_ASSIGNMENTS != null && P6TASKS != null && currentWORKPACK_ASSIGNMENTS.Count() != 0 && P6TASKS.Count() != 0)
            {
                currentWORKPACK_ASSIGNMENTS = currentWORKPACK_ASSIGNMENTS.OrderBy(x => x.LOW_VALUE);
                DateTime? lastRecalcDate = P6PROJECT.last_recalc_date;
                BASELINE_ITEMProjection currentBASELINE_ITEM = reportableObject.BASELINE_ITEMJoinRATE;
                decimal totalUnits = currentBASELINE_ITEM.BASELINE_ITEM.TOTAL_HOURS;
                decimal totalEarnedUnits = reportableObject.TOTAL_EARNED_UNITS;
                decimal totalCosts = currentBASELINE_ITEM.TOTAL_COSTS;

                var assignedWorkpack = workpackAssignedUnits.Where(x => x.Key == currentWORKPACKGuid)
                            .Select(e => (KeyValuePair<Guid, decimal>?)e).FirstOrDefault();

                decimal totalUnitsAssigned;
                if (assignedWorkpack != null)
                {
                    totalUnitsAssigned = ((KeyValuePair<Guid, decimal>)assignedWorkpack).Value;
                    workpackAssignedUnits.Remove(((KeyValuePair<Guid, decimal>)assignedWorkpack).Key);
                }
                else
                    totalUnitsAssigned = 0;

                if (processingType == DataPointsType.Remaining)
                    totalUnitsAssigned += reportableObject.TOTAL_EARNED_UNITS;

                decimal currentAssignmentRemainingUnits;
                if (processingType == DataPointsType.Planned || processingType == DataPointsType.Remaining)
                    currentAssignmentRemainingUnits = totalUnits;
                else 
                    currentAssignmentRemainingUnits = totalEarnedUnits;

                foreach (WORKPACK_ASSIGNMENT currentWORKPACK_ASSIGNMENT in currentWORKPACK_ASSIGNMENTS)
                {
                    if (currentAssignmentRemainingUnits == 0)
                        continue;

                    if (currentWORKPACK_ASSIGNMENT.LOW_VALUE <= totalUnitsAssigned + 1 && totalUnitsAssigned + 1 <= currentWORKPACK_ASSIGNMENT.HIGH_VALUE)
                    {
                        TASK currentAssignmentTASK = P6TASKS.FirstOrDefault(task => task.task_code == currentWORKPACK_ASSIGNMENT.P6_ACTIVITYID);
                        DateTime CurrentAssignmentStartDate;
                        if (processingType == DataPointsType.Planned)
                            CurrentAssignmentStartDate = (DateTime)currentAssignmentTASK.target_start_date;
                        else if (processingType == DataPointsType.Earned)
                            CurrentAssignmentStartDate = (DateTime)currentAssignmentTASK.act_start_date;
                        else
                            CurrentAssignmentStartDate = (DateTime)currentAssignmentTASK.early_start_date;

                        DateTime CurrentAssignmentEndDate;
                        if (processingType == DataPointsType.Planned)
                            CurrentAssignmentEndDate = (DateTime)currentAssignmentTASK.target_end_date;
                        else if(processingType == DataPointsType.Earned)
                        {
                            if (currentAssignmentTASK.act_end_date == null)
                                CurrentAssignmentEndDate = (DateTime)lastRecalcDate;
                            else
                                CurrentAssignmentEndDate = (DateTime)currentAssignmentTASK.act_end_date;
                        }
                        else
                            CurrentAssignmentEndDate = (DateTime)currentAssignmentTASK.early_end_date;

                        TimeSpan CurrentAssignmentWorkingPeriod = CurrentAssignmentEndDate - CurrentAssignmentStartDate;
                        //decimal currentWorkingDays = Convert.ToDecimal(CurrentAssignmentWorkingPeriod.TotalDays);
                        decimal CurrentAssignmentUnits;

                        if (currentAssignmentRemainingUnits < currentWORKPACK_ASSIGNMENT.HIGH_VALUE)
                            CurrentAssignmentUnits = currentAssignmentRemainingUnits;
                        else
                            CurrentAssignmentUnits = (currentWORKPACK_ASSIGNMENT.HIGH_VALUE - currentWORKPACK_ASSIGNMENT.LOW_VALUE) + 1;

                        decimal CurrentAssignmentCosts = CurrentAssignmentUnits * reportableObject.BASELINE_ITEMJoinRATE.ITEMRATE;

                        nonCumulativeP6DataPoints.AddRange(ISupportProgressReportingExtensions.DataPointsGenerator(SummaryObject, CurrentAssignmentWorkingPeriod, CurrentAssignmentUnits, CurrentAssignmentCosts, CurrentAssignmentStartDate, currentBASELINE_ITEM.BASELINE_ITEM.GUID_ORIGINAL, this.CurrencyConversion, null, null, null));
                        currentAssignmentRemainingUnits -= CurrentAssignmentUnits;
                        totalUnitsAssigned += CurrentAssignmentUnits;
                    }
                }

                workpackAssignedUnits.Add(currentWORKPACKGuid, totalUnitsAssigned);
                return true;
            }
            else
                return false;
        }

        public override void BuildVariationDataPoints()
        {
            if (this.SummaryObject.VARIATIONS == null)
                return;

            ObservableCollection<VariationAdjustment> approvedVariation = new ObservableCollection<VariationAdjustment>();
            foreach (VARIATION VARIATION in this.SummaryObject.VARIATIONS)
            {
                DateTime? approvedDate = VARIATION.APPROVED;
                if (VARIATION.GUID_PROJECT == SummaryObject.LivePROGRESS.GUID_PROJECT && approvedDate != null && VARIATION.BASELINE1 != null && VARIATION.BASELINE != null)
                {
                    IEnumerable<BASELINE_ITEMProjection> contextBASELINE_ITEMS = this.SummaryObject.ReportableObjects.Select(x => x.BASELINE_ITEMJoinRATE);

                    foreach (VARIATION_ITEM VARIATION_ITEM in VARIATION.VARIATION_ITEM)
                    {
                        if (VARIATION_ITEM.ACTION != VariationAction.Add && VARIATION_ITEM.ACTION != VariationAction.Append)
                            continue;

                        var contextBASELINE_ITEM = contextBASELINE_ITEMS.FirstOrDefault(x => x.BASELINE_ITEM.GUID_ORIGINAL == VARIATION_ITEM.GUID_ORIBASEITEM);
                        if (contextBASELINE_ITEM != null)
                        {
                            approvedVariation.Add(new VariationAdjustment()
                            {
                                AdjustmentDate = (DateTime)approvedDate,
                                AdjustmentUnits = VARIATION_ITEM.VARIATION_UNITS,
                                AdjustmentRate = contextBASELINE_ITEM.ITEMRATE,
                                BaselineItemGuid = contextBASELINE_ITEM.BASELINE_ITEM.GUID_ORIGINAL
                            });
                        }
                    }
                }
            }

            foreach (ReportableObject ReportableObject in SummaryObject.ReportableObjects)
            {
                ReportableObject.NonCumulative_VariationAdjustments = new ObservableCollection<VariationAdjustment>(approvedVariation.Where(variation => variation.BaselineItemGuid == ReportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_ORIGINAL).ToList());
            }

            SummaryObject.NonCumulative_VariationAdjustments = new ObservableCollection<VariationAdjustment>(approvedVariation);
        }

        /// <summary>
        /// Calculates each baselineItem earned data point while populating aggregate non cumulative earned data points
        /// </summary>
        /// <returns>Non cumulative earned progress data points</returns>
        public override void BuildEarnedDataPoints()
        {
            Dictionary<Guid, decimal> workpackAssignedUnits = new Dictionary<Guid, decimal>();
            foreach (ReportableObject reportableObject in SummaryObject.ReportableObjects)
            {
                //Assign the report date for stats display
                reportableObject.ReportingDataDate = SummaryObject.ReportingDataDate;
                List<ProgressInfo> progressItemP6DataPoints;
                if (!string.IsNullOrEmpty(this.SummaryObject.LivePROGRESS.P6PROGRESS_NAME))
                    this.PROGRESS_TASKS = GetP6ScheduleTasks(SummaryObject.LivePROGRESS.P6PROGRESS_NAME, out this.PROGRESS_PROJECT);

                bool isProgressDataDateMatch = (this.PROGRESS_PROJECT != null && this.PROGRESS_PROJECT.last_recalc_date != null && ((DateTime)this.PROGRESS_PROJECT.last_recalc_date).Date == SummaryObject.LivePROGRESS.DATA_DATE.Date);
                
                if (isProgressDataDateMatch && TryBuildP6DataPoints(this.PROGRESS_PROJECT, this.PROGRESS_TASKS, reportableObject, DataPointsType.Earned, WorkpackAssignmentLoadType.Modified, workpackAssignedUnits, out progressItemP6DataPoints))
                {
                    reportableObject.NonCumulative_EarnedDataPoints = new ObservableCollection<ProgressInfo>(progressItemP6DataPoints);
                    reportableObject.isDataPointsGeneratedFromP6 = true;
                }

                else
                {
                    IQueryable<ProgressInfo> progressItemEarnedDataPoints = reportableObject.PROGRESS_ITEMSUpToCurrentDate.Select(x => new ProgressInfo()
                    {
                        BudgetedUnits = reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.TOTAL_HOURS,
                        BudgetedCosts = reportableObject.BASELINE_ITEMJoinRATE.TOTAL_COSTS * this.CurrencyConversion,
                        BaselineItemGuid = reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_ORIGINAL,
                        Units = x.EARNED_UNITS,
                        Costs = x.EARNED_UNITS * reportableObject.BASELINE_ITEMJoinRATE.ITEMRATE * this.CurrencyConversion,
                        ProgressDate = x.EARNED_DATE,
                    }).AsQueryable();

                    reportableObject.NonCumulative_EarnedDataPoints = new ObservableCollection<ProgressInfo>(progressItemEarnedDataPoints);
                }
            }

            SummaryObject.NonCumulative_EarnedDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(progressItem => progressItem.NonCumulative_EarnedDataPoints));
        }

        public override void BuildRemainingDataPoints()
        {
            BuildProductivity();
            //Establishing aligned week ending dates
            List<DateTime> alignedWeekEndingDates = ISupportProgressReportingExtensions.GenerateAlignedDatesCollection(SummaryObject.FirstAlignedDataDate, SummaryObject.FirstAlignedDataDate.AddYears(1), SummaryObject.IntervalPeriod);

            IQueryable<ProgressInfo> progressItemsEarnedDataPointsBeforeDataDate = SummaryObject.ReportableObjects.SelectMany(progressItem => progressItem.NonCumulative_EarnedDataPoints.Where(dataPoint => dataPoint.ProgressDate.Date <= SummaryObject.ReportingDataDate.Date)).AsQueryable();
            decimal totalEarnedUnits = progressItemsEarnedDataPointsBeforeDataDate.Sum(dataPoint => dataPoint.Units);
            if (totalEarnedUnits == 0)
                return;

            List<Period> exceptionPeriods = new List<Period>();
            exceptionPeriods.AddRange(ISupportProgressReportingExtensions.NonWorkingPeriods);
            Dictionary<Guid, decimal> workpackAssignedUnits = new Dictionary<Guid, decimal>();
            foreach (ReportableObject reportableObject in SummaryObject.ReportableObjects)
            {
                //when remaining units is more than 0 continue calculation
                if (reportableObject.RemainingUnitsAfterDataDate > 0 && reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.WORKPACK != null)
                {
                    List<ProgressInfo> progressItemP6DataPoints;
                    if (reportableObject.isDataPointsGeneratedFromP6 && TryBuildP6DataPoints(this.PROGRESS_PROJECT, this.PROGRESS_TASKS, reportableObject, DataPointsType.Remaining, WorkpackAssignmentLoadType.Modified, workpackAssignedUnits, out progressItemP6DataPoints))
                        reportableObject.NonCumulative_RemainingPlannedDataPoints = new ObservableCollection<ProgressInfo>(progressItemP6DataPoints);
                    else
                    {
                        DateTime startDateToUse;
                        DateTime firstAlignedWeekEndingDataDate;
                        decimal firstPeriodProRate;

                        if (reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.WORKPACK.FORECASTSTARTDATE != null)
                            startDateToUse = (DateTime)reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.WORKPACK.FORECASTSTARTDATE;
                        else
                            startDateToUse = (DateTime)reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.WORKPACK.STARTDATE;

                        //when workpack dates are later than data date use workpack dates but have a prorate value ready for first period
                        if (startDateToUse > SummaryObject.LivePROGRESS.DATA_DATE)
                        {
                            firstAlignedWeekEndingDataDate = alignedWeekEndingDates.FirstOrDefault(dates => dates.Date >= startDateToUse);
                            firstPeriodProRate = Convert.ToDecimal((firstAlignedWeekEndingDataDate.AddSeconds(1) - startDateToUse).TotalDays / SummaryObject.IntervalPeriod.TotalDays);
                        }
                        else
                        {
                            firstAlignedWeekEndingDataDate = SummaryObject.LivePROGRESS.DATA_DATE.AddDays(SummaryObject.IntervalPeriod.Days);
                            firstPeriodProRate = 1;
                        }

                        decimal maxInefficiency = 0.5M;

                        decimal currentEfficiency = (reportableObject.ActualProductivity / reportableObject.BaselineProductivity);

                        reportableObject.NonCumulative_RemainingPlannedDataPoints = ISupportProgressReportingExtensions.RemainingDataPointsGenerator(SummaryObject, reportableObject, firstAlignedWeekEndingDataDate, exceptionPeriods, reportableObject.RemainingUnitsAfterDataDate, reportableObject.BaselineProductivity, this.CurrencyConversion, firstPeriodProRate);

                        //if there's a planned finish date based on baseline productivity, inflate periodic units/costs
                        DateTime? plannedLimitDate = (reportableObject.NonCumulative_RemainingPlannedDataPoints == null || reportableObject.NonCumulative_RemainingPlannedDataPoints.Count == 0) ? (DateTime?)null : reportableObject.NonCumulative_RemainingPlannedDataPoints.Last().ProgressDate;

                        if (currentEfficiency < maxInefficiency)
                            currentEfficiency = maxInefficiency;

                        decimal inflatedInefficientUnits = currentEfficiency > 0 ? (reportableObject.RemainingUnitsAfterDataDate / currentEfficiency) : reportableObject.RemainingUnitsAfterDataDate;

                        reportableObject.NonCumulative_RemainingCurrentDataPoints = ISupportProgressReportingExtensions.RemainingDataPointsGenerator(SummaryObject, reportableObject, firstAlignedWeekEndingDataDate, exceptionPeriods, inflatedInefficientUnits, reportableObject.ActualProductivity, this.CurrencyConversion, firstPeriodProRate, plannedLimitDate);
                    }
                }
            }

            //extract all data points out to be used as an overall summary
            SummaryObject.NonCumulative_RemainingPlannedDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(progressItem => progressItem.NonCumulative_RemainingPlannedDataPoints));
            SummaryObject.NonCumulative_RemainingCurrentDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(progressItem => progressItem.NonCumulative_RemainingCurrentDataPoints));
        }

        /// <summary>
        /// Populate all progress item productivity
        /// </summary>
        private void BuildProductivity()
        {
            //Establish exception periods
            List<Period> exceptionPeriods = new List<Period>();
            exceptionPeriods.AddRange(ISupportProgressReportingExtensions.NonWorkingPeriods);
            foreach (ReportableObject reportableItem in SummaryObject.ReportableObjects)
            {
                //when remaining units is more than 0 continue calculation
                if (reportableItem.RemainingUnitsAfterDataDate > 0)
                    BuildReportableObjectProductivity(reportableItem, exceptionPeriods);
            }
        }

        private void BuildReportableObjectProductivity(ReportableObject reportableObject, List<Period> exceptionPeriods)
        {
            //When productivity is below this threshold, escalate to workpack or project
            decimal minimumProductivityBeforeEscalating = 0.001M;

            if (reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.WORKPACK == null)
                return;

            //establish dates for productivity assessment
            DateTime workpackStart = (DateTime)reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.WORKPACK.STARTDATE;
            DateTime workpackEnd = (DateTime)reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.WORKPACK.ENDDATE;
            DateTime? workpackForecastStart = reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.WORKPACK.FORECASTSTARTDATE;
            DateTime? workpackForecastEnd = reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.WORKPACK.FORECASTENDDATE;

            DateTime assessmentStartDate;
            DateTime assessmentEndDate;

            if (workpackForecastStart != null)
                assessmentStartDate = (DateTime)workpackForecastStart;
            else
                assessmentStartDate = workpackStart;

            if (workpackForecastEnd != null)
                assessmentEndDate = (DateTime)workpackForecastEnd;
            else
                assessmentEndDate = workpackEnd;

            if (reportableObject.ReportingDataDate > assessmentStartDate)
                assessmentStartDate = reportableObject.ReportingDataDate;
            if (reportableObject.ReportingDataDate > assessmentEndDate)
                assessmentEndDate = reportableObject.ReportingDataDate;

            Period assessmentPeriod = new Period(assessmentStartDate.Date, assessmentEndDate.Date);

            //establish workpack productivity to be used when deliverable productivity is too low
            WORKPACK currentWORKPACK = reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.WORKPACK;

            //calculate deliverable productivity
            reportableObject.VariationProductivity = ISupportProgressReportingExtensions.CalculatePlannedProductivity(assessmentPeriod, exceptionPeriods, reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.TOTAL_HOURS);
            //progressItem.ProgressItem_CurrentProductivity = UnifiedCalculationMethods.CalculateProductivity(assessmentPeriod)

            decimal workpackVarProductivity = 0;
            if (reportableObject.VariationProductivity < minimumProductivityBeforeEscalating)
            {
                IEnumerable<BASELINE_ITEMProjection> WorkpackBASELINE_ITEMJoinRATES = SummaryObject.ReportableObjects.Where(x => x.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_WORKPACK == currentWORKPACK.GUID).Select(x => x.BASELINE_ITEMJoinRATE);

                //not checking for progressItemWorkpack null because all progress item should have workpacks assigned if the user 
                decimal totalWorkpackUnits = WorkpackBASELINE_ITEMJoinRATES.Sum(x => x.BASELINE_ITEM.TOTAL_HOURS);

                workpackVarProductivity = ISupportProgressReportingExtensions.CalculatePlannedProductivity(assessmentPeriod, exceptionPeriods, totalWorkpackUnits);
                if (workpackVarProductivity > 0)
                    reportableObject.VariationProductivity = workpackVarProductivity;
            }

            decimal workpackBaseProductivity = 0;
            //not checking for progressItemWorkpack null because all progress item should have workpacks assigned if the user 
            decimal totalWorkpackBudgetedUnits = (currentWORKPACK == null || currentWORKPACK.BASELINE_ITEM == null) ? 0 : currentWORKPACK.BASELINE_ITEM.Sum(pItem => pItem.ESTIMATED_HOURS);
            workpackBaseProductivity = ISupportProgressReportingExtensions.CalculatePlannedProductivity(assessmentPeriod, exceptionPeriods, reportableObject.RemainingUnitsAfterDataDate);

            if (reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.ESTIMATED_HOURS == 0)
                reportableObject.BaselineProductivity = workpackBaseProductivity;
            else
            {
                reportableObject.BaselineProductivity = ISupportProgressReportingExtensions.CalculatePlannedProductivity(assessmentPeriod, exceptionPeriods, reportableObject.RemainingUnitsAfterDataDate);
                //apply normalized productivity for unusually low calculated productivity
                if (reportableObject.BaselineProductivity < minimumProductivityBeforeEscalating)
                {
                    if (workpackBaseProductivity > 0)
                        reportableObject.BaselineProductivity = workpackBaseProductivity;
                }
            }

            List<ProgressInfo> deliverablePlannedDataPointsOnOrBeforeDataDate = reportableObject.NonCumulative_PlannedDataPoints.Where(dataPoint => dataPoint.ProgressDate <= SummaryObject.LivePROGRESS.DATA_DATE).ToList();
            decimal currentEfficiency = 0;
            if (reportableObject.TOTAL_EARNED_UNITS != 0 && deliverablePlannedDataPointsOnOrBeforeDataDate.Count() > 0)
            {
                decimal deliverablePlannedUnitsOnOrBeforeDataDate = deliverablePlannedDataPointsOnOrBeforeDataDate.Sum(dataPoint => dataPoint.Units);
                if (deliverablePlannedUnitsOnOrBeforeDataDate > 0)
                    currentEfficiency = reportableObject.TOTAL_EARNED_UNITS / deliverablePlannedUnitsOnOrBeforeDataDate;

                reportableObject.ActualProductivity = reportableObject.BaselineProductivity * currentEfficiency;
            }
            else
                reportableObject.ActualProductivity = reportableObject.BaselineProductivity; //assume productivity of 1 because there are no units to measure against

            if (reportableObject.ActualProductivity < minimumProductivityBeforeEscalating)
                reportableObject.ActualProductivity = reportableObject.BaselineProductivity;
        }

        /// <summary>
        /// Calculates each baselineItem burned/actual data point while populating aggregate non cumulative burned/actual data points
        /// </summary>
        /// <returns>Non cumulative earned progress data points</returns>
        public override void BuildBurnedDataPoints()
        {
            ObservableCollection<ProgressInfo> nonCumulative_BurnedDataPoints = new ObservableCollection<ProgressInfo>();
            DateTime firstAlignedDataDate = SummaryObject.FirstAlignedDataDate;
            TimeSpan progressInterval = SummaryObject.IntervalPeriod;
            DateTime loopDate = firstAlignedDataDate;

            IEnumerable<WORKPACK> WORKPACKS = SummaryObject.LiveBASELINE.PROJECT.WORKPACK;
            IEnumerable<string> qualifiedWorkpack = WORKPACKS == null ? new List<string>() : WORKPACKS.Select(x => x.INTERNAL_NAME1);
            var PrimeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            var jobTransactions = from JOBTRANS in PrimeroUnitOfWork.JOB_TRANSACTIONS
                                  join JOBCOST_HDR2 in PrimeroUnitOfWork.JOBCOST_HDR
                                  on JOBTRANS.MASTER_JOBNO equals JOBCOST_HDR2.JOBNO
                                  join JOBCOST_HDR1 in PrimeroUnitOfWork.JOBCOST_HDR
                                  on JOBTRANS.JOBNO equals JOBCOST_HDR1.JOBNO
                                  join JOBCOST_RESOURCE in PrimeroUnitOfWork.JOBCOST_RESOURCE
                                  on JOBTRANS.STAFFNO equals JOBCOST_RESOURCE.SEQNO
                                  where JOBCOST_HDR2.JOBCODE == SummaryObject.LiveBASELINE.PROJECT.NUMBER && JOBTRANS.TRANSTYPE == "T" && JOBTRANS.LINE_STATUS != "X"
                                  select new { JOBCOST_HDR1.JOBCODE, JOBTRANS.QUANTITY, JOBTRANS.LINETOTAL, JOBTRANS.LINECOST, JOBTRANS.TRANSDATE, JOBCOST_RESOURCE.RESOURCENAME };

            var jobTransactionsList = jobTransactions.ToList();
            if (jobTransactionsList.Count == 0)
                return;

            List<DateTime> alignedDataDates = ISupportProgressReportingExtensions.GenerateAlignedDatesCollection(firstAlignedDataDate, jobTransactionsList.Max(x => x.TRANSDATE).Value, progressInterval);
            foreach (var jobTransaction in jobTransactionsList)
            {
                if (qualifiedWorkpack.Contains(jobTransaction.JOBCODE))
                    nonCumulative_BurnedDataPoints.Add(new ProgressInfo()
                    {
                        BudgetedUnits = 0,
                        BudgetedCosts = 0,
                        Units = (decimal)jobTransaction.QUANTITY,
                        Costs = (decimal)jobTransaction.LINETOTAL * this.CurrencyConversion,
                        Actuals = jobTransaction.LINECOST == null ? 0 : (decimal)jobTransaction.LINECOST,
                        ProgressDate = alignedDataDates.FirstOrDefault(dates => dates.Date >= jobTransaction.TRANSDATE),
                        BaselineItemGuid = Guid.Empty,
                        WorkpackName = jobTransaction.JOBCODE,
                        ResourceName = jobTransaction.RESOURCENAME,
                        Quantity = (decimal)jobTransaction.QUANTITY
                    });
            }

            SummaryObject.NonCumulative_BurnedDataPoints = nonCumulative_BurnedDataPoints;
        }

        public override void BuildActualDataPoints()
        {
            List<ProgressInfo> convertBurnedToActualDataPoints = new List<ProgressInfo>();
            SummaryObject.NonCumulative_BurnedDataPoints.ToList().ForEach(dataPoint => convertBurnedToActualDataPoints.Add(new ProgressInfo()
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

            SummaryObject.NonCumulative_ActualDataPoints = new ObservableCollection<ProgressInfo>(convertBurnedToActualDataPoints);
        }

        private IEnumerable<TASK> GetP6ScheduleTasks(string shortName, out P6Data.PROJECT P6Schedule)
        {
            if (shortName != null && shortName != string.Empty)
            {
                var PROJECTRepository = this.P6UnitOfWork.PROJECT;
                P6Schedule = PROJECTRepository.FirstOrDefault(x => x.proj_short_name == shortName);

                if (P6Schedule != null)
                {
                    return P6Schedule.TASK;
                }
            }
            else
                P6Schedule = null;

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
