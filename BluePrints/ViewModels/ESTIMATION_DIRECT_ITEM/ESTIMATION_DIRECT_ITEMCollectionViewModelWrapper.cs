using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.View;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Document;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.P6EntitiesDataModel;
using BluePrints.View;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <ESTIMATION_DIRECT_ITEM, ESTIMATION_DIRECT_ITEMProjection, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of ESTIMATION_DIRECT_ITEM_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the PROJECTViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        private PROJECT loadPROJECT;
        private ESTIMATION_DIRECT loadESTIMATION_DIRECT;
        private DEPARTMENT defaultConstructionDEPARTMENT;
        private PHASE defaultConstructionPHASE;
        private bool isQueryForLiveStatus;
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        private IUnitOfWorkFactory<IP6EntitiesUnitOfWork> p6UnitOfWorkFactory =
            P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

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
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS, DEPARTMENTProjectionFunc, x => defaultConstructionDEPARTMENT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PHASES, PHASEProjectionFunc, x => defaultConstructionPHASE = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.ESTIMATION_DIRECTS, ESTIMATION_DIRECTProjectionFunc, x => loadESTIMATION_DIRECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, COMMODITY_CODEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.WORKPACKS, WORKPACKProjectionFunc);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.STOCK_CODES, STOCK_CODEProjectionFunc);
            InvokeEntitiesLoaderDescriptionLoading();
        }

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
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.TYPE == WorkpackType.SiteDirect);
        }

        private Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>> COMMODITY_CODEProjectionFunc()
        {
            return query => query.Include(x => x.PROJECT);
        }

        private Func<IRepositoryQuery<DEPARTMENT>, IQueryable<DEPARTMENT>> DEPARTMENTProjectionFunc()
        {
            return query => query.Where(x => x.NAME == BluePrintsResources.DefaultConstructionDepartment);
        }

        private Func<IRepositoryQuery<PHASE>, IQueryable<PHASE>> PHASEProjectionFunc()
        {
            return query => query.Where(x => x.INTERNAL_NUM == BluePrintsResources.WorkpackDefaultConstructionPhase);
        }

        private Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadESTIMATION_DIRECT.PROJECT.GUID && x.GUID_DEPARTMENT == defaultConstructionDEPARTMENT.GUID && x.COST_GROUP == CostGroup.Site);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadESTIMATION_DIRECT.PROJECT.GUID);
        }

        private Func<IRepositoryQuery<STOCK_CODE>, IQueryable<STOCK_CODE>> STOCK_CODEProjectionFunc()
        {
            return query => query.Where(x => (x.GUID_PROJECT == loadPROJECT.GUID || x.GUID_PROJECT == null));
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.ESTIMATION_DIRECT_ITEMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<ESTIMATION_DIRECT_ITEM>, IQueryable<ESTIMATION_DIRECT_ITEMProjection>>
            ConstructMainViewModelProjection()
        {
            return query => ESTIMATION_DIRECT_ITEMProjectionQueries.ESTIMATION_DIRECT_ITEMProjectionQuery(query, loaderCollection.GetCollection<RATE>(), loaderCollection.GetCollection<COMMODITY_CODE>(), ProjectSTOCK_CODECollection);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<ESTIMATION_DIRECT_ITEMProjection> entities)
        {
            MainViewModel.DisablePasting = true;
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = OnBeforeEntitySaved;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        #region Collection Call Backs
        protected bool ExistingRowAddUndoAndSaveCallBack(ESTIMATION_DIRECT_ITEMProjection projectionEntity, CellValueChangedEventArgs e)
        {
            return true;
        }

        private void createAndAssignProjectSpecificCOMMODITY_CODE(ESTIMATION_DIRECT_ITEMProjection projectionEntity)
        {
            if (projectionEntity.Entity.GUID_COMMODITY_CODE == null)
                return;

            if(!ProjectCOMMODITY_CODECollection.Any(x => x.GUID == projectionEntity.Entity.GUID_COMMODITY_CODE))
            {
                COMMODITY_CODE commodity_code = COMMODITY_CODECollection.FirstOrDefault(x => x.GUID == projectionEntity.Entity.GUID_COMMODITY_CODE);
                if (commodity_code != null)
                {
                    COMMODITY_CODE projectCOMMODITY_CODE = new COMMODITY_CODE();
                    DataUtils.ShallowCopy(projectCOMMODITY_CODE, commodity_code);
                    projectCOMMODITY_CODE.GUID = Guid.Empty;
                    projectCOMMODITY_CODE.GUID_PROJECT = loadPROJECT.GUID;
                    COMMODITY_CODECollectionViewModel.Save(projectCOMMODITY_CODE);
                    projectionEntity.Entity.GUID_COMMODITY_CODE = projectCOMMODITY_CODE.GUID;

                    //commodity code is by value with shallow copy in setter so that user edited changes can be cross check with existing project commodity code
                    projectionEntity.COMMODITY_CODE = projectCOMMODITY_CODE;
                }
                else
                    return;
            }

            return;
        }

        private Guid createNewCOMMODITY_CODE(COMMODITY_CODE fromCommodityCode)
        {
            COMMODITY_CODE newCommodityCode = new COMMODITY_CODE();
            DataUtils.ShallowCopy(newCommodityCode, fromCommodityCode);
            newCommodityCode.GUID = Guid.Empty;
            newCommodityCode.GUID_PROJECT = loadPROJECT.GUID;
            COMMODITY_CODECollectionViewModel.Save(newCommodityCode);
            return newCommodityCode.GUID;
        }

        private void updateCOMMODITY_CODE(COMMODITY_CODE commodityCode)
        {
            if (commodityCode.GUID == Guid.Empty)
                return;

            COMMODITY_CODECollectionViewModel.Save(commodityCode);
        }

        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public bool OnBeforeEntitySaved(ESTIMATION_DIRECT_ITEMProjection entity)
        {
            onBeforeSavedGenerateAndAssignWorkpack(entity);
            onBeforeSavedProjectCommodityCodeLogging(entity);
            entity.Entity.GUID_ESTIMATION_DIRECT = loadESTIMATION_DIRECT.GUID;
            return true;
        }

        private void onBeforeSavedGenerateAndAssignWorkpack(ESTIMATION_DIRECT_ITEMProjection entity)
        {
            if(entity.Entity.GUID_AREA != null && entity.Entity.GUID_DISCIPLINE != null)
            {
                string internalNumber = BluePrintsDataUtils.WORKPACK_Generate_InternalNumber2(entity.Entity.GUID_AREA, entity.Entity.GUID_SUBAREA, loadPROJECT, AREACollection, SUBAREACollection);
                if(internalNumber != string.Empty)
                {
                    WORKPACK existingWORKPACK = WORKPACKCollection.FirstOrDefault(x => x.INTERNAL_NAME1 == internalNumber);
                    if (existingWORKPACK == null)
                    {
                        var newWORKPACK = new WORKPACK();
                        newWORKPACK.GUID_PROJECT = loadPROJECT.GUID;
                        AREA defaultSubArea = SUBAREACollection.FirstOrDefault(x => x.INTERNAL_NUM == BluePrintsResources.WorkpackDefaultSubArea);

                        newWORKPACK.GUID_DAREA = entity.Entity.GUID_AREA;
                        newWORKPACK.GUID_DSUBAREA = entity.Entity.GUID_SUBAREA == null ? defaultSubArea == null ? (Guid?)null : defaultSubArea.GUID : entity.Entity.GUID_SUBAREA;
                        newWORKPACK.GUID_DPHASE = defaultConstructionPHASE.GUID;
                        newWORKPACK.GUID_DDEPARTMENT = defaultConstructionDEPARTMENT.GUID;
                        newWORKPACK.GUID_DDISCIPLINE = (Guid)entity.Entity.GUID_DISCIPLINE;
                        newWORKPACK.INTERNAL_NAME1 = internalNumber;
                        newWORKPACK.STARTDATE = DateTime.Now;
                        newWORKPACK.ENDDATE =
                            BluePrintsDataUtils.WORKPACK_Calculate_EndDate((DateTime)newWORKPACK.STARTDATE, loadPROJECT);
                        var reviewStartDate = (DateTime)newWORKPACK.STARTDATE;
                        var reviewEndDate = (DateTime)newWORKPACK.ENDDATE;
                        BluePrintsDataUtils.WORKPACK_Calculate_ReviewPeriod(ref reviewStartDate, ref reviewEndDate,
                            loadPROJECT, false);
                        newWORKPACK.REVIEWSTARTDATE = reviewStartDate;
                        newWORKPACK.REVIEWENDDATE = reviewEndDate;
                        newWORKPACK.AUTOGENERATED = true;
                        newWORKPACK.TYPE = WorkpackType.SiteDirect;
                        WORKPACKSCollectionViewModel.Save(newWORKPACK);
                        entity.Entity.GUID_WORKPACK = newWORKPACK.GUID;
                    }
                    else
                        entity.Entity.GUID_WORKPACK = existingWORKPACK.GUID;
                }
            }
        }

        private void onBeforeSavedProjectCommodityCodeLogging(ESTIMATION_DIRECT_ITEMProjection entity)
        {
            COMMODITY_CODE projectCommodityCode;
            projectCommodityCodeStatus commodityCodeStatus = getProjectCommodityCodeStatus(entity.COMMODITY_CODE, out projectCommodityCode);
            if (commodityCodeStatus == projectCommodityCodeStatus.IsEmpty)
                return;
            else if (commodityCodeStatus == projectCommodityCodeStatus.DontExists)
                createAndAssignProjectSpecificCOMMODITY_CODE(entity);
            else if (commodityCodeStatus == projectCommodityCodeStatus.MetaExistsOnDifferentRecord)
            {
                entity.Entity.GUID_COMMODITY_CODE = projectCommodityCode.GUID;
                entity.COMMODITY_CODE = projectCommodityCode;
            }
            else if (commodityCodeStatus == projectCommodityCodeStatus.ExistsWithDifferentRateHours)
            {
                UICommand addCommand = new UICommand()
                {
                    Id = DialogAction.Add,
                    Caption = "Add",
                    IsCancel = true,
                    IsDefault = false,
                };

                UICommand editCommand = new UICommand()
                {
                    Id = DialogAction.Edit,
                    Caption = "Update",
                    IsCancel = true,
                    IsDefault = false,
                };

                UICommand cancelCommand = new UICommand()
                {
                    Id = DialogAction.Cancel,
                    Caption = "Cancel",
                    IsCancel = true,
                    IsDefault = false,
                };

                string message = String.Format("Current commodity code with\nSupply Rate: {0:#} Install Hours: {0:#}\n" +
                    "Is changed to\nSupply Rate: {0:#} Install Hours: {0:#}\n" +
                    "Do you wish to add new or update?", projectCommodityCode.RATE_SUPPLY, projectCommodityCode.HOURS_INSTALL, entity.COMMODITY_CODE.RATE_SUPPLY, entity.COMMODITY_CODE.HOURS_INSTALL);

                BasicMessageBoxViewModel viewModel = BasicMessageBoxViewModel.Create(message);
                UICommand result = CommodityCodeDialogService.ShowDialog(new List<UICommand>() { addCommand, editCommand, cancelCommand }, "Commodity Code", "BasicMessageBox", viewModel);
                if (result == addCommand)
                {
                    Guid newCommodityCodeGuid = createNewCOMMODITY_CODE(entity.COMMODITY_CODE);
                    entity.Entity.GUID_COMMODITY_CODE = newCommodityCodeGuid;
                }
                else if (result == editCommand)
                    updateCOMMODITY_CODE(entity.COMMODITY_CODE);
            }
        }
        #endregion
        #endregion

        #region View Behavior
        #region Duplicate Behavior
        private bool _isProcessingMultiple;
        public bool CanDuplicateMultiple(BarEditItem barEdit)
        {
            if (MainViewModel == null || MainViewModel.SelectedEntities.Count == 0)
                return false;

            return true;
        }

        public void DuplicateMultiple(BarEditItem barEdit)
        {
            MainViewModel.EntitiesUndoRedoManager.PauseActionId();
            _isProcessingMultiple = true;
            var timesToDuplicate = 0;
            List<ESTIMATION_DIRECT_ITEMProjection> newEntities = new List<ESTIMATION_DIRECT_ITEMProjection>();
            if (int.TryParse(barEdit.EditValue.ToString(), out timesToDuplicate))
            {
                List<ESTIMATION_DIRECT_ITEMProjection> currentEnumerationSaveEntities = getNewEntities(timesToDuplicate, false);
                newEntities.AddRange(currentEnumerationSaveEntities);
            }

            MainViewModel.BulkSave(newEntities);
            _isProcessingMultiple = false;
            MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
        }

        public bool CanDuplicate()
        {
            if (MainViewModel == null || MainViewModel.SelectedEntities.Count == 0)
                return false;

            return true;
        }

        public void Duplicate()
        {
            if (!_isProcessingMultiple)
                MainViewModel.EntitiesUndoRedoManager.PauseActionId();

            List<ESTIMATION_DIRECT_ITEMProjection> newEntities = getNewEntities(1, false);
            MainViewModel.BulkSave(newEntities);
            if (!_isProcessingMultiple)
                MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
        }

        List<ESTIMATION_DIRECT_ITEMProjection> getNewEntities(int timesToDuplicate, bool isInsert)
        {
            List<ESTIMATION_DIRECT_ITEMProjection> unsavedEntities = new List<ESTIMATION_DIRECT_ITEMProjection>();
            for (int i = 0; i < timesToDuplicate; i++)
            {
                foreach (var selectedEntity in MainViewModel.SelectedEntities)
                {
                    var newProjection = new ESTIMATION_DIRECT_ITEMProjection();
                    DataUtils.ShallowCopy(newProjection.Entity, selectedEntity.Entity);
                    newProjection.Entity.EntityKey = Guid.Empty;
                    newProjection.Entity.GUID_ORIGINAL = Guid.Empty;
                    //newProjection.Entity.ESTIMATED_QUANTITY = IsBASELINELocked ? 0 : selectedEntity.Entity.ESTIMATED_QUANTITY;

                    MainViewModel.EntitiesUndoRedoManager.AddUndo(newProjection, null, null, null, EntityMessageType.Added);
                    unsavedEntities.Add(newProjection);
                }
            }

            return unsavedEntities;
        }
        #endregion

        /// <summary>
        /// Remove redundant project commodity codes when view is closed
        /// </summary>
        protected override void OnClose(CancelEventArgs e)
        {
            if(COMMODITY_CODECollectionViewModel != null)
            {
                List<COMMODITY_CODE> removeCommodityCodes = new List<COMMODITY_CODE>();
                foreach (COMMODITY_CODE projectCommodityCode in ProjectCOMMODITY_CODECollection)
                {
                    if (!MainViewModel.Entities.Any(x => x.Entity.GUID_COMMODITY_CODE == projectCommodityCode.GUID))
                        removeCommodityCodes.Add(projectCommodityCode);
                }
                COMMODITY_CODECollectionViewModel.BaseBulkDelete(removeCommodityCodes);
            }

            base.OnClose(e);
        }

        protected override bool IsSingleMainEntityRefreshIdentified(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
            if(changedType == typeof(COMMODITY_CODE))
            {
                foreach(var entities in MainViewModel.Entities)
                {
                    entities.Update();
                }
                return false;
            }

            return true;
        }

        protected override void CellValueAnyRowChanging(CellValueChangedEventArgs e)
        {
            //var activeESTIMATION_DIRECT_ITEM = (ESTIMATION_DIRECT_ITEMProjection)e.Row;
            //if (e.Column.FieldName ==
            //     BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().Entity) + "." +
            //     BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().GUID_COMMODITY_CODE))
            //{
            //    if (e.Value != null)
            //    {
            //        //Commodity code is required immediately for price/hours
            //        activeESTIMATION_DIRECT_ITEM.COMMODITY_CODE = COMMODITY_CODECollection.FirstOrDefault(x => x.GUID == (Guid)e.Value);
            //        activeESTIMATION_DIRECT_ITEM.Update();
            //    }
            //}

            base.CellValueAnyRowChanging(e);
        }

        /// <summary>
        /// Allow undo-redo behavior to be added for automated cell value changing. This behavior doesn't have to be applied on new row because AddUndo for EntityMessageType.Added is already handling this
        /// </summary>
        protected override void CellValueExistingRowChanging(CellValueChangedEventArgs e)
        {
            var activeESTIMATION_DIRECT_ITEM = (ESTIMATION_DIRECT_ITEMProjection)e.Row;

            if (e.Column.FieldName ==
                 BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().Entity) + "." +
                 BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().GUID_AREA))
            {
                resetProjectionSubArea(activeESTIMATION_DIRECT_ITEM);
                resetProjectionStockCode(activeESTIMATION_DIRECT_ITEM);
            }
            else if (e.Column.FieldName ==
                 BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().Entity) + "." +
                 BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().GUID_SUBAREA))
            {
                resetProjectionStockCode(activeESTIMATION_DIRECT_ITEM);
            }
            else if (e.Column.FieldName ==
                 BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().Entity) + "." +
                 BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().GUID_DISCIPLINE))
            {
                resetProjectionCommodityCode(activeESTIMATION_DIRECT_ITEM);
                resetProjectionStockCode(activeESTIMATION_DIRECT_ITEM);
            }

            base.CellValueExistingRowChanging(e);
        }

        private void setProjectionCommodityCode(ESTIMATION_DIRECT_ITEMProjection projection, Guid? commoditycodeGuid)
        {
            if (commoditycodeGuid != null)
                projection.COMMODITY_CODE = COMMODITY_CODECollection.FirstOrDefault(x => x.GUID == (Guid)commoditycodeGuid);
            else
                projection.COMMODITY_CODE = null;

            projection.Update();
        }

        private void updateProjectionCommodityCodeCollection(ESTIMATION_DIRECT_ITEMProjection projection, Guid? disciplineGuid)
        {
            if (disciplineGuid != null)
                //commodity code collection must be updated by discipline filter
                projection.CommodityCodeCollection = COMMODITY_CODECollection.Where(x => x.GUID_DISCIPLINE == disciplineGuid);
            else
                projection.CommodityCodeCollection = new List<COMMODITY_CODE>();

            projection.Update();
        }

        private void updateProjectionStockCodeCollection(ESTIMATION_DIRECT_ITEMProjection projection, Guid? areaGuid, Guid? subAreaGuid, Guid? disciplineGuid)
        {
            if (areaGuid != null && disciplineGuid != null)
                projection.StockCodeCollection = STOCK_CODECollection
                                                .Where(x => x.GUID_AREA == areaGuid && x.GUID_SUBAREA == subAreaGuid && x.GUID_DISCIPLINE == (Guid)disciplineGuid);
            else
                projection.StockCodeCollection = new List<STOCK_CODE>();

            projection.Update();
        }

        private void resetProjectionSubArea(ESTIMATION_DIRECT_ITEMProjection projection)
        {
            Guid? oldValue = projection.Entity.GUID_SUBAREA;
            if (oldValue != null)
            {
                Guid? newValue = (Guid?)null;
                string subAreaFieldName = BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().Entity) + "." +
                BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().SubAreaGuid);
                projection.Entity.GUID_SUBAREA = newValue;
                MainViewModel.EntitiesUndoRedoManager.PauseActionId();
                MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, subAreaFieldName, oldValue, newValue, EntityMessageType.Changed);
            }
        }

        private void resetProjectionCommodityCode(ESTIMATION_DIRECT_ITEMProjection projection)
        {
            Guid? oldValue = projection.CommodityCodeGuid;
            if (oldValue != null)
            {
                Guid? newValue = (Guid?)null;
                string commoditycodeFieldName = BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().Entity) + "." +
                BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().GUID_COMMODITY_CODE);
                projection.Entity.GUID_COMMODITY_CODE = newValue;
                MainViewModel.EntitiesUndoRedoManager.PauseActionId();
                MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, commoditycodeFieldName, oldValue, newValue, EntityMessageType.Changed);
            }
        }

        private void resetProjectionStockCode(ESTIMATION_DIRECT_ITEMProjection projection)
        {
            Guid? oldValue = projection.StockCodeGuid;
            if (oldValue != null)
            {
                Guid? newValue = (Guid?)null;
                string stockcodeFieldName = BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().Entity) + "." +
                BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().GUID_STOCK_CODE);
                projection.Entity.GUID_COMMODITY_CODE = newValue;
                MainViewModel.EntitiesUndoRedoManager.PauseActionId();
                MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, stockcodeFieldName, oldValue, newValue, EntityMessageType.Changed);
            }
        }

        protected override void CellValueNewRowChanging(CellValueChangedEventArgs e)
        {
            var activeESTIMATION_DIRECT_ITEM = (ESTIMATION_DIRECT_ITEMProjection)e.Row;
            if (e.Column.FieldName ==
                 BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().Entity) + "." +
                 BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().GUID_AREA))
            {
                if (e.Value != null)
                {
                    activeESTIMATION_DIRECT_ITEM.Entity.GUID_AREA = (Guid)e.Value;
                    //Area is required immediately for subarea selection
                    activeESTIMATION_DIRECT_ITEM.Entity.AREA = AREACollection.FirstOrDefault(x => x.GUID == (Guid)e.Value);
                    activeESTIMATION_DIRECT_ITEM.Update();
                }

                //SubArea must be removed immediately to nullify subarea selection
                if (activeESTIMATION_DIRECT_ITEM.Entity.GUID_SUBAREA != null)
                {
                    activeESTIMATION_DIRECT_ITEM.Entity.GUID_SUBAREA = null;
                    activeESTIMATION_DIRECT_ITEM.Update();
                }

                updateProjectionStockCodeCollection(activeESTIMATION_DIRECT_ITEM, (Guid?)e.Value, activeESTIMATION_DIRECT_ITEM.Entity.SubAreaGuid, activeESTIMATION_DIRECT_ITEM.Entity.GUID_DISCIPLINE);
            }
            else if (e.Column.FieldName ==
                 BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().Entity) + "." +
                 BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().SubAreaGuid))
            {
                updateProjectionStockCodeCollection(activeESTIMATION_DIRECT_ITEM, activeESTIMATION_DIRECT_ITEM.Entity.GUID_AREA, (Guid?)e.Value, activeESTIMATION_DIRECT_ITEM.Entity.GUID_DISCIPLINE);
            }
            else if (e.Column.FieldName ==
                 BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().Entity) + "." +
                 BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().GUID_DISCIPLINE))
            {
                updateProjectionStockCodeCollection(activeESTIMATION_DIRECT_ITEM, activeESTIMATION_DIRECT_ITEM.Entity.GUID_AREA, activeESTIMATION_DIRECT_ITEM.Entity.GUID_SUBAREA, (Guid?)e.Value);
                updateProjectionCommodityCodeCollection(activeESTIMATION_DIRECT_ITEM, (Guid?)e.Value);
            }
            else if (e.Column.FieldName ==
                 BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().Entity) + "." +
                 BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().GUID_COMMODITY_CODE))
            {
                setProjectionCommodityCode(activeESTIMATION_DIRECT_ITEM, (Guid?)e.Value);
            }
            
            base.CellValueNewRowChanging(e);
        }
        #endregion

        #region Commodity Code Helpers
        private enum projectCommodityCodeStatus
        {
            IsEmpty,
            DontExists,
            ExistsWithDifferentRateHours,
            MetaExistsOnDifferentRecord, 
            Exists
        }

        private projectCommodityCodeStatus getProjectCommodityCodeStatus(COMMODITY_CODE commodity_code, out COMMODITY_CODE projectCommodityCode)
        {
            projectCommodityCode = null;
            if (commodity_code == null)
                return projectCommodityCodeStatus.IsEmpty;

            projectCommodityCode = ProjectCOMMODITY_CODECollection.FirstOrDefault(x => x.GUID == commodity_code.GUID);
            bool isExists = false;
            if (projectCommodityCode != null)
                isExists = true;

            if (isExists && projectCommodityCode.RATE_SUPPLY == commodity_code.RATE_SUPPLY && projectCommodityCode.HOURS_INSTALL == commodity_code.HOURS_INSTALL)
                return projectCommodityCodeStatus.Exists;

            //look for other project commodity code with same meta
            COMMODITY_CODE sameMetaCommodityCode = ProjectCOMMODITY_CODECollection.FirstOrDefault(x => x.RATE_SUPPLY == commodity_code.RATE_SUPPLY && x.HOURS_INSTALL == commodity_code.HOURS_INSTALL);
            if (isExists && sameMetaCommodityCode == null)
                return projectCommodityCodeStatus.ExistsWithDifferentRateHours;

            if (sameMetaCommodityCode != null)
            {
                projectCommodityCode = sameMetaCommodityCode;
                return projectCommodityCodeStatus.MetaExistsOnDifferentRecord;
            }

            return projectCommodityCodeStatus.DontExists;
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

        private DevExpress.Mvvm.IDialogService CommodityCodeDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("CommodityCodeDialogService"); }
        }

        public IEnumerable<PHASE> PHASECollection
        {
            get
            {
                var collection = GetEntities<PHASE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.INTERNAL_NUM);
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

        public IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection
        {
            get
            {
                var collection = GetEntities<COMMODITY_CODE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.CODE);
                return collection;
            }
        }

        public IEnumerable<COMMODITY_CODE> ProjectCOMMODITY_CODECollection
        {
            get
            {
                if (loadPROJECT == null)
                    return null;

                return COMMODITY_CODECollection.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
            }
        }

        public IEnumerable<STOCK_CODE> STOCK_CODECollection
        {
            get
            {
                var collection = GetEntities<STOCK_CODE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.CODE);
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

        public CollectionViewModel<WORKPACK, WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork> WORKPACKSCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<WORKPACK, WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<WORKPACK>();
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