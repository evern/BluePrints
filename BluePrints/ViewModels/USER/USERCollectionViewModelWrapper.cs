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
using BaseModel.ViewModel.Dialogs;
using BaseModel.Misc;
using System.Threading.Tasks;
using System.Collections.Concurrent;

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
        private IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> pgaUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(BluePrintsResources.OfficeMontreal);
        IPrimeroEntitiesUnitOfWork primeroUnitOfWork;
        IPrimeroEntitiesUnitOfWork pgaUnitOfWork;
        IBluePrintsEntitiesUnitOfWork bluePrintsUnitOfWork;
        //timer to scan serial port
        protected override void resolveParameters(object parameter)
        {
            bluePrintsUnitOfWork = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
            primeroUnitOfWork = primeroUnitOfWorkFactory.CreateUnitOfWork();
            pgaUnitOfWork = pgaUnitOfWorkFactory.CreateUnitOfWork();
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<ROLE, ROLE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.ROLES);
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription<OFFICE, OFFICE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.OFFICES);
            loaderCollection.AddLoaderDescription<PROJECT_PERMISSION, PROJECT_PERMISSION, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.PROJECT_PERMISSIONS);
            loaderCollection.AddLoaderDescription<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.PROJECTS);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.USERS);
        }

        protected override Func<IRepositoryQuery<USER>, IQueryable<USER>> specifyMainViewModelProjection()
        {
            if (LoginCredentials.IsAdmin)
                return query => USERCollectionPopulation(query.OrderBy(x => x.NAME));
            else if (LoginCredentials.CurrentUser.GUID_ROLE == null)
                return query => query.Where(x => x.GUID == Guid.Empty);
            else
                return query => USERCollectionPopulation(query.OrderBy(x => x.NAME));
            //allow only authorised role per role hierarchy to be queried
            //else
            //    return query => query.ToArray().Where(x => x.GUID_ROLE == null || x.GUID_ROLE == LoginCredentials.CurrentUser.GUID_ROLE || ChildrenRoles((Guid)LoginCredentials.CurrentUser.GUID_ROLE).Contains((Guid)x.GUID_ROLE)).AsQueryable();
        }

        public IQueryable<USER> USERCollectionPopulation(IQueryable<USER> USERS)
        {
            //DbContext cannot support parallel operation, load it up first in cache
            //PROJECT_PERMISSIONCollection.ToList();
            List<USER> users;

            //use isFirstLoaded to avoid cross thread operation when updating
            if(isFirstLoaded)
            {
                foreach(USER user in USERS)
                {
                    populateUserProperties(user, OFFICECollection);
                    populateUserAuthorisedProjects(user, PROJECTCollection);
                }

                users = new List<USER>(USERS);
            }
            else
            {
                ConcurrentBag<USER> loopUSERs = new ConcurrentBag<USER>(USERS);
                Parallel.ForEach(loopUSERs, user =>
                {
                    populateUserProperties(user, OFFICECollection);
                    populateUserAuthorisedProjects(user, PROJECTCollection);
                });

                users = new List<USER>(loopUSERs);
            }

            return users.AsQueryable();
        }

        private void populateUserProperties(USER user, IEnumerable<OFFICE> OFFICECollection)
        {
            if (user.GUID_OFFICE != null)
            {
                OFFICE office = OFFICECollection.FirstOrDefault(x => x.GUID == user.GUID_OFFICE);
                user.QueryOfficeName = office.NAME;
            }
            user.Update();
        }

        private void populateUserAuthorisedProjects(USER user, IEnumerable<PROJECT> PROJECTCollection)
        {
            List<PROJECT_PERMISSION> userPROJECT_PERMISSION = PROJECT_PERMISSIONCollection.Where(x => x.GUID_USER == user.GUID).ToList();
            user.Projects = PROJECTCollection.Where(project => userPROJECT_PERMISSION.Any(permission => permission.GUID_PROJECT == project.GUID)).ToList();
            user.Update();
        }

        public static IEnumerable<Guid> ChildrenRoles(Guid roleGuid, IEnumerable<ROLE> ROLECollection)
        {
            foreach (var role in ROLECollection)
                if (role.PARENTGUID == roleGuid)
                {
                    yield return role.GUID;

                    foreach (var entityChild in ChildrenRoles(role.GUID, ROLECollection))
                        yield return entityChild;
                }
        }

        public override void UnifiedCellValueChanged(string field_name, object old_value, object new_value, USER projection, bool isNew)
        {
            if(field_name == BindableBase.GetPropertyName(() => new USER().GUID_OFFICE))
            {
                populateUserProperties(projection, OFFICECollection);
            }
            else if (field_name == BindableBase.GetPropertyName(() => new USER().NAME))
            {
                if (new_value != null)
                    projection.NAME = new_value.ToString().Trim();
            }

            base.UnifiedCellValueChanged(field_name, old_value, new_value, projection, isNew);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<USER> entities)
        {
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            HideLeave = true;
            this.RaisePropertyChanged(x => x.HideLeave);
            base.OnAfterAssignedCallbackAndRaisePropertyChanged();
        }

        protected override void OnAfterProjectionSave(USER projection, USER entity, bool isNew)
        {
            if (isNew && (entity.EXO_STAFF_ID == null || entity.EXO_STAFF_ID_REMOTE == null))
            {
                entity.START_DATE = DateTime.Now;

                if (entity.EXO_STAFF_ID == null)
                    entity.EXO_STAFF_ID = getExoStaffId(entity, PerthSTAFFCollection);

                if (entity.EXO_STAFF_ID_REMOTE == null)
                    entity.EXO_STAFF_ID_REMOTE = getExoStaffId(entity, MontrealSTAFFCollection);
            }

            saveProjectAssignments(entity);
            base.OnAfterProjectionSave(projection, entity, isNew);
        }
        #endregion

        #region Token Saving Behavior
        private void saveProjectAssignments(USER entity)
        {
            List<PROJECT_PERMISSION> addPermissions = new List<PROJECT_PERMISSION>();
            List<PROJECT_PERMISSION> removePermissions = new List<PROJECT_PERMISSION>();
            if (entity.Project_Assignments != null)
            {
                foreach (PROJECT_PERMISSION assignment in PROJECT_PERMISSIONCollection.Where(x => x.GUID_USER == entity.GUID))
                {
                    if (!entity.Project_Assignments.Any(x => x.GUID == assignment.GUID_PROJECT))
                        removePermissions.Add(assignment);
                }

                foreach (PROJECT project in entity.Project_Assignments)
                {
                    if (!PROJECT_PERMISSIONCollection.Where(x => x.GUID_USER == entity.GUID).Any(x => x.GUID_PROJECT == project.GUID))
                        addPermissions.Add(new PROJECT_PERMISSION() { GUID_PROJECT = project.GUID, GUID_USER = entity.GUID });
                }
            }
            else
            {
                foreach (PROJECT_PERMISSION assignment in PROJECT_PERMISSIONCollection.Where(x => x.GUID_USER == entity.GUID))
                {
                    removePermissions.Add(assignment);
                }
            }

            PROJECT_PERMISSIONCollectionViewModel.BaseBulkSave(addPermissions);
            PROJECT_PERMISSIONCollectionViewModel.BaseBulkDelete(removePermissions);
        }
        #endregion

        #region View Properties

        private static int? getExoStaffId(USER bluePrintsUser, IEnumerable<STAFF> officeSpecificStaffCollection)
        {
            string exoGuessUserName = bluePrintsUser.FIRST_NAME.ToUpper() + " " + bluePrintsUser.LAST_NAME.ToUpper();
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
                STAFF exoSTAFF2 = officeSpecificStaffCollection.FirstOrDefault(x => x.NAME == exoGuessUserName2);
                if (exoSTAFF2 != null)
                {
                    return exoSTAFF2.STAFFNO;
                }
            }

            return null;
        }

        public bool CanMatchExoStaffId()
        {
            return !IsLoading;
        }

        public void MatchExoStaffId()
        {
            if(SelectedEntities.Count == 0)
            {
                MessageBoxService.ShowMessage("Please select user(s) to update", "Error", MessageButton.OK, MessageIcon.Information);
                return;
            }

            List<USER> userToSave = new List<USER>();
            foreach(USER entity in SelectedEntities)
            {
                if (PopulateUserStaffIds(entity, PerthSTAFFCollection, MontrealSTAFFCollection))
                    userToSave.Add(entity);
            }

            MainViewModel.BaseBulkSave(userToSave);
        }

        public static bool PopulateUserStaffIds(USER entity, IEnumerable<STAFF> PerthSTAFFCollection, IEnumerable<STAFF> MontrealSTAFFCollection)
        {
            int? exoPerthId = getExoStaffId(entity, PerthSTAFFCollection);
            bool shouldSave = false;
            if (exoPerthId != null)
            {
                entity.EXO_STAFF_ID = exoPerthId;
                shouldSave = true;
            }

            int? exoMontrealId = getExoStaffId(entity, MontrealSTAFFCollection);
            if (exoMontrealId != null)
            {
                entity.EXO_STAFF_ID_REMOTE = exoMontrealId;
                shouldSave = true;
            }

            return shouldSave;
        }

        public bool CanTrimUsers()
        {
            return !IsLoading;
        }

        public void TrimUsers()
        {
            List<ErrorMessage> errorMessages = new List<ErrorMessage>();
            List<ErrorMessage> indeterminedUsersErrorMessage = new List<ErrorMessage>();
            List<USER> bulkEditUsers = new List<USER>();
            foreach(USER entity in Entities.Where(x => x.LEAVE_DATE == null))
            {
                if(entity.OFFICE != null)
                {
                    STAFF STAFF;
                    if (entity.QueryOfficeName.ToUpper() == BluePrintsResources.OfficePerth)
                        STAFF = PerthSTAFFCollection.FirstOrDefault(x => x.STAFFNO == entity.EXO_STAFF_ID);
                    else
                        STAFF = MontrealSTAFFCollection.FirstOrDefault(x => x.STAFFNO == entity.EXO_STAFF_ID_REMOTE);

                    if (STAFF != null)
                    {
                        if (STAFF.ISACTIVE == "N")
                        {
                            bulkEditUsers.Add(entity);
                            errorMessages.Add(new ErrorMessage(entity.NAME, "Is not active in exo"));
                        }
                    }
                    else
                    {
                        indeterminedUsersErrorMessage.Add(new ErrorMessage(entity.NAME, "Cannot be determined because " + entity.QueryOfficeName + " EXO Id is empty"));
                    }
                }
            }

            if(indeterminedUsersErrorMessage.Count > 0)
                ShowErrorMessage("System cannot determine whether these users are inactive", indeterminedUsersErrorMessage);

            if (bulkEditUsers.Count > 0)
            {
                if (ShowErrorMessage("Do you wish to set these users as inactive?", errorMessages))
                {
                    bulkEditUsers.ForEach(x => x.LEAVE_DATE = DateTime.Now);
                    MainViewModel.UnitOfWork.SaveChanges();
                    bulkEditUsers.ForEach(x => x.Update());
                    MessageBoxService.ShowMessage(bulkEditUsers.Count() + " user(s) has been set with a leave date of today");
                }
            }
            else
            {
                MessageBoxService.ShowMessage("There aren't any users to set as inactive");
            }
        }

        public override void FullRefresh()
        {
            if (!CanFullRefresh())
                return;

            GridControlService.ClearFilterCriteria();
            authorisedPROJECTS = null;
            base.FullRefresh();
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "USERCollectionViewModelWrapper_v2"; }
        }

        List<STAFF> perthStaffCollection;
        public IEnumerable<STAFF> PerthSTAFFCollection
        {
            get
            {
                if (primeroUnitOfWork == null)
                    return null;

                if (perthStaffCollection == null)
                {
                    LoadingScreenManager.ShowLoadingScreen(1);
                    LoadingScreenManager.SetMessage("Loading Perth Active Staffs");
                    perthStaffCollection = new List<STAFF>(primeroUnitOfWork.STAFF.Where(x => x.ISACTIVE == "Y").OrderBy(x => x.NAME));
                    LoadingScreenManager.CloseLoadingScreen();
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

                if (pgaStaffCollection == null)
                {
                    LoadingScreenManager.ShowLoadingScreen(1);
                    LoadingScreenManager.SetMessage("Loading Montreal Active Staffs");
                    pgaStaffCollection = new List<STAFF>(pgaUnitOfWork.STAFF.Where(x => x.ISACTIVE == "Y").OrderBy(x => x.NAME));
                    LoadingScreenManager.CloseLoadingScreen();
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

        List<ROLE> restrictedROLECollection;
        public IEnumerable<ROLE> RestrictedROLECollection
        {
            get
            {
                var collection = GetEntities<ROLE>();
                if (restrictedROLECollection == null && !IsLoading)
                    restrictedROLECollection = GetRestrictedRoleCollection(collection);

                return restrictedROLECollection;
            }
        }

        public static List<ROLE> GetRestrictedRoleCollection(IEnumerable<ROLE> ROLECollection)
        {
            List<ROLE> returnRoleCollection;
            if (LoginCredentials.IsAdmin)
                returnRoleCollection = ROLECollection.OrderBy(x => x.NAME).ToList();
            else if (LoginCredentials.CurrentUser.GUID_ROLE == null)
                returnRoleCollection = ROLECollection.Where(x => x.GUID == Guid.Empty).ToList();
            else
                returnRoleCollection = ROLECollection.Where(x => x.GUID == LoginCredentials.CurrentUser.GUID_ROLE || ChildrenRoles((Guid)LoginCredentials.CurrentUser.GUID_ROLE, ROLECollection).Contains((Guid)x.GUID)).OrderBy(x => x.NAME).ToList();

            return returnRoleCollection;
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
                if (IsLoading)
                    return new List<PROJECT>();

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

                return (CollectionViewModel<PROJECT_PERMISSION, PROJECT_PERMISSION, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<PROJECT_PERMISSION>();
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
            return !IsLoading && IsImpersonateVisible;
        }

        public void Impersonate()
        {
            LoginCredentials.CurrentUser = SelectedEntity;
            MessageBoxService.ShowMessage("Context user account has been changed to " + LoginCredentials.CurrentUser.NAME);
        }

        public bool CanUpdate_User()
        {
            return !IsLoading;
        }

        public void Update_User()
        {
            if (SelectedEntities.Count == 0)
            {
                MessageBoxService.ShowMessage("Please select user(s) to update", "Error", MessageButton.OK, MessageIcon.Information);
                return;
            }

            IEnumerable<USER> activeDirectoryUSERS = EmailServices.GetUSERS();
            List<USER> update_users = new List<USER>();
            foreach(USER user in SelectedEntities)
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

            MainViewModel.BaseBulkSave(update_users);
        }

        public bool CanImport()
        {
            return !IsLoading;
        }

        public void Import()
        {
            var selectEntitiesViewModel = USERSelectionViewModel.Create(MainViewModel.Entities);

            List<ErrorMessage> messages = new List<ErrorMessage>();
            if (USERImportDialogService.ShowDialog(MessageButton.OKCancel, "Select Users to Import", "USERSelectionView", selectEntitiesViewModel) == MessageResult.OK)
            {
                List<USER> add_new_users = new List<USER>();
                foreach(USER selected_entity in selectEntitiesViewModel.SelectedEntities)
                {
                    if (MainViewModel.Entities.Any(x => x.NAME == selected_entity.NAME))
                    {
                        messages.Add(new ErrorMessage(selected_entity.NAME, "Already Added"));
                    }
                    else
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
                }

                MainViewModel.BaseBulkSave(add_new_users);
            }

            if(messages.Count > 0)
            {
                DialogCollectionViewModel<ErrorMessage> viewModel = DialogCollectionViewModel<ErrorMessage>.Create(messages, "User(s) has already been added");
                ErrorMessagesDialogService.ShowDialog(MessageButton.OKCancel, string.Empty, "ListErrorMessages", viewModel);
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