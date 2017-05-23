using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
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
            var getCOMMODITY_CODEFunc = loaderCollection.GetCollectionFunc<COMMODITY_CODE>();
            return
                query =>
                    ESTIMATION_DIRECT_ITEMProjectionQueries.JoinRATESOnESTIMATION_DIRECT_ITEMS(query,
                        getESTIMATION_DIRECTFunc, getDEPARTMENTSFunc, getCOMMODITY_CODEFunc, getRATESFunc);
        }

        #region Saving Routine
        public void ApplyEntityPropertiesToProjectionCallBack(Guid primaryKey, ESTIMATION_DIRECT_ITEMProjection projectionEntity, ESTIMATION_DIRECT_ITEM entity, bool isNewEntity)
        {
            //Guid original is generated by interceptor
            projectionEntity.Entity.GUID_ORIGINAL = entity.GUID_ORIGINAL;
        }
        #endregion

        #region View Refresh
        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<ESTIMATION_DIRECT_ITEMProjection> entities)
        {
            COMMODITY_CODECollectionViewModel.OnAfterEntitySavedCallBack = OnAfterCommodityCodeSavedCallBack;
            MainViewModel.ApplyEntityPropertiesToProjectionCallBack = ApplyEntityPropertiesToProjectionCallBack;
            base.AssignCallBacksAndRaisePropertyChange(entities);
            Refresh_COMMODITY_GROUP_CODE();
        }

        protected override void OnBeforeApplyProjectionPropertiesToEntity(ESTIMATION_DIRECT_ITEMProjection projectionEntity, ESTIMATION_DIRECT_ITEM entity)
        {
            if(projectionEntity.TempCommodityCode != null)
            {
                bool isFullCodeExists;
                bool isNewSignature = IsNewCommodityCodeSignature(projectionEntity.TempCommodityCode, out isFullCodeExists);
                if (isNewSignature)
                {
                    if (!isFullCodeExists)
                    {
                        projectionEntity.Entity.GUID_COMMODITY_CODE = AddNewCommodityCode(projectionEntity.TempCommodityCode);
                    }
                    else if (MessageBoxService.ShowMessage("Do you wish to edit current commodity code?",
                         BluePrintsResources.Confirmation_Caption, MessageButton.YesNo) == MessageResult.No)
                    {
                        projectionEntity.Entity.GUID_COMMODITY_CODE = AddNewCommodityCode(projectionEntity.TempCommodityCode);
                    }
                }
            }

            projectionEntity.Entity.GUID_ESTIMATION_DIRECT = loadESTIMATION_DIRECT.GUID;
            base.OnBeforeApplyProjectionPropertiesToEntity(projectionEntity, entity);
        }

        private Guid AddNewCommodityCode(COMMODITY_CODE newCommodityCode)
        {
            COMMODITY_CODE newCOMMODITY_CODE = new COMMODITY_CODE();
            DataUtils.ShallowCopy(newCOMMODITY_CODE, newCommodityCode);
            newCOMMODITY_CODE.GUID = Guid.Empty;
            newCOMMODITY_CODE.GUID_PROJECT = loadPROJECT.GUID;
            COMMODITY_CODECollectionViewModel.Save(newCOMMODITY_CODE);
            return newCOMMODITY_CODE.GUID;
        }

        private bool EditCommodityCode(COMMODITY_CODE editCommodityCode)
        {
            COMMODITY_CODE actualCommodityCode = COMMODITY_CODECollection.FirstOrDefault(x => x.GUID == editCommodityCode.GUID);
            if (actualCommodityCode != null)
            {
                DataUtils.ShallowCopy(actualCommodityCode, editCommodityCode);
                COMMODITY_CODECollectionViewModel.Save(actualCommodityCode);
                return true;
            }
            else
                return false;
        }

        public void OnAfterCommodityCodeSavedCallBack(COMMODITY_CODE savedEntity, bool isNewEntity)
        {

        }

        public bool IsNewCommodityCodeSignature(COMMODITY_CODE commodity_code, out bool isFullCodeExists)
        {
            IEnumerable<COMMODITY_CODE> projectCommodity_Code = COMMODITY_CODECollection.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
            if(commodity_code.GUID_GROUP_PARENT != null)
            {
                //get the parent
                COMMODITY_CODE currentGroupParentCommodity_Code = projectCommodity_Code.FirstOrDefault(x => x.GUID == commodity_code.GUID_GROUP_PARENT);
                //when parent doesn't exists, make a new branch because of dirty data
                if (currentGroupParentCommodity_Code == null)
                {
                    isFullCodeExists = false;
                    return true;
                }
                else
                {
                    //get the entire branch of childrens
                    IEnumerable<COMMODITY_CODE> currentProjectGroupChildrenCommodity_Codes = projectCommodity_Code.Where(x => x.GUID_GROUP_PARENT == currentGroupParentCommodity_Code.GUID);
                    IEnumerable<COMMODITY_CODE> allProjectGroupParentCommodity_Codes = projectCommodity_Code.Where(x => x.GUID_COMMODITY_GROUP_DIRECT == currentGroupParentCommodity_Code.GUID_COMMODITY_GROUP_DIRECT);

                    if (allProjectGroupParentCommodity_Codes.Count() > 0)
                        isFullCodeExists = true;
                    else
                        isFullCodeExists = false;

                    bool isMatch = false;

                    foreach (COMMODITY_CODE allProjectGroupParentCommodity_Code in allProjectGroupParentCommodity_Codes)
                    {
                        IEnumerable<COMMODITY_CODE> allProjectGroupChildrenCommodity_Codes = projectCommodity_Code.Where(x => x.GUID_GROUP_PARENT == allProjectGroupParentCommodity_Code.GUID);
                        isMatch = IsGroupChildrenMatch(currentProjectGroupChildrenCommodity_Codes, allProjectGroupChildrenCommodity_Codes);
                        if (isMatch)
                            return false;
                    }

                    return true;
                }
            }
            else
            {
                IEnumerable<COMMODITY_CODE> projectStandaloneCommodity_Codes = projectCommodity_Code.Where(x => x.GUID_GROUP_PARENT == null);
                COMMODITY_CODE projectSameSignatureCommodity_Code = projectStandaloneCommodity_Codes
                    .FirstOrDefault(x => x.FULLCODE == commodity_code.FULLCODE && x.RATE_FREIGHT == commodity_code.RATE_FREIGHT && x.RATE_PLANT == commodity_code.RATE_PLANT && x.RATE_SUPPLY == commodity_code.RATE_SUPPLY && x.HOURS_INSTALL == commodity_code.HOURS_INSTALL);

                isFullCodeExists = projectStandaloneCommodity_Codes.Any(x => x.FULLCODE == commodity_code.FULLCODE);
                if (projectSameSignatureCommodity_Code != null)
                    return false;
                else
                    return true;
            }
        }

        public bool IsGroupChildrenMatch(IEnumerable<COMMODITY_CODE> currentGroupChildrenCommodity_Codes, IEnumerable<COMMODITY_CODE> otherGroupChildrenCommodity_Codes)
        {
            foreach(COMMODITY_CODE currentGroupChildrenCommodity_Code in currentGroupChildrenCommodity_Codes)
            {
                COMMODITY_CODE sameOtherGroupChildrenCommodity_Code = otherGroupChildrenCommodity_Codes
                    .FirstOrDefault(x => x.FULLCODE == currentGroupChildrenCommodity_Code.FULLCODE
                    && x.RATE_FREIGHT == currentGroupChildrenCommodity_Code.RATE_FREIGHT
                    && x.RATE_PLANT == currentGroupChildrenCommodity_Code.RATE_PLANT
                    && x.RATE_SUPPLY == currentGroupChildrenCommodity_Code.RATE_SUPPLY);

                if (sameOtherGroupChildrenCommodity_Code == null)
                    return false;
            }

            //swap the other way to see if anything is missing from the collection
            foreach (COMMODITY_CODE otherGroupChildrenCommodity_Code in otherGroupChildrenCommodity_Codes)
            {
                COMMODITY_CODE sameCurrentGroupChildrenCommodity_Codes = currentGroupChildrenCommodity_Codes
                    .FirstOrDefault(x => x.FULLCODE == otherGroupChildrenCommodity_Code.FULLCODE
                    && x.RATE_FREIGHT == otherGroupChildrenCommodity_Code.RATE_FREIGHT
                    && x.RATE_PLANT == otherGroupChildrenCommodity_Code.RATE_PLANT
                    && x.RATE_SUPPLY == otherGroupChildrenCommodity_Code.RATE_SUPPLY);

                if (sameCurrentGroupChildrenCommodity_Codes == null)
                    return false;
            }

            return true;
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

        #region View Behavior
        /// <summary>
        /// Influence column(s) when changes happens in other column
        /// </summary>
        public void CellValueChanging(CellValueChangedEventArgs e)
        {
            var activeProjection = (ESTIMATION_DIRECT_ITEMProjection)e.Row;
            if (e.Column.FieldName ==
                BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().Entity) + "." +
                BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().GUID_COMMODITY_CODE))
            {
                activeProjection.TempCommodityCode = COMMODITY_GROUP_CODECollection.FirstOrDefault(x => x.GUID == (Guid)e.Value);
            }
        }
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
                    collection = collection.Where(x => x.ISQUANTIFIABLE).OrderBy(x => x.GUID_PROJECT).OrderBy(x => x.CODE);
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

        public CollectionViewModel<COMMODITY_CODE, COMMODITY_CODE, Guid, IBluePrintsEntitiesUnitOfWork>
            COMMODITY_CODECollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return
                    (CollectionViewModel<COMMODITY_CODE, COMMODITY_CODE, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<COMMODITY_CODE>();
            }
        }

        private void Refresh_COMMODITY_GROUP_CODE()
        {
            commodity_group_codeCollection = null;
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertyChanged(x => x.COMMODITY_GROUP_CODECollection)));
        }


        const string childPrefix = "child";
        const string parentPrefix = "parent";
        List<COMMODITY_CODE> commodity_group_codeCollection;
        public IEnumerable<COMMODITY_CODE> COMMODITY_GROUP_CODECollection
        {
            get
            {
                if(commodity_group_codeCollection == null)
                {
                    commodity_group_codeCollection = new List<COMMODITY_CODE>();
                    if (COMMODITY_GROUP_DIRECTCollection != null)
                    {
                        //get the parents only
                        IEnumerable<COMMODITY_GROUP_DIRECT> commodityGroupParentCollection = COMMODITY_GROUP_DIRECTCollection.Where(x => x.GUID_PARENT == null);
                        //get the childs only
                        IEnumerable<COMMODITY_GROUP_DIRECT> commodityGroupChildCollection = COMMODITY_GROUP_DIRECTCollection.Where(x => x.GUID_PARENT != null && x.GUID_COMMODITYCODE != null);

                        //temp id used to identify parents in the view
                        int tempParentId = 0;
                        int tempChildId = 0;
                        if (commodityGroupParentCollection != null && COMMODITY_CODECollection != null)
                        {
                            foreach (COMMODITY_GROUP_DIRECT commodityGroup in commodityGroupParentCollection)
                            {
                                IEnumerable<COMMODITY_GROUP_DIRECT> currentGroupChildrens = commodityGroupChildCollection.Where(x => x.GUID_PARENT == commodityGroup.GUID);
                                List<COMMODITY_CODE> tempChildCommodityCodes = new List<COMMODITY_CODE>();
                                //construct child dynamically
                                foreach (COMMODITY_GROUP_DIRECT currentGroupChildren in currentGroupChildrens)
                                {
                                    COMMODITY_CODE childCommodityCode = COMMODITY_CODECollection.FirstOrDefault(x => x.GUID == currentGroupChildren.GUID_COMMODITYCODE);
                                    if(childCommodityCode != null)
                                    {
                                        COMMODITY_CODE newChildCommodityCode = new COMMODITY_CODE();
                                        DataUtils.ShallowCopy(newChildCommodityCode, childCommodityCode);
                                        newChildCommodityCode.ISGROUPHEADER = false;
                                        newChildCommodityCode.Temp_Id = childPrefix + tempChildId.ToString();
                                        newChildCommodityCode.Temp_Parent_Id = parentPrefix + tempParentId.ToString();
                                        tempChildId += 1;
                                        tempChildCommodityCodes.Add(newChildCommodityCode);
                                    }
                                }

                                COMMODITY_CODE tempParentCommodityCode = new COMMODITY_CODE()
                                {
                                    GUID = Guid.NewGuid(),
                                    GUID_PARENT = Guid.Empty,
                                    GUID_COMMODITY_GROUP_DIRECT = commodityGroup.GUID,
                                    COMMODITYCODETYPE = CommodityCodeType.Direct,
                                    DESCRIPTION = commodityGroup.DESCRIPTION,
                                    SortOrder = 0,
                                    IsExpanded = false,
                                    ISQUANTIFIABLE = false,
                                    ISGROUPHEADER = true,
                                    RATE_FREIGHT = tempChildCommodityCodes.Sum(x => x.RATE_FREIGHT),
                                    RATE_PLANT = tempChildCommodityCodes.Sum(x => x.RATE_PLANT),
                                    RATE_SUPPLY = tempChildCommodityCodes.Sum(x => x.RATE_SUPPLY),
                                    Temp_Id = parentPrefix + tempParentId.ToString()
                                };

                                tempParentId += 1;
                                commodity_group_codeCollection.Add(tempParentCommodityCode);
                                commodity_group_codeCollection.AddRange(tempChildCommodityCodes);
                            }
                        }
                    }

                    if (COMMODITY_CODECollection != null)
                    {
                        foreach(COMMODITY_CODE commodityCode in COMMODITY_CODECollection)
                        {
                            COMMODITY_CODE tempCommodityCode = new COMMODITY_CODE();
                            DataUtils.ShallowCopy(tempCommodityCode, commodityCode);
                            tempCommodityCode.Temp_Id = tempCommodityCode.GUID.ToString();
                            tempCommodityCode.Temp_Parent_Id = tempCommodityCode.GUID_PARENT.ToString();
                            commodity_group_codeCollection.Add(tempCommodityCode);
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