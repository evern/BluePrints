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
using DevExpress.Xpf.Printing;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Settings;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single BASELINE object view model.
    /// </summary>
    public partial class BASELINE_ITEMSViewModelWrapper : CollectionViewModelsWrapper<BASELINE_ITEM, BASELINE_ITEMProjection, Guid, IBluePrintsEntitiesUnitOfWork, CollectionViewModel<BASELINE_ITEM, BASELINE_ITEMProjection, Guid, IBluePrintsEntitiesUnitOfWork>>
    {
        public Action ShowWORKPACKInternalName1;
        public Action ShowWORKPACKInternalName2;
        /// <summary>
        /// Creates a new instance of BASELINE_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static BASELINE_ITEMSViewModelWrapper Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new BASELINE_ITEMSViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the BASELINEViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the BASELINEViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected BASELINE_ITEMSViewModelWrapper(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        PROJECT loadPROJECT;
        BASELINE loadBASELINE;
        bool isQueryForLiveStatus;
        IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected override void InitializeParameters(object parameter)
        {
            OptionalEntitiesParameter<PROJECT, BASELINE> receiveParameter = (OptionalEntitiesParameter<PROJECT, BASELINE>)parameter;
            this.loadPROJECT = receiveParameter.GetFirstEntity();
            this.loadBASELINE = receiveParameter.GetSecondEntity();

            if (this.loadPROJECT != null)
                isQueryForLiveStatus = true;
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddEntitiesLoader<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>(0, bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, null, isContinueLoadingAfterPROJECT, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<BASELINE, BASELINE, Guid, IBluePrintsEntitiesUnitOfWork>(1, bluePrintsUnitOfWorkFactory, x => x.BASELINES, BASELINEProjectionFunc, typeof(PROJECT), isContinueLoadingAfterBASELINE, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<WORKPACK, WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork>(2, bluePrintsUnitOfWorkFactory, x => x.WORKPACKS, WORKPACKProjectionFunc, typeof(PROJECT), null, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<PHASE, PHASE, Guid, IBluePrintsEntitiesUnitOfWork>(3, bluePrintsUnitOfWorkFactory, x => x.PHASES, PHASEProjectionFunc, typeof(PROJECT));
            loaderCollection.AddEntitiesLoader<AREA, AREA, Guid, IBluePrintsEntitiesUnitOfWork>(4, bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc, typeof(PROJECT));
            loaderCollection.AddEntitiesLoader<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(5, bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddEntitiesLoader<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(6, bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddEntitiesLoader<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(7, bluePrintsUnitOfWorkFactory, x => x.DOCTYPES);
            loaderCollection.AddEntitiesLoader<RATE, RATE, Guid, IBluePrintsEntitiesUnitOfWork>(8, bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc, typeof(PROJECT), null, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<PROJECT_REPORT, PROJECT_REPORT, Guid, IBluePrintsEntitiesUnitOfWork>(9, bluePrintsUnitOfWorkFactory, x => x.PROJECT_REPORTS, PROJECT_REPORTProjectionFunc, typeof(BluePrints.Data.PROJECT));
            
            InvokeEntitiesLoaderDescriptionLoading();
        }

        bool isContinueLoadingAfterPROJECT(IEnumerable<PROJECT> entities)
        {
            if(entities.Count() == 0)
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, "PROJECT"))));
                return false;
            }

            this.loadPROJECT = entities.First();
            return true;
        }

        bool isContinueLoadingAfterBASELINE(IEnumerable<BASELINE> entities)
        {
            if (entities.Count() == 0)
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, "BASELINE"))));
                return false;
            }

            this.loadBASELINE = entities.First();
            return true;
        }

        Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            if (isQueryForLiveStatus)
                return query => query.Where(x => x.GUID == this.loadPROJECT.GUID);
            else
                return query => query.Where(x => x.GUID == this.loadBASELINE.GUID_PROJECT);
        }

        Func<IRepositoryQuery<BASELINE>, IQueryable<BASELINE>> BASELINEProjectionFunc()
        {
            if (isQueryForLiveStatus)
                return query => query.Where(x => x.GUID_PROJECT == this.loadPROJECT.GUID && x.STATUS == BaselineStatus.Live);
            else
                return query => query.Where(x => x.GUID == this.loadBASELINE.GUID);
        }

        Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK>> WORKPACKProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.TYPE == WorkpackType.Design);
        }

        Func<IRepositoryQuery<PHASE>, IQueryable<PHASE>> PHASEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        Func<IRepositoryQuery<PROJECT_REPORT>, IQueryable<PROJECT_REPORT>> PROJECT_REPORTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.REPORT_TYPE == ReportType.Baseline_Report.ToString());
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(this.bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoader.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEMProjection>> ConstructMainViewModelProjection()
        {
            Func<IQueryable<RATE>> getRATESFunc = loaderCollection.GetCollectionFunc<RATE>();
            Func<BASELINE> getBASELINEFunc = loaderCollection.GetObjectFunc<BASELINE>();
            return query => BASELINE_ITEMProjectionQueries.JoinRATESOnBASELINE_ITEMS(query, getBASELINEFunc, getRATESFunc);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<BASELINE_ITEMProjection> entities)
        {
            MainViewModel.CreateNewProjectionFromNewEntityCallBack = this.CreateNewProjectionFromNewEntityCallBack;
            MainViewModel.ApplyProjectionPropertiesToEntityCallBack = this.ApplyProjectionPropertiesToEntity;
            MainViewModel.OnEntitySavedCallBack = this.OnEntitiesSavedCallBack;
            MainViewModel.SetParentViewModel(this);
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertiesChanged()));
            mainThreadDispatcher.BeginInvoke(new Action(() => this.ShowWORKPACKColumns()));
        }

        protected override void OnAfterEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender)
        {
            if (sender.ToString() == MainViewModel.ToString())
                return;

            if (changedType == typeof(WORKPACK))
            {
                this.RaisePropertyChanged(x => x.WORKPACKCollection);
                return;
            }

            if (loadBASELINE != null && changedType == typeof(BASELINE) && loadBASELINE.GUID.ToString() == key.ToString() ||
                loadPROJECT != null && changedType == typeof(PROJECT) && loadPROJECT.GUID.ToString() == key.ToString())
            {
                if (messageType == EntityMessageType.Added)
                    MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Restored, StringFormatUtils.GetEntityNameByType(changedType)));
                else if (messageType == EntityMessageType.Deleted)
                    MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, StringFormatUtils.GetEntityNameByType(changedType)));
            }

            if (loadPROJECT != null || loadBASELINE != null)
            {
                if (MainViewModel != null)
                    mainThreadDispatcher.BeginInvoke(new Action(() => MainViewModel.Refresh()));
                else if (loadPROJECT != null || loadBASELINE != null)
                    mainThreadDispatcher.BeginInvoke(new Action(() => InitializeAndLoadEntitiesLoaderDescription()));
            }

            return;
        }

        #region Collection Call Backs
        public void ApplyProjectionPropertiesToEntity(BASELINE_ITEMProjection projectionEntity, BASELINE_ITEM entity)
        {
            projectionEntity.BASELINE_ITEM.GUID_BASELINE = loadBASELINE.GUID;
            DataUtils.ShallowCopy(entity, projectionEntity.BASELINE_ITEM);
            //workaround for created because Save() only sets the projection primary key, this is used for property redo where the interceptor only tampers with UPDATED and CREATED is left as null
            if (entity.CREATED.Date.Year == 1)
                projectionEntity.BASELINE_ITEM.CREATED = DateTime.Now;

            entity.CREATED = projectionEntity.BASELINE_ITEM.CREATED;
        }

        public void OnEntitiesSavedCallBack(Guid primaryKey, BASELINE_ITEMProjection projectionEntity, BASELINE_ITEM entity, bool isNewEntity)
        {
            projectionEntity.GUID = entity.GUID;
            projectionEntity.BASELINE_ITEM.GUID = entity.GUID;
            projectionEntity.BASELINE_ITEM.GUID_ORIGINAL = entity.GUID_ORIGINAL;
        }
        #endregion
        #endregion

        #region View Behavior
        public BASELINE_ITEMProjection CreateNewProjectionFromNewEntityCallBack(BASELINE_ITEM entity)
        {
            return new BASELINE_ITEMProjection();
        }

        /// <summary>
        /// Influence column(s) when changes happens in other column
        /// </summary>
        public void CellValueChanging(CellValueChangedEventArgs e)
        {
            if (e.RowHandle != GridControl.NewItemRowHandle)
                return;

            BASELINE_ITEMProjection activeBASELINE_ITEM = (BASELINE_ITEMProjection)e.Row;
            if (e.Column.FieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().BASELINE_ITEM) + "." + BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_WORKPACK))
            {
                WORKPACK chosenWORKPACK = WORKPACKCollection.FirstOrDefault(entity => entity.GUID == (Guid)e.Value);
                if (chosenWORKPACK != null)
                {
                    activeBASELINE_ITEM.BASELINE_ITEM.GUID_AREA = chosenWORKPACK.GUID_DAREA;
                    activeBASELINE_ITEM.BASELINE_ITEM.GUID_DOCTYPE = chosenWORKPACK.GUID_DDOCTYPE;
                    activeBASELINE_ITEM.BASELINE_ITEM.GUID_DEPARTMENT = chosenWORKPACK.GUID_DDEPARTMENT;
                    activeBASELINE_ITEM.BASELINE_ITEM.GUID_DISCIPLINE = chosenWORKPACK.GUID_DDISCIPLINE;
                    activeBASELINE_ITEM.BASELINE_ITEM.GUID_PHASE = chosenWORKPACK.PHASE != null ? chosenWORKPACK.GUID_DPHASE : null;
                    var SelectedAREA = AREACollection.FirstOrDefault(x => x.GUID == chosenWORKPACK.GUID_DAREA);
                    var SelectedDOCTYPE = DOCTYPECollection.FirstOrDefault(x => x.GUID == chosenWORKPACK.GUID_DDOCTYPE);
                    var SelectedDISCIPLINE = DISCIPLINECollection.FirstOrDefault(x => x.GUID == chosenWORKPACK.GUID_DDISCIPLINE);

                    activeBASELINE_ITEM.BASELINE_ITEM.INTERNAL_NUM = BluePrintDataUtils.BASELINEITEM_Generate_InternalNumber(loadPROJECT, MainViewModel.Entities, SelectedAREA, SelectedDISCIPLINE, SelectedDOCTYPE);
                    MainViewModel.UpdateSelectedEntity();
                }
            }
            else if (e.Column.FieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().BASELINE_ITEM) + "." + BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_DOCTYPE))
            {
                DOCTYPE chosenDOCTYPE = DOCTYPECollection.FirstOrDefault(entity => entity.GUID == (Guid)e.Value);
                if (chosenDOCTYPE != null && chosenDOCTYPE.GUID_DDEPARTMENT != null)
                {
                    activeBASELINE_ITEM.BASELINE_ITEM.GUID_DEPARTMENT = chosenDOCTYPE.DEPARTMENT.GUID;
                    MainViewModel.UpdateSelectedEntity();
                }
            }
        }
        #endregion

        #region View Commands
        public bool CanDuplicate()
        {
            if (MainViewModel == null || MainViewModel.SelectedEntities.Count == 0)
                return false;

            return true;
        }

        public void Duplicate()
        {
            if (!isProcessingMultipleDuplicates)
                MainViewModel.EntitiesUndoRedoManager.PauseActionId();

            foreach(BASELINE_ITEMProjection selectedEntity in MainViewModel.SelectedEntities)
            {
                BASELINE_ITEMProjection newProjection = new BASELINE_ITEMProjection();
                DataUtils.ShallowCopy(newProjection.BASELINE_ITEM, selectedEntity.BASELINE_ITEM);
                newProjection.BASELINE_ITEM.GUID = Guid.Empty;
                newProjection.BASELINE_ITEM.GUID_ORIGINAL = Guid.Empty;
                AREA selectedAREA = AREACollection.FirstOrDefault(x => x.GUID == newProjection.BASELINE_ITEM.GUID_AREA);
                DISCIPLINE selectedDISCIPLINE = DISCIPLINECollection.FirstOrDefault(x => x.GUID == newProjection.BASELINE_ITEM.GUID_DISCIPLINE);
                DOCTYPE selectedDOCTYPE = DOCTYPECollection.FirstOrDefault(x => x.GUID == newProjection.BASELINE_ITEM.GUID_DOCTYPE);
                newProjection.BASELINE_ITEM.INTERNAL_NUM = BluePrintDataUtils.BASELINEITEM_Generate_InternalNumber(loadPROJECT, MainViewModel.Entities, selectedAREA, selectedDISCIPLINE, selectedDOCTYPE, newProjection.GUID);
                MainViewModel.EntitiesUndoRedoManager.AddUndo(newProjection, null, null, null, EntityMessageType.Added);
                MainViewModel.Save(newProjection);
            }

            if (!isProcessingMultipleDuplicates)
                MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
        }

        public bool CanDuplicateMultiple(BarEditItem barEdit)
        {
            if (MainViewModel == null || MainViewModel.SelectedEntities.Count == 0)
                return false;

            return true;
        }

        bool isProcessingMultipleDuplicates;
        /// <summary>
        /// Paste clipboard data multiple times
        /// </summary>
        public void DuplicateMultiple(BarEditItem barEdit)
        {
            MainViewModel.EntitiesUndoRedoManager.PauseActionId();
            isProcessingMultipleDuplicates = true;
            int timesToDuplicate = 0;
            if(Int32.TryParse(barEdit.EditValue.ToString(), out timesToDuplicate))
            {
                for(int i=0;i < timesToDuplicate;i++)
                {
                    Duplicate();
                }
            }
            isProcessingMultipleDuplicates = false;
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
            var info = GridPopupMenuBase.GetGridMenuInfo((DependencyObject)button) as GridMenuInfo;

            string departmentFieldName = BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().BASELINE_ITEM) + "." + BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_DEPARTMENT);
            string disciplineFieldName = BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().BASELINE_ITEM) + "." + BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_DISCIPLINE);
            string docTypeFieldName = BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().BASELINE_ITEM) + "." + BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_DOCTYPE);
            string areaFieldName = BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().BASELINE_ITEM) + "." + BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_AREA);
            string workpackFieldName = BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().BASELINE_ITEM) + "." + BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_WORKPACK);
            string internalNumberFieldName = BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().BASELINE_ITEM) + "." + BindableBase.GetPropertyName(() => new BASELINE_ITEM().INTERNAL_NUM);

            List<BASELINE_ITEMProjection> entitiesToSave = new List<BASELINE_ITEMProjection>();
            if (info.Column.FieldName == internalNumberFieldName)
            {
                foreach(BASELINE_ITEMProjection entity in MainViewModel.SelectedEntities)
                {
                    entity.BASELINE_ITEM.INTERNAL_NUM = string.Empty;
                }
            }

            foreach(BASELINE_ITEMProjection entity in MainViewModel.SelectedEntities)
            {
                WORKPACK entityWORKPACK = WORKPACKCollection.FirstOrDefault(x => x.GUID == entity.BASELINE_ITEM.GUID_WORKPACK);
                if (info.Column.FieldName == internalNumberFieldName)
                {
                    string internalNum = BluePrintDataUtils.BASELINEITEM_Generate_InternalNumber(loadPROJECT, MainViewModel.Entities, entity.BASELINE_ITEM.AREA, entity.BASELINE_ITEM.DISCIPLINE, entity.BASELINE_ITEM.DOCTYPE, entity.GUID);
                    MainViewModel.SetNestedValueWithUndo(entity, info.Column.FieldName, internalNum);
                    entitiesToSave.Add(entity);
                }
                else if(info.Column.FieldName == departmentFieldName || info.Column.FieldName == disciplineFieldName || info.Column.FieldName == docTypeFieldName || info.Column.FieldName == areaFieldName)
                {
                    if(entityWORKPACK == null)
                        continue;

                    if (info.Column.FieldName == departmentFieldName)
                        MainViewModel.SetNestedValueWithUndo(entity, info.Column.FieldName, entityWORKPACK.GUID_DDEPARTMENT);
                    else if (info.Column.FieldName == disciplineFieldName)
                        MainViewModel.SetNestedValueWithUndo(entity, info.Column.FieldName, entityWORKPACK.GUID_DDISCIPLINE);
                    else if (info.Column.FieldName == docTypeFieldName)
                        MainViewModel.SetNestedValueWithUndo(entity, info.Column.FieldName, entityWORKPACK.GUID_DDOCTYPE);
                    else if (info.Column.FieldName == areaFieldName)
                        MainViewModel.SetNestedValueWithUndo(entity, info.Column.FieldName, entityWORKPACK.GUID_DAREA);

                    entitiesToSave.Add(entity);
                }
                else if(info.Column.FieldName == workpackFieldName)
                {
                    if (entity.BASELINE_ITEM.GUID_DISCIPLINE == Guid.Empty || entity.BASELINE_ITEM.GUID_DEPARTMENT == Guid.Empty ||
                       entity.BASELINE_ITEM.GUID_DOCTYPE == Guid.Empty || entity.BASELINE_ITEM.GUID_AREA == Guid.Empty)
                        continue;

                    WORKPACK findWORKPACK = WORKPACKCollection.FirstOrDefault(x => x.GUID_DAREA == entity.BASELINE_ITEM.GUID_AREA && x.GUID_DDOCTYPE == entity.BASELINE_ITEM.GUID_DOCTYPE && x.GUID_DDISCIPLINE == entity.BASELINE_ITEM.GUID_DISCIPLINE);
                    if (findWORKPACK == null)
                    {
                        WORKPACK newWORKPACK = new WORKPACK();
                        newWORKPACK.GUID_PROJECT = loadPROJECT.GUID;
                        if (entity.BASELINE_ITEM.GUID_AREA != null)
                            newWORKPACK.GUID_DAREA = (Guid)entity.BASELINE_ITEM.GUID_AREA;
                        if (entity.BASELINE_ITEM.GUID_PHASE != null)
                            newWORKPACK.GUID_DPHASE = entity.BASELINE_ITEM.GUID_PHASE;
                        if (entity.BASELINE_ITEM.GUID_DISCIPLINE != null)
                            newWORKPACK.GUID_DDISCIPLINE = (Guid)entity.BASELINE_ITEM.GUID_DISCIPLINE;
                        if (entity.BASELINE_ITEM.GUID_DEPARTMENT != null)
                            newWORKPACK.GUID_DDEPARTMENT = (Guid)entity.BASELINE_ITEM.GUID_DEPARTMENT;
                        if (entity.BASELINE_ITEM.GUID_DOCTYPE != null)
                            newWORKPACK.GUID_DDOCTYPE = (Guid)entity.BASELINE_ITEM.GUID_DOCTYPE;

                        newWORKPACK.INTERNAL_NAME1 = BluePrintDataUtils.WORKPACK_Generate_InternalNumber1(loadPROJECT, newWORKPACK, WORKPACKCollection, AREACollection, DISCIPLINECollection, DOCTYPECollection);
                        newWORKPACK.INTERNAL_NAME2 = BluePrintDataUtils.WORKPACK_Generate_InternalNumber2(loadPROJECT, newWORKPACK, WORKPACKCollection, AREACollection, DISCIPLINECollection, PHASECollection);

                        if (newWORKPACK.INTERNAL_NAME1 == string.Empty && newWORKPACK.INTERNAL_NAME2 == string.Empty)
                            return;

                        newWORKPACK.STARTDATE = DateTime.Now;
                        newWORKPACK.ENDDATE = BluePrintDataUtils.WORKPACK_Calculate_EndDate((DateTime)newWORKPACK.STARTDATE, loadPROJECT);
                        DateTime reviewStartDate = (DateTime)newWORKPACK.STARTDATE;
                        DateTime reviewEndDate = (DateTime)newWORKPACK.ENDDATE;
                        BluePrintDataUtils.WORKPACK_Calculate_ReviewPeriod(ref reviewStartDate, ref reviewEndDate, loadPROJECT, false);
                        newWORKPACK.REVIEWSTARTDATE = reviewStartDate;
                        newWORKPACK.REVIEWENDDATE = reviewEndDate;
                        newWORKPACK.AUTOGENERATED = true;
                        newWORKPACK.TYPE = WorkpackType.Design;
                        ((CollectionViewModel<WORKPACK, WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<WORKPACK>()).Save(newWORKPACK);

                        MainViewModel.SetNestedValueWithUndo(entity, info.Column.FieldName, newWORKPACK.GUID);
                    }
                    else
                        MainViewModel.SetNestedValueWithUndo(entity, info.Column.FieldName, findWORKPACK.GUID);

                    entitiesToSave.Add(entity);
                }
            }

            MainViewModel.BulkSave(entitiesToSave);
            MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
        }
        #endregion

        #region View Properties
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get
            {
                return "BASELINE_ITEMSViewModelWrapper";
            }
        }

        /// <summary>
        /// The workpack internal name to be used
        /// </summary>
        public string WORKPACKDisplayMember
        {
            get
            {
                if (loadPROJECT == null || loadPROJECT.USELEGACYWORKPACK)
                    return BindableBase.GetPropertyName(() => new WORKPACK().INTERNAL_NAME1);
                else
                    return BindableBase.GetPropertyName(() => new WORKPACK().INTERNAL_NAME2);
            }
        }

        public void ShowWORKPACKColumns()
        {
            if(ShowWORKPACKInternalName1 == null || ShowWORKPACKInternalName2 == null)
                return;

            if (loadPROJECT == null || loadPROJECT.USELEGACYWORKPACK)
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
            REPORTDesigner reportDesigner = new REPORTDesigner(loadPROJECT, (CollectionViewModel<PROJECT_REPORT, PROJECT_REPORT, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<PROJECT_REPORT>(), ReportType.Baseline_Report);
            if (reportDesigner.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                reportDesigner.Dispose();
            else
                reportDesigner.Dispose();
        }

        public void ViewReport()
        {
            XtraReportBASELINE_ITEMS baselineReport = new XtraReportBASELINE_ITEMS();
            PROJECT_REPORT dbProjectReport = loaderCollection.GetObject<PROJECT_REPORT>();
            if (dbProjectReport != null)
            {
                string reportString = dbProjectReport.REPORT.ToString();
                using (StreamWriter sw = new StreamWriter(new MemoryStream()))
                {
                    sw.Write(reportString);
                    sw.Flush();
                    baselineReport.LoadLayout(sw.BaseStream);
                }
            }

            //make sure disciplines are all populated
            PopulateNavigationalProperties();
            baselineReport.AssignProperties(loadPROJECT, loadBASELINE, MainViewModel.Entities);
            DocumentPreviewWindow previewWindow = new DocumentPreviewWindow();
            previewWindow.PreviewControl.DocumentSource = baselineReport;
            previewWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            previewWindow.WindowState = WindowState.Maximized;
            baselineReport.RequestParameters = false;
            baselineReport.CreateDocument(true);
            previewWindow.ShowDialog();
        }


        void PopulateNavigationalProperties()
        {
            foreach(BASELINE_ITEMProjection projection in MainViewModel.Entities)
            {
                if(projection.BASELINE_ITEM.GUID_DISCIPLINE != null && projection.BASELINE_ITEM.DISCIPLINE == null)
                {
                    projection.BASELINE_ITEM.DISCIPLINE = DISCIPLINECollection.FirstOrDefault(x => x.GUID == projection.BASELINE_ITEM.GUID_DISCIPLINE);
                }

                if(projection.BASELINE_ITEM.GUID_AREA != null && projection.BASELINE_ITEM.AREA == null)
                {
                    projection.BASELINE_ITEM.AREA = AREACollection.FirstOrDefault(x => x.GUID == projection.BASELINE_ITEM.GUID_AREA);
                }
            }
        }
        #endregion
    }
}
