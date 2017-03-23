using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.DataModel;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.P6EntitiesDataModel;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    public class PROJECT_Dashboard : PROJECTSummary, IHaveGUID
    {
        public PROJECT_Dashboard()
        {
        }

        public Guid GUID { get; set; }
        public PROJECT PROJECT { get; set; }
        public PROJECTSummaryBuilder SummaryBuilder { get; private set; }

        public void InitializeBuilder(IEnumerable<ReportableObject> reportableObjects, PROGRESS livePROGRESS,
            BASELINE liveBASELINE, IBluePrintsEntitiesUnitOfWork bluePrintsUnitOfWork,
            IP6EntitiesUnitOfWork p6UnitOfWork, PROJECT currentProject = null)
        {
            ReportableObjects = reportableObjects;
            LiveBASELINE = liveBASELINE;
            LivePROGRESS = livePROGRESS;
            SummaryBuilder = new PROJECTSummaryBuilder(this, bluePrintsUnitOfWork, p6UnitOfWork, PROJECT);
        }
    }

    public static class PROJECT_DashboardQueries
    {
        public static IQueryable<PROJECT_Dashboard> SummarizePROJECTDashboard(IQueryable<PROJECT> PROJECTS,
            Func<IEnumerable<PROGRESS>> getLivePROGRESSESFunc, Func<IEnumerable<PROGRESS_ITEM>> getLivePROGRESS_ITEMFunc,
            Func<IEnumerable<BASELINE>> getLiveBASELINESFunc, Func<IEnumerable<RATE>> getRATESFunc, Func<IEnumerable<DELIVERABLES_STATUS>> getDELIVERABLES_STATUSESFunc, 
            Func<IEnumerable<VARIATION>> getApprovedVARIATIONFunc = null, Action raisePropertyChanged = null,
            Guid? SinglePROJECTGuid = null, Guid? USERGuid = null)
        {
            var LiveBASELINES = getLiveBASELINESFunc();
            var LivePROGRESSES = getLivePROGRESSESFunc();

            IEnumerable<VARIATION> ApprovedVARIATIONS = ApprovedVARIATIONS = getApprovedVARIATIONFunc();
            IEnumerable<RATE> AllRATES = getRATESFunc();
            IQueryable<PROJECT> singleOrActivePROJECT;

            if (SinglePROJECTGuid != null)
                singleOrActivePROJECT = PROJECTS.Where(x => x.GUID == SinglePROJECTGuid);
                    //process only active PROJECTS
            else
                singleOrActivePROJECT = PROJECTS.Where(x => x.STATUS == ProjectStatus.Active);
            //process only active PROJECTS

            List<PROJECT_Dashboard> PROJECTDashboard = new List<PROJECT_Dashboard>();
            var bluePrintsUnitOfWork =
                BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            var p6UnitOfWork = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();

            foreach (var localPROJECT in singleOrActivePROJECT)
            {
                BASELINE currentPROJECTLiveBASELINE =
                    LiveBASELINES.FirstOrDefault(x => x.GUID_PROJECT == localPROJECT.GUID);
                if (currentPROJECTLiveBASELINE == null)
                    continue;

                PROGRESS currentPROJECTLivePROGRESS =
                    LivePROGRESSES.FirstOrDefault(x => x.GUID_PROJECT == localPROJECT.GUID && x.STATUS == ProgressStatus.Live);

                if (currentPROJECTLivePROGRESS == null)
                    continue;

                IEnumerable<PROGRESS_ITEM> LivePROGRESS_ITEMS =
                    getLivePROGRESS_ITEMFunc()
                        .Where(x => x.PROGRESS.GUID == currentPROJECTLivePROGRESS.GUID);

                IEnumerable<BASELINE_ITEM> LiveBASELINE_ITEMS;
                if (USERGuid == null)
                    LiveBASELINE_ITEMS = currentPROJECTLiveBASELINE.BASELINE_ITEM;
                else
                    LiveBASELINE_ITEMS = currentPROJECTLiveBASELINE.BASELINE_ITEM.Where(x => x.GUID_USER == USERGuid);

                IEnumerable<RATE> RATESByProject = AllRATES.Where(x => x.GUID_PROJECT == localPROJECT.GUID);
                IEnumerable<VARIATION> ApprovedVARIATIONSByProject =
                    ApprovedVARIATIONS.Where(x => x.GUID_PROJECT == localPROJECT.GUID);

                IEnumerable<ReportableObject> PROJECTInfos =
                    PROGRESS_ITEMProjectionQueries.JoinRATESAndPROGRESS_ITEMSOnBASELINE_ITEMS(
                        LiveBASELINE_ITEMS.AsQueryable(), () => currentPROJECTLivePROGRESS, () => currentPROJECTLiveBASELINE,
                        () => LivePROGRESS_ITEMS, () => RATESByProject, () => getDELIVERABLES_STATUSESFunc()).ToArray().AsEnumerable();

                var currentPROJECT_Dashboard = new PROJECT_Dashboard()
                {
                    GUID = localPROJECT.GUID,
                    PROJECT = localPROJECT,
                    VARIATIONS = ApprovedVARIATIONSByProject
                };

                currentPROJECT_Dashboard.InitializeBuilder(PROJECTInfos, currentPROJECTLivePROGRESS,
                    currentPROJECTLiveBASELINE, bluePrintsUnitOfWork, p6UnitOfWork);
                PROJECTDashboard.Add(currentPROJECT_Dashboard);
            }

            var summaryBackgroundWorker = new BackgroundWorker();
            summaryBackgroundWorker.DoWork += summaryBackgroundWorker_DoWork;
            summaryBackgroundWorker.WorkerSupportsCancellation = true;
            summaryBackgroundWorker.RunWorkerAsync(new object[] { PROJECTDashboard, raisePropertyChanged});

            return PROJECTDashboard.AsQueryable();
        }

        private static void summaryBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var argumentObject = (object[]) e.Argument;
            var summaryManufacturer = new ProjectSummarizingFactory();
            var projects = (IEnumerable<PROJECT_Dashboard>) argumentObject[0];
            var raisePropertyChanged = (Action) argumentObject[1];

            foreach (var project in projects)
            {
                summaryManufacturer.Manufacture(project.SummaryBuilder);
                if (((BackgroundWorker) sender).CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                raisePropertyChanged?.Invoke();
            }
        }

        public static PROJECT_Dashboard SummarizeSinglePROJECTDashboard(PROJECT PROJECT, Func<PROGRESS> getPROGRESSFunc,
            Func<IEnumerable<PROGRESS_ITEM>> getPROGRESS_ITEMSFunc, Func<IEnumerable<BASELINE_ITEM>> getBASELINE_ITEMSFunc,
            Func<BASELINE> getBASELINEFunc, Func<IEnumerable<RATE>> getRATESFunc, Func<IEnumerable<DELIVERABLES_STATUS>> getDELIVERABLES_STATUSESFunc)
        {
            var bluePrintsUnitOfWork =
                BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            var p6UnitOfWork = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();

            IEnumerable<ReportableObject> PROJECTInfos =
                PROGRESS_ITEMProjectionQueries.JoinRATESAndPROGRESS_ITEMSOnBASELINE_ITEMS(
                        getBASELINE_ITEMSFunc().AsQueryable(), getPROGRESSFunc, getBASELINEFunc, getPROGRESS_ITEMSFunc, getRATESFunc, getDELIVERABLES_STATUSESFunc)
                    .ToArray()
                    .AsEnumerable();

            var currentPROJECT_Dashboard = new PROJECT_Dashboard()
            {
                GUID = PROJECT.GUID,
                PROJECT = PROJECT
            };

            currentPROJECT_Dashboard.InitializeBuilder(PROJECTInfos, getPROGRESSFunc(), getBASELINEFunc(),
                bluePrintsUnitOfWork, p6UnitOfWork);
            var summaryManufacturer = new ProjectSummarizingFactory();
            summaryManufacturer.Manufacture(currentPROJECT_Dashboard.SummaryBuilder);

            return currentPROJECT_Dashboard;
        }
    }
}