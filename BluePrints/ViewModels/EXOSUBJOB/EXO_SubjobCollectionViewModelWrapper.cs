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
using BluePrints.Common.Helpers;
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
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Printing;
using DevExpress.XtraEditors;
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
    public partial class EXO_SubjobCollectionViewModelWrapper : BluePrintsEntitiesCollectionWrapper<BASELINE_ITEM, ExoSubJobProjection, Guid, IBluePrintsEntitiesUnitOfWork>
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
        protected IPrimeroEntitiesUnitOfWork localPrimeroUnitOfWork;
        protected IBluePrintsEntitiesUnitOfWork bluePrintsEntitiesUnitOfWork;
        protected IEnumerable<JOB_COSTGROUPS> costGroups;
        protected IEnumerable<JOBCOST_HDR> existingSubJobs;
        protected JOBCOST_HDR masterJob;
        protected JOBCOST_LINES copyLine;
        protected bool runPlannedChecker = false;
        protected bool initializeOptionalViewCollectionsOnRefresh = true;
        public CriteriaOperator FilterCriteria { get; set; }
        //user from exo will do a lookup to get additional details from user's in BluePrints
        protected bool tryCombineLocalUsers = false;
        public string SubJobRegex { get; set; }
        public string DisciplineRegex { get; set; }
        public bool IgnoreCostGroupCostType { get; set; }
        public bool ValidateDesignDirectIndirect { get; set; }
        protected bool ignoreExoBudgetError { get; set; }
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
            ignoreExoBudgetError = true;
            bluePrintsEntitiesUnitOfWork = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            initializeOptionalViewCollections();
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, COMMODITY_CODEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.USERS, USERProjectionFunc);
            loaderCollection.AddLoaderDescription<PrimeroData.PROFILE, PrimeroData.PROFILE, int, IPrimeroEntitiesUnitOfWork>(localPrimeroUnitOfWorkFactory, x => x.PROFILE);
            loaderCollection.AddLoaderDescription<PrimeroData.STOCK_ITEMS, PrimeroData.STOCK_ITEMS, string, IPrimeroEntitiesUnitOfWork>(localPrimeroUnitOfWorkFactory, x => x.STOCK_ITEMS);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.FORECAST_JOBS, FORECAST_JOBProjectionFunc);
        }

        protected virtual Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>> COMMODITY_CODEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == null);
        }

        protected virtual Func<IRepositoryQuery<FORECAST_JOB>, IQueryable<FORECAST_JOB>> FORECAST_JOBProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected virtual Func<IRepositoryQuery<USER>, IQueryable<USER>> USERProjectionFunc()
        {
            return query => query.Where(x => x.LEAVE_DATE == null || x.LEAVE_DATE > DateTime.Now);
        }

        protected void initializeCompulsoryViewProperties(Data.PROJECT project)
        {
            localPrimeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(loadPROJECT.OfficeNameForExo);
            localPrimeroUnitOfWork = localPrimeroUnitOfWorkFactory.CreateUnitOfWork();

            masterJob = ExoQueries.GetProjectSubJob(localPrimeroUnitOfWork, project.NUMBER);
            copyLine = ExoQueries.GetAnyProjectLineByJobNumber(localPrimeroUnitOfWork, project.NUMBER);

            List<STAFF> localSTAFFS = ExoQueries.GetStaffs(localPrimeroUnitOfWork).ToList();
            //List<STAFF> remoteSTAFFS = ExoQueries.GetStaffs(remotePrimeroUnitOfWork).ToList();
            //do not add remote staff so it's easy to troubleshoot when user is not added into specific locale authorisation
            exoSTAFFS = new List<STAFF>();
            exoSTAFFS.AddRange(localSTAFFS);
            //exoSTAFFS.AddRange(remoteSTAFFS);
        }

        protected void initializeOptionalViewCollections()
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

        private void updateBudgetedSubJobs(IEnumerable<ExoSubJobProjection> designSubjobs, object parent_id)
        {
            if (MainViewModel == null)
                return;

            foreach (ExoSubJobProjection designSubjob in designSubjobs)
            {
                ExoSubJobProjection findSubJob = Entities.FirstOrDefault(x => x.SubJobCode == designSubjob.SubJobCode && x.DisciplineCode == designSubjob.DisciplineCode && x.CommodityCode == designSubjob.CommodityCode);
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
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<ExoSubJobProjection>> specifyMainViewModelProjection()
        {
            return query => ExoQueries.GetNativeExoSubJobEditableProjection(localPrimeroUnitOfWork, loadPROJECT, COMMODITY_CODECollection, STOCK_ITEMSCollection, exoSTAFFS, loadPROJECT.OfficeNameForExo, true);
        }
       
        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<ExoSubJobProjection> entities)
        {
            MainViewModel.AlwaysSkipMessage = this.AlwaysSkipMessage;
            MainViewModel.SetParentViewModel(this);

            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        //only populate lookup edit items source with validated entries during pop up opening to reduce memory footprint
        public void CommodityCodePopupOpening(OpenPopupEventArgs e)
        {
            DevExpress.Xpf.Grid.LookUp.LookUpEdit sender = e.Source as DevExpress.Xpf.Grid.LookUp.LookUpEdit;
            if(SelectedEntity != null)
                sender.ItemsSource = SelectedEntity.TaggedValidCommodityCodes;
        }

        //only populate lookup edit items source with validated entries during pop up opening to reduce memory footprint
        public void StockCodePopupOpening(OpenPopupEventArgs e)
        {
            DevExpress.Xpf.Grid.LookUp.LookUpEdit sender = e.Source as DevExpress.Xpf.Grid.LookUp.LookUpEdit;
            sender.ItemsSource = SelectedEntity.TaggedValidStockItems;
        }

        public override void UnifiedNewRowInitializationFromView(ExoSubJobProjection projection)
        {
            projection.PopulateCommodityCodes(COMMODITY_CODECollection);
            projection.PopulateStockItems(STOCK_ITEMSCollection);
            projection.IgnoreExoBudgetError = ignoreExoBudgetError;
            base.UnifiedNewRowInitializationFromView(projection);
        }

        protected override OperationInterceptMode OnBeforeProjectionSaveIsContinue(ExoSubJobProjection projection, out bool isNew)
        {
            isNew = false;
            if (UnifiedRowValidation(projection) == string.Empty)
            {
                ExoSubJobProjection newlyAddedProjection = commitToExo(projection);
                if (newlyAddedProjection != null)
                {
                    isNew = true;
                    newlyAddedProjection.IgnoreExoBudgetError = ignoreExoBudgetError;
                }
            }

            return OperationInterceptMode.SkipOneAndAllDbSaves;
        }

        protected override OperationInterceptMode OnBeforeProjectionDeleteIsContinue(ExoSubJobProjection projection, out List<ErrorMessage> errorMessages)
        {
            errorMessages = new List<ErrorMessage>();
            if(FORECAST_JOBCollection.Any(x => x.SUBJOB_CODE == projection.SubJobCode && x.DISCIPLINE_CODE == projection.DisciplineCode && x.COMMODITY_CODE == projection.CommodityCode && x.VARIATION_CODE == projection.VariationCode))
            {
                if (MessageBoxService.ShowMessage(projection.FullCode + " Exists in indirect forecast\nIf you delete this line it'll be removed from indirect forecast\n\nAre you sure you wish to continue?", "Confirmation", MessageButton.YesNo) == MessageResult.No)
                    return OperationInterceptMode.SkipAll;
            }

            if (!projection.IsLineExistsInExo)
                return OperationInterceptMode.SkipOneAndAllDbSaves;

            removeFromExo(projection);
            return OperationInterceptMode.SkipOneAndAllDbSaves;
        }

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
        #endregion

        #region Events
        public override string UnifiedRowValidation(ExoSubJobProjection projection)
        {
            if (projection.SubJobCode == null || projection.SubJobCode == string.Empty)
                return "Sub Job not assigned";

            if (!IgnoreCostGroupCostType)
            {
                if(projection.StockCode != BluePrintsResources.VariationStockCode)
                {
                    //only validate cost group and cost type for new row
                    if(projection.LineId == null)
                    {
                        if (projection.DisciplineCode == null || projection.DisciplineCode == string.Empty)
                            return "Discipline code not assigned";

                        if (projection.CommodityCode == null || projection.CommodityCode == string.Empty)
                            return "Commodity code not assigned";

                        if ((projection.DisciplineCode != null && projection.DisciplineCode != string.Empty) && projection.DisciplineCode.Length > 4)
                            return "Discipline code cannot be more than 4 characters";

                        if ((projection.CommodityCode != null && projection.CommodityCode != string.Empty) && projection.CommodityCode.Length > 4)
                            return "Commodity code cannot be more than 4 characters";
                    }
                }

                if ((projection.DisciplineCode != null && projection.DisciplineCode != string.Empty) && projection.SubJobCode.Length > 15)
                {
                    return "Sub Job code cannot be more than 15 characters";
                }
            }

            if (projection.LineId == null && Entities.Any(x => x.LineId != null && x.SubJobCode == projection.SubJobCode && x.DisciplineCode == projection.DisciplineCode && x.CommodityCode == projection.CommodityCode && x.StockCode == projection.StockCode && x.VariationCode == projection.VariationCode))
            {
                return "Duplicate Subjob: " + formatCodeError(projection.SubJobCode) + " Discipline: " + formatCodeError(projection.DisciplineCode) + " Commodity: " + formatCodeError(projection.CommodityCode) + " Variation: " + formatCodeError(projection.VariationCode);
            }

            if (!projection.IsCommodityCodeValid)
                return "Invalid commodity code. Validity can be maintained in Data -> Commodity Codes";

            if(!projection.IsStockCodeValid)
                return "Invalid stock code. Validity can be maintained in Data -> Commodity Codes";

            return string.Empty;
        }

        private string formatCodeError(string code)
        {
            if (code == null || code == string.Empty)
                return "(Blank)";

            return code;
        }

        public override void UnifiedCellValueChanged(string field_name, object old_value, object new_value, ExoSubJobProjection projection, bool isNew)
        {
            string errorMessage = string.Empty;
            projection.Update();

            List<KeyValuePair<string, string>> constraintIssues;
            if (MainViewModel.IsValidEntity(projection, null, ref errorMessage, out constraintIssues) && projection.IsLineExistsInExo)
            {
                CommonMethods.SubJobLineValueChanged(field_name, old_value, new_value, projection, Entities, isNew, loadPROJECT.NUMBER, localPrimeroUnitOfWork, bluePrintsEntitiesUnitOfWork, MessageBoxService, BulkColumnEditDialogService, masterJob, () => this.RaisePropertyChanged(x => x.COMMODITY_CODEStringCollection), () => this.RaisePropertyChanged(x => x.STOCK_CODEStringCollection));
            }

            base.UnifiedCellValueChanged(field_name, old_value, new_value, projection, isNew);
        }

        public override void UnifiedCellValueChanging(string field_name, object old_value, object new_value, ExoSubJobProjection projection, bool isNew)
        {
            if (field_name.Contains(BindableBase.GetPropertyName(() => new ExoSubJobProjection().SubJobCode)))
            {
                if (new_value != null)
                {
                    string newSubJobCode = new_value.ToString();
                    //Cannot set property immediately because it must go through validation and revert to old value when it's not valid, hence passing in new value is necessary
                    ExoMethods.ViewUpdateSubJobTitle(projection, Entities, localPrimeroUnitOfWork, loadPROJECT.NUMBER, newSubJobCode, false);
                    projection.Update();
                }
            }
            else if (field_name.Contains(BindableBase.GetPropertyName(() => new ExoSubJobProjection().DisciplineCode)))
            {
                if (new_value != null)
                {
                    string newCostGroupCode = new_value.ToString();
                    //Cannot set property immediately because it must go through validation and revert to old value when it's not valid, hence passing in new value is necessary
                    ExoMethods.ViewUpdateCostGroupTitle(projection, Entities, localPrimeroUnitOfWork, newCostGroupCode, false);
                    projection.Update();
                }
            }

            base.UnifiedCellValueChanging(field_name, old_value, new_value, projection, isNew);
        }

        /// <summary>
        /// Show commodity code even when it is not valid
        /// </summary>
        public void CustomColumnDisplayText(CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName == BindableBase.GetPropertyName(() => new ExoSubJobProjection().CommodityCode) && e.Row != null)
            {
                ExoSubJobProjection projection = (ExoSubJobProjection)e.Row;
                if (!projection.IsCommodityCodeValid)
                {
                    //in new row this is called before property has been changed
                    if (e.Value != null)
                        e.DisplayText = e.Value.ToString();
                    else
                        e.DisplayText = projection.CommodityCode;
                }
            }
            else if (e.Column.FieldName == BindableBase.GetPropertyName(() => new ExoSubJobProjection().StockCode) && e.Row != null)
            {
                ExoSubJobProjection projection = (ExoSubJobProjection)e.Row;
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
            if (e.Column.FieldName == (BindableBase.GetPropertyName(() => new ExoSubJobProjection().VariationCode)))
            {
                TableView tableView = e.Source as TableView;
                if (tableView != null && e.RowHandle != GridControl.NewItemRowHandle)
                {
                    tableView.CommitEditing();
                }
            }
        }

        private void removeFromExo(ExoSubJobProjection projection)
        {
            List<ExoSubJobProjection> removeLines = new List<ExoSubJobProjection>();
            ExoSubJobProjection newLine = projection;
            removeLines.Add(newLine);
            removeFromExo(removeLines);
        }

        private void removeFromExo(IEnumerable<ExoSubJobProjection> removeProjections)
        {
            List<ExoSubJobProjection> viewRemoveProjections = new List<ExoSubJobProjection>();
            foreach (ExoSubJobProjection removeProjection in removeProjections)
            {
                JOBCOST_LINES line = localPrimeroUnitOfWork.JOBCOST_LINES.FirstOrDefault(x => x.SEQNO == removeProjection.LineId);
                if (line != null)
                {
                    ExoMethods.UpdateJOBCOST_LINES_AUDIT(bluePrintsEntitiesUnitOfWork, removeProjection, line, true);
                    localPrimeroUnitOfWork.JOBCOST_LINES.Remove(line);
                    removeProjection.LineId = null;
                    removeProjection.IsLineExistsInExo = false;
                    localPrimeroUnitOfWork.SaveChanges();
                }

                viewRemoveProjections.Add(removeProjection);
            }

            foreach (ExoSubJobProjection viewRemoveProjection in viewRemoveProjections)
            {
                Entities.Remove(viewRemoveProjection);
            }
        }

        public override string UnifiedValueValidation(ExoSubJobProjection projection, string field_name, object new_value, bool isPaste)
        {
            if (field_name == BindableBase.GetPropertyName(() => new ExoSubJobProjection().Budget))
            {
                if(new_value != null && new_value.ToString() != string.Empty)
                {
                    if (LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Permission_EXO_ChangeBudget)) == LoginCredentials.PermissionStatus.None)
                        return "You do not have authority to set or change budget";
                }
            }
            else if(field_name == BindableBase.GetPropertyName(() => new ExoSubJobProjection().VariationCode))
            {
                if (new_value != null && new_value.ToString().Length > 50)
                {
                    return "Variation code cannot be more than 50 characters";
                }
            }
            else if(field_name == BindableBase.GetPropertyName(() => new ExoSubJobProjection().SubJobCode))
            {
                if (new_value == null || new_value.ToString() == string.Empty)
                    return "Sub Job not assigned";
                if ((projection.DisciplineCode != null && projection.DisciplineCode != string.Empty) && new_value.ToString().Length > 15)
                {
                    return "Sub Job code cannot be more than 15 characters";
                }
            }

            //only validate cost group and cost type for new row
            if (!IgnoreCostGroupCostType)
            {
                if (field_name == BindableBase.GetPropertyName(() => new ExoSubJobProjection().DisciplineCode))
                {
                    if (new_value.ToString() == null || new_value.ToString() == string.Empty)
                        return "Discipline code not assigned";
                    else if ((new_value != null && new_value.ToString() != string.Empty) && new_value.ToString().Length > 4)
                        return "Discipline code cannot be more than 4 characters";
                }
                else if (field_name == BindableBase.GetPropertyName(() => new ExoSubJobProjection().CommodityCode))
                {
                    if (new_value.ToString() == null || new_value.ToString() == string.Empty)
                        return "Commodity code not assigned";
                    else if (new_value.ToString().Length > 4)
                        return "Commodity code cannot be more than 4 characters";
                    if (!projection.IsValidCommodityCode(new_value.ToString()))
                        return "Invalid commodity code. Validity can be maintained in Data -> Commodity Codes";
                }
                else if (field_name == BindableBase.GetPropertyName(() => new ExoSubJobProjection().StockCode))
                {
                    if (new_value.ToString() == null || new_value.ToString() == string.Empty)
                        return "Stock code not assigned";
                    if (!projection.IsValidStockCode(new_value.ToString()))
                        return "Invalid stock code. Validity can be maintained in Data -> Commodity Codes";
                }
            }

            return string.Empty;
        }

        public override void FullRefresh()
        {
            if (!CanFullRefresh())
                return;

            userCollection = null;
            IsActualCostSummaryVisible = false;
            IsMaterialCostSummaryVisible = false;
            IsRemainingPOCostSummaryVisible = false;

            this.RaisePropertyChanged(x => x.IsActualCostSummaryVisible);
            this.RaisePropertyChanged(x => x.IsMaterialCostSummaryVisible);
            this.RaisePropertyChanged(x => x.IsRemainingPOCostSummaryVisible);
            initializeCompulsoryViewProperties(loadPROJECT);
            if(initializeOptionalViewCollectionsOnRefresh)
                initializeOptionalViewCollections();

            base.FullRefresh();
        }
