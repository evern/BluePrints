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
        private Action<object> navigateCoreCommand;
        protected override void resolveParameters(object parameter)
        {
            navigateCoreCommand = ((EntitiesParameter<Action<object>>)parameter).GetEntity();
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

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINES, BASELINEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.ESTIMATES, ESTIMATEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.SUBJOBS, SUBJOBProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.WORKPACKS, WORKPACKProjectionFunc);
            loaderCollection.AddLoaderDescription<PHASE, PHASE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.PHASES);
            loaderCollection.AddLoaderDescription<DELIVERABLES_STATUS, DELIVERABLES_STATUS, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DELIVERABLES_STATUSES);
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription<PROJECT_DISCIPLINE, PROJECT_DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.PROJECT_DISCIPLINES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS, BASELINE_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DOCTYPES);
            loaderCollection.AddLoaderDescription<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.USERS);
            loaderCollection.AddLoaderDescription<OFFICE, OFFICE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.OFFICES);
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
            return query => query;
        }

        /// <summary>
        /// SUBJOB is used for write only so just load a single entry for repository to be initialized
        /// </summary>
        private Func<IRepositoryQuery<SUBJOB>, IQueryable<SUBJOB>> SUBJOBProjectionFunc()
        {
            return query => query;
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
            //commented out because this is not a reliable way of determining entity due to created date isn't mapped back to projection on after saved
            //bool? isEntityNew = DataUtils.IsNewEntity<PROJECT>(projectionEntity);
            //end comment

            DateTime? tenderStartDate = entity.TENDER_PROJECT_START;
            DateTime? tenderEndDate = entity.TENDER_PROJECT_START == null ? (DateTime?)null : ((DateTime)entity.TENDER_PROJECT_START).AddDays(Convert.ToDouble((decimal)entity.TENDER_PROJECT_DURATION) * 7);
            IBluePrintsEntitiesUnitOfWork unitOfWork = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
            //only way to determine whether current entity is new to avoid creating multiple 
            if (isNewEntity)
            {
                BASELINE newBASELINE = new BASELINE();
                newBASELINE.GUID_PROJECT = entity.GUID;
                newBASELINE.NAME = entity.NUMBER + "_001";
                newBASELINE.REVISION = "A";
                newBASELINE.STATUS = BaselineStatus.Live;
                unitOfWork.BASELINES.Add(newBASELINE);
                //BASELINEViewModel.Save(newBASELINE);

                ESTIMATE newESTIMATE_DIRECT = new ESTIMATE();
                newESTIMATE_DIRECT.GUID_PROJECT = entity.GUID;
                newESTIMATE_DIRECT.NAME = entity.NUMBER + "_001";
                newESTIMATE_DIRECT.REVISION = "A";
                newESTIMATE_DIRECT.STATUS = BaselineStatus.Working;
                unitOfWork.ESTIMATES.Add(newESTIMATE_DIRECT);
                //ESTIMATEViewModel.Save(newESTIMATE_DIRECT);

                PROGRESS newDesignPROGRESS = new PROGRESS();
                newDesignPROGRESS.GUID_PROJECT = entity.GUID;
                newDesignPROGRESS.NAME = entity.NUMBER + "WEEKLY_001";
                newDesignPROGRESS.PROGRESS_START = tenderStartDate == null ? DateTime.Now : (DateTime)tenderStartDate;
                newDesignPROGRESS.DATA_DATE = CommonMethods.StartOfWeek(newDesignPROGRESS.PROGRESS_START, DayOfWeek.Sunday).AddDays(1).AddSeconds(-1);
                newDesignPROGRESS.INTERVAL_COUNT = 1;
                newDesignPROGRESS.INTERVAL_TYPE = ProgressIntervalType.Weekly;
                newDesignPROGRESS.STATUS = ProgressStatus.Live;
                newDesignPROGRESS.TYPE = PhaseType.Design;
                unitOfWork.PROGRESSES.Add(newDesignPROGRESS);
                //PROGRESSViewModel.Save(newDesignPROGRESS);

                PROGRESS newConstructionPROGRESS = new PROGRESS();
                newConstructionPROGRESS.GUID_PROJECT = entity.GUID;
                newConstructionPROGRESS.NAME = entity.NUMBER + "DAILY_001";
                newConstructionPROGRESS.PROGRESS_START = tenderStartDate == null ? DateTime.Now : (DateTime)tenderStartDate;
                newConstructionPROGRESS.DATA_DATE = CommonMethods.StartOfWeek(newConstructionPROGRESS.PROGRESS_START, DayOfWeek.Sunday).AddDays(1).AddSeconds(-1);
                newConstructionPROGRESS.INTERVAL_COUNT = 1;
                newConstructionPROGRESS.INTERVAL_TYPE = ProgressIntervalType.Daily;
                newConstructionPROGRESS.STATUS = ProgressStatus.Live;
                newConstructionPROGRESS.TYPE = PhaseType.Construct;
                unitOfWork.PROGRESSES.Add(newConstructionPROGRESS);
                //PROGRESSViewModel.Save(newConstructionPROGRESS);

                AREA defaultArea = new AREA();
                defaultArea.GUID_PROJECT = entity.GUID;
                defaultArea.INTERNAL_NUM = "000";
                defaultArea.CLIENT_NUM = "000";
                defaultArea.TITLE = "General";
                unitOfWork.AREAS.Add(defaultArea);
                //AREAViewModel.Save(defaultArea);
                unitOfWork.SaveChanges();

                PHASE defaultDirectPhase = unitOfWork.PHASES.FirstOrDefault(x => x.INTERNAL_NUM == "D1");
                PHASE defaultIndirectPhase = unitOfWork.PHASES.FirstOrDefault(x => x.INTERNAL_NUM == "I1");

                DEPARTMENT defaultDepartment = unitOfWork.DEPARTMENTS.FirstOrDefault(x => x.NAME == BluePrintsResources.Default_New_Project_Department);
                DISCIPLINE defaultDiscipline = unitOfWork.DISCIPLINES.FirstOrDefault(x => x.NAME == BluePrintsResources.Default_New_Project_Discipline);
                DOCTYPE defaultDocType = unitOfWork.DOCTYPES.FirstOrDefault(x => x.NAME == BluePrintsResources.Default_New_Project_DocType);
                PROJECT defaultCopyProject = unitOfWork.PROJECTS.FirstOrDefault(x => x.NUMBER == "00000");
                if(defaultCopyProject != null)
                {
                    foreach(RATE rate in defaultCopyProject.RATE)
                    {
                        RATE newRATE = new RATE();
                        DataUtils.ShallowCopy(newRATE, rate);
                        newRATE.GUID = Guid.Empty;
                        newRATE.GUID_PROJECT = entity.GUID;
                        unitOfWork.RATES.Add(newRATE);
                        //RATEViewModel.Save(newRATE);
                    }

                    foreach(DELIVERABLES_STATUS status in defaultCopyProject.DELIVERABLES_STATUS)
                    {
                        DELIVERABLES_STATUS newSTATUS = new DELIVERABLES_STATUS();
                        DataUtils.ShallowCopy(newSTATUS, status);
                        newSTATUS.GUID = Guid.Empty;
                        newSTATUS.GUID_PROJECT = entity.GUID;
                        unitOfWork.DELIVERABLES_STATUSES.Add(newSTATUS);
                        //DELIVERABLES_STATUSViewModel.Save(newSTATUS);
                    }

                    foreach(HOLIDAY holiday in defaultCopyProject.HOLIDAY)
                    {
                        HOLIDAY newHOLIDAY = new HOLIDAY();
                        DataUtils.ShallowCopy(newHOLIDAY, holiday);
                        newHOLIDAY.GUID = Guid.Empty;
                        newHOLIDAY.GUID_PROJECT = entity.GUID;
                        unitOfWork.HOLIDAYS.Add(newHOLIDAY);
                    }
                }

                SUBJOB newSUBJOB = new SUBJOB();
                newSUBJOB.GUID_PROJECT = entity.GUID;
                newSUBJOB.INTERNAL_NAME1 = entity.NUMBER;
                newSUBJOB.STARTDATE = tenderStartDate == null ? CommonMethods.StartOfWeek(DateTime.Now, DayOfWeek.Sunday) : tenderStartDate;
                newSUBJOB.ENDDATE = tenderEndDate == null ? ((DateTime)newSUBJOB.STARTDATE).AddDays(7).AddSeconds(-1) : tenderEndDate;
                newSUBJOB.REVIEWSTARTDATE = (DateTime)newSUBJOB.STARTDATE; //effectively nullifies review date
                newSUBJOB.REVIEWENDDATE = (DateTime)newSUBJOB.STARTDATE; //effectively nullifies review date
                newSUBJOB.BELLCURVESHAPE = BellCurveShape.Balanced;

                if(entity.STATUS == ProjectStatus.Tender || entity.STATUS == ProjectStatus.TenderSubmitted)
                {
                    newSUBJOB.BELLCURVESHAPE = BellCurveShape.Balanced;
                }
 
                newSUBJOB.GUID_DAREA = defaultArea.GUID;
                newSUBJOB.GUID_DPHASE = defaultDirectPhase.GUID;
                unitOfWork.SUBJOBS.Add(newSUBJOB);
                //SUBJOBViewModel.Save(newSUBJOB);

                if (defaultDirectPhase != null)
                {
                    //if (defaultDepartment != null && defaultDiscipline != null)
                    //{

                    //}

                    SUBJOB defaultDesignSUBJOB = new SUBJOB();
                    defaultDesignSUBJOB.GUID_PROJECT = entity.GUID;
                    defaultDesignSUBJOB.INTERNAL_NAME1 = entity.NUMBER + "-000-00-D1";
                    defaultDesignSUBJOB.STARTDATE = tenderStartDate == null ? CommonMethods.StartOfWeek(DateTime.Now, DayOfWeek.Sunday) : tenderStartDate;
                    defaultDesignSUBJOB.ENDDATE = tenderEndDate == null ? ((DateTime)newSUBJOB.STARTDATE).AddDays(7).AddSeconds(-1) : tenderEndDate;
                    defaultDesignSUBJOB.REVIEWSTARTDATE = (DateTime)newSUBJOB.STARTDATE; //effectively nullifies review date
                    defaultDesignSUBJOB.REVIEWENDDATE = (DateTime)newSUBJOB.STARTDATE; //effectively nullifies review date
                    defaultDesignSUBJOB.GUID_DAREA = defaultArea.GUID;
                    defaultDesignSUBJOB.GUID_DPHASE = defaultDirectPhase.GUID;
                    defaultDesignSUBJOB.BELLCURVESHAPE = BellCurveShape.Balanced;
                    if (entity.STATUS == ProjectStatus.Tender || entity.STATUS == ProjectStatus.TenderSubmitted)
                    {
                        defaultDesignSUBJOB.BELLCURVESHAPE = BellCurveShape.Balanced;
                    }
                    unitOfWork.SUBJOBS.Add(defaultDesignSUBJOB);
                    unitOfWork.SaveChanges();
                    //SUBJOBViewModel.Save(defaultDesignSUBJOB);

                    DISCIPLINE PMDiscipline = unitOfWork.DISCIPLINES.FirstOrDefault(x => x.CODE == "PM");
                    if (PMDiscipline != null)
                    {
                        WORKPACK pmWORKPACK = new WORKPACK();
                        pmWORKPACK.GUID_SUBJOB = defaultDesignSUBJOB.GUID;
                        pmWORKPACK.GUID_DISCIPLINE = PMDiscipline.GUID;
                        pmWORKPACK.NAME = entity.NUMBER + "-000-00-D1-PM01";
                        unitOfWork.WORKPACKS.Add(pmWORKPACK);
                        //WORKPACKViewModel.Save(pmWORKPACK);
                        unitOfWork.SaveChanges();

                        DOCTYPE manDOCTYPE = unitOfWork.DOCTYPES.FirstOrDefault(x => x.CODE == "MAN");
                        DEPARTMENT emDEPARTMENT = unitOfWork.DEPARTMENTS.FirstOrDefault(x => x.CODE == "EM");
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
                            unitOfWork.BASELINE_ITEMS.Add(dmBASELINE_ITEM);
                            //BASELINE_ITEMViewModel.Save(dmBASELINE_ITEM);
                        }
                    }

                    DISCIPLINE GEDiscipline = unitOfWork.DISCIPLINES.FirstOrDefault(x => x.CODE == "GE");
                    if (GEDiscipline != null)
                    {
                        WORKPACK geWORKPACK = new WORKPACK();
                        geWORKPACK.GUID_SUBJOB = defaultDesignSUBJOB.GUID;
                        geWORKPACK.GUID_DISCIPLINE = GEDiscipline.GUID;
                        geWORKPACK.NAME = entity.NUMBER + "-000-00-D1-GE01";
                        unitOfWork.WORKPACKS.Add(geWORKPACK);
                        //WORKPACKViewModel.Save(geWORKPACK);
                        unitOfWork.SaveChanges();

                        DOCTYPE mtgDOCTYPE = unitOfWork.DOCTYPES.FirstOrDefault(x => x.CODE == "MTG");
                        DOCTYPE repDOCTYPE = unitOfWork.DOCTYPES.FirstOrDefault(x => x.CODE == "REP");
                        DEPARTMENT enDEPARTMENT = unitOfWork.DEPARTMENTS.FirstOrDefault(x => x.CODE == "EN");
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
                            unitOfWork.BASELINE_ITEMS.Add(meetBASELINE_ITEM);
                            //BASELINE_ITEMViewModel.Save(meetBASELINE_ITEM);
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
                            unitOfWork.BASELINE_ITEMS.Add(rptBASELINE_ITEM);
                            //BASELINE_ITEMViewModel.Save(rptBASELINE_ITEM);
                        }
                    }
                }

                if (defaultIndirectPhase != null)
                {
                    SUBJOB indirectDesignSUBJOB = new SUBJOB();
                    indirectDesignSUBJOB.GUID_PROJECT = entity.GUID;
                    indirectDesignSUBJOB.INTERNAL_NAME1 = entity.NUMBER + "-000-00-I1";
                    indirectDesignSUBJOB.STARTDATE = tenderStartDate == null ? CommonMethods.StartOfWeek(DateTime.Now, DayOfWeek.Sunday) : tenderStartDate;
                    indirectDesignSUBJOB.ENDDATE = tenderEndDate == null ? ((DateTime)newSUBJOB.STARTDATE).AddDays(7).AddSeconds(-1) : tenderEndDate;
                    indirectDesignSUBJOB.REVIEWSTARTDATE = (DateTime)newSUBJOB.STARTDATE; //effectively nullifies review date
                    indirectDesignSUBJOB.REVIEWENDDATE = (DateTime)newSUBJOB.STARTDATE; //effectively nullifies review date
                    indirectDesignSUBJOB.GUID_DAREA = defaultArea.GUID;
                    indirectDesignSUBJOB.GUID_DPHASE = defaultIndirectPhase.GUID;
                    indirectDesignSUBJOB.BELLCURVESHAPE = BellCurveShape.Balanced;

                    if (entity.STATUS == ProjectStatus.Tender || entity.STATUS == ProjectStatus.TenderSubmitted)
                    {
                        indirectDesignSUBJOB.BELLCURVESHAPE = BellCurveShape.Balanced;
                    }

                    unitOfWork.SUBJOBS.Add(indirectDesignSUBJOB);
                    //SUBJOBViewModel.Save(indirectDesignSUBJOB);

                    unitOfWork.SaveChanges();
                    DOCTYPE g02DOCTYPE = unitOfWork.DOCTYPES.FirstOrDefault(x => x.CODE == "G02");
                    DEPARTMENT adDEPARTMENT = unitOfWork.DEPARTMENTS.FirstOrDefault(x => x.CODE == "AD");
                    DISCIPLINE PMDiscipline = unitOfWork.DISCIPLINES.FirstOrDefault(x => x.CODE == "PM");
                    if (PMDiscipline != null)
                    {
                        WORKPACK pmWORKPACK = new WORKPACK();
                        pmWORKPACK.GUID_SUBJOB = indirectDesignSUBJOB.GUID;
                        pmWORKPACK.GUID_DISCIPLINE = PMDiscipline.GUID;
                        pmWORKPACK.NAME = entity.NUMBER + "-000-00-I1-PM01";
                        unitOfWork.WORKPACKS.Add(pmWORKPACK);
                        unitOfWork.SaveChanges();
                        //WORKPACKViewModel.Save(pmWORKPACK);

                        if (g02DOCTYPE != null && adDEPARTMENT != null)
                        {
                            BASELINE_ITEM dcBASELINE_ITEM = new BASELINE_ITEM();
                            dcBASELINE_ITEM.GUID_BASELINE = newBASELINE.GUID;
                            dcBASELINE_ITEM.GUID_SUBJOB = indirectDesignSUBJOB.GUID;
                            dcBASELINE_ITEM.GUID_DEPARTMENT = adDEPARTMENT.GUID;
                            dcBASELINE_ITEM.GUID_DISCIPLINE = PMDiscipline.GUID;
                            dcBASELINE_ITEM.GUID_DOCTYPE = g02DOCTYPE.GUID;
                            dcBASELINE_ITEM.INTERNAL_NUM = entity.NUMBER + "-000-G02-PM-001";
                            dcBASELINE_ITEM.PRIMARY_TITLE = "Document Control";
                            dcBASELINE_ITEM.GUID_WORKPACK = pmWORKPACK.GUID;
                            dcBASELINE_ITEM.GUID_AREA = defaultArea.GUID;
                            dcBASELINE_ITEM.GUID_PHASE = defaultIndirectPhase.GUID;
                            unitOfWork.BASELINE_ITEMS.Add(dcBASELINE_ITEM);
                            //BASELINE_ITEMViewModel.Save(dcBASELINE_ITEM);
                        }
                    }
                }
            }
            else if(shouldInvokeTenderSubjobDates && (entity.STATUS == ProjectStatus.Tender || entity.STATUS == ProjectStatus.TenderSubmitted))
            {
                if(tenderStartDate != null && tenderEndDate != null)
                {
                    if (MessageBoxService.ShowMessage("Since project is in tender phase do you wish to change the start and finish dates of all SUBJOBS and PROGRESS in this project?\n\nStart date will be tender project start date\n\nEnd date will be tender project start date plus duration", "Change Subjob Dates", MessageButton.YesNo) == MessageResult.Yes)
                    {
                        decimal duration = (decimal)entity.TENDER_PROJECT_DURATION;
                        IEnumerable<SUBJOB> allSubJobs = unitOfWork.SUBJOBS.Where(x => x.GUID_PROJECT == entity.GUID);
                        List<SUBJOB> saveSUBJOB = new List<SUBJOB>();
                        foreach (SUBJOB subjob in allSubJobs)
                        {
                            subjob.STARTDATE = tenderStartDate;
                            subjob.ENDDATE = tenderEndDate;
                            subjob.REVIEWSTARTDATE = subjob.STARTDATE;
                            subjob.REVIEWENDDATE = subjob.STARTDATE;
                        }

                        IEnumerable<PROGRESS> allProgresses = unitOfWork.PROGRESSES.Where(x => x.GUID_PROJECT == entity.GUID);
                        List<PROGRESS> savePROGRESS = new List<PROGRESS>();
                        foreach(PROGRESS progress in allProgresses)
                        {
                            progress.PROGRESS_START = (DateTime)tenderStartDate;
                            progress.DATA_DATE = CommonMethods.StartOfWeek(progress.PROGRESS_START, DayOfWeek.Sunday).AddDays(1).AddSeconds(-1);
                        }

                        unitOfWork.SaveChanges();
                    }
                }
            }

            shouldInvokeTenderSubjobDates = false;
        }
        #endregion

        #endregion

        #region View Behavior
        protected DevExpress.Mvvm.IDialogService TenderProfileSelectionDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("TenderProfileSelectionDialog"); }
        }

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

        bool shouldInvokeTenderSubjobDates = false;
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

            if (field_name == BindableBase.GetPropertyName(() => new PROJECT().Status))
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                BluePrintsContextHelper.AsyncRefreshDeliverablesDataPointsByProject(projection.NUMBER);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                ProjectStatus oldStatus = (ProjectStatus)old_value;
                ProjectStatus newStatus = (ProjectStatus)new_value;
                //switching between tender won't do anything
                if ((oldStatus == ProjectStatus.Tender || oldStatus == ProjectStatus.TenderSubmitted) && (newStatus != ProjectStatus.Tender && newStatus != ProjectStatus.TenderSubmitted))
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
                else if ((oldStatus != ProjectStatus.Tender || oldStatus != ProjectStatus.TenderSubmitted) && (newStatus == ProjectStatus.Tender || newStatus == ProjectStatus.TenderSubmitted))
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

                projection.Update();
            }

            if (field_name == BindableBase.GetPropertyName(() => new PROJECT().TENDER_DUE))
            {
                DateTime? oldValue = projection.TENDER_PROJECT_START;
                projection.TENDER_PROJECT_START = (DateTime?)new_value;
                MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new PROJECT().TENDER_PROJECT_START), oldValue, new_value, EntityMessageType.Changed);
                shouldInvokeTenderSubjobDates = true;
            }

            if (field_name == BindableBase.GetPropertyName(() => new PROJECT().TENDER_PROJECT_START) || field_name == BindableBase.GetPropertyName(() => new PROJECT().TENDER_PROJECT_DURATION))
            {
                shouldInvokeTenderSubjobDates = true;
            }


            base.UnifiedCellValueChanging(field_name, old_value, new_value, projection, isNew);
        }

        public override string UnifiedRowValidation(PROJECT projection)
        {
            return string.Empty;
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
            else if (field_name == BindableBase.GetPropertyName(() => new PROJECT().NUMBER))
            {
                if(DisplayEntities.Any(x => x.NUMBER != null && x.NUMBER.ToUpper() == new_value.ToString().ToUpper()))
                {
                    return "Project number already exists";
                }
            }
            //else if (field_name == BindableBase.GetPropertyName(() => new PROJECT().TENDER_PROJECT_START))
            //{
            //    DateTime? tenderProjectStart = (DateTime?)new_value;
            //    if (tenderProjectStart < projection.TENDER_DUE)
            //    {
            //        return "Tender project start date cannot be before tender due date";
            //    }
            //}

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


        public CollectionViewModel<DELIVERABLES_STATUS, DELIVERABLES_STATUS, Guid, IBluePrintsEntitiesUnitOfWork> DELIVERABLES_STATUSViewModel
        {
            get
            {
                if (loaderCollection == null)
                    return null;

                return
                    (CollectionViewModel<DELIVERABLES_STATUS, DELIVERABLES_STATUS, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<DELIVERABLES_STATUS>();
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

        public void TenderProfile()
        {
            if (DisplaySelectedEntity == null)
                return;

            if(DisplaySelectedEntity.STATUS != ProjectStatus.Tender && DisplaySelectedEntity.STATUS != ProjectStatus.TenderSubmitted)
            {
                MessageBoxService.ShowMessage("Project must be a tender to begin tender profiling");
                return;
            }

            if (DisplaySelectedEntity.TENDER_PROJECT_START == null)
            {
                MessageBoxService.ShowMessage("Please set tender start date before proceeding");
                return;
            }

            if(DisplaySelectedEntity.TENDER_PROJECT_DURATION == null)
            {
                MessageBoxService.ShowMessage("Please set tender duration before proceeding");
                return;
            }

            TENDER_PROFILE_ITEMSelectionViewModelWrapper tenderProfileSelectionViewModel = TENDER_PROFILE_ITEMSelectionViewModelWrapper.Create();
            tenderProfileSelectionViewModel.OnParameterChanged(new EntitiesParameter<PROJECT>(DisplaySelectedEntity));
            TenderProfileSelectionDialogService.ShowDialog(MessageButton.OK, "Apply tender profile", "TENDER_PROFILESelectionView", tenderProfileSelectionViewModel);
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
                new DualEntitiesParameter<PROJECT, Action<object>>(DisplaySelectedEntity, navigateCoreCommand), 
                "PROJECTView", 
                "[" + DisplaySelectedEntity.NUMBER + "]");

            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
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

        public CollectionViewModel<SUBJOB, SUBJOB, Guid, IBluePrintsEntitiesUnitOfWork> SUBJOBCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<SUBJOB, SUBJOB, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<SUBJOB>();
            }
        }

        public CollectionViewModel<PROGRESS, PROGRESS, Guid, IBluePrintsEntitiesUnitOfWork> PROGRESSCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<PROGRESS, PROGRESS, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<PROGRESS>();
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