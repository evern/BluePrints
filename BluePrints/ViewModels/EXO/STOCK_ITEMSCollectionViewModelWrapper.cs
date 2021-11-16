using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Base;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class STOCK_ITEMSCollectionViewModelWrapper : BluePrintsEntitiesCollectionWrapper<STOCK_ITEMS, STOCK_ITEMS, string, IPrimeroEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of STOCK_ITEMSCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static STOCK_ITEMSCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new STOCK_ITEMSCollectionViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the STOCK_ITEMSCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the STOCK_ITEMSCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected STOCK_ITEMSCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void resolveParameters(object parameter)
        {
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<GLACCS, GLACCS, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.GLACCS);
            loaderCollection.AddLoaderDescription<JOB_COSTTYPES, JOB_COSTTYPES, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOB_COSTTYPES);
            loaderCollection.AddLoaderDescription<JOB_COSTGROUPS, JOB_COSTGROUPS, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOB_COSTGROUPS);
            loaderCollection.AddLoaderDescription<X_DEPARTMENT, X_DEPARTMENT, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.X_DEPARTMENTS);
            loaderCollection.AddLoaderDescription<STOCK_GROUPS, STOCK_GROUPS, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.STOCK_GROUPS);
            loaderCollection.AddLoaderDescription<STOCK_GROUP2S, STOCK_GROUP2S, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.STOCK_GROUP2S);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(primeroUnitOfWorkFactory, x => x.STOCK_ITEMS);
        }

        protected override Func<IRepositoryQuery<STOCK_ITEMS>, IQueryable<STOCK_ITEMS>> specifyMainViewModelProjection()
        {
            return query => query;
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<STOCK_ITEMS> entities)
        {
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }
        #endregion

        #region Saving Behavior
        public override void UnifiedNewRowInitializationFromView(STOCK_ITEMS projection)
        {
            BluePrintsDataUtils.PopulateNewStockItemsDefaults(projection);
            base.UnifiedNewRowInitializationFromView(projection);
        }

        protected override OperationInterceptMode OnBeforeProjectionSaveIsContinue(STOCK_ITEMS projection, out bool isNew)
        {
            if (projection.STDCOST == null)
                projection.STDCOST = 0;

            if (projection.SELLPRICE1 == null)
                projection.SELLPRICE1 = 0;

            return base.OnBeforeProjectionSaveIsContinue(projection, out isNew);
        }

        public override string UnifiedRowValidation(STOCK_ITEMS projection)
        {
            if (projection.STDCOST == null)
                return "Cost cannot be empty";

            if (projection.SELLPRICE1 == null)
                return "Sell price cannot be empty";

            return string.Empty;
        }

        public override string UnifiedValueValidation(STOCK_ITEMS projection, string field_name, object new_value, bool isPaste)
        {
            if (field_name == BindableBase.GetPropertyName(() => new STOCK_ITEMS().STDCOST))
            {
                if (new_value == null)
                    return "Cost cannot be empty";
            }
            else if (field_name == BindableBase.GetPropertyName(() => new STOCK_ITEMS().SELLPRICE1))
            {
                if (new_value == null)
                    return "Sell price cannot be empty";
            }

            return string.Empty;
        }
        #endregion

        #region View Properties
        public IEnumerable<JOB_COSTGROUPS> JOB_COSTGROUPSCollection
        {
            get
            {
                var collection = GetEntities<JOB_COSTGROUPS>();
                return collection;
            }
        }

        public IEnumerable<JOB_COSTTYPES> JOB_COSTTYPESCollection
        {
            get
            {
                var collection = GetEntities<JOB_COSTTYPES>();
                return collection;
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

        public IEnumerable<STOCK_GROUP2S> STOCK_GROUP2SCollection
        {
            get
            {
                var collection = GetEntities<STOCK_GROUP2S>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.GROUPNO);
                return collection;
            }
        }

        public IEnumerable<STOCK_GROUPS> STOCK_GROUPSCollection
        {
            get
            {
                var collection = GetEntities<STOCK_GROUPS>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.GROUPNO);
                return collection;
            }
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "STOCK_ITEMSCollectionViewModelWrapper_v1"; }
        }
        #endregion
    }
}