#endregion

#region EXO Database
        private ExoSubJobProjection commitToExo(ExoSubJobProjection projection)
        {
            List<ExoSubJobProjection> newLines = new List<ExoSubJobProjection>();
            ExoSubJobProjection newLine = projection;
            newLines.Add(newLine);
            IEnumerable<ExoSubJobProjection> newlyAddedProjections = CommitToExo(newLines);
            if (newlyAddedProjections.Count() > 0)
                return newlyAddedProjections.First();

            return null;
        }

        public IEnumerable<ExoSubJobProjection> CommitToExo(IEnumerable<ExoSubJobProjection> projections, bool updateBudgetIfExist = false)
        {
            List<ErrorMessage> errorMessages;
            IEnumerable<ExoSubJobProjection> addedProjections = ExoMethods.CommitToExo(projections, MessageBoxService, masterJob, copyLine, loadPROJECT, USERCollection, localPrimeroUnitOfWork, bluePrintsEntitiesUnitOfWork, BulkColumnEditDialogService, out errorMessages, updateBudgetIfExist, IgnoreCostGroupCostType, ValidateDesignDirectIndirect);

            ShowErrorMessage("Errors", errorMessages);
            if (addedProjections.Count() > 0)
            {
                foreach (ExoSubJobProjection addedProjection in addedProjections)
                {
                    addedProjection.PopulateCommodityCodes(COMMODITY_CODECollection);
                    addedProjection.PopulateStockItems(STOCK_ITEMSCollection);
                    addedProjection.IsLineExistsInExo = true;
                }

                return addedProjections;
            }

            return new List<ExoSubJobProjection>();
        }

