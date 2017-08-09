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
using BluePrints.Common.ViewModel.Reporting;
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
        <ESTIMATION_DIRECT_ITEM, ESTIMATION_DIRECT_ITEMProgress, Guid, IBluePrintsEntitiesUnitOfWork>, IDeliverableCollectionViewModelWrapper<ESTIMATION_DIRECT_ITEMProgress, ESTIMATION_DIRECT_ITEM>
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
        private PROGRESS livePROGRESS;
        private ESTIMATION_DIRECT loadESTIMATION_DIRECT;
        public Guid load_context_guid => loadESTIMATION_DIRECT == null ? Guid.Empty : loadESTIMATION_DIRECT.GUID;
        private PHASE defaultConstructionPHASE;
        private bool isQueryForLiveStatus;
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        public string Base_Entity_String => "Entity.Entity.";
        public string Projection_Entity_String => "Entity.";

        public string DefaultPhaseInternalNumber { get; set; }
        public Func<ESTIMATION_DIRECT_ITEMProgress> SelectedEntityCallBack { get; set; }
        public ESTIMATION_DIRECT_ITEMProgress SelectedEntity { get => SelectedEntityCallBack != null ? SelectedEntityCallBack.Invoke() : DisplaySelectedEntity; }
        public IEnumerable<ESTIMATION_DIRECT_ITEMProgress> SelectedEntities { get; set; }
        public IEnumerable<ESTIMATION_DIRECT_ITEMProgress> EditableAllEntities { get; set; }

        private IUnitOfWorkFactory<IP6EntitiesUnitOfWork> p6UnitOfWorkFactory =
            P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void resolveParameters(object parameter)
        {
            Interface_InitializeParameters(parameter);
        }

        public void Interface_InitializeParameters(object parameter)
        {
            var receiveParameter = (DualEntitiesParameter<PROJECT, IAmBaseline>)parameter;
            loadPROJECT = receiveParameter.GetFirstEntity();
            loadESTIMATION_DIRECT = (ESTIMATION_DIRECT)receiveParameter.GetSecondEntity();

            if (loadPROJECT != null)
                isQueryForLiveStatus = true;
        }

        protected override void initializeEntitiesLoadersDescription()
        {
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PHASES, PHASEProjectionFunc, x => defaultConstructionPHASE = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.ESTIMATION_DIRECTS, ESTIMATION_DIRECTProjectionFunc, x => assign_estimation_direct(x));
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc, x => assign_progress(x));
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESS_ITEMS, PROGRESS_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.STOCK_CODES, STOCK_CODEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.STOCK_GROUPS, STOCK_GROUPProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, COMMODITY_CODEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.WORKPACKS, WORKPACKProjectionFunc);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
        }

        private void assign_estimation_direct(ESTIMATION_DIRECT estimation_direct)
        {
            if (estimation_direct == null && !SupressCompulsoryEntityNotFoundMessage)
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage("Live estimate not found")));

            loadESTIMATION_DIRECT = estimation_direct;
        }

        private void assign_progress(PROGRESS progress)
        {
            if (progress == null && !SupressCompulsoryEntityNotFoundMessage)
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage("Live progress not found")));

            livePROGRESS = progress;
        }

        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            if (isQueryForLiveStatus)
                return query => query.Where(x => x.GUID == loadPROJECT.GUID);
            else
                return query => query.Where(x => x.GUID == loadESTIMATION_DIRECT.GUID_PROJECT);
        }

        private Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> PROGRESSProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.TYPE == ProgressType.Construct && x.STATUS == ProgressStatus.Live);
        }

        private Func<IRepositoryQuery<PROGRESS_ITEM>, IQueryable<PROGRESS_ITEM>> PROGRESS_ITEMProjectionFunc()
        {
            if (livePROGRESS == null)
                return query => query.Where(x => x.GUID_PROGRESS == Guid.Empty);
            else
                return query => query.Where(x => x.GUID_PROGRESS == livePROGRESS.GUID);
        }

        private Func<IRepositoryQuery<ESTIMATION_DIRECT>, IQueryable<ESTIMATION_DIRECT>> ESTIMATION_DIRECTProjectionFunc()
        {
            if (isQueryForLiveStatus)
                return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == BaselineStatus.Live);
            else
                return query => query.Where(x => x.GUID == loadESTIMATION_DIRECT.GUID);
        }

        private Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK>> WORKPACKProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.TYPE == WorkpackType.SiteDirect);
        }

        private Func<IRepositoryQuery<STOCK_CODE>, IQueryable<STOCK_CODE>> STOCK_CODEProjectionFunc()
        {
            return query => query.Include(x => x.PROJECT);
        }

        private Func<IRepositoryQuery<PHASE>, IQueryable<PHASE>> PHASEProjectionFunc()
        {
            return query => query.Where(x => x.INTERNAL_NUM.ToUpper() == BluePrintsResources.Default_Construction_Phase);
        }

        private Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadESTIMATION_DIRECT.PROJECT.GUID && x.COST_GROUP == CostGroup.Site);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadESTIMATION_DIRECT.PROJECT.GUID);
        }

        private Func<IRepositoryQuery<STOCK_GROUP>, IQueryable<STOCK_GROUP>> STOCK_GROUPProjectionFunc()
        {
            return query => query.Where(x => (x.GUID_PROJECT == loadPROJECT.GUID || x.GUID_PROJECT == null));
        }

        private Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>> COMMODITY_CODEProjectionFunc()
        {
            return query => query.Where(x => (x.GUID_PROJECT == loadPROJECT.GUID || x.GUID_PROJECT == null));
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.ESTIMATION_DIRECT_ITEMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<ESTIMATION_DIRECT_ITEM>, IQueryable<ESTIMATION_DIRECT_ITEMProgress>>
            specifyMainViewModelProjection()
        {
            return query => ESTIMATION_DIRECT_ITEMProjectionQueries.IDeliverable_Progress_Transformation(base_entity_query(query), loadPROJECT, loaderCollection.GetCollection<RATE>(), livePROGRESS, PROGRESS_ITEMCollection, STOCK_CODECollection, loaderCollection.GetCollection<STOCK_GROUP>());
        }

        public Func<IRepositoryQuery<ESTIMATION_DIRECT_ITEM>, IQueryable<ESTIMATION_DIRECT_ITEM>> BaseEntityQueryCallBack { get; set; }
        private IQueryable<ESTIMATION_DIRECT_ITEM> base_entity_query(IRepositoryQuery<ESTIMATION_DIRECT_ITEM> query)
        {
            if (BaseEntityQueryCallBack != null)
                return BaseEntityQueryCallBack(query);

            return query.Where(x => x.GUID_ESTIMATION_DIRECT == load_context_guid);
        }

        public Action<ESTIMATION_DIRECT_ITEMProgress, string, object, object, EntityMessageType> InterfaceAddUndoRedoCallBack { get; set; }
        public void AddUndo(ESTIMATION_DIRECT_ITEMProgress changedEntity, string propertyName, object oldValue, object newValue, EntityMessageType messageType)
        {
            if (InterfaceAddUndoRedoCallBack != null)
                InterfaceAddUndoRedoCallBack(changedEntity, propertyName, oldValue, newValue, messageType);
            else
            {
                if (propertyName == null)
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(changedEntity, null, null, null, messageType);
                else
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(changedEntity, localizeColumnFieldName(propertyName), oldValue, newValue, messageType);
            }
        }

        public Action InterfacePauseUndoRedoCallBack { get; set; }
        public void PauseUndoRedo()
        {
            if (InterfacePauseUndoRedoCallBack != null)
                InterfacePauseUndoRedoCallBack();
            else
                MainViewModel.EntitiesUndoRedoManager.PauseActionId();
        }

        public Action InterfaceUnpauseUndoRedoCallBack { get; set; }
        public void UnpauseUndoRedo()
        {
            if (InterfaceUnpauseUndoRedoCallBack != null)
                InterfaceUnpauseUndoRedoCallBack();
            else
                MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
        }

        private string localizeColumnFieldName(string fieldName)
        {
            return Base_Entity_String + DataUtils.FormatColumnFieldname(fieldName);
        }

        public Action<IEnumerable<ESTIMATION_DIRECT_ITEMProgress>> OnReportablesLoadedCallBack { get; set; }
        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<ESTIMATION_DIRECT_ITEMProgress> entities)
        {
            MainViewModel.DisablePasting = true;
            MainViewModel.ApplyEntityPropertiesToProjectionCallBack = OnEntitiesSavedCallBack;
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = OnBeforeEntitySaved;
            MainViewModel.AdditionalValidateCellCallBack = AdditionalValidateCellCallBack;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);

            //used for interface when this is loaded from variation
            if (OnReportablesLoadedCallBack != null)
            {
                OnReportablesLoadedCallBack(entities);
                return;
            }

            SetViewSpecificProperties();
        }

        /// <summary>
        /// this view model can be used in variation or default collection view, only default collection view specific properties are set here
        /// </summary>
        private void SetViewSpecificProperties()
        {
            SelectedEntities = DisplaySelectedEntities;
            EditableAllEntities = MainViewModel.Entities;
            DefaultPhaseInternalNumber = BluePrintsResources.Default_Construction_Phase;
        }

        public Action<ESTIMATION_DIRECT_ITEMProgress> ApplyViewSpecificPropertiesToEntityCallBack { get; set; }
        protected override void OnBeforeApplyProjectionPropertiesToEntity(ESTIMATION_DIRECT_ITEMProgress projectionEntity, ESTIMATION_DIRECT_ITEM entity)
        {
            if (ApplyViewSpecificPropertiesToEntityCallBack == null)
                projectionEntity.Entity.Entity.GUID_ESTIMATION_DIRECT = load_context_guid;
            else
                ApplyViewSpecificPropertiesToEntityCallBack.Invoke(projectionEntity);

            //because TProjection is not IProjection<TMainEntity>, do it manually here
            DataUtils.ShallowCopy(entity, projectionEntity.Entity.Entity);
            base.OnBeforeApplyProjectionPropertiesToEntity(projectionEntity, entity);
        }

        #region Collection Call Backs
        protected bool ExistingRowAddUndoAndSaveCallBack(ESTIMATION_DIRECT_ITEMProgress projectionEntity, CellValueChangedEventArgs e)
        {
            return true;
        }

        private void createAndAssignProjectSpecificSTOCK_CODE(ESTIMATION_DIRECT_ITEMProgress projectionEntity)
        {
            if (projectionEntity.Entity.Entity.GUID_STOCK_CODE == null)
                return;

            if(!ProjectSTOCK_CODECollection.Any(x => x.GUID == projectionEntity.Entity.Entity.GUID_STOCK_CODE))
            {
                STOCK_CODE stock_code = STOCK_CODECollection.FirstOrDefault(x => x.GUID == projectionEntity.Entity.Entity.GUID_STOCK_CODE);
                if (stock_code != null)
                {
                    STOCK_CODE projectSTOCK_CODE = new STOCK_CODE();
                    DataUtils.ShallowCopy(projectSTOCK_CODE, stock_code);
                    projectSTOCK_CODE.GUID = Guid.Empty;
                    projectSTOCK_CODE.GUID_PROJECT = loadPROJECT.GUID;
                    STOCK_CODECollectionViewModel.Save(projectSTOCK_CODE);
                    projectionEntity.Entity.Entity.GUID_STOCK_CODE = projectSTOCK_CODE.GUID;

                    //stock group is by value with shallow copy in setter so that user edited changes can be cross check with existing project stock group
                    projectionEntity.Entity.STOCK_CODE = projectSTOCK_CODE;
                }
                else
                    return;
            }

            return;
        }

        private Guid createNewSTOCK_CODE(STOCK_CODE fromStockCode)
        {
            STOCK_CODE newStockCode = new STOCK_CODE();
            DataUtils.ShallowCopy(newStockCode, fromStockCode);
            newStockCode.GUID = Guid.Empty;
            newStockCode.GUID_PROJECT = loadPROJECT.GUID;
            newStockCode.GUID_ORIGINAL = fromStockCode.GUID_ORIGINAL;
            STOCK_CODECollectionViewModel.Save(newStockCode);
            return newStockCode.GUID;
        }

        private void updateSTOCK_CODE(STOCK_CODE stock_code)
        {
            if (stock_code.GUID == Guid.Empty)
                return;

            STOCK_CODECollectionViewModel.Save(stock_code);
        }

        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public bool OnBeforeEntitySaved(ESTIMATION_DIRECT_ITEMProgress entity)
        {
            onBeforeSavedGenerateAndAssignWorkpack(entity);
            onBeforeSavedProjectStockCodeLogging(entity);

            //entity.Entity.Entity.GUID_ESTIMATION_DIRECT = loadESTIMATION_DIRECT.GUID;
            return true;
        }

        public void OnEntitiesSavedCallBack(Guid primaryKey, ESTIMATION_DIRECT_ITEMProgress projectionEntity, ESTIMATION_DIRECT_ITEM entity, bool isNewEntity)
        {
            projectionEntity.Entity.Entity.GUID_ORIGINAL = entity.GUID_ORIGINAL;
        }

        private void onBeforeSavedGenerateAndAssignWorkpack(ESTIMATION_DIRECT_ITEMProgress entity)
        {
            if(entity.Entity.Entity.GUID_AREA != null && entity.Entity.Entity.GUID_DISCIPLINE != null)
            {
                string internalNumber = BluePrintsDataUtils.WORKPACK_Generate_InternalNumber(entity.Entity.Entity.GUID_AREA, entity.Entity.Entity.GUID_SUBAREA, loadPROJECT, AREACollection, SUBAREACollection);
                if(internalNumber != string.Empty)
                {
                    WORKPACK existingWORKPACK = WORKPACKCollection.FirstOrDefault(x => x.INTERNAL_NAME1 == internalNumber);
                    if (existingWORKPACK == null)
                    {
                        var newWORKPACK = new WORKPACK();
                        newWORKPACK.GUID_PROJECT = loadPROJECT.GUID;
                        AREA defaultSubArea = SUBAREACollection.FirstOrDefault(x => x.INTERNAL_NUM == BluePrintsResources.Default_Sub_Area);

                        newWORKPACK.GUID_DAREA = entity.Entity.Entity.GUID_AREA;
                        newWORKPACK.GUID_DSUBAREA = entity.Entity.Entity.GUID_SUBAREA == null ? defaultSubArea == null ? (Guid?)null : defaultSubArea.GUID : entity.Entity.Entity.GUID_SUBAREA;
                        newWORKPACK.GUID_DPHASE = defaultConstructionPHASE.GUID;
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
                        entity.Entity.Entity.GUID_WORKPACK = newWORKPACK.GUID;
                    }
                    else
                        entity.Entity.Entity.GUID_WORKPACK = existingWORKPACK.GUID;
                }
            }
        }

        private void onBeforeSavedProjectStockCodeLogging(ESTIMATION_DIRECT_ITEMProgress entity)
        {
            if(entity.Entity.STOCK_CODE != null)
            {
                STOCK_CODE projectStockCode;
                projectStock_CodeStatus commodityCodeStatus = getProjectStockCodeStatus(entity.Entity.STOCK_CODE, out projectStockCode);
                if (commodityCodeStatus == projectStock_CodeStatus.IsEmpty)
                    return;
                else if (commodityCodeStatus == projectStock_CodeStatus.DontExists)
                    createAndAssignProjectSpecificSTOCK_CODE(entity);
                else if (commodityCodeStatus == projectStock_CodeStatus.MetaExistsOnDifferentRecord)
                {
                    entity.Entity.Entity.GUID_STOCK_CODE = projectStockCode.GUID;
                    entity.Entity.STOCK_CODE = projectStockCode;
                }
                else if (commodityCodeStatus == projectStock_CodeStatus.ExistsWithDifferentRateHours)
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

                    string message = String.Format("Current stock code with\nSupply Rate: {0:#} Install Hours: {1:#}\n" +
                        "Is changed to\nSupply Rate: {2:#} Install Hours: {3:#}\n" +
                        "Do you wish to add new or update?", projectStockCode.RATE_SUPPLY, projectStockCode.HOURS_INSTALL, entity.Entity.STOCK_CODE.RATE_SUPPLY, entity.Entity.STOCK_CODE.HOURS_INSTALL);

                    BasicMessageBoxViewModel viewModel = BasicMessageBoxViewModel.Create(message);
                    UICommand result = StockCodeDialogService.ShowDialog(new List<UICommand>() { addCommand, editCommand, cancelCommand }, "Stock Code", "BasicMessageBox", viewModel);
                    if (result == addCommand)
                    {
                        Guid newStockCodeGuid = createNewSTOCK_CODE(entity.Entity.STOCK_CODE);
                        entity.Entity.Entity.GUID_STOCK_CODE = newStockCodeGuid;
                    }
                    else if (result == editCommand)
                        updateSTOCK_CODE(entity.Entity.STOCK_CODE);
                }
                else if (commodityCodeStatus == projectStock_CodeStatus.Exists)
                    updateSTOCK_CODE(entity.Entity.STOCK_CODE);
            }
        }
        #endregion
        #endregion

        #region View Behavior
        #region Duplicate Behavior
        private bool _isProcessingMultiple;
        public bool CanDuplicateMultiple(BarEditItem barEdit)
        {
            if (MainViewModel == null || SelectedEntities.Count() == 0)
                return false;

            return true;
        }

        public void DuplicateMultiple(BarEditItem barEdit)
        {
            PauseUndoRedo();
            _isProcessingMultiple = true;
            var timesToDuplicate = 0;
            List<ESTIMATION_DIRECT_ITEMProgress> newEntities = new List<ESTIMATION_DIRECT_ITEMProgress>();
            if (int.TryParse(barEdit.EditValue.ToString(), out timesToDuplicate))
            {
                List<ESTIMATION_DIRECT_ITEMProgress> currentEnumerationSaveEntities = getNewEntities(timesToDuplicate, false);
                newEntities.AddRange(currentEnumerationSaveEntities);
            }

            MainViewModel.BulkSave(newEntities);
            _isProcessingMultiple = false;
            MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
        }

        public bool CanDuplicate()
        {
            if (MainViewModel == null || SelectedEntities == null || SelectedEntities.Count() == 0)
                return false;

            return true;
        }

        public void Duplicate()
        {
            if (!_isProcessingMultiple)
                PauseUndoRedo();

            List<ESTIMATION_DIRECT_ITEMProgress> newEntities = getNewEntities(1, false);
            MainViewModel.BulkSave(newEntities);
            if (!_isProcessingMultiple)
                MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
        }

        List<ESTIMATION_DIRECT_ITEMProgress> getNewEntities(int timesToDuplicate, bool isInsert)
        {
            List<ESTIMATION_DIRECT_ITEMProgress> unsavedEntities = new List<ESTIMATION_DIRECT_ITEMProgress>();
            for (int i = 0; i < timesToDuplicate; i++)
            {
                foreach (var selectedEntity in SelectedEntities)
                {
                    var newProjection = new ESTIMATION_DIRECT_ITEMProgress();
                    DataUtils.ShallowCopy(newProjection.Entity.Entity, selectedEntity.Entity.Entity);
                    newProjection.Entity.Entity.EntityKey = Guid.Empty;
                    newProjection.Entity.Entity.GUID_ORIGINAL = Guid.Empty;

                    //because this function is used in variation, let ApplyProjection handle this
                    newProjection.Entity.Entity.GUID_ESTIMATION_DIRECT = null;
                    newProjection.Entity.Entity.GUID_VARIATION = null;

                    //when duplicated by variation this should be 0
                    newProjection.Entity.Entity.ESTIMATED_QUANTITY = 0;

                    newProjection.Entity.Entity.DC_QUANTITY = 0;
                    newProjection.Entity.Entity.PROGRESS_TYPE = Estimation_DirectProgressType.Standalone;
                    newProjection.Entity.Entity.DB_Productivity_Override = null;
                    //newProjection.Entity.Entity.ESTIMATED_QUANTITY = IsBASELINELocked ? 0 : selectedEntity.Entity.Entity.ESTIMATED_QUANTITY;

                    AddUndo(newProjection, null, null, null, EntityMessageType.Added);
                    unsavedEntities.Add(newProjection);
                }
            }

            return unsavedEntities;
        }
        #endregion

        ///// <summary>
        ///// Remove redundant project stock groups when view is closed
        ///// </summary>
        //protected override void OnClose(CancelEventArgs e)
        //{
        //    if(STOCK_CODECollectionViewModel != null)
        //    {
        //        List<STOCK_CODE> removeStockCodes = new List<STOCK_CODE>();
        //        foreach (STOCK_CODE projectStockCode in ProjectSTOCK_CODECollection)
        //        {
        //            if (!MainViewModel.Entities.Any(x => x.Entity.Entity.GUID_STOCK_CODE == projectStockCode.GUID))
        //                removeStockCodes.Add(projectStockCode);
        //        }
        //        STOCK_CODECollectionViewModel.BaseBulkDelete(removeStockCodes);
        //    }

        //    base.OnClose(e);
        //}

        protected override bool IsSingleMainEntityRefreshIdentified(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
            if(changedType == typeof(STOCK_CODE))
            {
                this.RaisePropertyChanged(x => x.STOCK_CODECollection);
                STOCK_CODE changedStock_Code = STOCK_CODECollection.FirstOrDefault(x => x.GUID == (Guid)key);
                if(changedStock_Code != null)
                {
                    foreach (var entities in MainViewModel.Entities)
                    {
                        if(entities.Entity.Entity.GUID_STOCK_CODE == (Guid)key)
                        {
                            entities.Entity.STOCK_CODE = changedStock_Code;
                            entities.Update();
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        private void AdditionalValidateCellCallBack(GridCellValidationEventArgs e)
        {
            string fieldName = DataUtils.FormatColumnFieldname(e.Column.FieldName);
            string error_message = Interface_AdditionalValidateCellCallBack((ESTIMATION_DIRECT_ITEMProgress)e.Row, e.Value, fieldName);
            if (error_message != string.Empty)
            {
                e.IsValid = false;
                e.ErrorContent = error_message;
                e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Critical;
            }

        }

        public string Interface_AdditionalValidateCellCallBack(ESTIMATION_DIRECT_ITEMProgress validateEntity, object currentValue, string fieldName)
        {
            string error_message = string.Empty;
            //estimated hours field is disabled but just in case

            if (fieldName == BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().PROGRESS_TYPE))
            {
                if (validateEntity.Entity.Entity.GUID_STOCK_GROUP != null)
                {
                    STOCK_GROUP entity_commodity_code = STOCK_GROUPCollection.FirstOrDefault(x => x.GUID == validateEntity.Entity.Entity.GUID_STOCK_GROUP);
                    if (entity_commodity_code != null)
                    {
                        if ((validateEntity.Entity.Entity.STOCK_CODE.UOM != entity_commodity_code.UOM) && ((Estimation_DirectProgressType)currentValue) == Estimation_DirectProgressType.Trackable)
                        {
                            error_message = "Cannot set trackable when UOM is different from stock group";
                        }
                    }
                }
                else if (validateEntity.Entity.Entity.GUID_STOCK_GROUP == null)
                {
                    Estimation_DirectProgressType newValue = (Estimation_DirectProgressType)currentValue;
                    if (newValue != Estimation_DirectProgressType.Standalone)
                    {
                        error_message = "Cannot set " + newValue.ToString() + " when stock group is empty";
                    }
                }
            }
            else if (fieldName == BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().GUID_STOCK_GROUP))
            {
                if (validateEntity.Entity.Entity.PROGRESS_TYPE == Estimation_DirectProgressType.Trackable && currentValue != null)
                {
                    STOCK_GROUP entity_commodity_code = STOCK_GROUPCollection.FirstOrDefault(x => x.GUID == (Guid)currentValue);
                    if (entity_commodity_code != null)
                    {
                        if ((validateEntity.Entity.Entity.STOCK_CODE.UOM != entity_commodity_code.UOM))
                        {
                            error_message = "Cannot set a stock group with different UOM than stock code when deliverable is trackable";
                        }
                    }
                }
            }
            else if (fieldName == BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProgress().Entity.StockCodeGuid))
            {
                if (validateEntity.Entity.Entity.PROGRESS_TYPE == Estimation_DirectProgressType.Trackable && currentValue != null)
                {
                    STOCK_GROUP entity_commodity_code = STOCK_GROUPCollection.FirstOrDefault(x => x.GUID == validateEntity.Entity.Entity.GUID_STOCK_GROUP);
                    STOCK_CODE entity_stock_code = STOCK_CODECollection.FirstOrDefault(x => x.GUID == (Guid)currentValue);
                    if (entity_stock_code != null && entity_commodity_code != null)
                    {
                        if ((entity_commodity_code.UOM != entity_stock_code.UOM))
                        {
                            error_message = "Cannot set a stock code with different UOM than stock group when deliverable is trackable";
                        }
                    }
                }
            }

            return error_message;
        }

        /// <summary>
        /// Allow undo-redo behavior to be added for automated cell value changing. This behavior doesn't have to be applied on new row because AddUndo for EntityMessageType.Added is already handling this
        /// </summary>
        protected override void CellValueExistingRowChanging(CellValueChangedEventArgs e)
        {
            var active_progress = (ESTIMATION_DIRECT_ITEMProgress)e.Row;
            Interface_CellValueExistingRowChanging(e.Column.FieldName, e.Value, active_progress);
            base.CellValueExistingRowChanging(e);
        }

        public void Interface_CellValueExistingRowChanging(string field_name, object new_value, ESTIMATION_DIRECT_ITEMProgress active_progress)
        {
            field_name = DataUtils.FormatColumnFieldname(field_name);
            if (field_name == BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().GUID_DISCIPLINE))
            {
                resetProjectionCommodityCode(active_progress);
            }
            else if (field_name == BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProgress().Entity.StockCodeGuid))
            {
                if(new_value != null)
                {
                    STOCK_CODE stock_code = STOCK_CODECollection.FirstOrDefault(x => x.GUID == (Guid)new_value);
                    if(stock_code != null)
                    {
                        Guid? oldValue = active_progress.Entity.Entity.GUID_COMMODITY_CODE;
                        Guid? newValue = stock_code.GUID_COMMODITY_CODE;
                        string commodity_code_field_name = Base_Entity_String + BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().GUID_COMMODITY_CODE);
                        active_progress.Entity.Entity.GUID_COMMODITY_CODE = newValue;
                        PauseUndoRedo();
                        AddUndo(active_progress, commodity_code_field_name, oldValue, newValue, EntityMessageType.Changed);
                    }
                }
            }
            else if (field_name == BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProgress().Entity.Entity.GUID_COMMODITY_CODE))
            {
                if (new_value != null)
                {
                    COMMODITY_CODE entity_commodity_code = COMMODITY_CODECollection.FirstOrDefault(x => x.GUID == (Guid)new_value);
                    if (entity_commodity_code != null)
                    {
                        Guid? oldValue = active_progress.Entity.Entity.GUID_DISCIPLINE;
                        Guid? newValue = entity_commodity_code.GUID_DISCIPLINE;
                        string discipline_field_name = Base_Entity_String + BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().GUID_DISCIPLINE);
                        active_progress.Entity.Entity.GUID_DISCIPLINE = newValue;
                        PauseUndoRedo();
                        AddUndo(active_progress, discipline_field_name, oldValue, newValue, EntityMessageType.Changed);
                    }
                }
            }
            else if (field_name == BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProgress().Entity.Entity.PROGRESS_TYPE))
            {
                Estimation_DirectProgressType progress_Type = (Estimation_DirectProgressType)new_value;
                if (progress_Type == Estimation_DirectProgressType.Standalone)
                {
                    Guid? oldValue = active_progress.Entity.Entity.GUID_STOCK_GROUP;
                    Guid? newValue = null;
                    active_progress.Entity.Entity.GUID_STOCK_GROUP = newValue;
                    string stock_group_fieldname = Base_Entity_String + BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().GUID_STOCK_GROUP);
                    PauseUndoRedo();
                    AddUndo(active_progress, stock_group_fieldname, oldValue, newValue, EntityMessageType.Changed);
                    active_progress.Update();
                }
            }
            else if (field_name == BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProgress().Entity.Entity.GUID_STOCK_GROUP))
            {
                if (new_value == null)
                {
                    Estimation_DirectProgressType oldValue = active_progress.Entity.Entity.PROGRESS_TYPE;
                    Estimation_DirectProgressType newValue = Estimation_DirectProgressType.Standalone;
                    active_progress.Entity.Entity.PROGRESS_TYPE = newValue;
                    string progress_type_fieldname = Base_Entity_String + BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().PROGRESS_TYPE);
                    PauseUndoRedo();
                    AddUndo(active_progress, progress_type_fieldname, oldValue, newValue, EntityMessageType.Changed);
                    active_progress.Update();
                }
            }
        }

        public void Interface_CellValueChanged(string field_name, ESTIMATION_DIRECT_ITEMProgress projection)
        {

        }

        private void setProjectionStockCode(ESTIMATION_DIRECT_ITEMProgress projection, Guid? stockCodeGuid)
        {
            if (stockCodeGuid != null)
                projection.Entity.STOCK_CODE = STOCK_CODECollection.FirstOrDefault(x => x.GUID == (Guid)stockCodeGuid);
            else
                projection.Entity.STOCK_CODE = null;

            projection.Update();
        }

        private void updateProjectionStockCodeCollection(ESTIMATION_DIRECT_ITEMProgress projection, Guid? disciplineGuid)
        {
            if (disciplineGuid != null)
                //stock code collection must be updated by discipline filter
                projection.Entity.StockCodeCollection = STOCK_CODECollection.Where(x => x.GUID_DISCIPLINE == disciplineGuid);
            else
                projection.Entity.StockCodeCollection = new List<STOCK_CODE>();

            projection.Update();
        }

        private void resetProjectionSubArea(ESTIMATION_DIRECT_ITEMProgress projection)
        {
            Guid? oldValue = projection.Entity.Entity.GUID_SUBAREA;
            if (oldValue != null)
            {
                Guid? newValue = (Guid?)null;
                string subAreaFieldName = BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().SubAreaGuid);
                projection.Entity.Entity.GUID_SUBAREA = newValue;
                PauseUndoRedo();
                AddUndo(projection, subAreaFieldName, oldValue, newValue, EntityMessageType.Changed);
            }
        }

        private void resetProjectionCommodityCode(ESTIMATION_DIRECT_ITEMProgress projection)
        {
            Guid? oldValue = projection.Entity.Entity.GUID_COMMODITY_CODE;
            if (oldValue != null)
            {
                Guid? newValue = (Guid?)null;
                string commoditycodeFieldName = Base_Entity_String + BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().GUID_COMMODITY_CODE);
                projection.Entity.Entity.GUID_COMMODITY_CODE = newValue;
                PauseUndoRedo();
                AddUndo(projection, commoditycodeFieldName, oldValue, newValue, EntityMessageType.Changed);
            }
        }

        protected override void CellValueNewRowChanging(CellValueChangedEventArgs e)
        {
            var active_progress = (ESTIMATION_DIRECT_ITEMProgress)e.Row;
            Interface_CellValueNewRowChanging(e.Column.FieldName, e.Value, active_progress);

            base.CellValueNewRowChanging(e);
        }

        public void Interface_CellValueNewRowChanging(string field_name, object new_value, ESTIMATION_DIRECT_ITEMProgress active_progress)
        {
            field_name = DataUtils.FormatColumnFieldname(field_name);
            if (field_name == BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().GUID_AREA))
            {
                if (new_value != null)
                {
                    active_progress.Entity.Entity.GUID_AREA = (Guid)new_value;
                    //Area is required immediately for subarea selection
                    active_progress.Entity.Entity.AREA = AREACollection.FirstOrDefault(x => x.GUID == (Guid)new_value);

                    active_progress.Update();
                }

                //SubArea must be removed immediately to nullify subarea selection
                if (active_progress.Entity.Entity.GUID_SUBAREA != null)
                {
                    active_progress.Entity.Entity.GUID_SUBAREA = null;
                    active_progress.Update();
                }

                active_progress.Update();
            }
            else if (field_name == BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProgress().Entity.StockCodeGuid))
            {
                if (new_value != null)
                {
                    STOCK_CODE stock_code = STOCK_CODECollection.FirstOrDefault(x => x.GUID == (Guid)new_value);
                    if (stock_code != null)
                    {
                        active_progress.Entity.Entity.GUID_COMMODITY_CODE = stock_code.GUID_COMMODITY_CODE;
                        active_progress.Update();
                    }
                }
            }
            else if (field_name == BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().GUID_DISCIPLINE))
            {
                updateProjectionStockCodeCollection(active_progress, (Guid?)new_value);
                active_progress.Update();
            }
            else if (field_name == BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProgress().Entity.StockCodeGuid))
            {
                setProjectionStockCode(active_progress, (Guid?)new_value);
                active_progress.Update();
            }
            else if (field_name == BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProgress().Entity.Entity.GUID_COMMODITY_CODE))
            {
                var chosen_commodity_code = COMMODITY_CODECollection.FirstOrDefault(entity => entity.GUID == (Guid)new_value);
                if (chosen_commodity_code != null)
                {
                    active_progress.Entity.Entity.GUID_DISCIPLINE = chosen_commodity_code.GUID_DISCIPLINE;
                    active_progress.Update();
                }
            }
            else if (field_name == BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProgress().Entity.Entity.PROGRESS_TYPE))
            {
                Estimation_DirectProgressType progress_Type = (Estimation_DirectProgressType)new_value;
                if (progress_Type == Estimation_DirectProgressType.Standalone)
                {
                    active_progress.Entity.Entity.GUID_STOCK_GROUP = null;
                    active_progress.Update();
                }
            }
            else if (field_name == BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProgress().Entity.Entity.GUID_STOCK_GROUP))
            {
                if(new_value == null)
                {
                    active_progress.Entity.Entity.PROGRESS_TYPE = Estimation_DirectProgressType.Standalone;
                    active_progress.Update();
                }
            }
        }

        #endregion

        #region stock group Helpers
        private enum projectStock_CodeStatus
        {
            IsEmpty,
            DontExists,
            ExistsWithDifferentRateHours,
            MetaExistsOnDifferentRecord, 
            Exists
        }

        private projectStock_CodeStatus getProjectStockCodeStatus(STOCK_CODE stock_code, out STOCK_CODE projectStock_Code)
        {
            projectStock_Code = null;
            if (stock_code == null)
                return projectStock_CodeStatus.IsEmpty;

            projectStock_Code = ProjectSTOCK_CODECollection.FirstOrDefault(x => x.GUID == stock_code.GUID);
            bool isExists = false;
            if (projectStock_Code != null)
                isExists = true;

            if (isExists && projectStock_Code.CODE == stock_code.CODE && projectStock_Code.RATE_SUPPLY == stock_code.RATE_SUPPLY && projectStock_Code.HOURS_INSTALL == stock_code.HOURS_INSTALL)
                return projectStock_CodeStatus.Exists;

            //look for other project stock group with same meta
            STOCK_CODE sameMetaStockCode = ProjectSTOCK_CODECollection.FirstOrDefault(x => x.CODE == stock_code.CODE && x.RATE_SUPPLY == stock_code.RATE_SUPPLY && x.HOURS_INSTALL == stock_code.HOURS_INSTALL);
            if (isExists && sameMetaStockCode == null)
                return projectStock_CodeStatus.ExistsWithDifferentRateHours;

            if (sameMetaStockCode != null)
            {
                projectStock_Code = sameMetaStockCode;
                return projectStock_CodeStatus.MetaExistsOnDifferentRecord;
            }

            return projectStock_CodeStatus.DontExists;
        }

        #endregion

        public void Save(ESTIMATION_DIRECT_ITEMProgress progress_entity)
        {
            MainViewModel.Save(progress_entity);
        }

        public void Delete(ESTIMATION_DIRECT_ITEMProgress progress_entity)
        {
            MainViewModel.Delete(progress_entity);
        }

        public bool CanInsertMultiple(BarEditItem barEdit)
        {
            return CanDuplicateMultiple(barEdit);
        }

        public void InsertMultiple(BarEditItem barEdit)
        {

        }

        public bool CanInsert()
        {
            return CanDuplicate();
        }

        public void Insert()
        {

        }

        public bool CanAutoPopulate(object button)
        {
            if (SelectedEntities == null || SelectedEntities.Count() == 0)
                return false;

            return true;
        }

        public void AutoPopulate(object button)
        {
        }

        public bool CanFindReplace(object button)
        {
            return false;
        }

        public void FindReplace(object button)
        {
        }

        #region View Properties
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper"; }
        }

        private DevExpress.Mvvm.IDialogService StockCodeDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("StockCodeDialogService"); }
        }

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMCollection
        {
            get
            {
                return GetEntities<PROGRESS_ITEM>();
            }
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

        public IEnumerable<STOCK_GROUP> STOCK_GROUPCollection
        {
            get
            {
                var collection = GetEntities<STOCK_GROUP>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.CODE);
                return collection;
            }
        }

        public IEnumerable<STOCK_CODE> ProjectSTOCK_CODECollection
        {
            get
            {
                if (loadPROJECT == null)
                    return null;

                return STOCK_CODECollection.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).OrderBy(x => x.CODE);
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

        public IEnumerable<STOCK_GROUP> ProjectSTOCK_GROUPCollection
        {
            get
            {
                var collection = GetEntities<STOCK_GROUP>();
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

        public CollectionViewModel<STOCK_GROUP, STOCK_GROUP, Guid, IBluePrintsEntitiesUnitOfWork> STOCK_GROUPCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<STOCK_GROUP, STOCK_GROUP, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<STOCK_GROUP>();
            }
        }

        public Func<IEnumerable<ESTIMATION_DIRECT_ITEMProgress>> GetEditableAllEntitiesCallBack { get; set; }

        #endregion
    }
}