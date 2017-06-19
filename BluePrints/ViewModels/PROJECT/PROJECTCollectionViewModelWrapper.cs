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

        protected override void InitializeParameters(object parameter)
        {
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINES, BASELINEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.WORKPACKS, WORKPACKProjectionFunc);
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS, BASELINE_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DOCTYPES);
            loaderCollection.AddLoaderDescription<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.USERS);
            InvokeEntitiesLoaderDescriptionLoading();
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.PROJECTS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> ConstructMainViewModelProjection()
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
        /// PROGRESS is used for write only so just load a single entry for repository to be initialized
        /// </summary>
        private Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> PROGRESSProjectionFunc()
        {
            return query => query.Take(1);
        }

        /// <summary>
        /// WORKPACK is used for write only so just load a single entry for repository to be initialized
        /// </summary>
        private Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK>> WORKPACKProjectionFunc()
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
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }
        #region Collection Call Backs

        private void PostSave(Guid key, PROJECT projectionEntity, PROJECT entity, bool isNewEntity)
        {
            if (isNewEntity)
            {
                var newBASELINE = new BASELINE();
                newBASELINE.GUID_PROJECT = entity.GUID;
                newBASELINE.NAME = entity.NUMBER + "_001";
                newBASELINE.REVISION = "A";
                newBASELINE.STATUS = BaselineStatus.Live;
                BASELINEViewModel.Save(newBASELINE);

                var newPROGRESS = new PROGRESS();
                newPROGRESS.GUID_PROJECT = entity.GUID;
                newPROGRESS.NAME = entity.NUMBER + "WEEKLY_001";
                newPROGRESS.PROGRESS_START = DateTime.Now;
                newPROGRESS.DATA_DATE = CommonMethods.StartOfWeek(DateTime.Now, DayOfWeek.Sunday);
                newPROGRESS.INTERVAL_COUNT = 1;
                newPROGRESS.INTERVAL_TYPE = ProgressIntervalType.Weekly;
                newPROGRESS.STATUS = ProgressStatus.Live;
                PROGRESSViewModel.Save(newPROGRESS);

                var newAREA = new AREA();
                newAREA.GUID_PROJECT = entity.GUID;
                newAREA.INTERNAL_NUM = "000";
                newAREA.CLIENT_NUM = "000";
                newAREA.TITLE = "General";
                AREAViewModel.Save(newAREA);

                DEPARTMENT defaultDepartment = DEPARTMENTViewModel.Entities.FirstOrDefault(x => x.NAME == BluePrintsResources.NewProject_DefaultDepartment);
                DISCIPLINE defaultDiscipline = DISCIPLINEViewModel.Entities.FirstOrDefault(x => x.NAME == BluePrintsResources.NewProject_DefaultDiscipline);
                DOCTYPE defaultDocType = DOCTYPEViewModel.Entities.FirstOrDefault(x => x.NAME == BluePrintsResources.NewProject_DefaultDocType);

                if(defaultDepartment != null && defaultDiscipline != null)
                {
                    var newWORKPACK = new WORKPACK();
                    newWORKPACK.GUID_PROJECT = entity.GUID;
                    newWORKPACK.INTERNAL_NAME1 = entity.NUMBER;
                    newWORKPACK.STARTDATE = CommonMethods.StartOfWeek(DateTime.Now, DayOfWeek.Sunday);
                    newWORKPACK.ENDDATE = ((DateTime)newWORKPACK.STARTDATE).AddDays(7).AddSeconds(-1);
                    newWORKPACK.REVIEWSTARTDATE = (DateTime)newWORKPACK.STARTDATE; //effectively nullifies review date
                    newWORKPACK.REVIEWENDDATE = (DateTime)newWORKPACK.STARTDATE; //effectively nullifies review date
                    newWORKPACK.GUID_DAREA = newAREA.GUID;
                    newWORKPACK.GUID_DDEPARTMENT = defaultDepartment.GUID;
                    newWORKPACK.GUID_DDISCIPLINE = defaultDiscipline.GUID;
                    newWORKPACK.GUID_DDOCTYPE = defaultDocType.GUID;
                    WORKPACKViewModel.Save(newWORKPACK);

                    var newBASELINE_ITEM = new BASELINE_ITEM();
                    newBASELINE_ITEM.GUID_BASELINE = newBASELINE.GUID;
                    newBASELINE_ITEM.GUID_WORKPACK = newWORKPACK.GUID;
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

        protected override void CellValueAnyRowChanging(CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == BindableBase.GetPropertyName(() => new PROJECT().DOC_KICKOFF) ||
                   e.Column.FieldName == BindableBase.GetPropertyName(() => new PROJECT().DOC_CLOSEOUT) ||
                   e.Column.FieldName == BindableBase.GetPropertyName(() => new PROJECT().DOC_SIDREPORT))
            {
                MainViewModel.EntitiesUndoRedoManager.PauseActionId(); //Unpaused in existingRowAddUndoAndSave
                PROJECT activePROJECT = (PROJECT)e.Row;
                if (e.Value == null || ((ProjectDocumentStatus)e.Value) != ProjectDocumentStatus.Yes)
                {
                    if (e.Column.FieldName == BindableBase.GetPropertyName(() => new PROJECT().DOC_KICKOFF))
                    {
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(activePROJECT, BindableBase.GetPropertyName(() => new PROJECT().DOC_KICKOFF_PATH), activePROJECT.DOC_KICKOFF_PATH, null, EntityMessageType.Changed);
                        activePROJECT.DOC_KICKOFF_PATH = null;
                    }
                    else if (e.Column.FieldName == BindableBase.GetPropertyName(() => new PROJECT().DOC_CLOSEOUT))
                    {
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(activePROJECT, BindableBase.GetPropertyName(() => new PROJECT().DOC_CLOSEOUT_PATH), activePROJECT.DOC_CLOSEOUT_PATH, null, EntityMessageType.Changed);
                        activePROJECT.DOC_CLOSEOUT_PATH = null;
                    }
                    else
                    {
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(activePROJECT, BindableBase.GetPropertyName(() => new PROJECT().DOC_SIDREPORT_PATH), activePROJECT.DOC_SIDREPORT_PATH, null, EntityMessageType.Changed);
                        activePROJECT.DOC_SIDREPORT_PATH = null;
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
                        if (e.Column.FieldName == BindableBase.GetPropertyName(() => new PROJECT().DOC_KICKOFF))
                        {
                            MainViewModel.EntitiesUndoRedoManager.AddUndo(activePROJECT, BindableBase.GetPropertyName(() => new PROJECT().DOC_KICKOFF_PATH), null, fullPath, EntityMessageType.Changed);
                            activePROJECT.DOC_KICKOFF_PATH = fullPath;
                        }
                        else if (e.Column.FieldName == BindableBase.GetPropertyName(() => new PROJECT().DOC_CLOSEOUT))
                        {
                            MainViewModel.EntitiesUndoRedoManager.AddUndo(activePROJECT, BindableBase.GetPropertyName(() => new PROJECT().DOC_CLOSEOUT_PATH), null, fullPath, EntityMessageType.Changed);
                            activePROJECT.DOC_CLOSEOUT_PATH = fullPath;
                        }
                        else
                        {
                            MainViewModel.EntitiesUndoRedoManager.AddUndo(activePROJECT, BindableBase.GetPropertyName(() => new PROJECT().DOC_SIDREPORT), null, fullPath, EntityMessageType.Changed);
                            activePROJECT.DOC_SIDREPORT_PATH = fullPath;
                        }
                    }
                }

                e.Handled = true;
            }

            base.CellValueAnyRowChanging(e);
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


        public CollectionViewModel<WORKPACK, WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork> WORKPACKViewModel
        {
            get
            {
                if (loaderCollection == null)
                    return null;

                return
                    (CollectionViewModel<WORKPACK, WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<WORKPACK>();
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

            DocumentManagerService.ShowExistingEntityDocument(DocumentInfo, this);
        }
        #endregion
    }
}