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
            return query => query.Where(x => x.PHASE_TYPE == PhaseType.Construct);
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
            MainViewModel.OnAfterEntitySavedCallBack = OnAfterEntitySavedCallBack;
            MainViewModel.ApplyEntityPropertiesToProjectionCallBack = ApplyEntityPropertiesToProjectionCallBack;
            base.AssignCallBacksAndRaisePropertyChange(entities);
            Refresh_COMMODITY_GROUP_CODE();
        }

        private DevExpress.Mvvm.IDialogService BulkColumnEditDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("BulkColumnEditService"); }
        }

        protected override void OnBeforeApplyProjectionPropertiesToEntity(ESTIMATION_DIRECT_ITEMProjection projectionEntity, ESTIMATION_DIRECT_ITEM entity)
        {
            //newly added estimate from code will not have COMMODITY_CODE populated, see OnAfterEntitySavedCallBack
            if (projectionEntity.COMMODITY_CODE != null)
            {
                //for standalone commodity code existing project matching fullcode will be returned
                //for group commodity code new group composition will be returned if existing project group code doesn't match
                List<COMMODITY_CODE> existingStandaloneOrAddNewGroupCommodityCodes; 
                bool isExists = isProjectCodeSignatureExist(projectionEntity.COMMODITY_CODE, out existingStandaloneOrAddNewGroupCommodityCodes);
                if (!isExists)
                {
                    //also validate for count == 0 so that ToList() clause can be used in subroutines
                    if (existingStandaloneOrAddNewGroupCommodityCodes == null || existingStandaloneOrAddNewGroupCommodityCodes.Count == 0)
                    {
                        projectionEntity.Entity.GUID_COMMODITY_CODE = AddNewCommodityCode(projectionEntity.COMMODITY_CODE);
                    }
                    else if (MessageBoxService.ShowMessage("Do you wish to add as new commodity code?",
                         BluePrintsResources.Confirmation_Caption, MessageButton.YesNo) == MessageResult.Yes)
                    {
                        if(projectionEntity.COMMODITY_CODE.IsChildren)
                        {
                            COMMODITY_CODE groupParent = existingStandaloneOrAddNewGroupCommodityCodes.First(x => x.GUID == projectionEntity.COMMODITY_CODE.GUID_GROUP_PARENT);
                            Guid groupParentGuid = AddNewCommodityCode(groupParent);
                            existingStandaloneOrAddNewGroupCommodityCodes.Remove(groupParent);
                            foreach (COMMODITY_CODE existingStandaloneOrAddNewGroupCommodityCode in existingStandaloneOrAddNewGroupCommodityCodes)
                            {
                                Guid newGuid = AddNewCommodityCode(existingStandaloneOrAddNewGroupCommodityCode, groupParentGuid);
                                if (existingStandaloneOrAddNewGroupCommodityCode.FULLCODE == projectionEntity.COMMODITY_CODE.FULLCODE)
                                    projectionEntity.Entity.GUID_COMMODITY_CODE = newGuid;
                            }
                        }
                        else
                            projectionEntity.Entity.GUID_COMMODITY_CODE = AddNewCommodityCode(projectionEntity.COMMODITY_CODE);
                    }
                    else
                    {
                        EditCommodityCode(projectionEntity.COMMODITY_CODE, projectionEntity.COMMODITY_CODE.GUID);
                        //Guid? selectedGuid = null;
                        //if(projectionEntity.COMMODITY_CODE != null)
                        //    selectedGuid = projectionEntity.COMMODITY_CODE.GUID;
                        //ESTIMATE_COMMODITY_CODESelectionViewModel estimateCommodityCodeSelectionViewModel = ESTIMATE_COMMODITY_CODESelectionViewModel.Create(COMMODITY_GROUP_CODECollection, selectedGuid, PROJECTCollection, DISCIPLINECollection);
                        //if(BulkColumnEditDialogService.ShowDialog(MessageButton.OKCancel, "Select Item to edit",
                        //    "ESTIMATE_COMMODITY_CODESelectionView", estimateCommodityCodeSelectionViewModel) == MessageResult.OK)
                        //{
                        //    EditCommodityCode(projectionEntity.COMMODITY_CODE, estimateCommodityCodeSelectionViewModel.SelectedItem.GUID);
                        //}
                    }
                }
            }
            
            projectionEntity.Entity.GUID_ESTIMATION_DIRECT = loadESTIMATION_DIRECT.GUID;
            base.OnBeforeApplyProjectionPropertiesToEntity(projectionEntity, entity);
        }
        
        private Guid AddNewCommodityCode(COMMODITY_CODE newCommodityCode, Guid? groupParentGuid = null)
        {
            COMMODITY_CODE newCOMMODITY_CODE = new COMMODITY_CODE();
            DataUtils.ShallowCopy(newCOMMODITY_CODE, newCommodityCode);
            newCOMMODITY_CODE.GUID = Guid.Empty;
            if (groupParentGuid != null)
                newCOMMODITY_CODE.GUID_GROUP_PARENT = groupParentGuid;

            newCOMMODITY_CODE.GUID_PROJECT = loadPROJECT.GUID;
            COMMODITY_CODECollectionViewModel.Save(newCOMMODITY_CODE);
            return newCOMMODITY_CODE.GUID;
        }

        private bool EditCommodityCode(COMMODITY_CODE editedCommodityCode, Guid selectedCommodityCodeGuid)
        {
            if (editedCommodityCode.GUID == Guid.Empty || selectedCommodityCodeGuid == Guid.Empty)
                throw new InvalidOperationException();

            COMMODITY_CODE actualCommodityCode = COMMODITY_CODECollection.FirstOrDefault(x => x.GUID == selectedCommodityCodeGuid);
            if (actualCommodityCode != null)
            {
                DataUtils.ShallowCopy(actualCommodityCode, editedCommodityCode);
                COMMODITY_CODECollectionViewModel.Save(actualCommodityCode);
                return true;
            }
            else
                return false;
        }

        public void OnAfterEntitySavedCallBack(ESTIMATION_DIRECT_ITEMProjection savedEntity, bool isNewEntity)
        {
            if (isNewEntity && savedEntity.COMMODITY_CODE != null && savedEntity.COMMODITY_CODE.GUID_COMMODITY_GROUP_DIRECT != null)
            {
                //get the parent
                COMMODITY_GROUP_DIRECT commodityGroup = COMMODITY_GROUP_DIRECTCollection.FirstOrDefault(x => x.GUID == savedEntity.COMMODITY_CODE.GUID_COMMODITY_GROUP_DIRECT);
                //get the group childrens
                IEnumerable<COMMODITY_GROUP_DIRECT> commodityChildrens = COMMODITY_GROUP_DIRECTCollection.Where(x => x.GUID_PARENT == commodityGroup.GUID);
                //get the commodity code childrens
                IEnumerable<COMMODITY_CODE> childrenCommodityCodes = COMMODITY_CODECollection.Where(x => commodityChildrens.Any(z => z.GUID_COMMODITYCODE == x.GUID));
                List<COMMODITY_CODE> newChildrenCommodityCodes = new List<COMMODITY_CODE>();

                foreach(COMMODITY_CODE childrenCommodityCode in childrenCommodityCodes)
                {
                    ESTIMATION_DIRECT_ITEMProjection newEstimation = new ESTIMATION_DIRECT_ITEMProjection();
                    newEstimation.Entity.GUID_ORIGINAL_PARENT = savedEntity.Entity.GUID_ORIGINAL;

                    //Relies on OnBeforeApplyProjectionPropertiesToEntity to save Commodity Code
                    //also since entity is not refreshed savedEntity.COMMODITY_CODE.GUID_COMMODITY_GROUP_DIRECT is still using the global version
                    Guid actualParentCommodityCodeGuid = (Guid)savedEntity.Entity.GUID_COMMODITY_CODE;
                    newEstimation.Entity.GUID_COMMODITY_CODE = AddNewCommodityCode(childrenCommodityCode, actualParentCommodityCodeGuid);
                    MainViewModel.Save(newEstimation);
                }
            }
        }

        private bool isProjectCodeSignatureExist(COMMODITY_CODE commodity_code)
        {
            List<COMMODITY_CODE> dummyCollection;
            return isProjectCodeSignatureExist(commodity_code, out dummyCollection);
        }

        private bool isProjectCodeSignatureExist(COMMODITY_CODE commodity_code, out List<COMMODITY_CODE> existingStandaloneOrAddNewGroupCommodityCodes)
        {
            IEnumerable<COMMODITY_CODE> projectCommodity_Code = COMMODITY_CODECollection.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
            if(commodity_code.IsChildren)
            {
                //get the parent
                COMMODITY_CODE currentGroupParentCommodity_Code = projectCommodity_Code.FirstOrDefault(x => x.GUID == commodity_code.GUID_GROUP_PARENT);
                //when parent doesn't exists, make a new branch
                if (currentGroupParentCommodity_Code == null)
                {
                    existingStandaloneOrAddNewGroupCommodityCodes = null;
                    return false;
                }
                else
                {
                    //parent should have group guid
                    if (currentGroupParentCommodity_Code.GUID_COMMODITY_GROUP_DIRECT == null)
                    {
                        existingStandaloneOrAddNewGroupCommodityCodes = null;
                        return false;
                    }

                    //get the entire branch of childrens, excluding current children
                    List<COMMODITY_CODE> currentProjectGroupChildrenCommodity_Codes = projectCommodity_Code.Where(x => x.GUID_GROUP_PARENT == currentGroupParentCommodity_Code.GUID && x.GUID != commodity_code.GUID).ToList();
                    currentProjectGroupChildrenCommodity_Codes.Add(commodity_code);
                    bool result = isProjectGroupSignatureExist((Guid)currentGroupParentCommodity_Code.GUID_COMMODITY_GROUP_DIRECT, currentProjectGroupChildrenCommodity_Codes, out existingStandaloneOrAddNewGroupCommodityCodes);
                    existingStandaloneOrAddNewGroupCommodityCodes.Add(currentGroupParentCommodity_Code);
                    return result;
                }
            }
            else
            {
                IEnumerable<COMMODITY_CODE> projectStandaloneCommodity_Codes = projectCommodity_Code.Where(x => x.GUID_GROUP_PARENT == null);
                existingStandaloneOrAddNewGroupCommodityCodes = projectStandaloneCommodity_Codes.Where(x => x.FULLCODE == commodity_code.FULLCODE).ToList();

                if (isCommodityCodeExists(commodity_code, projectStandaloneCommodity_Codes))
                    return true;
                else
                    return false;
            }
        }

        private bool isProjectCommodityGroupExists(Guid commodityGroupGuid)
        {
            IEnumerable<COMMODITY_CODE> projectCommodity_Code = COMMODITY_CODECollection.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
            return projectCommodity_Code.Any(x => x.GUID_COMMODITY_GROUP_DIRECT == commodityGroupGuid);
        }

        private bool isProjectGroupSignatureExist(Guid commodityGroupGuid, IEnumerable<COMMODITY_CODE> currentProjectGroupChildrenCommodity_Codes)
        {
            List<COMMODITY_CODE> dummyCollection;
            return isProjectGroupSignatureExist(commodityGroupGuid, currentProjectGroupChildrenCommodity_Codes, out dummyCollection);
        }

        private bool isProjectGroupSignatureExist(Guid commodityGroupGuid, IEnumerable<COMMODITY_CODE> currentProjectGroupChildrenCommodity_Codes, out List<COMMODITY_CODE> existingStandaloneOrAddNewGroupCommodityCodes)
        {
            IEnumerable<COMMODITY_CODE> projectCommodity_Code = COMMODITY_CODECollection.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
            IEnumerable<COMMODITY_CODE> allProjectGroupParentCommodity_Codes = projectCommodity_Code.Where(x => x.GUID_COMMODITY_GROUP_DIRECT == commodityGroupGuid);

            if (allProjectGroupParentCommodity_Codes.Count() > 0)
                existingStandaloneOrAddNewGroupCommodityCodes = currentProjectGroupChildrenCommodity_Codes.ToList();
            else
                existingStandaloneOrAddNewGroupCommodityCodes = null;

            bool isMatch = false;

            foreach (COMMODITY_CODE allProjectGroupParentCommodity_Code in allProjectGroupParentCommodity_Codes)
            {
                IEnumerable<COMMODITY_CODE> allProjectGroupChildrenCommodity_Codes = projectCommodity_Code.Where(x => x.GUID_GROUP_PARENT == allProjectGroupParentCommodity_Code.GUID);
                isMatch = IsGroupChildrensSignatureMatch(currentProjectGroupChildrenCommodity_Codes, allProjectGroupChildrenCommodity_Codes);
                if (isMatch)
                    return true;
            }

            return false;
        }

        private bool IsGroupChildrensSignatureMatch(IEnumerable<COMMODITY_CODE> currentGroupChildrenCommodity_Codes, IEnumerable<COMMODITY_CODE> otherGroupChildrenCommodity_Codes)
        {
            foreach(COMMODITY_CODE currentGroupChildrenCommodity_Code in currentGroupChildrenCommodity_Codes)
            {
                if (!isCommodityCodeExists(currentGroupChildrenCommodity_Code, otherGroupChildrenCommodity_Codes))
                    return false;
            }

            //swap the other way to see if anything is missing from the collection
            foreach (COMMODITY_CODE otherGroupChildrenCommodity_Code in otherGroupChildrenCommodity_Codes)
            {
                if (!isCommodityCodeExists(otherGroupChildrenCommodity_Code, currentGroupChildrenCommodity_Codes))
                    return false;
            }

            return true;
        }

        private bool isCommodityCodeExists(COMMODITY_CODE commodity_code, IEnumerable<COMMODITY_CODE> comparisonContext)
        {
            COMMODITY_CODE contextSameCommodityCode = comparisonContext
            .FirstOrDefault(x => x.FULLCODE == commodity_code.FULLCODE
            && x.RATE_FREIGHT == commodity_code.RATE_FREIGHT
            && x.HOURS_INSTALL == commodity_code.HOURS_INSTALL
            && x.RATE_SUPPLY == commodity_code.RATE_SUPPLY);

            if (contextSameCommodityCode != null)
                return true;
            else
                return false;
        }

        public override void OnAfterAffectingEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender)
        {
            if(changedType == typeof(COMMODITY_CODE))
            {
                Refresh_COMMODITY_GROUP_CODE();
                UpdateCommodityCodes((Guid)key);
            }

            base.OnAfterAffectingEntitiesChanged(key, changedType, messageType, sender);
        }

        private void UpdateCommodityCodes(Guid commodity_code_guid)
        {
            if (MainViewModel == null || MainViewModel.Entities == null)
                return;

            foreach(ESTIMATION_DIRECT_ITEMProjection estimation_direct_item in MainViewModel.Entities.Where(x => x.Entity.GUID_COMMODITY_CODE == commodity_code_guid))
            {
                estimation_direct_item.UpdateCommodityCode(COMMODITY_CODECollection);
            }
        }
        #endregion

        #region Collection Call Backs

        #endregion

        #endregion

        #region View Behavior
        protected override void CellValueAnyRowChanging(CellValueChangedEventArgs e)
        {
            var activeProjection = (ESTIMATION_DIRECT_ITEMProjection)e.Row;
            if (e.Column.FieldName ==
                BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().Entity) + "." +
                BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().GUID_COMMODITY_CODE))
            {
                activeProjection.COMMODITY_CODE = COMMODITY_GROUP_CODECollection.FirstOrDefault(x => x.GUID == (Guid)e.Value);
                e.Handled = true;
            }

            base.CellValueAnyRowChanging(e);
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
                    collection = collection.OrderBy(x => x.INTERNAL_NAME1);
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
                    commodity_group_codeCollection = getGlobal_CommodityGroupCodes();
                    commodity_group_codeCollection.AddRange(getAllCommodityCodes());
                }

                return commodity_group_codeCollection;
            }
        }

        private List<COMMODITY_CODE> getAllCommodityCodes()
        {
            List<COMMODITY_CODE> commodityCodes = new List<COMMODITY_CODE>();
            commodityCodes.AddRange(getCommodityCodes(true));
            commodityCodes.AddRange(getCommodityCodes(false));

            return commodityCodes;
        }

        private List<COMMODITY_CODE> getCommodityCodes(bool isProjectSpecific)
        {
            List<COMMODITY_CODE> commodityCodes = new List<COMMODITY_CODE>();
            if (COMMODITY_CODECollection != null)
            {
                IEnumerable<COMMODITY_CODE> CommodityCodes;
                if (isProjectSpecific)
                    CommodityCodes = COMMODITY_CODECollection.Where(x => x.GUID_PROJECT != null);
                else
                    CommodityCodes = COMMODITY_CODECollection.Where(x => x.ISQUANTIFIABLE).Where(x => x.GUID_PROJECT == null);

                foreach (COMMODITY_CODE commodityCode in CommodityCodes)
                {
                    if (isProjectSpecific || !isProjectCodeSignatureExist(commodityCode))
                    {
                        COMMODITY_CODE tempCommodityCode = new COMMODITY_CODE();
                        DataUtils.ShallowCopy(tempCommodityCode, commodityCode);
                        tempCommodityCode.Temp_Id = tempCommodityCode.GUID.ToString();
                        tempCommodityCode.Temp_Parent_Id = tempCommodityCode.GUID_PARENT.ToString();
                        commodityCodes.Add(tempCommodityCode);
                    }
                }
            }

            return commodityCodes;
        }

        private List<COMMODITY_CODE> getGlobal_CommodityGroupCodes()
        {
            List<COMMODITY_CODE> globalCommodityCodes = new List<COMMODITY_CODE>();
            if (COMMODITY_GROUP_DIRECTCollection != null)
            {
                //get the parents only
                IEnumerable<COMMODITY_GROUP_DIRECT> groupParents = COMMODITY_GROUP_DIRECTCollection.Where(x => x.GUID_PARENT == null);

                //temp id used to identify parents in the view
                int tempParentId = 0;
                if (groupParents != null && COMMODITY_CODECollection != null)
                {
                    foreach (COMMODITY_GROUP_DIRECT groupParent in groupParents)
                    {
                        if (!isProjectCommodityGroupExists(groupParent.GUID))
                        {
                            COMMODITY_CODE tempParentCommodityCode = new COMMODITY_CODE()
                            {
                                GUID = Guid.NewGuid(),
                                GUID_PARENT = Guid.Empty,
                                GUID_COMMODITY_GROUP_DIRECT = groupParent.GUID,
                                COMMODITYCODETYPE = CommodityCodeType.Direct,
                                NAME = groupParent.DESCRIPTION,
                                FULLCODE = groupParent.GROUP_CODE,
                                CODE = groupParent.GROUP_CODE,
                                SortOrder = 0,
                                IsExpanded = false,
                                ISQUANTIFIABLE = false,
                                ISGROUPHEADER = true,
                                RATE_FREIGHT = 0,
                                HOURS_INSTALL = 0,
                                RATE_PLANT = 0,
                                RATE_SUPPLY = 0,
                                Temp_Id = parentPrefix + tempParentId.ToString()
                            };

                            globalCommodityCodes.Add(tempParentCommodityCode);
                        }

                        tempParentId += 1;
                    }
                }
            }

            return globalCommodityCodes;
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