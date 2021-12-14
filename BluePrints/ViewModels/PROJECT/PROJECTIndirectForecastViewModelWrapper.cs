using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Helpers;
using BaseModel.Misc;
using BaseModel.View;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Loader;
using BaseModel.ViewModel.UndoRedo;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Base;
using BluePrints.Common.Helpers;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Misc;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Core.Serialization;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Threading;

namespace BluePrints.ViewModels
{
    public class PROJECTIndirectForecastViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <FORECAST_JOB, FORECAST_JOB, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of FORECAST_JOBCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static PROJECTIndirectForecastViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new PROJECTIndirectForecastViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the FORECAST_JOBCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the FORECAST_JOBCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected PROJECTIndirectForecastViewModelWrapper(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        public PROJECT LoadPROJECT { get; set; }
        public DateTime? FixedDataDateMonthEnd => FixedDataDate == null ? (DateTime?)null : new DateTime(((DateTime)FixedDataDate).Year, ((DateTime)FixedDataDate).Month, 1).AddMonths(1).AddDays(-1);

        public DateTime? fixedDataDate;
        public DateTime? FixedDataDate
        {
            get => fixedDataDate;
            set
            {
                if (value == null)
                {
                    //when opening up multiple tabs, devexpress sometimes come in and set fixed data date as null
                    return;
                }

                //when switching tabs this value will be anonymously set by new DateTime()
                if (((DateTime)value).Year == new DateTime().Year)
                    return;

                fixedDataDate = value;
                this.RaisePropertyChanged(x => x.FixedDataDate);
            }
        }

        public DateTime fixedEndDate;
        public DateTime FixedEndDate
        {
            get => fixedEndDate;
            set
            {                
                //when switching tabs this value will be anonymously set by new DateTime()
                if (value.Year == new DateTime().Year)
                    return;

                fixedEndDate = value;
                this.RaisePropertyChanged(x => x.FixedEndDate);
            }
        }

        protected List<DateTime> alignedDataDateCollection;
        public List<ExoSubJobProjection> QueryJobs { get; set; }
        protected List<ExoTimeAuthorisation> queryJobLines { get; set; }
        public bool IsWeeks { get; set; }
        protected IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory;
        IPrimeroEntitiesUnitOfWork primeroEntitiesUnitOfWork;
        DispatcherTimer focusNewlyAddedProjectionTimer = new DispatcherTimer();
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<PROJECT>)parameter;
            LoadPROJECT = PROJECTParameter.GetEntity();
            primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(LoadPROJECT.OfficeNameForExo);
            primeroEntitiesUnitOfWork = primeroUnitOfWorkFactory.CreateUnitOfWork();
            IsWeeks = true;
            IsLoading = true;
            this.RaisePropertyChanged(x => x.IsLoading);

