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
    public partial class ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper :
        BluePrintsEntitiesMasterDetailCollectionsWrapper
        <ESTIMATION_DIRECT_ITEM, ESTIMATION_DIRECT_ITEMProjection, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of ESTIMATION_DIRECT_ITEMCollectionViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the ESTIMATION_DIRECT_ITEMCollectionViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the ESTIMATION_DIRECT_ITEMCollectionViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operation
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        private List<Guid> RestoreExpandedGuids = new List<Guid>();
        private PROJECT loadPROJECT;
        private ESTIMATION_DIRECT loadESTIMATION_DIRECT;
        protected override void InitializeParameters(object parameter)
        {
            var receiveParameter =
                (OptionalEntitiesParameter<PROJECT, ESTIMATION_DIRECT>)parameter;
            loadPROJECT = receiveParameter.GetFirstEntity();
            loadESTIMATION_DIRECT = receiveParameter.GetSecondEntity();

            if (loadPROJECT != null)
                isQueryForLiveStatus = true;
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.ESTIMATION_DIRECTS, ESTIMATION_DIRECTProjectionFunc, x => loadESTIMATION_DIRECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.WORKPACKS, WORKPACKProjectionFunc);
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_GROUP_DIRECT, COMMODITY_GROUP_DIRECTProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, COMMODITY_CODEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECT_REPORTS, PROJECT_REPORTProjectionFunc, null, true);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc);
            InvokeEntitiesLoaderDescriptionLoading();
        }

        private bool isQueryForLiveStatus { get; set; }

        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            if (isQueryForLiveStatus)
                return query => query.Where(x => x.GUID == loadPROJECT.GUID);
            else
                return query => query.Where(x => x.GUID == loadESTIMATION_DIRECT.GUID_PROJECT);
        }

        private Func<IRepositoryQuery<ESTIMATION_DIRECT>, IQueryable<ESTIMATION_DIRECT>> ESTIMATION_DIRECTProjectionFunc()
        {
            if (isQueryForLiveStatus)
                return
                    query =>
                        query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == EstimationStatus.Live);
            else
                return query => query.Where(x => x.GUID == loadESTIMATION_DIRECT.GUID);
        }

        private Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK>> WORKPACKProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.TYPE == WorkpackType.Design);
        }

        private Func<IRepositoryQuery<PHASE>, IQueryable<PHASE>> PHASEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.COST_GROUP == CostGroup.Site);
        }

        private Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>> COMMODITY_CODEProjectionFunc()
        {
            return query => query.Where(x => x.COMMODITYCODETYPE == CommodityCodeType.Direct);
        }

        private Func<IRepositoryQuery<COMMODITY_GROUP_DIRECT>, IQueryable<COMMODITY_GROUP_DIRECT>>
            COMMODITY_GROUP_DIRECTProjectionFunc()
        {
            return query => query;
        }

        private Func<IRepositoryQuery<PROJECT_REPORT>, IQueryable<PROJECT_REPORT>> PROJECT_REPORTProjectionFunc()
        {
            return query => query;
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.ESTIMATION_DIRECT_ITEMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<ESTIMATION_DIRECT_ITEM>, IQueryable<ESTIMATION_DIRECT_ITEMProjection>>
            ConstructMainViewModelProjection()
        {
            var getDEPARTMENTSFunc = loaderCollection.GetCollectionFunc<DEPARTMENT>();
            var getRATESFunc = loaderCollection.GetCollectionFunc<RATE>();
            var getESTIMATION_DIRECTFunc = loaderCollection.GetObjectFunc<ESTIMATION_DIRECT>();
            return
                query =>
                    ESTIMATION_DIRECT_ITEMProjectionQueries.JoinRATESOnESTIMATION_DIRECT_ITEMS(query,
                        getESTIMATION_DIRECTFunc, getDEPARTMENTSFunc, getRATESFunc);
        }

        #region View Refresh
        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<ESTIMATION_DIRECT_ITEMProjection> entities)
        {
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        public override void OnAfterAffectingEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender)
        {
            base.OnAfterAffectingEntitiesChanged(key, changedType, messageType, sender);
            Refresh_COMMODITY_GROUP_CODE();
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
            get { return "ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper"; }
        }
        #endregion

        #region View Properties
        public IEnumerable<PROJECT> PROJECTCollection
        {
            get
            {
                var collection = GetEntities<PROJECT>();
                return collection;
            }
        }

        public IEnumerable<WORKPACK> WORKPACKCollection
        {
            get
            {
                var collection = GetEntities<WORKPACK>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.INTERNAL_NAME1).OrderBy(x => x.INTERNAL_NAME2);
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

        public IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection
        {
            get
            {
                var collection = GetEntities<COMMODITY_CODE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.GUID_PROJECT).OrderBy(x => x.CODE);
                return collection;
            }
        }

        public IEnumerable<COMMODITY_GROUP_DIRECT> COMMODITY_GROUP_DIRECTCollection
        {
            get
            {
                var collection = GetEntities<COMMODITY_GROUP_DIRECT>();
                return collection;
            }
        }

        private void Refresh_COMMODITY_GROUP_CODE()
        {
            commodity_group_codeCollection = null;
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertyChanged(x => x.COMMODITY_GROUP_CODECollection)));
        }

        List<COMMODITY_CODE> commodity_group_codeCollection;
        public IEnumerable<COMMODITY_CODE> COMMODITY_GROUP_CODECollection
        {
            get
            {
                if(commodity_group_codeCollection == null)
                {
                    commodity_group_codeCollection = new List<COMMODITY_CODE>();
                    //get the parents only
                    IEnumerable<COMMODITY_GROUP_DIRECT> commodityGroupParentCollection = COMMODITY_GROUP_DIRECTCollection.Where(x => x.GUID_PARENT == null);
                    //get the childs only
                    IEnumerable<COMMODITY_GROUP_DIRECT> commodityGroupChildCollection = COMMODITY_GROUP_DIRECTCollection.Where(x => x.GUID_PARENT != null && x.GUID_COMMODITYCODE != null);

                    //temp id used to identify parents in the view
                    int tempParentId = 0;
                    int tempChildId = 0;
                    if (commodityGroupParentCollection != null)
                    {
                        foreach(COMMODITY_GROUP_DIRECT commodityGroup in commodityGroupParentCollection)
                        {
                            IEnumerable<COMMODITY_GROUP_DIRECT> currentGroupChildrens = commodityGroupChildCollection.Where(x => x.GUID_PARENT == commodityGroup.GUID);
                            List<COMMODITY_CODE> tempChildCommodityCodes = new List<COMMODITY_CODE>();
                            //construct child dynamically
                            foreach (COMMODITY_GROUP_DIRECT currentGroupChildren in currentGroupChildrens)
                            {
                                COMMODITY_CODE childCommodityCode = new COMMODITY_CODE();
                                DataUtils.ShallowCopy(childCommodityCode, currentGroupChildren.COMMODITY_CODE);
                                childCommodityCode.Temp_Id = "child" + tempChildId.ToString();
                                childCommodityCode.Temp_Parent_Id = tempParentId.ToString();
                                tempChildId += 1;
                                tempChildCommodityCodes.Add(childCommodityCode);
                            }

                            COMMODITY_CODE tempParentCommodityCode = new COMMODITY_CODE()
                            {
                                GUID = Guid.Empty,
                                GUID_PARENT = Guid.Empty,
                                COMMODITYCODETYPE = CommodityCodeType.Direct,
                                DESCRIPTION = commodityGroup.DESCRIPTION,
                                SortOrder = 0,
                                IsExpanded = false,
                                ISQUANTIFIABLE = false,
                                ISGROUPHEADER = true,
                                RATE_FREIGHT = tempChildCommodityCodes.Sum(x => x.RATE_FREIGHT),
                                RATE_PLANT = tempChildCommodityCodes.Sum(x => x.RATE_PLANT),
                                RATE_SUPPLY = tempChildCommodityCodes.Sum(x => x.RATE_SUPPLY),
                                Temp_Id = tempParentId.ToString()
                            };

                            tempParentId += 1;
                            commodity_group_codeCollection.Add(tempParentCommodityCode);
                            commodity_group_codeCollection.AddRange(tempChildCommodityCodes);
                        }
                    }

                    if (COMMODITY_CODECollection != null)
                    {
                        foreach(COMMODITY_CODE commodityCode in COMMODITY_CODECollection)
                        {
                            COMMODITY_CODE tempCommodityCode = new COMMODITY_CODE();
                            DataUtils.ShallowCopy(tempCommodityCode, commodityCode);
                            tempCommodityCode.Temp_Id = commodityCode.GUID.ToString();
                            tempCommodityCode.Temp_Parent_Id = commodityCode.GUID_PARENT.ToString();
                            commodity_group_codeCollection.Add(commodityCode);
                        }
                    }
                }

                return commodity_group_codeCollection;
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