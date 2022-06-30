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
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.Utils;
using BluePrints.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Data.Filtering;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace BluePrints.ViewModels
{
    public class PROJECTCollectionViewModelWrapper : BluePrintsEntitiesCollectionWrapper<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>
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

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private Action<object> navigateCoreCommand;
        BackgroundWorker backgroundWorker = new BackgroundWorker();
        protected IPrimeroEntitiesUnitOfWork primeroPerthUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
        protected IPrimeroEntitiesUnitOfWork primeroMontrealUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(BluePrintsResources.OfficeMontreal).CreateUnitOfWork();

        protected override void resolveParameters(object parameter)
        {
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.WorkerSupportsCancellation = true;
            navigateCoreCommand = ((EntitiesParameter<Action<object>>)parameter).GetEntity();
        }

        public override void OnLoaded()
        {
            if (!isFirstLoaded)
            {
                if (AppNotificationService == null || GlobalVariables.IsProjectCollectionViewNotificationShown)
                {
                    base.OnLoaded();
                    return;
                }

                INotification notification = AppNotificationService.CreatePredefinedNotification("Update: go to rate, baseline, progress and estimate from right clicking or top menu", null, null, null);
                GlobalVariables.IsProjectViewNotificationShown = true;
                notification.ShowAsync();
            }

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
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.USERS, USERProjectionFunc);
            loaderCollection.AddLoaderDescription<OFFICE, OFFICE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.OFFICES);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.PROJECTS);
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

        private void saveProjectDiscipline(PROJECT entity)
        {
            List<PROJECT_DISCIPLINE> removeProjectDisciplines = new List<PROJECT_DISCIPLINE>();

            if (entity.Disciplines != null)
            {
                foreach (PROJECT_DISCIPLINE assignment in PROJECT_DISCIPLINECollection.Where(x => x.GUID_PROJECT == entity.GUID))
                {
                    if (!entity.Project_Disciplines.Any(x => x.GUID == assignment.GUID_DISCIPLINE))
                        removeProjectDisciplines.Add(assignment);
                }

                PROJECT_DISCIPLINECollectionViewModel.BaseBulkDelete(removeProjectDisciplines);
                List<PROJECT_DISCIPLINE> addProjectDisciplines = new List<PROJECT_DISCIPLINE>();
                foreach (DISCIPLINE project_discipline in entity.Project_Disciplines)
                {
                    if (!PROJECT_DISCIPLINECollection.Any(x => x.GUID_DISCIPLINE == project_discipline.GUID && x.GUID_PROJECT == entity.GUID))
                        addProjectDisciplines.Add(new PROJECT_DISCIPLINE() { GUID_DISCIPLINE = project_discipline.GUID, GUID_PROJECT = entity.GUID });
                }

                PROJECT_DISCIPLINECollectionViewModel.BaseBulkSave(addProjectDisciplines);
            }
            else
            {
                foreach (PROJECT_DISCIPLINE assignment in PROJECT_DISCIPLINECollection.Where(x => x.GUID_PROJECT == entity.GUID))
                {
                    removeProjectDisciplines.Add(assignment);
                }

                PROJECT_DISCIPLINECollectionViewModel.BaseBulkDelete(removeProjectDisciplines);
            }
        }

        private void onAfterEntitySaved(PROJECT projection, PROJECT entity, bool isNewEntity)
        {
            saveProjectDiscipline(projection);
            if(entity != null)
                PostSave(projection, entity, isNewEntity);
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

        protected virtual Func<IRepositoryQuery<USER>, IQueryable<USER>> USERProjectionFunc()
        {
            return query => query.Where(x => x.LEAVE_DATE == null || x.LEAVE_DATE > DateTime.Now);
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
            MainViewModel.OnAfterProjectionSavedCallBack = onAfterEntitySaved;
            MainViewModel.CanFillDownCallBack = CanFillDownCallBack;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        #region Collection Call Backs
        public override void OnAfterAuxiliaryEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender, Guid senderKey, bool isBulkRefresh)
        {
            if(messageType == EntityMessageType.Added)
            {
                PROJECT findPROJECT = MainViewModel.Entities.FirstOrDefault(x => x.GUID == (Guid)key);
                if(findPROJECT != null)
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    BluePrintsContextHelper.AsyncRefreshDeliverablesDataPointsByProject(findPROJECT);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }

            base.OnAfterAuxiliaryEntitiesChanged(key, changedType, messageType, sender, senderKey, isBulkRefresh);
        }

        protected override OperationInterceptMode OnBeforeProjectionDeleteIsContinue(PROJECT projection, out List<ErrorMessage> errorMessages)
        {           
            //Avoid EF exception on PROJECT_DISCIPLINE foreign key: The relationship could not be changed because one or more of the foreign-key properties is non-nullable
            saveProjectDiscipline(projection);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            BluePrintsContextHelper.AsyncRefreshDeliverablesDataPointsByProject(projection);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            return base.OnBeforeProjectionDeleteIsContinue(projection, out errorMessages);
        }

        private void PostSave(PROJECT projectionEntity, PROJECT entity, bool isNewEntity)
        {
            //commented out because this is not a reliable way of determining entity due to created date isn't mapped back to projection on after saved
            //bool? isEntityNew = DataUtils.IsNewEntity<PROJECT>(projectionEntity);
            //end comment

            Tuple<DateTime, DateTime> tenderStartEndDate = BluePrintsDataUtils.GetTenderStartEndDate(entity);

            DateTime? tenderStartDate = tenderStartEndDate.Item1;
            DateTime? tenderEndDate = tenderStartEndDate.Item2;
            IBluePrintsEntitiesUnitOfWork unitOfWork = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
            //only way to determine whether current entity is new to avoid creating multiple 
            if (isNewEntity)
            {
                BluePrintsDataUtils.CreateNewProjectDefaults(entity, unitOfWork);
            }
            else if(shouldInvokeTenderSubjobDates && (entity.STATUS == ProjectStatus.Tender || entity.STATUS == ProjectStatus.TenderSubmitted))
            {
                if(tenderStartDate != null && tenderEndDate != null)
                {
                    if (MessageBoxService.ShowMessage("Since project is in tender phase do you wish to change the start and finish dates of all SUBJOBS and PROGRESS in this project?\n\nStart date will be tender project start date\n\nEnd date will be tender project start date plus duration", "Change Subjob Dates", MessageButton.YesNo) == MessageResult.Yes)
                    {
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
                            progress.DATA_DATE = CommonMethods.GetStartOfWeek(progress.PROGRESS_START, DayOfWeek.Sunday).AddDays(1).AddSeconds(-1);
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
                if(!backgroundWorker.IsBusy)
                    backgroundWorker.RunWorkerAsync(new object[] { projection });

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

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var argumentObject = (object[])e.Argument;
            var project = (PROJECT)argumentObject[0];

            Thread.Sleep(10000);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            BluePrintsContextHelper.AsyncRefreshDeliverablesDataPointsByProject(project);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public override string UnifiedRowValidation(PROJECT projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(PROJECT projection, string field_name, object new_value, bool isPaste)
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
                if(Entities.Any(x => x.NUMBER != null && x.NUMBER.ToUpper() == new_value.ToString().ToUpper()))
                {
                    return "Project number already exists";
                }
            }
            //disallow office changes when it exists in EXO
            //else if (field_name == BindableBase.GetPropertyName(() => new PROJECT().GUID_OFFICE))
            //{
            //    if (projection.NUMBER != string.Empty)
            //    {
            //        if (projection.GUID_OFFICE != null && new_value != null)
            //        {
            //            Guid newOfficeGuid = (Guid)new_value;
            //            OFFICE findOldOFFICE = OFFICECollection.FirstOrDefault(x => x.GUID == projection.GUID_OFFICE);
            //            if (findOldOFFICE != null)
            //            {
            //                JOBCOST_LINES exoLine;
            //                if (findOldOFFICE.NAME.ToUpper() == BluePrintsResources.OfficeMontreal)
            //                    exoLine = ExoQueries.GetAnyProjectLineByJobNumber(primeroMontrealUnitOfWork, projection.NUMBER);
            //                else
            //                    exoLine = ExoQueries.GetAnyProjectLineByJobNumber(primeroPerthUnitOfWork, projection.NUMBER);

            //                if (exoLine != null && newOfficeGuid.ToString() != projection.GUID_OFFICE.ToString())
            //                {
            //                    return "Exo job already already exists in " + findOldOFFICE.NAME + ", so office cannot be changed";
            //                }
            //            }
            //        }
            //    }
            //}

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

        public IEnumerable<USER> USERCollection => GetEntities<USER>();

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
        public override string ViewName
        {
            get { return "PROJECTCollectionViewModelWrapper_v2"; }
        }

        public void TenderProfile()
        {
            if (SelectedEntity == null)
                return;

            if(SelectedEntity.STATUS != ProjectStatus.Tender && SelectedEntity.STATUS != ProjectStatus.TenderSubmitted)
            {
                MessageBoxService.ShowMessage("Project must be a tender to begin tender profiling");
                return;
            }

            if (SelectedEntity.TENDER_PROJECT_START == null)
            {
                MessageBoxService.ShowMessage("Please set tender start date before proceeding");
                return;
            }

            if(SelectedEntity.TENDER_PROJECT_DURATION == null)
            {
                MessageBoxService.ShowMessage("Please set tender duration before proceeding");
                return;
            }

            TENDER_PROFILE_ITEMSelectionViewModelWrapper tenderProfileSelectionViewModel = TENDER_PROFILE_ITEMSelectionViewModelWrapper.Create();
            tenderProfileSelectionViewModel.OnParameterChange(new EntitiesParameter<PROJECT>(SelectedEntity));
            TenderProfileSelectionDialogService.ShowDialog(MessageButton.OK, "Apply tender profile", "TENDER_PROFILESelectionView", tenderProfileSelectionViewModel);
        }

        public void EditArea()
        {
            if (SelectedEntity == null)
                return;

            DocumentInfo DocumentInfo = new DocumentInfo("View_ProjectAreas" + SelectedEntity.GUID.ToString(),
                new EntitiesParameter<PROJECT>(SelectedEntity),
                    "AREACollectionView",
                    "[" + SelectedEntity.NUMBER + "] Areas");

            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }

        public void EditRate()
        {
            if (SelectedEntity == null)
                return;

            DocumentInfo DocumentInfo = new DocumentInfo("View_ProjectRates" + SelectedEntity.GUID.ToString(),
                new EntitiesParameter<PROJECT>(SelectedEntity),
                    "RATECollectionView",
                    "[" + SelectedEntity.NUMBER + "] Rates");

            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }

        public void EditBaseline()
        {
            if (SelectedEntity == null)
                return;

            DocumentInfo DocumentInfo = new DocumentInfo("View_ProjectBaselines" + SelectedEntity.GUID.ToString(),
                new EntitiesParameter<PROJECT>(SelectedEntity),
                    "BASELINECollectionView",
                    "[" + SelectedEntity.NUMBER + "] Baselines");

            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }

        public void EditEstimate()
        {
            if (SelectedEntity == null)
                return;

            DocumentInfo DocumentInfo = new DocumentInfo("View_ProjectEstimates" + SelectedEntity.GUID.ToString(),
                new EntitiesParameter<PROJECT>(SelectedEntity),
                    "ESTIMATECollectionView",
                    "[" + SelectedEntity.NUMBER + "] Estimates");

            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }

        public void EditProgress()
        {
            if (SelectedEntity == null)
                return;

            DocumentInfo DocumentInfo = new DocumentInfo("View_ProjectEstimates" + SelectedEntity.GUID.ToString(),
                new EntitiesParameter<PROJECT>(SelectedEntity),
                    "PROGRESSCollectionView",
                    "[" + SelectedEntity.NUMBER + "] Progresses");

            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }
        #endregion

        #region ISupportCustomDocumentTypeAndParameter

        public bool CanEdit()
        {
            if (SelectedEntity == null)
                return false;

            return true;
        }

        protected IDocumentManagerService DocumentManagerService
        {
            get { return this.GetService<IDocumentManagerService>(); }
        }

        protected override void OnClose(CancelEventArgs e)
        {
            backgroundWorker.CancelAsync();
            base.OnClose(e);
        }

        public void Edit()
        {
            if (SelectedEntity == null)
                return;

            DocumentInfo DocumentInfo = new DocumentInfo(SelectedEntity.GUID.ToString(), 
                new DualEntitiesParameter<PROJECT, Action<object>>(SelectedEntity, navigateCoreCommand), 
                "PROJECTView", 
                "[" + SelectedEntity.NUMBER + "]");

            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }

        public CriteriaOperator ResourceFilterCriteria => CriteriaOperator.Parse("[ROLE.ISMANAGER] In (True)");

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