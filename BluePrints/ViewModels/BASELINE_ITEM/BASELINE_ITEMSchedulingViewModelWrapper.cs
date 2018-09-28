using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Helpers;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Common.Reports;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using BluePrints.Reports;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Printing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single BASELINE object view model.
    /// </summary>
    public partial class BASELINE_ITEMSchedulingViewModelWrapper :
        BluePrintsEntitiesSchedulingCollectionWrapper
        <BASELINE_ITEM, BASELINE_ITEMProgress, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        public Action ShowSUBJOBInternalName1;
        public Action ShowSUBJOBInternalName2;
        public Action<bool> SetBaselineLockUnlock;

        /// <summary>
        /// Creates a new instance of BASELINE_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static BASELINE_ITEMSchedulingViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new BASELINE_ITEMSchedulingViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the BASELINEViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the BASELINEViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected BASELINE_ITEMSchedulingViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        private BASELINE loadBASELINE;

        protected override PhaseType phase_type => PhaseType.Design;

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINES, BASELINEProjectionFunc, assign_baseline);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.SUBJOBS, SUBJOBProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PHASES, PHASEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECT_REPORTS, PROJECT_REPORTProjectionFunc, null, true);
            loaderCollection.AddLoaderDescription<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DOCTYPES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DELIVERABLES_STATUSES, DELIVERABLES_STATUSProjectionFunc);
            loaderCollection.AddLoaderDescription<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.USERS);

            base.addEntitiesLoader();
        }
        
        private Func<IRepositoryQuery<Data.PROJECT>, IQueryable<Data.PROJECT>> PROJECTProjectionFunc()
        {
            if (isFromPROGRESS)
                return query => query.Where(x => x.GUID == live_PROGRESS.GUID_PROJECT);
            else
                return query => query.Where(x => x.GUID == p6_baseline_entity.project_guid);
        }

        private Func<IRepositoryQuery<BASELINE>, IQueryable<BASELINE>> BASELINEProjectionFunc()
        {
            if (isFromPROGRESS)
                return query => query.Where(x => x.GUID_PROJECT == live_PROGRESS.GUID_PROJECT && x.STATUS == BaselineStatus.Live);
            else
                return query => query.Where(x => x.GUID == p6_baseline_entity.EntityKey);
        }

        private void assign_baseline(BASELINE entity)
        {
            if (entity == null && !SupressCompulsoryEntityNotFoundMessage)
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage("Live baseline not found")));

            p6_baseline_entity = entity;
            loadBASELINE = entity;
            if (entity.BUDGETED_UNITS != null && entity.BUDGETED_UNITS > 0)
                SetBaselineLockUnlock?.Invoke(true);
            else
                SetBaselineLockUnlock?.Invoke(false);
        }

        private Func<IRepositoryQuery<SUBJOB>, IQueryable<SUBJOB>> SUBJOBProjectionFunc()
        {
            //legacy subjob restrictions
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
            //return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && (x.PHASE != null && x.PHASE.PHASE_TYPE == PhaseType.Design));
        }

        private Func<IRepositoryQuery<Data.PHASE>, IQueryable<Data.PHASE>> PHASEProjectionFunc()
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
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == ProgressStatus.Live);

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
            IEnumerable<P6_ASSIGNMENT> P6_ASSIGNMENTS = GetEntities<P6_ASSIGNMENT>();
            return query => ProgressQueries.OffsiteDirectProgressItemTransformation(query.Where(x => x.GUID_BASELINE == loadBASELINE.GUID), loadPROJECT, live_PROGRESS, RATECollection, PROGRESS_ITEMCollection, null, true, P6_ASSIGNMENTS);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<BASELINE_ITEMProgress> entities)
        {
            MainViewModel.OnAfterEntitySavedCallBack = OnEntitiesSavedCallBack;
            MainViewModel.PasteListener = this.PasteListener;
            MainViewModel.SetParentViewModel(this);
            P6_ASSIGNMENTSCollectionViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
            //mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertiesChanged()));
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

        private void PasteListener(PasteStatus pasteStatus)
        {

        }

        protected override void OnBeforeApplyProjectionPropertiesToEntity(BASELINE_ITEMProgress projectionEntity, BASELINE_ITEM entity)
        {
            projectionEntity.Entity.Entity.GUID_BASELINE = loadBASELINE.GUID;
            //because TProjection is not IProjection<TMainEntity>, do it manually here
            DataUtils.ShallowCopy(entity, projectionEntity.Entity.Entity);
            base.OnBeforeApplyProjectionPropertiesToEntity(projectionEntity, entity);
        }

        public void OnEntitiesSavedCallBack(BASELINE_ITEMProgress projectionEntity, BASELINE_ITEM entity, bool isNewEntity)
        {
            projectionEntity.Entity.Entity.GUID_ORIGINAL = entity.GUID_ORIGINAL;
        }
        #endregion

        #endregion

        #region View Behavior
        public override string UnifiedRowValidation(BASELINE_ITEMProgress projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(BASELINE_ITEMProgress projection, string field_name, object new_value)
        {
            //budget hours field is disabled but just in case
            if (field_name == BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity) + "." +
                BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity) + "." +
                BindableBase.GetPropertyName(() => new BASELINE_ITEM().BUDGET_HOURS))
            {
                BASELINE_ITEMProgress validateEntity = projection;
                if (validateEntity.Entity.Entity.BY_DURATION && ((decimal)new_value) > 0)
                    return "Cannot set budget hours when deliverable is by duration";
            }
            else if (field_name == BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity) + "." +
                BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity) + "." +
                BindableBase.GetPropertyName(() => new BASELINE_ITEM().BY_DURATION))
            {
                BASELINE_ITEMProgress validateEntity = projection;
                if (validateEntity.Earned_Units_Total > 0)
                {
                    return "Cannot change deliverable tracking type when percentage is already earned";
                }
            }

            return string.Empty;
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
                decimal totalEstimatedHours = MainViewModel.Entities.Sum(x => x.Entity.Entity.BUDGET_HOURS);
                loadBASELINE.BUDGETED_UNITS = totalEstimatedHours;
            }

            BASELINECollectionViewModel.Save(loadBASELINE);
            SetBaselineLockUnlock?.Invoke(isLock);
            this.RaisePropertiesChanged();
        }

        public bool CanDuplicate()
        {
            if (MainViewModel == null || MainViewModel.SelectedEntities.Count == 0)
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
                MainViewModel.EntitiesUndoRedoManager.PauseActionId();

            List<BASELINE_ITEMProgress> newEntities = getNewEntities(1, true);
            newEntities = concatenateNewEntitiesWithExistingRenameEntities(newEntities);
            MainViewModel.BulkSave(newEntities);
            if (!_isProcessingMultiple)
                MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
        }

        /// <summary>
        /// Concatenate entities to be saved and entities to be renamed.
        /// </summary>
        /// <param name="newEntities">Entities to be saved.</param>
        /// <returns></returns>
        private List<BASELINE_ITEMProgress> concatenateNewEntitiesWithExistingRenameEntities(List<BASELINE_ITEMProgress> newEntities)
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
                    List<BASELINE_ITEMProgress> renameEntities = getRenameExistingEntities(valueToFillStringOnly, lowestUnsavedNumericValue, highestUnsavedNumericValue);
                    concatenatedEntities.AddRange(renameEntities);
                }
            }

            return concatenatedEntities;
        }

        public void Duplicate()
        {
            if (!_isProcessingMultiple)
                MainViewModel.EntitiesUndoRedoManager.PauseActionId();

            List<BASELINE_ITEMProgress> newEntities = getNewEntities(1, false);
            MainViewModel.BulkSave(newEntities);
            if (!_isProcessingMultiple)
                MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
        }

        /// <summary>
        /// Identify entities which internal number require to be named.
        /// </summary>
        /// <param name="renameStringOnly">Rename internal number string component only.</param>
        /// <param name="startNumber">Start of internal number to be named</param>
        /// <param name="endNumber">End if internal number to be named</param>
        /// <returns></returns>
        private List<BASELINE_ITEMProgress> getRenameExistingEntities(string renameStringOnly, long startNumber, long endNumber)
        {
            long valueToAdd = (endNumber - startNumber) + 1;
            List<BASELINE_ITEMProgress> renameEntities = new List<BASELINE_ITEMProgress>();
            foreach (BASELINE_ITEMProgress entity in MainViewModel.Entities)
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
                    entity.Entity.Entity.INTERNAL_NUM = StringFormatUtils.AppendStringWithEnumerator(valueToFillStringOnly, increasedNumber, numericFieldLength);
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(entity, "Entity.Entity.INTERNAL_NUM", oldInternalNum, entity.Entity.Entity.INTERNAL_NUM, EntityMessageType.Changed);
                    renameEntities.Add(entity);
                }
            }

            return renameEntities;
        }

        List<BASELINE_ITEMProgress> getNewEntities(int timesToDuplicate, bool isInsert)
        {
            List<BASELINE_ITEMProgress> unsavedEntities = new List<BASELINE_ITEMProgress>();
            for(int i = 0; i < timesToDuplicate; i++)
            {
                foreach (var selectedEntity in MainViewModel.SelectedEntities)
                {
                    var newProjection = new BASELINE_ITEMProgress();
                    DataUtils.ShallowCopy(newProjection.Entity.Entity, selectedEntity.Entity.Entity);
                    newProjection.Entity.EntityKey = Guid.Empty;
                    newProjection.Entity.Entity.GUID_ORIGINAL = Guid.Empty;
                    newProjection.Entity.Entity.BUDGET_HOURS = IsBASELINELocked ? 0 : selectedEntity.Entity.Entity.BUDGET_HOURS;
                    newProjection.Entity.Entity.DC_HOURS = 0;
                    var selectedAREA = AREACollection.FirstOrDefault(x => x.GUID == newProjection.Entity.Entity.GUID_AREA);
                    var selectedDISCIPLINE =
                        DISCIPLINECollection.FirstOrDefault(x => x.GUID == newProjection.Entity.Entity.GUID_DISCIPLINE);
                    var selectedDOCTYPE =
                        DOCTYPECollection.FirstOrDefault(x => x.GUID == newProjection.Entity.Entity.GUID_DOCTYPE);

                    newProjection.Entity.Entity.INTERNAL_NUM = 
                        BluePrintsDataUtils.GetNewInternalNumber(MainViewModel.Entities.Select(x => x.Entity), unsavedEntities.Select(x => x.Entity), selectedEntity.Entity.Entity.INTERNAL_NUM, MainViewModel.SelectedEntities.Select(x => x.Entity), isInsert);

                    //newProjection.Entity.Entity.INTERNAL_NUM = string.Empty;
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(newProjection, null, null, null, EntityMessageType.Added);
                    unsavedEntities.Add(newProjection);
                }
            }

            return unsavedEntities;
        }

        public bool CanDuplicateMultiple(BarEditItem barEdit)
        {
            if (MainViewModel == null || MainViewModel.SelectedEntities.Count == 0)
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
            MainViewModel.EntitiesUndoRedoManager.PauseActionId();
            _isProcessingMultiple = true;
            var timesToDuplicate = 0;
            List<BASELINE_ITEMProgress> newEntities = new List<BASELINE_ITEMProgress>();
            if (int.TryParse(barEdit.EditValue.ToString(), out timesToDuplicate))
            {
                List<BASELINE_ITEMProgress> currentEnumerationSaveEntities = getNewEntities(timesToDuplicate, false);
                newEntities.AddRange(currentEnumerationSaveEntities);
            }

            MainViewModel.BulkSave(newEntities);
            _isProcessingMultiple = false;
            MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
        }

        public void InsertMultiple(BarEditItem barEdit)
        {
            MainViewModel.EntitiesUndoRedoManager.PauseActionId();
            _isProcessingMultiple = true;
            var timesToInsert = 0;
            List<BASELINE_ITEMProgress> newEntities = new List<BASELINE_ITEMProgress>();
            if (int.TryParse(barEdit.EditValue.ToString(), out timesToInsert))
            {
                List<BASELINE_ITEMProgress> currentEnumerationSaveEntities = getNewEntities(timesToInsert, true);
                newEntities.AddRange(currentEnumerationSaveEntities);
            }

            newEntities = concatenateNewEntitiesWithExistingRenameEntities(newEntities);

            MainViewModel.BulkSave(newEntities);
            _isProcessingMultiple = false;
            MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
        }

        public bool CanAutoPopulate(object button)
        {
            if (MainViewModel == null || MainViewModel.SelectedEntities.Count == 0)
                return false;

            return true;
        }

        public void AutoPopulate(object button)
        {
            MainViewModel.EntitiesUndoRedoManager.PauseActionId();
            var info = GridPopupMenuBase.GetGridMenuInfo((DependencyObject) button) as GridMenuInfo;
            if (info.Column == null)
                return;

            var areaFieldName =     BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity) + "." +
                                    BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity) + "." +
                                    BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_AREA);
            var subAreaFieldName =  BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity) + "." +
                                    BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity) + "." +
                                    BindableBase.GetPropertyName(() => new BASELINE_ITEM().SubAreaGuid);
            var subjobFieldName = BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity) + "." + 
                                    BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity) + "." + 
                                    BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_SUBJOB);
            var internalNumberFieldName =
                BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity) + "." + 
                BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity) + "." +
                BindableBase.GetPropertyName(() => new BASELINE_ITEM().INTERNAL_NUM);

            var entitiesToSave = new List<BASELINE_ITEMProgress>();
            if (info.Column.FieldName == internalNumberFieldName)
                foreach (var entity in MainViewModel.SelectedEntities)
                    entity.Entity.Entity.INTERNAL_NUM = string.Empty;

            foreach (var entity in MainViewModel.SelectedEntities)
            {
                var entitySUBJOB =
                    SUBJOBCollection.FirstOrDefault(x => x.GUID == entity.Entity.Entity.GUID_SUBJOB);
                if (info.Column.FieldName == internalNumberFieldName)
                {
                    string internalNumber = generateInternalNumber(entity);
                    SetMainNestedValueWithUndoAndRefresh(entity, info.Column.FieldName, internalNumber);
                    entitiesToSave.Add(entity);
                }
                else if (info.Column.FieldName == areaFieldName || info.Column.FieldName == subAreaFieldName)
                {
                    if (entitySUBJOB == null)
                        continue;

                    if (info.Column.FieldName == areaFieldName)
                        SetMainNestedValueWithUndoAndRefresh(entity, info.Column.FieldName, entitySUBJOB.GUID_DAREA);
                    else if(info.Column.FieldName == subAreaFieldName)
                        SetMainNestedValueWithUndoAndRefresh(entity, info.Column.FieldName, entitySUBJOB.GUID_DSUBAREA);

                    entitiesToSave.Add(entity);
                }
                else if (info.Column.FieldName == subjobFieldName)
                {
                    if (entity.Entity.Entity.GUID_AREA == Guid.Empty || entity.Entity.Entity.GUID_DISCIPLINE == Guid.Empty)
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

                        List<AREA> sub_area_collection = new List<AREA>();
                        AREA defaultSubArea = null;
                        if (sub_area_collection.Count > 0)
                            defaultSubArea = sub_area_collection.FirstOrDefault(x => x.INTERNAL_NUM == BluePrintsResources.Default_Sub_Area);


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
                        ((CollectionViewModel<SUBJOB, SUBJOB, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<SUBJOB>()).Save(newSUBJOB);

                        SetMainNestedValueWithUndoAndRefresh(entity, info.Column.FieldName, newSUBJOB.GUID);
                    }
                    else
                    {
                        SetMainNestedValueWithUndoAndRefresh(entity, info.Column.FieldName, findSUBJOB.GUID);
                    }

                    entitiesToSave.Add(entity);
                }
            }

            MainViewModel.BulkSave(entitiesToSave);
            MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
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
            //get { return "BASELINE_ITEMSViewModelWrapper" + view_project_specific_affix; }
            get { return "BASELINE_ITEMSSchedulingGroupViewModelWrapper_v1" + view_project_specific_affix; }
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

        public IEnumerable<RATE> RATECollection
        {
            get
            {
                return GetEntities<RATE>();
            }
        }

        public IEnumerable<AREA> GetSUBAREACollection()
        {
            var collection = GetEntities<AREA>();
            if (collection != null)
                collection = collection.Where(x => x.GUID_PARENT != null).OrderBy(x => x.INTERNAL_NUM);
            return collection;
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

        public IEnumerable<Data.PHASE> PHASECollection
        {
            get
            {
                return GetEntities<Data.PHASE>();
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

        public override IEnumerable<ICanAssignP6> Deliverables_Source => DisplayEntities;
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
            IEnumerable<BASELINE_ITEMProgress> gridVisibleRows = GetGridVisibleRows();

            baselineReport.AssignProperties(loadPROJECT, loadBASELINE, gridVisibleRows.Select(x => x.Entity));
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