using BaseModel.Data.Helpers;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Objects;
using System.Linq;
using static BluePrints.Data.BluePrintsEntities;

namespace BluePrints.Common.ViewModel.Reporting
{
    public class FullStatsBuilder : PartialStatsBuilder
    {
        readonly string ProjectNumber;
        readonly IPrimeroEntitiesUnitOfWork PrimeroUOW;

        public FullStatsBuilder(Data.PROJECT PROJECT, BASELINE liveBASELINE, PROGRESS LivePROGRESS, IEnumerable<WORKPACK> WORKPACKS, IEnumerable<WORKPACK_ASSIGNMENT> WORKPACK_ASSIGNMENTS, IP6EntitiesUnitOfWork p6UOW = null, IPrimeroEntitiesUnitOfWork primeroUOW = null)
            : base(PROJECT, liveBASELINE, LivePROGRESS, WORKPACKS, WORKPACK_ASSIGNMENTS, p6UOW)
        {
            ProjectNumber = PROJECT.NUMBER;
            PrimeroUOW = primeroUOW == null ? PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork() : primeroUOW;
        }

        public void BuildExoDataPoints(ProjectSummaryStats summaryObject)
        {
            try
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
                                      join JOB_COSTGROUPS in PrimeroUnitOfWork.JOB_COSTGROUPS
                                      on JOBTRANS.COST_GROUP equals JOB_COSTGROUPS.SEQNO
                                      join JOB_COSTTYPES in PrimeroUnitOfWork.JOB_COSTTYPES
                                      on JOBTRANS.COST_TYPE equals JOB_COSTTYPES.SEQNO
                                      where JOBCOST_HDR2.JOBCODE == projectNumber && JOBTRANS.TRANSTYPE == "T" && JOBTRANS.LINE_STATUS != "X"
                                      select new { JOBCOST_HDR1.JOBCODE, JOBTRANS.QUANTITY, JOBTRANS.LINETOTAL, JOBTRANS.LINECOST, JOBTRANS.TRANSDATE, JOBCOST_RESOURCE.RESOURCENAME, JOB_COSTGROUPS.COSTDESC, COSTDESC3 = JOB_COSTTYPES.COSTDESC };

                var exoWorkpacks = from JOBCOST_HDR in PrimeroUnitOfWork.JOBCOST_HDR
                                   where JOBCOST_HDR.JOBCODE.Contains(projectNumber)
                                   select new { JOBCOST_HDR.TITLE, JOBCOST_HDR.JOBCODE };

                var exoWorkpacksList = exoWorkpacks.ToList();
                foreach (WORKPACK workpack in workpacks)
                {
                    var exoWorkpack = exoWorkpacksList.FirstOrDefault(x => x.JOBCODE == workpack.INTERNAL_NAME1);
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
                        burnedDataPoint.CostGroup = jobTransaction.COSTDESC;
                        burnedDataPoint.CostType = jobTransaction.COSTDESC3;

                        burnedDataPoints.Add(burnedDataPoint);

                        ExoDataPoint actualDataPoint = new ExoDataPoint();
                        DataUtils.ShallowCopy(actualDataPoint, burnedDataPoint);
                        actualDataPoint.Costs = jobTransaction.LINECOST == null ? 0 : (decimal)jobTransaction.LINECOST;
                        actualDataPoints.Add(actualDataPoint);
                    }
                }

                projectSummaryStats.Burned = new Stats(summaryObject);
                projectSummaryStats.Actual = new Stats(summaryObject);

                projectSummaryStats.Burned.SetData(burnedDataPoints);
                projectSummaryStats.Actual.SetData(actualDataPoints);
                //LoadingScreenManager.Progress();
            }
            catch(Exception e)
            {
                string s = e.ToString();
            }
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

        readonly string p6BaselineName;
        readonly string p6ProgressProjectName;
        readonly DateTime dataDate;

        IP6EntitiesUnitOfWork P6UnitOfWork { get; set; }
        public PartialStatsBuilder(Data.PROJECT PROJECT, BASELINE LiveBASELINE, PROGRESS LivePROGRESS, IEnumerable<WORKPACK> WORKPACKS, IEnumerable<WORKPACK_ASSIGNMENT> WORKPACK_ASSIGNMENTS, IP6EntitiesUnitOfWork p6UOW = null)
        {
            if (p6UOW == null)
                this.P6UnitOfWork = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            else
                this.P6UnitOfWork = p6UOW;

            this.p6ProgressProjectName = LivePROGRESS.P6PROGRESS_NAME;
            this.progressP6Tasks = GetP6ScheduleTasks(LivePROGRESS.P6PROGRESS_NAME);
            this.dataDate = LivePROGRESS.DATA_DATE;

            this.ReportingInterval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(LivePROGRESS);
            this.FirstAlignedDataDate = ChronologicalHelpers.GenerateFirstAlignedDataDate(LivePROGRESS);
            this.alignedWeekEndingDates = ChronologicalHelpers.GenerateAlignedDatesCollection(this.FirstAlignedDataDate, this.FirstAlignedDataDate.AddYears(Int16.Parse(BluePrintsResources.DataPointsBuilder_MaxProjectDuration)), this.ReportingInterval);


            this.p6BaselineName = LiveBASELINE.P6BASELINE_NAME;
            this.originalBaselineP6Tasks = GetP6ScheduleTasks(LiveBASELINE.P6BASELINE_NAME);
            this.modifiedBaselineP6Tasks = GetP6ScheduleTasks(LiveBASELINE.P6MODBASELINE_NAME);
            this.projectWORKPACKS = WORKPACKS.ToList();
            this.projectWORKPACK_ASSIGNMENTS = WORKPACK_ASSIGNMENTS.ToList();
            this.CurrencyConversion = PROJECT.CURRENCYCONVERSION;

            this.exceptionPeriods = new List<Period>();
            this.exceptionPeriods.AddRange(ChronologicalHelpers.NonWorkingPeriods);
        }

