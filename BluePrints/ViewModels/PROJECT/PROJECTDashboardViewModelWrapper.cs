using BaseModel.DataModel;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Document;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the PROJECTS collection view model.
    /// </summary>
    public class PROJECTDashboardViewModelWrapper :
        DashboardViewModelWrapper<PROJECT, PROJECT_Dashboard, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        //ensure mainviewmodel is loaded before calling background worker
        private DispatcherTimer onMainViewModelFirstLoadedTimer;
        //allow background worker to be cancelled
        List<BackgroundWorker> backgroundWorkerCollection = new List<BackgroundWorker>();

        /// <summary>
        /// Creates a new instance of PROJECT_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static PROJECTDashboardViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new PROJECTDashboardViewModelWrapper());
        }

        /// <summary>
        /// Initializes a new instance of the PROJECTViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTViewModel type without the POCO proxy factory.
        /// </summary>
        protected PROJECTDashboardViewModelWrapper()
        {
            onMainViewModelFirstLoadedTimer = new DispatcherTimer();
            onMainViewModelFirstLoadedTimer.Interval = new TimeSpan(0, 0, 0, 1);
            onMainViewModelFirstLoadedTimer.Tick += onMainViewModelFirstLoaded;
        }

        private void BuildStatsTimer_Tick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        #region Database Operation

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        ActionObject actionObject;
        protected override void InitializeParameters(object parameter)
        {
            if (parameter != null)
            {
                actionObject = parameter as ActionObject;
            }
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINES, BASELINEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESS_ITEMS, PROGRESS_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription<RATE, RATE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.RATES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.VARIATIONS, VARIATIONProjectionFunc);
            loaderCollection.AddLoaderDescription<DELIVERABLES_STATUS, DELIVERABLES_STATUS, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DELIVERABLES_STATUSES);
            loaderCollection.AddLoaderDescription<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.USERS);
            InvokeEntitiesLoaderDescriptionLoading();
        }

        private Func<IRepositoryQuery<BASELINE>, IQueryable<BASELINE>> BASELINEProjectionFunc()
        {
            return query => query.Where(x => x.STATUS == BaselineStatus.Live);
        }

        private Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> PROGRESSProjectionFunc()
        {
            return query => query.Where(x => x.STATUS == ProgressStatus.Live);
        }

        private Func<IRepositoryQuery<PROGRESS_ITEM>, IQueryable<PROGRESS_ITEM>> PROGRESS_ITEMProjectionFunc()
        {
            return
                query =>
                    query.Where(
                        x =>
                            x.PROGRESS.STATUS == ProgressStatus.Live &&
                            x.PROGRESS.PROJECT.STATUS == ProjectStatus.Active);
        }

        private Func<IRepositoryQuery<VARIATION>, IQueryable<VARIATION>> VARIATIONProjectionFunc()
        {
            return query => query.Where(x => x.APPROVED != null);
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            if (actionObject != null)
            {
                actionObject.ExecuteAction();
                return;
            }

            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.PROJECTS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT_Dashboard>>
            ConstructMainViewModelProjection()
        {
            var getBASELINESFunc = loaderCollection.GetCollectionFunc<BASELINE>();
            var getPROGRESSESFunc = loaderCollection.GetCollectionFunc<PROGRESS>();
            var getPROGRESS_ITEMSFunc = loaderCollection.GetCollectionFunc<PROGRESS_ITEM>();
            var getRATESFunc = loaderCollection.GetCollectionFunc<RATE>();
            var getVARIATIONSFunc = loaderCollection.GetCollectionFunc<VARIATION>();
            var getDELIVERABLES_STATUSESFunc = loaderCollection.GetCollectionFunc<DELIVERABLES_STATUS>();

            return
                query =>
                    PROJECT_DashboardQueries.SummarizePROJECTDashboard(query.OrderBy(x => x.NUMBER), getPROGRESSESFunc,
                        getPROGRESS_ITEMSFunc, getBASELINESFunc, getRATESFunc, getDELIVERABLES_STATUSESFunc, getVARIATIONSFunc,
                        () => RaisePropertyChanged(), null, false);
        }

        protected override bool OnMainViewModelLoaded(IEnumerable<PROJECT_Dashboard> entities)
        {
            MainViewModel =
                (CollectionViewModel<PROJECT, PROJECT_Dashboard, Guid, IBluePrintsEntitiesUnitOfWork>)
                mainEntityLoaderDescription.GetViewModel();
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertiesChanged()));
            MainViewModel.SetParentViewModel(this);
            onMainViewModelFirstLoadedTimer.Start();
            
            return base.OnMainViewModelLoaded(entities);
        }

        private void onMainViewModelFirstLoaded(object sender, EventArgs e)
        {
            onMainViewModelFirstLoadedTimer.Stop();
            backgroundWorkerCollection.Clear();
            foreach (PROJECT_Dashboard entity in MainViewModel.Entities)
            {
                BuildProjectsStats(entity);
            }
        }

        void BuildProjectsStats(PROJECT_Dashboard entity)
        {
            BackgroundWorker summaryBackgroundWorker = new BackgroundWorker();
            backgroundWorkerCollection.Add(summaryBackgroundWorker);
            summaryBackgroundWorker.DoWork += summaryBackgroundWorker_DoWork;
            summaryBackgroundWorker.RunWorkerCompleted += summaryBackgroundWorker_RunWorkerCompleted;
            summaryBackgroundWorker.WorkerSupportsCancellation = true;
            summaryBackgroundWorker.RunWorkerAsync(new object[] { entity });
        }

        private static void summaryBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var argumentObject = (object[])e.Argument;
            var project = (PROJECT_Dashboard)argumentObject[0];

            project.BuildStats(false);
            project.RecalculateStats(false);
            
            if (((BackgroundWorker)sender).CancellationPending)
            {
                e.Cancel = true;
                return;
            }
        }

        private void summaryBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RefreshSummary();
        }

        private void RefreshSummary()
        {
            mainThreadDispatcher.BeginInvoke(new Action(() => Redraw?.Invoke()));
        }

        public override void FullRefresh()
        {
            mainThreadDispatcher.BeginInvoke(new Action(() => StoreViewState()));
            InitializeAndLoadEntitiesLoaderDescription();
        }
        #endregion

        #region View Behavior

        public Action Redraw;

        public void RaisePropertyChanged()
        {
            if (Redraw != null)
                mainThreadDispatcher.BeginInvoke(new Action(() => Redraw()));

            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertiesChanged()));
        }

        #endregion

        #region View Properties
        public IEnumerable<USER> MANAGERCollection
        {
            get
            {
                var collection = GetEntities<USER>();
                if (collection != null)
                    collection = collection.Where(x => x.ROLE != null && x.ROLE.ISMANAGER).OrderBy(x => x.NAME);
                return collection;
            }
        }

        public bool CanEditReport()
        {
            if (DisplaySelectedEntities.Count > 0)
                return false;

            return true;
        }

        public bool CanViewReport()
        {
            if (DisplaySelectedEntities.Count > 0)
                return false;

            return true;
        }

        public bool CanEdit()
        {
            if (DisplaySelectedEntity == null)
                return false;

            return true;
        }

        protected IDocumentManagerService DocumentManagerService
        {
            get { return this.GetService<IDocumentManagerService>(); }
        }

        public void Edit()
        {
            if (DisplaySelectedEntity == null)
                return;

            DocumentInfo DocumentInfo = new DocumentInfo(DisplaySelectedEntity.GUID.ToString() + "WorkpackDashboardView",
                DisplaySelectedEntity,
                "WORKPACKDashboardView",
                "[" + DisplaySelectedEntity.Entity.NUMBER + "] WORKPACK Dashboard");

            DocumentManagerService.ShowExistingEntityDocument(DocumentInfo, this);
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "PROJECTDashboardViewModelWrapper"; }
        }

        #endregion

        #region Dispose
        protected override void OnClose(CancelEventArgs e)
        {
            backgroundWorkerCollection.ForEach(x => x.CancelAsync());
            backgroundWorkerCollection.Clear();
            base.OnClose(e);
        }
        #endregion
    }
}