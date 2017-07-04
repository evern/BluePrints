using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Data;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class STOCK_CODECollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <STOCK_CODE, STOCK_CODEProjection, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of STOCK_CODECollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static STOCK_CODECollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new STOCK_CODECollectionViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the STOCK_CODECollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the STOCK_CODECollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected STOCK_CODECollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private PROJECT loadPROJECT;

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        public bool IsProjectSpecific
        {
            get { return loadPROJECT != null; }
        }

        protected override void InitializeParameters(object parameter)
        {
            if (parameter != null)
            {
                var PROJECTParameter = (EntitiesParameter<PROJECT>)parameter;
                loadPROJECT = PROJECTParameter.GetEntity();
            }
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.PROJECTS);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription<UOM, UOM, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.UOMS);
            //need to add another viewmodel so that all stock codes are loaded for stock codes generation
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.STOCK_CODES, STOCK_CODEProjectionFunc);
            InvokeEntitiesLoaderDescriptionLoading();
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            if (IsProjectSpecific)
                return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
            else
                //not necessary to load anything when it's global commodity code
                return query => query.Where(x => x.GUID_PROJECT == Guid.Empty);
        }

        private Func<IRepositoryQuery<STOCK_CODE>, IQueryable<STOCK_CODE>> STOCK_CODEProjectionFunc()
        {
            if (IsProjectSpecific)
                return query => query.Where(x => (x.GUID_PROJECT == loadPROJECT.GUID || x.GUID_PROJECT == null));
            else
                return query => query.Where(x => x.GUID == Guid.Empty);
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.STOCK_CODES);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<STOCK_CODE>, IQueryable<STOCK_CODEProjection>> ConstructMainViewModelProjection()
        {
            if(IsProjectSpecific)
                return query => STOCK_CODEProjectionQueries.STOCK_CODEProjectionQuery(query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID));
            else
                return query => STOCK_CODEProjectionQueries.STOCK_CODEProjectionQuery(query.Where(x => x.GUID_PROJECT == null));
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<STOCK_CODEProjection> entities)
        {
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = OnBeforeEntitySaved;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        #region Collection Call Backs
        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public bool OnBeforeEntitySaved(STOCK_CODEProjection entity)
        {
            if (IsProjectSpecific)
                entity.Entity.GUID_PROJECT = loadPROJECT.GUID;
            return true;
        }
        #endregion

        #endregion

        #region View Commands
        public void GenerateStockCodes()
        {
            if (STOCK_CODECollectionViewModel != null && DisplayEntities != null)
            {
                List<STOCK_CODE> addStockCodes = new List<STOCK_CODE>();
                foreach (AREA area in AREACollection)
                {
                    //if default subarea already exists
                    if (!ProjectSTOCK_CODECollection.Any(x => x.GUID_AREA == area.GUID))
                    {
                        addStockCodes.AddRange(getAreaStockCodes(area));
                    }

                    IEnumerable<AREA> currentSubArea = SUBAREACollection.Where(x => x.GUID_PARENT == area.GUID);
                    foreach (AREA subArea in currentSubArea)
                    {
                        addStockCodes.AddRange(getAreaStockCodes(area, subArea));
                    }
                }

                STOCK_CODECollectionViewModel.BulkSave(addStockCodes);
            }
        }

        private List<STOCK_CODE> getAreaStockCodes(AREA area, AREA subArea = null)
        {
            List<STOCK_CODE> areaStockCodes = new List<STOCK_CODE>();
            Guid? subAreaGuid = null;
            if (subArea != null)
                subAreaGuid = subArea.GUID;

            foreach (STOCK_CODE globalStockCode in GlobalSTOCK_CODECollection)
            {
                if (!ProjectSTOCK_CODECollection.Any(x => x.CODE == globalStockCode.CODE && x.GUID_AREA == area.GUID && x.GUID_SUBAREA == subAreaGuid && x.GUID_DISCIPLINE == globalStockCode.GUID_DISCIPLINE))
                    areaStockCodes.Add(new STOCK_CODE()
                    {   GUID_AREA = area.GUID,
                        GUID_DISCIPLINE = globalStockCode.GUID_DISCIPLINE,
                        GUID_PROJECT = loadPROJECT.GUID,
                        GUID_SUBAREA = subAreaGuid,
                        CODE = globalStockCode.CODE,
                        DESCRIPTION = globalStockCode.DESCRIPTION
                    });
            }

            return areaStockCodes;
        }
        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "STOCK_CODECollectionViewModelWrapper"; }
        }

        public IEnumerable<AREA> AREACollection
        {
            get
            {
                var collection = GetEntities<AREA>();
                if (collection != null)
                    collection = collection.Where(x => x.GUID_PARENT == null).OrderBy(x => x.INTERNAL_NUM);
                return collection;
            }
        }

        public IEnumerable<AREA> SUBAREACollection
        {
            get
            {
                var collection = GetEntities<AREA>();
                if (collection != null)
                    collection = collection.Where(x => x.GUID_PARENT != null).OrderBy(x => x.INTERNAL_NUM);
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
       
        public IEnumerable<STOCK_CODE> GlobalSTOCK_CODECollection
        {
            get
            {
                var collection = GetEntities<STOCK_CODE>();
                if (collection != null)
                    collection = collection.Where(x => x.GUID_PROJECT == null).OrderBy(x => x.CODE);
                return collection;
            }
        }

        public IEnumerable<STOCK_CODE> ProjectSTOCK_CODECollection
        {
            get
            {
                var collection = GetEntities<STOCK_CODE>();
                if (collection != null)
                    collection = collection.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).OrderBy(x => x.CODE);
                return collection;
            }
        }

        public CollectionViewModel<STOCK_CODE, STOCK_CODE, Guid, IBluePrintsEntitiesUnitOfWork> STOCK_CODECollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<STOCK_CODE, STOCK_CODE, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<STOCK_CODE>();
            }
        }
        #endregion
    }
}