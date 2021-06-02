using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Dialogs;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single BASELINE object view model.
    /// </summary>
    public partial class EXO_ResourcesCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <JOBCOST_RESOURCE, ExoResourceProjection, int, IPrimeroEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of BASELINE_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static EXO_ResourcesCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new EXO_ResourcesCollectionViewModelWrapper(unitOfWorkFactory));
        }
        
        /// <summary>
        /// Initializes a new instance of the BASELINEViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the BASELINEViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected EXO_ResourcesCollectionViewModelWrapper(
            IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
#if MONTREAL
        private readonly IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(true);
#else
        private readonly IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
#endif

        private readonly IPrimeroEntitiesUnitOfWork primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
        private readonly IPrimeroEntitiesUnitOfWork pgaUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(true).CreateUnitOfWork();
        private readonly IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        List<USER> activeDirectoryUSERS;
        protected override void resolveParameters(object parameter)
        {
            AlwaysSkipMessage = true;
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<GLACCS, GLACCS, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.GLACCS);
            loaderCollection.AddLoaderDescription<JOB_COSTTYPES, JOB_COSTTYPES, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOB_COSTTYPES);
            loaderCollection.AddLoaderDescription<JOB_COSTGROUPS, JOB_COSTGROUPS, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOB_COSTGROUPS);
            loaderCollection.AddLoaderDescription<PROFILE, PROFILE, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.PROFILE);
            loaderCollection.AddLoaderDescription<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.USERS);
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription<ROLE, ROLE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.ROLES);
            loaderCollection.AddLoaderDescription<X_DEPARTMENT, X_DEPARTMENT, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.X_DEPARTMENTS);
            loaderCollection.AddLoaderDescription<OFFICE, OFFICE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.OFFICES);
        }
