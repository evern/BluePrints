using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Helpers;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Dialogs;
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
using System.Windows.Forms;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single BASELINE object view model.
    /// </summary>
    public partial class EXO_DesignSubjobViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <BASELINE_ITEM, ExoSubJobProjection, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of BASELINE_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static EXO_DesignSubjobViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new EXO_DesignSubjobViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the BASELINEViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the BASELINEViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected EXO_DesignSubjobViewModelWrapper(
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

        protected override void addEntitiesLoader()
        {
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

        private void updateViewTitles()
        {
            updateSubjobTitles();
            updateCostGroupTitles();
        }

        private void updateSubjobTitles()
        {
            if (MainViewModel == null || DisplayEntities.Count == 0)
                return;

            IEnumerable<JOBCOST_HDR> existingSubJobs = ExoQueries.GetProjectSubJobs(primeroUnitOfWork, loadPROJECT.NUMBER);
            foreach (ExoSubJobProjection entity in DisplayEntities)
            {
                if (entity.SubJob == null)
                    return;

                JOBCOST_HDR existingSubJob = existingSubJobs.FirstOrDefault(x => x.JOBCODE == entity.SubJob.Code);
                if (existingSubJob != null)
                {
                    entity.SubJob.Title = existingSubJob.TITLE;
                    entity.Update();
                }
            }
        }

        private void updateCostGroupTitles()
        {
            if (MainViewModel == null || DisplayEntities.Count == 0)
                return;

            IEnumerable<JOB_COSTGROUPS> costGroups = ExoQueries.GetCostGroups(primeroUnitOfWork);
            foreach (ExoSubJobProjection entity in DisplayEntities)
            {
                if (entity.Discipline == null)
                    continue;

                JOB_COSTGROUPS costGroup = costGroups.FirstOrDefault(x => x.SHORTCODE == entity.Discipline.Code);
                if (costGroup != null)
                {
                    entity.Discipline.Name = costGroup.COSTDESC;
                    entity.Update();
                }
            }
        }

        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            updateViewTitles();
            base.OnAfterAssignedCallbackAndRaisePropertyChanged();
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

                    if (newUser.User != null && newUser.User.ROLE != null && newUser.User.ROLE.ROLE_COMMODITY.Count > 0 && newUser.User.ROLE.ROLE_COMMODITY.Any(x => DisplaySelectedEntities.Any(y => y.Commodity.Code == x.DOCTYPE.CODE)))
                        newUser.ShouldAssign = true;

                    permissions.Add(newUser);
                }

                return permissions.OrderBy(x => x.User.Full_Name);
            }
        }

        private void refreshPermissions()
        {
            this.RaisePropertyChanged(x => x.Users);
            this.RaisePropertyChanged(x => x.IsPermissionGridEnabled);
        }

        public void PermissionCellValueChanging(CellValueChangedEventArgs e)
        {
            ExoSubJobAuth editingSubJobAuth = (ExoSubJobAuth)e.Row;
            //don't need to validate fieldname since only this field is changeable in role permission grid control

            bool newValue = (bool)e.Value;
            if (newValue)
            {
                foreach(ExoSubJobProjection selectedEntity in DisplaySelectedEntities.Where(x => x.IsLineExistsInExo && x.SubJob != null && x.SubJob.Id != null))
                {
                    ExoMethods.findExistingOrAddResourceAllocation(editingSubJobAuth, (int)selectedEntity.SubJob.Id);
                    editingSubJobAuth.IsAssigned = true;
                    selectedEntity.AuthUsers.Add(editingSubJobAuth);

                    foreach(ExoSubJobProjection sameSubJobEntity in DisplayEntities.Where(x => x.SubJob != null && x.SubJob.Id == selectedEntity.SubJob.Id))
                    {
                        ExoSubJobAuth findAuth = sameSubJobEntity.AuthUsers.FirstOrDefault(x => x.User.EXO_STAFF_ID == editingSubJobAuth.User.EXO_STAFF_ID);
                        if (findAuth == null)
                        {
                            sameSubJobEntity.AuthUsers.Add(editingSubJobAuth);
                        }
                        else if (findAuth.IsAssigned == null || !(bool)findAuth.IsAssigned)
                            findAuth.IsAssigned = true;
                    }
                }

                e.Handled = true;
            }
            else
            {
                foreach (ExoSubJobProjection selectedEntity in DisplaySelectedEntities.Where(x => x.IsLineExistsInExo && x.SubJob != null && x.SubJob.Id != null))
                {
                    ExoSubJobAuth existingPermission = selectedEntity.AuthUsers.FirstOrDefault(x => x.User.EXO_STAFF_ID == editingSubJobAuth.User.EXO_STAFF_ID);
                    if (existingPermission != null)
                    {
                        ExoMethods.deleteResourceAllocation(editingSubJobAuth, (int)selectedEntity.SubJob.Id);
                        selectedEntity.AuthUsers.Remove(existingPermission);
                        e.Handled = true;
                    }

                    foreach (ExoSubJobProjection sameSubJobEntity in DisplayEntities.Where(x => x.SubJob != null && x.SubJob.Id == selectedEntity.SubJob.Id))
                    {
                        ExoSubJobAuth findAuth = sameSubJobEntity.AuthUsers.FirstOrDefault(x => x.User.EXO_STAFF_ID == editingSubJobAuth.User.EXO_STAFF_ID);
                        if (findAuth != null)
                            sameSubJobEntity.AuthUsers.Remove(findAuth);
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

        public bool IsPermissionGridEnabled
        {
            get
            {
                if (DisplayEntities == null || DisplaySelectedEntities.Count == 0)
                    return false;

                return DisplaySelectedEntities.Any(x => x.IsLineExistsInExo);
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
                    if(newUser.ShouldAssign && subJob.SubJob.Id != null)
                    {
                        if (ExoMethods.findExistingOrAddResourceAllocation(newUser, (int)subJob.SubJob.Id))
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

        private DevExpress.Mvvm.IDialogService BulkColumnEditDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("BulkColumnEditService"); }
        }

        public void KeyboardCopy()
        {
            SendKeys.SendWait("^c");
        }

        public void KeyboardPaste()
        {
            SendKeys.SendWait("^v");
        }

        public void EditTitle()
        {
            foreach (ExoSubJobProjection selectedLine in DisplaySelectedEntities)
            {
                JOBCOST_HDR existingSubJobs = ExoQueries.GetProjectSubJob(primeroUnitOfWork, loadPROJECT.NUMBER, selectedLine.SubJob.Code);
                if(existingSubJobs == null)
                {
                    MessageBoxService.ShowMessage(selectedLine.SubJob.Code + " doesn't exists in exo yet, please upload to exo before clicking edit title");
                    continue;
                }

                var bulkEditStringsViewModel = BulkEditStringsViewModel.Create(existingSubJobs.TITLE, selectedLine.SubJob.Code + " Title:");
                string title = string.Empty;
                if (BulkColumnEditDialogService.ShowDialog(MessageButton.OKCancel, "Please input title", "BulkEditStrings", bulkEditStringsViewModel) == MessageResult.OK)
                {
                    title = bulkEditStringsViewModel.EditValue;
                    existingSubJobs.TITLE = title;
                    primeroUnitOfWork.SaveChanges();
                }
            }

            updateSubjobTitles();
        }

        public void EditCostGroupTitle()
        {
            IEnumerable<JOB_COSTGROUPS> costGroups = ExoQueries.GetCostGroups(primeroUnitOfWork);
            foreach (ExoSubJobProjection selectedLine in DisplaySelectedEntities)
            {
                JOB_COSTGROUPS costGroup = costGroups.FirstOrDefault(x => x.SHORTCODE == selectedLine.Discipline.Code);
                if (costGroup == null)
                {
                    MessageBoxService.ShowMessage(selectedLine.Discipline.Code + " doesn't exists in exo yet, please upload to exo before clicking edit cost group title");
                    continue;
                }

                var bulkEditStringsViewModel = BulkEditStringsViewModel.Create(costGroup.COSTDESC, selectedLine.Discipline.Code + " Title:");
                string title = string.Empty;
                if (BulkColumnEditDialogService.ShowDialog(MessageButton.OKCancel, "Please input title", "BulkEditStrings", bulkEditStringsViewModel) == MessageResult.OK)
                {
                    title = bulkEditStringsViewModel.EditValue;
                    costGroup.COSTDESC = title;
                    primeroUnitOfWork.SaveChanges();
                }
            }

            updateCostGroupTitles();
        }

        public void UploadToExo()
        {
            JOBCOST_HDR masterJob = ExoQueries.GetProjectSubJob(primeroUnitOfWork, loadPROJECT.NUMBER, loadPROJECT.NUMBER);
            JOBCOST_LINES existingLine = ExoQueries.GetAnyProjectLineByJobNumber(primeroUnitOfWork, loadPROJECT.NUMBER);
            if(masterJob.CATEGORY == null || ((int)masterJob.CATEGORY) >=5 )
            {
                MessageBoxService.ShowMessage("This job is in tender phase in exoand hence pushing to exo is disabled, please contact accounts to enable this feature");
                return;
            }

            if(masterJob == null)
            {
                MessageBoxService.ShowMessage("Project number doesn't exists in exo, please contact accounts to add job");
                return;
            }

            if(existingLine == null)
            {
                MessageBoxService.ShowMessage("Project line is not setup in exo, please contact accounts to add job line");
                return;
            }

            if (DisplaySelectedEntities.Any(x => x.SubJob != null && x.SubJob.Code.Length > 15))
            {
                MessageBoxService.ShowMessage("Some lines have subjobs that is more than 12 characters for job codes, hence operation cancelled");
                return;
            }

            int updatedLineCount = 0;
            Dictionary<int, string> addedLines = new Dictionary<int, string>();
            foreach (ExoSubJobProjection selectedLine in DisplaySelectedEntities)
            {
                ChargeType? subjobPhaseType = selectedLine.SubJob == null ? null : selectedLine.SubJob.ChargeType;
                bool isIndirectOnly = selectedLine.Commodity == null ? false : selectedLine.Commodity.IsIndirectOnly;

                if(subjobPhaseType == ChargeType.Direct && isIndirectOnly)
                {
                    MessageBoxService.ShowMessage("This commodity can only be assigned to indirect subjobs");
                    continue;
                }

                if(!selectedLine.IsLineExistsInExo)
                {
                    if (selectedLine.SubJob.Id == null && !addedLines.Any(x => x.Value == selectedLine.SubJob.Code))
                    {
                        string title = string.Empty;
                        var bulkEditStringsViewModel = BulkEditStringsViewModel.Create(string.Empty, selectedLine.SubJob.Code + " Title:");
                        if (BulkColumnEditDialogService.ShowDialog(MessageButton.OKCancel, "Please input title", "BulkEditStrings", bulkEditStringsViewModel) == MessageResult.OK)
                        {
                            title = bulkEditStringsViewModel.EditValue;
                        }

                        int? subJobId = ExoMethods.findExistingOrAddSubJob(selectedLine.SubJob.Code, masterJob, loadPROJECT.NUMBER, title);
                        if (subJobId != null)
                        {
                            addedLines.Add((int)subJobId, selectedLine.SubJob.Code);
                            selectedLine.SubJob.Id = subJobId;
                        }
                    }
                    else if(addedLines.Any(x => x.Value == selectedLine.SubJob.Code))
                    {
                        selectedLine.SubJob.Id = addedLines.First(x => x.Value == selectedLine.SubJob.Code).Key;
                    }

                    if (selectedLine.Discipline.Id == null)
                    {
                        int? disciplineId = ExoMethods.findExistingOrAddDiscipline(selectedLine.Discipline.Code, selectedLine.Discipline.Name);
                        if (disciplineId != null)
                        {
                            selectedLine.Discipline.Id = disciplineId;
                        }
                    }

                    if(selectedLine.Commodity.Id == null)
                    {
                        if(selectedLine.Discipline.Id != null)
                        {
                            int? commodityId = ExoMethods.findExistingCommodity(selectedLine.Commodity.Code, selectedLine.Commodity.Name, (int)selectedLine.Discipline.Id);
                            if (commodityId != null)
                            {
                                selectedLine.Commodity.Id = commodityId;
                            }
                            else
                                MessageBoxService.ShowMessage(selectedLine.Commodity.Code + " cost type does not exists in exo, please ask accounts to create it");
                        }
                    }

                    selectedLine.LineId = ExoMethods.findExistingOrAddLine(selectedLine, existingLine, loadPROJECT.NUMBER);
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
            updateViewTitles();
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

        public override string UnifiedRowValidation(ExoSubJobProjection projection)
        {
            return string.Empty;
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