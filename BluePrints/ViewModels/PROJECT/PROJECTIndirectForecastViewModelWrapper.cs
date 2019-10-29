using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.View;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Loader;
using BaseModel.ViewModel.UndoRedo;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Misc;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
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
        protected PROJECTIndirectForecastViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        public PROJECT LoadPROJECT { get; set; }
        public DateTime FixedDataDateMonthEnd => new DateTime((FixedDataDate).Year, (FixedDataDate).Month, 1).AddMonths(1).AddDays(-1);
        public DateTime FixedDataDate { get; set; }
        public DateTime FixedEndDate { get; set; }
        protected List<DateTime> alignedDataDateCollection;
        public List<ExoSubJobProjection> QueryJobs { get; set; }
        protected List<ExoTimeAuthorisation> queryJobLines { get; set; }
        public bool IsWeeks { get; set; }
        protected IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory;
        IPrimeroEntitiesUnitOfWork primeroEntitiesUnitOfWork;
        public bool IsLoadingForecast { get; set; }

        DispatcherTimer focusNewlyAddedProjectionTimer = new DispatcherTimer();
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<PROJECT>)parameter;
            LoadPROJECT = PROJECTParameter.GetEntity();
            primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(LoadPROJECT.OfficeNameForExo == BluePrintsResources.OfficeMontreal);
            primeroEntitiesUnitOfWork = primeroUnitOfWorkFactory.CreateUnitOfWork();
            IsWeeks = true;
            List<ExoTimeAuthorisation> jobLines = new List<ExoTimeAuthorisation>();
            QueryJobs = ExoQueries.GetNativeExoSubJobProjection(primeroEntitiesUnitOfWork, LoadPROJECT, ref jobLines).Where(x => x.SubJob != null && x.SubJob.Code.Contains("I1")).ToList();
            List<ExoSubJobProjection> uniqueQueryJobs = new List<ExoSubJobProjection>();

            foreach(ExoSubJobProjection queryJob in QueryJobs)
            {
                if (!uniqueQueryJobs.Any(x => x.FullCode == queryJob.FullCode))
                    uniqueQueryJobs.Add(queryJob);
            }

            QueryJobs = uniqueQueryJobs.OrderBy(x => x.FullCode).ToList();
            queryJobLines = jobLines;
            IsLoadingForecast = true;


            focusNewlyAddedProjectionTimer = new DispatcherTimer();
            focusNewlyAddedProjectionTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            focusNewlyAddedProjectionTimer.Tick += FocusNewlyAddedProjectionTimer_Tick;
            GlobalMethods.SetAccordionExpandedState?.Invoke(false);
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => setProject(x));
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.FORECAST_JOB_HOURS, FORECAST_JOB_HOURSProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, COMMODITY_CODEProjectionFunc);
            loaderCollection.AddLoaderDescription<STOCK_ITEMS, STOCK_ITEMS, string, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.STOCK_ITEMS);
            loaderCollection.AddLoaderDescription<JOB_COSTTYPES, JOB_COSTTYPES, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOB_COSTTYPES);
        }

        private Func<IRepositoryQuery<Data.PROJECT>, IQueryable<Data.PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == LoadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<FORECAST_JOB_HOUR>, IQueryable<FORECAST_JOB_HOUR>> FORECAST_JOB_HOURSProjectionFunc()
        {
            return query => query.Where(x => x.FORECAST_JOB.GUID_PROJECT == LoadPROJECT.GUID);
        }
        private Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>> COMMODITY_CODEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == null);
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

            //get immutable data
            alignedDataDateCollection = generateDates();
            this.RaisePropertiesChanged();
        }

        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            loadDataPointsTable();
            base.OnAfterAssignedCallbackAndRaisePropertyChanged();
        }
        #endregion
        #region View Properties
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

        private void loadDataPointsTable()
        {
            IsLoadingForecast = true;
            this.RaisePropertyChanged(x => x.IsLoadingForecast);
            dataPointsTable = null;
            updateDataPointsTable();
            this.RaisePropertyChanged(x => x.DataPointsTable);
            IsLoadingForecast = false;
            this.RaisePropertyChanged(x => x.IsLoadingForecast);
        }

        private void updateDataPointsTable()
        {
            GridControlService.GridControl.BeginDataUpdate();
            dataPointsTable = new DataTable();

            InitializeColumnSource(ParentViewColumns, ParentSummaries, alignedDataDateCollection, false);
            LoadingScreenManager.ShowLoadingScreen(MainViewModel.Entities.Count);
            LoadingScreenManager.SetMessage("Preparing View...");

            //construct data points table
            dataPointsTable.Columns.Add(columnFullCode, typeof(string));
            dataPointsTable.Columns.Add(columnCommodityName, typeof(string));
            dataPointsTable.Columns.Add(columnProjection, typeof(ExoSubJobProjection));
            dataPointsTable.Columns.Add(columnGUID, typeof(Guid));
            dataPointsTable.Columns.Add(columnDescription, typeof(string));
            dataPointsTable.Columns.Add(columnStockItem, typeof(string));
            dataPointsTable.Columns.Add(columnStockItemName, typeof(string));
            dataPointsTable.Columns.Add(columnReference, typeof(string));
            dataPointsTable.Columns.Add(columnNote, typeof(string));
            dataPointsTable.Columns.Add(columnUOM, typeof(string));
            dataPointsTable.Columns.Add(columnForecastRate, typeof(decimal));
            dataPointsTable.Columns.Add(columnTotalHours, typeof(decimal));
            dataPointsTable.Columns.Add(columnTotalCosts, typeof(decimal));

            foreach (DateTime alignedDataDate in alignedDataDateCollection)
            {
                string columnFieldName = alignedDataDate.Date.ToString(BluePrintsResources.ColumnDateFormat);
                dataPointsTable.Columns.Add(columnFieldName, typeof(decimal));
            }

            List<ExoDataPoint> allDataPoints = new List<ExoDataPoint>();
            foreach (FORECAST_JOB job in MainViewModel.Entities)
            {
                ExoSubJobProjection projection = QueryJobs.Where(x => x.Commodity != null && x.Discipline != null && x.SubJob != null).FirstOrDefault(x => x.Commodity.Code == job.COMMODITY_CODE && x.Discipline.Code == job.DISCIPLINE_CODE && x.SubJob.Code == job.SUBJOB_CODE && x.Variation_Code == job.VARIATION_CODE);
                if (projection == null)
                    continue;

                DataRow newRow = dataPointsTable.NewRow();
                newRow[columnFullCode] = projection.FullCode;
                newRow[columnProjection] = projection;
                newRow[columnGUID] = job.GUID;
                newRow[columnDescription] = job.DESCRIPTION;
                newRow[columnReference] = job.REFERENCE;
                newRow[columnNote] = job.NOTE;
                newRow[columnStockItem] = job.STOCK_ITEM;
                newRow[columnUOM] = job.UOM;
                if (job.FORECAST_RATE == null)
                    newRow[columnForecastRate] = DBNull.Value;
                else
                    newRow[columnForecastRate] = job.FORECAST_RATE;

                dataPointsTable.Rows.Add(newRow);
                foreach (DateTime alignedDate in alignedDataDateCollection)
                {
                    string columnFieldName = alignedDate.Date.ToString(BluePrintsResources.ColumnDateFormat);

                    FORECAST_JOB_HOUR jobHourOnAlignedDate = FORECAST_JOB_HOURCollection.FirstOrDefault(x => x.GUID_FORECAST_JOB == job.GUID && x.FORECAST_DATE.Date == alignedDate.Date);
                    if (jobHourOnAlignedDate != null && jobHourOnAlignedDate.FORECAST_HOUR != null)
                        newRow[columnFieldName] = jobHourOnAlignedDate.FORECAST_HOUR;
                    else
                        newRow[columnFieldName] = DBNull.Value;
                }

                updateRowReadOnlyAttributes(newRow);

                LoadingScreenManager.Progress();
            }

            GridControlService.GridControl.EndDataUpdate();
            LoadingScreenManager.CloseLoadingScreen();
        }

        private void updateRowReadOnlyAttributes(DataRow row)
        {
            if (row[columnProjection] == DBNull.Value)
                return;

            //update commodity name
            ExoSubJobProjection projection = (ExoSubJobProjection)row[columnProjection];

            JOB_COSTTYPES findCOST_TYPE = JOB_COSTTYPESCollection.FirstOrDefault(x => x.SHORTCODE == projection.Commodity.Code);
            if (findCOST_TYPE != null)
                row[columnCommodityName] = findCOST_TYPE.COSTDESC;
            else
                row[columnCommodityName] = string.Empty;

            //update stock item name          
            if (row[columnStockItem] != DBNull.Value)
            {
                STOCK_ITEMS findSTOCK_ITEM = STOCK_ITEMCollection.FirstOrDefault(x => x.STOCKCODE == row[columnStockItem].ToString());
                if (findSTOCK_ITEM != null)
                    row[columnStockItemName] = findSTOCK_ITEM.DESCRIPTION;
                else
                    row[columnStockItemName] = string.Empty;
            }

            if (row[columnGUID] == DBNull.Value)
                return;

            //update total hours
            decimal rate = 0.00m;
            Guid guid = (Guid)row[columnGUID];
            decimal totalForecastHours = FORECAST_JOB_HOURCollection.Where(x => x.GUID_FORECAST_JOB == guid && x.FORECAST_HOUR != null).Sum(x => (decimal)x.FORECAST_HOUR);
            row[columnTotalHours] = totalForecastHours;

            //update total costs
            if (row[columnForecastRate] != DBNull.Value)
            {
                rate = (decimal)row[columnForecastRate];
                row[columnTotalCosts] = rate * totalForecastHours;
            }
            else
                row[columnTotalCosts] = 0.00m;
        }

        public virtual void PastingFromClipboard(PastingFromClipboardEventArgs e)
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

                if (MainViewModel.SelectMode == MultiSelectMode.Row)
                {
                    List<ErrorMessage> invalidRows = pasteRowData(gridTableView, RowData);

                    if (invalidRows.Count > 0)
                    {
                        DialogCollectionViewModel<ErrorMessage> viewModel = DialogCollectionViewModel<ErrorMessage>.Create(invalidRows, "Paste status");
                        ErrorMessagesDialogService.ShowDialog(MessageButton.OK, string.Empty, "ListErrorMessages", viewModel);
                    }
                }
                else
                    pasteCellData(gridControl, gridTableView, RowData);

                GridControlService.GridControl.RefreshData();
                e.Handled = true;
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
            List<ErrorMessage> invalidRows = new List<ErrorMessage>();

            LoadingScreenManager.ShowLoadingScreen(RowData.Count());
            LoadingScreenManager.SetMessage("Pasting Rows...");
            foreach (var Row in RowData)
            {
                var ColumnStrings = Row.Split('\t');
                string fullCode = ColumnStrings[0];
                ExoSubJobProjection queryJob = QueryJobs.FirstOrDefault(x => x.FullCode == fullCode);
                if (queryJob != null)
                {
                    addNewPasteRow(queryJob, gridTableView, ColumnStrings);
                    pasteRowCount += 1;
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
                                addNewPasteRow(queryJob, gridTableView, ColumnStrings);
                                pasteRowCount += 1;
                                invalidRows.Add(new ErrorMessage(oldCode, "Row is pasted, but " + oldCode + " has been remapped to " + fullCode + ", because " + stockCode + " is a stock code"));
                            }
                            else
                                invalidRows.Add(new ErrorMessage(fullCode, "Row is not pasted, because exo job doesn't exists"));
                        }
                        else
                            invalidRows.Add(new ErrorMessage(fullCode, "Row is not pasted, because exo job doesn't exists"));
                    }
                    else
                        invalidRows.Add(new ErrorMessage(fullCode, "Row is not pasted, because exo job doesn't exists"));
                }

                LoadingScreenManager.Progress();
            }

            focusNewlyAddedProjectionTimer.Start();
            EntitiesUndoRedoManager.UnpauseActionId();
            LoadingScreenManager.CloseLoadingScreen();
            return invalidRows;
        }

        private DataRow addNewPasteRow(ExoSubJobProjection queryJob, TableView gridTableView, string[] ColumnStrings)
        {
            DataRow newRow = DataPointsTable.NewRow();
            newRow[columnFullCode] = queryJob.FullCode;
            newRow[columnProjection] = queryJob;
            addNewFORECAST_JOB(newRow);
            for (var i = 1; i < ColumnStrings.Count(); i++)
            {
                if (i > gridTableView.VisibleColumns.Count - 1)
                    continue;

                string pasteData = ColumnStrings[i];
                ColumnBase copyColumn = gridTableView.VisibleColumns[i];
                basePasteData(newRow, copyColumn, pasteData, true);
            }

            DataPointsTable.Rows.Add(newRow);
            return newRow;
        }

        int pasteRowCount = 0;
        private void FocusNewlyAddedProjectionTimer_Tick(object sender, EventArgs e)
        {
            focusNewlyAddedProjectionTimer.Stop();
            if (DataPointsTable == null || pasteRowCount == 0)
                return;

            IEnumerable<DataRow> selectRows = DataPointsTable.AsEnumerable().Skip(Math.Max(0, DisplayEntities.Count() - pasteRowCount));
            selectedDataRows.Clear();

            foreach (var selectRow in selectRows)
            {
                DataRowView dataRowView = DataPointsTable.DefaultView[DataPointsTable.Rows.IndexOf(selectRow)];
                selectedDataRows.Add(dataRowView);
                SelectedDataRow = dataRowView;
            }

            this.RaisePropertyChanged(x => x.SelectedDataRow);
            this.RaisePropertyChanged(x => x.SelectedDataRows);
            pasteRowCount = 0;
        }

        private void pasteCellData(GridControl gridControl, TableView gridTableView, string[] RowData)
        {
            EntitiesUndoRedoManager.PauseActionId();
            GridControlHelpers.PasteCellData(gridControl, gridTableView, RowData, basePasteData);
            EntitiesUndoRedoManager.UnpauseActionId();
        }

        private bool basePasteData(DataRow newRow, ColumnBase copyColumn, string pasteData, bool isLastRow)
        {
            DateTime columnDateTime;
            if(copyColumn.FieldName == columnFullCode)
            {
                ExoSubJobProjection queryJob = QueryJobs.FirstOrDefault(x => x.FullCode == pasteData);
                if(queryJob != null)
                {
                    ExoSubJobProjection oldJob = null;
                    if (newRow[copyColumn.FieldName] != DBNull.Value)
                        oldJob = (ExoSubJobProjection)newRow[copyColumn.FieldName];

                    newRow[copyColumn.FieldName] = queryJob;
                    if(MainViewModel.IsPasteCellLevel)
                        EntitiesUndoRedoManager.AddUndo(newRow, copyColumn.FieldName, oldJob, queryJob, EntityMessageType.Changed);
                }
            }
            else if(copyColumn.FieldName == columnStockItem)
            {
                string oldStockItem = newRow[copyColumn.FieldName].ToString();
                stockItemValueChangedFromView(newRow, newRow[copyColumn.FieldName], pasteData, MainViewModel.IsPasteCellLevel);
                newRow[copyColumn.FieldName] = pasteData;

                commitCellValue(copyColumn.FieldName, newRow, pasteData, !isLastRow);
                if (MainViewModel.IsPasteCellLevel)
                    EntitiesUndoRedoManager.AddUndo(newRow, copyColumn.FieldName, oldStockItem, pasteData, EntityMessageType.Changed);
            }
            else if (copyColumn.FieldName == columnForecastRate || DateTime.TryParse(copyColumn.FieldName, out columnDateTime))
            {
                if (copyColumn.FieldType == typeof(decimal))
                {
                    var rgx = new Regex("[^0-9a-z\\.]");
                    var cleanColumnString = rgx.Replace(pasteData, string.Empty);
                    decimal? oldValue = newRow[copyColumn.FieldName] == DBNull.Value ? (decimal?)null : (decimal)newRow[copyColumn.FieldName];
                    decimal decimal_value;
                    if (decimal.TryParse(cleanColumnString, out decimal_value))
                    {
                        commitCellValue(copyColumn.FieldName, newRow, decimal_value, !isLastRow);
                        if (MainViewModel.IsPasteCellLevel)
                            EntitiesUndoRedoManager.AddUndo(newRow, copyColumn.FieldName, oldValue, decimal_value, EntityMessageType.Changed);
                    }
                    else
                    {
                        commitCellValue(copyColumn.FieldName, newRow, null, !isLastRow);
                        if (MainViewModel.IsPasteCellLevel)
                            EntitiesUndoRedoManager.AddUndo(newRow, copyColumn.FieldName, oldValue, null, EntityMessageType.Changed);
                    }
                }
            }
            else if (copyColumn.FieldType == typeof(string) && !copyColumn.ReadOnly)
            {
                string oldValue = newRow[copyColumn.FieldName].ToString();
                newRow[copyColumn.FieldName] = pasteData;
                commitCellValue(copyColumn.FieldName, newRow, pasteData, !isLastRow);
                if (MainViewModel.IsPasteCellLevel)
                    EntitiesUndoRedoManager.AddUndo(newRow, copyColumn.FieldName, oldValue, pasteData, EntityMessageType.Changed);
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

                addNewFORECAST_JOB(row.Row);

                //added not working well atm because when row is removed from datatable its itemarray is cleared
                //EntitiesUndoRedoManager.AddUndo(row.Row, null, null, null, EntityMessageType.Added);
                EntitiesUndoRedoManager.UnpauseActionId();
            }
        }

        private void addNewFORECAST_JOB(DataRow row)
        {
            FORECAST_JOB newFORECAST_JOB = new FORECAST_JOB();

            if (row[columnFullCode] == DBNull.Value)
                return;

            ExoSubJobProjection projection = QueryJobs.FirstOrDefault(x => x.FullCode == row[columnFullCode].ToString());
            if (projection != null)
            {
                newFORECAST_JOB.SUBJOB_CODE = projection.SubJob.Code;
                newFORECAST_JOB.DISCIPLINE_CODE = projection.Discipline.Code;
                newFORECAST_JOB.COMMODITY_CODE = projection.Commodity.Code;
                if (projection.Variation_Code == null)
                    newFORECAST_JOB.VARIATION_CODE = string.Empty;
                else
                    newFORECAST_JOB.VARIATION_CODE = projection.Variation_Code;

                newFORECAST_JOB.DESCRIPTION = row[columnDescription].ToString();
                newFORECAST_JOB.STOCK_ITEM = row[columnStockItem].ToString();
                newFORECAST_JOB.NOTE = row[columnNote].ToString();
                newFORECAST_JOB.UOM = row[columnUOM].ToString();
                newFORECAST_JOB.REFERENCE = row[columnReference].ToString();
                if (row[columnForecastRate] != DBNull.Value)
                    newFORECAST_JOB.FORECAST_RATE = (decimal)row[columnForecastRate];

                newFORECAST_JOB.GUID_PROJECT = LoadPROJECT.GUID;
                MainViewModel.Save(newFORECAST_JOB);
                row[columnGUID] = newFORECAST_JOB.GUID;
                row[columnProjection] = projection;
                //add undo must be after so that Guid is populated
            }
        }

        /// <summary>
        /// Influence column(s) when changes happens in other column
        /// </summary>
        public void CellValueChangedUpdate(CellValueChangedEventArgs e)
        {
            DataRowView dataRowView = (DataRowView)e.Row;

            //new item handling
            if (e.RowHandle == GridControl.AutoFilterRowHandle || e.RowHandle == GridControl.NewItemRowHandle)
            {
                if (e.Column.FieldName == columnFullCode && e.Value != null)
                {
                    ExoSubJobProjection queryJob = QueryJobs.FirstOrDefault(x => x.FullCode == e.Value.ToString());
                    dataRowView[columnProjection] = queryJob;
                }

                updateRowReadOnlyAttributes(dataRowView.Row);
                return;
            }

            //existing item handling
            EntitiesUndoRedoManager.PauseActionId();
            Guid guid = (Guid)dataRowView[columnGUID];
            string fieldName = e.Column.FieldName;

            if (e.Column.FieldName == columnStockItem)
                stockItemValueChangedFromView(dataRowView.Row, e.OldValue, e.Value, true);

            commitCellValue(fieldName, dataRowView.Row, e.Value);
            EntitiesUndoRedoManager.AddUndo(dataRowView.Row, fieldName, e.OldValue, e.Value, EntityMessageType.Changed);
            EntitiesUndoRedoManager.UnpauseActionId();

            e.Handled = true;
        }

        private void stockItemValueChangedFromView(DataRow row, object oldValue, object newValue, bool addUndoRedo)
        {
            if (newValue == null)
                return;

            STOCK_ITEMS findSTOCK_ITEM = STOCK_ITEMCollection.FirstOrDefault(x => x.STOCKCODE == newValue.ToString());
            if (findSTOCK_ITEM != null)
            {
                decimal? oldDecimalValue = null;
                if (row[columnForecastRate] != DBNull.Value)
                    oldDecimalValue = (decimal)row[columnForecastRate];

                decimal? newDecimalValue = null;
                if (findSTOCK_ITEM.STDCOST != null && findSTOCK_ITEM.STDCOST > 0)
                    newDecimalValue = Convert.ToDecimal(findSTOCK_ITEM.STDCOST);

                if (newDecimalValue == null)
                    row[columnForecastRate] = DBNull.Value;
                else
                    row[columnForecastRate] = newDecimalValue;

                if(addUndoRedo)
                    EntitiesUndoRedoManager.AddUndo(row, columnForecastRate, oldDecimalValue, newDecimalValue, EntityMessageType.Changed);
            }
        }

        protected virtual void commitCellValue(string fieldName, DataRow row, object newValue, bool skipUpdate = false)
        {
            Guid guid = (Guid)row[columnGUID];

            DateTime dateTime;
            if (DateTime.TryParse(fieldName, out dateTime))
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

                FORECAST_JOB_HOUR forecastJobHour = FORECAST_JOB_HOURCollection.FirstOrDefault(x => x.GUID_FORECAST_JOB == guid && x.FORECAST_DATE.Date == dateTime.Date);
                FORECAST_JOB_HOUR editForecastJobHour;
                if (forecastJobHour == null)
                    editForecastJobHour = new FORECAST_JOB_HOUR();
                else
                    editForecastJobHour = forecastJobHour;

                editForecastJobHour.FORECAST_DATE = dateTime.Date;
                editForecastJobHour.GUID_FORECAST_JOB = guid;
                editForecastJobHour.FORECAST_HOUR = forecastHours;
                FORECAST_JOB_HOURCollectionViewModel.Save(editForecastJobHour);

                //for undo/redo
                if (forecastHours == null)
                    row[fieldName] = DBNull.Value;
                else
                    row[fieldName] = forecastHours;
            }
            else
            {
                FORECAST_JOB editFORECAST_JOB = MainViewModel.Entities.FirstOrDefault(x => x.GUID == guid);
                if (editFORECAST_JOB != null)
                {
                    if (fieldName == columnDescription)
                        editFORECAST_JOB.DESCRIPTION = newValue.ToString();
                    else if (fieldName == columnStockItem)
                        editFORECAST_JOB.STOCK_ITEM = newValue.ToString();
                    else if (fieldName == columnNote)
                        editFORECAST_JOB.NOTE = newValue.ToString();
                    else if (fieldName == columnReference)
                        editFORECAST_JOB.REFERENCE = newValue.ToString();
                    else if (fieldName == columnUOM)
                        editFORECAST_JOB.UOM = newValue.ToString();
                    else if (fieldName == columnForecastRate)
                    {
                        if (newValue == null)
                            editFORECAST_JOB.FORECAST_RATE = (decimal?)null;
                        else
                            editFORECAST_JOB.FORECAST_RATE = (decimal)newValue;
                    }

                    //for undo/redo
                    if (newValue == null)
                        row[fieldName] = DBNull.Value;
                    else
                        row[fieldName] = newValue;

                    MainViewModel.Save(editFORECAST_JOB);
                }
            }

            if (!skipUpdate)
                updateRowReadOnlyAttributes(row);
        }

        private DataRow searchRow(Guid guid)
        {
            IEnumerable<DataRow> findRows = (from DataRow dr in dataPointsTable.Rows
                                                         where (Guid)dr[columnGUID] == guid
                                                         select dr);

            return findRows.FirstOrDefault();
        }

        string columnFullCode = "FullCode";
        string columnCommodityName = "CommodityName";
        string columnProjection = "Projection";
        string columnGUID = "GUID";
        string columnDescription = "DESCRIPTION";
        string columnStockItem = "STOCK_ITEM";
        string columnStockItemName = "StockItemName";
        string columnReference = "REFERENCE";
        string columnNote = "NOTE";
        string columnUOM = "UOM";
        string columnForecastRate = "FORECAST_RATE";
        string columnTotalHours = "TOTAL_HOURS";
        string columnTotalCosts = "TOTAL_COSTS";
        private void InitializeColumnSource(ObservableCollection<ColumnDescriptor> columns, ObservableCollection<SummaryDescriptor> summaries, List<DateTime> alignedDates, bool isChild)
        {
            columns.Clear();
            summaries.Clear();

            columns.Add(new ColumnDescriptor() { FieldName = columnFullCode, ReadOnly = false, Header = "Full Code", Fixed = FixedStyle.Left, Width = 150, Settings = SettingsType.FullCode });
            summaries.Add(new SummaryDescriptor() { FieldName = columnFullCode, DisplayFormat = "Total {0} Records", Type = SummaryItemType.Count });
            columns.Add(new ColumnDescriptor() { FieldName = columnCommodityName, ReadOnly = true, Header = "Commodity Name (AutoFilled)", Fixed = FixedStyle.Left, Width = 150, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = columnDescription, ReadOnly = false, Header = "Description", Fixed = FixedStyle.Left, Width = 100, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = columnStockItem, ReadOnly = false, Header = "Source", HeaderToolTip="Changing this value will automatically populate rate", Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.StockItem });
            columns.Add(new ColumnDescriptor() { FieldName = columnStockItemName, ReadOnly = true, Header = "Source Description (AutoFilled)", Fixed = FixedStyle.Left, Width = 100, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = columnReference, ReadOnly = false, Header = "Reference", Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = columnNote, ReadOnly = false, Header = "Note", Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = columnUOM, ReadOnly = false, Header = "UOM", Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = columnForecastRate, ReadOnly = false, Header = "Rate", Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number, Mask = "c0" });
            columns.Add(new ColumnDescriptor() { FieldName = columnTotalHours, ReadOnly = false, Header = "Total Hrs", Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number, Mask = "n0" });
            summaries.Add(new SummaryDescriptor() { FieldName = columnTotalHours, DisplayFormat = "n0", Type = SummaryItemType.Sum });
            columns.Add(new ColumnDescriptor() { FieldName = columnTotalCosts, ReadOnly = false, Header = "Total $", Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number, Mask = "c0" });
            summaries.Add(new SummaryDescriptor() { FieldName = columnTotalCosts, DisplayFormat = "c0", Type = SummaryItemType.Sum });

            foreach (DateTime alignedDate in alignedDates.OrderBy(x => x))
            {
                string columnFieldName = alignedDate.Date.ToString(BluePrintsResources.ColumnDateFormat);

                if (alignedDate > FixedDataDateMonthEnd)
                {
                    columns.Add(new ColumnDescriptor() { FieldName = columnFieldName, Header = columnFieldName, Fixed = FixedStyle.None, Width = 60, Settings = SettingsType.ForecastFuture });
                    summaries.Add(new SummaryDescriptor() { FieldName = columnFieldName, DisplayFormat = "0", Type = SummaryItemType.Sum });
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
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<FORECAST_JOB>, IQueryable<FORECAST_JOB>> specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_PROJECT == LoadPROJECT.GUID);
        }

        public override string UnifiedValueValidation(FORECAST_JOB projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }

        public override string UnifiedRowValidation(FORECAST_JOB projection)
        {
            return string.Empty;
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<FORECAST_JOB> entities)
        {
            MainViewModel.SetParentViewModel(this);
            MainViewModel.IsPasteCellLevel = false;
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "FORECAST_JOBCollectionViewModelWrapper"; }
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
            IEnumerable<UndoRedoEntityInfo<DataRow>> bulkAddedProperties = entityProperties.Where(x => x.MessageType == EntityMessageType.Added);
            IEnumerable<UndoRedoEntityInfo<DataRow>> bulkSaveProperties = entityProperties.Where(x => x.MessageType == EntityMessageType.Changed);
            foreach(UndoRedoEntityInfo<DataRow> entityProperty in bulkAddedProperties)
            {
                Guid guid = (Guid)entityProperty.ChangedEntity[columnGUID];
                FORECAST_JOB findFORECAST_JOB = MainViewModel.Entities.FirstOrDefault(x => x.GUID == guid);
                if(findFORECAST_JOB != null)
                {
                    MainViewModel.Delete(findFORECAST_JOB);
                }

                DataRow findRow = searchRow(guid);
                if(findRow != null)
                {
                    dataPointsTable.Rows.Remove(findRow);
                }
            }

            foreach (UndoRedoEntityInfo<DataRow> entityProperty in bulkSaveProperties)
            {
                commitCellValue(entityProperty.PropertyName, entityProperty.ChangedEntity, entityProperty.OldValue);
            }

            GridControlService.GridControl.RefreshData();
        }

        /// <summary>
        /// Function to redo the entity changes
        /// Must be used in conjunction of EntitiesUndoManager
        /// </summary>
        /// <param name="entityProperty">Entity passed over from EntitiesUndoRedo</param>
        public virtual void BulkPropertyRedo(IEnumerable<UndoRedoEntityInfo<DataRow>> entityProperties)
        {
            IEnumerable<UndoRedoEntityInfo<DataRow>> bulkAddedProperties = entityProperties.Where(x => x.MessageType == EntityMessageType.Added);
            IEnumerable<UndoRedoEntityInfo<DataRow>> bulkSaveProperties = entityProperties.Where(x => x.MessageType == EntityMessageType.Changed);
            foreach (UndoRedoEntityInfo<DataRow> entityProperty in bulkAddedProperties)
            {
                addNewFORECAST_JOB(entityProperty.ChangedEntity);
                dataPointsTable.Rows.Add(entityProperty.ChangedEntity);
            }

            foreach (UndoRedoEntityInfo<DataRow> entityProperty in bulkSaveProperties)
            {
                commitCellValue(entityProperty.PropertyName, entityProperty.ChangedEntity, entityProperty.NewValue);
            }

            GridControlService.GridControl.RefreshData();
        }

        public bool CanDeleteRows()
        {
            return !IsLoading && SelectedDataRows != null && SelectedDataRows.Count > 0;
        }

        public void DeleteRows()
        {
            List<DataRow> removeRows = new List<DataRow>();
            foreach(DataRowView selectedRow in SelectedDataRows)
            {
                Guid guid = (Guid)selectedRow.Row[columnGUID];
                FORECAST_JOB findFORECAST_JOB = MainViewModel.Entities.FirstOrDefault(x => x.GUID == guid);
                if (findFORECAST_JOB != null)
                    MainViewModel.Delete(findFORECAST_JOB);

                DataRow findRow = searchRow(guid);
                if (findRow != null)
                    removeRows.Add(findRow);
            }

            foreach(DataRow removeRow in removeRows)
            {
                dataPointsTable.Rows.Remove(removeRow);
            }
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

        public void KeyboardCopy()
        {
            System.Windows.Forms.SendKeys.SendWait("^c");
        }

        public void KeyboardPaste()
        {
            System.Windows.Forms.SendKeys.SendWait("^v");
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

        public IEnumerable<Data.FORECAST_JOB_HOUR> FORECAST_JOB_HOURCollection
        {
            get
            {
                return GetEntities<Data.FORECAST_JOB_HOUR>();
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

        public CollectionViewModel<FORECAST_JOB_HOUR, FORECAST_JOB_HOUR, Guid, IBluePrintsEntitiesUnitOfWork> FORECAST_JOB_HOURCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return
                    (CollectionViewModel<FORECAST_JOB_HOUR, FORECAST_JOB_HOUR, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<FORECAST_JOB_HOUR>();
            }
        }

        protected override void OnClose(CancelEventArgs e)
        {
            GlobalMethods.SetAccordionExpandedState?.Invoke(true);
            base.OnClose(e);
        }
        #endregion
    }
}