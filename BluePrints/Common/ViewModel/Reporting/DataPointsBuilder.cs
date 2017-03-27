using BluePrints.Common.Projections;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BluePrints.Common.ViewModel.Reporting.PROJECTSummaryBuilder;
using static BluePrints.Data.BluePrintsEntities;

namespace BluePrints.Common.ViewModel.Reporting
{
    public class ProjectReportableDataPointsBuilder : IBuildDataPoints
    {
        private IEnumerable<TASK> originalBaselineP6Tasks { get; set; }
        private IEnumerable<TASK> modifiedBaselineP6Tasks { get; set; }
        private IEnumerable<TASK> progressP6Tasks { get; set; }
        private IEnumerable<WORKPACK> projectWORKPACKS { get; set; }
        private TimeSpan progressInterval { get; set; }
        private DateTime reportingDataDate { get; set; }
        private DateTime firstAlignedDataDate { get; set; }
        private decimal currencyConversion { get; set; }
        private IEnumerable<DateTime> alignedWeekEndingDates { get; set; }
        private IEnumerable<VARIATION_ITEMProjection> approvedVariation_Items { get; set; }
        private List<Period> exceptionPeriods { get; set; }

        IP6EntitiesUnitOfWork P6UnitOfWork { get; set; }
        public ProjectReportableDataPointsBuilder(TimeSpan progressInterval, DateTime reportingDataDate, DateTime firstAlignedDataDate, decimal currencyConversion, IEnumerable<VARIATION_ITEMProjection> approvedVariation_Items, IEnumerable<WORKPACK> WORKPACKS, IP6EntitiesUnitOfWork p6UnitOfWork = null, 
          string p6OriginalScheduleName = "", string p6ModifiedScheduleName = "", string p6ProgressScheduleName = "")
        {
            if (p6UnitOfWork == null)
                this.P6UnitOfWork = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            else
                this.P6UnitOfWork = p6UnitOfWork;

            originalBaselineP6Tasks = GetP6ScheduleTasks(p6OriginalScheduleName);
            modifiedBaselineP6Tasks = GetP6ScheduleTasks(p6ModifiedScheduleName);
            progressP6Tasks = GetP6ScheduleTasks(p6ProgressScheduleName);
            projectWORKPACKS = WORKPACKS;

            this.progressInterval = progressInterval;
            this.reportingDataDate = reportingDataDate;
            this.firstAlignedDataDate = firstAlignedDataDate;
            this.currencyConversion = currencyConversion;
            this.approvedVariation_Items = approvedVariation_Items;
            alignedWeekEndingDates = ISupportProgressReportingExtensions.GenerateAlignedDatesCollection(this.firstAlignedDataDate, this.firstAlignedDataDate.AddYears(Int16.Parse(CommonResources.DataPointsBuilder_MaxProjectDuration)), this.progressInterval);

            exceptionPeriods = new List<Period>();
            exceptionPeriods.AddRange(ISupportProgressReportingExtensions.NonWorkingPeriods);
        }

        public void BuildVariationAdjustments(ReportableObject reportableObject)
        {
            IEnumerable<VariationAdjustment> applicableVariation_Adjustment = this.approvedVariation_Items.Where(x => x.APPROVED != null &&
            x.VARIATION_ITEM.GUID_ORIBASEITEM == reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_ORIGINAL && (x.VARIATION_ITEM.ACTION == VariationAction.Add || x.VARIATION_ITEM.ACTION == VariationAction.Append)).Select(x => new VariationAdjustment() { AdjustmentDate = (DateTime)x.APPROVED, AdjustmentUnits = x.VARIATION_ITEM.VARIATION_UNITS, AdjustmentRate = reportableObject.BASELINE_ITEMJoinRATE.ITEMRATE, BaselineItemGuid = reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_ORIGINAL });

            reportableObject.NonCumulative_VariationAdjustments = new ObservableCollection<VariationAdjustment>(applicableVariation_Adjustment);
            reportableObject.Cumulative_VariationAdjustments = ISupportProgressReportingExtensions.PopulateCumulativeVariationAdjustments(reportableObject.NonCumulative_VariationAdjustments, this.firstAlignedDataDate, this.progressInterval);
        }

