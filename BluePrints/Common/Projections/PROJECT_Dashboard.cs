using BluePrints.BluePrintsEntitiesDataModel;
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
    public class PROJECT_Dashboard : PROJECTSummary
    {
        public PROJECT_Dashboard()
        {

        }

        public Guid GUID { get; set; }
        public PROJECT PROJECT { get; set; }
        public PROJECTSummaryBuilder SummaryBuilder { get; private set; }
        public void InitializeBuilder(IEnumerable<ReportableObject> reportableObjects, PROGRESS livePROGRESS, BASELINE liveBASELINE, IBluePrintsEntitiesUnitOfWork bluePrintsUnitOfWork, IP6EntitiesUnitOfWork p6UnitOfWork)
        {
            this.ReportableObjects = reportableObjects;
            this.LiveBASELINE = liveBASELINE;
            this.LivePROGRESS = livePROGRESS;

            SummaryBuilder = new PROJECTSummaryBuilder(this, bluePrintsUnitOfWork, p6UnitOfWork);
        }
    }

    public static class PROJECT_DashboardQueries
    {
        public static IQueryable<PROJECT_Dashboard> SummarizePROJECTDashboard(IQueryable<PROJECT> PROJECTS, Func<IQueryable<PROGRESS>> getLivePROGRESSESFunc, Func<IQueryable<BASELINE>> getLiveBASELINESFunc, Func<IQueryable<RATE>> getRATESFunc, Func<IQueryable<VARIATION>> getApprovedVARIATIONFunc = null, Action raisePropertyChanged = null, bool ignoreLiveStatus = false)
        {
            IEnumerable<BASELINE> LiveBASELINES = getLiveBASELINESFunc().ToArray().AsEnumerable();
            IEnumerable<PROGRESS> LivePROGRESSES = getLivePROGRESSESFunc().ToArray().AsEnumerable();
            IEnumerable<VARIATION> ApprovedVARIATIONS;
            if (getApprovedVARIATIONFunc != null)
                ApprovedVARIATIONS = getApprovedVARIATIONFunc().ToArray().AsEnumerable();
            else
                ApprovedVARIATIONS = new List<VARIATION>();
            
            IEnumerable<RATE> AllRATES = getRATESFunc();
            IEnumerable<PROJECT> localPROJECTS;
            if(ignoreLiveStatus)
                localPROJECTS = PROJECTS.ToArray().AsEnumerable();
            else
                localPROJECTS = PROJECTS.Where(x => x.STATUS == ProjectStatus.Active).ToArray().AsEnumerable(); //process only active PROJECTS

            List<PROJECT_Dashboard> returnPROJECT_Dashboard = new List<PROJECT_Dashboard>();
            IBluePrintsEntitiesUnitOfWork bluePrintsUnitOfWork = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            IP6EntitiesUnitOfWork p6UnitOfWork = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();

            foreach(PROJECT localPROJECT in localPROJECTS)
            {
                BASELINE currentPROJECTLiveBASELINE = LiveBASELINES.FirstOrDefault(x => x.GUID_PROJECT == localPROJECT.GUID);
                if (currentPROJECTLiveBASELINE == null)
                    continue;

                PROGRESS currentPROJECTLivePROGRESS = LivePROGRESSES.FirstOrDefault(x => x.GUID_PROJECT == localPROJECT.GUID);
                if (currentPROJECTLivePROGRESS == null)
                    continue;

                IQueryable<PROGRESS_ITEM> LivePROGRESS_ITEMS = currentPROJECTLivePROGRESS.PROGRESS_ITEM.AsQueryable();
                IQueryable<BASELINE_ITEM> LiveBASELINE_ITEMS = currentPROJECTLiveBASELINE.BASELINE_ITEM.AsQueryable();
                IQueryable<RATE> RATESByProject = AllRATES.Where(x => x.GUID_PROJECT == localPROJECT.GUID).AsQueryable();
                IEnumerable<VARIATION> ApprovedVARIATIONSByProject = ApprovedVARIATIONS.Where(x => x.GUID_PROJECT == localPROJECT.GUID).AsEnumerable();

                IEnumerable<ReportableObject> PROJECTInfos = PROGRESS_ITEMProjectionQueries.JoinRATESAndPROGRESS_ITEMSOnBASELINE_ITEMS(
                    LiveBASELINE_ITEMS, () => currentPROJECTLivePROGRESS, () => currentPROJECTLiveBASELINE, () => LivePROGRESS_ITEMS, () => RATESByProject).ToArray().AsEnumerable();

                PROJECT_Dashboard currentPROJECT_Dashboard = new PROJECT_Dashboard() { GUID = localPROJECT.GUID, PROJECT = localPROJECT, VARIATIONS = ApprovedVARIATIONSByProject };

                currentPROJECT_Dashboard.InitializeBuilder(PROJECTInfos, currentPROJECTLivePROGRESS, currentPROJECTLiveBASELINE, bluePrintsUnitOfWork, p6UnitOfWork);
                returnPROJECT_Dashboard.Add(currentPROJECT_Dashboard);
            }

            BackgroundWorker summaryBackgroundWorker = new BackgroundWorker();
            summaryBackgroundWorker.DoWork += summaryBackgroundWorker_DoWork;
            summaryBackgroundWorker.WorkerSupportsCancellation = true;
            summaryBackgroundWorker.RunWorkerAsync(new object[] { returnPROJECT_Dashboard, raisePropertyChanged });

            return returnPROJECT_Dashboard.AsQueryable();
        }

        static void summaryBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            object[] argumentObject = (object[])e.Argument;
            BuildProjectStats summaryManufacturer = new BuildProjectStats();
            IEnumerable<PROJECT_Dashboard> projects = (IEnumerable<PROJECT_Dashboard>)argumentObject[0];
            Action raisePropertyChanged = (Action)argumentObject[1];

            foreach(PROJECT_Dashboard project in projects)
            {
                summaryManufacturer.Manufacture(project.SummaryBuilder);
                if (raisePropertyChanged != null)
                    raisePropertyChanged();
            }

            if (((BackgroundWorker)sender).CancellationPending)
            {
                e.Cancel = true;
                return;
            }
            else
                e.Result = raisePropertyChanged;
        }

        public static PROJECT_Dashboard SummarizeSinglePROJECTDashboard(PROJECT PROJECT, Func<PROGRESS> getPROGRESSFunc, Func<IQueryable<PROGRESS_ITEM>> getPROGRESS_ITEMSFunc, Func<IQueryable<BASELINE_ITEM>> getBASELINE_ITEMSFunc, Func<BASELINE> getBASELINEFunc, Func<IQueryable<RATE>> getRATESFunc)
        {

            IBluePrintsEntitiesUnitOfWork bluePrintsUnitOfWork = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            IP6EntitiesUnitOfWork p6UnitOfWork = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();

            IEnumerable<ReportableObject> PROJECTInfos = PROGRESS_ITEMProjectionQueries.JoinRATESAndPROGRESS_ITEMSOnBASELINE_ITEMS(
                    getBASELINE_ITEMSFunc(), getPROGRESSFunc, getBASELINEFunc, getPROGRESS_ITEMSFunc, getRATESFunc).ToArray().AsEnumerable();

            PROJECT_Dashboard currentPROJECT_Dashboard = new PROJECT_Dashboard() { GUID = PROJECT.GUID, PROJECT = PROJECT };
            currentPROJECT_Dashboard.InitializeBuilder(PROJECTInfos, getPROGRESSFunc(), getBASELINEFunc(), bluePrintsUnitOfWork, p6UnitOfWork);
            BuildProjectStats summaryManufacturer = new BuildProjectStats();
            summaryManufacturer.Manufacture(currentPROJECT_Dashboard.SummaryBuilder);

            return currentPROJECT_Dashboard;
        }
    }
}
