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
using System.Text.RegularExpressions;

namespace BluePrints.ViewModels
{
    public class ESTIMATE_ITEMCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <ESTIMATE_ITEM, ESTIMATE_ITEMProgress, Guid, IBluePrintsEntitiesUnitOfWork>, IDeliverableCollectionViewModelWrapper<ESTIMATE_ITEMProgress, ESTIMATE_ITEM>
    {
        /// <summary>
        /// Creates a new instance of ESTIMATE_ITEM_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static ESTIMATE_ITEMCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new ESTIMATE_ITEMCollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the PROJECTViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected ESTIMATE_ITEMCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        private PROJECT loadPROJECT;
        private PROGRESS livePROGRESS;
        private ESTIMATE loadESTIMATE;
        public Guid load_context_guid => loadESTIMATE == null ? Guid.Empty : loadESTIMATE.GUID;
        private bool isQueryForLiveStatus;
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        public string Base_Entity_String => "Entity.Entity.";
        public string Projection_Entity_String => "Entity.";

        public string DefaultPhaseInternalNumber { get; set; }
        private DEPARTMENT defaultDepartment;
        public Func<ESTIMATE_ITEMProgress> SelectedEntityCallBack { get; set; }
        public ESTIMATE_ITEMProgress SelectedEntity { get => SelectedEntityCallBack != null ? SelectedEntityCallBack.Invoke() : DisplaySelectedEntity; }
        public IEnumerable<ESTIMATE_ITEMProgress> SelectedEntities { get; set; }
        public IEnumerable<ESTIMATE_ITEMProgress> EditableAllEntities { get; set; }
        public bool IsProcurementSubjobVisible { get; set; }
        private IUnitOfWorkFactory<IP6EntitiesUnitOfWork> p6UnitOfWorkFactory =
            P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        private DeliverablesViewType viewType { get; set; }
        private EstimateViewMode viewMode { get; set; }
        protected override void resolveParameters(object parameter)
        {
            Interface_InitializeParameters(parameter);
        }

        public void Interface_InitializeParameters(object parameter)
        {
            var receiveParameter = (TripleEntitiesParameter<PROJECT, IAmBaseline, object>)parameter;
            loadPROJECT = receiveParameter.GetFirstEntity();
            loadESTIMATE = (ESTIMATE)receiveParameter.GetSecondEntity();

            KeyValuePair<DeliverablesViewType, EstimateViewMode> viewParameter = (KeyValuePair<DeliverablesViewType, EstimateViewMode>)receiveParameter.GetThirdEntity();
            viewType = viewParameter.Key;
            viewMode = viewParameter.Value;

            IsProcurementSubjobVisible = viewType != DeliverablesViewType.Indirect;
            if (loadPROJECT != null)
                isQueryForLiveStatus = true;
        }