        public void BuildPlannedDataPoints(ReportableObject reportableObject, AssignmentLoadType assignmentLoadType, 
          IEnumerable<StoredProcedure_DeliverablesDataPoints> dbDataPointsCollection = null)
        {
            reportableObject.ReportingDataDate = this.reportingDataDate;

            BASELINE_ITEMProjection currentBASELINE_ITEM = reportableObject.BASELINE_ITEMJoinRATE;
            WORKPACK currentWORKPACK = currentBASELINE_ITEM.BASELINE_ITEM.WORKPACK;
            if (currentWORKPACK == null)
                return;

            //Use original P6 tasks if modified is null else follow assignment type
            IEnumerable<TASK> p6BaselineTasks = assignmentLoadType == AssignmentLoadType.Original ? this.originalBaselineP6Tasks : this.modifiedBaselineP6Tasks == null ? this.originalBaselineP6Tasks : this.modifiedBaselineP6Tasks;

            List<ProgressInfo> progressItemP6DataPoints;

            string s;
            if (reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.INTERNAL_NUM == "P027-22200-MOD-ME-843")
                s = string.Empty;


            if (TryBuildP6DataPoints(p6BaselineTasks, reportableObject, DataPointsType.Planned, assignmentLoadType, out progressItemP6DataPoints))
            {
                reportableObject.isPlannedDataPointsFromP6 = true;
                if (assignmentLoadType == AssignmentLoadType.Original)
                    reportableObject.NonCumulative_OriginalDataPoints = new ObservableCollection<ProgressInfo>(progressItemP6DataPoints);
                else
                    reportableObject.NonCumulative_PlannedDataPoints = new ObservableCollection<ProgressInfo>(progressItemP6DataPoints);
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
                if (assignmentLoadType == AssignmentLoadType.Original) //if it's generating from original baseline ignore variation
                {
                    if (dbDataPointsCollection != null)
                    {
                        IEnumerable<StoredProcedure_DeliverablesDataPoints> currentReportableDbDataPoints =
                        dbDataPointsCollection.Where(x => x.GUID_ORIGINAL == reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_ORIGINAL)
                        .OrderBy(x => x.UniversalPeriodStartDate);

                        reportableObject.NonCumulative_OriginalDataPoints =
                            new ObservableCollection<ProgressInfo>(ConvertDeliverablesDataPointToProgressInfo(currentReportableDbDataPoints, reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.TOTAL_HOURS, reportableObject.BASELINE_ITEMJoinRATE.TOTAL_COSTS));
                    }
                    else
                    {
                        TimeSpan workingBaseTimeSpan = (DateTime)currentWORKPACK.ENDDATE - (DateTime)currentWORKPACK.STARTDATE;
                        plannedDataPoints = ISupportProgressReportingExtensions.DataPointsGenerator(this.progressInterval, this.firstAlignedDataDate, workingBaseTimeSpan, BaselineItemBaseUnits, BaselineItemBaseCosts, (DateTime)currentWORKPACK.STARTDATE, currentBASELINE_ITEM.BASELINE_ITEM.GUID_ORIGINAL, this.currencyConversion, workpackSuspensionPeriod, BaselineItemTotalUnits, BaselineItemTotalCosts);
                        reportableObject.NonCumulative_OriginalDataPoints = new ObservableCollection<ProgressInfo>(plannedDataPoints);
                    }
                }
                else
                {
                    if (dbDataPointsCollection != null)
                    {
                        IEnumerable<StoredProcedure_DeliverablesDataPoints> currentDeliverableDataPoints = dbDataPointsCollection.Where(x => x.GUID_ORIGINAL == reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_ORIGINAL).OrderBy(x => x.UniversalPeriodStartDate);
                        reportableObject.NonCumulative_PlannedDataPoints = new ObservableCollection<ProgressInfo>(ConvertDeliverablesDataPointToProgressInfo(currentDeliverableDataPoints, reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.TOTAL_HOURS, reportableObject.BASELINE_ITEMJoinRATE.TOTAL_COSTS));
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
                        plannedDataPoints = ISupportProgressReportingExtensions.DataPointsGenerator(this.progressInterval, this.firstAlignedDataDate, workingModifiedTimeSpan, BaselineItemBaseUnits, BaselineItemBaseCosts, (DateTime)currentWORKPACK.STARTDATE, currentBASELINE_ITEM.BASELINE_ITEM.GUID_ORIGINAL, this.currencyConversion, workpackSuspensionPeriod, null, null, reportableObject.Cumulative_VariationAdjustments);

                        //Used to show normalized variation
                        //plannedDataPoints = DataPointsGenerator(WorkingPeriod, progressInterval, BaselineItemTotalUnits, BaselineItemTotalCosts, this.CurrencyConversion, baselineItem.WORKPACK.STARTDATE, firstAlignedDataDate, baselineItem.GUID_ORIGINAL);
                        reportableObject.NonCumulative_PlannedDataPoints = new ObservableCollection<ProgressInfo>(plannedDataPoints);
                    }
                }
            }
        }

