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
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.WORKPACKS, WORKPACKProjectionFunc);
            loaderCollection.AddLoaderDescription<PHASE, PHASE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.PHASES);
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription<PROJECT_DISCIPLINE, PROJECT_DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.PROJECT_DISCIPLINES);
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
            return query => populate_project_discipline(query.OrderBy(x => x.NUMBER));
        }

        private IQueryable<PROJECT> populate_project_discipline(IQueryable<PROJECT> query)
        {
            List<PROJECT> projects = query.ToList();
            //need to call ToList for tokenComboBoxEditSettings to work
            projects.ForEach(x => populate_disciplines(x));
            return projects.AsQueryable();
        }

        private void populate_disciplines(PROJECT project)
        {
            project.Disciplines = DISCIPLINECollection.Where(discipline => PROJECT_DISCIPLINECollection.Any(pd => pd.GUID_PROJECT == project.GUID && pd.GUID_DISCIPLINE == discipline.GUID)).ToList();
        }

        private void save_project_discipline(PROJECT entity)
        {
            List<PROJECT_DISCIPLINE> remove_project_disciplines = new List<PROJECT_DISCIPLINE>();

            if (entity.Disciplines != null)
            {
                foreach (PROJECT_DISCIPLINE assignment in PROJECT_DISCIPLINECollection.Where(x => x.GUID_PROJECT == entity.GUID))
                {
                    if (!entity.Project_Disciplines.Any(x => x.GUID == assignment.GUID_DISCIPLINE))
                        remove_project_disciplines.Add(assignment);
                }

                PROJECT_DISCIPLINECollectionViewModel.BaseBulkDelete(remove_project_disciplines);
                List<PROJECT_DISCIPLINE> add_project_disciplines = new List<PROJECT_DISCIPLINE>();
                foreach (DISCIPLINE project_discipline in entity.Project_Disciplines)
                {
                    if (!PROJECT_DISCIPLINECollection.Any(x => x.GUID_DISCIPLINE == project_discipline.GUID && x.GUID_PROJECT == entity.GUID))
                        add_project_disciplines.Add(new PROJECT_DISCIPLINE() { GUID_DISCIPLINE = project_discipline.GUID, GUID_PROJECT = entity.GUID });
                }

                PROJECT_DISCIPLINECollectionViewModel.BulkSave(add_project_disciplines);
            }
            else
            {
                foreach (PROJECT_DISCIPLINE assignment in PROJECT_DISCIPLINECollection.Where(x => x.GUID_PROJECT == entity.GUID))
                {
                    remove_project_disciplines.Add(assignment);
                }

                PROJECT_DISCIPLINECollectionViewModel.BaseBulkDelete(remove_project_disciplines);
            }
        }

        private void onAfterEntitySaved(PROJECT entity, PROJECT projection, bool isNewEntity)
        {
            save_project_discipline(entity);
            PostSave(entity, projection, isNewEntity);
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
        /// WORKPACK is used for write only so just load a single entry for repository to be initialized
        /// </summary>
        private Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK>> WORKPACKProjectionFunc()
        {
            return query => query.Take(1);
        }

        /// <summary>
        /// RATE is used for write only so just load a single entry for repository to be initialized
        /// </summary>
        private Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
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
            MainViewModel.OnAfterEntitySavedCallBack = onAfterEntitySaved;
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

        private void PostSave(PROJECT projectionEntity, PROJECT entity, bool isNewEntity)
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
                newDesignPROGRESS.TYPE = PhaseType.Design;
                PROGRESSViewModel.Save(newDesignPROGRESS);

                PROGRESS newConstructionPROGRESS = new PROGRESS();
                newConstructionPROGRESS.GUID_PROJECT = entity.GUID;
                newConstructionPROGRESS.NAME = entity.NUMBER + "DAILY_001";
                newConstructionPROGRESS.PROGRESS_START = DateTime.Now;
                newConstructionPROGRESS.DATA_DATE = CommonMethods.StartOfWeek(DateTime.Now, DayOfWeek.Sunday).AddDays(1).AddSeconds(-1);
                newConstructionPROGRESS.INTERVAL_COUNT = 1;
                newConstructionPROGRESS.INTERVAL_TYPE = ProgressIntervalType.Daily;
                newConstructionPROGRESS.STATUS = ProgressStatus.Live;
                newConstructionPROGRESS.TYPE = PhaseType.Construct;
                PROGRESSViewModel.Save(newConstructionPROGRESS);

                AREA defaultArea = new AREA();
                defaultArea.GUID_PROJECT = entity.GUID;
                defaultArea.INTERNAL_NUM = "000";
                defaultArea.CLIENT_NUM = "000";
                defaultArea.TITLE = "General";
                AREAViewModel.Save(defaultArea);

                PHASE defaultDirectPhase = PHASECollection.FirstOrDefault(x => x.INTERNAL_NUM == "D1");
                DEPARTMENT defaultDepartment = DEPARTMENTViewModel.Entities.FirstOrDefault(x => x.NAME == BluePrintsResources.Default_New_Project_Department);
                DISCIPLINE defaultDiscipline = DISCIPLINEViewModel.Entities.FirstOrDefault(x => x.NAME == BluePrintsResources.Default_New_Project_Discipline);
                DOCTYPE defaultDocType = DOCTYPEViewModel.Entities.FirstOrDefault(x => x.NAME == BluePrintsResources.Default_New_Project_DocType);
                PROJECT defaultRATESProject = MainViewModel.Entities.FirstOrDefault(x => x.NUMBER == "00000");
                if(defaultRATESProject != null)
                {
                    foreach(RATE rate in defaultRATESProject.RATE)
                    {
                        RATE newRATE = new RATE();
                        DataUtils.ShallowCopy(newRATE, rate);
                        newRATE.GUID = Guid.Empty;
                        newRATE.GUID_PROJECT = entity.GUID;
                        RATEViewModel.Save(newRATE);
                    }
                }

                if(defaultDirectPhase != null)
                {
                    SUBJOB newSUBJOB = new SUBJOB();
                    newSUBJOB.GUID_PROJECT = entity.GUID;
                    newSUBJOB.INTERNAL_NAME1 = entity.NUMBER;
                    newSUBJOB.STARTDATE = CommonMethods.StartOfWeek(DateTime.Now, DayOfWeek.Sunday);
                    newSUBJOB.ENDDATE = ((DateTime)newSUBJOB.STARTDATE).AddDays(7).AddSeconds(-1);
                    newSUBJOB.REVIEWSTARTDATE = (DateTime)newSUBJOB.STARTDATE; //effectively nullifies review date
                    newSUBJOB.REVIEWENDDATE = (DateTime)newSUBJOB.STARTDATE; //effectively nullifies review date
                    newSUBJOB.GUID_DAREA = defaultArea.GUID;
                    newSUBJOB.GUID_DPHASE = defaultDirectPhase.GUID;
                    SUBJOBViewModel.Save(newSUBJOB);

                    //if (defaultDepartment != null && defaultDiscipline != null)
                    //{

                    //}

                    SUBJOB defaultDesignSUBJOB = new SUBJOB();
                    defaultDesignSUBJOB.GUID_PROJECT = entity.GUID;
                    defaultDesignSUBJOB.INTERNAL_NAME1 = entity.NUMBER + "-000-00-D1";
                    defaultDesignSUBJOB.STARTDATE = CommonMethods.StartOfWeek(DateTime.Now, DayOfWeek.Sunday);
                    defaultDesignSUBJOB.ENDDATE = ((DateTime)newSUBJOB.STARTDATE).AddDays(7).AddSeconds(-1);
                    defaultDesignSUBJOB.REVIEWSTARTDATE = (DateTime)newSUBJOB.STARTDATE; //effectively nullifies review date
                    defaultDesignSUBJOB.REVIEWENDDATE = (DateTime)newSUBJOB.STARTDATE; //effectively nullifies review date
                    defaultDesignSUBJOB.GUID_DAREA = defaultArea.GUID;
                    defaultDesignSUBJOB.GUID_DPHASE = defaultDirectPhase.GUID;
                    SUBJOBViewModel.Save(defaultDesignSUBJOB);

                    DISCIPLINE PMDiscipline = DISCIPLINEViewModel.Entities.FirstOrDefault(x => x.CODE == "PM");
                    if (PMDiscipline != null)
                    {
                        WORKPACK pmWORKPACK = new WORKPACK();
                        pmWORKPACK.GUID_SUBJOB = defaultDesignSUBJOB.GUID;
                        pmWORKPACK.GUID_DISCIPLINE = PMDiscipline.GUID;
                        pmWORKPACK.NAME = entity.NUMBER + "-000-00-D1-PM01";
                        WORKPACKViewModel.Save(pmWORKPACK);

                        DOCTYPE manDOCTYPE = DOCTYPEViewModel.Entities.FirstOrDefault(x => x.CODE == "MAN");
                        DEPARTMENT emDEPARTMENT = DEPARTMENTViewModel.Entities.FirstOrDefault(x => x.CODE == "EM");
                        if (manDOCTYPE != null && emDEPARTMENT != null)
                        {
                            BASELINE_ITEM dmBASELINE_ITEM = new BASELINE_ITEM();
                            dmBASELINE_ITEM.GUID_BASELINE = newBASELINE.GUID;
                            dmBASELINE_ITEM.GUID_SUBJOB = defaultDesignSUBJOB.GUID;
                            dmBASELINE_ITEM.GUID_DEPARTMENT = emDEPARTMENT.GUID;
                            dmBASELINE_ITEM.GUID_DISCIPLINE = PMDiscipline.GUID;
                            dmBASELINE_ITEM.GUID_DOCTYPE = manDOCTYPE.GUID;
                            dmBASELINE_ITEM.INTERNAL_NUM = entity.NUMBER + "-000-MAN-PM-001";
                            dmBASELINE_ITEM.PRIMARY_TITLE = "Design Management";
                            dmBASELINE_ITEM.GUID_WORKPACK = pmWORKPACK.GUID;
                            dmBASELINE_ITEM.GUID_AREA = defaultArea.GUID;
                            dmBASELINE_ITEM.GUID_PHASE = defaultDirectPhase.GUID;
                            BASELINE_ITEMViewModel.Save(dmBASELINE_ITEM);
                        }

                        DOCTYPE g02DOCTYPE = DOCTYPEViewModel.Entities.FirstOrDefault(x => x.CODE == "G02");
                        DEPARTMENT adDEPARTMENT = DEPARTMENTViewModel.Entities.FirstOrDefault(x => x.CODE == "AD");
                        if (g02DOCTYPE != null && adDEPARTMENT != null)
                        {
                            BASELINE_ITEM dcBASELINE_ITEM = new BASELINE_ITEM();
                            dcBASELINE_ITEM.GUID_BASELINE = newBASELINE.GUID;
                            dcBASELINE_ITEM.GUID_SUBJOB = defaultDesignSUBJOB.GUID;
                            dcBASELINE_ITEM.GUID_DEPARTMENT = adDEPARTMENT.GUID;
                            dcBASELINE_ITEM.GUID_DISCIPLINE = PMDiscipline.GUID;
                            dcBASELINE_ITEM.GUID_DOCTYPE = g02DOCTYPE.GUID;
                            dcBASELINE_ITEM.INTERNAL_NUM = entity.NUMBER + "-000-G02-PM-001";
                            dcBASELINE_ITEM.PRIMARY_TITLE = "Document Control";
                            dcBASELINE_ITEM.GUID_WORKPACK = pmWORKPACK.GUID;
                            dcBASELINE_ITEM.GUID_AREA = defaultArea.GUID;
                            dcBASELINE_ITEM.GUID_PHASE = defaultDirectPhase.GUID;
                            BASELINE_ITEMViewModel.Save(dcBASELINE_ITEM);
                        }
                    }

                    DISCIPLINE GEDiscipline = DISCIPLINEViewModel.Entities.FirstOrDefault(x => x.CODE == "GE");
                    if (GEDiscipline != null)
                    {
                        WORKPACK geWORKPACK = new WORKPACK();
                        geWORKPACK.GUID_SUBJOB = defaultDesignSUBJOB.GUID;
                        geWORKPACK.GUID_DISCIPLINE = GEDiscipline.GUID;
                        geWORKPACK.NAME = entity.NUMBER + "-000-00-D1-GE01";
                        WORKPACKViewModel.Save(geWORKPACK);

                        DOCTYPE mtgDOCTYPE = DOCTYPEViewModel.Entities.FirstOrDefault(x => x.CODE == "MTG");
                        DOCTYPE repDOCTYPE = DOCTYPEViewModel.Entities.FirstOrDefault(x => x.CODE == "REP");
                        DEPARTMENT enDEPARTMENT = DEPARTMENTViewModel.Entities.FirstOrDefault(x => x.CODE == "EN");
                        if (mtgDOCTYPE != null && enDEPARTMENT != null)
                        {
                            BASELINE_ITEM meetBASELINE_ITEM = new BASELINE_ITEM();
                            meetBASELINE_ITEM.GUID_BASELINE = newBASELINE.GUID;
                            meetBASELINE_ITEM.GUID_SUBJOB = defaultDesignSUBJOB.GUID;
                            meetBASELINE_ITEM.GUID_DEPARTMENT = enDEPARTMENT.GUID;
                            meetBASELINE_ITEM.GUID_DISCIPLINE = GEDiscipline.GUID;
                            meetBASELINE_ITEM.GUID_DOCTYPE = mtgDOCTYPE.GUID;
                            meetBASELINE_ITEM.INTERNAL_NUM = entity.NUMBER + "-000-MTG-GE-001";
                            meetBASELINE_ITEM.PRIMARY_TITLE = "Meetings";
                            meetBASELINE_ITEM.GUID_WORKPACK = geWORKPACK.GUID;
                            meetBASELINE_ITEM.GUID_AREA = defaultArea.GUID;
                            meetBASELINE_ITEM.GUID_PHASE = defaultDirectPhase.GUID;
                            BASELINE_ITEMViewModel.Save(meetBASELINE_ITEM);
                        }

                        if(repDOCTYPE != null && enDEPARTMENT != null)
                        {
                            BASELINE_ITEM rptBASELINE_ITEM = new BASELINE_ITEM();
                            rptBASELINE_ITEM.GUID_BASELINE = newBASELINE.GUID;
                            rptBASELINE_ITEM.GUID_SUBJOB = defaultDesignSUBJOB.GUID;
                            rptBASELINE_ITEM.GUID_DEPARTMENT = enDEPARTMENT.GUID;
                            rptBASELINE_ITEM.GUID_DISCIPLINE = GEDiscipline.GUID;
                            rptBASELINE_ITEM.GUID_DOCTYPE = repDOCTYPE.GUID;
                            rptBASELINE_ITEM.INTERNAL_NUM = entity.NUMBER + "-000-REP-GE-001";
                            rptBASELINE_ITEM.GUID_WORKPACK = geWORKPACK.GUID;
                            rptBASELINE_ITEM.GUID_AREA = defaultArea.GUID;
                            rptBASELINE_ITEM.GUID_PHASE = defaultDirectPhase.GUID;
                            rptBASELINE_ITEM.PRIMARY_TITLE = "Report";
                            BASELINE_ITEMViewModel.Save(rptBASELINE_ITEM);
                        }
                    }
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
            if (field_name == BindableBase.GetPropertyName(() => new PROJECT().DOC_KICKOFF) || field_name == BindableBase.GetPropertyName(() => new PROJECT().DOC_CLOSEOUT) || field_name == BindableBase.GetPropertyName(() => new PROJECT().DOC_SIDREPORT))
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
                ProjectStatus oldStatus = (ProjectStatus)old_value;
                ProjectStatus newStatus = (ProjectStatus)new_value;
                //switching between tender won't do anything
                if ((oldStatus == ProjectStatus.Tender || oldStatus == ProjectStatus.TenderSubmitted) && (newStatus != ProjectStatus.Tender || newStatus != ProjectStatus.TenderSubmitted))
                {
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new PROJECT().TENDER_CHANCE_OF_WIN), projection.TENDER_CHANCE_OF_WIN, null, EntityMessageType.Changed);
                    projection.TENDER_CHANCE_OF_WIN = null;
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new PROJECT().TENDER_DUE), projection.TENDER_DUE, null, EntityMessageType.Changed);
                    projection.TENDER_DUE = null;
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new PROJECT().TENDER_PROJECT_DURATION), projection.TENDER_PROJECT_DURATION, null, EntityMessageType.Changed);
                    projection.TENDER_PROJECT_DURATION = null;
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new PROJECT().TENDER_PROJECT_START), projection.TENDER_PROJECT_START, null, EntityMessageType.Changed);
                    projection.TENDER_PROJECT_START = null;
                }
                else if ((oldStatus != ProjectStatus.Tender && oldStatus != ProjectStatus.TenderSubmitted) && (newStatus == ProjectStatus.Tender || newStatus == ProjectStatus.TenderSubmitted))
                {
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new PROJECT().TENDER_CHANCE_OF_WIN), projection.TENDER_CHANCE_OF_WIN, 0, EntityMessageType.Changed);
                    projection.TENDER_CHANCE_OF_WIN = 0;
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new PROJECT().TENDER_CHANCE_OF_WIN), projection.TENDER_DUE, DateTime.Now, EntityMessageType.Changed);
                    projection.TENDER_DUE = DateTime.Now;
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new PROJECT().TENDER_CHANCE_OF_WIN), projection.TENDER_PROJECT_DURATION, 0, EntityMessageType.Changed);
                    projection.TENDER_PROJECT_DURATION = 0;
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new PROJECT().TENDER_CHANCE_OF_WIN), projection.TENDER_PROJECT_START, DateTime.Now, EntityMessageType.Changed);
                    projection.TENDER_PROJECT_START = DateTime.Now;
                }
            }

            base.UnifiedCellValueChanging(field_name, old_value, new_value, projection, isNew);
        }

        public override string UnifiedValueValidation(PROJECT projection, string field_name, object new_value)
        {
            string missingPathErrorString = "Path not selected";
            bool isError = false;

            if (field_name == BindableBase.GetPropertyName(() => new PROJECT().DOC_KICKOFF))
            {
                if (new_value != null && ((ProjectDocumentStatus)new_value) == ProjectDocumentStatus.Yes && projection.DOC_KICKOFF_PATH == null)
                    isError = true;
            }
            else if (field_name == BindableBase.GetPropertyName(() => new PROJECT().DOC_CLOSEOUT))
            {
                if (new_value != null && ((ProjectDocumentStatus)new_value) == ProjectDocumentStatus.Yes && projection.DOC_CLOSEOUT_PATH == null)
                    isError = true;
            }
            else if (field_name == BindableBase.GetPropertyName(() => new PROJECT().DOC_SIDREPORT))
            {
                if (new_value != null && ((ProjectDocumentStatus)new_value) == ProjectDocumentStatus.Yes && projection.DOC_SIDREPORT_PATH == null)
                    isError = true;
            }
            else if (field_name == BindableBase.GetPropertyName(() => new PROJECT().TENDER_DUE))
            {
                DateTime? tenderDue = (DateTime?)new_value;
                if(tenderDue > projection.TENDER_PROJECT_START)
                {
                    return "Tender due date cannot be after project start date";
                }
            }
            else if (field_name == BindableBase.GetPropertyName(() => new PROJECT().TENDER_PROJECT_START))
            {
                DateTime? tenderProjectStart = (DateTime?)new_value;
                if (tenderProjectStart < projection.TENDER_DUE)
                {
                    return "Tender project start date cannot be before tender due date";
                }
            }

            if (isError)
                return missingPathErrorString;

            return string.Empty;
        }

        #endregion

        #region View Properties
        public void CustomColumnDisplayText(CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName == BindableBase.GetPropertyName(() => new PROJECT().Disciplines))
            {
                if(e.Row != null && e.Value != null && DISCIPLINECollection != null)
                {
                    IEnumerable<DISCIPLINE> selected_disciplines = ((PROJECT)e.Row).Project_Disciplines;
                    if(DISCIPLINECollection.All(x => selected_disciplines.Any(selected => selected.GUID == x.GUID)))
                    {
                        e.DisplayText = "All Disciplines";
                    }
                }
            }
        }

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

        public CollectionViewModel<RATE, RATE, Guid, IBluePrintsEntitiesUnitOfWork> RATEViewModel
        {
            get
            {
                if (loaderCollection == null)
                    return null;

                return
                    (CollectionViewModel<RATE, RATE, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<RATE>();
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


        DISCIPLINE allDiscipline = new DISCIPLINE() { GUID = Guid.NewGuid(), NAME = "All" };
        public IEnumerable<DISCIPLINE> DISCIPLINECollection
        {
            get
            {
                var collection = GetEntities<DISCIPLINE>();
                if (collection != null)
                {
                    collection = collection.OrderBy(x => x.NAME);
                }

                return collection;
            }
        }

        public IEnumerable<PHASE> PHASECollection
        {
            get
            {
                return GetEntities<PHASE>();
            }
        }

        public IEnumerable<PROJECT_DISCIPLINE> PROJECT_DISCIPLINECollection
        {
            get
            {
                var collection = GetEntities<PROJECT_DISCIPLINE>();
                return collection;
            }
        }


        public CollectionViewModel<PROJECT_DISCIPLINE, PROJECT_DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork> PROJECT_DISCIPLINECollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<PROJECT_DISCIPLINE, PROJECT_DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<PROJECT_DISCIPLINE>();
            }
        }
        #endregion
    }
}