#endregion

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(primeroUnitOfWorkFactory, x => x.JOBCOST_RESOURCE);
        }

        protected override Func<IRepositoryQuery<JOBCOST_RESOURCE>, IQueryable<ExoResourceProjection>> specifyMainViewModelProjection()
        {
            return query => ExoQueries.GetResources(primeroUnitOfWork, USERCollection);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<ExoResourceProjection> entities)
        {
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        protected override OperationInterceptMode OnBeforeProjectionSaveIsContinue(ExoResourceProjection projection, out bool isNew)
        {
            isNew = false;
            IEnumerable<ExoResourceProjection> addedResources = commitToExo(projection);
            if (addedResources.Count() > 0)
                isNew = true;

            return OperationInterceptMode.SkipOneAndAllDbSaves;
        }

        protected override OperationInterceptMode OnBeforeProjectionDeleteIsContinue(ExoResourceProjection projection, out List<ErrorMessage> errorMessages)
        {
            errorMessages = new List<ErrorMessage>();
            deleteResource(projection);
            return OperationInterceptMode.SkipOneAndAllDbSaves;
        }

        protected bool onBeforeEntitySaved(ExoResourceProjection projection)
        {
            commitToExo(projection);
            return false;
        }

        protected OperationInterceptMode onBeforeEntityDeleted(ExoResourceProjection projection)
        {
            return OperationInterceptMode.SkipOneAndAllDbSaves;
        }

        private IEnumerable<ExoResourceProjection> commitToExo(ExoResourceProjection projection)
        {
            List<ExoResourceProjection> newlyAddedResources = new List<ExoResourceProjection>();
            ExoResourceProjection remoteProjection = new ExoResourceProjection();
            DataUtils.ShallowCopy(remoteProjection, projection);

            string upperCaseName = projection.RESOURCENAME.ToUpper();

            string partialShortCode;
            //use new unit of work to prevent concurrency issues
            List<int> availablePrimeroEnumerations = ExoQueries.GetAvailableStaffEnumerations(primeroUnitOfWork, upperCaseName, out partialShortCode);

            //use new unit of work to prevent concurrency issues
            List<int> availablePgaEnumerations = ExoQueries.GetAvailableStaffEnumerations(pgaUnitOfWork, upperCaseName, out partialShortCode);

            int commonAvailableNameCount = -1;
            foreach(int primeroEnumeration in availablePrimeroEnumerations)
            {
                if(availablePgaEnumerations.Any(x => x == primeroEnumeration))
                {
                    commonAvailableNameCount = primeroEnumeration;
                    break;
                }
            }

            string newResourceShortCode = commonAvailableNameCount == -1 ? "N/A" : string.Concat(partialShortCode, commonAvailableNameCount.ToString());
            string primaryDbStaffName;

            string newItemSearchName = projection.STAFFNO == null ? projection.RESOURCENAME.ToUpper() : string.Empty;
            bool isNew = commitToExo(projection, primeroUnitOfWork, newResourceShortCode, newItemSearchName, out primaryDbStaffName);
            string secondaryDbStaffName;
            commitToExo(remoteProjection, pgaUnitOfWork, newResourceShortCode, primaryDbStaffName, out secondaryDbStaffName);

            //need to add post to capture generated id and properties
            //forceNewEntry is to accomodate row added from newitemrow, because it is automatically added into display entities hence the need to overridden
            if (isNew)
            {
                newlyAddedResources.Add(projection);
            }

            return newlyAddedResources;
        }

        private bool commitToExo(ExoResourceProjection resource, IPrimeroEntitiesUnitOfWork primeroUOW, string newResourceShortCode, string forceSearchName, out string primaryDbStaffName)
        {
            bool isNew;
            STAFF addedStaff = ExoMethods.FindExistingOrAddStaff(primeroUOW, resource.STAFFNO, resource.RESOURCENAME, resource.TITLE, resource.SECURITYPROFILEID, resource.USERPROFILEID, resource.REPORTS_TO_STAFFNO, resource.PAYROLL_ID, forceSearchName, out primaryDbStaffName, out isNew);

            resource.STAFFNO = addedStaff.STAFFNO;
            //map back generated properties to projection
            //do not map back because multiple contexts are involved
            resource.REPORTS_TO_STAFFNO = addedStaff.REPORTS_TO_STAFFNO;
            string activeShortCode = isNew ? newResourceShortCode : resource.SHORTCODE;
            JOBCOST_RESOURCE addedResource = ExoMethods.FindExistingOrAddResource(primeroUOW, resource.STAFFNO, resource.RESOURCE_SEQNO, resource.RESOURCENAME, resource.TITLE, resource.DEFAULT_STOCKCODE, activeShortCode, forceSearchName);

            //map back generated properties to projection
            resource.DEFAULT_STOCKCODE = addedResource.DEFAULT_STOCKCODE;
            resource.SHORTCODE = addedResource.SHORTCODE;
            resource.RESOURCE_SEQNO = addedResource.SEQNO;

            STOCK_ITEMS stockItem = ExoMethods.FindExistingOrAddStockItem(primeroUOW, resource.SHORTCODE, resource.RESOURCENAME, resource.SELLPRICE1, resource.SALES_GL_CODE, resource.PURCH_GL_CODE, resource.COS_GL_CODE, resource.STDCOST, resource.COSTGROUP, resource.COSTTYPE, resource.DEPARTMENT);
            primeroUOW.SaveChanges();
            resource.IsViewNewRow = false;
            resource.Update();

            return isNew;
        }

        private void deleteResource(ExoResourceProjection projection)
        {
            ExoResourceProjection remoteProjection = new ExoResourceProjection();
            DataUtils.ShallowCopy(remoteProjection, projection);

            string primaryDbName;
            deleteResources(projection, primeroUnitOfWork, string.Empty, out primaryDbName);
            string remoteDbName;
            deleteResources(remoteProjection, pgaUnitOfWork, primaryDbName, out remoteDbName, true);
        }

        private void deleteResources(ExoResourceProjection projection, IPrimeroEntitiesUnitOfWork primeroUOW, string forceSearchName, out string primaryDbName, bool isRemoteOperation = false)
        {
            ExoMethods.RemoveStaff(primeroUOW, projection, forceSearchName, out primaryDbName);
            ExoMethods.RemoveResources(primeroUOW, projection, forceSearchName);
            ExoMethods.RemoveStockItem(primeroUOW, projection);
            primeroUOW.SaveChanges();
        }

        private IDialogService USERAddDialogService
        {
            get { return this.GetRequiredService<IDialogService>("USERAddDialogService"); }
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get
            {
                //return "BASELINE_ITEMSViewModelWrapper" + view_project_specific_affix;
                return "EXO_ResourcesCollectionViewModelWrapper_v2";
            }
        }

        public override string UnifiedRowValidation(ExoResourceProjection projection)
        {
            if (projection.RESOURCENAME == null || projection.RESOURCENAME == string.Empty)
                return "Name is required";

            return string.Empty;
        }

        public override void UnifiedCellValueChanged(string field_name, object old_value, object new_value, ExoResourceProjection projection, bool isNew)
        {
            if (field_name == BindableBase.GetPropertyName(() => new ExoResourceProjection().IsExistInBP))
            {
                if((bool)new_value)
                {
                    USER newUSER = USERCollection.FirstOrDefault(x => x.EXO_STAFF_ID == projection.STAFFNO);
                    if (newUSER == null && userAdditionViewModel != null)
                    {
                        newUSER = userAdditionViewModel.GetNewUser();
                        USERViewModel.Save(newUSER);
                        userAdditionViewModel = null;
                    }
                }
            }

            base.UnifiedCellValueChanged(field_name, old_value, new_value, projection, isNew);
        }

        USERAdditionViewModel userAdditionViewModel;
        public override string UnifiedValueValidation(ExoResourceProjection projection, string field_name, object new_value, bool isPaste)
        {
            if (field_name == BindableBase.GetPropertyName(() => new ExoResourceProjection().RESOURCENAME))
            {
                if (new_value == null || new_value.ToString() == string.Empty)
                    return "Name is required";
            }

            if (!projection.IsViewNewRow && (IsChangingValueFromBackgroundEvents && !MainViewModel.EntitiesUndoRedoManager.IsInUndoRedoOperation || CellValueChangingFieldName != null))
            {
                string validateFieldName = field_name;
                if (!IsChangingValueFromBackgroundEvents && CellValueChangingFieldName != null && CellValueChangingFieldName == validateFieldName)
                {
                    if (validateFieldName == BindableBase.GetPropertyName(() => new ExoResourceProjection().SHORTCODE) || validateFieldName == BindableBase.GetPropertyName(() => new ExoResourceProjection().DEFAULT_STOCKCODE))
                    {
                        if (MessageBoxService.ShowMessage("Are you sure you want to change " + field_name + " for " + projection.RESOURCENAME + "?", "Warning", MessageButton.OKCancel, MessageIcon.Warning) == MessageResult.Cancel)
                            return "Operation cancelled";
                    }
                    else if (validateFieldName == BindableBase.GetPropertyName(() => new ExoResourceProjection().IsExistInBP))
                    {
                        if (((bool)new_value))
                        {
                            USER newUSER = USERCollection.FirstOrDefault(x => x.EXO_STAFF_ID == projection.STAFFNO);
                            if (newUSER == null)
                            {
                                USER activeDirectoryUSER = getActiveDirectoryUser(projection.RESOURCENAME);
                                if (activeDirectoryUSER == null)
                                    return "Cannot add user because user doesn't exist in active directory";
                                else
                                {
                                    if (MessageBoxService.ShowMessage("Are you sure you add " + projection.RESOURCENAME + " to BluePrints?", "Confirmation", MessageButton.OKCancel, MessageIcon.Warning) == MessageResult.OK)
                                    {
                                        userAdditionViewModel = USERAdditionViewModel.Create(activeDirectoryUSER, DEPARTMENTCollection, DISCIPLINECollection, USERCollection, OFFICECollection, activeDirectoryUSER.TITLE, activeDirectoryUSER.DESCRIPTION, primeroUnitOfWork.STAFF, pgaUnitOfWork.STAFF);
                                        userAdditionViewModel.PopulateUSERStaffId();
                                        if (USERAddDialogService.ShowDialog(MessageButton.OKCancel, "New User", "USERAdditionView", userAdditionViewModel) == MessageResult.Cancel)
                                        {
                                            userAdditionViewModel = null;
                                            return "Operation cancelled";
                                        }
                                    }
                                    else
                                        return "Operation cancelled";
                                }
                            }
                        }
                        else
                        {
                            MessageBoxService.ShowMessage("Please remove user from User maintenance in BluePrints", "Confirmation", MessageButton.OK, MessageIcon.Warning);
                            return "Operation cancelled";
                        }
                    }
                }
            }

            return string.Empty;
        }

        private USER getActiveDirectoryUser(string resourceName)
        {
            if (activeDirectoryUSERS == null)
                activeDirectoryUSERS = EmailServices.GetUSERS();

            string activeDirectorySearchName = resourceName.Replace(" ", ".").ToUpper();
            return activeDirectoryUSERS.FirstOrDefault(x => x.NAME.ToUpper() == activeDirectorySearchName);
        }

        public IEnumerable<PROFILE> SecurityPROFILECollection
        {
            get
            {
                var collection = GetEntities<PROFILE>();
                if (collection != null)
                    collection = collection.Where(x => x.PROFILETYPE == 4).OrderBy(x => x.PROFILENAME);
                return collection;
            }
        }

        public IEnumerable<PROFILE> UserPROFILECollection
        {
            get
            {
                var collection = GetEntities<PROFILE>();
                if (collection != null)
                    collection = collection.Where(x => x.PROFILETYPE == 2).OrderBy(x => x.PROFILENAME);
                return collection;
            }
        }

        public IEnumerable<GLACCS> GLACCSCollection
        {
            get
            {
                var collection = GetEntities<GLACCS>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        public IEnumerable<JOB_COSTGROUPS> JOB_COSTGROUPSCollection
        {
            get
            {
                var collection = GetEntities<JOB_COSTGROUPS>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.COSTDESC);
                return collection;
            }
        }

        public IEnumerable<DEPARTMENT> DEPARTMENTCollection
        {
            get
            {
                return GetEntities<DEPARTMENT>();
            }
        }

        public IEnumerable<DISCIPLINE> DISCIPLINECollection
        {
            get
            {
                return GetEntities<DISCIPLINE>();
            }
        }

        public IEnumerable<USER> USERCollection
        {
            get
            {
                return GetEntities<USER>();
            }
        }

        public IEnumerable<OFFICE> OFFICECollection
        {
            get
            {
                return GetEntities<OFFICE>();
            }
        }

        public CollectionViewModel<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork> USERViewModel
        {
            get
            {
                if (loaderCollection == null)
                    return null;

                return
                    (CollectionViewModel<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<USER>();
            }
        }

        public IEnumerable<JOB_COSTTYPES> JOB_COSTTYPESCollection
        {
            get
            {
                var collection = GetEntities<JOB_COSTTYPES>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.COSTDESC);
                return collection;
            }
        }


        List<ROLE> restrictedROLECollection;
        public IEnumerable<ROLE> RestrictedROLECollection
        {
            get
            {
                var collection = GetEntities<ROLE>();
                if (restrictedROLECollection == null)
                    restrictedROLECollection = USERCollectionViewModelWrapper.GetRestrictedRoleCollection(collection);

                return restrictedROLECollection;
            }
        }
        
        public IEnumerable<X_DEPARTMENT> X_DEPARTMENTCollection
        {
            get
            {
                var collection = GetEntities<X_DEPARTMENT>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.X_Number);

                return collection;
            }
        }
    }
}