using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
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

        protected override void resolveParameters(object parameter)
        {
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<GLACCS, GLACCS, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.GLACCS);
            loaderCollection.AddLoaderDescription<JOB_COSTTYPES, JOB_COSTTYPES, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOB_COSTTYPES);
            loaderCollection.AddLoaderDescription<JOB_COSTGROUPS, JOB_COSTGROUPS, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOB_COSTGROUPS);
            loaderCollection.AddLoaderDescription<PROFILE, PROFILE, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.PROFILE);
        }
#endregion

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(primeroUnitOfWorkFactory, x => x.JOBCOST_RESOURCE);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<JOBCOST_RESOURCE>, IQueryable<ExoResourceProjection>> specifyMainViewModelProjection()
        {
            return query => ExoQueries.GetResources(primeroUnitOfWork);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<ExoResourceProjection> entities)
        {
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = onBeforeEntitySaved;
            MainViewModel.OnBeforeEntityDeletedIsContinueCallBack = onBeforeEntityDeleted;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            base.OnAfterAssignedCallbackAndRaisePropertyChanged();
        }

        protected override void OnSelectedEntitiesChanged()
        {
        }

        protected bool onBeforeEntitySaved(ExoResourceProjection projection)
        {
            commitToExo(projection);
            return false;
        }

        protected DeleteInterceptMode onBeforeEntityDeleted(ExoResourceProjection projection)
        {
            delete(projection);
            return DeleteInterceptMode.Skip;
        }

        /// <summary>
        /// Since CollectionViewModelBase is a POCO view model, an the instance of this class will also expose the AddUndoCommand property that can be used as a binding source in views.
        /// </summary>
        public virtual void CommitNewRow(RowEventArgs e)
        {
            if (e.RowHandle == DataControlBase.NewItemRowHandle)
            {
                ExoResourceProjection exoResource = (ExoResourceProjection)e.Row;
                commitToExo(exoResource, true);
            }
        }

        private void commitToExo(ExoResourceProjection projection, bool addedFromView = false)
        {
            ExoResourceProjection remoteProjection = new ExoResourceProjection();
            DataUtils.ShallowCopy(remoteProjection, projection);

            string upperCaseName = projection.RESOURCENAME.ToUpper();
            int primeroNameCount;
            //use new unit of work to prevent concurrency issues
            string generatedShortCodeFromPrimero = ExoQueries.GetStaffShortcode(primeroUnitOfWork, upperCaseName, out primeroNameCount);

            int pgaNameCount;
            //use new unit of work to prevent concurrency issues
            string generatedShortCodeFromPga = ExoQueries.GetStaffShortcode(pgaUnitOfWork, upperCaseName, out pgaNameCount);


            string newResourceShortCode = primeroNameCount > pgaNameCount ? generatedShortCodeFromPrimero : generatedShortCodeFromPga;
            string primaryDbStaffName;

            string newItemSearchName = projection.STAFFNO == null ? projection.RESOURCENAME.ToUpper() : string.Empty;
            commitToExo(projection, primeroUnitOfWork, newResourceShortCode, newItemSearchName, out primaryDbStaffName);
            string secondaryDbStaffName;
            commitToExo(remoteProjection, pgaUnitOfWork, newResourceShortCode, primaryDbStaffName, out secondaryDbStaffName);

            //need to add post to capture generated id and properties
            //forceNewEntry is to accomodate row added from newitemrow, because it is automatically added into display entities hence the need to overridden
            if (addedFromView || !DisplayEntities.Any(x => x.STAFFNO == projection.STAFFNO))
            {
                if(!addedFromView)
                {
                    DisplayEntities.Add(projection);
                }

                List<ExoResourceProjection> newlyAddedResources = new List<ExoResourceProjection>();
                newlyAddedResources.Add(projection);
                OnAfterNewRowAdded(newlyAddedResources);
                if (!MainViewModel.EntitiesUndoRedoManager.IsInUndoRedoOperation())
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, null, null, null, EntityMessageType.Added);
            }
        }

        private void commitToExo(ExoResourceProjection resource, IPrimeroEntitiesUnitOfWork primeroUOW, string newResourceShortCode, string forceSearchName, out string primaryDbStaffName)
        {
            bool isNew;
            STAFF addedStaff = ExoMethods.FindExistingOrAddStaff(primeroUOW, resource.STAFFNO, resource.RESOURCENAME, resource.TITLE, resource.SECURITYPROFILEID, resource.USERPROFILEID, resource.REPORTS_TO_STAFFNO, resource.PAYROLL_ID, forceSearchName, out primaryDbStaffName, out isNew);

            resource.STAFFNO = addedStaff.STAFFNO;
            //map back generated properties to projection
            //do not map back because multiple contexts are involved
            //resource.STAFFNO = addedStaff.STAFFNO;
            resource.REPORTS_TO_STAFFNO = addedStaff.REPORTS_TO_STAFFNO;
            string activeShortCode = isNew ? newResourceShortCode : resource.SHORTCODE;
            JOBCOST_RESOURCE addedResource = ExoMethods.FindExistingOrAddResource(primeroUOW, resource.STAFFNO, resource.RESOURCE_SEQNO, resource.RESOURCENAME, resource.TITLE, resource.DEFAULT_STOCKCODE, activeShortCode, forceSearchName);

            //map back generated properties to projection
            resource.DEFAULT_STOCKCODE = addedResource.DEFAULT_STOCKCODE;
            resource.SHORTCODE = addedResource.SHORTCODE;
            resource.RESOURCE_SEQNO = addedResource.SEQNO;

            STOCK_ITEMS stockItem = ExoMethods.FindExistingOrAddStockItem(primeroUOW, resource.SHORTCODE, resource.RESOURCENAME, resource.SELLPRICE1, resource.SALES_GL_CODE, resource.PURCH_GL_CODE, resource.COS_GL_CODE, resource.STDCOST, resource.COSTGROUP, resource.COSTTYPE);

            resource.IsNewRow = false;
            resource.Update();

            primeroUOW.SaveChanges();
        }

        public void DeleteSelected()
        {
            if (MessageBoxService.ShowMessage("Are you sure you want to remove selected resource(s)\n\nThis will mark resource as inactive in EXO", "Warning", MessageButton.OKCancel, MessageIcon.Warning) == MessageResult.Cancel)
                return;

            foreach(ExoResourceProjection projection in DisplaySelectedEntities)
            {
                delete(projection);
            }
        }

        private void delete(ExoResourceProjection projection)
        {
            ExoResourceProjection remoteProjection = new ExoResourceProjection();
            DataUtils.ShallowCopy(remoteProjection, projection);

            string primaryDbName;
            delete(projection, primeroUnitOfWork, string.Empty, out primaryDbName);
            string remoteDbName;
            delete(remoteProjection, pgaUnitOfWork, primaryDbName, out remoteDbName, true);
        }

        private void delete(ExoResourceProjection projection, IPrimeroEntitiesUnitOfWork primeroUOW, string forceSearchName, out string primaryDbName, bool isRemoteOperation = false)
        {
            ExoMethods.RemoveStaff(primeroUOW, projection, forceSearchName, out primaryDbName);
            ExoMethods.RemoveResources(primeroUOW, projection, forceSearchName);
            ExoMethods.RemoveStockItem(primeroUOW, projection);
            primeroUOW.SaveChanges();

            if(!isRemoteOperation)
            {
                DisplayEntities.Remove(projection);
                if (!MainViewModel.EntitiesUndoRedoManager.IsInUndoRedoOperation())
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, null, null, null, EntityMessageType.Deleted);
            }
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get
            {
                //return "BASELINE_ITEMSViewModelWrapper" + view_project_specific_affix;
                return "EXO_ResourcesCollectionViewModelWrapper";
            }
        }

        public override string UnifiedRowValidation(ExoResourceProjection projection)
        {
            if (projection.RESOURCENAME == null || projection.RESOURCENAME == string.Empty)
                return "Name is required";

            return string.Empty;
        }

        public override string UnifiedValueValidation(ExoResourceProjection projection, string field_name, object new_value, bool isPaste)
        {
            if (field_name == BindableBase.GetPropertyName(() => new ExoResourceProjection().RESOURCENAME))
            {
                if (new_value == null || new_value.ToString() == string.Empty)
                    return "Name is required";
            }

            if (!projection.IsNewRow)
            {
                if(field_name == BindableBase.GetPropertyName(() => new ExoResourceProjection().SHORTCODE) || field_name == BindableBase.GetPropertyName(() => new ExoResourceProjection().DEFAULT_STOCKCODE))
                {
                    if (MessageBoxService.ShowMessage("Are you sure you change " + field_name + " for " + projection.RESOURCENAME + "?", "Warning", MessageButton.OKCancel, MessageIcon.Warning) == MessageResult.Cancel)
                        return "User cancel";
                }
            }

            return string.Empty;
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
    }
}