        protected override void initializeEntitiesLoadersDescription()
        {
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PHASES, PHASEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.ESTIMATES, ESTIMATEProjectionFunc, x => assign_estimation_direct(x));
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc, x => assign_progress(x));
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESS_ITEMS, PROGRESS_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.STOCK_CODES, STOCK_CODEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.STOCK_GROUPS, STOCK_GROUPProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, COMMODITY_CODEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.SUBJOBS, SUBJOBProjectionFunc);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS, DEPARTMENTProjectionFunc, x => defaultDepartment = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
        }

        private void assign_estimation_direct(ESTIMATE estimation_direct)
        {
            if (estimation_direct == null && !SupressCompulsoryEntityNotFoundMessage)
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage("Working estimate not found")));

            if (viewMode == EstimateViewMode.Budget && (estimation_direct != null && estimation_direct.STATUS == BaselineStatus.Working))
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage("Budget (live estimate) not found")));

            loadESTIMATE = estimation_direct;
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
                return query => query.Where(x => x.GUID == loadESTIMATE.GUID_PROJECT);
        }

        private Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> PROGRESSProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.TYPE == ProgressType.Construct && x.STATUS == ProgressStatus.Live);
        }

        private Func<IRepositoryQuery<DEPARTMENT>, IQueryable<DEPARTMENT>> DEPARTMENTProjectionFunc()
        {
            return query => query.Where(x => x.NAME == BluePrintsResources.Default_Construction_Department);
        }

        private Func<IRepositoryQuery<PROGRESS_ITEM>, IQueryable<PROGRESS_ITEM>> PROGRESS_ITEMProjectionFunc()
        {
            if (livePROGRESS == null)
                return query => query.Where(x => x.GUID_PROGRESS == Guid.Empty);
            else
                return query => query.Where(x => x.GUID_PROGRESS == livePROGRESS.GUID);
        }

        private Func<IRepositoryQuery<ESTIMATE>, IQueryable<ESTIMATE>> ESTIMATEProjectionFunc()
        {
            if (isQueryForLiveStatus)
            {
                if(viewMode == EstimateViewMode.Estimate)
                    return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS != BaselineStatus.Superseded);
                else
                    return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == BaselineStatus.Live);
            }
            else
                return query => query.Where(x => x.GUID == loadESTIMATE.GUID);
        }

        private Func<IRepositoryQuery<SUBJOB>, IQueryable<SUBJOB>> SUBJOBProjectionFunc()
        {
            if (viewType == DeliverablesViewType.Direct)
                return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.PHASE != null && ((x.PHASE.PHASE_TYPE == PhaseType.Construct && x.PHASE.CHARGE_TYPE == ChargeType.Direct) || (x.PHASE.PHASE_TYPE == PhaseType.Procurement)));
            else if (viewType == DeliverablesViewType.Indirect)
                return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.PHASE != null && (x.PHASE.PHASE_TYPE == PhaseType.Construct && x.PHASE.CHARGE_TYPE == ChargeType.Indirect));
            else
                return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.PHASE != null);
        }

        private Func<IRepositoryQuery<STOCK_CODE>, IQueryable<STOCK_CODE>> STOCK_CODEProjectionFunc()
        {
            return query => query.Include(x => x.PROJECT);
        }

        private Func<IRepositoryQuery<PHASE>, IQueryable<PHASE>> PHASEProjectionFunc()
        {
            return query => query.Where(x => x.PHASE_TYPE == PhaseType.Construct || x.PHASE_TYPE == PhaseType.Procurement);
        }

        private Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadESTIMATE.PROJECT.GUID && x.COST_GROUP == CostGroup.Site);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadESTIMATE.PROJECT.GUID);
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
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.ESTIMATE_ITEMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<ESTIMATE_ITEM>, IQueryable<ESTIMATE_ITEMProgress>>
            specifyMainViewModelProjection()
        {
            return query => ESTIMATE_ITEMProjectionQueries.IDeliverable_Progress_Transformation(base_entity_query(query), loadPROJECT, loaderCollection.GetCollection<RATE>(), livePROGRESS, PROGRESS_ITEMCollection, STOCK_CODECollection, loaderCollection.GetCollection<STOCK_GROUP>());
        }

        public Func<IRepositoryQuery<ESTIMATE_ITEM>, IQueryable<ESTIMATE_ITEM>> BaseEntityQueryCallBack { get; set; }
        private IQueryable<ESTIMATE_ITEM> base_entity_query(IRepositoryQuery<ESTIMATE_ITEM> query)
        {
            if (BaseEntityQueryCallBack != null)
                return BaseEntityQueryCallBack(query);

            if (viewType == DeliverablesViewType.Direct)
                return query.Where(x => x.GUID_ESTIMATE == load_context_guid && x.PHASE != null && x.PHASE.CHARGE_TYPE == ChargeType.Direct);
            else if (viewType == DeliverablesViewType.Indirect)
                return query.Where(x => x.GUID_ESTIMATE == load_context_guid && x.PHASE != null && x.PHASE.CHARGE_TYPE == ChargeType.Indirect);
            else
                return query.Where(x => x.GUID_ESTIMATE == load_context_guid);
        }

        public Action<ESTIMATE_ITEMProgress, string, object, object, EntityMessageType> InterfaceAddUndoRedoCallBack { get; set; }
        public void AddUndo(ESTIMATE_ITEMProgress changedEntity, string propertyName, object oldValue, object newValue, EntityMessageType messageType)
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

        public Action<IEnumerable<ESTIMATE_ITEMProgress>> OnReportablesLoadedCallBack { get; set; }
        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<ESTIMATE_ITEMProgress> entities)
        {
            //MainViewModel.DisablePasting = true;
            MainViewModel.ApplyEntityPropertiesToProjectionCallBack = OnEntitiesSavedCallBack;
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = OnBeforeEntitySaved;
            MainViewModel.ManualPasteAction = ManualPasteAction;
            MainViewModel.SetParentViewModel(this);
            mainThreadDispatcher.BeginInvoke(new Action(() => showViewReadOnlyMessage()));
            base.AssignCallBacksAndRaisePropertyChange(entities);

            //used for interface when this is loaded from variation
            if (OnReportablesLoadedCallBack != null)
            {
                OnReportablesLoadedCallBack(entities);
                return;
            }

            SetViewSpecificProperties();
        }

        private void showViewReadOnlyMessage()
        {
            if (!AllowEditingOnEstimate)
                MessageBoxService.ShowMessage("Estimate is currently in read only mode because it has already been approved as a budget");
        }

        /// <summary>
        /// Each estimation entity will need to be assigned to a construction phased subjob and a procurement phased subjob
        /// </summary>
        /// <param name="entity"></param>
        private void onBeforeSavedDualSubjobAssignment(ESTIMATE_ITEMProgress entity)
        {
            PhaseType? phaseType = null;
            ChargeType? chargeType = null;

            PhaseType? procurementPhaseType = null;

            PHASE defaultPHASE = PHASECollection.FirstOrDefault(x => (x.PHASE_TYPE != null && x.PHASE_TYPE == PhaseType.Construct) && (x.CHARGE_TYPE != null && x.CHARGE_TYPE == ChargeType.Direct));
            if (viewType == DeliverablesViewType.Direct)
            {
                phaseType = PhaseType.Construct;
                chargeType = ChargeType.Direct;
                procurementPhaseType = PhaseType.Procurement;
                if (defaultPHASE != null)
                    entity.Phase_Guid = defaultPHASE.GUID;
            }
            else if(viewType == DeliverablesViewType.Indirect)
            {
                phaseType = PhaseType.Construct;
                chargeType = ChargeType.Indirect;
                procurementPhaseType = PhaseType.Procurement;
                PHASE indirectPHASE = PHASECollection.FirstOrDefault(x => (x.PHASE_TYPE != null && x.PHASE_TYPE == PhaseType.Construct) && (x.CHARGE_TYPE != null && x.CHARGE_TYPE == ChargeType.Indirect));
                if (indirectPHASE != null)
                    entity.Phase_Guid = indirectPHASE.GUID;
            }
            else if (entity.Phase_Guid == null && defaultPHASE != null)
            {
                entity.Phase_Guid = defaultPHASE.GUID;
            }

            BluePrintsDataUtils.OnBeforeSavedGenerateAndAssignSubjob(loadPROJECT, PHASECollection, AREACollection, SUBAREACollection, entity, SUBJOBSCollectionViewModel, phaseType, chargeType);
            if(chargeType != ChargeType.Indirect)
                //by passing in only procurement phase type, the first occurence of procurement PHASE will be retrieved
                BluePrintsDataUtils.OnBeforeSavedGenerateAndAssignSubjob(loadPROJECT, PHASECollection, AREACollection, SUBAREACollection, entity, SUBJOBSCollectionViewModel, procurementPhaseType, null, true);
        }

        public void ManualPasteAction(List<KeyValuePair<ColumnBase, string>> pasteData, ESTIMATE_ITEMProgress pasteEntity)
        {
            onBeforeSavedDualSubjobAssignment(pasteEntity);
            KeyValuePair<ColumnBase, string> stock_code_data = pasteData.FirstOrDefault(x => x.Key.FieldName.Contains(BindableBase.GetPropertyName(() => new ESTIMATE_ITEMProgress().Entity.Estimate_StockCodeGuid)));

            if (stock_code_data.Key != null)
            {
                KeyValuePair<ColumnBase, string> supply_rate_data = pasteData.FirstOrDefault(x => x.Key.FieldName.Contains(BindableBase.GetPropertyName(() => new ESTIMATE_ITEMProgress().Entity.ESTIMATE_STOCK_CODE.RATE_SUPPLY)));
                KeyValuePair<ColumnBase, string> install_rate_data = pasteData.FirstOrDefault(x => x.Key.FieldName.Contains(BindableBase.GetPropertyName(() => new ESTIMATE_ITEMProgress().Entity.ESTIMATE_STOCK_CODE.HOURS_INSTALL)));

                if (supply_rate_data.Key != null && install_rate_data.Key != null)
                {
                    Regex rgx = new Regex("[^0-9a-z\\.]");
                    string clean_supply_rate = rgx.Replace(supply_rate_data.Value, string.Empty);
                    string clean_install_rate = rgx.Replace(install_rate_data.Value, string.Empty);
                    if (clean_supply_rate == string.Empty)
                        clean_supply_rate = "0";

                    if (clean_install_rate == string.Empty)
                        clean_install_rate = "0";

                    decimal supply_value;
                    decimal install_value;
                    bool rate_result = decimal.TryParse(clean_supply_rate, out supply_value);
                    bool install_result = decimal.TryParse(clean_install_rate, out install_value);

                    if(rate_result && install_result)
                    {
                        STOCK_CODE project_stock_code = ProjectSTOCK_CODECollection.FirstOrDefault(x => x.CODE == stock_code_data.Value && x.RATE_SUPPLY == supply_value && x.HOURS_INSTALL == install_value);
                        STOCK_CODE editing_stock_code;
                        if (IsBudget)
                            editing_stock_code = pasteEntity.Entity.BUDGET_STOCK_CODE;
                        else
                            editing_stock_code = pasteEntity.Entity.ESTIMATE_STOCK_CODE;

                        if (project_stock_code != null)
                        {
                            editing_stock_code = project_stock_code;
                            if(IsBudget)
                            {
                                pasteEntity.Entity.Budget_StockCodeGuid = project_stock_code.GUID;
                                pasteEntity.Entity.Entity.GUID_BUDGET_STOCK_CODE = project_stock_code.GUID;
                            }
                            else
                            {
                                pasteEntity.Entity.Estimate_StockCodeGuid = project_stock_code.GUID;
                                pasteEntity.Entity.Entity.GUID_ESTIMATE_STOCK_CODE = project_stock_code.GUID;
                            }
                        }
                        else
                        {
                            KeyValuePair<ColumnBase, string> uom_data = pasteData.FirstOrDefault(x => x.Key.FieldName.Contains(BindableBase.GetPropertyName(() => new ESTIMATE_ITEMProgress().Entity.ESTIMATE_STOCK_CODE.UOM)));
                            KeyValuePair<ColumnBase, string> name_data = pasteData.FirstOrDefault(x => x.Key.FieldName.Contains(BindableBase.GetPropertyName(() => new ESTIMATE_ITEMProgress().Entity.ESTIMATE_STOCK_CODE.NAME)));
                            KeyValuePair<ColumnBase, string> type_data = pasteData.FirstOrDefault(x => x.Key.FieldName.Contains(BindableBase.GetPropertyName(() => new ESTIMATE_ITEMProgress().Entity.ESTIMATE_STOCK_CODE.TYPE)));
                            KeyValuePair<ColumnBase, string> spec_data = pasteData.FirstOrDefault(x => x.Key.FieldName.Contains(BindableBase.GetPropertyName(() => new ESTIMATE_ITEMProgress().Entity.ESTIMATE_STOCK_CODE.SPEC)));
                            KeyValuePair<ColumnBase, string> desc_data = pasteData.FirstOrDefault(x => x.Key.FieldName.Contains(BindableBase.GetPropertyName(() => new ESTIMATE_ITEMProgress().Entity.ESTIMATE_STOCK_CODE.DESCRIPTION)));
                            editing_stock_code.CODE = stock_code_data.Value;
                            editing_stock_code.UOM = uom_data.Value;
                            editing_stock_code.NAME = name_data.Value;
                            editing_stock_code.TYPE = type_data.Value;
                            editing_stock_code.SPEC = spec_data.Value;
                            editing_stock_code.DESCRIPTION = desc_data.Value;

                            Guid discipline_guid = Guid.Empty;
                            if (pasteEntity.Entity.Entity.GUID_DISCIPLINE == null)
                                discipline_guid = DISCIPLINECollection.First().GUID;
                            else
                                discipline_guid = (Guid)pasteEntity.Entity.Entity.GUID_DISCIPLINE;

                            editing_stock_code.GUID_DISCIPLINE = discipline_guid;
                            editing_stock_code.GUID_DEPARTMENT = defaultDepartment.GUID;

                            //use global stock code as original guid
                            STOCK_CODE from_stock_code = STOCK_CODECollection.FirstOrDefault(x => x.CODE == stock_code_data.Value);

                            if (from_stock_code != null)
                                editing_stock_code.GUID_ORIGINAL = from_stock_code.GUID;

                            if(editing_stock_code.NAME != string.Empty)
                            {
                                pasteEntity.Entity.Budget_StockCodeGuid = createNewSTOCK_CODE(editing_stock_code);
                                if (IsBudget)
                                    pasteEntity.Entity.Entity.GUID_BUDGET_STOCK_CODE = pasteEntity.Entity.Budget_StockCodeGuid;
                                else
                                    pasteEntity.Entity.Entity.GUID_ESTIMATE_STOCK_CODE = pasteEntity.Entity.Estimate_StockCodeGuid;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// this view model can be used in variation or default collection view, only default collection view specific properties are set here
        /// </summary>
        private void SetViewSpecificProperties()
        {
            //When this is not called externally as a nested wrapper
            if(MainViewModel != null)
            {
                SelectedEntities = DisplaySelectedEntities;
                EditableAllEntities = MainViewModel.Entities;
                DefaultPhaseInternalNumber = BluePrintsResources.Default_Construction_Phase;
            }
        }

        public Action<ESTIMATE_ITEMProgress> ApplyViewSpecificPropertiesToEntityCallBack { get; set; }
        protected override void OnBeforeApplyProjectionPropertiesToEntity(ESTIMATE_ITEMProgress projectionEntity, ESTIMATE_ITEM entity)
        {
            if (ApplyViewSpecificPropertiesToEntityCallBack == null)
                projectionEntity.Entity.Entity.GUID_ESTIMATE = load_context_guid;
            else
                ApplyViewSpecificPropertiesToEntityCallBack.Invoke(projectionEntity);

            //because TProjection is not IProjection<TMainEntity>, do it manually here
            DataUtils.ShallowCopy(entity, projectionEntity.Entity.Entity);
            base.OnBeforeApplyProjectionPropertiesToEntity(projectionEntity, entity);
        }

        #region Collection Call Backs
        private void createAndAssignProjectSpecificSTOCK_CODE(ESTIMATE_ITEMProgress projectionEntity)
        {
            Guid? stockcodeGuid;
            if (IsBudget)
                stockcodeGuid = projectionEntity.Entity.Entity.GUID_BUDGET_STOCK_CODE;
            else
                stockcodeGuid = projectionEntity.Entity.Entity.GUID_ESTIMATE_STOCK_CODE;

            if (stockcodeGuid == null)
                return;

            if(!ProjectSTOCK_CODECollection.Any(x => x.GUID == stockcodeGuid))
            {
                STOCK_CODE stock_code = STOCK_CODECollection.FirstOrDefault(x => x.GUID == stockcodeGuid);
                if (stock_code != null)
                {
                    STOCK_CODE newSTOCK_CODE = new STOCK_CODE();
                    DataUtils.ShallowCopy(newSTOCK_CODE, stock_code);
                    newSTOCK_CODE.GUID = Guid.Empty;
                    newSTOCK_CODE.GUID_PROJECT = loadPROJECT.GUID;
                    newSTOCK_CODE.STOCK_CODE_TYPE = IsBudget ? StockCodeType.Budget : StockCodeType.Estimate;
                    STOCK_CODECollectionViewModel.Save(newSTOCK_CODE);

                    if(IsBudget)
                    {
                        projectionEntity.Entity.Entity.GUID_BUDGET_STOCK_CODE = newSTOCK_CODE.GUID;
                        //stock group is by value with shallow copy in setter so that user edited changes can be cross check with existing project stock group
                        projectionEntity.Entity.BUDGET_STOCK_CODE = newSTOCK_CODE;
                    }
                    else
                    {
                        projectionEntity.Entity.Entity.GUID_ESTIMATE_STOCK_CODE = newSTOCK_CODE.GUID;
                        //stock group is by value with shallow copy in setter so that user edited changes can be cross check with existing project stock group
                        projectionEntity.Entity.ESTIMATE_STOCK_CODE = newSTOCK_CODE;
                    }
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
            newStockCode.STOCK_CODE_TYPE = IsBudget ? StockCodeType.Budget : StockCodeType.Estimate;
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
        public bool OnBeforeEntitySaved(ESTIMATE_ITEMProgress entity)
        {
            onBeforeSavedDualSubjobAssignment(entity);
            onBeforeSavedProjectStockCodeLogging(entity);

            //entity.Entity.Entity.GUID_ESTIMATE = loadESTIMATE.GUID;
            return true;
        }

        public void OnEntitiesSavedCallBack(Guid primaryKey, ESTIMATE_ITEMProgress projectionEntity, ESTIMATE_ITEM entity, bool isNewEntity)
        {
            projectionEntity.Entity.Entity.GUID_ORIGINAL = entity.GUID_ORIGINAL;
        }

        private void onBeforeSavedProjectStockCodeLogging(ESTIMATE_ITEMProgress entity)
        {
            STOCK_CODE editingSTOCK_CODE;
            if (IsBudget)
                editingSTOCK_CODE = entity.Entity.BUDGET_STOCK_CODE;
            else
                editingSTOCK_CODE = entity.Entity.ESTIMATE_STOCK_CODE;

            if (editingSTOCK_CODE != null)
            {
                STOCK_CODE projectStockCode;
                projectStock_CodeStatus commodityCodeStatus = getProjectStockCodeStatus(editingSTOCK_CODE, out projectStockCode);
                if (commodityCodeStatus == projectStock_CodeStatus.IsEmpty)
                    return;
                else if (commodityCodeStatus == projectStock_CodeStatus.DontExists)
                    createAndAssignProjectSpecificSTOCK_CODE(entity);
                else if (commodityCodeStatus == projectStock_CodeStatus.MetaExistsOnDifferentRecord)
                {
                    if (IsBudget)
                        entity.Entity.Entity.GUID_BUDGET_STOCK_CODE = projectStockCode.GUID;
                    else
                        entity.Entity.Entity.GUID_ESTIMATE_STOCK_CODE = projectStockCode.GUID;

                    editingSTOCK_CODE = projectStockCode;
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
                        "Do you wish to add new or update?", projectStockCode.RATE_SUPPLY, projectStockCode.HOURS_INSTALL, editingSTOCK_CODE.RATE_SUPPLY, editingSTOCK_CODE.HOURS_INSTALL);

                    BasicMessageBoxViewModel viewModel = BasicMessageBoxViewModel.Create(message);
                    UICommand result = StockCodeDialogService.ShowDialog(new List<UICommand>() { addCommand, editCommand, cancelCommand }, "Stock Code", "BasicMessageBox", viewModel);
                    if (result == addCommand)
                    {
                        Guid newStockCodeGuid = createNewSTOCK_CODE(editingSTOCK_CODE);
                        if (IsBudget)
                            entity.Entity.Entity.GUID_BUDGET_STOCK_CODE = newStockCodeGuid;
                        else
                            entity.Entity.Entity.GUID_ESTIMATE_STOCK_CODE = newStockCodeGuid;
                    }
                    else if (result == editCommand)
                        updateSTOCK_CODE(editingSTOCK_CODE);
                }
                else if (commodityCodeStatus == projectStock_CodeStatus.Exists)
                    updateSTOCK_CODE(editingSTOCK_CODE);
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
            List<ESTIMATE_ITEMProgress> newEntities = new List<ESTIMATE_ITEMProgress>();
            if (int.TryParse(barEdit.EditValue.ToString(), out timesToDuplicate))
            {
                List<ESTIMATE_ITEMProgress> currentEnumerationSaveEntities = getNewEntities(timesToDuplicate, false);
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

            List<ESTIMATE_ITEMProgress> newEntities = getNewEntities(1, false);
            MainViewModel.BulkSave(newEntities);
            if (!_isProcessingMultiple)
                MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
        }

        List<ESTIMATE_ITEMProgress> getNewEntities(int timesToDuplicate, bool isInsert)
        {
            List<ESTIMATE_ITEMProgress> unsavedEntities = new List<ESTIMATE_ITEMProgress>();
            for (int i = 0; i < timesToDuplicate; i++)
            {
                foreach (var selectedEntity in SelectedEntities)
                {
                    var newProjection = new ESTIMATE_ITEMProgress();
                    DataUtils.ShallowCopy(newProjection.Entity.Entity, selectedEntity.Entity.Entity);
                    newProjection.Entity.Entity.EntityKey = Guid.Empty;
                    newProjection.Entity.Entity.GUID_ORIGINAL = Guid.Empty;

                    //because this function is used in variation, let ApplyProjection handle this
                    newProjection.Entity.Entity.GUID_ESTIMATE = null;
                    newProjection.Entity.Entity.GUID_VARIATION = null;

                    //when duplicated by variation this should be 0
                    newProjection.Entity.Entity.BUDGET_QUANTITY = 0;

                    newProjection.Entity.Entity.DC_QUANTITY = 0;
                    newProjection.Entity.Entity.PROGRESS_TYPE = EstimateProgressType.Standalone;
                    newProjection.Entity.Entity.DB_Productivity_Override = null;
                    //newProjection.Entity.Entity.ESTIMATE_QUANTITY = IsBASELINELocked ? 0 : selectedEntity.Entity.Entity.ESTIMATE_QUANTITY;

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
                        Guid? stock_code_guid;
                        STOCK_CODE stock_code;
                        if (IsBudget)
                        {
                            stock_code_guid = entities.Entity.Entity.GUID_BUDGET_STOCK_CODE;
                            stock_code = entities.Entity.BUDGET_STOCK_CODE;
                        }
                        else
                        {
                            stock_code_guid = entities.Entity.Entity.GUID_ESTIMATE_STOCK_CODE;
                            stock_code = entities.Entity.ESTIMATE_STOCK_CODE;
                        }

                        if (stock_code_guid == (Guid)key)
                        {
                            stock_code = changedStock_Code;
                            entities.Update();
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        public override string UnifiedValueValidation(ESTIMATE_ITEMProgress projection, string field_name, object new_value)
        {
            string fieldName = DataUtils.FormatColumnFieldname(field_name);
            //budgeted hours field is disabled but just in case
            if (field_name == BindableBase.GetPropertyName(() => new ESTIMATE_ITEM().PROGRESS_TYPE))
            {
                if (projection.Entity.Entity.STOCK_CODE == null)
                {
                    EstimateProgressType newValue = (EstimateProgressType)new_value;
                    if (newValue != EstimateProgressType.Standalone)
                    {
                        return "Cannot set " + newValue.ToString() + " when stock code is empty";
                    }
                }
                else if (projection.Entity.Entity.GUID_STOCK_GROUP != null)
                {
                    STOCK_GROUP entity_stock_group = STOCK_GROUPCollection.FirstOrDefault(x => x.GUID == projection.Entity.Entity.GUID_STOCK_GROUP);
                    if (entity_stock_group != null)
                    {
                        if ((projection.Entity.Entity.STOCK_CODE.UOM != entity_stock_group.UOM) && ((EstimateProgressType)new_value) == EstimateProgressType.Trackable)
                        {
                            return "Cannot set trackable when UOM is different from stock group";
                        }
                    }
                }
                else if (projection.Entity.Entity.GUID_STOCK_GROUP == null)
                {
                    EstimateProgressType newValue = (EstimateProgressType)new_value;
                    if (newValue != EstimateProgressType.Standalone)
                    {
                        return "Cannot set " + newValue.ToString() + " when stock group is empty";
                    }
                }
            }
            else if (field_name == BindableBase.GetPropertyName(() => new ESTIMATE_ITEM().GUID_STOCK_GROUP))
            {
                if (projection.Entity.Entity.PROGRESS_TYPE == EstimateProgressType.Trackable && new_value != null)
                {
                    STOCK_GROUP entity_commodity_code = STOCK_GROUPCollection.FirstOrDefault(x => x.GUID == (Guid)new_value);
                    if (entity_commodity_code != null)
                    {
                        if ((projection.Entity.Entity.STOCK_CODE.UOM != entity_commodity_code.UOM))
                        {
                            return "Cannot set a stock group with different UOM than stock code when deliverable is trackable";
                        }
                    }
                }
            }
            else if (field_name == BindableBase.GetPropertyName(() => new ESTIMATE_ITEMProgress().Entity.Estimate_StockCodeGuid))
            {
                if (projection.Entity.Entity.PROGRESS_TYPE == EstimateProgressType.Trackable && new_value != null)
                {
                    STOCK_GROUP entity_commodity_code = STOCK_GROUPCollection.FirstOrDefault(x => x.GUID == projection.Entity.Entity.GUID_STOCK_GROUP);
                    STOCK_CODE entity_stock_code = STOCK_CODECollection.FirstOrDefault(x => x.GUID == (Guid)new_value);
                    if (entity_stock_code != null && entity_commodity_code != null)
                    {
                        if ((entity_commodity_code.UOM != entity_stock_code.UOM))
                        {
                            return "Cannot set a stock code with different UOM than stock group when deliverable is trackable";
                        }
                    }
                }
            }

            return string.Empty;
        }

        public override void UnifiedCellValueChanging(string field_name, object old_value, object new_value, ESTIMATE_ITEMProgress projection, bool isNew)
        {
            field_name = DataUtils.FormatColumnFieldname(field_name);
            //if (field_name == BindableBase.GetPropertyName(() => new ESTIMATE_ITEM().GUID_DISCIPLINE))
            //{
            //    resetProjectionCommodityCode(active_progress);
            //}
            if (field_name == BindableBase.GetPropertyName(() => new ESTIMATE_ITEM().GUID_AREA))
            {
                Guid? oldValue = projection.Entity.Entity.GUID_SUBAREA;
                Guid? newValue = (Guid?)null;

                projection.Entity.Entity.GUID_SUBAREA = newValue;
                if (!isNew)
                {
                    string subAreaFieldName = BindableBase.GetPropertyName(() => new ESTIMATE_ITEM().SubAreaGuid);
                    PauseUndoRedo();
                    AddUndo(projection, subAreaFieldName, oldValue, newValue, EntityMessageType.Changed);
                }
                else
                {
                    //Area is required immediately for subarea selection
                    projection.Entity.Entity.AREA = AREACollection.FirstOrDefault(x => x.GUID == (Guid)new_value);
                    projection.Update();
                }
            }
            else if (field_name == BindableBase.GetPropertyName(() => new ESTIMATE_ITEM().GUID_DISCIPLINE))
            {
                updateProjectionStockCodeCollection(projection, (Guid?)new_value);
                projection.Update();
            }
            //set default commodity code when stock code is changed
            else if (field_name == BindableBase.GetPropertyName(() => new ESTIMATE_ITEMProgress().Entity.Estimate_StockCodeGuid))
            {
                if (new_value != null)
                {
                    Guid? commodityCodeGuid = null;
                    setProjectionEstimateStockCode(projection, (Guid)new_value, out commodityCodeGuid);
                    Guid? oldValue = projection.Entity.Entity.GUID_COMMODITY_CODE;
                    Guid? newValue = commodityCodeGuid;
                    projection.Entity.Entity.GUID_COMMODITY_CODE = newValue;

                    if (!isNew)
                    {
                        string commodity_code_field_name = Base_Entity_String + BindableBase.GetPropertyName(() => new ESTIMATE_ITEM().GUID_COMMODITY_CODE);
                        PauseUndoRedo();
                        AddUndo(projection, commodity_code_field_name, oldValue, newValue, EntityMessageType.Changed);
                    }
                    else
                        projection.Update();
                }
            }
            //set default discipline when commodity code is changed
            else if (field_name == BindableBase.GetPropertyName(() => new ESTIMATE_ITEMProgress().Entity.Entity.GUID_COMMODITY_CODE))
            {
                if (new_value != null)
                {
                    COMMODITY_CODE entity_commodity_code = COMMODITY_CODECollection.FirstOrDefault(x => x.GUID == (Guid)new_value);
                    if (entity_commodity_code != null)
                    {
                        Guid? oldValue = projection.Entity.Entity.GUID_DISCIPLINE;
                        Guid? newValue = entity_commodity_code.GUID_DISCIPLINE;
                        projection.Entity.Entity.GUID_DISCIPLINE = newValue;

                        if (!isNew)
                        {
                            string discipline_field_name = Base_Entity_String + BindableBase.GetPropertyName(() => new ESTIMATE_ITEM().GUID_DISCIPLINE);
                            PauseUndoRedo();
                            AddUndo(projection, discipline_field_name, oldValue, newValue, EntityMessageType.Changed);
                        }
                        else
                            projection.Update();
                    }
                }
            }
            //set stock group to null when progress type is changed
            else if (field_name == BindableBase.GetPropertyName(() => new ESTIMATE_ITEMProgress().Entity.Entity.PROGRESS_TYPE))
            {
                EstimateProgressType progress_Type = (EstimateProgressType)new_value;
                if (progress_Type == EstimateProgressType.Standalone)
                {
                    Guid? oldValue = projection.Entity.Entity.GUID_STOCK_GROUP;
                    Guid? newValue = null;
                    projection.Entity.Entity.GUID_STOCK_GROUP = newValue;
                    string stock_group_fieldname = Base_Entity_String + BindableBase.GetPropertyName(() => new ESTIMATE_ITEM().GUID_STOCK_GROUP);
                    PauseUndoRedo();
                    AddUndo(projection, stock_group_fieldname, oldValue, newValue, EntityMessageType.Changed);
                    projection.Update();
                }
            }
            //set progress type to standalone when stock group is changed
            else if (field_name == BindableBase.GetPropertyName(() => new ESTIMATE_ITEMProgress().Entity.Entity.GUID_STOCK_GROUP))
            {
                if (new_value == null)
                {
                    EstimateProgressType oldValue = projection.Entity.Entity.PROGRESS_TYPE;
                    EstimateProgressType newValue = EstimateProgressType.Standalone;
                    projection.Entity.Entity.PROGRESS_TYPE = newValue;
                    string progress_type_fieldname = Base_Entity_String + BindableBase.GetPropertyName(() => new ESTIMATE_ITEM().PROGRESS_TYPE);
                    PauseUndoRedo();
                    AddUndo(projection, progress_type_fieldname, oldValue, newValue, EntityMessageType.Changed);
                    projection.Update();
                }
            }

            base.UnifiedCellValueChanging(field_name, old_value, new_value, projection, isNew);
        }

        private void setProjectionEstimateStockCode(ESTIMATE_ITEMProgress projection, Guid? stockCodeGuid, out Guid? commodityCodeGuid)
        {
            STOCK_CODE findSTOCK_CODE = STOCK_CODECollection.FirstOrDefault(x => x.GUID == (Guid)stockCodeGuid);
            if (stockCodeGuid != null)
            {
                projection.Entity.ESTIMATE_STOCK_CODE = findSTOCK_CODE;
                commodityCodeGuid = findSTOCK_CODE.GUID_COMMODITY_CODE;
            }
            else
            {
                projection.Entity.ESTIMATE_STOCK_CODE = null;
                commodityCodeGuid = null;
            }

            projection.Update();
        }

        private void updateProjectionStockCodeCollection(ESTIMATE_ITEMProgress projection, Guid? disciplineGuid)
        {
            if (disciplineGuid != null)
                //stock code collection must be updated by discipline filter
                projection.Entity.StockCodeCollection = STOCK_CODECollection.Where(x => x.GUID_DISCIPLINE == disciplineGuid);
            else
                projection.Entity.StockCodeCollection = new List<STOCK_CODE>();

            projection.Update();
        }

        private void resetProjectionSubArea(ESTIMATE_ITEMProgress projection)
        {
            Guid? oldValue = projection.Entity.Entity.GUID_SUBAREA;
            if (oldValue != null)
            {
                Guid? newValue = (Guid?)null;
                string subAreaFieldName = BindableBase.GetPropertyName(() => new ESTIMATE_ITEM().SubAreaGuid);
                projection.Entity.Entity.GUID_SUBAREA = newValue;
                PauseUndoRedo();
                AddUndo(projection, subAreaFieldName, oldValue, newValue, EntityMessageType.Changed);
            }
        }

        private void resetProjectionCommodityCode(ESTIMATE_ITEMProgress projection)
        {
            Guid? oldValue = projection.Entity.Entity.GUID_COMMODITY_CODE;
            if (oldValue != null)
            {
                Guid? newValue = (Guid?)null;
                string commoditycodeFieldName = Base_Entity_String + BindableBase.GetPropertyName(() => new ESTIMATE_ITEM().GUID_COMMODITY_CODE);
                projection.Entity.Entity.GUID_COMMODITY_CODE = newValue;
                PauseUndoRedo();
                AddUndo(projection, commoditycodeFieldName, oldValue, newValue, EntityMessageType.Changed);
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

        public void Save(ESTIMATE_ITEMProgress progress_entity)
        {
            MainViewModel.Save(progress_entity);
        }

        public void Delete(ESTIMATE_ITEMProgress progress_entity)
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
            //get { return "ESTIMATE_ITEMCollectionViewModelWrapper" + view_project_specific_affix; }
            get { return "ESTIMATE_ITEMCollectionViewModelWrapper_v1" + view_project_specific_affix; }
        }

        private string view_project_specific_affix
        {
            get
            {
                if (loadPROJECT == null)
                    return string.Empty;
                return loadPROJECT.GUID.ToString();
            }
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

        public IEnumerable<PHASE> ConstructionPHASECollection
        {
            get
            {
                var collection = GetEntities<PHASE>();
                if (collection != null)
                    collection = collection.Where(x => x.PHASE_TYPE == PhaseType.Construct).OrderBy(x => x.INTERNAL_NUM);
                return collection;
            }
        }

        public IEnumerable<SUBJOB> SUBJOBCollection
        {
            get
            {
                var collection = GetEntities<SUBJOB>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.INTERNAL_NAME1);
                return collection;
            }
        }

        public IEnumerable<SUBJOB> ProcurementSUBJOBCollection
        {
            get
            {
                var collection = GetEntities<SUBJOB>();
                if (collection != null)
                    collection = collection.Where(x => x.PHASE != null && x.PHASE.PHASE_TYPE == PhaseType.Procurement).OrderBy(x => x.INTERNAL_NAME1);
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

        public CollectionViewModel<SUBJOB, SUBJOB, Guid, IBluePrintsEntitiesUnitOfWork> SUBJOBSCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<SUBJOB, SUBJOB, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<SUBJOB>();
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

        public Func<IEnumerable<ESTIMATE_ITEMProgress>> GetEditableAllEntitiesCallBack { get; set; }

        private bool IsBudget => loadESTIMATE.STATUS == BaselineStatus.Live;

        public bool AllowEditingOnEstimate
        {
            get
            {
                if (loadESTIMATE != null && (viewMode == EstimateViewMode.Estimate && loadESTIMATE.STATUS == BaselineStatus.Live))
                    return false;
                else
                    return true;
            }
        }
        #endregion
    }
}