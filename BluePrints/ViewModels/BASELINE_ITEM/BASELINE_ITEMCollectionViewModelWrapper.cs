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
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using BluePrints.PrimeroData;
using BluePrints.Common.Misc;

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
        string UnifiedValueValidation(TProgress projection, string field_name, object new_value);
        void UnifiedCellValueChanging(string field_name, object old_value, object new_value, TProgress projection, bool isNew);
        Action<object> RaisePropertyChangeCallBack { get; set; }
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
        private IPrimeroEntitiesUnitOfWork primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
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

        public override void OnLoaded()
        {
            if (AppNotificationService == null || GlobalVariables.IsBaselineItemNotificationShown)
            {
                base.OnLoaded();
                return;
            }

            //INotification notification = AppNotificationService.CreatePredefinedNotification("Resource has been changed to allow multiple resources, please re-assign them if you use them, sorry for any inconvenience!", null, null, null);
            //GlobalVariables.IsBaselineItemNotificationShown = true;
            //notification.ShowAsync();

            base.OnLoaded();
        }

        protected override void addEntitiesLoader()
        {
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
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DOCTYPES, DOCTYPEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DELIVERABLES_STATUSES, DELIVERABLES_STATUSProjectionFunc);
            loaderCollection.AddLoaderDescription<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.USERS);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEM_WORKS, BASELINE_ITEM_WORKProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.P6_ASSIGNMENTS, P6_ASSIGNMENTProjectionFunc);
            loaderCollection.AddLoaderDescription<REGISTER_HOLD_REF, REGISTER_HOLD_REF, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.REGISTER_HOLD_REF);
            loaderCollection.AddLoaderDescription<OFFICE, OFFICE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.OFFICES);
        }

        private Func<IRepositoryQuery<P6_ASSIGNMENT>, IQueryable<P6_ASSIGNMENT>> P6_ASSIGNMENTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private void assign_baseline(BASELINE entity)
        {
            if (entity == null && !SupressCompulsoryEntityNotFoundMessage)
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage("Live baseline not found")));
                return;
            }
            else if (entity == null)
                return;

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

        private Func<IRepositoryQuery<BASELINE_ITEM_WORK>, IQueryable<BASELINE_ITEM_WORK>> BASELINE_ITEM_WORKProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<DOCTYPE>, IQueryable<DOCTYPE>> DOCTYPEProjectionFunc()
        {
            if (viewType == DeliverablesViewType.Both)
                return query => query;
            else if (viewType == DeliverablesViewType.Direct)
                return query => query.Where(x => !x.IS_INDIRECT_ONLY);
            else
                return query => query;
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

        private Func<IRepositoryQuery<REGISTER_HOLD>, IQueryable<REGISTER_HOLD>> REGISTER_HOLDProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
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
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.TYPE == PhaseType.Design && x.STATUS == ProgressStatus.Live);
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

        List<ExoTimeAuthorisation> exoAuthorisations = new List<ExoTimeAuthorisation>();
        List<string> narratives = new List<string>();
        List<string> variationCodes = new List<string>();
        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            exoAuthorisations = ExoQueries.GetExoLinesAuthorisations(primeroUnitOfWork, loadPROJECT.NUMBER, false);
            narratives = ExoQueries.GetJobNarratives(primeroUnitOfWork, loadPROJECT.NUMBER);
            variationCodes = ExoQueries.GetJobVariationCode(primeroUnitOfWork, loadPROJECT.NUMBER);
            
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEMProgress>>
            specifyMainViewModelProjection()
        {
            return query => ProgressQueries.OffsiteDirectProgressItemTransformation(base_entity_query(query), loadPROJECT, livePROGRESS, RATECollection, PROGRESS_ITEMCollection, null, false, P6_ASSIGNMENTCollection, InternalNumberMode, false, null, USERCollection, BASELINE_ITEM_WORKCollection, false, exoAuthorisations, REGISTER_HOLD_REFCollection);
        }

        public Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEM>> BaseEntityQueryCallBack { get; set; }
        private IQueryable<BASELINE_ITEM> base_entity_query(IRepositoryQuery<BASELINE_ITEM> query)
        {
            if (BaseEntityQueryCallBack != null)
                return BaseEntityQueryCallBack(query);

            //if(viewType == DeliverablesViewType.Direct)
            //    return query.Where(x => x.GUID_BASELINE == load_context_guid && x.PHASE != null && x.PHASE.CHARGE_TYPE == ChargeType.Direct);
            //else if(viewType == DeliverablesViewType.Indirect)
            //    return query.Where(x => x.GUID_BASELINE == load_context_guid && x.PHASE != null && x.PHASE.CHARGE_TYPE == ChargeType.Indirect);
            //else
            return query.Where(x => x.GUID_BASELINE == load_context_guid);
        }

        public Action<IEnumerable<BASELINE_ITEMProgress>> OnReportablesLoadedCallBack { get; set; }
        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<BASELINE_ITEMProgress> entities)
        {
            FilterTreeViewModel = FiltersSettings.GetBASELINE_ITEMProgressFilterTree(this, entities);
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertyChanged(x => x.FilterTreeViewModel)));
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = OnBeforeEntitySaved;
            MainViewModel.OnAfterEntitySavedCallBack = OnEntitiesSavedCallBack;
            MainViewModel.OnBeforeEntityDeletedIsContinueCallBack = onBeforeEntitiesDeleted;
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

        //allows raise property change to propagate to parent
        public Action<object> RaisePropertyChangeCallBack { get; set; }
        protected override bool IsSingleMainEntityRefreshIdentified(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
            return base.IsSingleMainEntityRefreshIdentified(key, changedType, messageType, sender, isBulkRefresh);
        }

        private bool onBeforeEntitiesDeleted(BASELINE_ITEMProgress entity)
        {
            if (entity.PROGRESS_ITEMS.Count > 0 && entity.PROGRESS_ITEMS.Sum(x => x.EARNED_UNITS) > 0)
            {
                MessageBoxService.ShowMessage("Cannot delete " + entity.Entity.Entity.INTERNAL_NUM + " because it has been progressed");
                return false;
            }
            
            return true;
        }

        private void save_deliverable_users(BASELINE_ITEMProgress entity)
        {
            List<BASELINE_ITEM_WORK> remove_baseline_item_work = new List<BASELINE_ITEM_WORK>();

            if (entity.Entity.AssignUsers != null)
            {
                foreach (BASELINE_ITEM_WORK assignment in BASELINE_ITEM_WORKCollection.Where(x => x.GUID_BASELINE_ITEM_ORIGINAL == entity.OriginalEntityKey))
                {
                    if (!entity.Entity.AssignUsers.Any(x => x.GUID == assignment.GUID_USER))
                        remove_baseline_item_work.Add(assignment);
                }

                BASELINE_ITEM_WORKCollectionViewModel.BaseBulkDelete(remove_baseline_item_work);
                List<BASELINE_ITEM_WORK> add_project_disciplines = new List<BASELINE_ITEM_WORK>();
                foreach (USER user in entity.Entity.AssignUsers)
                {
                    if (!BASELINE_ITEM_WORKCollection.Any(x => x.GUID_USER == user.GUID && x.GUID_BASELINE_ITEM_ORIGINAL == entity.OriginalEntityKey))
                    {
                        add_project_disciplines.Add(new BASELINE_ITEM_WORK() { GUID_USER = user.GUID, GUID_BASELINE_ITEM_ORIGINAL = entity.OriginalEntityKey, GUID_PROJECT = loadBASELINE.GUID_PROJECT, WEIGHTING = 1 });
                    }

                }

                BASELINE_ITEM_WORKCollectionViewModel.BulkSave(add_project_disciplines);
            }
            else
            {
                foreach (BASELINE_ITEM_WORK assignment in BASELINE_ITEM_WORKCollection.Where(x => x.GUID_BASELINE_ITEM_ORIGINAL == entity.OriginalEntityKey))
                {
                    remove_baseline_item_work.Add(assignment);
                }

                BASELINE_ITEM_WORKCollectionViewModel.BaseBulkDelete(remove_baseline_item_work);
            }
        }

        public decimal? FreeUnits
        {
            get
            {
                if (MainViewModel == null || DisplayEntities == null)
                    return 0;

                if (loadBASELINE.BUDGETED_UNITS == null || loadBASELINE.BUDGETED_UNITS == 0)
                    return null;

                return (decimal)loadBASELINE.BUDGETED_UNITS - DisplayEntities.Sum(x => x.Budget_Units);
            }
        }
        //public void OnFillOrCellLevelPasting(IEnumerable<BASELINE_ITEMProgress> entities, string fieldName)
        //{
        //    if(fieldName.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity.Entity.GUID_AREA)) ||
        //        fieldName.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity.Entity.GUID_DOCTYPE)) ||
        //        fieldName.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity.Entity.GUID_DISCIPLINE)))
        //    {
        //        foreach (BASELINE_ITEMProgress entity in entities)
        //        {
        //            if (entity.IsInternalNumberEditable && !entity.IsInternalNumberManualOnly)
        //            {
        //                string oldValue = entity.Entity.Entity.INTERNAL_NUM;
        //                string newValue = generateInternalNumber(entity);
        //                entity.Entity.Entity.INTERNAL_NUM = newValue;

        //                MainViewModel.EntitiesUndoRedoManager.AddUndo(entity, "Entity.Entity." + BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity.Entity.INTERNAL_NUM), oldValue, newValue, EntityMessageType.Changed);
        //            }
        //        }
        //    }
        //}

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
            else if(changedType == typeof(BASELINE_ITEM))
            {
                this.RaisePropertyChanged(x => x.FreeUnits);
                //Need to raise property change to stimulate converter to calculate maxValue for each deliverable
                this.RaisePropertyChanged(x => x.DisplayEntities);
            }

            base.OnAfterAuxiliaryEntitiesChanged(key, changedType, messageType, sender, isBulkRefresh);
        }

        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public bool OnBeforeEntitySaved(BASELINE_ITEMProgress entity)
        {
            //if (MainViewModel.isBackgroundEdit)
            //    return true;

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
            BluePrintsDataUtils.OnBeforeSavedGenerateAndAssignSubjob(loadPROJECT, PHASECollection, AREACollection, SUBAREACollection, entity, SUBJOBSCollectionViewModel, phaseType, chargeType, false, allowSubJobDeletion);
            BluePrintsDataUtils.OnBeforeSavedGenerateAndAssignWorkpack(entity, WORKPACKSCollectionViewModel, SUBJOBCollection, DISCIPLINECollection, allowWorkpackDeletion);
            //entity.Entity.Entity.GUID_ESTIMATE = loadESTIMATE.GUID;
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

        public void OnEntitiesSavedCallBack(BASELINE_ITEMProgress projectionEntity, BASELINE_ITEM entity, bool isNewEntity)
        {
            projectionEntity.Entity.Entity.GUID_ORIGINAL = entity.GUID_ORIGINAL;
            save_deliverable_users(projectionEntity);
        }
        #endregion

        #region View Localization Helpers
        private void setNestedValueWithUndo(BASELINE_ITEMProgress entity, string propertyName, object newValue, Dictionary<Guid, string> internalNumberUndoInfos = null)
        {
            string localizedPropertyName = localizeColumnFieldName(propertyName);
            object oldValue = null;
            if (internalNumberUndoInfos != null)
            {
                var keyValuePair = internalNumberUndoInfos.FirstOrDefault(x => x.Key == entity.EntityKey);
                oldValue = keyValuePair.Value;
            }
            else
            {
                oldValue = DataUtils.GetNestedValue(localizedPropertyName, entity);
            }

            DataUtils.SetNestedValue(localizedPropertyName, entity, newValue);
            AddUndo(entity, localizedPropertyName, oldValue, newValue, EntityMessageType.Changed);
            entity.Update();
            RaisePropertyChangeCallBack?.Invoke(entity.GUID);
        }

        private string localizeColumnFieldName(string fieldName)
        {
            //Technical debt, must move AssignUserObject to base entity
            if (fieldName.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity.AssignUserObject)))
                return fieldName;

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

        public void ValidateCellValue(GridCellValidationEventArgs e)
        {
            string fieldName = DataUtils.FormatColumnFieldname(e.Column.FieldName);
            string error_message = UnifiedValueValidation((BASELINE_ITEMProgress)e.Row, fieldName, e.Value);
            if (error_message != string.Empty)
            {
                e.IsValid = false;
                e.ErrorContent = error_message;
                e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Critical;
            }
        }

        public override string UnifiedValueValidation(BASELINE_ITEMProgress entity, string column_name, object newValue)
        {
            string fieldName = DataUtils.FormatColumnFieldname(column_name);
            //budget hours field is disabled but just in case
            if (fieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEM().BUDGET_HOURS))
            {
                if (entity.Entity.Entity.BY_DURATION && ((decimal)newValue) > 0)
                    return "Cannot set budgeted hours when deliverables is by duration";
                else if ((decimal)newValue < entity.MinEstimateUnits)
                    return "Budgeted hours cannot be less than " + entity.MinEstimateUnits.ToString();
            }
            else if (fieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEM().BY_DURATION))
            {
                if (entity.Earned_Units_Total > 0)
                    return "Cannot set budgeted hours when deliverables is by duration";
            }
            else if (fieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEM().INTERNAL_NUM))
            {
                if (!entity.IsInternalNumberEditable)
                    return "Cannot change internal number because deliverables has already been progressed";
            }
            else if (fieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_PHASE))
            {
                if(newValue != null)
                {
                    if (!isDocTypePhaseValid(entity.Entity.Entity.GUID_DOCTYPE, (Guid)newValue))
                    {
                        return "Selected document type is reserved for indirect only";
                    }
                }
            }
            else if (fieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_DOCTYPE))
            {
                if (newValue != null)
                {
                    if (!isDocTypePhaseValid((Guid)newValue, entity.Entity.Entity.GUID_PHASE))
                    {
                        return "Selected document type is reserved for indirect only";
                    }
                }
            }

            return string.Empty;
        }

        public override string UnifiedRowValidation(BASELINE_ITEMProgress projection)
        {
            if(!isDocTypePhaseValid(projection.Entity.Entity.GUID_DOCTYPE, projection.Entity.Entity.GUID_PHASE))
                return "Selected document type is reserved for indirect only";

            return string.Empty;
        }

        private bool isDocTypePhaseValid(Guid? doctypeGuid, Guid? phaseGuid)
        {
            if (doctypeGuid != null && phaseGuid != null)
            {
                PHASE phase = PHASECollection.FirstOrDefault(x => x.GUID == phaseGuid);
                if (phase != null)
                {
                    DOCTYPE doctype = DOCTYPECollection.FirstOrDefault(x => x.GUID == doctypeGuid);
                    if (doctype != null)
                    {
                        if (doctype.IS_INDIRECT_ONLY && phase.CHARGE_TYPE == ChargeType.Direct)
                            return false;
                    }
                }
            }

            return true;
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

        //public void OnCustomColumnSort(CustomColumnSortEventArgs e)
        //{
        //    if (e.Column.FieldName == Base_Entity_String + BindableBase.GetPropertyName(() => new BASELINE_ITEM().BUDGET_HOURS))
        //    {
        //        decimal decimal_value1 = (decimal)e.Value1;
        //        decimal decimal_value2 = (decimal)e.Value2;

        //        e.Result = decimal_value1.CompareTo(decimal_value2);
        //        e.Handled = true;
        //    }
        //}

        public override void UnifiedCellValueChanging(string field_name, object old_value, object new_value, BASELINE_ITEMProgress projection, bool isNew)
        {
            field_name = DataUtils.FormatColumnFieldname(field_name);
            if(isNew)
            {
                projection.Entity.Entity.OFFICE = loadPROJECT.OFFICE;
            }

            if (field_name == BindableBase.GetPropertyName(() => new BASELINE_ITEM().BY_DURATION))
            {
                if ((bool)new_value)
                {
                    decimal oldValue = projection.Entity.Entity.BUDGET_HOURS;
                    if (oldValue > 0)
                    {
                        decimal newValue = 0;
                        string budgetHoursFieldName = BindableBase.GetPropertyName(() => new BASELINE_ITEM().BUDGET_HOURS);
                        projection.Entity.Entity.BUDGET_HOURS = newValue;
                        PauseUndoRedo();
                        AddUndo(projection, budgetHoursFieldName, oldValue, newValue, EntityMessageType.Changed);
                    }
                }
            }

            if (field_name == BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_AREA))
            {
                Guid? oldValue = projection.Entity.Entity.GUID_SUBAREA;
                Guid? newValue = (Guid?)null;

                projection.Entity.Entity.GUID_SUBAREA = newValue;
                if (!isNew)
                {
                    string subAreaFieldName = BindableBase.GetPropertyName(() => new BASELINE_ITEM().SubAreaGuid);
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

            //only new row will change department according to doc type selection
            if (field_name == BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_DOCTYPE))
            {
                var chosenDOCTYPE = DOCTYPECollection.FirstOrDefault(entity => entity.GUID == (Guid)new_value);
                if(isNew)
                {
                    if (chosenDOCTYPE != null)
                    {
                        if (chosenDOCTYPE.GUID_DDEPARTMENT != null)
                            projection.Entity.Entity.GUID_DEPARTMENT = chosenDOCTYPE.DEPARTMENT.GUID;

                        //Baseline and Department is required immediately for deliverables status selection
                        projection.Entity.Entity.BASELINE = loadBASELINE;
                        projection.Entity.Entity.DOCTYPE = DOCTYPECollection.FirstOrDefault(x => x.GUID == (Guid)new_value);
                        projection.Update();
                    }
                }

                if (chosenDOCTYPE.IS_INDIRECT_ONLY)
                {
                    PHASE indirectPhase = PHASECollection.FirstOrDefault(x => x.PHASE_TYPE == PhaseType.Design && x.CHARGE_TYPE == ChargeType.Indirect);
                    if (indirectPhase != null)
                        projection.Entity.Entity.GUID_PHASE = indirectPhase.GUID;
                }
            }
            //only new row will change area and subarea according to subjob selection
            if (isNew && field_name == BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_SUBJOB))
            {
                var chosenSUBJOB = SUBJOBCollection.FirstOrDefault(entity => entity.GUID == (Guid)new_value);
                if (chosenSUBJOB != null)
                {
                    projection.Entity.Entity.GUID_AREA = chosenSUBJOB.GUID_DAREA;
                    //Area is required immediately for subarea selection
                    projection.Entity.Entity.AREA = AREACollection.FirstOrDefault(x => x.GUID == chosenSUBJOB.GUID_DAREA);
                    projection.Entity.Entity.GUID_SUBAREA = chosenSUBJOB.GUID_DSUBAREA;
                    projection.Entity.Entity.GUID_PHASE = chosenSUBJOB.PHASE != null ? chosenSUBJOB.GUID_DPHASE : null;
                    projection.Update();
                }
            }

            if (field_name == BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_DOCTYPE) || field_name == BindableBase.GetPropertyName(() => new BASELINE_ITEM().DELIVERABLE_TYPE))
            {
                Guid? oldValue = projection.Entity.Entity.GUID_STATUS;
                Guid? newValue = (Guid?)null;
                projection.Entity.Entity.GUID_STATUS = newValue;

                if (!isNew)
                {
                    string deliverableStatusFieldName = BindableBase.GetPropertyName(() => new BASELINE_ITEM().DeliverableStatusGuid);
                    PauseUndoRedo();
                    AddUndo(projection, deliverableStatusFieldName, oldValue, newValue, EntityMessageType.Changed);
                }
                else
                    projection.Update();
            }

            if (field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity.Entity.GUID_AREA)) ||
                field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity.Entity.GUID_DOCTYPE)) ||
                field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity.Entity.GUID_DISCIPLINE)))
            {
                if (projection.IsInternalNumberEditable && !projection.IsInternalNumberManualOnly)
                {
                    string fieldName = "Entity.Entity." + field_name;
                    //commit the latest value for internal number generation
                    DataUtils.SetNestedValue(fieldName, projection, new_value);
                    string oldValue = projection.Entity.Entity.INTERNAL_NUM;
                    string errorMessage = string.Empty;
                    string newValue = generateInternalNumber(projection, out errorMessage);
                    projection.Entity.Entity.INTERNAL_NUM = newValue;

                    PauseUndoRedo();
                    AddUndo(projection, BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity.Entity.INTERNAL_NUM), oldValue, newValue, EntityMessageType.Changed);
                    projection.Update();
                }
            }

            if (field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity.Entity.GUID_SUBJOB)) && new_value == null)
            {
                allowSubJobDeletion = true;
            }

            if (field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity.Entity.GUID_WORKPACK)) && new_value == null)
            {
                allowWorkpackDeletion = true;
            }

            base.UnifiedCellValueChanging(field_name, old_value, new_value, projection, isNew);
        }

        bool allowSubJobDeletion = false;
        bool allowWorkpackDeletion = false;
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
                this.RaisePropertyChanged(x => x.FreeUnits);
            }
        }

        private void LockUnlockBASELINE(bool isLock)
        {
            var BASELINECollectionViewModel = (CollectionViewModel<BASELINE, BASELINE, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<BASELINE>();
            if (!isLock)
                loadBASELINE.BUDGETED_UNITS = 0;
            else
            {
                decimal totalEstimatedHours = MainViewModel.Entities.Sum(x => x.Entity.Entity.BUDGET_HOURS);
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
            TableViewService.SetImmediateUpdateRowPosition(true);
            if (!_isProcessingMultiple)
                PauseUndoRedo();

            List<BASELINE_ITEMProgress> newEntities = getNewProgressEntities(1, true, MainViewModel.Entities, SelectedEntities);
            newEntities = concatenateNewEntitiesWithExistingRenameEntities(newEntities, EditableAllEntities);

            foreach(BASELINE_ITEMProgress newEntity in newEntities)
                MainViewModel.Save(newEntity);

            //Add undo must happen after save so that variation can pick it up
            foreach (BASELINE_ITEMProgress newEntity in newEntities)
                AddUndo(newEntity, null, null, null, EntityMessageType.Added);

            if (!_isProcessingMultiple)
                UnpauseUndoRedo();
            TableViewService.SetImmediateUpdateRowPosition(false);
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
            TableViewService.SetImmediateUpdateRowPosition(true);
            if (!_isProcessingMultiple)
                PauseUndoRedo();

            List<BASELINE_ITEMProgress> newEntities = getNewProgressEntities(1, false, MainViewModel.Entities, SelectedEntities);

            //because bulk save will invoke refresh on this collectionviewmodel. Variation will not know about the refresh
            foreach(BASELINE_ITEMProgress newEntity in newEntities)
                MainViewModel.Save(newEntity);

            //Add undo must happen after save so that variation can pick it up
            foreach (BASELINE_ITEMProgress newEntity in newEntities)
                AddUndo(newEntity, null, null, null, EntityMessageType.Added);

            if (!_isProcessingMultiple)
                UnpauseUndoRedo();

            TableViewService.SetImmediateUpdateRowPosition(false);
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
                        newProjection.Entity.Entity.BUDGET_HOURS = 0;
                    else
                        newProjection.Entity.Entity.BUDGET_HOURS = IsBASELINELocked ? 0 : selectedEntity.Entity.Entity.BUDGET_HOURS;
                    
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
            TableViewService.SetImmediateUpdateRowPosition(true);
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
            TableViewService.SetImmediateUpdateRowPosition(false);
        }

        public void InsertMultiple(BarEditItem barEdit)
        {
            TableViewService.SetImmediateUpdateRowPosition(true);
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
            TableViewService.SetImmediateUpdateRowPosition(false);
        }
        
        public bool CanAutoPopulate(object button)
        {
            if (SelectedEntities == null || SelectedEntities.Count() == 0)
                    return false;

            return true;
        }

        public void AutoPopulate(object button)
        {
            MainViewModel.isBackgroundEdit = true;
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

            Dictionary<Guid, string> internalNumberUndoInfos = new Dictionary<Guid, string>();
            if (fieldName == internalNumberFieldName)
                foreach (var entity in SelectedEntities)
                {
                    if(entity.IsInternalNumberEditable)
                    {
                        internalNumberUndoInfos.Add(entity.GUID, entity.Entity.Entity.INTERNAL_NUM);
                        entity.Entity.Entity.INTERNAL_NUM = string.Empty;
                    }
                }

            foreach (var entity in SelectedEntities)
            {
                var entitySUBJOB = SUBJOBCollection.FirstOrDefault(x => x.GUID == entity.Entity.Entity.GUID_SUBJOB);
                if (fieldName == internalNumberFieldName && entity.IsInternalNumberEditable)
                {
                    string errorMessage = string.Empty;
                    string internalNumber = generateInternalNumber(entity, out errorMessage);
                    if(errorMessage != string.Empty)
                    {
                        MessageBoxService.ShowMessage(errorMessage);
                    }
                    else
                    {
                        setNestedValueWithUndo(entity, fieldName, internalNumber, internalNumberUndoInfos);
                        entitiesToSave.Add(entity);
                    }
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

                    var findSUBJOB = SUBJOBCollection.FirstOrDefault(x => x.INTERNAL_NAME1 == internalName);

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
            MainViewModel.isBackgroundEdit = false;
            UnpauseUndoRedo();
            BackgroundRefresh();
        }

        protected override void onAfterRefresh()
        {
            allowSubJobDeletion = false;
            allowWorkpackDeletion = false;
            base.onAfterRefresh();
        }

        private string generateInternalNumber(BASELINE_ITEMProgress projectionEntity, out string errorMessage)
        {
            AREA currentItemAREA = AREACollection.FirstOrDefault((x => x.GUID == projectionEntity.Entity.Entity.GUID_AREA));
            DISCIPLINE currentItemDISCIPLINE = DISCIPLINECollection.FirstOrDefault((x => x.GUID == projectionEntity.Entity.Entity.GUID_DISCIPLINE));
            DOCTYPE currentItemDOCTYPE = DOCTYPECollection.FirstOrDefault((x => x.GUID == projectionEntity.Entity.Entity.GUID_DOCTYPE));

            errorMessage = string.Empty;
            if (currentItemAREA == null)
                errorMessage += "Area, ";

            if (currentItemDISCIPLINE == null)
                errorMessage += "Discipline, ";

            if (currentItemDOCTYPE == null)
                errorMessage += "Document Type, ";

            if(errorMessage.Length > 2)
                errorMessage = errorMessage.Substring(0, errorMessage.Length - 2) + " is missing";

            var internalNum = BluePrintsDataUtils.BASELINEITEM_Generate_InternalNumber(loadPROJECT, MainViewModel.Entities.Select(x => x.Entity.Entity), currentItemAREA, currentItemDISCIPLINE, currentItemDOCTYPE, projectionEntity.EntityKey);

            return internalNum;
        }
        #endregion

        #region Find and Replace
        private DevExpress.Mvvm.IDialogService BulkColumnEditDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("BulkColumnEditService"); }
        }

        private DevExpress.Mvvm.IDialogService BookTimeDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("BookTimeDialog"); }
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

            //FullRefresh();
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

        public bool CanShowBookable()
        {
            if (MainViewModel == null || SelectedEntities == null || SelectedEntities.Count() == 0)
                return false;

            return true;
        }

        bool showBookable;
        public bool ShowBookable
        {
            get
            {
                return showBookable;
            }
            set
            {
                showBookable = value;
                if (GridControlService != null)
                {
                    if(value)
                    {
                        CriteriaOperator criteriaOperator = GridControlService.GetFilterCriteria();
                        CriteriaOperator newCriteriaOperator;
                        if (!ReferenceEquals(criteriaOperator, null))
                        {
                            string filterCriteria = criteriaOperator.ToString() + " And [CanBook] In (True)";
                            newCriteriaOperator = CriteriaOperator.Parse(filterCriteria);
                        }
                        else
                        {
                            newCriteriaOperator = CriteriaOperator.Parse("[CanBook] In (True)");
                        }

                        GridControlService.SetFilterCriteria(newCriteriaOperator);
                    }
                    else
                    {
                        CriteriaOperator criteriaOperator = GridControlService.GetFilterCriteria();
                        if (!ReferenceEquals(criteriaOperator, null))
                        {
                            CriteriaOperator newCriteriaOperator;
                            string currentFilterCriteria = criteriaOperator.ToString();
                            string newfilterCriteria = currentFilterCriteria.Replace("And [CanBook] In (True)", "");
                            newfilterCriteria = newfilterCriteria.Replace("[CanBook] In (True)", "");
                            if (newfilterCriteria.Length >= 5)
                            {
                                string firstFiveChar = newfilterCriteria.Substring(0, 5);
                                if(firstFiveChar.ToUpper().Contains("AND"))
                                    newfilterCriteria = newfilterCriteria.Substring(5, newfilterCriteria.Length - 5);
                            }


                            newCriteriaOperator = CriteriaOperator.Parse(newfilterCriteria);
                            GridControlService.SetFilterCriteria(newCriteriaOperator);
                        }
                    }
                }
            }
        }

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
                return "BASELINE_ITEMSViewModelWrapper_v4";
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

        public IEnumerable<REGISTER_HOLD_REF> REGISTER_HOLD_REFCollection
        {
            get
            {
                var collection = GetEntities<REGISTER_HOLD_REF>();
                return collection;
            }
        }

        public IEnumerable<REGISTER_HOLD> REGISTER_HOLDCollection
        {
            get
            {
                var collection = GetEntities<REGISTER_HOLD>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NUMBER);
                return collection;
            }
        }

        public IEnumerable<BASELINE_ITEM_WORK> BASELINE_ITEM_WORKCollection
        {
            get
            {
                var collection = GetEntities<BASELINE_ITEM_WORK>();
                return collection;
            }
        }

        public IEnumerable<P6_ASSIGNMENT> P6_ASSIGNMENTCollection
        {
            get
            {
                return GetEntities<P6_ASSIGNMENT>();
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

        public IEnumerable<OFFICE> OFFICECollection
        {
            get
            {
                var collection = GetEntities<OFFICE>();
                if (collection != null)
                {
                    collection = collection.OrderBy(x => x.NAME);
                }

                return collection;
            }
        }

        public CollectionViewModel<BASELINE_ITEM_WORK, BASELINE_ITEM_WORK, Guid, IBluePrintsEntitiesUnitOfWork> BASELINE_ITEM_WORKCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<BASELINE_ITEM_WORK, BASELINE_ITEM_WORK, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<BASELINE_ITEM_WORK>();
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
            var reportDesigner = new UserReportDesigner(loadPROJECT, (CollectionViewModel<PROJECT_REPORT, PROJECT_REPORT, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<PROJECT_REPORT>(), ReportType.Baseline_Report);
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

        public void BookTime()
        {
            var bookTimeViewModel = BookTimeSheetViewModel.Create(loadPROJECT, DisplaySelectedEntity, primeroUnitOfWork, exoAuthorisations, variationCodes, narratives);
            if (bookTimeViewModel.GetResource() == null)
            {
                MessageBoxService.ShowMessage("You are not authorised to book time on this subjob, please contact the project manager for assistance");
            }
            else if (bookTimeViewModel.GetCostType() == null)
            {
                MessageBoxService.ShowMessage("You do not have \nSub Job: " + DisplaySelectedEntity.Subjob_Name + "\nCost Group: " + DisplaySelectedEntity.Discipline_Code + "\nCost Type: " + DisplaySelectedEntity.Commodity_Code + "\nAdded in exo, please contact the project manager for assistance");
            }
            else if (BookTimeDialogService.ShowDialog(MessageButton.OKCancel, "Enter time to book", "BookTimeDialog", bookTimeViewModel) == MessageResult.OK)
            {
                string variationCode = bookTimeViewModel.GetVariationCode();
                string narrative = bookTimeViewModel.GetNarratives();
                PrimeroSubJob subJob = bookTimeViewModel.GetSubJob();
                PrimeroResource bookResource = bookTimeViewModel.GetResource();
                TimesheetDate bookDate = bookTimeViewModel.GetTimesheetDate();
                PrimeroDiscipline bookCostGroup = bookTimeViewModel.GetCostGroup();
                PrimeroCommodity bookCostType = bookTimeViewModel.GetCostType();
                decimal bookTime = bookTimeViewModel.BookHours;

                if(bookResource != null && bookCostGroup != null && bookCostType != null)
                {
                    JOB_TIMESHEETS timesheet = primeroUnitOfWork.JOB_TIMESHEETS.FirstOrDefault(x => x.STAFFNO == bookResource.SeqNo && x.JOBNO == subJob.Id && x.STOCKCODE == bookCostType.StockCode && x.COST_GROUP == bookCostGroup.Id && x.COST_TYPE == bookCostType.Id && x.WEEK_START_DATE == bookDate.WeekStartDate);
                    if (timesheet != null)
                    {
                        AdjustTimeSheetHours(timesheet, bookDate, DisplaySelectedEntity, bookTime);
                    }
                    else
                    {
                        JOB_TIMESHEETS newTimeSheet = new JOB_TIMESHEETS();
                        newTimeSheet.STAFFNO = bookResource.SeqNo;
                        newTimeSheet.JOBNO = subJob.Id;
                        newTimeSheet.TITLE = subJob.Code + " : " + subJob.Title;
                        newTimeSheet.STOCKCODE = bookCostType.StockCode;
                        newTimeSheet.DESCRIPTION = bookCostType.StockDescription;
                        newTimeSheet.UNITPRICE = 0;
                        newTimeSheet.WEEK_START_DATE = bookDate.WeekStartDate;
                        AdjustTimeSheetHours(newTimeSheet, bookDate, DisplaySelectedEntity, bookTime);
                        newTimeSheet.IS_OVERTIME = "N";
                        newTimeSheet.DAY1_POSTED = "N";
                        newTimeSheet.DAY2_POSTED = "N";
                        newTimeSheet.DAY3_POSTED = "N";
                        newTimeSheet.DAY4_POSTED = "N";
                        newTimeSheet.DAY5_POSTED = "N";
                        newTimeSheet.DAY6_POSTED = "N";
                        newTimeSheet.DAY7_POSTED = "N";
                        newTimeSheet.RATE_SEQNO = 0;
                        newTimeSheet.RATE_FACTOR = 1;
                        newTimeSheet.COST_GROUP = bookCostGroup.Id;
                        newTimeSheet.COST_TYPE = bookCostType.Id;
                        newTimeSheet.LABOUR_ALLOWANCE = 0;
                        newTimeSheet.HAS_ALLOWANCE = "N";
                        newTimeSheet.X_DECLINED = false;
                        newTimeSheet.X_APPROVAL_MANAGER = -1;
                        newTimeSheet.X_SUBMITTED = false;
                        newTimeSheet.X_NARRATIVE = narrative;
                        newTimeSheet.X_VARIATIONCODE = variationCode;
                        primeroUnitOfWork.JOB_TIMESHEETS.Add(newTimeSheet);
                    }

                    primeroUnitOfWork.SaveChanges();
                }
            }
        }

        private void AdjustTimeSheetHours(JOB_TIMESHEETS timesheet, TimesheetDate bookDate, IDeliverable deliverable, decimal bookTime)
        {
            Double dblTime = Convert.ToDouble(bookTime);
            switch(bookDate.DayNumber)
            {
                case 1:
                    timesheet.DAY1 = dblTime;
                    timesheet.DAY1_NARRATIVE = FindExistingOrAddNewNarrative(deliverable.Deliverable_Name);
                    break;
                case 2:
                    timesheet.DAY2 = dblTime;
                    timesheet.DAY2_NARRATIVE = FindExistingOrAddNewNarrative(deliverable.Deliverable_Name);
                    break;
                case 3:
                    timesheet.DAY3 = dblTime;
                    timesheet.DAY3_NARRATIVE = FindExistingOrAddNewNarrative(deliverable.Deliverable_Name);
                    break;
                case 4:
                    timesheet.DAY4 = dblTime;
                    timesheet.DAY4_NARRATIVE = FindExistingOrAddNewNarrative(deliverable.Deliverable_Name);
                    break;
                case 5:
                    timesheet.DAY5 = dblTime;
                    timesheet.DAY5_NARRATIVE = FindExistingOrAddNewNarrative(deliverable.Deliverable_Name);
                    break;
                case 6:
                    timesheet.DAY6 = dblTime;
                    timesheet.DAY6_NARRATIVE = FindExistingOrAddNewNarrative(deliverable.Deliverable_Name);
                    break;
                case 7:
                    timesheet.DAY7 = dblTime;
                    timesheet.DAY7_NARRATIVE = FindExistingOrAddNewNarrative(deliverable.Deliverable_Name);
                    break;
            }
        }

        private int FindExistingOrAddNewNarrative(string description)
        {
            NARRATIVES narrative = primeroUnitOfWork.NARRATIVES.FirstOrDefault(x => x.NARRATIVE == description);
            if (narrative != null)
                return narrative.SEQNO;
            else
            {
                NARRATIVES newNarrative = new NARRATIVES();
                newNarrative.NARRATIVE = description;
                primeroUnitOfWork.NARRATIVES.Add(newNarrative);
                primeroUnitOfWork.SaveChanges();
                return newNarrative.SEQNO;
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