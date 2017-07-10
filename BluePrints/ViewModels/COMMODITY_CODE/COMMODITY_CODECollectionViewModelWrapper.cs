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
    public class COMMODITY_CODECollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <COMMODITY_CODE, COMMODITY_CODEProjection, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of COMMODITY_CODECollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static COMMODITY_CODECollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new COMMODITY_CODECollectionViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the COMMODITY_CODECollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the COMMODITY_CODECollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected COMMODITY_CODECollectionViewModelWrapper(
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
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, COMMODITY_CODEProjectionFunc);
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

        private Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>> COMMODITY_CODEProjectionFunc()
        {
            if (IsProjectSpecific)
                return query => query.Where(x => (x.GUID_PROJECT == loadPROJECT.GUID || x.GUID_PROJECT == null));
            else
                return query => query.Where(x => x.GUID == Guid.Empty);
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODEProjection>> ConstructMainViewModelProjection()
        {
            if(IsProjectSpecific)
                return query => COMMODITY_CODEProjectionQueries.COMMODITY_CODEProjectionQuery(query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID));
            else
                return query => COMMODITY_CODEProjectionQueries.COMMODITY_CODEProjectionQuery(query.Where(x => x.GUID_PROJECT == null));
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<COMMODITY_CODEProjection> entities)
        {
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = OnBeforeEntitySaved;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        #region Collection Call Backs
        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public bool OnBeforeEntitySaved(COMMODITY_CODEProjection entity)
        {
            if (IsProjectSpecific)
                entity.Entity.GUID_PROJECT = loadPROJECT.GUID;
            return true;
        }
        #endregion

        #endregion

        #region View Commands
        public void GenerateCommodityCodes()
        {
            if (COMMODITY_CODECollectionViewModel != null && DisplayEntities != null)
            {
                List<COMMODITY_CODE> addStockCodes = new List<COMMODITY_CODE>();
                foreach (AREA area in AREACollection)
                {
                    //if default subarea already exists
                    if (!ProjectCOMMODITY_CODECollection.Any(x => x.GUID_AREA == area.GUID))
                    {
                        addStockCodes.AddRange(getAreaStockCodes(area));
                    }

                    IEnumerable<AREA> currentSubArea = SUBAREACollection.Where(x => x.GUID_PARENT == area.GUID);
                    foreach (AREA subArea in currentSubArea)
                    {
                        addStockCodes.AddRange(getAreaStockCodes(area, subArea));
                    }
                }

                COMMODITY_CODECollectionViewModel.BulkSave(addStockCodes);
            }
        }

        private List<COMMODITY_CODE> getAreaStockCodes(AREA area, AREA subArea = null)
        {
            List<COMMODITY_CODE> areaStockCodes = new List<COMMODITY_CODE>();
            Guid? subAreaGuid = null;
            if (subArea != null)
                subAreaGuid = subArea.GUID;

            foreach (COMMODITY_CODE globalStockCode in GlobalCOMMODITY_CODECollection)
            {
                if (!ProjectCOMMODITY_CODECollection.Any(x => x.CODE == globalStockCode.CODE && x.GUID_AREA == area.GUID && x.GUID_SUBAREA == subAreaGuid && x.GUID_DISCIPLINE == globalStockCode.GUID_DISCIPLINE))
                    areaStockCodes.Add(new COMMODITY_CODE()
                    {   GUID_AREA = area.GUID,
                        GUID_DISCIPLINE = globalStockCode.GUID_DISCIPLINE,
                        GUID_PROJECT = loadPROJECT.GUID,
                        GUID_SUBAREA = subAreaGuid,
                        UOM = globalStockCode.UOM,
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
            get { return "COMMODITY_CODECollectionViewModelWrapper"; }
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
       
        public IEnumerable<COMMODITY_CODE> GlobalCOMMODITY_CODECollection
        {
            get
            {
                var collection = GetEntities<COMMODITY_CODE>();
                if (collection != null)
                    collection = collection.Where(x => x.GUID_PROJECT == null).OrderBy(x => x.CODE);
                return collection;
            }
        }

        public IEnumerable<COMMODITY_CODE> ProjectCOMMODITY_CODECollection
        {
            get
            {
                var collection = GetEntities<COMMODITY_CODE>();
                if (collection != null)
                    collection = collection.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).OrderBy(x => x.CODE);
                return collection;
            }
        }

        public CollectionViewModel<COMMODITY_CODE, COMMODITY_CODE, Guid, IBluePrintsEntitiesUnitOfWork> COMMODITY_CODECollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<COMMODITY_CODE, COMMODITY_CODE, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<COMMODITY_CODE>();
            }
        }
        #endregion
    }
}