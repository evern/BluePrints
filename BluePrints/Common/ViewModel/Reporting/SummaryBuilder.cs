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
        public abstract void SummarizePlannedDataPoints();
        public abstract void SummarizeModifiedPlannedDataPoints();
        public abstract void SummarizeEarnedDataPoints();
        public abstract void SummarizeBurnedDataPoints();
        public abstract void SummarizeRemainingDataPoints();
        public abstract void SummarizeActualDataPoints();
        public abstract void GroupAndAccumulateDataPointsByPeriod();
        public abstract void GroupAndAccumulateReportableDataPointsByPeriod();

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

        public override void SummarizePlannedDataPoints()
        {
            SummaryObject.NonCumulative_OriginalDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_OriginalDataPoints));
        }

        public override void SummarizeModifiedPlannedDataPoints()
        {
            SummaryObject.NonCumulative_PlannedDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_PlannedDataPoints));
        }

        public override void SummarizeEarnedDataPoints()
        {
            SummaryObject.NonCumulative_EarnedDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_EarnedDataPoints));
        }

        public override void SummarizeActualDataPoints()
        {
            throw new InvalidOperationException("there is no need to roll up non cumulative actual data points from ReportableObjects.");
        }

        public override void SummarizeBurnedDataPoints()
        {
            throw new InvalidOperationException("there is no need to roll up non cumulative burned data points from ReportableObjects.");
        }

        public override void SummarizeRemainingDataPoints()
        {
            SummaryObject.NonCumulative_RemainingCurrentDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_RemainingCurrentDataPoints));
            SummaryObject.NonCumulative_RemainingPlannedDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_RemainingPlannedDataPoints));
        }

        public override void GroupAndAccumulateDataPointsByPeriod()
        {
            ISupportProgressReportingExtensions.GenerateCumulativeSummaryDataPoints(SummaryObject);
        }

        public override void GroupAndAccumulateReportableDataPointsByPeriod()
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

        public override void SummarizePlannedDataPoints()
        {
            SummaryObject.NonCumulative_OriginalDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_OriginalDataPoints));
        }

        public override void SummarizeModifiedPlannedDataPoints()
        {
            SummaryObject.NonCumulative_PlannedDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_PlannedDataPoints));
        }

        public override void SummarizeEarnedDataPoints()
        {
            SummaryObject.NonCumulative_EarnedDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_EarnedDataPoints));
        }

        public override void SummarizeActualDataPoints()
        {
            throw new InvalidOperationException("there is no need to roll up non cumulative actual data points from ReportableObjects.");
        }

        public override void SummarizeBurnedDataPoints()
        {
            throw new InvalidOperationException("there is no need to roll up non cumulative burned data points from ReportableObjects.");
        }

        public override void SummarizeRemainingDataPoints()
        {
            SummaryObject.NonCumulative_RemainingCurrentDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_RemainingCurrentDataPoints));
            SummaryObject.NonCumulative_RemainingPlannedDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_RemainingPlannedDataPoints));
        }

        public override void GroupAndAccumulateDataPointsByPeriod()
        {
            ISupportProgressReportingExtensions.GenerateCumulativeSummaryDataPoints(SummaryObject);
        }

        public override void GroupAndAccumulateReportableDataPointsByPeriod()
        {
            throw new NotImplementedException();
        }
    }

    public class PROJECTSummaryBuilder : SummaryBuilder
    {
        IBluePrintsEntitiesUnitOfWork BluePrintsUnitOfWork { get; set; }
        IP6EntitiesUnitOfWork P6UnitOfWork { get; set; }
        decimal CurrencyConversion { get; set; }
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

        public override void GroupAndAccumulateDataPointsByPeriod()
        {
            ISupportProgressReportingExtensions.GenerateCumulativeSummaryDataPoints(this.SummaryObject);
        }

        public override void GroupAndAccumulateReportableDataPointsByPeriod()
        {
            foreach (ReportableObject reportableObject in SummaryObject.ReportableObjects)
            {
                ISupportProgressReportingExtensions.GenerateCumulativeSummaryDataPoints(reportableObject, this.SummaryObject.FirstAlignedDataDate, this.SummaryObject.IntervalPeriod);
            }
        }

        public override void SummarizePlannedDataPoints()
        {
            //PlannedDataPointsBuilderFromDatabase(CURRENTPROJECT.NUMBER, false);
            SummarizePlannedDataPointsByType(true);
        }

        public override void SummarizeModifiedPlannedDataPoints()
        {
            //PlannedDataPointsBuilderFromDatabase(CURRENTPROJECT.NUMBER, true);
            SummarizePlannedDataPointsByType(false);
        }

        private void SummarizePlannedDataPointsFromDatabase(string ProjectNumber, bool isOriginal)
        {
            BluePrintsEntities bluePrintDataContext = new BluePrintsEntities();
            ObjectResult<StoredProcedure_DeliverablesDataPoints> deliverablesDataPoints = bluePrintDataContext.GetDataPointsByProject(ProjectNumber, isOriginal);
            SummarizePlannedDataPointsByType(isOriginal, deliverablesDataPoints.ToList());
        }

        private void SummarizePlannedDataPointsByType(bool isOriginal, IEnumerable<StoredProcedure_DeliverablesDataPoints> DataPointsCollection = null)
        {
            AssignmentLoadType assignmentLoadType = isOriginal == true ? AssignmentLoadType.Original : assignmentLoadType = AssignmentLoadType.Modified;
            string p6ScheduleShortName = isOriginal == true ? SummaryObject.LiveBASELINE.P6BASELINE_NAME : SummaryObject.LiveBASELINE.P6MODBASELINE_NAME;
            IEnumerable <TASK> p6Tasks = p6ScheduleShortName == string.Empty ? null : GetP6ScheduleTasks(p6ScheduleShortName);

            Dictionary<Guid, decimal> workpackP6AssignedUnits = new Dictionary<Guid, decimal>();
            foreach (ReportableObject reportableObject in SummaryObject.ReportableObjects)
            {
                //Populate the progressItem variation adjustments
                reportableObject.NonCumulative_VariationAdjustments = new ObservableCollection<VariationAdjustment>(SummaryObject.NonCumulative_VariationAdjustments.Where(adjustment => adjustment.BaselineItemGuid == reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_ORIGINAL).ToList());
                reportableObject.Cumulative_VariationAdjustments = ISupportProgressReportingExtensions.PopulateCumulativeVariationAdjustments(reportableObject.NonCumulative_VariationAdjustments, SummaryObject.FirstAlignedDataDate, SummaryObject.IntervalPeriod);

                BuildPlannedDataPoints(reportableObject, p6Tasks, SummaryObject.IntervalPeriod, SummaryObject.FirstAlignedDataDate, SummaryObject.ReportingDataDate, assignmentLoadType, workpackP6AssignedUnits, DataPointsCollection);
            }

            if (isOriginal)
                SummaryObject.NonCumulative_OriginalDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_OriginalDataPoints));
            else
                SummaryObject.NonCumulative_PlannedDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_PlannedDataPoints));
        }

        public void BuildPlannedDataPoints(ReportableObject reportableObject, IEnumerable<TASK> p6ProjectTasks, TimeSpan intervalPeriod, DateTime firstAlignedDataDate, DateTime reportingDataDate, AssignmentLoadType assignmentLoadType, Dictionary<Guid, decimal> workpackP6AssignedUnits,
          IEnumerable<StoredProcedure_DeliverablesDataPoints> dbDataPointsCollection = null)
        {
            //Assign the report date for stats display
            reportableObject.ReportingDataDate = reportingDataDate;

            BASELINE_ITEMProjection currentBASELINE_ITEM = reportableObject.BASELINE_ITEMJoinRATE;
            WORKPACK currentWORKPACK = currentBASELINE_ITEM.BASELINE_ITEM.WORKPACK;
            if (currentWORKPACK == null)
                return;

            List<ProgressInfo> progressItemP6DataPoints;
            if (TryBuildP6DataPoints(p6ProjectTasks, reportableObject, DataPointsType.Planned, assignmentLoadType, workpackP6AssignedUnits, out progressItemP6DataPoints))
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
                        plannedDataPoints = ISupportProgressReportingExtensions.DataPointsGenerator(SummaryObject.IntervalPeriod, SummaryObject.FirstAlignedDataDate, workingBaseTimeSpan, BaselineItemBaseUnits, BaselineItemBaseCosts, (DateTime)currentWORKPACK.STARTDATE, currentBASELINE_ITEM.BASELINE_ITEM.GUID_ORIGINAL, this.CurrencyConversion, workpackSuspensionPeriod, BaselineItemTotalUnits, BaselineItemTotalCosts);
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
                        plannedDataPoints = ISupportProgressReportingExtensions.DataPointsGenerator(SummaryObject.IntervalPeriod, SummaryObject.FirstAlignedDataDate, workingModifiedTimeSpan, BaselineItemBaseUnits, BaselineItemBaseCosts, (DateTime)currentWORKPACK.STARTDATE, currentBASELINE_ITEM.BASELINE_ITEM.GUID_ORIGINAL, this.CurrencyConversion, workpackSuspensionPeriod, null, null, reportableObject.Cumulative_VariationAdjustments);

                        //Used to show normalized variation
                        //plannedDataPoints = DataPointsGenerator(WorkingPeriod, progressInterval, BaselineItemTotalUnits, BaselineItemTotalCosts, this.CurrencyConversion, baselineItem.WORKPACK.STARTDATE, firstAlignedDataDate, baselineItem.GUID_ORIGINAL);
                        reportableObject.NonCumulative_PlannedDataPoints = new ObservableCollection<ProgressInfo>(plannedDataPoints);
                    }
                }
            }
        }

        /// <summary>
        /// Calculates each baselineItem earned data point while populating aggregate non cumulative earned data points
        /// </summary>
        /// <returns>Non cumulative earned progress data points</returns>
        public override void SummarizeEarnedDataPoints()
        {
            Dictionary<Guid, decimal> workpackAssignedUnits = new Dictionary<Guid, decimal>();
            foreach (ReportableObject reportableObject in SummaryObject.ReportableObjects)
            {
                BuildEarnedDataPoints(reportableObject, SummaryObject.ReportingDataDate);
            }

            SummaryObject.NonCumulative_EarnedDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(progressItem => progressItem.NonCumulative_EarnedDataPoints));
        }

        public void BuildEarnedDataPoints(ReportableObject reportableObject, DateTime reportingDataDate)
        {
            reportableObject.ReportingDataDate = reportingDataDate;
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

        public override void SummarizeRemainingDataPoints()
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
            Dictionary<Guid, decimal> workpackRemainingAssignedUnits = new Dictionary<Guid, decimal>();
            IEnumerable<TASK> PROGRESS_TASKS = GetP6ScheduleTasks(SummaryObject.LivePROGRESS.P6PROGRESS_NAME);
            foreach (ReportableObject reportableObject in SummaryObject.ReportableObjects)
            {
                BuildRemainingDataPoints(reportableObject, SummaryObject.ReportingDataDate, SummaryObject.IntervalPeriod, alignedWeekEndingDates, PROGRESS_TASKS, workpackRemainingAssignedUnits, exceptionPeriods);
            }

            //extract all data points out to be used as an overall summary
            SummaryObject.NonCumulative_RemainingPlannedDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(progressItem => progressItem.NonCumulative_RemainingPlannedDataPoints));
            SummaryObject.NonCumulative_RemainingCurrentDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(progressItem => progressItem.NonCumulative_RemainingCurrentDataPoints));
        }

        private void BuildRemainingDataPoints(ReportableObject reportableObject, DateTime dataDate, TimeSpan progressInterval, List<DateTime> alignedWeekEndingDates, IEnumerable<TASK> p6ProgressTASKS, Dictionary<Guid, decimal> workpackRemainingAssignedUnits, List<Period> exceptionPeriods)
        {
            //when remaining units is more than 0 continue calculation
            if (reportableObject.RemainingUnitsAfterDataDate > 0 && reportableObject.BASELINE_ITEMJoinRATE.BASELINE_ITEM.WORKPACK != null)
            {
                List<ProgressInfo> progressItemP6DataPoints;
                if (TryBuildP6DataPoints(p6ProgressTASKS, reportableObject, DataPointsType.Remaining, AssignmentLoadType.Modified, workpackRemainingAssignedUnits, out progressItemP6DataPoints))
                {
                    reportableObject.isRemainingDataPointsFromP6 = true;
                    reportableObject.NonCumulative_RemainingPlannedDataPoints = new ObservableCollection<ProgressInfo>(progressItemP6DataPoints);
                }
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
                    if (startDateToUse > dataDate)
                    {
                        firstAlignedWeekEndingDataDate = alignedWeekEndingDates.FirstOrDefault(dates => dates.Date >= startDateToUse);
                        firstPeriodProRate = Convert.ToDecimal((firstAlignedWeekEndingDataDate.AddSeconds(1) - startDateToUse).TotalDays / progressInterval.TotalDays);
                    }
                    else
                    {
                        firstAlignedWeekEndingDataDate = dataDate.AddDays(progressInterval.Days);
                        firstPeriodProRate = 1;
                    }

                    decimal maxInefficiency = 0.5M;

                    decimal currentEfficiency = (reportableObject.ActualProductivity / reportableObject.BaselineProductivity);

                    reportableObject.NonCumulative_RemainingPlannedDataPoints = ISupportProgressReportingExtensions.RemainingDataPointsGenerator(progressInterval, reportableObject, firstAlignedWeekEndingDataDate, exceptionPeriods, reportableObject.RemainingUnitsAfterDataDate, reportableObject.BaselineProductivity, this.CurrencyConversion, firstPeriodProRate);

                    //if there's a planned finish date based on baseline productivity, inflate periodic units/costs
                    DateTime? plannedLimitDate = (reportableObject.NonCumulative_RemainingPlannedDataPoints == null || reportableObject.NonCumulative_RemainingPlannedDataPoints.Count == 0) ? (DateTime?)null : reportableObject.NonCumulative_RemainingPlannedDataPoints.Last().ProgressDate;

                    if (currentEfficiency < maxInefficiency)
                        currentEfficiency = maxInefficiency;

                    decimal inflatedInefficientUnits = currentEfficiency > 0 ? (reportableObject.RemainingUnitsAfterDataDate / currentEfficiency) : reportableObject.RemainingUnitsAfterDataDate;

                    reportableObject.NonCumulative_RemainingCurrentDataPoints = ISupportProgressReportingExtensions.RemainingDataPointsGenerator(progressInterval, reportableObject, firstAlignedWeekEndingDataDate, exceptionPeriods, inflatedInefficientUnits, reportableObject.ActualProductivity, this.CurrencyConversion, firstPeriodProRate, plannedLimitDate);
                }
            }
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
        private bool TryBuildP6DataPoints(IEnumerable<TASK> P6TASKS, ReportableObject reportableObject, DataPointsType processingType, AssignmentLoadType assignmentLoadType, Dictionary<Guid, Decimal> workpackAssignedUnits, out List<ProgressInfo> nonCumulativeP6DataPoints)
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

                //because the earned unit portion is generated independent of P6 tasks, we start forecast generation after earned units portion
                if (processingType == DataPointsType.Remaining)
                    totalUnitsAssigned += reportableObject.TOTAL_EARNED_UNITS;

                decimal currentAssignmentRemainingUnits = totalUnits;
                foreach (WORKPACK_ASSIGNMENT currentWORKPACK_ASSIGNMENT in currentWORKPACK_ASSIGNMENTS)
                {
                    if (currentAssignmentRemainingUnits == 0)
                        break;

                    decimal compareUnitsAssigned = Math.Round(totalUnitsAssigned + 1, 0);
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

                        List<ProgressInfo> p6ProgressInfo = ISupportProgressReportingExtensions.DataPointsGenerator(SummaryObject.IntervalPeriod, SummaryObject.FirstAlignedDataDate, CurrentAssignmentWorkingPeriod, CurrentAssignmentUnits, CurrentAssignmentCosts, CurrentAssignmentStartDate, currentBASELINE_ITEM.BASELINE_ITEM.GUID_ORIGINAL, this.CurrencyConversion, null, null, null);
                        nonCumulativeP6DataPoints.AddRange(p6ProgressInfo);
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
        public override void SummarizeBurnedDataPoints()
        {
            ObservableCollection<ProgressInfo> nonCumulative_BurnedDataPoints = new ObservableCollection<ProgressInfo>();
            DateTime firstAlignedDataDate = SummaryObject.FirstAlignedDataDate;
            TimeSpan progressInterval = SummaryObject.IntervalPeriod;
            DateTime loopDate = firstAlignedDataDate;

            IEnumerable<WORKPACK> workpacks = SummaryObject.LiveBASELINE.PROJECT.WORKPACK.ToList();
            string projectNumber = SummaryObject.LiveBASELINE.PROJECT.NUMBER;

            IEnumerable<string> qualifiedWorkpacks = workpacks == null ? new List<string>() : workpacks.Select(x => x.INTERNAL_NAME1);
            var PrimeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
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
            SummaryObject.missingExoWorkpacks = new List<WORKPACK>();
            foreach (WORKPACK workpack in workpacks)
            {
                var exoWorkpack = exoWorkpacksList.FirstOrDefault(x => x.JOBCODE == workpack.INTERNAL_NAME1 || x.JOBCODE == workpack.INTERNAL_NAME2);
                if (exoWorkpack == null)
                {
                    SummaryObject.missingExoWorkpacks.Add(workpack);
                }
            }

            var jobTransactionsList = jobTransactions.ToList();
            if (jobTransactionsList.Count == 0)
                return;

            List<DateTime> alignedDataDates = ISupportProgressReportingExtensions.GenerateAlignedDatesCollection(firstAlignedDataDate, jobTransactionsList.Max(x => x.TRANSDATE).Value, progressInterval);
            foreach (var jobTransaction in jobTransactionsList)
            {
                if (qualifiedWorkpacks.Contains(jobTransaction.JOBCODE))
                {
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
            }

            
            SummaryObject.NonCumulative_BurnedDataPoints = nonCumulative_BurnedDataPoints;
        }

        public override void SummarizeActualDataPoints()
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

        private enum DataPointsType
        {
            Planned = 0,
            Earned = 1,
            Remaining = 2
        }

        public enum AssignmentLoadType
        {
            Original,
            Modified,
            Both
        }
    }
}