            focusNewlyAddedProjectionTimer = new DispatcherTimer();
            focusNewlyAddedProjectionTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            GlobalMethods.SetAccordionExpandedState?.Invoke(false);
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => setProject(x));
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, COMMODITY_CODEProjectionFunc);
            loaderCollection.AddLoaderDescription(primeroUnitOfWorkFactory, x => x.STOCK_ITEMS, STOCK_ITEMSProjectionFunc);
            loaderCollection.AddLoaderDescription<JOB_COSTTYPES, JOB_COSTTYPES, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOB_COSTTYPES);
            loaderCollection.AddLoaderDescription<JOB_COSTGROUPS, JOB_COSTGROUPS, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOB_COSTGROUPS);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.FORECAST_EACS, FORECAST_EACProjectionFunc);
            loaderCollection.AddLoaderDescription<STOCK_GROUPS, STOCK_GROUPS, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.STOCK_GROUPS);
            loaderCollection.AddLoaderDescription<STOCK_GROUP2S, STOCK_GROUP2S, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.STOCK_GROUP2S);
        }

        private Func<IRepositoryQuery<Data.PROJECT>, IQueryable<Data.PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == LoadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>> COMMODITY_CODEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == null);
        }

        private Func<IRepositoryQuery<STOCK_ITEMS>, IQueryable<STOCK_ITEMS>> STOCK_ITEMSProjectionFunc()
        {
            return query => query.Where(x => x.ISACTIVE == "Y");
        }

        protected virtual Func<IRepositoryQuery<FORECAST_EAC>, IQueryable<FORECAST_EAC>> FORECAST_EACProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == LoadPROJECT.GUID);
        }

        public DateTime? LoadDataDate { get; set; }
        private void setProject(Data.PROJECT project)
        {
            LoadPROJECT = project;

            DateTime dataDate;
            if (LoadPROJECT.FORECAST_DATA_DATE == null)
                dataDate = DateTime.Now;
            else
                dataDate = (DateTime)LoadPROJECT.FORECAST_DATA_DATE;

            dataDate = new DateTime(dataDate.Year, dataDate.Month, 1).AddMonths(1).AddSeconds(-1);
            LoadDataDate = dataDate;
            FixedDataDate = dataDate;

            DateTime endDate;
            if (LoadPROJECT.FORECAST_END_DATE == null)
                endDate = DateTime.Now.AddMonths(1);
            else
                endDate = (DateTime)LoadPROJECT.FORECAST_END_DATE;

            FixedEndDate = endDate;
            this.RaisePropertiesChanged();
        }

        protected override void BackgroundRefresh(bool loadLayout = false)
        {
            if (isJobsLoaded)
                base.BackgroundRefresh(true);
            //wait for exo data to load before refreshing data table
        }

        bool isJobsLoaded = false;
        BackgroundWorker jobsLoadingBackgroundWorker;
        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<FORECAST_JOB> entities)
        {
            isJobsLoaded = false;
            jobsLoadingBackgroundWorker = new BackgroundWorker();
            jobsLoadingBackgroundWorker.DoWork += JobsLoadingBackgroundWorker_DoWork;
            jobsLoadingBackgroundWorker.RunWorkerCompleted += JobsLoadingBackgroundWorker_RunWorkerCompleted;
            jobsLoadingBackgroundWorker.WorkerSupportsCancellation = true;
            jobsLoadingBackgroundWorker.RunWorkerAsync();

            MainViewModel.SetParentViewModel(this);
            MainViewModel.IsPasteCellLevel = false;
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        public override void FullRefresh()
        {
            EntitiesUndoRedoManager.Clear();
            dataPointsTable = null;
            this.RaisePropertyChanged(x => x.DataPointsTable);
            base.FullRefresh();
        }

        private void JobsLoadingBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (jobsLoadingBackgroundWorker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            refreshJobs();
        }

        protected override void OnClose(CancelEventArgs e)
        {
            if (shouldPromptForSavingSnapshot)
            {
                if (MessageBoxService.ShowMessage("Forecast edited, do you wish to save a snapshot?", "Save", MessageButton.YesNo) == MessageResult.Yes)
                    SaveSnapshot();
            }

            jobsLoadingBackgroundWorker.CancelAsync();
            GlobalMethods.SetAccordionExpandedState?.Invoke(true);
            base.OnClose(e);
        }

        private void JobsLoadingBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            isJobsLoaded = true;
            BackgroundRefresh();
        }

        protected override bool loadDataPointsTable()
        {
            IsLoading = true;
            //get immutable data
            alignedDataDateCollection = generateDates();
            this.RaisePropertyChanged(x => x.IsLoading);
            dataPointsTable = null;
            updateDataPointsTable();
            this.RaisePropertyChanged(x => x.DataPointsTable);
            IsLoading = false;
            this.RaisePropertyChanged(x => x.IsLoading);
            CommonMethods.AddSaveLayoutHandler(GridControlService.GetGridColumns());

            return true;
        }

        private void refreshJobs()
        {
            List<ExoTimeAuthorisation> jobLines = new List<ExoTimeAuthorisation>();
            IBluePrintsEntitiesUnitOfWork bluePrintsEntitiesUnitOfWork = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
            List<string> indirectPhases = bluePrintsEntitiesUnitOfWork.PHASES.Where(x => x.PHASE_TYPE == Common.PhaseType.Indirect).Select(x => x.INTERNAL_NUM).ToList();
            List<ExoSubJobProjection> tempQueryJobs = ExoQueries.GetNativeExoSubJobProjection(primeroEntitiesUnitOfWork, LoadPROJECT, ref jobLines).ToList();
            QueryJobs = new List<ExoSubJobProjection>();
            foreach (ExoSubJobProjection tempQueryJob in tempQueryJobs)
            {
                if (tempQueryJob.SubJobId == null)
                    continue;

                foreach (string indirectPhase in indirectPhases)
                {
                    if (tempQueryJob.SubJobCode.Contains(indirectPhase))
                        QueryJobs.Add(tempQueryJob);
                }
            }

            List<ExoSubJobProjection> uniqueQueryJobs = new List<ExoSubJobProjection>();

            foreach (ExoSubJobProjection queryJob in QueryJobs)
            {
                if (!uniqueQueryJobs.Any(x => x.FullCode == queryJob.FullCode))
                    uniqueQueryJobs.Add(queryJob);
            }

            QueryJobs = uniqueQueryJobs.OrderBy(x => x.FullCode).ToList();
            queryJobLines = jobLines;
        }
        #endregion

        #region View Properties

        /// <summary>
        /// Show stock code even when it is not valid
        /// </summary>
        public void CustomColumnDisplayText(CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName == columnStockItem && e.Row != null)
            {
                DataRowView dataRowView = (DataRowView)e.Row;
                if(dataRowView.Row[columnStockItem] != DBNull.Value)
                {
                    string stockCode = dataRowView.Row[columnStockItem].ToString();
                    e.DisplayText = stockCode;
                }
            }
        }

        DataTable dataPointsTable = null;
        public DataTable DataPointsTable
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return dataPointsTable;
            }
        }
        
        private void updateDataPointsTable()
        {
            GridControlService.GridControl.BeginDataUpdate();
            dataPointsTable = new DataTable();
            dataPointsTable.RowChanged += DataPointsTable_RowChanged;
            InitializeColumnSource(ParentViewColumns, ParentSummaries, alignedDataDateCollection, false);
            LoadingScreenManager.ShowLoadingScreen(MainViewModel.Entities.Count);
            LoadingScreenManager.SetMessage("Preparing View...");

            //construct data points table
            dataPointsTable.Columns.Add(columnFullCode, typeof(string));
            dataPointsTable.Columns.Add(columnCommodityName, typeof(string));
            dataPointsTable.Columns.Add(columnDescription, typeof(string));
            dataPointsTable.Columns.Add(columnStockItem, typeof(string));
            dataPointsTable.Columns.Add(columnReference, typeof(string));
            dataPointsTable.Columns.Add(columnNote, typeof(string));
            dataPointsTable.Columns.Add(columnUOM, typeof(string));
            dataPointsTable.Columns.Add(columnProjection, typeof(ExoSubJobProjection));
            dataPointsTable.Columns.Add(columnForecastJob, typeof(FORECAST_JOB));
            dataPointsTable.Columns.Add(columnStockItemName, typeof(string));
            dataPointsTable.Columns.Add(columnStockItemGroup2, typeof(string));
            dataPointsTable.Columns.Add(columnStockItemErrorImageWidth, typeof(decimal));
            dataPointsTable.Columns.Add(columnRecommendedForecastRate, typeof(decimal));
            dataPointsTable.Columns.Add(columnTotalHours, typeof(decimal));
            dataPointsTable.Columns.Add(columnTotalCosts, typeof(decimal));

            foreach (DateTime alignedDataDate in alignedDataDateCollection)
            {
                string columnFieldName = alignedDataDate.Date.ToString(BluePrintsResources.ColumnDateFormat);
                dataPointsTable.Columns.Add(columnFieldName, typeof(decimal));
            }

            List<ExoDataPoint> allDataPoints = new List<ExoDataPoint>();
            List<FORECAST_JOB> deleteJobs = new List<FORECAST_JOB>();
            foreach (FORECAST_JOB job in MainViewModel.Entities)
            {
                //when job doesn't exist in EXO delete it
                ExoSubJobProjection projection = QueryJobs.Where(x => x.CommodityId != null && x.DisciplineId != null && x.SubJobId != null).FirstOrDefault(x => x.CommodityCode == job.COMMODITY_CODE && x.DisciplineCode == job.DISCIPLINE_CODE && x.SubJobCode == job.SUBJOB_CODE && x.VariationCode == job.VARIATION_CODE);
                if (projection == null)
                {
                    job.DELETE_REASON = "Removed from Budget Input";
                    deleteJobs.Add(job);
                    continue;
                }

                DataRow newRow = dataPointsTable.NewRow();
                newRow[columnFullCode] = projection.FullCode;
                newRow[columnProjection] = projection;
                newRow[columnForecastJob] = job;
                mapJobDataToDatatable(newRow);
                dataPointsTable.Rows.Add(newRow);

                job.ForecastJobHours = MainEntityUnitOfWork.FORECAST_JOB_HOURS.Where(x => x.GUID_FORECAST_JOB == job.GUID).ToList();
                job.BluePrintsEntitiesUnitOfWork = MainEntityUnitOfWork;
                foreach (DateTime alignedDate in alignedDataDateCollection)
                {
                    string columnFieldName = alignedDate.Date.ToString(BluePrintsResources.ColumnDateFormat);

                    FORECAST_JOB_HOUR jobHourOnAlignedDate = job.ForecastJobHours.FirstOrDefault(x => x.GUID_FORECAST_JOB == job.GUID && x.FORECAST_DATE.Date == alignedDate.Date);
                    if (jobHourOnAlignedDate != null && jobHourOnAlignedDate.FORECAST_HOUR != null)
                        newRow[columnFieldName] = jobHourOnAlignedDate.FORECAST_HOUR;
                    else
                        newRow[columnFieldName] = DBNull.Value;
                }

                updateRowReadOnlyAttributes(newRow);

                LoadingScreenManager.Progress();
            }

            GridControlService.GridControl.EndDataUpdate();
            LoadingScreenManager.SetMessage("Deleting deprecated indirect forecasts");
            if(deleteJobs.Count() > 0)
            {
                //saves the delete reason
                MainViewModel.UnitOfWork.SaveChanges();

                foreach (FORECAST_JOB deleteJob in deleteJobs)
                    MainViewModel.UnitOfWork.FORECAST_JOBS.Remove(deleteJob);

                MainViewModel.UnitOfWork.SaveChanges();
            }

            LoadingScreenManager.CloseLoadingScreen();
        }

        private void mapJobDataToDatatable(DataRow row)
        {
            FORECAST_JOB forecastJob = (FORECAST_JOB)row[columnForecastJob];
            row[columnDescription] = forecastJob.DESCRIPTION;
            row[columnStockItem] = forecastJob.STOCK_ITEM;
            row[columnReference] = forecastJob.REFERENCE;
            row[columnNote] = forecastJob.NOTE;
            row[columnUOM] = forecastJob.UOM;
        }

        private void mapDataTableToJobData(DataRow row)
        {
            FORECAST_JOB forecastJob = (FORECAST_JOB)row[columnForecastJob];
            forecastJob.DESCRIPTION = row[columnDescription].ToString();
            forecastJob.STOCK_ITEM = row[columnStockItem].ToString();
            forecastJob.REFERENCE = row[columnReference].ToString();
            forecastJob.NOTE = row[columnNote].ToString();
            forecastJob.UOM = row[columnUOM].ToString();
        }

        private void updateRowReadOnlyAttributes(DataRow row, bool isNewRow = false)
        {
            if (row[columnProjection] == DBNull.Value)
                return;

            //update commodity name
            ExoSubJobProjection projection = (ExoSubJobProjection)row[columnProjection];
            FORECAST_JOB forecastJob = (FORECAST_JOB)row[columnForecastJob];
            JOB_COSTTYPES findCOST_TYPE = JOB_COSTTYPESCollection.FirstOrDefault(x => x.SHORTCODE == projection.CommodityCode);
            if (findCOST_TYPE != null)
                row[columnCommodityName] = findCOST_TYPE.COSTDESC;
            else
                row[columnCommodityName] = string.Empty;

            //update stock item name          
            if (forecastJob.STOCK_ITEM != string.Empty)
            {
                STOCK_ITEMS findSTOCK_ITEM = STOCK_ITEMCollection.FirstOrDefault(x => x.STOCKCODE == forecastJob.STOCK_ITEM);
                if (findSTOCK_ITEM != null)
                {
                    row[columnStockItemName] = findSTOCK_ITEM.DESCRIPTION;
                    STOCK_GROUP2S findSTOCK_GROUP2S = STOCK_GROUP2SCollection.FirstOrDefault(x => x.GROUPNO == findSTOCK_ITEM.STOCKGROUP2);
                    if (findSTOCK_GROUP2S != null)
                        row[columnStockItemGroup2] = findSTOCK_GROUP2S.GROUPNAME;

                    decimal? newDecimalValue = null;
                    if (findSTOCK_ITEM.STDCOST != null && findSTOCK_ITEM.STDCOST > 0)
                        newDecimalValue = Convert.ToDecimal(findSTOCK_ITEM.STDCOST);

                    if (newDecimalValue == null)
                    {
                        row[columnRecommendedForecastRate] = DBNull.Value;
                        if(isNewRow)
                            forecastJob.FORECAST_RATE = 0;
                    }
                    else
                    {
                        row[columnRecommendedForecastRate] = newDecimalValue;
                        if (isNewRow)
                            forecastJob.FORECAST_RATE = newDecimalValue;
                    }

                    row[columnStockItemErrorImageWidth] = 0m;
                }
                else
                {
                    row[columnStockItemErrorImageWidth] = 15m;
                    row[columnStockItemName] = string.Empty;
                    row[columnStockItemGroup2] = string.Empty;
                }
            }
            else
            {
                row[columnStockItemName] = string.Empty;
                row[columnStockItemGroup2] = string.Empty;
                row[columnStockItemErrorImageWidth] = 0m;
            }

            //update total hours
            decimal rate = 0.00m;
            decimal totalForecastHours = forecastJob.ForecastJobHours.Where(x => x.FORECAST_HOUR != null && x.FORECAST_DATE > FixedDataDate).Sum(x => (decimal)x.FORECAST_HOUR);
            row[columnTotalHours] = totalForecastHours;

            //update total costs
            if (forecastJob.FORECAST_RATE != null)
            {
                rate = (decimal)forecastJob.FORECAST_RATE;
                row[columnTotalCosts] = rate * totalForecastHours;
            }
            else
                row[columnTotalCosts] = 0.00m;
        }

        private string findDefaultStockCode(ExoSubJobProjection exoSubJobProjection)
        {
            if (exoSubJobProjection.DisciplineId == null || exoSubJobProjection.CommodityId == null)
                return string.Empty;

            JOB_COSTGROUPS findJOB_COSTGROUPS = JOB_COSTGROUPSCollection.FirstOrDefault(x => x.SHORTCODE == exoSubJobProjection.DisciplineCode);
            if (findJOB_COSTGROUPS == null)
                return string.Empty;

            JOB_COSTTYPES findJOB_COSTTYPES = JOB_COSTTYPESCollection.FirstOrDefault(x => x.SHORTCODE == exoSubJobProjection.CommodityCode);
            if (findJOB_COSTTYPES == null)
                return string.Empty;

            STOCK_ITEMS findSTOCK_ITEMS = STOCK_ITEMCollection.FirstOrDefault(x => x.COSTGROUP == findJOB_COSTGROUPS.SEQNO && x.COSTTYPE == findJOB_COSTTYPES.SEQNO);
            if (findSTOCK_ITEMS == null)
                return string.Empty;

            return findSTOCK_ITEMS.STOCKCODE;
        }

        public override void PastingFromClipboard(PastingFromClipboardEventArgs e)
        {
            GridControl gridControl = (GridControl)e.Source;
            TableView gridTableView = (TableView)gridControl.View;
            string newValueString = Clipboard.GetText().ToString();

            if (newValueString == string.Empty)
                newValueString = " ";
            //remove tab in front
            if (newValueString != string.Empty)
            {
                //if (newValueString.Substring(0, 1) == "\t")
                //{
                //    newValueString = newValueString.Substring(1, newValueString.Length - 1);
                //}

                string[] RowData = DataUtils.ExcelSplit(newValueString).ToArray();
                List<ErrorMessage> errorMessages;

                if (MainViewModel.SelectMode == MultiSelectMode.Row)
                    errorMessages = pasteRowData(gridTableView, RowData);
                else
                    pasteCellData(gridControl, gridTableView, RowData, out errorMessages);

                GridControlService.GridControl.RefreshData();

                e.Handled = true;
                ShowErrorMessage("Errors", errorMessages);
            }
        }

        /// <summary>
        /// Paste data from string into rows
        /// </summary>
        /// <param name="RowData">Invalid rows</param>
        /// <returns></returns>
        private List<ErrorMessage> pasteRowData(TableView gridTableView, string[] RowData)
        {
            EntitiesUndoRedoManager.PauseActionId();
            List<ErrorMessage> errorMessages = new List<ErrorMessage>();


            List<DataRow> saveDataRows = new List<DataRow>();
            foreach (var Row in RowData)
            {
                var ColumnStrings = Row.Split('\t');
                string fullCode = ColumnStrings[0];
                ExoSubJobProjection queryJob = QueryJobs.FirstOrDefault(x => x.FullCode == fullCode);
                if (queryJob != null)
                {
                    DataRow newRow = addNewPasteRow(queryJob, gridTableView, ColumnStrings, out errorMessages);
                    saveDataRows.Add(newRow);
                }
                else
                {
                    //try to see if user mistaken stock code as commodity code
                    if(fullCode.Length > 3)
                    {
                        string stockCode = fullCode.Substring(fullCode.Length - 3, 3);
                        COMMODITY_CODE findCOMMODITY_CODE = COMMODITY_CODECollection.FirstOrDefault(x => x.DEFAULT_STOCKCODE == stockCode);
                        if(findCOMMODITY_CODE != null)
                        {
                            string oldCode = fullCode;
                            fullCode = fullCode.Replace(stockCode, findCOMMODITY_CODE.CODE);
                            queryJob = QueryJobs.FirstOrDefault(x => x.FullCode == fullCode);
                            if(queryJob != null)
                            {
                                DataRow newRow = addNewPasteRow(queryJob, gridTableView, ColumnStrings, out errorMessages);
                                saveDataRows.Add(newRow);
                                errorMessages.Add(new ErrorMessage(oldCode, "Row is pasted, but " + oldCode + " has been remapped to " + fullCode + ", because " + stockCode + " is a stock code"));
                            }
                            else
                                errorMessages.Add(new ErrorMessage(fullCode, "Row is not pasted, because exo job doesn't exists"));
                        }
                        else
                            errorMessages.Add(new ErrorMessage(fullCode, "Row is not pasted, because commodity code doesn't exists"));
                    }
                    else
                        errorMessages.Add(new ErrorMessage(fullCode, "Row is not pasted, because of invalid WBS code doesn't exists"));
                }

            }

            bulkSaveDataRows(saveDataRows);
            focusNewlyAddedProjectionTimer.Start();
            EntitiesUndoRedoManager.UnpauseActionId();
            LoadingScreenManager.CloseLoadingScreen();
            return errorMessages;
        }

        private void bulkSaveDataRows(IEnumerable<DataRow> saveDataRows)
        {
            LoadingScreenManager.ShowLoadingScreen(saveDataRows.Count());
            LoadingScreenManager.SetMessage("Saving Rows...");
            foreach (DataRow saveDataRow in saveDataRows)
            {
                FORECAST_JOB forecastJob = (FORECAST_JOB)saveDataRow[columnForecastJob];
                forecastJob.PrepareForSaveChanges(false);
                updateRowReadOnlyAttributes(saveDataRow);
                LoadingScreenManager.Progress();
            }

            MainEntityUnitOfWork.SaveChanges();
            shouldPromptForSavingSnapshot = true;
        }

        public bool CanUpdateFloatingRates()
        {
            return !IsLoading;
        }

        public void UpdateFloatingRates()
        {
            if (MessageBoxService.ShowMessage("This will update all rates that have floating ticked to recommended rate, do you wish to continue?", "Confirmation", MessageButton.OKCancel) == MessageResult.Cancel)
                return;

            int floatingRateUpdateCount = 0;
            EntitiesUndoRedoManager.PauseActionId();
            foreach (DataRow dataRow in DataPointsTable.Rows)
            {
                FORECAST_JOB forecastJob = (FORECAST_JOB)dataRow[columnForecastJob];
                if (forecastJob.IS_FLOATING_RATE && dataRow[columnRecommendedForecastRate] != DBNull.Value)
                {
                    decimal? oldValue = forecastJob.FORECAST_RATE;
                    decimal? newValue = dataRow[columnRecommendedForecastRate] == DBNull.Value ? null : (decimal?)dataRow[columnRecommendedForecastRate];
                    EntitiesUndoRedoManager.AddUndo(updateForecastJobFromDataRow(dataRow), columnForecastRate, oldValue, newValue, EntityMessageType.Changed);
                    commitCellValue(columnForecastRate, dataRow, newValue);
                    floatingRateUpdateCount += 1;
                }
            }
            EntitiesUndoRedoManager.UnpauseActionId();

            GridControlService.GridControl.RefreshData();
            MessageBoxService.ShowMessage(floatingRateUpdateCount.ToString() + " record(s) updated", "Update", MessageButton.OK);
        }

        private DataRow addNewPasteRow(ExoSubJobProjection queryJob, TableView gridTableView, string[] ColumnStrings, out List<ErrorMessage> errorMessages)
        {
            errorMessages = new List<ErrorMessage>();
            DataRow newRow = DataPointsTable.NewRow();
            newRow[columnFullCode] = queryJob.FullCode;
            newRow[columnProjection] = queryJob;
            findExistingOrAddNewFORECAST_JOB(newRow, false);
            for (var i = 1; i < ColumnStrings.Count(); i++)
            {
                if (i > gridTableView.VisibleColumns.Count - 1)
                    continue;

                string pasteData = ColumnStrings[i];
                ColumnBase copyColumn = gridTableView.VisibleColumns[i];
                basePasteData(newRow, copyColumn, pasteData, true, out errorMessages);
            }

            DataPointsTable.Rows.Add(newRow);
            EntitiesUndoRedoManager.AddUndo(updateForecastJobFromDataRow(newRow), null, null, null, EntityMessageType.Added);
            return newRow;
        }

        private void DataPointsTable_RowChanged(object sender, DataRowChangeEventArgs e)
        {
            if (e.Action == DataRowAction.Add)
            {
                if (IsLoading)
                    return;

                int rowIndex = DataPointsTable.Rows.IndexOf(e.Row);
                if (rowIndex >= 0)
                {
                    DataRowView dataRowView = DataPointsTable.DefaultView[DataPointsTable.Rows.IndexOf(e.Row)];
                    OnAfterNewProjectionsAdded(dataRowView);
                }
            }
        }

        protected virtual void OnAfterNewProjectionsAdded(DataRowView newRow)
        {
            if (newRow != null)
            {
                if (newlyAddedRows == null)
                    newlyAddedRows = new List<DataRowView>();

                newlyAddedRows.Add(newRow);
                //Uncomment this to allow grid to focus on new row
                focusNewlyAddedProjectionTimer.Tick -= FocusNewlyAddedProjectionTimer_Tick;
                focusNewlyAddedProjectionTimer.Tick += FocusNewlyAddedProjectionTimer_Tick;
                focusNewlyAddedProjectionTimer.Start();
            }
        }

        List<DataRowView> newlyAddedRows;
        private void FocusNewlyAddedProjectionTimer_Tick(object sender, EventArgs e)
        {
            focusNewlyAddedProjectionTimer.Stop();
            if (Entities == null || newlyAddedRows == null || newlyAddedRows.Count() == 0)
                return;

            List<DataRowView> selectedRows = new List<DataRowView>();
            foreach (DataRowView newlyAddedRow in newlyAddedRows)
            {
                selectedRows.Add(newlyAddedRow);
            }

            newlyAddedRows.Clear();
            SelectedDataRows?.Clear();
            foreach (DataRowView selectedRow in selectedRows)
            {
                SelectedDataRows?.Add(selectedRow);
            }

            if (selectedRows.Count > 0)
            {
                SelectedDataRow = selectedRows.Last();
                this.RaisePropertyChanged(x => x.SelectedDataRows);
                this.RaisePropertyChanged(x => x.SelectedDataRow);
            }
        }

        private void pasteCellData(GridControl gridControl, TableView gridTableView, string[] RowData, out List<ErrorMessage> errorMessages)
        {
            EntitiesUndoRedoManager.PauseActionId();
            List<DataRow> saveDataRows = GridControlHelpers.PasteCellData(gridControl, gridTableView, RowData, basePasteData, out errorMessages);
            bulkSaveDataRows(saveDataRows);
            EntitiesUndoRedoManager.UnpauseActionId();

            GridControlService.RefreshSummary();
        }

        private bool basePasteData(DataRow newRow, ColumnBase copyColumn, string pasteData, bool isLastRow, out List<ErrorMessage> errorMessages)
        {
            errorMessages = new List<ErrorMessage>();
            DateTime columnDateTime;
            if(copyColumn.FieldName == columnFullCode)
            {
                FORECAST_JOB forecastJob = (FORECAST_JOB)newRow[columnForecastJob];
                ExoSubJobProjection queryJob = QueryJobs.FirstOrDefault(x => x.FullCode == pasteData);
                if(queryJob != null)
                {
                    ExoSubJobProjection oldJob = forecastJob.ExoJob;
                    forecastJob.ExoJob = queryJob;
                    if(MainViewModel.IsPasteCellLevel)
                        EntitiesUndoRedoManager.AddUndo(forecastJob, copyColumn.FieldName, oldJob, queryJob, EntityMessageType.Changed);
                }
            }
            else if(copyColumn.FieldName.Contains(BindableBase.GetPropertyName(() => new FORECAST_JOB().STOCK_ITEM)))
            {
                FORECAST_JOB forecastJob = (FORECAST_JOB)newRow[columnForecastJob];
                string oldStockItem = forecastJob.STOCK_ITEM;
                forecastJob.STOCK_ITEM = pasteData;

                commitCellValue(copyColumn.FieldName, newRow, pasteData, true, false);
                if (MainViewModel.IsPasteCellLevel)
                    EntitiesUndoRedoManager.AddUndo(forecastJob, copyColumn.FieldName, oldStockItem, pasteData, EntityMessageType.Changed);
            }
            else if (copyColumn.FieldName.Contains(BindableBase.GetPropertyName(() => new FORECAST_JOB().IS_FLOATING_RATE)))
            {
                FORECAST_JOB forecastJob = (FORECAST_JOB)newRow[columnForecastJob];
                bool oldCheckedStatus = forecastJob.IS_FLOATING_RATE;
                bool newCheckedStatus = pasteData.ToUpper() == "CHECKED" ? true : false;
                forecastJob.IS_FLOATING_RATE = newCheckedStatus;

                commitCellValue(copyColumn.FieldName, newRow, newCheckedStatus, true, false);
                if (MainViewModel.IsPasteCellLevel)
                    EntitiesUndoRedoManager.AddUndo(updateForecastJobFromDataRow(newRow), copyColumn.FieldName, oldCheckedStatus, newCheckedStatus, EntityMessageType.Changed);
            }
            else if (copyColumn.FieldName.Contains(BindableBase.GetPropertyName(() => new FORECAST_JOB().FORECAST_RATE)) || DateTime.TryParse(copyColumn.FieldName, out columnDateTime))
            {
                var rgx = new Regex(BluePrintsResources.Regex_NumbersOnly);
                var cleanColumnString = rgx.Replace(pasteData, string.Empty);
                object oldValue;
                if(copyColumn.FieldName.Contains(BindableBase.GetPropertyName(() => new FORECAST_JOB().FORECAST_RATE)))
                {
                    FORECAST_JOB forecastJob = (FORECAST_JOB)newRow[columnForecastJob];
                    oldValue = forecastJob.FORECAST_RATE;
                }
                else
                    oldValue = newRow[copyColumn.FieldName] == DBNull.Value ? (decimal?)null : (decimal)newRow[copyColumn.FieldName];

                decimal decimal_value;
                if (decimal.TryParse(cleanColumnString, out decimal_value))
                {
                    commitCellValue(copyColumn.FieldName, newRow, decimal_value, true, false);
                    if (MainViewModel.IsPasteCellLevel)
                        EntitiesUndoRedoManager.AddUndo(updateForecastJobFromDataRow(newRow), copyColumn.FieldName, oldValue, decimal_value, EntityMessageType.Changed);
                }
                else
                {
                    commitCellValue(copyColumn.FieldName, newRow, null, true, false);
                    if (MainViewModel.IsPasteCellLevel)
                        EntitiesUndoRedoManager.AddUndo(updateForecastJobFromDataRow(newRow), copyColumn.FieldName, oldValue, null, EntityMessageType.Changed);
                }
            }
            else if (copyColumn.FieldType == typeof(string) && !copyColumn.ReadOnly)
            {
                if(copyColumn.FieldName.Contains(columnForecastJob))
                {
                    string sanitisedPropertyName = copyColumn.FieldName.Replace(columnForecastJob + ".", "");
                    FORECAST_JOB forecastJob = (FORECAST_JOB)newRow[columnForecastJob];
                    object oldValue = DataUtils.GetNestedValue(sanitisedPropertyName, forecastJob);
                    DataUtils.SetNestedValue(sanitisedPropertyName, forecastJob, pasteData);
                    commitCellValue(copyColumn.FieldName, newRow, pasteData, true, false);
                    if (MainViewModel.IsPasteCellLevel)
                        EntitiesUndoRedoManager.AddUndo(updateForecastJobFromDataRow(newRow), copyColumn.FieldName, oldValue, pasteData, EntityMessageType.Changed);
                }
                else
                {
                    string oldValue = newRow[copyColumn.FieldName].ToString();
                    commitCellValue(copyColumn.FieldName, newRow, pasteData, true, false);
                    if (MainViewModel.IsPasteCellLevel)
                        EntitiesUndoRedoManager.AddUndo(updateForecastJobFromDataRow(newRow), copyColumn.FieldName, oldValue, pasteData, EntityMessageType.Changed);
                }
            }

            return true;
        }

        /// <summary>
        /// Influence column(s) when changes happens in other column
        /// </summary>
        public void NewIndirectRowAddUndoAndSave(RowEventArgs e)
        {
            if (e.RowHandle == DataControlBase.NewItemRowHandle)
            {
                EntitiesUndoRedoManager.PauseActionId();

                DataRowView row = (DataRowView)e.Row;

                findExistingOrAddNewFORECAST_JOB(row.Row, true);
                shouldPromptForSavingSnapshot = true;
                EntitiesUndoRedoManager.AddUndo(updateForecastJobFromDataRow(row.Row), null, null, null, EntityMessageType.Added);
                focusNewlyAddedProjectionTimer.Start();
                //added not working well atm because when row is removed from datatable its itemarray is cleared
                //EntitiesUndoRedoManager.AddUndo(row.Row, null, null, null, EntityMessageType.Added);
                EntitiesUndoRedoManager.UnpauseActionId();
            }
        }

        private void findExistingOrAddNewFORECAST_JOB(DataRow row, bool commitToDb)
        {
            if (row[columnFullCode] == DBNull.Value)
                return;

            FORECAST_JOB editFORECAST_JOB = row[columnForecastJob] == DBNull.Value ? null : (FORECAST_JOB)row[columnForecastJob];
                
            //MainViewModel.Entities.FirstOrDefault(x => x.GUID == guidToSearch);
            if (editFORECAST_JOB == null)
            {
                editFORECAST_JOB = new FORECAST_JOB();
                editFORECAST_JOB.BluePrintsEntitiesUnitOfWork = MainEntityUnitOfWork;
            }
            else
            {
                FORECAST_JOB findExistingFORECAST_JOB = MainEntityUnitOfWork.FORECAST_JOBS.FirstOrDefault(x => x.GUID == editFORECAST_JOB.GUID);
                if (findExistingFORECAST_JOB == null)
                    editFORECAST_JOB.GUID = Guid.Empty;
            }

            ExoSubJobProjection projection = QueryJobs.FirstOrDefault(x => x.FullCode == row[columnFullCode].ToString());
            if (projection != null)
            {
                editFORECAST_JOB.ExoJob = projection;
                editFORECAST_JOB.SUBJOB_CODE = projection.SubJobCode;
                editFORECAST_JOB.DISCIPLINE_CODE = projection.DisciplineCode;
                editFORECAST_JOB.COMMODITY_CODE = projection.CommodityCode;
                if (projection.VariationCode == null)
                    editFORECAST_JOB.VARIATION_CODE = string.Empty;
                else
                    editFORECAST_JOB.VARIATION_CODE = projection.VariationCode;

                editFORECAST_JOB.DELETE_REASON = string.Empty;
                editFORECAST_JOB.GUID_PROJECT = LoadPROJECT.GUID;
                editFORECAST_JOB.PrepareForSaveChanges(commitToDb);
                //MainViewModel.Save(editFORECAST_JOB);
                row[columnForecastJob] = editFORECAST_JOB;
                //add undo must be after so that Guid is populated
            }
        }

        private DataRow createNewDataRowFromFORECAST_JOB(FORECAST_JOB job)
        {
            DataRow row = DataPointsTable.NewRow();
            row[columnFullCode] = job.ExoJob.ToString();
            row[columnForecastJob] = job;
            row[columnProjection] = job.ExoJob;
            mapJobDataToDatatable(row);

            foreach(KeyValuePair<string, decimal> datesForecast in job.DatesForecasts)
            {
                row[datesForecast.Key] = datesForecast.Value;
            }

            return row;
        }

        private FORECAST_JOB updateForecastJobFromDataRow(DataRow row)
        {
            FORECAST_JOB forecastJob = (FORECAST_JOB)row[columnForecastJob];
            if(!EntitiesUndoRedoManager.IsInUndoRedoOperation)
            {
                if (row[columnProjection] != DBNull.Value)
                    forecastJob.ExoJob = (ExoSubJobProjection)row[columnProjection];
                else
                    forecastJob.ExoJob = null;

                mapDataTableToJobData(row);
                forecastJob.DatesForecasts.Clear();
                foreach (DataColumn column in DataPointsTable.Columns)
                {
                    DateTime dateTime;
                    if (DateTime.TryParse(column.ColumnName, out dateTime))
                    {
                        decimal dateValue = row[column.ColumnName] == DBNull.Value ? 0.00m : ((decimal)row[column.ColumnName]);
                        forecastJob.DatesForecasts.Add(new KeyValuePair<string, decimal>(column.ColumnName, dateValue));
                    }
                }
            }

            return forecastJob;
        }

        /// <summary>
        /// Influence column(s) when changes happens in other column
        /// </summary>
        public void CellValueChangedUpdate(CellValueChangedEventArgs e)
        {
            DataRowView dataRowView = (DataRowView)e.Row;
            if (e.RowHandle == GridControl.AutoFilterRowHandle)
                return;

            //new item handling
            if (e.RowHandle == GridControl.NewItemRowHandle)
            {

                FORECAST_JOB newFORECAST_JOB;
                if (dataRowView[columnForecastJob] == DBNull.Value)
                {
                    newFORECAST_JOB = new FORECAST_JOB();
                    newFORECAST_JOB.BluePrintsEntitiesUnitOfWork = MainEntityUnitOfWork;
                    dataRowView[columnForecastJob] = newFORECAST_JOB;
                }
                else
                    newFORECAST_JOB = (FORECAST_JOB)dataRowView[columnForecastJob];

                if (e.Column.FieldName == columnFullCode && e.Value != null)
                {
                    ExoSubJobProjection queryJob = QueryJobs.FirstOrDefault(x => x.FullCode == e.Value.ToString());
                    dataRowView[columnProjection] = queryJob;

                    if(dataRowView[columnStockItem] == DBNull.Value || dataRowView[columnStockItem].ToString() == string.Empty)
                        dataRowView[columnStockItem] = findDefaultStockCode(queryJob);
                }

                mapDataTableToJobData(dataRowView.Row);
                updateRowReadOnlyAttributes(dataRowView.Row, true);
                return;
            }

            //existing item handling
            EntitiesUndoRedoManager.PauseActionId();
            string fieldName = e.Column.FieldName;

            commitCellValue(fieldName, dataRowView.Row, e.Value);
            EntitiesUndoRedoManager.AddUndo(updateForecastJobFromDataRow(dataRowView.Row), fieldName, e.OldValue, e.Value, EntityMessageType.Changed);
            EntitiesUndoRedoManager.UnpauseActionId();

            GridControlService.RefreshSummary();
            e.Handled = true;
        }

        public void UpdateRecommendedRate()
        {
            EntitiesUndoRedoManager.PauseActionId();
            foreach (DataRowView row in SelectedDataRows)
            {
                FORECAST_JOB forecastJob = (FORECAST_JOB)row[columnForecastJob];
                if (forecastJob.STOCK_ITEM != string.Empty)
                {
                    STOCK_ITEMS findSTOCK_ITEM = STOCK_ITEMCollection.FirstOrDefault(x => x.STOCKCODE == forecastJob.STOCK_ITEM);
                    if (findSTOCK_ITEM != null)
                    {
                        decimal? oldDecimalValue = null;
                        oldDecimalValue = forecastJob.FORECAST_RATE;

                        decimal? newDecimalValue = null;
                        if (findSTOCK_ITEM.STDCOST != null && findSTOCK_ITEM.STDCOST > 0)
                            newDecimalValue = Convert.ToDecimal(findSTOCK_ITEM.STDCOST);

                        forecastJob.FORECAST_RATE = newDecimalValue;
                        EntitiesUndoRedoManager.AddUndo(updateForecastJobFromDataRow(row.Row), columnForecastRate, oldDecimalValue, newDecimalValue, EntityMessageType.Changed);
                    }
                }
            }
            EntitiesUndoRedoManager.UnpauseActionId();
        }

        protected virtual void commitCellValue(string fieldName, DataRow row, object newValue, bool skipUpdate = false, bool saveChanges = true)
        {
            DateTime dateTime;
            FORECAST_JOB forecastJob = (FORECAST_JOB)row[columnForecastJob];
            if (fieldName == columnFullCode)
            {
                if(newValue != null && newValue.ToString() != string.Empty)
                {
                    ExoSubJobProjection job = QueryJobs.FirstOrDefault(x => x.FullCode == newValue.ToString());
                    if (job != null)
                    {
                        row[columnFullCode] = job.ToString();
                        forecastJob.ExoJob = job;
                        row[columnProjection] = job;
                    }
                    else
                    {
                        row[columnFullCode] = string.Empty;
                        row[columnProjection] = DBNull.Value;
                    }

                    if(row[columnStockItem] == DBNull.Value || row[columnStockItem].ToString() == string.Empty)
                    {
                        string defaultStockItem = findDefaultStockCode(job);
                        row[columnStockItem] = defaultStockItem;
                    }

                    updateForecastJobFromDataRow(row);
                    findExistingOrAddNewFORECAST_JOB(row, saveChanges);

                    if(saveChanges)
                        shouldPromptForSavingSnapshot = true;

                    updateRowReadOnlyAttributes(row);
                    return;
                }
            }
            else if (DateTime.TryParse(fieldName, out dateTime))
            {
                decimal? forecastHours = null;
                decimal convertUnits = 0;
                if (newValue != null && decimal.TryParse(newValue.ToString(), out convertUnits))
                {
                    if (convertUnits == 0)
                        forecastHours = null;
                    else
                        forecastHours = convertUnits;
                }

                FORECAST_JOB_HOUR forecastJobHour = forecastJob.ForecastJobHours.FirstOrDefault(x => x.GUID_FORECAST_JOB == forecastJob.GUID && x.FORECAST_DATE.Date == dateTime.Date);
                FORECAST_JOB_HOUR editForecastJobHour;
                if (forecastJobHour == null)
                {
                    editForecastJobHour = new FORECAST_JOB_HOUR();
                    forecastJob.ForecastJobHours.Add(editForecastJobHour);
                }
                else
                    editForecastJobHour = forecastJobHour;

                editForecastJobHour.FORECAST_DATE = dateTime.Date;
                editForecastJobHour.GUID_FORECAST_JOB = forecastJob.GUID;
                editForecastJobHour.FORECAST_HOUR = forecastHours;

                if (saveChanges)
                {
                    forecastJob.PrepareForSaveChanges(saveChanges);
                    shouldPromptForSavingSnapshot = true;
                }
                //FORECAST_JOB_HOURCollectionViewModel.Save(editForecastJobHour);

                //for undo/redo
                if (forecastHours == null)
                    row[fieldName] = DBNull.Value;
                else
                    row[fieldName] = forecastHours;

                updateForecastJobFromDataRow(row);
            }
            else if(fieldName.Contains(columnForecastJob))
            {
                string sanitisedPropertyName = fieldName.Replace(columnForecastJob + ".", "");
                DataUtils.SetNestedValue(sanitisedPropertyName, forecastJob, newValue);

                if (saveChanges)
                {
                    forecastJob.PrepareForSaveChanges(saveChanges);
                    shouldPromptForSavingSnapshot = true;
                }

                //findExistingOrAddNewFORECAST_JOB(row);
            }
            else if(DataPointsTable.Columns[fieldName].DataType == typeof(string))
            {
                row[fieldName] = newValue;
                mapDataTableToJobData(row);

                if (saveChanges)
                {
                    forecastJob.PrepareForSaveChanges(saveChanges);
                    shouldPromptForSavingSnapshot = true;
                }

                //findExistingOrAddNewFORECAST_JOB(row);
            }

            if (!skipUpdate)
                updateRowReadOnlyAttributes(row);
        }

        private DataRow searchRow(Guid guid)
        {
            foreach(DataRow dr in DataPointsTable.Rows)
            {
                if (dr[columnForecastJob] == DBNull.Value)
                    continue;

                if (((FORECAST_JOB)dr[columnForecastJob]).GUID == guid)
                    return dr;
            }

            return null;
        }

        static string columnFullCode = "FullCode";
        static string columnForecastJob = BluePrintsResources.Forecast_ForecastJobColumn;
        static string columnCommodityName = "CommodityName";
        static string columnProjection = "Projection";
        static string columnStockItemName = "StockItemName";
        static string columnStockItemGroup2 = "StockItemGroup2";
        static string columnRecommendedForecastRate = "RecommendedRate";
        static string columnTotalHours = "TOTAL_HOURS";
        static string columnTotalCosts = "TOTAL_COSTS";
        static string columnDescription = "Description";
        static string columnStockItem = "StockItem";
        static string columnStockItemErrorImageWidth = "StockItemErrorImageWidth";
        static string columnReference = "Reference";
        static string columnNote = "Note";
        static string columnUOM = BluePrintsResources.ForecastIndirectUOMColumn;
        static string columnForecastRate = columnForecastJob + "." + BindableBase.GetPropertyName(() => new FORECAST_JOB().FORECAST_RATE);
        private void InitializeColumnSource(ObservableCollection<ColumnDescriptor> columns, ObservableCollection<SummaryDescriptor> summaries, List<DateTime> alignedDates, bool isChild)
        {
            columns.Clear();
            summaries.Clear();

            columns.Add(new ColumnDescriptor() { FieldName = columnFullCode, ReadOnly = false, Header = "Full Code", ItemsSource = QueryJobs, Fixed = FixedStyle.Left, Width = 150, Settings = SettingsType.FullCode });
            summaries.Add(new SummaryDescriptor() { FieldName = columnFullCode, DisplayFormat = "Total {0} Records", Type = SummaryItemType.Count });
            columns.Add(new ColumnDescriptor() { FieldName = columnCommodityName, ReadOnly = true, Header = "Commodity Name (AutoFilled)", Fixed = FixedStyle.Left, Width = 150, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = columnDescription, ReadOnly = false, Header = "Description", Fixed = FixedStyle.Left, Width = 100, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = columnStockItem, ReadOnly = false, Header = "Stock Code", ItemsSource = STOCK_ITEMCollection, HeaderToolTip="Changing this value will automatically populate rate", Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.StockItem });
            columns.Add(new ColumnDescriptor() { FieldName = columnStockItemName, ReadOnly = true, Header = "Stock Name (AutoFilled)", Fixed = FixedStyle.Left, Width = 100, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = columnStockItemGroup2, ReadOnly = true, Header = "Stock Group 2 (AutoFilled)", Fixed = FixedStyle.Left, Width = 100, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = columnReference, ReadOnly = false, Header = "Reference", Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = columnNote, ReadOnly = false, Header = "Note", Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = columnUOM, ReadOnly = false, Header = "UOM", Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = columnRecommendedForecastRate, ReadOnly = true, Header = "Recommended Rate", Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number, Mask = "c0" });
            columns.Add(new ColumnDescriptor() { FieldName = columnForecastRate, ReadOnly = false, Header = "Rate", Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number, Mask = "c0" });
            columns.Add(new ColumnDescriptor() { FieldName = columnForecastJob + "." + BindableBase.GetPropertyName(() => new FORECAST_JOB().IS_FLOATING_RATE), ReadOnly = false, Header = "Floating Rate", Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = columnTotalHours, ReadOnly = true, Header = "Total Hrs", Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number, Mask = "n0" });
            summaries.Add(new SummaryDescriptor() { FieldName = columnTotalHours, DisplayFormat = "n0", Type = SummaryItemType.Sum });
            columns.Add(new ColumnDescriptor() { FieldName = columnTotalCosts, ReadOnly = false, Header = "Total $", Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number, Mask = "c0" });
            summaries.Add(new SummaryDescriptor() { FieldName = columnTotalCosts, DisplayFormat = "c0", Type = SummaryItemType.Sum });

            foreach (DateTime alignedDate in alignedDates.OrderBy(x => x))
            {
                string columnFieldName = alignedDate.Date.ToString(BluePrintsResources.ColumnDateFormat);

                if (alignedDate > FixedDataDateMonthEnd)
                {
                    columns.Add(new ColumnDescriptor() { FieldName = columnFieldName, Header = columnFieldName, Fixed = FixedStyle.None, Width = 60, Settings = SettingsType.ForecastFuture });
                    summaries.Add(new SummaryDescriptor() { FieldName = columnFieldName, DisplayFormat = "n0", Type = SummaryItemType.Sum });
                }
            }
        }
        private List<DateTime> generateDates()
        {
            return ChronologicalHelpers.GenerateEndDatesCollection((DateTime)FixedDataDate, FixedEndDate, true);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.FORECAST_JOBS);
        }

        protected override Func<IRepositoryQuery<FORECAST_JOB>, IQueryable<FORECAST_JOB>> specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_PROJECT == LoadPROJECT.GUID);
        }

        public override void ValidateRow(GridRowValidationEventArgs e)
        {
            DataRow dataRow = ((DataRowView)e.Row).Row;
            string errorMessage = UnifiedRowValidation(dataRow);

            if (errorMessage != string.Empty)
            {
                e.IsValid = false;
                e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Critical;
                e.ErrorContent = errorMessage;
            }
        }

        public override string UnifiedValueValidation(FORECAST_JOB projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }

        public override string UnifiedRowValidation(FORECAST_JOB projection)
        {
            return string.Empty;
        }

        public string UnifiedRowValidation(DataRow dataRow)
        {
            if (dataRow[columnFullCode] == DBNull.Value)
            {
                return "Full code must be entered";
            }
            else
            {
                ExoSubJobProjection projection = QueryJobs.FirstOrDefault(x => x.FullCode == dataRow[columnFullCode].ToString());
                if (projection == null)
                {
                    return "Full code is invalid";
                }
            }

            return string.Empty;
        }

        protected bool shouldPromptForSavingSnapshot;
        public void SaveSnapshot()
        {
            Common.LoadingScreenManager.ShowLoadingScreen(1);
            Common.LoadingScreenManager.SetMessage("Saving Indirect Forecast Snapshot...");
            BluePrintsContextHelper.RefreshIndirectForecastData(LoadPROJECT.NUMBER, (DateTime)LoadDataDate);
            Common.LoadingScreenManager.CloseLoadingScreen();

            shouldPromptForSavingSnapshot = false;
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "FORECAST_JOBCollectionViewModelWrapper_v2"; }
        }

        /// <summary>
        /// Manages all undo and redo operation
        /// </summary>
        private EntitiesUndoRedoManager<FORECAST_JOB> entitiesundoredomanager { get; set; }

        public EntitiesUndoRedoManager<FORECAST_JOB> EntitiesUndoRedoManager
        {
            get
            {
                if (entitiesundoredomanager == null)
                    entitiesundoredomanager = new EntitiesUndoRedoManager<FORECAST_JOB>(BulkPropertyUndo, BulkPropertyRedo);

                return entitiesundoredomanager;
            }
        }

        /// <summary>
        /// Function to undo the entity changes
        /// Must be used in conjunction of EntitiesUndoManager
        /// </summary>
        /// <param name="entityProperty">Entity passed over from EntitiesUndoRedo</param>
        public virtual void BulkPropertyUndo(IEnumerable<UndoRedoEntityInfo<FORECAST_JOB>> entityProperties)
        {
            IEnumerable<UndoRedoEntityInfo<FORECAST_JOB>> bulkAddedProperties = entityProperties.Where(x => x.MessageType == EntityMessageType.Added);
            IEnumerable<UndoRedoEntityInfo<FORECAST_JOB>> bulkSaveProperties = entityProperties.Where(x => x.MessageType == EntityMessageType.Changed);
            IEnumerable<UndoRedoEntityInfo<FORECAST_JOB>> bulkDeleteProperties = entityProperties.Where(x => x.MessageType == EntityMessageType.Deleted);

            foreach (UndoRedoEntityInfo<FORECAST_JOB> entityProperty in bulkSaveProperties)
            {
                DataRow findRow = searchRow(entityProperty.ChangedEntity.GUID);
                if(findRow != null)
                {
                    commitCellValue(entityProperty.PropertyName, findRow, entityProperty.OldValue);
                    updateRowReadOnlyAttributes(findRow);
                }
            }

            foreach (UndoRedoEntityInfo<FORECAST_JOB> entityProperty in bulkDeleteProperties)
            {
                DataRow newRow = createNewDataRowFromFORECAST_JOB(entityProperty.ChangedEntity);
                findExistingOrAddNewFORECAST_JOB(newRow, true);
                foreach (KeyValuePair<string, decimal> datesForecast in entityProperty.ChangedEntity.DatesForecasts)
                    commitCellValue(datesForecast.Key, newRow, datesForecast.Value, true);

                shouldPromptForSavingSnapshot = true;
                updateRowReadOnlyAttributes(newRow);
                dataPointsTable.Rows.Add(newRow);
            }

            foreach (UndoRedoEntityInfo<FORECAST_JOB> entityProperty in bulkAddedProperties)
            {
                DataRow findRow = searchRow(entityProperty.ChangedEntity.GUID);
                if(findRow != null)
                {
                    int rowIndex = dataPointsTable.Rows.IndexOf(findRow);
                    if (rowIndex >= 0)
                    {
                        DataRowView dataRowView = dataPointsTable.DefaultView[rowIndex];
                        deleteRow(dataRowView, "Undo");
                        dataPointsTable.Rows.Remove(dataRowView.Row);
                    }
                }
            }

            GridControlService.GridControl.RefreshData();
        }

        /// <summary>
        /// Function to redo the entity changes
        /// Must be used in conjunction of EntitiesUndoManager
        /// </summary>
        /// <param name="entityProperty">Entity passed over from EntitiesUndoRedo</param>
        public virtual void BulkPropertyRedo(IEnumerable<UndoRedoEntityInfo<FORECAST_JOB>> entityProperties)
        {
            IEnumerable<UndoRedoEntityInfo<FORECAST_JOB>> bulkAddedProperties = entityProperties.Where(x => x.MessageType == EntityMessageType.Added);
            IEnumerable<UndoRedoEntityInfo<FORECAST_JOB>> bulkSaveProperties = entityProperties.Where(x => x.MessageType == EntityMessageType.Changed);
            IEnumerable<UndoRedoEntityInfo<FORECAST_JOB>> bulkDeleteProperties = entityProperties.Where(x => x.MessageType == EntityMessageType.Deleted);
            foreach (UndoRedoEntityInfo<FORECAST_JOB> entityProperty in bulkAddedProperties)
            {
                DataRow newRow = createNewDataRowFromFORECAST_JOB(entityProperty.ChangedEntity);
                findExistingOrAddNewFORECAST_JOB(newRow, true);
                foreach (KeyValuePair<string, decimal> datesForecast in entityProperty.ChangedEntity.DatesForecasts)
                {
                    commitCellValue(datesForecast.Key, newRow, datesForecast.Value, true);
                }

                shouldPromptForSavingSnapshot = true;
                updateRowReadOnlyAttributes(newRow);
                dataPointsTable.Rows.Add(newRow);
            }

            foreach (UndoRedoEntityInfo<FORECAST_JOB> entityProperty in bulkSaveProperties)
            {
                DataRow findRow = searchRow(entityProperty.ChangedEntity.GUID);
                if (findRow != null)
                {
                    commitCellValue(entityProperty.PropertyName, findRow, entityProperty.NewValue);
                    updateRowReadOnlyAttributes(findRow);
                }
            }

            foreach (UndoRedoEntityInfo<FORECAST_JOB> entityProperty in bulkDeleteProperties)
            {
                DataRow findRow = searchRow(entityProperty.ChangedEntity.GUID);
                if (findRow != null)
                {
                    int rowIndex = dataPointsTable.Rows.IndexOf(findRow);
                    if (rowIndex >= 0)
                    {
                        DataRowView dataRowView = dataPointsTable.DefaultView[rowIndex];
                        deleteRow(dataRowView, "Redo");
                        dataPointsTable.Rows.Remove(dataRowView.Row);
                    }
                }
            }

            GridControlService.GridControl.RefreshData();
        }

        public override bool CanKeyboardCopy()
        {
            return !IsLoading;
        }

        public override bool CanKeyboardPaste()
        {
            return !IsLoading;
        }

        public override bool CanSaveLayout()
        {
            return !IsLoading;
        }

        public override bool CanResetLayout()
        {
            if (IsLoading)
                return false;

            return base.CanResetLayout();
        }

        public override bool CanExportToExcel()
        {
            return !IsLoading;
        }

        public bool CanDeleteRows()
        {
            return !IsLoading && SelectedDataRows != null && SelectedDataRows.Count > 0;
        }

        public void DeleteRows()
        {
            if (MessageBoxService.ShowMessage("Are you sure you want to delete " + selectedDataRows.Count + " selected entries?", "Confirmation", MessageButton.OKCancel) == MessageResult.Cancel)
                return;

            List<DataRow> removeRows = new List<DataRow>();
            EntitiesUndoRedoManager.PauseActionId();
            foreach(DataRowView selectedRow in SelectedDataRows)
            {
                deleteRow(selectedRow, "User manual delete");
                removeRows.Add(selectedRow.Row);
            }
            EntitiesUndoRedoManager.UnpauseActionId();

            foreach (DataRow removeRow in removeRows)
                DataPointsTable.Rows.Remove(removeRow);
        }

        private void deleteRow(DataRowView selectedRow, string deleteReason)
        {
            FORECAST_JOB findFORECAST_JOB = (FORECAST_JOB)selectedRow[columnForecastJob];
            if (findFORECAST_JOB != null)
            {
                findFORECAST_JOB.DELETE_REASON = deleteReason;
                //save the delete reason
                MainViewModel.Save(findFORECAST_JOB);
                MainViewModel.Delete(findFORECAST_JOB);

                shouldPromptForSavingSnapshot = true;
            }

            if (!EntitiesUndoRedoManager.IsInUndoRedoOperation)
                EntitiesUndoRedoManager.AddUndo(updateForecastJobFromDataRow(selectedRow.Row), null, null, null, EntityMessageType.Deleted);
        }

        public bool CanSaveDateAndRefresh()
        {
            return !IsLoading && FixedDataDate != null;
        }

        public void SaveDateAndRefresh()
        {
            DateTime? changedDate = FixedDataDate;
            BluePrintsDataUtils.SaveDateAndRefresh(LoadPROJECT, LoadDataDate, ref changedDate, FixedEndDate, FORECAST_EACCollection, PROJECTCollectionViewModel, MessageBoxService);

            EntitiesUndoRedoManager.Clear();
            loadDataPointsTable();

            FixedDataDate = changedDate;
            this.RaisePropertyChanged(x => x.FixedDataDate);
            this.RaisePropertyChanged(x => x.FixedEndDate);
        }

        public override bool CanUndo()
        {
            if (IsLoading)
                return false;

            if (EntitiesUndoRedoManager == null)
                return false;

            return EntitiesUndoRedoManager.CanUndo();
        }

        public override bool CanRedo()
        {
            if (IsLoading)
                return false;

            if (EntitiesUndoRedoManager == null)
                return false;

            return EntitiesUndoRedoManager.CanRedo();
        }

        public override void Undo()
        {
            if (!CanUndo())
                return;

            EntitiesUndoRedoManager.Undo();
        }

        public override void Redo()
        {
            if (!CanRedo())
                return;

            EntitiesUndoRedoManager.Redo();
        }
        
        public override bool CanFillDown(object button)
        {
            if (IsLoading)
                return false;

            var info = GridPopupMenuBase.GetGridMenuInfo((DependencyObject)button) as GridMenuInfo;
            return SelectedDataRows != null && SelectedDataRows.Count > 1 && DataPointsTable != null && DataPointsTable.Rows.Count > 1 && !IsLoading && info != null && info.Column != null && !info.Column.ReadOnly;
        }

        public override bool CanFillUp(object button)
        {
            if (IsLoading)
                return false;

            var info = GridPopupMenuBase.GetGridMenuInfo((DependencyObject)button) as GridMenuInfo;
            return SelectedDataRows != null && SelectedDataRows.Count > 1 && DataPointsTable != null && DataPointsTable.Rows.Count > 1 && !IsLoading && info != null && info.Column != null && !info.Column.ReadOnly;
        }

        public override void FillDown(object button)
        {
            Fill(button, false);
        }

        public override void FillUp(object button)
        {
            Fill(button, true);
        }

        public void Fill(object button, bool isUp)
        {
            EntitiesUndoRedoManager.Clear();
            GridMenuInfo info = GridPopupMenuBase.GetGridMenuInfo((DependencyObject)button) as GridMenuInfo;
            object valueToFill;
            object nextValueInSequence;

            if (isUp)
            {
                valueToFill = SelectedDataRows[SelectedDataRows.Count - 1][info.Column.FieldName];
                nextValueInSequence = SelectedDataRows[SelectedDataRows.Count - 2][info.Column.FieldName];
            }
            else
            {
                valueToFill = SelectedDataRows[0][info.Column.FieldName];
                nextValueInSequence = SelectedDataRows[1][info.Column.FieldName];
            }

            EntitiesUndoRedoManager.PauseActionId();
            var bulkSaveEntities = new List<DataRow>();

            long? enumerationDifferences = null;
            long? enumerator = null;
            int? numericIndex = null;
            int numericFieldLength = 0;
            EnumerationType enumerationType;
            if (valueToFill != null && valueToFill.GetType() == typeof(string) && nextValueInSequence != null)
                enumerationType = DataUtils.GetEnumerateType(valueToFill.ToString(), nextValueInSequence.ToString(), out enumerationDifferences, out enumerator, out numericIndex, out numericFieldLength);
            else
                enumerationType = EnumerationType.None;

            if (!isUp)
            {
                for (int i = 1; i < SelectedDataRows.Count; i++)
                {
                    if (enumerationType == EnumerationType.Increase)
                        enumerator += enumerationDifferences;
                    else
                    {
                        enumerator -= enumerationDifferences;
                        if (enumerator < 0)
                            enumerator = 0;
                    }


                    DataRowView seletedEntity = SelectedDataRows[i];
                    setEntityProperty(seletedEntity, info, valueToFill, numericIndex, enumerator, numericFieldLength);
                }
            }
            else
            {
                for (int i = SelectedDataRows.Count - 2; i >= 0; i--)
                {
                    if (enumerationType == EnumerationType.Increase)
                        enumerator += enumerationDifferences;
                    else
                    {
                        enumerator -= enumerationDifferences;
                        if (enumerator < 0)
                            enumerator = 0;
                    }

                    DataRowView seletedEntity = SelectedDataRows[i];
                    setEntityProperty(seletedEntity, info, valueToFill, numericIndex, enumerator, numericFieldLength);
                }
            }

            EntitiesUndoRedoManager.UnpauseActionId();
        }

        private void setEntityProperty(DataRowView editRow, GridMenuInfo info, object valueToFill, int? numericIndex, long? enumerator, int numericFieldLength)
        {
            if (numericIndex != null && enumerator != null)
            {
                string valueToFillStringOnly = valueToFill.ToString().Substring(0, valueToFill.ToString().Length - numericFieldLength);
                valueToFill = StringFormatUtils.AppendStringWithEnumerator(valueToFillStringOnly, (long)enumerator, numericFieldLength);
            }

            
            var OldValue = editRow[info.Column.FieldName];
            EntitiesUndoRedoManager.AddUndo(updateForecastJobFromDataRow(editRow.Row), info.Column.FieldName, OldValue, valueToFill, EntityMessageType.Changed);
            commitCellValue(info.Column.FieldName, editRow.Row, valueToFill);
        }

        public DataRowView SelectedDataRow { get; set; }

        ObservableCollection<DataRowView> selectedDataRows { get; set; }
        public ObservableCollection<DataRowView> SelectedDataRows
        {
            get
            {
                if (selectedDataRows == null)
                    selectedDataRows = new ObservableCollection<DataRowView>();

                return selectedDataRows;
            }
            set
            {
                selectedDataRows = value;
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

        public IEnumerable<Data.COMMODITY_CODE> COMMODITY_CODECollection
        {
            get
            {
                return GetEntities<Data.COMMODITY_CODE>();
            }
        }

        public IEnumerable<STOCK_ITEMS> STOCK_ITEMCollection
        {
            get
            {
                return GetEntities<STOCK_ITEMS>();
            }
        }

        public IEnumerable<JOB_COSTTYPES> JOB_COSTTYPESCollection
        {
            get
            {
                return GetEntities<JOB_COSTTYPES>();
            }
        }

        public IEnumerable<JOB_COSTGROUPS> JOB_COSTGROUPSCollection
        {
            get
            {
                return GetEntities<JOB_COSTGROUPS>();
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

        public IEnumerable<STOCK_GROUP2S> STOCK_GROUP2SCollection
        {
            get
            {
                var collection = GetEntities<STOCK_GROUP2S>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.GROUPNO);
                return collection;
            }
        }

        public IEnumerable<STOCK_GROUPS> STOCK_GROUPSCollection
        {
            get
            {
                var collection = GetEntities<STOCK_GROUPS>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.GROUPNO);
                return collection;
            }
        }

        public IEnumerable<FORECAST_EAC> FORECAST_EACCollection
        {
            get
            {
                return GetEntities<FORECAST_EAC>();
            }
        }
        #endregion
    }
}