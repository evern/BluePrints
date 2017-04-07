using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Data;
using BluePrints.Data.Helpers;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BluePrints.Common.ViewModel.Reporting.FullSummarizer;
using static BluePrints.Data.BluePrintsEntities;

namespace BluePrints.Common.ViewModel.Reporting
{
    public class FullStatsBuilder : PartialStatsBuilder
    {
        readonly string ProjectNumber;
        readonly IPrimeroEntitiesUnitOfWork PrimeroUOW;

        public FullStatsBuilder(Data.PROJECT PROJECT, BASELINE liveBASELINE, string p6ProgressName, DateTime firstAlignedDataDate, TimeSpan reportInterval, IEnumerable<WORKPACK> WORKPACKS, IEnumerable<WORKPACK_ASSIGNMENT> WORKPACK_ASSIGNMENTS, IP6EntitiesUnitOfWork p6UOW = null, IPrimeroEntitiesUnitOfWork primeroUOW = null)
            : base(PROJECT, liveBASELINE, p6ProgressName, reportInterval, firstAlignedDataDate, WORKPACKS, WORKPACK_ASSIGNMENTS, p6UOW)
        {
            ProjectNumber = PROJECT.NUMBER;
            PrimeroUOW = primeroUOW == null ? PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork() : PrimeroUOW;
        }

        public FullStatsBuilder(Data.PROJECT PROJECT, BASELINE liveBASELINE, PROGRESS LivePROGRESS, IEnumerable<WORKPACK> WORKPACKS, IEnumerable<WORKPACK_ASSIGNMENT> WORKPACK_ASSIGNMENTS, IP6EntitiesUnitOfWork p6UOW = null, IPrimeroEntitiesUnitOfWork primeroUOW = null)
            : base(PROJECT, liveBASELINE, LivePROGRESS, WORKPACKS, WORKPACK_ASSIGNMENTS, p6UOW)
        {
            ProjectNumber = PROJECT.NUMBER;
            PrimeroUOW = primeroUOW == null ? PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork() : primeroUOW;
        }

