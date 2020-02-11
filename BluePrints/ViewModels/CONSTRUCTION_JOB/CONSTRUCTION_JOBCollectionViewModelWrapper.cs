using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.View;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Document;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Helpers;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using BluePrints.View;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Grid.DragDrop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Threading;

namespace BluePrints.ViewModels
{
    public class CONSTRUCTION_JOBCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <CONSTRUCTION_JOB, CONSTRUCTION_JOB, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of CONSTRUCTION_JOB_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static CONSTRUCTION_JOBCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new CONSTRUCTION_JOBCollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the PROJECTViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected CONSTRUCTION_JOBCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        private Data.PROJECT loadPROJECT;
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private IUnitOfWorkFactory<IP6EntitiesUnitOfWork> p6UnitOfWorkFactory = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        public string DefaultPhaseInternalNumber { get; set; }
        protected IBluePrintsEntitiesUnitOfWork bluePrintsUnitOfWork;
        IPrimeroEntitiesUnitOfWork primeroUnitOfWork;
        protected override void resolveParameters(object parameter)
        {
            var receiveParameter = (TripleEntitiesParameter<Data.PROJECT, IAmBaseline, object>)parameter;
            loadPROJECT = receiveParameter.GetFirstEntity();

            primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(loadPROJECT.OfficeNameForExo == BluePrintsResources.OfficeMontreal).CreateUnitOfWork();
            bluePrintsUnitOfWork = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
        }

