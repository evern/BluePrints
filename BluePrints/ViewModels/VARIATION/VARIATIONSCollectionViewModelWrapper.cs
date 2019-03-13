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
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
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
        protected override void resolveParameters(object parameter)
        {
            var project_phasetype_parameter = (DualEntitiesParameter<PROJECT, PhaseTypeClass>) parameter;
            loadPROJECT = project_phasetype_parameter.GetFirstEntity();
            phaseType = project_phasetype_parameter.GetSecondEntity().phaseType;
            exoJobCollectionViewModel = EXO_SubjobCollectionViewModelWrapper.Create(bluePrintsUnitOfWorkFactory);
            exoJobCollectionViewModel.OnParameterChange(new EntitiesParameter<Data.PROJECT>(loadPROJECT));
            exoJobCollectionViewModel.AlwaysSkipMessage = true;
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
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<VARIATION>, IQueryable<VARIATIONProjection>> specifyMainViewModelProjection()
        {
            return query => VARIATIONProjectionQueries.VariationProjection_Transformation(query.Where(x => x.PHASE == phaseType), loadPROJECT);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<VARIATIONProjection> entities)
        {
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = OnBeforeEntitySaved;
            MainViewModel.IsContinueSaveCallBack = BeforeSaveValidation;
            VARIATION_ITEMSViewModel.SetParentViewModel(this);
            MainViewModel.SetParentViewModel(this);
            variationSummaryBackgroundWorker.RunWorkerAsync(entities);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        private void variationSummaryBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            IEnumerable<VARIATIONProjection> entities = (IEnumerable<VARIATIONProjection>)e.Argument;
            if (variationSummaryBackgroundWorker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

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
            isApproving = false;
            //BackgroundRefresh();
        }

        #region CallBacks
        public bool BeforeSaveValidation(VARIATIONProjection entity, bool isNewEntity)
        {
            if (LiveBASELINE == null && LiveESTIMATE == null)
                return false;

            return true;
        }

        protected override void OnBeforeApplyProjectionPropertiesToEntity(VARIATIONProjection projectionEntity, VARIATION entity)
        {
            if (entity.CREATED.Date.Year == 1)
            {
                //Although EF convention will generate this but we require it immediately in the view
                projectionEntity.Entity.CREATEDBY = LoginCredentials.CurrentUserGuid;
            }

            base.OnBeforeApplyProjectionPropertiesToEntity(projectionEntity, entity);
        }

        public bool OnBeforeEntitySaved(VARIATIONProjection entity)
        {
            entity.Entity.GUID_PROJECT = loadPROJECT.GUID;
            entity.Entity.PHASE = phaseType;

            if (entity.Entity.APPROVED != null)
            {
                if (phaseType == PhaseType.Design)
                    entity.Entity.GUID_ORIBASELINE = entity.Entity.GUID_ORIBASELINE ?? LiveBASELINE.GUID;
                else
                    entity.Entity.GUID_ORIBASELINE = entity.Entity.GUID_ORIBASELINE ?? LiveESTIMATE.GUID;
            }
            else
                entity.Entity.GUID_ORIBASELINE = null;

            return true;
        }
        #endregion

        #endregion

        #region View Properties
        Guid? selectedEntityKey = null;
        public override void FullRefresh()
        {
            if (DisplaySelectedEntity != null)
                selectedEntityKey = DisplaySelectedEntity.GUID;

            ReloadEntitiesCollection();
        }

        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            base.OnAfterAssignedCallbackAndRaisePropertyChanged();
            if(selectedEntityKey != null && DisplayEntities != null)
            {
                DisplaySelectedEntity = DisplayEntities.FirstOrDefault(x => x.GUID == selectedEntityKey);
                if (DisplaySelectedEntity != null)
                {
                    DisplaySelectedEntities.Clear();
                    DisplaySelectedEntities.Add(DisplaySelectedEntity);
                    selectedEntityKey = null;
                    this.RaisePropertyChanged(x => x.DisplaySelectedEntity);
                    this.RaisePropertyChanged(x => x.DisplaySelectedEntities);
                }
            }
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            //get { return "VARIATIONSViewModelWrapper" + view_project_specific_affix; }
            get { return "VARIATIONSViewModelWrapper_v1"; }
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
            if (DisplaySelectedEntity == null)
                return false;

            return true;
        }

        public void Edit()
        {
            if (DisplaySelectedEntity == null)
                return;

            string view_name = "OffsiteDirectVariationCollectionView";
            string tab_title = "Design Variation";

            DocumentInfo DocumentInfo = new DocumentInfo(DisplaySelectedEntity.GUID.ToString(), new DualEntitiesParameter<PROJECT, VARIATION>(loadPROJECT, DisplaySelectedEntity.Entity), view_name, "[" + loadPROJECT.NUMBER + "] " + "[" + DisplaySelectedEntity.Entity.NAME + "] " + tab_title);
            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }

        /// <summary>
        /// Determines whether an entities can be Submitd
        /// Since CollectionViewModelBase is a POCO view model, this method will be used as a CanExecute callback for SubmitCommand.
        /// </summary>
        /// <param name="projectionEntity">Entities to Submit.</param>
        public bool CanSubmit()
        {
            if (DisplaySelectedEntity == null)
                return false;

            if (LiveBASELINE == null && LiveESTIMATE == null)
                return false;

            if (DisplaySelectedEntity.Entity == null)
                return false;

            if (DisplaySelectedEntity.Entity.SUBMITTED != null)
                return false;

            if (DisplaySelectedEntity.Entity.APPROVED != null)
                return false;

            return true;
        }

        public bool CanUnsubmit()
        {
            if (DisplaySelectedEntity == null)
                return false;

            if (LiveBASELINE == null && LiveESTIMATE == null)
                return false;

            if (DisplaySelectedEntity.Entity == null)
                return false;

            if (DisplaySelectedEntity.Entity.SUBMITTED == null)
                return false;

            if (DisplaySelectedEntity != null && DisplaySelectedEntity.Entity.APPROVED != null)
                return false;

            return true;
        }

        public void Submit()
        {
            CreateVARIATION_ITEMSViewModelWrapper<BASELINE_ITEMProgress>(DisplaySelectedEntity.Entity, OnVariationSubmit, null, false);
        }

        public void Unsubmit()
        {
            CreateVARIATION_ITEMSViewModelWrapper<BASELINE_ITEMProgress>(DisplaySelectedEntity.Entity, OnVariationUnsubmit, null, false);
        }

        /// <summary>
        /// Determines whether an entities can be approved
        /// Since CollectionViewModelBase is a POCO view model, this method will be used as a CanExecute callback for ApproveCommand.
        /// </summary>
        /// <param name="projectionEntity">Entities to approve.</param>
        public bool CanApprove()
        {
            if (isApproving)
                return false;

            if (MainViewModel == null || DisplaySelectedEntity == null)
                return false;

            if (DisplaySelectedEntity.Entity == null)
                return false;

            if (DisplaySelectedEntity.Entity.SUBMITTED == null)
                return false;

            if (DisplaySelectedEntity.Entity.APPROVED != null)
                return false;

            return true;
        }

        bool isApproving;
        /// <summary>
        /// Approves an entity.
        /// Since CollectionViewModelBase is a POCO view model, an the instance of this class will also expose the ApproveCommand property that can be used as a binding source in views.
        /// </summary>
        /// <param name="projectionEntity">An entity to approve.</param>
        public void Approve()
        {
            var errorMessage = string.Empty;
            if (DisplaySelectedEntity == null)
                errorMessage = "Nothing within variation to approve";
            else if (loadPROJECT == null)
                errorMessage = "Project not found";
            else if (LivePROGRESS == null)
                errorMessage = "Live progress not found";
            else
            {
                if (phaseType == PhaseType.Design && LiveBASELINE == null)
                    errorMessage = "Live baseline not found";
                else if(phaseType == PhaseType.Construct && LiveESTIMATE == null)
                    errorMessage = "Live estimate not found";
            }
            
            if (errorMessage != string.Empty)
            {
                MessageBoxService.ShowMessage(errorMessage);
                return;
            }

            if(MessageBoxService.ShowMessage("Are you sure you want to approve variation " + DisplaySelectedEntity.Entity.NAME + "?", "Approve", MessageButton.OKCancel) == MessageResult.Cancel)
                return;

            isApproving = true;
            CreateVARIATION_ITEMSViewModelWrapper<BASELINE_ITEMProgress>(DisplaySelectedEntity.Entity, OnVariationApprove, null, false);
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

        public bool CanUnapprove()
        {
            if (isApproving)
                return false;

            if (MainViewModel == null || DisplaySelectedEntity == null)
                return false;

            if (DisplaySelectedEntity.Entity == null)
                return false;

            if (DisplaySelectedEntity.Entity.SUBMITTED == null)
                return false;

            return DisplaySelectedEntity.Entity.APPROVED != null;
        }

        public void Unapprove()
        {
            if (MessageBoxService.ShowMessage("Are you sure you want to unapprove the selected variation?", "Unapprove Variation", MessageButton.OKCancel) == MessageResult.Cancel)
                return;

            //if (MessageBoxService.ShowMessage("This will revert from the latest variation onwards.\nCurrent live baseline will be superseded with suffix '_Reverted'\nAre you sure you want to revert changes to the baseline?", "Revert Baseline", MessageButton.OKCancel) == MessageResult.Cancel)
            //    return;

            //string liveBaselineRevision = LiveBASELINE.Revision;

            //string lastBaselineRevision = string.Empty;
            //string valueToFill = liveBaselineRevision;
            //int numericFieldLength = 0;
            //int? numericIndex = StringFormatUtils.GetNumericIndex(valueToFill, out numericFieldLength);
            //if (numericIndex == null)
            //    lastBaselineRevision = ((char)(liveBaselineRevision.Last() - 1)).ToString();
            //else
            //{
            //    string valueToFillStringOnly = valueToFill.Substring(0, valueToFill.Length - numericFieldLength);
            //    long valueToFillNumberOnly = Int64.Parse(valueToFill.Substring(numericIndex.Value, valueToFill.Length - numericIndex.Value));
            //    if(valueToFillNumberOnly == 1)
            //        lastBaselineRevision = ((char)(valueToFillStringOnly.Last())).ToString();
            //    else
            //        lastBaselineRevision = valueToFillStringOnly + (valueToFillNumberOnly - 1).ToString();
            //}

            //VARIATIONProjection lastVariation = MainViewModel.Entities.FirstOrDefault(x => getBaselineRevision(x.Entity.GUID_BASELINE) == liveBaselineRevision);
            //if(lastVariation == null)
            //{
            //    MessageBoxService.ShowMessage("Last variation revision does not match latest baseline revision");
            //    return;
            //}

            //BASELINE lastBaseline = BASELINECollection.FirstOrDefault(x => x.REVISION == lastBaselineRevision);
            //if(lastBaseline == null)
            //{
            //    MessageBoxService.ShowMessage("Last baseline not found");
            //    return;
            //}

            //BASELINE live_baseline = new BASELINE();
            //DataUtils.ShallowCopy(live_baseline, LiveBASELINE);
            //live_baseline.REVISION = live_baseline.REVISION + "_Reverted";
            //live_baseline.STATUS = BaselineStatus.Superseded;
            //BASELINEViewModel.Save(live_baseline);

            //lastBaseline.STATUS = BaselineStatus.Live;
            //BASELINEViewModel.Save(lastBaseline);

            isApproving = true;
            CreateVARIATION_ITEMSViewModelWrapper<BASELINE_ITEMProgress>(DisplaySelectedEntity.Entity, OnVariationUnapprove, null, false);
        }

        public override string UnifiedRowValidation(VARIATIONProjection projection)
        {
            return string.Empty;
        }


        public override string UnifiedValueValidation(VARIATIONProjection projection, string field_name, object new_value)
        {
            if (field_name == BindableBase.GetPropertyName(() => new VARIATIONProjection().Entity) + "." + BindableBase.GetPropertyName(() => new VARIATIONProjection().Entity.APPROVED))
            {
                if (projection.Entity.APPROVED == null && new_value != null)
                    return "Please use the button above to approve this variation, if you wish to edit the date you can do so after approving it.";
                else if(projection.Entity.APPROVED != null && new_value == null)
                    return "Please use the revert button above to unapprove this variation.";
            }

            if (field_name == BindableBase.GetPropertyName(() => new VARIATIONProjection().Entity) + "." + BindableBase.GetPropertyName(() => new VARIATIONProjection().Entity.SUBMITTED))
            {
                if (projection.Entity.SUBMITTED == null && new_value != null)
                    return "Please use the button above to submit this variation, if you wish to edit the date you can do so after submitting it.";
                else if (projection.Entity.SUBMITTED != null && new_value == null)
                    return "Please use the revert button above to unsubmit this variation.";
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
            string variationCode = DisplaySelectedEntity.Entity.NAME;
            TBaseline historianBASELINE = null;
            //only revise baseline if variation is approved, this method can be called from submitted which creates a new variation with IsCreateExoVariation == true
            if (variationStage == VariationStages.Approve)
            {
                historianBASELINE = new TBaseline();
                DataUtils.ShallowCopy(historianBASELINE, liveBASELINE);
                historianBASELINE.GUID = Guid.Empty;
                historianBASELINE.Revision = DisplaySelectedEntity.Entity.NAME;
                historianBASELINE.Baseline_Status = BaselineStatus.Variation;
                collectionViewModel.Save(historianBASELINE);

                DisplaySelectedEntity.Entity.APPROVED = DateTime.Now;
                DisplaySelectedEntity.Entity.GUID_ORIBASELINE = liveBASELINE.GUID;
                DisplaySelectedEntity.Entity.GUID_BASELINE = historianBASELINE.GUID;
                MainViewModel.Save(DisplaySelectedEntity);
            }
            else if(variationStage == VariationStages.Unapprove)
            {                
                //tracking entities are attached to a baseline where snapshot was taken at the time of approval
                //it is used for future reference on variation changes, so that current changes to deliverable's via other variation won't affect how it was recognised
                List<TEntity> historianDeliverables = repository.Where(x => x.GUID_BASELINE == DisplaySelectedEntity.Entity.GUID_BASELINE && x.GUID_VARIATION == DisplaySelectedEntity.GUID).ToList();
                foreach (TEntity historianDeliverable in historianDeliverables)
                {
                    repository.Remove(historianDeliverable);
                }

                TBaseline historianBaseline = collectionViewModel.Entities.FirstOrDefault(x => x.GUID == DisplaySelectedEntity.Entity.GUID_BASELINE);
                if(historianBaseline != null)
                {
                    collectionViewModel.Delete(historianBaseline);
                }

                DisplaySelectedEntity.Entity.APPROVED = null;
                DisplaySelectedEntity.Entity.GUID_ORIBASELINE = null;
                DisplaySelectedEntity.Entity.GUID_BASELINE = null;
                MainViewModel.Save(DisplaySelectedEntity);
            }

            List<TEntity> newBASELINE_ITEMS = new List<TEntity>();

            LoadingScreenManager.ShowLoadingScreen(deliverables.Count());
            List<ExoSubJobEditableProjection> exoVariations = new List<ExoSubJobEditableProjection>();
            IBluePrintsEntitiesUnitOfWork bluePrintsUnitOfWork = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
            foreach (var deliverable in deliverables)
            {
                if(variationStage == VariationStages.Unapprove)
                {
                    //remove deliverable if none of the attached variation is approved
                    var deliverableVariationQuery = from BASELINE_ITEM in bluePrintsUnitOfWork.BASELINE_ITEMS
                                                    join BASELINE in bluePrintsUnitOfWork.BASELINES
                                                    on BASELINE_ITEM.GUID_BASELINE equals BASELINE.GUID
                                                    join VARIATION_ITEMS in bluePrintsUnitOfWork.VARIATION_ITEMS
                                                    on BASELINE_ITEM.GUID_ORIGINAL equals VARIATION_ITEMS.GUID_ORIBASEITEM
                                                    join VARIATIONS in bluePrintsUnitOfWork.VARIATIONS
                                                    on VARIATION_ITEMS.GUID_VARIATION equals VARIATIONS.GUID
                                                    where BASELINE.GUID == liveBASELINE.GUID && BASELINE_ITEM.GUID_ORIGINAL == deliverable.GUID_ORIGINAL
                                                    select new { BASELINE_ITEM, VARIATIONS };

                    List<BASELINE_ITEM_VARIATIONContainer> deliverableVariations = deliverableVariationQuery.Select(x => new BASELINE_ITEM_VARIATIONContainer() { BASELINE_ITEM = x.BASELINE_ITEM, VARIATION = x.VARIATIONS }).ToList();
                    if (deliverableVariations.Count > 0 && deliverableVariations.All(x => x.VARIATION.APPROVED == null))
                    {
                        bluePrintsUnitOfWork.BASELINE_ITEMS.Remove(deliverableVariations.First().BASELINE_ITEM);
                        bluePrintsUnitOfWork.SaveChanges();
                    }
                }
                //only revise when new baseline is created
                else if(variationStage == VariationStages.Approve && historianBASELINE != null && historianBASELINE.GUID != Guid.Empty)
                {
                    VARIATION_ITEM updateVARIATION_ITEM = deliverable.VARIATION_ITEM;
                    decimal? variationUnits = null;
                    TEntity deliverable_history = null;
                    if (deliverable.DisplayVariationAction == VariationAction.Cancel)
                    {
                        if (deliverable.Earned_Units_Total == 0)
                            variationUnits = -1 * deliverable.Total_Units;
                        else
                            variationUnits = -1 * (deliverable.Total_Units - deliverable.Earned_Units_Total);

                        deliverable.GUID_VARIATION = DisplaySelectedEntity.GUID;
                        deliverable_history = new TEntity();
                    }
                    else if (deliverable.DisplayVariationAction == VariationAction.Append)
                    {
                        if (deliverable.DisplayVariationUnits < 0)
                        {
                            decimal maximumReducibleUnits = -1 * (deliverable.Total_Units - deliverable.Earned_Units_Total);
                            if (deliverable.DisplayVariationUnits < maximumReducibleUnits)
                                variationUnits = maximumReducibleUnits;
                            else
                                variationUnits = deliverable.DisplayVariationUnits;
                        }
                        else
                            variationUnits = deliverable.DisplayVariationUnits;

                        deliverable_history = new TEntity();
                    }
                    else if (deliverable.DisplayVariationAction == VariationAction.Add)
                    {
                        variationUnits = deliverable.DisplayVariationUnits;

                        //when the deliverable is approved by multiple variation and unapproved and re-approved on the variation that defines it as add
                        //multiple copies exists and we only want to process one copy and save it in historian
                        if (!repository.Any(x => x.GUID_BASELINE == historianBASELINE.GUID && x.GUID_ORIGINAL == deliverable.GUID_ORIGINAL))
                            deliverable_history = new TEntity();

                        //when the deliverable is approved by multiple variation and reapproved on the variation that defines it as Add, dont' add the deliverable
                        if(!repository.Any(x => x.GUID_BASELINE == liveBASELINE.GUID && x.GUID_ORIGINAL == deliverable.GUID_ORIGINAL))
                        {
                            TEntity new_deliverable = new TEntity();
                            DataUtils.ShallowCopy(new_deliverable, deliverable.Entity);
                            new_deliverable.GUID = Guid.Empty;
                            new_deliverable.GUID_BASELINE = liveBASELINE.GUID;
                            new_deliverable.GUID_VARIATION = DisplaySelectedEntity.GUID;
                            repository.Add(new_deliverable);
                        }
                    }

                    if(deliverable_history != null)
                    {
                        DataUtils.ShallowCopy(deliverable_history, deliverable.Entity);
                        deliverable_history.GUID = Guid.Empty;
                        deliverable_history.GUID_BASELINE = historianBASELINE.GUID;
                        deliverable_history.GUID_VARIATION = DisplaySelectedEntity.GUID;
                        repository.Add(deliverable_history);
                    }

                    //Save variation units for future viewing
                    if(variationUnits != null)
                    {
                        deliverable.VARIATION_ITEM.VARIATION_UNITS = (decimal)variationUnits;
                        VARIATION_ITEMSViewModel.Save(updateVARIATION_ITEM);
                    }
                }
                //if its purely a scan to determine variation
                else if (variationStage != VariationStages.Approve && (deliverable.DisplayVariationAction == VariationAction.Add || deliverable.DisplayVariationAction == VariationAction.Append) && variationCode != string.Empty)
                {
                    string subJobCode = deliverable.Subjob_Name;
                    string disciplineCode = deliverable.Discipline_Code;
                    string commodityCode = deliverable.Commodity_Code;
                    decimal variationCost = deliverable.Forecast_Costs;

                    ExoSubJobEditableProjection exoVariation = exoVariations.FirstOrDefault((x => x.SubJobCode == subJobCode && x.DisciplineCode == disciplineCode && x.CommodityCode == commodityCode && x.VariationCode == variationCode));
                    if (exoVariation == null)
                    {
                        ExoSubJobEditableProjection newVariationSubJob = new ExoSubJobEditableProjection() { SubJobCode = subJobCode, DisciplineCode = disciplineCode, CommodityCode = commodityCode, VariationCode = variationCode, Budget = variationCost };
                        //set commodity code convention so that error can be raised natively within model with GetPropertyError
                        newVariationSubJob.PopulateCommodityCodes(COMMODITY_CODECollection);
                        exoVariations.Add(newVariationSubJob);
                    }
                    else
                    {
                        exoVariation.Budget += variationCost;
                    }
                }

                LoadingScreenManager.Progress();
            }

            if (variationStage == VariationStages.Submit || variationStage == VariationStages.Unsubmit)
                addVariationJobToExo(exoVariations, variationStage);
            else
            {
                unitOfWork.SaveChanges();
                if(variationStage == VariationStages.Unapprove)
                    Unsubmit();
                else
                    //Full refresh is required to pick up summary
                    FullRefresh();
            }

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

        private DevExpress.Mvvm.IDialogService ConfirmationDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("ConfirmationDialogService"); }
        }

        private readonly IPrimeroEntitiesUnitOfWork primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
        private void addVariationJobToExo(List<ExoSubJobEditableProjection> exoVariationJobs, VariationStages exoInteraction)
        {
            bool isAnyVariationJobsExists = false;
            bool isAnyVariationJobNotExists = false;
            foreach (var exoVariationJob in exoVariationJobs)
            {
                JOBCOST_LINES line = ExoQueries.GetProjectLine(primeroUnitOfWork, loadPROJECT.NUMBER, exoVariationJob);
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
                FullRefresh();
                return;
            }
            else if(exoInteraction == VariationStages.Unsubmit && !isAnyVariationJobsExists)
            {
                UnsubmitSelectedEntity();
                //refresh is required to populate summary
                FullRefresh();
                return;
            }

            string message = string.Empty;
            if (exoInteraction == VariationStages.Submit)
                message = "Push OK to commit the following variation jobs to EXO, or push cancel and revise added deliverables if the codes are incorrect";
            else
                message = "Push OK to remove the following variation jobs from EXO";

            DialogCollectionViewModel<ExoSubJobEditableProjection> viewModel = DialogCollectionViewModel<ExoSubJobEditableProjection>.Create(exoVariationJobs, message);
            if (ConfirmationDialogService.ShowDialog(MessageButton.OKCancel, string.Empty, "ExoVariationConfirmation", viewModel) == MessageResult.OK)
            {
                if(exoInteraction == VariationStages.Submit)
                {
                    if (exoJobCollectionViewModel.CommitToExo(exoVariationJobs))
                    {
                        SubmitSelectedEntity();
                        MessageBoxService.ShowMessage("Variation code(s) pushed to exo");
                        //refresh is required to populate summary
                        FullRefresh();
                    }
                    else
                    {
                        MessageBoxService.ShowMessage("Pushed to exo failed, variation is not submitted");
                    }
                }
                else
                {
                    bool hasRemoved = false;
                    foreach (var exoVariationJob in exoVariationJobs)
                    {
                        JOBCOST_LINES line = ExoQueries.GetProjectLine(primeroUnitOfWork, loadPROJECT.NUMBER, exoVariationJob);
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
                    //refresh is required to populate summary
                    FullRefresh();
                }
            }
        }

        private void UnsubmitSelectedEntity()
        {
            DisplaySelectedEntity.Entity.SUBMITTED = null;
            DisplaySelectedEntity.Entity.SUBMITTEDBY = null;
            MainViewModel.Save(DisplaySelectedEntity);
        }

        private void SubmitSelectedEntity()
        {
            DisplaySelectedEntity.Entity.SUBMITTED = DateTime.Now;
            DisplaySelectedEntity.Entity.SUBMITTEDBY = LoginCredentials.CurrentUserGuid;
            MainViewModel.Save(DisplaySelectedEntity);
        }

        protected override void OnClose(CancelEventArgs e)
        {
            exoJobCollectionViewModel.Dispose();
            base.OnClose(e);
        }

        public enum VariationStages
        {
            Approve,
            Unapprove,
            Submit,
            Unsubmit
        }

        public class BASELINE_ITEM_VARIATIONContainer
        {
            public BASELINE_ITEM BASELINE_ITEM { get; set; }
            public VARIATION VARIATION { get; set; }
        }
        #endregion
    }
}