        public void BuildEarnedDataPoints(ReportableObject reportableObject)
        {
            reportableObject.ReportingDataDate = this.reportingDataDate;
            IQueryable<ProgressInfo> progressItemEarnedDataPoints = reportableObject.PROGRESS_ITEMSUpToCurrentDate.Select(x => new ProgressInfo()
            {
                BudgetedUnits = reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.TOTAL_HOURS,
                BudgetedCosts = reportableObject.BASELINE_ITEMJoinRATE.TOTAL_COSTS * this.currencyConversion,
                BaselineItemGuid = reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_ORIGINAL,
                Units = x.EARNED_UNITS,
                Costs = x.EARNED_UNITS * reportableObject.BASELINE_ITEMJoinRATE.ITEMRATE * this.currencyConversion,
                ProgressDate = x.EARNED_DATE,
            }).AsQueryable();
            reportableObject.NonCumulative_EarnedDataPoints = new ObservableCollection<ProgressInfo>(progressItemEarnedDataPoints);
        }

        public void BuildRemainingDataPoints(ReportableObject reportableObject)
        {
            //when remaining units is more than 0 continue calculation
            if (reportableObject.RemainingUnitsAfterDataDate > 0 && reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.WORKPACK != null)
            {
                string s;
                if (reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.INTERNAL_NUM == "P027-22200-MOD-ME-843")
                    s = string.Empty;

                List<ProgressInfo> progressItemP6DataPoints;
                if (TryBuildP6DataPoints(this.progressP6Tasks, reportableObject, DataPointsType.Remaining, AssignmentLoadType.Modified, out progressItemP6DataPoints))
                {
                    reportableObject.isRemainingDataPointsFromP6 = true;
                    reportableObject.NonCumulative_RemainingPlannedDataPoints = new ObservableCollection<ProgressInfo>(progressItemP6DataPoints);
                }
                else
                {
                    DateTime firstAlignedWeekEndingDataDate;

                    decimal firstPeriodProRate;
                    WORKPACK lookUpWORKPACK = projectWORKPACKS.FirstOrDefault(x => x.GUID == reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_WORKPACK);

                    if (lookUpWORKPACK == null)
                        return;

                    DateTime startDateToUse;
                    if (lookUpWORKPACK.FORECASTSTARTDATE != null)
                        startDateToUse = (DateTime)lookUpWORKPACK.FORECASTSTARTDATE;
                    else if (lookUpWORKPACK.STARTDATE != null)
                        startDateToUse = (DateTime)lookUpWORKPACK.STARTDATE;
                    else
                        return;

                    //use end date to limit charting beyond end date
                    DateTime endDateToUse = reportableObject.NonCumulative_PlannedDataPoints.Max(x => x.ProgressDate);
                    endDateToUse = endDateToUse.AddDays(progressInterval.Days);
                    //when workpack dates are later than data date use workpack dates but have a prorate value ready for first period
                    if (startDateToUse > this.reportingDataDate)
                    {
                        firstAlignedWeekEndingDataDate = alignedWeekEndingDates.FirstOrDefault(dates => dates.Date >= startDateToUse);
                        firstPeriodProRate = Convert.ToDecimal((firstAlignedWeekEndingDataDate - startDateToUse).TotalDays / progressInterval.TotalDays);
                    }
                    else
                    {
                        firstAlignedWeekEndingDataDate = this.reportingDataDate.AddDays(progressInterval.Days);
                        firstPeriodProRate = 1;
                    }

                    reportableObject.NonCumulative_RemainingPlannedDataPoints = ISupportProgressReportingExtensions.RemainingDataPointsGenerator(progressInterval, reportableObject, firstAlignedWeekEndingDataDate, exceptionPeriods, reportableObject.RemainingUnitsAfterDataDate, 1, this.currencyConversion, firstPeriodProRate, endDateToUse);
                }
            }
        }

