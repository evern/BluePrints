using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Document;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
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
        private ProgressType phaseType;
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void resolveParameters(object parameter)
        {
            var project_phasetype_parameter = (DualEntitiesParameter<PROJECT, ProgressTypeClass>) parameter;
            loadPROJECT = project_phasetype_parameter.GetFirstEntity();
            phaseType = project_phasetype_parameter.GetSecondEntity().progressType;
        }

        protected override void initializeEntitiesLoadersDescription()
        {
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINES, BASELINEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.ESTIMATES, ESTIMATEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS, BASELINE_ITEMProjectionFunc, null, true);
            loaderCollection.AddLoaderDescription<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.USERS);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.VARIATION_ITEMS, VARIATION_ITEMProjectionFunc);
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

        private Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEM>> BASELINE_ITEMProjectionFunc()
        {
            if (LiveBASELINE == null)
                return query => query.Where(x => x.GUID == Guid.Empty);
            else
                return query => query.Where(x => x.GUID_BASELINE == LiveBASELINE.GUID);
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

            if(phaseType == ProgressType.Design)
            {
                foreach (var entity in entities)
                {
                    CreateVARIATION_ITEMSViewModelWrapper<BASELINE_ITEMVariation>(entity.Entity, (projections, parentId) => mainThreadDispatcher.BeginInvoke(new Action(() => AssignVariationSummary(projections, parentId))), () => entity.GUID, true);
                }
            }
            else
            {
                foreach (var entity in entities)
                {
                    CreateVARIATION_ITEMSViewModelWrapper<ESTIMATE_ITEMVariation>(entity.Entity, (projections, parentId) => mainThreadDispatcher.BeginInvoke(new Action(() => AssignVariationSummary(projections, parentId))), () => entity.GUID, true);
                }
            }
        }

        private void AssignVariationSummary<TMainProjectionEntity>(IEnumerable<TMainProjectionEntity> variation_projections, object parent_id)
            where TMainProjectionEntity : class, ISupportVariationSummary, IGuidEntityKey, new()
        {
            //When refresh button is pushed too fast, MainViewModel may not be initialized
            if (MainViewModel == null)
                return;

            VARIATIONProjection projection = MainViewModel.Entities.First(x => x.EntityKey == (Guid)parent_id);
            projection.DetailEntities = new ObservableCollection<ISupportVariationSummary>(variation_projections);
            BackgroundRefresh(true);
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
                if (phaseType == ProgressType.Design)
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

        #region Variation_Item revision
        public ICollectionViewModelsWrapper<TMainProjectionEntity> CreateVARIATION_ITEMSViewModelWrapper<TMainProjectionEntity>(VARIATION loadVARIATION, Action<IEnumerable<TMainProjectionEntity>, object> onLoadedAction, Func<object> getParentIdFunc, bool supressCompulsoryEntityNotFoundMessage)
            where TMainProjectionEntity : class, IGuidEntityKey, new()
        {
            if (loadPROJECT != null)
            {
                ICollectionViewModelsWrapper<TMainProjectionEntity> variation_itemsViewModelWrapper;
                if (phaseType == ProgressType.Design)
                    variation_itemsViewModelWrapper = (ICollectionViewModelsWrapper<TMainProjectionEntity>)OffsiteDirectVariationCollectionViewModelWrapper.Create();
                else
                    variation_itemsViewModelWrapper = (ICollectionViewModelsWrapper<TMainProjectionEntity>)SiteDirectVariationCollectionViewModelWrapper.Create();

                variation_itemsViewModelWrapper.SetParentViewModel(this);
                variation_itemsViewModelWrapper.OnEntitiesLoadedCallBack = onLoadedAction;
                variation_itemsViewModelWrapper.OnEntitiesLoadedCallBackRelateParam = getParentIdFunc;
                variation_itemsViewModelWrapper.SuppressNotification = true;
                variation_itemsViewModelWrapper.SupressCompulsoryEntityNotFoundMessage = supressCompulsoryEntityNotFoundMessage;
                var baselineSupportParameterObj = variation_itemsViewModelWrapper as ISupportParameter;
                baselineSupportParameterObj.Parameter = new DualEntitiesParameter<PROJECT, VARIATION>(loadPROJECT, loadVARIATION);

                return variation_itemsViewModelWrapper;
            }

            return null;
        }
        #endregion

        #region View Properties
        public override void FullRefresh()
        {
            ReloadEntitiesCollection();
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

        public CollectionViewModel<BASELINE_ITEM, BASELINE_ITEM, Guid, IBluePrintsEntitiesUnitOfWork> BASELINE_ITEMSViewModel
        {
            get
            {
                return (CollectionViewModel<BASELINE_ITEM, BASELINE_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<BASELINE_ITEM>();
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
                    return ESTIMATECollection.FirstOrDefault(x => x.STATUS == EstimateStatus.LiveBudget);
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
                if(phaseType == ProgressType.Design)
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

        public IEnumerable<BASELINE_ITEM> BASELINE_ITEMCollection
        {
            get
            {
                var collection = GetEntities<BASELINE_ITEM>();
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

            string view_name;
            string tab_title;
            if (DisplaySelectedEntity.Entity.PHASE == ProgressType.Design)
            {
                view_name = "OffsiteDirectVariationCollectionView";
                tab_title = "Design Variation";
            }
            else
            {
                view_name = "SiteDirectVariationCollectionView";
                tab_title = "Construction Variation";
            }

            DocumentInfo DocumentInfo = new DocumentInfo(DisplaySelectedEntity.GUID.ToString(),
                new DualEntitiesParameter<PROJECT, VARIATION>(loadPROJECT, DisplaySelectedEntity.Entity),
                view_name,
                "[" + loadPROJECT.NUMBER + "] " + "[" + DisplaySelectedEntity.Entity.NAME + "] " + tab_title);

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

            if (DisplaySelectedEntity != null && DisplaySelectedEntity.Entity.APPROVED != null)
                return false;

            return true;
        }

        /// <summary>
        /// Submits an entity.
        /// Since CollectionViewModelBase is a POCO view model, an the instance of this class will also expose the SubmitCommand property that can be used as a binding source in views.
        /// </summary>
        /// <param name="projectionEntity">An entity to Submit.</param>
        public void Submit()
        {
            DisplaySelectedEntity.Entity.SUBMITTED = DateTime.Now;
            DisplaySelectedEntity.Entity.SUBMITTEDBY = LoginCredentials.CurrentUserGuid;
            MainViewModel.Save(DisplaySelectedEntity);

            //Full refresh is required to pick up summary
            FullRefresh();
        }

        /// <summary>
        /// Determines whether an entities can be approved
        /// Since CollectionViewModelBase is a POCO view model, this method will be used as a CanExecute callback for ApproveCommand.
        /// </summary>
        /// <param name="projectionEntity">Entities to approve.</param>
        public bool CanApprove()
        {
            if (MainViewModel == null || DisplaySelectedEntity == null)
                return false;

            if (DisplaySelectedEntity.Entity == null)
                return false;

            if (DisplaySelectedEntity.Entity.SUBMITTED == null)
                return false;

            if (DisplaySelectedEntity != null && DisplaySelectedEntity.Entity.APPROVED != null)
                return false;

            return true;
        }

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
                if (phaseType == ProgressType.Design && LiveBASELINE == null)
                    errorMessage = "Live baseline not found";
                else if(phaseType == ProgressType.Construct && LiveESTIMATE == null)
                    errorMessage = "Live estimate not found";
            }


            if (errorMessage != string.Empty)
            {
                MessageBoxService.ShowMessage(errorMessage);
                return;
            }

            if(phaseType == ProgressType.Design)
                CreateVARIATION_ITEMSViewModelWrapper<BASELINE_ITEMVariation>(DisplaySelectedEntity.Entity, OnVARIATION_ITEMSLoaded, null, false);
            else if(phaseType == ProgressType.Construct)
                CreateVARIATION_ITEMSViewModelWrapper<ESTIMATE_ITEMVariation>(DisplaySelectedEntity.Entity, OnVARIATION_ITEMSLoaded, null, false);
        }

        private void OnVARIATION_ITEMSLoaded(IEnumerable<object> projections, object parentId)
        {
            IBluePrintsEntitiesUnitOfWork bluePrintsUOW = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
            if(phaseType == ProgressType.Design)
                mainThreadDispatcher.BeginInvoke(new Action(() => ReviseBASELINE<BASELINE, BASELINE_ITEM, BASELINE_ITEMProjection, BASELINE_ITEMProgress, BASELINE_ITEMVariation>(projections.ToList(), LiveBASELINE, BASELINEViewModel, bluePrintsUOW, bluePrintsUOW.BASELINE_ITEMS)));
            else if (phaseType == ProgressType.Construct)
                mainThreadDispatcher.BeginInvoke(new Action(() => ReviseBASELINE<ESTIMATE, ESTIMATE_ITEM, ESTIMATE_ITEMProjection, ESTIMATE_ITEMProgress, ESTIMATE_ITEMVariation>(projections.ToList(), LiveESTIMATE, ESTIMATEViewModel, bluePrintsUOW, bluePrintsUOW.ESTIMATE_ITEMS)));
        }

        public bool CanRevert()
        {
            return (MainViewModel != null && MainViewModel.Entities.Count > 0);
        }

        public void Revert()
        {
            if (MessageBoxService.ShowMessage("Are you sure you want to revert changes to the baseline?", "Revert Baseline", MessageButton.OKCancel) == MessageResult.Cancel)
                return;

            string liveBaselineRevision = LiveBASELINE.Revision;
            string lastBaselineRevision = ((char)(liveBaselineRevision.Last() - 1)).ToString();
            VARIATIONProjection lastVariation = MainViewModel.Entities.FirstOrDefault(x => getBaselineRevision(x.Entity.GUID_BASELINE) == liveBaselineRevision);
            if(lastVariation == null)
            {
                MessageBoxService.ShowMessage("Last variation revision does not match latest baseline revision");
                return;
            }

            BASELINE lastBaseline = BASELINECollection.FirstOrDefault(x => x.REVISION == lastBaselineRevision);
            if(lastBaseline == null)
            {
                MessageBoxService.ShowMessage("Last baseline not found");
                return;
            }

            BASELINE live_baseline = new BASELINE();
            DataUtils.ShallowCopy(live_baseline, LiveBASELINE);
            live_baseline.REVISION = live_baseline.REVISION + "_Reverted";
            live_baseline.STATUS = BaselineStatus.Superseded;
            BASELINEViewModel.Save(live_baseline);

            lastBaseline.STATUS = BaselineStatus.Live;
            BASELINEViewModel.Save(lastBaseline);

            lastVariation.Entity.APPROVED = null;
            lastVariation.Entity.APPROVEDBY = null;
            MainViewModel.Save(lastVariation);

            Refresh();
        }

        private string getBaselineRevision(Guid? baseline_guid)
        {
            BASELINE baseline = BASELINECollection.FirstOrDefault(x => x.GUID == baseline_guid);
            if (baseline != null)
                return baseline.REVISION;

            return string.Empty;
        }

        public void ReviseBASELINE<TBaseline, TEntity, TDeliverableRate, TReportable, TVariation>(IEnumerable<object> objects, TBaseline liveBASELINE, CollectionViewModel<TBaseline, TBaseline, Guid, IBluePrintsEntitiesUnitOfWork> collectionViewModel, IBluePrintsEntitiesUnitOfWork unitOfWork, IRepository<TEntity, Guid> repository)
            where TBaseline : class, IAmBaseline, new()
            where TEntity : class, IDeliverable, ISupportVariation, new()
            where TDeliverableRate : class, IDeliverable_Rates, IProjection<TEntity>, new()
            where TReportable : class, IReportable, IProjection<TDeliverableRate>, new()
            where TVariation : class, IBluePrintsVariationBase<TReportable>, new()
        {
            //Must cleanup before doing baseline update to prevent wrapper from refreshing
            //CleanUpVARIATION_ITEMS(variation_itemsViewModelWrapperForApproval);
            TBaseline newBASELINE = new TBaseline();
            liveBASELINE.Baseline_Status = BaselineStatus.Superseded;
            collectionViewModel.Save(liveBASELINE);

            DataUtils.ShallowCopy(newBASELINE, liveBASELINE);
            newBASELINE.EntityKey = Guid.Empty;
            newBASELINE.Revision = ((char)(liveBASELINE.Revision.Last() + 1)).ToString();
            //not saving new baseline as live yet because editBASELINE_ITEMS still depends on the current live baseline for copying BASELINE_ITEMS
            newBASELINE.Baseline_Status = BaselineStatus.Live;
            collectionViewModel.Save(newBASELINE);

            DisplaySelectedEntity.Entity.APPROVED = DateTime.Now;
            DisplaySelectedEntity.Entity.GUID_ORIBASELINE = liveBASELINE.EntityKey;
            DisplaySelectedEntity.Entity.GUID_BASELINE = newBASELINE.EntityKey;
            MainViewModel.Save(DisplaySelectedEntity);

            //var newBASELINE_ITEMS = new ObservableCollection<BASELINE_ITEM>();
            List<TEntity> baseline_itemForInternalNumberGeneration = new List<TEntity>();
            List<TVariation> variation_items = objects.Select(x => (TVariation)x).ToList();
            List<TEntity> newBASELINE_ITEMS = new List<TEntity>();

            LoadingScreenManager.ShowLoadingScreen(variation_items.Count);

            foreach (var variation_item in variation_items)
            {
                TEntity new_deliverable = new TEntity();
                DataUtils.ShallowCopy(new_deliverable, variation_item.Entity.Entity.Entity);

                if (variation_item.Variation_Action == VariationAction.Cancel)
                {
                    if (variation_item.Entity.Earned_Units_Total == 0)
                        new_deliverable.DC_Value += -1 * new_deliverable.Total_Units;
                    else
                        new_deliverable.DC_Value += -1 * (new_deliverable.Total_Units - variation_item.Entity.Earned_Units_Total);

                    //Save deducted variation units for future viewing
                    variation_item.VARIATION_ITEM.VARIATION_UNITS = new_deliverable.DC_Value;
                    VARIATION_ITEMSViewModel.Save(variation_item.VARIATION_ITEM);
                }
                else if (variation_item.Variation_Action == VariationAction.Append)
                {
                    decimal edit_value;
                    if (DisplaySelectedEntity.Entity.TYPE == VariationType.Internal)
                        edit_value = new_deliverable.Estimated_Value;
                    else
                        edit_value = new_deliverable.DC_Value;

                    if (variation_item.Variation_Units < 0)
                    {
                        decimal maximumReducibleUnits = -1 * variation_item.Entity.Earned_Units_Total;
                        if (variation_item.Variation_Units < maximumReducibleUnits)
                            edit_value += maximumReducibleUnits;
                        else
                            edit_value += variation_item.Variation_Units;
                    }
                    else
                        edit_value += variation_item.Variation_Units;

                    if (DisplaySelectedEntity.Entity.TYPE == VariationType.Internal)
                        new_deliverable.Estimated_Value = edit_value;
                    else
                        new_deliverable.DC_Value = edit_value;
                }
                else if (variation_item.Variation_Action == VariationAction.Add)
                {
                    new_deliverable.EntityKey = Guid.Empty;
                    new_deliverable.Baseline_Guid = newBASELINE.EntityKey;
                    //newBASELINE_ITEM.INTERNAL_NUM = BluePrintsDataUtils.BASELINEITEM_Generate_InternalNumber(
                    //    loadPROJECT, baseline_itemForInternalNumberGeneration, newBASELINE_ITEM.AREA, newBASELINE_ITEM.DISCIPLINE,
                    //    newBASELINE_ITEM.DOCTYPE);

                    if (DisplaySelectedEntity.Entity.TYPE == VariationType.Internal)
                        new_deliverable.Estimated_Value += variation_item.Variation_Units;
                    else
                        new_deliverable.DC_Value += variation_item.Variation_Units;

                    new_deliverable.Variation_Guid = DisplaySelectedEntity.EntityKey;
                    baseline_itemForInternalNumberGeneration.Add(new_deliverable);
                }

                if (variation_item.Variation_Action != VariationAction.NoAction)
                    new_deliverable.Variation_Guid = DisplaySelectedEntity.EntityKey;

                new_deliverable.EntityKey = Guid.Empty;
                new_deliverable.Baseline_Guid = newBASELINE.EntityKey;
                //BASELINE_ITEMSViewModel.Save(newBASELINE_ITEM);
                repository.Add(new_deliverable);
                LoadingScreenManager.Progress();
            }

            unitOfWork.SaveChanges();
            //Full refresh is required to pick up summary
            FullRefresh();
        }
        #endregion
    }
}