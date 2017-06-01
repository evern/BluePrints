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
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
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
    public partial class BASELINE_ITEMSCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <BASELINE_ITEM, PROGRESS_ITEMProjection, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        public Action ShowWORKPACKInternalName1;
        public Action ShowWORKPACKInternalName2;
        public Action<bool> SetBaselineLockUnlock;

        /// <summary>
        /// Creates a new instance of BASELINE_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static BASELINE_ITEMSCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new BASELINE_ITEMSCollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the BASELINEViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the BASELINEViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected BASELINE_ITEMSCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        private PROJECT _loadProject;
        private BASELINE _LoadBaseline;
        private PROGRESS _LiveProgress;
        private bool _isQueryForLiveStatus;
        public bool IsPhaseVisible
        {
            get
            {
                if (_loadProject == null)
                    return false;

                return !_loadProject.USELEGACYWORKPACK;
            }
        }

        private readonly IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> _bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void InitializeParameters(object parameter)
        {
            var receiveParameter =
                (OptionalEntitiesParameter<PROJECT, BASELINE>) parameter;
            _loadProject = receiveParameter.GetFirstEntity();
            _LoadBaseline = receiveParameter.GetSecondEntity();

            if (_loadProject != null)
                _isQueryForLiveStatus = true;
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(_bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => _loadProject = x);
            loaderCollection.AddLoaderDescription(_bluePrintsUnitOfWorkFactory, x => x.BASELINES, BASELINEProjectionFunc, SetBASELINEIsLocked);
            loaderCollection.AddLoaderDescription(_bluePrintsUnitOfWorkFactory, x => x.WORKPACKS, WORKPACKProjectionFunc);
            loaderCollection.AddLoaderDescription(_bluePrintsUnitOfWorkFactory, x => x.PHASES, PHASEProjectionFunc);
            loaderCollection.AddLoaderDescription(_bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
            loaderCollection.AddLoaderDescription(_bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc);
            loaderCollection.AddLoaderDescription(_bluePrintsUnitOfWorkFactory, x => x.PROJECT_REPORTS, PROJECT_REPORTProjectionFunc, null, true);
            loaderCollection.AddLoaderDescription(_bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc, SetPROGRESStoCurrentDateOnLoaded);
            loaderCollection.AddLoaderDescription(_bluePrintsUnitOfWorkFactory, x => x.PROGRESS_ITEMS, PROGRESS_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(_bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(_bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(_bluePrintsUnitOfWorkFactory, x => x.DOCTYPES);
            loaderCollection.AddLoaderDescription(_bluePrintsUnitOfWorkFactory, x => x.DELIVERABLES_STATUSES, DELIVERABLES_STATUSProjectionFunc);
            loaderCollection.AddLoaderDescription<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(_bluePrintsUnitOfWorkFactory, x => x.USERS);

            InvokeEntitiesLoaderDescriptionLoading();
        }
        
        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            if (_isQueryForLiveStatus)
                return query => query.Where(x => x.GUID == _loadProject.GUID);
            else
                return query => query.Where(x => x.GUID == _LoadBaseline.GUID_PROJECT);
        }

        private Func<IRepositoryQuery<BASELINE>, IQueryable<BASELINE>> BASELINEProjectionFunc()
        {
            if (_isQueryForLiveStatus)
                return
                    query =>
                        query.Where(x => x.GUID_PROJECT == _loadProject.GUID && x.STATUS == BaselineStatus.Live);
            else
                return query => query.Where(x => x.GUID == _LoadBaseline.GUID);
        }

        private void SetBASELINEIsLocked(BASELINE entity)
        {
            _LoadBaseline = entity;
            if (entity.BUDGETED_UNITS != null && entity.BUDGETED_UNITS > 0)
                SetBaselineLockUnlock?.Invoke(true);
            else
                SetBaselineLockUnlock?.Invoke(false);
        }

        private void SetPROGRESStoCurrentDateOnLoaded(PROGRESS entity)
        {
            _LiveProgress = entity;
        }

        private Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK>> WORKPACKProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == _loadProject.GUID && x.TYPE == WorkpackType.Design);
        }

        private Func<IRepositoryQuery<PHASE>, IQueryable<PHASE>> PHASEProjectionFunc()
        {
            return query => query.Where(x => x.PHASE_TYPE == PhaseType.Design);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == _loadProject.GUID);
        }

        private Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == _loadProject.GUID);
        }

        private Func<IRepositoryQuery<DELIVERABLES_STATUS>, IQueryable<DELIVERABLES_STATUS>> DELIVERABLES_STATUSProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == _loadProject.GUID);
        }

        private Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> PROGRESSProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == _loadProject.GUID && x.STATUS == ProgressStatus.Live);

        }

        private Func<IRepositoryQuery<PROGRESS_ITEM>, IQueryable<PROGRESS_ITEM>> PROGRESS_ITEMProjectionFunc()
        {
            if(_LiveProgress == null)
                return query => query.Where(x => x.GUID_PROGRESS == Guid.Empty);
            else
                return query => query.Where(x => x.GUID_PROGRESS == _LiveProgress.GUID);
        }

        private Func<IRepositoryQuery<PROJECT_REPORT>, IQueryable<PROJECT_REPORT>> PROJECT_REPORTProjectionFunc()
        {
            return
                query =>
                    query.Where(
                        x =>
                            x.GUID_PROJECT == _loadProject.GUID && x.REPORT_TYPE == ReportType.Baseline_Report.ToString());
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(_bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<PROGRESS_ITEMProjection>>
            ConstructMainViewModelProjection()
        {
            var getRATESFunc = loaderCollection.GetCollectionFunc<RATE>();
            var getBASELINEFunc = loaderCollection.GetObjectFunc<BASELINE>();
            var getDELIVERABLES_STATUSESFunc =
                loaderCollection.GetCollectionFunc<DELIVERABLES_STATUS>();
            var getPROGRESSFunc = loaderCollection.GetObjectFunc<PROGRESS>();
            var getPROGRESS_ITEMSFunc = loaderCollection.GetCollectionFunc<PROGRESS_ITEM>();

            return
                query =>
                    PROGRESS_ITEMProjectionQueries.JoinRATESAndPROGRESS_ITEMSOnBASELINE_ITEMS(
                        query.OrderBy(x => x.CREATED), getPROGRESSFunc, getBASELINEFunc, getPROGRESS_ITEMSFunc,
                        getRATESFunc, getDELIVERABLES_STATUSESFunc, GetSUBAREACollection);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<PROGRESS_ITEMProjection> entities)
        {
            MainViewModel.CreateNewProjectionFromNewEntityCallBack = CreateNewProjectionFromNewEntityCallBack;
            MainViewModel.ExistingRowAddUndoAndSaveCallBack = ExistingRowAddUndoAndSaveCallBack;
            MainViewModel.ApplyProjectionPropertiesToEntityCallBack = ApplyProjectionPropertiesToEntity;
            MainViewModel.ApplyEntityPropertiesToProjectionCallBack = OnEntitiesSavedCallBack;
            MainViewModel.AdditionalValidateCellCallBack = AdditionalValidateCellCallBack;
            MainViewModel.PasteListener = this.PasteListener;
            MainViewModel.SetParentViewModel(this);
            mainThreadDispatcher.BeginInvoke(new Action(() => ShowWORKPACKColumns()));

            base.AssignCallBacksAndRaisePropertyChange(entities);
            //mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertiesChanged()));
        }

        #region Collection Call Backs
        public override void OnAfterAffectingEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender)
        {
            if (changedType == typeof(PROGRESS_ITEM))
            {
                FullRefreshWithoutClearingUndoRedo();
                return;
            }

            base.OnAfterAffectingEntitiesChanged(key, changedType, messageType, sender);
        }

        private void PasteListener(PasteStatus pasteStatus)
        {

        }


        public void ApplyProjectionPropertiesToEntity(PROGRESS_ITEMProjection projectionEntity, BASELINE_ITEM entity)
        {
            projectionEntity.Entity.Entity.GUID_BASELINE = _LoadBaseline.GUID;
            DataUtils.ShallowCopy(entity, projectionEntity.Entity.Entity);
            //workaround for created because Save() only sets the projection primary key, this is used for property redo where the interceptor only tampers with UPDATED and CREATED is left as null
            if (entity.CREATED.Date.Year == 1)
                projectionEntity.Entity.Entity.CREATED = DateTime.Now;

            entity.CREATED = projectionEntity.Entity.Entity.CREATED;
        }

        public void OnEntitiesSavedCallBack(Guid primaryKey, PROGRESS_ITEMProjection projectionEntity,
            BASELINE_ITEM entity, bool isNewEntity)
        {
            projectionEntity.Entity.Entity.GUID_ORIGINAL = entity.GUID_ORIGINAL;
        }

        private bool ExistingRowAddUndoAndSaveCallBack(PROGRESS_ITEMProjection projectionEntity, CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == BindableBase.GetPropertyName(() => new PROGRESS_ITEMProjection().Entity) + "." +
                BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity) + "." +
                BindableBase.GetPropertyName(() => new BASELINE_ITEM().BY_DURATION))
            {
                var activeBASELINE_ITEM = (PROGRESS_ITEMProjection)e.Row;
                if ((bool)e.Value)
                {
                    MainViewModel.EntitiesUndoRedoManager.PauseActionId();
                    decimal oldValue = activeBASELINE_ITEM.Entity.Entity.ESTIMATED_HOURS;
                    if (oldValue > 0)
                    {
                        decimal newValue = 0;
                        string estimatedHoursFieldName = BindableBase.GetPropertyName(() => new PROGRESS_ITEMProjection().Entity) + "." +
                        BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity) + "." +
                        BindableBase.GetPropertyName(() => new BASELINE_ITEM().ESTIMATED_HOURS);
                        activeBASELINE_ITEM.Entity.Entity.ESTIMATED_HOURS = newValue;
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(activeBASELINE_ITEM, estimatedHoursFieldName, oldValue, newValue, EntityMessageType.Changed);
                    }
                }
            }
            else if (e.Column.FieldName ==
                 BindableBase.GetPropertyName(() => new PROGRESS_ITEMProjection().Entity) + "." +
                 BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity) + "." +
                 BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_AREA))
            {
                var activeBASELINE_ITEM = (PROGRESS_ITEMProjection)e.Row;
                Guid? oldValue = activeBASELINE_ITEM.Entity.Entity.GUID_SUBAREA;
                if (e.Value != null && oldValue != null)
                {
                    Guid? newValue = (Guid?)null;
                    string subAreaFieldName = BindableBase.GetPropertyName(() => new PROGRESS_ITEMProjection().Entity) + "." +
                    BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity) + "." +
                    BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_SUBAREA);
                    activeBASELINE_ITEM.Entity.Entity.GUID_SUBAREA = newValue;
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(activeBASELINE_ITEM, subAreaFieldName, oldValue, newValue, EntityMessageType.Changed);
                }
            }
            else if (
                e.Column.FieldName ==
                BindableBase.GetPropertyName(() => new PROGRESS_ITEMProjection().Entity) + "." +
                BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity) + "." +
                BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_STATUS))
            {
                if (e.Value == null)
                    return false;
                else
                    return true;
            }

            return true;
        }
        #endregion

        #endregion

        #region View Behavior

        public PROGRESS_ITEMProjection CreateNewProjectionFromNewEntityCallBack()
        {
            return new PROGRESS_ITEMProjection();
        }

        private void AdditionalValidateCellCallBack(GridCellValidationEventArgs e)
        {
            //estimated hours field is disabled but just in case
            if (e.Column.FieldName == BindableBase.GetPropertyName(() => new PROGRESS_ITEMProjection().Entity) + "." +
                BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity) + "." +
                BindableBase.GetPropertyName(() => new BASELINE_ITEM().ESTIMATED_HOURS))
            {
                PROGRESS_ITEMProjection validateEntity = (PROGRESS_ITEMProjection)e.Row;
                if (validateEntity.Entity.Entity.BY_DURATION && ((decimal)e.Value) > 0)
                {
                    e.IsValid = false;
                    e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Critical;
                    e.ErrorContent = "Cannot set estimated hours when deliverable is by duration";
                }
            }
            else if (e.Column.FieldName == BindableBase.GetPropertyName(() => new PROGRESS_ITEMProjection().Entity) + "." +
                BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity) + "." +
                BindableBase.GetPropertyName(() => new BASELINE_ITEM().BY_DURATION))
            {
                PROGRESS_ITEMProjection validateEntity = (PROGRESS_ITEMProjection)e.Row;
                if (validateEntity.TOTAL_EARNED_UNITS > 0)
                {
                    e.IsValid = false;
                    e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Critical;
                    e.ErrorContent = "Cannot change deliverable tracking type when percentage is already earned";
                }
            }
        }

        /// <summary>
        /// Influence column(s) when changes happens in other column
        /// </summary>
        public void CellValueChanging(CellValueChangedEventArgs e)
        {
            var activeBASELINE_ITEM = (PROGRESS_ITEMProjection)e.Row;
            if (e.Column.FieldName == BindableBase.GetPropertyName(() => new PROGRESS_ITEMProjection().Entity) + "." +
                BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity) + "." +
                BindableBase.GetPropertyName(() => new BASELINE_ITEM().BY_DURATION))
            {
                if(e.RowHandle != DataControlBase.NewItemRowHandle)
                {
                    PostEditor?.Invoke();
                    return;
                }

                if ((bool)e.Value)
                {
                    decimal estimatedHours = activeBASELINE_ITEM.Entity.Entity.ESTIMATED_HOURS;
                    if(estimatedHours > 0)
                    {
                        decimal newValue = 0;
                        string estimatedHoursFieldName = BindableBase.GetPropertyName(() => new PROGRESS_ITEMProjection().Entity) + "." +
                        BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity) + "." +
                        BindableBase.GetPropertyName(() => new BASELINE_ITEM().ESTIMATED_HOURS);
                        activeBASELINE_ITEM.Entity.Entity.ESTIMATED_HOURS = newValue;
                        MainViewModel.UpdateSelectedEntity();
                    }
                }
            }

            if (e.Column.FieldName ==
                     BindableBase.GetPropertyName(() => new PROGRESS_ITEMProjection().Entity) + "." +
                     BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity) + "." +
                     BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_DOCTYPE))
            {
                var chosenDOCTYPE = DOCTYPECollection.FirstOrDefault(entity => entity.GUID == (Guid)e.Value);
                if (chosenDOCTYPE != null && activeBASELINE_ITEM.Entity.Entity.GUID_STATUS != null)
                {
                    activeBASELINE_ITEM.Entity.Entity.GUID_STATUS = null;
                    MainViewModel.UpdateSelectedEntity();
                }

                if (chosenDOCTYPE != null && chosenDOCTYPE.GUID_DDEPARTMENT != null)
                {
                    activeBASELINE_ITEM.Entity.Entity.GUID_DEPARTMENT = chosenDOCTYPE.DEPARTMENT.GUID;
                    MainViewModel.UpdateSelectedEntity();
                }
            }

            if (e.Column.FieldName ==
                 BindableBase.GetPropertyName(() => new PROGRESS_ITEMProjection().Entity) + "." +
                 BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity) + "." +
                 BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_AREA))
            {
                if(e.Value != null)
                {
                    activeBASELINE_ITEM.Entity.Entity.GUID_AREA = (Guid)e.Value;
                    activeBASELINE_ITEM.Entity.SetAvailableSubAreas(SUBAREACollection);
                    MainViewModel.UpdateSelectedEntity();
                }

                if (e.Value != null && activeBASELINE_ITEM.Entity.Entity.GUID_SUBAREA != null)
                {
                    activeBASELINE_ITEM.Entity.Entity.GUID_SUBAREA = null;
                    MainViewModel.UpdateSelectedEntity();
                }
            }

            if (e.RowHandle != DataControlBase.NewItemRowHandle)
                return;

            if (e.Column.FieldName ==
                BindableBase.GetPropertyName(() => new PROGRESS_ITEMProjection().Entity) + "." +
                BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity) + "." +
                BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_WORKPACK))
            {
                var chosenWORKPACK = WORKPACKCollection.FirstOrDefault(entity => entity.GUID == (Guid) e.Value);
                if (chosenWORKPACK != null)
                {
                    activeBASELINE_ITEM.Entity.Entity.GUID_AREA = chosenWORKPACK.GUID_DAREA;
                    activeBASELINE_ITEM.Entity.Entity.GUID_DOCTYPE = chosenWORKPACK.GUID_DDOCTYPE;
                    activeBASELINE_ITEM.Entity.Entity.GUID_DEPARTMENT = chosenWORKPACK.GUID_DDEPARTMENT;
                    activeBASELINE_ITEM.Entity.Entity.GUID_DISCIPLINE = chosenWORKPACK.GUID_DDISCIPLINE;
                    activeBASELINE_ITEM.Entity.Entity.GUID_PHASE = chosenWORKPACK.PHASE != null
                        ? chosenWORKPACK.GUID_DPHASE
                        : null;
                    var SelectedAREA = AREACollection.FirstOrDefault(x => x.GUID == chosenWORKPACK.GUID_DAREA);
                    var SelectedDOCTYPE = DOCTYPECollection.FirstOrDefault(x => x.GUID == chosenWORKPACK.GUID_DDOCTYPE);
                    var SelectedDISCIPLINE =
                        DISCIPLINECollection.FirstOrDefault(x => x.GUID == chosenWORKPACK.GUID_DDISCIPLINE);

                    activeBASELINE_ITEM.Entity.Entity.INTERNAL_NUM =
                        BluePrintsDataUtils.BASELINEITEM_Generate_InternalNumber(_loadProject, MainViewModel.Entities.Select(x => x.Entity),
                            SelectedAREA, SelectedDISCIPLINE, SelectedDOCTYPE);
                    MainViewModel.UpdateSelectedEntity();
                }
            }

            if (e.Column.FieldName ==
                 BindableBase.GetPropertyName(() => new PROGRESS_ITEMProjection().Entity) + "." +
                 BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity) + "." +
                 BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_DOCTYPE))
            {
                var chosenDOCTYPE = DOCTYPECollection.FirstOrDefault(entity => entity.GUID == (Guid)e.Value);
                if (chosenDOCTYPE != null && chosenDOCTYPE.GUID_DDEPARTMENT != null)
                {
                    activeBASELINE_ITEM.Entity.Entity.GUID_DEPARTMENT = chosenDOCTYPE.DEPARTMENT.GUID;
                    MainViewModel.UpdateSelectedEntity();
                }
            }
        }

        /// <summary>
        /// Refresh all min max units for converter to do estimated hours validation
        /// </summary>
        public void CellValueChanged(CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName ==
                                 BindableBase.GetPropertyName(() => new PROGRESS_ITEMProjection().Entity) + "." +
                                 BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity) + "." +
                                 BindableBase.GetPropertyName(() => new BASELINE_ITEM().ESTIMATED_HOURS))
                this.RaisePropertiesChanged();
        }
        #endregion

        #region View Commands
        public bool IsBASELINELocked
        {
            get
            {
                if (_LoadBaseline == null)
                    return true;
                else
                    return _LoadBaseline.BUDGETED_UNITS != null && _LoadBaseline.BUDGETED_UNITS > 0;
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
                _LoadBaseline.BUDGETED_UNITS = 0;
            else
            {
                decimal totalEstimatedHours = MainViewModel.Entities.Sum(x => x.Entity.Entity.ESTIMATED_HOURS);
                _LoadBaseline.BUDGETED_UNITS = totalEstimatedHours;
            }

            BASELINECollectionViewModel.Save(_LoadBaseline);
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

            List<PROGRESS_ITEMProjection> newEntities = getNewEntities(1, true);
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
        private List<PROGRESS_ITEMProjection> concatenateNewEntitiesWithExistingRenameEntities(List<PROGRESS_ITEMProjection> newEntities)
        {
            List<PROGRESS_ITEMProjection> concatenatedEntities = new List<PROGRESS_ITEMProjection>();
            concatenatedEntities.AddRange(newEntities);

            List<string> processedValueToFillStringOnly = new List<string>();
            foreach(PROGRESS_ITEMProjection entity in newEntities.OrderBy(x => x.Entity.Entity.INTERNAL_NUM))
            {
                long lowestUnsavedNumericValue = 0;
                long highestUnsavedNumericValue = 0;

                int numericFieldLength = 0;
                long arbitraryNumericValue = 0;
                string valueToFill = entity.Entity.Entity.INTERNAL_NUM;
                if (valueToFill == string.Empty)
                    return concatenatedEntities;

                string valueToFillStringOnly = StringFormatUtils.ParseStringIntoComponents(valueToFill, out numericFieldLength, out arbitraryNumericValue);

                List<PROGRESS_ITEMProjection> relatedNewEntities = newEntities.Where(x => x.Entity.Entity.INTERNAL_NUM.Contains(valueToFillStringOnly)).ToList();
                PROGRESS_ITEMProjection smallestNumberEntity = relatedNewEntities.First();
                PROGRESS_ITEMProjection largestNumberEntity = relatedNewEntities.Last();

                string smallestInternalNum = smallestNumberEntity.Entity.Entity.INTERNAL_NUM;
                string largestInternalNum = largestNumberEntity.Entity.Entity.INTERNAL_NUM;

                valueToFillStringOnly = StringFormatUtils.ParseStringIntoComponents(smallestInternalNum, out numericFieldLength, out lowestUnsavedNumericValue);
                valueToFillStringOnly = StringFormatUtils.ParseStringIntoComponents(largestInternalNum, out numericFieldLength, out highestUnsavedNumericValue);
                if(!processedValueToFillStringOnly.Contains(valueToFillStringOnly))
                {
                    processedValueToFillStringOnly.Add(valueToFillStringOnly);
                    List<PROGRESS_ITEMProjection> renameEntities = getRenameExistingEntities(valueToFillStringOnly, lowestUnsavedNumericValue, highestUnsavedNumericValue);
                    concatenatedEntities.AddRange(renameEntities);
                }
            }

            return concatenatedEntities;
        }

        public void Duplicate()
        {
            if (!_isProcessingMultiple)
                MainViewModel.EntitiesUndoRedoManager.PauseActionId();

            List<PROGRESS_ITEMProjection> newEntities = getNewEntities(1, false);
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
        private List<PROGRESS_ITEMProjection> getRenameExistingEntities(string renameStringOnly, long startNumber, long endNumber)
        {
            long valueToAdd = (endNumber - startNumber) + 1;
            List<PROGRESS_ITEMProjection> renameEntities = new List<PROGRESS_ITEMProjection>();
            foreach (PROGRESS_ITEMProjection entity in MainViewModel.Entities)
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

        List<PROGRESS_ITEMProjection> getNewEntities(int timesToDuplicate, bool isInsert)
        {
            List<PROGRESS_ITEMProjection> unsavedEntities = new List<PROGRESS_ITEMProjection>();
            for(int i = 0; i < timesToDuplicate; i++)
            {
                foreach (var selectedEntity in MainViewModel.SelectedEntities)
                {
                    var newProjection = new PROGRESS_ITEMProjection();
                    DataUtils.ShallowCopy(newProjection.Entity.Entity, selectedEntity.Entity.Entity);
                    newProjection.Entity.EntityKey = Guid.Empty;
                    newProjection.Entity.Entity.GUID_ORIGINAL = Guid.Empty;
                    newProjection.Entity.Entity.ESTIMATED_HOURS = IsBASELINELocked ? 0 : selectedEntity.Entity.Entity.ESTIMATED_HOURS;
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
            List<PROGRESS_ITEMProjection> newEntities = new List<PROGRESS_ITEMProjection>();
            if (int.TryParse(barEdit.EditValue.ToString(), out timesToDuplicate))
            {
                List<PROGRESS_ITEMProjection> currentEnumerationSaveEntities = getNewEntities(timesToDuplicate, false);
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
            List<PROGRESS_ITEMProjection> newEntities = new List<PROGRESS_ITEMProjection>();
            if (int.TryParse(barEdit.EditValue.ToString(), out timesToInsert))
            {
                List<PROGRESS_ITEMProjection> currentEnumerationSaveEntities = getNewEntities(timesToInsert, true);
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

            var departmentFieldName =
                BindableBase.GetPropertyName(() => new PROGRESS_ITEMProjection().Entity) + "." +
                BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity) + "." +
                BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_DEPARTMENT);
            var disciplineFieldName =
                                    BindableBase.GetPropertyName(() => new PROGRESS_ITEMProjection().Entity) + "." +
                                    BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity) + "." +
                                    BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_DISCIPLINE);
            var docTypeFieldName =  BindableBase.GetPropertyName(() => new PROGRESS_ITEMProjection().Entity) + "." +
                                    BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity) + "." +             BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_DOCTYPE);
            var areaFieldName =     BindableBase.GetPropertyName(() => new PROGRESS_ITEMProjection().Entity) + "." +
                                    BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity) + "." +
                                    BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_AREA);
            var subAreaFieldName =  BindableBase.GetPropertyName(() => new PROGRESS_ITEMProjection().Entity) + "." +
                                    BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().SubAreaGuid);
            var workpackFieldName = BindableBase.GetPropertyName(() => new PROGRESS_ITEMProjection().Entity) + "." +
                                    BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity) + "." +             BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_WORKPACK);
            var internalNumberFieldName =
                BindableBase.GetPropertyName(() => new PROGRESS_ITEMProjection().Entity) + "." + 
                BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity) + "." +
                BindableBase.GetPropertyName(() => new BASELINE_ITEM().INTERNAL_NUM);

            var entitiesToSave = new List<PROGRESS_ITEMProjection>();
            if (info.Column.FieldName == internalNumberFieldName)
                foreach (var entity in MainViewModel.SelectedEntities)
                    entity.Entity.Entity.INTERNAL_NUM = string.Empty;

            foreach (var entity in MainViewModel.SelectedEntities)
            {
                var entityWORKPACK =
                    WORKPACKCollection.FirstOrDefault(x => x.GUID == entity.Entity.Entity.GUID_WORKPACK);
                if (info.Column.FieldName == internalNumberFieldName)
                {
                    AREA currentItemAREA = AREACollection.FirstOrDefault((x => x.GUID == entity.Entity.Entity.GUID_AREA));
                    DISCIPLINE currentItemDISCIPLINE =
                        DISCIPLINECollection.FirstOrDefault((x => x.GUID == entity.Entity.Entity.GUID_DISCIPLINE));
                    DOCTYPE currentItemDOCTYPE =
                        DOCTYPECollection.FirstOrDefault((x => x.GUID == entity.Entity.Entity.GUID_DOCTYPE));
                    var internalNum = BluePrintsDataUtils.BASELINEITEM_Generate_InternalNumber(_loadProject,
                        MainViewModel.Entities.Select(x => x.Entity), currentItemAREA, currentItemDISCIPLINE,
                        currentItemDOCTYPE, entity.EntityKey);
                    SetMainNestedValueWithUndoAndRefresh(entity, info.Column.FieldName, internalNum);
                    entitiesToSave.Add(entity);
                }
                else if (info.Column.FieldName == departmentFieldName || info.Column.FieldName == disciplineFieldName ||
                         info.Column.FieldName == docTypeFieldName || info.Column.FieldName == areaFieldName || info.Column.FieldName == subAreaFieldName)
                {
                    if (entityWORKPACK == null)
                        continue;

                    if (info.Column.FieldName == departmentFieldName)
                        SetMainNestedValueWithUndoAndRefresh(entity, info.Column.FieldName,
                            entityWORKPACK.GUID_DDEPARTMENT);
                    else if (info.Column.FieldName == disciplineFieldName)
                        SetMainNestedValueWithUndoAndRefresh(entity, info.Column.FieldName,
                            entityWORKPACK.GUID_DDISCIPLINE);
                    else if (info.Column.FieldName == docTypeFieldName)
                        SetMainNestedValueWithUndoAndRefresh(entity, info.Column.FieldName, entityWORKPACK.GUID_DDOCTYPE);
                    else if (info.Column.FieldName == areaFieldName)
                        SetMainNestedValueWithUndoAndRefresh(entity, info.Column.FieldName, entityWORKPACK.GUID_DAREA);
                    else if(info.Column.FieldName == subAreaFieldName && IsPhaseVisible)
                        SetMainNestedValueWithUndoAndRefresh(entity, info.Column.FieldName, entityWORKPACK.GUID_DSUBAREA);

                    entitiesToSave.Add(entity);
                }
                else if (info.Column.FieldName == workpackFieldName)
                {
                    if (entity.Entity.Entity.GUID_DISCIPLINE == Guid.Empty ||
                        entity.Entity.Entity.GUID_DEPARTMENT == Guid.Empty ||
                        entity.Entity.Entity.GUID_DOCTYPE == Guid.Empty || entity.Entity.Entity.GUID_AREA == Guid.Empty)
                        continue;

                    if (IsPhaseVisible && entity.Entity.Entity.GUID_PHASE == Guid.Empty)
                        continue;

                    var findWORKPACK =
                        WORKPACKCollection.FirstOrDefault(
                            x =>
                                x.GUID_DAREA == entity.Entity.Entity.GUID_AREA &&
                                x.GUID_DDOCTYPE == entity.Entity.Entity.GUID_DOCTYPE &&
                                x.GUID_DDISCIPLINE == entity.Entity.Entity.GUID_DISCIPLINE);
                    if (findWORKPACK == null)
                    {
                        var newWORKPACK = new WORKPACK();
                        newWORKPACK.GUID_PROJECT = _loadProject.GUID;
                        if (entity.Entity.Entity.GUID_AREA != null)
                            newWORKPACK.GUID_DAREA = (Guid) entity.Entity.Entity.GUID_AREA;
                        if (entity.Entity.Entity.GUID_SUBAREA != null)
                            newWORKPACK.GUID_DSUBAREA = (Guid)entity.Entity.Entity.GUID_SUBAREA;
                        if (entity.Entity.Entity.GUID_PHASE != null)
                            newWORKPACK.GUID_DPHASE = entity.Entity.Entity.GUID_PHASE;
                        if (entity.Entity.Entity.GUID_DISCIPLINE != null)
                            newWORKPACK.GUID_DDISCIPLINE = (Guid) entity.Entity.Entity.GUID_DISCIPLINE;
                        if (entity.Entity.Entity.GUID_DEPARTMENT != null)
                            newWORKPACK.GUID_DDEPARTMENT = (Guid) entity.Entity.Entity.GUID_DEPARTMENT;
                        if (entity.Entity.Entity.GUID_DOCTYPE != null)
                            newWORKPACK.GUID_DDOCTYPE = (Guid) entity.Entity.Entity.GUID_DOCTYPE;

                        if(IsPhaseVisible)
                            newWORKPACK.INTERNAL_NAME1 = BluePrintsDataUtils.WORKPACK_Generate_InternalNumber2(_loadProject, newWORKPACK, WORKPACKCollection, AREACollection, SUBAREACollection, DISCIPLINECollection, PHASECollection, DOCTYPECollection);
                        else
                            newWORKPACK.INTERNAL_NAME1 = BluePrintsDataUtils.WORKPACK_Generate_InternalNumber1(_loadProject, newWORKPACK, WORKPACKCollection, AREACollection, DISCIPLINECollection, DOCTYPECollection);

                        if (newWORKPACK.INTERNAL_NAME1 == string.Empty)
                            return;

                        newWORKPACK.STARTDATE = DateTime.Now;
                        newWORKPACK.ENDDATE =
                            BluePrintsDataUtils.WORKPACK_Calculate_EndDate((DateTime) newWORKPACK.STARTDATE, _loadProject);
                        var reviewStartDate = (DateTime) newWORKPACK.STARTDATE;
                        var reviewEndDate = (DateTime) newWORKPACK.ENDDATE;
                        BluePrintsDataUtils.WORKPACK_Calculate_ReviewPeriod(ref reviewStartDate, ref reviewEndDate,
                            _loadProject, false);
                        newWORKPACK.REVIEWSTARTDATE = reviewStartDate;
                        newWORKPACK.REVIEWENDDATE = reviewEndDate;
                        newWORKPACK.AUTOGENERATED = true;
                        newWORKPACK.TYPE = WorkpackType.Design;
                        ((CollectionViewModel<WORKPACK, WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork>)
                            loaderCollection.GetViewModel<WORKPACK>()).Save(newWORKPACK);

                        SetMainNestedValueWithUndoAndRefresh(entity, info.Column.FieldName, newWORKPACK.GUID);
                    }
                    else
                    {
                        SetMainNestedValueWithUndoAndRefresh(entity, info.Column.FieldName, findWORKPACK.GUID);
                    }

                    entitiesToSave.Add(entity);
                }
            }

            MainViewModel.BulkSave(entitiesToSave);
            MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
            RefreshView();
        }

        #endregion

        #region View Properties
        public decimal TotalAllowedUnits
        {
            get
            {
                return (_LoadBaseline == null || _LoadBaseline.BUDGETED_UNITS == null) ? 1000000000 : (decimal)_LoadBaseline.BUDGETED_UNITS;
            }
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "BASELINE_ITEMSViewModelWrapper"; }
        }

        public void ShowWORKPACKColumns()
        {
            if (ShowWORKPACKInternalName1 == null || ShowWORKPACKInternalName2 == null)
                return;

            if (_loadProject == null || _loadProject.USELEGACYWORKPACK)
                ShowWORKPACKInternalName1();
            else
                ShowWORKPACKInternalName2();
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
            var reportDesigner = new UserReportDesigner(_loadProject,
                (CollectionViewModel<PROJECT_REPORT, PROJECT_REPORT, Guid, IBluePrintsEntitiesUnitOfWork>)
                loaderCollection.GetViewModel<PROJECT_REPORT>(), ReportType.Baseline_Report);
            if (reportDesigner.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                reportDesigner.Dispose();
            else
                reportDesigner.Dispose();
        }

        public Func<IEnumerable<PROGRESS_ITEMProjection>> GetGridVisibleRows;

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
            IEnumerable<PROGRESS_ITEMProjection> gridVisibleRows = GetGridVisibleRows();

            baselineReport.AssignProperties(_loadProject, _LoadBaseline, gridVisibleRows.Select(x => x.Entity));
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
            return _loadProject.NUMBER + "_Baseline_Rev_" + _LoadBaseline.REVISION + ".xlsx";
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