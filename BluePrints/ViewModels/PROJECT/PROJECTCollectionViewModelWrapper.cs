using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Document;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Helpers;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class PROJECTCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of PROJECT_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static PROJECTCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new PROJECTCollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the PROJECTViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected PROJECTCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void resolveParameters(object parameter)
        {
        }

        public override void OnLoaded()
        {
            if (AppNotificationService == null || GlobalVariables.IsProjectCollectionViewNotificationShown)
            {
                base.OnLoaded();
                return;
            }

            INotification notification = AppNotificationService.CreatePredefinedNotification("Update: go to rate, baseline, progress and estimate from right clicking or top menu", null, null, null);
            GlobalVariables.IsProjectViewNotificationShown = true;
            notification.ShowAsync();

            base.OnLoaded();
        }

        protected override void initializeEntitiesLoadersDescription()
        {
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINES, BASELINEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.ESTIMATES, ESTIMATEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.SUBJOBS, SUBJOBProjectionFunc);
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS, BASELINE_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DOCTYPES);
            loaderCollection.AddLoaderDescription<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.USERS);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.PROJECTS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> specifyMainViewModelProjection()
        {
            return query => query.OrderBy(x => x.NUMBER);
        }

        /// <summary>
        /// BASELINE is used for write only so just load a single entry for repository to be initialized
        /// </summary>
        private Func<IRepositoryQuery<BASELINE>, IQueryable<BASELINE>> BASELINEProjectionFunc()
        {
            return query => query.Take(1);
        }

        /// <summary>
        /// ESTIMATE is used for write only so just load a single entry for repository to be initialized
        /// </summary>
        private Func<IRepositoryQuery<ESTIMATE>, IQueryable<ESTIMATE>> ESTIMATEProjectionFunc()
        {
            return query => query.Take(1);
        }

        /// <summary>
        /// PROGRESS is used for write only so just load a single entry for repository to be initialized
        /// </summary>
        private Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> PROGRESSProjectionFunc()
        {
            return query => query.Take(1);
        }

        /// <summary>
        /// SUBJOB is used for write only so just load a single entry for repository to be initialized
        /// </summary>
        private Func<IRepositoryQuery<SUBJOB>, IQueryable<SUBJOB>> SUBJOBProjectionFunc()
        {
            return query => query.Take(1);
        }

        /// <summary>
        /// AREA is used for write only so just load a single entry for repository to be initialized
        /// </summary>
        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Take(1);
        }

        /// <summary>
        /// BASELINE_ITEM is used for write only so just load a single entry for repository to be initialized
        /// </summary>
        private Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEM>> BASELINE_ITEMProjectionFunc()
        {
            return query => query.Take(1);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<PROJECT> entities)
        {
            MainViewModel.ApplyEntityPropertiesToProjectionCallBack = PostSave;
            MainViewModel.AdditionalValidateCellCallBack = AdditionalCellValidation;
            MainViewModel.CanFillDownCallBack = CanFillDownCallBack;
            MainViewModel.OnBeforeEntitiesDeleteCallBack = onBeforeEntitiesDeleted;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }
        #region Collection Call Backs
        public override void OnAfterAuxiliaryEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
            if(messageType == EntityMessageType.Added)
            {
                PROJECT findPROJECT = MainViewModel.Entities.FirstOrDefault(x => x.GUID == (Guid)key);
                if(findPROJECT != null)
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    BluePrintsContextHelper.AsyncRefreshDeliverablesDataPointsByProject(findPROJECT.NUMBER);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }

            base.OnAfterAuxiliaryEntitiesChanged(key, changedType, messageType, sender, isBulkRefresh);
        }

        private void onBeforeEntitiesDeleted(IEnumerable<PROJECT> deletedPROJECTs)
        {
            foreach(PROJECT project in deletedPROJECTs)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                BluePrintsContextHelper.AsyncRefreshDeliverablesDataPointsByProject(project.NUMBER);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        private void PostSave(Guid key, PROJECT projectionEntity, PROJECT entity, bool isNewEntity)
        {
            bool? isEntityNew = DataUtils.IsNewEntity<PROJECT>(projectionEntity);

            //only way to determine whether current entity is new to avoid creating multiple 
            if (isEntityNew != null && ((bool)isEntityNew))
            {
                BASELINE newBASELINE = new BASELINE();
                newBASELINE.GUID_PROJECT = entity.GUID;
                newBASELINE.NAME = entity.NUMBER + "_001";
                newBASELINE.REVISION = "A";
                newBASELINE.STATUS = BaselineStatus.Live;
                BASELINEViewModel.Save(newBASELINE);

                ESTIMATE newESTIMATE_DIRECT = new ESTIMATE();
                newESTIMATE_DIRECT.GUID_PROJECT = entity.GUID;
                newESTIMATE_DIRECT.NAME = entity.NUMBER + "_001";
                newESTIMATE_DIRECT.REVISION = "A";
                newESTIMATE_DIRECT.STATUS = BaselineStatus.Working;
                ESTIMATEViewModel.Save(newESTIMATE_DIRECT);

                PROGRESS newDesignPROGRESS = new PROGRESS();
                newDesignPROGRESS.GUID_PROJECT = entity.GUID;
                newDesignPROGRESS.NAME = entity.NUMBER + "WEEKLY_001";
                newDesignPROGRESS.PROGRESS_START = DateTime.Now;
                newDesignPROGRESS.DATA_DATE = CommonMethods.StartOfWeek(DateTime.Now, DayOfWeek.Sunday);
                newDesignPROGRESS.INTERVAL_COUNT = 1;
                newDesignPROGRESS.INTERVAL_TYPE = ProgressIntervalType.Weekly;
                newDesignPROGRESS.STATUS = ProgressStatus.Live;
                newDesignPROGRESS.TYPE = ProgressType.Design;
                PROGRESSViewModel.Save(newDesignPROGRESS);

                PROGRESS newConstructionPROGRESS = new PROGRESS();
                newConstructionPROGRESS.GUID_PROJECT = entity.GUID;
                newConstructionPROGRESS.NAME = entity.NUMBER + "DAILY_001";
                newConstructionPROGRESS.PROGRESS_START = DateTime.Now;
                newConstructionPROGRESS.DATA_DATE = CommonMethods.StartOfWeek(DateTime.Now, DayOfWeek.Sunday);
                newConstructionPROGRESS.INTERVAL_COUNT = 1;
                newConstructionPROGRESS.INTERVAL_TYPE = ProgressIntervalType.Daily;
                newConstructionPROGRESS.STATUS = ProgressStatus.Live;
                newConstructionPROGRESS.TYPE = ProgressType.Construct;
                PROGRESSViewModel.Save(newConstructionPROGRESS);

                var newAREA = new AREA();
                newAREA.GUID_PROJECT = entity.GUID;
                newAREA.INTERNAL_NUM = "000";
                newAREA.CLIENT_NUM = "000";
                newAREA.TITLE = "General";
                AREAViewModel.Save(newAREA);

                DEPARTMENT defaultDepartment = DEPARTMENTViewModel.Entities.FirstOrDefault(x => x.NAME == BluePrintsResources.Default_New_Project_Department);
                DISCIPLINE defaultDiscipline = DISCIPLINEViewModel.Entities.FirstOrDefault(x => x.NAME == BluePrintsResources.Default_New_Project_Discipline);
                DOCTYPE defaultDocType = DOCTYPEViewModel.Entities.FirstOrDefault(x => x.NAME == BluePrintsResources.Default_New_Project_DocType);

                if(defaultDepartment != null && defaultDiscipline != null)
                {
                    SUBJOB newSUBJOB = new SUBJOB();
                    newSUBJOB.GUID_PROJECT = entity.GUID;
                    newSUBJOB.INTERNAL_NAME1 = entity.NUMBER;
                    newSUBJOB.STARTDATE = CommonMethods.StartOfWeek(DateTime.Now, DayOfWeek.Sunday);
                    newSUBJOB.ENDDATE = ((DateTime)newSUBJOB.STARTDATE).AddDays(7).AddSeconds(-1);
                    newSUBJOB.REVIEWSTARTDATE = (DateTime)newSUBJOB.STARTDATE; //effectively nullifies review date
                    newSUBJOB.REVIEWENDDATE = (DateTime)newSUBJOB.STARTDATE; //effectively nullifies review date
                    newSUBJOB.GUID_DAREA = newAREA.GUID;
                    SUBJOBViewModel.Save(newSUBJOB);

                    BASELINE_ITEM newBASELINE_ITEM = new BASELINE_ITEM();
                    newBASELINE_ITEM.GUID_BASELINE = newBASELINE.GUID;
                    newBASELINE_ITEM.GUID_SUBJOB = newSUBJOB.GUID;
                    newBASELINE_ITEM.GUID_DEPARTMENT = defaultDepartment.GUID;
                    newBASELINE_ITEM.GUID_DISCIPLINE = defaultDiscipline.GUID;
                    newBASELINE_ITEM.GUID_DOCTYPE = defaultDocType.GUID;
                    newBASELINE_ITEM.INTERNAL_NUM = entity.NUMBER + "-000-REP-GE-001";
                    BASELINE_ITEMViewModel.Save(newBASELINE_ITEM);

                }
            }
        }

        #endregion

        #endregion

        #region View Behavior
        protected IOpenFileDialogService OpenFileDialogService
        {
            get { return this.GetService<IOpenFileDialogService>(); }
        }

        public bool CanFillDownCallBack(IEnumerable<PROJECT> selectedEntities, GridMenuInfo info)
        {
            if (info.Column.FieldName == BindableBase.GetPropertyName(() => new PROJECT().DOC_KICKOFF) ||
                info.Column.FieldName == BindableBase.GetPropertyName(() => new PROJECT().DOC_CLOSEOUT) ||
                info.Column.FieldName == BindableBase.GetPropertyName(() => new PROJECT().DOC_SIDREPORT))
                return false;

            return true;
        }

        public override void UnifiedCellValueChanging(string field_name, object old_value, object new_value, PROJECT projection, bool isNew)
        {
            if (field_name == BindableBase.GetPropertyName(() => new PROJECT().DOC_KICKOFF) ||
                      field_name == BindableBase.GetPropertyName(() => new PROJECT().DOC_CLOSEOUT) ||
                      field_name == BindableBase.GetPropertyName(() => new PROJECT().DOC_SIDREPORT))
            {
                MainViewModel.EntitiesUndoRedoManager.PauseActionId(); //Unpaused in existingRowAddUndoAndSave
                if (new_value == null || ((ProjectDocumentStatus)new_value) != ProjectDocumentStatus.Yes)
                {
                    if (field_name == BindableBase.GetPropertyName(() => new PROJECT().DOC_KICKOFF))
                    {
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new PROJECT().DOC_KICKOFF_PATH), projection.DOC_KICKOFF_PATH, null, EntityMessageType.Changed);
                        projection.DOC_KICKOFF_PATH = null;
                    }
                    else if (field_name == BindableBase.GetPropertyName(() => new PROJECT().DOC_CLOSEOUT))
                    {
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new PROJECT().DOC_CLOSEOUT_PATH), projection.DOC_CLOSEOUT_PATH, null, EntityMessageType.Changed);
                        projection.DOC_CLOSEOUT_PATH = null;
                    }
                    else
                    {
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new PROJECT().DOC_SIDREPORT_PATH), projection.DOC_SIDREPORT_PATH, null, EntityMessageType.Changed);
                        projection.DOC_SIDREPORT_PATH = null;
                    }
                }
                else
                {
                    OpenFileDialogService.Filter = "PDF (*.PDF)|*.PDF";
                    bool DialogResult;

                    DialogResult = OpenFileDialogService.ShowDialog();
                    if (DialogResult)
                    {
                        string fullPath = OpenFileDialogService.File.GetFullName();
                        if (field_name == BindableBase.GetPropertyName(() => new PROJECT().DOC_KICKOFF))
                        {
                            MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new PROJECT().DOC_KICKOFF_PATH), null, fullPath, EntityMessageType.Changed);
                            projection.DOC_KICKOFF_PATH = fullPath;
                        }
                        else if (field_name == BindableBase.GetPropertyName(() => new PROJECT().DOC_CLOSEOUT))
                        {
                            MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new PROJECT().DOC_CLOSEOUT_PATH), null, fullPath, EntityMessageType.Changed);
                            projection.DOC_CLOSEOUT_PATH = fullPath;
                        }
                        else
                        {
                            MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new PROJECT().DOC_SIDREPORT), null, fullPath, EntityMessageType.Changed);
                            projection.DOC_SIDREPORT_PATH = fullPath;
                        }
                    }
                }
            }

            if (field_name == BindableBase.GetPropertyName(() => new PROJECT().STATUS))
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                BluePrintsContextHelper.AsyncRefreshDeliverablesDataPointsByProject(projection.NUMBER);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }

            base.UnifiedCellValueChanging(field_name, old_value, new_value, projection, isNew);
        }

        private void AdditionalCellValidation(GridCellValidationEventArgs e)
        {
            PROJECT activePROJECT = (PROJECT)e.Row;
            string missingPathErrorString = "Path not selected";
            bool isError = false;

            if (e.Column.FieldName == BindableBase.GetPropertyName(() => new PROJECT().DOC_KICKOFF))
            {
                if (e.Value != null &&  ((ProjectDocumentStatus)e.Value) == ProjectDocumentStatus.Yes && activePROJECT.DOC_KICKOFF_PATH == null)
                {
                    isError = true;
                }
            }
            else if (e.Column.FieldName == BindableBase.GetPropertyName(() => new PROJECT().DOC_CLOSEOUT))
            {
                if (e.Value != null && ((ProjectDocumentStatus)e.Value) == ProjectDocumentStatus.Yes && activePROJECT.DOC_CLOSEOUT_PATH == null)
                {
                    isError = true;
                }
            }
            else if (e.Column.FieldName == BindableBase.GetPropertyName(() => new PROJECT().DOC_SIDREPORT))
            {
                if (e.Value != null && ((ProjectDocumentStatus)e.Value) == ProjectDocumentStatus.Yes && activePROJECT.DOC_SIDREPORT_PATH == null)
                {
                    isError = true;
                }
            }

            if (isError)
            {
                e.IsValid = false;
                e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Critical;
                e.ErrorContent = missingPathErrorString;
            }
        }
        #endregion

        #region View Properties
        public IEnumerable<USER> MANAGERCollection
        {
            get
            {
                var collection = GetEntities<USER>();
                if (collection != null)
                    collection = collection.Where(x => x.ROLE != null && x.ROLE.ISMANAGER).OrderBy(x => x.NAME);
                return collection;
            }
        }

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

        public CollectionViewModel<ESTIMATE, ESTIMATE, Guid, IBluePrintsEntitiesUnitOfWork> ESTIMATEViewModel
        {
            get
            {
                if (loaderCollection == null)
                    return null;

                return
                    (CollectionViewModel<ESTIMATE, ESTIMATE, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<ESTIMATE>();
            }
        }

        public CollectionViewModel<PROGRESS, PROGRESS, Guid, IBluePrintsEntitiesUnitOfWork> PROGRESSViewModel
        {
            get
            {
                if (loaderCollection == null)
                    return null;

                return
                    (CollectionViewModel<PROGRESS, PROGRESS, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<PROGRESS>();
            }
        }


        public CollectionViewModel<SUBJOB, SUBJOB, Guid, IBluePrintsEntitiesUnitOfWork> SUBJOBViewModel
        {
            get
            {
                if (loaderCollection == null)
                    return null;

                return
                    (CollectionViewModel<SUBJOB, SUBJOB, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<SUBJOB>();
            }
        }

        public CollectionViewModel<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork> DISCIPLINEViewModel
        {
            get
            {
                if (loaderCollection == null)
                    return null;

                return
                    (CollectionViewModel<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<DISCIPLINE>();
            }
        }

        public CollectionViewModel<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork> DEPARTMENTViewModel
        {
            get
            {
                if (loaderCollection == null)
                    return null;

                return
                    (CollectionViewModel<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<DEPARTMENT>();
            }
        }

        public CollectionViewModel<AREA, AREA, Guid, IBluePrintsEntitiesUnitOfWork> AREAViewModel
        {
            get
            {
                if (loaderCollection == null)
                    return null;

                return
                    (CollectionViewModel<AREA, AREA, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<AREA>();
            }
        }


        public CollectionViewModel<BASELINE_ITEM, BASELINE_ITEM, Guid, IBluePrintsEntitiesUnitOfWork> BASELINE_ITEMViewModel
        {
            get
            {
                if (loaderCollection == null)
                    return null;

                return
                    (CollectionViewModel<BASELINE_ITEM, BASELINE_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<BASELINE_ITEM>();
            }
        }

        public CollectionViewModel<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork> DOCTYPEViewModel
        {
            get
            {
                if (loaderCollection == null)
                    return null;

                return
                    (CollectionViewModel<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<DOCTYPE>();
            }
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "PROJECTCollectionViewModelWrapper"; }
        }

        public void EditArea()
        {
            if (DisplaySelectedEntity == null)
                return;

            DocumentInfo DocumentInfo = new DocumentInfo("View_ProjectAreas" + DisplaySelectedEntity.GUID.ToString(),
                new EntitiesParameter<PROJECT>(DisplaySelectedEntity),
                    "AREACollectionView",
                    "[" + DisplaySelectedEntity.NUMBER + "] Areas");

            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }

        public void EditRate()
        {
            if (DisplaySelectedEntity == null)
                return;

            DocumentInfo DocumentInfo = new DocumentInfo("View_ProjectRates" + DisplaySelectedEntity.GUID.ToString(),
                new EntitiesParameter<PROJECT>(DisplaySelectedEntity),
                    "RATECollectionView",
                    "[" + DisplaySelectedEntity.NUMBER + "] Rates");

            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }

        public void EditBaseline()
        {
            if (DisplaySelectedEntity == null)
                return;

            DocumentInfo DocumentInfo = new DocumentInfo("View_ProjectBaselines" + DisplaySelectedEntity.GUID.ToString(),
                new EntitiesParameter<PROJECT>(DisplaySelectedEntity),
                    "BASELINECollectionView",
                    "[" + DisplaySelectedEntity.NUMBER + "] Baselines");

            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }

        public void EditEstimate()
        {
            if (DisplaySelectedEntity == null)
                return;

            DocumentInfo DocumentInfo = new DocumentInfo("View_ProjectEstimates" + DisplaySelectedEntity.GUID.ToString(),
                new EntitiesParameter<PROJECT>(DisplaySelectedEntity),
                    "ESTIMATECollectionView",
                    "[" + DisplaySelectedEntity.NUMBER + "] Estimates");

            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }

        public void EditProgress()
        {
            if (DisplaySelectedEntity == null)
                return;

            DocumentInfo DocumentInfo = new DocumentInfo("View_ProjectEstimates" + DisplaySelectedEntity.GUID.ToString(),
                new EntitiesParameter<PROJECT>(DisplaySelectedEntity),
                    "PROGRESSCollectionView",
                    "[" + DisplaySelectedEntity.NUMBER + "] Progresses");

            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }
        #endregion

        #region ISupportCustomDocumentTypeAndParameter

        public bool CanEdit()
        {
            if (DisplaySelectedEntity == null)
                return false;

            return true;
        }

        protected IDocumentManagerService DocumentManagerService
        {
            get { return this.GetService<IDocumentManagerService>(); }
        }

        public void Edit()
        {
            if (DisplaySelectedEntity == null)
                return;

            DocumentInfo DocumentInfo = new DocumentInfo(DisplaySelectedEntity.GUID.ToString(), 
                new EntitiesParameter<PROJECT>(DisplaySelectedEntity), 
                "PROJECTView", 
                "[" + DisplaySelectedEntity.NUMBER + "]");

            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }
        #endregion
    }
}