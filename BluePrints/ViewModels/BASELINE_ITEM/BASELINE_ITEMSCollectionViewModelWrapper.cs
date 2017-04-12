using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using BluePrints.Common.Utils;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.DataModel;
using BluePrints.Data;
using BluePrints.Common.ViewModel;
using DevExpress.Xpf.Grid;
using System.Threading;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data.Helpers;
using BluePrints.Common;
using BluePrints.Common.Helpers;
using BluePrints.Common.Projections;
using System.Windows.Threading;
using System.Windows;
using DevExpress.Xpf.Bars;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Views;
using BluePrints.Reports;
using System.IO;
using DevExpress.Data.Filtering;
using DevExpress.Xpf.Printing;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Settings;
using BluePrints.Common.Reports;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single BASELINE object view model.
    /// </summary>
    public partial class BASELINE_ITEMSCollectionViewModelWrapper :
        CollectionViewModelsWrapper
        <BASELINE_ITEM, PROGRESS_ITEMProjection, Guid, IBluePrintsEntitiesUnitOfWork,
            CollectionViewModel<BASELINE_ITEM, PROGRESS_ITEMProjection, Guid, IBluePrintsEntitiesUnitOfWork>>
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
            loaderCollection.AddLoaderDescription<DELIVERABLES_STATUS, DELIVERABLES_STATUS, Guid, IBluePrintsEntitiesUnitOfWork>(_bluePrintsUnitOfWorkFactory, x => x.DELIVERABLES_STATUSES);
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
            return query => query.Where(x => x.GUID_PROJECT == _loadProject.GUID);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == _loadProject.GUID);
        }

        private Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
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
                        query.OrderBy(x => x.INTERNAL_NUM), getPROGRESSFunc, getBASELINEFunc, getPROGRESS_ITEMSFunc,
                        getRATESFunc, getDELIVERABLES_STATUSESFunc);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<PROGRESS_ITEMProjection> entities)
        {
            MainViewModel.CreateNewProjectionFromNewEntityCallBack = CreateNewProjectionFromNewEntityCallBack;
            MainViewModel.ApplyProjectionPropertiesToEntityCallBack = ApplyProjectionPropertiesToEntity;
            MainViewModel.ApplyEntityPropertiesToProjectionCallBack = OnEntitiesSavedCallBack;
            MainViewModel.PasteListener = this.PasteListener;
            MainViewModel.SetParentViewModel(this);
            mainThreadDispatcher.BeginInvoke(new Action(() => ShowWORKPACKColumns()));

            base.AssignCallBacksAndRaisePropertyChange(entities);
            //mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertiesChanged()));
        }

        #region Collection Call Backs

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

        #endregion

        #endregion

        #region View Behavior

        public PROGRESS_ITEMProjection CreateNewProjectionFromNewEntityCallBack()
        {
            return new PROGRESS_ITEMProjection();
        }

        /// <summary>
        /// Influence column(s) when changes happens in other column
        /// </summary>
        public void CellValueChanging(CellValueChangedEventArgs e)
        {
            if (e.RowHandle != DataControlBase.NewItemRowHandle)
                return;

            var activeBASELINE_ITEM = (PROGRESS_ITEMProjection) e.Row;
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
                        BluePrintDataUtils.BASELINEITEM_Generate_InternalNumber(_loadProject, MainViewModel.Entities.Select(x => x.Entity),
                            SelectedAREA, SelectedDISCIPLINE, SelectedDOCTYPE);
                    MainViewModel.UpdateSelectedEntity();
                }
            }
            else if (e.Column.FieldName ==
                     BindableBase.GetPropertyName(() => new PROGRESS_ITEMProjection().Entity) + "." +
                     BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity) + "." +
                     BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_DOCTYPE))
            {
                var chosenDOCTYPE = DOCTYPECollection.FirstOrDefault(entity => entity.GUID == (Guid) e.Value);
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

        public void Duplicate()
        {
            if (!_isProcessingMultipleDuplicates)
                MainViewModel.EntitiesUndoRedoManager.PauseActionId();

            List<PROGRESS_ITEMProjection> newSaveEntities = getNewDuplicateEntities();
            MainViewModel.BulkSave(newSaveEntities);
            if (!_isProcessingMultipleDuplicates)
                MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
        }

        List<PROGRESS_ITEMProjection> getNewDuplicateEntities()
        {
            List<PROGRESS_ITEMProjection> saveEntities = new List<PROGRESS_ITEMProjection>();
            foreach (var selectedEntity in MainViewModel.SelectedEntities)
            {
                var newProjection = new PROGRESS_ITEMProjection();
                DataUtils.ShallowCopy(newProjection.Entity.Entity, selectedEntity.Entity.Entity);
                newProjection.Entity.GUID = Guid.Empty;
                newProjection.Entity.Entity.GUID_ORIGINAL = Guid.Empty;
                newProjection.Entity.Entity.ESTIMATED_HOURS = 0;
                newProjection.Entity.Entity.DC_HOURS = 0;
                var selectedAREA = AREACollection.FirstOrDefault(x => x.GUID == newProjection.Entity.Entity.GUID_AREA);
                var selectedDISCIPLINE =
                    DISCIPLINECollection.FirstOrDefault(x => x.GUID == newProjection.Entity.Entity.GUID_DISCIPLINE);
                var selectedDOCTYPE =
                    DOCTYPECollection.FirstOrDefault(x => x.GUID == newProjection.Entity.Entity.GUID_DOCTYPE);
                //newProjection.Entity.Entity.INTERNAL_NUM =
                //    BluePrintDataUtils.BASELINEITEM_Generate_InternalNumber(_loadProject, MainViewModel.Entities.Select(x => x.Entity),
                //        selectedAREA, selectedDISCIPLINE, selectedDOCTYPE, newProjection.GUID);
                newProjection.Entity.Entity.INTERNAL_NUM = string.Empty;

                MainViewModel.EntitiesUndoRedoManager.AddUndo(newProjection, null, null, null, EntityMessageType.Added);
                saveEntities.Add(newProjection);
            }

            return saveEntities;
        }

        public bool CanDuplicateMultiple(BarEditItem barEdit)
        {
            if (MainViewModel == null || MainViewModel.SelectedEntities.Count == 0)
                return false;

            return true;
        }

        private bool _isProcessingMultipleDuplicates;

        /// <summary>
        /// Paste clipboard data multiple times
        /// </summary>
        public void DuplicateMultiple(BarEditItem barEdit)
        {
            MainViewModel.EntitiesUndoRedoManager.PauseActionId();
            _isProcessingMultipleDuplicates = true;
            var timesToDuplicate = 0;
            List<PROGRESS_ITEMProjection> newSaveEntities = new List<PROGRESS_ITEMProjection>();
            if (int.TryParse(barEdit.EditValue.ToString(), out timesToDuplicate))
                for (var i = 0; i < timesToDuplicate; i++)
                {
                    List<PROGRESS_ITEMProjection> currentEnumerationSaveEntities = getNewDuplicateEntities();
                    newSaveEntities.AddRange(currentEnumerationSaveEntities);
                }

            MainViewModel.BulkSave(newSaveEntities);
            _isProcessingMultipleDuplicates = false;
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
                    var internalNum = BluePrintDataUtils.BASELINEITEM_Generate_InternalNumber(_loadProject,
                        MainViewModel.Entities.Select(x => x.Entity), currentItemAREA, currentItemDISCIPLINE,
                        currentItemDOCTYPE, entity.GUID);
                    SetMainNestedValueWithUndoAndRefresh(entity, info.Column.FieldName, internalNum);
                    entitiesToSave.Add(entity);
                }
                else if (info.Column.FieldName == departmentFieldName || info.Column.FieldName == disciplineFieldName ||
                         info.Column.FieldName == docTypeFieldName || info.Column.FieldName == areaFieldName)
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

                    entitiesToSave.Add(entity);
                }
                else if (info.Column.FieldName == workpackFieldName)
                {
                    if (entity.Entity.Entity.GUID_DISCIPLINE == Guid.Empty ||
                        entity.Entity.Entity.GUID_DEPARTMENT == Guid.Empty ||
                        entity.Entity.Entity.GUID_DOCTYPE == Guid.Empty || entity.Entity.Entity.GUID_AREA == Guid.Empty)
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
                        if (entity.Entity.Entity.GUID_PHASE != null)
                            newWORKPACK.GUID_DPHASE = entity.Entity.Entity.GUID_PHASE;
                        if (entity.Entity.Entity.GUID_DISCIPLINE != null)
                            newWORKPACK.GUID_DDISCIPLINE = (Guid) entity.Entity.Entity.GUID_DISCIPLINE;
                        if (entity.Entity.Entity.GUID_DEPARTMENT != null)
                            newWORKPACK.GUID_DDEPARTMENT = (Guid) entity.Entity.Entity.GUID_DEPARTMENT;
                        if (entity.Entity.Entity.GUID_DOCTYPE != null)
                            newWORKPACK.GUID_DDOCTYPE = (Guid) entity.Entity.Entity.GUID_DOCTYPE;

                        newWORKPACK.INTERNAL_NAME1 = BluePrintDataUtils.WORKPACK_Generate_InternalNumber1(_loadProject,
                            newWORKPACK, WORKPACKCollection, AREACollection, DISCIPLINECollection, DOCTYPECollection);
                        newWORKPACK.INTERNAL_NAME2 = BluePrintDataUtils.WORKPACK_Generate_InternalNumber2(_loadProject,
                            newWORKPACK, WORKPACKCollection, AREACollection, DISCIPLINECollection, PHASECollection);

                        if (newWORKPACK.INTERNAL_NAME1 == string.Empty && newWORKPACK.INTERNAL_NAME2 == string.Empty)
                            return;

                        newWORKPACK.STARTDATE = DateTime.Now;
                        newWORKPACK.ENDDATE =
                            BluePrintDataUtils.WORKPACK_Calculate_EndDate((DateTime) newWORKPACK.STARTDATE, _loadProject);
                        var reviewStartDate = (DateTime) newWORKPACK.STARTDATE;
                        var reviewEndDate = (DateTime) newWORKPACK.ENDDATE;
                        BluePrintDataUtils.WORKPACK_Calculate_ReviewPeriod(ref reviewStartDate, ref reviewEndDate,
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
                return _LoadBaseline.BUDGETED_UNITS == null ? 1000000000 : (decimal)_LoadBaseline.BUDGETED_UNITS;
            }
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "BASELINE_ITEMSViewModelWrapper"; }
        }

        /// <summary>
        /// The workpack internal name to be used
        /// </summary>
        public string WORKPACKDisplayMember
        {
            get
            {
                if (_loadProject == null || _loadProject.USELEGACYWORKPACK)
                    return BindableBase.GetPropertyName(() => new WORKPACK().INTERNAL_NAME1);
                else
                    return BindableBase.GetPropertyName(() => new WORKPACK().INTERNAL_NAME2);
            }
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
                    collection = collection.OrderBy(x => x.INTERNAL_NAME1).OrderBy(x => x.INTERNAL_NAME2);
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
                    collection = collection.OrderBy(x => x.INTERNAL_NUM);
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