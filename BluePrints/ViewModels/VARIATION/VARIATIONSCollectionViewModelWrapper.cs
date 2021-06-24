using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Helpers;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Document;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Misc;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Misc;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
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
    public class VARIATIONSCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <VARIATION, VARIATIONProjection, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of VARIATION_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static VARIATIONSCollectionViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new VARIATIONSCollectionViewModelWrapper());
        }

        BackgroundWorker variationSummaryBackgroundWorker;
        /// <summary>
        /// Initializes a new instance of the PROJECTViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTViewModel type without the POCO proxy factory.
        /// </summary>
        protected VARIATIONSCollectionViewModelWrapper()
        {
            DoNotAutoRefresh = true;
            variationSummaryBackgroundWorker = new BackgroundWorker();
            variationSummaryBackgroundWorker.DoWork += variationSummaryBackgroundWorker_DoWork;
            variationSummaryBackgroundWorker.WorkerSupportsCancellation = true;
        }
        #region Database Operation

        private PROJECT loadPROJECT;
        private PhaseType phaseType;

        EXO_SubjobCollectionViewModelWrapper exoJobCollectionViewModel;
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private IPrimeroEntitiesUnitOfWork primeroUnitOfWork;
        public bool IsSummaryColumnsVisible { get; set; }
        public bool IsBusy => IsLoading || isSubmitting || isApproving || isSummarizing;
        protected override void resolveParameters(object parameter)
        {
            var project_phasetype_parameter = (DualEntitiesParameter<PROJECT, PhaseTypeClass>) parameter;
            loadPROJECT = project_phasetype_parameter.GetFirstEntity();
            primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(loadPROJECT.OfficeNameForExo == BluePrintsResources.OfficeMontreal).CreateUnitOfWork();
            phaseType = project_phasetype_parameter.GetSecondEntity().phaseType;
            exoJobCollectionViewModel = EXO_SubjobCollectionViewModelWrapper.Create(bluePrintsUnitOfWorkFactory);
            exoJobCollectionViewModel.OnParameterChange(new EntitiesParameter<Data.PROJECT>(loadPROJECT));
            exoJobCollectionViewModel.AlwaysSkipMessage = true;
            exoJobCollectionViewModel.IgnoreCostGroupCostType = true;
            exoJobCollectionViewModel.SetParentViewModel(this);
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINES, BASELINEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.ESTIMATES, ESTIMATEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc);
            loaderCollection.AddLoaderDescription<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.USERS);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.VARIATION_ITEMS, VARIATION_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, COMMODITY_CODEProjectionFunc);
        }

        protected virtual Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>> COMMODITY_CODEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == null);
        }

        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<BASELINE>, IQueryable<BASELINE>> BASELINEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<ESTIMATE>, IQueryable<ESTIMATE>> ESTIMATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> PROGRESSProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == ProgressStatus.Live);
        }

        private Func<IRepositoryQuery<VARIATION_ITEM>, IQueryable<VARIATION_ITEM>> VARIATION_ITEMProjectionFunc()
        {
            return query => query.Where(x => x.VARIATION.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.VARIATIONS);
        }

        protected override Func<IRepositoryQuery<VARIATION>, IQueryable<VARIATIONProjection>> specifyMainViewModelProjection()
        {
            return query => VARIATIONProjectionQueries.VariationProjection_Transformation(query.Where(x => x.PHASE == phaseType), loadPROJECT);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<VARIATIONProjection> entities)
        {
            VARIATION_ITEMSViewModel.SetParentViewModel(this);
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        private bool isSummarizing;
        private int summariesAssigned = 0;
        private void variationSummaryBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            IEnumerable<VARIATIONProjection> entities = (IEnumerable<VARIATIONProjection>)e.Argument;
            if (variationSummaryBackgroundWorker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            isSummarizing = true;
            this.RaisePropertyChanged(x => IsBusy);

            totalSummariesAssignment = entities.Count();
            summariesAssigned = 0;
            foreach (var entity in entities)
            {
                CreateVARIATION_ITEMSViewModelWrapper<BASELINE_ITEMProgress>(entity.Entity, (projections, parentId) => mainThreadDispatcher.BeginInvoke(new Action(() => AssignVariationSummary(projections, parentId))), () => entity.GUID, true);
            }
        }

        private void AssignVariationSummary(IEnumerable<ISupportVariationSummary> variation_projections, object parent_id)
        {
            //When refresh button is pushed too fast, MainViewModel may not be initialized
            if (MainViewModel == null)
                return;

            VARIATIONProjection projection = MainViewModel.Entities.First(x => x.GUID == (Guid)parent_id);
            List<ISupportVariation<IDeliverable>> variations = new List<ISupportVariation<IDeliverable>>();
            foreach(var variationProjection in variation_projections)
            {
                ISupportVariation<IDeliverable> supportVariation = variationProjection as ISupportVariation<IDeliverable>;
                if (supportVariation != null)
                    variations.Add(supportVariation);
            }

            projection.DetailEntities = new ObservableCollection<ISupportVariationSummary>(variation_projections);
            projection.Update();
            refreshTotalsSummary();

            summariesAssigned += 1;
            if(summariesAssigned == totalSummariesAssignment)
            {
                mainThreadDispatcher.BeginInvoke(new Action(() =>
                {
                    isSummarizing = false;
                    this.RaisePropertyChanged(x => IsBusy);
                }));
            }
        }

        private void refreshTotalsSummary()
        {
            if(GridControlService != null)
                mainThreadDispatcher.BeginInvoke(new Action(() => GridControlService.RefreshSummary()));
        }

        public override bool OnBeforeEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender, Guid senderKey, bool isBulkRefresh)
        {
            if (sender != MainViewModel && changedType == typeof(VARIATION))
            {
                Guid guid = (Guid)key;
                VARIATIONProjection variationToRefresh = Entities.FirstOrDefault(x => x.GUID == guid);
                List<VARIATIONProjection> refreshEntities = new List<VARIATIONProjection>();
                refreshEntities.Add(variationToRefresh);
                if (variationToRefresh != null)
                    variationSummaryBackgroundWorker.RunWorkerAsync(refreshEntities);
            }

            return base.OnBeforeEntitiesChanged(key, changedType, messageType, sender, senderKey, isBulkRefresh);
        }

        #region CallBacks
        protected override void OnSelectedEntitiesChanged()
        {
            this.RaisePropertyChanged(x => x.IsSubmitted);
            this.RaisePropertyChanged(x => x.IsApproved);
            this.RaisePropertyChanged(x => x.IsClientApproved);
            base.OnSelectedEntitiesChanged();
        }

        protected override OperationInterceptMode OnBeforeProjectionSaveIsContinue(VARIATIONProjection projection, out bool isNew)
        {
            isNew = false;
            if (LiveBASELINE == null && LiveESTIMATE == null)
                return OperationInterceptMode.SkipOneAndAllDbSaves;

            projection.Entity.GUID_PROJECT = loadPROJECT.GUID;
            projection.Entity.PHASE = phaseType;

            if (projection.Entity.APPROVED != null)
            {
                if (phaseType == PhaseType.Design)
                    projection.Entity.GUID_ORIBASELINE = projection.Entity.GUID_ORIBASELINE ?? LiveBASELINE.GUID;
                else
                    projection.Entity.GUID_ORIBASELINE = projection.Entity.GUID_ORIBASELINE ?? LiveESTIMATE.GUID;
            }
            else
                projection.Entity.GUID_ORIBASELINE = null;


            return base.OnBeforeProjectionSaveIsContinue(projection, out isNew);
        }
        #endregion

        #endregion

        #region View Properties
        Guid? selectedEntityKey = null;
        public override void FullRefresh()
        {
            if (!CanFullRefresh())
                return;

            if (SelectedEntity != null)
                selectedEntityKey = SelectedEntity.GUID;

            IsSummaryColumnsVisible = false;
            this.RaisePropertyChanged(x => x.IsSummaryColumnsVisible);
            ReloadEntitiesCollection();
        }

        private int totalSummariesAssignment = 0;
        private void refreshSummary()
        {
            if (SelectedEntity != null)
                selectedEntityKey = SelectedEntity.GUID;

            variationSummaryBackgroundWorker.RunWorkerAsync(Entities);
        }

        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            base.OnAfterAssignedCallbackAndRaisePropertyChanged();
            restoreSelectedEntity();
        }

        private void restoreSelectedEntity()
        {
            if (selectedEntityKey != null && Entities != null)
            {
                SelectedEntity = Entities.FirstOrDefault(x => x.GUID == selectedEntityKey);
                if (SelectedEntity != null)
                {
                    SelectedEntities.Clear();
                    SelectedEntities.Add(SelectedEntity);
                    selectedEntityKey = null;
                    this.RaisePropertyChanged(x => x.SelectedEntity);
                    this.RaisePropertyChanged(x => x.SelectedEntities);
                }
            }
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            //get { return "VARIATIONSViewModelWrapper" + view_project_specific_affix; }
            get { return "VARIATIONSViewModelWrapper_v2"; }
        }

        private string view_project_specific_affix
        {
            get
            {
                if (loadPROJECT == null)
                    return string.Empty;
                return loadPROJECT.GUID.ToString();
            }
        }

        public CollectionViewModel<BASELINE, BASELINE, Guid, IBluePrintsEntitiesUnitOfWork> BASELINEViewModel
        {
            get
            {
                return (CollectionViewModel<BASELINE, BASELINE, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<BASELINE>();
            }
        }

        public CollectionViewModel<ESTIMATE, ESTIMATE, Guid, IBluePrintsEntitiesUnitOfWork> ESTIMATEViewModel
        {
            get
            {
                return (CollectionViewModel<ESTIMATE, ESTIMATE, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<ESTIMATE>();
            }
        }

        public CollectionViewModel<VARIATION_ITEM, VARIATION_ITEM, Guid, IBluePrintsEntitiesUnitOfWork> VARIATION_ITEMSViewModel
        {
            get
            {
                return (CollectionViewModel<VARIATION_ITEM, VARIATION_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<VARIATION_ITEM>();
            }
        }

        public BASELINE LiveBASELINE
        {
            get
            {
                if (BASELINECollection == null || BASELINECollection.Count() == 0)
                    return null;
                else
                    return BASELINECollection.FirstOrDefault(x => x.STATUS == BaselineStatus.Live);
            }
        }

        public ESTIMATE LiveESTIMATE
        {
            get
            {
                if (ESTIMATECollection == null || BASELINECollection.Count() == 0)
                    return null;
                else
                    return ESTIMATECollection.FirstOrDefault(x => x.STATUS == BaselineStatus.Live);
            }
        }

        public PROGRESS LivePROGRESS
        {
            get
            {
                if (PROGRESSCollection == null || PROGRESSCollection.Count() == 0)
                    return null;
                else
                    return PROGRESSCollection.FirstOrDefault(x => x.STATUS == ProgressStatus.Live);
            }
        }

        public IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection
        {
            get
            {
                return GetEntities<COMMODITY_CODE>();
            }
        }

        public IEnumerable<BASELINE> BASELINECollection
        {
            get
            {
                return GetEntities<BASELINE>();
            }
        }

        public IEnumerable<IAmBaseline> IAmBaselineCollection
        {
            get
            {
                if(phaseType == PhaseType.Design)
                    return BASELINECollection;
                else
                    return ESTIMATECollection;
            }
        }

        public IEnumerable<ESTIMATE> ESTIMATECollection
        {
            get
            {
                var collection = GetEntities<ESTIMATE>();
                return collection;
            }
        }

        public IEnumerable<PROGRESS> PROGRESSCollection
        {
            get
            {
                var collection = GetEntities<PROGRESS>();
                return collection;
            }
        }

        public IEnumerable<USER> USERCollection
        {
            get
            {
                var collection = GetEntities<USER>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);

                return collection;
            }
        }

        public IEnumerable<VARIATION_ITEM> VARIATION_ITEMCollection
        {
            get
            {
                var collection = GetEntities<VARIATION_ITEM>();
                return collection;
            }
        }

        protected IDocumentManagerService DocumentManagerService
        {
            get { return this.GetService<IDocumentManagerService>(); }
        }

        public bool CanEdit()
        {
            if (IsBusy)
                return false;

            if (SelectedEntity == null)
                return false;

            return true;
        }

        public void Edit()
        {
            if (SelectedEntity == null)
                return;

            string view_name = "OffsiteDirectVariationCollectionView";
            string tab_title = "Design Variation";

            DocumentInfo DocumentInfo = new DocumentInfo(SelectedEntity.GUID.ToString(), new DualEntitiesParameter<PROJECT, VARIATION>(loadPROJECT, SelectedEntity.Entity), view_name, "[" + loadPROJECT.NUMBER + "] " + "[" + SelectedEntity.Entity.NAME + "] " + tab_title);
            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }

        const string messageEntityNotSelected = "Please select a variation";
        const string messageBaselineDoesntExists = "Live baseline does not exists";
        const string messageIsApproving = "Variation is in the process of approving, please wait";
        bool isSubmitting = false;
        bool isApproving = false;
        public bool IsSubmitted
        {
            get => isSubmitting || (SelectedEntity == null ? false : SelectedEntity.Entity.SUBMITTED != null);
            set
            {
                string errorMessage = string.Empty;
                if (SelectedEntity == null)
                    errorMessage = messageEntityNotSelected;

                if (LiveBASELINE == null && LiveESTIMATE == null)
                    errorMessage = messageBaselineDoesntExists;

                if (!value && SelectedEntity.Entity.APPROVED != null)
                    errorMessage = "Please unapprove variation before unsubmitting";

                if (errorMessage != string.Empty)
                {
                    MessageBoxService.ShowMessage(errorMessage, "Error", MessageButton.OK, MessageIcon.Exclamation);
                    return;
                }

                isSubmitting = true;
                this.RaisePropertyChanged(x => x.IsSubmitted);
                this.RaisePropertyChanged(x => IsBusy);
                if (value)
                    CreateVARIATION_ITEMSViewModelWrapper<BASELINE_ITEMProgress>(SelectedEntity.Entity, OnVariationSubmit, null, false);
                else if(!value)
                    CreateVARIATION_ITEMSViewModelWrapper<BASELINE_ITEMProgress>(SelectedEntity.Entity, OnVariationUnsubmit, null, false);
            }
        }

        public bool IsApproved
        {
            get => isApproving || (SelectedEntity == null ? false : SelectedEntity.Entity.APPROVED != null);
            set
            {
                string errorMessage = string.Empty;
                if (isApproving)
                    errorMessage = messageIsApproving;

                if (SelectedEntity == null)
                    errorMessage = messageEntityNotSelected;

                if (value && SelectedEntity.Entity.SUBMITTED == null)
                    errorMessage = "Please submit variation before approving";

                if (errorMessage != string.Empty)
                {
                    MessageBoxService.ShowMessage(errorMessage, "Error", MessageButton.OK, MessageIcon.Exclamation);
                    return;
                }

                if (value)
                {
                    if (MessageBoxService.ShowMessage("Are you sure you want to approve " + SelectedEntity.Entity.NAME + "?", "Confirmation", MessageButton.OKCancel) == MessageResult.OK)
                    {
                        isApproving = true;
                        this.RaisePropertyChanged(x => x.IsApproved);
                        this.RaisePropertyChanged(x => IsBusy);
                        CreateVARIATION_ITEMSViewModelWrapper<BASELINE_ITEMProgress>(SelectedEntity.Entity, OnVariationApprove, null, false);
                    }
                }
                else if(!value)
                {
                    if (MessageBoxService.ShowMessage("Are you sure you want to unapprove " + SelectedEntity.Entity.NAME + "?", "Confirmation", MessageButton.OKCancel) == MessageResult.OK)
                    {
                        isApproving = true;
                        this.RaisePropertyChanged(x => x.IsApproved);
                        this.RaisePropertyChanged(x => IsBusy);
                        CreateVARIATION_ITEMSViewModelWrapper<BASELINE_ITEMProgress>(SelectedEntity.Entity, OnVariationUnapprove, null, false);
                    }
                }
            }
        }

        public bool IsClientApproved
        {
            get => SelectedEntity == null ? false : SelectedEntity.Entity.CLIENT_APPROVED != null;
            set
            {
                string errorMessage = string.Empty;
                if (isApproving)
                    errorMessage = messageIsApproving;

                if (SelectedEntity == null)
                    errorMessage = messageEntityNotSelected;

                if (value && SelectedEntity.Entity.SUBMITTED == null)
                    errorMessage = "Please submit variation before marking it as client approved";

                if (value && SelectedEntity.Entity.APPROVED == null)
                    errorMessage = "Please approve variation before marking it as client approved";

                if (errorMessage != string.Empty)
                    MessageBoxService.ShowMessage(errorMessage, "Error", MessageButton.OK, MessageIcon.Exclamation);
                else if (value)
                {
                    SelectedEntity.Entity.CLIENT_APPROVED = DateTime.Now;
                    SelectedEntity.Entity.CLIENT_APPROVEDBY = LoginCredentials.CurrentUserGuid;
                    MainViewModel.Save(SelectedEntity);
                    SelectedEntity.Update();
                }
                else if (!value)
                {
                    SelectedEntity.Entity.CLIENT_APPROVED = null;
                    SelectedEntity.Entity.CLIENT_APPROVEDBY = null;
                    MainViewModel.Save(SelectedEntity);
                    SelectedEntity.Update();
                }
            }
        }

        public bool CanUpdateVariationBudget()
        {
            return !isSubmitting && !isApproving && !IsBusy;
        }

        public void UpdateVariationBudget()
        {
            if (SelectedEntity == null)
            {
                MessageBoxService.ShowMessage("Please select a variation to update", "Error", MessageButton.OK, MessageIcon.Exclamation);
                return;
            }

            if(SelectedEntity.Entity.APPROVED == null)
            {
                MessageBoxService.ShowMessage("Only approved variation budget can be updated", "Error", MessageButton.OK, MessageIcon.Exclamation);
                return;
            }

            CreateVARIATION_ITEMSViewModelWrapper<BASELINE_ITEMProgress>(SelectedEntity.Entity, OnVariationUpdate, null, false);
        }

        #region Variation_Item revision
        public ICollectionViewModelsWrapper<TMainProjectionEntity> CreateVARIATION_ITEMSViewModelWrapper<TMainProjectionEntity>(VARIATION loadVARIATION, Action<IEnumerable<TMainProjectionEntity>, object> onLoadedAction, Func<object> getParentIdFunc, bool supressCompulsoryEntityNotFoundMessage)
            where TMainProjectionEntity : class, IGuidEntityKey, new()
        {
            if (loadPROJECT != null)
            {
                ICollectionViewModelsWrapper<TMainProjectionEntity> variation_itemsViewModelWrapper = (ICollectionViewModelsWrapper<TMainProjectionEntity>)OffsiteDirectVariationCollectionViewModelWrapper.Create();

                variation_itemsViewModelWrapper.SetParentViewModel(this);
                variation_itemsViewModelWrapper.OnEntitiesLoadedCallBack = onLoadedAction;
                variation_itemsViewModelWrapper.OnEntitiesLoadedCallBackRelateParam = getParentIdFunc;
                variation_itemsViewModelWrapper.SuppressNotification = true;
                variation_itemsViewModelWrapper.SupressCompulsoryEntityNotFoundMessage = supressCompulsoryEntityNotFoundMessage;
                variation_itemsViewModelWrapper.InViewModelOnlyMode = true;
                variation_itemsViewModelWrapper.AlwaysSkipMessage = true;
                var baselineSupportParameterObj = variation_itemsViewModelWrapper as ISupportParameter;
                baselineSupportParameterObj.Parameter = new DualEntitiesParameter<PROJECT, VARIATION>(loadPROJECT, loadVARIATION);

                return variation_itemsViewModelWrapper;
            }

            return null;
        }
        #endregion

        private void OnVariationApprove(IEnumerable<BASELINE_ITEMProgress> deliverables, object parameter)
        {
            IBluePrintsEntitiesUnitOfWork bluePrintsUOW = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
            mainThreadDispatcher.BeginInvoke(new Action(() => ReviseBASELINE(deliverables, LiveBASELINE, BASELINEViewModel, bluePrintsUOW, bluePrintsUOW.BASELINE_ITEMS, VariationStages.Approve)));
        }

        private void OnVariationUnapprove(IEnumerable<BASELINE_ITEMProgress> deliverables, object parameter)
        {
            IBluePrintsEntitiesUnitOfWork bluePrintsUOW = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
            mainThreadDispatcher.BeginInvoke(new Action(() => ReviseBASELINE(deliverables, LiveBASELINE, BASELINEViewModel, bluePrintsUOW, bluePrintsUOW.BASELINE_ITEMS, VariationStages.Unapprove)));
        }

        private void OnVariationSubmit(IEnumerable<BASELINE_ITEMProgress> deliverables, object parameter)
        {
            IBluePrintsEntitiesUnitOfWork bluePrintsUOW = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
            mainThreadDispatcher.BeginInvoke(new Action(() => ReviseBASELINE(deliverables, LiveBASELINE, BASELINEViewModel, bluePrintsUOW, bluePrintsUOW.BASELINE_ITEMS, VariationStages.Submit)));
        }

        private void OnVariationUnsubmit(IEnumerable<BASELINE_ITEMProgress> deliverables, object parameter)
        {
            IBluePrintsEntitiesUnitOfWork bluePrintsUOW = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
            mainThreadDispatcher.BeginInvoke(new Action(() => ReviseBASELINE(deliverables, LiveBASELINE, BASELINEViewModel, bluePrintsUOW, bluePrintsUOW.BASELINE_ITEMS, VariationStages.Unsubmit)));
        }

        private void OnVariationUpdate(IEnumerable<BASELINE_ITEMProgress> deliverables, object parameter)
        {
            IBluePrintsEntitiesUnitOfWork bluePrintsUOW = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
            mainThreadDispatcher.BeginInvoke(new Action(() => ReviseBASELINE(deliverables, LiveBASELINE, BASELINEViewModel, bluePrintsUOW, bluePrintsUOW.BASELINE_ITEMS, VariationStages.Update)));
        }

        public bool CanCalculateVariationSummary()
        {
            return !IsBusy;
        }

        public void CalculateVariationSummary()
        {
            IsSummaryColumnsVisible = true;
            this.RaisePropertyChanged(x => x.IsSummaryColumnsVisible);
            refreshSummary();
            //variationSummaryBackgroundWorker.RunWorkerAsync(Entities);
        }

        public bool CanUnapprove()
        {
            if (isApproving)
                return false;

            if (MainViewModel == null || SelectedEntity == null)
                return false;

            if (SelectedEntity.Entity == null)
                return false;

            if (SelectedEntity.Entity.SUBMITTED == null)
                return false;

            return SelectedEntity.Entity.APPROVED != null;
        }

        public void Unapprove()
        {
            if (MessageBoxService.ShowMessage("Are you sure you want to unapprove the selected variation?", "Unapprove Variation", MessageButton.OKCancel) == MessageResult.Cancel)
                return;
            
            isApproving = true;
            CreateVARIATION_ITEMSViewModelWrapper<BASELINE_ITEMProgress>(SelectedEntity.Entity, OnVariationUnapprove, null, false);
        }

        public override string UnifiedRowValidation(VARIATIONProjection projection)
        {
            return string.Empty;
        }


        public override string UnifiedValueValidation(VARIATIONProjection projection, string field_name, object new_value, bool isPaste)
        {
            if (field_name == BindableBase.GetPropertyName(() => new VARIATIONProjection().Entity) + "." + BindableBase.GetPropertyName(() => new VARIATIONProjection().Entity.CLIENT_APPROVED))
            {
                if (projection.Entity.CLIENT_APPROVED == null && new_value != null)
                    return "Please check the client approve button above to client approve this variation, if you wish to edit the date you can do so after client approving it.";
                else if (projection.Entity.CLIENT_APPROVED != null && new_value == null)
                    return "Please use the revert button above to unsubmit this variation.";
            }

            if (field_name == BindableBase.GetPropertyName(() => new VARIATIONProjection().Entity) + "." + BindableBase.GetPropertyName(() => new VARIATIONProjection().Entity.APPROVED))
            {
                if (projection.Entity.APPROVED == null && new_value != null)
                    return "Please check the approve button above to approve this variation, if you wish to edit the date you can do so after approving it.";
                else if(projection.Entity.APPROVED != null && new_value == null)
                    return "Please use the revert button above to unapprove this variation.";
            }

            if (field_name == BindableBase.GetPropertyName(() => new VARIATIONProjection().Entity) + "." + BindableBase.GetPropertyName(() => new VARIATIONProjection().Entity.SUBMITTED))
            {
                if (projection.Entity.SUBMITTED == null && new_value != null)
                    return "Please check the submit button above to submit this variation, if you wish to edit the date you can do so after submitting it.";
                else if (projection.Entity.SUBMITTED != null && new_value == null)
                    return "Please uncheck the submit button above to unsubmit this variation.";
            }

            if (field_name == BindableBase.GetPropertyName(() => new VARIATIONProjection().Entity) + "." + BindableBase.GetPropertyName(() => new VARIATIONProjection().Entity.ADJUSTMENT_TO_BUDGET))
            {
                if(new_value != null)
                {
                    bool adjustBudget = (bool)new_value;
                    if (projection.Entity.APPROVED == null && adjustBudget)
                        return "Variation must be approved, please check the approve button above to approve this variation, if you wish to edit the date you can do so after approving it.";
                }
            }

            if (field_name == BindableBase.GetPropertyName(() => new VARIATIONProjection().Entity) + "." + BindableBase.GetPropertyName(() => new VARIATIONProjection().Entity.NAME))
            {
                if (projection.Entity != null && projection.Entity.NAME != null && projection.Entity.NAME.Length > 50)
                    return "Variation name must be less than 50 characters";
            }

            return string.Empty;
        }

        private string getBaselineRevision(Guid? baseline_guid)
        {
            BASELINE baseline = BASELINECollection.FirstOrDefault(x => x.GUID == baseline_guid);
            if (baseline != null)
                return baseline.REVISION;

            return string.Empty;
        }

        public void ReviseBASELINE<TBaseline, TEntity>(IEnumerable<ISupportVariation<TEntity>> deliverables, TBaseline liveBASELINE, CollectionViewModel<TBaseline, TBaseline, Guid, IBluePrintsEntitiesUnitOfWork> collectionViewModel, IBluePrintsEntitiesUnitOfWork unitOfWork, IRepository<TEntity, Guid> repository, VariationStages variationStage)
            where TBaseline : class, IAmBaseline, new()
            where TEntity : class, IDeliverable, ISupportVariationRevision, new()
        {
            IBluePrintsEntitiesUnitOfWork bluePrintsUnitOfWork = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
            List<VariationApprovalAction<TEntity>> approvalActions = new List<VariationApprovalAction<TEntity>>();
            string variationCode = SelectedEntity.Entity.NAME;

            if (variationStage == VariationStages.Approve || variationStage == VariationStages.Unapprove)
            {
                //run through deliverables first to check for reduction in units more than earned
                foreach (var deliverable in deliverables)
                {
                    VariationApprovalAction<TEntity> deliverableAction = new VariationApprovalAction<TEntity>(deliverable, variationStage);

                    //when approving, discrepancies between earned and negative variation units needs to be resolved
                    if (variationStage == VariationStages.Approve)
                    {
                        if (deliverable.DisplayVariationAction == VariationAction.Append)
                        {
                            if (deliverable.DisplayVariationUnits < 0)
                            {
                                if (deliverable.DisplayVariationUnits < deliverableAction.MaximumReducibleUnits)
                                    approvalActions.Add(deliverableAction);
                            }
                        }
                    }
                    //when unapproving variation units that has already been earned needs to be resolved
                    else
                    {
                        //check whether unapproving current deliverable will cause it's total units to go below earned units
                        decimal reducedUnits = (deliverable.Total_Units - deliverable.DisplayVariationUnits);
                        if (reducedUnits < deliverable.Earned_Units_Total)
                            approvalActions.Add(deliverableAction);
                    }
                }

                if (approvalActions.Count > 0)
                {
                    VariationApprovalViewModel<TEntity> viewModel = VariationApprovalViewModel<TEntity>.CreateViewModel(approvalActions);
                    if (ErrorMessagesDialogService.ShowDialog(MessageButton.OKCancel, string.Empty, "ListVariationApprovalAction", viewModel) == MessageResult.OK)
                    {
                        PROGRESS livePROGRESS = bluePrintsUnitOfWork.PROGRESSES.FirstOrDefault(x => x.STATUS == ProgressStatus.Live && x.GUID_PROJECT == loadPROJECT.GUID);
                        if (livePROGRESS != null)
                        {
                            if(approvalActions.Any(x => x.ReduceEarned))
                            {
                                LoadingScreenManager.ShowLoadingScreen(1);
                                LoadingScreenManager.SetMessage("Creating progress backup");

                                string backupPrefixStr = string.Concat(variationCode, " ", variationStage.ToString());
                                BluePrintsDataUtils.CreateProgressBackup(livePROGRESS, backupPrefixStr);
                                LoadingScreenManager.CloseLoadingScreen();
                            }

                            foreach (VariationApprovalAction<TEntity> approvalAction in approvalActions)
                            {
                                if (approvalAction.ReduceEarned)
                                {
                                    decimal totalUnitsToReduce = 0;
                                    if(variationStage == VariationStages.Approve)
                                        totalUnitsToReduce  = - 1 * approvalAction.Deliverable.DisplayVariationUnits;
                                    else if (variationStage == VariationStages.Unapprove)
                                        totalUnitsToReduce = approvalAction.Deliverable.DisplayVariationUnits;

                                    //code to reduce latest earned units
                                    IEnumerable<PROGRESS_ITEM> deliverablePROGRESSES = livePROGRESS.PROGRESS_ITEM.Where(x => x.GUID_ORIBASEITEM == approvalAction.Deliverable.OriginalEntityKey).OrderByDescending(x => x.EARNED_DATE);
                                    foreach (PROGRESS_ITEM deliverablePROGRESS in deliverablePROGRESSES)
                                    {
                                        if (totalUnitsToReduce > 0)
                                        {
                                            if (deliverablePROGRESS.EarnedUnits >= totalUnitsToReduce)
                                            {
                                                deliverablePROGRESS.EarnedUnits -= totalUnitsToReduce;
                                                totalUnitsToReduce = 0;
                                            }
                                            else
                                            {
                                                totalUnitsToReduce -= deliverablePROGRESS.EarnedUnits;
                                                deliverablePROGRESS.EarnedUnits = 0;
                                            }
                                        }
                                        else
                                            break;
                                    }

                                    bluePrintsUnitOfWork.SaveChanges();
                                }
                                else
                                {
                                    approvalAction.Deliverable.DisplayVariationUnits = approvalAction.MaximumReducibleUnits;
                                }
                            }
                        }
                    }
                    else
                    {
                        isSubmitting = false;
                        isApproving = false;
                        this.RaisePropertyChanged(x => IsBusy);
                        this.RaisePropertyChanged(x => IsSubmitted);
                        this.RaisePropertyChanged(x => IsApproved);
                        LoadingScreenManager.CloseLoadingScreen();
                        return;
                    }
                }
            }

            TBaseline revisedBaseline = null;
            //only revise baseline if variation is approved, this method can be called from submitted which creates a new variation with IsCreateExoVariation == true
            if (variationStage == VariationStages.Approve)
            {
                liveBASELINE.Baseline_Status = BaselineStatus.Superseded;
                collectionViewModel.Save(liveBASELINE);

                revisedBaseline = new TBaseline();
                DataUtils.ShallowCopy(revisedBaseline, liveBASELINE);
                revisedBaseline.GUID = Guid.Empty;
                revisedBaseline.Revision = getNewRevisionNumber(liveBASELINE.Revision);
                revisedBaseline.Baseline_Status = BaselineStatus.Live;
                collectionViewModel.Save(revisedBaseline);

                SelectedEntity.Entity.APPROVED = DateTime.Now;
                SelectedEntity.Entity.APPROVEDBY = LoginCredentials.CurrentUserGuid;
                SelectedEntity.Entity.GUID_ORIBASELINE = liveBASELINE.GUID;
                SelectedEntity.Entity.GUID_BASELINE = revisedBaseline.GUID;
                MainViewModel.Save(SelectedEntity);

                liveBASELINE.GUID = revisedBaseline.GUID;
            }

            List<TEntity> newBASELINE_ITEMS = new List<TEntity>();

            LoadingScreenManager.ShowLoadingScreen(deliverables.Count());
            List<ExoSubJobProjection> exoVariations = new List<ExoSubJobProjection>();

            List<ErrorMessage> errorMessages = new List<ErrorMessage>();
            foreach (var deliverable in deliverables)
            {
                if(variationStage == VariationStages.Unapprove)
                {
                    //BASELINE_ITEM.GUID_VARIATION != null only finds deliverable's that was added through variation, so we don't touch any deliverable that weren't added through variation
                    var deliverableVariationQuery = from BASELINE_ITEM in bluePrintsUnitOfWork.BASELINE_ITEMS
                                                    join BASELINE in bluePrintsUnitOfWork.BASELINES
                                                    on BASELINE_ITEM.GUID_BASELINE equals BASELINE.GUID
                                                    join VARIATION_ITEMS in bluePrintsUnitOfWork.VARIATION_ITEMS
                                                    on BASELINE_ITEM.GUID_ORIGINAL equals VARIATION_ITEMS.GUID_ORIBASEITEM
                                                    join VARIATIONS in bluePrintsUnitOfWork.VARIATIONS
                                                    on VARIATION_ITEMS.GUID_VARIATION equals VARIATIONS.GUID
                                                    where BASELINE.GUID == liveBASELINE.GUID && BASELINE_ITEM.GUID_ORIGINAL == deliverable.GUID_ORIGINAL && BASELINE_ITEM.GUID_VARIATION != null
                                                    select new { BASELINE_ITEM, VARIATIONS };

                    List<BASELINE_ITEM_VARIATIONContainer> deliverableVariations = deliverableVariationQuery.Select(x => new BASELINE_ITEM_VARIATIONContainer() { BASELINE_ITEM = x.BASELINE_ITEM, VARIATION = x.VARIATIONS }).ToList();

                    //only remove this deliverable is only the variation responsible of adding it exists
                    if (deliverableVariations.Count > 0 && deliverableVariations.All(x => x.BASELINE_ITEM.GUID_VARIATION == SelectedEntity.GUID))
                    {
                        BASELINE_ITEM removeDeliverable = deliverableVariations.First().BASELINE_ITEM;
                        BASELINE_ITEM variationDeliverable = bluePrintsUnitOfWork.BASELINE_ITEMS.FirstOrDefault(x => x.GUID_ORIGINAL == deliverable.GUID_ORIGINAL && x.GUID_BASELINE == null && x.GUID_VARIATION == removeDeliverable.GUID_VARIATION);
                        if (variationDeliverable != null)
                        {
                            //copy everything from existing deliverable to variation deliverable before deleting
                            Guid variationDeliverableGuid = variationDeliverable.GUID;
                            DataUtils.ShallowCopy(variationDeliverable, removeDeliverable);
                            variationDeliverable.GUID_BASELINE = null;
                            variationDeliverable.GUID = variationDeliverableGuid;
                        }

                        Messenger.Default.Send(new EntityMessage<BASELINE_ITEM, Guid>(removeDeliverable.GUID, MainViewModel.Key, EntityMessageType.Deleted, this, CurrentHWID, false));
                        bluePrintsUnitOfWork.BASELINE_ITEMS.Remove(removeDeliverable);
                        bluePrintsUnitOfWork.SaveChanges();
                    }

                    //adjust variation units to accomodate earned
                    //decimal updatedMaximumReducibleUnits = getDeliverableUpdatedMaximumReducibleUnits(bluePrintsUnitOfWork, deliverable);
                    ////check whether unapproving current deliverable will cause it's total units to go below earned units
                    //if (deliverable.VARIATION_ITEM.VARIATION_UNITS > updatedMaximumReducibleUnits)
                    //{
                    //    deliverable.VARIATION_ITEM.VARIATION_UNITS = updatedMaximumReducibleUnits;
                    //    VARIATION_ITEMSViewModel.Save(deliverable.VARIATION_ITEM);
                    //}
                }
                //}
                //only revise when new baseline is created
                else if(variationStage == VariationStages.Approve && revisedBaseline != null && revisedBaseline.GUID != Guid.Empty)
                {
                    VARIATION_ITEM updateVARIATION_ITEM = deliverable.VARIATION_ITEM;
                    decimal? variationUnits = null;

                    //deliverable copieed from current baseline
                    //TEntity copyDeliverable = null;
                    if (deliverable.DisplayVariationAction == VariationAction.Cancel)
                    {
                        if (deliverable.Earned_Units_Total == 0)
                            variationUnits = -1 * deliverable.Total_Units;
                        else
                            variationUnits = -1 * (deliverable.Total_Units - deliverable.Earned_Units_Total);

                        //copyDeliverable = new TEntity();
                    }
                    else if (deliverable.DisplayVariationAction == VariationAction.Append)
                    {
                        if (deliverable.DisplayVariationUnits < 0)
                        {
                            decimal currentVariationReductionUnits = -1 * deliverable.DisplayVariationUnits;
                            decimal updatedMaximumReducibleUnits = getDeliverableUpdatedMaximumReducibleUnits(bluePrintsUnitOfWork, deliverable);

                            if (currentVariationReductionUnits > updatedMaximumReducibleUnits)
                                variationUnits = -1 * updatedMaximumReducibleUnits;
                            else
                                variationUnits = deliverable.DisplayVariationUnits;
                        }
                        else
                            variationUnits = deliverable.DisplayVariationUnits;

                    }
                    else if (deliverable.DisplayVariationAction == VariationAction.Add)
                    {
                        variationUnits = deliverable.DisplayVariationUnits;

                        ////when the deliverable is approved by multiple variation and unapproved and re-approved on the variation that defines it as add
                        ////multiple copies exists and we only want to process one copy and save it in historian
                        //if (!repository.Any(x => x.GUID_BASELINE == revisedBaseline.GUID && x.GUID_ORIGINAL == deliverable.GUID_ORIGINAL))
                        //    copyDeliverable = new TEntity();

                        //when the deliverable is approved by multiple variation and reapproved on the variation that defines it as Add, dont' add the deliverable
                        //if(!repository.Any(x => x.GUID_BASELINE == liveBASELINE.GUID && x.GUID_ORIGINAL == deliverable.GUID_ORIGINAL))
                        //{
                        //    TEntity newDeliverable = new TEntity();
                        //    DataUtils.ShallowCopy(newDeliverable, deliverable.Entity);
                        //    newDeliverable.GUID = Guid.Empty;
                        //    newDeliverable.GUID_BASELINE = revisedBaseline.GUID;
                        //    newDeliverable.GUID_VARIATION = DisplaySelectedEntity.GUID;
                        //    repository.Add(newDeliverable);
                        //}
                    }

                    //when the deliverable is approved by multiple variation and reapproved on the variation that defines it as Add, dont' add the deliverable
                    if (!repository.Any(x => x.GUID_BASELINE == revisedBaseline.GUID && x.GUID_ORIGINAL == deliverable.GUID_ORIGINAL))
                    {
                        TEntity newDeliverable = new TEntity();
                        DataUtils.ShallowCopy(newDeliverable, deliverable.Entity);
                        newDeliverable.GUID = Guid.Empty;
                        newDeliverable.GUID_BASELINE = revisedBaseline.GUID;
                        //remember deliverable that was added from variation so that when we delete we don't touch deliverable that weren't added from variation
                        if (deliverable.DisplayVariationAction == VariationAction.Add)
                            newDeliverable.GUID_VARIATION = SelectedEntity.GUID;
                        repository.Add(newDeliverable);
                    }

                    //Save variation units for future viewing
                    if(variationUnits != null)
                    {
                        deliverable.VARIATION_ITEM.VARIATION_UNITS = (decimal)variationUnits;
                        VARIATION_ITEMSViewModel.Save(updateVARIATION_ITEM);
                    }
                }

                //if its purely a scan to determine variation
                if (SelectedEntity.Entity.TYPE == VariationType.External && (deliverable.DisplayVariationAction == VariationAction.Add || (deliverable.DisplayVariationAction == VariationAction.Append)) && variationCode != string.Empty)
                {
                    string subJobCode = deliverable.Subjob_Name;
                    string disciplineCode = deliverable.Discipline_Code;
                    string commodityCode = deliverable.Commodity_Code;

                    decimal exoBudget = variationStage == VariationStages.Approve || variationStage == VariationStages.Update ? deliverable.Forecast_InternalCosts : 0;
                    ExoSubJobProjection exoVariation = exoVariations.FirstOrDefault((x => x.SubJobCode == subJobCode && x.VariationCode == variationCode));
                    if (exoVariation == null)
                    {
                        ExoSubJobProjection newVariationSubJob = new ExoSubJobProjection() { SubJobCode = subJobCode, VariationCode = variationCode, StockCode = BluePrintsResources.VariationStockCode, ExoBudget = exoBudget };
                        //set commodity code convention so that error can be raised natively within model with GetPropertyError
                        newVariationSubJob.PopulateCommodityCodes(COMMODITY_CODECollection);
                        exoVariations.Add(newVariationSubJob);
                    }
                    else
                        exoVariation.ExoBudget += exoBudget;
                }

                LoadingScreenManager.Progress();
            }

            if ((variationStage == VariationStages.Submit || variationStage == VariationStages.Unsubmit))
            {
                if (SelectedEntity.Entity.TYPE == VariationType.External)
                    addVariationJobToExo(exoVariations, variationStage);
                else if (variationStage == VariationStages.Submit)
                    SubmitSelectedEntity();
                else
                    UnsubmitSelectedEntity();
            }
            //approve and unapprove
            else
            {
                unitOfWork.SaveChanges();
                if (variationStage == VariationStages.Unapprove)
                {
                    if (errorMessages.Count > 0)
                    {
                        DialogCollectionViewModel<ErrorMessage> viewModel = DialogCollectionViewModel<ErrorMessage>.Create(errorMessages, "Unapprove cannot continue because of the following error");
                        ErrorMessagesDialogService.ShowDialog(MessageButton.OK, string.Empty, "ListErrorMessages", viewModel);
                    }
                    else
                    {
                        SelectedEntity.Entity.ADJUSTMENT_TO_BUDGET = false;
                        SelectedEntity.Entity.APPROVED = null;
                        SelectedEntity.Entity.APPROVEDBY = null;
                        SelectedEntity.Entity.GUID_ORIBASELINE = null;
                        SelectedEntity.Entity.GUID_BASELINE = null;
                        MainViewModel.Save(SelectedEntity);

                        //this will invoke unsubmit in the setter
                        IsSubmitted = false;
                    }
                }
                else
                    addVariationJobToExo(exoVariations, variationStage);

                if(variationStage != VariationStages.Update)
                    backwardCompatibilityDC_HOURS(liveBASELINE.GUID);

                //because live baseline has been changed, full refresh is required
                if (variationStage == VariationStages.Approve)
                    FullRefresh();
                //else
                //    refreshSummary();
            }

            isSubmitting = false;
            isApproving = false;
            this.RaisePropertyChanged(x => IsBusy);
            this.RaisePropertyChanged(x => IsSubmitted);
            this.RaisePropertyChanged(x => IsApproved);
            #region Send Email
            //isApproving = false;
            //if (addedDeliverables.Count > 0)
            //{
            //    string emailMessage = @"<html> 
            //          <body> 
            //          <p>Variation has been approved for project " + loadPROJECT.NUMBER + " and the following deliverable(s) has been added</p>";

            //    foreach (string addedDeliverable in addedDeliverables)
            //    {
            //        emailMessage += "<p>" + addedDeliverable + "</p>";
            //    }
            //    emailMessage += "</body></html>";
            //    ActiveDirectory.SendEmail(LoginCredentials.CurrentUser.NAME, emailMessage, loadPROJECT.NUMBER + " variation approved");
            //} 
            #endregion
        }

        private decimal getDeliverableUpdatedMaximumReducibleUnits<TEntity>(IBluePrintsEntitiesUnitOfWork bluePrintsUnitOfWork, ISupportVariation<TEntity> deliverable)
            where TEntity : class, IDeliverable, ISupportVariationRevision, new()
        {
            decimal earnedUnits = getDeliverableEarned(bluePrintsUnitOfWork, deliverable);
            return deliverable.Total_Units - earnedUnits;
        }

        private decimal getDeliverableEarned<TEntity>(IBluePrintsEntitiesUnitOfWork bluePrintsUnitOfWork, ISupportVariation<TEntity> deliverable)
            where TEntity : class, IDeliverable, ISupportVariationRevision, new()
        {
            var earnedUnitsQuery = from PROGRESS_ITEM in bluePrintsUnitOfWork.PROGRESS_ITEMS
                                   join PROGRESS in bluePrintsUnitOfWork.PROGRESSES
                                   on PROGRESS_ITEM.GUID_PROGRESS equals PROGRESS.GUID
                                   where PROGRESS.STATUS == ProgressStatus.Live && PROGRESS_ITEM.GUID_ORIBASEITEM == deliverable.OriginalEntityKey
                                   select new { PROGRESS_ITEM };

            var earnedUnitsItems = earnedUnitsQuery.ToList();
            return earnedUnitsItems.Count == 0 ? 0 : earnedUnitsItems.Sum(x => x.PROGRESS_ITEM.EarnedUnits);
        }

        //revise all DC units to comply with old standards of using DC_HOURS for variation instead of dynamically queried
        private void backwardCompatibilityDC_HOURS(Guid baselineGuid)
        {
            IBluePrintsEntitiesUnitOfWork bluePrintsUnitOfWork = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
            BASELINE reviseBASELINE = bluePrintsUnitOfWork.BASELINES.FirstOrDefault(x => x.GUID == baselineGuid);
            if(reviseBASELINE != null)
            {
                foreach(BASELINE_ITEM deliverable in reviseBASELINE.BASELINE_ITEM)
                {
                    var queryVARIATION_UNITS = from VARIATION_ITEM in bluePrintsUnitOfWork.VARIATION_ITEMS
                                               join VARIATION in bluePrintsUnitOfWork.VARIATIONS
                                               on VARIATION_ITEM.GUID_VARIATION equals VARIATION.GUID
                                               where VARIATION.APPROVED != null && VARIATION.ADJUSTMENT_TO_BUDGET == false && VARIATION_ITEM.GUID_ORIBASEITEM == deliverable.GUID_ORIGINAL
                                               select VARIATION_ITEM;

                    List<VARIATION_ITEM> deliverableVariations = queryVARIATION_UNITS.ToList();
                    deliverable.DC_HOURS = deliverableVariations.Sum(x => x.VARIATION_UNITS);
                }

                bluePrintsUnitOfWork.SaveChanges();
            }
        }

        private DevExpress.Mvvm.IDialogService ConfirmationDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("ConfirmationDialogService"); }
        }

        private void addVariationJobToExo(List<ExoSubJobProjection> exoVariationJobs, VariationStages exoInteraction)
        {
            bool isAnyVariationJobsExists = false;
            bool isAnyVariationJobNotExists = false;
            foreach (var exoVariationJob in exoVariationJobs)
            {
                JOBCOST_LINES line = ExoQueries.GetProjectLine(primeroUnitOfWork, loadPROJECT.NUMBER, exoVariationJob, true);
                if (line != null)
                    isAnyVariationJobsExists = true;
                else
                    isAnyVariationJobNotExists = true;
            }

            //when all jobs already exists just continue to submit the current variation
            if (exoInteraction == VariationStages.Submit && !isAnyVariationJobNotExists)
            {
                SubmitSelectedEntity();
                //refresh is required to populate summary
                //refreshSummary();
                return;
            }
            else if(exoInteraction == VariationStages.Unsubmit && !isAnyVariationJobsExists)
            {
                UnsubmitSelectedEntity();
                //refresh is required to populate summary
                //refreshSummary();
                return;
            }

            string message = string.Empty;
            if (exoInteraction == VariationStages.Submit || exoInteraction == VariationStages.Approve)
                message = "Push OK to commit the following variation jobs to EXO, or push cancel and revise added deliverables if the codes are incorrect";
            else if(exoInteraction == VariationStages.Update)
                message = "Push OK to update the following variation jobs to EXO with budget";
            else
                message = "Push OK to remove the following variation jobs from EXO";

            DialogCollectionViewModel<ExoSubJobProjection> viewModel = DialogCollectionViewModel<ExoSubJobProjection>.Create(exoVariationJobs, message);
            if (ConfirmationDialogService.ShowDialog(MessageButton.OKCancel, string.Empty, "ExoVariationConfirmation", viewModel) == MessageResult.OK)
            {
                if(exoInteraction == VariationStages.Submit)
                {
                    IEnumerable<ExoSubJobProjection> newlyAddedProjections = exoJobCollectionViewModel.CommitToExo(exoVariationJobs);
                    if (newlyAddedProjections.Count() > 0)
                    {
                        SubmitSelectedEntity();
                        MessageBoxService.ShowMessage("Variation code(s) pushed to exo");
                    }
                    else
                        MessageBoxService.ShowMessage("Pushed to exo failed, variation is not submitted");
                }
                else if(exoInteraction == VariationStages.Approve || exoInteraction == VariationStages.Update)
                {
                    IEnumerable<ExoSubJobProjection> newlyAddedProjections = exoJobCollectionViewModel.CommitToExo(exoVariationJobs, true);
                    if (newlyAddedProjections.Count() > 0)
                        MessageBoxService.ShowMessage("Variation code(s) pushed to exo with budget");
                    else
                        MessageBoxService.ShowMessage("Pushed to exo failed, variation(s) are not submitted");
                }
                else
                {
                    bool hasRemoved = false;
                    foreach (var exoVariationJob in exoVariationJobs)
                    {
                        JOBCOST_LINES line = ExoQueries.GetProjectLine(primeroUnitOfWork, loadPROJECT.NUMBER, exoVariationJob, true);
                        if (line != null)
                        {
                            hasRemoved = true;
                            primeroUnitOfWork.JOBCOST_LINES.Remove(line);
                        }
                    }

                    if(hasRemoved)
                    {
                        primeroUnitOfWork.SaveChanges();
                        MessageBoxService.ShowMessage("Variation code(s) removed from exo");
                    }

                    UnsubmitSelectedEntity();
                }
            }

            //refreshSummary();
        }

        private string getNewRevisionNumber(string currentRevision)
        {
            string valueToFill = currentRevision;
            int numericFieldLength = 0;
            int? numericIndex = StringFormatUtils.GetNumericIndex(valueToFill, out numericFieldLength);
            if (numericIndex == null)
            {
                return currentRevision.Last().ToString() + 1.ToString();
            }
            else
            {
                string valueToFillStringOnly = valueToFill.Substring(0, valueToFill.Length - numericFieldLength);
                long valueToFillNumberOnly = Int64.Parse(valueToFill.Substring(numericIndex.Value, valueToFill.Length - numericIndex.Value));
                return valueToFillStringOnly + (valueToFillNumberOnly + 1).ToString();
            }
        }

        private void UnsubmitSelectedEntity()
        {
            SelectedEntity.Entity.SUBMITTED = null;
            SelectedEntity.Entity.SUBMITTEDBY = null;
            SelectedEntity.Update();
            MainViewModel.Save(SelectedEntity);
        }

        private void SubmitSelectedEntity()
        {
            SelectedEntity.Entity.SUBMITTED = DateTime.Now;
            SelectedEntity.Entity.SUBMITTEDBY = LoginCredentials.CurrentUserGuid;
            SelectedEntity.Update();
            MainViewModel.Save(SelectedEntity);
        }

        protected override void OnClose(CancelEventArgs e)
        {
            variationSummaryBackgroundWorker.CancelAsync();
            exoJobCollectionViewModel.Dispose();
            base.OnClose(e);
        }

        public enum VariationStages
        {
            Approve,
            Unapprove,
            Submit,
            Unsubmit,
            Update
        }

        public class BASELINE_ITEM_VARIATIONContainer
        {
            public BASELINE_ITEM BASELINE_ITEM { get; set; }
            public VARIATION VARIATION { get; set; }
        }
#endregion
    }
}