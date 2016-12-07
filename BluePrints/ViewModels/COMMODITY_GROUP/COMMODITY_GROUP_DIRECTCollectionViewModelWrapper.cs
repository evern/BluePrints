using System;
using System.Linq;
using DevExpress.Mvvm.POCO;
using BluePrints.Common.Utils;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.DataModel;
using BluePrints.Data;
using BluePrints.Common.ViewModel;
using BluePrints.Common.Projections;
using BluePrints.Data.Helpers;
using System.Collections.Generic;
using DevExpress.Xpf.Grid.TreeList;
using BluePrints.Common.ViewModel.Utils;
using DevExpress.Mvvm;
using BluePrints.Common;
using BluePrints.Common.Helpers;
using BluePrints.Views;
using DevExpress.Xpf.Grid;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the COMMODITIES collection view model.
    /// </summary>
    public partial class COMMODITY_GROUP_DIRECTCollectionViewModelWrapper : CollectionViewModelsWrapper<COMMODITY_GROUP_DIRECT, COMMODITY_GROUP_DIRECTProjection, Guid, IBluePrintsEntitiesUnitOfWork, CollectionViewModel<COMMODITY_GROUP_DIRECT, COMMODITY_GROUP_DIRECTProjection, Guid, IBluePrintsEntitiesUnitOfWork>>
    {
        /// <summary>
        /// Creates a new instance of COMMODITY_GROUP_DIRECTCollectionViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static COMMODITY_GROUP_DIRECTCollectionViewModelWrapper Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new COMMODITY_GROUP_DIRECTCollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the COMMODITY_GROUP_DIRECTCollectionViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the COMMODITY_GROUP_DIRECTCollectionViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected COMMODITY_GROUP_DIRECTCollectionViewModelWrapper(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operation
        PROJECT loadPROJECT;
        bool isPROJECTSpecific;
        DispatcherTimer delayedRefresher;
        IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected override void InitializeParameters(object parameter)
        {
            delayedRefresher = new DispatcherTimer();
            delayedRefresher.Interval = new TimeSpan(0, 0, 0, 0, 10);
            delayedRefresher.Tick += delayedRefresher_Tick;

            if (parameter != null)
            {
                EntitiesParameter<BluePrints.Data.PROJECT> PROJECTParameter = (EntitiesParameter<BluePrints.Data.PROJECT>)parameter;
                this.loadPROJECT = PROJECTParameter.GetEntity();
                isPROJECTSpecific = true;
            }
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddEntitiesLoader<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>(1, bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, null, isContinueLoadingAfterPROJECT, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<COMMODITY_CODE, COMMODITY_CODE, Guid, IBluePrintsEntitiesUnitOfWork>(2, bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, COMMODITY_CODEProjectionFunc, null, null, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(3, bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddEntitiesLoader<UOM, UOM, Guid, IBluePrintsEntitiesUnitOfWork>(4, bluePrintsUnitOfWorkFactory, x => x.UOMS);
            InvokeEntitiesLoaderDescriptionLoading();
        }

        bool isContinueLoadingAfterPROJECT(IEnumerable<PROJECT> entities)
        {
            if (isPROJECTSpecific && entities.Count() == 0)
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, "PROJECT"))));
                return false;
            }

            if (isPROJECTSpecific)
                this.loadPROJECT = entities.First();

            return true;
        }

        Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            if (isPROJECTSpecific)
                return query => query.Where(x => x.GUID == loadPROJECT.GUID);
            else
                return query => query.Where(x => x.GUID == Guid.Empty);
        }

        Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>> COMMODITY_CODEProjectionFunc()
        {
            return query => getLastChildrenCOMMODITY_CODE(query);
        }

        public IQueryable<COMMODITY_CODE> getLastChildrenCOMMODITY_CODE(IQueryable<COMMODITY_CODE> COMMODITY_CODE)
        {
            IEnumerable<COMMODITY_CODE> readCOMMODITY_CODE = COMMODITY_CODE.Where(x => x.COMMODITYCODETYPE == CommodityCodeType.Direct).ToArray().AsEnumerable();
            IEnumerable<COMMODITY_CODE> lastCOMMODITY_CODE = readCOMMODITY_CODE.Where(x => !readCOMMODITY_CODE.Any(y => y.GUID_PARENT == x.GUID));

            return lastCOMMODITY_CODE.AsQueryable();
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(this.bluePrintsUnitOfWorkFactory, x => x.COMMODITY_GROUP_DIRECT);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoader.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<COMMODITY_GROUP_DIRECT>, IQueryable<COMMODITY_GROUP_DIRECTProjection>> ConstructMainViewModelProjection()
        {
            if (isPROJECTSpecific)
                return query => COMMODITY_GROUP_DIRECTProjectionQueries.ConvertToProjectionCOMMODITY_GROUP_DIRECT(query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID));
            else
                return query => COMMODITY_GROUP_DIRECTProjectionQueries.ConvertToProjectionCOMMODITY_GROUP_DIRECT(query.Where(x => x.GUID_PROJECT == null));
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<COMMODITY_GROUP_DIRECTProjection> entities)
        {
            MainViewModel.EntitiesBeforeDeletionCallBack = EntitiesBeforeDeletion;
            MainViewModel.OnBeforeEntitiesChangedCallBack = OnBeforeEntitiesChanged;
            MainViewModel.NewProjectionInitializeCallBack = NewProjectionInitialization;
            MainViewModel.OnEntitySavedCallBack = OnEntitiesSavedCallBack;
            MainViewModel.ApplyProjectionPropertiesToEntityCallBack = this.ApplyProjectionPropertiesToEntity;
            MainViewModel.SetParentViewModel(this);
            delayedRefresher.Start();
        }

        void delayedRefresher_Tick(object sender, EventArgs e)
        {
            delayedRefresher.Stop();
            this.RaisePropertiesChanged();
        }

        public bool OnBeforeEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender)
        {
            if (changedType == typeof(COMMODITY_GROUP_DIRECT))
            {
                if (messageType == EntityMessageType.Deleted)
                {
                    IEnumerable<COMMODITY_GROUP_DIRECTProjection> flatDisplayEntities = displayEntities.Concat(displayEntities.SelectMany(x => x.CHILD_COMMODITY_GROUP));
                    COMMODITY_GROUP_DIRECTProjection deletedEntity = flatDisplayEntities.First(x => x.GUID == (Guid)key);
                    if (deletedEntity.COMMODITY_GROUP.GUID_PARENT != null)
                    {
                        COMMODITY_GROUP_DIRECTProjection parentEntity = displayEntities.FirstOrDefault(x => x.GUID == deletedEntity.COMMODITY_GROUP.GUID_PARENT);
                        if (parentEntity != null)
                        {
                            parentEntity.CHILD_COMMODITY_GROUP.Remove(deletedEntity);
                        }
                    }
                    else
                        displayEntities.Remove(deletedEntity);
                }

                this.RaisePropertyChanged(x => x.DisplayEntities);
            }

            return true;
        }


        protected override void OnAfterEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender)
        {
            if (changedType == typeof(COMMODITY_GROUP_DIRECT))
            {
                if (MainViewModel == null)
                    return;

                if (!MainViewModel.EntitiesUndoRedoManager.IsInUndoRedoOperation() && (sender == MainViewModel || sender == this))
                    return;

                if (messageType == EntityMessageType.Added)
                {
                    COMMODITY_GROUP_DIRECTProjection addedEntity = MainViewModel.Entities.First(x => x.GUID == (Guid)key);
                    if (addedEntity.COMMODITY_GROUP.GUID_PARENT != null)
                    {
                        COMMODITY_GROUP_DIRECTProjection parentEntity = displayEntities.FirstOrDefault(x => x.GUID == addedEntity.COMMODITY_GROUP.GUID_PARENT);
                        if (parentEntity != null)
                        {
                            parentEntity.CHILD_COMMODITY_GROUP.Add(addedEntity);
                        }
                    }
                    else
                        displayEntities.Add(addedEntity);
                }
                else if(messageType == EntityMessageType.Changed)
                {
                    IEnumerable<COMMODITY_GROUP_DIRECTProjection> flatDisplayEntities = displayEntities.Concat(displayEntities.SelectMany(x => x.CHILD_COMMODITY_GROUP));
                    COMMODITY_GROUP_DIRECTProjection changedEntity = flatDisplayEntities.First(x => x.GUID == (Guid)key);
                    if (changedEntity.COMMODITY_GROUP.GUID_PARENT != null)
                    {
                        COMMODITY_GROUP_DIRECTProjection parentEntity = displayEntities.FirstOrDefault(x => x.GUID == changedEntity.COMMODITY_GROUP.GUID_PARENT);
                        if (parentEntity != null)
                        {
                            int index = parentEntity.CHILD_COMMODITY_GROUP.IndexOf(changedEntity);
                            parentEntity.CHILD_COMMODITY_GROUP.Remove(changedEntity);
                            this.RaisePropertyChanged(x => x.DisplayEntities);
                            parentEntity.CHILD_COMMODITY_GROUP.Insert(index, changedEntity);
                        }
                    }
                    else
                    {
                        int index = displayEntities.IndexOf(changedEntity);
                        displayEntities.Remove(changedEntity);
                        this.RaisePropertyChanged(x => x.DisplayEntities);
                        displayEntities.Insert(index, changedEntity);
                    }
                }

                this.RaisePropertyChanged(x => x.DisplayEntities);
                return;
            }

            if (sender == MainViewModel || sender == this)
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
                else if (loadPROJECT != null )
                    mainThreadDispatcher.BeginInvoke(new Action(() => InitializeAndLoadEntitiesLoaderDescription()));
            }
        }

        #region Collection Call Backs
        public void NewProjectionInitialization(RowEventArgs e, COMMODITY_GROUP_DIRECTProjection projectionEntity)
        {
            var gridView = (GridViewBase)e.Source;
            var grid = gridView.Grid;
            var masterGrid = grid.GetMasterGrid();

            if (masterGrid != null)
            {
                var masterRowHandle = grid.GetMasterRowHandle();
                COMMODITY_GROUP_DIRECTProjection masterEntity = (COMMODITY_GROUP_DIRECTProjection)masterGrid.GetRow(masterRowHandle);
                projectionEntity.COMMODITY_GROUP.GUID_PARENT = masterEntity.GUID;
            }
        }

        public void ApplyProjectionPropertiesToEntity(COMMODITY_GROUP_DIRECTProjection projectionEntity, COMMODITY_GROUP_DIRECT entity)
        {
            if(loadPROJECT != null)
                projectionEntity.COMMODITY_GROUP.GUID_PROJECT = loadPROJECT.GUID;

            DataUtils.ShallowCopy(entity, projectionEntity.COMMODITY_GROUP);
            //workaround for created because Save() only sets the projection primary key, this is used for property redo where the interceptor only tampers with UPDATED and CREATED is left as null
            if (entity.CREATED.Date.Year == 1)
                projectionEntity.COMMODITY_GROUP.CREATED = DateTime.Now;

            entity.CREATED = projectionEntity.COMMODITY_GROUP.CREATED;
        }

        public void OnEntitiesSavedCallBack(Guid primaryKey, COMMODITY_GROUP_DIRECTProjection projectionEntity, COMMODITY_GROUP_DIRECT entity, bool isNewEntity)
        {
            projectionEntity.COMMODITY_GROUP.GUID = entity.GUID;
        }

        //Remove children before parent deletion
        private void EntitiesBeforeDeletion(IEnumerable<COMMODITY_GROUP_DIRECTProjection> entities)
        {
            //Undo manager is paused in bulk deletion and will be unpaused in bulk deletion too
            List<COMMODITY_GROUP_DIRECTProjection> childrenEntities = new List<COMMODITY_GROUP_DIRECTProjection>();
            foreach (var entity in entities)
            {
                var childrenEntitiesInTotal = entity.CHILD_COMMODITY_GROUP;
                List<COMMODITY_GROUP_DIRECTProjection> childrenEntitiesNotInDeletionCollection = new List<COMMODITY_GROUP_DIRECTProjection>();
                foreach (var childrenEntityInTotal in childrenEntitiesInTotal)
                {
                    if (!entities.Any(x => x.GUID == childrenEntityInTotal.GUID))
                        childrenEntitiesNotInDeletionCollection.Add(childrenEntityInTotal);
                }

                childrenEntities = childrenEntities.Concat(childrenEntitiesNotInDeletionCollection).ToList();
            }

            //can't use bulk delete here due to stack overflow
            foreach (var childrenEntity in childrenEntities)
            {
                MainViewModel.EntitiesUndoRedoManager.AddUndo(childrenEntity, null, null, null, EntityMessageType.Deleted);
                MainViewModel.Delete(childrenEntity);
            }
        }

        public void dragDropManager_Drop(object sender, DevExpress.Xpf.Grid.DragDrop.GridDropEventArgs e)
        {
            //foreach (var obj in e.DraggedRows)
            //{
            //    COMMODITY_CODE droppedCOMMODITY_CODE = obj as COMMODITY_CODE;
            //    if (droppedCOMMODITY_CODE == null)
            //        continue;

            //    COMMODITY_GROUP_DIRECT newCOMMODITY_GROUP_DIRECT = new COMMODITY_GROUP_DIRECT();
            //    newCOMMODITY_GROUP_DIRECT.GUID_COMMODITYCODE = droppedCOMMODITY_CODE.GUID;
            //    newCOMMODITY_GROUP_DIRECT.COMMODITY_CODE = droppedCOMMODITY_CODE;
            //    AddCOMMODITY_GROUP_DIRECT(newCOMMODITY_GROUP_DIRECT);
            //}

            //e.Handled = true;
        }
        #endregion
        #endregion

        #region View Properties
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get
            {
                return "COMMODITY_GROUP_DIRECTCollectionViewModelWrapper";
            }
        }

        ObservableCollection<COMMODITY_GROUP_DIRECTProjection> displayEntities;
        public ObservableCollection<COMMODITY_GROUP_DIRECTProjection> DisplayEntities
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                if (displayEntities == null)
                {
                    IEnumerable<COMMODITY_GROUP_DIRECTProjection> parentCOMMODITY_GROUP_DIRECTS = MainViewModel.Entities.Where(x => x.COMMODITY_GROUP.GUID_PARENT == null).AsEnumerable();
                    IEnumerable<COMMODITY_GROUP_DIRECTProjection> childCOMMODITY_GROUP_DIRECTS = MainViewModel.Entities.Where(x => x.COMMODITY_GROUP.GUID_PARENT != null).AsEnumerable();
                    displayEntities = new ObservableCollection<COMMODITY_GROUP_DIRECTProjection>(parentCOMMODITY_GROUP_DIRECTS);
                    foreach (COMMODITY_GROUP_DIRECTProjection displayEntity in displayEntities)
                    {
                        displayEntity.CHILD_COMMODITY_GROUP = new ObservableCollection<COMMODITY_GROUP_DIRECTProjection>(childCOMMODITY_GROUP_DIRECTS.Where(y => y.COMMODITY_GROUP.GUID_PARENT == displayEntity.GUID));
                    }
                }

                return displayEntities;
            }
        }
        #endregion

        #region View Commands
        public bool CanRefresh()
        {
            if (MainViewModel == null)
                return false;

            return MainViewModel.CanRefresh();
        }

        public void Refresh()
        {
            MainViewModel.Refresh();
            displayEntities = null;
            this.RaisePropertyChanged(x => x.DisplayEntities);
        }
        #endregion

        #region View Properties
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
                    collection = collection.OrderBy(x => x.CODE);
                return collection;
            }
        }
        #endregion

        #region View Behavior

        #endregion
    }
}