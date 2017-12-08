using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Helpers;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Loader;
using BaseModel.ViewModel.UndoRedo;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Filtering;
using BluePrints.Common.Projections;
using BluePrints.Common.Reports;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.Reports;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Grid.DragDrop;
using DevExpress.Xpf.Printing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Linq.Expressions;
using DevExpress.Data.Filtering;

namespace BluePrints.ViewModels
{
    public interface IDeliverableCollectionViewModelWrapper<TProgress, TDeliverable>
        where TProgress : class, IReportable
        where TDeliverable : class, IDeliverable
    {
        Guid load_context_guid { get; }
        void OnParameterChanged(object parameter);
        Action<IEnumerable<TProgress>> OnReportablesLoadedCallBack { get; set; }
        Action<TProgress> ApplyViewSpecificPropertiesToEntityCallBack { get; set; }
        Func<IRepositoryQuery<TDeliverable>, IQueryable<TDeliverable>> BaseEntityQueryCallBack { get; set; }
        string DefaultPhaseInternalNumber { get; set; }
        Func<TProgress> SelectedEntityCallBack { get; set; }
        IEnumerable<TProgress> SelectedEntities { get; set; }
        //some functionality will edit existing live deliverables, so this has to be used to validate in context
        Func<IEnumerable<TProgress>> GetEditableAllEntitiesCallBack { get; set; }
        void CleanUpEntitiesLoader();

        #region Undo-Redo
        Action<TProgress, string, object, object, EntityMessageType> InterfaceAddUndoRedoCallBack { get; set; }
        Action InterfacePauseUndoRedoCallBack { get; set; }
        Action InterfaceUnpauseUndoRedoCallBack { get; set; }
        #endregion

        #region Events
        string Interface_AdditionalValidateCellCallBack(TProgress active_progress, object new_value, string field_name);
        void Interface_CellValueExistingRowChanging(string field_name, object new_value, TProgress active_progress);
        void Interface_CellValueNewRowChanging(string field_name, object new_value, TProgress active_progress);
        void Interface_CellValueChanged(string field_name, TProgress projection);
        #endregion

        #region Commands
        bool CanDuplicateMultiple(BarEditItem barEdit);
        bool CanInsertMultiple(BarEditItem barEdit);
        bool CanDuplicate();
        bool CanInsert();
        bool CanAutoPopulate(object button);
        bool CanFindReplace(object button);

        void DuplicateMultiple(BarEditItem barEdit);
        void InsertMultiple(BarEditItem barEdit);
        void Duplicate();
        void Insert();
        void AutoPopulate(object button);
        void FindReplace(object button);
        #endregion

        void Save(TProgress progress_entity);
        void Delete(TProgress progress_entity);
    }

    /// <summary>
    /// Represents the single BASELINE object view model.
    /// </summary>
    public partial class BASELINE_ITEMCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <BASELINE_ITEM, BASELINE_ITEMProgress, Guid, IBluePrintsEntitiesUnitOfWork>, IDeliverableCollectionViewModelWrapper<BASELINE_ITEMProgress, BASELINE_ITEM>, ISupportFiltering<BASELINE_ITEMProgress>
    {
        public Action<bool> SetBaselineLockUnlock;
        public Func<BASELINE_ITEMProgress> SelectedEntityCallBack { get; set; }
        public string Base_Entity_String => "Entity.Entity.";
        public string Projection_Entity_String { get; set; }
        public string DefaultPhaseInternalNumber { get; set; }
        public BASELINE_ITEMProgress SelectedEntity { get => SelectedEntityCallBack != null ? SelectedEntityCallBack.Invoke() : DisplaySelectedEntity; }
        public IEnumerable<BASELINE_ITEMProgress> SelectedEntities { get; set; }
        public virtual IEnumerable<BASELINE_ITEMProgress> EditableAllEntities => GetEditableAllEntitiesCallBack != null ? GetEditableAllEntitiesCallBack() : MainViewModel.Entities;
        public Func<IEnumerable<BASELINE_ITEMProgress>> GetEditableAllEntitiesCallBack { get; set; }
        private DeliverablesViewType viewType { get; set; }

        /// <summary>
        /// Creates a new instance of BASELINE_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static BASELINE_ITEMCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new BASELINE_ITEMCollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the BASELINEViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the BASELINEViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected BASELINE_ITEMCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        public FilterTreeViewModel<BASELINE_ITEMProgress, Guid> FilterTreeViewModel { get; set; }
        public PROJECT loadPROJECT { get; set; }
        public BASELINE loadBASELINE { get; set; }
        public Guid load_context_guid => loadBASELINE == null ? Guid.Empty : loadBASELINE.GUID;
        public PROGRESS livePROGRESS { get; set; }
        private bool isQueryForLiveStatus;
        //public bool Is_Autofill_Internal_Number { get; set; }
        private bool allow_drag_drop { get; set; }
        public bool Allow_Drag_Drop
        {
            get => allow_drag_drop;
            set
            {
                allow_drag_drop = value;
                this.RaisePropertyChanged(x => x.Allow_Drag_Drop);
            }
        }

        public bool InternalNumAlwaysEditable
        {
            get => InternalNumberMode == DeliverableInternalNumberMode.AlwaysEditable;
            set
            {
                if (value)
                {
                    InternalNumberMode = DeliverableInternalNumberMode.AlwaysEditable;
                    FullRefresh();
                }
            }
        }

        public bool InternalNumDefault
        {
            get => InternalNumberMode == DeliverableInternalNumberMode.Default;
            set
            {
                if (value)
                {
                    InternalNumberMode = DeliverableInternalNumberMode.Default;
                    FullRefresh();
                }
            }
        }

        public bool InternalNumManual
        {
            get => InternalNumberMode == DeliverableInternalNumberMode.Manual;
            set
            {
                if (value)
                {
                    InternalNumberMode = DeliverableInternalNumberMode.Manual;
                    FullRefresh();
                }
            }
        }

        private DeliverableInternalNumberMode InternalNumberMode { get; set; }

        private readonly IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected override void resolveParameters(object parameter)
        {
            Interface_InitializeParameters(parameter);
        }

        //#region Interface Delegates
        public void Save(BASELINE_ITEMProgress progress_entity)
        {
            MainViewModel.Save(progress_entity);
        }

        public void Delete(BASELINE_ITEMProgress progress_entity)
        {
            MainViewModel.Delete(progress_entity);
        }
        #endregion

        public void Interface_InitializeParameters(object parameter)
        {
            var receiveParameter = (TripleEntitiesParameter<PROJECT, IAmBaseline, object>)parameter;
            loadPROJECT = receiveParameter.GetFirstEntity();
            loadBASELINE = (BASELINE)receiveParameter.GetSecondEntity();
            viewType = (DeliverablesViewType)receiveParameter.GetThirdEntity();
            InternalNumberMode = DeliverableInternalNumberMode.Default;

            if (loadPROJECT != null)
                isQueryForLiveStatus = true;

            Allow_Drag_Drop = false;
            //Is_Autofill_Internal_Number = true;
        }

