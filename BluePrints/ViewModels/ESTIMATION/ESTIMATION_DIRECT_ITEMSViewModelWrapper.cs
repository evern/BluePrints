using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using BluePrints.Common.Utils;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.DataModel;
using BluePrints.Data;
using BluePrints.Common.ViewModel;
using DevExpress.Xpf.Grid;
using System.Threading;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data.Helpers;
using BluePrints.Common;
using BluePrints.Common.Helpers;
using BluePrints.Common.Projections;
using System.Windows.Threading;
using System.Windows;
using DevExpress.Xpf.Bars;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Views;
using BluePrints.Reports;
using System.IO;
using DevExpress.Xpf.Printing;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Settings;
using System.ComponentModel;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single ESTIMATION_DIRECT object view model.
    /// </summary>
    public partial class ESTIMATION_DIRECT_ITEMSViewModelWrapper :
        CollectionViewModelsWrapper
        <ESTIMATION_DIRECT_ITEM, ESTIMATION_DIRECT_ITEMProjection, Guid, IBluePrintsEntitiesUnitOfWork,
            CollectionViewModel
            <ESTIMATION_DIRECT_ITEM, ESTIMATION_DIRECT_ITEMProjection, Guid, IBluePrintsEntitiesUnitOfWork>>
    {
        public Action ShowWORKPACKInternalName1;
        public Action ShowWORKPACKInternalName2;

        /// <summary>
        /// Creates a new instance of ESTIMATION_DIRECT_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static ESTIMATION_DIRECT_ITEMSViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new ESTIMATION_DIRECT_ITEMSViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the ESTIMATION_DIRECTViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the ESTIMATION_DIRECTViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected ESTIMATION_DIRECT_ITEMSViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private PROJECT loadPROJECT;
        private ESTIMATION_DIRECT loadESTIMATION_DIRECT;

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        private BackgroundWorker refreshBackgroundWorker;
        private BackgroundWorker displayEntitiesRefreshBackgroundWorker;
        private BackgroundWorker userStateStoreBackgroundWorker;
        private BackgroundWorker userStateRestoreBackgroundWorker;

        public bool TryUsingProjectRates { get; set; }
        public ESTIMATION_DIRECT_ITEMProjection SelectedEntity { get; set; }
        private ObservableCollection<ESTIMATION_DIRECT_ITEMProjection> selectedentities { get; set; }

        public ObservableCollection<ESTIMATION_DIRECT_ITEMProjection> SelectedEntities
        {
            get { return selectedentities; }
            set { selectedentities = value; }
        }

        private Guid RestoreSelectedEntityGuid;
        private List<Guid> RestoreSelectedEntitiesGuids = new List<Guid>();
        private List<Guid> RestoreExpandedGuids = new List<Guid>();

        protected override void InitializeParameters(object parameter)
        {
            RestoreSelectedEntityGuid = Guid.Empty;
            SelectedEntities = new ObservableCollection<ESTIMATION_DIRECT_ITEMProjection>();
            userStateRestoreBackgroundWorker = new BackgroundWorker();
            userStateRestoreBackgroundWorker.DoWork += userStateRestoreBackgroundWorker_DoWork;
            userStateRestoreBackgroundWorker.WorkerSupportsCancellation = true;

            userStateStoreBackgroundWorker = new BackgroundWorker();
            userStateStoreBackgroundWorker.DoWork += userStateStoreBackgroundWorker_DoWork;
            userStateStoreBackgroundWorker.WorkerSupportsCancellation = true;

            refreshBackgroundWorker = new BackgroundWorker();
            refreshBackgroundWorker.DoWork += refreshBackgroundWorker_DoWork;
            refreshBackgroundWorker.WorkerSupportsCancellation = true;

            displayEntitiesRefreshBackgroundWorker = new BackgroundWorker();
            displayEntitiesRefreshBackgroundWorker.DoWork += displayEntitiesRefreshBackgroundWorker_DoWork;
            displayEntitiesRefreshBackgroundWorker.WorkerSupportsCancellation = true;

            var receiveParameter =
                (OptionalEntitiesParameter<PROJECT, ESTIMATION_DIRECT>) parameter;
            loadPROJECT = receiveParameter.GetFirstEntity();
            loadESTIMATION_DIRECT = receiveParameter.GetSecondEntity();
        }

        private bool isQueryForLiveStatus
        {
            get { return loadPROJECT != null; }
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddEntitiesLoader<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>(0,
                bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, null, isContinueLoadingAfterPROJECT,
                OnAfterEntitiesChanged);
            loaderCollection
                .AddEntitiesLoader<ESTIMATION_DIRECT, ESTIMATION_DIRECT, Guid, IBluePrintsEntitiesUnitOfWork>(1,
                    bluePrintsUnitOfWorkFactory, x => x.ESTIMATION_DIRECTS, ESTIMATION_DIRECTProjectionFunc,
                    typeof(PROJECT), isContinueLoadingAfterESTIMATION_DIRECT, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<WORKPACK, WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork>(2,
                bluePrintsUnitOfWorkFactory, x => x.WORKPACKS, WORKPACKProjectionFunc, typeof(PROJECT), null,
                OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(3,
                bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddEntitiesLoader<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(4,
                bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection
                .AddEntitiesLoader<COMMODITY_GROUP_DIRECT, COMMODITY_GROUP_DIRECT, Guid, IBluePrintsEntitiesUnitOfWork>(
                    5, bluePrintsUnitOfWorkFactory, x => x.COMMODITY_GROUP_DIRECT, COMMODITY_GROUP_DIRECTProjectionFunc,
                    typeof(PROJECT), null, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<COMMODITY_CODE, COMMODITY_CODE, Guid, IBluePrintsEntitiesUnitOfWork>(6,
                bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, COMMODITY_CODEProjectionFunc, null, null,
                OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<PROJECT_REPORT, PROJECT_REPORT, Guid, IBluePrintsEntitiesUnitOfWork>(7,
                bluePrintsUnitOfWorkFactory, x => x.PROJECT_REPORTS, PROJECT_REPORTProjectionFunc,
                typeof(PROJECT));
            loaderCollection.AddEntitiesLoader<RATE, RATE, Guid, IBluePrintsEntitiesUnitOfWork>(8,
                bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc, typeof(PROJECT), null,
                OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<UOM, UOM, Guid, IBluePrintsEntitiesUnitOfWork>(9,
                bluePrintsUnitOfWorkFactory, x => x.UOMS, null, null, null, OnAfterEntitiesChanged);

            InvokeEntitiesLoaderDescriptionLoading();
        }

        private bool isContinueLoadingAfterPROJECT(IEnumerable<PROJECT> entities)
        {
            if (!entities.Any())
            {
                mainThreadDispatcher.BeginInvoke(
                    new Action(
                        () =>
                            MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, "PROJECT"))));
                return false;
            }

            loadPROJECT = entities.First();
            return true;
        }

        private bool isContinueLoadingAfterESTIMATION_DIRECT(IEnumerable<ESTIMATION_DIRECT> entities)
        {
            if (!entities.Any())
            {
                mainThreadDispatcher.BeginInvoke(
                    new Action(
                        () =>
                            MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed,
                                "ESTIMATION_DIRECT"))));
                return false;
            }

            loadESTIMATION_DIRECT = entities.First();
            return true;
        }

        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            if (isQueryForLiveStatus)
                return query => query.Where(x => x.GUID == loadPROJECT.GUID);
            else
                return query => query.Where(x => x.GUID == loadESTIMATION_DIRECT.GUID_PROJECT);
        }

        private Func<IRepositoryQuery<ESTIMATION_DIRECT>, IQueryable<ESTIMATION_DIRECT>> ESTIMATION_DIRECTProjectionFunc()
        {
            if (isQueryForLiveStatus)
                return
                    query =>
                        query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == EstimationStatus.Live);
            else
                return query => query.Where(x => x.GUID == loadESTIMATION_DIRECT.GUID);
        }

        private Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK>> WORKPACKProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.TYPE == WorkpackType.Design);
        }

        private Func<IRepositoryQuery<PHASE>, IQueryable<PHASE>> PHASEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.COST_GROUP == CostGroup.Site);
        }

        private Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>> COMMODITY_CODEProjectionFunc()
        {
            return query => query.Where(x => x.COMMODITYCODETYPE == CommodityCodeType.Direct);
        }

        private Func<IRepositoryQuery<COMMODITY_GROUP_DIRECT>, IQueryable<COMMODITY_GROUP_DIRECT>>
            COMMODITY_GROUP_DIRECTProjectionFunc()
        {
            return query => query;
        }

        private Func<IRepositoryQuery<PROJECT_REPORT>, IQueryable<PROJECT_REPORT>> PROJECT_REPORTProjectionFunc()
        {
            return query => query;
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.ESTIMATION_DIRECT_ITEMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoader.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<ESTIMATION_DIRECT_ITEM>, IQueryable<ESTIMATION_DIRECT_ITEMProjection>>
            ConstructMainViewModelProjection()
        {
            var getDEPARTMENTSFunc = loaderCollection.GetCollectionFunc<DEPARTMENT>();
            var getRATESFunc = loaderCollection.GetCollectionFunc<RATE>();
            var getESTIMATION_DIRECTFunc = loaderCollection.GetObjectFunc<ESTIMATION_DIRECT>();
            return
                query =>
                    ESTIMATION_DIRECT_ITEMProjectionQueries.JoinRATESOnESTIMATION_DIRECT_ITEMS(query,
                        getESTIMATION_DIRECTFunc, getDEPARTMENTSFunc, getRATESFunc);
        }

        #region View Refresh

        private List<Guid> SelectedEntitiesGuid = new List<Guid>();

        protected override void OnAfterEntitiesChanged(object key, Type changedType, EntityMessageType messageType,
            object sender)
        {
            if (changedType == typeof(COMMODITY_CODE) || changedType == typeof(COMMODITY_GROUP_DIRECT) ||
                changedType == typeof(ESTIMATION_DIRECT_ITEM))
            {
                if (!displayEntitiesRefreshBackgroundWorker.IsBusy)
                {
                    storeViewState();
                    if (changedType == typeof(COMMODITY_CODE))
                    {
                        mainThreadDispatcher.BeginInvoke(new Action(() => COMMODITY_CODECollectionViewModel.Refresh()));
                    }
                    else
                    {
                        if (sender.ToString() != MainViewModel.ToString())
                            mainThreadDispatcher.BeginInvoke(new Action(() => MainViewModel.Refresh()));
                        else
                            mainThreadDispatcher.BeginInvoke(
                                new Action(() => MainViewModel.RefreshWithoutClearingUndoManager()));
                    }

                    displayEntitiesRefreshBackgroundWorker.RunWorkerAsync();
                }

                return;
            }

            if (sender.ToString() == MainViewModel.ToString())
                return;

            if (changedType == typeof(WORKPACK))
            {
                this.RaisePropertyChanged(x => x.WORKPACKCollection);
                return;
            }

            if (loadESTIMATION_DIRECT != null && changedType == typeof(ESTIMATION_DIRECT) &&
                loadESTIMATION_DIRECT.GUID.ToString() == key.ToString() ||
                loadPROJECT != null && changedType == typeof(PROJECT) && loadPROJECT.GUID.ToString() == key.ToString())
                if (messageType == EntityMessageType.Added)
                    MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Restored,
                        StringFormatUtils.GetEntityNameByType(changedType)));
                else if (messageType == EntityMessageType.Deleted)
                    MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed,
                        StringFormatUtils.GetEntityNameByType(changedType)));

            if (loadPROJECT != null || loadESTIMATION_DIRECT != null)
                if (MainViewModel != null)
                    mainThreadDispatcher.BeginInvoke(new Action(() => MainViewModel.Refresh()));
                else if (loadPROJECT != null || loadESTIMATION_DIRECT != null)
                    mainThreadDispatcher.BeginInvoke(new Action(() => InitializeAndLoadEntitiesLoaderDescription()));

            return;
        }

        private void refreshBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(500);
            if (((BackgroundWorker) sender).CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertiesChanged()));
        }

        private void displayEntitiesRefreshBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(100);
            if (((BackgroundWorker) sender).CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            displayEntities = null;
            mainThreadDispatcher.BeginInvoke(
                new Action(() => this.RaisePropertyChanged(x => x.COMMODITY_GROUP_MasterDetailDisplayCollection)));
            mainThreadDispatcher.BeginInvoke(
                new Action(
                    () =>
                        this.RaisePropertyChanged(
                            x => x.COMMODITY_GROUP_MasterDetailFlatProjectSpecificDisplayCollection)));
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertyChanged(x => x.DisplayEntities)));
        }

        private void userStateStoreBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(10);
            var changedType = (Type) e.Argument;
            if (((BackgroundWorker) sender).CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            mainThreadDispatcher.BeginInvoke(new Action(() => storeViewState()));
        }

        private void userStateRestoreBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(1);
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
                DisplayEntities.Concat(DisplayEntities.SelectMany(x => x.CHILD_ESTIMATION_DIRECT_ITEM))
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
                    DisplayEntities.Concat(DisplayEntities.SelectMany(x => x.CHILD_ESTIMATION_DIRECT_ITEM))
                        .FirstOrDefault(x => x.GUID == RestoreSelectedEntityGuid);
                if (restoreSelectedEntity != null)
                    SelectedEntity = restoreSelectedEntity;
            }
        }

        #endregion

        protected override void AssignCallBacksAndRaisePropertyChange(
            IEnumerable<ESTIMATION_DIRECT_ITEMProjection> entities)
        {
            MainViewModel.CreateNewProjectionFromNewEntityCallBack = CreateNewProjectionFromNewEntityCallBack;
            MainViewModel.ApplyProjectionPropertiesToEntityCallBack = ApplyProjectionPropertiesToEntity;
            MainViewModel.OnEntitySavedCallBack = OnEntitiesSavedCallBack;
            MainViewModel.ExistingRowAddUndoAndSaveCallBack = ExistingProjectionEditCallBack;
            MainViewModel.NewRowAddUndoAndBeforeSaveCallBack = NewRowAddUndoAndBeforeSaveCallBack;
            MainViewModel.NewRowAddUndoAndAfterSavedCallBack = NewRowAddUndoAndAfterSavedCallBack;
            MainViewModel.EntitiesBeforeDeletionCallBack = EntitiesBeforeDeletion;
            MainViewModel.SetParentViewModel(this);

            var initializeCOMMODITY_CODEMasterDetailView = COMMODITY_CODEMasterDetailViewModel;
            refreshBackgroundWorker.RunWorkerAsync();
        }

        #region Collection Call Backs

        public void ApplyProjectionPropertiesToEntity(ESTIMATION_DIRECT_ITEMProjection projectionEntity,
            ESTIMATION_DIRECT_ITEM entity)
        {
            projectionEntity.ESTIMATION_DIRECT_ITEM.GUID_ESTIMATION_DIRECT = loadESTIMATION_DIRECT.GUID;
            DataUtils.ShallowCopy(entity, projectionEntity.ESTIMATION_DIRECT_ITEM);
            //workaround for created because Save() only sets the projection primary key, this is used for property redo where the interceptor only tampers with UPDATED and CREATED is left as null
            if (entity.CREATED.Date.Year == 1)
                projectionEntity.ESTIMATION_DIRECT_ITEM.CREATED = DateTime.Now;

            entity.CREATED = projectionEntity.ESTIMATION_DIRECT_ITEM.CREATED;
        }

        public void OnEntitiesSavedCallBack(Guid primaryKey, ESTIMATION_DIRECT_ITEMProjection projectionEntity,
            ESTIMATION_DIRECT_ITEM entity, bool isNewEntity)
        {
            projectionEntity.GUID = entity.GUID;
            projectionEntity.ESTIMATION_DIRECT_ITEM.GUID = entity.GUID;
            projectionEntity.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL = entity.GUID_ORIGINAL;
        }

        public bool NewRowAddUndoAndBeforeSaveCallBack(RowEventArgs e, ESTIMATION_DIRECT_ITEMProjection projectionEntity)
        {
            projectionEntity.ESTIMATION_DIRECT_ITEM.ESTIMATED_QUANTITY = 1;
            NewRowAndExistingAddUndoAndSave(projectionEntity, true);
            return true;
        }

        public void NewRowAddUndoAndAfterSavedCallBack(RowEventArgs e, ESTIMATION_DIRECT_ITEMProjection projectionEntity)
        {
            //Needs to be after so projectionEntity is guaranteed to have GUID generated
            if (projectionEntity.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_GROUP_DIRECT != null)
                AddChildrenESTIMATION_DIRECT_ITEM(projectionEntity);
        }

        public bool ExistingProjectionEditCallBack(ESTIMATION_DIRECT_ITEMProjection projectionEntity,
            CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName ==
                BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().ESTIMATION_DIRECT_ITEM) + "." +
                BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().RATE_FREIGHT)
                ||
                e.Column.FieldName ==
                BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().ESTIMATION_DIRECT_ITEM) + "." +
                BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().RATE_SUPPLY)
                ||
                e.Column.FieldName ==
                BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().ESTIMATION_DIRECT_ITEM) + "." +
                BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().HOURS_INSTALL))
            {
                var childrenManaged = AddProjectSpecificCOMMODITY_CODEGroup(projectionEntity);
                if (childrenManaged)
                {
                    MainViewModel.Save(projectionEntity);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            if (e.Column.FieldName ==
                BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().COMMODITY_GROUP_CODE_SELECTION)
                ||
                e.Column.FieldName ==
                BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().ESTIMATION_DIRECT_ITEM) + "." +
                BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().GUID_COMMODITY_CODE))
                NewRowAndExistingAddUndoAndSave(projectionEntity, false);
            else if (e.Column.FieldName ==
                     BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().ESTIMATION_DIRECT_ITEM) + "." +
                     BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().ESTIMATED_QUANTITY))
                MultiplyChildrenEntityRates(projectionEntity, (decimal) e.OldValue, (decimal) e.Value);
            else if (e.Column.FieldName ==
                     BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().ESTIMATION_DIRECT_ITEM) + "." +
                     BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().GUID_INSTALLWORKPACK))
                foreach (
                    var childrenEntity in projectionEntity.CHILD_ESTIMATION_DIRECT_ITEM)
                {
                    var oldValue = childrenEntity.ESTIMATION_DIRECT_ITEM.GUID_INSTALLWORKPACK;
                    var newValue = (Guid?) e.Value;

                    childrenEntity.ESTIMATION_DIRECT_ITEM.GUID_INSTALLWORKPACK = newValue;
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(childrenEntity, e.Column.FieldName, oldValue, newValue,
                        EntityMessageType.Changed);
                    MainViewModel.Save(childrenEntity);
                }
            else if (e.Column.FieldName ==
                     BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().ESTIMATION_DIRECT_ITEM) + "." +
                     BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().GUID_SUPPLYWORKPACK))
                foreach (
                    var childrenEntity in projectionEntity.CHILD_ESTIMATION_DIRECT_ITEM)
                {
                    var oldValue = childrenEntity.ESTIMATION_DIRECT_ITEM.GUID_SUPPLYWORKPACK;
                    var newValue = (Guid?) e.Value;

                    childrenEntity.ESTIMATION_DIRECT_ITEM.GUID_SUPPLYWORKPACK = newValue;
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(childrenEntity, e.Column.FieldName, oldValue, newValue,
                        EntityMessageType.Changed);
                    MainViewModel.Save(childrenEntity);
                }
            else if (e.Column.FieldName ==
                     BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().ESTIMATION_DIRECT_ITEM) + "." +
                     BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().GUID_DISCIPLINE))
                foreach (
                    var childrenEntity in projectionEntity.CHILD_ESTIMATION_DIRECT_ITEM)
                {
                    var oldValue = childrenEntity.ESTIMATION_DIRECT_ITEM.GUID_DISCIPLINE;
                    var newValue = (Guid?) e.Value;

                    childrenEntity.ESTIMATION_DIRECT_ITEM.GUID_DISCIPLINE = newValue;
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(childrenEntity, e.Column.FieldName, oldValue, newValue,
                        EntityMessageType.Changed);
                    MainViewModel.Save(childrenEntity);
                }


            return true;
        }

        private void NewRowAndExistingAddUndoAndSave(ESTIMATION_DIRECT_ITEMProjection projectionEntity, bool isNewRow)
        {
            if (projectionEntity.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_CODE != null)
            {
                COMMODITY_CODE findCOMMODITY_CODE;
                findCOMMODITY_CODE =
                    COMMODITY_CODECollection.FirstOrDefault(
                        x => x.GUID == projectionEntity.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_CODE);

                if (findCOMMODITY_CODE != null)
                {
                    object oldValue = projectionEntity.ESTIMATION_DIRECT_ITEM.ESTIMATED_QUANTITY;
                    object newValue = projectionEntity.ESTIMATION_DIRECT_ITEM.ESTIMATED_QUANTITY == 0
                        ? 1
                        : projectionEntity.ESTIMATION_DIRECT_ITEM.ESTIMATED_QUANTITY;

                    if (!isNewRow)
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(projectionEntity,
                            BindableBase.GetPropertyName(
                                () => new ESTIMATION_DIRECT_ITEMProjection().ESTIMATION_DIRECT_ITEM) + "." +
                            BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().ESTIMATED_QUANTITY),
                            oldValue, newValue, EntityMessageType.Changed);

                    projectionEntity.ESTIMATION_DIRECT_ITEM.ESTIMATED_QUANTITY = 1;

                    oldValue = projectionEntity.ESTIMATION_DIRECT_ITEM.RATE_FREIGHT;
                    newValue = findCOMMODITY_CODE.RATE_FREIGHT;

                    if (!isNewRow)
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(projectionEntity,
                            BindableBase.GetPropertyName(
                                () => new ESTIMATION_DIRECT_ITEMProjection().ESTIMATION_DIRECT_ITEM) + "." +
                            BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().RATE_FREIGHT), oldValue,
                            newValue, EntityMessageType.Changed);

                    projectionEntity.ESTIMATION_DIRECT_ITEM.RATE_FREIGHT = findCOMMODITY_CODE.RATE_FREIGHT;

                    oldValue = projectionEntity.ESTIMATION_DIRECT_ITEM.RATE_SUPPLY;
                    newValue = findCOMMODITY_CODE.RATE_SUPPLY;

                    if (!isNewRow)
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(projectionEntity,
                            BindableBase.GetPropertyName(
                                () => new ESTIMATION_DIRECT_ITEMProjection().ESTIMATION_DIRECT_ITEM) + "." +
                            BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().RATE_SUPPLY), oldValue,
                            newValue, EntityMessageType.Changed);

                    projectionEntity.ESTIMATION_DIRECT_ITEM.RATE_SUPPLY = findCOMMODITY_CODE.RATE_SUPPLY;

                    oldValue = projectionEntity.ESTIMATION_DIRECT_ITEM.HOURS_INSTALL;
                    newValue = findCOMMODITY_CODE.HOURS_INSTALL;

                    if (!isNewRow)
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(projectionEntity,
                            BindableBase.GetPropertyName(
                                () => new ESTIMATION_DIRECT_ITEMProjection().ESTIMATION_DIRECT_ITEM) + "." +
                            BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().HOURS_INSTALL), oldValue,
                            newValue, EntityMessageType.Changed);

                    projectionEntity.ESTIMATION_DIRECT_ITEM.HOURS_INSTALL = findCOMMODITY_CODE.HOURS_INSTALL;
                }

                DeleteChildrenESTIMATION_DIRECT_ITEM(projectionEntity);
            }
        }

        private void MultiplyChildrenEntityRates(ESTIMATION_DIRECT_ITEMProjection parentProjectionEntity,
            decimal oldValue, decimal newValue)
        {
            if (oldValue == 0)
                return;

            foreach (
                var childrenEntity in parentProjectionEntity.CHILD_ESTIMATION_DIRECT_ITEM)
            {
                var childOldValue = childrenEntity.ESTIMATION_DIRECT_ITEM.ESTIMATED_QUANTITY;
                var childNewValue = childOldValue / oldValue * newValue;
                childrenEntity.ESTIMATION_DIRECT_ITEM.ESTIMATED_QUANTITY = childNewValue;
                MainViewModel.EntitiesUndoRedoManager.AddUndo(childrenEntity,
                    BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().ESTIMATION_DIRECT_ITEM) +
                    "." + BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().ESTIMATED_QUANTITY),
                    childOldValue, childNewValue, EntityMessageType.Changed);
                MainViewModel.Save(childrenEntity);
            }
        }

        //Remove children before parent deletion
        private void EntitiesBeforeDeletion(IEnumerable<ESTIMATION_DIRECT_ITEMProjection> entities)
        {
            //Undo manager is paused in bulk deletion and will be unpaused in bulk deletion too
            var childrenEntities = new List<ESTIMATION_DIRECT_ITEMProjection>();
            var parentEntitiesNotInList =
                new List<ESTIMATION_DIRECT_ITEMProjection>();

            foreach (var entity in entities)
            {
                var childrenEntitiesInTotal = entity.CHILD_ESTIMATION_DIRECT_ITEM;
                var childrenEntitiesNotInDeletionCollection =
                    new List<ESTIMATION_DIRECT_ITEMProjection>();
                foreach (var childrenEntityInTotal in childrenEntitiesInTotal)
                    if (!entities.Any(x => x.GUID == childrenEntityInTotal.GUID))
                        childrenEntitiesNotInDeletionCollection.Add(childrenEntityInTotal);

                ESTIMATION_DIRECT_ITEMProjection parentEntity = null;
                if (entity.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL_PARENT != Guid.Empty)
                {
                    parentEntity =
                        MainViewModel.Entities.FirstOrDefault(
                            x => x.GUID == entity.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL_PARENT);
                    if (parentEntity != null)
                        if (!entities.Any(x => x.GUID == parentEntity.GUID))
                            parentEntitiesNotInList.Add(parentEntity);
                }

                childrenEntities = childrenEntities.Concat(childrenEntitiesNotInDeletionCollection).ToList();
            }

            //can't use bulk delete here due to stack overflow
            foreach (var childrenEntity in childrenEntities)
            {
                MainViewModel.EntitiesUndoRedoManager.AddUndo(childrenEntity, null, null, null,
                    EntityMessageType.Deleted);
                MainViewModel.Delete(childrenEntity);
            }
        }

        #endregion

        #endregion

        #region Local Methods

        public Action<ESTIMATION_DIRECT_ITEMProjection> SetIsRowExpanded;

        private void AddChildrenESTIMATION_DIRECT_ITEM(ESTIMATION_DIRECT_ITEMProjection parentEstimation_Direct_Item)
        {
            var findCOMMODITY_GROUP =
                COMMODITY_CODEMasterDetailViewModel.DisplayEntities.FirstOrDefault(
                    x => x.CHILD_COMMODITY_CODES.Count > 0
                         &&
                         x.COMMODITY_CODE.GUID_COMMODITY_GROUP_DIRECT ==
                         parentEstimation_Direct_Item.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_GROUP_DIRECT
                         &&
                         x.COMMODITY_CODE.COMMODITY_GROUP_DIRECT_ID ==
                         parentEstimation_Direct_Item.ESTIMATION_DIRECT_ITEM.COMMODITY_GROUP_DIRECT_ID);

            if (findCOMMODITY_GROUP != null)
                if (findCOMMODITY_GROUP.CHILD_COMMODITY_CODES.Count > 0)
                    foreach (
                        var childCOMMODITY_CODE in
                        findCOMMODITY_GROUP.CHILD_COMMODITY_CODES)
                    {
                        var childESTIMATION_DIRECT_ITEM =
                            new ESTIMATION_DIRECT_ITEMProjection();
                        childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_CODE =
                            childCOMMODITY_CODE.COMMODITY_CODE.GUID;
                        childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_GROUP_DIRECT =
                            parentEstimation_Direct_Item.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_GROUP_DIRECT;
                        childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.COMMODITY_GROUP_DIRECT_ID =
                            parentEstimation_Direct_Item.ESTIMATION_DIRECT_ITEM.COMMODITY_GROUP_DIRECT_ID;
                        if (parentEstimation_Direct_Item.ESTIMATION_DIRECT_ITEM.GUID_DISCIPLINE != null)
                            childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_DISCIPLINE =
                                parentEstimation_Direct_Item.ESTIMATION_DIRECT_ITEM.GUID_DISCIPLINE;
                        else
                            childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_DISCIPLINE =
                                childCOMMODITY_CODE.COMMODITY_CODE.GUID_DISCIPLINE;
                        childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL_PARENT =
                            parentEstimation_Direct_Item.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL;
                        childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.ESTIMATED_QUANTITY =
                            parentEstimation_Direct_Item.ESTIMATION_DIRECT_ITEM.ESTIMATED_QUANTITY;
                        childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_INSTALLWORKPACK =
                            parentEstimation_Direct_Item.ESTIMATION_DIRECT_ITEM.GUID_INSTALLWORKPACK;
                        childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_SUPPLYWORKPACK =
                            parentEstimation_Direct_Item.ESTIMATION_DIRECT_ITEM.GUID_SUPPLYWORKPACK;

                        childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.RATE_FREIGHT =
                            childCOMMODITY_CODE.COMMODITY_CODE.RATE_FREIGHT;
                        childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.RATE_SUPPLY =
                            childCOMMODITY_CODE.COMMODITY_CODE.RATE_SUPPLY;
                        childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.HOURS_INSTALL =
                            childCOMMODITY_CODE.COMMODITY_CODE.HOURS_INSTALL;

                        MainViewModel.EntitiesUndoRedoManager.AddUndo(childESTIMATION_DIRECT_ITEM, null, null, null,
                            EntityMessageType.Added);
                        MainViewModel.Save(childESTIMATION_DIRECT_ITEM);
                    }
        }

        private void DeleteChildrenESTIMATION_DIRECT_ITEM(ESTIMATION_DIRECT_ITEMProjection parentEstimation_Direct_Item)
        {
            foreach (
                var childESTIMATION_DIRECT_ITEM in
                parentEstimation_Direct_Item.CHILD_ESTIMATION_DIRECT_ITEM)
            {
                MainViewModel.EntitiesUndoRedoManager.AddUndo(childESTIMATION_DIRECT_ITEM, null, null, null,
                    EntityMessageType.Deleted);
                MainViewModel.Delete(childESTIMATION_DIRECT_ITEM);
            }
        }


        private DevExpress.Mvvm.IDialogService BulkColumnEditDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("BulkColumnEditService"); }
        }

        private bool AddProjectSpecificCOMMODITY_CODEGroup(ESTIMATION_DIRECT_ITEMProjection editedProjectionEntity)
        {
            if (editedProjectionEntity.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL_PARENT != null)
            {
                var parentESTIMATION_DIRECT_ITEM =
                    DisplayEntities.First(
                        x =>
                            x.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL ==
                            editedProjectionEntity.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL_PARENT);
                var findCOMMODITY_CODEforID =
                    COMMODITY_CODECollection.FirstOrDefault(
                        x => x.GUID == editedProjectionEntity.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_CODE);
                if (findCOMMODITY_CODEforID == null)
                    //this will happen if commodity code is deleted for this estimation item, user will notice it from the lack of fullcode in the view
                    return false;

                //Check whether project specific commodity code group exists
                if (findCOMMODITY_CODEforID.GUID_PROJECT != null)
                {
                    var findAllCOMMODITY_GROUP =
                        COMMODITY_CODEMasterDetailViewModel.DisplayEntities.Where(
                            x =>
                                x.COMMODITY_CODE.GUID_COMMODITY_GROUP_DIRECT ==
                                parentESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_GROUP_DIRECT);
                    var findCurrentCOMMODITY_GROUP =
                        findAllCOMMODITY_GROUP.FirstOrDefault(
                            x =>
                                x.COMMODITY_CODE.COMMODITY_GROUP_DIRECT_ID ==
                                findCOMMODITY_CODEforID.COMMODITY_GROUP_DIRECT_ID);
                    if (findCurrentCOMMODITY_GROUP == null)
                        //this will happen if all project specific items within the commodity group is deleted, user will notice it from the lack of fullcode in the children view
                        return false;

                    //no doubt there is only one commodity code grouping by that estimate original parent guid
                    //asks user if they want to create new or edit existing group
                    MessageResult selectedResult;
                    selectedResult =
                        MessageBoxService.ShowMessage(CommonResources.Estimation_Item_Direct_EditOrAddNewCommodityCode,
                            CommonResources.Confirmation_Caption, MessageButton.YesNoCancel);
                    if (selectedResult == MessageResult.Yes)
                        return FindOrAddGroupedCOMMODITY_CODES(parentESTIMATION_DIRECT_ITEM, findCurrentCOMMODITY_GROUP);
                    else if (selectedResult == MessageResult.No)
                        return FindOrAddGroupedCOMMODITY_CODES(parentESTIMATION_DIRECT_ITEM, null);
                    else
                        return false;
                }
                //when there are no project specific commodity code group
                else
                {
                    if (FindOrAddGroupedCOMMODITY_CODES(parentESTIMATION_DIRECT_ITEM, null))
                    {
                        MessageBoxService.ShowMessage(
                            CommonResources.Estimation_Item_Direct_ProjectSpecificCommodityCodeCreated);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            //it is confirmed that current commodity code is standalone
            else
            {
                var addedOrExistingCOMMODITY_CODE = FindOrAddStandaloneCOMMODITY_CODE(editedProjectionEntity);
                if (addedOrExistingCOMMODITY_CODE != null)
                {
                    editedProjectionEntity.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_CODE =
                        (Guid) addedOrExistingCOMMODITY_CODE;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private bool FindOrAddGroupedCOMMODITY_CODES(ESTIMATION_DIRECT_ITEMProjection parentProjectionEntity,
            COMMODITY_CODEMasterDetailProjection selectedCOMMODITY_CODE_Group)
        {
            var parentCOMMODITY_GROUP_DIRECT =
                COMMODITY_GROUP_DIRECTCollection.FirstOrDefault(
                    x => x.GUID == parentProjectionEntity.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_GROUP_DIRECT);
            int? groupId = null;
            foreach (
                var childESTIMATION_DIRECT_ITEM in
                parentProjectionEntity.CHILD_ESTIMATION_DIRECT_ITEM)
            {
                var findCOMMODITY_CODEforID =
                    COMMODITY_CODECollection.FirstOrDefault(
                        x => x.GUID == childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_CODE);
                if (findCOMMODITY_CODEforID != null)
                    if (selectedCOMMODITY_CODE_Group == null)
                    {
                        var groupDescription = parentCOMMODITY_GROUP_DIRECT.DESCRIPTION;
                        var groupGuid = parentCOMMODITY_GROUP_DIRECT.GUID;

                        var newCOMMODITY_CODE = new COMMODITY_CODE();
                        DataUtils.ShallowCopy(newCOMMODITY_CODE, findCOMMODITY_CODEforID);
                        newCOMMODITY_CODE.GUID = Guid.Empty;
                        newCOMMODITY_CODE.GUID_PROJECT = loadPROJECT.GUID;
                        newCOMMODITY_CODE.GUID_PARENT = Guid.Empty;
                        //this is only used in commodity group, project specific group is specified by a combination of GUID_COMMODITY_GROUP_DIRECT and COMMODITY_GROUP_DIRECT_ID
                        newCOMMODITY_CODE.COMMODITY_GROUP_DESC = groupDescription;
                        newCOMMODITY_CODE.GUID_COMMODITY_GROUP_DIRECT = groupGuid;
                        if (groupId == null)
                            groupId =
                                COMMODITY_CODEMasterDetailViewModel.DisplayEntities.Count(
                                    x =>
                                        x.COMMODITY_CODE.GUID_COMMODITY_GROUP_DIRECT == groupGuid &&
                                        x.COMMODITY_CODE.COMMODITY_GROUP_DIRECT_ID !=
                                        parentProjectionEntity.ESTIMATION_DIRECT_ITEM.COMMODITY_GROUP_DIRECT_ID);

                        newCOMMODITY_CODE.RATE_FREIGHT = childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.RATE_FREIGHT;
                        newCOMMODITY_CODE.RATE_SUPPLY = childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.RATE_SUPPLY;
                        newCOMMODITY_CODE.HOURS_INSTALL =
                            childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.HOURS_INSTALL;
                        newCOMMODITY_CODE.COMMODITY_GROUP_DIRECT_ID = groupId;
                        COMMODITY_CODECollectionViewModel.Save(newCOMMODITY_CODE);

                        childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_CODE = newCOMMODITY_CODE.GUID;
                        MainViewModel.Save(childESTIMATION_DIRECT_ITEM);
                    }
                    else
                    {
                        var findCOMMODITY_CODEInGroup =
                            selectedCOMMODITY_CODE_Group.CHILD_COMMODITY_CODES.FirstOrDefault(
                                x => x.COMMODITY_CODE.FULLCODE == findCOMMODITY_CODEforID.FULLCODE);
                        if (findCOMMODITY_CODEInGroup != null)
                        {
                            var findActualCOMMODITY_CODE =
                                COMMODITY_CODECollection.FirstOrDefault(x => x.GUID == findCOMMODITY_CODEInGroup.GUID);
                            if (findActualCOMMODITY_CODE != null)
                            {
                                findActualCOMMODITY_CODE.RATE_FREIGHT =
                                    childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.RATE_FREIGHT;
                                findActualCOMMODITY_CODE.RATE_SUPPLY =
                                    childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.RATE_SUPPLY;
                                findActualCOMMODITY_CODE.HOURS_INSTALL =
                                    childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.HOURS_INSTALL;
                                COMMODITY_CODECollectionViewModel.Save(findCOMMODITY_CODEInGroup.COMMODITY_CODE);
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            var newCOMMODITY_CODE = new COMMODITY_CODE();
                            DataUtils.ShallowCopy(newCOMMODITY_CODE, findCOMMODITY_CODEforID);
                            newCOMMODITY_CODE.GUID = Guid.Empty;
                            newCOMMODITY_CODE.GUID_PROJECT = loadPROJECT.GUID;
                            newCOMMODITY_CODE.GUID_PARENT = Guid.Empty;
                            //this is only used in commodity group, project specific group is specified by a combination of GUID_COMMODITY_GROUP_DIRECT and COMMODITY_GROUP_DIRECT_ID
                            newCOMMODITY_CODE.COMMODITY_GROUP_DESC =
                                selectedCOMMODITY_CODE_Group.COMMODITY_CODE.COMMODITY_GROUP_DESC;
                            newCOMMODITY_CODE.GUID_COMMODITY_GROUP_DIRECT =
                                selectedCOMMODITY_CODE_Group.COMMODITY_CODE.GUID_COMMODITY_GROUP_DIRECT;
                            newCOMMODITY_CODE.COMMODITY_GROUP_DIRECT_ID =
                                selectedCOMMODITY_CODE_Group.COMMODITY_CODE.COMMODITY_GROUP_DIRECT_ID;
                            newCOMMODITY_CODE.RATE_FREIGHT =
                                childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.RATE_FREIGHT;
                            newCOMMODITY_CODE.RATE_SUPPLY =
                                childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.RATE_SUPPLY;
                            newCOMMODITY_CODE.HOURS_INSTALL =
                                childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.HOURS_INSTALL;
                            COMMODITY_CODECollectionViewModel.Save(newCOMMODITY_CODE);
                            childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_CODE =
                                newCOMMODITY_CODE.GUID;
                            MainViewModel.Save(childESTIMATION_DIRECT_ITEM);
                        }
                    }
                else
                    continue;
            }

            return true;
        }

        private Guid? FindOrAddStandaloneCOMMODITY_CODE(ESTIMATION_DIRECT_ITEMProjection editedProjectionEntity)
        {
            if (editedProjectionEntity.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_CODE == null)
                return null;

            var currentCOMMODITY_CODE =
                COMMODITY_CODECollection.FirstOrDefault(
                    x => x.GUID == (Guid) editedProjectionEntity.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_CODE);
            if (currentCOMMODITY_CODE != null)
                if (currentCOMMODITY_CODE.GUID_PROJECT != null)
                {
                    if (
                        MessageBoxService.ShowMessage(CommonResources.Estimation_Item_Direct_EditOrAddNewCommodityCode,
                            CommonResources.Confirmation_Caption, MessageButton.YesNo) == MessageResult.Yes)
                    {
                        var projectSpecific_COMMODITY_CODES =
                            COMMODITY_CODECollection.Where(
                                x => x.GUID_PROJECT == loadPROJECT.GUID && x.FULLCODE == currentCOMMODITY_CODE.FULLCODE);
                        if (projectSpecific_COMMODITY_CODES.Count() > 1)
                            return MultipleProjectSpecificCommodityCodeSelection(currentCOMMODITY_CODE,
                                projectSpecific_COMMODITY_CODES, editedProjectionEntity);
                        else
                            return UpdateAndSaveCOMMODITY_CODE(currentCOMMODITY_CODE, null, editedProjectionEntity,
                                false);
                    }
                    else
                    {
                        return UpdateAndSaveCOMMODITY_CODE(new COMMODITY_CODE(), currentCOMMODITY_CODE,
                            editedProjectionEntity, true);
                    }
                }
                else
                {
                    var projectSpecific_COMMODITY_CODES =
                        COMMODITY_CODECollection.Where(
                            x => x.GUID_PROJECT == loadPROJECT.GUID && x.FULLCODE == currentCOMMODITY_CODE.FULLCODE);
                    if (projectSpecific_COMMODITY_CODES.Count() > 0)
                    {
                        if (
                            MessageBoxService.ShowMessage(
                                CommonResources.Estimation_Item_Direct_EditOrAddNewCommodityCode,
                                CommonResources.Confirmation_Caption, MessageButton.YesNo) == MessageResult.Yes)
                            return MultipleProjectSpecificCommodityCodeSelection(currentCOMMODITY_CODE,
                                projectSpecific_COMMODITY_CODES, editedProjectionEntity);
                        else
                            return UpdateAndSaveCOMMODITY_CODE(new COMMODITY_CODE(), currentCOMMODITY_CODE,
                                editedProjectionEntity, true);
                    }
                    else
                    {
                        MessageBoxService.ShowMessage(
                            CommonResources.Estimation_Item_Direct_ProjectSpecificCommodityCodeCreated);
                        return UpdateAndSaveCOMMODITY_CODE(new COMMODITY_CODE(), currentCOMMODITY_CODE,
                            editedProjectionEntity, true);
                    }
                }

            return null;
        }

        private Guid? MultipleProjectSpecificCommodityCodeSelection(COMMODITY_CODE currentCOMMODITY_CODE,
            IEnumerable<COMMODITY_CODE> projectSpecific_COMMODITY_CODES,
            ESTIMATION_DIRECT_ITEMProjection ratesESTIMATION_DIRECT_ITEM)
        {
            var findCOMMODITY_CODE =
                COMMODITY_CODEMasterDetailViewModel.DisplayEntities.Where(
                    x => x.COMMODITY_CODE.GUID_COMMODITY_GROUP_DIRECT == null);
            var sourceObjects =
                new ObservableCollection<COMMODITY_CODEMasterDetailProjection>(findCOMMODITY_CODE);
            var currentObject =
                sourceObjects.FirstOrDefault(x => x.COMMODITY_CODE.GUID == currentCOMMODITY_CODE.GUID);
            var commodity_codeSelectionViewModel = COMMODITY_CODESelectionViewModel.Create(sourceObjects, currentObject,
                DISCIPLINECollection);
            if (
                BulkColumnEditDialogService.ShowDialog(MessageButton.OKCancel, "Select Item to edit",
                    "COMMODITY_CODESelectionView", commodity_codeSelectionViewModel) == MessageResult.OK)
            {
                if (commodity_codeSelectionViewModel.SelectedItem != null)
                {
                    var actualCOMMODITY_CODE =
                        COMMODITY_CODECollection.FirstOrDefault(
                            x => x.GUID == commodity_codeSelectionViewModel.SelectedItem.COMMODITY_CODE.GUID);
                    if (actualCOMMODITY_CODE != null)
                        return UpdateAndSaveCOMMODITY_CODE(actualCOMMODITY_CODE, null, ratesESTIMATION_DIRECT_ITEM,
                            false);
                    else
                        return null;
                }
            }
            else
            {
                return UpdateAndSaveCOMMODITY_CODE(currentCOMMODITY_CODE, null, ratesESTIMATION_DIRECT_ITEM, false);
            }

            return null;
        }

        private Guid UpdateAndSaveCOMMODITY_CODE(COMMODITY_CODE newOrExistingCOMMODITY_CODE,
            COMMODITY_CODE creationCOMMODITY_CODE, ESTIMATION_DIRECT_ITEMProjection ratesESTIMATION_DIRECT_ITEM,
            bool populateSourceDescription)
        {
            if (creationCOMMODITY_CODE != null)
            {
                DataUtils.ShallowCopy(newOrExistingCOMMODITY_CODE, creationCOMMODITY_CODE);
                newOrExistingCOMMODITY_CODE.GUID = Guid.Empty;
                newOrExistingCOMMODITY_CODE.GUID_PROJECT = loadPROJECT.GUID;
                newOrExistingCOMMODITY_CODE.GUID_PARENT = Guid.Empty;
            }

            newOrExistingCOMMODITY_CODE.RATE_FREIGHT = ratesESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.RATE_FREIGHT;
            newOrExistingCOMMODITY_CODE.RATE_SUPPLY = ratesESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.RATE_SUPPLY;
            newOrExistingCOMMODITY_CODE.HOURS_INSTALL = ratesESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.HOURS_INSTALL;

            if (ratesESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL_PARENT != null)
            {
                var parentESTIMATION_DIRECT_ITEM =
                    DisplayEntities.FirstOrDefault(
                        x =>
                            x.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL ==
                            ratesESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL_PARENT);
                if (parentESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_GROUP_DIRECT != null)
                {
                    var parentCOMMODITY_GROUP_DIRECT =
                        COMMODITY_GROUP_DIRECTCollection.FirstOrDefault(
                            x =>
                                x.GUID ==
                                parentESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_GROUP_DIRECT);
                    var groupDescription = parentCOMMODITY_GROUP_DIRECT.DESCRIPTION;
                    var groupGuid = parentCOMMODITY_GROUP_DIRECT.GUID;

                    if (parentCOMMODITY_GROUP_DIRECT != null)
                    {
                        newOrExistingCOMMODITY_CODE.COMMODITY_GROUP_DESC = groupDescription;
                        newOrExistingCOMMODITY_CODE.GUID_COMMODITY_GROUP_DIRECT = groupGuid;
                        newOrExistingCOMMODITY_CODE.COMMODITY_GROUP_DIRECT_ID =
                            COMMODITY_CODEMasterDetailViewModel.DisplayEntities.Count(
                                x =>
                                    x.COMMODITY_CODE.GUID_COMMODITY_GROUP_DIRECT == groupGuid &&
                                    x.COMMODITY_CODE.COMMODITY_GROUP_DIRECT_ID !=
                                    parentESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.COMMODITY_GROUP_DIRECT_ID);
                    }
                }
            }

            COMMODITY_CODECollectionViewModel.Save(newOrExistingCOMMODITY_CODE);
            return newOrExistingCOMMODITY_CODE.GUID;
        }

        protected override void OnClose(CancelEventArgs e)
        {
            refreshBackgroundWorker.CancelAsync();
            displayEntitiesRefreshBackgroundWorker.CancelAsync();
            userStateRestoreBackgroundWorker.CancelAsync();
            base.OnClose(e);
        }

        #endregion

        #region View Behavior

        public ESTIMATION_DIRECT_ITEMProjection CreateNewProjectionFromNewEntityCallBack(ESTIMATION_DIRECT_ITEM entity)
        {
            return new ESTIMATION_DIRECT_ITEMProjection();
        }

        #endregion

        #region View Commands

        public void MasterRowExpanded(RowEventArgs e)
        {
            ((ESTIMATION_DIRECT_ITEMProjection) e.Row).ISEXPANDED = true;
        }

        public void MasterRowCollapsed(RowEventArgs e)
        {
            ((ESTIMATION_DIRECT_ITEMProjection) e.Row).ISEXPANDED = false;
        }

        private void ExpandDisplayRow(ESTIMATION_DIRECT_ITEMProjection row)
        {
            row.ISEXPANDED = true;
            if (SetIsRowExpanded != null)
                SetIsRowExpanded(row);
        }

        public virtual bool CanBulkDelete()
        {
            return MainViewModel != null && MainViewModel.Entities != null && MainViewModel.Entities.Count > 0 &&
                   !IsLoading && SelectedEntities.Count > 0;
        }

        public void BulkDelete()
        {
            MainViewModel.EntitiesUndoRedoManager.PauseActionId();
            MainViewModel.BaseBulkDelete(SelectedEntities);
            MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
        }

        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "ESTIMATION_DIRECT_ITEMSViewModelWrapper"; }
        }


        public CollectionViewModel<COMMODITY_CODE, COMMODITY_CODE, Guid, IBluePrintsEntitiesUnitOfWork>
            COMMODITY_CODECollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return
                    (CollectionViewModel<COMMODITY_CODE, COMMODITY_CODE, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<COMMODITY_CODE>();
            }
        }

        private COMMODITY_CODEMasterDetailViewModelWrapper commodity_codeMasterDetailViewModel;

        public COMMODITY_CODEMasterDetailViewModelWrapper COMMODITY_CODEMasterDetailViewModel
        {
            get
            {
                if (commodity_codeMasterDetailViewModel == null && loadPROJECT != null)
                {
                    commodity_codeMasterDetailViewModel = COMMODITY_CODEMasterDetailViewModelWrapper.Create();
                    commodity_codeMasterDetailViewModel.SetParentViewModel(this);
                    var baselineSupportParameterObj =
                        commodity_codeMasterDetailViewModel as ISupportParameter;
                    baselineSupportParameterObj.Parameter =
                        new OptionalEntitiesParameter<PROJECT, CommodityCodeTypeClass>(loadPROJECT,
                            new CommodityCodeTypeClass(CommodityCodeType.Direct));
                }

                return commodity_codeMasterDetailViewModel;
            }
        }

        public ObservableCollection<COMMODITY_CODEMasterDetailProjection> COMMODITY_GROUP_MasterDetailDisplayCollection
        {
            get
            {
                if (COMMODITY_CODEMasterDetailViewModel == null)
                    return null;

                return COMMODITY_CODEMasterDetailViewModel.DisplayEntities;
            }
        }

        public ObservableCollection<COMMODITY_CODEMasterDetailProjection>
            COMMODITY_GROUP_MasterDetailFlatProjectSpecificDisplayCollection
        {
            get
            {
                if (COMMODITY_GROUP_MasterDetailDisplayCollection == null)
                    return null;

                return
                    new ObservableCollection<COMMODITY_CODEMasterDetailProjection>(
                        COMMODITY_GROUP_MasterDetailDisplayCollection.Where(
                                x =>
                                    x.ProjectionType == COMMODITY_CODEProjectionType.GeneralGrouped ||
                                    x.ProjectionType == COMMODITY_CODEProjectionType.ProjectSpecificGrouped)
                            .SelectMany(x => x.CHILD_COMMODITY_CODES));
            }
        }


        private ObservableCollection<ESTIMATION_DIRECT_ITEMProjection> displayEntities;

        public ObservableCollection<ESTIMATION_DIRECT_ITEMProjection> DisplayEntities
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                if (displayEntities == null)
                {
                    displayEntities = new ObservableCollection<ESTIMATION_DIRECT_ITEMProjection>();
                    var parentESTIMATION_DIRECT_ITEMS =
                        MainViewModel.Entities.Where(x => x.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL_PARENT == null)
                            .AsEnumerable();
                    var AllChildESTIMATION_DIRECT_ITEMS =
                        MainViewModel.Entities.Where(x => x.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL_PARENT != null)
                            .AsEnumerable();
                    foreach (
                        var parentESTIMATION_DIRECT_ITEM in parentESTIMATION_DIRECT_ITEMS)
                    {
                        var parentESTIMATION_DIRECT_ITEMSPOCO =
                            ViewModelSource.Create(() => new ESTIMATION_DIRECT_ITEMProjection());
                        parentESTIMATION_DIRECT_ITEMSPOCO.GUID = parentESTIMATION_DIRECT_ITEM.GUID;
                        DataUtils.ShallowCopy(parentESTIMATION_DIRECT_ITEMSPOCO.ESTIMATION_DIRECT_ITEM,
                            parentESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM);
                        parentESTIMATION_DIRECT_ITEM.RATE = new RATE();
                        DataUtils.ShallowCopy(parentESTIMATION_DIRECT_ITEMSPOCO.RATE, parentESTIMATION_DIRECT_ITEM.RATE);
                        displayEntities.Add(parentESTIMATION_DIRECT_ITEMSPOCO);
                    }

                    foreach (var displayEntity in displayEntities)
                    {
                        var childESTIMATION_DIRECT_ITEMS =
                            AllChildESTIMATION_DIRECT_ITEMS.Where(
                                y =>
                                    y.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL_PARENT ==
                                    displayEntity.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL);
                        if (childESTIMATION_DIRECT_ITEMS.Count() > 0)
                        {
                            displayEntity.ESTIMATION_DIRECT_ITEM.RATE_FREIGHT =
                                childESTIMATION_DIRECT_ITEMS.Sum(x => x.ESTIMATION_DIRECT_ITEM.RATE_FREIGHT);
                            displayEntity.ESTIMATION_DIRECT_ITEM.RATE_SUPPLY =
                                childESTIMATION_DIRECT_ITEMS.Sum(x => x.ESTIMATION_DIRECT_ITEM.RATE_SUPPLY);
                            displayEntity.ESTIMATION_DIRECT_ITEM.HOURS_INSTALL =
                                childESTIMATION_DIRECT_ITEMS.Sum(x => x.ESTIMATION_DIRECT_ITEM.HOURS_INSTALL);
                            displayEntity.RATE = new RATE();
                            displayEntity.RATE.RATE1 =
                                childESTIMATION_DIRECT_ITEMS.Where(x => x.RATE != null).Sum(y => y.RATE.RATE1);

                            foreach (
                                var childESTIMATION_DIRECT_ITEM in
                                childESTIMATION_DIRECT_ITEMS)
                            {
                                var childESTIMATION_DIRECT_ITEMPOCO =
                                    ViewModelSource.Create(() => new ESTIMATION_DIRECT_ITEMProjection());
                                childESTIMATION_DIRECT_ITEMPOCO.GUID = childESTIMATION_DIRECT_ITEM.GUID;
                                DataUtils.ShallowCopy(childESTIMATION_DIRECT_ITEMPOCO.ESTIMATION_DIRECT_ITEM,
                                    childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM);
                                childESTIMATION_DIRECT_ITEMPOCO.RATE = new RATE();
                                DataUtils.ShallowCopy(childESTIMATION_DIRECT_ITEMPOCO.RATE,
                                    childESTIMATION_DIRECT_ITEM.RATE);
                                displayEntity.CHILD_ESTIMATION_DIRECT_ITEM.Add(childESTIMATION_DIRECT_ITEMPOCO);
                            }
                        }
                    }

                    if (!userStateRestoreBackgroundWorker.IsBusy)
                        userStateRestoreBackgroundWorker.RunWorkerAsync();
                }

                return displayEntities;
            }
        }

        public IEnumerable<PROJECT> PROJECTCollection
        {
            get
            {
                var collection = GetEntities<PROJECT>();
                return collection;
            }
        }

        public IEnumerable<WORKPACK> WORKPACKCollection
        {
            get
            {
                var collection = GetEntities<WORKPACK>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.INTERNAL_NAME1).OrderBy(x => x.INTERNAL_NAME2);
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

        public IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection
        {
            get
            {
                var collection = GetEntities<COMMODITY_CODE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.GUID_PROJECT).OrderBy(x => x.CODE);
                return collection;
            }
        }

        public IEnumerable<COMMODITY_GROUP_DIRECT> COMMODITY_GROUP_DIRECTCollection
        {
            get
            {
                var collection = GetEntities<COMMODITY_GROUP_DIRECT>();
                return collection;
            }
        }

        #endregion
    }
}