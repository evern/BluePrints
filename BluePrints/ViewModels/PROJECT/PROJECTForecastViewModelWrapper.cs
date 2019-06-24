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

        }

        protected override void addEntitiesLoader()
        {
            base.addEntitiesLoader();
            //need to reassign project because forecast dates information on project might changed since navigation is loaded since loadPROJECT comes from navigation
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.FORECASTS, FORECASTProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.VARIATION_REGISTERS, VARIATION_REGISTERProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, COMMODITY_CODEProjectionFunc);
            loaderCollection.AddLoaderDescription(p6UnitOfWorkFactory, x => x.PROJWBS, P6PROJECTProjectionFunc);
        }

        private Func<IRepositoryQuery<PROJWBS>, IQueryable<PROJWBS>> P6PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.proj_node_flag == "Y" && x.wbs_short_name.Contains(loadPROJECT.NUMBER)).OrderBy(proj => proj.wbs_short_name);
        }

        private Func<IRepositoryQuery<FORECAST>, IQueryable<FORECAST>> FORECASTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<Data.PROJECT>, IQueryable<Data.PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<VARIATION_REGISTER>, IQueryable<VARIATION_REGISTER>> VARIATION_REGISTERProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected virtual Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>> COMMODITY_CODEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == null);
        }

        public bool IsLoadingForecast { get; set; }
        public bool IsHidden { get; set; }
        public ForecastSummary ForecastSummary { get; set; }
        public CriteriaOperator FilterCriteria { get; set; }
        public virtual DateTime EndSelectionDate { get; set; }
        public virtual DateTime StartSelectionDate { get; set; }
        public virtual IEnumerable<string> Subjobs { get; set; }
        protected List<ExoTimeAuthorisation> queryJobLines { get; set; }
        protected JOBCOST_HDR masterJob;
        protected JOBCOST_LINES copyLine;
        IPrimeroEntitiesUnitOfWork primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
        IP6EntitiesUnitOfWork p6UnitOfWork = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
        IEnumerable<ExoSubJobProjection> queryJobs;
        List<string> hiddenColumnFieldNames = new List<string>();
        List<DateTime> alignedDataDateCollection;
        protected virtual IGridControlService DetailGridControlService { get { return this.GetService<IGridControlService>("DetailGridControlService"); } }
        protected virtual IGridControlService ExportGridControlService { get { return this.GetService<IGridControlService>("ExportGridControlService"); } }
        protected virtual ITableViewService ExportTableViewService { get { return this.GetService<ITableViewService>("ExportTableViewService"); } }
        IBluePrintsEntitiesUnitOfWork bluePrintsUnitOfWork;

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

        protected override void resolveParameters(object parameter)
        {
            base.resolveParameters(parameter);
            bluePrintsUnitOfWork = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            ForecastSummary = new ForecastSummary();
            forceRetrieveAllBurned = true; //force exo burned to retrieve subjobs that aren't defined
            useProductivityFactorOnRemaining = true; //calculate remaining costs using productivity factor
            IsLoadingForecast = true;
            LoadingScreenManager.DisableLoadingScreen = false;
            skipBindingSwitch = true;
            hiddenColumnFieldNames.Add(columnEntity);
            hiddenColumnFieldNames.Add(columnChild);
            SelectedDataRows = new ObservableCollection<DataRowView>();
            StartSelectionDate = DateTime.Now;
            DetailedData = new List<ExoDataPoint>();
            alignedDataDateCollection = new List<DateTime>();
            IsHidden = true;
            delayPostLoadedTimer = true;
            //isExcelExportDataAware = false;
            isVariationSeparated = true;
            showStatsBuildingLoadingScreen = true;
            this.RaisePropertiesChanged();
        }

        private void loadExoMethodsData()
        {
            IPrimeroEntitiesUnitOfWork primeroEntitiesUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            masterJob = ExoQueries.GetProjectSubJob(primeroEntitiesUnitOfWork, loadPROJECT.NUMBER, loadPROJECT.NUMBER);
            copyLine = ExoQueries.GetAnyProjectLineByJobNumber(primeroEntitiesUnitOfWork, loadPROJECT.NUMBER);
        }

        private void loadSummaryStats()
        {
            IPrimeroEntitiesUnitOfWork primeroEntitiesUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            List<ExoTimeAuthorisation> jobLines = new List<ExoTimeAuthorisation>(); 
            queryJobs = ExoQueries.GetNativeExoSubJobProjection(primeroEntitiesUnitOfWork, loadPROJECT, ref jobLines);
            queryJobLines = jobLines;

            dynamic revenueLine = ExoQueries.GetProjectRevenue(primeroEntitiesUnitOfWork, loadPROJECT.NUMBER);
            if (revenueLine != null)
                ForecastSummary.Revenue = Convert.ToDecimal(revenueLine.BUDGETED_REV);

            ForecastSummary.TotalClaims = ExoQueries.GetProjectClaims(primeroEntitiesUnitOfWork, loadPROJECT.NUMBER);
        }

        protected override List<StatsCalculationType> getForecastTypes()
        {
            List<StatsCalculationType> calcTypes = new List<StatsCalculationType>();
            calcTypes.Add(StatsCalculationType.Forecast);

            return calcTypes;
        }

        public override DateTime? FixedStartDate
        {
            get
            {
                //do this to prevent binding errors
                if (liveDesignProgress == null || loadPROJECT == null)
                    return DateTime.Now;

                return loadPROJECT.FORECAST_DATA_DATE == null ? liveDesignProgress.DATA_DATE : (DateTime)loadPROJECT.FORECAST_DATA_DATE;
                //return loadPROJECT.FORECAST_START_DATE == null ? liveDesignProgress.PROGRESS_START : (DateTime)loadPROJECT.FORECAST_START_DATE;
            }
            set
            {
                if (isCompletelyLoaded)
                {
                    loadPROJECT.FORECAST_START_DATE = value;
                    PROJECTCollectionViewModel.Save(loadPROJECT);
                    this.RaisePropertyChanged(x => x.FixedStartDate);
                }
            }
        }

        public DateTime FixedDataDateMonthEnd => ((DateTime)FixedDataDate).AddMonths(1).AddDays(-1);

        public override DateTime? FixedDataDate
        {
            get
            {
                //do this to prevent binding errors
                if (liveDesignProgress == null || loadPROJECT == null || loadPROJECT.FORECAST_DATA_DATE == null)
                    return DateTime.Now;

                return loadPROJECT.FORECAST_DATA_DATE;
            }
            set
            {
                if (isCompletelyLoaded)
                {
                    loadPROJECT.FORECAST_DATA_DATE = value;
                    PROJECTCollectionViewModel.Save(loadPROJECT);
                    this.RaisePropertyChanged(x => x.FixedDataDate);
                }
            }
        }

        public DateTime FixedEndDate
        {
            get
            {
                //do this to prevent binding errors
                if (liveDesignProgress == null || loadPROJECT == null || loadPROJECT.FORECAST_END_DATE == null)
                    return DateTime.Now;

                return (DateTime)loadPROJECT.FORECAST_END_DATE;
            }
            set
            {
                if (isCompletelyLoaded)
                {
                    loadPROJECT.FORECAST_END_DATE = value;
                    PROJECTCollectionViewModel.Save(loadPROJECT);
                    this.RaisePropertyChanged(x => x.FixedDataDate);
                }
            }
        }

        public string P6ForecastProject
        {
            get
            {
                if (loadPROJECT == null)
                    return string.Empty;

                return loadPROJECT.P6FORECAST_NAME;
            }
            set
            {
                loadPROJECT.P6FORECAST_NAME = value;
                PROJECTCollectionViewModel.Save(loadPROJECT);
            }
        }

        public DateTime? P6DataDate
        {
            get
            {
                if (loadPROJECT == null)
                    return null;

                return loadPROJECT.P6FORECAST_DATADATE;
            }
        }

        public bool CanReloadP6Forecast()
        {
            return !IsLoadingForecast;
        }

        public async void ReloadP6Forecast()
        {
            IsLoadingForecast = true;
            this.RaisePropertyChanged(x => x.IsLoadingForecast);
            if (summaryBackgroundWorker != null)
                summaryBackgroundWorker.CancelAsync();

            //LoadingScreenManager.ShowLoadingScreen(1);
            await BluePrintsContextHelper.RefreshDeliverablesRemainingDataPointsByProject(loadPROJECT.NUMBER, true);
            //LoadingScreenManager.Progress();
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
            alignedDataDateCollection.Clear();
            DetailedData.Clear();
            EntitiesUndoRedoManager.Clear();
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
                this.RaisePropertyChanged(x => x.DataPointsTable);
                LoadingScreenManager.DisableLoadingScreen = false;
                LoadingScreenManager.ShowLoadingScreen(1);
                LoadingScreenManager.SetMessage("Applying Columns Best Fit...");
                postLoadedDispatcherTimer = new Timer();
                postLoadedDispatcherTimer.Interval = 10;
                postLoadedDispatcherTimer.Elapsed += post_loaded_dispatcher_timer_tick;
                postLoadedDispatcherTimer.Start();
            }
        }

        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            base.OnAfterAssignedCallbackAndRaisePropertyChanged();
            LoadingScreenManager.CloseLoadingScreen();
        }

        public override void FullRefresh()
        {
            dataPointsTable = null;
            ForecastSummary.Reset();
            loadSummaryStats();
            base.FullRefresh();
        }

        #region Data Points Table
        protected string columnEntity = "Entity";
        protected string columnChild = "ChildEntities";
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
        List<ForecastJobData> disciplineJobs;
        public virtual DataTable DataPointsTable
        {
            get
            {
                if (MainViewModel == null || AllProjectDashboards == null)
                    return null;

                if (dataPointsTable == null)
                {
                    dataPointsTable = new DataTable();
                    //get immutable data
                    List<ExoDataPoint> allDataPoints = new List<ExoDataPoint>();
                    List<ExoSubJobProjection> unifiedJobList = ForecastHelper.ConstructUnifiedJobList(queryJobs, AllProjectDashboards, COMMODITY_CODECollection, ref allDataPoints);
                    DetailedData.AddRange(allDataPoints);
                    alignedDataDateCollection = ChronologicalHelpers.GenerateMonthEndDatesCollection((DateTime)FixedStartDate, (DateTime)FixedEndDate);
                    disciplineJobs = ForecastHelper.CreateDisciplineProjections(unifiedJobList, queryJobLines, AllProjectDashboards, alignedDataDateCollection);

                    //construct data points table
                    dataPointsTable.Columns.Add(columnEntity, typeof(ForecastJobData));
                    dataPointsTable.Columns.Add(columnChild, typeof(DataTable));
                    foreach (DateTime alignedDataDate in alignedDataDateCollection)
                    {
                        string columnFieldName = alignedDataDate.Date.ToShortDateString();
                        dataPointsTable.Columns.Add(columnFieldName, typeof(decimal));
                    }

                    LoadingScreenManager.ShowLoadingScreen(disciplineJobs.Count);
                    LoadingScreenManager.SetMessage("Preparing View...");
                    foreach(ForecastJobData disciplineJob in disciplineJobs)
                    {
                        DataRow disciplineRow = dataPointsTable.NewRow();
                        disciplineRow[columnEntity] = disciplineJob;

                        foreach (ForecastDateCost dateCost in disciplineJob.DateCosts)
                        {
                            disciplineRow[dateCost.Date.ToShortDateString()] = dateCost.Cost;
                        }

                        DataTable commodityDataTable = dataPointsTable.Clone();
                        foreach (ForecastJobData commodityJob in disciplineJob.CommodityJobs)
                        {
                            DataRow commodityRow = commodityDataTable.NewRow();
                            commodityRow[columnEntity] = commodityJob;
                            foreach(ForecastDateCost dateCost in commodityJob.DateCosts)
                            {
                                commodityRow[dateCost.Date.ToShortDateString()] = dateCost.Cost;
                            }

                            ForecastSummary.Costs += commodityJob.Budget;
                            updateForecast(commodityRow);
                            calculateUncommitted(commodityRow);
                            commodityDataTable.Rows.Add(commodityRow);
                        }

                        disciplineRow[columnChild] = commodityDataTable;
                        recurseCalculateBudget(disciplineRow);

                        updateForecast(disciplineRow);
                        calculateUncommitted(disciplineRow);

                        //calculate project summary
                        ForecastSummary.Actuals += disciplineJob.Actuals;
                        ForecastSummary.Commitments += disciplineJob.Outstanding;

                        ForecastSummary.EstimateAtCompletion += disciplineJob.EstimateAtCompletion;
                        dataPointsTable.Rows.Add(disciplineRow);

                        LoadingScreenManager.Progress();
                    }

                    LoadingScreenManager.CloseLoadingScreen();
                    this.RaisePropertyChanged(x => x.ForecastSummary);
                    this.RaisePropertyChanged(x => x.ExportTable);
                    TableViewService.ScrollToLast();
                }

                return dataPointsTable;
            }
        }
        
        private void addExoSubJob(List<ExoSubJobProjection> combinedSubJobs, string subJobCode, string disciplineCode, string commodityCode, string variationCode,
            string subJobTitle = "", string disciplineName = "")
        {
            COMMODITY_CODE findCOMMODITY_CODE = COMMODITY_CODECollection.FirstOrDefault(x => x.CODE == commodityCode);
            string commodityCodeName = string.Empty;
            string commodityCodeDescription = string.Empty;
            string commodityCodeUOM = string.Empty;
            if (findCOMMODITY_CODE != null)
            {
                commodityCodeName = findCOMMODITY_CODE.NAME;
                commodityCodeDescription = findCOMMODITY_CODE.DESCRIPTION;
                commodityCodeUOM = findCOMMODITY_CODE.UOM;
            }

            if(subJobTitle == string.Empty)
            {
                ExoSubJobProjection findSubJobProjection = queryJobs == null ? null : queryJobs.FirstOrDefault(x => x.SubJob.Code == subJobCode);
                if (findSubJobProjection != null)
                    subJobTitle = findSubJobProjection.SubJob.Title;
            }

            if(disciplineName == string.Empty)
            {
                ExoSubJobProjection findDisciplineProjection = queryJobs == null ? null : queryJobs.FirstOrDefault(x => x.Discipline.Code == disciplineCode);
                if (findDisciplineProjection != null)
                    disciplineName = findDisciplineProjection.Discipline.Name;
            }

            if (!combinedSubJobs.Any(x => x.SubJob.Code == subJobCode && x.Discipline.Code == disciplineCode && x.Commodity.Code == commodityCode && x.Variation_Code == variationCode))
            {
                combinedSubJobs.Add(new ExoSubJobProjection() { SubJob = new PrimeroSubJob() { Code = subJobCode, Title = subJobTitle }, Discipline = new PrimeroDiscipline() { Code = disciplineCode, Name = disciplineName }, Commodity = new PrimeroCommodity() { Code = commodityCode, Name = commodityCodeName, Description = commodityCodeDescription, UOM = commodityCodeUOM }, Variation_Code = normalizeVariationCode(variationCode) });
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

        public List<ExoDataPoint> DetailedData { get; set; }
        private void updateDataRowForecast(ExoSubJobProjection disciplineEntity)
        {
            DataRow dataRow = findDataRow(disciplineEntity.SubJob.Code, disciplineEntity.Discipline.Code, disciplineEntity.Commodity.Code, disciplineEntity.Variation_Code);
            if (dataRow == null)
                return;

            updateForecast(dataRow);
        }

        private DataRow findDataRow(string subJobCode, string disciplineCode, string commodityCode, string variationCode)
        {
            if (commodityCode == string.Empty)
            {
                return (from DataRow dr in dataPointsTable.Rows
                           where (((ForecastJobData)dr[columnEntity])).Projection.SubJob.Code == subJobCode && (((ForecastJobData)dr[columnEntity])).Projection.Discipline.Code == disciplineCode && (((ForecastJobData)dr[columnEntity])).Projection.Variation_Code == variationCode
                        select dr).FirstOrDefault();
            }
            else
            {
                IEnumerable<DataTable> childTables = from DataRow dr in dataPointsTable.Rows
                                                     select (DataTable)dr[columnChild];

                IEnumerable<DataRow> childRowsCollection = childTables.SelectMany(x => x.Rows.Cast<DataRow>().ToArray());
                return (from DataRow dr in childRowsCollection
                        where (((ForecastJobData)dr[columnEntity])).Projection.SubJob.Code == subJobCode && (((ForecastJobData)dr[columnEntity])).Projection.Discipline.Code == disciplineCode && (((ForecastJobData)dr[columnEntity])).Projection.Commodity.Code == commodityCode && (((ForecastJobData)dr[columnEntity])).Projection.Variation_Code == variationCode
                        select dr).FirstOrDefault();
            }
        }

        private void setForecastCellNull(DataRow updateRow, ExoSubJobProjection entity, string fieldName)
        {
            DateTime dateTime;
            if(DateTime.TryParse(fieldName, out dateTime))
            {
                IEnumerable<DashboardFlatStructure> relevantDashboards;
                if(entity.Commodity.Code == string.Empty)
                    relevantDashboards = AllProjectDashboards.Where(x => x.SubjobCode == entity.SubJob.Code && x.DisciplineCode == entity.Discipline.Code);
                else
                    relevantDashboards = AllProjectDashboards.Where(x => x.SubjobCode == entity.SubJob.Code && x.DisciplineCode == entity.Discipline.Code && x.CommodityCode == entity.Commodity.Code);

                IEnumerable<Common.ViewModel.Reporting.DataPoint> dataPoints = relevantDashboards.Where(x => x.Stats != null && x.Stats.Remaining != null && x.Stats.Remaining.DataPoints != null).SelectMany(x => x.Stats.Remaining.DataPoints);
                IEnumerable<Common.ViewModel.Reporting.DataPoint> dateSpecificDataPoints = dataPoints.Where(x => x.ProgressDate.Date == dateTime);

                if(DataPointsTable.Columns.Contains(fieldName))
                {
                    decimal sumCost = dateSpecificDataPoints.Sum(x => x.Costs);
                    if (sumCost == 0)
                        updateRow[fieldName] = 0.0m;
                    else
                        updateRow[fieldName] = sumCost;
                }

                calculateUncommitted(updateRow);
            }
        }

        private void updateForecast(DataRow dataRow)
        {
            ForecastJobData job = (ForecastJobData)dataRow[columnEntity];
            IEnumerable<FORECAST> currentRowFORECASTS = FORECASTCollectionViewModel.Entities.Where(x => x.SUBJOB_CODE == job.Projection.SubJob.Code && x.DISCIPLINE_CODE == job.Projection.Discipline.Code && x.COMMODITY_CODE == job.Projection.Commodity.Code && x.VARIATION_CODE == job.Projection.Variation_Code && !x.IS_EAC);

            foreach (FORECAST currentRowFORECAST in currentRowFORECASTS)
            {
                string dateField = currentRowFORECAST.FORECAST_DATE.ToShortDateString();
                DateTime parseDate;
                if (DateTime.TryParse(dateField, out parseDate))
                {
                    DateTime? alignedDataDate = alignedDataDateCollection.OrderBy(x => x).FirstOrDefault(x => x.Date >= parseDate.Date);
                    if(alignedDataDate != null)
                    {
                        string alignedDateField = ((DateTime)alignedDataDate).ToShortDateString();
                        //put forecast history only on compare datatable
                        if (alignedDataDate > FixedDataDateMonthEnd)
                            if (dataPointsTable.Columns.Contains(alignedDateField))
                            {
                                if (currentRowFORECAST.FORECAST_UNITS != null)
                                    dataRow[alignedDateField] = currentRowFORECAST.FORECAST_UNITS;
                            }
                    }
                }
            }
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
                TableView tableView = (TableView)e.Source;
                TableViewHitInfo hi = ((TableView)e.Source).CalcHitInfo(e.OriginalSource as DependencyObject);
                RowData clickRowData = tableView.FocusedRowData;
                if(clickRowData.Row == null)
                {
                    GridControl masterGrid = tableView.Grid;
                    var selected_cells = Enumerable.Range(0, masterGrid.VisibleRowCount)
                    .Select(x => (GridControl)masterGrid.GetDetail(x))
                    .Where(x => x != null).
                    Select(x => ((TableView)(x).View).FocusedRowData).ToList();

                    clickRowData = selected_cells.FirstOrDefault();
                }

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

            if (gridColumn.ReadOnly)
            {
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
                else if (gridColumn.FieldName.ToUpper().Contains("ACTUALS"))
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
            }
            else
                IsHidden = true;

            this.RaisePropertyChanged(x => x.IsHidden);
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
                    e.Column.MaxWidth = 50;
                }
                else
                {
                    e.Column.ReadOnly = true;
                    e.Column.Fixed = FixedStyle.Left;
                }
            }
        }

        public void AutoGeneratingPercentageColumns(AutoGeneratingColumnEventArgs e)
        {
            if (!hiddenColumnFieldNames.Any(x => x == e.Column.FieldName))
            {
                GridControl gridControl = (GridControl)e.Source;
                DateTime parsedate;
                if (DateTime.TryParse(e.Column.FieldName, out parsedate))
                {
                    if(parsedate <= FixedDataDateMonthEnd)
                    {
                        e.Column.CellTemplate = Application.Current.Resources["forecastTemplatePast"] as DataTemplate;
                        e.Column.AllowEditing = DevExpress.Utils.DefaultBoolean.False;
                        e.Column.ReadOnly = true;
                    }
                    else
                        e.Column.CellTemplate = Application.Current.Resources["forecastTemplateFuture"] as DataTemplate;

                    GridControlService.AddSummary(e.Column.FieldName, SummaryItemType.Sum, "c0");
                    e.Column.FilterPopupMode = FilterPopupMode.CheckedList;
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

        public void AutoGeneratingChildPercentageColumns(AutoGeneratingColumnEventArgs e)
        {
            if (!hiddenColumnFieldNames.Any(x => x == e.Column.FieldName))
            {
                DateTime parsedate;
                if (DateTime.TryParse(e.Column.FieldName, out parsedate))
                {
                    if (parsedate <= FixedDataDateMonthEnd)
                    {
                        e.Column.CellTemplate = Application.Current.Resources["forecastTemplateChild"] as DataTemplate;
                        e.Column.AllowEditing = DevExpress.Utils.DefaultBoolean.False;
                        e.Column.ReadOnly = true;
                    }
                    else
                        e.Column.CellTemplate = Application.Current.Resources["forecastTemplateChild"] as DataTemplate;

                    e.Column.FilterPopupMode = FilterPopupMode.CheckedList;
                }
                else
                {
                    e.Column.ReadOnly = true;
                    e.Column.Fixed = FixedStyle.Left;
                }
            }
            else
            {
                e.Cancel = true;
            }
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

                GridControlService.RefreshData();
                e.Handled = true;
            }

        }


        private void pasteCellData(GridControl gridControl, TableView gridTableView, string[] RowData)
        {
            //EntitiesUndoRedoManager.Clear();
            EntitiesUndoRedoManager.PauseActionId();
            var selected_cells = gridTableView.GetSelectedCells();
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
                    gridTableView = (TableView)selected_cells.First().Column.View;
                    gridControl = gridTableView.Grid;
                }
            }

            List<List<string>> row_data = new List<List<string>>();
            foreach (var row in RowData)
            {
                string formatRow = row;
                //remove tab in front
                if (row.Substring(0, 1) == "\t")
                {
                    formatRow = row.Substring(1, row.Length - 1);
                }

                List<string> column_data = formatRow.Split('\t').ToList();
                row_data.Add(column_data);
            }

            var grouped_results = row_data
            .SelectMany(inner => inner.Select((item, index) => new { item, index }))
            .GroupBy(i => i.index, i => i.item)
            .Select(g => g.ToList())
            .ToList();

            var selected_cells_groupby_columns = selected_cells.GroupBy(x => x.Column.FieldName).Select(group => new { FieldName = group.Key, Cells = group.ToList() });
            if (grouped_results.Count == 0)
            {
                foreach (var selected_cell in selected_cells)
                {
                    int row_handle = selected_cell.RowHandle;
                    DataRowView editing_row_view = (DataRowView)gridControl.GetRow(row_handle);
                    DataRow editing_row = editing_row_view.Row;
                    basePasteData(editing_row, selected_cell.Column, string.Empty, false);
                }
            }
            else
            {
                GridCell first_selected_cell = selected_cells.First();
                GridCell last_selected_cell = selected_cells.Last();

                int first_row_handle = selected_cells.Min(x => x.RowHandle);
                int last_row_handle = selected_cells.Max(x => x.RowHandle);
                int first_row_visible_index = gridControl.GetRowVisibleIndexByHandle(first_row_handle);
                int last_row_visible_index = gridControl.GetRowVisibleIndexByHandle(last_row_handle);
                int numberOfSelectedRows = (last_row_visible_index - first_row_visible_index) + 1;
                int numberOfCopiedRows = grouped_results.First().Count;

                List<GridColumn> visible_columns = gridTableView.VisibleColumns.ToList();
                int first_column_visible_index = visible_columns.First(x => x.FieldName == first_selected_cell.Column.FieldName).VisibleIndex;
                int last_column_visible_index = visible_columns.First(x => x.FieldName == last_selected_cell.Column.FieldName).VisibleIndex;

                int numberOfSelectedColumns = (last_column_visible_index - first_column_visible_index) + 1;
                int numberOfCopiedColumns = grouped_results.Count;

                //commented out because not accurate during banded view
                //int first_column_visible_index = first_selected_cell.Column.VisibleIndex;

                int rowOffsetSelection = numberOfSelectedRows > numberOfCopiedRows ? numberOfSelectedRows : numberOfCopiedRows;
                int columnOffsetSelection = numberOfSelectedColumns > numberOfCopiedColumns ? numberOfSelectedColumns : numberOfCopiedColumns;

                int pasteValueRowOffset = 0;
                for (int rowOffset = 0; rowOffset < rowOffsetSelection; rowOffset++)
                {
                    int pasteValueColumnOffset = 0;
                    for (int columnOffset = 0; columnOffset < columnOffsetSelection; columnOffset++)
                    {
                        if (!visible_columns.Any(x => x.VisibleIndex == (first_column_visible_index + columnOffset)))
                            continue;

                        GridColumn current_column = visible_columns.First(x => x.VisibleIndex == (first_column_visible_index + columnOffset));
                        string columnValue = grouped_results[pasteValueColumnOffset][pasteValueRowOffset];

                        int current_row_visible_index = first_row_visible_index + rowOffset;
                        int current_row_handle = gridControl.GetRowHandleByVisibleIndex(current_row_visible_index);

                        object rowObject = gridControl.GetRow(current_row_handle);
                        if (rowObject == null)
                            continue;

                        DataRowView editing_row_view = (DataRowView)rowObject;
                        DataRow editing_row = editing_row_view.Row;
                        var gg = from c in editing_row.Table.Columns.Cast<DataColumn>()
                                select c.ColumnName;
                        
                        pasteValueColumnOffset += 1;
                        if (pasteValueColumnOffset >= grouped_results.Count)
                            pasteValueColumnOffset = 0;

                        basePasteData(editing_row, current_column, columnValue, false);
                    }

                    pasteValueRowOffset += 1;
                    if (pasteValueRowOffset >= grouped_results[pasteValueColumnOffset].Count)
                        pasteValueRowOffset = 0;
                }
            }

            EntitiesUndoRedoManager.UnpauseActionId();
        }

        private bool basePasteData(DataRow newRow, ColumnBase copyColumn, string pasteData, bool isNewRow)
        {
            ExoSubJobProjection entity = ((ForecastJobData)newRow[columnEntity]).Projection;
            
            if (copyColumn.FieldType == typeof(decimal))
            {
                var rgx = new Regex("[^0-9a-z\\.]");
                var cleanColumnString = rgx.Replace(pasteData, string.Empty);
                decimal decimal_value;
                if (decimal.TryParse(cleanColumnString, out decimal_value))
                {
                    if (!isNewRow)
                        EntitiesUndoRedoManager.AddUndo(newRow, copyColumn.FieldName, newRow[copyColumn.FieldName], decimal_value, EntityMessageType.Changed);

                    newRow[copyColumn.FieldName] = decimal_value;
                    DateTime columnDateTime;
                    if(DateTime.TryParse(copyColumn.FieldName, out columnDateTime))
                    {
                        findExistingOrAddNewForecast(newRow, entity, columnDateTime, decimal_value, false);
                    }
                }
                else
                {
                    if (!isNewRow)
                        EntitiesUndoRedoManager.AddUndo(newRow, copyColumn.FieldName, newRow[copyColumn.FieldName], DBNull.Value, EntityMessageType.Changed);

                    setForecastCellNull(newRow, entity, copyColumn.FieldName);
                    return false;
                }
            }
            else if (copyColumn.FieldType == typeof(string))
            {
                newRow[copyColumn.FieldName] = pasteData;
            }

            return true;
        }

        public void DeleteCellContent(object parameter)
        {
            //EntitiesUndoRedoManager.Clear();
            GridControl gridControl = (GridControl)parameter;
            TableView tableView = gridControl.View as TableView;
            EntitiesUndoRedoManager.PauseActionId();
            var selected_cells = tableView.GetSelectedCells();
            if(selected_cells.Count == 0)
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

            foreach (var selected_cell in selected_cells)
            {
                int row_handle = selected_cell.RowHandle;
                DataRowView editing_row_view = (DataRowView)gridControl.GetRow(row_handle);
                DataRow editing_row = editing_row_view.Row;
                DataColumn editing_column = editing_row.Table.Columns[selected_cell.Column.FieldName];
                ExoSubJobProjection entity = ((ForecastJobData)editing_row[columnEntity]).Projection;

                string columnFieldName = selected_cell.Column.FieldName;
                DateTime deleteCellDate;
                if(DateTime.TryParse(columnFieldName, out deleteCellDate))
                {
                    EntitiesUndoRedoManager.AddUndo(editing_row, columnFieldName, editing_row[columnFieldName], null, EntityMessageType.Changed);
                    setForecastCellNull(editing_row, entity, columnFieldName);
                    findExistingOrAddNewForecast(editing_row, entity, deleteCellDate, null, false);
                    //editing_row[columnFieldName] = 0.00m;
                }
            }

            EntitiesUndoRedoManager.UnpauseActionId();
            GridControlService.RefreshData();
        }

        public override void OnAfterAuxiliaryEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
            // if (changedType == typeof(FORECAST))
            //{
            //    FORECAST changedFORECAST = FORECASTCollectionViewModel.Entities.FirstOrDefault(x => x.GUID == (Guid)key);
            //    if (changedFORECAST != null)
            //    {
            //        ExoSubJobProjection findUpdatedEntity = exoSubJobs.FirstOrDefault(x => x.SubJob.Code == changedFORECAST.SUBJOB_CODE && x.Discipline.Code == changedFORECAST.DISCIPLINE_CODE);
            //        if(findUpdatedEntity != null)
            //        {
            //            updateDataRowForecast(findUpdatedEntity);
            //        }
            //    }

            //    mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertyChanged(x => x.DataPointsTable)));
            //}

            base.OnAfterAuxiliaryEntitiesChanged(key, changedType, messageType, sender, isBulkRefresh);
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
            commitCellValue(e.Column.FieldName, dataRowView.Row, e.OldValue, e.Value);
            EntitiesUndoRedoManager.UnpauseActionId();

            this.RaisePropertyChanged(x => x.ForecastSummary);
            GridControlService.RefreshData();
            e.Handled = true;
        }

        protected virtual void commitCellValue(string fieldName, DataRow row, object oldValue, object newValue)
        {
            ExoSubJobProjection entity = ((ForecastJobData)row[columnEntity]).Projection;

            if (fieldName.ToUpper() == "ENTITY.BUDGET" || fieldName.ToUpper().Contains("ENTITY.RATE"))
            {
                bool isRate = fieldName.ToUpper().Contains("ENTITY.RATE");
                decimal newDecimalValue = 0;
                if (newValue != null && decimal.TryParse(newValue.ToString(), out newDecimalValue))
                {
                    ForecastSummary.Costs -= (decimal)oldValue;
                    ForecastSummary.Costs += newDecimalValue;

                    ExoSubJobEditableProjection projection = new ExoSubJobEditableProjection(entity);
                    JOBCOST_LINES findExistingOrAddLine = ExoQueries.GetProjectLine(primeroUnitOfWork, loadPROJECT.NUMBER, projection);
                    bool isError = false;
                    if (isRate)
                        projection.Rate = newDecimalValue;
                    else
                        projection.Budget = newDecimalValue;

                    if (findExistingOrAddLine == null)
                    {
                        if (masterJob == null)
                        {
                            MessageBoxService.ShowMessage("Cannot change budget because the master job is not created for project " + loadPROJECT.NUMBER + " isn't added\nPlease contact " + BluePrintsResources.Default_CFO);
                            isError = true;
                        }
                        else if (copyLine == null)
                        {
                            MessageBoxService.ShowMessage("Cannot change budget because the master line is not created for project " + loadPROJECT.NUMBER + " isn't added\nPlease contact " + BluePrintsResources.Default_CFO);
                            isError = true;
                        }
                        else if (ExoMethods.CommitLineSubJob(projection, false, BulkColumnEditDialogService, masterJob, loadPROJECT.NUMBER, primeroUnitOfWork))
                        {
                            if (ExoMethods.CommitLineDiscipline(projection, false, BulkColumnEditDialogService, masterJob, loadPROJECT.NUMBER, primeroUnitOfWork))
                            {
                                //stock item cannot be added, so it must exists before commodity can be added using it
                                string stockCode = projection.GetStockCode();
                                STOCK_ITEMS stock_item = ExoQueries.FindSTOCK_ITEM(primeroUnitOfWork, stockCode);
                                if (stock_item != null)
                                {
                                    projection.StockName = stock_item.DESCRIPTION;
                                    if (ExoMethods.CommitLineCommodity(projection, stock_item, false, BulkColumnEditDialogService, masterJob, loadPROJECT.NUMBER, primeroUnitOfWork))
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
                            projection.Budget = 0;
                        else
                        {
                            DataRow disciplineRow = findDisciplineRow(entity);
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
                        if(isRate)
                            findExistingOrAddLine.QUOTE_UNITPR = Convert.ToDouble(newDecimalValue);
                        else
                            findExistingOrAddLine.ACTUAL_UNITCOST = Convert.ToDouble(newDecimalValue);

                        primeroUnitOfWork.SaveChanges();

                        DataRow disciplineRow = findDisciplineRow(entity);
                        if (disciplineRow != null)
                        {
                            recurseCalculateBudget(disciplineRow);
                        }
                    }
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

                    EntitiesUndoRedoManager.AddUndo(row, fieldName, oldValue, forecastUnits, EntityMessageType.Changed);
                    findExistingOrAddNewForecast(row, entity, dateTime, forecastUnits, false);
                }
            }
        }

        private void recurseCalculateBudget(DataRow disciplineRow)
        {
            DataTable childTable = (DataTable)disciplineRow[columnChild];
            decimal totalBudget = 0;
            decimal totalRate = 0;
            for (int i = 0; i < childTable.Rows.Count; i++)
            {
                DataRow childRow = childTable.Rows[i];
                ForecastJobData childCalculation = (ForecastJobData)childRow[columnEntity];
                totalBudget += childCalculation.Budget;
                totalRate += childCalculation.Rate;
            }

            ForecastJobData job = (ForecastJobData)disciplineRow[columnEntity];
            job.SetBudgetCost(totalBudget);
            job.SetForecastRate(totalRate);
        }

        private void findExistingOrAddNewForecast(DataRow dataRow, ExoSubJobProjection entity, DateTime forecastDate, decimal? forecastUnits, bool isRecursive = false)
        {
            decimal? oldValue = 0.00m;
            FORECAST findFORECAST = FORECASTCollectionViewModel.Entities.FirstOrDefault(x => x.FORECAST_DATE == forecastDate.Date && x.SUBJOB_CODE == entity.SubJob.Code && x.DISCIPLINE_CODE == entity.Discipline.Code && x.COMMODITY_CODE == entity.Commodity.Code && x.VARIATION_CODE == entity.Variation_Code && !x.IS_EAC);
            if (findFORECAST == null)
            {
                FORECAST newFORECAST = new FORECAST();
                newFORECAST.GUID = Guid.Empty;
                newFORECAST.GUID_PROJECT = loadPROJECT.GUID;
                newFORECAST.SUBJOB_CODE = entity.SubJob.Code;
                newFORECAST.DISCIPLINE_CODE = entity.Discipline.Code;
                newFORECAST.COMMODITY_CODE = entity.Commodity.Code;
                newFORECAST.VARIATION_CODE = normalizeVariationCode(entity.Variation_Code);
                newFORECAST.FORECAST_DATE = forecastDate.Date;
                newFORECAST.FORECAST_UNITS = forecastUnits;
                FORECASTCollectionViewModel.Save(newFORECAST);
            }
            else
            {
                oldValue = findFORECAST.FORECAST_UNITS;
                findFORECAST.FORECAST_UNITS = forecastUnits;
                FORECASTCollectionViewModel.Save(findFORECAST);
            }

            decimal? newValue = 0.00m;
            //used to ensure child row is set
            if (forecastUnits != null)
                newValue = forecastUnits;

            dataRow[forecastDate.ToShortDateString()] = newValue;
            //only do this on discipline level so we don't add new forecasted units twice
            if (entity.Commodity.Code == string.Empty)
            {
                updateEstimateAtComplete(oldValue, newValue);
            }

            if (!isRecursive)
            {
                string dateFieldName = forecastDate.ToShortDateString();
                //need to set child forecast empty
                if (entity.Commodity.Code == string.Empty)
                {
                    DataTable childTable = (DataTable)dataRow[columnChild];
                    foreach (DataRow childRow in childTable.Rows)
                    {
                        ExoSubJobProjection childEntity = ((ForecastJobData)childRow[columnEntity]).Projection;
                        if (childTable.Columns.Contains(dateFieldName))
                        {
                            if(!isBackgroundEdit)
                                EntitiesUndoRedoManager.AddUndo(childRow, dateFieldName, childRow[dateFieldName], 0.00m, EntityMessageType.Changed);

                            findExistingOrAddNewForecast(childRow, childEntity, forecastDate.Date, 0.00m, true);
                            //setForecastCellNull(childRow, (ExoSubJobProjection)childRow[columnEntity], dateFieldName);
                        }
                    }

                    calculateUncommitted(dataRow);
                }
                //set parent forecast empty
                else
                {
                    DataRow disciplineRow = findDisciplineRow(entity);
                    if (disciplineRow != null)
                    {
                        if (dataPointsTable.Columns.Contains(dateFieldName))
                        {
                            decimal cumulativeCosts = 0;
                            DataTable childTable = (DataTable)disciplineRow[columnChild];
                            foreach (DataRow childRow in childTable.Rows)
                            {
                                ExoSubJobProjection childEntity = ((ForecastJobData)childRow[columnEntity]).Projection;
                                decimal childCostOnDate = 0;
                                if(childRow[dateFieldName] != DBNull.Value)
                                    childCostOnDate = (decimal)childRow[dateFieldName];
                                cumulativeCosts += childCostOnDate;
                            }

                            if (!isBackgroundEdit)
                                //only visually represents the costs but stores null in the database
                                EntitiesUndoRedoManager.AddUndo(disciplineRow, dateFieldName, disciplineRow[dateFieldName], cumulativeCosts, EntityMessageType.Changed);

                            findExistingOrAddNewForecast(disciplineRow, ((ForecastJobData)disciplineRow[columnEntity]).Projection, forecastDate.Date, cumulativeCosts, true);
                            disciplineRow[dateFieldName] = cumulativeCosts;
                        }
                    }

                    calculateUncommitted(dataRow);
                    calculateUncommitted(disciplineRow);
                }


            }
        }

        private void updateEstimateAtComplete(decimal? oldValue, decimal? newValue)
        {
            decimal oldForecastUnits = oldValue == null ? 0 : (decimal)oldValue;
            decimal newForecastUnits = newValue == null ? 0 : (decimal)newValue;
            ForecastSummary.EstimateAtCompletion -= oldForecastUnits;
            ForecastSummary.EstimateAtCompletion += newForecastUnits;
            this.RaisePropertyChanged(x => x.ForecastSummary);
        }

        private DataRow findDisciplineRow(ExoSubJobProjection entity)
        {
            if(entity.Variation_Code == string.Empty || entity.Variation_Code == null)
            {
                return (from DataRow dr in dataPointsTable.Rows
                        where (((ForecastJobData)dr[columnEntity])).Projection.SubJob.Code == entity.SubJob.Code && (((ForecastJobData)dr[columnEntity])).Projection.Discipline.Code == entity.Discipline.Code && ((((ForecastJobData)dr[columnEntity])).Projection.Variation_Code == string.Empty || (((ForecastJobData)dr[columnEntity])).Projection.Variation_Code == null)
                        select dr).FirstOrDefault();
            }
            else
            {
                return (from DataRow dr in dataPointsTable.Rows
                        where (((ForecastJobData)dr[columnEntity])).Projection.SubJob.Code == entity.SubJob.Code && (((ForecastJobData)dr[columnEntity])).Projection.Discipline.Code == entity.Discipline.Code && (((ForecastJobData)dr[columnEntity])).Projection.Variation_Code == entity.Variation_Code
                        select dr).FirstOrDefault();
            }
        }

        private void removeProjectEACOnDate(DateTime forecastDate)
        {
            List<FORECAST> projectDateEACs = bluePrintsUnitOfWork.FORECASTS.Where(x => x.FORECAST_DATE == forecastDate.Date && x.IS_EAC).ToList();
            LoadingScreenManager.ShowLoadingScreen(projectDateEACs.Count);
            foreach(FORECAST projectDateEAC in projectDateEACs)
            {
                string eacName;
                if (projectDateEAC.COMMODITY_CODE != string.Empty)
                    eacName = projectDateEAC.SUBJOB_CODE + "-" + projectDateEAC.DISCIPLINE_CODE + "-" + projectDateEAC.COMMODITY_CODE;
                else
                    eacName = projectDateEAC.SUBJOB_CODE + "-" + projectDateEAC.DISCIPLINE_CODE;

                LoadingScreenManager.SetMessage("Removing old EAC on " + forecastDate.ToShortDateString() + " for job: " + eacName);
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
            newFORECAST.GUID_PROJECT = loadPROJECT.GUID;
            newFORECAST.SUBJOB_CODE = entity.SubJob.Code;
            newFORECAST.DISCIPLINE_CODE = entity.Discipline.Code;
            newFORECAST.COMMODITY_CODE = entity.Commodity.Code;
            newFORECAST.VARIATION_CODE = normalizeVariationCode(entity.Variation_Code);
            newFORECAST.FORECAST_DATE = forecastDate.Date;
            newFORECAST.FORECAST_UNITS = eacAmount;
            newFORECAST.IS_EAC = true;
            bluePrintsUnitOfWork.FORECASTS.Add(newFORECAST);
        }

        /// <summary>
        /// Sum uncommitted values
        /// </summary>
        private void calculateUncommitted(DataRow dataRow)
        {
            ForecastJobData job = (ForecastJobData)dataRow[columnEntity];
            DataTable dataTable = dataRow.Table;

            decimal uncommittedRecalculation = 0;
            for (int i = 0; i < dataRow.ItemArray.Count(); i++)
            {
                DataColumn dataColumn = dataTable.Columns[i];
                string columnName = dataColumn.ColumnName;
                DateTime parseDateTime;
                if (DateTime.TryParse(columnName, out parseDateTime))
                    if(parseDateTime > FixedDataDateMonthEnd)
                        if(dataRow[columnName] != DBNull.Value && dataRow[columnName] != null)
                            if(((decimal)dataRow[columnName]) > 0)
                                uncommittedRecalculation += (decimal)dataRow[columnName];
            }

            ExoSubJobProjection entity = ((ForecastJobData)dataRow[columnEntity]).Projection;
            if (entity.Commodity.Code == string.Empty)
            {
                if(dataRow[columnChild] != DBNull.Value)
                {
                    DataTable childTable = (DataTable)dataRow[columnChild];
                    for (int i = 0; i < dataRow.ItemArray.Count(); i++)
                    {
                        DataColumn dataColumn = dataTable.Columns[i];
                        string columnName = dataColumn.ColumnName;
                        DateTime parseDateTime;
                        if (DateTime.TryParse(columnName, out parseDateTime))
                        {
                            if (parseDateTime > FixedDataDateMonthEnd)
                            {
                                decimal cumulativeCostsOnDate = 0;
                                foreach (DataRow childRow in childTable.Rows)
                                {
                                    if (childRow[columnName] != DBNull.Value && childRow[columnName] != null)
                                        if (((decimal)childRow[columnName]) > 0)
                                            cumulativeCostsOnDate += (decimal)childRow[columnName];
                                }

                                if (cumulativeCostsOnDate != 0)
                                    dataRow[columnName] = cumulativeCostsOnDate;
                            }
                        }
                    }
                }
            }

            job.Uncommitted = uncommittedRecalculation;
        }

        public bool CanSaveEAC => isCompletelyLoaded;

        public void SaveEAC()
        {
            removeProjectEACOnDate(FixedDataDateMonthEnd);

            LoadingScreenManager.ShowLoadingScreen(DataPointsTable.Rows.Count);
            foreach (DataRow masterRow in DataPointsTable.Rows)
            {
                if (masterRow[columnChild] != DBNull.Value)
                {
                    DataTable childTable = (DataTable)masterRow[columnChild];
                    foreach(DataRow childRow in childTable.Rows)
                    {
                        ForecastJobData childEntity = (ForecastJobData)childRow[columnEntity];
                        if(childEntity.EstimateAtCompletion > 0)
                        {
                            LoadingScreenManager.SetMessage("Adding EAC for Job: " + childEntity.ToString());
                            findExistingOrAddNewEAC(childEntity.Projection, FixedDataDateMonthEnd, childEntity.EstimateAtCompletion);
                        }
                    }
                }

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
            MessageBoxService.ShowMessage("EAC for data date: " + FixedDataDateMonthEnd.ToShortDateString() + " is saved", "EAC Saved", MessageButton.OK, MessageIcon.Information);
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

        protected bool isBackgroundEdit = false;
        /// <summary>
        /// Function to undo the entity changes
        /// Must be used in conjunction of EntitiesUndoManager
        /// </summary>
        /// <param name="entityProperty">Entity passed over from EntitiesUndoRedo</param>
        public virtual void BulkPropertyUndo(IEnumerable<UndoRedoEntityInfo<DataRow>> entityProperties)
        {
            isBackgroundEdit = true;
            IEnumerable<UndoRedoEntityInfo<DataRow>> bulkSaveProperties = entityProperties.Where(x => x.MessageType == EntityMessageType.Changed);
            foreach (UndoRedoEntityInfo<DataRow> entityProperty in bulkSaveProperties)
            {
                object oldValue = entityProperty.OldValue;
                ExoSubJobProjection exoSubJob = ((ForecastJobData)entityProperty.ChangedEntity[columnEntity]).Projection;
                if (oldValue == null || oldValue == DBNull.Value)
                {
                    setForecastCellNull(entityProperty.ChangedEntity, exoSubJob, entityProperty.PropertyName);
                    //oldValue = 0.00m;
                }
                else
                    entityProperty.ChangedEntity[entityProperty.PropertyName] = oldValue;

                DateTime parseDateTime;
                if (DateTime.TryParse(entityProperty.PropertyName, out parseDateTime))
                {
                    decimal? oldValueDecimal = null;
                    if (entityProperty.OldValue != null)
                        oldValueDecimal = (decimal)entityProperty.OldValue;
                    findExistingOrAddNewForecast(entityProperty.ChangedEntity, exoSubJob, parseDateTime, oldValueDecimal, true);
                }
            }

            foreach(UndoRedoEntityInfo<DataRow> entityProperty in bulkSaveProperties)
            {
                calculateUncommitted(entityProperty.ChangedEntity);
            }

            GridControlService.RefreshData();
            isBackgroundEdit = false;
        }

        /// <summary>
        /// Function to redo the entity changes
        /// Must be used in conjunction of EntitiesUndoManager
        /// </summary>
        /// <param name="entityProperty">Entity passed over from EntitiesUndoRedo</param>
        public virtual void BulkPropertyRedo(IEnumerable<UndoRedoEntityInfo<DataRow>> entityProperties)
        {
            isBackgroundEdit = true;
            IEnumerable<UndoRedoEntityInfo<DataRow>> bulkSaveProperties = entityProperties.Where(x => x.MessageType == EntityMessageType.Changed);
            foreach (UndoRedoEntityInfo<DataRow> entityProperty in bulkSaveProperties)
            {
                object newValue = entityProperty.NewValue;
                ExoSubJobProjection exoSubJob = ((ForecastJobData)entityProperty.ChangedEntity[columnEntity]).Projection;
                if (newValue == null || newValue == DBNull.Value)
                {
                    setForecastCellNull(entityProperty.ChangedEntity, exoSubJob, entityProperty.PropertyName);
                    //newValue = 0.00m;
                }
                else
                    entityProperty.ChangedEntity[entityProperty.PropertyName] = newValue;

                DateTime parseDateTime;
                if (DateTime.TryParse(entityProperty.PropertyName, out parseDateTime))
                {
                    decimal? newValueDecimal = null;
                    if (entityProperty.NewValue != DBNull.Value && entityProperty.NewValue != null)
                        newValueDecimal = (decimal)entityProperty.NewValue;
                    findExistingOrAddNewForecast(entityProperty.ChangedEntity, exoSubJob, parseDateTime, newValueDecimal, true);
                }
            }

            foreach (UndoRedoEntityInfo<DataRow> entityProperty in bulkSaveProperties)
            {
                calculateUncommitted(entityProperty.ChangedEntity);
            }

            GridControlService.RefreshData();
            isBackgroundEdit = false;
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

                        string exportPath = ResultPath + "\\" + exportDate.Year + "-" + exportDate.ToString("MMM") + "_" + loadPROJECT.NUMBER + "_Forecast" + ".xlsx";
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
        #endregion

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

        public IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection
        {
            get
            {
                return GetEntities<COMMODITY_CODE>();
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
            Revenue = 0;
            Costs = 0;
            TotalClaims = 0;
            Actuals = 0;
            Commitments = 0;
            EstimateAtCompletion = 0;
        }

        public decimal Revenue { get; set; }
        public decimal Costs { get; set; }
        public decimal Margin => Revenue - Costs;
        public decimal Margin_Percent => Revenue == 0 ? 0 : Margin / Revenue;
        public decimal TotalClaims { get; set; }
        public decimal Actuals { get; set; }
        public decimal Commitments { get; set; }
        public decimal EstimateAtCompletion { get; set; }
    }
}