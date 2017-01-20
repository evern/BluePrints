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
    public partial class COMMODITY_CODEProjectSpecificViewModelWrapper : CollectionViewModelsWrapper<COMMODITY_CODE, COMMODITY_CODE_ProjectSpecificProjection, Guid, IBluePrintsEntitiesUnitOfWork, CollectionViewModel<COMMODITY_CODE, COMMODITY_CODE_ProjectSpecificProjection, Guid, IBluePrintsEntitiesUnitOfWork>>
    {
        /// <summary>
        /// Creates a new instance of COMMODITY_CODESCollectionViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static COMMODITY_CODEProjectSpecificViewModelWrapper Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new COMMODITY_CODEProjectSpecificViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the COMMODITY_CODESCollectionViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the COMMODITY_CODESCollectionViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected COMMODITY_CODEProjectSpecificViewModelWrapper(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        CommodityCodeType loadCommodityCodeType;
        PROJECT loadPROJECT;
        IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        BackgroundWorker refreshBackgroundWorker;
        BackgroundWorker displayEntitiesRefreshBackgroundWorker;
        BackgroundWorker userStateRestoreBackgroundWorker;

        public COMMODITY_CODE_ProjectSpecificProjection SelectedEntity { get; set; }
        ObservableCollection<COMMODITY_CODE_ProjectSpecificProjection> selectedentities { get; set; }
        public ObservableCollection<COMMODITY_CODE_ProjectSpecificProjection> SelectedEntities
        {
            get { return selectedentities; }
            set { selectedentities = value; }
        }

        Guid RestoreSelectedEntityGuid;
        List<Guid> RestoreSelectedEntitiesGuids = new List<Guid>();
        List<Guid> RestoreExpandedGuids = new List<Guid>();
        protected override void InitializeParameters(object parameter)
        {
            RestoreSelectedEntityGuid = Guid.Empty;
            SelectedEntities = new ObservableCollection<COMMODITY_CODE_ProjectSpecificProjection>();

            refreshBackgroundWorker = new BackgroundWorker();
            refreshBackgroundWorker.DoWork += refreshBackgroundWorker_DoWork;
            refreshBackgroundWorker.WorkerSupportsCancellation = true;

            displayEntitiesRefreshBackgroundWorker = new BackgroundWorker();
            displayEntitiesRefreshBackgroundWorker.DoWork += displayEntitiesRefreshBackgroundWorker_DoWork;
            displayEntitiesRefreshBackgroundWorker.WorkerSupportsCancellation = true;

            userStateRestoreBackgroundWorker = new BackgroundWorker();
            userStateRestoreBackgroundWorker.DoWork += userStateRestoreBackgroundWorker_DoWork;
            userStateRestoreBackgroundWorker.WorkerSupportsCancellation = true;

            OptionalEntitiesParameter<PROJECT, CommodityCodeTypeClass> receiveParameter = (OptionalEntitiesParameter<PROJECT, CommodityCodeTypeClass>)parameter;
            this.loadPROJECT = receiveParameter.GetFirstEntity();
            CommodityCodeTypeClass loadCommodityCodeTypeClass = receiveParameter.GetSecondEntity();
            this.loadCommodityCodeType = loadCommodityCodeTypeClass.commodityCodeType;
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddEntitiesLoader<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>(1, bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, null, isContinueLoadingAfterPROJECT, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(2, bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS, null, null, isContinueLoadingAfterDEPARTMENT, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(3, bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES, null, null, isContinueLoadingAfterDISCIPLINE, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<INDIRECT_TYPE, INDIRECT_TYPE, Guid, IBluePrintsEntitiesUnitOfWork>(4, bluePrintsUnitOfWorkFactory, x => x.INDIRECT_TYPES, null, null, null, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<UOM, UOM, Guid, IBluePrintsEntitiesUnitOfWork>(5, bluePrintsUnitOfWorkFactory, x => x.UOMS, null, null, null, OnAfterEntitiesChanged);
            InvokeEntitiesLoaderDescriptionLoading();
        }

        bool isContinueLoadingAfterPROJECT(IEnumerable<PROJECT> entities)
        {
            if (entities.Count() == 0)
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, "PROJECT"))));
                return false;
            }

            this.loadPROJECT = entities.First();
            return true;
        }

        Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        bool isContinueLoadingAfterDEPARTMENT(IEnumerable<DEPARTMENT> entities)
        {
            if (entities.Count() == 0)
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage(CommonResources.CommodityCode_NoDepartment)));
                return false;
            }

            return true;
        }

        bool isContinueLoadingAfterDISCIPLINE(IEnumerable<DISCIPLINE> entities)
        {
            if (entities.Count() == 0)
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage(CommonResources.CommodityCode_NoDiscipline)));
                return false;
            }

            return true;
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(this.bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoader.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE_ProjectSpecificProjection>> ConstructMainViewModelProjection()
        {
            return query => COMMODITY_CODE_ProjectSpecific_ProjectionQueries.transformCOMMODITY_CODE(query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID));
        }

        #region View Refresh
        List<Guid> SelectedEntitiesGuid = new List<Guid>();
        protected override void OnAfterEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender)
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
            {
                if (messageType == EntityMessageType.Added)
                    MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Restored, StringFormatUtils.GetEntityNameByType(changedType)));
                else if (messageType == EntityMessageType.Deleted)
                    MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, StringFormatUtils.GetEntityNameByType(changedType)));
            }

            if (loadPROJECT != null)
            {
                if (MainViewModel != null)
                    mainThreadDispatcher.BeginInvoke(new Action(() => MainViewModel.Refresh()));
                else if (loadPROJECT != null)
                    mainThreadDispatcher.BeginInvoke(new Action(() => InitializeAndLoadEntitiesLoaderDescription()));
            }

            return;
        }

        void refreshBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            System.Threading.Thread.Sleep(500);
            if (((BackgroundWorker)sender).CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertiesChanged()));
        }

        void displayEntitiesRefreshBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            System.Threading.Thread.Sleep(100);
            if (((BackgroundWorker)sender).CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            displayEntities = null;
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertyChanged(x => x.DisplayEntities)));
        }

        void userStateRestoreBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            System.Threading.Thread.Sleep(1);
            if (((BackgroundWorker)sender).CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            mainThreadDispatcher.BeginInvoke(new Action(() => this.restoreViewState()));
        }

        void storeViewState()
        {
            RestoreSelectedEntityGuid = Guid.Empty;
            RestoreSelectedEntitiesGuids.Clear();
            RestoreExpandedGuids.Clear();

            foreach (COMMODITY_CODE_ProjectSpecificProjection selectedEntity in SelectedEntities)
            {
                RestoreSelectedEntitiesGuids.Add(new Guid(selectedEntity.GUID.ToString()));
            }

            foreach (COMMODITY_CODE_ProjectSpecificProjection entity in DisplayEntities)
            {
                if (entity.ISEXPANDED)
                    RestoreExpandedGuids.Add(entity.GUID);
            }

            if (SelectedEntity != null)
                RestoreSelectedEntityGuid = SelectedEntity.GUID;
        }

        void restoreViewState()
        {
            IEnumerable<COMMODITY_CODE_ProjectSpecificProjection> restoreSelectedEntities = DisplayEntities.Concat(DisplayEntities.SelectMany(x => x.CHILD_COMMODITY_CODES)).Where(x => RestoreSelectedEntitiesGuids.Any(y => y == x.GUID));
            SelectedEntities.Clear();
            if (restoreSelectedEntities.Count() > 0)
            {
                foreach (COMMODITY_CODE_ProjectSpecificProjection restoreSelectedEntity in restoreSelectedEntities)
                {
                    SelectedEntities.Add(restoreSelectedEntity);
                }
            }

            foreach (Guid expandedGuid in RestoreExpandedGuids)
            {
                COMMODITY_CODE_ProjectSpecificProjection restoreExpandedEntity = DisplayEntities.FirstOrDefault(x => x.GUID == expandedGuid);
                if (restoreExpandedEntity != null)
                {
                    ExpandDisplayRow(restoreExpandedEntity);
                }
            }

            if (RestoreSelectedEntityGuid != Guid.Empty)
            {
                COMMODITY_CODE_ProjectSpecificProjection restoreSelectedEntity = DisplayEntities.Concat(DisplayEntities.SelectMany(x => x.CHILD_COMMODITY_CODES)).FirstOrDefault(x => x.GUID == RestoreSelectedEntityGuid);
                if (restoreSelectedEntity != null)
                    SelectedEntity = restoreSelectedEntity;
            }
        }
        #endregion

        #region Local Methods
        public Action<COMMODITY_CODE_ProjectSpecificProjection> SetIsRowExpanded;
        void DeleteChildrenCOMMODITY_CODE(COMMODITY_CODE_ProjectSpecificProjection parentCOMMODITY_CODE)
        {
            foreach (COMMODITY_CODE_ProjectSpecificProjection childCOMMODITY_CODE in parentCOMMODITY_CODE.CHILD_COMMODITY_CODES)
            {
                MainViewModel.EntitiesUndoRedoManager.AddUndo(childCOMMODITY_CODE, null, null, null, EntityMessageType.Deleted);
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

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<COMMODITY_CODE_ProjectSpecificProjection> entities)
        {
            MainViewModel.OnBeforeEntitySavedCallBack = this.OnBeforeEntitiesSaved;
            MainViewModel.ApplyProjectionPropertiesToEntityCallBack = this.ApplyProjectionPropertiesToEntity;
            MainViewModel.OnEntitySavedCallBack = this.OnEntitiesSavedCallBack;
            MainViewModel.SetParentViewModel(this);

            if (this.ShowDEPARTMENT != null && ShowDISCIPLINE != null && ShowDIRECT_RATES != null && ShowINDIRECT_TYPE != null && ShowINDIRECT_RATES != null)
            {
                if (loadCommodityCodeType == CommodityCodeType.Design)
                    mainThreadDispatcher.BeginInvoke(new Action(() => this.ShowDEPARTMENT()));
                else if (loadCommodityCodeType == CommodityCodeType.Direct)
                {
                    mainThreadDispatcher.BeginInvoke(new Action(() => this.ShowDISCIPLINE()));
                    mainThreadDispatcher.BeginInvoke(new Action(() => this.ShowDIRECT_RATES()));
                }
                else
                {
                    mainThreadDispatcher.BeginInvoke(new Action(() => this.ShowINDIRECT_TYPE()));
                    mainThreadDispatcher.BeginInvoke(new Action(() => this.ShowINDIRECT_RATES()));
                }
            }

            refreshBackgroundWorker.RunWorkerAsync();
        }

        #region Collection Call Backs
        public void OnEntitiesSavedCallBack(Guid primaryKey, COMMODITY_CODE_ProjectSpecificProjection projectionEntity, COMMODITY_CODE entity, bool isNewEntity)
        {
            projectionEntity.GUID = entity.GUID;
            projectionEntity.COMMODITY_CODE.GUID = entity.GUID;
        }

        public void ApplyProjectionPropertiesToEntity(COMMODITY_CODE_ProjectSpecificProjection projectionEntity, COMMODITY_CODE entity)
        {
            projectionEntity.COMMODITY_CODE.GUID_PROJECT = loadPROJECT.GUID;
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
            ((COMMODITY_CODE_ProjectSpecificProjection)e.Row).ISEXPANDED = true;
        }

        public void MasterRowCollapsed(RowEventArgs e)
        {
            ((COMMODITY_CODE_ProjectSpecificProjection)e.Row).ISEXPANDED = false;
        }

        void ExpandDisplayRow(COMMODITY_CODE_ProjectSpecificProjection row)
        {
            row.ISEXPANDED = true;
            if (SetIsRowExpanded != null)
                SetIsRowExpanded(row);
        }

        public virtual bool CanBulkDelete()
        {
            return MainViewModel != null && MainViewModel.Entities != null && MainViewModel.Entities.Count > 0 && !IsLoading && SelectedEntities.Count > 0;
        }

        public void BulkDelete()
        {
            MainViewModel.EntitiesUndoRedoManager.PauseActionId();
            MainViewModel.BaseBulkDelete(this.SelectedEntities);
            MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
        }
        #endregion

        #region View Properties
        ObservableCollection<COMMODITY_CODE_ProjectSpecificProjection> displayEntities;
        public ObservableCollection<COMMODITY_CODE_ProjectSpecificProjection> DisplayEntities
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                if (displayEntities == null)
                {
                    displayEntities = new ObservableCollection<COMMODITY_CODE_ProjectSpecificProjection>();
                    var groups = MainViewModel.Entities.Where(x => x.COMMODITY_CODE.GUID_COMMODITY_GROUP_DIRECT != null && x.COMMODITY_CODE.COMMODITY_GROUP_DIRECT_ID != null).GroupBy(x => x.GROUP_ID);

                    foreach (var group in groups)
                    {
                        COMMODITY_CODE_ProjectSpecificProjection firstItemInGroup = group.First();
                        COMMODITY_CODE_ProjectSpecificProjection projectionParentPOCO = ViewModelSource.Create(() => new COMMODITY_CODE_ProjectSpecificProjection());
                        projectionParentPOCO.GUID = Guid.Empty;
                        projectionParentPOCO.COMMODITY_CODE.FULLCODE = firstItemInGroup.COMMODITY_CODE.COMMODITY_GROUP_DESC;
                        projectionParentPOCO.ISGENERATED = true;
                        foreach(var item in group)
                        {
                            COMMODITY_CODE_ProjectSpecificProjection projectionChildPOCO = ViewModelSource.Create(() => new COMMODITY_CODE_ProjectSpecificProjection());
                            DataUtils.ShallowCopy(projectionChildPOCO.COMMODITY_CODE, item.COMMODITY_CODE);
                            projectionChildPOCO.GUID = item.COMMODITY_CODE.GUID;
                            projectionParentPOCO.CHILD_COMMODITY_CODES.Add(projectionChildPOCO);
                        }

                        displayEntities.Add(projectionParentPOCO);
                    }

                    IEnumerable<COMMODITY_CODE_ProjectSpecificProjection> COMMODITY_CODENotInGroup = MainViewModel.Entities.Where(x => x.COMMODITY_CODE.GUID_COMMODITY_GROUP_DIRECT == null);

                    foreach (COMMODITY_CODE_ProjectSpecificProjection COMMODITY_CODEProjection in COMMODITY_CODENotInGroup)
                    {
                        COMMODITY_CODE_ProjectSpecificProjection projectionPOCO = ViewModelSource.Create(() => new COMMODITY_CODE_ProjectSpecificProjection());
                        DataUtils.ShallowCopy(projectionPOCO.COMMODITY_CODE, COMMODITY_CODEProjection.COMMODITY_CODE);
                        projectionPOCO.GUID = COMMODITY_CODEProjection.GUID;

                        displayEntities.Add(projectionPOCO);
                    }

                    if (!userStateRestoreBackgroundWorker.IsBusy)
                        userStateRestoreBackgroundWorker.RunWorkerAsync();
                }

                return displayEntities;
            }
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get
            {
                return "COMMODITY_CODEProjectSpecificViewModelWrapper";
            }
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