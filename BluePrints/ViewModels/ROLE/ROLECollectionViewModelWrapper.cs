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
using BluePrints.Common.ViewModel;
using System.Resources;
using System.Collections;
using System.Collections.ObjectModel;
using DevExpress.Xpf.Bars;
using System.Windows;

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
            SelectedPermissions = new ObservableCollection<RolePermissionAssignment>();
        }

        #region Database Operations
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> BluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        ResourceSet resourceSet = NavigationResources.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
        protected override void resolveParameters(object parameter)
        {
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<ROLE_PERMISSION, ROLE_PERMISSION, Guid, IBluePrintsEntitiesUnitOfWork>(BluePrintsUnitOfWorkFactory, x => x.ROLE_PERMISSIONS);
            loaderCollection.AddLoaderDescription<ROLE_COMMODITY, ROLE_COMMODITY, Guid, IBluePrintsEntitiesUnitOfWork>(BluePrintsUnitOfWorkFactory, x => x.ROLE_COMMODITIES);
            loaderCollection.AddLoaderDescription<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(BluePrintsUnitOfWorkFactory, x => x.DOCTYPES);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(BluePrintsUnitOfWorkFactory, x => x.ROLES);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<ROLE>, IQueryable<ROLEProjection>>
            specifyMainViewModelProjection()
        {
            return query => ROLEProjectionQueries.JoinROLE_PERMISSIONOnROLES(query, ROLE_PERMISSIONCollection, ROLE_COMMODITYCollection);
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
        public ObservableCollection<RolePermissionAssignment> SelectedPermissions { get; set; }
        BluePrintsEntitiesViewModel bluePrintsEntitiesViewModel;
        private List<RolePermissionAssignment> permissions;
        public List<RolePermissionAssignment> Permissions
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                if (DisplaySelectedEntity == null)
                    return null;

                if (permissions == null)
                {
                    permissions = new List<RolePermissionAssignment>();
                    if(bluePrintsEntitiesViewModel == null)
                    {
                        bluePrintsEntitiesViewModel = new BluePrintsEntitiesViewModel(false);
                        bluePrintsEntitiesViewModel.LoadSecurityEntries();
                    }

                    foreach (BluePrintsEntitiesModuleDescription module in bluePrintsEntitiesViewModel.Modules)
                    {
                        //don't allow current user to set permission to him/herself
                        if (LoginCredentials.getPermissionStatus(module.SecurityKey) == LoginCredentials.PermissionStatus.None)
                            continue;

                        ROLE_PERMISSION findROLE_PERMISSION = DisplaySelectedEntity.ROLE_PERMISSIONS.FirstOrDefault(x => x.PERMISSION == module.SecurityKey);
                        bool isSelectedRoleHasPermission = findROLE_PERMISSION != null;
                        bool isPermissionReadOnly = false;
                        if (isSelectedRoleHasPermission)
                            isPermissionReadOnly = findROLE_PERMISSION.ISREADONLY;

                        permissions.Add(ViewModelSource.Create(() => new RolePermissionAssignment() { DisplayName = module.ModuleTitle, SecurityKey = module.SecurityKey, PermissionId = module.NavigationId, PermissionParentId = module.ParentId, IsAssigned = isSelectedRoleHasPermission, IsReadOnly = isPermissionReadOnly, CanAssign = module.DocumentType != string.Empty, CanAssignReadOnly = permissionHasReadOnlyMode(module.SecurityKey) }));
                    }
                }

                return permissions;
            }
        }

        private bool permissionHasReadOnlyMode(string securityKey)
        {
            foreach (System.Collections.DictionaryEntry permission in resourceSet)
            {
                if(permission.Key.ToString() == securityKey)
                {
                    return permission.Value.ToString().ToUpper().Contains(@"READ/WRITE");
                }
            }

            return false;
        }

        public DocTypePermissionAssignment SelectedDocTypePermission { get; set; }
        public IEnumerable<DocTypePermissionAssignment> DocTypePermissions
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                var permissions = new List<DocTypePermissionAssignment>();

                if (DisplaySelectedEntity == null)
                    return null;

                permissions.AddRange(DisplaySelectedEntity.ROLE_COMMODITIES.Select(x => new DocTypePermissionAssignment() { DocType = findDOCTYPE(x.GUID_COMMODITY), IsAssigned = true }).ToList());
                foreach (DOCTYPE docType in DOCTYPECollection)
                {
                    DocTypePermissionAssignment findPermission = permissions.Where(x => x.DocType != null).FirstOrDefault(x => x.DocType.GUID == docType.GUID);
                    if (findPermission == null)
                        permissions.Add(new DocTypePermissionAssignment() { DocType = docType, IsAssigned = false });
                }

                return permissions.OrderBy(x => x.DocType.NAME);
            }
        }

        private DOCTYPE findDOCTYPE(Guid guid)
        {
            return DOCTYPECollection.FirstOrDefault(x => x.GUID == guid);
        }

        private void refreshPermissions()
        {
            permissions = null;
            this.RaisePropertyChanged(x => x.Permissions);
            this.RaisePropertyChanged(x => x.DocTypePermissions);
        }

        //because tree list doesn't update when property changed is raised, use refreshdata as workaround
        private void refreshPermissionsViewOnly()
        {
            GridControlService.RefreshData();
        }

        public override void OnDisplaySelectedEntityChanged(ROLEProjection entity)
        {
            refreshPermissions();
            base.OnDisplaySelectedEntityChanged(entity);
        }

        public void PermissionCellValueChanging(TreeListCellValueChangedEventArgs e)
        {
            RolePermissionAssignment editPermission = (RolePermissionAssignment)e.Row;
            changePermission(editPermission, e.Column.FieldName, (bool)e.Value);
            refreshPermissionsViewOnly();
        }

        private void changePermission(RolePermissionAssignment editPermission, string fieldName, bool newValue)
        {
            bool isAssign = false;
            bool isAssignReadOnly = false;

            if (newValue)
                isAssign = true;

            //because the same permission can exists in different areas in the list, all of them should be updated equally
            List<RolePermissionAssignment> editPermissions = Permissions.Where(x => x.SecurityKey == editPermission.SecurityKey).ToList();
            if (fieldName == BindableBase.GetPropertyName(() => new RolePermissionAssignment().IsReadOnly))
            {
                if (newValue)
                    isAssignReadOnly = true;
                else //when read only checkbox is clicked is assign should still hold true
                {
                    isAssign = true;
                    editPermissions.ForEach(x => x.IsReadOnly = false);
                }
            }

            if (isAssign)
            {
                ROLE_PERMISSION editROLE_PERMISSION = ROLE_PERMISSIONViewModel.Entities.FirstOrDefault(x => x.PERMISSION == editPermission.SecurityKey && x.GUID_ROLE == DisplaySelectedEntity.GUID);
                if (editROLE_PERMISSION == null)
                    editROLE_PERMISSION = new ROLE_PERMISSION();

                editROLE_PERMISSION.GUID_ROLE = DisplaySelectedEntity.GUID;
                editROLE_PERMISSION.PERMISSION = editPermission.SecurityKey;
                editROLE_PERMISSION.ISREADONLY = isAssignReadOnly;
                editPermissions.ForEach(x => x.IsAssigned = true);
                editPermissions.ForEach(x => x.IsReadOnly = isAssignReadOnly);

                //must add from collection before saving because it'll invoke OnPersistentAfterAuxiliaryEntitiesChanges, and trigger OnDisplaySelectedEntityChanged 
                ROLE_PERMISSION findNavigationalROLE_PERMISSION = DisplaySelectedEntity.ROLE_PERMISSIONS.FirstOrDefault(x => x.PERMISSION == editPermission.SecurityKey);
                if (findNavigationalROLE_PERMISSION == null)
                    DisplaySelectedEntity.ROLE_PERMISSIONS.Add(editROLE_PERMISSION);
                else
                    DataUtils.ShallowCopy(findNavigationalROLE_PERMISSION, editROLE_PERMISSION);

                ROLE_PERMISSIONViewModel.Save(editROLE_PERMISSION);
            }
            else
            {
                ROLE_PERMISSION existingROLE_PERMISSION = DisplaySelectedEntity.ROLE_PERMISSIONS.FirstOrDefault(x => x.PERMISSION == editPermission.SecurityKey);
                if (existingROLE_PERMISSION != null)
                {
                    //must remove from collection before deletion because it'll invoke OnPersistentAfterAuxiliaryEntitiesChanges, and trigger OnDisplaySelectedEntityChanged 
                    DisplaySelectedEntity.ROLE_PERMISSIONS.Remove(existingROLE_PERMISSION);
                    ROLE_PERMISSIONViewModel.Delete(existingROLE_PERMISSION);
                }

                editPermissions.ForEach(x => x.IsAssigned = false);
                editPermissions.ForEach(x => x.IsReadOnly = false);
            }
        }

        protected override void OnPersistentAfterAuxiliaryEntitiesChanges(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
            if (changedType == typeof(ROLE_PERMISSION))
                return;

            base.OnPersistentAfterAuxiliaryEntitiesChanges(key, changedType, messageType, sender, isBulkRefresh);
        }

        public void DocTypePermissionCellValueChanging(CellValueChangedEventArgs e)
        {
            DocTypePermissionAssignment editingDocTypePermissionAssignment = (DocTypePermissionAssignment)e.Row;
            //don't need to validate fieldname since only this field is changeable in role permission grid control

            bool newValue = (bool)e.Value;
            if (newValue)
            {
                ROLE_COMMODITY newROLE_COMMODITY = new ROLE_COMMODITY();
                newROLE_COMMODITY.GUID_ROLE = DisplaySelectedEntity.GUID;
                newROLE_COMMODITY.GUID_COMMODITY = editingDocTypePermissionAssignment.DocType.GUID;
                ROLE_COMMODITYViewModel.Save(newROLE_COMMODITY);
                DisplaySelectedEntity.ROLE_COMMODITIES.Add(newROLE_COMMODITY);
                e.Handled = true;
            }
            else
            {
                ROLE_COMMODITY existingROLE_COMMODITY = DisplaySelectedEntity.ROLE_COMMODITIES.FirstOrDefault(x => x.GUID_COMMODITY == editingDocTypePermissionAssignment.DocType.GUID);
                if (existingROLE_COMMODITY != null)
                {
                    ROLE_COMMODITYViewModel.Delete(existingROLE_COMMODITY);
                    DisplaySelectedEntity.ROLE_COMMODITIES.Remove(existingROLE_COMMODITY);
                    e.Handled = true;
                }
            }

            refreshPermissions();
            base.CellValueChanging(e);
        }

        public bool CanUncheckPermissions(object button)
        {
            GridMenuInfo info = GridPopupMenuBase.GetGridMenuInfo((DependencyObject)button) as GridMenuInfo;
            if (IsLoading || button == null || info == null)
                return false;

            if (info.Column.FieldName != BindableBase.GetPropertyName(() => new RolePermissionAssignment().IsAssigned) && info.Column.FieldName != BindableBase.GetPropertyName(() => new RolePermissionAssignment().IsReadOnly))
                return false;
            else
                return true;
        }

        public void UncheckPermissions(object button)
        {
            BulkEditPermission(button, false);
        }

        public bool CanCheckPermissions(object button)
        {
            return CanUncheckPermissions(button);
        }

        public void CheckPermissions(object button)
        {
            BulkEditPermission(button, true);
        }

        public void BulkEditPermission(object button, bool isChecked)
        {
            GridMenuInfo info = GridPopupMenuBase.GetGridMenuInfo((DependencyObject)button) as GridMenuInfo;
            bool valueToFill = isChecked;

            //because bulk save isn't used SelectedPermissions will be changed after saving, so cache it first
            List<RolePermissionAssignment> fillPermissions = SelectedPermissions.ToList();
            var bulkSaveEntities = new List<BASELINE_ITEMProgress>();

            for (int i = 0; i < fillPermissions.Count; i++)
            {
                changePermission(fillPermissions[i], info.Column.FieldName, valueToFill);
            }

            refreshPermissionsViewOnly();
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

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
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

        public override string UnifiedRowValidation(ROLEProjection projection)
        {
            return string.Empty;
        }


        public override string UnifiedValueValidation(ROLEProjection projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
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

        public CollectionViewModel<ROLE_COMMODITY, ROLE_COMMODITY, Guid, IBluePrintsEntitiesUnitOfWork> ROLE_COMMODITYViewModel
        {
            get
            {
                if (loaderCollection == null)
                    return null;

                return
                    (CollectionViewModel<ROLE_COMMODITY, ROLE_COMMODITY, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<ROLE_COMMODITY>();
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

        public IEnumerable<ROLE_COMMODITY> ROLE_COMMODITYCollection
        {
            get
            {
                var collection = GetEntities<ROLE_COMMODITY>();
                return collection;
            }

        }
        public IEnumerable<DOCTYPE> DOCTYPECollection
        {
            get
            {
                var collection = GetEntities<DOCTYPE>();
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
        public RolePermissionAssignment()
        {

        }

        public string DisplayName { get; set; }
        public string SecurityKey { get; set; }
        public string PermissionId { get; set; }
        public string PermissionParentId { get; set; }
        public bool IsAssigned { get; set; }
        public bool CanAssign { get; set; }
        public bool IsReadOnly { get; set; }
        public bool CanAssignReadOnly { get; set; }
    }

    public class DocTypePermissionAssignment
    {
        public DOCTYPE DocType { get; set; }
        public bool IsAssigned { get; set; }
    }
}