        protected override void initializeEntitiesLoadersDescription()
        {
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINES, BASELINEProjectionFunc, assign_baseline);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.SUBJOBS, SUBJOBProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.WORKPACKS, WORKPACKProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PHASES, PHASEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECT_REPORTS, PROJECT_REPORTProjectionFunc, null, true);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc, assign_progress);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESS_ITEMS, PROGRESS_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DOCTYPES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DELIVERABLES_STATUSES, DELIVERABLES_STATUSProjectionFunc);
            loaderCollection.AddLoaderDescription<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.USERS);
        }

        private void assign_baseline(BASELINE entity)
        {
            if (entity == null && !SupressCompulsoryEntityNotFoundMessage)
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage("Live baseline not found")));

            loadBASELINE = entity;
            if (entity.BUDGETED_UNITS != null && entity.BUDGETED_UNITS > 0)
                SetBaselineLockUnlock?.Invoke(true);
            else
                SetBaselineLockUnlock?.Invoke(false);
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
                return query => query.Where(x => x.GUID == loadBASELINE.GUID_PROJECT);
        }

        private Func<IRepositoryQuery<BASELINE>, IQueryable<BASELINE>> BASELINEProjectionFunc()
        {
            if (isQueryForLiveStatus)
                return
                    query =>
                        query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == BaselineStatus.Live);
            else
                return query => query.Where(x => x.GUID == loadBASELINE.GUID);
        }

        private Func<IRepositoryQuery<SUBJOB>, IQueryable<SUBJOB>> SUBJOBProjectionFunc()
        {
            //not ready for this yet because some active projects are still using legacy subjob name
            //if (viewType == DeliverablesViewType.Direct)
            //    return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && (x.PHASE != null && x.PHASE.PHASE_TYPE == PhaseType.Design && x.PHASE.CHARGE_TYPE == ChargeType.Direct));
            //else if (viewType == DeliverablesViewType.Indirect)
            //    return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && (x.PHASE != null && x.PHASE.PHASE_TYPE == PhaseType.Design && x.PHASE.CHARGE_TYPE == ChargeType.Indirect));
            //else
            //    return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && (x.PHASE != null && x.PHASE.PHASE_TYPE == PhaseType.Design));
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK>> WORKPACKProjectionFunc()
        {
            return query => query.Where(x => x.SUBJOB.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<PHASE>, IQueryable<PHASE>> PHASEProjectionFunc()
        {
            return query => query.Where(x => x.PHASE_TYPE == PhaseType.Design);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<DELIVERABLES_STATUS>, IQueryable<DELIVERABLES_STATUS>> DELIVERABLES_STATUSProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> PROGRESSProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.TYPE == ProgressType.Design && x.STATUS == ProgressStatus.Live);
        }

        private Func<IRepositoryQuery<PROGRESS_ITEM>, IQueryable<PROGRESS_ITEM>> PROGRESS_ITEMProjectionFunc()
        {
            if (livePROGRESS == null)
                return query => query.Where(x => x.GUID_PROGRESS == Guid.Empty);
            else
                return query => query.Where(x => x.GUID_PROGRESS == livePROGRESS.GUID);
        }

        private Func<IRepositoryQuery<PROJECT_REPORT>, IQueryable<PROJECT_REPORT>> PROJECT_REPORTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.REPORT_TYPE == ReportType.Baseline_Report.ToString());
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEMProgress>>
            specifyMainViewModelProjection()
        {
            return query => ProgressQueries.OffsiteDirectProgressItemTransformation(base_entity_query(query), loadPROJECT, livePROGRESS, RATECollection, PROGRESS_ITEMCollection, null, false, null, InternalNumberMode);
        }

        public Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEM>> BaseEntityQueryCallBack { get; set; }
        private IQueryable<BASELINE_ITEM> base_entity_query(IRepositoryQuery<BASELINE_ITEM> query)
        {
            if (BaseEntityQueryCallBack != null)
                return BaseEntityQueryCallBack(query);

            if(viewType == DeliverablesViewType.Direct)
                return query.Where(x => x.GUID_BASELINE == load_context_guid && x.PHASE != null && x.PHASE.CHARGE_TYPE == ChargeType.Direct);
            else if(viewType == DeliverablesViewType.Indirect)
                return query.Where(x => x.GUID_BASELINE == load_context_guid && x.PHASE != null && x.PHASE.CHARGE_TYPE == ChargeType.Indirect);
            else
                return query.Where(x => x.GUID_BASELINE == load_context_guid);
        }

        public Action<IEnumerable<BASELINE_ITEMProgress>> OnReportablesLoadedCallBack { get; set; }
        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<BASELINE_ITEMProgress> entities)
        {
            FilterTreeViewModel = FiltersSettings.GetBASELINE_ITEMProgressFilterTree(this, entities);
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertyChanged(x => x.FilterTreeViewModel)));
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = OnBeforeEntitySaved;
            MainViewModel.ApplyEntityPropertiesToProjectionCallBack = OnEntitiesSavedCallBack;
            MainViewModel.AdditionalValidateCellCallBack = AdditionalValidateCellCallBack;
            MainViewModel.ValidateSetValueIsContinueCallBack = validateSetValueCallBack;
            MainViewModel.OnFillOrCellLevelPasting = OnFillOrCellLevelPasting;
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

        public void OnFillOrCellLevelPasting(IEnumerable<BASELINE_ITEMProgress> entities, string fieldName)
        {
            if(fieldName.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity.Entity.GUID_AREA)) ||
                fieldName.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity.Entity.GUID_DOCTYPE)) ||
                fieldName.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity.Entity.GUID_DISCIPLINE)))
            {
                foreach (BASELINE_ITEMProgress entity in entities)
                {
                    if (entity.IsInternalNumberEditable && !entity.IsInternalNumberManualOnly)
                    {
                        string oldValue = entity.Entity.Entity.INTERNAL_NUM;
                        string newValue = generateInternalNumber(entity);
                        entity.Entity.Entity.INTERNAL_NUM = newValue;

                        MainViewModel.EntitiesUndoRedoManager.AddUndo(entity, "Entity.Entity." + BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity.Entity.INTERNAL_NUM), oldValue, newValue, EntityMessageType.Changed);
                    }
                }
            }
        }

        public Action InterfacePauseUndoRedoManagerCallBack { get; set; }

        /// <summary>
        /// this view model can be used in variation or default collection view, only default collection view specific properties are set here
        /// </summary>
        private void SetViewSpecificProperties()
        {
            SelectedEntities = DisplaySelectedEntities;
            DefaultPhaseInternalNumber = BluePrintsResources.Default_Design_Phase;
        }

        #region Collection Call Backs
        public override void OnAfterAuxiliaryEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
            if (changedType == typeof(PROGRESS_ITEM))
            {
                FullRefreshWithoutClearingUndoRedo();
                return;
            }

            base.OnAfterAuxiliaryEntitiesChanged(key, changedType, messageType, sender, isBulkRefresh);
        }

        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public bool OnBeforeEntitySaved(BASELINE_ITEMProgress entity)
        {
            PhaseType? phaseType = null;
            ChargeType? chargeType = null;

            PHASE defaultPHASE = PHASECollection.FirstOrDefault(x => (x.PHASE_TYPE != null && x.PHASE_TYPE == PhaseType.Design) && (x.CHARGE_TYPE != null && x.CHARGE_TYPE == ChargeType.Direct));
            if (viewType == DeliverablesViewType.Direct)
            {
                phaseType = PhaseType.Design;
                chargeType = ChargeType.Direct;
                if (defaultPHASE != null)
                    entity.Phase_Guid = defaultPHASE.GUID;
            }
            else if(viewType == DeliverablesViewType.Indirect)
            {
                phaseType = PhaseType.Design;
                chargeType = ChargeType.Indirect;
                PHASE indirectPHASE = PHASECollection.FirstOrDefault(x => (x.PHASE_TYPE != null && x.PHASE_TYPE == PhaseType.Design) && (x.CHARGE_TYPE != null && x.CHARGE_TYPE == ChargeType.Indirect));
                if (indirectPHASE != null)
                    entity.Phase_Guid = indirectPHASE.GUID;
            }
            else if (entity.Phase_Guid == null && defaultPHASE != null)
            {
                entity.Phase_Guid = defaultPHASE.GUID;
            }

            //if(entity.IsInternalNumberEditable)
            //    entity.Entity.Entity.INTERNAL_NUM = generateInternalNumber(entity);
            BluePrintsDataUtils.OnBeforeSavedGenerateAndAssignSubjob(loadPROJECT, PHASECollection, AREACollection, SUBAREACollection, entity, SUBJOBSCollectionViewModel, phaseType, chargeType);
            BluePrintsDataUtils.OnBeforeSavedGenerateAndAssignWorkpack(entity, WORKPACKSCollectionViewModel, SUBJOBCollection, DISCIPLINECollection);
            
            //entity.Entity.Entity.GUID_ESTIMATION_DIRECT = loadESTIMATION_DIRECT.GUID;
            return true;
        }

        public Action<BASELINE_ITEMProgress> ApplyViewSpecificPropertiesToEntityCallBack { get; set; }
        protected override void OnBeforeApplyProjectionPropertiesToEntity(BASELINE_ITEMProgress projectionEntity, BASELINE_ITEM entity)
        {
            //if (projectionEntity.Entity.Entity.GUID_PHASE == null)
            //{
            //    IEnumerable<PHASE> phase_collection = loaderCollection.GetCollection<PHASE>();
            //    if (phase_collection != null)
            //    {
            //        PHASE default_design_phase = phase_collection.FirstOrDefault(x => x.INTERNAL_NUM == DefaultPhaseInternalNumber);
            //        if (default_design_phase != null)
            //            projectionEntity.Entity.Entity.GUID_PHASE = default_design_phase.GUID;
            //    }
            //}

            if (ApplyViewSpecificPropertiesToEntityCallBack == null)
                projectionEntity.Entity.Entity.GUID_BASELINE = loadBASELINE.GUID;
            else
                ApplyViewSpecificPropertiesToEntityCallBack.Invoke(projectionEntity);

            //because TProjection is not IProjection<TMainEntity>, do it manually here
            DataUtils.ShallowCopy(entity, projectionEntity.Entity.Entity);
            base.OnBeforeApplyProjectionPropertiesToEntity(projectionEntity, entity);
        }

        public void OnEntitiesSavedCallBack(Guid primaryKey, BASELINE_ITEMProgress projectionEntity, BASELINE_ITEM entity, bool isNewEntity)
        {
            projectionEntity.Entity.Entity.GUID_ORIGINAL = entity.GUID_ORIGINAL;
        }
        #endregion

        #region View Localization Helpers
        private void setNestedValueWithUndo(BASELINE_ITEMProgress entity, string propertyName, object newValue)
        {
            string localizedPropertyName = localizeColumnFieldName(propertyName);
            var oldValue = DataUtils.GetNestedValue(localizedPropertyName, entity);
            DataUtils.SetNestedValue(localizedPropertyName, entity, newValue);
            AddUndo(entity, localizedPropertyName, oldValue, newValue, EntityMessageType.Changed);
        }

        private string localizeColumnFieldName(string fieldName)
        {
            return Base_Entity_String + DataUtils.FormatColumnFieldname(fieldName);
        }
        #endregion

        #region View Behavior
        protected DevExpress.Mvvm.IDialogService MapDeliverableDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("MapDeliverableDialog"); }
        }

        public bool CanMapClientNumber()
        {
            return MainViewModel != null && MainViewModel.Entities.Count() > 0;
        }

        public void MapClientNumber()
        {
            List<ClientNumberAssignment> internal_number_mapping = new List<ClientNumberAssignment>();

            foreach (BASELINE_ITEMProgress entity in MainViewModel.Entities)
            {
                internal_number_mapping.Add(ViewModelSource.Create(() => new ClientNumberAssignment() { INTERNAL_NUM = entity.Entity.Entity.INTERNAL_NUM, CLIENT_NUM = entity.Entity.Entity.CLIENT_NUM }));
            }

            MapDeliverablesClientNumberDialogViewModel<ClientNumberAssignment> internal_number_remap_view_model = MapDeliverablesClientNumberDialogViewModel<ClientNumberAssignment>.CreateViewModel(internal_number_mapping);
            if (MapDeliverableDialogService.ShowDialog(MessageButton.OKCancel, "Re-Assign Client Number", "MapDeliverablesClientNumber", internal_number_remap_view_model) == MessageResult.OK)
            {
                IEnumerable<ClientNumberAssignment> reassignments = internal_number_mapping.Where(x => x.CLIENT_NUM != null && x.CLIENT_NUM != string.Empty);

                List<BASELINE_ITEMProgress> reassigned_deliverables = new List<BASELINE_ITEMProgress>();
                foreach (ClientNumberAssignment reassignment in reassignments)
                {
                    BASELINE_ITEMProgress user_remapped_deliverable = MainViewModel.Entities.FirstOrDefault(x => x.Entity.Entity.INTERNAL_NUM == reassignment.INTERNAL_NUM);
                    if (user_remapped_deliverable != null)
                    {
                        user_remapped_deliverable.Entity.Entity.CLIENT_NUM = reassignment.CLIENT_NUM;
                        reassigned_deliverables.Add(user_remapped_deliverable);
                    }
                }

                if (reassigned_deliverables.Count > 0)
                {
                    MainViewModel.BulkSave(reassigned_deliverables);
                    MessageBoxService.ShowMessage(reassigned_deliverables.Count + " internal number re-assigned");
                }
            }
        }

        public bool validateSetValueCallBack(BASELINE_ITEMProgress entity, string column_name, object newValue)
        {
            string fieldName = DataUtils.FormatColumnFieldname(column_name);
            //estimated hours field is disabled but just in case
            if (fieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEM().ESTIMATED_HOURS))
            {
                if (entity.Entity.Entity.BY_DURATION && ((decimal)newValue) > 0)
                    return false;
                else if ((decimal)newValue < entity.MinEstimateUnits)
                    return false;
            }
            else if (fieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEM().BY_DURATION))
            {
                if (entity.Earned_Units_Total > 0)
                    return false;
            }
            else if (fieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEM().INTERNAL_NUM))
            {
                return entity.IsInternalNumberEditable;
            }

            return true;
        }

        public void AdditionalValidateCellCallBack(GridCellValidationEventArgs e)
        {
            string fieldName = DataUtils.FormatColumnFieldname(e.Column.FieldName);
            string error_message = Interface_AdditionalValidateCellCallBack((BASELINE_ITEMProgress)e.Row, e.Value, fieldName);
            if(error_message != string.Empty)
            {
                e.IsValid = false;
                e.ErrorContent = error_message;
                e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Critical;
            }
        }

        public string Interface_AdditionalValidateCellCallBack(BASELINE_ITEMProgress validateEntity, object currentValue, string fieldName)
        {
            string error_message = string.Empty;
            //estimated hours field is disabled but just in case
            if (fieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEM().ESTIMATED_HOURS))
            {
                if (validateEntity.Entity.Entity.BY_DURATION && ((decimal)currentValue) > 0)
                    error_message = "Cannot set estimated hours when deliverable is by duration";
            }
            else if (fieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEM().BY_DURATION))
            {
                if (validateEntity.Earned_Units_Total > 0)
                    error_message = "Cannot change deliverable tracking type when percentage is already earned";
            }

            return error_message;
        }

        public Action<BASELINE_ITEMProgress, string, object, object, EntityMessageType> InterfaceAddUndoRedoCallBack { get; set; }
        public void AddUndo(BASELINE_ITEMProgress changedEntity, string propertyName, object oldValue, object newValue, EntityMessageType messageType)
        {
            if (InterfaceAddUndoRedoCallBack != null)
                InterfaceAddUndoRedoCallBack(changedEntity, propertyName == null ? null : localizeColumnFieldName(propertyName), oldValue, newValue, messageType);
            else
                MainViewModel.EntitiesUndoRedoManager.AddUndo(changedEntity, propertyName == null ? null : localizeColumnFieldName(propertyName), oldValue, newValue, messageType);
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

        public void OnCustomColumnSort(CustomColumnSortEventArgs e)
        {
            if (e.Column.FieldName == Base_Entity_String + BindableBase.GetPropertyName(() => new BASELINE_ITEM().ESTIMATED_HOURS))
            {
                decimal decimal_value1 = (decimal)e.Value1;
                decimal decimal_value2 = (decimal)e.Value2;

                e.Result = decimal_value1.CompareTo(decimal_value2);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Allow undo-redo behavior to be added for automated cell value changing. This behavior doesn't have to be applied on new row because AddUndo for EntityMessageType.Added is already handling this
        /// </summary>
        protected override void CellValueExistingRowChanging(CellValueChangedEventArgs e)
        {
            Interface_CellValueExistingRowChanging(e.Column.FieldName, e.Value, (BASELINE_ITEMProgress)e.Row);
            base.CellValueExistingRowChanging(e);
        }

        public void Interface_CellValueExistingRowChanging(string field_name, object new_value, BASELINE_ITEMProgress active_progress)
        {
            field_name = DataUtils.FormatColumnFieldname(field_name);
            if (field_name == BindableBase.GetPropertyName(() => new BASELINE_ITEM().BY_DURATION))
            {
                if ((bool)new_value)
                {
                    decimal oldValue = active_progress.Entity.Entity.ESTIMATED_HOURS;
                    if (oldValue > 0)
                    {
                        decimal newValue = 0;
                        string estimatedHoursFieldName = BindableBase.GetPropertyName(() => new BASELINE_ITEM().ESTIMATED_HOURS);
                        active_progress.Entity.Entity.ESTIMATED_HOURS = newValue;
                        PauseUndoRedo();
                        AddUndo(active_progress, estimatedHoursFieldName, oldValue, newValue, EntityMessageType.Changed);
                    }
                }
            }
            else if (field_name == BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_AREA))
            {
                Guid? oldValue = active_progress.Entity.Entity.GUID_SUBAREA;
                if (oldValue != null)
                {
                    Guid? newValue = (Guid?)null;
                    string subAreaFieldName = BindableBase.GetPropertyName(() => new BASELINE_ITEM().SubAreaGuid);
                    active_progress.Entity.Entity.GUID_SUBAREA = newValue;
                    PauseUndoRedo();
                    AddUndo(active_progress, subAreaFieldName, oldValue, newValue, EntityMessageType.Changed);
                }
            }
            else if (field_name == BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_DOCTYPE) || field_name == BindableBase.GetPropertyName(() => new BASELINE_ITEM().DELIVERABLE_TYPE))
            {
                Guid? oldValue = active_progress.Entity.Entity.GUID_STATUS;
                if (oldValue != null)
                {
                    Guid? newValue = (Guid?)null;
                    string deliverableStatusFieldName = BindableBase.GetPropertyName(() => new BASELINE_ITEM().DeliverableStatusGuid);
                    active_progress.Entity.Entity.GUID_STATUS = newValue;
                    PauseUndoRedo();
                    AddUndo(active_progress, deliverableStatusFieldName, oldValue, newValue, EntityMessageType.Changed);
                }
            }
        }

        protected override void CellValueNewRowChanging(CellValueChangedEventArgs e)
        {
            Interface_CellValueNewRowChanging(e.Column.FieldName, e.Value, (BASELINE_ITEMProgress)e.Row);
            base.CellValueNewRowChanging(e);
        }

        public void Interface_CellValueNewRowChanging(string field_name, object new_value, BASELINE_ITEMProgress active_progress)
        {
            field_name = DataUtils.FormatColumnFieldname(field_name);

            if (field_name == BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_AREA))
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
            }

            if (field_name == BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_DOCTYPE))
            {
                var chosenDOCTYPE = DOCTYPECollection.FirstOrDefault(entity => entity.GUID == (Guid)new_value);
                if (chosenDOCTYPE != null)
                {
                    if (chosenDOCTYPE.GUID_DDEPARTMENT != null)
                        active_progress.Entity.Entity.GUID_DEPARTMENT = chosenDOCTYPE.DEPARTMENT.GUID;

                    //Baseline and Department is required immediately for deliverables status selection
                    active_progress.Entity.Entity.BASELINE = loadBASELINE;
                    active_progress.Entity.Entity.DOCTYPE = DOCTYPECollection.FirstOrDefault(x => x.GUID == (Guid)new_value);
                    active_progress.Update();
                }
            }

            if (field_name == BindableBase.GetPropertyName(() => new BASELINE_ITEM().DELIVERABLE_TYPE))
            {
                active_progress.Update();
            }

            if (field_name == BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_SUBJOB))
            {
                var chosenSUBJOB = SUBJOBCollection.FirstOrDefault(entity => entity.GUID == (Guid)new_value);
                if (chosenSUBJOB != null)
                {
                    active_progress.Entity.Entity.GUID_AREA = chosenSUBJOB.GUID_DAREA;
                    //Area is required immediately for subarea selection
                    active_progress.Entity.Entity.AREA = AREACollection.FirstOrDefault(x => x.GUID == chosenSUBJOB.GUID_DAREA);
                    active_progress.Entity.Entity.GUID_SUBAREA = chosenSUBJOB.GUID_DSUBAREA;
                    active_progress.Entity.Entity.GUID_PHASE = chosenSUBJOB.PHASE != null
                        ? chosenSUBJOB.GUID_DPHASE
                        : null;

                    active_progress.Update();
                }
            }
        }

        /// <summary>
        /// Refresh all min max units for converter to do estimated hours validation
        /// </summary>
        public void CellValueChanged(CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == Base_Entity_String + BindableBase.GetPropertyName(() => new BASELINE_ITEM().ESTIMATED_HOURS))
                this.RaisePropertiesChanged();

            Interface_CellValueChanged(e.Column.FieldName, (BASELINE_ITEMProgress)e.Row);
        }

        public void Interface_CellValueChanged(string field_name, BASELINE_ITEMProgress projection)
        {
            field_name = DataUtils.FormatColumnFieldname(field_name);
            if (field_name == BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_DOCTYPE) ||
                field_name == BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_AREA) ||
                field_name == BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_DEPARTMENT) ||
                field_name == BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_DISCIPLINE))
            {
                if (projection.IsInternalNumberEditable && !projection.IsInternalNumberManualOnly)
                    projection.Entity.Entity.INTERNAL_NUM = generateInternalNumber(projection);

                projection.Update();
            }
        }
        #endregion

        #region View Commands
        public bool IsBASELINELocked
        {
            get
            {
                if (loadBASELINE == null)
                    return true;
                else
                    return loadBASELINE.BUDGETED_UNITS != null && loadBASELINE.BUDGETED_UNITS > 0;
            }
            set
            {
                LockUnlockBASELINE(value);
            }
        }

        private void LockUnlockBASELINE(bool isLock)
        {
            var BASELINECollectionViewModel = (CollectionViewModel<BASELINE, BASELINE, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<BASELINE>();
            if (!isLock)
                loadBASELINE.BUDGETED_UNITS = 0;
            else
            {
                decimal totalEstimatedHours = MainViewModel.Entities.Sum(x => x.Entity.Entity.ESTIMATED_HOURS);
                loadBASELINE.BUDGETED_UNITS = totalEstimatedHours;
            }

            BASELINECollectionViewModel.Save(loadBASELINE);
            SetBaselineLockUnlock?.Invoke(isLock);
            this.RaisePropertiesChanged();
        }

        public bool CanDuplicate()
        {
            if (MainViewModel == null || SelectedEntities == null || SelectedEntities.Count() == 0)
                return false;

            return true;
        }

        public bool CanInsert()
        {
            return CanDuplicate();
        }
        
        public void Insert()
        {
            if (!_isProcessingMultiple)
                PauseUndoRedo();

            List<BASELINE_ITEMProgress> newEntities = getNewProgressEntities(1, true, MainViewModel.Entities, SelectedEntities);
            newEntities = concatenateNewEntitiesWithExistingRenameEntities(newEntities, EditableAllEntities);

            MainViewModel.BulkSave(newEntities);
            //Add undo must happen after save so that variation can pick it up
            foreach (BASELINE_ITEMProgress newEntity in newEntities)
                AddUndo(newEntity, null, null, null, EntityMessageType.Added);

            if (!_isProcessingMultiple)
                UnpauseUndoRedo();
        }

        /// <summary>
        /// Concatenate entities to be saved and entities to be renamed.
        /// </summary>
        /// <param name="newEntities">Entities to be saved.</param>
        /// <returns></returns>
        private List<BASELINE_ITEMProgress> concatenateNewEntitiesWithExistingRenameEntities(List<BASELINE_ITEMProgress> newEntities, IEnumerable<BASELINE_ITEMProgress> existing_entities)
        {
            List<BASELINE_ITEMProgress> concatenatedEntities = new List<BASELINE_ITEMProgress>();
            concatenatedEntities.AddRange(newEntities);

            List<string> processedValueToFillStringOnly = new List<string>();
            foreach(BASELINE_ITEMProgress entity in newEntities.OrderBy(x => x.Entity.Entity.INTERNAL_NUM))
            {
                long lowestUnsavedNumericValue = 0;
                long highestUnsavedNumericValue = 0;

                int numericFieldLength = 0;
                long arbitraryNumericValue = 0;
                string valueToFill = entity.Entity.Entity.INTERNAL_NUM;
                if (valueToFill == string.Empty)
                    return concatenatedEntities;

                string valueToFillStringOnly = StringFormatUtils.ParseStringIntoComponents(valueToFill, out numericFieldLength, out arbitraryNumericValue);

                List<BASELINE_ITEMProgress> relatedNewEntities = newEntities.Where(x => x.Entity.Entity.INTERNAL_NUM.Contains(valueToFillStringOnly)).ToList();
                BASELINE_ITEMProgress smallestNumberEntity = relatedNewEntities.First();
                BASELINE_ITEMProgress largestNumberEntity = relatedNewEntities.Last();

                string smallestInternalNum = smallestNumberEntity.Entity.Entity.INTERNAL_NUM;
                string largestInternalNum = largestNumberEntity.Entity.Entity.INTERNAL_NUM;

                valueToFillStringOnly = StringFormatUtils.ParseStringIntoComponents(smallestInternalNum, out numericFieldLength, out lowestUnsavedNumericValue);
                valueToFillStringOnly = StringFormatUtils.ParseStringIntoComponents(largestInternalNum, out numericFieldLength, out highestUnsavedNumericValue);
                if(!processedValueToFillStringOnly.Contains(valueToFillStringOnly))
                {
                    processedValueToFillStringOnly.Add(valueToFillStringOnly);
                    List<BASELINE_ITEMProgress> renameEntities = getRenameExistingEntities(valueToFillStringOnly, lowestUnsavedNumericValue, highestUnsavedNumericValue, existing_entities);
                    concatenatedEntities.AddRange(renameEntities);
                }
            }

            return concatenatedEntities;
        }

        public void Duplicate()
        {
            if (!_isProcessingMultiple)
                PauseUndoRedo();

            List<BASELINE_ITEMProgress> newEntities = getNewProgressEntities(1, false, MainViewModel.Entities, SelectedEntities);

            MainViewModel.BulkSave(newEntities);

            //Add undo must happen after save so that variation can pick it up
            foreach (BASELINE_ITEMProgress newEntity in newEntities)
                AddUndo(newEntity, null, null, null, EntityMessageType.Added);

            if (!_isProcessingMultiple)
                UnpauseUndoRedo();
        }

        /// <summary>
        /// Identify entities which internal number require to be named.
        /// </summary>
        /// <param name="renameStringOnly">Rename internal number string component only.</param>
        /// <param name="startNumber">Start of internal number to be named</param>
        /// <param name="endNumber">End if internal number to be named</param>
        /// <returns></returns>
        private List<BASELINE_ITEMProgress> getRenameExistingEntities(string renameStringOnly, long startNumber, long endNumber, IEnumerable<BASELINE_ITEMProgress> existing_editable_entities)
        {
            long valueToAdd = (endNumber - startNumber) + 1;
            List<BASELINE_ITEMProgress> renameEntities = new List<BASELINE_ITEMProgress>();
            foreach (BASELINE_ITEMProgress entity in existing_editable_entities)
            {
                string stringValueToFill = entity.Entity.Entity.INTERNAL_NUM;
                if (stringValueToFill == null)
                    continue;

                if (!stringValueToFill.Contains(renameStringOnly))
                    continue;

                int numericFieldLength = 0;
                long valueToFillNumberOnly = 0;
                string valueToFillStringOnly = StringFormatUtils.ParseStringIntoComponents(stringValueToFill, out numericFieldLength, out valueToFillNumberOnly);

                if (valueToFillNumberOnly >= startNumber)
                {
                    long increasedNumber = valueToFillNumberOnly + valueToAdd;
                    string oldInternalNum = entity.Entity.Entity.INTERNAL_NUM;
                    string internal_number_fieldname = BindableBase.GetPropertyName(() => new BASELINE_ITEM().INTERNAL_NUM);
                    entity.Entity.Entity.INTERNAL_NUM = StringFormatUtils.AppendStringWithEnumerator(valueToFillStringOnly, increasedNumber, numericFieldLength);
                    AddUndo(entity, internal_number_fieldname, oldInternalNum, entity.Entity.Entity.INTERNAL_NUM, EntityMessageType.Changed);
                    renameEntities.Add(entity);
                }
            }

            return renameEntities;
        }

        private List<BASELINE_ITEMProgress> getNewProgressEntities(int timesToDuplicate, bool isInsert, IEnumerable<BASELINE_ITEMProgress> all_entities, IEnumerable<BASELINE_ITEMProgress> selected_entities)
        {
            List<BASELINE_ITEMProgress> unsavedEntities = new List<BASELINE_ITEMProgress>();
            for(int i = 0; i < timesToDuplicate; i++)
            {
                foreach (var selectedEntity in selected_entities)
                {
                    var newProjection = new BASELINE_ITEMProgress();
                    DataUtils.ShallowCopy(newProjection.Entity.Entity, selectedEntity.Entity.Entity);
                    newProjection.Entity.EntityKey = Guid.Empty;
                    newProjection.Entity.Entity.GUID_ORIGINAL = Guid.Empty;

                    //because this function is used in variation, let ApplyProjection handle this
                    newProjection.Entity.Entity.GUID_BASELINE = null;
                    newProjection.Entity.Entity.GUID_VARIATION = null;

                    //when duplicated by variation this should be 0
                    if(BaseEntityQueryCallBack != null)
                        newProjection.Entity.Entity.ESTIMATED_HOURS = 0;
                    else
                        newProjection.Entity.Entity.ESTIMATED_HOURS = IsBASELINELocked ? 0 : selectedEntity.Entity.Entity.ESTIMATED_HOURS;
                    
                    newProjection.Entity.Entity.DC_HOURS = 0;
                    var selectedAREA = AREACollection.FirstOrDefault(x => x.GUID == newProjection.Entity.Entity.GUID_AREA);
                    var selectedDISCIPLINE =
                        DISCIPLINECollection.FirstOrDefault(x => x.GUID == newProjection.Entity.Entity.GUID_DISCIPLINE);
                    var selectedDOCTYPE =
                        DOCTYPECollection.FirstOrDefault(x => x.GUID == newProjection.Entity.Entity.GUID_DOCTYPE);

                    newProjection.Entity.Entity.INTERNAL_NUM = 
                        BluePrintsDataUtils.GetNewInternalNumber(all_entities.Select(x => x.Entity), unsavedEntities.Select(x => x.Entity), selectedEntity.Entity.Entity.INTERNAL_NUM, selected_entities.Select(x => x.Entity), isInsert);

                    //newProjection.Entity.Entity.INTERNAL_NUM = string.Empty;
                    AddUndo(newProjection, null, null, null, EntityMessageType.Added);
                    unsavedEntities.Add(newProjection);
                }
            }

            return unsavedEntities;
        }

        public bool CanDuplicateMultiple(BarEditItem barEdit)
        {
            if (SelectedEntities == null || SelectedEntities.Count() == 0)
                return false;

            return true;
        }

        public bool CanInsertMultiple(BarEditItem barEdit)
        {
            return CanDuplicateMultiple(barEdit);
        }

        private bool _isProcessingMultiple;

        public void DuplicateMultiple(BarEditItem barEdit)
        {
            PauseUndoRedo();
            _isProcessingMultiple = true;
            var timesToDuplicate = 0;
            List<BASELINE_ITEMProgress> newEntities = new List<BASELINE_ITEMProgress>();
            if (int.TryParse(barEdit.EditValue.ToString(), out timesToDuplicate))
            {
                List<BASELINE_ITEMProgress> currentEnumerationSaveEntities = getNewProgressEntities(timesToDuplicate, false, MainViewModel.Entities, SelectedEntities);
                newEntities.AddRange(currentEnumerationSaveEntities);
            }

            MainViewModel.BulkSave(newEntities);
            _isProcessingMultiple = false;
            UnpauseUndoRedo();
        }

        public void InsertMultiple(BarEditItem barEdit)
        {
            PauseUndoRedo();
            _isProcessingMultiple = true;
            var timesToInsert = 0;
            List<BASELINE_ITEMProgress> newEntities = new List<BASELINE_ITEMProgress>();
            if (int.TryParse(barEdit.EditValue.ToString(), out timesToInsert))
            {
                List<BASELINE_ITEMProgress> currentEnumerationSaveEntities = getNewProgressEntities(timesToInsert, true, MainViewModel.Entities, SelectedEntities);
                newEntities.AddRange(currentEnumerationSaveEntities);
            }

            newEntities = concatenateNewEntitiesWithExistingRenameEntities(newEntities, MainViewModel.Entities);
            MainViewModel.BulkSave(newEntities);
            _isProcessingMultiple = false;
            UnpauseUndoRedo();
        }
        
        public bool CanAutoPopulate(object button)
        {
            if (SelectedEntities == null || SelectedEntities.Count() == 0)
                    return false;

            return true;
        }

        public void AutoPopulate(object button)
        {
            PauseUndoRedo();
            var info = GridPopupMenuBase.GetGridMenuInfo((DependencyObject) button) as GridMenuInfo;
            if (info.Column == null)
                return;

            var areaFieldName = localizeColumnFieldName(BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_AREA));
            var subAreaFieldName = localizeColumnFieldName(BindableBase.GetPropertyName(() => new BASELINE_ITEM().SubAreaGuid));
            var subjobFieldName = localizeColumnFieldName(BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_SUBJOB));
            var workpackFieldName = localizeColumnFieldName(BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_WORKPACK));
            var internalNumberFieldName = localizeColumnFieldName(BindableBase.GetPropertyName(() => new BASELINE_ITEM().INTERNAL_NUM));

            var entitiesToSave = new List<BASELINE_ITEMProgress>();
            string fieldName = localizeColumnFieldName(info.Column.FieldName);

            if (fieldName == internalNumberFieldName)
                foreach (var entity in SelectedEntities)
                    entity.Entity.Entity.INTERNAL_NUM = string.Empty;

            foreach (var entity in SelectedEntities)
            {
                var entitySUBJOB = SUBJOBCollection.FirstOrDefault(x => x.GUID == entity.Entity.Entity.GUID_SUBJOB);
                if (fieldName == internalNumberFieldName && entity.IsInternalNumberEditable)
                {
                    string internalNumber = generateInternalNumber(entity);
                    setNestedValueWithUndo(entity, fieldName, internalNumber);
                    entitiesToSave.Add(entity);
                }
                else if (fieldName == areaFieldName || fieldName == subAreaFieldName)
                {
                    if (entitySUBJOB == null)
                        continue;

                    if (fieldName == areaFieldName)
                        setNestedValueWithUndo(entity, fieldName, entitySUBJOB.GUID_DAREA);
                    else if(fieldName == subAreaFieldName)
                        setNestedValueWithUndo(entity, fieldName, entitySUBJOB.GUID_DSUBAREA);

                    entitiesToSave.Add(entity);
                }
                else if (fieldName == subjobFieldName)
                {
                    if (entity.Entity.Entity.GUID_AREA == null || entity.Entity.Entity.GUID_DISCIPLINE == null)
                        continue;

                    Guid? phase_guid;
                    string internalName = BluePrintsDataUtils.SUBJOB_Generate_InternalNumber(
                        entity.Entity.Entity.GUID_AREA, entity.Entity.Entity.GUID_SUBAREA, 
                        loadPROJECT, AREACollection, SUBAREACollection, out phase_guid, entity.Entity.Entity.GUID_PHASE, PHASECollection);

                    if (internalName == string.Empty)
                        return;

                    var findSUBJOB =
                        SUBJOBCollection.FirstOrDefault(
                            x =>
                                x.INTERNAL_NAME1 == internalName);

                    if (findSUBJOB == null)
                    {
                        var newSUBJOB = new SUBJOB();

                        List<AREA> sub_area_collection = SUBAREACollection.ToList();
                        AREA defaultSubArea = sub_area_collection.Count() == 0 ? null : sub_area_collection.FirstOrDefault(x => x.INTERNAL_NUM == BluePrintsResources.Default_Sub_Area);

                        newSUBJOB.GUID_PROJECT = loadPROJECT.GUID;
                        newSUBJOB.GUID_DAREA = entity.Entity.Entity.GUID_AREA;
                        newSUBJOB.GUID_DSUBAREA = entity.Entity.Entity.GUID_SUBAREA == null ? defaultSubArea != null ? defaultSubArea.GUID : (Guid?)null : entity.Entity.Entity.GUID_SUBAREA;
                        newSUBJOB.GUID_DPHASE = entity.Entity.Entity.GUID_PHASE;

                        newSUBJOB.INTERNAL_NAME1 = internalName; 
                        newSUBJOB.STARTDATE = DateTime.Now;
                        newSUBJOB.ENDDATE =
                            BluePrintsDataUtils.SUBJOB_Calculate_EndDate((DateTime) newSUBJOB.STARTDATE, loadPROJECT);
                        var reviewStartDate = (DateTime) newSUBJOB.STARTDATE;
                        var reviewEndDate = (DateTime) newSUBJOB.ENDDATE;
                        BluePrintsDataUtils.SUBJOB_Calculate_ReviewPeriod(ref reviewStartDate, ref reviewEndDate,
                            loadPROJECT, false);
                        newSUBJOB.REVIEWSTARTDATE = reviewStartDate;
                        newSUBJOB.REVIEWENDDATE = reviewEndDate;
                        newSUBJOB.AUTOGENERATED = true;
                        ((CollectionViewModel<SUBJOB, SUBJOB, Guid, IBluePrintsEntitiesUnitOfWork>)
                            loaderCollection.GetViewModel<SUBJOB>()).Save(newSUBJOB);

                        setNestedValueWithUndo(entity, fieldName, newSUBJOB.GUID);
                    }
                    else
                    {
                        setNestedValueWithUndo(entity, fieldName, findSUBJOB.GUID);
                    }

                    entitiesToSave.Add(entity);
                }
                else if(fieldName == workpackFieldName)
                {
                    if (entity.Entity.Entity.GUID_SUBJOB == null|| entity.Entity.Entity.GUID_DISCIPLINE == null)
                        continue;

                    WORKPACK queryWORKPACK = WORKPACKCollection.FirstOrDefault(x => x.GUID_SUBJOB == entity.Entity.Entity.GUID_SUBJOB && x.GUID_DISCIPLINE == entity.Entity.Entity.GUID_DISCIPLINE && x.DISCIPLINE_NUM == entity.Entity.Entity.DISCIPLINE_NUM);
                    if(queryWORKPACK != null)
                    {
                        setNestedValueWithUndo(entity, fieldName, queryWORKPACK.GUID);
                        entitiesToSave.Add(entity);
                    }
                    else
                    {
                        WORKPACK newWORKPACK = new WORKPACK();
                        newWORKPACK.GUID_SUBJOB = (Guid)entity.Entity.Entity.GUID_SUBJOB;
                        newWORKPACK.GUID_DISCIPLINE = (Guid)entity.Entity.Entity.GUID_DISCIPLINE;
                        newWORKPACK.DISCIPLINE_NUM = entity.Entity.Entity.DISCIPLINE_NUM;
                        BluePrintsDataUtils.WORKPACK_Populate_Name(newWORKPACK, SUBJOBCollection, DISCIPLINECollection);
                        ((CollectionViewModel<WORKPACK, WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork>)
                        loaderCollection.GetViewModel<WORKPACK>()).Save(newWORKPACK);

                        setNestedValueWithUndo(entity, fieldName, newWORKPACK.GUID);
                        entitiesToSave.Add(entity);
                    }
                }
            }

            MainViewModel.BulkSave(entitiesToSave);
            UnpauseUndoRedo();
            BackgroundRefresh();
        }

        private string generateInternalNumber(BASELINE_ITEMProgress projectionEntity)
        {
            AREA currentItemAREA = AREACollection.FirstOrDefault((x => x.GUID == projectionEntity.Entity.Entity.GUID_AREA));
            DISCIPLINE currentItemDISCIPLINE = DISCIPLINECollection.FirstOrDefault((x => x.GUID == projectionEntity.Entity.Entity.GUID_DISCIPLINE));
            DOCTYPE currentItemDOCTYPE = DOCTYPECollection.FirstOrDefault((x => x.GUID == projectionEntity.Entity.Entity.GUID_DOCTYPE));
            var internalNum = BluePrintsDataUtils.BASELINEITEM_Generate_InternalNumber(loadPROJECT,
                MainViewModel.Entities.Select(x => x.Entity.Entity), currentItemAREA, currentItemDISCIPLINE, currentItemDOCTYPE, projectionEntity.EntityKey);

            return internalNum;
        }
        #endregion

        #region Find and Replace
        private DevExpress.Mvvm.IDialogService BulkColumnEditDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("BulkColumnEditService"); }
        }

        public bool CanFindReplace(object button)
        {
            var info = GridPopupMenuBase.GetGridMenuInfo((DependencyObject)button) as GridMenuInfo;
            if (info == null)
                return false;

            if (info.Column == null)
                return false;

            if (info.Column.ReadOnly)
                return false;

            if (SelectedEntity == null || SelectedEntities.Count() < 2 || info.Column.ReadOnly == true)
                return false;

            var columnPropertyInfo = DataUtils.GetNestedPropertyInfo(localizeColumnFieldName(info.Column.FieldName), SelectedEntity);
            if (columnPropertyInfo.PropertyType == typeof(string))
            {
                var constraintString = DataUtils.GetConstraintPropertyStrings(SelectedEntity.GetType());
                if (constraintString == null)
                    constraintString = DataUtils.GetConstraintPropertyStrings(SelectedEntity.GetType().BaseType);

                var bulkEditDisabledString =
                    DataUtils.GetBulkEditDisabledPropertyStrings(SelectedEntity.GetType());
                if (bulkEditDisabledString == null)
                    bulkEditDisabledString =
                        DataUtils.GetBulkEditDisabledPropertyStrings(SelectedEntity.GetType().BaseType);

                if (constraintString != null && constraintString.Any(x => x == columnPropertyInfo.Name) ||
                    bulkEditDisabledString != null && bulkEditDisabledString.Any(x => x == columnPropertyInfo.Name))
                    return false;
                else
                    return true;
            }

            return false;
        }

        public void FindReplace(object button)
        {
            var info = GridPopupMenuBase.GetGridMenuInfo((DependencyObject)button) as GridMenuInfo;
            BASELINE_ITEMProgress first_selected_entity = SelectedEntities.First();
            string fieldName = localizeColumnFieldName(info.Column.FieldName);
            object find_nested_value = DataUtils.GetNestedValue(fieldName, first_selected_entity);

            string find_value;
            if (find_nested_value == null)
                find_value = string.Empty;
            else
                find_value = find_nested_value.ToString();

            if (find_value == string.Empty)
            {
                MessageBoxService.ShowMessage("Cannot find anything to replace");
                return;
            }

            var bulkFindAndReplaceViewModel = BulkFindAndReplaceViewModel.Create(find_value);

            List<BASELINE_ITEMProgress> save_entities = new List<BASELINE_ITEMProgress>();
            if (BulkColumnEditDialogService.ShowDialog(MessageButton.OKCancel, "Type in text to replace", "BulkFindAndReplace", bulkFindAndReplaceViewModel) == MessageResult.OK)
            {
                PauseUndoRedo();
                string new_find_value = bulkFindAndReplaceViewModel.FindValue;
                string replace_value = bulkFindAndReplaceViewModel.ReplaceValue;
                foreach (BASELINE_ITEMProgress selected_entity in SelectedEntities)
                {
                    object nested_value = DataUtils.GetNestedValue(fieldName, selected_entity);
                    string old_column_value;
                    if (nested_value == null)
                        old_column_value = string.Empty;
                    else
                        old_column_value = nested_value.ToString();

                    string new_column_value = old_column_value.Replace(new_find_value, replace_value);
                    setNestedValueWithUndo(selected_entity, fieldName, new_column_value);
                    save_entities.Add(selected_entity);
                }

                UnpauseUndoRedo();
                MainViewModel.BulkSave(save_entities);
            }
        }
        #endregion

        #region DragDrop
        public void TableView_Drop(GridDropEventArgs e)
        {
            e.Handled = true;
        }

        public void TableView_Dropped(GridDroppedEventArgs e)
        {
            IEnumerable<BASELINE_ITEMProgress> dropped_deliverables = ((IEnumerable<object>)e.DraggedRows).Select(x => (BASELINE_ITEMProgress)x).AsEnumerable();
            BASELINE_ITEMProgress target_deliverable = (BASELINE_ITEMProgress)e.TargetRow;

            if(dropped_deliverables.Count() > 0 && target_deliverable != null)
            {
                BASELINE_ITEMProgress first_dropped_deliverable = dropped_deliverables.First();
                string old_value = first_dropped_deliverable.Entity.Entity.INTERNAL_NUM;
                string new_value = target_deliverable.Deliverable_Name;
                string internal_number_fieldname = BindableBase.GetPropertyName(() => new BASELINE_ITEM().INTERNAL_NUM);

                PauseUndoRedo();
                first_dropped_deliverable.Entity.Entity.INTERNAL_NUM = new_value;
                AddUndo(first_dropped_deliverable, internal_number_fieldname, old_value, new_value, EntityMessageType.Changed);

                target_deliverable.Entity.Entity.INTERNAL_NUM = old_value;
                AddUndo(target_deliverable, internal_number_fieldname, new_value, old_value, EntityMessageType.Changed);

                MainViewModel.Save(first_dropped_deliverable);
                MainViewModel.Save(target_deliverable);

                UnpauseUndoRedo();
            }

        }
        #endregion

        #region View Properties
        public decimal TotalAllowedUnits
        {
            get
            {
                return (loadBASELINE == null || loadBASELINE.BUDGETED_UNITS == null) ? 1000000000 : (decimal)loadBASELINE.BUDGETED_UNITS;
            }
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get
            {
                //return "BASELINE_ITEMSViewModelWrapper" + view_project_specific_affix;
                return "BASELINE_ITEMSViewModelWrapper_v3";
            }
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

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMCollection
        {
            get
            {
                return GetEntities<PROGRESS_ITEM>();
            }
        }

        public IEnumerable<RATE> RATECollection
        {
            get
            {
                return GetEntities<RATE>();
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

        public IEnumerable<WORKPACK> WORKPACKCollection
        {
            get
            {
                var collection = GetEntities<WORKPACK>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
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
                return GetSUBAREACollection();
            }
        }

        public IEnumerable<AREA> GetSUBAREACollection()
        {
            var collection = GetEntities<AREA>();
            if (collection != null)
                collection = collection.Where(x => x.GUID_PARENT != null).OrderBy(x => x.INTERNAL_NUM);
            return collection;
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

        public IEnumerable<DELIVERABLES_STATUS> DELIVERABLES_STATUSCollection
        {
            get
            {
                var collection = GetEntities<DELIVERABLES_STATUS>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.MAX_PERCENTAGE);
                return collection;
            }
        }

        public IEnumerable<USER> USERCollection
        {
            get
            {
                var collection = GetEntities<USER>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        public IEnumerable<DOCTYPE> DOCTYPECollection
        {
            get
            {
                var collection = GetEntities<DOCTYPE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.CODE);
                return collection;
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

        public CollectionViewModel<WORKPACK, WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork> WORKPACKSCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<WORKPACK, WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<WORKPACK>();
            }
        }
        #endregion

        #region Reporting

        public bool CanEditReport()
        {
            if (MainViewModel == null || MainViewModel.Entities.Count == 0)
                return false;

            return true;
        }

        public bool CanViewReport()
        {
            if (MainViewModel == null || MainViewModel.Entities.Count == 0)
                return false;

            return true;
        }

        public void EditReport()
        {
            var reportDesigner = new UserReportDesigner(loadPROJECT,
                (CollectionViewModel<PROJECT_REPORT, PROJECT_REPORT, Guid, IBluePrintsEntitiesUnitOfWork>)
                loaderCollection.GetViewModel<PROJECT_REPORT>(), ReportType.Baseline_Report);
            if (reportDesigner.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                reportDesigner.Dispose();
            else
                reportDesigner.Dispose();
        }

        public Func<IEnumerable<BASELINE_ITEMProgress>> GetGridVisibleRows;

        public void ViewReport()
        {
            var baselineReport = new XtraReportBASELINE_ITEMS();
            var dbProjectReport = loaderCollection.GetObject<PROJECT_REPORT>();
            if (dbProjectReport != null)
            {
                var reportString = dbProjectReport.REPORT.ToString();
                using (var sw = new StreamWriter(new MemoryStream()))
                {
                    sw.Write(reportString);
                    sw.Flush();
                    baselineReport.LoadLayout(sw.BaseStream);
                }
            }

            //make sure disciplines are all populated
            PopulateNavigationalProperties();
            IEnumerable<object> gridVisibleRows = GridControlService.GetVisibleRowObjects();
            baselineReport.AssignProperties(loadPROJECT, loadBASELINE, gridVisibleRows.Select(x => ((BASELINE_ITEMProgress)x).Entity));
            var previewWindow = new DocumentPreviewWindow();
            previewWindow.PreviewControl.DocumentSource = baselineReport;
            previewWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            previewWindow.WindowState = WindowState.Maximized;
            baselineReport.RequestParameters = false;
            baselineReport.CreateDocument(true);
            previewWindow.Show();
        }

        private void PopulateNavigationalProperties()
        {
            foreach (var projection in MainViewModel.Entities)
            {
                if (projection.Entity.Entity.GUID_DISCIPLINE != null && projection.Entity.Entity.DISCIPLINE == null)
                    projection.Entity.Entity.DISCIPLINE =
                        DISCIPLINECollection.FirstOrDefault(x => x.GUID == projection.Entity.Entity.GUID_DISCIPLINE);

                if (projection.Entity.Entity.GUID_AREA != null && projection.Entity.Entity.AREA == null)
                    projection.Entity.Entity.AREA =
                        AREACollection.FirstOrDefault(x => x.GUID == projection.Entity.Entity.GUID_AREA);
            }
        }

        protected override string ExportExcelFilename()
        {
            return loadPROJECT.NUMBER + "_Baseline_Rev_" + loadBASELINE.REVISION + ".xlsx";
        }
        #endregion

        #region For Variation Usage
        public CollectionViewModel<BASELINE, BASELINE, Guid, IBluePrintsEntitiesUnitOfWork> BASELINEViewModel
        {
            get
            {
                if (loaderCollection == null)
                    return null;

                return
                    (CollectionViewModel<BASELINE, BASELINE, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<BASELINE>();
            }
        }
        #endregion
    }
}