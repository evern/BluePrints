using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Document;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the PROJECTS collection view model.
    /// </summary>
    public class PROJECTViewModelWrapper :
        DashboardViewModelWrapper<PROJECT, PROJECT_Dashboard, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of PROJECT_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static PROJECTViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new PROJECTViewModelWrapper());
        }

        /// <summary>
        /// Initializes a new instance of the PROJECTViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTViewModel type without the POCO proxy factory.
        /// </summary>
        protected PROJECTViewModelWrapper()
        {
        }

        #region Database Operation
        private PROJECT loadPROJECT;
        public Action<BASELINECollectionViewModelWrapper> AssignBASELINEDelegates;
        public Action<PROGRESSCollectionViewModelWrapper> AssignPROGRESSDelegates;
        public Action<ESTIMATION_DIRECTCollectionViewModelWrapper> AssignESTIMATION_DIRECTDelegates;
        public Action<AREACollectionViewModelWrapper> AssignAREADelegates;
        public Action<RATECollectionViewModelWrapper> AssignRATEDelegates;

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        public ObservableCollection<Phase_Dashboard> SelectedPhaseDashboards { get; set; }
        public ObservableCollection<Discipline_Dashboard> SelectedDisciplineDashboards { get; set; }
        public ObservableCollection<Commodity_Dashboard> SelectedCommodityDashboards { get; set; }
        protected override void InitializeParameters(object parameter)
        {
            var PROJECTParameter =
                (EntitiesParameter<PROJECT>) parameter;
            loadPROJECT = PROJECTParameter.GetEntity();

            SelectedPhaseDashboards = new ObservableCollection<Phase_Dashboard>();
            SelectedDisciplineDashboards = new ObservableCollection<Discipline_Dashboard>();
            SelectedCommodityDashboards = new ObservableCollection<Commodity_Dashboard>();
            SelectedPhaseDashboards.CollectionChanged += SelectedPhaseDashboard_CollectionChanged;
            SelectedDisciplineDashboards.CollectionChanged += SelectedDisciplineDashboard_CollectionChanged;
            SelectedCommodityDashboards.CollectionChanged += SelectedCommodityDashboard_CollectionChanged;
        }

        private void SelectedPhaseDashboard_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            IEnumerable<SummaryStats> entitiesSummary = SelectedPhaseDashboards.Select(x => (SummaryStats)x.Stats);
            SummaryEntity.Stats = new SummaryStats(entitiesSummary);
            this.RaisePropertyChanged(x => x.SummaryEntity);
        }

        private void SelectedDisciplineDashboard_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            IEnumerable<SummaryStats> entitiesSummary = SelectedDisciplineDashboards.Select(x => (SummaryStats)x.Stats);
            SummaryEntity.Stats = new SummaryStats(entitiesSummary);
            this.RaisePropertyChanged(x => x.SummaryEntity);
        }

        private void SelectedCommodityDashboard_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            IEnumerable<SummaryStats> entitiesSummary = SelectedCommodityDashboards.Select(x => (SummaryStats)x.Stats);
            SummaryEntity.Stats = new SummaryStats(entitiesSummary);
            this.RaisePropertyChanged(x => x.SummaryEntity);
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINES, BASELINEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.ESTIMATION_DIRECTS, ESTIMATION_DIRECTProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESS_ITEMS, PROGRESS_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.VARIATIONS, VARIATIONProjectionFunc);
            loaderCollection.AddLoaderDescription<DELIVERABLES_STATUS, DELIVERABLES_STATUS, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DELIVERABLES_STATUSES);
            loaderCollection.AddLoaderDescription<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.USERS);

            InvokeEntitiesLoaderDescriptionLoading();
        }

        private Func<IRepositoryQuery<BASELINE>, IQueryable<BASELINE>> BASELINEProjectionFunc()
        {
            return query => query.Where(x => x.STATUS == BaselineStatus.Live && x.GUID_PROJECT == loadPROJECT.GUID).OrderBy(x => x.REVISION);
        }

        private Func<IRepositoryQuery<ESTIMATION_DIRECT>, IQueryable<ESTIMATION_DIRECT>> ESTIMATION_DIRECTProjectionFunc()
        {
            return query => query.Where(x => x.STATUS == EstimationStatus.Live && x.GUID_PROJECT == loadPROJECT.GUID).OrderBy(x => x.REVISION);
        }

        private Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> PROGRESSProjectionFunc()
        {
            return query => query.Where(x => x.STATUS == ProgressStatus.Live && x.GUID_PROJECT == loadPROJECT.GUID).OrderBy(x => x.STATUS);
        }

        private Func<IRepositoryQuery<PROGRESS_ITEM>, IQueryable<PROGRESS_ITEM>> PROGRESS_ITEMProjectionFunc()
        {
            return
                query =>
                    query.Where(
                        x => x.PROGRESS.STATUS == ProgressStatus.Live && x.PROGRESS.PROJECT.GUID == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<VARIATION>, IQueryable<VARIATION>> VARIATIONProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).OrderBy(x => x.NAME);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).OrderBy(x => x.INTERNAL_NUM);
        }

        private Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).OrderBy(x => x.RATE1);
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.PROJECTS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT_Dashboard>>
            ConstructMainViewModelProjection()
        {
            var BASELINE = loaderCollection.GetObject<BASELINE>();
            var ESTIMATION_DIRECT = loaderCollection.GetObject<ESTIMATION_DIRECT>();
            var PROGRESSES = loaderCollection.GetCollection<PROGRESS>();
            var PROGRESS_ITEMS = loaderCollection.GetCollection<PROGRESS_ITEM>();
            var RATES = loaderCollection.GetCollection<RATE>();
            var VARIATIONS = loaderCollection.GetCollection<VARIATION>();

            List<PROJECT_Dashboard> project_dashboards = new List<PROJECT_Dashboard>();
            PROJECT_Dashboard project_dashboard = DashboardQueries.Single_Project_DashboardTransformation(loadPROJECT, BASELINE, ESTIMATION_DIRECT, PROGRESSES, PROGRESS_ITEMS, RATES, VARIATIONS);

            project_dashboards.Add(project_dashboard);
            return query => project_dashboards.AsQueryable();
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<PROJECT_Dashboard> entities)
        {
            MainViewModel.AdditionalValidateCellCallBack = AdditionalCellValidation;
            MainViewModel.CanFillDownCallBack = CanFillDownCallBack;
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        protected override bool OnMainViewModelLoaded(IEnumerable<PROJECT_Dashboard> entities)
        {
            MainViewModel =
                (CollectionViewModel<PROJECT, PROJECT_Dashboard, Guid, IBluePrintsEntitiesUnitOfWork>)
                mainEntityLoaderDescription.GetViewModel();

            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertiesChanged()));
            MainViewModel.SetParentViewModel(this);

            DisplaySelectedEntities_CollectionChanged();

            if(entities.Count() > 0)
            {
                this.DisplaySelectedEntity = entities.First();
                BackgroundWorker summaryBackgroundWorker = new BackgroundWorker();
                summaryBackgroundWorker.DoWork += summaryBackgroundWorker_DoWork;
                summaryBackgroundWorker.WorkerSupportsCancellation = true;
                summaryBackgroundWorker.RunWorkerAsync(new object[] { entities.First() });
            }

            base.OnMainViewModelLoaded(entities);
            return true;
        }

        private void summaryBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var argumentObject = (object[])e.Argument;
            var project = (PROJECT_Dashboard)argumentObject[0];

            if(project != null)
            {
                project.BuildStats(false);
                project.RecalculateStats(false);
                project.Phase_Dashboards = DashboardQueries.Construct_Phase_Dashboards((ProjectSummaryStats)project.Stats);
                project.Update();
            }

            if (((BackgroundWorker)sender).CancellationPending)
            {
                e.Cancel = true;
                return;
            }
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

        protected IOpenFileDialogService OpenFileDialogService
        {
            get { return this.GetService<IOpenFileDialogService>(); }
        }

        public bool CanFillDownCallBack(IEnumerable<PROJECT_Dashboard> selectedEntities, GridMenuInfo info)
        {
            if (info.Column.FieldName == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_KICKOFF) ||
                info.Column.FieldName == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_CLOSEOUT) ||
                info.Column.FieldName == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_SIDREPORT))
                return false;

            return true;
        }

        protected override void CellValueAnyRowChanging(CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_KICKOFF) ||
                   e.Column.FieldName == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_CLOSEOUT) ||
                   e.Column.FieldName == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_SIDREPORT))
            {
                MainViewModel.EntitiesUndoRedoManager.PauseActionId(); //Unpaused in existingRowAddUndoAndSave
                PROJECT_Dashboard activePROJECT = (PROJECT_Dashboard)e.Row;
                ProjectDocumentStatus newValue = (ProjectDocumentStatus)e.Value;
                if (newValue == ProjectDocumentStatus.Yes)
                {
                    OpenFileDialogService.Filter = "PDF (*.PDF)|*.PDF";
                    bool DialogResult;

                    DialogResult = OpenFileDialogService.ShowDialog();
                    if (DialogResult)
                    {
                        string fullPath = OpenFileDialogService.File.GetFullName();
                        if (e.Column.FieldName == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_KICKOFF))
                        {
                            MainViewModel.EntitiesUndoRedoManager.AddUndo(activePROJECT, BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_KICKOFF_PATH), null, fullPath, EntityMessageType.Changed);
                            activePROJECT.Entity.DOC_KICKOFF_PATH = fullPath;
                        }
                        else if (e.Column.FieldName == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_CLOSEOUT))
                        {
                            MainViewModel.EntitiesUndoRedoManager.AddUndo(activePROJECT, BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_CLOSEOUT_PATH), null, fullPath, EntityMessageType.Changed);
                            activePROJECT.Entity.DOC_CLOSEOUT_PATH = fullPath;
                        }
                        else
                        {
                            MainViewModel.EntitiesUndoRedoManager.AddUndo(activePROJECT, BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_SIDREPORT), null, fullPath, EntityMessageType.Changed);
                            activePROJECT.Entity.DOC_SIDREPORT_PATH = fullPath;
                        }
                    }
                }
                else
                {
                    if (e.Column.FieldName == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_KICKOFF))
                    {
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(activePROJECT, BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_KICKOFF_PATH), activePROJECT.Entity.DOC_KICKOFF_PATH, null, EntityMessageType.Changed);
                        activePROJECT.Entity.DOC_KICKOFF_PATH = null;
                    }
                    else if (e.Column.FieldName == BindableBase.GetPropertyName(() => new PROJECT().DOC_CLOSEOUT))
                    {
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(activePROJECT, BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_CLOSEOUT_PATH), activePROJECT.Entity.DOC_CLOSEOUT_PATH, null, EntityMessageType.Changed);
                        activePROJECT.Entity.DOC_CLOSEOUT_PATH = null;
                    }
                    else
                    {
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(activePROJECT, BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_SIDREPORT_PATH), activePROJECT.Entity.DOC_SIDREPORT_PATH, null, EntityMessageType.Changed);
                        activePROJECT.Entity.DOC_SIDREPORT_PATH = null;
                    }
                }

                e.Handled = true;
            }

            if (e.Column.FieldName == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().STATUS))
            {
                PROJECT_Dashboard activePROJECT = (PROJECT_Dashboard)e.Row;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                BluePrintsContextHelper.AsyncRefreshDeliverablesDataPointsByProject(activePROJECT.Entity.NUMBER);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }

            base.CellValueAnyRowChanging(e);
        }

        public async void Refresh_From_P6()
        {
            LoadingScreenManager.ShowLoadingScreen(1);
            await BluePrintsContextHelper.RefreshDeliverablesDataPointsByProject(loadPROJECT.NUMBER);
            LoadingScreenManager.Progress();
            FullRefresh();
        }

        private void AdditionalCellValidation(GridCellValidationEventArgs e)
        {
            PROJECT_Dashboard activePROJECT = (PROJECT_Dashboard)e.Row;
            string missingPathErrorString = "Path not selected";
            bool isError = false;

            if (e.Column.FieldName == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_KICKOFF))
            {
                ProjectDocumentStatus newValue = (ProjectDocumentStatus)e.Value;
                if (newValue == ProjectDocumentStatus.Yes && activePROJECT.Entity.DOC_KICKOFF_PATH == null)
                {
                    isError = true;
                }
            }
            else if (e.Column.FieldName == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_CLOSEOUT))
            {
                ProjectDocumentStatus newValue = (ProjectDocumentStatus)e.Value;
                if (newValue == ProjectDocumentStatus.Yes && activePROJECT.Entity.DOC_CLOSEOUT_PATH == null)
                {
                    isError = true;
                }
            }
            else if (e.Column.FieldName == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_SIDREPORT))
            {
                ProjectDocumentStatus newValue = (ProjectDocumentStatus)e.Value;
                if (newValue == ProjectDocumentStatus.Yes && activePROJECT.Entity.DOC_SIDREPORT_PATH == null)
                {
                    isError = true;
                }
            }

            if (isError)
            {
                e.IsValid = false;
                e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Critical;
                e.ErrorContent = missingPathErrorString;
            }
        }
        #endregion

        #region View Properties
        private BASELINECollectionViewModelWrapper baselineViewModel;

        public BASELINECollectionViewModelWrapper BASELINEViewModel
        {
            get
            {
                if (baselineViewModel == null && loadPROJECT != null)
                {
                    baselineViewModel = BASELINECollectionViewModelWrapper.Create();
                    baselineViewModel.SetParentViewModel(this);
                    var baselineSupportParameterObj = baselineViewModel as ISupportParameter;
                    baselineSupportParameterObj.Parameter = new EntitiesParameter<PROJECT>(loadPROJECT);
                    AssignBASELINEDelegates?.Invoke(baselineViewModel);
                }

                return baselineViewModel;
            }
        }

        private PROGRESSCollectionViewModelWrapper progressViewModel;

        public PROGRESSCollectionViewModelWrapper PROGRESSViewModel
        {
            get
            {
                if (progressViewModel == null && loadPROJECT != null)
                {
                    progressViewModel = PROGRESSCollectionViewModelWrapper.Create();
                    progressViewModel.SetParentViewModel(this);
                    var baselineSupportParameterObj = progressViewModel as ISupportParameter;
                    baselineSupportParameterObj.Parameter = new EntitiesParameter<PROJECT>(loadPROJECT);
                    AssignPROGRESSDelegates?.Invoke(progressViewModel);
                }

                return progressViewModel;
            }
        }

        private AREACollectionViewModelWrapper areaViewModel;

        public AREACollectionViewModelWrapper AREAViewModel
        {
            get
            {
                if (areaViewModel == null && loadPROJECT != null)
                {
                    areaViewModel = AREACollectionViewModelWrapper.Create();
                    areaViewModel.SetParentViewModel(this);
                    var baselineSupportParameterObj = areaViewModel as ISupportParameter;
                    baselineSupportParameterObj.Parameter = new EntitiesParameter<PROJECT>(loadPROJECT);
                    AssignAREADelegates?.Invoke(areaViewModel);
                }

                return areaViewModel;
            }
        }

        private RATECollectionViewModelWrapper rateViewModel;

        public RATECollectionViewModelWrapper RATEViewModel
        {
            get
            {
                if (rateViewModel == null && loadPROJECT != null)
                {
                    rateViewModel = RATECollectionViewModelWrapper.Create();
                    rateViewModel.SetParentViewModel(this);
                    var baselineSupportParameterObj = rateViewModel as ISupportParameter;
                    baselineSupportParameterObj.Parameter = new EntitiesParameter<PROJECT>(loadPROJECT);
                    AssignRATEDelegates?.Invoke(rateViewModel);
                }

                return rateViewModel;
            }
        }

        private ESTIMATION_DIRECTCollectionViewModelWrapper estimationDirectViewModel;

        public ESTIMATION_DIRECTCollectionViewModelWrapper ESTIMATION_DIRECTViewModel
        {
            get
            {
                if (estimationDirectViewModel == null && loadPROJECT != null)
                {
                    estimationDirectViewModel = ESTIMATION_DIRECTCollectionViewModelWrapper.Create();
                    estimationDirectViewModel.SetParentViewModel(this);
                    var baselineSupportParameterObj = estimationDirectViewModel as ISupportParameter;
                    baselineSupportParameterObj.Parameter = new EntitiesParameter<PROJECT>(loadPROJECT);
                    AssignESTIMATION_DIRECTDelegates?.Invoke(estimationDirectViewModel);
                }

                return estimationDirectViewModel;
            }
        }

        public bool CanEditReport()
        {
            if (MainViewModel == null || MainViewModel.Entities.Count == 0)
                return false;

            return true;
        }

        public bool CanViewReport()
        {
            if (MainViewModel == null || MainViewModel.Entities.Count == 0)
                return false;

            return true;
        }

        public bool CanEdit()
        {
            if (DisplaySelectedEntity == null)
                return false;

            return true;
        }

        public override void FullRefresh()
        {
            mainThreadDispatcher.BeginInvoke(new Action(() => StoreViewState()));
            InitializeAndLoadEntitiesLoaderDescription();
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
            get { return "PROJECTViewModelWrapper"; }
        }

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

        protected override bool isMasterDetailView => true;
        #endregion
    }
}