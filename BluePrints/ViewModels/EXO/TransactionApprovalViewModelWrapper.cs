using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class TransactionApprovalViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <TRANSACTION_APPROVAL, TRANSACTION_APPROVAL, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of TransactionApprovalViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static TransactionApprovalViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new TransactionApprovalViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the TransactionApprovalViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the TransactionApprovalViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected TransactionApprovalViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory;
        private IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory;
        private IPrimeroEntitiesUnitOfWork primeroUnitOfWork;
        private IBluePrintsEntitiesUnitOfWork bluePrintsUnitOfWork;
        private Data.PROJECT loadPROJECT;
        JOBCOST_HDR loadJOBCOST_HDR;
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<Data.PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
            primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(loadPROJECT.OfficeNameForExo).CreateUnitOfWork();
            primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(primeroUnitOfWork);
            bluePrintsUnitOfWork = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(bluePrintsUnitOfWork);
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.PROJECTS);
            loaderCollection.AddLoaderDescription<JOB_COSTGROUPS, JOB_COSTGROUPS, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOB_COSTGROUPS);
            loaderCollection.AddLoaderDescription<JOB_COSTTYPES, JOB_COSTTYPES, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOB_COSTTYPES);
            loaderCollection.AddLoaderDescription(primeroUnitOfWorkFactory, x => x.JOBCOST_HDR, JOBCOST_HDRProjectionFunc, x => loadJOBCOST_HDR = x);
            loaderCollection.AddLoaderDescription<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.USERS);
        }

        private Func<IRepositoryQuery<JOBCOST_HDR>, IQueryable<JOBCOST_HDR>> JOBCOST_HDRProjectionFunc()
        {
            return query => query.Where(x => x.JOBCODE == loadPROJECT.NUMBER.ToString());
        }

        protected override Func<IRepositoryQuery<TRANSACTION_APPROVAL>, IQueryable<TRANSACTION_APPROVAL>> specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.TRANSACTION_APPROVALS);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<TRANSACTION_APPROVAL> entities)
        {
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }
        #endregion

        #region Saving Behavior
        public override string UnifiedRowValidation(TRANSACTION_APPROVAL projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(TRANSACTION_APPROVAL projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }

        public bool CanApproveTransaction()
        {
            return !IsLoading && SelectedEntities.Count > 0;
        }

        public void ApproveTransaction()
        {
            foreach(TRANSACTION_APPROVAL projection in SelectedEntities)
            {
                if(projection.APPROVEDON == null)
                {
                    JOB_TRANSACTIONS findJOB_TRANSACTION = primeroUnitOfWork.JOB_TRANSACTIONS.FirstOrDefault(x => x.SEQNO == projection.JOB_TRANSACTION_SEQNO);
                    if(findJOB_TRANSACTION != null)
                    {
                        if (projection.NEW_JOBNO != null)
                            findJOB_TRANSACTION.JOBNO = projection.NEW_JOBNO;

                        if (projection.NEW_COST_GROUP_NO != null)
                            findJOB_TRANSACTION.COST_GROUP = projection.NEW_COST_GROUP_NO;

                        if (projection.NEW_COST_TYPE_NO != null)
                            findJOB_TRANSACTION.COST_TYPE = projection.NEW_COST_TYPE_NO;

                        if (projection.NEW_STOCK_CODE != null)
                            findJOB_TRANSACTION.STOCKCODE = projection.NEW_STOCK_CODE;

                        if (projection.NEW_VARIATION_CODE != null)
                            findJOB_TRANSACTION.X_VARIATIONCODE = projection.NEW_VARIATION_CODE;

                        projection.STATUS = TransactionApprovalStatus.Approved;
                        projection.APPROVEDON = DateTime.Now;
                        projection.APPROVEDBY = LoginCredentials.CurrentUserGuid;
                        projection.Update();
                    }
                }
            }

            bluePrintsUnitOfWork.SaveChanges();
            primeroUnitOfWork.SaveChanges();
        }
        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "TransactionApprovalViewModelWrapper_v1"; }
        }

        private List<JOBCOST_HDR> JOBCOST_HDRList;
        public List<JOBCOST_HDR> JOBCOST_HDRCollection
        {
            get
            {
                if (JOBCOST_HDRList == null && primeroUnitOfWork != null && loadJOBCOST_HDR != null)
                    JOBCOST_HDRList = JOBCOST_HDRProjection(primeroUnitOfWork.JOBCOST_HDR).ToList();

                return JOBCOST_HDRList;
            }
        }

        protected IQueryable<JOBCOST_HDR> JOBCOST_HDRProjection(IRepositoryQuery<JOBCOST_HDR> query)
        {
            return query.Where(x => x.MASTER_JOBNO == loadJOBCOST_HDR.MASTER_JOBNO);
        }

        public IEnumerable<USER> USERCollection
        {
            get
            {
                var collection = GetEntities<USER>();
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
                    collection = collection.OrderBy(x => x.SHORTCODE);

                return collection;
            }
        }


        public IEnumerable<JOB_COSTTYPES> JOB_COSTTYPESCollection
        {
            get
            {
                var collection = GetEntities<JOB_COSTTYPES>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.SHORTCODE);

                return collection;
            }
        }
        #endregion
    }
}