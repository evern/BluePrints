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
using System.Windows.Input;
using System.Windows;
using System.Threading.Tasks;
using System.ComponentModel;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the COMMODITIES collection view model.
    /// </summary>
    public partial class COMMODITY_GROUP_DIRECTCollectionViewModelWrapper :
        CollectionViewModelsWrapper
        <COMMODITY_GROUP_DIRECT, COMMODITY_GROUP_DIRECTProjection, Guid, IBluePrintsEntitiesUnitOfWork,
            CollectionViewModel
            <COMMODITY_GROUP_DIRECT, COMMODITY_GROUP_DIRECTProjection, Guid, IBluePrintsEntitiesUnitOfWork>>
    {
        /// <summary>
        /// Creates a new instance of COMMODITY_GROUP_DIRECTCollectionViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static COMMODITY_GROUP_DIRECTCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new COMMODITY_GROUP_DIRECTCollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the COMMODITY_GROUP_DIRECTCollectionViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the COMMODITY_GROUP_DIRECTCollectionViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected COMMODITY_GROUP_DIRECTCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operation
        private BackgroundWorker displayEntitiesRefreshBackgroundWorker;
        private BackgroundWorker userStateRestoreBackgroundWorker;

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        private Guid RestoreSelectedEntityGuid;
        private List<Guid> RestoreSelectedEntitiesGuids = new List<Guid>();
        private List<Guid> RestoreExpandedGuids = new List<Guid>();

        protected override void InitializeParameters(object parameter)
        {
            RestoreSelectedEntityGuid = Guid.Empty;
            DisplaySelectedEntities = new ObservableCollection<COMMODITY_GROUP_DIRECTProjection>();
            userStateRestoreBackgroundWorker = new BackgroundWorker();
            userStateRestoreBackgroundWorker.DoWork += userStateRestoreBackgroundWorker_DoWork;
            userStateRestoreBackgroundWorker.WorkerSupportsCancellation = true;

            displayEntitiesRefreshBackgroundWorker = new BackgroundWorker();
            displayEntitiesRefreshBackgroundWorker.DoWork += displayEntitiesRefreshBackgroundWorker_DoWork;
            displayEntitiesRefreshBackgroundWorker.WorkerSupportsCancellation = true;
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddEntitiesLoader<COMMODITY_CODE, COMMODITY_CODE, Guid, IBluePrintsEntitiesUnitOfWork>(1,
                bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, COMMODITY_CODEProjectionFunc, null, null,
                null, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(2,
                bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddEntitiesLoader<UOM, UOM, Guid, IBluePrintsEntitiesUnitOfWork>(3,
                bluePrintsUnitOfWorkFactory, x => x.UOMS);
            InvokeEntitiesLoaderDescriptionLoading();
        }

        private Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>> COMMODITY_CODEProjectionFunc()
        {
            return query => query.Where(x => x.COMMODITYCODETYPE == CommodityCodeType.Direct && x.GUID_PROJECT == null);
        }

        public IQueryable<COMMODITY_CODE> getLastChildrenCOMMODITY_CODE(IQueryable<COMMODITY_CODE> COMMODITY_CODE)
        {
            var readCOMMODITY_CODE =
                COMMODITY_CODE.Where(x => x.COMMODITYCODETYPE == CommodityCodeType.Direct).ToArray().AsEnumerable();
            var lastCOMMODITY_CODE =
                readCOMMODITY_CODE.Where(x => !readCOMMODITY_CODE.Any(y => y.GUID_PARENT == x.GUID));

            return lastCOMMODITY_CODE.AsQueryable();
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_GROUP_DIRECT);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoader.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<COMMODITY_GROUP_DIRECT>, IQueryable<COMMODITY_GROUP_DIRECTProjection>>
            ConstructMainViewModelProjection()
        {
            return query => COMMODITY_GROUP_DIRECTProjectionQueries.ConvertToProjectionCOMMODITY_GROUP_DIRECT(query);
        }

        #region View Refresh

        protected override void AssignCallBacksAndRaisePropertyChange(
            IEnumerable<COMMODITY_GROUP_DIRECTProjection> entities)
        {
            MainViewModel.OnBeforeEntitiesDeleteCallBack = EntitiesBeforeDeletion;
            MainViewModel.IsContinueNewRowFromViewCallBack = NewRowAddUndoAndSave;
            MainViewModel.ApplyEntityPropertiesToProjectionCallBack = OnEntitiesSavedCallBack;
            MainViewModel.IsValidFromViewCallBack = AdditionalCellValidation;
            MainViewModel.AdditionalValidateRowCallBack = AdditionalRowValidation;
            MainViewModel.ApplyProjectionPropertiesToEntityCallBack = ApplyProjectionPropertiesToEntity;

            MainViewModel.SetParentViewModel(this);
            RefreshView();
        }

        protected override void OnAfterEntitiesChanged(object key, Type changedType, EntityMessageType messageType,
            object sender)
        {
            if (changedType == typeof(COMMODITY_GROUP_DIRECT))
            {
                if (sender.ToString() != MainViewModel.ToString())
                    mainThreadDispatcher.BeginInvoke(new Action(() => MainViewModel.Refresh()));
                else
                    mainThreadDispatcher.BeginInvoke(new Action(() => MainViewModel.RefreshWithoutClearingUndoManager()));

                RefreshDisplayEntities();
            }

            if (sender.ToString() == MainViewModel.ToString() || sender.ToString() == ToString())
                return;

            if (MainViewModel != null)
                mainThreadDispatcher.BeginInvoke(new Action(() => MainViewModel.Refresh()));
        }

        private void displayEntitiesRefreshBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            System.Threading.Thread.Sleep(100);
            if(displayEntitiesRefreshBackgroundWorker.CancellationPending)
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
            if (userStateRestoreBackgroundWorker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            mainThreadDispatcher.BeginInvoke(new Action(() => restoreViewState()));
        }

        protected override void storeViewState()
        {
            RestoreSelectedEntityGuid = Guid.Empty;
            RestoreSelectedEntitiesGuids.Clear();
            RestoreExpandedGuids.Clear();

            foreach (var selectedEntity in DisplaySelectedEntities)
                RestoreSelectedEntitiesGuids.Add(new Guid(selectedEntity.GUID.ToString()));

            foreach (var entity in DisplayEntities)
                if (entity.ISEXPANDED)
                    RestoreExpandedGuids.Add(entity.GUID);

            if (DisplaySelectedEntity != null)
                RestoreSelectedEntityGuid = DisplaySelectedEntity.GUID;
        }

        protected override void restoreViewState()
        {
            var restoreSelectedEntities =
                DisplayEntities.Concat(DisplayEntities.SelectMany(x => x.CHILD_COMMODITY_GROUP))
                    .Where(x => RestoreSelectedEntitiesGuids.Any(y => y == x.GUID));
            DisplaySelectedEntities.Clear();
            if (restoreSelectedEntities.Count() > 0)
                foreach (var restoreSelectedEntity in restoreSelectedEntities)
                    DisplaySelectedEntities.Add(restoreSelectedEntity);

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
                    DisplayEntities.Concat(DisplayEntities.SelectMany(x => x.CHILD_COMMODITY_GROUP))
                        .FirstOrDefault(x => x.GUID == RestoreSelectedEntityGuid);
                if (restoreSelectedEntity != null)
                    DisplaySelectedEntity = restoreSelectedEntity;
            }
        }

        #endregion

        #region Collection Call Backs

        private bool NewRowAddUndoAndSave(RowEventArgs e, COMMODITY_GROUP_DIRECTProjection projectionEntity)
        {
            var gridView = (GridViewBase) e.Source;
            var grid = gridView.Grid;
            var masterGrid = grid.GetMasterGrid();

            if (masterGrid != null)
            {
                var masterRowHandle = grid.GetMasterRowHandle();
                var masterEntity =
                    (COMMODITY_GROUP_DIRECTProjection) masterGrid.GetRow(masterRowHandle);
                if (masterEntity.COMMODITY_GROUP.GUID_COMMODITYCODE == null)
                {
                    projectionEntity.COMMODITY_GROUP.GUID_PARENT = masterEntity.GUID;
                }
                else
                {
                    masterEntity.CHILD_COMMODITY_GROUP.Remove(projectionEntity);
                    MessageBoxService.ShowMessage(CommonResources.CommodityGroup_CannotAddChild);
                    return false;
                }
            }

            return true;
        }

        private bool AdditionalRowValidation(GridRowValidationEventArgs e)
        {
            var gridControl = (GridControl) e.Source;
            var masterGrid = gridControl.GetMasterGrid();

            if (masterGrid != null)
            {
                var editingCOMMODITY_GROUP = (COMMODITY_GROUP_DIRECTProjection) e.Row;
                if (editingCOMMODITY_GROUP.COMMODITY_GROUP.GUID_COMMODITYCODE == null)
                {
                    e.IsValid = false;
                    e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Critical;
                    e.ErrorContent = CommonResources.CommodityGroup_MustSelectCommodity;
                    return false;
                }
            }

            return true;
        }

        private bool AdditionalCellValidation(GridCellValidationEventArgs e)
        {
            if (e.Column.FieldName == "COMMODITY_GROUP.GUID_COMMODITYCODE")
            {
                var editingCOMMODITY_GROUP = (COMMODITY_GROUP_DIRECTProjection)e.Row;
                if (editingCOMMODITY_GROUP.CHILD_COMMODITY_GROUP != null &&
                    editingCOMMODITY_GROUP.CHILD_COMMODITY_GROUP.Count > 0)
                {
                    e.IsValid = false;
                    e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Critical;
                    e.ErrorContent = CommonResources.CommodityGroup_CannotAssignCommodity;
                    return false;
                }

                //Avoid user from selecting WBS COMMODITY_CODE
                var newCOMMODITY_CODE = COMMODITY_CODECollection.First(x => x.GUID == (Guid) e.Value);
                if (COMMODITY_CODECollection.Any(x => x.GUID_PARENT == newCOMMODITY_CODE.GUID))
                {
                    e.IsValid = false;
                    e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Critical;
                    e.ErrorContent = CommonResources.CommodityGroup_CannotSelectParent;
                    return false;
                }
            }

            return true;
        }

        public void ExistingChildrenRowAddUndoAndSave(CellValueChangedEventArgs e)
        {
            var editedCOMMODITY = (COMMODITY_GROUP_DIRECTProjection) e.Row;
            if (e.RowHandle == DataControlBase.NewItemRowHandle)
                return;

            MainViewModel.EntitiesUndoRedoManager.PauseActionId();
            MainViewModel.EntitiesUndoRedoManager.AddUndo(editedCOMMODITY, e.Column.FieldName, e.OldValue, e.Value,
                EntityMessageType.Changed);
            MainViewModel.Save(editedCOMMODITY);
            MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
        }

        private void ApplyProjectionPropertiesToEntity(COMMODITY_GROUP_DIRECTProjection projectionEntity,
            COMMODITY_GROUP_DIRECT entity)
        {
            DataUtils.ShallowCopy(entity, projectionEntity.COMMODITY_GROUP);
            //workaround for created because Save() only sets the projection primary key, this is used for property redo where the interceptor only tampers with UPDATED and CREATED is left as null
            if (entity.CREATED.Date.Year == 1)
                projectionEntity.COMMODITY_GROUP.CREATED = DateTime.Now;

            entity.CREATED = projectionEntity.COMMODITY_GROUP.CREATED;
        }

        private void OnEntitiesSavedCallBack(Guid primaryKey, COMMODITY_GROUP_DIRECTProjection projectionEntity,
            COMMODITY_GROUP_DIRECT entity, bool isNewEntity)
        {
            projectionEntity.COMMODITY_GROUP.GUID = entity.GUID;
        }

        //Remove children before parent deletion
        private void EntitiesBeforeDeletion(IEnumerable<COMMODITY_GROUP_DIRECTProjection> entities)
        {
            //Undo manager is paused in bulk deletion and will be unpaused in bulk deletion too
            var childrenEntities = new List<COMMODITY_GROUP_DIRECTProjection>();
            var parentEntitiesNotInList =
                new List<COMMODITY_GROUP_DIRECTProjection>();

            foreach (var entity in entities)
            {
                var childrenEntitiesInTotal = entity.CHILD_COMMODITY_GROUP;
                var childrenEntitiesNotInDeletionCollection =
                    new List<COMMODITY_GROUP_DIRECTProjection>();
                foreach (var childrenEntityInTotal in childrenEntitiesInTotal)
                    if (!entities.Any(x => x.GUID == childrenEntityInTotal.GUID))
                        childrenEntitiesNotInDeletionCollection.Add(childrenEntityInTotal);

                COMMODITY_GROUP_DIRECTProjection parentEntity = null;
                if (entity.COMMODITY_GROUP.GUID_PARENT != Guid.Empty)
                {
                    parentEntity =
                        MainViewModel.Entities.FirstOrDefault(x => x.GUID == entity.COMMODITY_GROUP.GUID_PARENT);
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

        public Action Redraw;
        public Action<COMMODITY_GROUP_DIRECTProjection> SetIsRowExpanded;

        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "COMMODITY_GROUP_DIRECTCollectionViewModelWrapper"; }
        }

        private void RefreshDisplayEntities()
        {
            if (!displayEntitiesRefreshBackgroundWorker.IsBusy)
                displayEntitiesRefreshBackgroundWorker.RunWorkerAsync();
        }

        private void RestoreUserState()
        {
            if (!userStateRestoreBackgroundWorker.IsBusy)
                userStateRestoreBackgroundWorker.RunWorkerAsync();
        }

        private ObservableCollection<COMMODITY_GROUP_DIRECTProjection> displayEntities;

        public override ObservableCollection<COMMODITY_GROUP_DIRECTProjection> DisplayEntities
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                if (displayEntities == null)
                {
                    displayEntities = new ObservableCollection<COMMODITY_GROUP_DIRECTProjection>();
                    var parentCOMMODITY_GROUP_DIRECTS =
                        MainViewModel.Entities.Where(x => x.COMMODITY_GROUP.GUID_PARENT == null).AsEnumerable();
                    var allChildCOMMODITY_GROUP_DIRECTS =
                        MainViewModel.Entities.Where(x => x.COMMODITY_GROUP.GUID_PARENT != null).AsEnumerable();
                    foreach (
                        var parentCOMMODITY_GROUP_DIRECT in parentCOMMODITY_GROUP_DIRECTS)
                    {
                        var parentCOMMODITY_GROUP_DIRECTPOCO =
                            ViewModelSource.Create(() => new COMMODITY_GROUP_DIRECTProjection());
                        parentCOMMODITY_GROUP_DIRECTPOCO.GUID = parentCOMMODITY_GROUP_DIRECT.GUID;
                        DataUtils.ShallowCopy(parentCOMMODITY_GROUP_DIRECTPOCO.COMMODITY_GROUP,
                            parentCOMMODITY_GROUP_DIRECT.COMMODITY_GROUP);
                        displayEntities.Add(parentCOMMODITY_GROUP_DIRECTPOCO);
                    }

                    //displayEntities = new ObservableCollection<COMMODITY_GROUP_DIRECTProjection>(parentCOMMODITY_GROUP_DIRECTS);
                    foreach (var displayEntity in displayEntities)
                    {
                        var childCOMMODITY_GROUP_DIRECTS =
                            allChildCOMMODITY_GROUP_DIRECTS.Where(
                                y => y.COMMODITY_GROUP.GUID_PARENT == displayEntity.GUID);
                        foreach (
                            var childCOMMODITY_GROUP_DIRECT in childCOMMODITY_GROUP_DIRECTS
                        )
                        {
                            var childCOMMODITY_GROUP_DIRECTPOCO =
                                ViewModelSource.Create(() => new COMMODITY_GROUP_DIRECTProjection());
                            childCOMMODITY_GROUP_DIRECTPOCO.GUID = childCOMMODITY_GROUP_DIRECT.GUID;
                            DataUtils.ShallowCopy(childCOMMODITY_GROUP_DIRECTPOCO.COMMODITY_GROUP,
                                childCOMMODITY_GROUP_DIRECT.COMMODITY_GROUP);
                            displayEntity.CHILD_COMMODITY_GROUP.Add(childCOMMODITY_GROUP_DIRECTPOCO);
                        }
                    }

                    RestoreUserState();
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

        public override void FullRefresh()
        {
            MainViewModel.Refresh();
            displayEntities = null;

            this.RaisePropertyChanged(x => x.DisplayEntities);
        }

        public virtual bool CanBulkDelete()
        {
            return MainViewModel != null && MainViewModel.Entities != null && MainViewModel.Entities.Count > 0 &&
                   !IsLoading && DisplaySelectedEntities.Count > 0;
        }

        public void BulkDelete()
        {
            MainViewModel.EntitiesUndoRedoManager.PauseActionId();
            MainViewModel.BaseBulkDelete(DisplaySelectedEntities);
            MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
        }

        public void MasterRowExpanded(RowEventArgs e)
        {
            ((COMMODITY_GROUP_DIRECTProjection) e.Row).ISEXPANDED = true;
        }

        public void MasterRowCollapsed(RowEventArgs e)
        {
            ((COMMODITY_GROUP_DIRECTProjection) e.Row).ISEXPANDED = false;
        }

        private void ExpandDisplayRow(COMMODITY_GROUP_DIRECTProjection row)
        {
            row.ISEXPANDED = true;
            if (SetIsRowExpanded != null)
                SetIsRowExpanded(row);
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

        /// <summary>
        /// Influence column(s) when changes happens in other column
        /// </summary>
        public void CellValueChanging(CellValueChangedEventArgs e)
        {
            if (e.RowHandle != DataControlBase.NewItemRowHandle)
                return;

            var activeCOMMODITY_GROUP_DIRECT = (COMMODITY_GROUP_DIRECTProjection) e.Row;
            if (e.Column.FieldName ==
                BindableBase.GetPropertyName(() => new COMMODITY_GROUP_DIRECTProjection().COMMODITY_GROUP) + "." +
                BindableBase.GetPropertyName(() => new COMMODITY_GROUP_DIRECT().GUID_COMMODITYCODE))
            {
                var chosenCOMMODITY_CODE =
                    COMMODITY_CODECollection.FirstOrDefault(entity => entity.GUID == (Guid) e.Value);
                if (chosenCOMMODITY_CODE != null)
                {
                    activeCOMMODITY_GROUP_DIRECT.COMMODITY_GROUP.GUID_DISCIPLINE = chosenCOMMODITY_CODE.GUID_DISCIPLINE;
                    this.RaisePropertyChanged(x => x.DisplayEntities);
                }
            }
        }

        public void dragDropManager_Drop(object sender, DevExpress.Xpf.Grid.DragDrop.GridDropEventArgs e)
        {
            foreach (var obj in e.DraggedRows)
            {
                var droppedNode = obj as TreeListNode;
                if (droppedNode == null)
                    continue;

                var droppedCOMMODITY_CODE = droppedNode.Content as COMMODITY_CODE;
                if (droppedCOMMODITY_CODE == null || droppedCOMMODITY_CODE.COMMODITYCODETYPE != CommodityCodeType.Direct)
                    continue;

                var targetCOMMODITY_GROUP = e.TargetRow as COMMODITY_GROUP_DIRECTProjection;
                var newCOMMODITY_GROUP_DIRECT = new COMMODITY_GROUP_DIRECTProjection();
                newCOMMODITY_GROUP_DIRECT.COMMODITY_GROUP.DESCRIPTION = CommonResources.CommodityCodeGroup_New;

                if (targetCOMMODITY_GROUP != null)
                {
                    if (targetCOMMODITY_GROUP.COMMODITY_GROUP.GUID_COMMODITYCODE != null)
                    {
                        MessageBoxService.ShowMessage(CommonResources.CommodityGroup_CannotAssignCommodity);
                        continue;
                    }

                    newCOMMODITY_GROUP_DIRECT.COMMODITY_GROUP.GUID_PARENT = targetCOMMODITY_GROUP.GUID;
                    newCOMMODITY_GROUP_DIRECT.COMMODITY_GROUP.GUID_COMMODITYCODE = droppedCOMMODITY_CODE.GUID;

                    var errorMessage = string.Empty;
                    if (MainViewModel.IsValidEntity(newCOMMODITY_GROUP_DIRECT, ref errorMessage))
                    {
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(newCOMMODITY_GROUP_DIRECT, null, null, null,
                            EntityMessageType.Added);
                        MainViewModel.Save(newCOMMODITY_GROUP_DIRECT);
                    }
                    else
                    {
                        MessageBoxService.ShowMessage(errorMessage + " is not unique");
                    }
                }
            }

            e.Handled = true;
        }

        protected override void OnClose(CancelEventArgs e)
        {
            displayEntitiesRefreshBackgroundWorker.CancelAsync();
            userStateRestoreBackgroundWorker.CancelAsync();
            base.OnClose(e);
        }

        #endregion
    }
}