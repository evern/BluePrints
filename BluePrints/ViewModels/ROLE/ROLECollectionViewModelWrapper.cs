using System;
using System.Linq;
using DevExpress.Mvvm.POCO;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Data;
using BaseModel.ViewModel.Base;
using BaseModel.DataModel;
using BaseModel.ViewModel.Loader;
using System.Collections.Generic;
using System.Windows.Threading;
using System.ComponentModel;
using BaseModel.Misc;
using DevExpress.Xpf.Grid.TreeList;
using DevExpress.Mvvm;
using DevExpress.Xpf.Grid;
using System.Threading;
using BaseModel.ViewModel.Document;
using BluePrints.Common.Resources;
using System.Globalization;
using BluePrints.Common.Projections;
using BaseModel.Data.Helpers;
using BluePrints.Common;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the ROLE collection view model.
    /// </summary>
    public partial class ROLECollectionViewModelWrapper :
        CollectionViewModelsWrapper
        <ROLE, ROLEProjection, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        public Action NativeTreeListRefresh;
        /// <summary>
        /// Creates a new instance of ROLECollectionViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static ROLECollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new ROLECollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the ROLECollectionViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the ROLECollectionViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected ROLECollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> BluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        private BackgroundWorker refreshBackgroundWorker;
        protected override void InitializeParameters(object parameter)
        {
            refreshBackgroundWorker = new BackgroundWorker();
            refreshBackgroundWorker.DoWork += refreshBackgroundWorker_DoWork;
            refreshBackgroundWorker.WorkerSupportsCancellation = true;
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription<ROLE_PERMISSION, ROLE_PERMISSION, Guid, IBluePrintsEntitiesUnitOfWork>(BluePrintsUnitOfWorkFactory, x => x.ROLE_PERMISSIONS);
            InvokeEntitiesLoaderDescriptionLoading();
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(BluePrintsUnitOfWorkFactory, x => x.ROLES);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<ROLE>, IQueryable<ROLEProjection>>
            ConstructMainViewModelProjection()
        {
            return query => ROLEProjectionQueries.JoinROLE_PERMISSIONOnROLES(query, ROLE_PERMISSIONCollection);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<ROLEProjection> entities)
        {
            MainViewModel.OnAfterEntitiesDeletedCallBack = EntitiesAfterDeletion;
            MainViewModel.OnBeforeEntitiesDeleteCallBack = EntitiesBeforeDeletion;
            MainViewModel.ApplyProjectionPropertiesToEntityCallBack = ApplyProjectionPropertiesToEntity;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        private void refreshBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            System.Threading.Thread.Sleep(100);
            if (((BackgroundWorker)sender).CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            mainThreadDispatcher.BeginInvoke(new Action(() => MainViewModel.RefreshWithoutClearingUndoManager()));
        }

        #region Collection Call Backs
        public void ApplyProjectionPropertiesToEntity(ROLEProjection projectionEntity, ROLE entity)
        {
            DataUtils.ShallowCopy(entity, projectionEntity.Entity);
            //workaround for created because Save() only sets the projection primary key, this is used for property redo where the interceptor only tampers with UPDATED and CREATED is left as null
            if (entity.CREATED.Date.Year == 1)
                projectionEntity.Entity.CREATED = DateTime.Now;

            entity.CREATED = projectionEntity.Entity.CREATED;
        }

        //Remove children before parent deletion
        private void EntitiesBeforeDeletion(IEnumerable<ROLEProjection> entities)
        {
            //Undo manager is paused in bulk deletion and will be unpaused in bulk deletion too
            var childrenEntities = new List<ROLEProjection>();
            foreach (var entity in entities)
            {
                var childrenEntitiesInTotal = RecurseFindChildren(entity, MainViewModel.Entities);
                var childrenEntitiesNotInDeletionCollection = new List<ROLEProjection>();
                foreach (var childrenEntityInTotal in childrenEntitiesInTotal)
                    if (!entities.Any(x => x.GUID == childrenEntityInTotal.GUID))
                        childrenEntitiesNotInDeletionCollection.Add(childrenEntityInTotal);

                childrenEntities = childrenEntities.Concat(childrenEntitiesNotInDeletionCollection).ToList();
            }

            uniqueParent_Guids = new List<Guid>();
            //can't use bulk delete here due to stack overflow
            foreach (var childrenEntity in childrenEntities)
            {
                if (!uniqueParent_Guids.Any(x => x == childrenEntity.Entity.PARENTGUID))
                    uniqueParent_Guids.Add(childrenEntity.Entity.PARENTGUID);

                MainViewModel.EntitiesUndoRedoManager.AddUndo(childrenEntity, null, null, null,
                    EntityMessageType.Deleted);
                MainViewModel.Delete(childrenEntity);
            }
        }

        //Reorder tree after deletion
        private void EntitiesAfterDeletion(IEnumerable<ROLE> entities)
        {
            //Undo manager is paused in bulk deletion and will be unpaused in bulk deletion too
            //uniqueParent_Guids is initialized in EntitiesBeforeDeletion
            foreach (var entity in entities)
                if (!uniqueParent_Guids.Any(x => x == entity.PARENTGUID))
                    uniqueParent_Guids.Add(entity.PARENTGUID);

            MainViewModel.EntitiesUndoRedoManager.PauseActionId(); //save will unpause this
            ReorderAndSave(uniqueParent_Guids);
        }
        #endregion

        #endregion

        #region View Behavior
        private Guid Parent_GuidOldValue;
        private List<Guid> uniqueParent_Guids; //stores dropping entity parent guid before it gets reassigned

        #region Permissions
        public RolePermissionAssignment SelectedPermission { get; set; }
        public IEnumerable<RolePermissionAssignment> Permissions
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                var resourceSet = PermissionResources.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
                var permissions = new List<RolePermissionAssignment>();
                if (DisplaySelectedEntity == null && MainViewModel.Entities.Any())
                    DisplaySelectedEntity = MainViewModel.Entities.First();

                permissions.AddRange(DisplaySelectedEntity.ROLE_PERMISSIONS.Select(x => new RolePermissionAssignment() { PermissionKey = x.PERMISSION, IsAssigned = true }).ToList());
                foreach (System.Collections.DictionaryEntry permission in resourceSet)
                {
                    RolePermissionAssignment findPermission = permissions.FirstOrDefault(x => x.PermissionKey == permission.Key.ToString());
                    if(findPermission == null)
                        permissions.Add(new RolePermissionAssignment() { PermissionKey = permission.Key.ToString(), IsAssigned = false });
                }

                return permissions.OrderBy(x => x.PermissionKey);
            }
        }

        private void refreshPermissions()
        {
            this.RaisePropertyChanged(x => x.Permissions);

            //remove the selection instead of having it focused on first row
            SelectedPermission = null;
        }

        public override void OnDisplaySelectedEntityChanged(ROLEProjection entity)
        {
            refreshPermissions();
            base.OnDisplaySelectedEntityChanged(entity);
        }

        /// <summary>
        /// Influence column(s) when changes happens in other column
        /// </summary>
        public void CellValueChanging(CellValueChangedEventArgs e)
        {
            RolePermissionAssignment editingRolePermissionAssignment = (RolePermissionAssignment)e.Row;
            //don't need to validate fieldname since only this field is changeable in role permission grid control
            bool newValue = (bool)e.Value;
            if(newValue)
            {
                ROLE_PERMISSION newROLE_PERMISSION = new ROLE_PERMISSION();
                newROLE_PERMISSION.GUID_ROLE = DisplaySelectedEntity.GUID;
                newROLE_PERMISSION.PERMISSION = editingRolePermissionAssignment.PermissionKey;
                ROLE_PERMISSIONViewModel.Save(newROLE_PERMISSION);
                DisplaySelectedEntity.ROLE_PERMISSIONS.Add(newROLE_PERMISSION);
            }
            else
            {
                ROLE_PERMISSION existingROLE_PERMISSION = DisplaySelectedEntity.ROLE_PERMISSIONS.FirstOrDefault(x => x.PERMISSION == editingRolePermissionAssignment.PermissionKey);
                if (existingROLE_PERMISSION != null)
                {
                    ROLE_PERMISSIONViewModel.Delete(existingROLE_PERMISSION);
                    DisplaySelectedEntity.ROLE_PERMISSIONS.Remove(existingROLE_PERMISSION);
                }
            }

            refreshPermissions();
        }
        #endregion

        public void dragDropManager_Drop(object sender, DevExpress.Xpf.Grid.DragDrop.TreeListDropEventArgs e)
        {
            uniqueParent_Guids = new List<Guid>();
            Parent_GuidOldValue = Guid.Empty;

            if (e.TargetNode != null)
            {
                MainViewModel.EntitiesUndoRedoManager.PauseActionId(); //save will unpause this
                foreach (var obj in e.DraggedRows)
                {
                    var editROLE = e.SourceManager.GetObject(obj) as ROLEProjection;

                    Parent_GuidOldValue = editROLE.Entity.PARENTGUID;
                    if (!uniqueParent_Guids.Any(x => x == Parent_GuidOldValue))
                        uniqueParent_Guids.Add(Parent_GuidOldValue);
                }
            }
        }

        public void dragDropManager_Dropped(object sender, DevExpress.Xpf.Grid.DragDrop.TreeListDroppedEventArgs e)
        {
            Guid newParentGuid = Guid.Empty;
            if (e.TargetNode != null)
            {
                foreach (TreeListNode obj in e.DraggedRows)
                {
                    var droppedROLE = obj.Content as ROLEProjection;
                    var targetROLE = e.TargetNode.Content as ROLEProjection;

                    droppedROLE.Entity.OLDSORTORDER = droppedROLE.Entity.SORTORDER;
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(droppedROLE,
                    BindableBase.GetPropertyName(() => new ROLEProjection().Entity) + "." +
                    BindableBase.GetPropertyName(() => new ROLE().PARENTGUID), Parent_GuidOldValue,
                    droppedROLE.Entity.PARENTGUID, EntityMessageType.Changed);

                    if (e.DropTargetType == DropTargetType.InsertRowsAfter)
                    {
                        droppedROLE.Entity.SORTORDER = targetROLE.Entity.SORTORDER + 1;
                    }
                    else if (e.DropTargetType == DropTargetType.InsertRowsBefore)
                    {
                        droppedROLE.Entity.SORTORDER = targetROLE.Entity.SORTORDER - 1;
                    }
                    else
                    {
                        var targetROLEChild =
                            MainViewModel.Entities.Where(x => x.Entity.PARENTGUID == targetROLE.GUID);

                        var maxTargetChildrenOrder = 0;
                        if (targetROLEChild.Count() > 0)
                            maxTargetChildrenOrder = targetROLEChild.Max(x => x.Entity.SORTORDER);

                        maxTargetChildrenOrder += 1;

                        droppedROLE.Entity.SORTORDER = maxTargetChildrenOrder;
                    }

                    newParentGuid = droppedROLE.Entity.PARENTGUID;
                }

                if (!uniqueParent_Guids.Any(x => x == newParentGuid))
                    uniqueParent_Guids.Add(newParentGuid);

                ReorderAndSave(uniqueParent_Guids);
            }
        }

        private void ReorderAndSave(IEnumerable<Guid> guid_parents)
        {
            var childEntities = new List<ROLEProjection>();
            foreach (var guid_parent in guid_parents)
                childEntities = childEntities.Concat(ReorderAndSave(guid_parent, true)).ToList();

            MainViewModel.BulkSave(childEntities);
            NativeTreeListRefresh?.Invoke();
        }

        private IEnumerable<ROLEProjection> ReorderAndSave(Guid guid_parent, bool dontSave = false)
        {
            IEnumerable<ROLEProjection> childROLEs =
                MainViewModel.Entities.Where(x => x.Entity.PARENTGUID == guid_parent).OrderBy(x => x.Entity.SORTORDER).ToList();
            var childROLEsList = new List<ROLEProjection>(childROLEs);

            var project_WBSOrderCount = 10;
            foreach (var childROLE in childROLEsList)
            {
                if (childROLE.Entity.SORTORDER != project_WBSOrderCount)
                {
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(childROLE,
                        BindableBase.GetPropertyName(() => new ROLEProjection().Entity) + "." +
                        BindableBase.GetPropertyName(() => new ROLE().SORTORDER), childROLE.Entity.OLDSORTORDER == null ? childROLE.Entity.SORTORDER : childROLE.Entity.OLDSORTORDER,
                        project_WBSOrderCount, EntityMessageType.Changed);

                    childROLE.Entity.OLDSORTORDER = null; //Prepare for next possible drag-drop operation
                    childROLE.Entity.SORTORDER = project_WBSOrderCount;
                }

                project_WBSOrderCount += 10;
            }

            if (!dontSave)
                MainViewModel.BulkSave(childROLEsList);

            return childROLEsList;
        }

        public static IEnumerable<ROLEProjection> RecurseFindChildren(ROLEProjection parentEntity,
            IEnumerable<ROLEProjection> entities)
        {
            foreach (var entity in entities)
                if (entity.Entity.PARENTGUID == parentEntity.Entity.GUID)
                {
                    yield return entity;

                    foreach (var entityChild in RecurseFindChildren(entity, entities))
                        yield return entityChild;
                }
        }

        /// <summary>
        /// Save expanded state before closing
        /// </summary>
        protected override void OnClose(CancelEventArgs e)
        {
            MainViewModel.BulkSave(MainViewModel.Entities);
            base.OnClose(e);
        }
        #endregion

        #region View Commands

        public void AddROLERowBefore()
        {
            AddROLERow(false);
        }

        public void AddROLERowAfter()
        {
            AddROLERow(true);
        }

        private void AddROLERow(bool isAfter)
        {
            var project_WBSOrder = 0;
            Guid guid_parent = Guid.Empty;
            if (DisplaySelectedEntity != null)
            {
                if (isAfter)
                    project_WBSOrder = DisplaySelectedEntity.Entity.SORTORDER + 1;
                else
                    project_WBSOrder = DisplaySelectedEntity.Entity.SORTORDER - 1;

                guid_parent = DisplaySelectedEntity.Entity.PARENTGUID;
            }

            var newROLE = new ROLEProjection();

            newROLE.Entity.NAME = "(New)";
            newROLE.Entity.SORTORDER = project_WBSOrder;
            newROLE.Entity.PARENTGUID = guid_parent;
            MainViewModel.EntitiesUndoRedoManager.PauseActionId(); //Save will unpause this
            MainViewModel.EntitiesUndoRedoManager.AddUndo(newROLE, null, null, null, EntityMessageType.Added);
            MainViewModel.Save(newROLE);
            ReorderAndSave(guid_parent);
        }
        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "ROLEViewModelWrapper"; }
        }

        #endregion

        #region Navigation
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

            //DocumentInfo documentInfo = new DocumentInfo(loadProject.Guid.ToString() + DisplaySelectedEntity.Guid.ToString(), new EntitiesParameter<ROLE>(DisplaySelectedEntity), "CommodityCollectionView", "[" + DisplaySelectedEntity.Name + "] Commodities");
            //DocumentManagerService.ShowExistingEntityDocument(documentInfo, this);
        }

        public CollectionViewModel<ROLE_PERMISSION, ROLE_PERMISSION, Guid, IBluePrintsEntitiesUnitOfWork> ROLE_PERMISSIONViewModel
        {
            get
            {
                if (loaderCollection == null)
                    return null;

                return
                    (CollectionViewModel<ROLE_PERMISSION, ROLE_PERMISSION, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<ROLE_PERMISSION>();
            }
        }

        public IEnumerable<ROLE_PERMISSION> ROLE_PERMISSIONCollection
        {
            get
            {
                var collection = GetEntities<ROLE_PERMISSION>();
                return collection;
            }
        }

        public Dictionary<string, string> PermissionLookUp
        {
            get
            {
                return LoginCredentials.GetPermissionLookUpInDictionary();
            }
        }
        #endregion
    }

    public class RolePermissionAssignment
    {
        public string PermissionKey { get; set; }
        public bool IsAssigned { get; set; }
    }
}
