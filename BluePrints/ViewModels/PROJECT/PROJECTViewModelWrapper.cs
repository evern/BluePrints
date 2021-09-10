using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Document;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Misc;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using BluePrints.Reports;
using System.IO;
using BluePrints.Common.Reports;
using BaseModel.ViewModel.Dialogs;
using BluePrints.Common.Resources;
using BaseModel.ViewModel.Services;
using DevExpress.Mvvm.DataAnnotations;
using BluePrints.P6EntitiesDataModel;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the PROJECTS collection view model.
    /// </summary>
    public class PROJECTViewModelWrapper : DashboardViewModelWrapper<PROJECT, PROJECT_Dashboard, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of PROJECT_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static PROJECTViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new PROJECTViewModelWrapper());
        }

        /// <summary>
        /// Initializes a new instance of the PROJECTViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTViewModel type without the POCO proxy factory.
        /// </summary>
        protected PROJECTViewModelWrapper()
        {
            UseProductivityFactorOnRemaining = false;

            bool? isCurrentBenchmarkedAgainstBudget = LoginCredentials.GetUserPreferenceBool(DataUtils.GetNameOf(() => UserPreferences.SCurve_CurrentBenchmarkedAgainstBudget));
            Current_Benchmark = getBenchmarkTargetFromNullableBool(isCurrentBenchmarkedAgainstBudget);
            bool? isEarnedBenchmarkedAgainstBudget = LoginCredentials.GetUserPreferenceBool(DataUtils.GetNameOf(() => UserPreferences.SCurve_EarnedBenchmarkedAgainstBudget));
            Earned_Benchmark = getBenchmarkTargetFromNullableBool(isEarnedBenchmarkedAgainstBudget);
            bool? isBurnedBenchmarkedAgainstBudget = LoginCredentials.GetUserPreferenceBool(DataUtils.GetNameOf(() => UserPreferences.SCurve_BurnedBenchmarkedAgainstBudget));
            Burned_Benchmark = getBenchmarkTargetFromNullableBool(isBurnedBenchmarkedAgainstBudget);
            bool? isRemainingBenchmarkedAgainstBudget = LoginCredentials.GetUserPreferenceBool(DataUtils.GetNameOf(() => UserPreferences.SCurve_RemainingBenchmarkedAgainstBudget));
            Remaining_Benchmark = getBenchmarkTargetFromNullableBool(isRemainingBenchmarkedAgainstBudget);
        }

        private BenchmarkTarget getBenchmarkTargetFromNullableBool(bool? benchmarkBool)
        {
            if (benchmarkBool == null)
                return BenchmarkTarget.Current;
            else if ((bool)benchmarkBool)
                return BenchmarkTarget.Budget;
            else
                return BenchmarkTarget.Current;
        }

        protected IDialogService ActivityDetailDialogService
        {
            get { return this.GetRequiredService<IDialogService>("ProjectIssueDialog"); }
        }

        [ServiceProperty(Key = "DefaultChartControlService")]
        public virtual IChartControlService ChartControlService { get { return null; } }

        #region Database Operation
        public PROJECT LoadPROJECT { get; set; }
        public Action<BASELINECollectionViewModelWrapper> AssignBASELINEDelegates;
        public Action<PROGRESSCollectionViewModelWrapper> AssignPROGRESSDelegates;
        public Action<ESTIMATECollectionViewModelWrapper> AssignESTIMATEDelegates;
        public Action<AREACollectionViewModelWrapper> AssignAREADelegates;
        public Action<RATECollectionViewModelWrapper> AssignRATEDelegates;
        private DispatcherTimer selectAllDispatcher;
        protected IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected IUnitOfWorkFactory<IP6EntitiesUnitOfWork> p6UnitOfWorkFactory = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected DispatcherTimer delayedRefreshDispatcher;
        private Action<object> navigateCore;
        public bool ForceRetrieveAllJobs { get; set; } //force exo burned to retrieve subjobs that aren't defined
        public bool ForceRetrieveAllUnits { get; set; } //force exo burned to retrieve units that are beyond data date
        public bool ForceRetrieveAllPOs { get; set; } //force exo burned to retrieve units that are beyond data date
        public DashboardEXOQueryType DashboardEXOQueryType { get; set; } //define whether dashboard should query time, material or POs
        public bool UseProductivityFactorOnRemaining { get; set; } //calculate remaining costs using productivity factor
        public bool IsVariationSeparated { get; set; } //whether to split variation out from main job
        IBluePrintsEntitiesUnitOfWork bluePrintsUnitOfWork;
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (DualEntitiesParameter<PROJECT, Action<object>>)parameter;
            LoadPROJECT = PROJECTParameter.GetFirstEntity();
            navigateCore = PROJECTParameter.GetSecondEntity();
            isSuppressPropertyChange = true;

            selectAllDispatcher = new DispatcherTimer();
            selectAllDispatcher.Interval = new TimeSpan(0, 0, 0, 1);
            selectAllDispatcher.Tick += SelectAllDispatcher_Tick;
            HealthCheckIconName = "Apply";
            FixedStartDate = null;
            FixedDataDate = null;

            DashboardEXOQueryType = DashboardEXOQueryType.TimeAndMaterial;
            delayedRefreshDispatcher = new DispatcherTimer();
            delayedRefreshDispatcher.Interval = new TimeSpan(0, 0, 0, 1);
            delayedRefreshDispatcher.Tick += delayedRefreshDispatcher_Tick;
            adjustDataDate();
            viewType = DashboardViewType.Units;
            usePercentage = true;
        }

        private void adjustDataDate()
        {
            if(bluePrintsUnitOfWork == null)
                bluePrintsUnitOfWork = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();

            PROGRESS liveDesignProgress = bluePrintsUnitOfWork.PROGRESSES.FirstOrDefault(x => x.TYPE == PhaseType.Design && x.STATUS == ProgressStatus.Live && x.GUID_PROJECT == LoadPROJECT.GUID);
            if(liveDesignProgress != null)
            {
                if (BluePrintsUtils.ProgressDateChange(DateNavigationType.Current, liveDesignProgress, true))
                    bluePrintsUnitOfWork.SaveChanges();
            }

            PROGRESS liveConstructProgress = bluePrintsUnitOfWork.PROGRESSES.FirstOrDefault(x => x.TYPE == PhaseType.Construct && x.STATUS == ProgressStatus.Live && x.GUID_PROJECT == LoadPROJECT.GUID);
            if (liveConstructProgress != null)
            {
                if (BluePrintsUtils.ProgressDateChange(DateNavigationType.Current, liveConstructProgress, true))
                    bluePrintsUnitOfWork.SaveChanges();
            }
        }

        public virtual DateTime? FixedStartDate { get; set; }
        public string FixedStartDateStr => FixedStartDate == null ? string.Empty : ((DateTime)FixedStartDate).ToString("dd-MMM-yy");

        public virtual DateTime? FixedDataDate { get; set; }
        public string FixedDataDateStr => FixedDataDate == null ? string.Empty : ((DateTime)FixedDataDate).ToString("dd-MMM-yy");

        public override void OnLoaded()
        {
            if (!isFirstLoaded)
            {
                if (AppNotificationService == null || GlobalVariables.IsProjectViewNotificationShown)
                {
                    base.OnLoaded();
                    return;
                }
            }

            //INotification notification = AppNotificationService.CreatePredefinedNotification("If view is not responding please email and report to su.bing-wen@primero.com.au, sorry for any inconvenience!", null, null, null);
            //GlobalVariables.IsProjectViewNotificationShown = true;
            //notification.ShowAsync();

            base.OnLoaded();
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.SUBJOBS, SUBJOBProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECT_REPORTS, PROJECT_REPORTProjectionFunc, null, true);
            loaderCollection.AddLoaderDescription<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DOCTYPES);
        }

        private Func<IRepositoryQuery<BASELINE>, IQueryable<BASELINE>> BASELINEProjectionFunc()
        {
            return query => query.Where(x => x.STATUS == BaselineStatus.Live && x.GUID_PROJECT == LoadPROJECT.GUID).OrderBy(x => x.REVISION);
        }

        private Func<IRepositoryQuery<ESTIMATE>, IQueryable<ESTIMATE>> ESTIMATEProjectionFunc()
        {
            return query => query.Where(x => x.STATUS == BaselineStatus.Live && x.GUID_PROJECT == LoadPROJECT.GUID).OrderBy(x => x.REVISION);
        }

        private Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> PROGRESSProjectionFunc()
        {
            return query => query.Where(x => x.STATUS == ProgressStatus.Live && x.GUID_PROJECT == LoadPROJECT.GUID).OrderBy(x => x.STATUS);
        }

        private Func<IRepositoryQuery<BASELINE_ITEM_WORK>, IQueryable<BASELINE_ITEM_WORK>> BASELINE_ITEM_WORKProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == LoadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<PROJECT_REPORT>, IQueryable<PROJECT_REPORT>> PROJECT_REPORTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == LoadPROJECT.GUID && x.REPORT_TYPE == ReportType.Project_Report.ToString());
        }

        private Func<IRepositoryQuery<PROGRESS_ITEM>, IQueryable<PROGRESS_ITEM>> PROGRESS_ITEMProjectionFunc()
        {
            return query => query.Where(x => x.PROGRESS.STATUS == ProgressStatus.Live && x.PROGRESS.PROJECT.GUID == LoadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<VARIATION>, IQueryable<VARIATION>> VARIATIONProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == LoadPROJECT.GUID).OrderBy(x => x.NAME);
        }

        private Func<IRepositoryQuery<SUBJOB>, IQueryable<SUBJOB>> SUBJOBProjectionFunc()
        {
            //need to preload phase so that parallel for each won't throw exception
            return query => query.Where(x => x.GUID_PROJECT == LoadPROJECT.GUID).Include(x => x.PHASE);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == LoadPROJECT.GUID).OrderBy(x => x.INTERNAL_NUM);
        }

        private Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == LoadPROJECT.GUID).OrderBy(x => x.RATE1);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.PROJECTS);
        }

        public bool ForceRetrieveRemainingDataPoints;
        public bool ShowLoadingScreen = true;
        protected override Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT_Dashboard>> specifyMainViewModelProjection()
        {
            var PROGRESSES = loaderCollection.GetCollection<PROGRESS>();

            List<PROJECT_Dashboard> project_dashboards = new List<PROJECT_Dashboard>();
            PROJECT_Dashboard project_dashboard = DashboardQueries.Single_Project_DashboardTransformation(LoadPROJECT, PROGRESSES, FixedStartDate, FixedDataDate, ForceRetrieveRemainingDataPoints, IsVariationSeparated);

            IsLoading = true;
            project_dashboards.Add(project_dashboard);
            return query => project_dashboards.AsQueryable();
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<PROJECT_Dashboard> entities)
        {
            MainViewModel.CanFillDownCallBack = CanFillDownCallBack;
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        private void SelectAllDispatcher_Tick(object sender, EventArgs e)
        {
            selectAllDispatcher.Stop();
            mainThreadDispatcher.BeginInvoke(new Action(() => SelectAll()));
        }

        protected BackgroundWorker summaryBackgroundWorker;
        protected override bool OnMainViewModelLoaded(IEnumerable<PROJECT_Dashboard> entities)
        {
            MainViewModel = (CollectionViewModel<PROJECT, PROJECT_Dashboard, Guid, IBluePrintsEntitiesUnitOfWork>)mainEntityLoaderDescription.GetViewModel();
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertiesChanged()));
            MainViewModel.SetParentViewModel(this);
            
            if (entities.Count() > 0)
            {
                this.SelectedEntity = entities.First();
                summaryBackgroundWorker = new BackgroundWorker();
                summaryBackgroundWorker.DoWork += summaryBackgroundWorker_DoWork;
                summaryBackgroundWorker.RunWorkerCompleted += summaryBackgroundWorker_RunWorkerCompleted;
                summaryBackgroundWorker.WorkerSupportsCancellation = true;
                summaryBackgroundWorker.RunWorkerAsync(new object[] { entities.First() });
            }

            base.OnMainViewModelLoaded(entities);
            return true;
        }

        protected virtual List<StatsCalculationType> getForecastTypes()
        {
            return null;
        }

        private void summaryBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (summaryBackgroundWorker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            var argumentObject = (object[])e.Argument;
            var project = (PROJECT_Dashboard)argumentObject[0];
            
            if(project != null)
            {
                project.BuildStats(DashboardEXOQueryType, ShowLoadingScreen, 1, ForceRetrieveAllJobs, ForceRetrieveAllUnits, ForceRetrieveAllPOs, getForecastTypes(), UseProductivityFactorOnRemaining, IsVariationSeparated);
                project.RecalculateStats(false, true);
                project.Subjob_Dashboards = getDashboardStructure(project, IsVariationSeparated, ForceRetrieveRemainingDataPoints);
                project.Update();

                foreach (var subjobDashboard in project.Subjob_Dashboards)
                {
                    if (subjobDashboard != null)
                    {
                        subjobDashboard.IsManaged = false;
                        if (SUBJOBCollection.Any(x => x.INTERNAL_NAME1 == subjobDashboard.SubjobCode))
                            subjobDashboard.IsManaged = true;
                    }
                }

                mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertyChanged(x => x.SingleProjectDashboards)));
                mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertyChanged(x => x.AllProjectDashboards)));
                mainThreadDispatcher.BeginInvoke(new Action(() => IsSummaryLoading = false));
            }

            if (((BackgroundWorker)sender).CancellationPending)
            {
                e.Cancel = true;
                return;
            }
        }

        protected virtual List<DashboardFlatStructure> getDashboardStructure(PROJECT_Dashboard project, bool isVariationSeparated, bool forceRetrieveRemainingDataPoints = false)
        {
            return DashboardHelpers.ProjectDashboardSummaryBuilder((ProjectSummaryStats)project.Stats, SUBJOBCollection, ShowLoadingScreen, isVariationSeparated, forceRetrieveRemainingDataPoints, DOCTYPECollection);
        }

        private void summaryBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //for raising can export to excel
            this.RaiseCanExecuteChanged(x => x.ExportToExcel());
            onSummaryCalculateComplete();
            if (selectAllDispatcher != null)
                selectAllDispatcher.Start();
        }

        protected override void OnClose(CancelEventArgs e)
        {
            summaryBackgroundWorker.CancelAsync();
            base.OnClose(e);
        }

        protected virtual void onSummaryCalculateComplete()
        {
            IsLoading = false;
            this.RaisePropertyChanged(x => x.IsLoading);
        }

        public override bool CanExportToExcel()
        {
            bool canBaseExportToExcel = base.CanExportToExcel();
            return canBaseExportToExcel && Entities != null && Entities.Count > 0 && Entities.First().Subjob_Dashboards != null;
        }

        protected string exportFileName = string.Empty;
        protected override string ExportFilename()
        {
            return exportFileName;
        }

        public bool IsExportInternalNameVisible { get; set; }
        protected bool skipDashboardExcelFomatting;
        public override void ExportToExcel()
        {
            if (!skipDashboardExcelFomatting)
                dashboardExcelFomatting();

            base.ExportToExcel();
        }

        private void dashboardExcelFomatting()
        {
            LoadingScreenManager.ShowLoadingScreen(1);
            PROJECT_Dashboard dashboard = Entities.First();
            dashboard.Export_Data = DashboardHelpers.BuildExportData(((ProjectSummaryStats)Entities.First().Stats).WBSReportables, DOCTYPECollection);

            IsExportInternalNameVisible = false;
            this.RaisePropertyChanged(x => x.IsExportInternalNameVisible);
            this.RaisePropertyChanged(x => x.ExcelExportData);
            LoadingScreenManager.CloseLoadingScreen();

            exportFileName = LoadPROJECT.NUMBER + "_" + "All_BP_Export" + ((DateTime)designDataDate).ToString("yyyyMMdd");
        }

        public bool CanExportRemaining()
        {
            return CanExportToExcel();
        }

        public void ExportRemaining()
        {
            export(StatsType.Remaining);
        }

        public bool CanExportEarned()
        {
            return CanExportToExcel();
        }

        public void ExportEarned()
        {
            export(StatsType.Earned);
        }

        public bool CanExportPlanned()
        {
            return CanExportToExcel();
        }

        public void ExportPlanned()
        {
            export(StatsType.Planned);
        }

        public bool CanExportCurrent()
        {
            return CanExportToExcel();
        }

        public void ExportCurrent()
        {
            export(StatsType.Current);
        }

        private void export(StatsType statsType)
        {
            LoadingScreenManager.ShowLoadingScreen(1);
            PROJECT_Dashboard dashboard = Entities.First();
            dashboard.Export_Data = DashboardHelpers.BuildExportDataByType(statsType, LoadPROJECT.NUMBER, dashboard);
            IsExportInternalNameVisible = true;
            this.RaisePropertyChanged(x => x.IsExportInternalNameVisible);
            this.RaisePropertyChanged(x => x.ExcelExportData);
            LoadingScreenManager.CloseLoadingScreen();

            exportFileName = LoadPROJECT.NUMBER + "_" + statsType.ToString() +  "_BP_Export" + ((DateTime)designDataDate).ToString("yyyyMMdd");
            base.ExportToExcel();
        }

        public bool CanProjectHealthCheck()
        {
            return CanExportToExcel() && HealthCheckIconName == "Warning";
        }

        bool isHealthCheckClicked = false;
        public void ProjectHealthCheck()
        {
            isHealthCheckClicked = true;
            doHealthCheck();
        }

        private void doHealthCheck()
        {
            bool moreThanOneDesignLiveProgress = false;
            bool noDesignLiveProgress = false;
            bool moreThanOneLiveBaseline = false;
            bool noLiveBaseline = false;
            IEnumerable<BASELINE> liveBASELINES = LoadPROJECT.BASELINE.Where(x => x.STATUS == BaselineStatus.Live);
            BASELINE liveBASELINE = liveBASELINES.FirstOrDefault();

            if (liveBASELINES.Count() > 1)
                moreThanOneLiveBaseline = true;
            else if (liveBASELINES.Count() == 0)
                noLiveBaseline = true;

            IEnumerable<PROGRESS> livePROGRESS = LoadPROJECT.PROGRESS.Where(x => x.STATUS == ProgressStatus.Live && x.TYPE == PhaseType.Design);
            PROGRESS livePRO = livePROGRESS.FirstOrDefault();
            if (livePROGRESS.Count() > 1)
                moreThanOneDesignLiveProgress = true;
            else if (livePROGRESS.Count() == 0)
                noDesignLiveProgress = true;

            List<ProjectIssue> projectIssues = new List<ProjectIssue>();
            if (LoadPROJECT.BASELINE.Count == 0)
                projectIssues.Add(new ProjectIssue() { Severity = "Critical", Type = "Baseline", Description = string.Format("Project have no live baseline"), Resolve = "Please create a live baseline" });
            else
            {
                if (moreThanOneLiveBaseline)
                    projectIssues.Add(new ProjectIssue() { Severity = "Critical", Type = "Baseline", Description = string.Format("Project have more than one live baseline"), Resolve = "Please set only a single baseline to live" });
                else if (noLiveBaseline)
                    projectIssues.Add(new ProjectIssue() { Severity = "Critical", Type = "Baseline", Description = string.Format("Project have no live baseline"), Resolve = "Please set set baseline to live" });
            }

            if (LoadPROJECT.PROGRESS.Count == 0)
                projectIssues.Add(new ProjectIssue() { Severity = "Critical", Type = "Progress", Description = string.Format("Project have no live Progress"), Resolve = "Please create a live design progress" });
            else
            {
                if (moreThanOneDesignLiveProgress)
                    projectIssues.Add(new ProjectIssue() { Severity = "Critical", Type = "Progress", Description = string.Format("Project have more than one live Progress"), Resolve = "Please set only a single design progress to live" });
                else if (noDesignLiveProgress)
                    projectIssues.Add(new ProjectIssue() { Severity = "Critical", Type = "Progress", Description = string.Format("Project have no live Progress"), Resolve = "Please set a design progress to live" });
            }

            if (livePRO != null && liveBASELINE != null)
            {
                if (livePRO.P6PROGRESS_NAME == string.Empty && liveBASELINE.P6BASELINE_NAME != string.Empty)
                    projectIssues.Add(new ProjectIssue() { Severity = "Critical", Type = "Progress", Description = string.Format("Live progress is not assigned with P6 schedule"), Resolve = "Please set a p6 schedule on live design progress" });
                else if (livePRO.P6PROGRESS_NAME != string.Empty && liveBASELINE.P6BASELINE_NAME == string.Empty)
                    projectIssues.Add(new ProjectIssue() { Severity = "Critical", Type = "Baseline", Description = string.Format("Live Baseline is not assigned with P6 schedule"), Resolve = "Please set a p6 schedule on live Baseline" });

                IP6EntitiesUnitOfWork p6uow = p6UnitOfWorkFactory.CreateUnitOfWork();
                if (livePRO.P6PROGRESS_NAME != string.Empty)
                {
                    P6Data.PROJWBS p6WBS = p6uow.PROJWBS.FirstOrDefault(x => x.wbs_short_name == livePRO.P6PROGRESS_NAME);
                    if (p6WBS == null)
                        projectIssues.Add(new ProjectIssue() { Severity = "Critical", Type = "Progress", Description = string.Format("P6 progress schedule not found"), Resolve = "Please set a new p6 schedule on live design progress" });
                    else if (p6WBS.PROJECT.last_recalc_date == null || (p6WBS.PROJECT.last_recalc_date != null && ((DateTime)p6WBS.PROJECT.last_recalc_date).Date != livePRO.DATA_DATE.Date))
                        projectIssues.Add(new ProjectIssue() { Severity = "Critical", Type = "Progress", Description = string.Format("P6 progress schedule data date don't match"), Resolve = "Please open up p6 schedule and process F9 on " + livePRO.DATA_DATE.ToString("G") });
                }

                if (liveBASELINE.P6BASELINE_NAME != string.Empty)
                {
                    P6Data.PROJWBS p6WBS = p6uow.PROJWBS.FirstOrDefault(x => x.wbs_short_name == liveBASELINE.P6BASELINE_NAME);
                    if (p6WBS == null)
                        projectIssues.Add(new ProjectIssue() { Severity = "Critical", Type = "Baseline", Description = string.Format("P6 baseline schedule not found"), Resolve = "Please set a new p6 schedule on live baseline" });
                }
            }

            if (projectIssues.Count > 0)
                showHealthCheckDialog(projectIssues);
            else
            {
                BASELINE_ITEMCollectionViewModelWrapper deliverablesWrapper = BASELINE_ITEMCollectionViewModelWrapper.Create();
                deliverablesWrapper.OnReportablesLoadedCallBack = OnDeliverablesLoadedCallBack;
                TripleEntitiesParameter<PROJECT, IAmBaseline, object> collectionViewParameter = new TripleEntitiesParameter<PROJECT, IAmBaseline, object>(LoadPROJECT, null, DeliverablesViewType.Both);
                deliverablesWrapper.OnParameterChange(collectionViewParameter);
            }
        }

        private void OnDeliverablesLoadedCallBack(IEnumerable<BASELINE_ITEMProgress> entities)
        {
            PROJECT_Dashboard dashboard = Entities.First();
            List<ProjectIssue> projectIssues = new List<ProjectIssue>();
            BASELINE liveBASELINE = LoadPROJECT.BASELINE.FirstOrDefault(x => x.STATUS == BaselineStatus.Live);
            PROGRESS livePROGRESS = LoadPROJECT.PROGRESS.FirstOrDefault(x => x.STATUS == ProgressStatus.Live && x.TYPE == PhaseType.Design);

            bool useP6 = false;
            if (liveBASELINE.P6BASELINE_NAME != string.Empty)
                useP6 = true;

            int noAreaCount = 0;
            int noRateCount = 0;
            int noDepartment = 0;
            int noSubjob = 0;
            int noWorkpack = 0;
            int noAssignment = 0;

            foreach(BASELINE_ITEMProgress entity in entities)
            {
                if (entity.Budget_ItemRate == 0)
                    noRateCount += 1;

                if (entity.Area_Guid == null)
                    noAreaCount += 1;

                if (entity.Department_Code == string.Empty)
                    noDepartment += 1;

                if (entity.Subjob_Guid == null)
                    noSubjob += 1;

                if (entity.Workpack_Guid == null)
                    noWorkpack += 1;

                if (useP6 && entity.Assigned_Percentage == 0)
                    noAssignment += 1;
            }

            if(livePROGRESS.REPORT_DATE != null)
                projectIssues.Add(new ProjectIssue() { Severity = "Warning", Type = "Progress", Description = "Report date is assigned to progress", Resolve = "Earned and burned data in S-Curve is limited to report date, please remove it if to view updated data" });

            if (noAreaCount > 0)
                projectIssues.Add(new ProjectIssue() { Severity = "Warning", Type = "Deliverables", Description = string.Format("{0} deliverable(s) doesn't have area assigned", noAreaCount), Resolve = "Please assign area to deliverable(s)" });

            if (noRateCount > 0)
                projectIssues.Add(new ProjectIssue() { Severity = "Warning", Type = "Deliverables", Description = string.Format("{0} deliverable(s) doesn't have rate", noRateCount), Resolve = "Please add a rate corresponding to deliverable(s) department, discipline and commodity" });

            if (noDepartment > 0)
                projectIssues.Add(new ProjectIssue() { Severity = "Warning", Type = "Deliverables", Description = string.Format("{0} deliverable(s) doesn't have department", noDepartment), Resolve = "Please assign department to deliverable(s)" });

            if (noSubjob > 0)
                projectIssues.Add(new ProjectIssue() { Severity = "Warning", Type = "Deliverables", Description = string.Format("{0} deliverable(s) doesn't have subjob", noSubjob), Resolve = "Please assign subjob to deliverable(s)" });

            if (noWorkpack > 0)
                projectIssues.Add(new ProjectIssue() { Severity = "Warning", Type = "Deliverables", Description = string.Format("{0} deliverable(s) doesn't have workpack", noWorkpack), Resolve = "Please assign workpack to deliverable(s)" });

            if (noAssignment > 0)
            {
                if(LoadPROJECT.USE_WORKPACKS)
                    projectIssues.Add(new ProjectIssue() { Severity = "Warning", Type = "Baseline", Description = string.Format("{0} deliverable(s) doesn't have P6 assignment", noAssignment), Resolve = "Please complete workpack(s) to p6 assignment(s)" });
                else
                    projectIssues.Add(new ProjectIssue() { Severity = "Warning", Type = "Baseline", Description = string.Format("{0} deliverable(s) doesn't have P6 assignment", noAssignment), Resolve = "Please complete deliverable(s) to p6 assignment(s)" });
            }

            mainThreadDispatcher.BeginInvoke(new Action(() => showHealthCheckDialog(projectIssues)));
        }

        private void showHealthCheckDialog(IEnumerable<ProjectIssue> projectIssues)
        {
            if(projectIssues.Count() == 0)
            {
                if (isHealthCheckClicked)
                    mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage(BluePrintsResources.Notify_HealthCheck_OK)));

                Animate = false;
                HealthCheckIconName = "Apply";
            }
            else
            {
                if (isHealthCheckClicked)
                {
                    DialogCollectionViewModel<ProjectIssue> projectIssueViewModel = DialogCollectionViewModel<ProjectIssue>.Create(projectIssues);
                    mainThreadDispatcher.BeginInvoke(new Action(() => ActivityDetailDialogService.ShowDialog(MessageButton.OK, "Project Health Check", "PROJECTIssueView", projectIssueViewModel)));
                }

                Animate = true;
                HealthCheckIconName = "Warning";
                Animate = true;
            }

            isHealthCheckClicked = false;
            this.RaisePropertyChanged(x => x.Animate);
            this.RaisePropertyChanged(x => x.HealthCheckIconName);
        }


        public bool Animate { get; set; }
        public string HealthCheckIconName { get; set; }
        public List<Dashboard_Export_Data_Point> ExcelExportData => Entities == null ? null : Entities.Count == 0 ? null : Entities.First().Export_Data;
        #endregion

        #region View Behavior
        protected override void executeFirstLoadedActions()
        {
            //remove health check because it's heavy
            //doHealthCheck();
            ChartControlService?.Animate();

            base.executeFirstLoadedActions();
        }

        public Action Redraw;

        public new void RaisePropertyChanged()
        {
            if (Redraw != null)
                mainThreadDispatcher.BeginInvoke(new Action(() => Redraw()));

            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertiesChanged()));
        }

        protected IOpenFileDialogService OpenFileDialogService
        {
            get { return this.GetService<IOpenFileDialogService>(); }
        }

        public bool CanFillDownCallBack(IEnumerable<PROJECT_Dashboard> selectedEntities, GridMenuInfo info)
        {
            if (info.Column.FieldName == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_KICKOFF) ||
                info.Column.FieldName == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_CLOSEOUT) ||
                info.Column.FieldName == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_SIDREPORT))
                return false;

            return true;
        }

        public override void UnifiedCellValueChanging(string field_name, object old_value, object new_value, PROJECT_Dashboard projection, bool isNew)
        {
            if (field_name == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_KICKOFF) ||
                   field_name == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_CLOSEOUT) ||
                   field_name == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_SIDREPORT))
            {
                MainViewModel.EntitiesUndoRedoManager.PauseActionId(); //Unpaused in existingRowAddUndoAndSave
                ProjectDocumentStatus newValue = (ProjectDocumentStatus)new_value;
                if (newValue == ProjectDocumentStatus.Yes)
                {
                    OpenFileDialogService.Filter = "PDF (*.PDF)|*.PDF";
                    bool DialogResult;

                    DialogResult = OpenFileDialogService.ShowDialog();
                    if (DialogResult)
                    {
                        string fullPath = OpenFileDialogService.File.GetFullName();
                        if (field_name == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_KICKOFF))
                        {
                            MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_KICKOFF_PATH), null, fullPath, EntityMessageType.Changed);
                            projection.Entity.DOC_KICKOFF_PATH = fullPath;
                        }
                        else if (field_name == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_CLOSEOUT))
                        {
                            MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_CLOSEOUT_PATH), null, fullPath, EntityMessageType.Changed);
                            projection.Entity.DOC_CLOSEOUT_PATH = fullPath;
                        }
                        else
                        {
                            MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_SIDREPORT), null, fullPath, EntityMessageType.Changed);
                            projection.Entity.DOC_SIDREPORT_PATH = fullPath;
                        }
                    }
                }
                else
                {
                    if (field_name == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_KICKOFF))
                    {
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_KICKOFF_PATH), projection.Entity.DOC_KICKOFF_PATH, null, EntityMessageType.Changed);
                        projection.Entity.DOC_KICKOFF_PATH = null;
                    }
                    else if (field_name == BindableBase.GetPropertyName(() => new PROJECT().DOC_CLOSEOUT))
                    {
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_CLOSEOUT_PATH), projection.Entity.DOC_CLOSEOUT_PATH, null, EntityMessageType.Changed);
                        projection.Entity.DOC_CLOSEOUT_PATH = null;
                    }
                    else
                    {
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_SIDREPORT_PATH), projection.Entity.DOC_SIDREPORT_PATH, null, EntityMessageType.Changed);
                        projection.Entity.DOC_SIDREPORT_PATH = null;
                    }
                }
            }

            if (field_name == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().STATUS))
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                BluePrintsContextHelper.AsyncRefreshDeliverablesDataPointsByProject(projection.Entity.NUMBER);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }

            base.UnifiedCellValueChanging(field_name, old_value, new_value, projection, isNew);
        }


        decimal runningTotals;
        decimal runningCurrent;
        decimal runningPeriod;
        decimal currentValue;
        public void CustomSummary(CustomSummaryEventArgs e)
        {
            GridSummaryItem gridSummaryItem = e.Item as GridSummaryItem;
            string fieldName = gridSummaryItem.FieldName;
            if (e.SummaryProcess == CustomSummaryProcess.Start)
            {
                runningPeriod = 0;
                runningCurrent = 0;
                runningTotals = 0;
                currentValue = 0;
            }
            if (e.SummaryProcess == CustomSummaryProcess.Calculate)
            {
                if (gridSummaryItem != null)
                {
                    bool is_cost = fieldName.ToUpper().Contains("COSTS");
                    bool is_period = !fieldName.ToUpper().Contains("CUMULATIVE");
                    bool is_ratio = fieldName.ToUpper().Contains("RATIO");
                    bool is_tender = fieldName.ToUpper().Contains("TENDER");
                    bool is_earn = fieldName.ToUpper().Contains("EARNED");
                    bool is_budget = fieldName.ToUpper().Contains("BUDGETED");
                    bool is_current = fieldName.ToUpper().Contains("CURRENT");

                    if (is_cost)
                    {
                        SummaryStats summary = ((IHaveStats)e.Row).Stats as SummaryStats;
                        if (is_ratio)
                        {
                            if (((IHaveStats)e.Row).Stats.Earned.CurrentPeriodCumulativeDataPoint != null)
                                runningTotals += ((IHaveStats)e.Row).Stats.Earned.CurrentPeriodCumulativeDataPoint.Costs;
                        }
                        else
                        {
                            if(is_tender)
                                runningTotals += ((IHaveStats)e.Row).Stats.BudgetedCosts;
                            else
                                runningTotals += ((IHaveStats)e.Row).Stats.TotalCosts;
                        }

                        if (e.IsGroupSummary && ((IHaveStats)e.Row).Stats.Earned != null)
                        {
                            if (is_period)
                            {
                                if (is_ratio)
                                {
                                    if (summary != null && summary.Burned != null && summary.Burned.CurrentPeriodDataPoint != null)
                                        currentValue = summary.Burned.CurrentPeriodDataPoint.Costs;
                                }
                                else
                                {
                                    if(is_earn)
                                    {
                                        if (((IHaveStats)e.Row).Stats.Earned.CurrentPeriodDataPoint != null)
                                            currentValue = ((IHaveStats)e.Row).Stats.Earned.CurrentPeriodDataPoint.Costs;
                                    }
                                    else if(is_budget)
                                    {
                                        if (((IHaveStats)e.Row).Stats.Budgeted.CurrentPeriodDataPoint != null)
                                            currentValue = ((IHaveStats)e.Row).Stats.Budgeted.CurrentPeriodDataPoint.Costs;
                                    }
                                    else if(is_current)
                                    {
                                        if (((IHaveStats)e.Row).Stats.Current.CurrentPeriodDataPoint != null)
                                            currentValue = ((IHaveStats)e.Row).Stats.Current.CurrentPeriodDataPoint.Costs;
                                    }
                                }
                            }
                            else
                            {
                                if (is_ratio)
                                {
                                    if (summary != null && summary.Burned != null && summary.Burned.CurrentPeriodCumulativeDataPoint != null)
                                        currentValue = summary.Burned.CurrentPeriodCumulativeDataPoint.Costs;
                                }
                                else
                                {
                                    if (is_earn)
                                    {
                                        if (((IHaveStats)e.Row).Stats.Earned.CurrentPeriodCumulativeDataPoint != null)
                                            currentValue = ((IHaveStats)e.Row).Stats.Earned.CurrentPeriodCumulativeDataPoint.Costs;
                                    }
                                    else if (is_budget)
                                    {
                                        if (((IHaveStats)e.Row).Stats.Budgeted.CurrentPeriodCumulativeDataPoint != null)
                                            currentValue = ((IHaveStats)e.Row).Stats.Budgeted.CurrentPeriodCumulativeDataPoint.Costs;
                                    }
                                    else if (is_current)
                                    {
                                        if (((IHaveStats)e.Row).Stats.Current.CurrentPeriodCumulativeDataPoint != null)
                                            currentValue = ((IHaveStats)e.Row).Stats.Current.CurrentPeriodCumulativeDataPoint.Costs;
                                    }
                                }
                            }
                        }
                        else if (e.IsTotalSummary)
                        {
                            if (is_period)
                            {
                                if (is_ratio)
                                {
                                    if (summary != null && summary.Burned != null && summary.Burned.CurrentPeriodDataPoint != null)
                                        runningPeriod += summary.Burned.CurrentPeriodDataPoint.Costs;
                                }
                                else
                                {
                                    if (is_earn)
                                    {
                                        if (((IHaveStats)e.Row).Stats.Earned.CurrentPeriodDataPoint != null)
                                            runningPeriod += ((IHaveStats)e.Row).Stats.Earned.CurrentPeriodDataPoint.Costs;
                                    }
                                    else if (is_budget)
                                    {
                                        if (((IHaveStats)e.Row).Stats.Budgeted.CurrentPeriodDataPoint != null)
                                            runningPeriod += ((IHaveStats)e.Row).Stats.Budgeted.CurrentPeriodDataPoint.Costs;
                                    }
                                    else if (is_current)
                                    {
                                        if (((IHaveStats)e.Row).Stats.Current.CurrentPeriodDataPoint != null)
                                            runningPeriod += ((IHaveStats)e.Row).Stats.Current.CurrentPeriodDataPoint.Costs;
                                    }
                                }
                            }
                            else
                            {
                                if (is_ratio)
                                {
                                    if (summary != null && summary.Burned != null && summary.Burned.CurrentPeriodCumulativeDataPoint != null)
                                        runningCurrent += summary.Burned.CurrentPeriodCumulativeDataPoint.Costs;
                                }
                                else
                                {
                                    if(is_tender)
                                    {
                                        if (((IHaveStats)e.Row).Stats.TenderEarned.CurrentPeriodCumulativeDataPoint != null)
                                            runningCurrent += ((IHaveStats)e.Row).Stats.TenderEarned.CurrentPeriodCumulativeDataPoint.Costs;
                                    }
                                    if (is_earn)
                                    {
                                        if (((IHaveStats)e.Row).Stats.Earned.CurrentPeriodCumulativeDataPoint != null)
                                            runningCurrent += ((IHaveStats)e.Row).Stats.Earned.CurrentPeriodCumulativeDataPoint.Costs;
                                    }
                                    else if (is_budget)
                                    {
                                        if (((IHaveStats)e.Row).Stats.Budgeted.CurrentPeriodCumulativeDataPoint != null)
                                            runningCurrent += ((IHaveStats)e.Row).Stats.Budgeted.CurrentPeriodCumulativeDataPoint.Costs;
                                    }
                                    else if (is_current)
                                    {
                                        if (((IHaveStats)e.Row).Stats.Current.CurrentPeriodCumulativeDataPoint != null)
                                            runningCurrent += ((IHaveStats)e.Row).Stats.Current.CurrentPeriodCumulativeDataPoint.Costs;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        SummaryStats summary = ((IHaveStats)e.Row).Stats as SummaryStats;
                        if (is_ratio)
                        {
                            if (is_tender)
                            {
                                if (((IHaveStats)e.Row).Stats.TenderEarned.CurrentPeriodCumulativeDataPoint != null)
                                    runningTotals += ((IHaveStats)e.Row).Stats.TenderEarned.CurrentPeriodCumulativeDataPoint.Units;
                            }
                            if (is_earn)
                            {
                                if (((IHaveStats)e.Row).Stats.Earned.CurrentPeriodCumulativeDataPoint != null)
                                    runningTotals += ((IHaveStats)e.Row).Stats.Earned.CurrentPeriodCumulativeDataPoint.Units;
                            }
                            else if(is_budget)
                            {
                                if (((IHaveStats)e.Row).Stats.Budgeted.CurrentPeriodCumulativeDataPoint != null)
                                    runningTotals += ((IHaveStats)e.Row).Stats.Budgeted.CurrentPeriodCumulativeDataPoint.Units;
                            }
                            else if (is_current)
                            {
                                if (((IHaveStats)e.Row).Stats.Current.CurrentPeriodCumulativeDataPoint != null)
                                    runningTotals += ((IHaveStats)e.Row).Stats.Current.CurrentPeriodCumulativeDataPoint.Units;
                            }
                        }
                        else
                        {
                            if(is_tender || is_budget)
                                runningTotals += ((IHaveStats)e.Row).Stats.BudgetedUnits;
                            else if (is_current || is_earn)
                                runningTotals += ((IHaveStats)e.Row).Stats.TotalUnits;
                        }
                        if (e.IsGroupSummary)
                        {
                            if (is_period)
                            {
                                if(is_ratio)
                                {
                                    if (summary != null && summary.Burned != null && summary.Burned.CurrentPeriodDataPoint != null)
                                        currentValue = summary.Burned.CurrentPeriodDataPoint.Units;
                                }
                                else
                                {
                                    if(is_tender)
                                    {
                                        if (((IHaveStats)e.Row).Stats.TenderEarned.CurrentPeriodDataPoint != null)
                                            currentValue = ((IHaveStats)e.Row).Stats.TenderEarned.CurrentPeriodDataPoint.Units;
                                    }
                                    else if (is_earn)
                                    {
                                        if (((IHaveStats)e.Row).Stats.Earned.CurrentPeriodDataPoint != null)
                                            currentValue = ((IHaveStats)e.Row).Stats.Earned.CurrentPeriodDataPoint.Units;
                                    }
                                    else if (is_budget)
                                    {
                                        if (((IHaveStats)e.Row).Stats.Budgeted.CurrentPeriodDataPoint != null)
                                            currentValue = ((IHaveStats)e.Row).Stats.Budgeted.CurrentPeriodDataPoint.Units;
                                    }
                                    else if (is_current)
                                    {
                                        if (((IHaveStats)e.Row).Stats.Current.CurrentPeriodDataPoint != null)
                                            currentValue = ((IHaveStats)e.Row).Stats.Current.CurrentPeriodDataPoint.Units;
                                    }
                                }
                            }
                            else
                            {
                                if (is_ratio)
                                {
                                    if (summary != null && summary.Burned != null && summary.Burned.CurrentPeriodCumulativeDataPoint != null)
                                        currentValue = summary.Burned.CurrentPeriodCumulativeDataPoint.Units;
                                }
                                else
                                {
                                    if (is_tender)
                                    {
                                        if (((IHaveStats)e.Row).Stats.TenderEarned.CurrentPeriodCumulativeDataPoint != null)
                                            currentValue = ((IHaveStats)e.Row).Stats.TenderEarned.CurrentPeriodCumulativeDataPoint.Units;
                                    }
                                    else if (is_earn)
                                    {
                                        if(((IHaveStats)e.Row).Stats.Earned.CurrentPeriodCumulativeDataPoint != null)
                                            currentValue = ((IHaveStats)e.Row).Stats.Earned.CurrentPeriodCumulativeDataPoint.Units;
                                    }
                                    else if (is_budget)
                                    {
                                        if (((IHaveStats)e.Row).Stats.Budgeted.CurrentPeriodCumulativeDataPoint != null)
                                            currentValue = ((IHaveStats)e.Row).Stats.Budgeted.CurrentPeriodDataPoint.Units;
                                    }
                                    else if (is_current)
                                    {
                                        if (((IHaveStats)e.Row).Stats.Current.CurrentPeriodCumulativeDataPoint != null)
                                            currentValue = ((IHaveStats)e.Row).Stats.Current.CurrentPeriodDataPoint.Units;
                                    }
                                }
                            }
                        }
                        else if (e.IsTotalSummary)
                        {
                            if (is_period)
                            {
                                if (is_ratio)
                                {
                                    if (summary != null && summary.Burned != null && summary.Burned.CurrentPeriodCumulativeDataPoint != null)
                                        runningPeriod += summary.Burned.CurrentPeriodDataPoint.Units;
                                }
                                else
                                {
                                    if (is_tender)
                                    {
                                        if (((IHaveStats)e.Row).Stats.TenderEarned.CurrentPeriodDataPoint != null)
                                            runningPeriod = ((IHaveStats)e.Row).Stats.TenderEarned.CurrentPeriodDataPoint.Units;
                                    }
                                    else if (is_earn)
                                    {
                                        if (((IHaveStats)e.Row).Stats.Earned.CurrentPeriodDataPoint != null)
                                            runningPeriod = ((IHaveStats)e.Row).Stats.Earned.CurrentPeriodDataPoint.Units;
                                    }
                                    else if (is_budget)
                                    {
                                        if (((IHaveStats)e.Row).Stats.Budgeted.CurrentPeriodDataPoint != null)
                                            runningPeriod = ((IHaveStats)e.Row).Stats.Budgeted.CurrentPeriodDataPoint.Units;
                                    }
                                    else if (is_current)
                                    {
                                        if (((IHaveStats)e.Row).Stats.Current.CurrentPeriodDataPoint != null)
                                            runningPeriod = ((IHaveStats)e.Row).Stats.Current.CurrentPeriodDataPoint.Units;
                                    }
                                }
                                //else
                                //{
                                //    if (is_tender || is_budget)
                                //        runningTotals += ((IHaveStats)e.Row).Stats.BudgetedUnits;
                                //    else if (is_current || is_earn)
                                //    {
                                //        runningTotals += ((IHaveStats)e.Row).Stats.TotalUnits;
                                //    }
                                //}
                            }
                            else
                            {
                                if (is_ratio)
                                {
                                    if (summary != null && summary.Burned != null && summary.Burned.CurrentPeriodCumulativeDataPoint != null)
                                        runningCurrent += summary.Burned.CurrentPeriodCumulativeDataPoint.Units;
                                }
                                else
                                {
                                    if (is_tender)
                                    {
                                        if (((IHaveStats)e.Row).Stats.TenderEarned.CurrentPeriodCumulativeDataPoint != null)
                                            runningCurrent += ((IHaveStats)e.Row).Stats.TenderEarned.CurrentPeriodCumulativeDataPoint.Units;
                                    }
                                    else if (is_earn)
                                    {
                                        if (((IHaveStats)e.Row).Stats.Earned.CurrentPeriodCumulativeDataPoint != null)
                                            runningCurrent += ((IHaveStats)e.Row).Stats.Earned.CurrentPeriodCumulativeDataPoint.Units;
                                    }
                                    else if (is_budget)
                                    {
                                        if (((IHaveStats)e.Row).Stats.Budgeted.CurrentPeriodCumulativeDataPoint != null)
                                            runningCurrent += ((IHaveStats)e.Row).Stats.Budgeted.CurrentPeriodCumulativeDataPoint.Units;
                                    }
                                    else if (is_current)
                                    {
                                        if (((IHaveStats)e.Row).Stats.Current.CurrentPeriodCumulativeDataPoint != null)
                                            runningCurrent += ((IHaveStats)e.Row).Stats.Current.CurrentPeriodCumulativeDataPoint.Units;
                                    }
                                }
                            }
                        }
                    }

                    if (runningTotals != 0)
                    {
                        if (e.IsGroupSummary)
                            e.TotalValue = currentValue / runningTotals;
                        else if (e.IsTotalSummary)
                        {
                            if (is_period)
                                e.TotalValue = runningPeriod / runningTotals;
                            else
                                e.TotalValue = runningCurrent / runningTotals;
                        }
                    }
                    else
                        e.TotalValue = 0;

                    if (is_ratio && e.TotalValue != null && decimal.Parse(e.TotalValue.ToString()) == 0)
                        e.TotalValue = 1;
                }
                else
                    e.TotalValue = 0;


            }
        }

        public void Refresh_From_P6()
        {
            if(moveTenderDates())
                AsyncRefresh_From_P6();
        }

        private async void AsyncRefresh_From_P6()
        {
            //when project is approved to active, tender dates should be removed
            moveTenderDates();
            LoadingScreenManager.ShowLoadingScreen(1);
            await BluePrintsContextHelper.RefreshDeliverablesDataPointsByProject(LoadPROJECT.NUMBER);
            LoadingScreenManager.Progress();
            FullRefresh();
        }

        private bool moveTenderDates()
        {
            if(LoadPROJECT.STATUS == ProjectStatus.Active)
            {
                if (bluePrintsUnitOfWork == null)
                    bluePrintsUnitOfWork = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();

                BASELINE liveBASELINE = bluePrintsUnitOfWork.BASELINES.Where(x => x.GUID_PROJECT == LoadPROJECT.GUID && x.STATUS == BaselineStatus.Live).FirstOrDefault();
                if(liveBASELINE != null)
                {
                    List<BASELINE_ITEM> deliverables = liveBASELINE.BASELINE_ITEM.ToList();
                    if(deliverables.Any(x => x.TENDER_START_DATE != null || x.TENDER_START_DATE != null))
                    {
                        if (MessageBoxService.ShowMessage("There are deliverables with Tender Start/End dates but project is now in Active status\n\nPlease note that Tender Start/End dates will be moved to Start/End dates in deliverables list", "Tender Dates Found", MessageButton.OK) == MessageResult.OK)
                        {
                            foreach (BASELINE_ITEM deliverable in deliverables)
                            {
                                deliverable.START_DATE = deliverable.TENDER_START_DATE;
                                deliverable.END_DATE = deliverable.TENDER_END_DATE;
                                deliverable.TENDER_START_DATE = null;
                                deliverable.TENDER_END_DATE = null;
                                bluePrintsUnitOfWork.SaveChanges();
                            }

                            return true;
                        }
                        else
                            return false;
                    }
                }
            }

            return true;
        }

        public override string UnifiedRowValidation(PROJECT_Dashboard projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(PROJECT_Dashboard projection, string field_name, object new_value, bool isPaste)
        {
            string missingPathErrorString = "Path not selected";
            bool isError = false;

            if (field_name == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_KICKOFF))
            {
                ProjectDocumentStatus newValue = (ProjectDocumentStatus)new_value;
                if (newValue == ProjectDocumentStatus.Yes && projection.Entity.DOC_KICKOFF_PATH == null)
                    isError = true;
            }
            else if (field_name == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_CLOSEOUT))
            {
                ProjectDocumentStatus newValue = (ProjectDocumentStatus)new_value;
                if (newValue == ProjectDocumentStatus.Yes && projection.Entity.DOC_CLOSEOUT_PATH == null)
                    isError = true;
            }
            else if (field_name == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_SIDREPORT))
            {
                ProjectDocumentStatus newValue = (ProjectDocumentStatus)new_value;
                if (newValue == ProjectDocumentStatus.Yes && projection.Entity.DOC_SIDREPORT_PATH == null)
                    isError = true;
            }

            if (isError)
                return missingPathErrorString;

            return string.Empty;
        }
#endregion

#region View Properties
        protected override void OnAfterSelectedEntitiesChanged()
        {
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaiseSelectionChanged()));
            base.OnAfterSelectedEntitiesChanged();
        }

        public void RaiseSelectionChanged()
        {
            this.RaisePropertyChanged(x => x.IsAllSelected);
            this.RaisePropertyChanged(x => x.IsAllDesignSelected);
            this.RaisePropertyChanged(x => x.IsAllConstructSelected);
        }

        public bool IsAllSelected
        {
            get
            {
                if (SingleProjectDashboards == null)
                    return false;

                return Selected_Dashboards.Count >= SingleProjectDashboards.Count();
            }
        }

        public bool IsAllDesignSelected
        {
            get
            {
                if (SingleProjectDashboards == null)
                    return false;

                IEnumerable<DashboardFlatStructure> selected_dashboards = Selected_Dashboards.Select(x => (DashboardFlatStructure)x);
                IEnumerable<DashboardFlatStructure> designDashboards = SingleProjectDashboards.Where(x => x.Phase != null && x.Phase == PhaseType.Design);
                return designDashboards.All(x => Selected_Dashboards.Any(y => y == x));
            }
        }

        public bool IsAllConstructSelected
        {
            get
            {
                if (SingleProjectDashboards == null)
                    return false;

                IEnumerable<DashboardFlatStructure> selected_dashboards = Selected_Dashboards.Select(x => (DashboardFlatStructure)x);
                IEnumerable<DashboardFlatStructure> constructDashboard = SingleProjectDashboards.Where(x => x.Phase != null && x.Phase == PhaseType.Construct);
                return constructDashboard.All(x => Selected_Dashboards.Any(y => y == x));
            }
        }

        public void SelectAll()
        {
            if (SingleProjectDashboards == null)
                return;

            Selected_Dashboards.Clear();
            isSuppressPropertyChange = false;
            foreach (DashboardFlatStructure subjob_dashboard in SingleProjectDashboards)
            {
                Selected_Dashboards.Add(subjob_dashboard);
            }
        }

        public bool CanSelectSubjob(BarCheckItem button)
        {
            if (SingleProjectDashboards == null)
                return false;

            PhaseType? button_phase = button.Content.ToString() == PhaseType.Construct.ToString() ? PhaseType.Construct : button.Content.ToString() == PhaseType.Design.ToString() ? PhaseType.Design : (PhaseType?)null;
            return SingleProjectDashboards.Any(x => x.Phase == button_phase);
        }

        public void SelectSubjob(BarCheckItem button)
        {
            PhaseType? select_phase = button.Content.ToString() == PhaseType.Construct.ToString() ? PhaseType.Construct : button.Content.ToString() == PhaseType.Design.ToString() ? PhaseType.Design : (PhaseType?)null;
            
            if(button.IsChecked != null)
            {
                bool isCheck = (bool)button.IsChecked;
                if (isCheck)
                {
                    foreach (DashboardFlatStructure subjob_dashboard in SingleProjectDashboards.Where(x => x.Phase == select_phase))
                    {
                        Selected_Dashboards.Add(subjob_dashboard);
                    }
                }
                else
                {
                    List<DashboardFlatStructure> removeSubJobs = new List<DashboardFlatStructure>();
                    foreach (DashboardFlatStructure subjob_dashboard in SingleProjectDashboards.Where(x => x.Phase == select_phase))
                    {
                        removeSubJobs.Add(subjob_dashboard);
                    }

                    foreach(DashboardFlatStructure removeSubJob in removeSubJobs)
                    {
                        Selected_Dashboards.Remove(removeSubJob);
                    }
                }
            }
        }

        public IEnumerable<DashboardFlatStructure> SingleProjectDashboards
        {
            get
            {
                if (Entities == null || Entities.Count == 0)
                    return null;

                //Only return managed subjobs
                List<DashboardFlatStructure> dashboards = Entities.First().Subjob_Dashboards;
                IEnumerable<DashboardFlatStructure> singleProjectDashboard = null;
                if (dashboards != null)
                    singleProjectDashboard = dashboards.Where(x => x != null).Where(x => !x.ShouldHide);

                return singleProjectDashboard;
            }
        }

        /// <summary>
        /// Includes procurement and indirects
        /// </summary>
        public IEnumerable<DashboardFlatStructure> AllProjectDashboards
        {
            get
            {
                if (Entities == null || Entities.Count == 0)
                    return null;

                List<DashboardFlatStructure> singleProjectDashboard = Entities.First().Subjob_Dashboards;
                return singleProjectDashboard;
            }
        }

        private BASELINECollectionViewModelWrapper baselineViewModel;

        public BASELINECollectionViewModelWrapper BASELINEViewModel
        {
            get
            {
                if (baselineViewModel == null && LoadPROJECT != null)
                {
                    baselineViewModel = BASELINECollectionViewModelWrapper.Create();
                    baselineViewModel.SetParentViewModel(this);
                    var baselineSupportParameterObj = baselineViewModel as ISupportParameter;
                    baselineSupportParameterObj.Parameter = new EntitiesParameter<PROJECT>(LoadPROJECT);
                    AssignBASELINEDelegates?.Invoke(baselineViewModel);
                }

                return baselineViewModel;
            }
        }

        private PROGRESSCollectionViewModelWrapper progressViewModel;

        public PROGRESSCollectionViewModelWrapper PROGRESSViewModel
        {
            get
            {
                if (progressViewModel == null && LoadPROJECT != null)
                {
                    progressViewModel = PROGRESSCollectionViewModelWrapper.Create();
                    progressViewModel.SetParentViewModel(this);
                    var baselineSupportParameterObj = progressViewModel as ISupportParameter;
                    baselineSupportParameterObj.Parameter = new EntitiesParameter<PROJECT>(LoadPROJECT);
                    AssignPROGRESSDelegates?.Invoke(progressViewModel);
                }

                return progressViewModel;
            }
        }

        private AREACollectionViewModelWrapper areaViewModel;

        public AREACollectionViewModelWrapper AREAViewModel
        {
            get
            {
                if (areaViewModel == null && LoadPROJECT != null)
                {
                    areaViewModel = AREACollectionViewModelWrapper.Create();
                    areaViewModel.SetParentViewModel(this);
                    var baselineSupportParameterObj = areaViewModel as ISupportParameter;
                    baselineSupportParameterObj.Parameter = new EntitiesParameter<PROJECT>(LoadPROJECT);
                    AssignAREADelegates?.Invoke(areaViewModel);
                }

                return areaViewModel;
            }
        }

        private ESTIMATECollectionViewModelWrapper estimationDirectViewModel;

        public ESTIMATECollectionViewModelWrapper ESTIMATEViewModel
        {
            get
            {
                if (estimationDirectViewModel == null && LoadPROJECT != null)
                {
                    estimationDirectViewModel = ESTIMATECollectionViewModelWrapper.Create();
                    estimationDirectViewModel.SetParentViewModel(this);
                    var baselineSupportParameterObj = estimationDirectViewModel as ISupportParameter;
                    baselineSupportParameterObj.Parameter = new EntitiesParameter<PROJECT>(LoadPROJECT);
                    AssignESTIMATEDelegates?.Invoke(estimationDirectViewModel);
                }

                return estimationDirectViewModel;
            }
        }

        public virtual bool CanEditReport()
        {
            return !IsLoading;
        }

        public bool CanEdit()
        {
            if (SelectedEntity == null)
                return false;

            return !IsLoading;
        }

        public virtual void EditReport()
        {
            var reportDesigner = new UserReportDesigner(LoadPROJECT, (CollectionViewModel<PROJECT_REPORT, PROJECT_REPORT, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<PROJECT_REPORT>(), ReportType.Project_Report);
            if (reportDesigner.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                reportDesigner.Dispose();
            else
                reportDesigner.Dispose();
        }

        protected override void loadReportLayoutFromDatabase(XtraReportDashboard xtraReport)
        {
            var dbProjectReport = loaderCollection.GetObject<PROJECT_REPORT>();
            if (dbProjectReport != null)
            {
                var reportString = dbProjectReport.REPORT.ToString();
                using (var sw = new StreamWriter(new MemoryStream()))
                {
                    sw.Write(reportString);
                    sw.Flush();
                    xtraReport.LoadLayout(sw.BaseStream);
                }
            }

            base.loadReportLayoutFromDatabase(xtraReport);
        }

        public override void FullRefresh()
        {
            IsLoading = true;
            base.FullRefresh();
            ReloadEntitiesCollection();
        }

        protected IDocumentManagerService DocumentManagerService
        {
            get { return this.GetService<IDocumentManagerService>(); }
        }

        public void Edit()
        {
            if (SelectedEntity == null)
                return;

            DocumentInfo DocumentInfo = new DocumentInfo(SelectedEntity.GUID.ToString() + "SubjobDashboardView",
                SelectedEntity,
                "SUBJOBDashboardView",
                "[" + SelectedEntity.Entity.NUMBER + "] SUBJOB Dashboard");

            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "PROJECTViewModelWrapper_v3" + view_project_specific_affix; }
        }

        private string view_project_specific_affix
        {
            get
            {
                if (LoadPROJECT == null)
                    return string.Empty;
                return LoadPROJECT.GUID.ToString();
            }
        }

        public void EditArea()
        {
            if (LoadPROJECT == null)
                return;

            DocumentInfo DocumentInfo = new DocumentInfo("View_ProjectAreas" + LoadPROJECT.GUID.ToString(),
                new EntitiesParameter<PROJECT>(LoadPROJECT),
                    "AREACollectionView",
                    "[" + LoadPROJECT.NUMBER + "] Areas");

            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }

        public void EditBaseline()
        {
            if (LoadPROJECT == null)
                return;

            DocumentInfo DocumentInfo = new DocumentInfo("View_ProjectBaselines" + LoadPROJECT.GUID.ToString() ,
                new EntitiesParameter<PROJECT>(LoadPROJECT),
                    "BASELINECollectionView",
                    "[" + LoadPROJECT.NUMBER + "] Baseline Revisions");

            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }

        public void EditEstimate()
        {
            if (LoadPROJECT == null)
                return;

            DocumentInfo DocumentInfo = new DocumentInfo("View_ProjectEstimates" + LoadPROJECT.GUID.ToString(),
                new EntitiesParameter<PROJECT>(LoadPROJECT),
                    "ESTIMATECollectionView",
                    "[" + LoadPROJECT.NUMBER + "] Estimate Revisions");

            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }

        public void EditProgress()
        {
            if (LoadPROJECT == null)
                return;

            DocumentInfo DocumentInfo = new DocumentInfo("View_ProjectProgress" + LoadPROJECT.GUID.ToString(),
                new EntitiesParameter<PROJECT>(LoadPROJECT),
                    "PROGRESSCollectionView",
                    "[" + LoadPROJECT.NUMBER + "] Progress Revisions");

            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }

        public IEnumerable<USER> MANAGERCollection
        {
            get
            {
                var collection = GetEntities<USER>();
                if (collection != null)
                    collection = collection.Where(x => x.ROLE != null && x.ROLE.ISMANAGER).OrderBy(x => x.NAME);
                return collection;
            }
        }

        protected PROGRESS liveDesignProgress
        {
            get
            {
                var collection = GetEntities<PROGRESS>();
                if (collection != null)
                    return collection.FirstOrDefault(x => x.STATUS == ProgressStatus.Live && x.TYPE == PhaseType.Design);

                return null;
            }
        }

        protected DateTime? designDataDate
        {
            get
            {
                if (liveDesignProgress != null)
                {
                    DateTime dateToUse;
                    if (liveDesignProgress.REPORT_DATE != null)
                        dateToUse = (DateTime)liveDesignProgress.REPORT_DATE;
                    else
                        dateToUse = liveDesignProgress.DATA_DATE;

                    return dateToUse;
                }

                return null;
            }
        }

        public string DesignDataDate
        {
            get
            {
                if (designDataDate == null)
                    return "N/A";
                else
                    return ((DateTime)designDataDate).ToString("dd-MMM-yy");
            }
        }

        public bool CanDateBackward()
        {
            if (IsLoading || liveDesignProgress == null)
                return false;

            if (liveDesignProgress.DATA_DATE > liveDesignProgress.PROGRESS_START)
                return true;

            return false;
        }

        public bool CanDateForward()
        {
            if (IsLoading || liveDesignProgress == null)
                return false;

            return true;
        }

        public void DateForward()
        {
            DateChange(DateNavigationType.Forward);
        }

        public void DateBackward()
        {
            DateChange(DateNavigationType.Backward);
        }

        protected void DateChange(DateNavigationType navigationType)
        {
            if (BluePrintsUtils.ProgressDateChange(navigationType, liveDesignProgress, true))
            {
                PROGRESSCollectionViewModel.Save(liveDesignProgress);
                delayedRefreshDispatcher.Start();
            }
        }

        public bool IsDisableAutoReportDate
        {
            get
            {
                if (liveDesignProgress == null)
                    return false;

                return liveDesignProgress.DISABLE_AUTO_REPORT_DATE;
            }
            set
            {
                if(PROGRESSCollectionViewModel != null)
                {
                    liveDesignProgress.DISABLE_AUTO_REPORT_DATE = !liveDesignProgress.DISABLE_AUTO_REPORT_DATE;
                    PROGRESSCollectionViewModel.Save(liveDesignProgress);
                }
            }
        }

        protected virtual void delayedRefreshDispatcher_Tick(object sender, EventArgs e)
        {
            delayedRefreshDispatcher.Stop();
            FullRefresh();
        }

        protected PROGRESS liveConstructProgress
        {
            get
            {
                var collection = GetEntities<PROGRESS>();
                if (collection != null)
                    return collection.FirstOrDefault(x => x.STATUS == ProgressStatus.Live && x.TYPE == PhaseType.Construct);

                return null;
            }
        }

        protected DateTime? constructDataDate
        {
            get
            {
                if (liveConstructProgress != null)
                {
                    DateTime dateToUse;
                    if (liveConstructProgress.REPORT_DATE != null)
                        dateToUse = (DateTime)liveConstructProgress.REPORT_DATE;
                    else
                        dateToUse = liveConstructProgress.DATA_DATE;

                    return dateToUse;
                }

                return null;
            }
        }

        public string ConstructDataDate
        {
            get
            {
                if (constructDataDate == null)
                    return "N/A";
                else
                    return ((DateTime)constructDataDate).ToString("dd-MMM-yy");
            }
        }

        public IEnumerable<BASELINE_ITEM_WORK> BASELINE_ITEM_WORKCollection
        {
            get
            {
                var collection = GetEntities<BASELINE_ITEM_WORK>();
                return collection;
            }
        }

        public IEnumerable<DOCTYPE> DOCTYPECollection
        {
            get
            {
                var collection = GetEntities<DOCTYPE>();
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

        public IEnumerable<SUBJOB> SUBJOBCollection
        {
            get
            {
                var collection = GetEntities<SUBJOB>();
                return collection;
            }
        }

        public CollectionViewModel<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork> PROJECTCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return
                    (CollectionViewModel<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<PROJECT>();
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
        #endregion
    }
}