        public void BuildCumulativeSummary(ReportableObject reportableObject)
        {
            ISupportProgressReportingExtensions.GenerateCumulativeSummaryDataPoints(reportableObject, this.firstAlignedDataDate, this.progressInterval);
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
        private bool TryBuildP6DataPoints(IEnumerable<TASK> P6TASKS, ReportableObject reportableObject, DataPointsType processingType, AssignmentLoadType assignmentLoadType, out List<ProgressInfo> nonCumulativeP6DataPoints)
        {
            nonCumulativeP6DataPoints = new List<ProgressInfo>();
            if (reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_WORKPACK == null)
                return false;

            IEnumerable<WORKPACK_ASSIGNMENT> currentWORKPACK_ASSIGNMENTS;
            Guid currentWORKPACKGuid = (Guid)reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_WORKPACK;

            if (assignmentLoadType == AssignmentLoadType.Modified && reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.WORKPACK.WORKPACK_ASSIGNMENT.Any(assignment => assignment.ISMODIFIEDBASELINE == true))
                currentWORKPACK_ASSIGNMENTS = reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.WORKPACK.WORKPACK_ASSIGNMENT.Where(assignment => assignment.ISMODIFIEDBASELINE == true);
            else
                currentWORKPACK_ASSIGNMENTS = reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.WORKPACK.WORKPACK_ASSIGNMENT.Where(assignment => assignment.ISMODIFIEDBASELINE == false);

            if (currentWORKPACK_ASSIGNMENTS != null && P6TASKS != null && currentWORKPACK_ASSIGNMENTS.Count() != 0 && P6TASKS.Count() != 0)
            {
                currentWORKPACK_ASSIGNMENTS = currentWORKPACK_ASSIGNMENTS.OrderBy(x => x.LOW_VALUE);
                BASELINE_ITEMProjection currentBASELINE_ITEM = reportableObject.BASELINE_ITEMJoinRATE;
                decimal totalUnits = currentBASELINE_ITEM.BASELINE_ITEM.TOTAL_HOURS;
                decimal totalCosts = currentBASELINE_ITEM.TOTAL_COSTS;
                decimal reportableAssinmentStartUnitForWorkpackAssignmentPairing = reportableObject.WorkpackAssignmentStartUnit;

                decimal currentAssignmentRemainingUnits;
                //because the earned units portion is generated independent of P6 tasks, we are only interested in what happens after earned units
                if (processingType == DataPointsType.Remaining)
                {
                    reportableAssinmentStartUnitForWorkpackAssignmentPairing += reportableObject.TOTAL_EARNED_UNITS;
                    currentAssignmentRemainingUnits = totalUnits - reportableObject.TOTAL_EARNED_UNITS;
                }
                else
                    currentAssignmentRemainingUnits = totalUnits;

                foreach (WORKPACK_ASSIGNMENT currentWORKPACK_ASSIGNMENT in currentWORKPACK_ASSIGNMENTS)
                {
                    if (currentAssignmentRemainingUnits == 0)
                        break;

                    decimal compareUnitsAssigned = Math.Round(reportableAssinmentStartUnitForWorkpackAssignmentPairing, 0);
                    if (currentWORKPACK_ASSIGNMENT.LOW_VALUE <= compareUnitsAssigned && compareUnitsAssigned <= currentWORKPACK_ASSIGNMENT.HIGH_VALUE)
                    {
                        TASK currentAssignmentTASK = P6TASKS.FirstOrDefault(task => task.task_code == currentWORKPACK_ASSIGNMENT.P6_ACTIVITYID);
                        DateTime CurrentAssignmentStartDate;
                        if (processingType == DataPointsType.Planned)
                            CurrentAssignmentStartDate = (DateTime)currentAssignmentTASK.target_start_date;
                        else
                        {
                            if (currentAssignmentTASK.early_start_date == null)
                                return false;

                            CurrentAssignmentStartDate = (DateTime)currentAssignmentTASK.early_start_date;
                        }

                        DateTime CurrentAssignmentEndDate;
                        if (processingType == DataPointsType.Planned)
                            CurrentAssignmentEndDate = (DateTime)currentAssignmentTASK.target_end_date;
                        else
                            CurrentAssignmentEndDate = (DateTime)currentAssignmentTASK.early_end_date;

                        TimeSpan CurrentAssignmentWorkingPeriod = CurrentAssignmentEndDate - CurrentAssignmentStartDate;
                        decimal CurrentAssignmentUnits;

                        if (currentAssignmentRemainingUnits < currentWORKPACK_ASSIGNMENT.HIGH_VALUE)
                            CurrentAssignmentUnits = currentAssignmentRemainingUnits;
                        else
                            CurrentAssignmentUnits = (currentWORKPACK_ASSIGNMENT.HIGH_VALUE - currentWORKPACK_ASSIGNMENT.LOW_VALUE) + 1;

                        decimal CurrentAssignmentCosts = CurrentAssignmentUnits * reportableObject.BASELINE_ITEMJoinRATE.ITEMRATE;

                        List<ProgressInfo> p6ProgressInfo = ISupportProgressReportingExtensions.DataPointsGenerator(this.progressInterval, this.firstAlignedDataDate, CurrentAssignmentWorkingPeriod, CurrentAssignmentUnits, CurrentAssignmentCosts, CurrentAssignmentStartDate, currentBASELINE_ITEM.BASELINE_ITEM.GUID_ORIGINAL, this.currencyConversion, null, null, null);
                        nonCumulativeP6DataPoints.AddRange(p6ProgressInfo);
                        currentAssignmentRemainingUnits -= CurrentAssignmentUnits;
                        reportableAssinmentStartUnitForWorkpackAssignmentPairing += CurrentAssignmentUnits;
                    }
                }

                return true;
            }
            else
                return false;
        }

        #region Helpers
        private IEnumerable<TASK> GetP6ScheduleTasks(string shortName)
        {
            if (shortName != null && shortName != string.Empty)
            {
                var PROJECTRepository = this.P6UnitOfWork.PROJECT;
                P6Data.PROJECT P6Schedule = PROJECTRepository.FirstOrDefault(x => x.proj_short_name == shortName);

                if (P6Schedule != null)
                {
                    return P6Schedule.TASK;
                }
            }

            return null;
        }

        private IEnumerable<ProgressInfo> ConvertDeliverablesDataPointToProgressInfo(IEnumerable<StoredProcedure_DeliverablesDataPoints> deliverablesDataPoints, decimal BudgetedUnits, decimal BudgetedCosts)
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
        #endregion
    }

    public interface IBuildDataPoints
    {
        void BuildVariationAdjustments(ReportableObject reportableObject);
        void BuildPlannedDataPoints(ReportableObject reportableObject, AssignmentLoadType assignmentLoadType,
          IEnumerable<StoredProcedure_DeliverablesDataPoints> dbDataPointsCollection = null);
        void BuildEarnedDataPoints(ReportableObject reportableObject);
        void BuildRemainingDataPoints(ReportableObject reportableObject);
        void BuildCumulativeSummary(ReportableObject reportableObject);
    }
}