        protected override void addEntitiesLoader()
        {
            //need to reload project even though it came from parameter because we intend to edit it later
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PHASES, PHASEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, COMMODITY_CODEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.SUBJOBS, SUBJOBProjectionFunc);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.WORKPACKS, WORKPACKProjectionFunc);
            loaderCollection.AddLoaderDescription(p6UnitOfWorkFactory, x => x.PROJWBS, P6PROJECTProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.P6_ASSIGNMENTS, P6_ASSIGNMENTProjectionFunc);
        }

        private Func<IRepositoryQuery<PROJWBS>, IQueryable<PROJWBS>> P6PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.proj_node_flag == "Y" && x.wbs_short_name.Contains(loadPROJECT.NUMBER)).OrderBy(proj => proj.wbs_short_name);
        }

        private Func<IRepositoryQuery<P6_ASSIGNMENT>, IQueryable<P6_ASSIGNMENT>> P6_ASSIGNMENTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.TYPE == PhaseType.Construct);
        }

        private Func<IRepositoryQuery<Data.PROJECT>, IQueryable<Data.PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK>> WORKPACKProjectionFunc()
        {
            return query => query.Where(x => x.SUBJOB.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<SUBJOB>, IQueryable<SUBJOB>> SUBJOBProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.PHASE != null);
        }

        private Func<IRepositoryQuery<Data.PHASE>, IQueryable<Data.PHASE>> PHASEProjectionFunc()
        {
            return query => query.Where(x => x.PHASE_TYPE != PhaseType.Design);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>> COMMODITY_CODEProjectionFunc()
        {
            return query => query.Where(x => (x.GUID_PROJECT == loadPROJECT.GUID || x.GUID_PROJECT == null));
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.CONSTRUCTION_JOBS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<CONSTRUCTION_JOB>, IQueryable<CONSTRUCTION_JOB>> specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<CONSTRUCTION_JOB> entities)
        {
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = OnBeforeEntitySaved;
            MainViewModel.OnBeforeEntitiesDeleteIsContinueCallBack = onBeforeEntitiesDeleted;
            MainViewModel.FuncManualRowPastingIsContinue = FuncManualRowPasteAction;
            MainViewModel.UseRegularSplitting = true;
            MainViewModel.AlwaysSkipMessage = true;
            MainViewModel.SetParentViewModel(this);
            AREACollectionViewModel.SetParentViewModel(this);
            SUBJOBSCollectionViewModel.SetParentViewModel(this);
            WORKPACKSCollectionViewModel.SetParentViewModel(this);

            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        public override void OnAfterAuxiliaryEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
            if (changedType == typeof(SUBJOB))
            {
                this.RaisePropertyChanged(x => x.SUBJOBCollection);
            }

            base.OnAfterAuxiliaryEntitiesChanged(key, changedType, messageType, sender, isBulkRefresh);
        }

        /// <summary>
        /// Each estimation entity will need to be assigned to a construction phased subjob and a procurement phased subjob
        /// </summary>
        /// <param name="entity"></param>
        private void onBeforeSaveSubjobAssignment(CONSTRUCTION_JOB entity)
        {
            PhaseType? phaseType = null;
            ChargeType? chargeType = null;
            Data.PHASE chosenPHASE = PHASECollection.FirstOrDefault(x => x.GUID == entity.GUID_PHASE);
            if (chosenPHASE == null)
                return;
            else
                entity.CachedPHASE = chosenPHASE;

            phaseType = chosenPHASE.PHASE_TYPE;
            chargeType = chosenPHASE.CHARGE_TYPE;
            if (phaseType == null || chargeType == null)
                return;

            BluePrintsDataUtils.OnBeforeSavedGenerateAndAssignSubjob(loadPROJECT, PHASECollection, AREACollection, SUBAREACollection, entity, bluePrintsUnitOfWork, phaseType, chargeType);
            IEnumerable<SUBJOB> subJobs = bluePrintsUnitOfWork.SUBJOBS;
            //need to populate subjob for deliverable_name to be present
            if (entity.Subjob_Name == string.Empty && entity.GUID_SUBJOB != null)
                entity.CachedSUBJOB = subJobs.FirstOrDefault(x => x.GUID == entity.GUID_SUBJOB);

            if (entity.Discipline_Code == string.Empty && entity.GUID_DISCIPLINE != null)
                entity.CachedDISCIPLINE = DISCIPLINECollection.FirstOrDefault(x => x.GUID == entity.GUID_DISCIPLINE);
        }

        public Guid FindExistingOrAddNewArea(string areaCode)
        {
            AREA findAREA = AREACollection.FirstOrDefault(x => x.INTERNAL_NUM == areaCode);
            if (findAREA == null)
            {
                AREA newAREA = new AREA();
                newAREA.GUID_PROJECT = loadPROJECT.GUID;
                newAREA.INTERNAL_NUM = areaCode;
                newAREA.TITLE = "Generated";
                AREACollectionViewModel.Save(newAREA);
                findAREA = newAREA;
            }

            return findAREA.GUID;
        }

        public Guid FindExistingOrAddNewSubArea(Guid areaGuid, string subAreaCode)
        {
            AREA findSUBAREA = SUBAREACollection.FirstOrDefault(x => x.GUID_PARENT == areaGuid && x.INTERNAL_NUM == subAreaCode);
            if (findSUBAREA == null)
            {
                AREA newSUBAREA = new AREA();
                newSUBAREA.GUID_PROJECT = loadPROJECT.GUID;
                newSUBAREA.GUID_PARENT = areaGuid;
                newSUBAREA.INTERNAL_NUM = subAreaCode;
                newSUBAREA.TITLE = "Generated";
                AREACollectionViewModel.Save(newSUBAREA);
                findSUBAREA = newSUBAREA;
            }

            return findSUBAREA.GUID;
        }

        public Guid FindExistingOrAddNewDiscipline(string disciplineCode)
        {
            DISCIPLINE findDISCIPLINE = DISCIPLINECollection.FirstOrDefault(x => x.CODE == disciplineCode);
            if(findDISCIPLINE == null)
            {
                findDISCIPLINE = new DISCIPLINE();
                findDISCIPLINE.GUID = Guid.Empty;
                findDISCIPLINE.CODE = disciplineCode;
                findDISCIPLINE.NAME = "Generated";
                DISCIPLINECollectionViewModel.Save(findDISCIPLINE);
            }

            return findDISCIPLINE.GUID;
        }

        public void Align()
        {
            if (MessageBoxService.ShowMessage("This will add EXO jobs that doesn't exist in BluePrints or remove BluePrints jobs that doesn't exists in EXO, do you wish to continue?", "Warning", MessageButton.OKCancel, MessageIcon.Question) != MessageResult.OK)
                return;

            Common.LoadingScreenManager.ShowLoadingScreen(1);
            Common.LoadingScreenManager.SetMessage("Loading EXO jobs...");
            List<ExoTimeAuthorisation> exoLines = ExoQueries.GetProjectLines(primeroUnitOfWork, loadPROJECT.NUMBER);
            P6_ASSIGNMENTCollectionViewModel.Refresh();
            Common.LoadingScreenManager.CloseLoadingScreen();

            List<CONSTRUCTION_JOB> removeCONSTRUCTION_JOBS = new List<CONSTRUCTION_JOB>();
            List<CONSTRUCTION_JOB> optionalRemoveCONSTRUCTION_JOBS = new List<CONSTRUCTION_JOB>();
            List<CONSTRUCTION_JOB> newCONSTRUCTION_JOBS = new List<CONSTRUCTION_JOB>();
            List<ErrorMessage> messages = new List<ErrorMessage>();

            Common.LoadingScreenManager.ShowLoadingScreen(DisplayEntities.Count);
            Common.LoadingScreenManager.SetMessage("Parsing EXO jobs...");
            List<CONSTRUCTION_JOB> entities = DisplayEntities.ToList();

            for(int i=0;i < entities.Count;i++)
            {
                Common.LoadingScreenManager.Progress();
                CONSTRUCTION_JOB displayEntity = entities[i];
                IEnumerable<ExoTimeAuthorisation> findExoLines = exoLines.Where(x => x.SubJobCode == displayEntity.Subjob_Name && x.DisciplineCode == displayEntity.Discipline_Code && x.CommodityCode == displayEntity.Commodity_Code);
                ExoTimeAuthorisation findExoLine;
                if (displayEntity.VARIATION_CODE != null && displayEntity.VARIATION_CODE != string.Empty)
                    findExoLine = findExoLines.FirstOrDefault(x => x.VariationCode == displayEntity.VARIATION_CODE);
                else
                    findExoLine = findExoLines.FirstOrDefault(x => x.VariationCode == string.Empty || x.VariationCode == null);

                if (findExoLine == null)
                {
                    //remove extra jobs in BluePrints that's not in exo
                    messages.Add(new ErrorMessage(displayEntity.UniqueJobcode, "Remove"));
                    removeCONSTRUCTION_JOBS.Add(displayEntity);
                    entities.Remove(displayEntity);
                    i--;
                }
                else
                {
                    //remove duplicates
                    IEnumerable<CONSTRUCTION_JOB> duplicateEntities = DisplayEntities.Where(x => x.GUID != displayEntity.GUID && x.UniqueJobcode == displayEntity.UniqueJobcode);
                    if (duplicateEntities.Count() > 0)
                    {
                        foreach (CONSTRUCTION_JOB duplicateEntity in duplicateEntities)
                        {
                            if (!removeCONSTRUCTION_JOBS.Any(x => x.GUID == duplicateEntity.GUID))
                            {
                                messages.Add(new ErrorMessage(displayEntity.UniqueJobcode, "Remove"));
                                removeCONSTRUCTION_JOBS.Add(displayEntity);
                                //must be removed or else displayEntity will be scanned later and all duplication will be removed
                                entities.Remove(displayEntity);
                            }
                        }
                    }
                }
            }

            foreach (ExoTimeAuthorisation exoLine in exoLines)
            {
                //add jobs in BluePrints
                if (exoLine.SubJobCode.Length >= 15 && exoLine.DisciplineCode.Length >= 4 && exoLine.CommodityCode != string.Empty)
                {
                    string phaseCode = exoLine.SubJobCode.Substring(13, 2);
                    if (!phaseCode.ToUpper().Contains("D") && !phaseCode.ToUpper().Contains("P"))
                    {
                        string disciplineCode = exoLine.DisciplineCode.Substring(0, 2);
                        string disciplineNum = exoLine.DisciplineCode.Substring(2, 2);
                        string areaName = exoLine.SubJobCode.Substring(6, 3);
                        string subAreaName = exoLine.SubJobCode.Substring(10, 2);

                        string fullDisciplineCode = string.Concat(disciplineCode, disciplineNum);
                        string fullWBSCode = exoLine.SubJobCode + "-" + fullDisciplineCode + "-" + exoLine.CommodityCode;

                        CONSTRUCTION_JOB findCONSTRUCTION_JOB = DisplayEntities.FirstOrDefault(x => x.Deliverable_Name.ToUpper() == fullWBSCode.ToUpper() && x.VARIATION_CODE.ToUpper() == exoLine.VariationCode.ToUpper());
                        if (findCONSTRUCTION_JOB == null)
                        {
                            if(findCONSTRUCTION_JOB == null)
                            {
                                CONSTRUCTION_JOB newCONSTRUCTION_JOB = new CONSTRUCTION_JOB();
                                Data.PHASE findPHASE = PHASECollection.FirstOrDefault(x => x.INTERNAL_NUM.ToUpper() == phaseCode);
                                DISCIPLINE findDISCIPLINE = DISCIPLINECollection.FirstOrDefault(x => x.CODE == disciplineCode);
                                int disciplineInt = 1;
                                if (findPHASE != null && Int32.TryParse(disciplineNum, out disciplineInt))
                                {
                                    newCONSTRUCTION_JOB.GUID = Guid.Empty;
                                    newCONSTRUCTION_JOB.GUID_PHASE = findPHASE.GUID;
                                    newCONSTRUCTION_JOB.GUID_AREA = FindExistingOrAddNewArea(areaName);
                                    newCONSTRUCTION_JOB.GUID_SUBAREA = FindExistingOrAddNewSubArea((Guid)newCONSTRUCTION_JOB.GUID_AREA, subAreaName);
                                    newCONSTRUCTION_JOB.GUID_DISCIPLINE = FindExistingOrAddNewDiscipline(disciplineCode);
                                    newCONSTRUCTION_JOB.DISCIPLINE_NUM = disciplineInt;
                                    newCONSTRUCTION_JOB.COMMODITY_CODE = exoLine.CommodityCode;
                                    newCONSTRUCTION_JOB.VARIATION_CODE = exoLine.VariationCode;

                                    //look into the register that's yet to be added because some exo jobs have same commodity code but different stock code
                                    findCONSTRUCTION_JOB = newCONSTRUCTION_JOBS.FirstOrDefault(x => x.GUID_PHASE == findPHASE.GUID && x.GUID_AREA == newCONSTRUCTION_JOB.GUID_AREA && x.GUID_SUBAREA == newCONSTRUCTION_JOB.GUID_SUBAREA && x.GUID_DISCIPLINE == newCONSTRUCTION_JOB.GUID_DISCIPLINE && x.COMMODITY_CODE == newCONSTRUCTION_JOB.COMMODITY_CODE && x.VARIATION_CODE == newCONSTRUCTION_JOB.VARIATION_CODE);
                                    if(findCONSTRUCTION_JOB == null)
                                    {
                                        newCONSTRUCTION_JOBS.Add(newCONSTRUCTION_JOB);
                                        messages.Add(new ErrorMessage(exoLine.SubJobCode + "-" + exoLine.DisciplineCode + "-" + exoLine.CommodityCode + " " + exoLine.VariationCode, "Add"));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Common.LoadingScreenManager.CloseLoadingScreen();
            if(messages.Count > 0)
            {
                DialogCollectionViewModel<ErrorMessage> viewModel = DialogCollectionViewModel<ErrorMessage>.Create(messages, "Do you wish to continue with adding/removing jobs in the following list?");
                if(ErrorMessagesDialogService.ShowDialog(MessageButton.OKCancel, string.Empty, "ListErrorMessages", viewModel) == MessageResult.OK)
                {
                    MainViewModel.BulkSave(newCONSTRUCTION_JOBS);
                    MainViewModel.BaseBulkDelete(removeCONSTRUCTION_JOBS);
                    MessageBoxService.ShowMessage("All job(s) are aligned between BluePrints and EXO", "Congratulation!", MessageButton.OK);

                    FullRefresh();
                }
            }
            else
                MessageBoxService.ShowMessage("All job(s) are aligned between BluePrints and EXO", "Congratulation!", MessageButton.OK);
        }

        public bool FuncManualRowPasteAction(List<KeyValuePair<ColumnBase, string>> pasteData, CONSTRUCTION_JOB pasteEntity, bool isLastRow)
        {
            KeyValuePair<ColumnBase, string> area_data = pasteData.FirstOrDefault(x => x.Key.FieldName == BindableBase.GetPropertyName(() => new CONSTRUCTION_JOB().GUID_AREA));
            KeyValuePair<ColumnBase, string> subarea_data = pasteData.FirstOrDefault(x => x.Key.FieldName == BindableBase.GetPropertyName(() => new CONSTRUCTION_JOB().GUID_SUBAREA));
            KeyValuePair<ColumnBase, string> discipline_data = pasteData.FirstOrDefault(x => x.Key.FieldName == BindableBase.GetPropertyName(() => new CONSTRUCTION_JOB().GUID_DISCIPLINE));
            //KeyValuePair<ColumnBase, string> commodity_data = pasteData.FirstOrDefault(x => x.Key.FieldName == "Entity.Entity.GUID_COMMODITY_CODE");

            if (area_data.Key != null && subarea_data.Key != null)
            {
                if(area_data.Value != string.Empty)
                {
                    Guid areaGuid = FindExistingOrAddNewArea(area_data.Value);
                    pasteEntity.GUID_AREA = areaGuid;

                    Guid subAreaGuid = FindExistingOrAddNewSubArea(areaGuid, subarea_data.Value);
                    pasteEntity.GUID_SUBAREA = subAreaGuid;
                }
            }

            if(discipline_data.Key != null)
            {
                if(discipline_data.Value != string.Empty)
                {
                    Guid disciplineGuid = FindExistingOrAddNewDiscipline(discipline_data.Value);
                    pasteEntity.GUID_DISCIPLINE = disciplineGuid;
                }
            }

            onBeforeSaveSubjobAssignment(pasteEntity);
            BluePrintsDataUtils.OnBeforeSavedGenerateAndAssignWorkpack(pasteEntity, WORKPACKSCollectionViewModel, SUBJOBCollection, DISCIPLINECollection);
            return true;
        }

        protected virtual bool onBeforeEntitiesDeleted(IEnumerable<CONSTRUCTION_JOB> entities)
        {
            List<ErrorMessage> errorMessages = new List<ErrorMessage>();
            List<CONSTRUCTION_JOB> deleteEntities = new List<CONSTRUCTION_JOB>();
            bool showErrorMessage = false;
            foreach (CONSTRUCTION_JOB entity in entities)
            {
                IEnumerable<P6_ASSIGNMENT> attachedP6Assignments = P6_ASSIGNMENTCollection.Where(x => x.GUID_ORIGINAL == entity.GUID);

                //when there are variations that relates to this deliverable
                if (attachedP6Assignments.Count() > 0)
                {
                    string p6AssignmentName = string.Empty;
                    foreach (P6_ASSIGNMENT attachedP6Assignment in attachedP6Assignments)
                    {
                        p6AssignmentName += attachedP6Assignment.P6_ACTIVITYID + ", ";
                    }

                    if (p6AssignmentName.Length > 2)
                        p6AssignmentName = p6AssignmentName.Substring(0, p6AssignmentName.Length - 2);

                    errorMessages.Add(new ErrorMessage(entity.Deliverable_Name, "P6 assignment exists: " + p6AssignmentName));
                    showErrorMessage = true;
                }
                else
                {
                    //errorMessages.Add(new ErrorMessage(entity.Deliverable_Name, "Deleted"));
                    deleteEntities.Add(entity);
                }
            }

            if (showErrorMessage)
            {
                MainViewModel.BaseBulkDelete(deleteEntities);
                DialogCollectionViewModel<ErrorMessage> viewModel = DialogCollectionViewModel<ErrorMessage>.Create(errorMessages, "The following job(s) have P6 assignment(s), do you still wish to delete them?");
                if(ErrorMessagesDialogService.ShowDialog(MessageButton.OKCancel, string.Empty, "ListErrorMessages", viewModel) == MessageResult.Cancel)
                    return false;
            }

            return true;
        }

        #region Collection Call Backs
        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public bool OnBeforeEntitySaved(CONSTRUCTION_JOB entity)
        {
            //if (viewType == DeliverablesViewType.Indirect)
            //    entity.Entity.Entity.BY_DURATION = true;

            onBeforeSaveSubjobAssignment(entity);
            BluePrintsDataUtils.OnBeforeSavedGenerateAndAssignWorkpack(entity, WORKPACKSCollectionViewModel, SUBJOBCollection, DISCIPLINECollection);
            //entity.Entity.Entity.GUID_ESTIMATE = loadESTIMATE.GUID;
            return true;
        }
        #endregion
        #endregion

        #region View Behavior
        #region Duplicate Behavior
        /// <summary>
        /// Show commodity code even when it is not valid
        /// </summary>
        public void CustomColumnDisplayText(CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName.Contains(BindableBase.GetPropertyName(() => new CONSTRUCTION_JOB().COMMODITY_CODE)) && e.Row != null)
            {
                //the itemsource might not have the code so always show the code stored in db
                CONSTRUCTION_JOB projection = (CONSTRUCTION_JOB)e.Row;
                if (!projection.IsCommodityCodeValid && e.DisplayText == string.Empty)
                {
                    e.DisplayText = projection.COMMODITY_CODE;
                }
            }
            else if (e.Column.FieldName.Contains(BindableBase.GetPropertyName(() => new CONSTRUCTION_JOB().GUID_DISCIPLINE)) && e.Row != null)
            {
                CONSTRUCTION_JOB projection = (CONSTRUCTION_JOB)e.Row;
                if (!projection.IsDisciplineCodeValid)
                {
                    DISCIPLINE discipineCode;
                    if (projection.DISCIPLINE != null)
                        discipineCode = projection.DISCIPLINE;
                    else
                        discipineCode = DISCIPLINECollection.FirstOrDefault(x => x.GUID == projection.GUID_DISCIPLINE);

                    if (discipineCode != null)
                        e.DisplayText = discipineCode.CODE;
                    else
                        e.DisplayText = string.Empty;
                }
            }
        }
        #endregion
        public override string UnifiedValueValidation(CONSTRUCTION_JOB projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }

        public override string UnifiedRowValidation(CONSTRUCTION_JOB projection)
        {
            if (MainViewModel != null && MainViewModel.Entities.Where(x => x.GUID != projection.GUID).Any(x => x.UniqueJobcode == projection.UniqueJobcode))
                return "Duplicate entries";

            return string.Empty;
        }

        public void InitNewRow(InitNewRowEventArgs e)
        {
            var gridView = (TableView)e.OriginalSource;
            var grid = gridView.Grid;
            CONSTRUCTION_JOB projection = (CONSTRUCTION_JOB)grid.GetRow(e.RowHandle);
            projection.FullCOMMODITY_CODECollection = COMMODITY_CODECollection;
        }

        public override void UnifiedCellValueChanged(string field_name, object old_value, object new_value, CONSTRUCTION_JOB projection, bool isNew)
        {
            if (field_name.Contains(BindableBase.GetPropertyName(() => new CONSTRUCTION_JOB().GUID_PHASE)))
            {
                projection.FullCOMMODITY_CODECollection = COMMODITY_CODECollection;
                if (projection.GUID_PHASE != null)
                    projection.CachedPHASE = PHASECollection.FirstOrDefault(x => x.GUID == projection.GUID_PHASE);
                else
                    projection.CachedPHASE = null;
            }

            //update anyway for unique job code to show new value
            projection.Update();
            base.UnifiedCellValueChanged(field_name, old_value, new_value, projection, isNew);
        }

        public override void UnifiedCellValueChanging(string field_name, object old_value, object new_value, CONSTRUCTION_JOB projection, bool isNew)
        {
            field_name = DataUtils.FormatColumnFieldname(field_name);
            if (field_name.Contains(BindableBase.GetPropertyName(() => new CONSTRUCTION_JOB().GUID_AREA)))
            {
                Guid? oldValue = projection.GUID_SUBAREA;
                Guid? newValue = (Guid?)null;

                projection.GUID_SUBAREA = newValue;
                if (!isNew)
                {
                    string subAreaFieldName = BindableBase.GetPropertyName(() => new CONSTRUCTION_JOB().SubAreaGuid);
                    
                    MainViewModel.EntitiesUndoRedoManager.PauseActionId();
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, subAreaFieldName, oldValue, newValue, EntityMessageType.Changed);
                }
                else
                {
                    //Area is required immediately for subarea selection
                    if(projection.AREA == null)
                        projection.AREA = AREACollection.FirstOrDefault(x => x.GUID == (Guid)new_value);
                    projection.Update();
                }
            }
            else if (field_name.Contains(BindableBase.GetPropertyName(() => new CONSTRUCTION_JOB().GUID_DISCIPLINE)))
            {
                projection.GUID_DISCIPLINE = (Guid?)new_value;
                projection.Update();
            }
            
            base.UnifiedCellValueChanging(field_name, old_value, new_value, projection, isNew);
        }
        #endregion

        #region stock group Helpers
        private enum projectStock_CodeStatus
        {
            IsEmpty,
            DontExists,
            ExistsWithDifferentRateHours,
            MetaExistsOnDifferentRecord, 
            Exists
        }
        #endregion

        #region View Properties
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            //get { return "CONSTRUCTION_JOBCollectionViewModelWrapper" + view_project_specific_affix; }
            get { return "CONSTRUCTION_JOBCollectionViewModelWrapper_v1" + view_project_specific_affix; }
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

        public IEnumerable<Data.PHASE> PHASECollection
        {
            get
            {
                var collection = GetEntities<Data.PHASE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.INTERNAL_NUM);
                return collection;
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
                var collection = GetEntities<AREA>();
                if (collection != null)
                    collection = collection.Where(x => x.GUID_PARENT != null).OrderBy(x => x.INTERNAL_NUM);
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

        public IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection
        {
            get
            {
                var collection = GetEntities<COMMODITY_CODE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.CODE);
                return collection;
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

        public IEnumerable<P6_ASSIGNMENT> P6_ASSIGNMENTCollection
        {
            get
            {
                return GetEntities<P6_ASSIGNMENT>();
            }
        }

        public IEnumerable<PROJWBS> P6PROJECTSCollection
        {
            get
            {
                var collection = GetEntities<PROJWBS>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.wbs_short_name);
                return collection;
            }
        }

        public CollectionViewModel<P6_ASSIGNMENT, P6_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork> P6_ASSIGNMENTCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<P6_ASSIGNMENT, P6_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<P6_ASSIGNMENT>();
            }
        }

        public CollectionViewModel<Data.PROJECT, Data.PROJECT, Guid, IBluePrintsEntitiesUnitOfWork> PROJECTCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<Data.PROJECT, Data.PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<Data.PROJECT>();
            }
        }

        public CollectionViewModel<AREA, AREA, Guid, IBluePrintsEntitiesUnitOfWork> AREACollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<AREA, AREA, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<AREA>();
            }
        }


        public CollectionViewModel<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork> DISCIPLINECollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<DISCIPLINE>();
            }
        }

        public CollectionViewModel<COMMODITY_CODE, COMMODITY_CODE, Guid, IBluePrintsEntitiesUnitOfWork> COMMODITY_CODECollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<COMMODITY_CODE, COMMODITY_CODE, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<COMMODITY_CODE>();
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

        public string P6ForecastProject
        {
            get
            {
                if (loadPROJECT == null)
                    return string.Empty;

                return loadPROJECT.P6FORECAST_NAME;
            }
            set
            {
                if(PROJECTCollectionViewModel != null && loadPROJECT != null)
                {
                    loadPROJECT.P6FORECAST_NAME = value;
                    PROJECTCollectionViewModel.Save(loadPROJECT);
                }
            }
        }

        protected IDocumentManagerService DocumentManagerService
        {
            get { return this.GetService<IDocumentManagerService>(); }
        }

        public bool CanP6BASELINE_ASSIGN()
        {
            return !IsLoading && loadPROJECT != null && loadPROJECT.P6FORECAST_NAME != null && loadPROJECT.P6FORECAST_NAME != string.Empty;
        }

        public void P6BASELINE_ASSIGN()
        {
            string viewName = "CONSTRUCTION_JOBSchedulingView";
            string tabName = P6ForecastProject + " Mapping";
            DocumentInfo DocumentInfo = new DocumentInfo(tabName, new object[] { loadPROJECT, BaselineMappingSelectionType.Original, loadPROJECT, true }, viewName, tabName);
            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }
        #endregion
    }
}