        public void BuildExoDataPoints(ProjectSummaryStats summaryObject)
        {
            ProjectSummaryStats projectSummaryStats = summaryObject as ProjectSummaryStats;
            if (projectSummaryStats == null)
                return;

            ObservableCollection<ExoDataPoint> burnedDataPoints = new ObservableCollection<ExoDataPoint>();
            ObservableCollection<ExoDataPoint> actualDataPoints = new ObservableCollection<ExoDataPoint>();

            DateTime loopDate = FirstAlignedDataDate;

            IEnumerable<WORKPACK> workpacks = projectWORKPACKS;
            string projectNumber = ProjectNumber;

            IEnumerable<string> qualifiedWorkpacks = workpacks == null ? new List<string>() : workpacks.Select(x => x.INTERNAL_NAME1);
            var PrimeroUnitOfWork = PrimeroUOW;
            var jobTransactions = from JOBTRANS in PrimeroUnitOfWork.JOB_TRANSACTIONS
                                  join JOBCOST_HDR2 in PrimeroUnitOfWork.JOBCOST_HDR
                                  on JOBTRANS.MASTER_JOBNO equals JOBCOST_HDR2.JOBNO
                                  join JOBCOST_HDR1 in PrimeroUnitOfWork.JOBCOST_HDR
                                  on JOBTRANS.JOBNO equals JOBCOST_HDR1.JOBNO
                                  join JOBCOST_RESOURCE in PrimeroUnitOfWork.JOBCOST_RESOURCE
                                  on JOBTRANS.STAFFNO equals JOBCOST_RESOURCE.SEQNO
                                  where JOBCOST_HDR2.JOBCODE == projectNumber && JOBTRANS.TRANSTYPE == "T" && JOBTRANS.LINE_STATUS != "X"
                                  select new { JOBCOST_HDR1.JOBCODE, JOBTRANS.QUANTITY, JOBTRANS.LINETOTAL, JOBTRANS.LINECOST, JOBTRANS.TRANSDATE, JOBCOST_RESOURCE.RESOURCENAME };

            var exoWorkpacks = from JOBCOST_HDR in PrimeroUnitOfWork.JOBCOST_HDR
                               where JOBCOST_HDR.JOBCODE.Contains(projectNumber)
                               select new { JOBCOST_HDR.TITLE, JOBCOST_HDR.JOBCODE };

            var exoWorkpacksList = exoWorkpacks.ToList();
            foreach (WORKPACK workpack in workpacks)
            {
                var exoWorkpack = exoWorkpacksList.FirstOrDefault(x => x.JOBCODE == workpack.INTERNAL_NAME1 || x.JOBCODE == workpack.INTERNAL_NAME2);
                if (exoWorkpack == null)
                {
                    projectSummaryStats.AddMissingExoWorkpack(workpack);
                }
            }

            var jobTransactionsList = jobTransactions.ToList();
            if (jobTransactionsList.Count == 0)
                return;

            List<DateTime> alignedDataDates = ChronologicalHelpers.GenerateAlignedDatesCollection(FirstAlignedDataDate, jobTransactionsList.Max(x => x.TRANSDATE).Value, ReportingInterval);
            foreach (var jobTransaction in jobTransactionsList)
            {
                if (qualifiedWorkpacks.Contains(jobTransaction.JOBCODE))
                {
                    ExoDataPoint burnedDataPoint = new ExoDataPoint();
                    burnedDataPoint.BudgetedUnits = 0;
                    burnedDataPoint.BudgetedCosts = 0;
                    burnedDataPoint.Units = (decimal)jobTransaction.QUANTITY;
                    burnedDataPoint.Costs = (decimal)jobTransaction.LINETOTAL * this.CurrencyConversion;
                    burnedDataPoint.ProgressDate = alignedDataDates.FirstOrDefault(dates => dates.Date >= jobTransaction.TRANSDATE);
                    burnedDataPoint.WorkpackName = jobTransaction.JOBCODE;
                    burnedDataPoint.ResourceName = jobTransaction.RESOURCENAME;
                    burnedDataPoint.Quantity = (decimal)jobTransaction.QUANTITY;

                    burnedDataPoints.Add(burnedDataPoint);

                    ExoDataPoint actualDataPoint = new ExoDataPoint();
                    DataUtils.ShallowCopy(actualDataPoint, burnedDataPoint);
                    actualDataPoint.Costs = (decimal)jobTransaction.LINETOTAL * this.CurrencyConversion;
                    actualDataPoints.Add(actualDataPoint);
                }
            }

            projectSummaryStats.Burned = new Stats(summaryObject);
            projectSummaryStats.Actual = new Stats(summaryObject);

            projectSummaryStats.Burned.SetData(burnedDataPoints);
            projectSummaryStats.Actual.SetData(actualDataPoints);
            LoadingScreenManager.Progress();
        }
    }

    public class PartialStatsBuilder
    {
        private IEnumerable<TASK> originalBaselineP6Tasks { get; set; }
        private IEnumerable<TASK> modifiedBaselineP6Tasks { get; set; }
        private IEnumerable<TASK> progressP6Tasks { get; set; }
        protected IEnumerable<WORKPACK> projectWORKPACKS { get; set; }
        private IEnumerable<WORKPACK_ASSIGNMENT> projectWORKPACK_ASSIGNMENTS { get; set; }
        public TimeSpan ReportingInterval { get; private set; }
        public DateTime FirstAlignedDataDate { get; private set; }
        protected decimal CurrencyConversion { get; private set; }
        private IEnumerable<DateTime> alignedWeekEndingDates { get; set; }
        private List<Period> exceptionPeriods { get; set; }

        IP6EntitiesUnitOfWork P6UnitOfWork { get; set; }
        public PartialStatsBuilder(Data.PROJECT PROJECT, BASELINE LiveBASELINE, PROGRESS LivePROGRESS, IEnumerable<WORKPACK> WORKPACKS, IEnumerable<WORKPACK_ASSIGNMENT> WORKPACK_ASSIGNMENTS, IP6EntitiesUnitOfWork p6UOW = null)
            : this(PROJECT, LiveBASELINE, WORKPACKS, WORKPACK_ASSIGNMENTS, p6UOW)
        {
            this.progressP6Tasks = GetP6ScheduleTasks(LivePROGRESS.P6PROGRESS_NAME);

            this.ReportingInterval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(LivePROGRESS);
            this.FirstAlignedDataDate = ChronologicalHelpers.GenerateFirstAlignedDataDate(LivePROGRESS);
            this.alignedWeekEndingDates = ChronologicalHelpers.GenerateAlignedDatesCollection(this.FirstAlignedDataDate, this.FirstAlignedDataDate.AddYears(Int16.Parse(CommonResources.DataPointsBuilder_MaxProjectDuration)), this.ReportingInterval);
        }

