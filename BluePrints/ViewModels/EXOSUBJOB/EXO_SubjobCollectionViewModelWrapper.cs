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
using System.Text.RegularExpressions;
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

        }

        #region Code Properties
        protected Data.PROJECT loadPROJECT;
        protected List<STAFF> exoSTAFFS;
        private readonly IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> localPrimeroUnitOfWorkFactory;
        protected IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> remotePrimeroUnitOfWorkFactory;
        protected IPrimeroEntitiesUnitOfWork localPrimeroUnitOfWork;
        protected IPrimeroEntitiesUnitOfWork remotePrimeroUnitOfWork;
        private IEnumerable<JOB_COSTGROUPS> costGroups;
        private IEnumerable<JOBCOST_HDR> existingSubJobs;
        protected JOBCOST_HDR masterJob;
        protected JOBCOST_LINES copyLine;
        protected bool runPlannedChecker = false;
        protected bool initializeOptionalViewCollectionsOnRefresh = true;
        static string subJobMissingError = " doesn't exists in exo yet, please upload to exo before clicking edit title";
        static string costGroupMissingError = " doesn't exists in exo yet, please upload to exo before clicking edit cost group title";
        private int subjobCodeMaxLength = 15;
        private int disciplineCodeMaxLength = 4;
        private int commodityCodeMaxLength = 4;

        //user from exo will do a lookup to get additional details from user's in BluePrints
        protected bool tryCombineLocalUsers = false;
        #endregion

        #region Loading Operations
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<Data.PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
            initializeCompulsoryViewProperties(loadPROJECT);
            SubJobRegex = loadPROJECT.NUMBER + BluePrintsResources.Regex_SUBJOB;
            DisciplineRegex = BluePrintsResources.Regex_DISCIPLINE;
            backgroundBudgetChecker = new BackgroundWorker();
            backgroundBudgetChecker.DoWork += BackgroundBudgetChecker_DoWork;
            backgroundBudgetChecker.WorkerSupportsCancellation = true;
            initializeOptionalViewCollections();
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, COMMODITY_CODEProjectionFunc);
            loaderCollection.AddLoaderDescription<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.USERS);
            loaderCollection.AddLoaderDescription<PrimeroData.PROFILE, PrimeroData.PROFILE, int, IPrimeroEntitiesUnitOfWork>(localPrimeroUnitOfWorkFactory, x => x.PROFILE);
            loaderCollection.AddLoaderDescription<PrimeroData.STOCK_ITEMS, PrimeroData.STOCK_ITEMS, string, IPrimeroEntitiesUnitOfWork>(localPrimeroUnitOfWorkFactory, x => x.STOCK_ITEMS);
        }

        protected virtual Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>> COMMODITY_CODEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == null);
        }

        protected void initializeCompulsoryViewProperties(Data.PROJECT project)
        {
            localPrimeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(loadPROJECT.OfficeNameForExo == BluePrintsResources.OfficeMontreal);
            remotePrimeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(loadPROJECT.OfficeNameForExo != BluePrintsResources.OfficeMontreal);
            localPrimeroUnitOfWork = localPrimeroUnitOfWorkFactory.CreateUnitOfWork();
            remotePrimeroUnitOfWork = localPrimeroUnitOfWorkFactory.CreateUnitOfWork();

            masterJob = ExoQueries.GetProjectSubJob(localPrimeroUnitOfWork, project.NUMBER);
            copyLine = ExoQueries.GetAnyProjectLineByJobNumber(localPrimeroUnitOfWork, project.NUMBER);

            List<STAFF> localSTAFFS = ExoQueries.GetStaffs(localPrimeroUnitOfWork).ToList();
            List<STAFF> remoteSTAFFS = ExoQueries.GetStaffs(remotePrimeroUnitOfWork).ToList();
            exoSTAFFS = new List<STAFF>();
            exoSTAFFS.AddRange(localSTAFFS);
            exoSTAFFS.AddRange(remoteSTAFFS);
        }

        private void initializeOptionalViewCollections()
        {
            costGroups = ExoQueries.GetCostGroups(localPrimeroUnitOfWork);
            existingSubJobs = ExoQueries.GetProjectSubJobs(localPrimeroUnitOfWork, loadPROJECT.NUMBER);
        }

        private void BackgroundBudgetChecker_DoWork(object sender, DoWorkEventArgs e)
        {
            EXO_DesignSubjobCollectionViewModelWrapper designSubjobWrapper = EXO_DesignSubjobCollectionViewModelWrapper.CreateDesignSubJobCollection();
            designSubjobWrapper.SetParentViewModel(this);
            designSubjobWrapper.OnEntitiesLoadedCallBack = updateBudgetedSubJobs;
            designSubjobWrapper.SuppressNotification = true;
            designSubjobWrapper.SupressCompulsoryEntityNotFoundMessage = true;
            designSubjobWrapper.InViewModelOnlyMode = true;
            var supportParameterObj = designSubjobWrapper as ISupportParameter;
            supportParameterObj.Parameter = new EntitiesParameter<Data.PROJECT>(loadPROJECT);
        }

        private void updateBudgetedSubJobs(IEnumerable<ExoSubJobEditableProjection> designSubjobs, object parent_id)
        {
            if (MainViewModel == null)
                return;

            foreach (ExoSubJobEditableProjection designSubjob in designSubjobs)
            {
                ExoSubJobEditableProjection findSubJob = DisplayEntities.FirstOrDefault(x => x.SubJobCode == designSubjob.SubJobCode && x.DisciplineCode == designSubjob.DisciplineCode && x.CommodityCode == designSubjob.CommodityCode);
                if (findSubJob != null)
                {
                    findSubJob.HasBudget = true;
                    findSubJob.Update();
                }
            }
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<ExoSubJobEditableProjection>> specifyMainViewModelProjection()
        {
            return query => ExoQueries.GetNativeExoSubJobEditableProjection(localPrimeroUnitOfWork, loadPROJECT, COMMODITY_CODECollection, STOCK_ITEMSCollection, exoSTAFFS);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<ExoSubJobEditableProjection> entities)
        {
            MainViewModel.AlwaysSkipMessage = this.AlwaysSkipMessage;
            MainViewModel.FuncManualRowPastingIsContinue = this.ManualRowPasteAction;
            MainViewModel.SetParentViewModel(this);

            mainThreadDispatcher.BeginInvoke(new Action(() => filterUser()));
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        //filter out user's that have default security profile
        private void filterUser()
        {
            //CriteriaOperator newCriteriaOperator = CriteriaOperator.Parse("[User.SecurityProfileID] <> 4");
            //UserFilterCriteria = newCriteriaOperator;
            //this.RaisePropertyChanged(x => x.UserFilterCriteria);
        }

        public CriteriaOperator UserFilterCriteria { get; set; }

        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            if (masterJob == null)
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage("The master job is not created yet in exo, please contact " + BluePrintsResources.Default_CFO)));
            else if (copyLine == null)
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage("There is no job line yet in exo, please contact " + BluePrintsResources.Default_CFO)));

            if(backgroundBudgetChecker != null)
                backgroundBudgetChecker.RunWorkerAsync();

            base.OnAfterAssignedCallbackAndRaisePropertyChanged();
        }

        protected override void OnSelectedEntitiesChanged()
        {
            refreshPermissions();
        }
