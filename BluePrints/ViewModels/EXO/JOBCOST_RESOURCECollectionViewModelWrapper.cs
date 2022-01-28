using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Base;
using BluePrints.Common.Utils;
using BluePrints.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class JOBCOST_RESOURCECollectionViewModelWrapper : BluePrintsEntitiesCollectionWrapper<JOBCOST_RESOURCE, JOBCOST_RESOURCE, int, IPrimeroEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of JOBCOST_RESOURCECollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static JOBCOST_RESOURCECollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new JOBCOST_RESOURCECollectionViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the JOBCOST_RESOURCECollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the JOBCOST_RESOURCECollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected JOBCOST_RESOURCECollectionViewModelWrapper(
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
            loaderCollection.AddLoaderDescription<STAFF, STAFF, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.STAFF);
            loaderCollection.AddLoaderDescription<STOCK_ITEMS, STOCK_ITEMS, string, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.STOCK_ITEMS);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(primeroUnitOfWorkFactory, x => x.JOBCOST_RESOURCE);
        }

        protected override Func<IRepositoryQuery<JOBCOST_RESOURCE>, IQueryable<JOBCOST_RESOURCE>> specifyMainViewModelProjection()
        {
            return query => query;
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<JOBCOST_RESOURCE> entities)
        {
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }
        #endregion

        #region Saving Behavior
        public override void UnifiedNewRowInitializationFromView(JOBCOST_RESOURCE projection)
        {
            BluePrintsDataUtils.PopulateNewJobcostResourcesDefaults(projection);
            base.UnifiedNewRowInitializationFromView(projection);
        }

        public override string UnifiedRowValidation(JOBCOST_RESOURCE projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(JOBCOST_RESOURCE projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }
        #endregion

        #region View Properties
        public IEnumerable<STAFF> STAFFCollection
        {
            get
            {
                var collection = GetEntities<STAFF>();
                return collection;
            }
        }

        public IEnumerable<STOCK_ITEMS> STOCK_ITEMCollection
        {
            get
            {
                var collection = GetEntities<STOCK_ITEMS>();
                return collection;
            }
        }
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "JOBCOST_RESOURCECollectionViewModelWrapper_v1"; }
        }
        #endregion
    }
}