        public void BuildPlannedDataPointsFromQuery(PROGRESS_ITEMProjection progressItemStats)
        {
            using (BluePrintsEntities bluePrintDataContext = new BluePrintsEntities())
            {
                List<StoredProcedure_PlannedDataPoint> plannedDataPoints = bluePrintDataContext.QueryDeliverablePlannedDataPoints(progressItemStats.Entity.Entity.GUID);
                progressItemStats.Stats.Budgeted.SetPlannedData(plannedDataPoints);
                progressItemStats.Stats.Current.SetPlannedData(plannedDataPoints);
            }
        }

        public void BuildPlannedDataPointsFromStoredProcedure(PROGRESS_ITEMProjection progressItemStats)
        {
            using (BluePrintsEntities bluePrintDataContext = new BluePrintsEntities())
            {
                decimal totalUnits = progressItemStats.Stats == null ? 0 : progressItemStats.Stats.totalUnits;
                decimal rate = progressItemStats.Entity.RATE == null ? 0 : progressItemStats.Entity.RATE.RATE1 == null ? 0 :
                    (decimal)progressItemStats.Entity.RATE.RATE1;
                bool isByDuration = progressItemStats.Entity.Entity.BY_DURATION;
                Guid workpackKey = progressItemStats.Entity.Entity.GUID_WORKPACK == null ? Guid.Empty : (Guid)progressItemStats.Entity.Entity.GUID_WORKPACK;

                ObjectResult<StoredProcedure_PlannedDataPoint> deliverablesDataPoints = bluePrintDataContext.GetDeliverablePlannedDataPoints(this.p6BaselineName, this.p6ProgressProjectName, this.dataDate, progressItemStats.Entity.EntityKey, progressItemStats.Entity.Entity.GUID_ORIGINAL, workpackKey, totalUnits, rate, isByDuration);

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

                progressItemStats.Stats.Budgeted.SetPlannedData(plannedDataPoints);
                progressItemStats.Stats.Current.SetPlannedData(plannedDataPoints);
            }
        }

        public void BuildRemainingDataPointsFromQuery(PROGRESS_ITEMProjection progressItemStats)
        {
            using (BluePrintsEntities bluePrintDataContext = new BluePrintsEntities())
            {
                List<StoredProcedure_RemainingDataPoint> RemainingDataPoints = bluePrintDataContext.QueryDeliverableRemainingDataPoints(progressItemStats.Entity.Entity.GUID);
                progressItemStats.Stats.Remaining.SetRemainingData(RemainingDataPoints, progressItemStats.Stats.Earned.DataPoints);
            }
        }

