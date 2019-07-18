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
            forceRetrieveRemainingDataPoints = true;

            delayedProjectSaveTimer = new DispatcherTimer();
            delayedProjectSaveTimer.Interval = new TimeSpan(0, 0, 0, 1);

            delayedUpdateFloatingProjectSummaryTimer = new DispatcherTimer();
            delayedUpdateFloatingProjectSummaryTimer.Interval = new TimeSpan(0, 0, 0, 1);

            delayedGridUpdateTimer = new DispatcherTimer();
            delayedGridUpdateTimer.Interval = new TimeSpan(0, 0, 0, 1);

            delayedDateChangeMessageBoxTimer = new DispatcherTimer();
            delayedDateChangeMessageBoxTimer.Interval = new TimeSpan(0, 0, 0, 1);

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
        }

        private void setProject(Data.PROJECT project)
        {
            LoadPROJECT = project;

            DateTime dataDate;
            if (LoadPROJECT.FORECAST_DATA_DATE == null)
                dataDate = DateTime.Now;
            else
                dataDate = (DateTime)LoadPROJECT.FORECAST_DATA_DATE;

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

        protected virtual Func<IRepositoryQuery<FORECAST_PO>, IQueryable<FORECAST_PO>> FORECAST_POProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == LoadPROJECT.GUID);
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
        DispatcherTimer delayedProjectSaveTimer;
        DispatcherTimer delayedUpdateFloatingProjectSummaryTimer;
        DispatcherTimer delayedGridUpdateTimer;
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

        protected override void resolveParameters(object parameter)
        {
            base.resolveParameters(parameter);
            bluePrintsUnitOfWork = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            ForecastSummary = new ForecastSummary();
            forceRetrieveAllBurned = true; //force exo burned to retrieve subjobs that aren't defined
            useProductivityFactorOnRemaining = false; //calculate remaining costs using productivity factor
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
            isVariationSeparated = true;
            showStatsBuildingLoadingScreen = true;

            GlobalMethods.SetAccordionExpandedState?.Invoke(false);
            this.RaisePropertiesChanged();
        }

        private void loadExoMethodsData()
        {
            IPrimeroEntitiesUnitOfWork primeroEntitiesUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            masterJob = ExoQueries.GetProjectSubJob(primeroEntitiesUnitOfWork, LoadPROJECT.NUMBER, LoadPROJECT.NUMBER);
            copyLine = ExoQueries.GetAnyProjectLineByJobNumber(primeroEntitiesUnitOfWork, LoadPROJECT.NUMBER);
        }

        private void loadSummaryStats()
        {
            IPrimeroEntitiesUnitOfWork primeroEntitiesUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            List<ExoTimeAuthorisation> jobLines = new List<ExoTimeAuthorisation>(); 
            queryJobs = ExoQueries.GetNativeExoSubJobProjection(primeroEntitiesUnitOfWork, LoadPROJECT, ref jobLines);
            queryJobLines = jobLines;

            dynamic revenueLine = ExoQueries.GetProjectRevenue(primeroEntitiesUnitOfWork, LoadPROJECT.NUMBER);
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

            ForecastSummary.TotalClaims = ExoQueries.GetProjectClaims(primeroEntitiesUnitOfWork, LoadPROJECT.NUMBER);
        }

        protected override List<StatsCalculationType> getForecastTypes()
        {
            List<StatsCalculationType> calcTypes = new List<StatsCalculationType>();
            calcTypes.Add(StatsCalculationType.Forecast);
            calcTypes.Add(StatsCalculationType.Burned);

            return calcTypes;
        }

        public DateTime FixedDataDateMonthEnd => new DateTime(((DateTime)FixedDataDate).Year, ((DateTime)FixedDataDate).Month, 1).AddMonths(1).AddDays(-1);

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
                DateTime saveDateTime = (DateTime)FixedDataDate;
                LoadPROJECT.FORECAST_DATA_DATE = new DateTime(((DateTime)saveDateTime).Year, ((DateTime)saveDateTime).Month, 1).AddMonths(1).AddDays(-1);
                LoadPROJECT.FORECAST_END_DATE = FixedEndDate;
                PROJECTCollectionViewModel.Save(LoadPROJECT);
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
                PROJECTCollectionViewModel.Save(LoadPROJECT);
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
            return !IsLoadingForecast;
        }

        public async void ReloadP6Forecast()
        {
            IsLoadingForecast = true;
            this.RaisePropertyChanged(x => x.IsLoadingForecast);
            if (summaryBackgroundWorker != null)
                summaryBackgroundWorker.CancelAsync();

            //LoadingScreenManager.ShowLoadingScreen(1);
            await BluePrintsContextHelper.RefreshDeliverablesRemainingDataPointsByProject(LoadPROJECT.NUMBER, true);
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
            PROJECTCollectionViewModel.AlwaysSkipMessage = true;
            base.OnAfterAssignedCallbackAndRaisePropertyChanged();
            LoadingScreenManager.CloseLoadingScreen();
        }

        public override void FullRefresh()
        {
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
        List<ForecastJobData> commodityJobs;
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

                    IEnumerable<Common.ViewModel.Reporting.DataPoint> remainingDataPoints = AllProjectDashboards.Where(x => x.Stats != null && x.Stats.Remaining != null && x.Stats.Remaining.DataPoints != null).SelectMany(x => x.Stats.Remaining.DataPoints).ToList();
                    DateTime endDateToGenerate;

                    //because background worker haven't update this value yet, updating it will allow end date to be saved when it's less than remaining end date
                    isCompletelyLoaded = true;
                    if (remainingDataPoints.Count() > 0)
                    {
                        endDateToGenerate = remainingDataPoints.Max(x => x.ProgressDate);
                        if (endDateToGenerate > FixedEndDate)
                            FixedEndDate = endDateToGenerate;
                        else
                            endDateToGenerate = FixedEndDate;
                    }
                    else
                        endDateToGenerate = FixedEndDate;

                    alignedDataDateCollection = ChronologicalHelpers.GenerateMonthEndDatesCollection((DateTime)FixedDataDate, endDateToGenerate);
                    commodityJobs = ForecastHelper.CreateCommodityProjections(unifiedJobList, queryJobLines, AllProjectDashboards, FORECASTCollectionViewModel.Entities, FORECAST_POCollection, alignedDataDateCollection, (DateTime)FixedDataDate);

                    //construct data points table
                    dataPointsTable.Columns.Add(columnEntity, typeof(ForecastJobData));
                    dataPointsTable.Columns.Add(columnCompare, typeof(DataTable));
                    foreach (DateTime alignedDataDate in alignedDataDateCollection)
                    {
                        string columnFieldName = alignedDataDate.Date.ToShortDateString();
                        dataPointsTable.Columns.Add(columnFieldName, typeof(decimal));
                    }

                    LoadingScreenManager.ShowLoadingScreen(commodityJobs.Count);
                    LoadingScreenManager.SetMessage("Preparing View...");

                    //child data table is used to record original value of actuals + committed + remaining values before it is overridden by forecasts
                    foreach (ForecastJobData commodityJob in commodityJobs)
                    {
                        DataRow commodityRow = dataPointsTable.NewRow();

                        DataTable compareDataTable = dataPointsTable.Clone();
                        DataRow compareActualsRow = compareDataTable.NewRow();
                        compareActualsRow[columnEntity] = new ForecastJobData() { DropDownPhase = "Actuals $", CompareMask = "c0" };
                        DataRow compareMaterialRow = compareDataTable.NewRow();
                        compareMaterialRow[columnEntity] = new ForecastJobData() { DropDownPhase = "Materials $", CompareMask = "c0" };
                        DataRow comparePOForecastRow = compareDataTable.NewRow();
                        comparePOForecastRow[columnEntity] = new ForecastJobData() { DropDownPhase = "PO Forecast $", CompareMask = "c0" };
                        DataRow compareP6CostsRemainingRow = compareDataTable.NewRow();
                        compareP6CostsRemainingRow[columnEntity] = new ForecastJobData() { DropDownPhase = "P6 $", CompareMask = "c0" };
                        DataRow compareP6UnitsRemainingRow = compareDataTable.NewRow();
                        compareP6UnitsRemainingRow[columnEntity] = new ForecastJobData() { DropDownPhase = "P6 Hours", CompareMask = "n0" };

                        commodityRow[columnEntity] = commodityJob;
                        foreach(ForecastDateCost dateCost in commodityJob.DateCosts)
                        {
                            compareActualsRow[dateCost.Date.ToShortDateString()] = dateCost.ActualCosts;
                            compareMaterialRow[dateCost.Date.ToShortDateString()] = dateCost.MaterialCosts;
                            comparePOForecastRow[dateCost.Date.ToShortDateString()] = dateCost.POForecastCosts;
                            compareP6CostsRemainingRow[dateCost.Date.ToShortDateString()] = dateCost.P6Costs;
                            compareP6UnitsRemainingRow[dateCost.Date.ToShortDateString()] = dateCost.P6Hours;
                            commodityRow[dateCost.Date.ToShortDateString()] = dateCost.TotalCosts;
                        }

                        updateUncommittedOnDatesFromDb(commodityRow);
                        updateTotalUncommittedOnJob(commodityRow);

                        compareDataTable.Rows.Add(compareActualsRow);
                        compareDataTable.Rows.Add(compareMaterialRow);
                        compareDataTable.Rows.Add(comparePOForecastRow);
                        compareDataTable.Rows.Add(compareP6CostsRemainingRow);
                        compareDataTable.Rows.Add(compareP6UnitsRemainingRow);
                        commodityRow[columnCompare] = compareDataTable;

                        dataPointsTable.Rows.Add(commodityRow);

                        //calculate project summary, needs to be done after uncommitted is calculated
                        ForecastSummary.Budget_Cost += commodityJob.Budget;
                        ForecastSummary.Current_Cost += commodityJob.Actuals;
                        ForecastSummary.Commitments += commodityJob.Outstanding;
                        ForecastSummary.Uncommitted_Forecast += commodityJob.Uncommitted;
                        ForecastSummary.EstimateAtCompletion += commodityJob.EstimateAtCompletion;

                        LoadingScreenManager.Progress();
                    }

                    LoadingScreenManager.CloseLoadingScreen();
                    this.RaisePropertyChanged(x => x.ForecastSummary);
                    this.RaisePropertyChanged(x => x.ExportTable);

                    //refreshGridDataDelayed();
                    //TableViewService.ScrollToLast();
                }

                return dataPointsTable;
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
        private void updateDataRowForecast(ExoSubJobProjection disciplineEntity)
        {
            DataRow dataRow = findCommodityRow(disciplineEntity);
            if (dataRow == null)
                return;

            updateUncommittedOnDatesFromDb(dataRow);
        }

        private void resetRemainingOnJob(DataRow updateRow, string fieldName, bool addUndo)
        {
            if (updateRow[columnCompare] == DBNull.Value)
                return;

            ExoSubJobProjection entity = ((ForecastJobData)updateRow[columnEntity]).Projection;
            DataTable compareDataTable = (DataTable)updateRow[columnCompare];

            decimal oldValue = 0.00m;
            decimal newValue = 0.00m;
            if(compareDataTable.Columns.Contains(fieldName))
            {
                decimal resetValue = getMasterRowResetValue(compareDataTable, fieldName);
                oldValue = (decimal)updateRow[fieldName];
                newValue = resetValue;
                updateRow[fieldName] = newValue;
                EntitiesUndoRedoManager.AddUndo(updateRow, fieldName, oldValue, newValue, EntityMessageType.Changed);
                updateTotalUncommittedOnJob(updateRow);
            }
        }

        private void updateUncommittedOnDatesFromDb(DataRow dataRow)
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
                    e.Column.MaxWidth = 50;
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
                    e.Column.Width = 75;
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
            else
            {
                e.Cancel = true;
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
                        e.Column.ReadOnly = true;
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

                GridControlService.RefreshData();
                e.Handled = true;
            }

        }

        private void pasteCellData(GridControl gridControl, TableView gridTableView, string[] RowData)
        {
            EntitiesUndoRedoManager.PauseActionId();
            GridControlHelpers.PasteCellData(gridControl, gridTableView, RowData, basePasteData);
            EntitiesUndoRedoManager.UnpauseActionId();
        }

        private bool basePasteData(DataRow newRow, ColumnBase copyColumn, string pasteData)
        {
            if(copyColumn.FieldName.ToUpper() == "ENTITY.BUDGET")
            {
                //currently disabled on paste because view doesn't reflect changes
                //return commitBudget(newRow, pasteData);
            }
            else if (copyColumn.FieldType == typeof(decimal))
            {
                var rgx = new Regex("[^0-9a-z\\.]");
                var cleanColumnString = rgx.Replace(pasteData, string.Empty);
                decimal decimal_value;
                if (decimal.TryParse(cleanColumnString, out decimal_value))
                {
                    DateTime columnDateTime;
                    if (DateTime.TryParse(copyColumn.FieldName, out columnDateTime))
                    {
                        DataTable compareEntity = (DataTable)newRow["CompareEntities"];
                        if (compareEntity.Rows.Count > 3)
                        {
                            decimal actualCosts = (decimal)compareEntity.Rows[0][copyColumn.FieldName];
                            decimal materialCosts = (decimal)compareEntity.Rows[1][copyColumn.FieldName];
                            decimal poForecastCosts = (decimal)compareEntity.Rows[2][copyColumn.FieldName];
                            decimal p6RemainingCosts = (decimal)compareEntity.Rows[3][copyColumn.FieldName];
                            decimal totalCosts = actualCosts + materialCosts + poForecastCosts + p6RemainingCosts;
                            totalCosts = Math.Round(totalCosts);
                            if (decimal_value >= totalCosts)
                            {
                                EntitiesUndoRedoManager.AddUndo(newRow, copyColumn.FieldName, newRow[copyColumn.FieldName], decimal_value, EntityMessageType.Changed);
                                newRow[copyColumn.FieldName] = decimal_value;
                                findExistingOrAddNewForecast(newRow, columnDateTime, decimal_value, true);
                            }
                        }
                    }
                }
                else
                {
                    resetRemainingOnJob(newRow, copyColumn.FieldName, true);
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
                    resetRemainingOnJob(editing_row, columnFieldName, true);
                    findExistingOrAddNewForecast(editing_row, deleteCellDate, null, false);
                    //editing_row[columnFieldName] = 0.00m;
                }
            }

            EntitiesUndoRedoManager.UnpauseActionId();
            GridControlService.RefreshData();
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
                commitBudget(row, newValue);
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
                    findExistingOrAddNewForecast(row, dateTime, forecastUnits, true);
                }
            }
        }

        private bool commitBudget(DataRow dataRow, object newValue)
        {
            if(!LoginCredentials.hasPermission(PermissionResources.ChangeBudget))
                return false;

            ForecastJobData job = ((ForecastJobData)dataRow[columnEntity]);
            ExoSubJobProjection entity = job.Projection;
            decimal newDecimalValue = 0;
            if (newValue != null && decimal.TryParse(newValue.ToString(), out newDecimalValue))
            {
                ExoSubJobEditableProjection projection = new ExoSubJobEditableProjection(entity);
                JOBCOST_LINES findExistingOrAddLine = ExoQueries.GetProjectLine(primeroUnitOfWork, LoadPROJECT.NUMBER, projection);
                bool isError = false;
                projection.Budget = newDecimalValue;

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
                        projection.Budget = 0;
                    else
                    {
                        DataRow disciplineRow = findCommodityRow(entity);
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
                    DataRow disciplineRow = findCommodityRow(entity);
                    if (disciplineRow != null)
                    {
                        recurseCalculateBudget(disciplineRow);
                    }
                }

                GridControlService.RefreshData();
                updateFloatingSummaryMembers();
            }

            return true;
        }

        private void recurseCalculateBudget(DataRow commodityRow)
        {
            ForecastJobData commodityJob = (ForecastJobData)commodityRow[columnEntity];
            ForecastJobData job = (ForecastJobData)commodityRow[columnEntity];
            job.SetBudgetCost(commodityJob.Budget);
            job.SetForecastRate(commodityJob.Rate);
        }

        private decimal getMasterRowResetValue(DataTable compareDataTable, string dateFieldName)
        {
            if (compareDataTable != null && compareDataTable.Rows.Count > 0)
            {
                if (compareDataTable.Columns.Contains(dateFieldName))
                {
                    DataRow compareActualsRow = compareDataTable.Rows[0];
                    DataRow compareMaterialRow = compareDataTable.Rows[1];
                    DataRow comparePOForecastRow = compareDataTable.Rows[2];
                    DataRow compareP6CostsRemainingRow = compareDataTable.Rows[3];

                    decimal actualValue = compareActualsRow[dateFieldName] == DBNull.Value ? 0 : (decimal)compareActualsRow[dateFieldName];
                    decimal materialValue = compareMaterialRow[dateFieldName] == DBNull.Value ? 0 : (decimal)compareMaterialRow[dateFieldName];
                    decimal poValue = comparePOForecastRow[dateFieldName] == DBNull.Value ? 0 : (decimal)comparePOForecastRow[dateFieldName];
                    decimal p6Value = compareP6CostsRemainingRow[dateFieldName] == DBNull.Value ? 0 : (decimal)compareP6CostsRemainingRow[dateFieldName];
                    decimal totalValue = actualValue + materialValue + poValue + p6Value;

                    return totalValue;
                }
            }

            return 0.00m;
        }

        /// <summary>
        /// update view and database at the same time
        /// </summary>
        /// <param name="dataRow">data row containing the job and compare info</param>
        /// <param name="forecastDate">date of the forecast to update</param>
        /// <param name="viewNewValue">determine what will be updated in db but will be replaced by null if it's same as compare info, 
        /// however if it is passed in as null it signifies that the view is already updated and won't update it</param>
        /// <param name="addUndo">whether to add undo information</param>
        private void findExistingOrAddNewForecast(DataRow dataRow, DateTime forecastDate, decimal? viewNewValue, bool addUndo)
        {
            ExoSubJobProjection entity = ((ForecastJobData)dataRow[columnEntity]).Projection;
            string dateFieldName = forecastDate.ToShortDateString();
            decimal? oldValue = null;
            if (dataRow[dateFieldName] != DBNull.Value)
                oldValue = (decimal)dataRow[dateFieldName];

            decimal? compareValue = null;
            decimal? saveNewValue = viewNewValue;
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

            FORECAST findFORECAST = FORECASTCollectionViewModel.Entities.FirstOrDefault(x => x.FORECAST_DATE == forecastDate.Date && x.SUBJOB_CODE == entity.SubJob.Code && x.DISCIPLINE_CODE == entity.Discipline.Code && x.COMMODITY_CODE == entity.Commodity.Code && x.VARIATION_CODE == entity.Variation_Code && !x.IS_EAC);
            if (findFORECAST == null)
            {
                FORECAST newFORECAST = new FORECAST();
                newFORECAST.GUID = Guid.Empty;
                newFORECAST.GUID_PROJECT = LoadPROJECT.GUID;
                newFORECAST.SUBJOB_CODE = entity.SubJob.Code;
                newFORECAST.DISCIPLINE_CODE = entity.Discipline.Code;
                newFORECAST.COMMODITY_CODE = entity.Commodity.Code;
                newFORECAST.VARIATION_CODE = normalizeVariationCode(entity.Variation_Code);
                newFORECAST.FORECAST_DATE = forecastDate.Date;
                newFORECAST.FORECAST_UNITS = saveNewValue;
                FORECASTCollectionViewModel.Save(newFORECAST);
            }
            else
            {
                findFORECAST.FORECAST_UNITS = saveNewValue;
                FORECASTCollectionViewModel.Save(findFORECAST);
            }

            //used to ensure child row is set
            if (viewNewValue != null)
            {
                dataRow[forecastDate.ToShortDateString()] = viewNewValue;
            }

            if(addUndo)
                EntitiesUndoRedoManager.AddUndo(dataRow, dateFieldName, oldValue, saveNewValue, EntityMessageType.Changed);

            updateTotalUncommittedOnJob(dataRow);
            updateFloatingSummaryMembers();
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
            GridControlService.GridControl.RefreshData();
            DataControlDetailDescriptor gridDetail = (DataControlDetailDescriptor)GridControlService.GridControl.DetailDescriptor;
            GridControl childGrid = (GridControl)gridDetail.DataControl;
            childGrid.RefreshData();
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
            ForecastSummary.EstimateAtCompletion = 0;
            ForecastSummary.Uncommitted_Forecast = 0;
            ForecastSummary.Budget_Cost = 0;
            //cannot use parallel foreach because of inaccuracy
            foreach(ForecastJobData job in jobs)
            {
                ForecastSummary.Budget_Cost += job.Budget;
                ForecastSummary.EstimateAtCompletion += job.EstimateAtCompletion;
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

        private DataRow findCommodityRow(ExoSubJobProjection entity)
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
            newFORECAST.GUID_PROJECT = LoadPROJECT.GUID;
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
        /// Sum uncommitted values, need to be run after any updates to dates value
        /// </summary>
        private void updateTotalUncommittedOnJob(DataRow dataRow)
        {
            ForecastJobData job = (ForecastJobData)dataRow[columnEntity];
            DataTable dataTable = dataRow.Table;

            decimal preloadedSum = 0;
            decimal uncommittedRecalculation = 0;
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
                                    uncommittedRecalculation += (currentDateCellValue - Math.Round(dateCost.PreloadedCosts));
                                    preloadedSum += Math.Round(dateCost.PreloadedCosts);
                                }
                                else
                                    uncommittedRecalculation += currentDateCellValue;
                            }
            }

            //flag procurement jobs as error when uncommitted values on dates doesn't add up to outstanding POs
            if(job.Projection.SubJob.Code.ToUpper().Contains("P"))
            {
                decimal differences = Math.Round(job.Outstanding) - Math.Round(preloadedSum);
                differences = Math.Abs(differences);

                if (differences <= 10)
                    job.IsPOError = false;
                else
                    job.IsPOError = true;
            }

            job.Uncommitted = uncommittedRecalculation;
        }

        public bool CanSaveEAC => isCompletelyLoaded;

        public void SaveEAC()
        {
            List<ForecastJobData> jobs = getJobDataFromDatatable();
            if(jobs.Any(x => x.IsPOError))
            {
                MessageBoxService.ShowMessage("Some PO forecast aren't completed yet, please complete it before saving", "Error", MessageButton.OK, MessageIcon.Exclamation);
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
                    resetRemainingOnJob(entityProperty.ChangedEntity, entityProperty.PropertyName, false);
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
                    findExistingOrAddNewForecast(entityProperty.ChangedEntity, parseDateTime, oldValueDecimal, false);
                }
            }

            foreach(UndoRedoEntityInfo<DataRow> entityProperty in bulkSaveProperties)
            {
                updateTotalUncommittedOnJob(entityProperty.ChangedEntity);
            }

            GridControlService.RefreshData();
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
                    resetRemainingOnJob(entityProperty.ChangedEntity, entityProperty.PropertyName, false);
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
                    findExistingOrAddNewForecast(entityProperty.ChangedEntity, parseDateTime, newValueDecimal, false);
                }
            }

            foreach (UndoRedoEntityInfo<DataRow> entityProperty in bulkSaveProperties)
            {
                updateTotalUncommittedOnJob(entityProperty.ChangedEntity);
            }

            GridControlService.RefreshData();
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
            Original_Revenue = 0;
            Approved_Var_Revenue = 0;
            Budget_Cost = 0;
            EAC_Revenue = 0;
            Current_Cost = 0;
            Commitments = 0;
            Uncommitted_Forecast = 0;
            EstimateAtCompletion = 0;
            TotalClaims = 0;
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
        public decimal EstimateAtCompletion { get; set; }
        public decimal EAC_Margin => EAC_Revenue - EstimateAtCompletion;
        public decimal EAC_Margin_Percent => EAC_Revenue == 0 ? 0 : EAC_Margin / EAC_Revenue;

        public decimal TotalClaims { get; set; }
        public decimal UnderOverClaim => TotalClaims - Current_Cost;

        public SolidColorBrush Budget_Margin_Background => Budget_Margin > 0 ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.LightSalmon);
        public SolidColorBrush Budget_Margin_Percent_Background => Budget_Margin_Percent > 0 ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.LightSalmon);
        public SolidColorBrush EAC_Margin_Background => EAC_Margin > 0 ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.LightSalmon);
        public SolidColorBrush EAC_Margin_Percent_Background => EAC_Margin_Percent > 0 ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.LightSalmon);
        public SolidColorBrush UnderOverClaim_Background => UnderOverClaim > 0 ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.LightSalmon);
    }
}