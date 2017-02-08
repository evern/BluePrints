using System;
using System.Linq;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using BluePrints.Common.Utils;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.DataModel;
using BluePrints.Data;
using BluePrints.Common.ViewModel;
using DevExpress.Xpf.Grid;
using BluePrints.Common;
using BluePrints.Common.Helpers;
using System.Collections.Generic;
using BluePrints.Data.Helpers;
using System.Collections.ObjectModel;
using DevExpress.Xpf.Grid.TreeList;
using System.Windows.Threading;
using BluePrints.Common.Projections;
using System.ComponentModel;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the COMMODITY_CODES collection view model.
    /// </summary>
    public partial class COMMODITY_CODEMasterDetailViewModelWrapper :
        CollectionViewModelsWrapper
        <COMMODITY_CODE, COMMODITY_CODEMasterDetailProjection, Guid, IBluePrintsEntitiesUnitOfWork,
            CollectionViewModel
            <COMMODITY_CODE, COMMODITY_CODEMasterDetailProjection, Guid, IBluePrintsEntitiesUnitOfWork>>
    {
        /// <summary>
        /// Creates a new instance of COMMODITY_CODESCollectionViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static COMMODITY_CODEMasterDetailViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new COMMODITY_CODEMasterDetailViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the COMMODITY_CODESCollectionViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the COMMODITY_CODESCollectionViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected COMMODITY_CODEMasterDetailViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private CommodityCodeType loadCommodityCodeType;
        private PROJECT loadPROJECT;

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        private BackgroundWorker refreshBackgroundWorker;
        private BackgroundWorker displayEntitiesRefreshBackgroundWorker;
        private BackgroundWorker userStateRestoreBackgroundWorker;

        public COMMODITY_CODEMasterDetailProjection SelectedEntity { get; set; }
        private ObservableCollection<COMMODITY_CODEMasterDetailProjection> selectedentities { get; set; }

        public ObservableCollection<COMMODITY_CODEMasterDetailProjection> SelectedEntities
        {
            get { return selectedentities; }
            set { selectedentities = value; }
        }

        private Guid RestoreSelectedEntityGuid;
        private List<Guid> RestoreSelectedEntitiesGuids = new List<Guid>();
        private List<Guid> RestoreExpandedGuids = new List<Guid>();

        private bool isProjectSpecific
        {
            get { return loadPROJECT != null; }
        }

        protected override void InitializeParameters(object parameter)
        {
            RestoreSelectedEntityGuid = Guid.Empty;
            SelectedEntities = new ObservableCollection<COMMODITY_CODEMasterDetailProjection>();

            refreshBackgroundWorker = new BackgroundWorker();
            refreshBackgroundWorker.DoWork += refreshBackgroundWorker_DoWork;
            refreshBackgroundWorker.WorkerSupportsCancellation = true;

            displayEntitiesRefreshBackgroundWorker = new BackgroundWorker();
            displayEntitiesRefreshBackgroundWorker.DoWork += displayEntitiesRefreshBackgroundWorker_DoWork;
            displayEntitiesRefreshBackgroundWorker.WorkerSupportsCancellation = true;

            userStateRestoreBackgroundWorker = new BackgroundWorker();
            userStateRestoreBackgroundWorker.DoWork += userStateRestoreBackgroundWorker_DoWork;
            userStateRestoreBackgroundWorker.WorkerSupportsCancellation = true;

            var receiveParameter =
                (OptionalEntitiesParameter<PROJECT, CommodityCodeTypeClass>) parameter;
            loadPROJECT = receiveParameter.GetFirstEntity();
            var loadCommodityCodeTypeClass = receiveParameter.GetSecondEntity();
            loadCommodityCodeType = loadCommodityCodeTypeClass.commodityCodeType;
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddEntitiesLoader<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>(1,
                bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, null, isContinueLoadingAfterPROJECT,
                OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(2,
                bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS, null, null, isContinueLoadingAfterDEPARTMENT,
                OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(3,
                bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES, null, null, isContinueLoadingAfterDISCIPLINE,
                OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<INDIRECT_TYPE, INDIRECT_TYPE, Guid, IBluePrintsEntitiesUnitOfWork>(4,
                bluePrintsUnitOfWorkFactory, x => x.INDIRECT_TYPES, null, null, null, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<UOM, UOM, Guid, IBluePrintsEntitiesUnitOfWork>(5,
                bluePrintsUnitOfWorkFactory, x => x.UOMS, null, null, null, OnAfterEntitiesChanged);
            InvokeEntitiesLoaderDescriptionLoading();
        }

        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            if (isProjectSpecific)
                return query => query.Where(x => x.GUID == loadPROJECT.GUID);
            else
                return query => query;
        }

        private bool isContinueLoadingAfterPROJECT(IEnumerable<PROJECT> entities)
        {
            if (isProjectSpecific && entities.Count() == 0)
            {
                mainThreadDispatcher.BeginInvoke(
                    new Action(
                        () =>
                            MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, "PROJECT"))));
                return false;
            }

            return true;
        }

        private bool isContinueLoadingAfterDEPARTMENT(IEnumerable<DEPARTMENT> entities)
        {
            if (entities.Count() == 0)
            {
                mainThreadDispatcher.BeginInvoke(
                    new Action(() => MessageBoxService.ShowMessage(CommonResources.CommodityCode_NoDepartment)));
                return false;
            }

            return true;
        }

        private bool isContinueLoadingAfterDISCIPLINE(IEnumerable<DISCIPLINE> entities)
        {
            if (entities.Count() == 0)
            {
                mainThreadDispatcher.BeginInvoke(
                    new Action(() => MessageBoxService.ShowMessage(CommonResources.CommodityCode_NoDiscipline)));
                return false;
            }

            return true;
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoader.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODEMasterDetailProjection>>
            ConstructMainViewModelProjection()
        {
            return
                query =>
                    COMMODITY_CODEMasterDetailProjectionQueries.transformCOMMODITY_CODE(
                        query.Where(x => x.COMMODITYCODETYPE == loadCommodityCodeType).OrderBy(x => x.FULLCODE));
        }

        #region View Refresh

        private List<Guid> SelectedEntitiesGuid = new List<Guid>();

        protected override void OnAfterEntitiesChanged(object key, Type changedType, EntityMessageType messageType,
            object sender)
        {
            if (changedType == typeof(COMMODITY_CODE))
            {
                storeViewState();

                if (sender.ToString() != MainViewModel.ToString())
                    mainThreadDispatcher.BeginInvoke(new Action(() => MainViewModel.Refresh()));
                else
                    mainThreadDispatcher.BeginInvoke(new Action(() => MainViewModel.RefreshWithoutClearingUndoManager()));

                if (!displayEntitiesRefreshBackgroundWorker.IsBusy)
                    displayEntitiesRefreshBackgroundWorker.RunWorkerAsync();

                return;
            }

            if (sender.ToString() == MainViewModel.ToString())
                return;


            if (loadPROJECT != null && changedType == typeof(PROJECT) && loadPROJECT.GUID.ToString() == key.ToString())
                if (messageType == EntityMessageType.Added)
                    MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Restored,
                        StringFormatUtils.GetEntityNameByType(changedType)));
                else if (messageType == EntityMessageType.Deleted)
                    MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed,
                        StringFormatUtils.GetEntityNameByType(changedType)));

            if (loadPROJECT != null)
                if (MainViewModel != null)
                    mainThreadDispatcher.BeginInvoke(new Action(() => MainViewModel.Refresh()));
                else if (loadPROJECT != null)
                    mainThreadDispatcher.BeginInvoke(new Action(() => InitializeAndLoadEntitiesLoaderDescription()));

            return;
        }

        private void refreshBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            System.Threading.Thread.Sleep(500);
            if (((BackgroundWorker) sender).CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertiesChanged()));
        }

        private void displayEntitiesRefreshBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            System.Threading.Thread.Sleep(100);
            if (((BackgroundWorker) sender).CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            displayEntities = null;
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertyChanged(x => x.DisplayEntities)));
        }

        private void userStateRestoreBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            System.Threading.Thread.Sleep(1);
            if (((BackgroundWorker) sender).CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            mainThreadDispatcher.BeginInvoke(new Action(() => restoreViewState()));
        }

        private void storeViewState()
        {
            RestoreSelectedEntityGuid = Guid.Empty;
            RestoreSelectedEntitiesGuids.Clear();
            RestoreExpandedGuids.Clear();

            foreach (var selectedEntity in SelectedEntities)
                RestoreSelectedEntitiesGuids.Add(new Guid(selectedEntity.GUID.ToString()));

            foreach (var entity in DisplayEntities)
                if (entity.ISEXPANDED)
                    RestoreExpandedGuids.Add(entity.GUID);

            if (SelectedEntity != null)
                RestoreSelectedEntityGuid = SelectedEntity.GUID;
        }

        private void restoreViewState()
        {
            var restoreSelectedEntities =
                DisplayEntities.Concat(DisplayEntities.SelectMany(x => x.CHILD_COMMODITY_CODES))
                    .Where(x => RestoreSelectedEntitiesGuids.Any(y => y == x.GUID));
            SelectedEntities.Clear();
            if (restoreSelectedEntities.Count() > 0)
                foreach (var restoreSelectedEntity in restoreSelectedEntities)
                    SelectedEntities.Add(restoreSelectedEntity);

            foreach (var expandedGuid in RestoreExpandedGuids)
            {
                var restoreExpandedEntity =
                    DisplayEntities.FirstOrDefault(x => x.GUID == expandedGuid);
                if (restoreExpandedEntity != null)
                    ExpandDisplayRow(restoreExpandedEntity);
            }

            if (RestoreSelectedEntityGuid != Guid.Empty)
            {
                var restoreSelectedEntity =
                    DisplayEntities.Concat(DisplayEntities.SelectMany(x => x.CHILD_COMMODITY_CODES))
                        .FirstOrDefault(x => x.GUID == RestoreSelectedEntityGuid);
                if (restoreSelectedEntity != null)
                    SelectedEntity = restoreSelectedEntity;
            }
        }

        #endregion

        #region Local Methods

        public Action<COMMODITY_CODEMasterDetailProjection> SetIsRowExpanded;

        private void DeleteChildrenCOMMODITY_CODE(COMMODITY_CODEMasterDetailProjection parentCOMMODITY_CODE)
        {
            foreach (
                var childCOMMODITY_CODE in parentCOMMODITY_CODE.CHILD_COMMODITY_CODES)
            {
                MainViewModel.EntitiesUndoRedoManager.AddUndo(childCOMMODITY_CODE, null, null, null,
                    EntityMessageType.Deleted);
                MainViewModel.Delete(childCOMMODITY_CODE);
            }
        }

        protected override void OnClose(CancelEventArgs e)
        {
            refreshBackgroundWorker.CancelAsync();
            displayEntitiesRefreshBackgroundWorker.CancelAsync();
            userStateRestoreBackgroundWorker.CancelAsync();
            base.OnClose(e);
        }

        #endregion

        protected override void AssignCallBacksAndRaisePropertyChange(
            IEnumerable<COMMODITY_CODEMasterDetailProjection> entities)
        {
            MainViewModel.OnBeforeEntitySavedCallBack = OnBeforeEntitiesSaved;
            MainViewModel.ApplyProjectionPropertiesToEntityCallBack = ApplyProjectionPropertiesToEntity;
            MainViewModel.OnEntitySavedCallBack = OnEntitiesSavedCallBack;
            MainViewModel.SetParentViewModel(this);
            var initializeCOMMODITY_GROUP = COMMODITY_GROUPCollectionViewModel;

            if (ShowDEPARTMENT != null && ShowDISCIPLINE != null && ShowDIRECT_RATES != null &&
                ShowINDIRECT_TYPE != null && ShowINDIRECT_RATES != null)
                if (loadCommodityCodeType == CommodityCodeType.Design)
                {
                    mainThreadDispatcher.BeginInvoke(new Action(() => ShowDEPARTMENT()));
                }
                else if (loadCommodityCodeType == CommodityCodeType.Direct)
                {
                    mainThreadDispatcher.BeginInvoke(new Action(() => ShowDISCIPLINE()));
                    mainThreadDispatcher.BeginInvoke(new Action(() => ShowDIRECT_RATES()));
                }
                else
                {
                    mainThreadDispatcher.BeginInvoke(new Action(() => ShowINDIRECT_TYPE()));
                    mainThreadDispatcher.BeginInvoke(new Action(() => ShowINDIRECT_RATES()));
                }

            refreshBackgroundWorker.RunWorkerAsync();
        }

        #region Collection Call Backs

        public void OnEntitiesSavedCallBack(Guid primaryKey, COMMODITY_CODEMasterDetailProjection projectionEntity,
            COMMODITY_CODE entity, bool isNewEntity)
        {
            projectionEntity.GUID = entity.GUID;
            projectionEntity.COMMODITY_CODE.GUID = entity.GUID;
        }

        public void ApplyProjectionPropertiesToEntity(COMMODITY_CODEMasterDetailProjection projectionEntity,
            COMMODITY_CODE entity)
        {
            //projectionEntity.COMMODITY_CODE.GUID_PROJECT = loadPROJECT.GUID;
            DataUtils.ShallowCopy(entity, projectionEntity.COMMODITY_CODE);
            //workaround for created because Save() only sets the projection primary key, this is used for property redo where the interceptor only tampers with UPDATED and CREATED is left as null
            if (entity.CREATED.Date.Year == 1)
                projectionEntity.COMMODITY_CODE.CREATED = DateTime.Now;

            entity.CREATED = projectionEntity.COMMODITY_CODE.CREATED;
        }

        private void OnBeforeEntitiesSaved(COMMODITY_CODE entity)
        {
            entity.COMMODITYCODETYPE = loadCommodityCodeType;
        }

        #endregion

        #endregion

        #region View Behavior

        public Action ShowDISCIPLINE;
        public Action ShowDEPARTMENT;
        public Action ShowINDIRECT_TYPE;
        public Action ShowHOURSAWEEK;
        public Action ShowDIRECT_RATES;
        public Action ShowINDIRECT_RATES;

        #endregion

        #region View Commands

        public void MasterRowExpanded(RowEventArgs e)
        {
            ((COMMODITY_CODEMasterDetailProjection) e.Row).ISEXPANDED = true;
        }

        public void MasterRowCollapsed(RowEventArgs e)
        {
            ((COMMODITY_CODEMasterDetailProjection) e.Row).ISEXPANDED = false;
        }

        private void ExpandDisplayRow(COMMODITY_CODEMasterDetailProjection row)
        {
            row.ISEXPANDED = true;
            if (SetIsRowExpanded != null)
                SetIsRowExpanded(row);
        }

        public virtual bool CanBulkDelete()
        {
            return MainViewModel != null && MainViewModel.Entities != null && MainViewModel.Entities.Count > 0 &&
                   !IsLoading && SelectedEntities.Count > 0 &&
                   !SelectedEntities.Any(x => x.COMMODITY_CODE.GUID_PROJECT == null);
        }

        public void BulkDelete()
        {
            MainViewModel.EntitiesUndoRedoManager.PauseActionId();
            var deletingEntities =
                new List<COMMODITY_CODEMasterDetailProjection>();
            foreach (var selectedEntity in selectedentities)
                if (selectedEntity.COMMODITY_CODE.GUID == Guid.Empty)
                    foreach (var childrenEntity in selectedEntity.CHILD_COMMODITY_CODES
                    )
                        deletingEntities.Add(childrenEntity);
                else
                    deletingEntities.Add(selectedEntity);
            MainViewModel.BaseBulkDelete(deletingEntities);
            MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
        }

        #endregion

        #region View Properties

        public IEnumerable<PROJECT> PROJECTCollection
        {
            get { return GetEntities<PROJECT>(); }
        }

        private ObservableCollection<COMMODITY_CODEMasterDetailProjection> displayEntities;

        public ObservableCollection<COMMODITY_CODEMasterDetailProjection> DisplayEntities
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                if (displayEntities == null)
                {
                    displayEntities = new ObservableCollection<COMMODITY_CODEMasterDetailProjection>();

                    var projectSpecificCOMMODITY_CODEGrouped =
                        MainViewModel.Entities.Where(
                            x =>
                                (isProjectSpecific
                                    ? x.COMMODITY_CODE.GUID_PROJECT == loadPROJECT.GUID
                                    : x.COMMODITY_CODE.GUID_PROJECT != null) && x.COMMODITY_CODE.GUID_COMMODITY_GROUP_DIRECT != null && x.COMMODITY_CODE.COMMODITY_GROUP_DIRECT_ID != null).GroupBy(x => x.GROUP_ID);

                    var projectSpecificCOMMODITY_CODENotGrouped =
                        MainViewModel.Entities.Where(
                            x =>
                                (isProjectSpecific
                                    ? x.COMMODITY_CODE.GUID_PROJECT == loadPROJECT.GUID
                                    : x.COMMODITY_CODE.GUID_PROJECT != null) &&
                                x.COMMODITY_CODE.GUID_COMMODITY_GROUP_DIRECT == null &&
                                x.COMMODITY_CODE.ISQUANTIFIABLE == true);
                    var generalCOMMODITY_CODENotGrouped =
                        MainViewModel.Entities.Where(
                            x => x.COMMODITY_CODE.GUID_PROJECT == null && x.COMMODITY_CODE.ISQUANTIFIABLE == true);

                    foreach (var group in projectSpecificCOMMODITY_CODEGrouped)
                    {
                        var firstItemInGroup = group.First();
                        var parentProjectionPOCO =
                            ViewModelSource.Create(() => new COMMODITY_CODEMasterDetailProjection());
                        parentProjectionPOCO.GUID = Guid.NewGuid();
                        parentProjectionPOCO.COMMODITY_CODE.GUID = Guid.Empty;
                            //this is used by COMMODITY_GROUP_CODE_SELECTION to determine whether selection is group or code
                        parentProjectionPOCO.COMMODITY_CODE.GUID_PROJECT = firstItemInGroup.COMMODITY_CODE.GUID_PROJECT;
                        parentProjectionPOCO.COMMODITY_CODE.FULLCODE =
                            firstItemInGroup.COMMODITY_CODE.COMMODITY_GROUP_DESC;
                        parentProjectionPOCO.COMMODITY_CODE.GUID_COMMODITY_GROUP_DIRECT =
                            firstItemInGroup.COMMODITY_CODE.GUID_COMMODITY_GROUP_DIRECT;
                        parentProjectionPOCO.COMMODITY_CODE.COMMODITY_GROUP_DIRECT_ID =
                            firstItemInGroup.COMMODITY_CODE.COMMODITY_GROUP_DIRECT_ID;

                        foreach (var item in group)
                        {
                            var childProjectionPOCO =
                                ViewModelSource.Create(() => new COMMODITY_CODEMasterDetailProjection());
                            DataUtils.ShallowCopy(childProjectionPOCO.COMMODITY_CODE, item.COMMODITY_CODE);
                            childProjectionPOCO.GUID = item.COMMODITY_CODE.GUID;
                            childProjectionPOCO.IsEditable = true;
                            childProjectionPOCO.ProjectionType = COMMODITY_CODEProjectionType.ProjectSpecificGrouped;
                            parentProjectionPOCO.CHILD_COMMODITY_CODES.Add(childProjectionPOCO);
                        }

                        parentProjectionPOCO.COMMODITY_CODE.RATE_SUPPLY =
                            parentProjectionPOCO.CHILD_COMMODITY_CODES.Where(x => x.COMMODITY_CODE.RATE_SUPPLY != null)
                                .Sum(x => x.COMMODITY_CODE.RATE_SUPPLY);
                        parentProjectionPOCO.COMMODITY_CODE.RATE_FREIGHT =
                            parentProjectionPOCO.CHILD_COMMODITY_CODES.Where(x => x.COMMODITY_CODE.RATE_FREIGHT != null)
                                .Sum(x => x.COMMODITY_CODE.RATE_FREIGHT);
                        parentProjectionPOCO.COMMODITY_CODE.RATE_PLANT =
                            parentProjectionPOCO.CHILD_COMMODITY_CODES.Where(x => x.COMMODITY_CODE.RATE_PLANT != null)
                                .Sum(x => x.COMMODITY_CODE.RATE_PLANT);
                        parentProjectionPOCO.COMMODITY_CODE.HOURS_INSTALL =
                            parentProjectionPOCO.CHILD_COMMODITY_CODES.Where(x => x.COMMODITY_CODE.HOURS_INSTALL != null)
                                .Sum(x => x.COMMODITY_CODE.HOURS_INSTALL);

                        parentProjectionPOCO.IsEditable = false;
                        parentProjectionPOCO.ProjectionType = COMMODITY_CODEProjectionType.ProjectSpecificGrouped;
                        displayEntities.Add(parentProjectionPOCO);
                    }

                    foreach (
                        var COMMODITY_CODEProjection in
                        projectSpecificCOMMODITY_CODENotGrouped)
                    {
                        var projectionPOCO =
                            ViewModelSource.Create(() => new COMMODITY_CODEMasterDetailProjection());
                        DataUtils.ShallowCopy(projectionPOCO.COMMODITY_CODE, COMMODITY_CODEProjection.COMMODITY_CODE);
                        projectionPOCO.GUID = COMMODITY_CODEProjection.GUID;

                        projectionPOCO.IsEditable = true;
                        projectionPOCO.ProjectionType = COMMODITY_CODEProjectionType.ProjectSpecificNotGrouped;
                        displayEntities.Add(projectionPOCO);
                    }

                    foreach (
                        var COMMODITY_GROUPEntity in
                        COMMODITY_GROUPCollectionViewModel.DisplayEntities)
                    {
                        var parentProjectionPOCO =
                            ViewModelSource.Create(() => new COMMODITY_CODEMasterDetailProjection());
                        parentProjectionPOCO.GUID = Guid.NewGuid();
                        parentProjectionPOCO.COMMODITY_CODE.GUID = Guid.Empty;
                            //this is used by COMMODITY_GROUP_CODE_SELECTION to determine whether selection is group or code
                        parentProjectionPOCO.COMMODITY_CODE.FULLCODE = COMMODITY_GROUPEntity.COMMODITY_GROUP.DESCRIPTION;
                        parentProjectionPOCO.COMMODITY_CODE.GUID_COMMODITY_GROUP_DIRECT = COMMODITY_GROUPEntity.GUID;

                        foreach (
                            var childCOMMODITY_GROUPEntity in
                            COMMODITY_GROUPEntity.CHILD_COMMODITY_GROUP)
                        {
                            var childProjectionPOCO =
                                ViewModelSource.Create(() => new COMMODITY_CODEMasterDetailProjection());
                            var findCOMMODITY_CODE =
                                generalCOMMODITY_CODENotGrouped.FirstOrDefault(
                                    x =>
                                        x.COMMODITY_CODE.GUID ==
                                        childCOMMODITY_GROUPEntity.COMMODITY_GROUP.GUID_COMMODITYCODE);
                            if (findCOMMODITY_CODE != null)
                            {
                                DataUtils.ShallowCopy(childProjectionPOCO.COMMODITY_CODE,
                                    findCOMMODITY_CODE.COMMODITY_CODE);
                                childProjectionPOCO.GUID = findCOMMODITY_CODE.COMMODITY_CODE.GUID;
                                childProjectionPOCO.COMMODITY_CODE.GUID_COMMODITY_GROUP_DIRECT =
                                    COMMODITY_GROUPEntity.GUID;

                                childProjectionPOCO.IsEditable = true;
                                childProjectionPOCO.ProjectionType = COMMODITY_CODEProjectionType.GeneralGrouped;
                                parentProjectionPOCO.CHILD_COMMODITY_CODES.Add(childProjectionPOCO);
                            }
                        }

                        parentProjectionPOCO.COMMODITY_CODE.RATE_SUPPLY =
                            parentProjectionPOCO.CHILD_COMMODITY_CODES.Where(x => x.COMMODITY_CODE.RATE_SUPPLY != null)
                                .Sum(x => x.COMMODITY_CODE.RATE_SUPPLY);
                        parentProjectionPOCO.COMMODITY_CODE.RATE_FREIGHT =
                            parentProjectionPOCO.CHILD_COMMODITY_CODES.Where(x => x.COMMODITY_CODE.RATE_FREIGHT != null)
                                .Sum(x => x.COMMODITY_CODE.RATE_FREIGHT);
                        parentProjectionPOCO.COMMODITY_CODE.RATE_PLANT =
                            parentProjectionPOCO.CHILD_COMMODITY_CODES.Where(x => x.COMMODITY_CODE.RATE_PLANT != null)
                                .Sum(x => x.COMMODITY_CODE.RATE_PLANT);
                        parentProjectionPOCO.COMMODITY_CODE.HOURS_INSTALL =
                            parentProjectionPOCO.CHILD_COMMODITY_CODES.Where(x => x.COMMODITY_CODE.HOURS_INSTALL != null)
                                .Sum(x => x.COMMODITY_CODE.HOURS_INSTALL);

                        parentProjectionPOCO.IsEditable = false;
                        parentProjectionPOCO.ProjectionType = COMMODITY_CODEProjectionType.GeneralGrouped;
                        displayEntities.Add(parentProjectionPOCO);
                    }

                    foreach (
                        var COMMODITY_CODEProjection in generalCOMMODITY_CODENotGrouped
                    )
                    {
                        var projectionPOCO =
                            ViewModelSource.Create(() => new COMMODITY_CODEMasterDetailProjection());
                        DataUtils.ShallowCopy(projectionPOCO.COMMODITY_CODE, COMMODITY_CODEProjection.COMMODITY_CODE);
                        projectionPOCO.GUID = COMMODITY_CODEProjection.COMMODITY_CODE.GUID;

                        projectionPOCO.IsEditable = true;
                        projectionPOCO.ProjectionType = COMMODITY_CODEProjectionType.GeneralNotGrouped;
                        displayEntities.Add(projectionPOCO);
                    }

                    if (!userStateRestoreBackgroundWorker.IsBusy)
                        userStateRestoreBackgroundWorker.RunWorkerAsync();
                }

                return displayEntities;
            }
        }

        private COMMODITY_GROUP_DIRECTCollectionViewModelWrapper commodity_groupCollectionViewModel;

        public COMMODITY_GROUP_DIRECTCollectionViewModelWrapper COMMODITY_GROUPCollectionViewModel
        {
            get
            {
                if (commodity_groupCollectionViewModel == null)
                {
                    commodity_groupCollectionViewModel = COMMODITY_GROUP_DIRECTCollectionViewModelWrapper.Create();
                    commodity_groupCollectionViewModel.SetParentViewModel(this);
                    var baselineSupportParameterObj =
                        commodity_groupCollectionViewModel as ISupportParameter;
                    baselineSupportParameterObj.Parameter =
                        new OptionalEntitiesParameter<PROJECT, CommodityCodeTypeClass>(loadPROJECT,
                            new CommodityCodeTypeClass(CommodityCodeType.Direct));
                }

                return commodity_groupCollectionViewModel;
            }
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "COMMODITY_CODEMasterDetailViewModelWrapper"; }
        }

        public IEnumerable<DEPARTMENT> DEPARTMENTCollection
        {
            get
            {
                var collection = GetEntities<DEPARTMENT>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        public IEnumerable<DISCIPLINE> DISCIPLINECollection
        {
            get
            {
                var collection = GetEntities<DISCIPLINE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        public IEnumerable<UOM> UOMCollection
        {
            get
            {
                var collection = GetEntities<UOM>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.UOM1);
                return collection;
            }
        }

        public IEnumerable<INDIRECT_TYPE> INDIRECT_TYPECollection
        {
            get
            {
                var collection = GetEntities<INDIRECT_TYPE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        #endregion
    }
}