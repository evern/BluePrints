using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Misc;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.IO;
using BaseModel.ViewModel.Dialogs;
using BluePrints.Common.Resources;
using BaseModel.ViewModel.Services;
using System.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Utils.Filtering;
using System.ComponentModel.DataAnnotations;
using DevExpress.Data.Filtering;
using BaseModel.ViewModel.UndoRedo;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Timers;
using DevExpress.Xpf.Spreadsheet;
using DevExpress.Spreadsheet;
using BluePrints.Common.ViewModel.Misc;
using System.Threading.Tasks;
using BluePrints.P6EntitiesDataModel;
using BluePrints.P6Data;
using DevExpress.Xpf.Editors;
using System.Windows.Threading;
using System.Windows.Media;
using DevExpress.Xpf.Core.Serialization;
using System.Windows.Input;
using BluePrints.Common.ViewModel.Utils;
using DevExpress.Xpf.Editors.Settings;
using System.Windows.Controls;
using System.Windows.Data;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the PROJECTS collection view model.
    /// </summary>
    public class PROJECTForecastViewModelWrapper : PROJECTViewModelWrapper
    {
        /// <summary>
        /// Creates a new instance of PROJECT_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public new static PROJECTForecastViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new PROJECTForecastViewModelWrapper());
        }

        /// <summary>
        /// Initializes a new instance of the PROJECTViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTViewModel type without the POCO proxy factory.
        /// </summary>
        protected PROJECTForecastViewModelWrapper()
        {
            ForceRetrieveRemainingDataPoints = true;
            ShowLoadingScreen = true;

            delayedProjectSaveTimer = new DispatcherTimer();
            delayedProjectSaveTimer.Interval = new TimeSpan(0, 0, 0, 1);

            delayedUpdateFloatingProjectSummaryTimer = new DispatcherTimer();
            delayedUpdateFloatingProjectSummaryTimer.Interval = new TimeSpan(0, 0, 0, 1);

            delayedGridUpdateTimer = new DispatcherTimer();
            delayedGridUpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);

            delayedDateChangeMessageBoxTimer = new DispatcherTimer();
            delayedDateChangeMessageBoxTimer.Interval = new TimeSpan(0, 0, 0, 1);

            delayedDataTableRefreshTimer = new DispatcherTimer();
            delayedDataTableRefreshTimer.Interval = new TimeSpan(0, 0, 0, 1);
            delayedDataTableRefreshTimer.Tick += DelayedDataTableRefreshTimer_Tick;
            projectSavingBackgroundWorker.DoWork += ProjectSavingBackgroundWorker_DoWork;
            projectSavingBackgroundWorker.WorkerSupportsCancellation = true;
        }

        protected override void addEntitiesLoader()
        {
            base.addEntitiesLoader();
            //need to reassign project because forecast dates information on project might changed since navigation is loaded since loadPROJECT comes from navigation
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => setProject(x));
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.FORECASTS, FORECASTProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.VARIATION_REGISTERS, VARIATION_REGISTERProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, COMMODITY_CODEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.FORECAST_POS, FORECAST_POProjectionFunc);
            loaderCollection.AddLoaderDescription(p6UnitOfWorkFactory, x => x.PROJWBS, P6PROJECTProjectionFunc);
            loaderCollection.AddLoaderDescription<Data.PHASE, Data.PHASE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.PHASES);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DOCTYPES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.FORECAST_JOBS, FORECAST_JOBProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.FORECAST_JOB_SETTINGS, FORECAST_JOB_SETTINGProjectionFunc);
            loaderCollection.AddLoaderDescription<JOB_COSTTYPES, JOB_COSTTYPES, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOB_COSTTYPES);
        }

        private void setProject(Data.PROJECT project)
        {
            LoadPROJECT = project;

            DateTime dataDate;
            if (LoadPROJECT.FORECAST_DATA_DATE == null)
            {
                dataDate = DateTime.Now;
                LoadDataDate = null;
            }
            else
            {
                dataDate = (DateTime)LoadPROJECT.FORECAST_DATA_DATE;
                LoadDataDate = dataDate;
            }

            FixedDataDate = dataDate;

            DateTime endDate;
            if (LoadPROJECT.FORECAST_END_DATE == null)
                endDate = DateTime.Now.AddMonths(1);
            else
                endDate = (DateTime)LoadPROJECT.FORECAST_END_DATE;

            FixedEndDate = endDate;

            this.RaisePropertiesChanged();
        }

        private Func<IRepositoryQuery<PROJWBS>, IQueryable<PROJWBS>> P6PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.proj_node_flag == "Y" && x.wbs_short_name.Contains(LoadPROJECT.NUMBER)).OrderBy(proj => proj.wbs_short_name);
        }

        private Func<IRepositoryQuery<FORECAST>, IQueryable<FORECAST>> FORECASTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == LoadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<Data.PROJECT>, IQueryable<Data.PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == LoadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<VARIATION_REGISTER>, IQueryable<VARIATION_REGISTER>> VARIATION_REGISTERProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == LoadPROJECT.GUID);
        }

        protected virtual Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>> COMMODITY_CODEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == null);
        }

        protected virtual Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == LoadPROJECT.GUID && x.COST_TYPE == CostType.Cost);
        }

        protected virtual Func<IRepositoryQuery<FORECAST_JOB>, IQueryable<FORECAST_JOB>> FORECAST_JOBProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == LoadPROJECT.GUID);
        }

        protected virtual Func<IRepositoryQuery<FORECAST_JOB_SETTING>, IQueryable<FORECAST_JOB_SETTING>> FORECAST_JOB_SETTINGProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == LoadPROJECT.GUID);
        }

        protected virtual Func<IRepositoryQuery<FORECAST_PO>, IQueryable<FORECAST_PO>> FORECAST_POProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == LoadPROJECT.GUID);
        }

        public bool IsLoadingForecast { get; set; }
        public bool IsHidden { get; set; }
        public bool IsJobForecast;
        public ForecastSummary ForecastSummary { get; set; }
        public CriteriaOperator FilterCriteria { get; set; }
        public virtual DateTime EndSelectionDate { get; set; }
        public virtual DateTime StartSelectionDate { get; set; }
        public virtual IEnumerable<string> Subjobs { get; set; }
        protected List<ExoTimeAuthorisation> queryJobLines { get; set; }
        protected JOBCOST_HDR masterJob;
        protected JOBCOST_LINES copyLine;
        IP6EntitiesUnitOfWork p6UnitOfWork = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
        IEnumerable<ExoSubJobProjection> queryJobs;
        List<string> hiddenColumnFieldNames = new List<string>();
        protected List<DateTime> alignedDataDateCollection;
        protected virtual IGridControlService DetailGridControlService { get { return this.GetService<IGridControlService>("DetailGridControlService"); } }
        protected virtual IGridControlService ExportGridControlService { get { return this.GetService<IGridControlService>("ExportGridControlService"); } }
        protected virtual ITableViewService ExportTableViewService { get { return this.GetService<ITableViewService>("ExportTableViewService"); } }
        IBluePrintsEntitiesUnitOfWork bluePrintsUnitOfWork;
        DispatcherTimer delayedProjectSaveTimer;
        DispatcherTimer delayedUpdateFloatingProjectSummaryTimer;
        DispatcherTimer delayedGridUpdateTimer;
        DispatcherTimer delayedDataTableRefreshTimer;
        DispatcherTimer delayedDateChangeMessageBoxTimer;
        BackgroundWorker projectSavingBackgroundWorker = new BackgroundWorker();


        protected int spreadSheetPhaseIndex = 0;
        protected int spreadSheetAreaIndex = 1;
        protected int spreadSheetSubAreaIndex = 2;
        protected int spreadSheetSubJobIndex = 3;
        protected int spreadSheetSubJobTitleIndex = 4;
        protected int spreadSheetVariationIndex = 5;
        protected int spreadSheetDisciplineIndex = 6;
        protected int spreadSheetDisciplineNameIndex = 7;
        protected int spreadSheetCommodityIndex = 8;
        protected int spreadSheetCommodityNameIndex = 9;
        protected int spreadSheetCommodityDescriptionIndex = 10;
        protected int spreadSheetCommodityUOMIndex = 11;
        protected int spreadSheetRateIndex = 12;
        protected int spreadSheetBudgetIndex = 13;
        protected int spreadSheetDateStartIndex = 14;

        bool isWeeks;
        public bool IsWeeks
        {
            get => isWeeks;
            set
            {
                if(isWeeks != value)
                {
                    isWeeks = value;
                    ForecastSummary.Reset();
                    EntitiesUndoRedoManager.Clear();
                    mainThreadDispatcher.BeginInvoke(new Action(() => loadDataPointsTable()));
                }
            }
        }

        public Action<DataTable> OnDataTableLoaded { get; set; }
        private void loadDataPointsTable()
        {
            dataPointsTable = null;
            commodityJobs = null;

            updateDataPointsTable();
            OnDataTableLoaded?.Invoke(DataPointsTable);
            this.RaisePropertyChanged(x => x.DataPointsTable);
        }

        public bool FullScreenView = true;
        protected IPrimeroEntitiesUnitOfWork primeroEntitiesUnitOfWork;
        protected override void resolveParameters(object parameter)
        {
            base.resolveParameters(parameter);
            primeroEntitiesUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(LoadPROJECT.OfficeNameForExo == BluePrintsResources.OfficeMontreal).CreateUnitOfWork();
            bluePrintsUnitOfWork = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            ForecastSummary = new ForecastSummary();
            ForceRetrieveAllJobs = true; //force exo burned to retrieve subjobs that aren't defined
            ForceRetrieveAllUnits = false; //force exo burned to retrieve units that are beyond data date
            UseProductivityFactorOnRemaining = false; //calculate remaining costs using productivity factor
            IsLoadingForecast = true;
            LoadingScreenManager.DisableLoadingScreen = false;
            skipBindingSwitch = true;
            hiddenColumnFieldNames.Add(columnEntity);
            hiddenColumnFieldNames.Add(columnCompare);
            SelectedDataRows = new ObservableCollection<DataRowView>();
            StartSelectionDate = DateTime.Now;
            DetailedData = new List<ExoDataPoint>();
            alignedDataDateCollection = new List<DateTime>();
            IsHidden = true;
            delayPostLoadedTimer = true;
            //isExcelExportDataAware = false;
            IsVariationSeparated = true;

            if(FullScreenView)
                GlobalMethods.SetAccordionExpandedState?.Invoke(false);

            this.RaisePropertiesChanged();
        }

        private void loadExoMethodsData()
        {
            IPrimeroEntitiesUnitOfWork threadSafePrimeroEntitiesUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(LoadPROJECT.OfficeNameForExo == BluePrintsResources.OfficeMontreal).CreateUnitOfWork();
            masterJob = ExoQueries.GetProjectSubJob(threadSafePrimeroEntitiesUnitOfWork, LoadPROJECT.NUMBER, LoadPROJECT.NUMBER);
            copyLine = ExoQueries.GetAnyProjectLineByJobNumber(threadSafePrimeroEntitiesUnitOfWork, LoadPROJECT.NUMBER);
        }

        private void loadSummaryStats()
        {
            IPrimeroEntitiesUnitOfWork threadSafePrimeroEntitiesUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(LoadPROJECT.OfficeNameForExo == BluePrintsResources.OfficeMontreal).CreateUnitOfWork();
            List<ExoTimeAuthorisation> jobLines = new List<ExoTimeAuthorisation>(); 
            queryJobs = ExoQueries.GetNativeExoSubJobProjection(threadSafePrimeroEntitiesUnitOfWork, LoadPROJECT, ref jobLines);
            queryJobLines = jobLines;

            dynamic revenueLine = ExoQueries.GetProjectRevenue(threadSafePrimeroEntitiesUnitOfWork, LoadPROJECT.NUMBER);
            if (revenueLine != null)
            {
                if(LoadPROJECT.ORI_REVENUE == null)
                    LoadPROJECT.ORI_REVENUE = Convert.ToDecimal(revenueLine.BUDGETED_REV);

                savePROJECT();
            }
            //dynamic revenueLine = ExoQueries.GetProjectRevenue(primeroEntitiesUnitOfWork, loadPROJECT.NUMBER);
            //if (revenueLine != null)
            ForecastSummary.Original_Revenue = LoadPROJECT.ORI_REVENUE == null ? 0 : (decimal)LoadPROJECT.ORI_REVENUE;
            ForecastSummary.Approved_Var_Revenue = LoadPROJECT.VAR_REVENUE == null ? 0 : (decimal)LoadPROJECT.VAR_REVENUE;
            ForecastSummary.EAC_Revenue = LoadPROJECT.EAC_REVENUE == null ? 0 : (decimal)LoadPROJECT.EAC_REVENUE;

            ForecastSummary.TotalClaims = ExoQueries.GetProjectClaims(threadSafePrimeroEntitiesUnitOfWork, LoadPROJECT.NUMBER);
        }

        protected override List<StatsCalculationType> getForecastTypes()
        {
            List<StatsCalculationType> calcTypes = new List<StatsCalculationType>();
            calcTypes.Add(StatsCalculationType.Forecast);
            calcTypes.Add(StatsCalculationType.Burned);
            calcTypes.Add(StatsCalculationType.Earned);

            return calcTypes;
        }

        public DateTime FixedDataDateMonthEnd => new DateTime(((DateTime)FixedDataDate).Year, ((DateTime)FixedDataDate).Month, 1).AddMonths(1).AddDays(-1);

        public DateTime? LoadDataDate { get; set; }
        public override DateTime? FixedDataDate { get; set; }
        public DateTime FixedEndDate { get; set; }

        public bool CanSaveDateAndRefresh()
        {
            return isCompletelyLoaded;
        }

        public void SaveDateAndRefresh()
        {
            if(FixedDataDate != null)
            {
                ForecastSummary.Reset();
                if(FixedDataDate != LoadDataDate)
                {
                    if (FixedDataDate < LoadDataDate)
                    {
                        IEnumerable<FORECAST> EACForecasts = FORECASTCollectionViewModel.Entities.Where(x => x.FORECAST_TYPE == ForecastDataType.EAC);
                        if(EACForecasts.Count() > 0)
                        {
                            DateTime lastEACDataDate = EACForecasts.Max(x => x.FORECAST_DATE);
                            if(FixedDataDate < lastEACDataDate)
                            {
                                if (!LoginCredentials.hasPermission(PermissionResources.CanRewindDataDate))
                                {
                                    MessageBoxService.ShowMessage("Cannot move data date backwards because EAC is finalised for " + ((DateTime)lastEACDataDate).ToShortDateString(), "Error", MessageButton.OK, MessageIcon.Exclamation);
                                    FixedDataDate = LoadDataDate;
                                    this.RaisePropertyChanged(x => x.FixedDataDate);
                                    return;
                                }
                            }

                        }
                    }
                    //restrict user from moving data date forward if there are forecast but EAC isn't saved
                    else if (FixedDataDate > LoadDataDate)
                    {
                        bool hasEACOnCurrentDataDate = FORECASTCollectionViewModel.Entities.Where(x => x.FORECAST_TYPE == ForecastDataType.EAC && x.FORECAST_DATE == LoadDataDate).Count() > 0;
                        if (LoadDataDate != null && !hasEACOnCurrentDataDate)
                        {
                            if(!LoginCredentials.hasPermission(PermissionResources.CanForwardDataDate))
                            {
                                MessageBoxService.ShowMessage("Cannot move data date forward because EAC isn't saved for " + ((DateTime)LoadDataDate).ToShortDateString(), "Error", MessageButton.OK, MessageIcon.Exclamation);
                                FixedDataDate = LoadDataDate;
                                this.RaisePropertyChanged(x => x.FixedDataDate);
                                return;
                            }
                        }
                    }
                }

                DateTime saveDateTime = (DateTime)FixedDataDate;
                LoadPROJECT.FORECAST_DATA_DATE = new DateTime(((DateTime)saveDateTime).Year, ((DateTime)saveDateTime).Month, 1).AddMonths(1).AddDays(-1);
                LoadPROJECT.FORECAST_END_DATE = FixedEndDate;
                PROJECTCollectionViewModel.Save(LoadPROJECT);
                LoadDataDate = FixedDataDate;
                FullRefresh();
            }
        }

        private void showDateChangeMessage()
        {
            delayedDateChangeMessageBoxTimer.Tick -= DelayedMessageBoxTimer_Tick;
            delayedDateChangeMessageBoxTimer.Tick += DelayedMessageBoxTimer_Tick;
            delayedDateChangeMessageBoxTimer.Start();
        }

        private void DelayedMessageBoxTimer_Tick(object sender, EventArgs e)
        {
            delayedDateChangeMessageBoxTimer.Stop();
            MessageBoxService.ShowMessage("Please close and re-open this view after changing dates, refresh button doesn't produce an accurate result at the moment", "Info", MessageButton.OK, MessageIcon.Information);
        }

        public string P6ForecastProject
        {
            get
            {
                if (LoadPROJECT == null)
                    return string.Empty;

                return LoadPROJECT.P6FORECAST_NAME;
            }
            set
            {
                LoadPROJECT.P6FORECAST_NAME = value;
                PROJECTCollectionViewModel?.Save(LoadPROJECT);
            }
        }

        public DateTime? P6DataDate
        {
            get
            {
                if (LoadPROJECT == null)
                    return null;

                return LoadPROJECT.P6FORECAST_DATADATE;
            }
        }

        public bool CanReloadP6Forecast()
        {
            return isCompletelyLoaded && !IsLoadingForecast;
        }

        public async void ReloadP6Forecast()
        {
            IsLoadingForecast = true;
            this.RaisePropertyChanged(x => x.IsLoadingForecast);
            if (summaryBackgroundWorker != null)
                summaryBackgroundWorker.CancelAsync();
            
            await BluePrintsContextHelper.RefreshDeliverablesRemainingDataPointsByProject(LoadPROJECT.NUMBER, true);
            await BluePrintsContextHelper.RefreshDeliverablesPlannedDataPointsByProject(LoadPROJECT.NUMBER, true);
            FullRefresh();
        }

        protected override bool OnMainViewModelLoaded(IEnumerable<PROJECT_Dashboard> entities)
        {
            BackgroundWorker exoLoadingBackgroundWorker = new BackgroundWorker();
            exoLoadingBackgroundWorker.DoWork += ExoLoadingBackgroundWorker_DoWork; ;
            exoLoadingBackgroundWorker.RunWorkerCompleted += ExoLoadingBackgroundWorker_RunWorkerCompleted;
            exoLoadingBackgroundWorker.WorkerSupportsCancellation = true;
            exoLoadingBackgroundWorker.RunWorkerAsync();
            LoadingScreenManager.CloseLoadingScreen();
            return base.OnMainViewModelLoaded(entities);
        }

        private void ExoLoadingBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<Task> TaskList = new List<Task>();
            Task loadExoTask = new Task(loadExoMethodsData);
            Task loadSummaryTask = new Task(loadSummaryStats);

            TaskList.Add(loadExoTask);
            TaskList.Add(loadSummaryTask);
            loadExoTask.Start();
            loadSummaryTask.Start();

            Task.WaitAll(TaskList.ToArray());
        }

        bool isLoadingExo = true;
        private void ExoLoadingBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            isLoadingExo = false;
            backgroundProcessCompleted();
        }

        protected override void executeFirstLoadedActions()
        {
            //do nothing so base is skipped
        }

        protected override void onSummaryCalculateComplete()
        {
            //indicating that this wrapper is disposed
            if (FORECASTCollectionViewModel == null)
                return;

            if(MainViewModel != null)
            {
                MainViewModel.IsPasteCellLevel = true;
                this.RaisePropertyChanged(x => x.MainViewModel.IsPasteCellLevel);
            }

            FORECASTCollectionViewModel.SetParentViewModel(this);
            VARIATION_REGISTERCollectionViewModel.SetParentViewModel(this);

            IsLoadingForecast = false;
            this.RaisePropertyChanged(x => x.IsLoadingForecast);
            backgroundProcessCompleted();

            base.onSummaryCalculateComplete();
        }

        private void backgroundProcessCompleted()
        {
            if(!isLoadingExo && !IsLoadingForecast)
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => loadDataPointsTable()));
                LoadingScreenManager.DisableLoadingScreen = false;
                if(ShowLoadingScreen)
                {
                    LoadingScreenManager.ShowLoadingScreen(0);
                    LoadingScreenManager.SetMessage("Applying Columns Best Fit...");
                }

                postLoadedDispatcherTimer = new Timer();
                postLoadedDispatcherTimer.Interval = 10;
                postLoadedDispatcherTimer.Elapsed += post_loaded_dispatcher_timer_tick;
                postLoadedDispatcherTimer.Start();
            }
        }

        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            PROJECTCollectionViewModel.AlwaysSkipMessage = true;
            base.OnAfterAssignedCallbackAndRaisePropertyChanged();
            LoadingScreenManager.CloseLoadingScreen();
        }

        public override bool CanFullRefresh()
        {
            return isCompletelyLoaded;
        }

        public override void FullRefresh()
        {
            IsLoadingForecast = true;
            isCompletelyLoaded = false;
            this.RaisePropertyChanged(x => x.isCompletelyLoaded);
            alignedDataDateCollection.Clear();
            DetailedData.Clear();
            EntitiesUndoRedoManager.Clear();
            dataPointsTable = null;
            ForecastSummary.Reset();
            loadSummaryStats();
            base.FullRefresh();
        }

        #region Data Points Table
        protected string columnEntity = "Entity";
        protected string columnCompare = "CompareEntities";
        DataTable exportTable = null;
        public DataTable ExportTable
        {
            get
            {
                if (DataPointsTable == null)
                    return null;

                if(exportTable == null)
                {
                    exportTable = new DataTable();
                }

                return exportTable;
            }
        }

        DataTable dataPointsTable = null;
        List<ForecastJobData> commodityJobs = null;
        public virtual DataTable DataPointsTable
        {
            get
            {
                return dataPointsTable;
            }
        }

        private void updateDataPointsTable()
        {
            dataPointsTable = new DataTable();
            GridControlService.GridControl.BeginDataUpdate();
            //get immutable data
            alignedDataDateCollection = generateDates();
            InitializeColumnSource(ParentViewColumns, ParentSummaries, alignedDataDateCollection, false);
            InitializeColumnSource(ChildViewColumns, ChildSummaries, alignedDataDateCollection, true);

            bool isNewData = false;
            if (commodityJobs == null)
            {
                List<ExoDataPoint> allDataPoints = new List<ExoDataPoint>();
                List<ExoSubJobProjection> unifiedJobList = ForecastHelper.ConstructUnifiedJobList(queryJobLines, COMMODITY_CODECollection, ref allDataPoints, JOB_COSTTYPESCollection, ShowLoadingScreen, AllProjectDashboards);
                DetailedData.AddRange(allDataPoints);
                commodityJobs = ForecastHelper.CreateCommodityProjections(unifiedJobList, queryJobLines, AllProjectDashboards, FORECASTCollectionViewModel.Entities, FORECAST_POCollection, FORECAST_JOBCollection, FORECAST_JOB_SETTINGCollection, COMMODITY_CODECollection, alignedDataDateCollection, (DateTime)FixedDataDate, isWeeks, ShowLoadingScreen);
                isNewData = true;
            }

            if(ShowLoadingScreen)
            {
                LoadingScreenManager.ShowLoadingScreen(commodityJobs.Count);
                LoadingScreenManager.SetMessage("Preparing View...");
            }
            //construct data points table
            dataPointsTable.Columns.Add(columnEntity, typeof(ForecastJobData));
            dataPointsTable.Columns.Add(columnCompare, typeof(DataTable));
            foreach (DateTime alignedDataDate in alignedDataDateCollection)
            {
                string columnFieldName = alignedDataDate.Date.ToString(BluePrintsResources.ColumnDateFormat);
                dataPointsTable.Columns.Add(columnFieldName, typeof(decimal));
            }

            //child data table is used to record original value of actuals + committed + remaining values before it is overridden by forecasts
            foreach (ForecastJobData commodityJob in commodityJobs)
            {
                ForecastHelper.PopulateEAC(commodityJob, FORECASTCollectionViewModel.Entities, (DateTime)FixedDataDate);
                updateAdditionalJobInfo(commodityJob);
                updateDataTable(commodityJob, isNewData);
                LoadingScreenManager.Progress();
            }

            GridControlService.GridControl.EndDataUpdate();
            LoadingScreenManager.CloseLoadingScreen();
            this.RaisePropertyChanged(x => x.ForecastSummary);
            this.RaisePropertyChanged(x => x.ExportTable);
        }

        protected virtual void updateAdditionalJobInfo(ForecastJobData commodityJob)
        {

        }

        private List<DateTime> generateDates()
        {
            IEnumerable<Common.ViewModel.Reporting.DataPoint> remainingDataPoints = AllProjectDashboards.Where(x => x.Stats != null && x.Stats.Remaining != null && x.Stats.Remaining.DataPoints != null).SelectMany(x => x.Stats.Remaining.DataPoints).ToList();
            DateTime endDateToGenerate;

            //because background worker haven't update this value yet, updating it will allow end date to be saved when it's less than remaining end date
            isCompletelyLoaded = true;
            if (remainingDataPoints.Count() > 0)
            {
                endDateToGenerate = remainingDataPoints.Max(x => x.ProgressDate);
                endDateToGenerate = endDateToGenerate.AddMonths(1);
                if (endDateToGenerate > FixedEndDate)
                    FixedEndDate = endDateToGenerate;
                else
                    endDateToGenerate = FixedEndDate;
            }
            else
                endDateToGenerate = FixedEndDate;

            if (IsWeeks)
                return ChronologicalHelpers.GenerateEndDatesCollection((DateTime)FixedDataDate, endDateToGenerate, true);
            else
                return ChronologicalHelpers.GenerateEndDatesCollection((DateTime)FixedDataDate, endDateToGenerate);
        }

        private void updateDataTable(ForecastJobData commodityJob, bool isNew)
        {
            DataRow commodityRow = dataPointsTable.NewRow();
            commodityRow[columnEntity] = commodityJob;
            #region fallback rate search
            //rate already present during update
            if (isNew)
            {
                Data.PHASE ratePHASE = PHASECollection.FirstOrDefault(x => x.INTERNAL_NUM == commodityJob.Projection.SubJob.PhaseCode);
                string disciplineCode = commodityJob.Projection.Discipline.Code.Length > 2 ? commodityJob.Projection.Discipline.Code.Substring(0, 2) : commodityJob.Projection.Discipline.Code;
                //fallback rate cannot be searched by department because department doesn't exists in WBS code structure
                DISCIPLINE rateDISCIPLINE = DISCIPLINECollection.FirstOrDefault(x => x.CODE == disciplineCode);
                if (ratePHASE != null && rateDISCIPLINE != null)
                {
                    COMMODITY_CODE rateCOMMODITY = COMMODITY_CODECollection.FirstOrDefault(x => x.PHASE_TYPE == ratePHASE.PHASE_TYPE && x.GUID_DISCIPLINE == rateDISCIPLINE.GUID && x.CODE == commodityJob.Projection.Commodity.Code);
                    string commodityCode = string.Empty;

                    //when commodity isn't found, get rate by phase and discipline only
                    if (rateCOMMODITY != null)
                        commodityCode = rateCOMMODITY.CODE;

                    commodityJob.FallBackRate = BluePrintsDataUtils.CascadeRateSearch(ratePHASE.GUID, rateDISCIPLINE.GUID, null, commodityCode, RATECollection, CostType.Cost);
                }
            }
            #endregion

            DataTable compareDataTable;
            //DataRow compareActualsRow;
            //DataRow compareMaterialRow;
            DataRow compareP6CostsRemainingRow;
            DataRow compareP6UnitsRemainingRow;
            DataTable compareChildDataTable;
            DataRow compareChildP6CostsRemainingRow;
            DataRow compareChildP6UnitsRemainingRow;

            compareDataTable = dataPointsTable.Clone();
            compareDataTable.TableName = BluePrintsResources.ForecastCompareTableName;
            //compareActualsRow = compareDataTable.NewRow();
            //compareMaterialRow = compareDataTable.NewRow();
            compareP6CostsRemainingRow = compareDataTable.NewRow();
            compareP6UnitsRemainingRow = compareDataTable.NewRow();

            //compareActualsRow[columnEntity] = ViewModelSource.Create(() => new ForecastJobData() { DropDownPhase = "Actuals $", CompareMask = "c0" });
            //compareMaterialRow[columnEntity] = ViewModelSource.Create(() => new ForecastJobData() { DropDownPhase = "Materials $", CompareMask = "c0" });
            compareP6UnitsRemainingRow[columnEntity] = ViewModelSource.Create(() => new ForecastJobData() { DropDownPhase = "P6 Hours", CompareMask = "n2", FallBackRate = commodityJob.FallBackRate, Projection = commodityJob.Projection, DateCosts = commodityJob.DateCosts, IsP6HoursRow = true, P6RemainingUnits = commodityJob.P6RemainingUnits, P6RemainingCosts = commodityJob.P6RemainingCosts });
            compareP6CostsRemainingRow[columnEntity] = ViewModelSource.Create(() => new ForecastJobData() { DropDownPhase = "P6 $", CompareMask = "c0" });

            List<string> uniquePOStockCodes = new List<string>();
            List<string> uniqueIndirectStockCodes = new List<string>();
            List<string> uniqueMaterialStockCodes = new List<string>();
            List<string> uniqueActualStockCodes = new List<string>();

            if (commodityJob.DateCosts.Count > 0)
            {
                ForecastDateCost commodityDateCost = commodityJob.DateCosts.First();
                uniquePOStockCodes = commodityDateCost.RelevantForecastPOs.Where(x => x.ViewStockCode != null).Select(x => x.ViewStockCode).Distinct().ToList();
                uniqueIndirectStockCodes = commodityDateCost.RelevantIndirectCosts.Where(x => x.ViewStockCode != null).Select(x => x.ViewStockCode).Distinct().ToList();
                uniqueMaterialStockCodes = commodityDateCost.RelevantMaterialDataPoints.Where(x => x.StockCode != null).Select(x => x.StockCode).Distinct().ToList();
                uniqueActualStockCodes = commodityDateCost.RelevantActualDataPoints.Where(x => x.StockCode != null).Select(x => x.StockCode).Distinct().ToList();
            }

            compareChildDataTable = dataPointsTable.Clone();
            compareChildDataTable.TableName = BluePrintsResources.ForecastCompareChildTableName;
            compareChildP6CostsRemainingRow = compareChildDataTable.NewRow();
            compareChildP6UnitsRemainingRow = compareChildDataTable.NewRow();

            compareChildDataTable.Rows.Add(compareChildP6UnitsRemainingRow);
            compareChildDataTable.Rows.Add(compareChildP6CostsRemainingRow);

            compareP6UnitsRemainingRow[columnCompare] = compareChildDataTable;
            //compareDataTable.Rows.Add(compareActualsRow);
            //compareDataTable.Rows.Add(compareMaterialRow);
            compareDataTable.Rows.Add(compareP6UnitsRemainingRow);
            compareDataTable.Rows.Add(compareP6CostsRemainingRow);

            Dictionary<string, DataRow> poForecastRows = new Dictionary<string, DataRow>();
            Dictionary<string, DataRow> indirectForecastRows = new Dictionary<string, DataRow>();
            Dictionary<string, DataRow> materialForecastRows = new Dictionary<string, DataRow>();
            Dictionary<string, DataRow> actualForecastRows = new Dictionary<string, DataRow>();
            foreach (string uniquePOStockCode in uniquePOStockCodes)
            {
                DataRow comparePOForecastRow = compareDataTable.NewRow();
                comparePOForecastRow[columnEntity] = ViewModelSource.Create(() => new ForecastJobData() { DropDownPhase = BluePrintsResources.ForecastCompare_PORowPhase + " [" + uniquePOStockCode + "] $", CompareMask = "c0" });
                poForecastRows.Add(uniquePOStockCode, comparePOForecastRow);
                compareDataTable.Rows.Add(comparePOForecastRow);
            }

            foreach (string uniqueIndirectStockCode in uniqueIndirectStockCodes)
            {
                DataRow compareIndirectRemainingRow = compareDataTable.NewRow();
                compareIndirectRemainingRow[columnEntity] = ViewModelSource.Create(() => new ForecastJobData() { DropDownPhase = BluePrintsResources.ForecastCompare_IndirectRowPhase + " [" + uniqueIndirectStockCode + "] $", CompareMask = "c0" });
                indirectForecastRows.Add(uniqueIndirectStockCode, compareIndirectRemainingRow);
                compareDataTable.Rows.Add(compareIndirectRemainingRow);
            }

            foreach (string uniqueMaterialStockCode in uniqueMaterialStockCodes)
            {
                DataRow compareMaterialRemainingRow = compareDataTable.NewRow();
                compareMaterialRemainingRow[columnEntity] = ViewModelSource.Create(() => new ForecastJobData() { DropDownPhase = BluePrintsResources.ForecastCompare_MaterialRowPhase + " [" + uniqueMaterialStockCode + "] $", CompareMask = "c0" });
                materialForecastRows.Add(uniqueMaterialStockCode, compareMaterialRemainingRow);
                compareDataTable.Rows.Add(compareMaterialRemainingRow);
            }

            foreach (string uniqueActualStockCode in uniqueActualStockCodes)
            {
                DataRow compareActualRemainingRow = compareDataTable.NewRow();
                compareActualRemainingRow[columnEntity] = ViewModelSource.Create(() => new ForecastJobData() { DropDownPhase = BluePrintsResources.ForecastCompare_ActualRowPhase + " [" + uniqueActualStockCode + "] $", CompareMask = "c0" });
                actualForecastRows.Add(uniqueActualStockCode, compareActualRemainingRow);
                compareDataTable.Rows.Add(compareActualRemainingRow);
            }

            commodityRow[columnCompare] = compareDataTable;
            dataPointsTable.Rows.Add(commodityRow);

            if (!isNew)
                ForecastHelper.PopulateProjection(commodityJob, AllProjectDashboards, FORECAST_POCollection, FORECAST_JOBCollection, FORECAST_JOB_SETTINGCollection, alignedDataDateCollection, IsWeeks, false);

            ExoSubJobProjection projection = commodityJob.Projection;
            IEnumerable<FORECAST> FORECASTCollection = FORECASTCollectionViewModel.Entities;
            List<FORECAST> relevantFORECASTS = FORECASTCollection.Where(x => x.SUBJOB_CODE == projection.SubJob.Code && x.DISCIPLINE_CODE == projection.Discipline.Code && x.COMMODITY_CODE == projection.Commodity.Code && x.VARIATION_CODE == projection.Variation_Code).ToList();

            establishCurrentProductivity(commodityJob);
            decimal P6TotalCurrentRemainingUnits = 0;
            foreach (ForecastDateCost dateCost in commodityJob.DateCosts)
            {
                //dynamically generated rows
                foreach(string uniquePOStockCode in uniquePOStockCodes)
                {
                    DataRow poStockCodeDataRow = poForecastRows.First(x => x.Key == uniquePOStockCode).Value;
                    poStockCodeDataRow[dateCost.Date.ToString(BluePrintsResources.ColumnDateFormat)] = dateCost.CurrentPeriodForecastPOs.Where(x => x.ViewStockCode == uniquePOStockCode).Sum(x => (decimal)x.FORECAST_VALUE);
                }

                foreach(string uniqueIndirectStockCode in uniqueIndirectStockCodes)
                {
                    DataRow indirectStockCodeDataRow = indirectForecastRows.First(x => x.Key == uniqueIndirectStockCode).Value;
                    indirectStockCodeDataRow[dateCost.Date.ToString(BluePrintsResources.ColumnDateFormat)] = dateCost.CurrentPeriodIndirectCosts.Where(x => x.ViewStockCode == uniqueIndirectStockCode).Sum(x => x.ForecastRemainingCosts);
                }

                foreach (string uniqueMaterialStockCode in uniqueMaterialStockCodes)
                {
                    DataRow materialStockCodeDataRow = materialForecastRows.First(x => x.Key == uniqueMaterialStockCode).Value;
                    materialStockCodeDataRow[dateCost.Date.ToString(BluePrintsResources.ColumnDateFormat)] = dateCost.CurrentPeriodMaterialDataPoints.Where(x => x.StockCode == uniqueMaterialStockCode).Sum(x => x.Costs);
                }

                foreach (string uniqueActualStockCode in uniqueActualStockCodes)
                {
                    DataRow actualStockCodeDataRow = actualForecastRows.First(x => x.Key == uniqueActualStockCode).Value;
                    actualStockCodeDataRow[dateCost.Date.ToString(BluePrintsResources.ColumnDateFormat)] = dateCost.CurrentPeriodActualDataPoints.Where(x => x.StockCode == uniqueActualStockCode).Sum(x => x.Costs);
                }

                //static rows
                compareChildP6CostsRemainingRow[dateCost.Date.ToString(BluePrintsResources.ColumnDateFormat)] = dateCost.P6Costs;
                compareChildP6UnitsRemainingRow[dateCost.Date.ToString(BluePrintsResources.ColumnDateFormat)] = dateCost.P6Hours;

                List<FORECAST> forecastOverrides = relevantFORECASTS.Where(x => x.FORECAST_UNITS != null && x.FORECAST_DATE >= dateCost.FloorDate && x.FORECAST_DATE <= dateCost.CeilingDate).ToList();
                List<FORECAST> forecastCostsOverrides = forecastOverrides.Where(x => x.FORECAST_TYPE == ForecastDataType.Cost).ToList();
                List<FORECAST> forecastUnitsOverrides = forecastOverrides.Where(x => x.FORECAST_TYPE == ForecastDataType.P6).ToList();
                List<FORECAST> forecastJobHourOverrides = forecastOverrides.Where(x => x.FORECAST_TYPE == ForecastDataType.Hour).ToList();

                //skip when date is actual date
                if (forecastUnitsOverrides.Count > 0 && dateCost != commodityJob.DateCosts.First())
                {
                    decimal p6OverrideUnits = forecastUnitsOverrides.Sum(x => (decimal)x.FORECAST_UNITS);

                    compareP6UnitsRemainingRow[dateCost.Date.ToString(BluePrintsResources.ColumnDateFormat)] = p6OverrideUnits;
                    compareP6CostsRemainingRow[dateCost.Date.ToString(BluePrintsResources.ColumnDateFormat)] = p6OverrideUnits * commodityJob.P6NominalRate;
                    P6TotalCurrentRemainingUnits += p6OverrideUnits;
                }
                else
                {
                    compareP6UnitsRemainingRow[dateCost.Date.ToString(BluePrintsResources.ColumnDateFormat)] = dateCost.P6Hours;
                    compareP6CostsRemainingRow[dateCost.Date.ToString(BluePrintsResources.ColumnDateFormat)] = dateCost.P6Costs;
                    P6TotalCurrentRemainingUnits += dateCost.P6Hours;
                }

                if (forecastCostsOverrides.Count > 0 && dateCost != commodityJob.DateCosts.First())
                {
                    decimal overrideCosts = forecastCostsOverrides.Sum(x => (decimal)x.FORECAST_UNITS);
                    commodityRow[dateCost.Date.ToString(BluePrintsResources.ColumnDateFormat)] = overrideCosts;
                }
                else
                {
                    commodityRow[dateCost.Date.ToString(BluePrintsResources.ColumnDateFormat)] = dateCost.TotalCosts;
                }
            }

            commodityJob.P6RemainingUnitsOverride = P6TotalCurrentRemainingUnits;
            updateViewForecastsOnDatesFromDb(commodityRow);
            updateTotalUncommittedOnJob(commodityRow);

            //calculate project summary, needs to be done after uncommitted is calculated
            ForecastSummary.Budget_Cost += commodityJob.Budget;
            ForecastSummary.Current_Cost += commodityJob.ActualCosts;
            ForecastSummary.Commitments += commodityJob.Outstanding;
            ForecastSummary.Uncommitted_Forecast += commodityJob.Uncommitted;
            ForecastSummary.OriginalEstimateAtCompletion += commodityJob.OriginalEstimateAtCompletion;
            ForecastSummary.EstimateAtCompletion += commodityJob.EstimateAtCompletion;
            ForecastSummary.CurrentEstimateAtCompletion += commodityJob.CurrentEstimateAtCompletion;
        }

        private void InitializeColumnSource(ObservableCollection<ColumnDescriptor> columns, ObservableCollection<SummaryDescriptor> summaries, List<DateTime> alignedDates, bool isChild)
        {
            columns.Clear();
            summaries.Clear();

            if (!isChild)
            {
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.Projection.SubJob.PhaseCode", ReadOnly = true, Header = "Phase", Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Default });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.Projection.SubJob.Code", ReadOnly = true, Header = "Subjob", Fixed = FixedStyle.Left, Width = 95, Settings = SettingsType.JobError });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.Projection.SubJob.Title", ReadOnly = true, Header = "Subjob Title", Visible = false, Fixed = FixedStyle.Left, Width = 95, Settings = SettingsType.Default });
                
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.Projection.SubJob.Code", DisplayFormat = "Total {0} Records", Type = SummaryItemType.Count });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.Projection.Discipline.Code", ReadOnly = true, Header = "Discipline", Fixed = FixedStyle.Left, Width = 38, Settings = SettingsType.Default });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.Projection.Commodity.Code", ReadOnly = true, Header = "Commodity", Fixed = FixedStyle.Left, Width = 35, Settings = SettingsType.CommodityCode });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.Projection.Commodity.Name", ReadOnly = true, Header = "Commodity Name", Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Default });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.Projection.Variation_Code", ReadOnly = true, Header = "Variation", Fixed = FixedStyle.Left, Width = 60, Settings = SettingsType.Default });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.Budget", ReadOnly = false, Header = "Budget (A)", Increment = 1, Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Budget, HeaderToolTip = "Original budgeted cost at contract award" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.Budget", DisplayFormat = "c0", Type = SummaryItemType.Sum });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.DeliverableUnits", ReadOnly = true, Visible = false, Header = "Total Units", Mask = "###,##0h", Increment = 1, Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number, HeaderToolTip = "Total hours including variation, available for design only" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.DeliverableUnits", DisplayFormat = "n0", Type = SummaryItemType.Sum });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.ActualUnits", ReadOnly = true, Header = "Actual Units", Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Number, Mask = "n0", HeaderToolTip = "Actual units to date" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.ActualUnits", DisplayFormat = "n0", Type = SummaryItemType.Sum });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.P6RemainingUnitsOverride", ReadOnly = true, Header = "P6 Remaining Units", Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Number, Mask = "n0", HeaderToolTip = "Remaining units from refreshing P6" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.P6RemainingUnitsOverride", DisplayFormat = "n0", Type = SummaryItemType.Sum });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.Productivity", ReadOnly = false, Visible = false, Header = "PF", Increment = 0.1m, Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Number, Mask = "n2", HeaderToolTip = "Productivity Factor, 0 means there aren't any units from P6" });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.CurrentProductivity", ReadOnly = true, Visible = false, Header = "Current PF", Increment = 0.1m, Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Number, Mask = "n2", HeaderToolTip = "Current productivity factor, 0 means there aren't any earned or actuals units" });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.IsProductivityFloating", Visible = false, ReadOnly = true, Header = "Floating PF", Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Default, HeaderToolTip = "Productivity on job with floating productivity can be updated to match current productivity" });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.ActualCosts", ReadOnly = true, Header = "Actual Costs (B)", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number, Mask = "c0", HeaderToolTip = "Costs burned to Date" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.ActualCosts", DisplayFormat = "c0", Type = SummaryItemType.Sum });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.PctComplete", ReadOnly = true, Visible = false, Header = "% Complete", Fixed = FixedStyle.Left, Width = 40, Settings = SettingsType.Number, Mask = "p0", HeaderToolTip = "Procurement: Actuals / EAC, Others: (Budgeted Units - Remaining Units)/ Budgeted Units" });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.Outstanding", ReadOnly = true, Header = "Outstanding (C)", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number, Mask = "c0", HeaderToolTip = "Open Commitment, amount left on purchase order (outstanding PO) or amount left on P6 forecasts" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.Outstanding", DisplayFormat = "c0", Type = SummaryItemType.Sum });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.OriginalUncommitted", Visible = false, ReadOnly = true, Header = "Non-PF Uncommitted", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number, Mask = "c0", HeaderToolTip = "(Sum of uncommitted costs - (costs from the forecasting months)) + P6 Costs" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.OriginalUncommitted", DisplayFormat = "c0", Type = SummaryItemType.Sum });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.Uncommitted", ReadOnly = true, Header = "Uncommitted (D)", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number, Mask = "c0", HeaderToolTip = "(Sum of uncommitted costs - (costs from the forecasting months)) + (P6 Costs with or without PF)" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.Uncommitted", DisplayFormat = "c0", Type = SummaryItemType.Sum });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.CurrentUncommitted", Visible = false, ReadOnly = true, Header = "PF Uncommitted", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number, Mask = "c0", HeaderToolTip = "(Sum of uncommitted costs - (costs from the forecasting months)) + (P6 Costs with PF)" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.CurrentUncommitted", DisplayFormat = "c0", Type = SummaryItemType.Sum });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.EstimateToComplete", ReadOnly = true, Visible = false, Header = "ETC", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number, Mask = "c0", HeaderToolTip = "Estimate to Complete (or costs to complete) - equal to forecasted costs, plus open commitments (outstanding purchase order)" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.EstimateToComplete", DisplayFormat = "c0", Type = SummaryItemType.Sum });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.PreviousEAC", ReadOnly = true, Header = "Prev. EAC", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number, Mask = "c0", HeaderToolTip = "Previous estimate at completion" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.PreviousEAC", DisplayFormat = "c0", Type = SummaryItemType.Sum });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.EstimateAtCompletion", ReadOnly = true, Header = "EAC (E) (B + C + D)", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number, Mask = "c0", HeaderToolTip = "Estimate at complete, forecasted costs + open commitments + accruals" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.EstimateAtCompletion", DisplayFormat = "c0", Type = SummaryItemType.Sum });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.Variance", ReadOnly = true, Header = "Variance (A - E)", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number, Mask = "c0", HeaderToolTip = "Variance to budget" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.Variance", DisplayFormat = "c0", Type = SummaryItemType.Sum });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.PeriodMovement", Header = "Period Move", Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number, Mask = "c0", HeaderToolTip = "Difference from previous EAC" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.PeriodMovement", DisplayFormat = "c0", Type = SummaryItemType.Sum });
            }
            else
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.DropDownPhase", Header = "", Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Default, HeaderToolTip = "Source of forecasted costs/hours type" });

            foreach (DateTime alignedDate in alignedDates.OrderBy(x => x))
            {
                string columnFieldName = alignedDate.Date.ToString(BluePrintsResources.ColumnDateFormat);

                if (alignedDate <= FixedDataDateMonthEnd)
                {
                    //do not show actuals
                    //columns.Add(new ColumnDescriptor() { FieldName = columnFieldName, ReadOnly = true, Header = columnFieldName, Fixed = FixedStyle.None, Width = 60, Settings = SettingsType.ForecastPast });
                }
                else
                {
                    if (isChild)
                        columns.Add(new ColumnDescriptor() { FieldName = columnFieldName, ReadOnly = false, Header = columnFieldName, Fixed = FixedStyle.None, Width = 60, Settings = SettingsType.ForecastChild });
                    else
                        columns.Add(new ColumnDescriptor() { FieldName = columnFieldName, ReadOnly = false, Header = columnFieldName, Fixed = FixedStyle.None, Width = 60, Settings = SettingsType.ForecastFuture });
                }

                if(!isChild)
                    summaries.Add(new SummaryDescriptor() { FieldName = columnFieldName, DisplayFormat = "c0", Type = SummaryItemType.Sum });
            }
        }

        private void DelayedDataTableRefreshTimer_Tick(object sender, EventArgs e)
        {
            delayedDataTableRefreshTimer.Stop();
            GridControlService.GridControl.RefreshData();
            for (int i = 0; i < GridControlService.GridControl.VisibleRowCount; i++)
            {
                var detail = GridControlService.GridControl.GetDetail(GridControlService.GridControl.GetRowHandleByVisibleIndex(i));
                if (detail != null)
                    detail.RefreshData();
            }
        }

        /// <summary>
        /// For the purpose of presentation, variation code must always be empty
        /// But when budget is edited, findExistingOrAddNewLine will handle the difference between null and string.empty values
        /// </summary>
        private string normalizeVariationCode(string subjobVariationCode)
        {
            if (subjobVariationCode == null)
                return string.Empty;

            return subjobVariationCode;
        }

        private void setDateFieldsEmpty(DataRow dataRow, bool test)
        {
            for (int i = 0; i < dataRow.ItemArray.Count(); i++)
            {
                string columnName = dataPointsTable.Columns[i].ColumnName;
                DateTime parseDateTime;
                if (DateTime.TryParse(columnName, out parseDateTime))
                    dataRow[columnName] = test ? 1000m : 0.00m;
            }
        }

        public IEnumerable<ExoDataPoint> ActualsDetail => DetailedData;

        public List<ExoDataPoint> DetailedData { get; set; }

        private void resetViewRemainingOnJob(DataRow updateRow, string fieldName, bool addUndo)
        {
            if (updateRow[columnCompare] == DBNull.Value)
                return;

            ForecastJobData job = ((ForecastJobData)updateRow[columnEntity]);
            ExoSubJobProjection entity = job.Projection;
            DataTable compareDataTable = (DataTable)updateRow[columnCompare];

            decimal oldValue = 0.00m;
            decimal newValue = 0.00m;
            if(compareDataTable.Columns.Contains(fieldName))
            {
                decimal resetValue = getMasterRowResetValue(compareDataTable, fieldName);
                resetChildRow(compareDataTable, fieldName, addUndo);
                oldValue = (decimal)updateRow[fieldName];
                newValue = resetValue;

                //do it twice so that child row value can be updated (hack)
                updateRow[fieldName] = newValue;
                updateRow[fieldName] = newValue;
                EntitiesUndoRedoManager.AddUndo(updateRow, fieldName, oldValue, newValue, EntityMessageType.Changed);
                updateTotalUncommittedOnJob(updateRow, true);
            }
        }

        private void findExistingOrAddNewForecastJobSetting(DataRow updateRow, bool isFloatingProductivity)
        {
            ForecastJobData job = ((ForecastJobData)updateRow[columnEntity]);
            ExoSubJobProjection projection = job.Projection;
            FORECAST_JOB_SETTING relevantFORECAST_JOB_SETTING = FORECAST_JOB_SETTINGCollection.FirstOrDefault(x => x.SUBJOB_CODE == projection.SubJob.Code && x.DISCIPLINE_CODE == projection.Discipline.Code && x.COMMODITY_CODE == projection.Commodity.Code && x.VARIATION_CODE == projection.Variation_Code);
            if (relevantFORECAST_JOB_SETTING == null)
            {
                FORECAST_JOB_SETTING newFORECAST_JOB_SETTING = new FORECAST_JOB_SETTING();
                newFORECAST_JOB_SETTING.GUID_PROJECT = LoadPROJECT.GUID;
                newFORECAST_JOB_SETTING.SUBJOB_CODE = projection.SubJob.Code;
                newFORECAST_JOB_SETTING.DISCIPLINE_CODE = projection.Discipline.Code;
                newFORECAST_JOB_SETTING.COMMODITY_CODE = projection.Commodity.Code;

                if (projection.Variation_Code != null && projection.Variation_Code != string.Empty)
                    newFORECAST_JOB_SETTING.VARIATION_CODE = projection.Variation_Code;
                else
                    newFORECAST_JOB_SETTING.VARIATION_CODE = string.Empty;

                relevantFORECAST_JOB_SETTING = newFORECAST_JOB_SETTING;
            }

            relevantFORECAST_JOB_SETTING.IS_FLOATING_PRODUCTIVITY = isFloatingProductivity;
            job.IsProductivityFloating = isFloatingProductivity;
            FORECAST_JOB_SETTINGCollectionViewModel.Save(relevantFORECAST_JOB_SETTING);
        }

        private void establishCurrentProductivity(ForecastJobData job)
        {
            if (job.EarnedUnits != null && job.ActualUnits != 0)
            {
                if (job.EarnedUnits > 0)
                    job.CurrentProductivity = job.ActualUnits / (decimal)job.EarnedUnits;
                else
                    job.CurrentProductivity = 0.00m;
            }
            else
                job.CurrentProductivity = 0.00m;
        }

        /// <summary>
        /// Updates the view with forecast values from db for a single row
        /// </summary>
        private void updateViewForecastsOnDatesFromDb(DataRow dataRow, bool searchParentRow = false)
        {
            ForecastJobData job = (ForecastJobData)dataRow[columnEntity];
            ExoSubJobProjection projection = job.Projection;
            //need to map back into main row because datarow could be coming from p6 hours edit
            DataRow parentRow = searchParentRow ? findRow(projection, true) : dataRow;
            job = (ForecastJobData)parentRow[columnEntity];
            DataTable compareDataTable = (DataTable)parentRow[columnCompare];
            DataRow p6CostRow = compareDataTable.Rows[Convert.ToInt32(BluePrintsResources.ForecastCompare_P6CostRow)];
            DataRow p6HoursRow = compareDataTable.Rows[Convert.ToInt32(BluePrintsResources.ForecastCompare_P6HourRow)];

            DataTable childCompareDataTable = (DataTable)p6HoursRow[columnCompare];
            DataRow childCompareP6CostsRow = childCompareDataTable.Rows[Convert.ToInt32(BluePrintsResources.ForecastCompareChild_P6CostRow)];

            IEnumerable<FORECAST> currentRowFORECASTS = FORECASTCollectionViewModel.Entities.Where(x => x.SUBJOB_CODE == projection.SubJob.Code && x.DISCIPLINE_CODE == projection.Discipline.Code && x.COMMODITY_CODE == projection.Commodity.Code && x.VARIATION_CODE == projection.Variation_Code);
            
            decimal P6CurrentRemainingUnits = 0;
            foreach (ForecastDateCost dateCost in job.DateCosts)
            {
                DateTime? alignedDataDate = alignedDataDateCollection.OrderBy(x => x).FirstOrDefault(x => x.Date >= dateCost.Date);
                if (alignedDataDate != null)
                {
                    string alignedDateField = ((DateTime)alignedDataDate).ToString(BluePrintsResources.ColumnDateFormat);
                    //put forecast history only on compare datatable
                    if (alignedDataDate > FixedDataDateMonthEnd)
                    {
                        if (dataPointsTable.Columns.Contains(alignedDateField))
                        {
                            IEnumerable<FORECAST> currentRowDateFORECAST = currentRowFORECASTS.Where(x => x.FORECAST_UNITS != null && x.FORECAST_TYPE == ForecastDataType.Cost && x.FORECAST_DATE >= dateCost.FloorDate && x.FORECAST_DATE <= dateCost.CeilingDate);
                            IEnumerable<FORECAST> currentRowP6OverrideFORECAST = currentRowFORECASTS.Where(x => x.FORECAST_UNITS != null && x.FORECAST_TYPE == ForecastDataType.P6 && x.FORECAST_DATE >= dateCost.FloorDate && x.FORECAST_DATE <= dateCost.CeilingDate);

                            decimal currentP6Units = (decimal)p6HoursRow[alignedDateField];
                            P6CurrentRemainingUnits += currentP6Units;
                            decimal p6RemainingCostsOnDataDate = 0;
                            if (currentRowDateFORECAST.Count() > 0)
                            {
                                p6RemainingCostsOnDataDate = currentRowDateFORECAST.Sum(x => (decimal)x.FORECAST_UNITS);
                            }
                            else
                            {
                                p6RemainingCostsOnDataDate = getMasterRowResetValue(compareDataTable, alignedDateField);
                            }

                            parentRow[alignedDateField] = p6RemainingCostsOnDataDate;

                            if (currentRowP6OverrideFORECAST.Count() > 0)
                                p6CostRow[alignedDateField] = p6RemainingCostsOnDataDate;
                        }
                    }
                }
            }

            job.P6RemainingUnitsOverride = P6CurrentRemainingUnits;

            if (job.P6RemainingUnitsOverride != null && job.P6RemainingUnitsOverride != 0 && job.P6RemainingUnits != 0)
                job.Productivity = job.P6RemainingUnits / (decimal)job.P6RemainingUnitsOverride;
            else
                job.Productivity = 0.00m;
        }
        #endregion

        #region View Events
        /// <summary>
        /// Because grid alternate between showing editor and focused row, use mousedown to invoke set filter
        /// </summary>
        public void MouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                TableView tableView = e.Source as TableView;
                if (tableView == null)
                    return;

                TableViewHitInfo hi = ((TableView)e.Source).CalcHitInfo(e.OriginalSource as DependencyObject);
                RowData clickRowData = tableView.FocusedRowData;
                //if(clickRowData.Row == null)
                //{
                //    GridControl masterGrid = tableView.Grid;
                //    var selected_cells = Enumerable.Range(0, masterGrid.VisibleRowCount)
                //    .Select(x => (GridControl)masterGrid.GetDetail(x))
                //    .Where(x => x != null).
                //    Select(x => ((TableView)(x).View).FocusedRowData).ToList();

                //    clickRowData = selected_cells.FirstOrDefault();
                //}

                if(clickRowData != null)
                    setFilter((DataRowView)clickRowData.Row, hi.Column);
            }
            catch(Exception ex)
            {
                string s = ex.ToString();
            }
        }

        /// <summary>
        /// Because grid alternate between showing editor and focused row, use showing editor to invoke set filter
        /// </summary>
        public void ShowingEditor(DevExpress.Xpf.Grid.ShowingEditorEventArgs e)
        {
            setFilter((DataRowView)e.Row, e.Column);
        }

        public bool IsPOColumnsVisible { get; set; }
        private void setFilter(DataRowView dataRowView, GridColumn gridColumn)
        {
            if (gridColumn == null || dataRowView == null)
                return;

            //if (gridColumn.ReadOnly)
            //{
                DateTime parseEndDate;
                if (DateTime.TryParse(gridColumn.ActualColumnChooserHeaderCaption.ToString(), out parseEndDate))
                {
                    ExoSubJobProjection entity = ((ForecastJobData)dataRowView[columnEntity]).Projection;
                    parseEndDate = parseEndDate.AddDays(1).AddSeconds(-1);
                    EndSelectionDate = parseEndDate;
                    StartSelectionDate = new DateTime(EndSelectionDate.Year, EndSelectionDate.Month, 1);
                    if(parseEndDate.Date == alignedDataDateCollection.First().Date)
                    {
                        if(entity.Commodity.Code != string.Empty)
                            FilterCriteria = CriteriaOperator.Parse("[Subjob_Name] = '" + entity.SubJob.Code + "' And [Discipline_Code] = '" + entity.Discipline.Code + "' And [Variation_Code] = '" + entity.Variation_Code + "' And [Commodity_Code] = '" + entity.Commodity.Code + "' And [IsPO] = 'False'" + " And [ActualDate] <= #" + EndSelectionDate.Year + "-" + EndSelectionDate.Month + "-" + EndSelectionDate.Day + "#");
                        else
                            FilterCriteria = CriteriaOperator.Parse("[Subjob_Name] = '" + entity.SubJob.Code + "' And [Discipline_Code] = '" + entity.Discipline.Code + "' And [Variation_Code] = '" + entity.Variation_Code + "' And [IsPO] = 'False'" + " And [ActualDate] <= #" + EndSelectionDate.Year + "-" + EndSelectionDate.Month + "-" + EndSelectionDate.Day + "#");
                    }
                    else
                    {
                        if(entity.Commodity.Code != string.Empty)
                            FilterCriteria = CriteriaOperator.Parse("[Subjob_Name] = '" + entity.SubJob.Code + "' And [Discipline_Code] = '" + entity.Discipline.Code + "' And [Variation_Code] = '" + entity.Variation_Code + "' And [Commodity_Code] = '" + entity.Commodity.Code + "' And [IsPO] = 'False'" + " And [ActualDate] >= #" + StartSelectionDate.Year + "-" + StartSelectionDate.Month + "-" + StartSelectionDate.Day + "# And [ActualDate] <= #" + EndSelectionDate.Year + "-" + EndSelectionDate.Month + "-" + EndSelectionDate.Day + "#");
                        else
                            FilterCriteria = CriteriaOperator.Parse("[Subjob_Name] = '" + entity.SubJob.Code + "' And [Discipline_Code] = '" + entity.Discipline.Code + "' And [Variation_Code] = '" + entity.Variation_Code + "' And [IsPO] = 'False'" + " And [ActualDate] >= #" + StartSelectionDate.Year + "-" + StartSelectionDate.Month + "-" + StartSelectionDate.Day + "# And [ActualDate] <= #" + EndSelectionDate.Year + "-" + EndSelectionDate.Month + "-" + EndSelectionDate.Day + "#");
                    }

                    IsHidden = false;
                    IsPOColumnsVisible = false;
                    this.RaisePropertyChanged(x => x.FilterCriteria);
                    this.RaisePropertyChanged(x => x.IsPOColumnsVisible);
                }
                else if (gridColumn.FieldName.ToUpper().Contains("ACTUAL"))
                {
                    ExoSubJobProjection entity = ((ForecastJobData)dataRowView[columnEntity]).Projection;
                    if(entity.Commodity.Code != string.Empty)
                        FilterCriteria = CriteriaOperator.Parse("[Subjob_Name] = '" + entity.SubJob.Code + "' And [Discipline_Code] = '" + entity.Discipline.Code + "' And [Variation_Code] = '" + entity.Variation_Code + "' And [Commodity_Code] = '" + entity.Commodity.Code + "' And [IsPO] = 'False'");
                    else
                        FilterCriteria = CriteriaOperator.Parse("[Subjob_Name] = '" + entity.SubJob.Code + "' And [Discipline_Code] = '" + entity.Discipline.Code + "' And [Variation_Code] = '" + entity.Variation_Code + "' And [IsPO] = 'False'");

                    IsHidden = false;
                    IsPOColumnsVisible = false;
                    this.RaisePropertyChanged(x => x.FilterCriteria);
                    this.RaisePropertyChanged(x => x.IsPOColumnsVisible);
                }
                else if (gridColumn.FieldName.ToUpper().Contains("INVOICED"))
                {
                    ExoSubJobProjection entity = ((ForecastJobData)dataRowView[columnEntity]).Projection;
                    if (entity.Commodity.Code != string.Empty)
                        FilterCriteria = CriteriaOperator.Parse("[Subjob_Name] = '" + entity.SubJob.Code + "' And [Discipline_Code] = '" + entity.Discipline.Code + "' And [Variation_Code] = '" + entity.Variation_Code + "' And [Commodity_Code] = '" + entity.Commodity.Code + "' And [IsPO] = 'False' AND [InvoiceAmount] > 0.0m");
                    else
                        FilterCriteria = CriteriaOperator.Parse("[Subjob_Name] = '" + entity.SubJob.Code + "' And [Discipline_Code] = '" + entity.Discipline.Code + "' And [Variation_Code] = '" + entity.Variation_Code + "' And [IsPO] = 'False' AND [InvoiceAmount] > 0.0m");

                    IsHidden = false;
                    IsPOColumnsVisible = false;
                    this.RaisePropertyChanged(x => x.FilterCriteria);
                    this.RaisePropertyChanged(x => x.IsPOColumnsVisible);
                }
                else if(gridColumn.FieldName.ToUpper().Contains("OUTSTANDING"))
                {
                    ExoSubJobProjection entity = ((ForecastJobData)dataRowView[columnEntity]).Projection;
                    if (entity.Commodity.Code != string.Empty)
                        FilterCriteria = CriteriaOperator.Parse("[Subjob_Name] = '" + entity.SubJob.Code + "' And [Discipline_Code] = '" + entity.Discipline.Code + "' And [Variation_Code] = '" + entity.Variation_Code + "' And [Commodity_Code] = '" + entity.Commodity.Code + "' And [IsPO] = 'True'");
                    else
                        FilterCriteria = CriteriaOperator.Parse("[Subjob_Name] = '" + entity.SubJob.Code + "' And [Discipline_Code] = '" + entity.Discipline.Code + "' And [Variation_Code] = '" + entity.Variation_Code + "' And [IsPO] = 'True'");
                    IsHidden = false;

                    IsPOColumnsVisible = true;
                    this.RaisePropertyChanged(x => x.FilterCriteria);
                    this.RaisePropertyChanged(x => x.IsPOColumnsVisible);
                }
                else
                {
                    IsHidden = true;
                }
            //}
            //else
            //    IsHidden = true;

            //this.RaisePropertyChanged(x => x.IsHidden);
        }

        public void DetailGridKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.F)
                {
                    clearFilter();
                }
            }
        }

        private void clearFilter()
        {
            IsHidden = false;
            IsPOColumnsVisible = false;

            //workaround for when detail grid doesn't show anything when it's first loaded, bug on devexpress
            FilterCriteria = CriteriaOperator.Parse("[Subjob_Name] = '000'");
            this.RaisePropertyChanged(x => x.FilterCriteria);

            FilterCriteria = CriteriaOperator.Parse("");
            this.RaisePropertyChanged(x => x.IsHidden);
            this.RaisePropertyChanged(x => x.FilterCriteria);
            this.RaisePropertyChanged(x => x.IsPOColumnsVisible);
            this.RaisePropertyChanged(x => x.ActualsDetail);
        }

        public void HideColumns(AutoGeneratingColumnEventArgs e)
        {
            if (hiddenColumnFieldNames.Any(x => x == e.Column.FieldName))
            {
                e.Cancel = true;
            }
            else
            {
                DateTime parsedate;
                if (DateTime.TryParse(e.Column.FieldName, out parsedate))
                {
                    e.Column.HeaderTemplate = Application.Current.Resources["ForecastHeaderTemplate"] as DataTemplate;
                    e.Column.Width = 60;
                    e.Column.AllowBestFit = DevExpress.Utils.DefaultBoolean.False;
                }
                else
                {
                    e.Column.ReadOnly = true;
                    e.Column.Fixed = FixedStyle.Left;
                }
            }
        }

        public void AutoGeneratingColumns(AutoGeneratingColumnEventArgs e)
        {
            GridControl gridControl = (GridControl)e.Source;
            DateTime parsedate;
            if (DateTime.TryParse(e.Column.FieldName, out parsedate))
            {
                if(parsedate <= FixedDataDateMonthEnd)
                {
                    e.Column.HeaderTemplate = Application.Current.Resources["ForecastHeaderTemplate"] as DataTemplate;
                    e.Column.CellTemplate = Application.Current.Resources["forecastTemplatePast"] as DataTemplate;
                    e.Column.AllowEditing = DevExpress.Utils.DefaultBoolean.False;
                    e.Column.ReadOnly = true;
                }
                else
                {
                    e.Column.HeaderTemplate = Application.Current.Resources["ForecastHeaderTemplate"] as DataTemplate;
                    e.Column.CellTemplate = Application.Current.Resources["forecastTemplateFuture"] as DataTemplate;
                }

                GridControlService.AddSummary(e.Column.FieldName, SummaryItemType.Sum, "c0");
                e.Column.FilterPopupMode = FilterPopupMode.CheckedList;
                e.Column.Width = 60;
                e.Column.AllowBestFit = DevExpress.Utils.DefaultBoolean.False;
                e.Column.AddHandler(DXSerializer.AllowPropertyEvent, new AllowPropertyEventHandler(column_AllowProperty));
            }
            else
            {
                if (e.Column.FieldType == typeof(decimal))
                    GridControlService.AddSummary(e.Column.FieldName, SummaryItemType.Sum, e.Column.FieldName + ": {0:c0}");

                e.Column.ReadOnly = true;
                e.Column.Fixed = FixedStyle.Left;
            }
        }

        void column_AllowProperty(object sender, AllowPropertyEventArgs e)
        {
            e.Allow = false;
        }

        //there's a problem here where detaildescriptor autogeneratingcolumn only called once when expanded, so when user clicks refresh the old dates still stays
        public void AutoGeneratingChildColumns(AutoGeneratingColumnEventArgs e)
        {
            if (!hiddenColumnFieldNames.Any(x => x == e.Column.FieldName))
            {
                GridControl gridControl = (GridControl)e.Source;
                DateTime parsedate;
                if (DateTime.TryParse(e.Column.FieldName, out parsedate))
                {
                    //even this doesn't fix the problem because it is only called once
                    if(!alignedDataDateCollection.Any(x => x.Date.Date == parsedate))
                    {
                        e.Cancel = true;
                    }
                    else
                    {
                        if (parsedate <= FixedDataDateMonthEnd)
                        {
                            e.Column.CellTemplate = Application.Current.Resources["forecastTemplatePast"] as DataTemplate;
                            e.Column.AllowEditing = DevExpress.Utils.DefaultBoolean.False;
                            e.Column.ReadOnly = true;
                        }
                        else
                            e.Column.CellTemplate = Application.Current.Resources["forecastTemplateChild"] as DataTemplate;

                        e.Column.Width = 75;
                        e.Column.AddHandler(DXSerializer.AllowPropertyEvent, new AllowPropertyEventHandler(column_AllowProperty));
                        //GridControlService.AddSummary(e.Column.FieldName, SummaryItemType.Sum, "n0");
                        e.Column.FilterPopupMode = FilterPopupMode.CheckedList;
                    }
                }
                else
                {
                    if (e.Column.FieldType == typeof(decimal))
                        GridControlService.AddSummary(e.Column.FieldName, SummaryItemType.Sum, e.Column.FieldName + ": {0:c0}");

                    e.Column.ReadOnly = true;
                    e.Column.Fixed = FixedStyle.Left;
                }
            }
            else
            {
                e.Cancel = true;
            }
        }

        private bool gridSummaryItemExists(GridSummaryItemCollection gridSummaryItems, string fieldName)
        {
            for (int i = 0; i < gridSummaryItems.Count; i++)
            {
                if (gridSummaryItems[i].FieldName == fieldName)
                    return true;
            }

            return false;
        }

        public void KeyboardCopy()
        {
            System.Windows.Forms.SendKeys.SendWait("^c");
        }

        public void KeyboardPaste()
        {
            System.Windows.Forms.SendKeys.SendWait("^v");
        }
        
        public virtual void PastingFromClipboard(PastingFromClipboardEventArgs e)
        {
            GridControl gridControl = (GridControl)e.Source;
            TableView gridTableView = (TableView)gridControl.View;
            string newValueString = Clipboard.GetText().ToString();

            //remove tab in front
            if(newValueString != string.Empty)
            {
                if (newValueString.Substring(0, 1) == "\t")
                {
                    newValueString = newValueString.Substring(1, newValueString.Length - 1);
                }

                string[] RowData = DataUtils.ExcelSplit(newValueString).ToArray();
                pasteCellData(gridControl, gridTableView, RowData);

                refreshGridData();
                e.Handled = true;
            }

        }

        private void pasteCellData(GridControl gridControl, TableView gridTableView, string[] RowData)
        {
            EntitiesUndoRedoManager.PauseActionId();
            GridControlHelpers.PasteCellData(gridControl, gridTableView, RowData, basePasteData);
            EntitiesUndoRedoManager.UnpauseActionId();
        }

        private bool basePasteData(DataRow newRow, ColumnBase copyColumn, string pasteData, bool isLastRow)
        {
            if(copyColumn.FieldName.ToUpper() == "ENTITY.BUDGET")
            {
                //currently disabled on paste because view doesn't reflect changes
                //return commitBudget(newRow, pasteData);
            }
            else if(copyColumn.FieldName == "Entity." + BindableBase.GetPropertyName(() => new ForecastJobData().Productivity))
            {
                decimal decimal_value;
                var rgx = new Regex("[^0-9a-z\\.]");
                var cleanColumnString = rgx.Replace(pasteData, string.Empty);
                if (decimal.TryParse(cleanColumnString, out decimal_value))
                {
                    ForecastJobData job = ((ForecastJobData)newRow[columnEntity]);
                    decimal oldValue = job.Productivity;

                    commitCellValue(copyColumn.FieldName, newRow, oldValue, decimal_value);
                    findExistingOrAddNewForecastJobSetting(newRow, false);
                }
            }
            else if (copyColumn.FieldType == typeof(decimal))
            {
                decimal? oldValue = newRow[copyColumn.FieldName] == DBNull.Value ? (decimal?)null : (decimal)newRow[copyColumn.FieldName];
                var rgx = new Regex("[^0-9a-z\\.]");
                var cleanColumnString = rgx.Replace(pasteData, string.Empty);
                decimal decimal_value;
                if (decimal.TryParse(cleanColumnString, out decimal_value))
                {
                    DateTime columnDateTime;
                    if (DateTime.TryParse(copyColumn.FieldName, out columnDateTime))
                    {
                        DataTable compareDataTable = (DataTable)newRow["CompareEntities"];
                        //when this is called from parent grid
                        if (compareDataTable.TableName == BluePrintsResources.ForecastCompareTableName)
                        {
                            ForecastJobData job = (ForecastJobData)newRow[columnEntity];
                            decimal totalCosts = 0;
                            if (job.IsProcurement)
                                totalCosts = getMasterRowResetValue(compareDataTable, copyColumn.FieldName);

                            if (decimal_value >= totalCosts)
                            {
                                findExistingOrAddNewForecast(newRow, columnDateTime, decimal_value, newRow[copyColumn.FieldName], !isLastRow);
                                EntitiesUndoRedoManager.AddUndo(newRow, copyColumn.FieldName, oldValue, decimal_value, EntityMessageType.Changed);
                                newRow[copyColumn.FieldName] = decimal_value;
                            }
                        }
                        //when this called from child grid no validation required
                        else
                        {
                            findExistingOrAddNewForecast(newRow, columnDateTime, decimal_value, newRow[copyColumn.FieldName], !isLastRow);
                            newRow[copyColumn.FieldName] = decimal_value;
                        }
                    }
                }
                else
                {
                    resetViewRemainingOnJob(newRow, copyColumn.FieldName, true);
                    return false;
                }
            }
            else if (copyColumn.FieldType == typeof(string))
            {
                string oldValue = newRow[copyColumn.FieldName].ToString();
                newRow[copyColumn.FieldName] = pasteData;
                EntitiesUndoRedoManager.AddUndo(newRow, copyColumn.FieldName, oldValue, pasteData, EntityMessageType.Changed);
            }

            return true;
        }

        public void DeleteCellContent()
        {
            EntitiesUndoRedoManager.PauseActionId();
            var selected_cells = getSelectedCells();

            foreach (var selected_cell in selected_cells)
            {
                int row_handle = selected_cell.RowHandle;
                DataRowView editing_row_view = (DataRowView)GridControlService.GridControl.GetRow(row_handle);
                if (editing_row_view == null)
                    continue;

                DataRow editing_row = editing_row_view.Row;
                DataColumn editing_column = editing_row.Table.Columns[selected_cell.Column.FieldName];
                ExoSubJobProjection entity = ((ForecastJobData)editing_row[columnEntity]).Projection;

                string columnFieldName = selected_cell.Column.FieldName;
                DateTime deleteCellDate;
                if(DateTime.TryParse(columnFieldName, out deleteCellDate))
                {
                    resetViewRemainingOnJob(editing_row, columnFieldName, true);
                    findExistingOrAddNewForecast(editing_row, deleteCellDate, null);
                    //editing_row[columnFieldName] = 0.00m;
                }
            }

            EntitiesUndoRedoManager.UnpauseActionId();
            refreshGridData();
        }

        public void ApplyCurrentPF()
        {
            EntitiesUndoRedoManager.PauseActionId();
            GridControl gridControl = GridControlService.GridControl;
            TableView tableView = gridControl.View as TableView;
            var selectedRows = tableView.GetSelectedRows();

            foreach (var selectedRow in selectedRows)
            {
                int row_handle = selectedRow.RowHandle;
                DataRowView editing_row_view = (DataRowView)GridControlService.GridControl.GetRow(row_handle);
                DataRow editing_row = editing_row_view.Row;
                ForecastJobData job = (ForecastJobData)editing_row[columnEntity];
                ExoSubJobProjection entity = job.Projection;

                if(job.CurrentProductivity > 0)
                {
                    commitCellValue(BindableBase.GetPropertyName(() => new ForecastJobData().Productivity), editing_row, job.Productivity, job.CurrentProductivity);
                    findExistingOrAddNewForecastJobSetting(editing_row, true);
                }
            }

            EntitiesUndoRedoManager.UnpauseActionId();
            refreshGridData();
        }

        public void UpdateCurrentPF()
        {
            if (MessageBoxService.ShowMessage("Are you use you want to apply current PF to all jobs that had current PF applied?", "Confirmation", MessageButton.OKCancel, MessageIcon.Question) == MessageResult.Cancel)
                return;

            IEnumerable<DataRow> enumerableRows = from DataRow dr in dataPointsTable.Rows select dr;
            foreach (var row in enumerableRows)
            {
                ForecastJobData job = (ForecastJobData)row[columnEntity];
                if(job.IsProductivityFloating && job.CurrentProductivity > 0)
                {
                    commitCellValue(BindableBase.GetPropertyName(() => new ForecastJobData().Productivity), row, job.Productivity, job.CurrentProductivity);
                }
            }

            refreshGridData();
        }

        private IEnumerable<GridCell> getSelectedCells()
        {
            GridControl gridControl = GridControlService.GridControl;
            TableView tableView = gridControl.View as TableView;
            var selected_cells = tableView.GetSelectedCells();
            if (selected_cells.Count == 0)
            {
                selected_cells = Enumerable.Range(0, gridControl.VisibleRowCount)
                .Select(x => (GridControl)gridControl.GetDetail(x))
                .Where(x => x != null).
                SelectMany(x => ((TableView)(x).View).GetSelectedCells()).ToList();

                if (selected_cells.Count == 0)
                    return selected_cells;
                else
                {
                    tableView = (TableView)selected_cells.First().Column.View;
                    gridControl = tableView.Grid;
                }
            }

            return selected_cells;
        }

        /// <summary>
        /// Influence column(s) when changes happens in other column
        /// </summary>
        public void CellValueChangedUpdate(CellValueChangedEventArgs e)
        {
            if (e.RowHandle == GridControl.AutoFilterRowHandle)
                return;

            DataRowView dataRowView = (DataRowView)e.Row;
            EntitiesUndoRedoManager.PauseActionId();

            bool removeFloatingProductivity = false;
            ForecastJobData job = ((ForecastJobData)dataRowView.Row[columnEntity]);
            if(job.IsProductivityFloating)
            {
                DateTime dateTime;
                if (DateTime.TryParse(e.Column.FieldName, out dateTime))
                    removeFloatingProductivity = true;
                else if (e.Column.FieldName.Contains(BindableBase.GetPropertyName(() => new ForecastJobData().Productivity)))
                    removeFloatingProductivity = true;

                if (removeFloatingProductivity)
                {
                    findExistingOrAddNewForecastJobSetting(dataRowView.Row, false);
                }
            }

            commitCellValue(e.Column.FieldName, dataRowView.Row, e.OldValue, e.Value);
            EntitiesUndoRedoManager.UnpauseActionId();

            this.RaisePropertyChanged(x => x.ForecastSummary);
            refreshGridData();
            e.Handled = true;
        }

        protected virtual void commitCellValue(string fieldName, DataRow row, object oldValue, object newValue)
        {
            ForecastJobData forecastJobData = ((ForecastJobData)row[columnEntity]);
            ExoSubJobProjection entity = forecastJobData.Projection;

            fieldName = fieldName.Replace("Entity.", "");
            if (fieldName == BindableBase.GetPropertyName(() => new ForecastJobData().Budget) || fieldName == BindableBase.GetPropertyName(() => new ForecastJobData().Rate))
            {
                commitBudget(primeroEntitiesUnitOfWork, row, newValue);
                forecastJobData.JobErrorMessage = string.Empty;
                entity.ForecastErrorString = string.Empty;
                forecastJobData.RaisePropertiesChanged();
            }
            else if(fieldName.Contains(BindableBase.GetPropertyName(() => new ForecastJobData().Productivity)))
            {
                if(newValue != null)
                {
                    decimal newProductivity = (decimal)newValue;
                    ForecastJobData job = ((ForecastJobData)row[columnEntity]);

                    DataTable compareDataTable = (DataTable)row[columnCompare];
                    DataRow compareP6UnitsRemainingRow = compareDataTable.Rows[Convert.ToInt32(BluePrintsResources.ForecastCompare_P6HourRow)];

                    DataTable compareChildDataTable = (DataTable)compareP6UnitsRemainingRow[columnCompare];
                    DataRow compareChildP6UnitsRemainingRow = compareChildDataTable.Rows[Convert.ToInt32(BluePrintsResources.ForecastCompareChild_P6HourRow)];

                    EntitiesUndoRedoManager.PauseActionId();
                    List<FORECAST> resetFORECASTS = FORECASTCollectionViewModel.Entities.Where(x => x.SUBJOB_CODE == job.Projection.SubJob.Code && x.DISCIPLINE_CODE == job.Projection.Discipline.Code && x.COMMODITY_CODE == job.Projection.Commodity.Code && x.VARIATION_CODE == job.Projection.Variation_Code).ToList();
                    foreach (FORECAST resetFORECAST in resetFORECASTS)
                    {
                        resetFORECAST.FORECAST_UNITS = null;
                    }

                    FORECASTCollectionViewModel.BulkSave(resetFORECASTS);
                    foreach (ForecastDateCost dateCost in job.DateCosts)
                    {
                        string alignedDateField = (dateCost.Date).ToString(BluePrintsResources.ColumnDateFormat);
                        decimal originalP6Units = (decimal)compareChildP6UnitsRemainingRow[alignedDateField];
                        decimal oldP6Units = (decimal)compareP6UnitsRemainingRow[alignedDateField];
                        if (originalP6Units > 0)
                        {
                            decimal newP6Units = originalP6Units / newProductivity;
                            findExistingOrAddNewForecast(compareP6UnitsRemainingRow, dateCost.Date, newP6Units, oldP6Units);
                        }
                        else
                        {
                            resetChildRow(compareDataTable, alignedDateField, false);
                        }
                    }

                    EntitiesUndoRedoManager.UnpauseActionId();
                }
            }
            else
            {
                DateTime dateTime;
                if (DateTime.TryParse(fieldName, out dateTime))
                {
                    decimal? forecastUnits = null;
                    decimal convertUnits = 0;
                    if (newValue != null && decimal.TryParse(newValue.ToString(), out convertUnits))
                        forecastUnits = convertUnits;

                    findExistingOrAddNewForecast(row, dateTime, forecastUnits, oldValue);
                }
            }
        }

        public void ValidateCell(GridCellValidationEventArgs e)
        {
            if(e.Value != null)
            {
                DateTime dateTime;
                if (IsWeeks)
                {
                    e.ErrorContent = "Sorry, cells in weeks view aren't editable";
                    e.IsValid = false;
                }
                else if (DateTime.TryParse(e.Column.FieldName, out dateTime))
                {
                    decimal defaultCosts = getMasterRowResetValue((DataTable)(((DataRowView)e.Row)[columnCompare]), e.Column.FieldName);
                    if ((decimal)e.Value < defaultCosts)
                    {
                        e.ErrorContent = "Cannot set costs below forecasted costs";
                        e.IsValid = false;
                    }
                }
                else if(e.Column.FieldName.Contains(BindableBase.GetPropertyName(() => new ForecastJobData().Budget)))
                {
                    if (!LoginCredentials.hasPermission(PermissionResources.ChangeBudget))
                    {
                        e.ErrorContent = "You do not have permission to change the budget";
                        e.IsValid = false;
                    }    
                }
                else if(e.Column.FieldName.Contains(BindableBase.GetPropertyName(() => new ForecastJobData().Productivity)))
                {
                    DataRowView row = (DataRowView)e.Row;

                    if (sumP6Units(row) == 0)
                    {
                        e.ErrorContent = "There are no units from P6 to override productivity, please edit P6 hours manually";
                        e.IsValid = false;
                    }
                    else
                    {
                        ForecastJobData job = (ForecastJobData)row[columnEntity];
                        ExoSubJobProjection projection = job.Projection;
                        if (FORECASTCollectionViewModel.Entities.Any(x => x.SUBJOB_CODE == projection.SubJob.Code && x.DISCIPLINE_CODE == projection.Discipline.Code && x.COMMODITY_CODE == projection.Commodity.Code && x.VARIATION_CODE == projection.Variation_Code))
                        {
                            if(MessageBoxService.ShowMessage("Any forecast done on this job will be removed and automatically generated based on PF = " + e.Value.ToString() + ", do you wish to continue?", "Warning", MessageButton.OKCancel) == MessageResult.Cancel)
                            {
                                e.ErrorContent = "Action cancelled";
                                e.IsValid = false;
                            }
                        }
                    }
                }
            }
        }

        private bool commitBudget(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, DataRow dataRow, object newValue)
        {
            ForecastJobData job = ((ForecastJobData)dataRow[columnEntity]);
            ExoSubJobProjection entity = job.Projection;
            decimal newDecimalValue = 0;
            if (newValue != null && decimal.TryParse(newValue.ToString(), out newDecimalValue))
            {
                ExoSubJobEditableProjection projection = new ExoSubJobEditableProjection(entity);
                JOBCOST_LINES findExistingOrAddLine = ExoQueries.GetProjectLine(primeroUnitOfWork, LoadPROJECT.NUMBER, projection);
                bool isError = false;
                projection.ExoBudget = newDecimalValue;

                if (findExistingOrAddLine == null)
                {
                    if (masterJob == null)
                    {
                        MessageBoxService.ShowMessage("Cannot change budget because the master job is not created for project " + LoadPROJECT.NUMBER + " isn't added\nPlease contact " + BluePrintsResources.Default_CFO);
                        isError = true;
                    }
                    else if (copyLine == null)
                    {
                        MessageBoxService.ShowMessage("Cannot change budget because the master line is not created for project " + LoadPROJECT.NUMBER + " isn't added\nPlease contact " + BluePrintsResources.Default_CFO);
                        isError = true;
                    }
                    else if (ExoMethods.CommitLineSubJob(projection, false, BulkColumnEditDialogService, masterJob, LoadPROJECT.NUMBER, primeroUnitOfWork))
                    {
                        if (ExoMethods.CommitLineDiscipline(projection, false, BulkColumnEditDialogService, masterJob, LoadPROJECT.NUMBER, primeroUnitOfWork))
                        {
                            //stock item cannot be added, so it must exists before commodity can be added using it
                            string stockCode = projection.GetStockCode();
                            STOCK_ITEMS stock_item = ExoQueries.FindSTOCK_ITEM(primeroUnitOfWork, stockCode);
                            if (stock_item != null)
                            {
                                projection.StockName = stock_item.DESCRIPTION;
                                if (ExoMethods.CommitLineCommodity(projection, stock_item, false, BulkColumnEditDialogService, masterJob, LoadPROJECT.NUMBER, primeroUnitOfWork))
                                {
                                    int? maxJOBCOSTLINEID = ExoQueries.GetJOBCODELINEID(primeroUnitOfWork);
                                    JOBCOST_LINES newLine = ExoMethods.CreateNewLine(copyLine, projection, (int)maxJOBCOSTLINEID);
                                    primeroUnitOfWork.JOBCOST_LINES.Add(newLine);
                                    primeroUnitOfWork.SaveChanges();
                                    entity.LineId = newLine.SEQNO;
                                }
                                else
                                {
                                    MessageBoxService.ShowMessage("Cannot change budget because commodity code " + projection.CommodityCode + " is not added\nPlease contact " + BluePrintsResources.Default_CFO);
                                    isError = true;
                                }
                            }
                            else
                            {
                                MessageBoxService.ShowMessage("Cannot change budget because stock code " + stockCode + " is not added\nPlease contact " + BluePrintsResources.Default_CFO);
                                isError = true;
                            }
                        }
                        else
                        {
                            MessageBoxService.ShowMessage("Cannot change budget because cost group " + projection.DisciplineCode + " is not added\nPlease contact " + BluePrintsResources.Default_CFO);
                            isError = true;
                        }
                    }
                    else
                    {
                        MessageBoxService.ShowMessage("Cannot change budget because subjob " + projection.SubJobCode + " is not added\nPlease contact " + BluePrintsResources.Default_CFO);
                        isError = true;
                    }

                    if (isError)
                        projection.ExoBudget = 0;
                    else
                    {
                        DataRow disciplineRow = findRow(entity, false);
                        if (disciplineRow != null)
                        {
                            recurseCalculateBudget(disciplineRow);
                        }
                    }

                    projection.Update();
                }
                else
                {
                    findExistingOrAddLine.QUOTE_QTY = 1;
                    findExistingOrAddLine.ACTUAL_UNITCOST = Convert.ToDouble(newDecimalValue);

                    primeroUnitOfWork.SaveChanges();
                    DataRow disciplineRow = findRow(entity, false);
                    if (disciplineRow != null)
                    {
                        recurseCalculateBudget(disciplineRow);
                    }
                }

                refreshGridData();
                updateFloatingSummaryMembers();
            }

            return true;
        }

        private void recurseCalculateBudget(DataRow commodityRow)
        {
            ForecastJobData commodityJob = (ForecastJobData)commodityRow[columnEntity];
            commodityJob.SetBudgetCost(commodityJob.Budget);
            commodityJob.SetForecastRate(commodityJob.Rate);
        }

        private decimal getMasterRowResetValue(DataTable compareDataTable, string dateFieldName)
        {
            if (compareDataTable != null && compareDataTable.Rows.Count > 0)
            {
                if (compareDataTable.Columns.Contains(dateFieldName))
                {
                    decimal totalValue = 0;
                    if(compareDataTable.TableName == BluePrintsResources.ForecastCompareChildTableName)
                    {
                        //when delete button is pressed on the P6 units cell
                        DataRow compareP6HoursRemainingRow = compareDataTable.Rows[Convert.ToInt32(BluePrintsResources.ForecastCompareChild_P6HourRow)];
                        totalValue = compareP6HoursRemainingRow[dateFieldName] == DBNull.Value ? 0 : (decimal)compareP6HoursRemainingRow[dateFieldName];
                    }
                    else
                    {
                        List<DataRow> costRows = new List<DataRow>();
                        foreach(DataRow costRow in compareDataTable.Rows)
                        {
                            ForecastJobData job = (ForecastJobData)costRow[columnEntity];
                            string dropDownPhase = job.DropDownPhase;
                            if(dropDownPhase.Contains(BluePrintsResources.ForecastCompare_PORowPhase) || dropDownPhase.Contains(BluePrintsResources.ForecastCompare_IndirectRowPhase)
                               || dropDownPhase.Contains(BluePrintsResources.ForecastCompare_MaterialRowPhase) || dropDownPhase.Contains(BluePrintsResources.ForecastCompare_IndirectRowPhase))
                            {
                                costRows.Add(costRow);
                            }
                        }

                        DataRow compareP6CostsRemainingRow = compareDataTable.Rows[Convert.ToInt32(BluePrintsResources.ForecastCompare_P6CostRow)];
                        DataRow compareP6UnitsRemainingRow = compareDataTable.Rows[Convert.ToInt32(BluePrintsResources.ForecastCompare_P6HourRow)];

                        DataTable compareChildDataTable = (DataTable)compareP6UnitsRemainingRow[columnCompare];
                        DataRow compareChildP6CostsRemainingRow = compareChildDataTable.Rows[Convert.ToInt32(BluePrintsResources.ForecastCompareChild_P6CostRow)];
                        DataRow compareChildP6UnitsRemainingRow = compareChildDataTable.Rows[Convert.ToInt32(BluePrintsResources.ForecastCompareChild_P6HourRow)];

                        decimal p6CostValue = compareChildP6CostsRemainingRow[dateFieldName] == DBNull.Value ? 0 : (decimal)compareChildP6CostsRemainingRow[dateFieldName];
                        decimal dynamicCostsFromCostRows = 0;
                        foreach (DataRow costRow in costRows)
                        {
                            dynamicCostsFromCostRows += (decimal)costRow[dateFieldName];
                        }

                        totalValue = p6CostValue + dynamicCostsFromCostRows;
                    }

                    return totalValue;
                }
            }

            return 0.00m;
        }

        private void resetChildRow(DataTable compareDataTable, string dateFieldName, bool addUndo)
        {
            if (compareDataTable != null && compareDataTable.Rows.Count > 0)
            {
                if (compareDataTable.Columns.Contains(dateFieldName))
                {
                    if (compareDataTable.TableName == BluePrintsResources.ForecastCompareTableName)
                    {
                        DataRow compareP6CostsRemainingRow = compareDataTable.Rows[Convert.ToInt32(BluePrintsResources.ForecastCompare_P6CostRow)];
                        DataRow compareP6UnitsRemainingRow = compareDataTable.Rows[Convert.ToInt32(BluePrintsResources.ForecastCompare_P6HourRow)];

                        DataTable compareChildDataTable = (DataTable)compareP6UnitsRemainingRow[columnCompare];
                        DataRow compareChildP6CostsRemainingRow = compareChildDataTable.Rows[Convert.ToInt32(BluePrintsResources.ForecastCompareChild_P6CostRow)];
                        DataRow compareChildP6UnitsRemainingRow = compareChildDataTable.Rows[Convert.ToInt32(BluePrintsResources.ForecastCompareChild_P6HourRow)];

                        decimal childP6CostsValue = compareChildP6CostsRemainingRow[dateFieldName] == DBNull.Value ? 0 : (decimal)compareChildP6CostsRemainingRow[dateFieldName];
                        decimal childP6UnitsValue = compareChildP6UnitsRemainingRow[dateFieldName] == DBNull.Value ? 0 : (decimal)compareChildP6UnitsRemainingRow[dateFieldName];
                        if (addUndo)
                        {
                            //EntitiesUndoRedoManager.AddUndo(compareP6UnitsRemainingRow, dateFieldName, compareP6UnitsRemainingRow[dateFieldName], childP6CostsValue, EntityMessageType.Changed);
                            EntitiesUndoRedoManager.AddUndo(compareP6UnitsRemainingRow, dateFieldName, compareP6UnitsRemainingRow[dateFieldName], childP6UnitsValue, EntityMessageType.Changed);
                        }

                        compareP6CostsRemainingRow[dateFieldName] = childP6CostsValue;
                        compareP6UnitsRemainingRow[dateFieldName] = childP6UnitsValue;
                    }
                }
            }
        }

        private decimal sumP6Units(DataRowView parentRow)
        {
            decimal p6Units = 0;
            DataTable compareDataTable = (DataTable)parentRow[columnCompare];
            if (compareDataTable != null && compareDataTable.Rows.Count > 0)
            {
                if (compareDataTable.TableName == BluePrintsResources.ForecastCompareTableName)
                {
                    DataRow compareP6UnitsRemainingRow = compareDataTable.Rows[Convert.ToInt32(BluePrintsResources.ForecastCompare_P6HourRow)];
                    DataTable compareChildDataTable = (DataTable)compareP6UnitsRemainingRow[columnCompare];
                    DataRow compareChildP6UnitsRemainingRow = compareChildDataTable.Rows[Convert.ToInt32(BluePrintsResources.ForecastCompareChild_P6HourRow)];

                    foreach(DataColumn column in compareDataTable.Columns)
                    {
                        DateTime parseDateTime;
                        if(DateTime.TryParse(column.ColumnName, out parseDateTime))
                        {
                            decimal childP6UnitsValue = compareChildP6UnitsRemainingRow[column.ColumnName] == DBNull.Value ? 0 : (decimal)compareChildP6UnitsRemainingRow[column.ColumnName];

                            p6Units += childP6UnitsValue;
                        }
                    }
                }
            }

            return p6Units;
        }

        /// <summary>
        /// update view and database at the same time
        /// </summary>
        /// <param name="dataRow">data row containing the job and compare info</param>
        /// <param name="forecastDate">date of the forecast to update</param>
        /// <param name="viewNewValue">determine what will be updated in db but will be replaced by null if it's same as compare info, 
        /// however if it is passed in as null it signifies that the view is already updated and won't update it</param>
        /// <param name="addUndo">whether to add undo information</param>
        private void findExistingOrAddNewForecast(DataRow dataRow, DateTime forecastDate, decimal? viewNewValue, object oldValue = null, bool skipRowRefresh = false)
        {
            ForecastJobData job = (ForecastJobData)dataRow[columnEntity];
            ExoSubJobProjection entity = job.Projection;
            string dateFieldName = forecastDate.ToString(BluePrintsResources.ColumnDateFormat);
            decimal? compareValue = null;
            decimal? saveNewValue = viewNewValue;

            //when p6 % is going through undo/redo this will happen
            if (dataRow[columnCompare].GetType() != typeof(DataTable))
                return;

            DataTable compareDataTable = (DataTable)dataRow[columnCompare];
            decimal resetValue = getMasterRowResetValue(compareDataTable, dateFieldName);
            if (resetValue > 0)
            {
                compareValue = resetValue;
            }

            if(viewNewValue != null && compareValue != null && viewNewValue == compareValue)
            {
                saveNewValue = null;
            }

            ForecastDataType editForecastDataType = job.IsP6HoursRow ? ForecastDataType.P6 : ForecastDataType.Cost;
            if(oldValue != null)
            {
                //when cost has been edited on master row, reset p6 override row
                if (editForecastDataType == ForecastDataType.Cost)
                    resetChildRow(compareDataTable, dateFieldName, true);

                EntitiesUndoRedoManager.AddUndo(dataRow, dateFieldName, oldValue, saveNewValue, EntityMessageType.Changed);
            }

            //this is definitely present because the view is generated from datecost model
            ForecastDateCost dateCost = job.DateCosts.First(x => x.Date == forecastDate.Date);

            IEnumerable<FORECAST> findFORECASTS = FORECASTCollectionViewModel.Entities.Where(x => x.FORECAST_DATE >= dateCost.FloorDate && x.FORECAST_DATE <= dateCost.CeilingDate && x.SUBJOB_CODE == entity.SubJob.Code && x.DISCIPLINE_CODE == entity.Discipline.Code && x.COMMODITY_CODE == entity.Commodity.Code && x.VARIATION_CODE == entity.Variation_Code);
            IEnumerable<FORECAST> findCostFORECASTS = findFORECASTS.Where(x => x.FORECAST_TYPE == ForecastDataType.Cost);
            IEnumerable<FORECAST> findP6FORECASTS = findFORECASTS.Where(x => x.FORECAST_TYPE == ForecastDataType.P6);
            FORECAST costFORECAST = findCostFORECASTS.FirstOrDefault(x => x.FORECAST_DATE == forecastDate.Date);
            FORECAST p6FORECAST = findP6FORECASTS.FirstOrDefault(x => x.FORECAST_DATE == forecastDate.Date);
            
            FORECAST editFORECAST = editForecastDataType == ForecastDataType.Cost ? costFORECAST : p6FORECAST;
            FORECAST resetFORECAST = editForecastDataType == ForecastDataType.Cost ? p6FORECAST : costFORECAST;

            List<FORECAST> deleteFORECASTS = new List<FORECAST>();
            deleteFORECASTS.AddRange(findCostFORECASTS.Where(x => x.FORECAST_DATE != forecastDate.Date));
            deleteFORECASTS.AddRange(findP6FORECASTS.Where(x => x.FORECAST_DATE != forecastDate.Date));

            foreach(FORECAST deleteFORECAST in deleteFORECASTS)
            {
                deleteFORECAST.FORECAST_UNITS = null;
            }

            FORECASTCollectionViewModel.BulkSave(deleteFORECASTS);

            if (editFORECAST == null)
            {
                editFORECAST = new FORECAST();
                editFORECAST.GUID = Guid.Empty;
                editFORECAST.GUID_PROJECT = LoadPROJECT.GUID;
                editFORECAST.SUBJOB_CODE = entity.SubJob.Code;
                editFORECAST.DISCIPLINE_CODE = entity.Discipline.Code;
                editFORECAST.COMMODITY_CODE = entity.Commodity.Code;
                editFORECAST.VARIATION_CODE = normalizeVariationCode(entity.Variation_Code);
                editFORECAST.FORECAST_DATE = forecastDate.Date;
                editFORECAST.FORECAST_UNITS = saveNewValue;
                editFORECAST.FORECAST_TYPE = editForecastDataType;
                FORECASTCollectionViewModel.Save(editFORECAST);
            }
            else
            {
                editFORECAST.FORECAST_UNITS = saveNewValue;
                FORECASTCollectionViewModel.Save(editFORECAST);
            }

            if (resetFORECAST == null)
            {
                resetFORECAST = new FORECAST();
                DataUtils.ShallowCopy(resetFORECAST, editFORECAST);
                resetFORECAST.GUID = Guid.Empty;
                resetFORECAST.FORECAST_TYPE = editForecastDataType == ForecastDataType.Cost ? ForecastDataType.P6 : ForecastDataType.Cost;
            }

            //either reset p6 or cost info to null
            if (editForecastDataType == ForecastDataType.P6)
            {
                resetFORECAST.FORECAST_UNITS = saveNewValue * job.P6NominalRate;
            }
            else
                resetFORECAST.FORECAST_UNITS = null;

            FORECASTCollectionViewModel.Save(resetFORECAST);

            //used to ensure child row is set
            if (viewNewValue != null)
            {
                dataRow[forecastDate.ToString(BluePrintsResources.ColumnDateFormat)] = viewNewValue;
            }

            if(!skipRowRefresh)
            {
                updateViewForecastsOnDatesFromDb(dataRow, true);
                updateTotalUncommittedOnJob(dataRow, true);
                updateFloatingSummaryMembers();
            }
        }

        private void refreshGridData()
        {
            refreshGridDataDelayed();
        }

        private void refreshGridDataDelayed()
        {
            delayedGridUpdateTimer.Tick -= DelayedGridUpdateTimer_Tick;
            delayedGridUpdateTimer.Tick += DelayedGridUpdateTimer_Tick;
            delayedGridUpdateTimer.Start();
        }

        private void DelayedGridUpdateTimer_Tick(object sender, EventArgs e)
        {
            delayedGridUpdateTimer.Stop();
            GridControlEx gridControlEx = (GridControlEx)GridControlService.GridControl;
            ObservableCollection<BaseModel.Misc.GroupInfo> saveStates = gridControlEx.States;
            GridControlService.GridControl.RefreshData();
            DataControlDetailDescriptor gridDetail = (DataControlDetailDescriptor)GridControlService.GridControl.DetailDescriptor;
            GridControl childGrid = (GridControl)gridDetail.DataControl;

            childGrid.RefreshRow(0);
            childGrid.RefreshRow(1);
            childGrid.RefreshRow(2);
            childGrid.RefreshRow(3);
            childGrid.RefreshRow(4);

            gridControlEx.States = saveStates;
        }

        private void updateFloatingSummaryMembers()
        {
            delayedUpdateFloatingProjectSummaryTimer.Tick -= DelayedUpdateFloatingProjectSummaryTimer_Tick;
            delayedUpdateFloatingProjectSummaryTimer.Tick += DelayedUpdateFloatingProjectSummaryTimer_Tick;
            delayedUpdateFloatingProjectSummaryTimer.Start();
        }

        private void DelayedUpdateFloatingProjectSummaryTimer_Tick(object sender, EventArgs e)
        {
            delayedUpdateFloatingProjectSummaryTimer.Stop();

            List<ForecastJobData> jobs = getJobDataFromDatatable();
            ForecastSummary.Reset();
            //cannot use parallel foreach because of inaccuracy
            foreach(ForecastJobData job in jobs)
            {
                ForecastSummary.Budget_Cost += job.Budget;
                ForecastSummary.OriginalEstimateAtCompletion += job.OriginalEstimateAtCompletion;
                ForecastSummary.Current_Cost += job.ActualCosts;
                ForecastSummary.EstimateAtCompletion += job.EstimateAtCompletion;
                ForecastSummary.CurrentEstimateAtCompletion += job.CurrentEstimateAtCompletion;
                ForecastSummary.Uncommitted_Forecast += job.Uncommitted;
            }

            this.RaisePropertyChanged(x => x.ForecastSummary);
        }

        private List<ForecastJobData> getJobDataFromDatatable()
        {
            List<ForecastJobData> forecastJobs = new List<ForecastJobData>();
            if (dataPointsTable == null)
                return forecastJobs;
            else
            {
                IEnumerable<ForecastJobData> enumerableJobs = from DataRow dr in dataPointsTable.Rows select (ForecastJobData)dr[columnEntity];
                forecastJobs = enumerableJobs.ToList();
            }

            return forecastJobs;
        }

        private DataRow findRow(ExoSubJobProjection entity, bool searchCommodityLevel)
        {
            IEnumerable<DataRow> subjobDisciplineRows = (from DataRow dr in dataPointsTable.Rows
                                                    where (((ForecastJobData)dr[columnEntity])).Projection.SubJob.Code == entity.SubJob.Code && (((ForecastJobData)dr[columnEntity])).Projection.Discipline.Code == entity.Discipline.Code
                                                    select dr);

            IEnumerable<DataRow> variationRows;
            if (entity.Variation_Code == string.Empty || entity.Variation_Code == null)
                variationRows = subjobDisciplineRows.Where(x => ((((ForecastJobData)x[columnEntity])).Projection.Variation_Code == string.Empty || (((ForecastJobData)x[columnEntity])).Projection.Variation_Code == null));
            else
                variationRows = subjobDisciplineRows.Where(x => (((ForecastJobData)x[columnEntity])).Projection.Variation_Code == entity.Variation_Code);

            if (searchCommodityLevel)
                return variationRows.FirstOrDefault(x => (((ForecastJobData)x[columnEntity])).Projection.Commodity.Code == entity.Commodity.Code);
            else
                return variationRows.FirstOrDefault();
        }

        private void removeProjectEACOnDate(DateTime forecastDate)
        {
            List<FORECAST> projectDateEACs = bluePrintsUnitOfWork.FORECASTS.Where(x => x.FORECAST_DATE == forecastDate.Date && x.FORECAST_TYPE == ForecastDataType.EAC).ToList();
            LoadingScreenManager.ShowLoadingScreen(projectDateEACs.Count);
            foreach(FORECAST projectDateEAC in projectDateEACs)
            {
                string eacName;
                if (projectDateEAC.COMMODITY_CODE != string.Empty)
                    eacName = projectDateEAC.SUBJOB_CODE + "-" + projectDateEAC.DISCIPLINE_CODE + "-" + projectDateEAC.COMMODITY_CODE;
                else
                    eacName = projectDateEAC.SUBJOB_CODE + "-" + projectDateEAC.DISCIPLINE_CODE;

                LoadingScreenManager.SetMessage("Removing old EAC on " + forecastDate.ToString(BluePrintsResources.ColumnDateFormat) + " for job: " + eacName);
                bluePrintsUnitOfWork.FORECASTS.Remove(projectDateEAC);
                LoadingScreenManager.Progress();
            }

            bluePrintsUnitOfWork.SaveChanges();
            LoadingScreenManager.CloseLoadingScreen();
        }

        private void findExistingOrAddNewEAC(ExoSubJobProjection entity, DateTime forecastDate, decimal eacAmount)
        {
            FORECAST newFORECAST = new FORECAST();
            newFORECAST.GUID = Guid.Empty;
            newFORECAST.GUID_PROJECT = LoadPROJECT.GUID;
            newFORECAST.SUBJOB_CODE = entity.SubJob.Code;
            newFORECAST.DISCIPLINE_CODE = entity.Discipline.Code;
            newFORECAST.COMMODITY_CODE = entity.Commodity.Code;
            newFORECAST.VARIATION_CODE = normalizeVariationCode(entity.Variation_Code);
            newFORECAST.FORECAST_DATE = forecastDate.Date;
            newFORECAST.FORECAST_UNITS = eacAmount;
            newFORECAST.FORECAST_TYPE = ForecastDataType.EAC;
            bluePrintsUnitOfWork.FORECASTS.Add(newFORECAST);
        }

        /// <summary>
        /// Sum uncommitted values, need to be run after any updates to dates value
        /// </summary>
        private void updateTotalUncommittedOnJob(DataRow dataRow, bool searchParentRow = false)
        {
            ForecastJobData job = (ForecastJobData)dataRow[columnEntity];
            //need to map back into main row because datarow could be coming from p6 hours edit
            DataRow parentRow = searchParentRow ? findRow(job.Projection, true) : dataRow;
            dataRow = parentRow;
            job = (ForecastJobData)parentRow[columnEntity];

            DataTable dataTable = dataRow.Table;
            DataTable compareDataTable = (DataTable)dataRow[columnCompare];
            DataRow compareP6UnitsRemainingRow = compareDataTable.Rows[Convert.ToInt32(BluePrintsResources.ForecastCompare_P6HourRow)];
            DataTable compareChildDataTable = (DataTable)compareP6UnitsRemainingRow[columnCompare];
            DataRow compareChildP6UnitsRemainingRow = compareChildDataTable.Rows[Convert.ToInt32(BluePrintsResources.ForecastCompareChild_P6HourRow)];

            decimal uncommittedPOValues = 0;
            decimal uncommitedP6Values = 0;
            for (int i = 0; i < dataRow.ItemArray.Count(); i++)
            {
                DataColumn dataColumn = dataTable.Columns[i];
                string columnName = dataColumn.ColumnName;
                DateTime parseDateTime;
                if (DateTime.TryParse(columnName, out parseDateTime))
                    if (parseDateTime > FixedDataDateMonthEnd)
                        if (dataRow[columnName] != DBNull.Value && dataRow[columnName] != null)
                            if (((decimal)dataRow[columnName]) > 0)
                            {
                                decimal currentDateCellValue = (decimal)dataRow[columnName];
                                ForecastDateCost dateCost = job.DateCosts.FirstOrDefault(x => x.Date.Date == parseDateTime.Date);
                                if (dateCost != null)
                                {
                                    if (compareP6UnitsRemainingRow[columnName] != DBNull.Value && compareChildP6UnitsRemainingRow[columnName] != DBNull.Value)
                                    {
                                        //when current p6 units doesn't match original it means that the cost is directly contributed by P6
                                        decimal currentP6Units = (decimal)compareP6UnitsRemainingRow[columnName];
                                        decimal originalP6Units = (decimal)compareChildP6UnitsRemainingRow[columnName];

                                        if (currentP6Units == originalP6Units)
                                            uncommittedPOValues += (currentDateCellValue - dateCost.CommittedCosts - dateCost.P6Costs);
                                        else
                                            uncommitedP6Values += (currentDateCellValue - dateCost.CommittedCosts - dateCost.P6Costs);
                                    }
                                    else
                                        uncommittedPOValues += (currentDateCellValue - dateCost.CommittedCosts - dateCost.P6Costs);
                                }
                            }
            }

            //flag procurement jobs as error when uncommitted values on dates doesn't add up to outstanding POs
            if(job.IsProcurement)
            {
                decimal differences = Math.Round(job.Outstanding) - Math.Round(job.PORemainingCosts);
                differences = Math.Abs(differences);

                if (differences <= 10)
                    job.IsPOError = false;
                else
                    job.IsPOError = true;
            }

            job.Uncommitted = uncommittedPOValues + uncommitedP6Values + job.P6RemainingCosts;
            if (job.CurrentProductivity > 0)
                job.CurrentUncommitted = uncommittedPOValues + (job.P6RemainingCosts / job.CurrentProductivity);
            else
                job.CurrentUncommitted = job.Uncommitted;

            job.OriginalUncommitted = uncommittedPOValues + job.P6RemainingCosts;
        }

        public bool CanSaveCurrentMonthEAC()
        {
            return isCompletelyLoaded;
        }

        public void SaveCurrentMonthEAC()
        {
            if (!LoginCredentials.hasPermission(PermissionResources.SaveEAC))
            {
                MessageBoxService.ShowMessage("You are not authorised to use this function", "Not Authorised", MessageButton.OK, MessageIcon.Exclamation);
                return;
            }

            List<ForecastJobData> jobs = getJobDataFromDatatable();
            if(jobs.Any(x => x.IsPOError))
            {
                MessageBoxService.ShowMessage("Some PO forecast aren't completed yet or misaligned, please go to PO forecast module and click Align Actuals to fix all issues", "Error", MessageButton.OK, MessageIcon.Exclamation);
                return;
            }

            removeProjectEACOnDate(FixedDataDateMonthEnd);

            LoadingScreenManager.ShowLoadingScreen(DataPointsTable.Rows.Count);
            foreach (DataRow masterRow in DataPointsTable.Rows)
            {
                ForecastJobData entity = (ForecastJobData)masterRow[columnEntity];
                if (entity.EstimateAtCompletion > 0)
                {
                    LoadingScreenManager.SetMessage("Adding EAC for Job: " + entity.ToString());
                    findExistingOrAddNewEAC(entity.Projection, FixedDataDateMonthEnd, entity.EstimateAtCompletion);
                }

                LoadingScreenManager.Progress();
            }

            LoadingScreenManager.CloseLoadingScreen();
            LoadingScreenManager.ShowLoadingScreen(1);
            LoadingScreenManager.SetMessage("Saving changes...");
            bluePrintsUnitOfWork.SaveChanges();
            LoadingScreenManager.CloseLoadingScreen();
            MessageBoxService.ShowMessage("EAC for data date: " + FixedDataDateMonthEnd.ToString(BluePrintsResources.ColumnDateFormat) + " is saved\nData date will be changed to next month after closing this dialog", "EAC Saved", MessageButton.OK, MessageIcon.Information);
            FixedDataDate = FixedDataDateMonthEnd.AddMonths(1);
            LoadDataDate = FixedDataDate;
            SaveDateAndRefresh();
        }

        public bool CanUndo()
        {
            if (EntitiesUndoRedoManager == null)
                return false;

            return EntitiesUndoRedoManager.CanUndo();
        }

        public bool CanRedo()
        {
            if (EntitiesUndoRedoManager == null)
                return false;

            return EntitiesUndoRedoManager.CanRedo();
        }

        public void Undo()
        {
            EntitiesUndoRedoManager.Undo();
        }

        public void Redo()
        {
            EntitiesUndoRedoManager.Redo();
        }

        //Unstable
        public void ExpandAllMasterRows()
        {
            GridControlService.ExpandAllMasterRows();
        }

        public void CollapseAllMasterRows()
        {
            GridControlService.CollapseAllMasterRows();
        }

        public void EditValueChanged(EditValueChangedEventArgs e)
        {
            if (IsLoadingForecast)
                return;

            if (MainViewModel == null || LoadPROJECT == null || ForecastSummary == null || e.NewValue == null)
                return;

            decimal newValueDecimal = 0;
            decimal.TryParse(e.NewValue.ToString(), out newValueDecimal);
            string fieldName = ((BaseEdit)e.OriginalSource).Tag.ToString();
            DataUtils.TrySetNestedValue(fieldName, LoadPROJECT, newValueDecimal);
            savePROJECT();

            if (fieldName == BindableBase.GetPropertyName(() => new Data.PROJECT().ORI_REVENUE))
                ForecastSummary.Original_Revenue = newValueDecimal;
            else if (fieldName == BindableBase.GetPropertyName(() => new Data.PROJECT().VAR_REVENUE))
                ForecastSummary.Approved_Var_Revenue = newValueDecimal;
            else if (fieldName == BindableBase.GetPropertyName(() => new Data.PROJECT().EAC_REVENUE))
                ForecastSummary.EAC_Revenue = newValueDecimal;

            this.RaisePropertyChanged(x => x.ForecastSummary);
        }

        private void savePROJECT()
        {
            delayedProjectSaveTimer.Tick -= DelayedProjectSaveTimer_Tick;
            delayedProjectSaveTimer.Tick += DelayedProjectSaveTimer_Tick;

            delayedProjectSaveTimer.Start();
        }

        private void DelayedProjectSaveTimer_Tick(object sender, EventArgs e)
        {
            delayedProjectSaveTimer.Stop();
            projectSavingBackgroundWorker.RunWorkerAsync();
        }

        private void ProjectSavingBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //when view is closed halfway
            if (PROJECTCollectionViewModel != null)
                PROJECTCollectionViewModel.Save(LoadPROJECT);
        }

        /// <summary>
        /// Manages all undo and redo operation
        /// </summary>
        private EntitiesUndoRedoManager<DataRow> entitiesundoredomanager { get; set; }

        public EntitiesUndoRedoManager<DataRow> EntitiesUndoRedoManager
        {
            get
            {
                if (entitiesundoredomanager == null)
                    entitiesundoredomanager = new EntitiesUndoRedoManager<DataRow>(BulkPropertyUndo, BulkPropertyRedo);

                return entitiesundoredomanager;
            }
        }

        /// <summary>
        /// Function to undo the entity changes
        /// Must be used in conjunction of EntitiesUndoManager
        /// </summary>
        /// <param name="entityProperty">Entity passed over from EntitiesUndoRedo</param>
        public virtual void BulkPropertyUndo(IEnumerable<UndoRedoEntityInfo<DataRow>> entityProperties)
        {
            IEnumerable<UndoRedoEntityInfo<DataRow>> bulkSaveProperties = entityProperties.Where(x => x.MessageType == EntityMessageType.Changed);
            foreach (UndoRedoEntityInfo<DataRow> entityProperty in bulkSaveProperties)
            {
                object oldValue = entityProperty.OldValue;
                if (oldValue == null || oldValue == DBNull.Value)
                {
                    resetViewRemainingOnJob(entityProperty.ChangedEntity, entityProperty.PropertyName, false);
                    //oldValue = 0.00m;
                }
                else
                {
                    //do this twice so that detailed grid value can be updated (hack)
                    entityProperty.ChangedEntity[entityProperty.PropertyName] = oldValue;
                    entityProperty.ChangedEntity[entityProperty.PropertyName] = oldValue;
                }

                DateTime parseDateTime;
                if (DateTime.TryParse(entityProperty.PropertyName, out parseDateTime))
                {
                    decimal? oldValueDecimal = null;
                    if (entityProperty.OldValue != null)
                        oldValueDecimal = (decimal)entityProperty.OldValue;
                    findExistingOrAddNewForecast(entityProperty.ChangedEntity, parseDateTime, oldValueDecimal);
                }
            }

            foreach(UndoRedoEntityInfo<DataRow> entityProperty in bulkSaveProperties)
            {
                updateTotalUncommittedOnJob(entityProperty.ChangedEntity, true);
            }

            refreshGridData();
        }

        /// <summary>
        /// Function to redo the entity changes
        /// Must be used in conjunction of EntitiesUndoManager
        /// </summary>
        /// <param name="entityProperty">Entity passed over from EntitiesUndoRedo</param>
        public virtual void BulkPropertyRedo(IEnumerable<UndoRedoEntityInfo<DataRow>> entityProperties)
        {
            IEnumerable<UndoRedoEntityInfo<DataRow>> bulkSaveProperties = entityProperties.Where(x => x.MessageType == EntityMessageType.Changed);
            foreach (UndoRedoEntityInfo<DataRow> entityProperty in bulkSaveProperties)
            {
                object newValue = entityProperty.NewValue;
                if (newValue == null || newValue == DBNull.Value)
                {
                    resetViewRemainingOnJob(entityProperty.ChangedEntity, entityProperty.PropertyName, false);
                    //newValue = 0.00m;
                }
                else
                {
                    //do this twice so that detailed grid value can be updated (hack)
                    entityProperty.ChangedEntity[entityProperty.PropertyName] = newValue;
                    entityProperty.ChangedEntity[entityProperty.PropertyName] = newValue;
                }

                DateTime parseDateTime;
                if (DateTime.TryParse(entityProperty.PropertyName, out parseDateTime))
                {
                    decimal? newValueDecimal = null;
                    if (entityProperty.NewValue != DBNull.Value && entityProperty.NewValue != null)
                        newValueDecimal = (decimal)entityProperty.NewValue;
                    findExistingOrAddNewForecast(entityProperty.ChangedEntity, parseDateTime, newValueDecimal);
                }
            }

            foreach (UndoRedoEntityInfo<DataRow> entityProperty in bulkSaveProperties)
            {
                updateTotalUncommittedOnJob(entityProperty.ChangedEntity, true);
            }

            refreshGridData();
        }

        public void CopyDetailWithHeader()
        {
            DetailGridControlService.CopyWithHeader();
        }

        ObservableCollection<DataRowView> selectedDataRows { get; set; }
        public ObservableCollection<DataRowView> SelectedDataRows
        {
            get
            {
                return selectedDataRows;
            }
            set
            {
                selectedDataRows = value;
            }
        }

        private DevExpress.Mvvm.IDialogService DistributionDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("DistributionDialogService"); }
        }

        public bool CanDistributeUnits(object parameter)
        {
            if (!isCompletelyLoaded)
                return false;

            GridControl gridControl = (GridControl)parameter;
            TableView tableView = gridControl.View as TableView;
            var selected_cells = tableView.GetSelectedCells();
            if (selected_cells.Count == 0)
            {
                selected_cells = Enumerable.Range(0, gridControl.VisibleRowCount)
                .Select(x => (GridControl)gridControl.GetDetail(x))
                .Where(x => x != null).
                SelectMany(x => ((TableView)(x).View).GetSelectedCells()).ToList();

                if (selected_cells.Count == 0)
                    return false;
                else
                {
                    if (selected_cells.First().Column == null)
                        return false;

                    tableView = (TableView)selected_cells.First().Column.View;
                    gridControl = tableView.Grid;
                }
            }

            return true;
        }

        public void DistributeUnits(object parameter)
        {
            GridControl gridControl = (GridControl)parameter;
            TableView tableView = gridControl.View as TableView;
            var selected_cells = tableView.GetSelectedCells();
            if (selected_cells.Count == 0)
            {
                selected_cells = Enumerable.Range(0, gridControl.VisibleRowCount)
                .Select(x => (GridControl)gridControl.GetDetail(x))
                .Where(x => x != null).
                SelectMany(x => ((TableView)(x).View).GetSelectedCells()).ToList();

                if (selected_cells.Count == 0)
                    return;
                else
                {
                    tableView = (TableView)selected_cells.First().Column.View;
                    gridControl = tableView.Grid;
                }
            }

            foreach (var selectedCell in selected_cells)
            {
                var gridColumn = gridControl.Columns[selectedCell.Column.FieldName];
                if (gridColumn == null || gridColumn.ReadOnly)
                {
                    MessageBoxService.ShowMessage("Your selection contains read only cell, please revise your selection");
                    return;
                }
            }

            var distributionSelectViewModel = DistributionSelectViewModel.Create(gridControl, selected_cells);
            if (DistributionDialogService.ShowDialog(MessageButton.OKCancel, "Select distribution method", "DistributionSelect", distributionSelectViewModel) == MessageResult.OK)
            {
                string newValueString = distributionSelectViewModel.ConvertToPasteData();
                string[] RowData = DataUtils.ExcelSplit(newValueString).ToArray();
                pasteCellData(gridControl, tableView, RowData);
            }
        }

        public bool CanCreateExportSheet()
        {
            return ExportTable != null && FixedDataDate != null;
        }

        public void CreateExportSheet()
        {
            ExportTableViewService.ApplyBestFit();
            string ResultPath = string.Empty;
            if (FolderBrowserDialogService.ShowDialog())
            {
                ResultPath = FolderBrowserDialogService.ResultPath;
                DateTime exportDate = (DateTime)FixedDataDate;
                MemoryStream stream = new MemoryStream();

                //copy group configuration
                GridColumnCollection exportGridColumns = ExportGridControlService.GridColumns();
                GridColumnCollection defaultGridColumns = GridControlService.GridColumns();

                foreach (GridColumn gridColumn in defaultGridColumns)
                {
                    string fieldName = gridColumn.FieldName;
                    if (exportGridColumns.Any(x => x.FieldName == fieldName))
                    {
                        exportGridColumns[fieldName].GroupIndex = gridColumn.GroupIndex;
                    }
                }

                if (ExportTableViewService.ExportToXls(stream))
                {
                    using (SpreadsheetControl spreadsheetControl = new SpreadsheetControl())
                    {
                        System.Drawing.Color editableColor = System.Drawing.Color.Yellow;
                        string costFormat = "$#,###,###";
                        string numberFormat = "#,###,###";
                        spreadsheetControl.LoadDocument(stream);
                        Worksheet ws = spreadsheetControl.Document.Worksheets[0];
                        DevExpress.Spreadsheet.Range usedRange = ws.GetUsedRange();
                        ws["$A:$XFD"].Protection.Locked = false;
                        for (int rowIndex = 0; rowIndex < usedRange.RowCount; rowIndex++)
                        {
                            bool isReadOnly = false;
                            Cell commodityCell = usedRange[rowIndex, spreadSheetCommodityIndex];
                            if (commodityCell.Value.IsEmpty)
                                isReadOnly = true;

                            Cell phaseCell = usedRange[rowIndex, spreadSheetPhaseIndex];
                            Cell areaCell = usedRange[rowIndex, spreadSheetAreaIndex];
                            Cell subAreaCell = usedRange[rowIndex, spreadSheetSubAreaIndex];
                            Cell subJobCell = usedRange[rowIndex, spreadSheetSubJobIndex];
                            Cell subJobTitleCell = usedRange[rowIndex, spreadSheetSubJobTitleIndex];
                            Cell variationCell = usedRange[rowIndex, spreadSheetVariationIndex];
                            Cell disciplineCell = usedRange[rowIndex, spreadSheetDisciplineIndex];
                            Cell disciplineNameCell = usedRange[rowIndex, spreadSheetDisciplineNameIndex];
                            Cell commodityNameCell = usedRange[rowIndex, spreadSheetCommodityNameIndex];
                            Cell commodityDescriptionCell = usedRange[rowIndex, spreadSheetCommodityDescriptionIndex];
                            Cell commodityUOMCell = usedRange[rowIndex, spreadSheetCommodityUOMIndex];
                            Cell budgetCell = usedRange[rowIndex, spreadSheetBudgetIndex];
                            Cell rateCell = usedRange[rowIndex, spreadSheetRateIndex];

                            budgetCell.NumberFormat = costFormat;
                            rateCell.NumberFormat = costFormat;
                            ws[phaseCell.GetReferenceA1()].Protection.Locked = true;
                            ws[areaCell.GetReferenceA1()].Protection.Locked = true;
                            ws[subAreaCell.GetReferenceA1()].Protection.Locked = true;
                            ws[subJobCell.GetReferenceA1()].Protection.Locked = true;
                            ws[subJobTitleCell.GetReferenceA1()].Protection.Locked = true;
                            ws[variationCell.GetReferenceA1()].Protection.Locked = true;
                            ws[disciplineCell.GetReferenceA1()].Protection.Locked = true;
                            ws[disciplineNameCell.GetReferenceA1()].Protection.Locked = true;
                            ws[commodityCell.GetReferenceA1()].Protection.Locked = true;
                            ws[commodityNameCell.GetReferenceA1()].Protection.Locked = true;
                            ws[commodityDescriptionCell.GetReferenceA1()].Protection.Locked = true;
                            ws[commodityUOMCell.GetReferenceA1()].Protection.Locked = true;

                            string uomFormat = numberFormat;
                            if (!commodityUOMCell.Value.IsEmpty)
                                uomFormat = numberFormat + @" """ + commodityUOMCell.Value.TextValue + @"""";

                            if (isReadOnly)
                            {
                                ws[budgetCell.GetReferenceA1()].Protection.Locked = true;
                                ws[rateCell.GetReferenceA1()].Protection.Locked = true;
                            }
                            else
                            {
                                budgetCell.FillColor = editableColor;
                                rateCell.FillColor = editableColor;

                                for (int columnIndex = spreadSheetDateStartIndex; columnIndex < usedRange.ColumnCount; columnIndex++)
                                {
                                    DateTime columnDate;
                                    Cell dateCell = usedRange[0, columnIndex];
                                    if (!DateTime.TryParse(dateCell.Value.TextValue, out columnDate))
                                        continue;

                                    Cell currentCell = usedRange[rowIndex, columnIndex];
                                    currentCell.FillColor = editableColor;
                                    currentCell.NumberFormat = uomFormat;
                                }
                            }
                        }

                        string exportPath = ResultPath + "\\" + exportDate.Year + "-" + exportDate.ToString("MMM") + "_" + LoadPROJECT.NUMBER + "_Forecast" + ".xlsx";
                        try
                        {
                            
                            ws.Protect("", WorksheetProtectionPermissions.Default | WorksheetProtectionPermissions.FormatColumns | WorksheetProtectionPermissions.PivotTables | WorksheetProtectionPermissions.Sort | WorksheetProtectionPermissions.AutoFilters | WorksheetProtectionPermissions.SelectLockedCells | WorksheetProtectionPermissions.SelectUnlockedCells | WorksheetProtectionPermissions.FormatCells | WorksheetProtectionPermissions.FormatRows);

                            spreadsheetControl.Options.Behavior.Group.CollapseExpandOnProtectedSheet = DevExpress.XtraSpreadsheet.DocumentCapability.Enabled;
                            spreadsheetControl.Options.Behavior.Group.CollapseExpandOnReadOnlyControl = DevExpress.XtraSpreadsheet.DocumentCapability.Enabled;
                            spreadsheetControl.SaveDocument(exportPath);
                            Process.Start(exportPath);
                        }
                        catch
                        {
                            MessageBoxService.ShowMessage("Export failed because the file is in use", "Warning", MessageButton.OK, MessageIcon.Warning);
                        }
                    }
                }
                else
                    MessageBoxService.ShowMessage("Export failed because the file is in use", "Warning", MessageButton.OK, MessageIcon.Information);
            }
        }

        public override void ExportToExcel()
        {
            skipDashboardExcelFomatting = true;
            exportFileName = LoadPROJECT.NUMBER + "_" + "Forecast_" + ((DateTime)FixedDataDate).ToString("yyyyMMdd");
            base.ExportToExcel();
        }
        #endregion

        protected override void OnClose(CancelEventArgs e)
        {
            GlobalMethods.SetAccordionExpandedState?.Invoke(true);
            base.OnClose(e);
        }

        #region Entity Wrapper Properties
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

        public IEnumerable<Data.PHASE> PHASECollection
        {
            get
            {
                return GetEntities<Data.PHASE>();
            }
        }

        public IEnumerable<DISCIPLINE> DISCIPLINECollection
        {
            get
            {
                return GetEntities<DISCIPLINE>();
            }
        }

        public IEnumerable<DOCTYPE> DOCTYPECollection
        {
            get
            {
                return GetEntities<DOCTYPE>();
            }
        }

        public IEnumerable<JOB_COSTTYPES> JOB_COSTTYPESCollection
        {
            get
            {
                return GetEntities<JOB_COSTTYPES>();
            }
        }

        public CollectionViewModel<FORECAST, FORECAST, Guid, IBluePrintsEntitiesUnitOfWork> FORECASTCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return
                    (CollectionViewModel<FORECAST, FORECAST, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<FORECAST>();
            }
        }

        public IEnumerable<FORECAST_PO> FORECAST_POCollection
        {
            get
            {
                return GetEntities<FORECAST_PO>();
            }
        }

        public IEnumerable<FORECAST_JOB> FORECAST_JOBCollection
        {
            get
            {
                return GetEntities<FORECAST_JOB>();
            }
        }

        public IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection
        {
            get
            {
                return GetEntities<COMMODITY_CODE>();
            }
        }

        public IEnumerable<FORECAST_JOB_SETTING> FORECAST_JOB_SETTINGCollection
        {
            get
            {
                return GetEntities<FORECAST_JOB_SETTING>();
            }
        }

        public IEnumerable<RATE> RATECollection
        {
            get
            {
                return GetEntities<RATE>();
            }
        }

        protected ObservableCollection<ColumnDescriptor> parentViewColumns;
        public ObservableCollection<ColumnDescriptor> ParentViewColumns
        {
            get
            {
                if (parentViewColumns == null)
                {
                    parentViewColumns = new ObservableCollection<ColumnDescriptor>();
                }
                return parentViewColumns;
            }
        }

        protected ObservableCollection<ColumnDescriptor> childViewColumns;
        public ObservableCollection<ColumnDescriptor> ChildViewColumns
        {
            get
            {
                if (childViewColumns == null)
                {
                    childViewColumns = new ObservableCollection<ColumnDescriptor>();
                }
                return childViewColumns;
            }
        }

        protected ObservableCollection<SummaryDescriptor> parentSummaries;
        public ObservableCollection<SummaryDescriptor> ParentSummaries
        {
            get
            {
                if (parentSummaries == null)
                {
                    parentSummaries = new ObservableCollection<SummaryDescriptor>();
                }
                return parentSummaries;
            }
        }

        protected ObservableCollection<SummaryDescriptor> childSummaries;
        public ObservableCollection<SummaryDescriptor> ChildSummaries
        {
            get
            {
                if (childSummaries == null)
                {
                    childSummaries = new ObservableCollection<SummaryDescriptor>();
                }
                return childSummaries;
            }
        }

        public CollectionViewModel<FORECAST_JOB_SETTING, FORECAST_JOB_SETTING, Guid, IBluePrintsEntitiesUnitOfWork> FORECAST_JOB_SETTINGCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<FORECAST_JOB_SETTING, FORECAST_JOB_SETTING, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<FORECAST_JOB_SETTING>();
            }
        }

        public CollectionViewModel<VARIATION_REGISTER, VARIATION_REGISTER, Guid, IBluePrintsEntitiesUnitOfWork> VARIATION_REGISTERCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<VARIATION_REGISTER, VARIATION_REGISTER, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<VARIATION_REGISTER>();
            }
        }

        public override string ViewName => "PROJECTForecastView_v1.00";

        public void LoadLayout()
        {
            PersistentLayoutHelper.TryDeserializeLayout(LayoutSerializationService, ViewName);
        }
        #endregion
    }

    /// <summary>
    /// Currently unused
    /// </summary>
    public class CostFilteringViewModel
    {
        [FilterLookup("Subjobs", UseBlanks = false, UseSelectAll = false)]
        [Display(Name = "Subjob")]
        public string Subjob_Name { get; set; }
        [FilterRange("StartSelectionDate", "EndSelectionDate")]
        [Display(Name = "Actual Date")]
        public DateTime ActualDate { get; set; }
    }

    public class ForecastSummary
    {
        /// <summary>
        /// Reset all settable figures to 0
        /// </summary>
        public void Reset()
        {
            Budget_Cost = 0;
            Current_Cost = 0;
            Uncommitted_Forecast = 0;
            EstimateAtCompletion = 0;
            //TotalClaims = 0;

            OriginalEstimateAtCompletion = 0;
            CurrentEstimateAtCompletion = 0;
        }

        public decimal Original_Revenue { get; set; }
        public decimal Approved_Var_Revenue { get; set; }
        public decimal Revised_Revenue => Original_Revenue + Approved_Var_Revenue;
        public decimal Budget_Cost { get; set; }
        public decimal Budget_Margin => Revised_Revenue - Budget_Cost;
        public decimal Budget_Margin_Percent => Revised_Revenue == 0 ? 0 : Budget_Margin / Revised_Revenue;

        public decimal EAC_Revenue { get; set; }
        public decimal Current_Cost { get; set; }
        public decimal Commitments { get; set; }
        public decimal Uncommitted_Forecast { get; set; }
        public decimal OriginalEstimateAtCompletion { get; set; }
        public decimal EstimateAtCompletion { get; set; }
        public decimal CurrentEstimateAtCompletion { get; set; }
        public decimal OriginalEAC_Margin => EAC_Revenue - OriginalEstimateAtCompletion;
        public decimal EAC_Margin => EAC_Revenue - EstimateAtCompletion;
        public decimal CurrentEAC_Margin => EAC_Revenue - CurrentEstimateAtCompletion;
        public decimal OriginalEAC_Margin_Percent => EAC_Revenue == 0 ? 0 : OriginalEAC_Margin / EAC_Revenue;
        public decimal EAC_Margin_Percent => EAC_Revenue == 0 ? 0 : EAC_Margin / EAC_Revenue;
        public decimal CurrentEAC_Margin_Percent => EAC_Revenue == 0 ? 0 : CurrentEAC_Margin / EAC_Revenue;

        public decimal TotalClaims { get; set; }
        public decimal UnderOverClaim => TotalClaims - Current_Cost;

        public SolidColorBrush Budget_Margin_Background => Budget_Margin > 0 ? new SolidColorBrush(Colors.Chartreuse) : new SolidColorBrush(Colors.LightSalmon);
        public SolidColorBrush Budget_Margin_Percent_Background => Budget_Margin_Percent > 0 ? new SolidColorBrush(Colors.Chartreuse) : new SolidColorBrush(Colors.LightSalmon);
        public SolidColorBrush OriginalEAC_Margin_Background => OriginalEAC_Margin > 0 ? new SolidColorBrush(Colors.Chartreuse) : new SolidColorBrush(Colors.LightSalmon);
        public SolidColorBrush EAC_Margin_Background => EAC_Margin > 0 ? new SolidColorBrush(Colors.Chartreuse) : new SolidColorBrush(Colors.LightSalmon);
        public SolidColorBrush CurrentEAC_Margin_Background => CurrentEAC_Margin > 0 ? new SolidColorBrush(Colors.Chartreuse) : new SolidColorBrush(Colors.LightSalmon);
        public SolidColorBrush OriginalEAC_Margin_Percent_Background => OriginalEAC_Margin_Percent > 0 ? new SolidColorBrush(Colors.Chartreuse) : new SolidColorBrush(Colors.LightSalmon);
        public SolidColorBrush EAC_Margin_Percent_Background => EAC_Margin_Percent > 0 ? new SolidColorBrush(Colors.Chartreuse) : new SolidColorBrush(Colors.LightSalmon);
        public SolidColorBrush CurrentEAC_Margin_Percent_Background => CurrentEAC_Margin_Percent > 0 ? new SolidColorBrush(Colors.Chartreuse) : new SolidColorBrush(Colors.LightSalmon);
        public SolidColorBrush UnderOverClaim_Background => UnderOverClaim > 0 ? new SolidColorBrush(Colors.Chartreuse) : new SolidColorBrush(Colors.LightSalmon);
    }
}