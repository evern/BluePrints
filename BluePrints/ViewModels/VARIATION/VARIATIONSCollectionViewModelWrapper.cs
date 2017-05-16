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
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void InitializeParameters(object parameter)
        {
            var PROJECTParameter =
                (EntitiesParameter<PROJECT>) parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            //MainViewModel = null;
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINES, BASELINEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS, BASELINE_ITEMProjectionFunc, null, true);
            loaderCollection.AddLoaderDescription<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.USERS);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.VARIATION_ITEMS, VARIATION_ITEMProjectionFunc);
            InvokeEntitiesLoaderDescriptionLoading();
        }

        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<BASELINE>, IQueryable<BASELINE>> BASELINEProjectionFunc()
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

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.VARIATIONS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<VARIATION>, IQueryable<VARIATIONProjection>> ConstructMainViewModelProjection()
        {
            return query => VARIATIONProjectionQueries.JoinVARIATION_ITEMSOnVARIATIONS(query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID)
                .OrderBy(x => x.NAME));
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<VARIATIONProjection> entities)
        {
            MainViewModel.SetParentAssociationCallBack = OnBeforeEntitySaved;
            MainViewModel.ApplyProjectionPropertiesToEntityCallBack = ApplyProjectionPropertiesToEntity;
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

            foreach (var entity in entities)
            {
                VARIATION_ITEMSCollectionViewModelWrapper variationitemsCollectionViewModelWrapper = CreateVARIATION_ITEMSViewModelWrapper(entity.Entity, entity.EntityKey, OnVARIATION_ITEMSLoadedAssign);
            }
        }

        private void OnVARIATION_ITEMSLoadedAssign(IEnumerable<VARIATION_ITEMProjection> projections, object parameter)
        {
            Guid variationProjectionGuid = (Guid)parameter;
            mainThreadDispatcher.BeginInvoke(new Action(() => AssignVariationSummary(variationProjectionGuid, projections)));
        }

        private void AssignVariationSummary(Guid variationProjectionGuid, IEnumerable<VARIATION_ITEMProjection> projections)
        {
            //When refresh button is pushed too fast, MainViewModel may not be initialized
            if (MainViewModel == null)
                return;

            VARIATIONProjection projection = MainViewModel.Entities.First(x => x.EntityKey == variationProjectionGuid);
            projection.DetailEntities = new ObservableCollection<VARIATION_ITEMProjection>(projections);
            RefreshView(true);
        }

        #region CallBacks
        public bool BeforeSaveValidation(VARIATIONProjection entity, bool isNewEntity)
        {
            if (LiveBASELINE == null)
                return false;

            return true;
        }

        public void ApplyProjectionPropertiesToEntity(VARIATIONProjection projectionEntity, VARIATION entity)
        {
            DataUtils.ShallowCopy(entity, projectionEntity.Entity);
            //workaround for created because Save() only sets the projection primary key, this is used for property redo where the interceptor only tampers with UPDATED and CREATED is left as null
            if (entity.CREATED.Date.Year == 1)
            {
                projectionEntity.Entity.CREATED = DateTime.Now;
                //Although EF convention will generate this but we require it immediately in the view
                projectionEntity.Entity.CREATEDBY = LoginCredentials.CurrentUserGuid;
            }

            entity.CREATED = projectionEntity.Entity.CREATED;
        }

        public void OnBeforeEntitySaved(VARIATIONProjection entity)
        {
            entity.Entity.GUID_PROJECT = loadPROJECT.GUID;

            if (entity.Entity.APPROVED != null)
                entity.Entity.GUID_ORIBASELINE = entity.Entity.GUID_ORIBASELINE ?? LiveBASELINE.GUID;
            else
                entity.Entity.GUID_ORIBASELINE = null;
        }
        #endregion

        #endregion

        #region Variation_Item revision
        public VARIATION_ITEMSCollectionViewModelWrapper CreateVARIATION_ITEMSViewModelWrapper(VARIATION loadVARIATION,
            object OnEntitiesLoadedParameter, Action<IEnumerable<VARIATION_ITEMProjection>, object> OnLoadedAction)
        {
            VARIATION_ITEMSCollectionViewModelWrapper variation_itemsViewModelWrapper = null;

            if (loadPROJECT != null)
            {
                variation_itemsViewModelWrapper = new VARIATION_ITEMSCollectionViewModelWrapper();
                variation_itemsViewModelWrapper.SuppressNotification = true;
                //variation_itemsViewModelWrapper.SetParentViewModel(this);
                variation_itemsViewModelWrapper.OnEntitiesLoadedParameterCallBack = () => OnEntitiesLoadedParameter;
                variation_itemsViewModelWrapper.OnEntitiesLoadedWithParameterCallBack = OnLoadedAction;
                var baselineSupportParameterObj = variation_itemsViewModelWrapper as ISupportParameter;
                baselineSupportParameterObj.Parameter = new OptionalEntitiesParameter<PROJECT, VARIATION>(loadPROJECT, loadVARIATION);
            }

            return variation_itemsViewModelWrapper;
        }

        //public void CleanUpVARIATION_ITEMS(VARIATION_ITEMSCollectionViewModelWrapper variation_itemsViewModelWrapper)
        //{
        //    if (variation_itemsViewModelWrapper != null)
        //        variation_itemsViewModelWrapper.CleanUpEntitiesLoader();

        //    variation_itemsViewModelWrapper = null;
        //}
        #endregion

        #region View Properties
        public override void FullRefresh()
        {
            mainThreadDispatcher.BeginInvoke(new Action(() => StoreViewState()));
            InitializeAndLoadEntitiesLoaderDescription();
        }
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "VARIATIONSViewModelWrapper"; }
        }

        public CollectionViewModel<BASELINE, BASELINE, Guid, IBluePrintsEntitiesUnitOfWork> BASELINEViewModel
        {
            get
            {
                return (CollectionViewModel<BASELINE, BASELINE, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<BASELINE>();
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
                var collection = GetEntities<BASELINE>();
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

            DocumentInfo DocumentInfo = new DocumentInfo(DisplaySelectedEntity.GUID.ToString(),
                new OptionalEntitiesParameter<PROJECT, VARIATION>(loadPROJECT, DisplaySelectedEntity.Entity),
                "VARIATION_ITEMCollectionView",
                "[" + loadPROJECT.NUMBER + "] Variation");

            DocumentManagerService.ShowExistingEntityDocument(DocumentInfo, this);
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

            if (LiveBASELINE == null)
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

        // Initialize a separate wrapper for approval because summary wrapper is not persistent to avoid multiple OnMessage event from getting picked up for each variation
        VARIATION_ITEMSCollectionViewModelWrapper variation_itemsViewModelWrapperForApproval;
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
                errorMessage = "Project doesn't exists";
            else if (LiveBASELINE == null)
                errorMessage = "Live baseline doesn't exists";
            else if (LivePROGRESS == null)
                errorMessage = "Live progress doesn't exists";

            if (errorMessage != string.Empty)
            {
                MessageBoxService.ShowMessage(errorMessage);
                return;
            }

            variation_itemsViewModelWrapperForApproval = CreateVARIATION_ITEMSViewModelWrapper(DisplaySelectedEntity.Entity, null, OnVARIATION_ITEMSLoaded);
        }

        private void OnVARIATION_ITEMSLoaded(IEnumerable<VARIATION_ITEMProjection> projections, object parameter)
        {
            mainThreadDispatcher.BeginInvoke(new Action(() => ReviseBASELINE(projections.ToList())));
        }

        public void ReviseBASELINE(IEnumerable<VARIATION_ITEMProjection> projections)
        {
            //Must cleanup before doing baseline update to prevent wrapper from refreshing
            //CleanUpVARIATION_ITEMS(variation_itemsViewModelWrapperForApproval);
            var newBASELINE = new BASELINE();
            BASELINE liveBASELINE = LiveBASELINE;
            liveBASELINE.STATUS = BaselineStatus.Superseded;
            BASELINEViewModel.Save(liveBASELINE);

            DataUtils.ShallowCopy(newBASELINE, liveBASELINE);
            newBASELINE.GUID = Guid.Empty;
            newBASELINE.REVISION = ((char)(liveBASELINE.REVISION.Last() + 1)).ToString();
            //not saving new baseline as live yet because editBASELINE_ITEMS still depends on the current live baseline for copying BASELINE_ITEMS
            newBASELINE.STATUS = BaselineStatus.Live;
            BASELINEViewModel.Save(newBASELINE);

            DisplaySelectedEntity.Entity.APPROVED = DateTime.Now;
            DisplaySelectedEntity.Entity.GUID_ORIBASELINE = liveBASELINE.GUID;
            DisplaySelectedEntity.Entity.GUID_BASELINE = newBASELINE.GUID;
            MainViewModel.Save(DisplaySelectedEntity);

            //var newBASELINE_ITEMS = new ObservableCollection<BASELINE_ITEM>();
            List<BASELINE_ITEM> baseline_itemForInternalNumberGeneration = new List<BASELINE_ITEM>();
            List<VARIATION_ITEMProjection> variation_items = projections.ToList();
            List<BASELINE_ITEM> newBASELINE_ITEMS = new List<BASELINE_ITEM>();
            IBluePrintsEntitiesUnitOfWork bluePrintsUOW = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();

            LoadingScreenManager.ShowLoadingScreen(variation_items.Count);
            foreach (var currentVARIATION_ITEM in variation_items)
            {
                var newBASELINE_ITEM = new BASELINE_ITEM();
                DataUtils.ShallowCopy(newBASELINE_ITEM, currentVARIATION_ITEM.Entity.Entity);

                if (currentVARIATION_ITEM.VARIATION_ITEM.ACTION == VariationAction.Cancel)
                {
                    if (currentVARIATION_ITEM.TOTAL_EARNED_UNITS == 0)
                        newBASELINE_ITEM.DC_HOURS += -1 * newBASELINE_ITEM.TOTAL_HOURS;
                    else
                        newBASELINE_ITEM.DC_HOURS += -1 * (newBASELINE_ITEM.TOTAL_HOURS - currentVARIATION_ITEM.TOTAL_EARNED_UNITS);

                    //Save deducted variation units for future viewing
                    currentVARIATION_ITEM.VARIATION_ITEM.VARIATION_UNITS = newBASELINE_ITEM.DC_HOURS;
                    VARIATION_ITEMSViewModel.Save(currentVARIATION_ITEM.VARIATION_ITEM);
                }
                else if (currentVARIATION_ITEM.VARIATION_ITEM.ACTION == VariationAction.Append)
                {
                    if (currentVARIATION_ITEM.VARIATION_ITEM.VARIATION_UNITS < 0)
                    {
                        decimal maximumReducibleUnits = -1 * currentVARIATION_ITEM.TOTAL_EARNED_UNITS;
                        if (currentVARIATION_ITEM.VARIATION_ITEM.VARIATION_UNITS < maximumReducibleUnits)
                            newBASELINE_ITEM.DC_HOURS += maximumReducibleUnits;
                        else
                            newBASELINE_ITEM.DC_HOURS += currentVARIATION_ITEM.VARIATION_ITEM.VARIATION_UNITS;
                    }
                    else
                        newBASELINE_ITEM.DC_HOURS += currentVARIATION_ITEM.VARIATION_ITEM.VARIATION_UNITS;
                }
                else if (currentVARIATION_ITEM.VARIATION_ITEM.ACTION == VariationAction.Add)
                {
                    newBASELINE_ITEM.GUID = Guid.Empty;
                    newBASELINE_ITEM.GUID_BASELINE = newBASELINE.GUID;
                    //newBASELINE_ITEM.INTERNAL_NUM = BluePrintsDataUtils.BASELINEITEM_Generate_InternalNumber(
                    //    loadPROJECT, baseline_itemForInternalNumberGeneration, newBASELINE_ITEM.AREA, newBASELINE_ITEM.DISCIPLINE,
                    //    newBASELINE_ITEM.DOCTYPE);

                    if (DisplaySelectedEntity.Entity.TYPE == VariationType.Internal)
                        newBASELINE_ITEM.ESTIMATED_HOURS += currentVARIATION_ITEM.VARIATION_ITEM.VARIATION_UNITS;
                    else
                        newBASELINE_ITEM.DC_HOURS += currentVARIATION_ITEM.VARIATION_ITEM.VARIATION_UNITS;

                    newBASELINE_ITEM.GUID_VARIATION = DisplaySelectedEntity.EntityKey;
                    baseline_itemForInternalNumberGeneration.Add(newBASELINE_ITEM);
                }

                if (currentVARIATION_ITEM.VARIATION_ITEM.ACTION != VariationAction.NoAction)
                    newBASELINE_ITEM.GUID_VARIATION = DisplaySelectedEntity.EntityKey;

                newBASELINE_ITEM.GUID = Guid.Empty;
                newBASELINE_ITEM.GUID_BASELINE = newBASELINE.GUID;
                //BASELINE_ITEMSViewModel.Save(newBASELINE_ITEM);
                bluePrintsUOW.BASELINE_ITEMS.Add(newBASELINE_ITEM);
                LoadingScreenManager.Progress();
            }

            bluePrintsUOW.SaveChanges();

            //Full refresh is required to pick up summary
            FullRefresh();
        }
        #endregion
    }
}