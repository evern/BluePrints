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
            //backgroundBudgetChecker = new BackgroundWorker();
            //backgroundBudgetChecker.DoWork += BackgroundBudgetChecker_DoWork;
            //backgroundBudgetChecker.WorkerSupportsCancellation = true;
        }

        #region Database Operations
        protected Data.PROJECT loadPROJECT;
        private BASELINE liveBASELINE;
        private PROGRESS livePROGRESS;
        private List<STAFF> exoSTAFFS;
        private readonly IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private readonly IPrimeroEntitiesUnitOfWork primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
        public ObservableCollection<ExoSubJobEditableProjection> TestList;
        IEnumerable<JOB_COSTGROUPS> costGroups;
        IEnumerable<JOBCOST_HDR> existingSubJobs;
        protected JOBCOST_HDR masterJob;
        protected JOBCOST_LINES copyLine;
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<Data.PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
            exoSTAFFS = primeroUnitOfWork.STAFF.Where(x => x.ISACTIVE == "Y").ToList();
            costGroups = ExoQueries.GetCostGroups(primeroUnitOfWork);
            existingSubJobs = ExoQueries.GetProjectSubJobs(primeroUnitOfWork, loadPROJECT.NUMBER);
            masterJob = ExoQueries.GetProjectSubJob(primeroUnitOfWork, loadPROJECT.NUMBER, loadPROJECT.NUMBER);
            copyLine = ExoQueries.GetMasterProjectLineByJobNumber(primeroUnitOfWork, loadPROJECT.NUMBER);
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
            return query => ExoQueries.GetNativeExoSubJobEditableProjection(primeroUnitOfWork, loadPROJECT, COMMODITY_CODECollection, exoSTAFFS);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<ExoSubJobEditableProjection> entities)
        {
            //backgroundBudgetChecker.RunWorkerAsync();
            MainViewModel.ManualPasteAction = this.ManualPasteAction;
            //MainViewModel.RawPasteOverride = rawPasteOverride;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        private void updateViewTitles()
        {
            updateSubjobTitlesServer();
            updateCostGroupTitlesServer();
        }

        private void updateSubjobTitlesServer()
        {
            if (MainViewModel == null || DisplayEntities.Count == 0)
                return;

            existingSubJobs = ExoQueries.GetProjectSubJobs(primeroUnitOfWork, loadPROJECT.NUMBER);
            foreach (ExoSubJobEditableProjection projection in DisplayEntities)
            {
                updateSubJobTitle(projection, false);
            }
        }

        private void updateSubJobTitle(ExoSubJobEditableProjection projection, bool updateRelatedSubjobsEntries)
        {
            if (projection.SubJobCode == null)
                return;

            JOBCOST_HDR existingSubJob = existingSubJobs.FirstOrDefault(x => x.JOBCODE == projection.SubJobCode);
            if (existingSubJob != null)
            {
                projection.SubJobTitle = existingSubJob.TITLE;
                projection.Update();

                if (updateRelatedSubjobsEntries)
                {
                    foreach (ExoSubJobEditableProjection relatedProjection in DisplayEntities.Where(x => x.SubJobCode == projection.SubJobCode))
                    {
                        relatedProjection.SubJobTitle = existingSubJob.TITLE;
                        relatedProjection.Update();
                    }
                }
            }
        }

        private void updateCostGroupTitlesServer()
        {
            if (MainViewModel == null || DisplayEntities.Count == 0)
                return;

            costGroups = ExoQueries.GetCostGroups(primeroUnitOfWork);
            foreach(ExoSubJobEditableProjection projection in DisplayEntities)
            {
                updateCostGroupTitle(projection, false);
            }
        }

        private void updateCostGroupTitle(ExoSubJobEditableProjection projection, bool updateRelatedDisciplineCodeEntries)
        {
            if (projection.DisciplineCode == null)
                return;

            JOB_COSTGROUPS existingCostGroup = costGroups.FirstOrDefault(x => x.SHORTCODE == projection.DisciplineCode);
            if (existingCostGroup != null)
            {
                projection.DisciplineName = existingCostGroup.COSTDESC;
                projection.Update();

                if (updateRelatedDisciplineCodeEntries)
                {
                    foreach (ExoSubJobEditableProjection relatedProjection in DisplayEntities.Where(x => x.DisciplineCode == projection.DisciplineCode))
                    {
                        relatedProjection.DisciplineName = projection.DisciplineName;
                        relatedProjection.Update();
                    }
                }
            }
        }

        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            if(masterJob == null)
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage("The master job is not created yet in exo, please contact " + BluePrintsResources.Default_CFO)));
            else if (copyLine == null)
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage("There is no job line yet in exo, please contact " + BluePrintsResources.Default_CFO)));

            //else
            //    updateViewTitles();

            base.OnAfterAssignedCallbackAndRaisePropertyChanged();
        }

        protected override void OnSelectedEntitiesChanged()
        {
            refreshPermissions();
        }

        public ExoSubJobAuth SelectedUser { get; set; }
        public virtual IEnumerable<ExoSubJobAuth> Users
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

        public bool ManualPasteAction(List<KeyValuePair<ColumnBase, string>> pasteData, ExoSubJobEditableProjection pasteEntity)
        {
            KeyValuePair<ColumnBase, string> subjobCodeData = pasteData.FirstOrDefault(x => x.Key.FieldName.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().SubJobCode)));
            KeyValuePair<ColumnBase, string> subjobCodeTitleData = pasteData.FirstOrDefault(x => x.Key.FieldName.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().SubJobTitle)));
            KeyValuePair<ColumnBase, string> disciplineCodeData = pasteData.FirstOrDefault(x => x.Key.FieldName.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().DisciplineCode)));
            KeyValuePair<ColumnBase, string> disciplineNameData = pasteData.FirstOrDefault(x => x.Key.FieldName.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().DisciplineName)));
            KeyValuePair<ColumnBase, string> commodityCodeData = pasteData.FirstOrDefault(x => x.Key.FieldName.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().CommodityCode)));
            KeyValuePair<ColumnBase, string> variationCodeData = pasteData.FirstOrDefault(x => x.Key.FieldName.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().VariationCode)));
            KeyValuePair<ColumnBase, string> budgetData = pasteData.FirstOrDefault(x => x.Key.FieldName.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().Budget)));

            pasteEntity.SubJobCode = subjobCodeData.Value;
            pasteEntity.SubJobTitle = subjobCodeTitleData.Value;
            pasteEntity.DisciplineCode = disciplineCodeData.Value;
            pasteEntity.DisciplineName = disciplineNameData.Value;
            pasteEntity.CommodityCode = commodityCodeData.Value;
            pasteEntity.VariationCode = variationCodeData.Value;

            decimal budgetValue = 0;
            if(decimal.TryParse(budgetData.Value, out budgetValue))
            {
                pasteEntity.Budget = budgetValue;
            }

            pasteEntity.PopulateCommodityCodes(COMMODITY_CODECollection);
            pasteEntity.AuthUsers = new System.Collections.ObjectModel.ObservableCollection<ExoSubJobAuth>();
            ExoSubJobEditableProjection existingSameSubJobLine = DisplayEntities.FirstOrDefault(x => x.SubJobCode == pasteEntity.SubJobCode);
            if (existingSameSubJobLine != null)
            {
                foreach (ExoSubJobAuth authUser in existingSameSubJobLine.AuthUsers)
                {
                    ExoSubJobAuth newUser = new ExoSubJobAuth();
                    DataUtils.ShallowCopy(newUser, authUser);
                    pasteEntity.AuthUsers.Add(newUser);
                }
            }

            string errorMessage = string.Empty;
            if (MainViewModel.IsValidEntity(pasteEntity, ref errorMessage))
            {
                MainViewModel.Entities.Insert(0, pasteEntity);
                this.RaisePropertyChanged(x => x.DisplayEntities);
            }
            else
                MessageBoxService.ShowMessage(pasteEntity.SubJobCode + " " + pasteEntity.DisciplineCode + " " + pasteEntity.CommodityCode + " is not unique\nCurrent row will be skipped");

            return false;
        }

        public bool CanCommitUnbookableToExo()
        {
            if (DisplayEntities == null || masterJob == null)
                return false;

            return DisplayEntities.Any(x => x.SubJobId == null);
        }

        private DevExpress.Mvvm.IDialogService BulkColumnEditDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("BulkColumnEditService"); }
        }

        public void CommitNewLinesToExo()
        {
            if (masterJob == null)
                return;

            if (masterJob.CATEGORY == null || ((int)masterJob.CATEGORY) >= 5)
            {
                MessageBoxService.ShowMessage("This job is in tender phase and hence pushing to exo is disabled, please contact " +  BluePrintsResources.Default_CFO + " to enable this feature");
                return;
            }

            IEnumerable<ExoSubJobEditableProjection> newLines = DisplayEntities.Where(x => !x.IsLineExistsInExo);
            if(newLines.Any(x => x.SubJobCode != null && x.SubJobCode.Length > 15))
            {
                MessageBoxService.ShowMessage("Some lines have subjobs that is more than 12 characters for job codes, hence operation cancelled");
                return;
            }

            int updatedLineCount = 0;
            foreach (ExoSubJobEditableProjection projection in newLines)
            {
                if(ExoMethods.UpdateLineSubJob(projection, false, BulkColumnEditDialogService, masterJob, loadPROJECT.NUMBER, primeroUnitOfWork))
                {
                    if (ExoMethods.UpdateLineDiscipline(projection, false, BulkColumnEditDialogService, masterJob, loadPROJECT.NUMBER, primeroUnitOfWork))
                    {
                        if (ExoMethods.UpdateLineCommodity(projection, false, BulkColumnEditDialogService, masterJob, loadPROJECT.NUMBER, primeroUnitOfWork))
                        {
                            JOBCOST_LINES findExistingOrAddLine = ExoMethods.findExistingOrAddLine(projection, copyLine, loadPROJECT.NUMBER);
                            projection.LineId = findExistingOrAddLine.SEQNO;
                            if (projection.LineId != null)
                                updatedLineCount += 1;

                            projection.Update();
                        }
                    }
                }
            }

            if (updatedLineCount > 0)
                MessageBoxService.ShowMessage(updatedLineCount + " line(s) added");
            else
                MessageBoxService.ShowMessage("Please make sure grid have lines highlighted in red either added through new line or pasted in");

            costGroups = ExoQueries.GetCostGroups(primeroUnitOfWork);
            existingSubJobs = ExoQueries.GetProjectSubJobs(primeroUnitOfWork, loadPROJECT.NUMBER);
            updateViewTitles();
        }


        private bool editLineVariation(ExoSubJobEditableProjection projection)
        {
            if (projection.LineId != null)
            {
                JOBCOST_LINES line = primeroUnitOfWork.JOBCOST_LINES.First(x => x.SEQNO == projection.LineId);
                if (line != null)
                {
                    line.X_VARIATION_CODE = projection.VariationCode;
                    primeroUnitOfWork.SaveChanges();
                    return true;
                }
            }

            return false;
        }

        private bool editLineBudgetCost(ExoSubJobEditableProjection projection)
        {
            if (projection.LineId != null)
            {
                JOBCOST_LINES line = primeroUnitOfWork.JOBCOST_LINES.First(x => x.SEQNO == projection.LineId);
                if (line != null)
                {
                    line.QUOTE_QTY = 1;
                    line.ACTUAL_UNITCOST = Convert.ToDouble(projection.Budget);
                    primeroUnitOfWork.SaveChanges();
                    return true;
                }
            }

            return false;
        }

        public void RemoveSelected()
        {
            List<ExoSubJobEditableProjection> removeProjections = DisplaySelectedEntities.Where(x => x.IsLineExistsInExo).ToList();
            if (MessageBoxService.ShowMessage("Are you sure you want to remove " + removeProjections.Count + " selected lines from exo?", "Confirmation", MessageButton.OKCancel) == MessageResult.Cancel)
                return;

            List<ExoSubJobEditableProjection> viewRemoveProjections = new List<ExoSubJobEditableProjection>();
            foreach (ExoSubJobEditableProjection removeProjection in removeProjections)
            {
                JOBCOST_LINES line = primeroUnitOfWork.JOBCOST_LINES.First(x => x.SEQNO == removeProjection.LineId);
                if(line != null)
                {
                    primeroUnitOfWork.JOBCOST_LINES.Remove(line);
                    primeroUnitOfWork.SaveChanges();
                }


                viewRemoveProjections.Add(removeProjection);
            }

            foreach(ExoSubJobEditableProjection viewRemoveProjection in viewRemoveProjections)
            {
                DisplayEntities.Remove(viewRemoveProjection);
            }

            this.FullRefresh();
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

        private void refreshPermissions()
        {
            this.RaisePropertyChanged(x => x.Users);
            this.RaisePropertyChanged(x => x.IsPermissionGridEnabled);
        }

        public virtual void NewRowAdded(RowEventArgs e)
        {
            if (e.RowHandle == DataControlBase.NewItemRowHandle)
            {
                ExoSubJobEditableProjection projection = (ExoSubJobEditableProjection)e.Row;
                base.OnMainViewModelAfterNewRowAdded(projection);
            }
        }

        public void CellValueChanged(CellValueChangedEventArgs e)
        {
            string field_name = e.Column.FieldName;
            bool isNew = e.RowHandle == DataControlBase.NewItemRowHandle;
            ExoSubJobEditableProjection projection = (ExoSubJobEditableProjection)e.Row;

            string errorMessage = string.Empty;
            if (MainViewModel.IsValidEntity(projection, ref errorMessage))
            {
                if (field_name.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().SubJobTitle)))
                {
                    if (!isNew)
                    {
                        commitSubJobTitle(projection);
                        updateSubJobTitle(projection, true);
                    }
                    //titles for newly added record will be updated in CommitNewLinesToExo()
                }
                else if (field_name.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().DisciplineName)))
                {
                    if (!isNew)
                    {
                        commitCostGroupName(projection);
                        updateCostGroupTitle(projection, true);
                    }
                    //titles for newly added record will be updated in CommitNewLinesToExo()
                }
                else if (field_name.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().SubJobCode)))
                {
                    ExoMethods.UpdateLineSubJob(projection, true, BulkColumnEditDialogService, masterJob, loadPROJECT.NUMBER, primeroUnitOfWork);
                }
                else if (field_name.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().DisciplineCode)))
                {
                    ExoMethods.UpdateLineDiscipline(projection, true, BulkColumnEditDialogService, masterJob, loadPROJECT.NUMBER, primeroUnitOfWork);
                }
                else if (field_name.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().CommodityCode)))
                {
                    ExoMethods.UpdateLineCommodity(projection, true, BulkColumnEditDialogService, masterJob, loadPROJECT.NUMBER, primeroUnitOfWork);
                }
                else if (field_name.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().VariationCode)))
                {
                    editLineVariation(projection);
                }
                else if (field_name.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().Budget)))
                {
                    editLineBudgetCost(projection);
                }

                projection.Update();
            }
        }

        public override void UnifiedCellValueChanging(string field_name, object old_value, object new_value, ExoSubJobEditableProjection projection, bool isNew)
        {
            if (field_name.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().DisciplineCode)))
            {
                if (isNew)
                    projection.PopulateCommodityCodes(COMMODITY_CODECollection);

                //Need to set to property immediately before calling update()
                if (new_value == null)
                    projection.DisciplineCode = string.Empty;
                else
                    projection.DisciplineCode = new_value.ToString();

                updateCostGroupTitle(projection, false);
                projection.Update();
            }
            else if (field_name.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().SubJobCode)))
            {
                //Need to set to property immediately before calling update()
                if (new_value == null)
                    projection.SubJobCode = string.Empty;
                else
                    projection.SubJobCode = new_value.ToString();

                updateSubJobTitle(projection, false);
            }

            base.UnifiedCellValueChanging(field_name, old_value, new_value, projection, isNew);
        }

        protected override void CellValueChangingImmediatePost(CellValueChangedEventArgs e)
        {
            if(e.Column.FieldName == (BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().CommodityCode)))
            {
                TableView tableView = e.Source as TableView;
                if (tableView != null && e.RowHandle != GridControl.NewItemRowHandle)
                {
                    tableView.CommitEditing();
                }
            }
        }


        public void PermissionCellValueChanging(CellValueChangedEventArgs e)
        {
            //skip on new row
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

        static string subJobMissingError = " doesn't exists in exo yet, please upload to exo before clicking edit title";
        public void EditTitle()
        {
            foreach (ExoSubJobEditableProjection selectedLine in DisplaySelectedEntities)
            {
                JOBCOST_HDR existingSubJobs = ExoQueries.GetProjectSubJob(primeroUnitOfWork, loadPROJECT.NUMBER, selectedLine.SubJobCode);
                if (existingSubJobs == null)
                {
                    MessageBoxService.ShowMessage(selectedLine.SubJobCode + subJobMissingError);
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

        static string CostGroupMissingError = " doesn't exists in exo yet, please upload to exo before clicking edit cost group title";
        public void EditCostGroupTitle()
        {
            IEnumerable<JOB_COSTGROUPS> costGroups = ExoQueries.GetCostGroups(primeroUnitOfWork);
            foreach (ExoSubJobEditableProjection selectedLine in DisplaySelectedEntities)
            {
                JOB_COSTGROUPS costGroup = costGroups.FirstOrDefault(x => x.SHORTCODE == selectedLine.DisciplineCode);
                if (costGroup == null)
                {
                    MessageBoxService.ShowMessage(selectedLine.DisciplineCode + CostGroupMissingError);
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

            updateCostGroupTitlesServer();
        }

        private void commitSubJobTitle(ExoSubJobEditableProjection projection)
        {
            if (!projection.IsLineExistsInExo)
                return;

            JOBCOST_HDR existingSubJob = ExoQueries.GetProjectSubJob(primeroUnitOfWork, loadPROJECT.NUMBER, projection.SubJobCode);
            if (existingSubJob == null)
            {
                MessageBoxService.ShowMessage(projection.SubJobCode + subJobMissingError);
                return;
            }

            existingSubJob.TITLE = projection.SubJobTitle;
            primeroUnitOfWork.SaveChanges();

            existingSubJobs = ExoQueries.GetProjectSubJobs(primeroUnitOfWork, loadPROJECT.NUMBER);
        }

        private void commitCostGroupName(ExoSubJobEditableProjection projection)
        {
            if (!projection.IsLineExistsInExo)
                return;

            JOB_COSTGROUPS costGroup = costGroups.FirstOrDefault(x => x.SHORTCODE == projection.DisciplineCode);
            if (costGroup == null)
            {
                MessageBoxService.ShowMessage(projection.DisciplineCode + CostGroupMissingError);
                return;
            }

            costGroup.COSTDESC = projection.DisciplineName;
            primeroUnitOfWork.SaveChanges();

            costGroups = ExoQueries.GetCostGroups(primeroUnitOfWork);
        }

        /// <summary>
        /// Show commodity code even when it is not valid
        /// </summary>
        public void CustomColumnDisplayText(CustomColumnDisplayTextEventArgs e)
        {
            if(e.Column.FieldName == BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().CommodityCode) && e.Row != null)
            {
                ExoSubJobEditableProjection projection = (ExoSubJobEditableProjection)e.Row;
                if (!projection.IsCommodityCodeValid)
                {
                    e.DisplayText = projection.CommodityCode;
                }
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
                    return collection.OrderBy(x => x.CODE).Distinct().Select(x => x.CODE).Distinct();

                return new List<string>();
            }
        }

        public IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection
        {
            get
            {
                return GetEntities<COMMODITY_CODE>();
            }
        }

        public IEnumerable<string> DISCIPLINE_CODEStringCollection
        {
            get
            {
                var collection = GetEntities<DISCIPLINE>();
                if (collection != null)
                    return collection.OrderBy(x => x.CODE).Select(x => string.Concat(x.CODE, "01")).Distinct();

                return new List<string>();
            }
        }

        public IEnumerable<string> SUBJOBCodeStringCollection
        {
            get
            {
                if (DisplayEntities == null || DisplayEntities.Count() == 0)
                    return new List<string>();

                return DisplayEntities.Select(x => x.SubJobCode).OrderBy(x => x).Distinct();
            }
        }

        public IEnumerable<string> VariationCodeStringCollection
        {
            get
            {
                if (DisplayEntities == null || DisplayEntities.Count() == 0)
                    return new List<string>();

                return DisplayEntities.Select(x => x.VariationCode).OrderBy(x => x).Distinct();
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