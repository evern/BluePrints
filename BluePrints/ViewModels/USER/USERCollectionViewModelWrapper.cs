using BaseModel.DataModel;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Resources;
using BluePrints.Data;
using BluePrints.P6EntitiesDataModel;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO.Ports;
using System.Windows.Threading;
using System.ComponentModel;
using DevExpress.Data.Filtering;
using BaseModel.ViewModel.Base;

namespace BluePrints.ViewModels
{
    public class USERCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of USERCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static USERCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new USERCollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the USERCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the USERCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected USERCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> pgaUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(true);
        IPrimeroEntitiesUnitOfWork primeroUnitOfWork;
        IPrimeroEntitiesUnitOfWork pgaUnitOfWork;
        //timer to scan serial port
        protected override void resolveParameters(object parameter)
        {
            primeroUnitOfWork = primeroUnitOfWorkFactory.CreateUnitOfWork();
            pgaUnitOfWork = pgaUnitOfWorkFactory.CreateUnitOfWork();
        }
        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<ROLE, ROLE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.ROLES);
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription(primeroUnitOfWorkFactory, x => x.STAFF, STAFFProjectionFunc);
            loaderCollection.AddLoaderDescription<OFFICE, OFFICE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.OFFICES);
            loaderCollection.AddLoaderDescription<PROJECT_PERMISSION, PROJECT_PERMISSION, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.PROJECT_PERMISSIONS);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc);
        }

        private Func<IRepositoryQuery<STAFF>, IQueryable<STAFF>> STAFFProjectionFunc()
        {
            return query => query.Where(x => x.ISACTIVE == "Y");
        }

        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.STATUS == ProjectStatus.Active);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.USERS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<USER>, IQueryable<USER>> specifyMainViewModelProjection()
        {
            if (LoginCredentials.IsAdmin)
                return query => USERCollectionPopulation(query.OrderBy(x => x.NAME));
            else if (LoginCredentials.CurrentUser.GUID_ROLE == null)
                return query => query.Where(x => x.GUID == Guid.Empty);
            else
                return query => USERCollectionPopulation(query.OrderBy(x => x.NAME));
            //else
            //    return query => query.ToArray().Where(x => x.GUID_ROLE == null || x.GUID_ROLE == LoginCredentials.CurrentUser.GUID_ROLE || ChildrenRoles((Guid)LoginCredentials.CurrentUser.GUID_ROLE).Contains((Guid)x.GUID_ROLE)).AsQueryable();
        }

        public IQueryable<USER> USERCollectionPopulation(IQueryable<USER> USERS)
        {
            List<USER> userList = USERS.ToList();
            userList.ForEach(x => populateUserProperties(x, OFFICECollection));
            userList.ForEach(x => populateUserAuthorisedProjects(x, PROJECTCollection));

            return userList.AsQueryable();
        }

        private void populateUserProperties(USER user, IEnumerable<OFFICE> OFFICECollection)
        {
            if (user.OFFICE == null && user.GUID_OFFICE != null)
                user.OFFICE = OFFICECollection.FirstOrDefault(x => x.GUID == user.GUID_OFFICE);

            user.Update();
        }

        private void populateUserAuthorisedProjects(USER user, IEnumerable<PROJECT> PROJECTCollection)
        {
            user.Projects = PROJECTCollection.Where(project => PROJECT_PERMISSIONCollection.Any(permission => permission.GUID_USER == user.GUID && permission.GUID_PROJECT == project.GUID)).ToList();
            user.Update();
        }

        public IEnumerable<Guid> ChildrenRoles(Guid roleGuid)
        {
            foreach (var role in ROLECollection)
                if (role.PARENTGUID == roleGuid)
                {
                    yield return role.GUID;

                    foreach (var entityChild in ChildrenRoles(role.GUID))
                        yield return entityChild;
                }
        }

        public override void UnifiedCellValueChanged(string field_name, object old_value, object new_value, USER projection, bool isNew)
        {
            if(field_name == BindableBase.GetPropertyName(() => new USER().GUID_OFFICE))
            {
                populateUserProperties(projection, OFFICECollection);
            }

            base.UnifiedCellValueChanged(field_name, old_value, new_value, projection, isNew);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<USER> entities)
        {
            //MainViewModel.OnBeforeEntitySavedIsContinueCallBack = OnBeforeEntitySaved;
            MainViewModel.OnAfterEntitySavedCallBack = OnAfterEntitySaved;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            HideLeave = true;
            this.RaisePropertyChanged(x => x.HideLeave);
            base.OnAfterAssignedCallbackAndRaisePropertyChanged();
        }
        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public void OnAfterEntitySaved(USER projection, USER entity, bool isNewEntity)
        {
            if(isNewEntity && (entity.EXO_STAFF_ID == null || entity.EXO_STAFF_ID_REMOTE == null))
            {
                entity.START_DATE = DateTime.Now;

                if(entity.EXO_STAFF_ID == null)
                    entity.EXO_STAFF_ID = getExoStaffId(entity, PerthSTAFFCollection);

                if(entity.EXO_STAFF_ID_REMOTE == null)
                    entity.EXO_STAFF_ID_REMOTE = getExoStaffId(entity, MontrealSTAFFCollection);
            }

            saveProjectAssignments(entity);
        }
        #endregion

        #region Token Saving Behavior
        private void saveProjectAssignments(USER entity)
        {
            if (entity.Project_Assignments != null)
            {
                List<PROJECT_PERMISSION> remove_projects = new List<PROJECT_PERMISSION>();
                foreach (PROJECT_PERMISSION assignment in PROJECT_PERMISSIONCollection.Where(x => x.GUID_USER == entity.GUID))
                {
                    if (!entity.Project_Assignments.Any(x => x.GUID == assignment.GUID))
                        remove_projects.Add(assignment);
                }

                PROJECT_PERMISSIONCollectionViewModel.BaseBulkDelete(remove_projects);

                List<PROJECT_PERMISSION> add_projects = new List<PROJECT_PERMISSION>();
                foreach (PROJECT project in entity.Project_Assignments)
                {
                    if (!entity.PROJECT_PERMISSION.Any(x => x.GUID == project.GUID))
                        add_projects.Add(new PROJECT_PERMISSION() { GUID_PROJECT = project.GUID, GUID_USER = entity.GUID });
                }

                PROJECT_PERMISSIONCollectionViewModel.BulkSave(add_projects);
            }
            else
            {
                List<PROJECT_PERMISSION> remove_projects = new List<PROJECT_PERMISSION>();
                foreach (PROJECT_PERMISSION assignment in PROJECT_PERMISSIONCollection.Where(x => x.GUID_USER == entity.GUID))
                {
                    remove_projects.Add(assignment);
                }

                PROJECT_PERMISSIONCollectionViewModel.BaseBulkDelete(remove_projects);
            }
        }
        #endregion

        #region View Properties

        private int? getExoStaffId(USER bluePrintsUser, IEnumerable<STAFF> officeSpecificStaffCollection)
        {
            if (bluePrintsUser.GUID_OFFICE == null)
                return null;

            string exoGuessUserName = bluePrintsUser.FIRST_NAME.ToUpper() + " " + bluePrintsUser.LAST_NAME.ToUpper();
            OFFICE findOffice = OFFICECollection.FirstOrDefault(x => x.GUID == bluePrintsUser.GUID_OFFICE);
            if (findOffice == null)
                return null;

            STAFF exoSTAFF = officeSpecificStaffCollection.FirstOrDefault(x => x.NAME.Contains(exoGuessUserName));
            if (exoSTAFF != null)
            {
                return exoSTAFF.STAFFNO;
            }
            else
            {
                List<string> delimitedNames = bluePrintsUser.NAME.Split('.').ToList();
                string exoGuessUserName2 = string.Empty;
                foreach (string delimitedName in delimitedNames)
                {
                    exoGuessUserName2 += delimitedName.ToUpper() + " ";
                }

                exoGuessUserName2 = exoGuessUserName2.Trim();
                STAFF exoSTAFF2 = PerthSTAFFCollection.FirstOrDefault(x => x.NAME == exoGuessUserName2);
                if (exoSTAFF2 != null)
                {
                    return exoSTAFF2.STAFFNO;
                }
            }

            return null;
        }

        public void MatchExoStaffId()
        {
            if(DisplaySelectedEntities.Count == 0)
            {
                MessageBoxService.ShowMessage("Please select user(s) to update", "Error", MessageButton.OK, MessageIcon.Information);
                return;
            }

            bool showErrorMessage = false;
            List<USER> userToSave = new List<USER>();
            foreach(USER entity in DisplaySelectedEntities)
            {
                if (entity.GUID_OFFICE == null)
                {
                    showErrorMessage = true;
                    continue;
                }

                int? exoPerthId = getExoStaffId(entity, PerthSTAFFCollection);
                if(exoPerthId != null)
                {
                    entity.EXO_STAFF_ID = exoPerthId;
                    userToSave.Add(entity);
                }

                int? exoMontrealId = getExoStaffId(entity, MontrealSTAFFCollection);
                if (exoMontrealId != null)
                {
                    entity.EXO_STAFF_ID_REMOTE = exoMontrealId;
                    userToSave.Add(entity);
                }
            }

            if (showErrorMessage)
            {
                MessageBoxService.ShowMessage("Cannot assign Exo user because office isn't populated, please populate office then try again", "Error", MessageButton.OK, MessageIcon.Information);
            }

            MainViewModel.BulkSave(userToSave);
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "USERCollectionViewModelWrapper"; }
        }

        List<STAFF> perthStaffCollection;
        public IEnumerable<STAFF> PerthSTAFFCollection
        {
            get
            {
                if (pgaUnitOfWork == null)
                    return null;

                if (perthStaffCollection == null)
                {
                    perthStaffCollection = new List<STAFF>(primeroUnitOfWork.STAFF.Where(x => x.ISACTIVE == "Y").OrderBy(x => x.NAME));
                    perthStaffCollection.ForEach(x => x.Office = BluePrintsResources.OfficePerth);
                }

                return perthStaffCollection;
            }
        }

        List<STAFF> pgaStaffCollection;
        public IEnumerable<STAFF> MontrealSTAFFCollection
        {
            get
            {
                if (pgaUnitOfWork == null)
                    return null;

                if(pgaStaffCollection == null)
                {
                    pgaStaffCollection = new List<STAFF>(pgaUnitOfWork.STAFF.Where(x => x.ISACTIVE == "Y").OrderBy(x => x.NAME));
                    pgaStaffCollection.ForEach(x => x.Office = BluePrintsResources.OfficeMontreal);
                }

                return pgaStaffCollection;
            }
        }

        public IEnumerable<ROLE> ROLECollection
        {
            get
            {
                var collection = GetEntities<ROLE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        public IEnumerable<ROLE> RestrictedROLECollection
        {
            get
            {
                var collection = GetEntities<ROLE>();
                if (collection != null)
                {
                    if (LoginCredentials.IsAdmin)
                        collection = collection.OrderBy(x => x.NAME);
                    else if (LoginCredentials.CurrentUser.GUID_ROLE == null)
                        collection = collection.Where(x => x.GUID == Guid.Empty);
                    else
                        collection = collection.Where(x => x.GUID == LoginCredentials.CurrentUser.GUID_ROLE || ChildrenRoles((Guid)LoginCredentials.CurrentUser.GUID_ROLE).Contains((Guid)x.GUID)).OrderBy(x => x.NAME);
                }

                return collection;
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

        public IEnumerable<PROJECT> PROJECTCollection
        {
            get
            {
                var collection = GetEntities<PROJECT>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NUMBER);
                return collection;
            }
        }

        private List<PROJECT> authorisedPROJECTS;
        public IEnumerable<PROJECT> AuthorisedPROJECTCollection
        {
            get
            {
                if(authorisedPROJECTS == null)
                {
                    authorisedPROJECTS = new List<PROJECT>();
                    if (LoginCredentials.CurrentUser.PROJECT_PERMISSION.Count == 0)
                        authorisedPROJECTS = PROJECTCollection.ToList();
                    else
                        //need to pick from current project list so that hashcode is the same and project display property can be shown
                        authorisedPROJECTS = PROJECTCollection.Where(project => LoginCredentials.CurrentUser.PROJECT_PERMISSION.Any(x => x.GUID_PROJECT == project.GUID)).ToList();
                }

                return authorisedPROJECTS;
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

        public IEnumerable<PROJECT_PERMISSION> PROJECT_PERMISSIONCollection
        {
            get
            {
                return GetEntities<PROJECT_PERMISSION>();
            }
        }

        public CollectionViewModel<PROJECT_PERMISSION, PROJECT_PERMISSION, Guid, IBluePrintsEntitiesUnitOfWork> PROJECT_PERMISSIONCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return
                    (CollectionViewModel<PROJECT_PERMISSION, PROJECT_PERMISSION, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<PROJECT_PERMISSION>();
            }
        }

        public IEnumerable<OFFICE> OFFICECollection
        {
            get
            {
                var collection = GetEntities<OFFICE>();
                if (collection != null)
                {
                    collection = collection.OrderBy(x => x.NAME);
                }

                return collection;
            }
        }
        #endregion

        #region View Commands
        private IDialogService USERImportDialogService
        {
            get { return this.GetRequiredService<IDialogService>("USERImportDialogService"); }
        }

        public bool IsImpersonateVisible => LoginCredentials.IsAdmin;

        public bool CanImpersonate()
        {
            return IsImpersonateVisible;
        }

        public void Impersonate()
        {
            LoginCredentials.CurrentUser = DisplaySelectedEntity;
            MessageBoxService.ShowMessage("Context user account has been changed to " + LoginCredentials.CurrentUser.NAME);
        }

        public void Update_User()
        {
            if (DisplaySelectedEntities.Count == 0)
            {
                MessageBoxService.ShowMessage("Please select user(s) to update", "Error", MessageButton.OK, MessageIcon.Information);
                return;
            }

            IEnumerable<USER> activeDirectoryUSERS = ActiveDirectory.GetUSERS();
            List<USER> update_users = new List<USER>();
            foreach(USER user in DisplaySelectedEntities)
            {
                USER active_directory_user = activeDirectoryUSERS.FirstOrDefault(x => x.NAME == user.NAME);
                if(active_directory_user != null)
                {
                    user.FIRST_NAME = active_directory_user.FIRST_NAME;
                    user.LAST_NAME = active_directory_user.LAST_NAME;
                    user.DESCRIPTION = active_directory_user.DESCRIPTION;
                    user.DEPARTMENT = active_directory_user.DEPARTMENT;
                    DEPARTMENT department = DEPARTMENTCollection.FirstOrDefault(x => x.NAME.ToUpper() == user.DEPARTMENT.ToUpper());
                    if (department != null)
                        user.GUID_DEPARTMENT = department.GUID;
                    user.TITLE = active_directory_user.TITLE;
                    update_users.Add(user);
                }
            }

            MainViewModel.BulkSave(update_users);
        }

        public void Import()
        {
            var selectEntitiesViewModel = USERSelectionViewModel.Create(MainViewModel.Entities);
            if (USERImportDialogService.ShowDialog(MessageButton.OKCancel, "Select Users to Import", "USERSelectionView", selectEntitiesViewModel) == MessageResult.OK)
            {
                List<USER> add_new_users = new List<USER>();
                foreach(USER selected_entity in selectEntitiesViewModel.SelectedEntities)
                {
                    USER new_user = new USER();
                    new_user.TITLE = selected_entity.TITLE;
                    new_user.DESCRIPTION = selected_entity.DESCRIPTION;
                    new_user.FIRST_NAME = selected_entity.FIRST_NAME;
                    new_user.LAST_NAME = selected_entity.LAST_NAME;
                    new_user.NAME = selected_entity.NAME;
                    new_user.CREATED = DateTime.Now;
                    new_user.DEPARTMENT = selected_entity.DEPARTMENT;
                    DEPARTMENT department = DEPARTMENTCollection.FirstOrDefault(x => x.NAME.ToUpper() == selected_entity.DEPARTMENT.ToUpper());
                    if (department != null)
                        new_user.GUID_DEPARTMENT = department.GUID;

                    add_new_users.Add(new_user);
                }

                MainViewModel.BulkSave(add_new_users);
            }

            selectEntitiesViewModel = null;
        }

        public override string UnifiedRowValidation(USER projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(USER projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }

        bool hideLeaved;
        public bool HideLeave
        {
            get
            {
                return hideLeaved;
            }
            set
            {
                hideLeaved = value;
                if (GridControlService != null)
                {
                    string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
                    string filterString = "[LEAVE_DATE] IS NULL OR [LEAVE_DATE] >= #" + currentDate + "#";
                    if (value)
                    {
                        CriteriaOperator criteriaOperator = GridControlService.FilterCriteria;
                        CriteriaOperator newCriteriaOperator;
                        if (!ReferenceEquals(criteriaOperator, null))
                        {
                            string filterCriteria = criteriaOperator.ToString() + " AND " + filterString;
                            newCriteriaOperator = CriteriaOperator.Parse(filterCriteria);
                        }
                        else
                        {
                            newCriteriaOperator = CriteriaOperator.Parse(filterString);
                        }

                        GridControlService.FilterCriteria = newCriteriaOperator;
                    }
                    else
                    {
                        CriteriaOperator criteriaOperator = GridControlService.FilterCriteria;
                        if (!ReferenceEquals(criteriaOperator, null))
                        {
                            CriteriaOperator newCriteriaOperator;
                            string currentFilterCriteria = criteriaOperator.ToString();
                            string newfilterCriteria = currentFilterCriteria.ToUpper().Replace("AND " + filterString, "");
                            newfilterCriteria = newfilterCriteria.Replace(filterString, "");
                            if (newfilterCriteria.Length >= 5)
                            {
                                string firstFiveChar = newfilterCriteria.Substring(0, 5);
                                if (firstFiveChar.ToUpper().Contains("AND"))
                                    newfilterCriteria = newfilterCriteria.Substring(5, newfilterCriteria.Length - 5);
                            }


                            newCriteriaOperator = CriteriaOperator.Parse(newfilterCriteria);
                            GridControlService.FilterCriteria = newCriteriaOperator;
                        }
                    }
                }
            }
        }
        #endregion
    }
}