#endregion

#region View Properties
        ESTIMATE_ITEMCollectionViewModelWrapper estimateItemViewModel;
        public bool CanCopyToJob()
        {
            return !IsLoading;
        }

        public void CopyToJob()
        {
            if (MessageBoxService.ShowMessage("This will make a copy of existing jobs in exo (except design) into job setup, do you wish to continue?", "Confirmation", MessageButton.OKCancel, MessageIcon.Information) == MessageResult.OK)
            {
                estimateItemViewModel = ESTIMATE_ITEMCollectionViewModelWrapper.Create();
                estimateItemViewModel.OnEntitiesLoadedCallBack = onEstimateLoaded;
                estimateItemViewModel.OnEntitiesLoadedCallBackManualDispose = true;
                estimateItemViewModel.OnParameterChange(new TripleEntitiesParameter<Data.PROJECT, IAmBaseline, object>(loadPROJECT, null, new KeyValuePair<DeliverablesViewType, EstimateViewMode>(DeliverablesViewType.Direct, EstimateViewMode.Budget)));

                LoadingScreenManager.ShowLoadingScreen(Entities.Count);
            }
        }

        public bool CanPopulateStandardCodes()
        {
            return !IsLoading;
        }

        public override bool IsValidEntity(ExoSubJobProjection entity, IEnumerable<ExoSubJobProjection> preCommittedProjections, ref string errorMessage, out List<KeyValuePair<string, string>> constraintIssues)
        {
            return base.IsValidEntity(entity, preCommittedProjections, ref errorMessage, out constraintIssues);
        }

        public void PopulateStandardCodes()
        {
            IBluePrintsEntitiesUnitOfWork bluePrintsUnitOfWork = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
            IQueryable<COMMODITY_CODE> commodityCodes = bluePrintsUnitOfWork.COMMODITY_CODES.Where(x => x.IS_STANDARD);

            string standardJobCode = loadPROJECT.NUMBER + "-000-00-";
            List<ExoSubJobProjection> standardJobs = new List<ExoSubJobProjection>();
            foreach(COMMODITY_CODE commodityCode in commodityCodes)
            {
                if(commodityCode.DISCIPLINE == null)
                    continue;

                ExoSubJobProjection standardJob = new ExoSubJobProjection();
                if (commodityCode.PHASE_TYPE == PhaseType.Construct)
                    standardJob.SubJobCode = standardJobCode + BluePrintsResources.Default_Construction_Phase;
                else if (commodityCode.PHASE_TYPE == PhaseType.Design)
                    standardJob.SubJobCode = standardJobCode + BluePrintsResources.Default_Design_Phase;
                else if (commodityCode.PHASE_TYPE == PhaseType.Indirect)
                    standardJob.SubJobCode = standardJobCode + BluePrintsResources.Default_Indirect_Phase;
                else if (commodityCode.PHASE_TYPE == PhaseType.Procurement)
                    standardJob.SubJobCode = standardJobCode + BluePrintsResources.Default_Procurement_Phase;
                else if (commodityCode.PHASE_TYPE == PhaseType.Tender)
                    standardJob.SubJobCode = standardJobCode + BluePrintsResources.Default_Tender_Phase;

                standardJob.DisciplineCode = commodityCode.DISCIPLINE.CODE + BluePrintsResources.DefaultCostGroupAffix;
                standardJob.CommodityCode = commodityCode.CODE;
                standardJob.StockCode = commodityCode.DEFAULT_STOCKCODE;
                standardJobs.Add(standardJob);
            }

            IEnumerable<ExoSubJobProjection> addedProjections = CommitToExo(standardJobs);
            foreach(ExoSubJobProjection addedProjection in addedProjections)
                Entities.Add(addedProjection);

            OnAfterNewProjectionsAdded(addedProjections);
        }

        private void onEstimateLoaded(IEnumerable<ESTIMATE_ITEMProgress> estimateItems, object parent_id)
        {
            mainThreadDispatcher.BeginInvoke(new Action(() => {
                int saveCount = 0;
                List<ESTIMATE_ITEMProgress> newESTIMATE_ITEMS = new List<ESTIMATE_ITEMProgress>();
                List<ErrorMessage> errorMessages = new List<ErrorMessage>();
                foreach (var entity in Entities)
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
                                    newESTIMATE_ITEM.BUDGET_INSTALL_RATE = 0;
                                    ESTIMATE_ITEMProgress projection = new ESTIMATE_ITEMProgress();
                                    projection.Entity = new ESTIMATE_ITEMProjection();
                                    projection.Entity.Entity = newESTIMATE_ITEM;
                                    newESTIMATE_ITEMS.Add(projection);
                                    errorMessages.Add(new ErrorMessage(entity.SubJobCode + "-" + entity.DisciplineCode + "-" + entity.CommodityCode, "Added"));
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

                if (errorMessages.Count > 0)
                {
                    DialogCollectionViewModel<ErrorMessage> viewModel = DialogCollectionViewModel<ErrorMessage>.Create(errorMessages, "The following jobs are added into job setup");
                    ErrorMessagesDialogService.ShowDialog(MessageButton.OK, string.Empty, "ListErrorMessages", viewModel);
                }
                else
                    MessageBoxService.ShowMessage("No new jobs were detected", "Information", MessageButton.OK, MessageIcon.Information);
            }));
        }

        public bool CanPopulateActuals()
        {
            return !IsLoading;
        }

        public bool IsActualCostSummaryVisible { get; set; }
        public bool IsMaterialCostSummaryVisible { get; set; }
        public bool IsRemainingPOCostSummaryVisible { get; set; }
        public void PopulateActuals()
        {
            List<ExoDataPoint> burnedDataPoints = BluePrintsDataUtils.GetBurned(localPrimeroUnitOfWork, loadPROJECT.NUMBER, DateTime.Now, null, null, 1, true);
            List<ExoDataPoint> materialDataPoints = BluePrintsDataUtils.GetMaterials(localPrimeroUnitOfWork, loadPROJECT.NUMBER, DateTime.Now, null, 1, true);
            List<ExoDataPoint> poDataPoints = BluePrintsDataUtils.GetEXOPO(localPrimeroUnitOfWork, loadPROJECT.NUMBER, DateTime.Now, null, true);

            foreach (ExoSubJobProjection projection in MainViewModel.Entities)
            {
                if(projection.SubJobCode.Contains("-D"))
                    projection.SubJobActualCostSummary = burnedDataPoints.Where(x => x.Subjob_Name == projection.SubJobCode && x.Discipline_Code == projection.DisciplineCode && x.Commodity_Code == projection.CommodityCode && x.Variation_Code == projection.VariationCode).Sum(x => x.Costs);
                else
                    projection.SubJobActualCostSummary = burnedDataPoints.Where(x => x.Subjob_Name == projection.SubJobCode && x.Discipline_Code == projection.DisciplineCode && x.Commodity_Code == projection.CommodityCode && x.StockCode == projection.StockCode && x.Variation_Code == projection.VariationCode).Sum(x => x.Costs);

                projection.SubJobMaterialCostSummary = materialDataPoints.Where(x => x.Subjob_Name == projection.SubJobCode && x.Discipline_Code == projection.DisciplineCode && x.Commodity_Code == projection.CommodityCode && x.StockCode == projection.StockCode && x.Variation_Code == projection.VariationCode).Sum(x => x.Costs);
                projection.SubJobRemainingPOCostSummary = poDataPoints.Where(x => x.Subjob_Name == projection.SubJobCode && x.Discipline_Code == projection.DisciplineCode && x.Commodity_Code == projection.CommodityCode && x.StockCode == projection.StockCode && x.Variation_Code == projection.VariationCode).Sum(x => x.Costs);
            }

            IsActualCostSummaryVisible = true;
            IsMaterialCostSummaryVisible = true;
            IsRemainingPOCostSummaryVisible = true;

            this.RaisePropertyChanged(x => x.IsActualCostSummaryVisible);
            this.RaisePropertyChanged(x => x.IsMaterialCostSummaryVisible);
            this.RaisePropertyChanged(x => x.IsRemainingPOCostSummaryVisible);
            GridControlService.RefreshData();
        }

        public bool CanSetDesignFilter()
        {
            return !IsLoading;
        }

        public void SetDesignFilter()
        {
            setFilter("-D");
        }

        public bool CanSetIndirectFilter()
        {
            return !IsLoading;
        }

        public void SetIndirectFilter()
        {
            setFilter("-I");
        }

        public bool CanSetProcurementFilter()
        {
            return !IsLoading;
        }

        public void SetProcurementFilter()
        {
            setFilter("-P");
        }

        public bool CanSetConstructionFilter()
        {
            return !IsLoading;
        }

        public void SetConstructionFilter()
        {
            setFilter("-D");
        }

        private void setFilter(string filterString)
        {   
            FilterCriteria = CriteriaOperator.Parse("Contains([SubJobCode], '" + filterString + "')");
            this.RaisePropertyChanged(x => x.FilterCriteria);
        }

        public override void ShowNotification()
        {
            if (AppNotificationService == null)
                return;

            INotification notification1 = AppNotificationService.CreatePredefinedNotification("Exo is connected to " + loadPROJECT.OfficeNameForExo, null, null, null);
            notification1.ShowAsync();
            INotification notification2 = AppNotificationService.CreatePredefinedNotification("Permission assignment has been moved to [Exo] -> [Master Job Permission] to reduce bugs here", null, null, null);
            notification2.ShowAsync();
        }

        public override string ViewName
        {
            get
            {
                //return "BASELINE_ITEMSViewModelWrapper" + view_project_specific_affix;
                return "EXO_SubjobCollectionViewModelWrapper_v2";
            }
        }

        List<USER> userCollection;
        public IEnumerable<USER> USERCollection
        {
            get
            {
                if(userCollection == null)
                {
                    userCollection = new List<USER>();
                    var collection = GetEntities<USER>();
                    foreach (USER user in collection)
                    {
                        user.ProjectLocale = loadPROJECT.OfficeNameForExo;
                        STAFF staff = exoSTAFFS.FirstOrDefault(x => x.STAFFNO == user.ProjectLocaleExoId);
                        //don't return any user that is disabled in EXO
                        if (staff != null)
                        {
                            //if(staff.SECURITYPROFILEID != 4)
                            userCollection.Add(user);
                        }
                    }
                }
                
                return userCollection.OrderBy(x => x.NAME);
            }
        }

        public IEnumerable<USER> AllUSERCollection
        {
            get
            {
                var collection = GetEntities<USER>();
                return collection.OrderBy(x => x.NAME);
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
                if(Entities != null && Entities.Count > 0)
                {
                    allCommodityCodes.AddRange(Entities.Select(x => x.CommodityCode));
                }

                return allCommodityCodes;
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
                if (Entities != null && Entities.Count > 0)
                {
                    allStockCodeRanges.AddRange(Entities.Select(x => x.StockCode));
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

        public IEnumerable<FORECAST_JOB> FORECAST_JOBCollection
        {
            get
            {
                return GetEntities<FORECAST_JOB>();
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
                if (Entities == null || Entities.Count() == 0)
                    return new List<string>();

                return Entities.Select(x => x.SubJobCode).OrderBy(x => x).Distinct();
            }
        }

        public IEnumerable<string> VariationCodeStringCollection
        {
            get
            {
                if (Entities == null || Entities.Count() == 0)
                    return new List<string>();

                return Entities.Select(x => x.VariationCode).OrderBy(x => x).Distinct();
            }
        }
#endregion
    }
}