using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Helpers;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Common.Reports;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using BluePrints.Reports;
using DevExpress.Data.Filtering;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Printing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single BASELINE object view model.
    /// </summary>
    public partial class EXODesignSubjobViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <BASELINE_ITEM, ExoSubJobProjection, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of BASELINE_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static EXODesignSubjobViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new EXODesignSubjobViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the BASELINEViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the BASELINEViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected EXODesignSubjobViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        private Data.PROJECT loadPROJECT;
        private BASELINE liveBASELINE;
        private PROGRESS livePROGRESS;
        private readonly IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private readonly IPrimeroEntitiesUnitOfWork primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();

        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<Data.PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity();

        }

        protected override void initializeEntitiesLoadersDescription()
        {
            MainViewModel = null;
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.WORKPACKS, WORKPACKProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINES, BASELINEProjectionFunc, assign_baseline);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc, assign_progress);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESS_ITEMS, PROGRESS_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.SUBJOBS, SUBJOBProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PHASES, PHASEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc);
            loaderCollection.AddLoaderDescription<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DOCTYPES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DELIVERABLES_STATUSES, DELIVERABLES_STATUSProjectionFunc);
            loaderCollection.AddLoaderDescription<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.USERS);
        }

        private Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK>> WORKPACKProjectionFunc()
        {
            return query => query.Where(x => x.SUBJOB.GUID_PROJECT == loadPROJECT.GUID && x.SUBJOB.PHASE.PHASE_TYPE == PhaseType.Design);
        }

        private Func<IRepositoryQuery<BASELINE>, IQueryable<BASELINE>> BASELINEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == BaselineStatus.Live);
        }

        private void assign_baseline(BASELINE entity)
        {
            if (entity == null && !SupressCompulsoryEntityNotFoundMessage)
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage("Live baseline not found")));

            liveBASELINE = entity;
        }

        private void assign_progress(PROGRESS progress)
        {
            if (progress == null && !SupressCompulsoryEntityNotFoundMessage)
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage("Live progress not found")));

            livePROGRESS = progress;
        }

        private Func<IRepositoryQuery<PROGRESS_ITEM>, IQueryable<PROGRESS_ITEM>> PROGRESS_ITEMProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROGRESS == livePROGRESS.GUID);
        }

        private Func<IRepositoryQuery<SUBJOB>, IQueryable<SUBJOB>> SUBJOBProjectionFunc()
        {
            //legacy subjob restrictions
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<Data.PHASE>, IQueryable<Data.PHASE>> PHASEProjectionFunc()
        {
            return query => query.Where(x => x.PHASE_TYPE == PhaseType.Design);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<DELIVERABLES_STATUS>, IQueryable<DELIVERABLES_STATUS>> DELIVERABLES_STATUSProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> PROGRESSProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == ProgressStatus.Live);

        }

        private Func<IRepositoryQuery<PROJECT_REPORT>, IQueryable<PROJECT_REPORT>> PROJECT_REPORTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.REPORT_TYPE == ReportType.Baseline_Report.ToString());
        }
        #endregion

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<ExoSubJobProjection>> specifyMainViewModelProjection()
        {
            return query => ExoQueries.GetExoSubJobProjection(query.Where(x => x.GUID_BASELINE == liveBASELINE.GUID), WORKPACKCollection, loadPROJECT, livePROGRESS, RATECollection, PROGRESS_ITEMCollection, primeroUnitOfWork, USERCollection);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<ExoSubJobProjection> entities)
        {
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        protected override void OnSelectedEntitiesChanged()
        {
            refreshPermissions();
        }

        public ExoSubJobAuth SelectedUser { get; set; }
        public IEnumerable<ExoSubJobAuth> Users
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                var permissions = new List<ExoSubJobAuth>();
                if (DisplaySelectedEntities == null && MainViewModel.Entities.Count > 0)
                    DisplaySelectedEntities.Add(MainViewModel.Entities.First());

                if (DisplaySelectedEntities == null || DisplaySelectedEntities.Count == 0)
                    return null;

                foreach (USER user in USERCollection)
                {
                    IEnumerable<ExoSubJobAuth> findUsers = DisplaySelectedEntities.SelectMany(x => x.AuthUsers);
                    ExoSubJobAuth newUser = new ExoSubJobAuth();
                    newUser.User = user;
                    if (DisplaySelectedEntities.All(x => x.AuthUsers.Any(y => y.User.EXO_STAFF_ID == user.EXO_STAFF_ID)))
                        newUser.IsAssigned = true;
                    else if (DisplaySelectedEntities.Any(x => x.AuthUsers.Any(y => y.User.EXO_STAFF_ID == user.EXO_STAFF_ID)))
                        newUser.IsAssigned = null;
                    else
                        newUser.IsAssigned = false;

                    if (newUser.User.ROLE.ROLE_COMMODITY.Any(x => DisplaySelectedEntities.Any(y => y.Commodity.Code == x.DOCTYPE.CODE)))
                        newUser.ShouldAssign = true;

                    permissions.Add(newUser);
                }

                return permissions.OrderBy(x => x.User.Full_Name);
            }
        }

        private void refreshPermissions()
        {
            this.RaisePropertyChanged(x => x.Users);
        }

        public void PermissionCellValueChanging(CellValueChangedEventArgs e)
        {
            ExoSubJobAuth editingSubJobAuth = (ExoSubJobAuth)e.Row;
            //don't need to validate fieldname since only this field is changeable in role permission grid control

            bool newValue = (bool)e.Value;
            if (newValue)
            {
                foreach(ExoSubJobProjection selectedEntity in DisplaySelectedEntities.Where(x => x.IsLineExistsInExo))
                {
                    findExistingOrAddResourceAllocation(editingSubJobAuth, selectedEntity);
                    editingSubJobAuth.IsAssigned = true;
                    selectedEntity.AuthUsers.Add(editingSubJobAuth);
                }

                e.Handled = true;
            }
            else
            {
                foreach (ExoSubJobProjection selectedEntity in DisplaySelectedEntities.Where(x => x.IsLineExistsInExo))
                {
                    ExoSubJobAuth existingPermission = selectedEntity.AuthUsers.FirstOrDefault(x => x.User.EXO_STAFF_ID == editingSubJobAuth.User.EXO_STAFF_ID);
                    if (existingPermission != null)
                    {
                        deleteResourceAllocation(editingSubJobAuth, selectedEntity);
                        selectedEntity.AuthUsers.Remove(existingPermission);
                        e.Handled = true;
                    }
                }
            }

            //refreshPermissions();
            base.CellValueChanging(e);
        }

        bool showPreferred;
        public bool ShowPreferred
        {
            get
            {
                return showPreferred;
            }
            set
            {
                showPreferred = value;
                if (GridControlService != null)
                {
                    if (value)
                    {
                        CriteriaOperator criteriaOperator = GridControlService.GetFilterCriteria();
                        CriteriaOperator newCriteriaOperator;
                        if (!ReferenceEquals(criteriaOperator, null))
                        {
                            string filterCriteria = criteriaOperator.ToString() + " And [ShouldAssign] In (True)";
                            newCriteriaOperator = CriteriaOperator.Parse(filterCriteria);
                        }
                        else
                        {
                            newCriteriaOperator = CriteriaOperator.Parse("[ShouldAssign] In (True)");
                        }

                        GridControlService.SetFilterCriteria(newCriteriaOperator);
                    }
                    else
                    {
                        CriteriaOperator criteriaOperator = GridControlService.GetFilterCriteria();
                        if (!ReferenceEquals(criteriaOperator, null))
                        {
                            CriteriaOperator newCriteriaOperator;
                            string currentFilterCriteria = criteriaOperator.ToString();
                            string newfilterCriteria = currentFilterCriteria.Replace("And [ShouldAssign] In (True)", "");
                            newfilterCriteria = newfilterCriteria.Replace("[ShouldAssign] In (True)", "");
                            if (newfilterCriteria.Length >= 5)
                            {
                                string firstFiveChar = newfilterCriteria.Substring(0, 5);
                                if (firstFiveChar.ToUpper().Contains("AND"))
                                    newfilterCriteria = newfilterCriteria.Substring(5, newfilterCriteria.Length - 5);
                            }


                            newCriteriaOperator = CriteriaOperator.Parse(newfilterCriteria);
                            GridControlService.SetFilterCriteria(newCriteriaOperator);
                        }
                    }
                }
            }
        }

        public void AutoAssignPermission()
        {
            if (MessageBoxService.ShowMessage("This will auto grant permission based on document type authorisation by role, but will not delete existing authorisations, do you wish to continue?", "Auto Assign Permission", MessageButton.OKCancel) == MessageResult.Cancel)
                return;

            int fullProgress = DisplayEntities.Count * USERCollection.Count();
            LoadingScreenManager.ShowLoadingScreen(fullProgress);

            int addedCount = 0;
            foreach(ExoSubJobProjection subJob in DisplayEntities.Where(x => x.IsLineExistsInExo))
            {
                subJob.AuthUsers.Clear();
                foreach(USER user in USERCollection)
                {
                    ExoSubJobAuth newUser = new ExoSubJobAuth();
                    newUser.User = user;
                    newUser.ShouldAssign = newUser.User.ROLE.ROLE_COMMODITY.Any(x => DisplaySelectedEntities.Any(y => y.Commodity.Code == x.DOCTYPE.CODE));
                    if(newUser.ShouldAssign)
                    {
                        if (findExistingOrAddResourceAllocation(newUser, subJob))
                            addedCount += 1;

                        newUser.IsAssigned = true;
                        subJob.AuthUsers.Add(newUser);
                    }

                    LoadingScreenManager.Progress();
                }
            }

            LoadingScreenManager.CloseLoadingScreen();
            MessageBoxService.ShowMessage(addedCount + " user permission added");
        }

        public void UploadToExo()
        {
            JOBCOST_HDR masterJob = ExoQueries.GetProjectSubJob(primeroUnitOfWork, loadPROJECT.NUMBER, loadPROJECT.NUMBER);
            JOBCOST_LINES masterLine = ExoQueries.GetProjectLineByCode(primeroUnitOfWork, loadPROJECT.NUMBER);

            if(masterJob == null)
            {
                MessageBoxService.ShowMessage("Project number doesn't exists in exo");
                return;
            }

            if(masterLine == null)
            {
                MessageBoxService.ShowMessage("Project line is not setup in exo");
                return;
            }

            int updatedLineCount = 0;
            foreach (ExoSubJobProjection selectedLine in DisplaySelectedEntities)
            {
                if(!selectedLine.IsLineExistsInExo)
                {
                    
                    if (selectedLine.SubJob.Id == null)
                    {
                        int? subJobId = findExistingOrAddSubJob(selectedLine.SubJob, masterJob);
                        if (subJobId != null)
                        {
                            selectedLine.SubJob.Id = subJobId;
                        }
                    }

                    if (selectedLine.Discipline.Id == null)
                    {
                        int? disciplineId = findExistingOrAddDiscipline(selectedLine.Discipline);
                        if (disciplineId != null)
                        {
                            selectedLine.Discipline.Id = disciplineId;
                        }
                    }

                    if(selectedLine.Commodity.Id == null)
                    {
                        if(selectedLine.Discipline.Id != null)
                        {
                            int? commodityId = findExistingOrAddCommodity(selectedLine.Commodity, (int)selectedLine.Discipline.Id);
                            if (commodityId != null)
                            {
                                selectedLine.Commodity.Id = commodityId;
                            }
                        }
                    }

                    selectedLine.LineId = findExistingOrAddLine(selectedLine, masterLine);
                    selectedLine.Update();

                    if(selectedLine.LineId != null)
                    {
                        ExoSubJobProjection existingSameSubJobLine = DisplayEntities.FirstOrDefault(x => x.SubJob.Id == selectedLine.SubJob.Id);
                        if(existingSameSubJobLine != null)
                        {
                            foreach(ExoSubJobAuth authUser in existingSameSubJobLine.AuthUsers)
                            {
                                ExoSubJobAuth newUser = new ExoSubJobAuth();
                                DataUtils.ShallowCopy(newUser, authUser);
                                selectedLine.AuthUsers.Add(newUser);
                            }
                        }

                        refreshPermissions();
                        updatedLineCount += 1;
                    }

                }
            }

            MessageBoxService.ShowMessage(updatedLineCount + " line(s) added");
        }

        /// <returns>Whether new record is added</returns>
        private bool findExistingOrAddResourceAllocation(ExoSubJobAuth existingPermission, ExoSubJobProjection subJob)
        {
            var pUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            JOB_RESOURCE_ALLOCATION resourceAllocation = ExoQueries.GetResourceAllocation(pUnitOfWork, existingPermission, subJob);

            if (resourceAllocation != null)
                return false;
            else
            {
                int? resourceNo = ExoQueries.GetStaffResourceNo(pUnitOfWork, existingPermission.User.EXO_STAFF_ID);
                if (resourceNo != null && subJob.SubJob.Id != null)
                {
                    JOB_RESOURCE_ALLOCATION newAllocation = new JOB_RESOURCE_ALLOCATION();
                    newAllocation.RESOURCE_SEQNO = (int)resourceNo;
                    newAllocation.JOBNO = (int)subJob.SubJob.Id;

                    int year = DateTime.Now.Year;
                    DateTime firstDay = new DateTime(year, 1, 1);
                    DateTime startTime = new DateTime(1899, 12, 30, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                    DateTime lastDay = new DateTime(2099, 1, 1);

                    newAllocation.START_DATE = firstDay;
                    newAllocation.END_DATE = lastDay;
                    newAllocation.START_TIME = startTime;
                    newAllocation.END_TIME = startTime;
                    newAllocation.TOTAL_HOURS = 999999;
                    newAllocation.APPOINTMENT_SCHEDULED = "N";
                    pUnitOfWork.JOB_RESOURCE_ALLOCATION.Add(newAllocation);
                    pUnitOfWork.SaveChanges();
                    return true;
                }
                else
                    return false;
            }
        }

        private void deleteResourceAllocation(ExoSubJobAuth existingPermission, ExoSubJobProjection subJob)
        {
            var pUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            JOB_RESOURCE_ALLOCATION resourceAllocation = ExoQueries.GetResourceAllocation(pUnitOfWork, existingPermission, subJob);

            if (resourceAllocation != null)
            {
                pUnitOfWork.JOB_RESOURCE_ALLOCATION.Remove(resourceAllocation);
                pUnitOfWork.SaveChanges();
            }
        }

        private int? findExistingOrAddLine(ExoSubJobProjection exoLine, JOBCOST_LINES masterLine)
        {
            var pUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            if (exoLine.SubJob.Id == null || exoLine.Discipline.Id == null || exoLine.Commodity.Id == null)
                return null;
            else
            {
                int? maxJOBCOSTLINEID = ExoQueries.GetJOBCODELINEID(pUnitOfWork);
                JOBCOST_LINES line = ExoQueries.GetProjectLine(pUnitOfWork, loadPROJECT.NUMBER, exoLine);
                if (line != null)
                    return line.SEQNO;
                else if(maxJOBCOSTLINEID != null)
                {
                    JOBCOST_LINES newLINE = new JOBCOST_LINES();
                    newLINE.QUOTE_QTY = masterLine.QUOTE_QTY;
                    newLINE.QUOTE_UNITPR = 0;
                    newLINE.ACTUAL_UNITCOST = 0;
                    newLINE.TRANSDATE = DateTime.Now.Date;
                    newLINE.EXCHRATE = masterLine.EXCHRATE;
                    newLINE.DISCOUNT = 0;
                    newLINE.UNITPRICE_INCTAX = 0;
                    newLINE.JOBNO = (int)exoLine.SubJob.Id;
                    newLINE.STOCKCODE = exoLine.Commodity.Code.ToUpper();
                    newLINE.DESCRIPTION = exoLine.Commodity.Name;
                    newLINE.SHOW_ON_INVOICE = masterLine.SHOW_ON_INVOICE;
                    newLINE.COST_CENTRE = exoLine.Commodity.Id;
                    newLINE.COST_CENTRE2 = exoLine.Discipline.Id;
                    newLINE.NARRATIVE = "N";
                    newLINE.LINE_STATUS = "Q";
                    newLINE.TAXNO = masterLine.TAXNO;
                    newLINE.BRANCHNO = 0;
                    newLINE.SUBCODE = 0;
                    newLINE.ANALYSIS = 0;
                    newLINE.CURRENCYNO = 0;
                    newLINE.ALINENO = 100;
                    newLINE.GLCODE = 0;
                    newLINE.MASTER_JOBNO = exoLine.SubJob.MasterId;
                    newLINE.COPY_FROM_QUOTE = "N";
                    newLINE.DIM_LENGTH = 1;
                    newLINE.DIM_WIDTH = 1;
                    newLINE.DIM_DEPTH = 1;
                    newLINE.TOTAL_QUANTITY = 1;
                    newLINE.LINETYPE = 0;
                    newLINE.KITSEQNO = -1;
                    newLINE.KITCODE = string.Empty;
                    newLINE.PRICE_OVERRIDDEN = "N";
                    newLINE.LINKED_STOCKCODE = exoLine.Commodity.Code.ToUpper();
                    newLINE.LINKED_QTY = 1;
                    newLINE.HIDDEN_COST = 0;
                    newLINE.HIDDEN_SELL = 0;
                    newLINE.SUPPLIERNO = 0;
                    newLINE.FROMLOC = 1;
                    newLINE.LINETOTAL = 0;
                    newLINE.BOMTYPE = "N";
                    newLINE.SHOWLINE = "Y";
                    newLINE.BOMPRICING = "N";
                    newLINE.LINKEDSTATUS = "L";
                    newLINE.LISTPRICE = 0;
                    newLINE.NUNITPR = 0;
                    newLINE.OPTION_NO = 0;
                    newLINE.X_LABOUR_ALLOWANCE = 0;
                    newLINE.SPREADVALUE = "Y";
                    newLINE.TAXRATE = masterLine.TAXRATE;
                    newLINE.LINETOTAL_TAX = 0;
                    newLINE.LINETOTAL_INCTAX = 0;
                    newLINE.LINE_TAX = 0;
                    newLINE.HIDDEN_LINETOTAL = 0;
                    newLINE.SCHEDULE_SEQNO = 0;
                    newLINE.JOBCOSTLINEID = ((int)maxJOBCOSTLINEID) + 1;
                    newLINE.SNTYPE = 0;
                    newLINE.SNEXPDAYS = -2;
                    newLINE.OPPLINEID = -1;
                    newLINE.COST_LINENO = -1;
                    newLINE.X_VARIATION_CODE = string.Empty;
                    pUnitOfWork.JOBCOST_LINES.Add(newLINE);
                    pUnitOfWork.SaveChanges();

                    return newLINE.SEQNO;
                }
                else
                {
                    return null;
                }
            }
        }

        private int? findExistingOrAddCommodity(PrimeroCommodity commodity, int defaultDisciplineId)
        {
            var pUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            JOB_COSTTYPES costTypes = ExoQueries.GetCommodity(pUnitOfWork, commodity.Code);

            if (costTypes != null)
                return costTypes.SEQNO;
            else
            {
                JOB_COSTTYPES newCOSTTYPE = new JOB_COSTTYPES();
                newCOSTTYPE.DEF_MARKUP = 0;
                newCOSTTYPE.DEF_OVERHEAD = 0;
                newCOSTTYPE.COSTDESC = commodity.Code.ToUpper() + " - " + commodity.Name.ToUpper();
                newCOSTTYPE.GLCODE = -1;
                newCOSTTYPE.GLSUBCODE = 0;
                newCOSTTYPE.SHOWONQUOTE = "F";
                newCOSTTYPE.SHORTCODE = commodity.Code.ToUpper();
                newCOSTTYPE.DEF_COSTGROUP = defaultDisciplineId;
                newCOSTTYPE.DEF_PURCH_GLCODE = -1;
                newCOSTTYPE.DEF_PURCH_GLSUBCODE = 0;
                newCOSTTYPE.CONSOLIDATE = "F";
                newCOSTTYPE.COPY_FROM_QUOTE = "N";
                pUnitOfWork.JOB_COSTTYPES.Add(newCOSTTYPE);
                pUnitOfWork.SaveChanges();
                return newCOSTTYPE.SEQNO;
            }
        }

        private int? findExistingOrAddDiscipline(PrimeroDiscipline discipline)
        {
            var pUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            JOB_COSTGROUPS costGroups = ExoQueries.GetDiscipline(pUnitOfWork, discipline.Code);

            if (costGroups != null)
                return costGroups.SEQNO;
            else
            {
                JOB_COSTGROUPS newCOSTGROUP = new JOB_COSTGROUPS();
                newCOSTGROUP.DEF_MARKUP = 0;
                newCOSTGROUP.DEF_OVERHEAD = 0;
                newCOSTGROUP.COSTDESC = discipline.Code.ToUpper();
                newCOSTGROUP.SHORTCODE = discipline.Code.ToUpper();
                newCOSTGROUP.SHOWONQUOTE = "F";
                newCOSTGROUP.CONSOLIDATE = "F";
                newCOSTGROUP.COPY_FROM_QUOTE = "N";
                pUnitOfWork.JOB_COSTGROUPS.Add(newCOSTGROUP);
                pUnitOfWork.SaveChanges();
                return newCOSTGROUP.SEQNO;
            }
        }

        private int? findExistingOrAddSubJob(PrimeroSubJob subJob, JOBCOST_HDR masterJob)
        {
            var pUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            JOBCOST_HDR existingSubJobs = ExoQueries.GetProjectSubJob(pUnitOfWork, loadPROJECT.NUMBER, subJob.Code);
            if (existingSubJobs != null)
                return (int)existingSubJobs.JOBNO;
            else
            {
                if (masterJob != null)
                {
                    JOBCOST_HDR newExoSubJob = new JOBCOST_HDR();
                    newExoSubJob.ESTIMATE = 0;
                    newExoSubJob.INVOICED = 0;
                    newExoSubJob.THETIME = 0;
                    newExoSubJob.MATERIALS = 0;
                    newExoSubJob.DEF_OVERHEAD = 0;
                    newExoSubJob.MATERIALSCOST = 0;
                    newExoSubJob.ESTIMATECOST = 0;
                    newExoSubJob.THETIMECOST = 0;
                    newExoSubJob.INVOICEDCOST = 0;
                    newExoSubJob.JOBCODE = subJob.Code;
                    newExoSubJob.ACCNO = masterJob.ACCNO;
                    newExoSubJob.CUSTORDNO = string.Empty;
                    newExoSubJob.STATUS = "C";
                    newExoSubJob.TITLE = string.Empty;
                    newExoSubJob.CATEGORY = masterJob.CATEGORY;
                    newExoSubJob.JOBTYPE = masterJob.JOBTYPE;
                    newExoSubJob.STAFFNO = masterJob.STAFFNO;
                    newExoSubJob.ACTIONBY = masterJob.ACTIONBY;
                    newExoSubJob.MASTER_JOBNO = masterJob.JOBNO;
                    newExoSubJob.COSTGL = 0;
                    newExoSubJob.SALESGL = 0;
                    newExoSubJob.SERIALNO = string.Empty;
                    newExoSubJob.CONTACT = string.Empty;
                    newExoSubJob.PRIVATE_NOTE = string.Empty;
                    newExoSubJob.COSTSUBGL = 0;
                    newExoSubJob.SALESSUBGL = 0;
                    newExoSubJob.CONTACTNO = masterJob.CONTACTNO;
                    newExoSubJob.DELADDR1 = masterJob.DELADDR1;
                    newExoSubJob.DELADDR2 = masterJob.DELADDR2;
                    newExoSubJob.DELADDR3 = masterJob.DELADDR3;
                    newExoSubJob.DELADDR4 = masterJob.DELADDR4;
                    newExoSubJob.DELADDR5 = masterJob.DELADDR5;
                    newExoSubJob.DELADDR6 = masterJob.DELADDR6;
                    newExoSubJob.WRITE_OFF_COST = masterJob.WRITE_OFF_COST;
                    newExoSubJob.TOTAL_HOURS = 0;
                    newExoSubJob.EST_HOURS = 0;
                    newExoSubJob.ASSET_COST = 0;
                    newExoSubJob.ASSET_VALUE = 0;
                    newExoSubJob.BRANCHNO = 0;
                    newExoSubJob.ISACTIVE = "Y";
                    newExoSubJob.HASUNBILLED = "N";
                    newExoSubJob.INVOICEREADY = "N";
                    newExoSubJob.CALLBACKDATE = DateTime.Now;
                    newExoSubJob.ENTRYDATE = DateTime.Now;
                    newExoSubJob.TOTALVALUE = 0;
                    newExoSubJob.TOTALCOST = 0;
                    newExoSubJob.WIPLOC = masterJob.WIPLOC;
                    newExoSubJob.EXCHRATE = masterJob.EXCHRATE;
                    newExoSubJob.RETENTION_RATE = 0;
                    newExoSubJob.RETENTION2_MIN = 0;
                    newExoSubJob.RETENTION2_RATE = 0;
                    newExoSubJob.RETENTION3_MIN = 0;
                    newExoSubJob.RETENTION3_RATE = 0;
                    newExoSubJob.ALLOWANCE = 0;
                    newExoSubJob.BILLINGMODE = 0;
                    newExoSubJob.DESCRIPTION = string.Empty;
                    newExoSubJob.CAMPAIGN_WAVE_SEQNO = -1;
                    newExoSubJob.OPPORTUNITY_SEQNO = -1;
                    newExoSubJob.LINECHARGE_WRITEOFF = 0;
                    newExoSubJob.INVOICE_VIA_MASTER = "Y";
                    pUnitOfWork.JOBCOST_HDR.Add(newExoSubJob);
                    pUnitOfWork.SaveChanges();
                    return newExoSubJob.JOBNO;
                }
                else
                    return null;
            }
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get
            {
                //return "BASELINE_ITEMSViewModelWrapper" + view_project_specific_affix;
                return "ExoDesignSubJobViewModelWrapper";
            }
        }

        public override string UnifiedValueValidation(ExoSubJobProjection projection, string field_name, object new_value)
        {
            return string.Empty;
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

        public IEnumerable<RATE> RATECollection
        {
            get
            {
                return GetEntities<RATE>();
            }
        }

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMCollection
        {
            get
            {
                return GetEntities<PROGRESS_ITEM>();
            }
        }
    }
}