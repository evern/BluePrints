using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the COMMODITIES collection view model.
    /// </summary>
    public partial class COMMODITY_GROUP_CODECollectionViewModelWrapper :
        BluePrintsEntitiesMasterDetailCollectionsWrapper
        <COMMODITY_CODE, COMMODITY_GROUP_CODEProjection, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of COMMODITY_GROUP_DIRECTCollectionViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static COMMODITY_GROUP_CODECollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new COMMODITY_GROUP_CODECollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the COMMODITY_GROUP_DIRECTCollectionViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the COMMODITY_GROUP_DIRECTCollectionViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected COMMODITY_GROUP_CODECollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operation
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private CommodityCodeType loadCommodityCodeType;
        private PROJECT loadPROJECT;
        private List<Guid> RestoreExpandedGuids = new List<Guid>();
        protected override void InitializeParameters(object parameter)
        {
            var receiveParameter = (OptionalEntitiesParameter<PROJECT, CommodityCodeTypeClass>)parameter;
            loadPROJECT = receiveParameter.GetFirstEntity();
            var loadCommodityCodeTypeClass = receiveParameter.GetSecondEntity();
            loadCommodityCodeType = loadCommodityCodeTypeClass.commodityCodeType;
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS, null, x => { });
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES, null, x => { });
            loaderCollection.AddLoaderDescription<INDIRECT_TYPE, INDIRECT_TYPE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.INDIRECT_TYPES, null, x => { });
            loaderCollection.AddLoaderDescription<UOM, UOM, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.UOMS);
            InvokeEntitiesLoaderDescriptionLoading();
        }

        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_GROUP_CODEProjection>>
            ConstructMainViewModelProjection()
        {
            if(loadPROJECT != null)
                return query => COMMODITY_GROUP_CODEProjectionQueries.ConvertToProjectionCOMMODITY_GROUP_CODE(query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID));
            else
                return query => COMMODITY_GROUP_CODEProjectionQueries.ConvertToProjectionCOMMODITY_GROUP_CODE(query);
        }

        #region View Refresh
        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<COMMODITY_GROUP_CODEProjection> entities)
        {
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }
        #endregion

        #region Collection Call Backs
        #endregion

        #endregion

        #region View Properties
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "COMMODITY_GROUP_CODECollectionViewModelWrapper"; }
        }
        #endregion

        #region View Properties
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

        public IEnumerable<UOM> UOMCollection
        {
            get
            {
                var collection = GetEntities<UOM>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.UOM1);
                return collection;
            }
        }

        public IEnumerable<INDIRECT_TYPE> INDIRECT_TYPECollection
        {
            get
            {
                var collection = GetEntities<INDIRECT_TYPE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }
        #endregion

        #region View Behavior
        protected override void OnClose(CancelEventArgs e)
        {
            base.OnClose(e);
        }

        #endregion
    }
}