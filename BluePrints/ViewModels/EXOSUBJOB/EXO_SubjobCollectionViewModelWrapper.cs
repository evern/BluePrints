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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single BASELINE object view model.
    /// </summary>
    public partial class EXO_SubjobCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <BASELINE_ITEM, ExoSubJobEditableProjection, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of BASELINE_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static EXO_SubjobCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new EXO_SubjobCollectionViewModelWrapper(unitOfWorkFactory));
        }

        BackgroundWorker backgroundBudgetChecker;
        /// <summary>
        /// Initializes a new instance of the BASELINEViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the BASELINEViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected EXO_SubjobCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            backgroundBudgetChecker = new BackgroundWorker();
            backgroundBudgetChecker.DoWork += BackgroundBudgetChecker_DoWork;
            backgroundBudgetChecker.WorkerSupportsCancellation = true;
        }

        #region Database Operations
        private Data.PROJECT loadPROJECT;
        private BASELINE liveBASELINE;
        private PROGRESS livePROGRESS;
        private List<STAFF> exoSTAFFS;
        private readonly IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private readonly IPrimeroEntitiesUnitOfWork primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
        public ObservableCollection<ExoSubJobEditableProjection> TestList;
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<Data.PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
            exoSTAFFS = primeroUnitOfWork.STAFF.Where(x => x.ISACTIVE == "Y").ToList();
        }

        private void BackgroundBudgetChecker_DoWork(object sender, DoWorkEventArgs e)
        {
            EXO_DesignSubjobCollectionViewModelWrapper designSubjobWrapper = EXO_DesignSubjobCollectionViewModelWrapper.Create();
            designSubjobWrapper.SetParentViewModel(this);
            designSubjobWrapper.OnEntitiesLoadedCallBack = highlightBudgetedSubJobs;
            designSubjobWrapper.SuppressNotification = true;
            designSubjobWrapper.SupressCompulsoryEntityNotFoundMessage = true;
            designSubjobWrapper.InViewModelOnlyMode = true;
            var supportParameterObj = designSubjobWrapper as ISupportParameter;
            supportParameterObj.Parameter = new EntitiesParameter<Data.PROJECT>(loadPROJECT);
        }

        private void highlightBudgetedSubJobs(IEnumerable<ExoSubJobEditableProjection> designSubjobs, object parent_id)
        {
            if (MainViewModel == null)
                return;

            foreach(ExoSubJobEditableProjection designSubjob in designSubjobs)
            {
                ExoSubJobEditableProjection findSubJob = DisplayEntities.FirstOrDefault(x => x.SubJobCode == designSubjob.SubJobCode && x.DisciplineCode == designSubjob.DisciplineCode && x.CommodityCode == designSubjob.CommodityCode);
                if (findSubJob != null)
                {
                    findSubJob.HasBudget = true;
                    findSubJob.Update();
                }
            }
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription<COMMODITY_CODE, COMMODITY_CODE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES);
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

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<ExoSubJobEditableProjection>> specifyMainViewModelProjection()
        {
            return query => ExoQueries.GetNativeExoSubJobEditableProjection(primeroUnitOfWork, loadPROJECT, exoSTAFFS);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<ExoSubJobEditableProjection> entities)
        {
            backgroundBudgetChecker.RunWorkerAsync();
            MainViewModel.RawPasteOverride = rawPasteOverride;
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
            foreach (ExoSubJobEditableProjection entity in DisplayEntities)
            {
                if (entity.SubJobCode == null)
                    continue;

                JOBCOST_HDR existingSubJob = existingSubJobs.FirstOrDefault(x => x.JOBCODE == entity.SubJobCode);
                if (existingSubJob != null)
                {
                    entity.SubJobTitle = existingSubJob.TITLE;
                    entity.Update();
                }
            }
        }

        private void updateCostGroupTitles()
        {
            if (MainViewModel == null || DisplayEntities.Count == 0)
                return;

            IEnumerable<JOB_COSTGROUPS> costGroups = ExoQueries.GetCostGroups(primeroUnitOfWork);
            foreach(ExoSubJobEditableProjection entity in DisplayEntities)
            {
                if (entity.DisciplineId == null)
                    continue;

                JOB_COSTGROUPS costGroup = costGroups.FirstOrDefault(x => x.SHORTCODE == entity.DisciplineCode);
                if(costGroup != null)
                {
                    entity.DisciplineName = costGroup.COSTDESC;
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

                List<ExoSubJobAuth> orderedAuthUsers = new List<ExoSubJobAuth>();
                foreach (STAFF staff in exoSTAFFS)
                {
                    ExoSubJobAuth displayUserAuth = new ExoSubJobAuth();
                    USER newUser = USERCollection.FirstOrDefault(x => x.EXO_STAFF_ID == staff.STAFFNO);
                    if (newUser == null)
                        newUser = new USER();

                    newUser.NAME = staff.NAME;
                    newUser.EXO_STAFF_ID = staff.STAFFNO;
                    displayUserAuth.User = newUser;

                    if (DisplaySelectedEntities.All(x => x.AuthUsers.Any(y => y.User.EXO_STAFF_ID == newUser.EXO_STAFF_ID)))
                        displayUserAuth.IsAssigned = true;
                    else if (DisplaySelectedEntities.Any(x => x.AuthUsers.Any(y => y.User.EXO_STAFF_ID == newUser.EXO_STAFF_ID)))
                        displayUserAuth.IsAssigned = null;
                    else
                        displayUserAuth.IsAssigned = false;

                    displayUserAuth.ShouldAssign = false;
                    orderedAuthUsers.Add(displayUserAuth);
                }

                permissions.AddRange(orderedAuthUsers.OrderBy(x => x.User.Full_Name));
                return permissions;
            }
        }

        private void rawPasteOverride(IEnumerable<string> rowData)
        {
            foreach (string row in rowData)
            {
                List<string> ColumnStrings = row.Split('\t').ToList();
                if (ColumnStrings.Count < 3)
                    continue;

                ExoSubJobEditableProjection tempSubJobProjection = ViewModelSource.Create(() => new ExoSubJobEditableProjection());
                tempSubJobProjection.SubJobCode = ColumnStrings[0];
                tempSubJobProjection.SubJobTitle = ColumnStrings[1];
                tempSubJobProjection.DisciplineCode = ColumnStrings[2];
                tempSubJobProjection.DisciplineName = ColumnStrings[3];
                tempSubJobProjection.CommodityCode = ColumnStrings[4];

                tempSubJobProjection.AuthUsers = new System.Collections.ObjectModel.ObservableCollection<ExoSubJobAuth>();
                ExoSubJobEditableProjection existingSameSubJobLine = DisplayEntities.FirstOrDefault(x => x.SubJobCode == tempSubJobProjection.SubJobCode);
                if (existingSameSubJobLine != null)
                {
                    foreach (ExoSubJobAuth authUser in existingSameSubJobLine.AuthUsers)
                    {
                        ExoSubJobAuth newUser = new ExoSubJobAuth();
                        DataUtils.ShallowCopy(newUser, authUser);
                        tempSubJobProjection.AuthUsers.Add(newUser);
                    }
                }

                MainViewModel.Entities.Insert(0, tempSubJobProjection);
            }

            this.RaisePropertyChanged(x => x.DisplayEntities);
        }

        public bool CanCommitUnbookableToExo()
        {
            if (DisplayEntities == null)
                return false;

            return DisplayEntities.Any(x => x.SubJobId == null);
        }

        private DevExpress.Mvvm.IDialogService BulkColumnEditDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("BulkColumnEditService"); }
        }

        public void CommitNewLinesToExo()
        {
            JOBCOST_HDR masterJob = ExoQueries.GetProjectSubJob(primeroUnitOfWork, loadPROJECT.NUMBER, loadPROJECT.NUMBER);
            if (masterJob.CATEGORY == null || ((int)masterJob.CATEGORY) >= 5)
            {
                MessageBoxService.ShowMessage("This job is in tender phase and hence pushing to exo is disabled, please contact Michelle Wilson or Ryan McFarlane to enable this feature");
                return;
            }

            IEnumerable<ExoSubJobEditableProjection> newLines = DisplayEntities.Where(x => !x.IsLineExistsInExo);
            if(newLines.Any(x => x.SubJobCode != null && x.SubJobCode.Length > 15))
            {
                MessageBoxService.ShowMessage("Some lines have subjobs that is more than 12 characters for job codes, hence operation cancelled");
                return;
            }

            JOBCOST_LINES copyLine = ExoQueries.GetAnyProjectLineByJobNumber(primeroUnitOfWork, loadPROJECT.NUMBER);
            int updatedLineCount = 0;
            List<string> addedLines = new List<string>();
            foreach (ExoSubJobEditableProjection projection in newLines)
            {
                if(!addedLines.Any(x => x == projection.SubJobCode))
                {
                    string title = projection.SubJobTitle;
                    if(title == string.Empty)
                    {
                        var bulkEditStringsViewModel = BulkEditStringsViewModel.Create(string.Empty, projection.SubJobCode + " Title:");
                        if (BulkColumnEditDialogService.ShowDialog(MessageButton.OKCancel, "Please input title", "BulkEditStrings", bulkEditStringsViewModel) == MessageResult.OK)
                        {
                            title = bulkEditStringsViewModel.EditValue;
                        }
                    }

                    int? subJobId = ExoMethods.findExistingOrAddSubJob(projection.SubJobCode, masterJob, loadPROJECT.NUMBER, title);
                    if (subJobId != null)
                    {
                        projection.SubJobId = subJobId;
                        addedLines.Add(projection.SubJobCode);
                    }
                }

                int? disciplineId = ExoMethods.findExistingOrAddDiscipline(projection.DisciplineCode, projection.DisciplineName);
                if (disciplineId != null)
                {
                    projection.DisciplineId = disciplineId;
                    int? commodityId = ExoMethods.findExistingCommodity(projection.CommodityCode, string.Empty, (int)disciplineId);
                    if (commodityId != null)
                    {
                        projection.CommodityId = commodityId;
                        projection.LineId = ExoMethods.findExistingOrAddLine(projection, copyLine, loadPROJECT.NUMBER);
                        if(projection.LineId != null)
                            updatedLineCount += 1;

                        projection.Update();
                    }
                }
            }

            MessageBoxService.ShowMessage(updatedLineCount + " line(s) added");
            updateViewTitles();
        }

        public void RemoveSelected()
        {
            List<ExoSubJobEditableProjection> removeProjections = DisplaySelectedEntities.Where(x => x.IsLineExistsInExo).ToList();
            if (MessageBoxService.ShowMessage("Are you sure you want to remove " + removeProjections.Count + " selected lines from exo?", "Confirmation", MessageButton.OKCancel) == MessageResult.Cancel)
                return;

            foreach (ExoSubJobEditableProjection removeProjection in removeProjections)
            {
                JOBCOST_LINES line = ExoQueries.GetProjectLine(primeroUnitOfWork, loadPROJECT.NUMBER, removeProjection);
                if(line != null)
                    primeroUnitOfWork.JOBCOST_LINES.Remove(line);
            }

            primeroUnitOfWork.SaveChanges();
            this.FullRefresh();
        }

        private void refreshPermissions()
        {
            this.RaisePropertyChanged(x => x.Users);
        }

        public void PermissionCellValueChanging(CellValueChangedEventArgs e)
        {
            if(e.RowHandle < 0)
            {
                e.Handled = true;
                base.CellValueChanging(e);
                return;
            }

            ExoSubJobAuth editingSubJobAuth = (ExoSubJobAuth)e.Row;
            //don't need to validate fieldname since only this field is changeable in role permission grid control

            bool newValue = (bool)e.Value;
            if (newValue)
            {
                foreach(ExoSubJobEditableProjection selectedEntity in DisplaySelectedEntities.Where(x => x.IsLineExistsInExo && x.SubJobCode != null && x.SubJobId != null))
                {
                    ExoMethods.findExistingOrAddResourceAllocation(editingSubJobAuth, (int)selectedEntity.SubJobId);
                    editingSubJobAuth.IsAssigned = true;
                    selectedEntity.AuthUsers.Add(editingSubJobAuth);

                    foreach (ExoSubJobEditableProjection sameSubJobEntity in DisplayEntities.Where(x => x.SubJobCode != null && x.SubJobId == selectedEntity.SubJobId))
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
                foreach (ExoSubJobEditableProjection selectedEntity in DisplaySelectedEntities.Where(x => x.IsLineExistsInExo && x.SubJobCode != null && x.SubJobId != null))
                {
                    ExoSubJobAuth existingPermission = selectedEntity.AuthUsers.FirstOrDefault(x => x.User.EXO_STAFF_ID == editingSubJobAuth.User.EXO_STAFF_ID);
                    if (existingPermission != null)
                    {
                        ExoMethods.deleteResourceAllocation(editingSubJobAuth, (int)selectedEntity.SubJobId);
                        selectedEntity.AuthUsers.Remove(existingPermission);
                        e.Handled = true;
                    }

                    foreach (ExoSubJobEditableProjection sameSubJobEntity in DisplayEntities.Where(x => x.SubJobCode != null && x.SubJobId == selectedEntity.SubJobId))
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
            foreach (ExoSubJobEditableProjection selectedLine in DisplaySelectedEntities)
            {
                JOBCOST_HDR existingSubJobs = ExoQueries.GetProjectSubJob(primeroUnitOfWork, loadPROJECT.NUMBER, selectedLine.SubJobCode);
                if (existingSubJobs == null)
                {
                    MessageBoxService.ShowMessage(selectedLine.SubJobCode + " doesn't exists in exo yet, please upload to exo before clicking edit title");
                    continue;
                }

                var bulkEditStringsViewModel = BulkEditStringsViewModel.Create(existingSubJobs.TITLE, selectedLine.SubJobCode + " Title:");
                string title = string.Empty;
                if (BulkColumnEditDialogService.ShowDialog(MessageButton.OKCancel, "Please input title", "BulkEditStrings", bulkEditStringsViewModel) == MessageResult.OK)
                {
                    title = bulkEditStringsViewModel.EditValue;
                    existingSubJobs.TITLE = title;
                    primeroUnitOfWork.SaveChanges();
                }
            }

            updateViewTitles();
        }

        public void EditCostGroupTitle()
        {
            IEnumerable<JOB_COSTGROUPS> costGroups = ExoQueries.GetCostGroups(primeroUnitOfWork);
            foreach (ExoSubJobEditableProjection selectedLine in DisplaySelectedEntities)
            {
                JOB_COSTGROUPS costGroup = costGroups.FirstOrDefault(x => x.SHORTCODE == selectedLine.DisciplineCode);
                if (costGroup == null)
                {
                    MessageBoxService.ShowMessage(selectedLine.DisciplineCode + " doesn't exists in exo yet, please upload to exo before clicking edit cost group title");
                    continue;
                }

                var bulkEditStringsViewModel = BulkEditStringsViewModel.Create(costGroup.COSTDESC, selectedLine.DisciplineCode + " Title:");
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

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get
            {
                //return "BASELINE_ITEMSViewModelWrapper" + view_project_specific_affix;
                return "EXO_SubjobCollectionViewModelWrapper";
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

        public IEnumerable<string> COMMODITY_CODEStringCollection
        {
            get
            {
                var collection = GetEntities<COMMODITY_CODE>();
                if (collection != null)
                    return collection.OrderBy(x => x.CODE).Select(x => x.CODE);

                return new List<string>();
            }
        }

        public IEnumerable<string> DISCIPLINE_CODEStringCollection
        {
            get
            {
                var collection = GetEntities<DISCIPLINE>();
                if (collection != null)
                    return collection.OrderBy(x => x.CODE).Select(x => string.Concat(x.CODE, "01"));

                return new List<string>();
            }
        }

        public IEnumerable<string> SUBJOBCodeStringCollection
        {
            get
            {
                if (DisplayEntities == null || DisplayEntities.Count() == 0)
                    return new List<string>();

                return DisplayEntities.Select(x => x.SubJobCode);
            }
        }

        public IEnumerable<string> SUBJOBTitleStringCollection
        {
            get
            {
                if (DisplayEntities == null || DisplayEntities.Count() == 0)
                    return new List<string>();

                return DisplayEntities.Select(x => x.SubJobTitle);
            }
        }

        public override string UnifiedRowValidation(ExoSubJobEditableProjection projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(ExoSubJobEditableProjection projection, string field_name, object new_value)
        {
            return string.Empty;
        }
    }
}