        public void BuildRemainingDataPointsFromStoredProcedure(PROGRESS_ITEMProjection progressItemStats)
        {
            using (BluePrintsEntities bluePrintDataContext = new BluePrintsEntities())
            {
                decimal totalUnits = progressItemStats.Stats == null ? 0 : progressItemStats.Stats.totalUnits;
                decimal rate = progressItemStats.Entity.RATE == null ? 0 : progressItemStats.Entity.RATE.RATE1 == null ? 0 :
                    (decimal)progressItemStats.Entity.RATE.RATE1;
                Guid workpackKey = progressItemStats.Entity.Entity.GUID_WORKPACK == null ? Guid.Empty : (Guid)progressItemStats.Entity.Entity.GUID_WORKPACK;

                decimal totalEarnedUnits = 0;
                if (progressItemStats.Stats.Earned.DataPoints != null && progressItemStats.Stats.Earned.DataPoints.Count > 0)
                    totalEarnedUnits = progressItemStats.Stats.Earned.DataPoints.Sum(x => x.Units);

                ObjectResult<StoredProcedure_RemainingDataPoint> deliverablesDataPoints = bluePrintDataContext.GetDeliverableRemainingDataPoints(this.p6ProgressProjectName, this.dataDate, progressItemStats.Entity.EntityKey, progressItemStats.Entity.Entity.GUID_ORIGINAL, workpackKey, totalUnits, totalEarnedUnits, rate);

                List<StoredProcedure_RemainingDataPoint> RemainingDataPoints = new List<StoredProcedure_RemainingDataPoint>();
                //circumvent EF issue when ObjectResult is null
                try
                {
                    RemainingDataPoints.AddRange(deliverablesDataPoints);
                }
                catch
                {
                    return;
                }

                //if (RemainingDataPoints != null && RemainingDataPoints.Count > 0)
                //    Debug.Print(progressItemStats.Entity.Entity.INTERNAL_NUM + "|" + progressItemStats.Stats.totalUnits + "|" + RemainingDataPoints.Sum(x => x.PeriodRemainingUnits));

                progressItemStats.Stats.Remaining.SetRemainingData(RemainingDataPoints, progressItemStats.Stats.Earned.DataPoints);
            }
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

                List<DataPoint> plannedDataPoints;
                if (assignmentLoadType == ReportingEnum.AssignmentLoadType.Original) //if it's generating from original baseline ignore variation
                {
                    TimeSpan workingBaseTimeSpan = (DateTime)currentWORKPACK.ENDDATE - (DateTime)currentWORKPACK.STARTDATE;

                    plannedDataPoints = DataPointsHelpers.DataPointsGenerator(this.ReportingInterval, this.FirstAlignedDataDate, workingBaseTimeSpan, progressItemStats.Stats.BudgetedUnits, progressItemStats.Stats.BudgetedCosts, (DateTime)currentWORKPACK.STARTDATE, this.CurrencyConversion, workpackSuspensionPeriod, progressItemStats.Stats.VariationAdjustments);

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

                    plannedDataPoints = DataPointsHelpers.DataPointsGenerator(this.ReportingInterval, this.FirstAlignedDataDate, workingModifiedTimeSpan, progressItemStats.Stats.BudgetedUnits, progressItemStats.Stats.BudgetedCosts, (DateTime)currentWORKPACK.STARTDATE, this.CurrencyConversion, workpackSuspensionPeriod, progressItemStats.Stats.VariationAdjustments);

                    progressItemStats.Stats.Current.SetData(new ObservableCollection<DataPoint>(plannedDataPoints));
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

            if (progressItem.Stats.totalUnits == 0)
                return false;

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
                decimal totalUnits = progressItem.Stats.totalUnits;
                //decimal currentAssignmentRemainingUnits;
                decimal earnedPercentage;
                //because the earned units portion is generated independent of P6 tasks, we are only interested in what happens after earned units
                if (processingType == ReportingEnum.DataPointsType.Remaining)
                    earnedPercentage = progressItem.TOTAL_EARNED_UNITS / totalUnits;
                else
                    earnedPercentage = 0;

                IEnumerable<WORKPACK_ASSIGNMENT> applicableWORKPACK_ASSIGNMENTS = currentWORKPACK_ASSIGNMENTS.Where(x => x.HIGH_VALUE > earnedPercentage);

                foreach (WORKPACK_ASSIGNMENT applicableWORKPACK_ASSIGNMENT in applicableWORKPACK_ASSIGNMENTS)
                {
                    TASK currentAssignmentTASK = P6TASKS.FirstOrDefault(task => task.task_code == applicableWORKPACK_ASSIGNMENT.P6_ACTIVITYID);
                    DateTime CurrentAssignmentStartDate;
                    if (processingType == ReportingEnum.DataPointsType.Planned)
                    {
                        CurrentAssignmentStartDate = (DateTime)currentAssignmentTASK.target_start_date;
                    }
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
                    decimal remainingLowValue = applicableWORKPACK_ASSIGNMENT.LOW_VALUE > earnedPercentage ? applicableWORKPACK_ASSIGNMENT.LOW_VALUE : earnedPercentage + 0.01m;

                    decimal currentAssignmentPercentage = (applicableWORKPACK_ASSIGNMENT.HIGH_VALUE - remainingLowValue) + 0.01m;
                    decimal currentAssignmentUnits = currentAssignmentPercentage * totalUnits;

                    decimal Rate = progressItem.Entity.ITEMRATE;
                    decimal CurrentAssignmentCosts = currentAssignmentUnits * Rate;

                    List<DataPoint> p6ProgressInfo = DataPointsHelpers.DataPointsGenerator(this.ReportingInterval, this.FirstAlignedDataDate, CurrentAssignmentWorkingPeriod, currentAssignmentUnits, CurrentAssignmentCosts, CurrentAssignmentStartDate, this.CurrencyConversion, this.exceptionPeriods);
                    nonCumulativeP6DataPoints.AddRange(p6ProgressInfo);
                }

                if (processingType == ReportingEnum.DataPointsType.Remaining && (nonCumulativeP6DataPoints.Sum(x => x.Units) + progressItem.TOTAL_EARNED_UNITS) < totalUnits)
                {
                    string s = string.Empty;
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
        #endregion

    }
}