        public PartialStatsBuilder(Data.PROJECT PROJECT, BASELINE LiveBASELINE, string p6ProgressName, TimeSpan progressInterval, DateTime firstAlignedDataDate, IEnumerable<WORKPACK> WORKPACKS, IEnumerable<WORKPACK_ASSIGNMENT> WORKPACK_ASSIGNMENTS, IP6EntitiesUnitOfWork p6UOW = null)
            : this(PROJECT, LiveBASELINE, WORKPACKS, WORKPACK_ASSIGNMENTS, p6UOW)
        {
            this.progressP6Tasks = GetP6ScheduleTasks(p6ProgressName);

            this.ReportingInterval = progressInterval;
            this.FirstAlignedDataDate = firstAlignedDataDate;

            this.alignedWeekEndingDates = ChronologicalHelpers.GenerateAlignedDatesCollection(this.FirstAlignedDataDate, this.FirstAlignedDataDate.AddYears(Int16.Parse(CommonResources.DataPointsBuilder_MaxProjectDuration)), this.ReportingInterval);
        }

        private PartialStatsBuilder(Data.PROJECT PROJECT, BASELINE LiveBASELINE, IEnumerable<WORKPACK> WORKPACKS, IEnumerable<WORKPACK_ASSIGNMENT> WORKPACK_ASSIGNMENTS, IP6EntitiesUnitOfWork p6UOW = null)
        {
            if (p6UOW == null)
                this.P6UnitOfWork = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            else
                this.P6UnitOfWork = p6UOW;

            this.originalBaselineP6Tasks = GetP6ScheduleTasks(LiveBASELINE.P6BASELINE_NAME);
            this.modifiedBaselineP6Tasks = GetP6ScheduleTasks(LiveBASELINE.P6MODBASELINE_NAME);
            this.projectWORKPACKS = WORKPACKS.ToList();
            this.projectWORKPACK_ASSIGNMENTS = WORKPACK_ASSIGNMENTS.ToList();
            this.CurrencyConversion = PROJECT.CURRENCYCONVERSION;

            this.exceptionPeriods = new List<Period>();
            this.exceptionPeriods.AddRange(ChronologicalHelpers.NonWorkingPeriods);
        }


