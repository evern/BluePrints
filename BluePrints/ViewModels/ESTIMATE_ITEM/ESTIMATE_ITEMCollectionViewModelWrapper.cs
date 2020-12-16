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
    public class ESTIMATE_ITEMCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <ESTIMATE_ITEM, ESTIMATE_ITEMProgress, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of ESTIMATE_ITEM_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static ESTIMATE_ITEMCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new ESTIMATE_ITEMCollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the PROJECTViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected ESTIMATE_ITEMCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        private Data.PROJECT loadPROJECT;
        private PROGRESS livePROGRESS;
        private ESTIMATE loadESTIMATE;
        public Guid load_context_guid => loadESTIMATE == null ? Guid.Empty : loadESTIMATE.GUID;
        private bool isQueryForLiveStatus;
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        public string Base_Entity_String => "Entity.Entity.";
        public string Projection_Entity_String => "Entity.";

        public string DefaultPhaseInternalNumber { get; set; }
        public Func<ESTIMATE_ITEMProgress> SelectedEntityCallBack { get; set; }
        public IEnumerable<ESTIMATE_ITEMProgress> EditableAllEntities { get; set; }
        public bool IsProcurementSubjobVisible { get; set; }
        private IUnitOfWorkFactory<IP6EntitiesUnitOfWork> p6UnitOfWorkFactory = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected IBluePrintsEntitiesUnitOfWork bluePrintsUnitOfWork;
        protected override void resolveParameters(object parameter)
        {
            Interface_InitializeParameters(parameter);
        }

        IPrimeroEntitiesUnitOfWork primeroUnitOfWork;
        public void Interface_InitializeParameters(object parameter)
        {
            var receiveParameter = (TripleEntitiesParameter<Data.PROJECT, IAmBaseline, object>)parameter;
            loadPROJECT = receiveParameter.GetFirstEntity();
            loadESTIMATE = (ESTIMATE)receiveParameter.GetSecondEntity();

            primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(loadPROJECT.OfficeNameForExo == BluePrintsResources.OfficeMontreal).CreateUnitOfWork();
            bluePrintsUnitOfWork = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            IsProcurementSubjobVisible = false;
            if (loadPROJECT != null)
                isQueryForLiveStatus = true;
        }

        protected override void addEntitiesLoader()
        {
            //need to reload project even though it came from parameter because we intend to edit it later
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PHASES, PHASEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.ESTIMATES, ESTIMATEProjectionFunc, x => assign_estimation_direct(x));
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc, x => assign_progress(x));
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESS_ITEMS, PROGRESS_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.STOCK_CODES, STOCK_CODEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.STOCK_GROUPS, STOCK_GROUPProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, COMMODITY_CODEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.SUBJOBS, SUBJOBProjectionFunc);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc);
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

        private void assign_estimation_direct(ESTIMATE estimation_direct)
        {
            bool createLiveEstimate = false;
            if (estimation_direct == null && !SupressCompulsoryEntityNotFoundMessage)
                createLiveEstimate = true;

            if ((estimation_direct != null && estimation_direct.STATUS == BaselineStatus.Working))
                createLiveEstimate = true;

            if(createLiveEstimate)
            {
                IBluePrintsEntitiesUnitOfWork bluePrintsUnitOfWork = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
                ESTIMATE newESTIMATE_DIRECT = new ESTIMATE();
                newESTIMATE_DIRECT.GUID_PROJECT = loadPROJECT.GUID;
                newESTIMATE_DIRECT.NAME = loadPROJECT.NUMBER + "_001";
                newESTIMATE_DIRECT.REVISION = "A";
                newESTIMATE_DIRECT.STATUS = BaselineStatus.Live;
                bluePrintsUnitOfWork.ESTIMATES.Add(newESTIMATE_DIRECT);
                bluePrintsUnitOfWork.SaveChanges();

                loadESTIMATE = newESTIMATE_DIRECT;
                stopSubsequentEntitiesLoading = true;
                FullRefresh();
            }
            else
                loadESTIMATE = estimation_direct;
        }

        private void assign_progress(PROGRESS progress)
        {
            if (progress == null && !SupressCompulsoryEntityNotFoundMessage)
            {
                IBluePrintsEntitiesUnitOfWork bluePrintsUnitOfWork = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
                PROGRESS newConstructionPROGRESS = new PROGRESS();
                newConstructionPROGRESS.GUID_PROJECT = loadPROJECT.GUID;
                newConstructionPROGRESS.NAME = loadPROJECT.NUMBER + "DAILY_001";
                newConstructionPROGRESS.PROGRESS_START = DateTime.Now;
                newConstructionPROGRESS.DATA_DATE = CommonMethods.StartOfWeek(newConstructionPROGRESS.PROGRESS_START, DayOfWeek.Sunday).AddDays(1).AddSeconds(-1);
                newConstructionPROGRESS.INTERVAL_COUNT = 1;
                newConstructionPROGRESS.INTERVAL_TYPE = ProgressIntervalType.Daily;
                newConstructionPROGRESS.STATUS = ProgressStatus.Live;
                newConstructionPROGRESS.TYPE = PhaseType.Construct;
                bluePrintsUnitOfWork.PROGRESSES.Add(newConstructionPROGRESS);
                bluePrintsUnitOfWork.SaveChanges();

                livePROGRESS = newConstructionPROGRESS;
                stopSubsequentEntitiesLoading = true;
                FullRefresh();
            }
            else
                livePROGRESS = progress;
        }

        private Func<IRepositoryQuery<Data.PROJECT>, IQueryable<Data.PROJECT>> PROJECTProjectionFunc()
        {
            if (isQueryForLiveStatus)
                return query => query.Where(x => x.GUID == loadPROJECT.GUID);
            else
                return query => query.Where(x => x.GUID == loadESTIMATE.GUID_PROJECT);
        }

        private Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK>> WORKPACKProjectionFunc()
        {
            return query => query.Where(x => x.SUBJOB.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> PROGRESSProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.TYPE == PhaseType.Construct && x.STATUS == ProgressStatus.Live);
        }

        private Func<IRepositoryQuery<PROGRESS_ITEM>, IQueryable<PROGRESS_ITEM>> PROGRESS_ITEMProjectionFunc()
        {
            if (livePROGRESS == null)
                return query => query.Where(x => x.GUID_PROGRESS == Guid.Empty);
            else
                return query => query.Where(x => x.GUID_PROGRESS == livePROGRESS.GUID);
        }

        private Func<IRepositoryQuery<ESTIMATE>, IQueryable<ESTIMATE>> ESTIMATEProjectionFunc()
        {
            if (isQueryForLiveStatus)
                return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && (x.STATUS == BaselineStatus.Live));
            else
                return query => query.Where(x => x.GUID == loadESTIMATE.GUID);
        }

        private Func<IRepositoryQuery<SUBJOB>, IQueryable<SUBJOB>> SUBJOBProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.PHASE != null);
        }

        private Func<IRepositoryQuery<STOCK_CODE>, IQueryable<STOCK_CODE>> STOCK_CODEProjectionFunc()
        {
            return query => query.Include(x => x.PROJECT);
        }

        private Func<IRepositoryQuery<Data.PHASE>, IQueryable<Data.PHASE>> PHASEProjectionFunc()
        {
            return query => query.Where(x => x.PHASE_TYPE != PhaseType.Design);
        }

        private Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadESTIMATE.PROJECT.GUID);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadESTIMATE.PROJECT.GUID);
        }

        private Func<IRepositoryQuery<STOCK_GROUP>, IQueryable<STOCK_GROUP>> STOCK_GROUPProjectionFunc()
        {
            return query => query.Where(x => (x.GUID_PROJECT == loadPROJECT.GUID || x.GUID_PROJECT == null));
        }

        private Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>> COMMODITY_CODEProjectionFunc()
        {
            return query => query.Where(x => (x.GUID_PROJECT == loadPROJECT.GUID || x.GUID_PROJECT == null));
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.ESTIMATE_ITEMS);
        }

        protected override Func<IRepositoryQuery<ESTIMATE_ITEM>, IQueryable<ESTIMATE_ITEMProgress>>
            specifyMainViewModelProjection()
        {
            return query => ESTIMATE_ITEMProjectionQueries.IDeliverable_Progress_Transformation(base_entity_query(query), loadPROJECT, loaderCollection.GetCollection<RATE>(), livePROGRESS, PROGRESS_ITEMCollection, false, STOCK_CODECollection, loaderCollection.GetCollection<STOCK_GROUP>(), null, false, null, false, COMMODITY_CODECollection);
        }

        public Func<IRepositoryQuery<ESTIMATE_ITEM>, IQueryable<ESTIMATE_ITEM>> BaseEntityQueryCallBack { get; set; }
        private IQueryable<ESTIMATE_ITEM> base_entity_query(IRepositoryQuery<ESTIMATE_ITEM> query)
        {
            if (BaseEntityQueryCallBack != null)
                return BaseEntityQueryCallBack(query);

            return query.Where(x => x.GUID_ESTIMATE == load_context_guid);
        }

        public Action<ESTIMATE_ITEMProgress, string, object, object, EntityMessageType> InterfaceAddUndoRedoCallBack { get; set; }
        public void AddUndo(ESTIMATE_ITEMProgress changedEntity, string propertyName, object oldValue, object newValue, EntityMessageType messageType)
        {
            if (InterfaceAddUndoRedoCallBack != null)
                InterfaceAddUndoRedoCallBack(changedEntity, propertyName, oldValue, newValue, messageType);
            else
            {
                if (propertyName == null)
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(changedEntity, null, null, null, messageType);
                else
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(changedEntity, localizeColumnFieldName(propertyName), oldValue, newValue, messageType);
            }
        }

        public Action InterfacePauseUndoRedoCallBack { get; set; }
        public void PauseUndoRedo()
        {
            if (InterfacePauseUndoRedoCallBack != null)
                InterfacePauseUndoRedoCallBack();
            else
                MainViewModel.EntitiesUndoRedoManager.PauseActionId();
        }

        public Action InterfaceUnpauseUndoRedoCallBack { get; set; }
        public void UnpauseUndoRedo()
        {
            if (InterfaceUnpauseUndoRedoCallBack != null)
                InterfaceUnpauseUndoRedoCallBack();
            else
                MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
        }

        private string localizeColumnFieldName(string fieldName)
        {
            return Base_Entity_String + DataUtils.FormatColumnFieldname(fieldName);
        }

        public Action<IEnumerable<ESTIMATE_ITEMProgress>> OnReportablesLoadedCallBack { get; set; }
        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<ESTIMATE_ITEMProgress> entities)
        {
            //MainViewModel.DisablePasting = true;
            MainViewModel.FuncManualRowPastingIsContinue = FuncManualRowPasteAction;
            MainViewModel.UseRegularSplitting = true;
            MainViewModel.AlwaysSkipMessage = true;
            MainViewModel.SetParentViewModel(this);
            GetAllEntities = () => { return MainViewModel.Entities; };
            STOCK_CODECollectionViewModel.SetParentViewModel(this);
            AREACollectionViewModel.SetParentViewModel(this);
            SUBJOBSCollectionViewModel.SetParentViewModel(this);
            WORKPACKSCollectionViewModel.SetParentViewModel(this);

            base.AssignCallBacksAndRaisePropertyChange(entities);

            //used for interface when this is loaded from variation
            if (OnReportablesLoadedCallBack != null)
            {
                OnReportablesLoadedCallBack(entities);
                return;
            }

            SetViewSpecificProperties();
        }

        public override void OnAfterAuxiliaryEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender, Guid senderKey, bool isBulkRefresh)
        {
            if (changedType == typeof(SUBJOB))
            {
                this.RaisePropertyChanged(x => x.SUBJOBCollection);
                this.RaisePropertyChanged(x => x.ConstructionPHASECollection);
                this.RaisePropertyChanged(x => x.ProcurementSUBJOBCollection);
            }

            base.OnAfterAuxiliaryEntitiesChanged(key, changedType, messageType, sender, senderKey, isBulkRefresh);
        }

        /// <summary>
        /// Each estimation entity will need to be assigned to a construction phased subjob and a procurement phased subjob
        /// </summary>
        /// <param name="entity"></param>
        private void onBeforeSavedDualSubjobAssignment(ESTIMATE_ITEMProgress entity)
        {
            PhaseType? phaseType = null;
            ChargeType? chargeType = null;
            Data.PHASE chosenPHASE = PHASECollection.FirstOrDefault(x => x.GUID == entity.Entity.Entity.GUID_PHASE);
            if (chosenPHASE == null)
                return;
            else
                entity.Entity.Entity.CachedPHASE = chosenPHASE;

            phaseType = chosenPHASE.PHASE_TYPE;
            chargeType = chosenPHASE.CHARGE_TYPE;
            if (phaseType == null || chargeType == null)
                return;

            BluePrintsDataUtils.OnBeforeSavedGenerateAndAssignSubjob(loadPROJECT, PHASECollection, AREACollection, SUBAREACollection, entity, bluePrintsUnitOfWork, phaseType, chargeType);
            IEnumerable<SUBJOB> subJobs = bluePrintsUnitOfWork.SUBJOBS;
            //need to populate subjob for deliverable_name to be present
            if (entity.Entity.Entity.Subjob_Name == string.Empty && entity.Entity.Entity.GUID_SUBJOB != null)
                entity.Entity.Entity.CachedSUBJOB = subJobs.FirstOrDefault(x => x.GUID == entity.Entity.Entity.GUID_SUBJOB);

            if (entity.Entity.Entity.Discipline_Code == string.Empty && entity.Entity.Entity.GUID_DISCIPLINE != null)
                entity.Entity.Entity.CachedDISCIPLINE = DISCIPLINECollection.FirstOrDefault(x => x.GUID == entity.Entity.Entity.GUID_DISCIPLINE);
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

        public bool CanAlign()
        {
            return !IsLoading;
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

            List<ESTIMATE_ITEMProgress> removeESTIMATE_ITEMS = new List<ESTIMATE_ITEMProgress>();
            List<ESTIMATE_ITEMProgress> optionalRemoveESTIMATE_ITEMS = new List<ESTIMATE_ITEMProgress>();
            List<ESTIMATE_ITEMProgress> newESTIMATE_ITEMS = new List<ESTIMATE_ITEMProgress>();
            List<ErrorMessage> messages = new List<ErrorMessage>();

            Common.LoadingScreenManager.ShowLoadingScreen(Entities.Count);
            Common.LoadingScreenManager.SetMessage("Parsing EXO jobs...");
            List<ESTIMATE_ITEMProgress> entities = Entities.ToList();

            for(int i=0;i < entities.Count;i++)
            {
                Common.LoadingScreenManager.Progress();
                ESTIMATE_ITEMProgress displayEntity = entities[i];
                IEnumerable<ExoTimeAuthorisation> findExoLines = exoLines.Where(x => x.SubJobCode == displayEntity.Subjob_Name && x.DisciplineCode == displayEntity.Discipline_Code && x.CommodityCode == displayEntity.Commodity_Code);
                ExoTimeAuthorisation findExoLine;
                if (displayEntity.Variation_Code != null && displayEntity.Variation_Code != string.Empty)
                    findExoLine = findExoLines.FirstOrDefault(x => x.VariationCode == displayEntity.Variation_Code);
                else
                    findExoLine = findExoLines.FirstOrDefault(x => x.VariationCode == string.Empty || x.VariationCode == null);

                if (findExoLine == null)
                {
                    //remove extra jobs in BluePrints that's not in exo
                    messages.Add(new ErrorMessage(displayEntity.UniqueJobcode, "Remove"));
                    removeESTIMATE_ITEMS.Add(displayEntity);
                    entities.Remove(displayEntity);
                    i--;
                }
                else
                {
                    //remove duplicates
                    IEnumerable<ESTIMATE_ITEMProgress> duplicateEntities = Entities.Where(x => x.UniqueJobcode == displayEntity.UniqueJobcode);
                    if (duplicateEntities.Count() > 1)
                    {
                        List<ESTIMATE_ITEMProgress> removeEntities = new List<ESTIMATE_ITEMProgress>();
                        foreach (ESTIMATE_ITEMProgress duplicateEntity in duplicateEntities)
                        {
                            //remove all but one
                            if(removeEntities.Count < duplicateEntities.Count() - 1)
                            {
                                //only try to remove when P6 assignment isn't found
                                string errorMessage = getP6AssignmentErrorMessage(duplicateEntity);
                                if (errorMessage == string.Empty)
                                {
                                    removeEntities.Add(duplicateEntity);
                                }
                            }
                        }

                        foreach(ESTIMATE_ITEMProgress removeEntity in removeEntities)
                        {
                            messages.Add(new ErrorMessage(removeEntity.UniqueJobcode, "Remove"));
                            removeESTIMATE_ITEMS.Add(removeEntity);
                            //must be removed or else displayEntity will be scanned later and all duplication will be removed
                            entities.Remove(removeEntity);
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

                        ESTIMATE_ITEMProgress findESTIMATE_ITEM = null;
                        if (exoLine.VariationCode == null || exoLine.VariationCode == string.Empty)
                            findESTIMATE_ITEM = Entities.FirstOrDefault(x => x.Deliverable_Name.ToUpper() == fullWBSCode.ToUpper());
                        else
                            findESTIMATE_ITEM = Entities.Where(x => x.Variation_Code != null).FirstOrDefault(x => x.Deliverable_Name.ToUpper() == fullWBSCode.ToUpper() && x.Variation_Code.ToUpper() == exoLine.VariationCode.ToUpper());
                        
                        if (findESTIMATE_ITEM == null)
                        {
                            ESTIMATE_ITEM newESTIMATE_ITEM = new ESTIMATE_ITEM();
                            Data.PHASE findPHASE = PHASECollection.FirstOrDefault(x => x.INTERNAL_NUM.ToUpper() == phaseCode);
                            DISCIPLINE findDISCIPLINE = DISCIPLINECollection.FirstOrDefault(x => x.CODE == disciplineCode);
                            int disciplineInt = 1;
                            if (findPHASE != null && Int32.TryParse(disciplineNum, out disciplineInt))
                            {
                                newESTIMATE_ITEM.GUID = Guid.Empty;
                                newESTIMATE_ITEM.GUID_PHASE = findPHASE.GUID;
                                newESTIMATE_ITEM.GUID_AREA = FindExistingOrAddNewArea(areaName);
                                newESTIMATE_ITEM.GUID_SUBAREA = FindExistingOrAddNewSubArea((Guid)newESTIMATE_ITEM.GUID_AREA, subAreaName);
                                newESTIMATE_ITEM.GUID_DISCIPLINE = FindExistingOrAddNewDiscipline(disciplineCode);
                                newESTIMATE_ITEM.DISCIPLINE_NUM = disciplineInt;
                                newESTIMATE_ITEM.COMMODITY_CODE = exoLine.CommodityCode;
                                newESTIMATE_ITEM.VARIATION_CODE = exoLine.VariationCode;

                                ESTIMATE_ITEMProgress projection = new ESTIMATE_ITEMProgress();
                                projection.Entity = new ESTIMATE_ITEMProjection();
                                projection.Entity.Entity = newESTIMATE_ITEM;

                                //look into the register that's yet to be added because some exo jobs have same commodity code but different stock code
                                findESTIMATE_ITEM = newESTIMATE_ITEMS.FirstOrDefault(x => x.Entity.Entity.GUID_PHASE == findPHASE.GUID && x.Entity.Entity.GUID_AREA == newESTIMATE_ITEM.GUID_AREA && x.Entity.Entity.GUID_SUBAREA == newESTIMATE_ITEM.GUID_SUBAREA && x.Entity.Entity.GUID_DISCIPLINE == newESTIMATE_ITEM.GUID_DISCIPLINE && x.Entity.Entity.COMMODITY_CODE == newESTIMATE_ITEM.COMMODITY_CODE && x.Entity.Entity.VARIATION_CODE == newESTIMATE_ITEM.VARIATION_CODE);
                                if(findESTIMATE_ITEM == null)
                                {
                                    newESTIMATE_ITEMS.Add(projection);
                                    messages.Add(new ErrorMessage(exoLine.SubJobCode + "-" + exoLine.DisciplineCode + "-" + exoLine.CommodityCode + " " + exoLine.VariationCode, "Add"));
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
                    MainViewModel.BaseBulkSave(newESTIMATE_ITEMS);
                    MainViewModel.BaseBulkDelete(removeESTIMATE_ITEMS);

                    FullRefresh();
                    MessageBoxService.ShowMessage("All job(s) are aligned between BluePrints and EXO", "Congratulation!", MessageButton.OK);
                }
            }
            else
                MessageBoxService.ShowMessage("All job(s) are aligned between BluePrints and EXO", "Congratulation!", MessageButton.OK);
        }

        public bool FuncManualRowPasteAction(List<KeyValuePair<ColumnBase, string>> pasteData, ESTIMATE_ITEMProgress pasteEntity, bool isLastRow)
        {
            string searchStockCodeFieldName;
            if (IsBudget)
                searchStockCodeFieldName = BindableBase.GetPropertyName(() => new ESTIMATE_ITEMProgress().Entity.Budget_StockCodeGuid);
            else
                searchStockCodeFieldName = BindableBase.GetPropertyName(() => new ESTIMATE_ITEMProgress().Entity.Estimate_StockCodeGuid);

            KeyValuePair<ColumnBase, string> stock_code_data = pasteData.FirstOrDefault(x => x.Key.FieldName.Contains(searchStockCodeFieldName));

            KeyValuePair<ColumnBase, string> area_data = pasteData.FirstOrDefault(x => x.Key.FieldName == "Entity.Entity.GUID_AREA");
            KeyValuePair<ColumnBase, string> subarea_data = pasteData.FirstOrDefault(x => x.Key.FieldName == "Entity.Entity.SubAreaGuid");
            KeyValuePair<ColumnBase, string> discipline_data = pasteData.FirstOrDefault(x => x.Key.FieldName == "Entity.Entity.GUID_DISCIPLINE");
            //KeyValuePair<ColumnBase, string> commodity_data = pasteData.FirstOrDefault(x => x.Key.FieldName == "Entity.Entity.GUID_COMMODITY_CODE");

            if (area_data.Key != null && subarea_data.Key != null)
            {
                if(area_data.Value != string.Empty)
                {
                    Guid areaGuid = FindExistingOrAddNewArea(area_data.Value);
                    pasteEntity.Entity.Entity.GUID_AREA = areaGuid;

                    Guid subAreaGuid = FindExistingOrAddNewSubArea(areaGuid, subarea_data.Value);
                    pasteEntity.Entity.Entity.GUID_SUBAREA = subAreaGuid;
                }
            }

            if(discipline_data.Key != null)
            {
                if(discipline_data.Value != string.Empty)
                {
                    Guid disciplineGuid = FindExistingOrAddNewDiscipline(discipline_data.Value);
                    pasteEntity.Entity.Entity.GUID_DISCIPLINE = disciplineGuid;
                }
            }

            if (stock_code_data.Key != null)
            {
                KeyValuePair<ColumnBase, string> supply_rate_data = pasteData.FirstOrDefault(x => x.Key.FieldName.Contains(BindableBase.GetPropertyName(() => new ESTIMATE_ITEMProgress().Entity.ESTIMATE_STOCK_CODE.RATE_SUPPLY)));
                KeyValuePair<ColumnBase, string> install_rate_data = pasteData.FirstOrDefault(x => x.Key.FieldName.Contains(BindableBase.GetPropertyName(() => new ESTIMATE_ITEMProgress().Entity.ESTIMATE_STOCK_CODE.HOURS_INSTALL)));

                if (supply_rate_data.Key != null && install_rate_data.Key != null)
                {
                    Regex rgx = new Regex(BluePrintsResources.Regex_NumbersOnly);
                    string clean_supply_rate = rgx.Replace(supply_rate_data.Value, string.Empty);
                    string clean_install_rate = rgx.Replace(install_rate_data.Value, string.Empty);
                    if (clean_supply_rate == string.Empty)
                        clean_supply_rate = "0";

                    if (clean_install_rate == string.Empty)
                        clean_install_rate = "0";

                    decimal supply_value;
                    decimal install_value;
                    bool rate_result = decimal.TryParse(clean_supply_rate, out supply_value);
                    bool install_result = decimal.TryParse(clean_install_rate, out install_value);

                    if(rate_result && install_result)
                    {
                        STOCK_CODE project_stock_code = ProjectSTOCK_CODECollection.FirstOrDefault(x => x.CODE == stock_code_data.Value && x.RATE_SUPPLY == supply_value && x.HOURS_INSTALL == install_value);
                        STOCK_CODE editing_stock_code;
                        if (IsBudget)
                            editing_stock_code = pasteEntity.Entity.BUDGET_STOCK_CODE;
                        else
                            editing_stock_code = pasteEntity.Entity.ESTIMATE_STOCK_CODE;

                        if (project_stock_code != null)
                        {
                            editing_stock_code = project_stock_code;
                            if(IsBudget)
                            {
                                pasteEntity.Entity.Budget_StockCodeGuid = project_stock_code.GUID;
                                pasteEntity.Entity.Entity.GUID_BUDGET_STOCK_CODE = project_stock_code.GUID;
                            }
                            else
                            {
                                pasteEntity.Entity.Estimate_StockCodeGuid = project_stock_code.GUID;
                                pasteEntity.Entity.Entity.GUID_ESTIMATE_STOCK_CODE = project_stock_code.GUID;
                            }
                        }
                        else
                        {
                            string fieldNameVar = "ESTIMATE";
                            if (IsBudget)
                                fieldNameVar = "BUDGET";

                            KeyValuePair<ColumnBase, string> uom_data = pasteData.FirstOrDefault(x => x.Key.FieldName.Contains("Entity." + fieldNameVar + "_STOCK_CODE.UOM"));
                            KeyValuePair<ColumnBase, string> name_data = pasteData.FirstOrDefault(x => x.Key.FieldName.Contains("Entity." + fieldNameVar + "_STOCK_CODE.NAME"));
                            KeyValuePair<ColumnBase, string> type_data = pasteData.FirstOrDefault(x => x.Key.FieldName.Contains("Entity." + fieldNameVar + "_STOCK_CODE.TYPE"));
                            KeyValuePair<ColumnBase, string> spec_data = pasteData.FirstOrDefault(x => x.Key.FieldName.Contains("Entity." + fieldNameVar + "_STOCK_CODE.SPEC"));
                            KeyValuePair<ColumnBase, string> desc_data = pasteData.FirstOrDefault(x => x.Key.FieldName.Contains("Entity." + fieldNameVar + "_STOCK_CODE.DESCRIPTION"));
                            editing_stock_code.CODE = stock_code_data.Value;
                            editing_stock_code.UOM = uom_data.Value;
                            editing_stock_code.NAME = name_data.Value;
                            editing_stock_code.TYPE = type_data.Value;
                            editing_stock_code.SPEC = spec_data.Value;
                            editing_stock_code.DESCRIPTION = desc_data.Value;

                            Guid discipline_guid = Guid.Empty;
                            if (pasteEntity.Entity.Entity.GUID_DISCIPLINE == null)
                                discipline_guid = DISCIPLINECollection.First().GUID;
                            else
                                discipline_guid = (Guid)pasteEntity.Entity.Entity.GUID_DISCIPLINE;

                            editing_stock_code.GUID_DISCIPLINE = discipline_guid;

                            //use global stock code as original guid
                            STOCK_CODE from_stock_code = STOCK_CODECollection.FirstOrDefault(x => x.CODE == stock_code_data.Value);

                            if (from_stock_code != null)
                                editing_stock_code.GUID_ORIGINAL = from_stock_code.GUID;

                            if(editing_stock_code.NAME != string.Empty)
                            {
                                if (IsBudget)
                                {
                                    pasteEntity.Entity.Budget_StockCodeGuid = createNewSTOCK_CODE(editing_stock_code);
                                    pasteEntity.Entity.Entity.GUID_BUDGET_STOCK_CODE = pasteEntity.Entity.Budget_StockCodeGuid;
                                }
                                else
                                {
                                    pasteEntity.Entity.Estimate_StockCodeGuid = createNewSTOCK_CODE(editing_stock_code);
                                    pasteEntity.Entity.Entity.GUID_ESTIMATE_STOCK_CODE = pasteEntity.Entity.Estimate_StockCodeGuid;
                                }
                            }
                        }
                    }
                }
            }

            onBeforeSavedDualSubjobAssignment(pasteEntity);
            BluePrintsDataUtils.OnBeforeSavedGenerateAndAssignWorkpack(pasteEntity, WORKPACKSCollectionViewModel, SUBJOBCollection, DISCIPLINECollection);
            return true;
        }

        /// <summary>
        /// this view model can be used in variation or default collection view, only default collection view specific properties are set here
        /// </summary>
        private void SetViewSpecificProperties()
        {
            //When this is not called externally as a nested wrapper
            if(MainViewModel != null)
            {
                EditableAllEntities = MainViewModel.Entities;
                DefaultPhaseInternalNumber = BluePrintsResources.Default_Construction_Phase;
            }
        }

        protected override bool OnBeforeApplyingProjectionPropertiesToEntityIsContinue(ESTIMATE_ITEMProgress projection, ESTIMATE_ITEM entity)
        {
            //because TProjection is not IProjection<TMainEntity>, do it manually here
            DataUtils.ShallowCopy(entity, projection.Entity.Entity);
            return false;
        }

        protected override OperationInterceptMode OnBeforeProjectionDeleteIsContinue(ESTIMATE_ITEMProgress projection, out List<ErrorMessage> errorMessages)
        {
            errorMessages = new List<ErrorMessage>();

            string p6AssignmentErroMessage = getP6AssignmentErrorMessage(projection);

            if(p6AssignmentErroMessage != string.Empty)
                errorMessages.Add(new ErrorMessage(projection.Deliverable_Name, p6AssignmentErroMessage));

            return OperationInterceptMode.Continue;
        }

        private string getP6AssignmentErrorMessage(ESTIMATE_ITEMProgress projection)
        {
            IEnumerable<P6_ASSIGNMENT> attachedP6Assignments = P6_ASSIGNMENTCollection.Where(x => x.GUID_ORIGINAL == projection.OriginalEntityKey);

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

                return "P6 assignment exists: " + p6AssignmentName;
            }

            return string.Empty;
        }

        #region Collection Call Backs
        private void createAndAssignProjectSpecificSTOCK_CODE(ESTIMATE_ITEMProgress projectionEntity)
        {
            Guid? stockcodeGuid;
            if (IsBudget)
                stockcodeGuid = projectionEntity.Entity.Entity.GUID_BUDGET_STOCK_CODE;
            else
                stockcodeGuid = projectionEntity.Entity.Entity.GUID_ESTIMATE_STOCK_CODE;

            if (stockcodeGuid == null)
                return;

            if(!ProjectSTOCK_CODECollection.Any(x => x.GUID == stockcodeGuid))
            {
                STOCK_CODE stock_code = STOCK_CODECollection.FirstOrDefault(x => x.GUID == stockcodeGuid);
                if (stock_code != null)
                {
                    STOCK_CODE newSTOCK_CODE = new STOCK_CODE();
                    DataUtils.ShallowCopy(newSTOCK_CODE, stock_code);
                    newSTOCK_CODE.GUID = Guid.Empty;
                    newSTOCK_CODE.GUID_PROJECT = loadPROJECT.GUID;
                    newSTOCK_CODE.STOCK_CODE_TYPE = IsBudget ? StockCodeType.Budget : StockCodeType.Estimate;
                    STOCK_CODECollectionViewModel.Save(newSTOCK_CODE);

                    if(IsBudget)
                    {
                        projectionEntity.Entity.Entity.GUID_BUDGET_STOCK_CODE = newSTOCK_CODE.GUID;
                        //stock group is by value with shallow copy in setter so that user edited changes can be cross check with existing project stock group
                        projectionEntity.Entity.BUDGET_STOCK_CODE = newSTOCK_CODE;
                    }
                    else
                    {
                        projectionEntity.Entity.Entity.GUID_ESTIMATE_STOCK_CODE = newSTOCK_CODE.GUID;
                        //stock group is by value with shallow copy in setter so that user edited changes can be cross check with existing project stock group
                        projectionEntity.Entity.ESTIMATE_STOCK_CODE = newSTOCK_CODE;
                    }
                }
                else
                    return;
            }

            return;
        }

        private Guid createNewSTOCK_CODE(STOCK_CODE fromStockCode)
        {
            STOCK_CODE newStockCode = new STOCK_CODE();
            DataUtils.ShallowCopy(newStockCode, fromStockCode);
            newStockCode.GUID = Guid.Empty;
            newStockCode.GUID_PROJECT = loadPROJECT.GUID;
            newStockCode.GUID_ORIGINAL = fromStockCode.GUID_ORIGINAL;
            newStockCode.STOCK_CODE_TYPE = IsBudget ? StockCodeType.Budget : StockCodeType.Estimate;
            STOCK_CODECollectionViewModel.Save(newStockCode);
            return newStockCode.GUID;
        }

        private void updateSTOCK_CODE(STOCK_CODE stock_code)
        {
            if (stock_code.GUID == Guid.Empty)
                return;

            STOCK_CODECollectionViewModel.Save(stock_code);
        }

        protected override OperationInterceptMode OnBeforeProjectionSaveIsContinue(ESTIMATE_ITEMProgress projection, out bool isNew)
        {
            projection.Entity.Entity.GUID_ESTIMATE = load_context_guid;

            onBeforeSavedDualSubjobAssignment(projection);
            onBeforeSavedProjectStockCodeLogging(projection);
            BluePrintsDataUtils.OnBeforeSavedGenerateAndAssignWorkpack(projection, WORKPACKSCollectionViewModel, SUBJOBCollection, DISCIPLINECollection);
            return base.OnBeforeProjectionSaveIsContinue(projection, out isNew);
        }

        protected override void OnAfterProjectionSave(ESTIMATE_ITEMProgress projection, ESTIMATE_ITEM entity, bool isNew)
        {
            projection.Entity.Entity.GUID_ORIGINAL = entity.GUID_ORIGINAL;
            if (isNew)
                OnAfterDuplicateCallBack?.Invoke(projection);
        }

        bool neverAskAndEdit;
        bool neverAskAndAdd;
        private void onBeforeSavedProjectStockCodeLogging(ESTIMATE_ITEMProgress entity)
        {
            STOCK_CODE editingSTOCK_CODE;
            if (IsBudget)
                editingSTOCK_CODE = entity.Entity.BUDGET_STOCK_CODE;
            else
                editingSTOCK_CODE = entity.Entity.ESTIMATE_STOCK_CODE;

            if (editingSTOCK_CODE != null)
            {
                STOCK_CODE projectStockCode;
                projectStock_CodeStatus commodityCodeStatus = getProjectStockCodeStatus(editingSTOCK_CODE, out projectStockCode);
                if (commodityCodeStatus == projectStock_CodeStatus.IsEmpty)
                    return;
                else if (commodityCodeStatus == projectStock_CodeStatus.DontExists)
                    createAndAssignProjectSpecificSTOCK_CODE(entity);
                else if (commodityCodeStatus == projectStock_CodeStatus.MetaExistsOnDifferentRecord)
                {
                    if (IsBudget)
                        entity.Entity.Entity.GUID_BUDGET_STOCK_CODE = projectStockCode.GUID;
                    else
                        entity.Entity.Entity.GUID_ESTIMATE_STOCK_CODE = projectStockCode.GUID;

                    editingSTOCK_CODE = projectStockCode;
                }
                else if (commodityCodeStatus == projectStock_CodeStatus.ExistsWithDifferentRateHours)
                {
                    UICommand addCommand = new UICommand()
                    {
                        Id = DialogAction.Add,
                        Caption = "Add",
                        IsCancel = true,
                        IsDefault = false,
                    };

                    UICommand editCommand = new UICommand()
                    {
                        Id = DialogAction.Edit,
                        Caption = "Update",
                        IsCancel = true,
                        IsDefault = false,
                    };

                    UICommand cancelCommand = new UICommand()
                    {
                        Id = DialogAction.Cancel,
                        Caption = "Cancel",
                        IsCancel = true,
                        IsDefault = false,
                    };

                    if(neverAskAndEdit)
                        updateSTOCK_CODE(editingSTOCK_CODE);
                    else if(neverAskAndAdd)
                        createNewStockCode(entity, editingSTOCK_CODE);
                    else
                    {
                        //string message = String.Format("Current stock code with\nSupply Rate: {0:#.##} Install Hours: {1:#.##} UOM: {2}\n\n" + "Is changed to\nSupply Rate: {3:#.##} Install Hours: {4:#.##} UOM: {5}\n\n" + "Do you wish to add new or update?\n\n", projectStockCode.RATE_SUPPLY, projectStockCode.HOURS_INSTALL, projectStockCode.UOM, editingSTOCK_CODE.RATE_SUPPLY, editingSTOCK_CODE.HOURS_INSTALL, editingSTOCK_CODE.UOM);
                        string message = String.Format("Supply rate, install hours or UOM changed\nDo you wish to edit stock or add as a new stock?", projectStockCode.RATE_SUPPLY, projectStockCode.HOURS_INSTALL, projectStockCode.UOM, editingSTOCK_CODE.RATE_SUPPLY, editingSTOCK_CODE.HOURS_INSTALL, editingSTOCK_CODE.UOM);

                        BasicMessageBoxViewModel viewModel = BasicMessageBoxViewModel.Create(message);
                        viewModel.CheckboxVisibility = Visibility.Hidden;
                        UICommand result = StockCodeDialogService.ShowDialog(new List<UICommand>() { addCommand, editCommand, cancelCommand }, "Stock Code", "BasicMessageBox", viewModel);
                        if (result == addCommand)
                        {
                            createNewStockCode(entity, editingSTOCK_CODE);
                            neverAskAndAdd = viewModel.IsChecked;
                        }
                        else if (result == editCommand)
                        {
                            updateSTOCK_CODE(editingSTOCK_CODE);
                            neverAskAndEdit = viewModel.IsChecked;
                        }
                    }
                }
                //improve performance, don't need to update stock code when nothing is changed
                //else if (commodityCodeStatus == projectStock_CodeStatus.Exists)
                //    updateSTOCK_CODE(editingSTOCK_CODE);
            }
        }

        private void createNewStockCode(ESTIMATE_ITEMProgress entity, STOCK_CODE editingSTOCK_CODE)
        {
            Guid newStockCodeGuid = createNewSTOCK_CODE(editingSTOCK_CODE);
            if (IsBudget)
                entity.Entity.Entity.GUID_BUDGET_STOCK_CODE = newStockCodeGuid;
            else
                entity.Entity.Entity.GUID_ESTIMATE_STOCK_CODE = newStockCodeGuid;
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
            if (e.Column.FieldName.Contains(BindableBase.GetPropertyName(() => new ESTIMATE_ITEMProgress().Entity.Entity.COMMODITY_CODE)) && e.Row != null)
            {
                //the itemsource might not have the code so always show the code stored in db
                ESTIMATE_ITEMProgress projection = (ESTIMATE_ITEMProgress)e.Row;
                if (!projection.Entity.Entity.IsCommodityCodeValid && e.DisplayText == string.Empty)
                {
                    e.DisplayText = projection.Entity.Entity.COMMODITY_CODE;
                }
            }
            else if (e.Column.FieldName.Contains(BindableBase.GetPropertyName(() => new ESTIMATE_ITEMProgress().Entity.Entity.GUID_DISCIPLINE)) && e.Row != null)
            {
                ESTIMATE_ITEMProgress projection = (ESTIMATE_ITEMProgress)e.Row;
                if (!projection.Entity.Entity.IsDisciplineCodeValid)
                {
                    DISCIPLINE discipineCode;
                    if (projection.Entity.Entity.DISCIPLINE != null)
                        discipineCode = projection.Entity.Entity.DISCIPLINE;
                    else
                        discipineCode = DISCIPLINECollection.FirstOrDefault(x => x.GUID == projection.Entity.Entity.GUID_DISCIPLINE);

                    if (discipineCode != null)
                        e.DisplayText = discipineCode.CODE;
                    else
                        e.DisplayText = string.Empty;
                }
            }
        }
        #endregion

        //allows raise property change to propagate to parent
        public Action<object> RaisePropertyChangeCallBack { get; set; }
        protected override bool IsSingleMainEntityRefreshIdentified(object key, Type changedType, EntityMessageType messageType, object sender, Guid senderKey, bool isBulkRefresh)
        {
            if (changedType == typeof(STOCK_CODE))
            {
                this.RaisePropertyChanged(x => x.STOCK_CODECollection);
                STOCK_CODE changedStock_Code = STOCK_CODECollection.FirstOrDefault(x => x.GUID == (Guid)key);
                if(changedStock_Code != null)
                {
                    foreach (var entity in Entities)
                    {
                        if(IsBudget)
                        {
                            if (entity.Entity.Entity.GUID_BUDGET_STOCK_CODE == (Guid)key)
                                entity.Entity.BUDGET_STOCK_CODE = changedStock_Code;
                        }
                        else
                        {
                            if (entity.Entity.Entity.GUID_ESTIMATE_STOCK_CODE == (Guid)key)
                                entity.Entity.ESTIMATE_STOCK_CODE = changedStock_Code;
                        }

                        entity.Update();
                    }

                    GridControlService.RefreshData();
                    return true;
                }
            }

            return false;
        }

        public override string UnifiedRowValidation(ESTIMATE_ITEMProgress projection)
        {
            if (MainViewModel != null && MainViewModel.Entities.Where(x => x.GUID != projection.GUID).Any(x => x.UniqueJobcode == projection.UniqueJobcode))
                return "Duplicate entries";

            return string.Empty;
        }

        public override string UnifiedValueValidation(ESTIMATE_ITEMProgress projection, string field_name, object new_value, bool isPaste)
        {
            string fieldName = DataUtils.FormatColumnFieldname(field_name);
            //budgeted hours field is disabled but just in case
            if (fieldName == BindableBase.GetPropertyName(() => new ESTIMATE_ITEM().PROGRESS_TYPE))
            {
                if (projection.Entity.Entity.STOCK_CODE == null)
                {
                    EstimateProgressType newValue = (EstimateProgressType)new_value;
                    if (newValue != EstimateProgressType.Standalone)
                    {
                        return "Cannot set " + newValue.ToString() + " when stock code is empty";
                    }
                }
                else if (projection.Entity.Entity.GUID_STOCK_GROUP != null)
                {
                    STOCK_GROUP entity_stock_group = STOCK_GROUPCollection.FirstOrDefault(x => x.GUID == projection.Entity.Entity.GUID_STOCK_GROUP);
                    if (entity_stock_group != null)
                    {
                        if ((projection.Entity.Entity.STOCK_CODE.UOM != entity_stock_group.UOM) && ((EstimateProgressType)new_value) == EstimateProgressType.Trackable)
                        {
                            return "Cannot set trackable when UOM is different from stock group";
                        }
                    }
                }
                else if (projection.Entity.Entity.GUID_STOCK_GROUP == null)
                {
                    EstimateProgressType newValue = (EstimateProgressType)new_value;
                    if (newValue != EstimateProgressType.Standalone)
                    {
                        return "Cannot set " + newValue.ToString() + " when stock group is empty";
                    }
                }
            }
            else if (fieldName == BindableBase.GetPropertyName(() => new ESTIMATE_ITEM().GUID_STOCK_GROUP))
            {
                if (projection.Entity.Entity.PROGRESS_TYPE == EstimateProgressType.Trackable && new_value != null)
                {
                    STOCK_GROUP entity_commodity_code = STOCK_GROUPCollection.FirstOrDefault(x => x.GUID == (Guid)new_value);
                    if (entity_commodity_code != null)
                    {
                        if ((projection.Entity.Entity.STOCK_CODE.UOM != entity_commodity_code.UOM))
                        {
                            return "Cannot set a stock group with different UOM than stock code when deliverable is trackable";
                        }
                    }
                }
            }
            else if (fieldName == BindableBase.GetPropertyName(() => new ESTIMATE_ITEMProgress().Entity.Estimate_StockCodeGuid))
            {
                if (projection.Entity.Entity.PROGRESS_TYPE == EstimateProgressType.Trackable && new_value != null)
                {
                    STOCK_GROUP entity_commodity_code = STOCK_GROUPCollection.FirstOrDefault(x => x.GUID == projection.Entity.Entity.GUID_STOCK_GROUP);
                    STOCK_CODE entity_stock_code = STOCK_CODECollection.FirstOrDefault(x => x.GUID == (Guid)new_value);
                    if (entity_stock_code != null && entity_commodity_code != null)
                    {
                        if ((entity_commodity_code.UOM != entity_stock_code.UOM))
                        {
                            return "Cannot set a stock code with different UOM than stock group when deliverable is trackable";
                        }
                    }
                }
            }

            return string.Empty;
        }

        public override void InitNewRow(InitNewRowEventArgs e)
        {
            var gridView = (TableView)e.OriginalSource;
            var grid = gridView.Grid;
            ESTIMATE_ITEMProgress projection = (ESTIMATE_ITEMProgress)grid.GetRow(e.RowHandle);
            UnifiedNewRowInitializationFromView(projection);
        }

        public override void UnifiedNewRowInitializationFromView(ESTIMATE_ITEMProgress projection)
        {
            projection.Entity.Entity.FullCOMMODITY_CODECollection = COMMODITY_CODECollection;
            base.UnifiedNewRowInitializationFromView(projection);
        }

        public override void UnifiedCellValueChanged(string field_name, object old_value, object new_value, ESTIMATE_ITEMProgress projection, bool isNew)
        {
            if (field_name.Contains(BindableBase.GetPropertyName(() => new ESTIMATE_ITEM().GUID_PHASE)))
            {
                projection.Entity.Entity.FullCOMMODITY_CODECollection = COMMODITY_CODECollection;
                if (projection.Entity.Entity.GUID_PHASE != null)
                    projection.Entity.Entity.CachedPHASE = PHASECollection.FirstOrDefault(x => x.GUID == projection.Entity.Entity.GUID_PHASE);
                else
                    projection.Entity.Entity.CachedPHASE = null;
            }

            //update anyway for unique job code to show new value
            projection.Update();
            base.UnifiedCellValueChanged(field_name, old_value, new_value, projection, isNew);
        }

        public override void UnifiedCellValueChanging(string field_name, object old_value, object new_value, ESTIMATE_ITEMProgress projection, bool isNew)
        {
            field_name = DataUtils.FormatColumnFieldname(field_name);
            if (field_name.Contains(BindableBase.GetPropertyName(() => new ESTIMATE_ITEM().GUID_AREA)))
            {
                Guid? oldValue = projection.Entity.Entity.GUID_SUBAREA;
                Guid? newValue = (Guid?)null;

                projection.Entity.Entity.GUID_SUBAREA = newValue;
                if (!isNew)
                {
                    string subAreaFieldName = BindableBase.GetPropertyName(() => new ESTIMATE_ITEM().SubAreaGuid);
                    PauseUndoRedo();
                    AddUndo(projection, subAreaFieldName, oldValue, newValue, EntityMessageType.Changed);
                }
                else
                {
                    //Area is required immediately for subarea selection
                    if(projection.Entity.Entity.AREA == null)
                        projection.Entity.Entity.AREA = AREACollection.FirstOrDefault(x => x.GUID == (Guid)new_value);
                    projection.Update();
                }
            }
            else if (field_name.Contains(BindableBase.GetPropertyName(() => new ESTIMATE_ITEM().GUID_DISCIPLINE)))
            {
                //discipline and commodity code collection is required immediately for subarea selection
                projection.Entity.Entity.GUID_DISCIPLINE = (Guid?)new_value;
                updateProjectionStockCodeCollection(projection, (Guid?)new_value);
                projection.Update();
            }
            //set default commodity code when stock code is changed
            else if (field_name.Contains(BindableBase.GetPropertyName(() => new ESTIMATE_ITEMProgress().Entity.Estimate_StockCodeGuid)))
            {
                if (new_value != null)
                {
                    Guid? commodityCodeGuid = null;
                    setProjectionEstimateStockCode(projection, (Guid)new_value, out commodityCodeGuid);
                    Guid? oldValue = projection.Entity.Entity.GUID_COMMODITY_CODE;
                    Guid? newValue = commodityCodeGuid;
                    projection.Entity.Entity.GUID_COMMODITY_CODE = newValue;

                    if (!isNew)
                    {
                        string commodity_code_field_name = Base_Entity_String + BindableBase.GetPropertyName(() => new ESTIMATE_ITEM().GUID_COMMODITY_CODE);
                        PauseUndoRedo();
                        AddUndo(projection, commodity_code_field_name, oldValue, newValue, EntityMessageType.Changed);
                    }
                    else
                        projection.Update();
                }
            }
            //set default discipline when commodity code is changed
            else if (field_name.Contains(BindableBase.GetPropertyName(() => new ESTIMATE_ITEMProgress().Entity.Entity.GUID_COMMODITY_CODE)))
            {
                if (new_value != null)
                {
                    COMMODITY_CODE entity_commodity_code = COMMODITY_CODECollection.FirstOrDefault(x => x.GUID == (Guid)new_value);
                    if (entity_commodity_code != null)
                    {
                        Guid? oldValue = projection.Entity.Entity.GUID_DISCIPLINE;
                        Guid? newValue = entity_commodity_code.GUID_DISCIPLINE;
                        projection.Entity.Entity.GUID_DISCIPLINE = newValue;
                        //need to set immediately for new row to display selection due to CustomColumnDisplayText event
                        projection.Entity.Entity.GUID_COMMODITY_CODE = (Guid?)new_value;
                        if (!isNew)
                        {
                            string discipline_field_name = Base_Entity_String + BindableBase.GetPropertyName(() => new ESTIMATE_ITEM().GUID_DISCIPLINE);
                            PauseUndoRedo();
                            AddUndo(projection, discipline_field_name, oldValue, newValue, EntityMessageType.Changed);
                        }
                        else
                            projection.Update();
                    }
                }
            }
            //set stock group to null when progress type is changed
            else if (field_name.Contains(BindableBase.GetPropertyName(() => new ESTIMATE_ITEMProgress().Entity.Entity.PROGRESS_TYPE)))
            {
                EstimateProgressType progress_Type = (EstimateProgressType)new_value;
                if (progress_Type == EstimateProgressType.Standalone)
                {
                    Guid? oldValue = projection.Entity.Entity.GUID_STOCK_GROUP;
                    Guid? newValue = null;
                    projection.Entity.Entity.GUID_STOCK_GROUP = newValue;
                    string stock_group_fieldname = Base_Entity_String + BindableBase.GetPropertyName(() => new ESTIMATE_ITEM().GUID_STOCK_GROUP);
                    PauseUndoRedo();
                    AddUndo(projection, stock_group_fieldname, oldValue, newValue, EntityMessageType.Changed);
                    projection.Update();
                }
            }
            //set progress type to standalone when stock group is changed
            else if (field_name.Contains(BindableBase.GetPropertyName(() => new ESTIMATE_ITEMProgress().Entity.Entity.GUID_STOCK_GROUP)))
            {
                if (new_value == null)
                {
                    EstimateProgressType oldValue = projection.Entity.Entity.PROGRESS_TYPE;
                    EstimateProgressType newValue = EstimateProgressType.Standalone;
                    projection.Entity.Entity.PROGRESS_TYPE = newValue;
                    string progress_type_fieldname = Base_Entity_String + BindableBase.GetPropertyName(() => new ESTIMATE_ITEM().PROGRESS_TYPE);
                    PauseUndoRedo();
                    AddUndo(projection, progress_type_fieldname, oldValue, newValue, EntityMessageType.Changed);
                    projection.Update();
                }
            }

            base.UnifiedCellValueChanging(field_name, old_value, new_value, projection, isNew);
        }

        private void setProjectionEstimateStockCode(ESTIMATE_ITEMProgress projection, Guid? stockCodeGuid, out Guid? commodityCodeGuid)
        {
            STOCK_CODE findSTOCK_CODE = STOCK_CODECollection.FirstOrDefault(x => x.GUID == (Guid)stockCodeGuid);
            if (stockCodeGuid != null)
            {
                projection.Entity.ESTIMATE_STOCK_CODE = findSTOCK_CODE;
                commodityCodeGuid = findSTOCK_CODE.GUID_COMMODITY_CODE;
            }
            else
            {
                projection.Entity.ESTIMATE_STOCK_CODE = null;
                commodityCodeGuid = null;
            }

            projection.Update();
        }

        private void updateProjectionStockCodeCollection(ESTIMATE_ITEMProgress projection, Guid? disciplineGuid)
        {
            if (disciplineGuid != null)
                //stock code collection must be updated by discipline filter
                projection.Entity.StockCodeCollection = STOCK_CODECollection.Where(x => x.GUID_DISCIPLINE == disciplineGuid);
            else
                projection.Entity.StockCodeCollection = new List<STOCK_CODE>();

            projection.Update();
        }

        private void resetProjectionSubArea(ESTIMATE_ITEMProgress projection)
        {
            Guid? oldValue = projection.Entity.Entity.GUID_SUBAREA;
            if (oldValue != null)
            {
                Guid? newValue = (Guid?)null;
                string subAreaFieldName = BindableBase.GetPropertyName(() => new ESTIMATE_ITEM().SubAreaGuid);
                projection.Entity.Entity.GUID_SUBAREA = newValue;
                PauseUndoRedo();
                AddUndo(projection, subAreaFieldName, oldValue, newValue, EntityMessageType.Changed);
            }
        }

        private void resetProjectionCommodityCode(ESTIMATE_ITEMProgress projection)
        {
            Guid? oldValue = projection.Entity.Entity.GUID_COMMODITY_CODE;
            if (oldValue != null)
            {
                Guid? newValue = (Guid?)null;
                string commoditycodeFieldName = Base_Entity_String + BindableBase.GetPropertyName(() => new ESTIMATE_ITEM().GUID_COMMODITY_CODE);
                projection.Entity.Entity.GUID_COMMODITY_CODE = newValue;
                PauseUndoRedo();
                AddUndo(projection, commoditycodeFieldName, oldValue, newValue, EntityMessageType.Changed);
            }
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

        private projectStock_CodeStatus getProjectStockCodeStatus(STOCK_CODE stock_code, out STOCK_CODE projectStock_Code)
        {
            projectStock_Code = null;
            if (stock_code == null)
                return projectStock_CodeStatus.IsEmpty;

            projectStock_Code = ProjectSTOCK_CODECollection.FirstOrDefault(x => x.GUID == stock_code.GUID);
            bool isExists = false;
            if (projectStock_Code != null)
                isExists = true;

            if (isExists && projectStock_Code.CODE == stock_code.CODE && projectStock_Code.RATE_SUPPLY == stock_code.RATE_SUPPLY && projectStock_Code.HOURS_INSTALL == stock_code.HOURS_INSTALL && projectStock_Code.UOM == stock_code.UOM)
                return projectStock_CodeStatus.Exists;

            //look for other project stock group with same meta
            STOCK_CODE sameMetaStockCode = ProjectSTOCK_CODECollection.FirstOrDefault(x => x.CODE == stock_code.CODE && x.RATE_SUPPLY == stock_code.RATE_SUPPLY && x.HOURS_INSTALL == stock_code.HOURS_INSTALL && x.UOM == stock_code.UOM);
            if (isExists && sameMetaStockCode == null)
                return projectStock_CodeStatus.ExistsWithDifferentRateHours;

            if (sameMetaStockCode != null)
            {
                projectStock_Code = sameMetaStockCode;
                return projectStock_CodeStatus.MetaExistsOnDifferentRecord;
            }

            return projectStock_CodeStatus.DontExists;
        }

        #endregion

        public void Save(ESTIMATE_ITEMProgress progress_entity)
        {
            MainViewModel.Save(progress_entity);
        }

        public void BulkSave(IEnumerable<ESTIMATE_ITEMProgress> progress_entities)
        {
            MainViewModel.BaseBulkSave(progress_entities, true);
        }

        public void Delete(ESTIMATE_ITEMProgress progress_entity)
        {
            MainViewModel.Delete(progress_entity);
        }

        public bool CanAutoPopulate(object button)
        {
            if (SelectedEntities == null || SelectedEntities.Count() == 0)
                return false;

            return true;
        }

        public void AutoPopulate(object button)
        {
            //MainViewModel.isBackgroundEdit = true;
            //PauseUndoRedo();
            //var info = GridPopupMenuBase.GetGridMenuInfo((DependencyObject)button) as GridMenuInfo;
            //if (info.Column == null)
            //    return;

            //List<ESTIMATE_ITEMProgress> entitiesToSave = new List<ESTIMATE_ITEMProgress>();
            //if(info.Column.FieldName == "Entity.Entity.GUID_COMMODITY_CODE")
            //{
            //    foreach(var entity in SelectedEntities)
            //    {
            //        STOCK_CODE stockCode = null;
            //        if (IsBudget)
            //        {
            //            if (entity.Entity.BUDGET_STOCK_CODE != null)
            //                stockCode = entity.Entity.BUDGET_STOCK_CODE;
            //        }
            //        else
            //        {
            //            if (entity.Entity.ESTIMATE_STOCK_CODE != null)
            //                stockCode = entity.Entity.ESTIMATE_STOCK_CODE;
            //        }

            //        if(stockCode != null)
            //        {
            //            COMMODITY_CODE findCOMMODITY_CODE = COMMODITY_CODECollection.FirstOrDefault(x => x.CODE == stockCode.CODE);
            //            if (findCOMMODITY_CODE != null)
            //                entity.Entity.Entity.GUID_COMMODITY_CODE = findCOMMODITY_CODE.GUID;
            //            else
            //            {
            //                COMMODITY_CODE newCOMMODITY_CODE = new COMMODITY_CODE();
            //                newCOMMODITY_CODE.GUID_PROJECT = loadPROJECT.GUID;
            //                if (entity.Discipline_Guid != null)
            //                    newCOMMODITY_CODE.GUID_DISCIPLINE = entity.Discipline_Guid;
            //                newCOMMODITY_CODE.CODE = stockCode.CODE;
            //                newCOMMODITY_CODE.DESCRIPTION = "Auto Populate";
            //                newCOMMODITY_CODE.UOM = entity.Entity.BUDGET_STOCK_CODE.UOM;
            //                newCOMMODITY_CODE.PHASE_TYPE = entity.Entity.Entity.PhaseType == null;
            //                COMMODITY_CODECollectionViewModel.Save(newCOMMODITY_CODE);
            //                entity.Entity.Entity.GUID_COMMODITY_CODE = newCOMMODITY_CODE.GUID;
            //                entitiesToSave.Add(entity);
            //            }
            //        }
            //    }
            //}


            //MainViewModel.BulkSave(entitiesToSave);
            //MainViewModel.isBackgroundEdit = false;
            //UnpauseUndoRedo();
            //BackgroundRefresh();
        }


        #region DragDrop
        public void TableView_Drop(GridDropEventArgs e)
        {
            e.Handled = true;
        }

        public void TableView_Dropped(GridDroppedEventArgs e)
        {
            IEnumerable<ESTIMATE_ITEMProgress> sources = ((IEnumerable<object>)e.DraggedRows).Select(x => (ESTIMATE_ITEMProgress)x).AsEnumerable();
            ESTIMATE_ITEMProgress target = (ESTIMATE_ITEMProgress)e.TargetRow;

            if (target.Entity.Entity.PROGRESS_TYPE == EstimateProgressType.Auto)
                return;

            PauseUndoRedo();
            List<ESTIMATE_ITEMProgress> saveItems = new List<ESTIMATE_ITEMProgress>();
            string parentFieldName = BindableBase.GetPropertyName(() => new ESTIMATE_ITEMProgress().Entity.Entity.GUID_PARENT);
            string progressTypeFieldName = BindableBase.GetPropertyName(() => new ESTIMATE_ITEMProgress().Entity.Entity.PROGRESS_TYPE);
            if (sources.Count() > 0 && target != null)
            {
                Guid newValue = target.OriginalEntityKey;
                foreach (var source in sources)
                {
                    Guid? oldValue = source.Entity.Entity.GUID_PARENT;
                    source.Entity.Entity.GUID_PARENT = newValue;
                    source.Entity.Entity.PROGRESS_TYPE = EstimateProgressType.Auto;
                    AddUndo(source, parentFieldName, oldValue, newValue, EntityMessageType.Changed);
                    AddUndo(source, progressTypeFieldName, EstimateProgressType.Standalone, EstimateProgressType.Auto, EntityMessageType.Changed);
                    saveItems.Add(source);
                }

                target.Entity.Entity.PROGRESS_TYPE = EstimateProgressType.Trackable;
                AddUndo(target, progressTypeFieldName, EstimateProgressType.Standalone, EstimateProgressType.Trackable, EntityMessageType.Changed);
                saveItems.Add(target);
            }

            UnpauseUndoRedo();
            MainViewModel.BaseBulkSave(saveItems);
        }
        #endregion

        public bool CanFindReplace(object button)
        {
            return false;
        }

        public void FindReplace(object button)
        {
        }

        #region View Properties
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            //get { return "ESTIMATE_ITEMCollectionViewModelWrapper" + view_project_specific_affix; }
            get { return "ESTIMATE_ITEMCollectionViewModelWrapper_v2" + view_project_specific_affix; }
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

        private DevExpress.Mvvm.IDialogService StockCodeDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("StockCodeDialogService"); }
        }

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMCollection
        {
            get
            {
                return GetEntities<PROGRESS_ITEM>();
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

        public IEnumerable<Data.PHASE> ConstructionPHASECollection
        {
            get
            {
                var collection = GetEntities<Data.PHASE>();
                if (collection != null)
                    collection = collection.Where(x => x.PHASE_TYPE == PhaseType.Construct).OrderBy(x => x.INTERNAL_NUM);
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

        public IEnumerable<SUBJOB> ProcurementSUBJOBCollection
        {
            get
            {
                var collection = GetEntities<SUBJOB>();
                //Cannot filter by PHASE TYPE because upon first addition navigational properties for Collection cannot be obtained
                if (collection != null)
                    collection = collection.Where(x => x.PHASE != null && x.PHASE.PHASE_TYPE == PhaseType.Procurement).OrderBy(x => x.INTERNAL_NAME1);
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

        public IEnumerable<STOCK_GROUP> STOCK_GROUPCollection
        {
            get
            {
                var collection = GetEntities<STOCK_GROUP>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.CODE);
                return collection;
            }
        }

        public IEnumerable<STOCK_CODE> ProjectSTOCK_CODECollection
        {
            get
            {
                if (loadPROJECT == null)
                    return null;

                if(IsBudget)
                    return STOCK_CODECollection.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STOCK_CODE_TYPE == StockCodeType.Budget).OrderBy(x => x.CODE);
                else
                    return STOCK_CODECollection.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STOCK_CODE_TYPE == StockCodeType.Estimate).OrderBy(x => x.CODE);
            }
        }

        public IEnumerable<STOCK_CODE> STOCK_CODECollection
        {
            get
            {
                var collection = GetEntities<STOCK_CODE>();
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

        public IEnumerable<STOCK_CODE> GlobalSTOCK_CODECollection
        {
            get
            {
                var collection = GetEntities<STOCK_CODE>();
                if (collection != null)
                    collection = collection.Where(x => x.GUID_PROJECT == null).OrderBy(x => x.CODE);
                return collection;
            }
        }

        public IEnumerable<STOCK_GROUP> ProjectSTOCK_GROUPCollection
        {
            get
            {
                var collection = GetEntities<STOCK_GROUP>();
                if (collection != null)
                    collection = collection.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).OrderBy(x => x.CODE);
                return collection;
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

        public CollectionViewModel<PROGRESS, PROGRESS, Guid, IBluePrintsEntitiesUnitOfWork> PROGRESSCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<PROGRESS, PROGRESS, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<PROGRESS>();
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

        public CollectionViewModel<ESTIMATE, ESTIMATE, Guid, IBluePrintsEntitiesUnitOfWork> ESTIMATECollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<ESTIMATE, ESTIMATE, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<ESTIMATE>();
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

        public CollectionViewModel<STOCK_CODE, STOCK_CODE, Guid, IBluePrintsEntitiesUnitOfWork> STOCK_CODECollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<STOCK_CODE, STOCK_CODE, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<STOCK_CODE>();
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

        public CollectionViewModel<STOCK_GROUP, STOCK_GROUP, Guid, IBluePrintsEntitiesUnitOfWork> STOCK_GROUPCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<STOCK_GROUP, STOCK_GROUP, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<STOCK_GROUP>();
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

        public Func<IEnumerable<ESTIMATE_ITEMProgress>> GetEditableAllEntitiesCallBack { get; set; }

        private bool IsBudget => loadESTIMATE.STATUS == BaselineStatus.Live;

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
            string viewName = "BUDGET_ITEMSchedulingView";
            string tabName = P6ForecastProject + " Mapping";
            DocumentInfo DocumentInfo = new DocumentInfo(tabName, new object[] { loadPROJECT, BaselineMappingSelectionType.Original, loadPROJECT, true }, viewName, tabName);
            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }

        public bool InVariationMode { get; set; }
        public Func<IEnumerable<ESTIMATE_ITEMProgress>> GetAllEntities { get; set; }
        public Action<ESTIMATE_ITEMProgress> OnAfterDuplicateCallBack { get; set; }
        #endregion
    }
}