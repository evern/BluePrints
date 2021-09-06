using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Filtering;
using BluePrints.Common.Projections;
using BluePrints.Common.Reports;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using BluePrints.Reports;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Printing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single PROGRESS object view model.
    /// </summary>
    public partial class OffsiteDirectProgressCollectionViewModelWrapper : BluePrintsEntitiesProgressCollectionWrapper<BASELINE_ITEM, BASELINE_ITEMProgress, Guid, IBluePrintsEntitiesUnitOfWork>, ISupportFiltering<BASELINE_ITEMProgress>
    {

        /// <summary>
        /// Creates a new instance of PROGRESS_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static OffsiteDirectProgressCollectionViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new OffsiteDirectProgressCollectionViewModelWrapper());
        }

        #region Database Operation
        protected override void resolveParameters(object parameter)
        {
            AlwaysSkipMessage = true;
            is_load_p6_task = true;
            isUseReportDate = LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Permission_DesignDeliverables_ProgressPreviousWeeksDate)) != LoginCredentials.PermissionStatus.None;
            canDateBackwardForward = LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Permission_DesignDeliverables_CanDateBackwardForward)) != LoginCredentials.PermissionStatus.None;
            base.resolveParameters(parameter);
        }

        private BASELINE loadBASELINE;
        public FilterTreeViewModel<BASELINE_ITEMProgress, Guid> FilterTreeViewModel { get; set; }
        protected override void addEntitiesLoader()
        {
            //in user offsite direct view model wrapper baseline should not be loaded because query gets from navigational baseline
            if(is_single_project_mode)
            {
                loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
                loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINES, BASELINEProjectionFunc, x => assign_baseline(x));
            }

            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DELIVERABLES_STATUSES, DELIVERABLES_STATUSProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DSTATUS_DOCTYPES, DSTATUS_DOCTYPEProjectionFunc);
            loaderCollection.AddLoaderDescription<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DOCTYPES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PHASES, PHASEProjectionFunc);

            base.addEntitiesLoader();
        }

        private Func<IRepositoryQuery<Data.PHASE>, IQueryable<Data.PHASE>> PHASEProjectionFunc()
        {
            return query => query.Where(x => x.PHASE_TYPE != PhaseType.Construct && x.PHASE_TYPE != PhaseType.Procurement);
        }

        private void assign_baseline(BASELINE baseline)
        {
            if (baseline == null && !SupressCompulsoryEntityNotFoundMessage)
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage("Live baseline not found")));

            loadBASELINE = baseline;
        }

        protected virtual Func<IRepositoryQuery<Data.PROJECT>, IQueryable<Data.PROJECT>> PROJECTProjectionFunc()
        {
            if (isQueryForLiveStatus)
                return query => query.Where(x => x.GUID == loadPROJECT.GUID);
            else
                return query => query.Where(x => x.GUID == loadPROGRESS.GUID_PROJECT).OrderBy(x => x.NUMBER);
        }

        protected virtual Func<IRepositoryQuery<BASELINE>, IQueryable<BASELINE>> BASELINEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == BaselineStatus.Live);
        }

        protected virtual Func<IRepositoryQuery<DELIVERABLES_STATUS>, IQueryable<DELIVERABLES_STATUS>> DELIVERABLES_STATUSProjectionFunc()
        {
            if (is_single_project_mode)
                return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
            else
                return query => query.Where(x => x.PROJECT.STATUS == ProjectStatus.Active);
        }

        protected virtual Func<IRepositoryQuery<DSTATUS_DOCTYPE>, IQueryable<DSTATUS_DOCTYPE>> DSTATUS_DOCTYPEProjectionFunc()
        {
            if (is_single_project_mode)
                return query => query.Where(x => x.DELIVERABLES_STATUS.GUID_PROJECT == loadPROJECT.GUID);
            else
                return query => query.Where(x => x.DELIVERABLES_STATUS.PROJECT.STATUS == ProjectStatus.Active);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS);
        }

        public bool CanUpdateAllPercentagesByStatus()
        {
            if (IsLoading)
                return false;

            return LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Permission_DesignDeliverables_UpdateProgressByStatus)) != LoginCredentials.PermissionStatus.None;
        }

        public void UpdateAllPercentagesByStatus()
        {
            if (IsLoading)
                return;

            if (MessageBoxService.ShowMessage("Warning\nThis action will update or delete progresses based on deliverable status and is not reversible\nDo you wish to continue?", BluePrintsResources.Warning_Caption, MessageButton.YesNo) == MessageResult.No)
                return;

            MainViewModel.AlwaysSkipMessage = true;
            BluePrintsDataUtils.UpdatePercentagesByStatus(MessageBoxService, PROGRESS_ITEMSCollectionViewModel, MainViewModel.Entities);
            MainViewModel.AlwaysSkipMessage = false;
            FullRefresh();
        }

        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            //avoid crash when this is disposed
            if(PROGRESS_ITEMSCollectionViewModel != null)
            {
                //progress items needs to get notified for view to reflect update
                PROGRESS_ITEMSCollectionViewModel.AlwaysSkipMessage = false;

                //only respond to message from same key
                PROGRESS_ITEMSCollectionViewModel.RefreshOnlyOnSameSenderKey = true;

                if (PROJECT_REPORTCollectionViewModel != null)
                    PROJECT_REPORTCollectionViewModel.AlwaysSkipMessage = false;
            }

            base.OnAfterAssignedCallbackAndRaisePropertyChanged();
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEMProgress>>
            specifyMainViewModelProjection()
        {
            return query => ProgressQueries.OffsiteDirectProgressItemTransformation(query.Where(x => x.GUID_BASELINE == loadBASELINE.GUID), loadPROJECT, loadPROGRESS, RATECollection, PROGRESS_ITEMCollection, VARIATIONCollection, false, P6_ASSIGNMENTCollection, DeliverableInternalNumberMode.Default, isUseReportDate, P6TASKCollection, null, null, null, null, DELIVERABLES_STATUSCollection, DSTATUS_DOCTYPECollection, null, null, null, PROGRESS_ETCCollection);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<BASELINE_ITEMProgress> entities)
        {
            FilterTreeViewModel = FiltersSettings.GetBASELINE_ITEMProgressFilterTree(this, entities);
            MainViewModel.ValidateFillDownCallBack = ValidateFillDownCallBack;
            MainViewModel.FuncManualCellPastingIsContinue = BluePrintsDataUtils.FuncManualCellPastingIsContinue;
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }
        #region Collection Call Backs

        public override void UnifiedCellValueChanged(string field_name, object old_value, object new_value, BASELINE_ITEMProgress projection, bool isNew)
        {
            if (field_name == BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().DeliverableStatusProgressGuid))
                projection.ShouldSave = true;
            if (field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity.Entity.DISCIPLINE_NUM)))
                projection.ShouldSave = true;
            if (field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity.Entity.FORECAST_START_DATE)))
                projection.ShouldSave = true;
            if (field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity.Entity.TARGET_DATE)))
                projection.ShouldSave = true;

            base.UnifiedCellValueChanged(field_name, old_value, new_value, projection, isNew);
        }

        protected override OperationInterceptMode OnBeforeProjectionSaveIsContinue(BASELINE_ITEMProgress projection, out bool isNew)
        {
            isNew = false;
            if (projection.ShouldSave)
            {
                projection.ShouldSave = false;
                return OperationInterceptMode.Continue;
            }
            else
                return OperationInterceptMode.SkipOneAndAllDbSaves;
        }

        public bool ValidateFillDownCallBack(BASELINE_ITEMProgress fillDownEntity, string fieldName, object fillValue)
        {
            if (fieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Total_Earned_Percentage))
            {
                var newPercentage = (decimal)fillValue;
                if (newPercentage > fillDownEntity.MaxPercentage)
                    return false;
                else if (newPercentage < fillDownEntity.MinPercentage)
                    return false;
            }

            return true;
        }
        #endregion

        #endregion

        #region View Properties
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            //get { return "OffsiteDirectProgressViewModelWrapper" + view_project_specific_affix; }
            get { return "OffsiteDirectProgressViewModelWrapper_v7"; }
        }

        public bool IsDataDateChangeVisible => canDateBackwardForward;

        private string view_project_specific_affix
        {
            get
            {
                if (loadPROJECT == null)
                    return string.Empty;
                return loadPROJECT.GUID.ToString();
            }
        }

        public IEnumerable<DELIVERABLES_STATUS> DELIVERABLES_STATUSCollection
        {
            get
            {
                var collection = GetEntities<DELIVERABLES_STATUS>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.MAX_PERCENTAGE);
                return collection;
            }
        }

        public IEnumerable<DSTATUS_DOCTYPE> DSTATUS_DOCTYPECollection
        {
            get
            {
                return GetEntities<DSTATUS_DOCTYPE>();
            }
        }

        public IEnumerable<DOCTYPE> DOCTYPECollection
        {
            get
            {
                var collection = GetEntities<DOCTYPE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.CODE);
                return collection;
            }
        }

        public IEnumerable<P6Data.TASK> P6TASKCollection
        {
            get
            {
                return GetEntities<P6Data.TASK>();
            }
        }

        public IEnumerable<P6_ASSIGNMENT> P6_ASSIGNMENTCollection
        {
            get
            {
                return GetEntities<P6_ASSIGNMENT>();
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
        protected override CostGroup cost_group => CostGroup.Offsite;

        protected override IEnumerable<IReportable> ReportableCollection => MainViewModel == null || MainViewModel.Entities == null ? new ObservableCollection<BASELINE_ITEMProgress>() : MainViewModel.Entities;

        private BASELINE_ITEMSchedulingViewModelWrapper baseline_item_scheduling_view_model;
        protected override IEntitiesSchedulingCollectionWrapper scheduling_view_model
        {
            get
            {
                if (baseline_item_scheduling_view_model == null)
                    baseline_item_scheduling_view_model = BASELINE_ITEMSchedulingViewModelWrapper.Create();

                return baseline_item_scheduling_view_model;
            }
        }

        protected override void dispose_scheduling_view_model()
        {
            baseline_item_scheduling_view_model = null;
        }

        protected override PhaseType progress_type => PhaseType.Design;

        protected override bool manuallySaveProgressOnAfterBaselineItemSaved => false;

        protected override bool isSingleProjectAndUserLocale => true;
        #endregion

        #region Reporting
        public bool CanEditReport()
        {
            return !IsLoading;
        }

        public void EditReport()
        {
            var reportDesigner = new UserReportDesigner(loadPROJECT, PROJECT_REPORTCollectionViewModel, ReportType.Progress_Report);
            if (reportDesigner.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                reportDesigner.Dispose();
            else
                reportDesigner.Dispose();
        }

        public bool CanFixVariation()
        {
            return !IsLoading;
        }

        public void FixVariation()
        {
            IBluePrintsEntitiesUnitOfWork unitOfWork = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
            IQueryable<VARIATION> approvedVariations = unitOfWork.VARIATIONS.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.APPROVED != null && x.ADJUSTMENT_TO_BUDGET == false);

            List<BASELINE_ITEMProgress> saveEntities = new List<BASELINE_ITEMProgress>();
            foreach (var entity in MainViewModel.Entities)
            {
                IQueryable<VARIATION_ITEM> variationItems = approvedVariations.SelectMany(x => x.VARIATION_ITEM);
                IQueryable<VARIATION_ITEM> currentEntityVariationItems = variationItems.Where(x => x.GUID_ORIBASEITEM == entity.OriginalEntityKey);
                if(currentEntityVariationItems.Count() > 0)
                {
                    decimal calculatedVariationUnits = currentEntityVariationItems.Sum(x => x.VARIATION_UNITS);
                    if (entity.Entity.Entity.DC_HOURS != calculatedVariationUnits)
                    {
                        entity.Entity.Entity.DC_HOURS = calculatedVariationUnits;
                        saveEntities.Add(entity);
                    }
                }
            }

            MainViewModel.BaseBulkSave(saveEntities);
        }

        public override void ShowNotification()
        {
            if (AppNotificationService == null)
                return;

            INotification notification1 = AppNotificationService.CreatePredefinedNotification("Please check whether data date is correct before updating", null, null, null);
            notification1.ShowAsync();
        }

        public bool CanUpdateTargetDates()
        {
            return !IsLoading;
        }

        public void UpdateTargetDates()
        {
            if (MessageBoxService.ShowMessage("This will copy start dates and due dates to target dates, do you wish to continue?", "Confirmation", MessageButton.OKCancel) == MessageResult.Cancel)
                return;

            List<BASELINE_ITEMProgress> saveProgresses = new List<BASELINE_ITEMProgress>();
            IBluePrintsEntitiesUnitOfWork bluePrintsUnitOfWork = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
            foreach (BASELINE_ITEMProgress progress in MainViewModel.Entities)
            {
                BASELINE_ITEM repositoryBASELINE_ITEM = bluePrintsUnitOfWork.BASELINE_ITEMS.FirstOrDefault(x => x.GUID == progress.Entity.Entity.GUID);
                if(repositoryBASELINE_ITEM != null)
                {
                    repositoryBASELINE_ITEM.FORECAST_START_DATE = progress.StartDate;
                    repositoryBASELINE_ITEM.TARGET_DATE = progress.DueDate;
                }
            }

            bluePrintsUnitOfWork.SaveChanges();
            FullRefresh();
        }

        public bool CanViewReport()
        {
            return !IsLoading;
        }

        public void ViewReport()
        {
            if (MessageBoxService.ShowMessage("Please make sure that you've already recalculated planned and remaining data for w/e " + DataDate.ToShortDateString() + "\n\nYou can do this by double clicking on the project title in the navigation bar on the left and press refresh", "Info", MessageButton.OKCancel) == MessageResult.Cancel)
                return;

            LoadingScreenManager.ShowLoadingScreen(1);
            var progressReport = new XtraReportPROGRESS_ITEMS();
            var dbProjectReport = loaderCollection.GetObject<PROJECT_REPORT>();
            if (dbProjectReport != null)
            {
                var reportString = dbProjectReport.REPORT.ToString();
                using (var sw = new StreamWriter(new MemoryStream()))
                {
                    sw.Write(reportString);
                    sw.Flush();
                    progressReport.LoadLayout(sw.BaseStream);
                }
            }

            //LoadingScreenManager.ShowLoadingScreen(1);
            //await BluePrintsContextHelper.RefreshDeliverablesDataPointsByProject(loadPROJECT.NUMBER);
            //LoadingScreenManager.Progress();

            TimeSpan reportInterval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(loadPROGRESS);
            DateTime firstAlignedDataDate = ChronologicalHelpers.GenerateFirstAlignedDataDate(loadPROGRESS);

            DateTime reporting_data_date = DataDate;
            TimeSpan reporting_interval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(loadPROGRESS);
            DateTime first_aligned_data_date = ChronologicalHelpers.GenerateFirstAlignedDataDate(loadPROGRESS);
            DeliverableSummaryStats projectSummary = new DeliverableSummaryStats(MainViewModel.Entities, reporting_data_date, reporting_interval, first_aligned_data_date);
            FullStatsBuilder fullStatsBuilder = new FullStatsBuilder(loadPROJECT.NUMBER, loadPROJECT.CURRENCYCONVERSION, reporting_interval, first_aligned_data_date, SUBJOBCollection, reporting_data_date, primeroUnitOfWork);
            fullSummarizer = new FullSummarizer(projectSummary, fullStatsBuilder, loadPROJECT.NUMBER, false);
            fullSummarizer.BuildBurnedDataPoints(DashboardEXOQueryType.TimeOnly, false, true);
            List<StatsCalculationType> statsCalcType = new List<StatsCalculationType>();
            statsCalcType.Add(StatsCalculationType.Planned);
            statsCalcType.Add(StatsCalculationType.Earned);
            statsCalcType.Add(StatsCalculationType.Remaining);
            fullSummarizer.Build(true, 1, statsCalcType);

            progressReport.AssignProperties(projectSummary, DataDate, loadPROGRESS.PROJECT.NAME);
            var previewWindow = new DocumentPreviewWindow();
            previewWindow.PreviewControl.DocumentSource = progressReport;
            previewWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            previewWindow.WindowState = WindowState.Maximized;
            progressReport.RequestParameters = false;
            progressReport.CreateDocument(true);
            LoadingScreenManager.CloseLoadingScreen();
            previewWindow.Show();
        }
        #endregion
    }
}