        public void BuildPlannedDataPoints(PROGRESS_ITEMProjection progressItemStats, ReportingEnum.AssignmentLoadType assignmentLoadType)
        {
            if (progressItemStats.Entity.Entity.GUID_WORKPACK == null)
                return;

            WORKPACK currentWORKPACK = projectWORKPACKS.FirstOrDefault(x => x.GUID == progressItemStats.Entity.Entity.GUID_WORKPACK);
            if (currentWORKPACK == null)
                return;

            //Use original P6 tasks if modified is null else follow assignment type
            IEnumerable<TASK> p6BaselineTasks = assignmentLoadType == ReportingEnum.AssignmentLoadType.Original ? this.originalBaselineP6Tasks : this.modifiedBaselineP6Tasks == null ? this.originalBaselineP6Tasks : this.modifiedBaselineP6Tasks;

            List<DataPoint> progressItemP6DataPoints;

            if (TryBuildP6DataPoints(p6BaselineTasks, projectWORKPACK_ASSIGNMENTS, progressItemStats, ReportingEnum.DataPointsType.Planned, assignmentLoadType, progressItemStats.Stats.VariationAdjustments, out progressItemP6DataPoints))
            {
                progressItemStats.Stats.Budgeted.SetFromP6();
                if (assignmentLoadType == ReportingEnum.AssignmentLoadType.Original)
                    progressItemStats.Stats.Budgeted.SetData(new ObservableCollection<DataPoint>(progressItemP6DataPoints));
                else
                    progressItemStats.Stats.Current.SetData(new ObservableCollection<DataPoint>(progressItemP6DataPoints));
            }
            else
            {
                List<Period> workpackSuspensionPeriod = new List<Period>();
                workpackSuspensionPeriod.Add(new Period((DateTime)currentWORKPACK.REVIEWSTARTDATE, (DateTime)currentWORKPACK.REVIEWENDDATE));

                decimal budgetedUnits = progressItemStats.Stats.BudgetedUnits;
                decimal budgetedCosts = progressItemStats.Stats.BudgetedCosts;

                List<DataPoint> plannedDataPoints;
                if (assignmentLoadType == ReportingEnum.AssignmentLoadType.Original) //if it's generating from original baseline ignore variation
                {
                    TimeSpan workingBaseTimeSpan = (DateTime)currentWORKPACK.ENDDATE - (DateTime)currentWORKPACK.STARTDATE;
                    //plannedDataPoints = ISupportProgressReportingExtensions.DataPointsGenerator(this.ReportingInterval, this.FirstAlignedDataDate, workingBaseTimeSpan, BaselineItemBaseUnits, BaselineItemBaseCosts, (DateTime)currentWORKPACK.STARTDATE, this.CurrencyConversion, workpackSuspensionPeriod, BaselineItemTotalUnits, BaselineItemTotalCosts);

                    plannedDataPoints = DataPointsHelpers.DataPointsGenerator(this.ReportingInterval, this.FirstAlignedDataDate, workingBaseTimeSpan, budgetedUnits, budgetedCosts, (DateTime)currentWORKPACK.STARTDATE, this.CurrencyConversion, workpackSuspensionPeriod, null);

                    progressItemStats.Stats.Budgeted.SetData(new ObservableCollection<DataPoint>(plannedDataPoints));
                }
                else
                {
                    DateTime modifiedEndDateToUse = (DateTime)currentWORKPACK.ENDDATE;
                    if (currentWORKPACK.FORECASTENDDATE != null)
                        modifiedEndDateToUse = (DateTime)currentWORKPACK.FORECASTENDDATE;

                    TimeSpan workingModifiedTimeSpan = modifiedEndDateToUse - (DateTime)currentWORKPACK.STARTDATE;
                    if (currentWORKPACK.FORECASTSTARTDATE != null && ((DateTime)currentWORKPACK.FORECASTSTARTDATE) > currentWORKPACK.ENDDATE)
                        workpackSuspensionPeriod.Add(new Period(((DateTime)currentWORKPACK.ENDDATE).AddDays(1), (DateTime)currentWORKPACK.FORECASTSTARTDATE));

                    plannedDataPoints = DataPointsHelpers.DataPointsGenerator(this.ReportingInterval, this.FirstAlignedDataDate, workingModifiedTimeSpan, budgetedUnits, budgetedCosts, (DateTime)currentWORKPACK.STARTDATE, this.CurrencyConversion, workpackSuspensionPeriod, progressItemStats.Stats.VariationAdjustments);
                    progressItemStats.Stats.Current.SetData(new ObservableCollection<DataPoint>(new ObservableCollection<DataPoint>(plannedDataPoints)));
                }
            }
        }

        public void BuildEarnedDataPoints(PROGRESS_ITEMProjection progressItemStats)
        {
            IEnumerable<DataPoint> progressItemEarnedDataPoints = progressItemStats.PROGRESS_ITEMSUpToCurrentDate.Select(x => new DataPoint()
            {
                BudgetedUnits = progressItemStats.Stats.BudgetedUnits,
                BudgetedCosts = progressItemStats.Stats.BudgetedCosts * this.CurrencyConversion,
                Units = x.EARNED_UNITS,
                Costs = x.EARNED_UNITS * progressItemStats.Entity.ITEMRATE * this.CurrencyConversion,
                ProgressDate = x.EARNED_DATE,
            }).ToArray();
            progressItemStats.Stats.Earned.SetData(new ObservableCollection<DataPoint>(progressItemEarnedDataPoints));
        }

