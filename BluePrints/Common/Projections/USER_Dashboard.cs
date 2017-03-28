using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.Data.Helpers;
using BluePrints.P6EntitiesDataModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    public class USER_Dashboard : SummarizableObject, IHaveGUID
    {
        public Guid GUID { get; set; }
        public PROJECT PROJECT { get; set; }
        public ReportableObject ReportableObject { get; set; }
        public ProjectReportableDataPointsBuilder DataPointsBuilder { get; set; }
        public decimal UnitsProgressRatio
        {
            get
            {
                if (Summary_CumulativeEarned != null && Summary_CumulativePlanned != null)
                {
                    decimal earnedUnits = Summary_CumulativeEarned.Units;
                    decimal plannedUnits = Summary_CumulativePlanned.Units;

                    if (plannedUnits > 0 && earnedUnits > 0)
                        return earnedUnits / plannedUnits;
                    else
                        return 0;
                }
                else
                    return 0;
            }
        }

        public decimal CostsProgressRatio
        {
            get
            {
                if (Summary_CumulativeEarned != null && Summary_CumulativePlanned != null)
                {
                    decimal earnedCosts = Summary_CumulativeEarned.Costs;
                    decimal plannedCosts = Summary_CumulativePlanned.Costs;

                    if (plannedCosts > 0 && earnedCosts > 0)
                        return earnedCosts / plannedCosts;
                    else
                        return 0;
                }
                else
                    return 0;
            }
        }
    }

    public static class USER_DashboardQueries
    {
        public static IQueryable<USER_Dashboard> SummarizeUserDashboard(IQueryable<PROJECT> activePROJECTS, Func<IEnumerable<DELIVERABLES_STATUS>> getDELIVERABLES_STATUSESFunc, USER user, Action raisePropertyChanged = null)
        {
            List<USER_Dashboard> USER_Dashboards = new List<USER_Dashboard>();
            IP6EntitiesUnitOfWork p6UnitOfWork = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            Dictionary<PROJECT, IEnumerable<BASELINE_ITEM>> projectUSER_DELIVERABLES = new Dictionary<PROJECT, IEnumerable<BASELINE_ITEM>>();

            foreach (PROJECT activePROJECT in activePROJECTS)
            {
                BASELINE liveBASELINE = activePROJECT.BASELINE.FirstOrDefault(x => x.STATUS == BaselineStatus.Live);
                if (liveBASELINE == null)
                    continue;

                IEnumerable<BASELINE_ITEM> userBASELINE_ITEMS = liveBASELINE.BASELINE_ITEM.Where(x => x.GUID_USER != null && x.GUID_USER == user.GUID).ToArray();
                if (userBASELINE_ITEMS.Count() == 0)
                    continue;

                projectUSER_DELIVERABLES.Add(activePROJECT, userBASELINE_ITEMS);
            }

            LoadingScreenManager.ShowLoadingScreen(projectUSER_DELIVERABLES.Sum(x => x.Value.Count()));
            foreach (KeyValuePair<PROJECT, IEnumerable<BASELINE_ITEM>> projectUSER_DELIVERABLE in projectUSER_DELIVERABLES)
            {
                PROJECT activePROJECT = projectUSER_DELIVERABLE.Key;
                IEnumerable<BASELINE_ITEM> userBASELINE_ITEMS = projectUSER_DELIVERABLE.Value;

                BASELINE liveBASELINE = activePROJECT.BASELINE.FirstOrDefault(x => x.STATUS == BaselineStatus.Live);
                IEnumerable<BASELINE_ITEM> projectBASELINE_ITEMS = liveBASELINE.BASELINE_ITEM.ToArray();

                List<PROGRESS_ITEMProjection> reportableObjectsForWorkpackAssignment = projectBASELINE_ITEMS.Select(x => new PROGRESS_ITEMProjection() { BASELINE_ITEMJoinRATE = new BASELINE_ITEMProjection() { BASELINE_ITEM = x } }).ToList();
                ISupportProgressReportingExtensions.SetWorkpackAssignmentStartUnit(reportableObjectsForWorkpackAssignment);

                IEnumerable<WORKPACK> projectWORKPACK = activePROJECT.WORKPACK;

                PROGRESS livePROGRESS = activePROJECT.PROGRESS.FirstOrDefault(x => x.STATUS == ProgressStatus.Live);
                if (livePROGRESS == null)
                    continue;

                IEnumerable<PROGRESS_ITEM> livePROGRESS_ITEMS = livePROGRESS.PROGRESS_ITEM;

                IEnumerable<RATE> projectRATES = activePROJECT.RATE;
                IEnumerable<VARIATION> approvedVARIATIONS = activePROJECT.VARIATION.Where(x => x.APPROVED != null);
                IEnumerable<VARIATION_ITEMProjection> approvedVARIATION_ITEMS = ISupportProgressReportingExtensions.ConvertVARIATIONITEMProjection(approvedVARIATIONS);

                IEnumerable<PROGRESS_ITEMProjection> PROJECT_PROGRESS_ITEMS = PROGRESS_ITEMProjectionQueries.JoinRATESAndPROGRESS_ITEMSOnBASELINE_ITEMS(userBASELINE_ITEMS.AsQueryable(), () => livePROGRESS, () => liveBASELINE, () => livePROGRESS_ITEMS, () => projectRATES, getDELIVERABLES_STATUSESFunc).ToArray();

                TimeSpan intervalTimeSpan = ISupportProgressReportingExtensions.ConvertProgressIntervalToPeriod(livePROGRESS);
                DateTime firstAlignedDataDate = ISupportProgressReportingExtensions.GenerateFirstAlignedDataDate(livePROGRESS);
                DateTime dataDate = livePROGRESS.DATA_DATE;

                ProjectReportableDataPointsBuilder dataPointsBuilder = new ProjectReportableDataPointsBuilder(intervalTimeSpan, dataDate, firstAlignedDataDate, activePROJECT.CURRENCYCONVERSION, approvedVARIATION_ITEMS, projectWORKPACK, p6UnitOfWork, liveBASELINE.P6BASELINE_NAME, liveBASELINE.P6MODBASELINE_NAME, livePROGRESS.P6PROGRESS_NAME);

                DataPointsBuildingFactory dataPointsBuildingFactory = new DataPointsBuildingFactory();
                foreach (PROGRESS_ITEMProjection PROJECT_PROGRESS_ITEM in PROJECT_PROGRESS_ITEMS)
                {
                    PROGRESS_ITEMProjection reportableObjectCopy = new PROGRESS_ITEMProjection();
                    //Need to copy reporting data date first because this affects PROGRESS_ITEM set
                    reportableObjectCopy.ReportingDataDate = PROJECT_PROGRESS_ITEM.ReportingDataDate;
                    DataUtils.ShallowCopy(reportableObjectCopy, PROJECT_PROGRESS_ITEM);
                    ReportableObject findWorkpackAssignment = reportableObjectsForWorkpackAssignment.First(x => x.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID == reportableObjectCopy.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID);
                    //string s;
                    reportableObjectCopy.WorkpackAssignmentStartUnit = findWorkpackAssignment.WorkpackAssignmentStartUnit;
                    //if (findWorkpackAssignment.WorkpackAssignmentStartUnit == 0)
                    //    s = string.Empty;

                    USER_Dashboard newUSERDashboard = new USER_Dashboard() { ReportableObject = reportableObjectCopy, DataPointsBuilder = dataPointsBuilder, GUID = reportableObjectCopy.PROGRESS_ITEMCurrent.GUID, PROJECT = activePROJECT, LiveBASELINE = liveBASELINE, LivePROGRESS = livePROGRESS, ReportingDataDate = dataDate, FirstAlignedDataDate = firstAlignedDataDate, IntervalPeriod = intervalTimeSpan };

                    dataPointsBuildingFactory.Manufacture(newUSERDashboard.DataPointsBuilder, newUSERDashboard.ReportableObject);
                    newUSERDashboard.NonCumulative_ActualDataPoints = newUSERDashboard.ReportableObject.NonCumulative_ActualDataPoints;
                    newUSERDashboard.NonCumulative_BurnedDataPoints = newUSERDashboard.ReportableObject.NonCumulative_BurnedDataPoints;
                    newUSERDashboard.NonCumulative_EarnedDataPoints = newUSERDashboard.ReportableObject.NonCumulative_EarnedDataPoints;
                    newUSERDashboard.NonCumulative_OriginalDataPoints = newUSERDashboard.ReportableObject.NonCumulative_OriginalDataPoints;
                    newUSERDashboard.NonCumulative_PlannedDataPoints = newUSERDashboard.ReportableObject.NonCumulative_PlannedDataPoints;
                    newUSERDashboard.NonCumulative_RemainingPlannedDataPoints = newUSERDashboard.ReportableObject.NonCumulative_RemainingPlannedDataPoints;
                    newUSERDashboard.NonCumulative_VariationAdjustments = newUSERDashboard.ReportableObject.NonCumulative_VariationAdjustments;
                    newUSERDashboard.Cumulative_VariationAdjustments = newUSERDashboard.ReportableObject.Cumulative_VariationAdjustments;
                    newUSERDashboard.Summary_CumulativeActualDataPoints = newUSERDashboard.ReportableObject.Summary_CumulativeActualDataPoints;
                    newUSERDashboard.Summary_CumulativeBurnedDataPoints = newUSERDashboard.ReportableObject.Summary_CumulativeBurnedDataPoints;
                    newUSERDashboard.Summary_CumulativeEarnedDataPoints = newUSERDashboard.ReportableObject.Summary_CumulativeEarnedDataPoints;
                    newUSERDashboard.Summary_CumulativeOriginalDataPoints = newUSERDashboard.ReportableObject.Summary_CumulativeOriginalDataPoints;
                    newUSERDashboard.Summary_CumulativePlannedDataPoints = newUSERDashboard.ReportableObject.Summary_CumulativePlannedDataPoints;
                    newUSERDashboard.Summary_CumulativeRemainingPlannedDataPoints = newUSERDashboard.ReportableObject.Summary_CumulativeRemainingPlannedDataPoints;

                    //Place user dashboard inside its summarizable object collection for DashboardWrapper summarizing purpose
                    List<ReportableObject> reportableObjectsForDashboardWrapper = new List<ReportableObject>();
                    reportableObjectsForDashboardWrapper.Add(newUSERDashboard.ReportableObject);
                    newUSERDashboard.ReportableObjects = reportableObjectsForDashboardWrapper;
                    USER_Dashboards.Add(newUSERDashboard);
                    LoadingScreenManager.Progress();
                    //USER_Dashboards.Add(new USER_Dashboard() { ReportableObject = reportableObjectCopy, DataPointsBuilder = dataPointsBuilder, GUID = reportableObjectCopy.PROGRESS_ITEMCurrent.GUID, PROJECT = activePROJECT, LiveBASELINE = liveBASELINE, LivePROGRESS = livePROGRESS, ReportingDataDate = dataDate, FirstAlignedDataDate = firstAlignedDataDate, IntervalPeriod = intervalTimeSpan });
                }
            }

            //BackgroundWorker summaryBackgroundWorker = new BackgroundWorker();
            //summaryBackgroundWorker.DoWork += summaryBackgroundWorker_DoWork;
            //summaryBackgroundWorker.WorkerSupportsCancellation = true;
            //summaryBackgroundWorker.RunWorkerAsync(new object[] { USER_Dashboards, raisePropertyChanged });

            //DataPointsBuildingFactory dataPointsBuildingFactory = new DataPointsBuildingFactory();

            //foreach (var userDashboard in USER_Dashboards)
            //{
            //    dataPointsBuildingFactory.Manufacture(userDashboard.DataPointsBuilder, userDashboard.ReportableObject);
            //    userDashboard.NonCumulative_ActualDataPoints = userDashboard.ReportableObject.NonCumulative_ActualDataPoints;
            //    userDashboard.NonCumulative_BurnedDataPoints = userDashboard.ReportableObject.NonCumulative_BurnedDataPoints;
            //    userDashboard.NonCumulative_EarnedDataPoints = userDashboard.ReportableObject.NonCumulative_EarnedDataPoints;
            //    userDashboard.NonCumulative_OriginalDataPoints = userDashboard.ReportableObject.NonCumulative_OriginalDataPoints;
            //    userDashboard.NonCumulative_PlannedDataPoints = userDashboard.ReportableObject.NonCumulative_PlannedDataPoints;
            //    userDashboard.NonCumulative_RemainingPlannedDataPoints = userDashboard.ReportableObject.NonCumulative_RemainingPlannedDataPoints;
            //    userDashboard.NonCumulative_VariationAdjustments = userDashboard.ReportableObject.NonCumulative_VariationAdjustments;
            //    userDashboard.Cumulative_VariationAdjustments = userDashboard.ReportableObject.Cumulative_VariationAdjustments;
            //    userDashboard.Summary_CumulativeActualDataPoints = userDashboard.ReportableObject.Summary_CumulativeActualDataPoints;
            //    userDashboard.Summary_CumulativeBurnedDataPoints = userDashboard.ReportableObject.Summary_CumulativeBurnedDataPoints;
            //    userDashboard.Summary_CumulativeEarnedDataPoints = userDashboard.ReportableObject.Summary_CumulativeEarnedDataPoints;
            //    userDashboard.Summary_CumulativeOriginalDataPoints = userDashboard.ReportableObject.Summary_CumulativeOriginalDataPoints;
            //    userDashboard.Summary_CumulativePlannedDataPoints = userDashboard.ReportableObject.Summary_CumulativePlannedDataPoints;
            //    userDashboard.Summary_CumulativeRemainingPlannedDataPoints = userDashboard.ReportableObject.Summary_CumulativeRemainingPlannedDataPoints;

            //    //Place user dashboard inside its summarizable object collection for DashboardWrapper summarizing purpose
            //    List<ReportableObject> reportableObjectsForDashboardWrapper = new List<ReportableObject>();
            //    reportableObjectsForDashboardWrapper.Add(userDashboard.ReportableObject);
            //    userDashboard.ReportableObjects = reportableObjectsForDashboardWrapper;
            //    LoadingScreenManager.Progress();
            //}

            return USER_Dashboards.AsQueryable();
        }

        //private static void summaryBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    var argumentObject = (object[])e.Argument;
        //    IEnumerable<USER_Dashboard> userDashboards = (IEnumerable<USER_Dashboard>)argumentObject[0];
        //    Action raisePropertyChanged = (Action)argumentObject[1];

        //    //datapoints factory dictates the sequence which datapoints are built
        //    DataPointsBuildingFactory dataPointsBuildingFactory = new DataPointsBuildingFactory();
        //    foreach (var userDashboard in userDashboards)
        //    {
        //        dataPointsBuildingFactory.Manufacture(userDashboard.DataPointsBuilder, userDashboard.ReportableObject);
        //        userDashboard.NonCumulative_ActualDataPoints = userDashboard.ReportableObject.NonCumulative_ActualDataPoints;
        //        userDashboard.NonCumulative_BurnedDataPoints = userDashboard.ReportableObject.NonCumulative_BurnedDataPoints;
        //        userDashboard.NonCumulative_EarnedDataPoints = userDashboard.ReportableObject.NonCumulative_EarnedDataPoints;
        //        userDashboard.NonCumulative_OriginalDataPoints = userDashboard.ReportableObject.NonCumulative_OriginalDataPoints;
        //        userDashboard.NonCumulative_PlannedDataPoints = userDashboard.ReportableObject.NonCumulative_PlannedDataPoints;
        //        userDashboard.NonCumulative_RemainingPlannedDataPoints = userDashboard.ReportableObject.NonCumulative_RemainingPlannedDataPoints;
        //        userDashboard.NonCumulative_VariationAdjustments = userDashboard.ReportableObject.NonCumulative_VariationAdjustments;
        //        userDashboard.Cumulative_VariationAdjustments = userDashboard.ReportableObject.Cumulative_VariationAdjustments;
        //        userDashboard.Summary_CumulativeActualDataPoints = userDashboard.ReportableObject.Summary_CumulativeActualDataPoints;
        //        userDashboard.Summary_CumulativeBurnedDataPoints = userDashboard.ReportableObject.Summary_CumulativeBurnedDataPoints;
        //        userDashboard.Summary_CumulativeEarnedDataPoints = userDashboard.ReportableObject.Summary_CumulativeEarnedDataPoints;
        //        userDashboard.Summary_CumulativeOriginalDataPoints = userDashboard.ReportableObject.Summary_CumulativeOriginalDataPoints;
        //        userDashboard.Summary_CumulativePlannedDataPoints = userDashboard.ReportableObject.Summary_CumulativePlannedDataPoints;
        //        userDashboard.Summary_CumulativeRemainingPlannedDataPoints = userDashboard.ReportableObject.Summary_CumulativeRemainingPlannedDataPoints;

        //        if (((BackgroundWorker)sender).CancellationPending)
        //        {
        //            e.Cancel = true;
        //            return;
        //        }

        //        List<ReportableObject> reportableObjectsForDashboardWrapper = new List<ReportableObject>();
        //        reportableObjectsForDashboardWrapper.Add(userDashboard.ReportableObject);
        //        userDashboard.ReportableObjects = reportableObjectsForDashboardWrapper;
        //        raisePropertyChanged?.Invoke();
        //    }
        //}
    }
}