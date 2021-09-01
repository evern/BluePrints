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
    public class ESTIMATE_ITEMCollectionViewModelWrapper : BluePrintsEntitiesCollectionWrapper<ESTIMATE_ITEM, ESTIMATE_ITEMProgress, Guid, IBluePrintsEntitiesUnitOfWork>
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
        List<ExoTimeAuthorisation> exoLines;
        public void Interface_InitializeParameters(object parameter)
        {
            var receiveParameter = (TripleEntitiesParameter<Data.PROJECT, IAmBaseline, object>)parameter;
            loadPROJECT = receiveParameter.GetFirstEntity();
            loadESTIMATE = (ESTIMATE)receiveParameter.GetSecondEntity();

            primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(loadPROJECT.OfficeNameForExo).CreateUnitOfWork();
            bluePrintsUnitOfWork = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            exoLines = ExoQueries.GetProjectLines(primeroUnitOfWork, loadPROJECT.NUMBER);
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
                newConstructionPROGRESS.DATA_DATE = CommonMethods.GetStartOfWeek(newConstructionPROGRESS.PROGRESS_START, DayOfWeek.Sunday).AddDays(1).AddSeconds(-1);
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
            return query => ESTIMATE_ITEMProjectionQueries.IDeliverable_Progress_Transformation(base_entity_query(query), loadPROJECT, loaderCollection.GetCollection<RATE>(), livePROGRESS, PROGRESS_ITEMCollection, false, null, false, null, false, COMMODITY_CODECollection);
        }

        private IQueryable<ESTIMATE_ITEM> base_entity_query(IRepositoryQuery<ESTIMATE_ITEM> query)
        {
            List<ESTIMATE_ITEM> ESTIMATE_ITEMS = query.Where(x => x.GUID_ESTIMATE == load_context_guid).ToList();
            ESTIMATE_ITEMS.ForEach(x => x.ExoLines = exoLines);

            return ESTIMATE_ITEMS.AsQueryable();
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
            MainViewModel.UseRegularSplitting = true;
            MainViewModel.AlwaysSkipMessage = true;
            MainViewModel.SetParentViewModel(this);
            GetAllEntities = () => { return MainViewModel.Entities; };
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
        protected override OperationInterceptMode OnBeforeProjectionSaveIsContinue(ESTIMATE_ITEMProgress projection, out bool isNew)
        {
            projection.Entity.Entity.GUID_ESTIMATE = load_context_guid;

            BluePrintsDataUtils.OnBeforeSavedGenerateAndAssignSubjob(loadPROJECT, PHASECollection, AREACollection, SUBAREACollection, projection, bluePrintsUnitOfWork, PhaseType.Construct, ChargeType.Chargeable);
            BluePrintsDataUtils.OnBeforeSavedGenerateAndAssignWorkpack(loadPROJECT, projection, WORKPACKSCollectionViewModel, SUBJOBCollection, DISCIPLINECollection);

            return base.OnBeforeProjectionSaveIsContinue(projection, out isNew);
        }

        protected override void OnAfterProjectionSave(ESTIMATE_ITEMProgress projection, ESTIMATE_ITEM entity, bool isNew)
        {
            projection.Entity.Entity.GUID_ORIGINAL = entity.GUID_ORIGINAL;
            if (isNew)
                OnAfterDuplicateCallBack?.Invoke(projection);
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
        public override string UnifiedRowValidation(ESTIMATE_ITEMProgress projection)
        {
            if (MainViewModel != null && MainViewModel.Entities.Where(x => x.GUID != projection.GUID).Any(x => x.UniqueJobcode == projection.UniqueJobcode))
                return "Duplicate entries";
            else if (projection.Entity.Entity.VARIATION_CODE != null && projection.Entity.Entity.VARIATION_CODE != string.Empty)
            {
                if (!VariationCodeStringCollection.Any(x => x == projection.Entity.Entity.VARIATION_CODE))
                    return "Invalid variation code";
            }

            return string.Empty;
        }

        public override string UnifiedValueValidation(ESTIMATE_ITEMProgress projection, string field_name, object new_value, bool isPaste)
        {
            if (field_name.Contains(BindableBase.GetPropertyName(() => new ESTIMATE_ITEMProgress().Entity.Entity.VARIATION_CODE)))
            {
                if (new_value != null && new_value.ToString() != string.Empty)
                {
                    if (!VariationCodeStringCollection.Any(x => x == new_value.ToString()))
                        return "Invalid variation code";
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
            projection.Entity.Entity.ExoLines = exoLines;
            projection.Entity.Entity.FullCOMMODITY_CODECollection = COMMODITY_CODECollection;
            projection.Entity.Entity.NewItemRowSubAREACollection = SUBAREACollection;

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
            else if (new_value != null && field_name.Contains(BindableBase.GetPropertyName(() => new ESTIMATE_ITEM().COMMODITY_CODE)))
            {
                if (projection.Entity.Entity.STOCK_CODE == null || projection.Entity.Entity.STOCK_CODE == string.Empty)
                {
                    COMMODITY_CODE findCOMMODITY_CODE = COMMODITY_CODECollection.FirstOrDefault(x => x.CODE == new_value.ToString());
                    if (findCOMMODITY_CODE != null)
                        projection.Entity.Entity.STOCK_CODE = findCOMMODITY_CODE.DEFAULT_STOCKCODE;
                }

            }
            else if (new_value != null && field_name.Contains(BindableBase.GetPropertyName(() => new ESTIMATE_ITEM().STOCK_CODE)))
            {
                if (projection.Entity.Entity.UOM == null || projection.Entity.Entity.UOM == string.Empty)
                {
                    COMMODITY_CODE findCOMMODITY_CODE = COMMODITY_CODECollection.FirstOrDefault(x => x.DEFAULT_STOCKCODE == new_value.ToString());
                    if (findCOMMODITY_CODE != null)
                        projection.Entity.Entity.UOM = findCOMMODITY_CODE.UOM;
                }
            }
            else if (new_value != null && field_name.Contains(BindableBase.GetPropertyName(() => new ESTIMATE_ITEM().BUDGET_HOURS)))
            {
                if (new_value != null)
                {
                    projection.Entity.Entity.BUDGET_INSTALL_HOURS_PER_QTY = (decimal)new_value / projection.Entity.Entity.BUDGET_QUANTITY;
                }
            }
            else if (new_value != null && field_name.Contains(BindableBase.GetPropertyName(() => new ESTIMATE_ITEM().BUDGET_INSTALL_HOURS_PER_QTY)))
            {
                if (new_value != null)
                {
                    projection.Entity.Entity.BUDGET_HOURS = projection.Entity.Entity.BUDGET_QUANTITY * (decimal)new_value;
                }
            }
            else if (new_value != null && field_name.Contains(BindableBase.GetPropertyName(() => new ESTIMATE_ITEM().BUDGET_QUANTITY)))
            {
                if (new_value != null)
                {
                    projection.Entity.Entity.BUDGET_HOURS = projection.Entity.Entity.BUDGET_INSTALL_HOURS_PER_QTY * (decimal)new_value;
                }
            }

            //update anyway for unique job code to show new value
            projection.Update();
            base.UnifiedCellValueChanged(field_name, old_value, new_value, projection, isNew);
        }

        //exposed for EXO SubJob collection
        public void BulkSave(IEnumerable<ESTIMATE_ITEMProgress> progress_entities)
        {
            MainViewModel.BaseBulkSave(progress_entities, true);
        }
        #endregion

        public bool CanAutoPopulate(object button)
        {
            if (SelectedEntities == null || SelectedEntities.Count() == 0)
                return false;

            return true;
        }

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
                    collection = collection.Where(x => x.PHASE_TYPE == PhaseType.Construct).OrderBy(x => x.CODE);

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

        public IEnumerable<string> VariationCodeStringCollection
        {
            get
            {
                if (exoLines == null || exoLines.Count() == 0)
                    return new List<string>();

                return exoLines.Select(x => x.VariationCode).OrderBy(x => x).Distinct();
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