        public void BuildRemainingDataPoints(PROGRESS_ITEMProjection progressItem)
        {
            //Add earned datapoints to the collection so that % will continue after earned instead of starting from 0% abruptly
            //Individual datapoints however is controlled through plotstartdate in ConvertCumulativeToPeriodDataPoint
            List<DataPoint> dataPoints;
            if (progressItem.Stats.Earned != null && progressItem.Stats.Earned != null && progressItem.Stats.Earned.DataPoints != null && progressItem.Stats.Earned.DataPoints.Count() > 0)
                dataPoints = progressItem.Stats.Earned.DataPoints.ToList();
            else
                dataPoints = new List<DataPoint>();

            if (progressItem.RemainingUnitsAfterDataDate > 0 && progressItem.Entity.Entity.GUID_WORKPACK != null)
            {
                List<DataPoint> p6DataPoints;

                if (TryBuildP6DataPoints(this.progressP6Tasks, projectWORKPACK_ASSIGNMENTS, progressItem, ReportingEnum.DataPointsType.Remaining, ReportingEnum.AssignmentLoadType.Modified, progressItem.Stats.VariationAdjustments, out p6DataPoints))
                {
                    dataPoints.AddRange(p6DataPoints);
                    progressItem.Stats.Remaining.SetFromP6();
                    progressItem.Stats.Remaining.SetData(dataPoints);
                }
                else
                {
                    DateTime firstAlignedWeekEndingDataDate;

                    decimal firstPeriodProRate;
                    WORKPACK lookUpWORKPACK = projectWORKPACKS.FirstOrDefault(x => x.GUID == progressItem.Entity.Entity.GUID_WORKPACK);

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
                    if (progressItem.Stats.Budgeted.DataPoints == null || progressItem.Stats.Budgeted.DataPoints.Count() == 0)
                        return;

                    DateTime endDateToUse = progressItem.Stats.Budgeted.DataPoints.Max(x => x.ProgressDate);
                    endDateToUse = endDateToUse.AddDays(ReportingInterval.Days);
                    //when workpack dates are later than data date use workpack dates but have a prorate value ready for first period
                    if (startDateToUse > progressItem.Stats.ReportingDataDate)
                    {
                        firstAlignedWeekEndingDataDate = alignedWeekEndingDates.FirstOrDefault(dates => dates.Date >= startDateToUse);
                        firstPeriodProRate = Convert.ToDecimal((firstAlignedWeekEndingDataDate - startDateToUse).TotalDays / ReportingInterval.TotalDays);
                    }
                    else
                    {
                        firstAlignedWeekEndingDataDate = progressItem.Stats.ReportingDataDate.AddDays(ReportingInterval.Days);
                        firstPeriodProRate = 1;
                    }

                    List<DataPoint> remainingDataPoint = DataPointsHelpers.RemainingDataPointsGenerator(ReportingInterval, progressItem, firstAlignedWeekEndingDataDate, exceptionPeriods, progressItem.RemainingUnitsAfterDataDate, 1, this.CurrencyConversion, firstPeriodProRate, endDateToUse);
                    dataPoints.AddRange(remainingDataPoint);
                }
            }

            progressItem.Stats.Remaining.SetData(dataPoints);
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
        private bool TryBuildP6DataPoints(IEnumerable<TASK> P6TASKS, IEnumerable<WORKPACK_ASSIGNMENT> projectWORKPACKASSIGNMENTS, PROGRESS_ITEMProjection progressItem, ReportingEnum.DataPointsType processingType, ReportingEnum.AssignmentLoadType assignmentLoadType, List<VariationAdjustment> deliverableVariationAdjustments, out List<DataPoint> nonCumulativeP6DataPoints)
        {
            nonCumulativeP6DataPoints = new List<DataPoint>();
            IEnumerable<WORKPACK_ASSIGNMENT> WORKPACK_ASSIGNMENTbyType;
            if (assignmentLoadType == ReportingEnum.AssignmentLoadType.Modified)
                WORKPACK_ASSIGNMENTbyType = projectWORKPACKASSIGNMENTS.Where(assignment => assignment.ISMODIFIEDBASELINE == true);
            else
                WORKPACK_ASSIGNMENTbyType = projectWORKPACKASSIGNMENTS.Where(assignment => assignment.ISMODIFIEDBASELINE == false);

            if (assignmentLoadType == ReportingEnum.AssignmentLoadType.Modified && WORKPACK_ASSIGNMENTbyType.Count() == 0)
            {
                WORKPACK_ASSIGNMENTbyType = projectWORKPACKASSIGNMENTS.Where(assignment => assignment.ISMODIFIEDBASELINE == false);
            }
            if (progressItem.Entity.Entity.GUID_WORKPACK == null || projectWORKPACKASSIGNMENTS == null || projectWORKPACKASSIGNMENTS.Count() == 0)
                return false;

            Guid currentWORKPACKGuid = (Guid)progressItem.Entity.Entity.GUID_WORKPACK;
            IEnumerable<WORKPACK_ASSIGNMENT> currentWORKPACK_ASSIGNMENTS = WORKPACK_ASSIGNMENTbyType.Where(x => x.GUID_WORKPACK == currentWORKPACKGuid);

            if (WORKPACK_ASSIGNMENTbyType != null && P6TASKS != null && currentWORKPACK_ASSIGNMENTS.Count() != 0 && P6TASKS.Count() != 0)
            {
                currentWORKPACK_ASSIGNMENTS = currentWORKPACK_ASSIGNMENTS.OrderBy(x => x.LOW_VALUE);
                decimal totalUnits = progressItem.Stats.TotalUnits;
                decimal totalCosts = progressItem.Stats.TotalCosts;
                decimal reportableAssinmentStartUnitForWorkpackAssignmentPairing = progressItem.WorkpackAssignmentStartUnit;

                decimal currentAssignmentRemainingUnits;
                //because the earned units portion is generated independent of P6 tasks, we are only interested in what happens after earned units
                if (processingType == ReportingEnum.DataPointsType.Remaining)
                {
                    decimal TotalEarnedUnits = progressItem.TOTAL_EARNED_UNITS;
                    reportableAssinmentStartUnitForWorkpackAssignmentPairing += TotalEarnedUnits;
                    currentAssignmentRemainingUnits = totalUnits - TotalEarnedUnits;
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
                        if (processingType == ReportingEnum.DataPointsType.Planned)
                            CurrentAssignmentStartDate = (DateTime)currentAssignmentTASK.target_start_date;
                        else
                        {
                            if (currentAssignmentTASK.early_start_date == null)
                                return false;

                            CurrentAssignmentStartDate = (DateTime)currentAssignmentTASK.early_start_date;
                        }

                        DateTime CurrentAssignmentEndDate;
                        if (processingType == ReportingEnum.DataPointsType.Planned)
                            CurrentAssignmentEndDate = (DateTime)currentAssignmentTASK.target_end_date;
                        else
                            CurrentAssignmentEndDate = (DateTime)currentAssignmentTASK.early_end_date;

                        TimeSpan CurrentAssignmentWorkingPeriod = CurrentAssignmentEndDate - CurrentAssignmentStartDate;
                        decimal CurrentAssignmentUnits;

                        if (currentAssignmentRemainingUnits < currentWORKPACK_ASSIGNMENT.HIGH_VALUE)
                            CurrentAssignmentUnits = currentAssignmentRemainingUnits;
                        else
                            CurrentAssignmentUnits = (currentWORKPACK_ASSIGNMENT.HIGH_VALUE - currentWORKPACK_ASSIGNMENT.LOW_VALUE) + 1;

                        decimal Rate = progressItem.Entity.ITEMRATE;
                        decimal CurrentAssignmentCosts = CurrentAssignmentUnits * Rate;

                        List<DataPoint> p6ProgressInfo = DataPointsHelpers.DataPointsGenerator(this.ReportingInterval, this.FirstAlignedDataDate, CurrentAssignmentWorkingPeriod, CurrentAssignmentUnits, CurrentAssignmentCosts, CurrentAssignmentStartDate, this.CurrencyConversion, this.exceptionPeriods);
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

        private IEnumerable<DataPoint> ConvertDeliverablesDataPointToProgressInfo(IEnumerable<StoredProcedure_DeliverablesDataPoints> deliverablesDataPoints, decimal BudgetedUnits, decimal BudgetedCosts)
        {
            List<DataPoint> progressInfoConversion = new List<DataPoint>();
            foreach (StoredProcedure_DeliverablesDataPoints deliverablesDataPoint in deliverablesDataPoints)
            {
                progressInfoConversion.Add(new DataPoint
                {
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
}
