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
using BluePrints.Common.Base;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the ROLE collection view model.
    /// </summary>
    public partial class ROLECollectionViewModelWrapper :
        BluePrintsProjectionTreeCollectionWrapper
        <ROLE, ROLEProjection, Guid, IBluePrintsEntitiesUnitOfWork>
    {
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

        protected override void resolveParameters(object parameter)
        {
        }

        protected override void initializeEntitiesLoadersDescription()
        {
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription<ROLE_PERMISSION, ROLE_PERMISSION, Guid, IBluePrintsEntitiesUnitOfWork>(BluePrintsUnitOfWorkFactory, x => x.ROLE_PERMISSIONS);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(BluePrintsUnitOfWorkFactory, x => x.ROLES);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<ROLE>, IQueryable<ROLEProjection>>
            specifyMainViewModelProjection()
        {
            return query => ROLEProjectionQueries.JoinROLE_PERMISSIONOnROLES(query, ROLE_PERMISSIONCollection);
        }
        #endregion

        #region View Behavior
        /// <summary>
        /// Remembers an entity property old value for undoing
        /// Since CollectionViewModelBase is a POCO view model, an the instance of this class will also expose the TreeListImmediateSaveCommand property that can be used as a binding source in views.
        /// </summary>
        public void TreeListImmediateSave(TreeListCellValueChangedEventArgs e)
        {
            if (e.Column.FieldName != "Entity.ISMANAGER")
                return;

            var projection = (ROLEProjection)e.Row;

            MainViewModel.EntitiesUndoRedoManager.PauseActionId();
            MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, e.Column.FieldName, e.OldValue, e.Value, EntityMessageType.Changed);
            DataUtils.SetNestedValue(e.Column.FieldName, projection, e.Value);
            MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();

            ROLEProjection selectedEntity = DisplaySelectedEntity;
            MainViewModel.Save(projection);
            DisplaySelectedEntity = selectedEntity;
            this.RaisePropertyChanged(x => x.DisplaySelectedEntity);
        }

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
                if (DisplaySelectedEntity == null && MainViewModel.Entities.Count > 0)
                    DisplaySelectedEntity = MainViewModel.Entities.First();

                if (DisplaySelectedEntity == null)
                    return null;

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

        protected override void CellValueAnyRowChanging(CellValueChangedEventArgs e)
        {
            RolePermissionAssignment editingRolePermissionAssignment = (RolePermissionAssignment)e.Row;
            //don't need to validate fieldname since only this field is changeable in role permission grid control

            bool newValue = (bool)e.Value;
            if (newValue)
            {
                ROLE_PERMISSION newROLE_PERMISSION = new ROLE_PERMISSION();
                newROLE_PERMISSION.GUID_ROLE = DisplaySelectedEntity.GUID;
                newROLE_PERMISSION.PERMISSION = editingRolePermissionAssignment.PermissionKey;
                ROLE_PERMISSIONViewModel.Save(newROLE_PERMISSION);
                DisplaySelectedEntity.ROLE_PERMISSIONS.Add(newROLE_PERMISSION);
                e.Handled = true;
            }
            else
            {
                ROLE_PERMISSION existingROLE_PERMISSION = DisplaySelectedEntity.ROLE_PERMISSIONS.FirstOrDefault(x => x.PERMISSION == editingRolePermissionAssignment.PermissionKey);
                if (existingROLE_PERMISSION != null)
                {
                    ROLE_PERMISSIONViewModel.Delete(existingROLE_PERMISSION);
                    DisplaySelectedEntity.ROLE_PERMISSIONS.Remove(existingROLE_PERMISSION);
                    e.Handled = true;
                }
            }

            refreshPermissions();
            base.CellValueAnyRowChanging(e);
        }
        #endregion

        /// <summary>
        /// Save expanded state before closing
        /// </summary>
        protected override void OnClose(CancelEventArgs e)
        {
            MainViewModel.BulkSave(MainViewModel.Entities);
            base.OnClose(e);
        }
        #endregion

        protected override void PopulateNewProjection(ROLEProjection projection)
        {
            projection.Entity.NAME = "(new)";
        }

        protected override string GetParentEntityKeyFieldName()
        {
            return BindableBase.GetPropertyName(() => new ROLEProjection().Entity) + "." + BindableBase.GetPropertyName(() => new ROLE().PARENTGUID);
        }

        protected override string GetSortOrderFieldName()
        {
            return BindableBase.GetPropertyName(() => new ROLEProjection().Entity) + "." + BindableBase.GetPropertyName(() => new ROLE().SORTORDER);
        }

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
            //DocumentManagerService.ShowExistingEntityDocumentWithLogging(documentInfo, this);
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