#endregion

#region Events
        /// <summary>
        /// Remembers an entity added for undoing
        /// Since CollectionViewModelBase is a POCO view model, an the instance of this class will also expose the AddUndoCommand property that can be used as a binding source in views.
        /// </summary>
        public virtual void CommitNewRow(RowEventArgs e)
        {
            if (e.RowHandle == DataControlBase.NewItemRowHandle)
            {
                commitToExo((ExoSubJobEditableProjection)e.Row);
            }
        }

        public bool ManualRowPasteAction(List<KeyValuePair<ColumnBase, string>> pasteData, ExoSubJobEditableProjection pasteEntity, bool isLastRow)
        {
            KeyValuePair<ColumnBase, string> subjobCodeData = pasteData.FirstOrDefault(x => x.Key.FieldName.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().SubJobCode)));
            KeyValuePair<ColumnBase, string> subjobCodeTitleData = pasteData.FirstOrDefault(x => x.Key.FieldName.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().SubJobTitle)));
            KeyValuePair<ColumnBase, string> disciplineCodeData = pasteData.FirstOrDefault(x => x.Key.FieldName.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().DisciplineCode)));
            KeyValuePair<ColumnBase, string> disciplineNameData = pasteData.FirstOrDefault(x => x.Key.FieldName.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().DisciplineName)));
            KeyValuePair<ColumnBase, string> commodityCodeData = pasteData.FirstOrDefault(x => x.Key.FieldName.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().CommodityCode)));
            KeyValuePair<ColumnBase, string> stockCodeData = pasteData.FirstOrDefault(x => x.Key.FieldName.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().StockCode)));
            KeyValuePair<ColumnBase, string> variationCodeData = pasteData.FirstOrDefault(x => x.Key.FieldName.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().VariationCode)));
            KeyValuePair<ColumnBase, string> budgetData = pasteData.FirstOrDefault(x => x.Key.FieldName.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().Budget)));

            pasteEntity.SubJobCode = subjobCodeData.Value.Trim();
            pasteEntity.SubJobTitle = subjobCodeTitleData.Value.Trim();
            pasteEntity.DisciplineCode = disciplineCodeData.Value.Trim();
            pasteEntity.DisciplineName = disciplineNameData.Value.Trim();
            pasteEntity.CommodityCode = commodityCodeData.Value.Trim();
            pasteEntity.StockCode = stockCodeData.Value.Trim();
            pasteEntity.VariationCode = variationCodeData.Value.Trim();

            decimal budgetValue = 0;
            if (decimal.TryParse(budgetData.Value, out budgetValue))
            {
                pasteEntity.Budget = budgetValue;
            }

            pasteEntity.PopulateCommodityCodes(COMMODITY_CODECollection);
            pasteEntity.PopulateStockCodes(STOCK_ITEMSCollection);
            pasteEntity.PopulateLineAuthUsers(DisplayEntities);

            string errorMessage = string.Empty;
            if (MainViewModel.IsValidEntity(pasteEntity, null, ref errorMessage))
            {
                if (commitToExo(pasteEntity, false))
                {
                    MainViewModel.Entities.Insert(0, pasteEntity);
                    if(isLastRow)
                        this.RaisePropertyChanged(x => x.DisplayEntities);
                }

                //remove restriction atm because user isn't familiar with system yet
                //if (pasteEntity.IsCommodityCodeValid)
                //{
                //    if(commitToExo(pasteEntity))
                //    {
                //        MainViewModel.Entities.Insert(0, pasteEntity);
                //        this.RaisePropertyChanged(x => x.DisplayEntities);
                //    }
                //}
                //else
                //{
                //    errorMessage = "Commodity code " + pasteEntity.CommodityCode + " does not belong to discipline code " + pasteEntity.DisciplineCode + " and phase type " + pasteEntity.PhaseTypeStr + "\nCurrent row will be skipped";
                //}
            }

            if(errorMessage != string.Empty)
            {
                if (errorMessage.ToUpper().Contains("UNIQUE"))
                    MessageBoxService.ShowMessage(pasteEntity.SubJobCode + " " + pasteEntity.DisciplineCode + " " + pasteEntity.CommodityCode + " " + pasteEntity.VariationCode + " is not unique\nCurrent row will be skipped", "Error", MessageButton.OK, MessageIcon.Exclamation);
                else
                    MessageBoxService.ShowMessage(errorMessage, "Error", MessageButton.OK, MessageIcon.Exclamation);
            }

            return false;
        }

        public override void UnifiedCellValueChanged(string field_name, object old_value, object new_value, ExoSubJobEditableProjection projection, bool isNew)
        {
            string errorMessage = string.Empty;
            projection.PopulateCommodityCodes(COMMODITY_CODECollection);
            projection.PopulateStockCodes(STOCK_ITEMSCollection);
            projection.Update();

            if (MainViewModel.IsValidEntity(projection, null, ref errorMessage) && projection.IsLineExistsInExo)
            {
                if (field_name.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().SubJobTitle)))
                {
                    commitSubJobTitle(projection);
                    viewOnlyUpdateSubJobTitle(projection, projection.SubJobCode, true);
                }
                else if (field_name.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().DisciplineName)))
                {
                    commitCostGroupName(projection);
                    viewOnlyUpdateCostGroupTitle(projection, projection.DisciplineCode, true);
                }
                else if (field_name.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().SubJobCode)))
                {
                    ExoMethods.CommitLineSubJob(projection, true, BulkColumnEditDialogService, masterJob, loadPROJECT.NUMBER, localPrimeroUnitOfWork);
                }
                else if (field_name.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().DisciplineCode)))
                {
                    ExoMethods.CommitLineDiscipline(projection, true, BulkColumnEditDialogService, masterJob, loadPROJECT.NUMBER, localPrimeroUnitOfWork);
                }
                else if (field_name.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().CommodityCode)))
                {                            
                    //stock item cannot be added, so it must exists before commodity can be added using it
                    string stockCode = projection.GetStockCode();
                    STOCK_ITEMS stock_item = ExoQueries.FindSTOCK_ITEM(localPrimeroUnitOfWork, stockCode);
                    if(stock_item != null)
                    {
                        projection.StockName = stock_item.DESCRIPTION;
                        if (ExoMethods.CommitLineCommodity(projection, stock_item, true, BulkColumnEditDialogService, masterJob, loadPROJECT.NUMBER, localPrimeroUnitOfWork))
                            this.RaisePropertyChanged(x => x.COMMODITY_CODEStringCollection);
                    }
                }
                else if(field_name.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().StockCode)))
                {
                    if(new_value != null)
                    {
                        STOCK_ITEMS stock_item = ExoQueries.FindSTOCK_ITEM(localPrimeroUnitOfWork, new_value.ToString());
                        projection.StockName = stock_item.DESCRIPTION;
                        if (ExoMethods.CommitLineCommodity(projection, stock_item, true, BulkColumnEditDialogService, masterJob, loadPROJECT.NUMBER, localPrimeroUnitOfWork))
                            this.RaisePropertyChanged(x => x.COMMODITY_CODEStringCollection);
                    }
                }
                else if (field_name.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().VariationCode)))
                {
                    commitLineVariation(projection);
                }
                else if (field_name.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().Budget)))
                {
                    commitLineBudgetCost(projection);
                }

                projection.Update();
            }

            base.UnifiedCellValueChanged(field_name, old_value, new_value, projection, isNew);
        }

        public override void UnifiedCellValueChanging(string field_name, object old_value, object new_value, ExoSubJobEditableProjection projection, bool isNew)
        {
            if (field_name.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().SubJobCode)))
            {
                if (new_value != null)
                {
                    string newSubJobCode = new_value.ToString();
                    //Cannot set property immediately because it must go through validation and revert to old value when it's not valid, hence passing in new value is necessary
                    viewOnlyUpdateSubJobTitle(projection, newSubJobCode, false);
                    projection.Update();
                }
            }
            else if (field_name.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().DisciplineCode)))
            {
                if (new_value != null)
                {
                    string newCostGroupCode = new_value.ToString();
                    //Cannot set property immediately because it must go through validation and revert to old value when it's not valid, hence passing in new value is necessary
                    viewOnlyUpdateCostGroupTitle(projection, newCostGroupCode, false);
                    projection.Update();
                }
            }

            base.UnifiedCellValueChanging(field_name, old_value, new_value, projection, isNew);
        }

        public void PermissionCellValueChanging(CellValueChangedEventArgs e)
        {
            //skip on new row
            if (e.RowHandle < 0)
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
                foreach (ExoSubJobEditableProjection selectedEntity in DisplaySelectedEntities.Where(x => x.IsLineExistsInExo && x.SubJobCode != null && x.SubJobId != null))
                {
                    ExoMethods.findExistingOrAddResourceAllocation(localPrimeroUnitOfWork, editingSubJobAuth, (int)selectedEntity.SubJobId);
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
                        ExoMethods.deleteResourceAllocation(localPrimeroUnitOfWork, editingSubJobAuth, (int)selectedEntity.SubJobId);
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

        private void viewOnlyUpdateSubJobTitle(ExoSubJobEditableProjection projection, string newSubJobCode, bool updateRelatedSubjobsEntries)
        {
            if (newSubJobCode == null)
                return;

            JOBCOST_HDR existingSubJob = existingSubJobs.FirstOrDefault(x => x.JOBCODE == newSubJobCode);
            if (existingSubJob != null)
            {
                projection.SubJobTitle = existingSubJob.TITLE;
                projection.Update();

                if (updateRelatedSubjobsEntries)
                {
                    foreach (ExoSubJobEditableProjection relatedProjection in DisplayEntities.Where(x => x.SubJobCode == newSubJobCode && x.IsLineExistsInExo))
                    {
                        relatedProjection.SubJobTitle = existingSubJob.TITLE;
                        relatedProjection.Update();
                    }
                }
            }
        }

        private void viewOnlyUpdateCostGroupTitle(ExoSubJobEditableProjection projection, string newCostGroupCode, bool updateRelatedDisciplineCodeEntries)
        {
            if (newCostGroupCode == null)
                return;

            JOB_COSTGROUPS existingCostGroup = costGroups.FirstOrDefault(x => x.SHORTCODE == newCostGroupCode);
            if (existingCostGroup != null)
            {
                projection.DisciplineName = existingCostGroup.COSTDESC;
                projection.Update();

                if (updateRelatedDisciplineCodeEntries)
                {
                    foreach (ExoSubJobEditableProjection relatedProjection in DisplayEntities.Where(x => x.DisciplineCode == newCostGroupCode))
                    {
                        relatedProjection.DisciplineName = projection.DisciplineName;
                        relatedProjection.Update();
                    }
                }
            }
        }

        /// <summary>
        /// Show commodity code even when it is not valid
        /// </summary>
        public void CustomColumnDisplayText(CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName == BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().CommodityCode) && e.Row != null)
            {
                ExoSubJobEditableProjection projection = (ExoSubJobEditableProjection)e.Row;
                if (!projection.IsCommodityCodeValid)
                {
                    //in new row this is called before property has been changed
                    if (e.Value != null)
                        e.DisplayText = e.Value.ToString();
                    else
                        e.DisplayText = projection.CommodityCode;
                }
            }
            else if (e.Column.FieldName == BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().StockCode) && e.Row != null)
            {
                ExoSubJobEditableProjection projection = (ExoSubJobEditableProjection)e.Row;
                if (!projection.IsStockCodeValid)
                {
                    //in new row this is called before property has been changed
                    if (e.Value != null)
                        e.DisplayText = e.Value.ToString();
                    else
                        e.DisplayText = projection.StockCode;
                }
            }
        }

        protected override void CellValueChangedImmediatePost(CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == (BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().CommodityCode)))
            {
                TableView tableView = e.Source as TableView;
                if (tableView != null && e.RowHandle != GridControl.NewItemRowHandle)
                {
                    tableView.CommitEditing();
                }
            }
        }

        public void CommitNewLinesToExo()
        {
            CommitToExo(DisplayEntities);
        }

        public void RemoveSelected()
        {
            List<ExoSubJobEditableProjection> removeProjections = DisplaySelectedEntities.Where(x => x.IsLineExistsInExo).ToList();
            if (MessageBoxService.ShowMessage("Are you sure you want to remove " + removeProjections.Count + " selected lines from exo?", "Confirmation", MessageButton.OKCancel) == MessageResult.Cancel)
                return;

            List<ExoSubJobEditableProjection> viewRemoveProjections = new List<ExoSubJobEditableProjection>();
            LoadingScreenManager.ShowLoadingScreen(removeProjections.Count);
            foreach (ExoSubJobEditableProjection removeProjection in removeProjections)
            {
                JOBCOST_LINES line = localPrimeroUnitOfWork.JOBCOST_LINES.First(x => x.SEQNO == removeProjection.LineId);
                if (line != null)
                {
                    localPrimeroUnitOfWork.JOBCOST_LINES.Remove(line);
                    localPrimeroUnitOfWork.SaveChanges();
                }
                viewRemoveProjections.Add(removeProjection);
                LoadingScreenManager.Progress();
            }

            LoadingScreenManager.CloseLoadingScreen();
            foreach (ExoSubJobEditableProjection viewRemoveProjection in viewRemoveProjections)
            {
                DisplayEntities.Remove(viewRemoveProjection);
            }

            this.FullRefresh();
        }

        public override string UnifiedRowValidation(ExoSubJobEditableProjection projection)
        {
            if (projection.SubJobCode == null || projection.SubJobCode == string.Empty)
                return "Sub Job not assigned";

            
            if (projection.DisciplineCode == null || projection.DisciplineCode == string.Empty)
                return "Discipline code not assigned";

            if (projection.CommodityCode == null || projection.CommodityCode == string.Empty)
                return "Commodity code not assigned";


            if (projection.SubJobCode.Length > subjobCodeMaxLength)
            {
                return "Sub Job code cannot be more than 15 characters";
            }

            if (projection.DisciplineCode.Length > disciplineCodeMaxLength)
            {
               return "Discipline code cannot be more than 4 characters";
            }

            if (projection.CommodityCode.Length > commodityCodeMaxLength)
            {
                return "Commodity code cannot be more than 4 characters";
            }

            return string.Empty;
        }

        public override string UnifiedValueValidation(ExoSubJobEditableProjection projection, string field_name, object new_value)
        {
            if (field_name.ToUpper().Contains("BUDGET"))
            {
                if(projection.guid != Guid.Empty)
                {
                    if (!LoginCredentials.hasPermission(PermissionResources.ChangeBudget))
                        return "You do not have authority to change the budget";
                    else if (!projection.IsLineExistsInExo)
                        return "Budget must be changed on bookable entries only";
                }
            }

            return string.Empty;
        }

        public void KeyboardCopy()
        {
            SendKeys.SendWait("^c");
        }

        public void KeyboardPaste()
        {
            SendKeys.SendWait("^v");
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

        protected void refreshPermissions()
        {
            isPermissionLoading = true;
            this.RaisePropertyChanged(x => x.IsPermissionLoading);

            this.RaisePropertyChanged(x => x.Users);
            this.RaisePropertyChanged(x => x.IsPermissionGridEnabled);
        }

        public override void FullRefresh()
        {
            initializeCompulsoryViewProperties(loadPROJECT);
            if(initializeOptionalViewCollectionsOnRefresh)
                initializeOptionalViewCollections();

            base.FullRefresh();
        }
#endregion

#region EXO Database
        private bool commitToExo(ExoSubJobEditableProjection projection, bool updatePermission = true)
        {
            List<ExoSubJobEditableProjection> newLines = new List<ExoSubJobEditableProjection>();
            ExoSubJobEditableProjection newLine = projection;
            newLines.Add(newLine);
            return CommitToExo(newLines, updatePermission);
        }

        public bool CommitToExo(IEnumerable<ExoSubJobEditableProjection> projections, bool updatePermission = true)
        {
            if(masterJob == null)
            {
                MessageBoxService.ShowMessage("Cannot upload to exo because job " + loadPROJECT.NUMBER + " is not created\nPlease contact " + BluePrintsResources.Default_CFO + " to add project", "Warning", MessageButton.OK, MessageIcon.Exclamation);
                return false;
            }

            //if (masterJob.CATEGORY == null || ((int)masterJob.CATEGORY) >= 5)
            //{
            //    MessageBoxService.ShowMessage("Uploading to EXO is disabled because this job is in tender phase\nPlease contact " + BluePrintsResources.Default_CFO + " to change project category", "Warning", MessageButton.OK, MessageIcon.Exclamation);
            //    return false;
            //}

            if (masterJob == null)
            {
                MessageBoxService.ShowMessage("Project master job doesn't exists in EXO\nPlease request " + BluePrintsResources.Default_CFO + " to add a job with job code " + loadPROJECT.NUMBER, "Warning", MessageButton.OK, MessageIcon.Exclamation);
                return false;
            }

            if (copyLine == null)
            {
                MessageBoxService.ShowMessage("Project master line is not setup in exo\nPlease request " + BluePrintsResources.Default_CFO + " to add a job line linked to master job with job code " + loadPROJECT.NUMBER, "Warning", MessageButton.OK, MessageIcon.Exclamation);
                return false;
            }

            if (projections.Any(x => x.SubJobCode == null) || projections.Any(x => x.SubJobCode == string.Empty) || projections.Any(x => !Regex.IsMatch(x.SubJobCode, SubJobRegex)))
            {
                MessageBoxService.ShowMessage("Some lines have invalid subjob code", "Warning", MessageButton.OK, MessageIcon.Exclamation);
                return false;
            }
            else if (projections.Any(x => x.SubJobCode.Length > subjobCodeMaxLength))
            {
                MessageBoxService.ShowMessage("Some lines have subjob code that is more than 15 characters", "Warning", MessageButton.OK, MessageIcon.Exclamation);
                return false;
            }

            if (projections.Any(x => x.DisciplineCode == null) || projections.Any(x => x.DisciplineCode == string.Empty) || projections.Any(x => !Regex.IsMatch(x.DisciplineCode, DisciplineRegex)))
            {
                MessageBoxService.ShowMessage("Some lines have invalid subjob code", "Warning", MessageButton.OK, MessageIcon.Exclamation);
                return false;
            }
            else if (projections.Any(x => x.DisciplineCode.Length > disciplineCodeMaxLength))
            {
                MessageBoxService.ShowMessage("Some lines have discipline code that is more than 4 characters", "Warning", MessageButton.OK, MessageIcon.Exclamation);
                return false;
            }


            if (projections.Any(x => x.CommodityCode == null) || projections.Any(x => x.CommodityCode == string.Empty))
            {
                MessageBoxService.ShowMessage("Some lines doesn't have commodity code", "Warning", MessageButton.OK, MessageIcon.Exclamation);
                return false;
            }
            else if (projections.Any(x => x.CommodityCode.Length > commodityCodeMaxLength))
            {
                MessageBoxService.ShowMessage("Some lines have commodity code that is more than 4 characters", "Warning", MessageButton.OK, MessageIcon.Exclamation);
                return false;
            }

            //Commodity code doesn't have to be valid for now
            //else if(projections.Any(x => !x.IsCommodityCodeValid))
            //{
            //    MessageBoxService.ShowMessage("Some lines have commodity code that doesn't match discipline code and phase", "Warning", MessageButton.OK, MessageIcon.Exclamation);
            //    return false;
            //}

            int updatedLineCount = 0;
            List<ExoSubJobEditableProjection> addedProjections = new List<ExoSubJobEditableProjection>();
            foreach (ExoSubJobEditableProjection projection in projections)
            {
                if (projection.PhaseType != null && projection.PhaseType == PhaseType.Design && projection.CommodityIsIndirectOnly)
                {
                    MessageBoxService.ShowMessage("The commodity " + projection.CommodityCode + " can only be assigned to indirect subjobs\nPlease change the subjob or assign a different commodity in the deliverable's list", "Warning", MessageButton.OK, MessageIcon.Exclamation);
                    continue;
                }

                if (!projection.IsLineExistsInExo)
                {
                    if (ExoMethods.CommitLineSubJob(projection, false, BulkColumnEditDialogService, masterJob, loadPROJECT.NUMBER, localPrimeroUnitOfWork))
                    {
                        if (ExoMethods.CommitLineDiscipline(projection, false, BulkColumnEditDialogService, masterJob, loadPROJECT.NUMBER, localPrimeroUnitOfWork))
                        {
                            //stock item cannot be added, so it must exists before commodity can be added using it
                            string stockCode = projection.GetStockCode();
                            STOCK_ITEMS stock_item = ExoQueries.FindSTOCK_ITEM(localPrimeroUnitOfWork, stockCode);
                            if (stock_item != null)
                            {
                                projection.StockName = stock_item.DESCRIPTION;
                                if (ExoMethods.CommitLineCommodity(projection, stock_item, false, BulkColumnEditDialogService, masterJob, loadPROJECT.NUMBER, localPrimeroUnitOfWork))
                                {
                                    JOBCOST_LINES findExistingOrAddLine = ExoMethods.findExistingOrAddLine(localPrimeroUnitOfWork, projection, copyLine, loadPROJECT.NUMBER);
                                    projection.LineId = findExistingOrAddLine.SEQNO;
                                    if (projection.LineId != null)
                                    {
                                        ExoSubJobEditableProjection existingSameSubJobLine = DisplayEntities.FirstOrDefault(x => x.SubJobId == projection.SubJobId);
                                        if (existingSameSubJobLine != null)
                                        {
                                            foreach (ExoSubJobAuth authUser in existingSameSubJobLine.AuthUsers)
                                            {
                                                ExoSubJobAuth newUser = new ExoSubJobAuth();
                                                DataUtils.ShallowCopy(newUser, authUser);
                                                projection.AuthUsers.Add(newUser);
                                            }
                                        }

                                        if(updatePermission)
                                            refreshPermissions();

                                        addedProjections.Add(projection);
                                        updatedLineCount += 1;
                                    }

                                    projection.Update();
                                }
                                else
                                {
                                    MessageBoxService.ShowMessage(projection.CommodityCode + " cost type does not exists in exo, please request it from " + BluePrintsResources.Default_CFO);
                                    continue;
                                }
                            }
                        }
                    }
                }
            }

            if(addedProjections.Count() > 0)
            {
                //MessageBoxService.ShowMessage(updatedLineCount + " line(s) added");
                OnAfterNewRowAdded(addedProjections.First());
                //Refreshes collection properties
                this.RaisePropertiesChanged();
                return true;
            }

            return false;
        }

        private bool commitLineVariation(ExoSubJobEditableProjection projection)
        {
            if (projection.LineId != null)
            {
                JOBCOST_LINES line = localPrimeroUnitOfWork.JOBCOST_LINES.First(x => x.SEQNO == projection.LineId);
                if (line != null)
                {
                    line.X_VARIATION_CODE = projection.VariationCode;
                    localPrimeroUnitOfWork.SaveChanges();
                    return true;
                }
            }

            return false;
        }

        private bool commitLineBudgetCost(ExoSubJobEditableProjection projection)
        {
            if (projection.LineId != null)
            {
                JOBCOST_LINES line = localPrimeroUnitOfWork.JOBCOST_LINES.First(x => x.SEQNO == projection.LineId);
                if (line != null)
                {
                    line.QUOTE_QTY = 1;
                    line.ACTUAL_UNITCOST = Convert.ToDouble(projection.Budget);
                    localPrimeroUnitOfWork.SaveChanges();
                    return true;
                }
            }

            return false;
        }

        private void commitSubJobTitle(ExoSubJobEditableProjection projection)
        {
            JOBCOST_HDR existingSubJob = ExoQueries.GetProjectSubJob(localPrimeroUnitOfWork, loadPROJECT.NUMBER, projection.SubJobCode);
            if (existingSubJob == null)
            {
                MessageBoxService.ShowMessage(projection.SubJobCode + subJobMissingError);
                return;
            }

            existingSubJob.TITLE = projection.SubJobTitle;
            localPrimeroUnitOfWork.SaveChanges();

            existingSubJobs = ExoQueries.GetProjectSubJobs(localPrimeroUnitOfWork, loadPROJECT.NUMBER);
        }

        private void commitCostGroupName(ExoSubJobEditableProjection projection)
        {
            JOB_COSTGROUPS costGroup = costGroups.FirstOrDefault(x => x.SHORTCODE == projection.DisciplineCode);
            if (costGroup == null)
            {
                MessageBoxService.ShowMessage(projection.DisciplineCode + costGroupMissingError);
                return;
            }

            costGroup.COSTDESC = projection.DisciplineName;
            localPrimeroUnitOfWork.SaveChanges();

            costGroups = ExoQueries.GetCostGroups(localPrimeroUnitOfWork);
        }
#endregion

#region View Properties
        ESTIMATE_ITEMCollectionViewModelWrapper estimateItemViewModel;
        public void CopyToJob()
        {
            if (MessageBoxService.ShowMessage("This will make a copy of existing jobs in exo (except design) into job setup, do you wish to continue?", "Confirmation", MessageButton.OKCancel, MessageIcon.Information) == MessageResult.OK)
            {
                estimateItemViewModel = ESTIMATE_ITEMCollectionViewModelWrapper.Create();
                estimateItemViewModel.OnEntitiesLoadedCallBack = onEstimateLoaded;
                estimateItemViewModel.OnEntitiesLoadedCallBackManualDispose = true;
                estimateItemViewModel.OnParameterChange(new TripleEntitiesParameter<Data.PROJECT, IAmBaseline, object>(loadPROJECT, null, new KeyValuePair<DeliverablesViewType, EstimateViewMode>(DeliverablesViewType.Direct, EstimateViewMode.Budget)));

                LoadingScreenManager.ShowLoadingScreen(DisplayEntities.Count);
            }
        }

        private void onEstimateLoaded(IEnumerable<ESTIMATE_ITEMProgress> estimateItems, object parent_id)
        {
            mainThreadDispatcher.BeginInvoke(new Action(() => {
                int saveCount = 0;
                List<ESTIMATE_ITEMProgress> newESTIMATE_ITEMS = new List<ESTIMATE_ITEMProgress>();
                foreach (var entity in DisplayEntities)
                {
                    if (entity.SubJobCode.Length >= 15 && entity.DisciplineCode.Length >= 4 && entity.CommodityCode != string.Empty)
                    {
                        string phaseCode = entity.SubJobCode.Substring(13, 2);
                        if (!phaseCode.ToUpper().Contains("D") && !phaseCode.ToUpper().Contains("P"))
                        {
                            string disciplineCode = entity.DisciplineCode.Substring(0, 2);
                            string disciplineNum = entity.DisciplineCode.Substring(2, 2);
                            string areaName = entity.SubJobCode.Substring(6, 3);
                            string subAreaName = entity.SubJobCode.Substring(10, 2);

                            string fullDisciplineCode = string.Concat(disciplineCode, disciplineNum);
                            string fullWBSCode = entity.SubJobCode + "-" + fullDisciplineCode + "-" + entity.CommodityCode;

                            ESTIMATE_ITEMProgress findESTIMATE_ITEM = estimateItems.FirstOrDefault(x => x.Deliverable_Name.ToUpper() == fullWBSCode.ToUpper() && x.Variation_Code.ToUpper() == entity.VariationCode.ToUpper());
                            if (findESTIMATE_ITEM == null)
                            {
                                ESTIMATE_ITEM newESTIMATE_ITEM = new ESTIMATE_ITEM();
                                Data.PHASE findPHASE = estimateItemViewModel.PHASECollection.FirstOrDefault(x => x.INTERNAL_NUM.ToUpper() == phaseCode);
                                DISCIPLINE findDISCIPLINE = estimateItemViewModel.DISCIPLINECollection.FirstOrDefault(x => x.CODE == disciplineCode);
                                int disciplineInt = 1;
                                if (findPHASE != null && Int32.TryParse(disciplineNum, out disciplineInt))
                                {
                                    newESTIMATE_ITEM.GUID = Guid.Empty;
                                    newESTIMATE_ITEM.GUID_PHASE = findPHASE.GUID;
                                    newESTIMATE_ITEM.GUID_AREA = estimateItemViewModel.FindExistingOrAddNewArea(areaName);
                                    newESTIMATE_ITEM.GUID_SUBAREA = estimateItemViewModel.FindExistingOrAddNewSubArea((Guid)newESTIMATE_ITEM.GUID_AREA, subAreaName);
                                    newESTIMATE_ITEM.GUID_DISCIPLINE = estimateItemViewModel.FindExistingOrAddNewDiscipline(disciplineCode);
                                    newESTIMATE_ITEM.DISCIPLINE_NUM = disciplineInt;
                                    newESTIMATE_ITEM.COMMODITY_CODE = entity.CommodityCode;
                                    newESTIMATE_ITEM.VARIATION_CODE = entity.VariationCode;

                                    ESTIMATE_ITEMProgress projection = new ESTIMATE_ITEMProgress();
                                    projection.Entity = new ESTIMATE_ITEMProjection();
                                    projection.Entity.Entity = newESTIMATE_ITEM;
                                    newESTIMATE_ITEMS.Add(projection);
                                    saveCount += 1;
                                }
                            }
                        }
                    }

                    LoadingScreenManager.Progress();
                }

                LoadingScreenManager.CloseLoadingScreen();
                estimateItemViewModel.BulkSave(newESTIMATE_ITEMS);
                estimateItemViewModel.Dispose();
                MessageBoxService.ShowMessage(saveCount + " jobs generated in job setup", "Information", MessageButton.OK, MessageIcon.Information);
            }));
        }

        protected bool isPermissionLoading;

        //if user clicks on an autofilter row and isPermissionLoading is true it won't be set to false ever and this can freeze up the view
        public bool IsPermissionLoading => !IsPermissionGridEnabled ? false : isPermissionLoading;

        public ExoSubJobAuth SelectedUser { get; set; }
        List<ExoSubJobAuth> orderedAuthUsers;
        public virtual IEnumerable<ExoSubJobAuth> Users
        {
            get
            {
                if (MainViewModel == null || !IsPermissionGridEnabled)
                    return null;

                var permissions = new List<ExoSubJobAuth>();
                if (DisplaySelectedEntities == null && MainViewModel.Entities.Count > 0)
                    DisplaySelectedEntities.Add(MainViewModel.Entities.First());

                if (DisplaySelectedEntities == null || DisplaySelectedEntities.Count == 0)
                    return null;

                if (orderedAuthUsers == null)
                {
                    orderedAuthUsers = new List<ExoSubJobAuth>();
                    foreach (STAFF staff in exoSTAFFS)
                    {
                        ExoSubJobAuth displayUserAuth = new ExoSubJobAuth();

                        USER newUser = null;
                        if (tryCombineLocalUsers)
                            newUser = USERCollection.FirstOrDefault(x => x.EXO_STAFF_ID == staff.STAFFNO);

                        if (newUser == null)
                            newUser = new USER();

                        if (!orderedAuthUsers.Any(x => x.User.EXO_STAFF_ID == staff.STAFFNO))
                        {
                            newUser.NAME = staff.NAME;
                            newUser.EXO_STAFF_ID = staff.STAFFNO;
                            newUser.TITLE = newUser.TITLE != null && newUser.TITLE != string.Empty ? newUser.TITLE : staff.JOBTITLE;
                            newUser.SecurityProfileID = staff.SECURITYPROFILEID;
                            displayUserAuth.User = newUser;

                            orderedAuthUsers.Add(displayUserAuth);
                        }
                    }
                }

                foreach(ExoSubJobAuth authorisation in orderedAuthUsers)
                {
                    if (DisplaySelectedEntities.All(x => x.AuthUsers.Any(y => y.User.EXO_STAFF_ID == authorisation.User.EXO_STAFF_ID)))
                        authorisation.IsAssigned = true;
                    else if (DisplaySelectedEntities.Any(x => x.AuthUsers.Any(y => y.User.EXO_STAFF_ID == authorisation.User.EXO_STAFF_ID)))
                        authorisation.IsAssigned = null;
                    else
                        authorisation.IsAssigned = false;

                    authorisation.ShouldAssign = false;
                }
                

                isPermissionLoading = false;
                this.RaisePropertyChanged(x => x.IsPermissionLoading);
                permissions.AddRange(orderedAuthUsers.OrderBy(x => x.User.Full_Name));
                return permissions;
            }
        }

        public override void ShowNotification()
        {
            if (AppNotificationService == null)
                return;

            INotification notification1 = AppNotificationService.CreatePredefinedNotification("Exo is connected to " + loadPROJECT.OfficeNameForExo, null, null, null);
            notification1.ShowAsync();
        }

        public override string ViewName
        {
            get
            {
                //return "BASELINE_ITEMSViewModelWrapper" + view_project_specific_affix;
                return "EXO_SubjobCollectionViewModelWrapper";
            }
        }

        public string SubJobRegex { get; set; }
        public string DisciplineRegex { get; set; }
        public IEnumerable<USER> USERCollection
        {
            get
            {
                var collection = GetEntities<USER>();

                List<USER> returnSTAFF = new List<USER>();
                foreach(USER user in collection)
                {
                    STAFF staff = exoSTAFFS.FirstOrDefault(x => x.STAFFNO == user.EXO_STAFF_ID);
                    //don't return any user that is disabled in EXO
                    if (staff != null)
                    {
                        //if(staff.SECURITYPROFILEID != 4)
                        returnSTAFF.Add(user);
                    }
                }
                
                return returnSTAFF.OrderBy(x => x.NAME);
            }
        }

        public IEnumerable<string> COMMODITY_CODEStringCollection
        {
            get
            {
                var collection = GetEntities<COMMODITY_CODE>();
                List<string> allCommodityCodes = new List<string>();
                if (collection != null)
                {
                    allCommodityCodes.AddRange(collection.OrderBy(x => x.CODE).Distinct().Select(x => x.CODE).Distinct());
                }

                //add commodity code from entities so that even commodity code that isn't valid will be displayed
                if(DisplayEntities != null && DisplayEntities.Count > 0)
                {
                    allCommodityCodes.AddRange(DisplayEntities.Select(x => x.CommodityCode));
                }

                return allCommodityCodes.OrderBy(x => x);
            }
        }

        public IEnumerable<PrimeroData.PROFILE> PROFILECollection
        {
            get
            {
                var collection = GetEntities<PrimeroData.PROFILE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.PROFILENAME);
                return collection;
            }
        }

        public IEnumerable<PrimeroData.STOCK_ITEMS> STOCK_ITEMSCollection
        {
            get
            {
                var collection = GetEntities<PrimeroData.STOCK_ITEMS>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.STOCKCODE);
                return collection;
            }
        }

        public IEnumerable<string> STOCK_CODEStringCollection
        {
            get
            {
                var collection = GetEntities<PrimeroData.STOCK_ITEMS>();
                List<string> allStockCodeRanges = new List<string>();
                if (collection != null)
                {
                    allStockCodeRanges.AddRange(collection.OrderBy(x => x.STOCKCODE).Distinct().Select(x => x.STOCKCODE).Distinct());
                }

                //add commodity code from entities so that even commodity code that isn't valid will be displayed
                if (DisplayEntities != null && DisplayEntities.Count > 0)
                {
                    allStockCodeRanges.AddRange(DisplayEntities.Select(x => x.StockCode));
                }

                return allStockCodeRanges.OrderBy(x => x);
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
#endregion
    }
}