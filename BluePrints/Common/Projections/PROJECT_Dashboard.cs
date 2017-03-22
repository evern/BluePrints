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
            Func<IQueryable<PROGRESS>> getLivePROGRESSESFunc, Func<IQueryable<PROGRESS_ITEM>> getLivePROGRESS_ITEMFunc,
            Func<IQueryable<BASELINE>> getLiveBASELINESFunc, Func<IQueryable<RATE>> getRATESFunc,
            Func<IQueryable<VARIATION>> getApprovedVARIATIONFunc = null, Action raisePropertyChanged = null,
            Guid? SinglePROJECTGuid = null, Guid? USERGuid = null)
        {
            var LiveBASELINES = getLiveBASELINESFunc().ToArray().AsEnumerable();
            var LivePROGRESSES = getLivePROGRESSESFunc().ToArray().AsEnumerable();

            IEnumerable<VARIATION> ApprovedVARIATIONS;
            if (getApprovedVARIATIONFunc != null)
                ApprovedVARIATIONS = getApprovedVARIATIONFunc().ToArray().AsEnumerable();
            else
                ApprovedVARIATIONS = new List<VARIATION>();

            IEnumerable<RATE> AllRATES = getRATESFunc();
            IEnumerable<PROJECT> localPROJECTS;

            if (SinglePROJECTGuid != null)
                localPROJECTS = PROJECTS.Where(x => x.GUID == SinglePROJECTGuid).ToArray().AsEnumerable();
                    //process only active PROJECTS
            else
                localPROJECTS = PROJECTS.Where(x => x.STATUS == ProjectStatus.Active).ToArray().AsEnumerable();
                    //process only active PROJECTS

            var returnPROJECT_Dashboard = new List<PROJECT_Dashboard>();
            var bluePrintsUnitOfWork =
                BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            var p6UnitOfWork = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();

            foreach (var localPROJECT in localPROJECTS)
            {
                var currentPROJECTLiveBASELINE =
                    LiveBASELINES.FirstOrDefault(x => x.GUID_PROJECT == localPROJECT.GUID);
                if (currentPROJECTLiveBASELINE == null)
                    continue;

                var currentPROJECTLivePROGRESS =
                    LivePROGRESSES.FirstOrDefault(x => x.GUID_PROJECT == localPROJECT.GUID && x.STATUS == ProgressStatus.Live);

                if (currentPROJECTLivePROGRESS == null)
                    continue;

                var LivePROGRESS_ITEMS =
                    getLivePROGRESS_ITEMFunc()
                        .Where(x => x.PROGRESS.GUID == currentPROJECTLivePROGRESS.GUID)
                        .AsQueryable();
                IQueryable<BASELINE_ITEM> LiveBASELINE_ITEMS;
                if (USERGuid == null)
                    LiveBASELINE_ITEMS = currentPROJECTLiveBASELINE.BASELINE_ITEM.AsQueryable();
                else
                    LiveBASELINE_ITEMS = currentPROJECTLiveBASELINE.BASELINE_ITEM.Where(x => x.GUID_USER == USERGuid).AsQueryable();

                var RATESByProject = AllRATES.Where(x => x.GUID_PROJECT == localPROJECT.GUID).AsQueryable();
                var ApprovedVARIATIONSByProject =
                    ApprovedVARIATIONS.Where(x => x.GUID_PROJECT == localPROJECT.GUID).AsEnumerable();

                IEnumerable<ReportableObject> PROJECTInfos =
                    PROGRESS_ITEMProjectionQueries.JoinRATESAndPROGRESS_ITEMSOnBASELINE_ITEMS(
                        LiveBASELINE_ITEMS, () => currentPROJECTLivePROGRESS, () => currentPROJECTLiveBASELINE,
                        () => LivePROGRESS_ITEMS, () => RATESByProject).ToArray().AsEnumerable();

                var currentPROJECT_Dashboard = new PROJECT_Dashboard()
                {
                    GUID = localPROJECT.GUID,
                    PROJECT = localPROJECT,
                    VARIATIONS = ApprovedVARIATIONSByProject
                };

                currentPROJECT_Dashboard.InitializeBuilder(PROJECTInfos, currentPROJECTLivePROGRESS,
                    currentPROJECTLiveBASELINE, bluePrintsUnitOfWork, p6UnitOfWork);
                returnPROJECT_Dashboard.Add(currentPROJECT_Dashboard);
            }

            var summaryBackgroundWorker = new BackgroundWorker();
            summaryBackgroundWorker.DoWork += summaryBackgroundWorker_DoWork;
            summaryBackgroundWorker.WorkerSupportsCancellation = true;
            summaryBackgroundWorker.RunWorkerAsync(new object[] { returnPROJECT_Dashboard, raisePropertyChanged});

            return returnPROJECT_Dashboard.AsQueryable();
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
            Func<IQueryable<PROGRESS_ITEM>> getPROGRESS_ITEMSFunc, Func<IQueryable<BASELINE_ITEM>> getBASELINE_ITEMSFunc,
            Func<BASELINE> getBASELINEFunc, Func<IQueryable<RATE>> getRATESFunc)
        {
            var bluePrintsUnitOfWork =
                BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            var p6UnitOfWork = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();

            IEnumerable<ReportableObject> PROJECTInfos =
                PROGRESS_ITEMProjectionQueries.JoinRATESAndPROGRESS_ITEMSOnBASELINE_ITEMS(
                        getBASELINE_ITEMSFunc(), getPROGRESSFunc, getBASELINEFunc, getPROGRESS_ITEMSFunc, getRATESFunc)
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