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
    public partial class SiteDirectProgressCollectionViewModelWrapper : BluePrintsEntitiesProgressCollectionWrapper<ESTIMATE_ITEM, ESTIMATE_ITEMProgress, Guid, IBluePrintsEntitiesUnitOfWork>, ISupportFiltering<ESTIMATE_ITEMProgress>
    {

        /// <summary>
        /// Creates a new instance of PROGRESS_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static SiteDirectProgressCollectionViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new SiteDirectProgressCollectionViewModelWrapper());
        }

        #region Database Operation
        protected override void resolveParameters(object parameter)
        {
            skipExoDataLoading = true;
            is_load_p6_task = true;
            isUseReportDate = LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Permission_DesignDeliverables_ProgressPreviousWeeksDate)) != LoginCredentials.PermissionStatus.None;
            canDateBackwardForward = LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Permission_DesignDeliverables_CanDateBackwardForward)) != LoginCredentials.PermissionStatus.None;
            base.resolveParameters(parameter);
        }

        private ESTIMATE loadESTIMATE;
        public FilterTreeViewModel<ESTIMATE_ITEMProgress, Guid> FilterTreeViewModel { get; set; }
        protected override void addEntitiesLoader()
        {
            //in user offsite direct view model wrapper baseline should not be loaded because query gets from navigational baseline
            if(is_single_project_mode)
            {
                loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
                loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.ESTIMATES, ESTIMATEProjectionFunc, x => assign_estimate(x));
            }

            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DELIVERABLES_STATUSES, DELIVERABLES_STATUSProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DSTATUS_DOCTYPES, DSTATUS_DOCTYPEProjectionFunc);
            loaderCollection.AddLoaderDescription<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DOCTYPES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, COMMODITY_CODEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PHASES, PHASEProjectionFunc);

            base.addEntitiesLoader();
        }

        private void assign_estimate(ESTIMATE estimate)
        {
            if (estimate == null && !SupressCompulsoryEntityNotFoundMessage)
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage("Live estimate not found")));

            loadESTIMATE = estimate;
        }

        protected virtual Func<IRepositoryQuery<Data.PROJECT>, IQueryable<Data.PROJECT>> PROJECTProjectionFunc()
        {
            if (isQueryForLiveStatus)
                return query => query.Where(x => x.GUID == loadPROJECT.GUID);
            else
                return query => query.Where(x => x.GUID == loadPROGRESS.GUID_PROJECT).OrderBy(x => x.NUMBER);
        }

        private Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>> COMMODITY_CODEProjectionFunc()
        {
            return query => query.Where(x => (x.GUID_PROJECT == loadPROJECT.GUID || x.GUID_PROJECT == null));
        }

        protected virtual Func<IRepositoryQuery<ESTIMATE>, IQueryable<ESTIMATE>> ESTIMATEProjectionFunc()
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

        private Func<IRepositoryQuery<Data.PHASE>, IQueryable<Data.PHASE>> PHASEProjectionFunc()
        {
            return query => query.Where(x => x.PHASE_TYPE != PhaseType.Design);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.ESTIMATE_ITEMS);
        }

        public bool CanUpdateAllPercentagesByStatus()
        {
            if (IsLoading)
                return false;

            return LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Permission_DesignDeliverables_UpdateProgressByStatus)) != LoginCredentials.PermissionStatus.None;
        }

        protected override Func<IRepositoryQuery<ESTIMATE_ITEM>, IQueryable<ESTIMATE_ITEMProgress>>
            specifyMainViewModelProjection()
        {
            return query => ESTIMATE_ITEMProjectionQueries.IDeliverable_Progress_Transformation(query.Where(x => x.ESTIMATE.GUID_PROJECT == loadPROJECT.GUID && x.ESTIMATE.STATUS == BaselineStatus.Live), loadPROJECT, loaderCollection.GetCollection<RATE>(), loadPROGRESS, PROGRESS_ITEMCollection, false, null, false, null, false, COMMODITY_CODECollection);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<ESTIMATE_ITEMProgress> entities)
        {
            MainViewModel.ValidateFillDownCallBack = ValidateFillDownCallBack;
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }
        #region Collection Call Backs

        public bool ValidateFillDownCallBack(ESTIMATE_ITEMProgress fillDownEntity, string fieldName, object fillValue)
        {
            if (fieldName == BindableBase.GetPropertyName(() => new ESTIMATE_ITEMProgress().Total_Earned_Percentage))
            {
                var newPercentage = (decimal)fillValue;
                if (newPercentage > fillDownEntity.MaxPercentage)
                    return false;
                else if (newPercentage < fillDownEntity.MinPercentage)
                    return false;
            }

            return true;
        }

        public override void FullRefresh()
        {
            if (!CanFullRefresh())
                return;

            ReloadEntitiesCollection();
        }
        #endregion

        #endregion

        #region View Properties
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            //get { return "SiteDirectProgressViewModelWrapper" + view_project_specific_affix; }
            get { return "SiteDirectProgressViewModelWrapper_v5"; }
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

        protected override CostGroup cost_group => CostGroup.Offsite;

        protected override IEnumerable<IReportable> ReportableCollection => MainViewModel == null || MainViewModel.Entities == null ? new ObservableCollection<ESTIMATE_ITEMProgress>() : MainViewModel.Entities;

        private BUDGET_ITEMSchedulingViewModelWrapper baseline_item_scheduling_view_model;
        protected override IEntitiesSchedulingCollectionWrapper scheduling_view_model
        {
            get
            {
                if (baseline_item_scheduling_view_model == null)
                    baseline_item_scheduling_view_model = BUDGET_ITEMSchedulingViewModelWrapper.Create();

                return baseline_item_scheduling_view_model;
            }
        }

        protected override void dispose_scheduling_view_model()
        {
            baseline_item_scheduling_view_model = null;
        }

        protected override PhaseType progress_type => PhaseType.Design;

        protected override bool manuallySaveProgressOnAfterBaselineItemSaved => false;
        #endregion

        #region Reporting
        public bool CanEditReport()
        {
            return !IsLoading;
        }

        public void EditReport()
        {
            var reportDesigner = new UserReportDesigner(loadPROJECT,
                (CollectionViewModel<PROJECT_REPORT, PROJECT_REPORT, Guid, IBluePrintsEntitiesUnitOfWork>)
                loaderCollection.GetViewModel<PROJECT_REPORT>(), ReportType.Progress_Report);
            if (reportDesigner.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                reportDesigner.Dispose();
            else
                reportDesigner.Dispose();
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

        public bool CanViewReport()
        {
            return !IsLoading;
        }

        public void ViewReport()
        {
            //if (MessageBoxService.ShowMessage("Please make sure that you've already recalculated planned and remaining data for w/e " + DataDate.ToShortDateString() + "\n\nYou can do this by double clicking on the project title in the navigation bar on the left and press refresh", "Info", MessageButton.OKCancel) == MessageResult.Cancel)
            //    return;

            //LoadingScreenManager.ShowLoadingScreen(1);
            //var progressReport = new XtraReportPROGRESS_ITEMS();
            //var dbProjectReport = loaderCollection.GetObject<PROJECT_REPORT>();
            //if (dbProjectReport != null)
            //{
            //    var reportString = dbProjectReport.REPORT.ToString();
            //    using (var sw = new StreamWriter(new MemoryStream()))
            //    {
            //        sw.Write(reportString);
            //        sw.Flush();
            //        progressReport.LoadLayout(sw.BaseStream);
            //    }
            //}

            ////LoadingScreenManager.ShowLoadingScreen(1);
            ////await BluePrintsContextHelper.RefreshDeliverablesDataPointsByProject(loadPROJECT.NUMBER);
            ////LoadingScreenManager.Progress();

            //TimeSpan reportInterval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(loadPROGRESS);
            //DateTime firstAlignedDataDate = ChronologicalHelpers.GenerateFirstAlignedDataDate(loadPROGRESS);
            //List<VariationAdjustment> projectVariationAdjustment = ProjectionHelpers.BuildProjectVariationAdjustments(VARIATIONCollection.AsQueryable(), ReportableCollection);

            //DateTime reporting_data_date = DataDate;
            //TimeSpan reporting_interval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(loadPROGRESS);
            //DateTime first_aligned_data_date = ChronologicalHelpers.GenerateFirstAlignedDataDate(loadPROGRESS);
            //DeliverableSummaryStats projectSummary = new DeliverableSummaryStats(MainViewModel.Entities, reporting_data_date, reporting_interval, first_aligned_data_date, projectVariationAdjustment);
            //FullStatsBuilder fullStatsBuilder = new FullStatsBuilder(loadPROJECT.NUMBER, loadPROJECT.CURRENCYCONVERSION, reporting_interval, first_aligned_data_date, SUBJOBCollection, reporting_data_date, primeroUnitOfWork);
            //fullSummarizer = new FullSummarizer(projectSummary, fullStatsBuilder, loadPROJECT.NUMBER);
            //fullSummarizer.BuildBurnedDataPoints(false, false, false, false, true);
            //fullSummarizer.Build();

            //progressReport.AssignProperties(projectSummary, DataDate, loadPROGRESS.PROJECT.NAME);
            //var previewWindow = new DocumentPreviewWindow();
            //previewWindow.PreviewControl.DocumentSource = progressReport;
            //previewWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            //previewWindow.WindowState = WindowState.Maximized;
            //progressReport.RequestParameters = false;
            //progressReport.CreateDocument(true);
            //LoadingScreenManager.CloseLoadingScreen();
            //previewWindow.Show();
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
        #endregion
    }
}