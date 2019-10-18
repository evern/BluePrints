using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BaseModel.ViewModel.UndoRedo;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Data;
using DevExpress.Data.Filtering;
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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;

namespace BluePrints.ViewModels
{
    public class PROJECTPOForecastViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <FORECAST_PO, FORECAST_PO, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of PROJECTPOForecastViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static PROJECTPOForecastViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new PROJECTPOForecastViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the PROJECTPOForecastViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTPOForecastViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected PROJECTPOForecastViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private IBluePrintsEntitiesUnitOfWork bluePrintsUnitOfWork;
        protected IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory;
        protected IPrimeroEntitiesUnitOfWork primeroUnitOfWork;

        protected PROJECT loadPROJECT;
        List<DateTime> alignedDataDateCollection;
        List<ExoDataPoint> allExoPos = new List<ExoDataPoint>();
        List<ExoDataPoint> allExoActuals = new List<ExoDataPoint>();
        List<string> hiddenColumnFieldNames = new List<string>();
        protected string columnEntity = "Entity";
        DispatcherTimer selectedItemsChangedDispatcher;
        DispatcherTimer closeEditorDispatcher;
        public CriteriaOperator FilterCriteria { get; set; }
        BackgroundWorker exoLoadingBackgroundWorker = new BackgroundWorker();
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity();

            primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(loadPROJECT.OfficeNameForExo == BluePrintsResources.OfficeMontreal);
            primeroUnitOfWork = primeroUnitOfWorkFactory.CreateUnitOfWork();

            hiddenColumnFieldNames.Add(columnEntity);

            selectedItemsChangedDispatcher = new DispatcherTimer();
            selectedItemsChangedDispatcher.Interval = new TimeSpan(0, 0, 0, 0, 1);

            closeEditorDispatcher = new DispatcherTimer();
            closeEditorDispatcher.Interval = new TimeSpan(0, 0, 0, 0, 1);
            closeEditorDispatcher.Tick += CloseEditorDispatcher_Tick;

            bluePrintsUnitOfWork = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
            GlobalMethods.SetAccordionExpandedState?.Invoke(false);

            exoLoadingBackgroundWorker.DoWork += ExoLoadingBackgroundWorker_DoWork;
            exoLoadingBackgroundWorker.WorkerSupportsCancellation = true;
            IsForecastLoading = true;
            this.RaisePropertyChanged(x => x.isForecastLoading);
        }

        private void ExoLoadingBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            loadExoData(primeroUnitOfWork);
        }

        bool isExoDataLoaded = false;
        private void loadExoData(IPrimeroEntitiesUnitOfWork primeroUOW)
        {
            isExoDataLoaded = false;
            //cannot put in assigncallback mainviewmodel because it can take too long and mainviewmodel will be null
            allExoPos = BluePrintsDataUtils.GetEXOPO(primeroUOW, loadPROJECT.NUMBER, DateTime.Now, null, true);
            allExoActuals = BluePrintsDataUtils.GetMaterials(primeroUOW, loadPROJECT.NUMBER, DateTime.Now, null, 1, true);
            setProject(loadPROJECT);
            generateAlignedDataDates();
            isExoDataLoaded = true;
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertyChanged(x => x.DataPointsTable)));
            mainThreadDispatcher.BeginInvoke(new Action(() => postLoadedDispatcherTimer.Start()));
        }

        private void CloseEditorDispatcher_Tick(object sender, EventArgs e)
        {
            closeEditorDispatcher.Stop();
            GridControlService.GridControl.View.CloseEditor();
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => setProject(x));
        }

        private void setProject(Data.PROJECT project)
        {
            loadPROJECT = project;

            DateTime dataDate;
            if (loadPROJECT.FORECAST_DATA_DATE != null)
                dataDate = (DateTime)loadPROJECT.FORECAST_DATA_DATE;
            else
                dataDate = DateTime.Now;

            ForecastStartDate = new DateTime(((DateTime)dataDate).Year, ((DateTime)dataDate).Month, 1).AddMonths(2).AddDays(-1);

            DateTime endDate;
            if (loadPROJECT.FORECAST_END_DATE != null)
                endDate = (DateTime)loadPROJECT.FORECAST_END_DATE;
            else
                endDate = DateTime.Now;

            ForecastEndDate = endDate;

            this.RaisePropertiesChanged();
        }

        private Func<IRepositoryQuery<Data.PROJECT>, IQueryable<Data.PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.FORECAST_POS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<FORECAST_PO>, IQueryable<FORECAST_PO>> specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        //List<ExoDataPoint> materialDataPoints;
        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<FORECAST_PO> entities)
        {
            delayPostLoadedTimer = false;
            exoLoadingBackgroundWorker.RunWorkerAsync();
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        public override string UnifiedValueValidation(FORECAST_PO projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }

        public override string UnifiedRowValidation(FORECAST_PO projection)
        {
            return string.Empty;
        }

        public virtual void PastingFromClipboard(PastingFromClipboardEventArgs e)
        {
            GridControl gridControl = (GridControl)e.Source;
            TableView gridTableView = (TableView)gridControl.View;
            string newValueString = System.Windows.Clipboard.GetText().ToString();

            //remove tab in front
            if (newValueString != string.Empty)
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

        private bool basePasteData(DataRow newRow, ColumnBase copyColumn, string pasteData, bool isLastRow)
        {
            if (copyColumn.FieldType == typeof(decimal))
            {
                var rgx = new Regex("[^0-9a-z\\.]");
                var cleanColumnString = rgx.Replace(pasteData, string.Empty);
                decimal viewDecimalValue;

                DateTime columnDateTime;
                if (DateTime.TryParse(copyColumn.FieldName, out columnDateTime))
                {
                    bool canParse = decimal.TryParse(cleanColumnString, out viewDecimalValue);
                    if (!canParse)
                        viewDecimalValue = 0.00m;

                    addUndo(newRow, copyColumn.FieldName, newRow[copyColumn.FieldName], viewDecimalValue, EntityMessageType.Changed);
                    newRow[copyColumn.FieldName] = viewDecimalValue;

                    findExistingOrAddNewFORECAST_PO(newRow, columnDateTime, viewDecimalValue, !isLastRow);
                }
                else
                {
                    return false;
                }
            }
            else if (copyColumn.FieldType == typeof(string))
            {
                newRow[copyColumn.FieldName] = pasteData;
            }

            return true;
        }

        private void addUndo(DataRow changedEntity, string fieldName, object oldValue, object viewNewValue, EntityMessageType entityMessageType)
        {
            if(viewNewValue.GetType() == typeof(decimal))
            {
                decimal? undoDecimalValue = viewNewValue == null ? (decimal?)null : (decimal)viewNewValue;
                EntitiesUndoRedoManager.AddUndo(changedEntity, fieldName, oldValue, undoDecimalValue, entityMessageType);
            }
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
                DateTime parseDateTime;
                if (DateTime.TryParse(entityProperty.PropertyName, out parseDateTime))
                {
                    decimal? oldValueDecimal = null;
                    if (entityProperty.OldValue != null && entityProperty.OldValue != DBNull.Value)
                        oldValueDecimal = (decimal)entityProperty.OldValue;

                    findExistingOrAddNewFORECAST_PO(entityProperty.ChangedEntity, parseDateTime, oldValueDecimal);
                }
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
                DateTime parseDateTime;
                if (DateTime.TryParse(entityProperty.PropertyName, out parseDateTime))
                {
                    decimal? newValueDecimal = null;
                    if (entityProperty.NewValue != null && entityProperty.NewValue != DBNull.Value)
                        newValueDecimal = (decimal)entityProperty.NewValue;

                    findExistingOrAddNewFORECAST_PO(entityProperty.ChangedEntity, parseDateTime, newValueDecimal);
                }
            }

            GridControlService.RefreshData();
        }
        #endregion

        #region View Properties
        DataTable dataPointsTable = null;
        public virtual DataTable DataPointsTable
        {
            get
            {
                if (MainViewModel == null || allExoPos == null || !isExoDataLoaded)
                    return null;

                if (dataPointsTable == null)
                {
                    //generate aligned dates
                    if (alignedDataDateCollection == null || alignedDataDateCollection.Count == 0)
                        return null;

                    //initialize datatable schema
                    dataPointsTable = new DataTable();
                    dataPointsTable.Columns.Add(columnEntity, typeof(POForecastProjection));

                    foreach (DateTime alignedDataDate in alignedDataDateCollection)
                    {
                        string columnFieldName = alignedDataDate.Date.ToString(BluePrintsResources.ColumnDateFormat);
                        dataPointsTable.Columns.Add(columnFieldName, typeof(decimal));
                    }

                    //construction projection from grouped po lines
                    List<POLine> poLines = getPOLines();
                    List<POForecastProjection> projections = new List<POForecastProjection>();
                    foreach (var poLine in poLines.OrderBy(x => x.PONumber))
                    {
                        POForecastProjection newForecast = ViewModelSource.Create(() => new POForecastProjection());
                        newForecast.PONO = poLine.PONumber;
                        //since it's a group it'll always contain at least a single element
                        ExoDataPoint dataPoint = poLine.DataPoints.First();
                        newForecast.Description = dataPoint.Description;
                        newForecast.Supplier = dataPoint.Supplier;
                        newForecast.ExoPOs = poLine.DataPoints;
                        projections.Add(newForecast);
                    }

                    //gets the forecasted data into dates bucket in the row and adds to datatable
                    foreach (POForecastProjection projection in projections)
                    {
                        DataRow newRow = DataPointsTable.NewRow();
                        newRow[columnEntity] = projection;
                        updateRowPOForecast(alignedDataDateCollection, DisplayEntities, allExoActuals, ActualsCutOffDate, projection.PONO, newRow);
                        dataPointsTable.Rows.Add(newRow);
                    }

                    //TableViewService.ScrollToLast();
                    IsForecastLoading = false;
                    this.RaisePropertyChanged(x => x.PODetails);
                    this.RaisePropertyChanged(x => x.IsForecastLoading);
                }

                return dataPointsTable;
            }
        }

        private bool generateAlignedDataDates()
        {
            if (MainViewModel == null || ForecastStartDate == null)
                return false;

            //since displayentities comes from mainviewmodel it should be populated by now
            DateTime latestDate = DisplayEntities.Count == 0 ? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).AddDays(-1) : DisplayEntities.Max(x => x.FORECAST_DATE);
            if (latestDate > ForecastEndDate)
                ForecastEndDate = latestDate;

            DateTime earliestDateBeginningOfMonth = new DateTime(((DateTime)ForecastStartDate).Year, ((DateTime)ForecastStartDate).Month, 1);
            alignedDataDateCollection = ChronologicalHelpers.GenerateEndDatesCollection(earliestDateBeginningOfMonth, ForecastEndDate);

            return true;
        }

        private POLine getPOLine(string poNo)
        {
            return getPOLines().FirstOrDefault(x => x.PONumber == poNo);
        }

        private List<POLine> getPOLines()
        {
            if (allExoPos == null)
                return new List<POLine>();

            return allExoPos.GroupBy(x => x.PONumber).Select(group => new POLine() { PONumber = group.Key, DataPoints = group.ToList() }).ToList();
        }

        public void AutoGeneratingColumns(AutoGeneratingColumnEventArgs e)
        {
            if (hiddenColumnFieldNames.Any(x => x == e.Column.FieldName))
            {
                e.Cancel = true;
            }
            else
            {
                GridControl gridControl = (GridControl)e.Source;
                DateTime parsedate;
                if (DateTime.TryParse(e.Column.FieldName, out parsedate))
                {
                    e.Column.CellTemplate = System.Windows.Application.Current.Resources["POForecastTemplate"] as DataTemplate;
                    e.Column.HeaderTemplate = System.Windows.Application.Current.Resources["POForecastHeaderTemplate"] as DataTemplate;
                    GridControlService.AddSummary(e.Column.FieldName, SummaryItemType.Sum, "c0");
                    e.Column.FilterPopupMode = FilterPopupMode.CheckedList;
                    e.Column.ReadOnly = false;
                    e.Column.FixedWidth = true;
                    e.Column.Width = 60;
                }
            }
        }

        public void DatesCellValueChanging(CellValueChangedEventArgs e)
        {
            DateTime parseDateTime;
            if(DateTime.TryParse(e.Column.FieldName, out parseDateTime) && e.Value != null)
            {
                decimal newValue = (decimal)e.Value;
                DataRowView dataRowView = (DataRowView)e.Row;
                findExistingOrAddNewFORECAST_PO(dataRowView.Row, parseDateTime, newValue, true);

                updateRowPOForecast(alignedDataDateCollection, DisplayEntities, allExoActuals, ActualsCutOffDate, string.Empty, dataRowView.Row);
                addUndo(dataRowView.Row, e.Column.FieldName, e.OldValue, newValue, EntityMessageType.Changed);
            }
        }

        private void clearPOForecast(string poNo)
        {
            List<FORECAST_PO> removePOForecasts = DisplayEntities.Where(x => x.PONO == poNo).ToList();
            MainViewModel.BaseBulkDelete(removePOForecasts);
        }

        private void findExistingOrAddNewFORECAST_PO(DataRow dataRow, DateTime forecastDate, decimal? viewCosts, bool skipUpdating = false)
        {
            POForecastProjection entity = (POForecastProjection)dataRow[columnEntity];

            //each PO have multiple items, so we need to store the pro-rated value per PO items in the database
            decimal proRateOnPOItem = 1;
            if(entity.PO_RemainingPrice > 0)
                proRateOnPOItem = (decimal)viewCosts / entity.PO_RemainingPrice;

            var groupByCodesPOItems = entity.ExoPOs.GroupBy(g => new { PONumber = g.PONumber, JobCode = g.Subjob_Name, DisciplineCode = g.Discipline_Code, CommodityCode = g.Commodity_Code, VariationCode = g.Variation_Code }).Select(g => new { PONumber = g.Key.PONumber, JobCode = g.Key.JobCode, DisciplineCode = g.Key.DisciplineCode, CommodityCode = g.Key.CommodityCode, VariationCode = g.Key.VariationCode, RemainingCosts = g.Sum(x => x.Costs) });
            foreach (var groupByCodesPOItem in groupByCodesPOItems)
            {
                FORECAST_PO findFORECAST_PO = DisplayEntities.FirstOrDefault(x => x.FORECAST_DATE == forecastDate.Date && x.PONO == groupByCodesPOItem.PONumber && x.COMMODITY_CODE == groupByCodesPOItem.CommodityCode && x.DISCIPLINE_CODE == groupByCodesPOItem.DisciplineCode && x.VARIATION_CODE == groupByCodesPOItem.VariationCode && x.JOB_CODE == groupByCodesPOItem.JobCode);

                if (findFORECAST_PO == null)
                {
                    findFORECAST_PO = new FORECAST_PO();
                    findFORECAST_PO.GUID = Guid.Empty;
                }

                findFORECAST_PO.GUID_PROJECT = loadPROJECT.GUID;
                findFORECAST_PO.PONO = groupByCodesPOItem.PONumber;
                findFORECAST_PO.JOB_CODE = groupByCodesPOItem.JobCode;
                findFORECAST_PO.DISCIPLINE_CODE = groupByCodesPOItem.DisciplineCode;
                findFORECAST_PO.COMMODITY_CODE = groupByCodesPOItem.CommodityCode;
                findFORECAST_PO.VARIATION_CODE = groupByCodesPOItem.VariationCode;
                findFORECAST_PO.FORECAST_DATE = new DateTime(forecastDate.Year, forecastDate.Month, forecastDate.Day);
                if (viewCosts == null || ((decimal)viewCosts) == 0.00m)
                    findFORECAST_PO.FORECAST_VALUE = null;
                else
                {
                    findFORECAST_PO.FORECAST_VALUE = groupByCodesPOItem.RemainingCosts * proRateOnPOItem;
                }

                MainViewModel.Save(findFORECAST_PO);
            }

            if(!skipUpdating)
                updateRowPOForecast(alignedDataDateCollection, DisplayEntities, allExoActuals, ActualsCutOffDate, string.Empty, dataRow);
        }

        public void ValidateCell(GridCellValidationEventArgs e)
        {
            //if (e.Column.FieldName.Contains(BindableBase.GetPropertyName(() => new POForecastProjection().PaymentTerms))
            // || e.Column.FieldName.Contains(BindableBase.GetPropertyName(() => new POForecastProjection().RemainingPeriodEdit))
            // || e.Column.FieldName.Contains(BindableBase.GetPropertyName(() => new POForecastProjection().FirstForecastDate)))
            //{
            //    DataRowView dataRowView = (DataRowView)e.Row;
            //    POForecastProjection projection = (POForecastProjection)dataRowView[columnEntity];
            //    initializeForecastConfig(projection);

            //    if (e.IsValid)
            //    {
            //        MainViewModel.Save(projection.ForecastConfig);
            //        generatePOForecast(projection, alignedDataDateCollection);
            //        GridControlService.RefreshData();
            //        TableView tableView = e.Source as TableView;
            //        tableView.CloseEditor();
            //    }
            //}
        }

        public bool CanShowCustomPaymentDialog
        {
            get
            {
                return SelectedDataRow != null;
            }
        }

        decimal? spreadPeriod = null;
        decimal? spreadInterval = null;
        /// <summary>
        /// Show dialog to allow user to input custom dates and percentage for a PO
        /// </summary>
        /// <param name="projection">Custom dates</param>
        /// <returns>User clicks ok</returns>
        public void PaymentSpread(object parameter)
        {
            GridControl gridControl = (GridControl)parameter;
            paymentSpread(gridControl);
        }

        private void paymentSpread(GridControl gridControl, bool useDefaultSpreadPeriod = false)
        {
            TableView tableView = gridControl.View as TableView;
            var selectedCells = tableView.GetSelectedCells();

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

            POSpreadViewModel poSpreadViewModel = POSpreadViewModel.Create(spreadPeriod, spreadInterval);
            if (useDefaultSpreadPeriod || CustomPODialogService.ShowDialog(MessageButton.OKCancel, "Change Spread Parameter", "POSpreadView", poSpreadViewModel) == MessageResult.OK)
            {
                //remember user choice
                spreadPeriod = poSpreadViewModel.Period;
                spreadInterval = poSpreadViewModel.Interval;

                if (spreadPeriod == null || spreadInterval == null)
                    return;

                EntitiesUndoRedoManager.PauseActionId();
                var selected_cells_groupby_columns = selected_cells.GroupBy(x => x.Column.FieldName).Select(group => new { FieldName = group.Key, Cells = group.ToList() });
                GridCell first_selected_cell = selected_cells.First();
                GridCell last_selected_cell = selected_cells.Last();

                int first_row_handle = selected_cells.Min(x => x.RowHandle);
                int last_row_handle = selected_cells.Max(x => x.RowHandle);
                int first_row_visible_index = gridControl.GetRowVisibleIndexByHandle(first_row_handle);
                int last_row_visible_index = gridControl.GetRowVisibleIndexByHandle(last_row_handle);
                int numberOfSelectedRows = (last_row_visible_index - first_row_visible_index) + 1;

                List<GridColumn> visible_columns = tableView.VisibleColumns.ToList();
                int first_column_visible_index = visible_columns.First(x => x.FieldName == first_selected_cell.Column.FieldName).VisibleIndex;
                int last_column_visible_index = visible_columns.First(x => x.FieldName == last_selected_cell.Column.FieldName).VisibleIndex;

                int rowOffsetSelection = numberOfSelectedRows;
                int columnOffsetSelection = (int)((decimal)spreadPeriod * (decimal)spreadInterval);

                int pasteValueRowOffset = 0;
                //becayse the date doesn't exists yet in the datatable
                bool forceRefreshDataTable = false;
                for (int rowOffset = 0; rowOffset < rowOffsetSelection; rowOffset++)
                {
                    int current_row_visible_index = first_row_visible_index + rowOffset;
                    int current_row_handle = gridControl.GetRowHandleByVisibleIndex(current_row_visible_index);
                    object rowObject = gridControl.GetRow(current_row_handle);
                    if (rowObject == null)
                        continue;

                    DataRowView editing_row_view = (DataRowView)rowObject;
                    DataRow editing_row = editing_row_view.Row;
                    POForecastProjection projection = (POForecastProjection)editing_row[columnEntity];
                    clearPOForecast(projection.PONO);
                    decimal costPerPeriod = projection.PO_RemainingPrice / (decimal)spreadPeriod;
                    DateTime? lastProcessedDate = null;

                    for (int columnOffset = 0; columnOffset < columnOffsetSelection; columnOffset += (int)spreadInterval)
                    {
                        string parseFieldName = string.Empty;
                        GridColumn current_column = null;
                        object oldValue = null;
                        if (!visible_columns.Any(x => x.VisibleIndex == (first_column_visible_index + columnOffset)))
                        {
                            if (lastProcessedDate == null)
                                continue;

                            parseFieldName = ((DateTime)lastProcessedDate).AddMonths((int)spreadInterval).AddDays(-1).ToString(BluePrintsResources.ColumnDateFormat);
                            oldValue = 0.00m;
                            forceRefreshDataTable = true;
                        }
                        else
                        {
                            current_column = visible_columns.First(x => x.VisibleIndex == (first_column_visible_index + columnOffset));
                            if (parseFieldName == string.Empty)
                                parseFieldName = current_column.FieldName;
                        }

                        DateTime parseDateTime;
                        if (DateTime.TryParse(parseFieldName, out parseDateTime))
                        {
                            if(dataPointsTable.Columns.Contains(parseFieldName))
                                oldValue = editing_row[parseFieldName];

                            addUndo(editing_row, parseFieldName, oldValue, costPerPeriod, EntityMessageType.Changed);
                            findExistingOrAddNewFORECAST_PO(editing_row, parseDateTime, costPerPeriod, true);
                            lastProcessedDate = parseDateTime;
                        }
                    }

                    if (!forceRefreshDataTable)
                    {
                        updateRowPOForecast(alignedDataDateCollection, DisplayEntities, allExoActuals, ActualsCutOffDate, string.Empty, editing_row);

                        //because grid doesn't refresh totals
                        GridControlService.RefreshData();
                    }

                    pasteValueRowOffset += 1;
                }

                if (forceRefreshDataTable)
                    refreshDataTable();

                EntitiesUndoRedoManager.UnpauseActionId();
            }
        }

        private void updateRowPOForecast(List<DateTime> alignedDates, IEnumerable<FORECAST_PO> FORECAST_POCollection, IEnumerable<ExoDataPoint> allActuals, DateTime cutOffDate, string POno = "", DataRow PORow = null)
        {
            if(PORow == null && POno != string.Empty)
                PORow = findPORow(POno);

            if (PORow != null)
            {
                POForecastProjection forecast = (POForecastProjection)PORow[columnEntity];
                forecast.UpdateForecastPayments(FORECAST_POCollection, allActuals, cutOffDate);

                //reset datarow dates
                foreach (DateTime alignedDate in alignedDataDateCollection)
                {
                    PORow[alignedDate.ToString(BluePrintsResources.ColumnDateFormat)] = 0;
                }

                foreach (ExoDataPoint forecastPayment in forecast.ForecastPayments)
                {
                    DateTime? alignedDataDate = alignedDataDateCollection.OrderBy(x => x).FirstOrDefault(x => x.Date >= forecastPayment.ActualDate);
                    if (alignedDataDate == null || ((DateTime)alignedDataDate).Year == 1)
                    {
                        refreshDataTable();
                        return;
                    }
                    else
                    {
                        string alignedDateField = ((DateTime)alignedDataDate).ToString(BluePrintsResources.ColumnDateFormat);
                        PORow[alignedDateField] = forecastPayment.Costs;
                    }
                }
            }
        }

        public void AlignPOsWithActuals()
        {
            IEnumerable<POForecastProjection> projections = from DataRow dr in dataPointsTable.Rows
                                                            select (POForecastProjection)dr[columnEntity];

            List <FORECAST_PO> saveFORECAST_POs = new List<FORECAST_PO>();
            //fix codes mis-alignment

            decimal adjustmentFactor = 0;
            //fix dates mis-alignment
            foreach (POForecastProjection projection in projections)
            {
                decimal totalForecastValue = projection.FORECAST_POs.Where(x => x.FORECAST_VALUE != null).Sum(x => (decimal)x.FORECAST_VALUE);
                if(totalForecastValue == 0)
                {
                    DataRow editing_row = findPORow(projection.PONO);
                    if(editing_row != null)
                    {
                        findExistingOrAddNewFORECAST_PO(editing_row, (DateTime)ForecastStartDate, projection.PO_RemainingPrice);
                    }
                }
                else
                {

                    var groupedFORECAST_POs = projection.FORECAST_POs.GroupBy(x => x.JOB_CODE + x.DISCIPLINE_CODE + x.COMMODITY_CODE).Select(group => new { FirstJob = group.First(), Forecasts = group.ToList() });
                    foreach (var groupedFORECAST_PO in groupedFORECAST_POs)
                    {
                        var firstFORECAST_PO = groupedFORECAST_PO.FirstJob;
                        decimal remainingUnits = projection.ExoPOs.Where(x => x.Subjob_Name == firstFORECAST_PO.JOB_CODE && x.Discipline_Code == firstFORECAST_PO.DISCIPLINE_CODE && x.Commodity_Code == firstFORECAST_PO.COMMODITY_CODE).Sum(x => x.Costs);
                        decimal groupUnits = groupedFORECAST_PO.Forecasts.Where(x => x.FORECAST_VALUE != null).Sum(x => (decimal)x.FORECAST_VALUE);

                        if (remainingUnits != groupUnits)
                        {
                            adjustmentFactor = remainingUnits / groupUnits;
                            foreach (FORECAST_PO forecast_po in groupedFORECAST_PO.Forecasts)
                            {
                                if (forecast_po.FORECAST_VALUE != null)
                                    forecast_po.FORECAST_VALUE *= adjustmentFactor;
                            }
                        }
                    }

                    decimal viewForecastCosts = projection.FORECAST_POs.Where(x => x.FORECAST_DATE.Date > ActualsCutOffDate.Date && x.FORECAST_VALUE != null).Sum(x => (decimal)x.FORECAST_VALUE);
                    decimal costDifferences = projection.PO_RemainingPrice - viewForecastCosts;
                    foreach (FORECAST_PO FORECAST_PO in projection.FORECAST_POs.OrderBy(x => x.FORECAST_DATE))
                    {
                        if (FORECAST_PO.FORECAST_DATE.Date <= ActualsCutOffDate.Date)
                        {
                            //store as 0 so that when we rewind and adjust actuals again this point will actually be used
                            FORECAST_PO.FORECAST_VALUE = 0.00m;
                            saveFORECAST_POs.Add(FORECAST_PO);
                            continue;
                        }

                        //cost adjustment
                        if (costDifferences > 0)
                        {
                            FORECAST_PO.FORECAST_VALUE += costDifferences;
                            saveFORECAST_POs.Add(FORECAST_PO);
                            break;
                        }
                        else if (costDifferences < 0)
                        {
                            decimal forecastValue = FORECAST_PO.FORECAST_VALUE == null ? 0 : (decimal)FORECAST_PO.FORECAST_VALUE;
                            decimal postAdjustmentCosts = forecastValue + costDifferences;
                            if (postAdjustmentCosts > 0)
                            {
                                FORECAST_PO.FORECAST_VALUE += costDifferences;
                                saveFORECAST_POs.Add(FORECAST_PO);
                                break;
                            }
                            else
                            {
                                costDifferences += forecastValue;
                                FORECAST_PO.FORECAST_VALUE = 0.00m;
                                saveFORECAST_POs.Add(FORECAST_PO);
                            }
                        }
                        else if (costDifferences == 0)
                        {
                            //need to save adjustment anyway
                            if (adjustmentFactor != 0)
                                saveFORECAST_POs.Add(FORECAST_PO);
                        }
                    }
                }
            }

            LastAlignedDate = DateTime.Now;
            MainViewModel.BulkSave(saveFORECAST_POs);
            refreshDataTable();
        }

        bool? isForecastLoading = null;
        public bool IsForecastLoading { get; set; }
        private void refreshDataTable()
        {
            generateAlignedDataDates();
            dataPointsTable = null;
            this.RaisePropertyChanged(x => x.DataPointsTable);
        }

        private DataRow findPORow(string PONumber)
        {
            return (from DataRow dr in dataPointsTable.Rows
                    where ((POForecastProjection)dr[columnEntity]).PONO == PONumber
                    select dr).FirstOrDefault();
        }

        public override void FullRefresh()
        {
            EntitiesUndoRedoManager.Clear();
            dataPointsTable = null;
            allPODetails = null;
            //loadExoData();
            IsForecastLoading = true;
            this.RaisePropertyChanged(x => x.IsForecastLoading);
            base.FullRefresh();
        }

        public DateTime ActualsCutOffDate
        {
            get
            {
                if (ForecastStartDate == null)
                    return DateTime.Now;
                else
                {
                    DateTime forecastStartDate = (DateTime)ForecastStartDate;
                    return new DateTime(forecastStartDate.Year, forecastStartDate.Month, 1).AddDays(-1);
                }
            }
        }

        public DateTime? ForecastStartDate { get; set; }
        public DateTime ForecastEndDate { get; set; }

        public bool CanSaveDateAndRefresh()
        {
            return !IsForecastLoading;
        }

        public void SaveDateAndRefresh()
        {
            if(ForecastStartDate != null)
            {
                DateTime saveDateTime = (DateTime)ForecastStartDate;
                loadPROJECT.FORECAST_END_DATE = ForecastEndDate;
                loadPROJECT.FORECAST_DATA_DATE = new DateTime(((DateTime)saveDateTime).Year, ((DateTime)saveDateTime).Month, 1).AddDays(-1);
                PROJECTCollectionViewModel.Save(loadPROJECT);
                refreshDataTable();

                this.RaisePropertyChanged(x => x.ForecastStartDate);
            }
        }

        public DateTime? LastAlignedDate
        {
            get
            {
                //do this to prevent binding errors
                if (loadPROJECT != null && loadPROJECT.FORECAST_PO_LAST_ALIGNED != null)
                    return (DateTime)loadPROJECT.FORECAST_PO_LAST_ALIGNED;

                return null;
            }
            set
            {
                if (!IsForecastLoading)
                {
                    loadPROJECT.FORECAST_PO_LAST_ALIGNED = value;
                    PROJECTCollectionViewModel.Save(loadPROJECT);

                    this.RaisePropertyChanged(x => x.LastAlignedDate);
                }
            }
        }

        public DataRowView SelectedDataRow { get; set; }

        ObservableCollection<DataRowView> selectedDataRows { get; set; }
        public ObservableCollection<DataRowView> SelectedDataRows
        {
            get
            {
                if (selectedDataRows == null)
                {
                    selectedDataRows = new ObservableCollection<DataRowView>();
                    selectedDataRows.CollectionChanged += SelectedDataRows_CollectionChanged;
                }

                return selectedDataRows;
            }
            set
            {
                selectedDataRows = value;
            }
        }

        private void SelectedDataRows_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //this.RaisePropertyChanged(x => x.PODetails);
            selectedItemsChangedDispatcher.Tick -= SelectedItemsChangedDispatcher_Tick;
            selectedItemsChangedDispatcher.Tick += SelectedItemsChangedDispatcher_Tick;
            selectedItemsChangedDispatcher.Start();
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

        private void SelectedItemsChangedDispatcher_Tick(object sender, EventArgs e)
        {
            selectedItemsChangedDispatcher.Stop();
            //setFilter();
            //this.RaisePropertyChanged(x => x.PODetails);
        }

        List<ExoDataPoint> allPODetails;
        public IEnumerable<ExoDataPoint> PODetails
        {
            get
            {
                if (IsLoading || IsForecastLoading)
                    return new List<ExoDataPoint>();

                if(allPODetails == null)
                {
                    allPODetails = new List<ExoDataPoint>();
                    allPODetails.AddRange(allExoPos);
                    allPODetails.AddRange(allExoActuals);
                }

                return allPODetails;
            }
        }

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
                if (clickRowData.Row == null)
                {
                    GridControl masterGrid = tableView.Grid;
                    var selected_cells = Enumerable.Range(0, masterGrid.VisibleRowCount)
                    .Select(x => (GridControl)masterGrid.GetDetail(x))
                    .Where(x => x != null).
                    Select(x => ((TableView)(x).View).FocusedRowData).ToList();

                    clickRowData = selected_cells.FirstOrDefault();
                }

                if (clickRowData != null)
                    setFilter((DataRowView)clickRowData.Row, hi.Column);
            }
            catch (Exception ex)
            {
                string s = ex.ToString();
            }
        }
        public bool IsPOColumnsVisible { get; set; }
        private void setFilter(DataRowView dataRowView, GridColumn gridColumn)
        {
            if (SelectedDataRows == null || SelectedDataRows.Count == 0)
                return;

            if (gridColumn.FieldName.ToUpper().Contains("PO_REMAININGPRICE"))
            {
                POForecastProjection entity = (POForecastProjection)dataRowView[columnEntity];
                FilterCriteria = CriteriaOperator.Parse("[PONumber] = '" + entity.PONO + "' And [IsPO] = 'True'");
            }
            else if (gridColumn.FieldName.ToUpper().Contains("PO_TOTALPRICE"))
            {
                POForecastProjection entity = (POForecastProjection)dataRowView[columnEntity];
                FilterCriteria = CriteriaOperator.Parse("[PONumber] = '" + entity.PONO + "'");
            }
            else if (gridColumn.FieldName.ToUpper().Contains("PO_INVOICED"))
            {
                POForecastProjection entity = (POForecastProjection)dataRowView[columnEntity];
                FilterCriteria = CriteriaOperator.Parse("[PONumber] = '" + entity.PONO + "' And [IsPO] = 'False'");
            }



            IsPOColumnsVisible = false;
            this.RaisePropertyChanged(x => x.FilterCriteria);
            this.RaisePropertyChanged(x => x.IsPOColumnsVisible);
        }

        private void clearFilter()
        {
            FilterCriteria = null;
            this.RaisePropertyChanged(x => x.FilterCriteria);
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

        public void Window_KeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                if (spreadInterval == null)
                    spreadInterval = 1;

                bool isNumberKeyPressed = false;
                if(e.Key == Key.D1 || e.Key == Key.NumPad1)
                {
                    spreadPeriod = 1;
                    isNumberKeyPressed = true;
                }

                if (e.Key == Key.D2 || e.Key == Key.NumPad2)
                {
                    spreadPeriod = 2;
                    isNumberKeyPressed = true;
                }

                if (e.Key == Key.D3 || e.Key == Key.NumPad3)
                {
                    spreadPeriod = 3;
                    isNumberKeyPressed = true;
                }

                if (e.Key == Key.D4 || e.Key == Key.NumPad4)
                {
                    spreadPeriod = 4;
                    isNumberKeyPressed = true;
                }

                if (e.Key == Key.D5 || e.Key == Key.NumPad5)
                {
                    spreadPeriod = 5;
                    isNumberKeyPressed = true;
                }

                if (e.Key == Key.D6 || e.Key == Key.NumPad6)
                {
                    spreadPeriod = 6;
                    isNumberKeyPressed = true;
                }

                if (e.Key == Key.D7 || e.Key == Key.NumPad7)
                {
                    spreadPeriod = 7;
                    isNumberKeyPressed = true;
                }

                if (e.Key == Key.D8 || e.Key == Key.NumPad8)
                {
                    spreadPeriod = 8;
                    isNumberKeyPressed = true;
                }

                if (e.Key == Key.D9 || e.Key == Key.NumPad9)
                {
                    spreadPeriod = 9;
                    isNumberKeyPressed = true;
                }

                if(isNumberKeyPressed)
                {
                    paymentSpread(GridControlService.GridControl, true);
                    closeEditorDispatcher.Start();
                }
            }
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

        public void KeyboardCopy()
        {
            System.Windows.Forms.SendKeys.SendWait("^c");
        }

        public void KeyboardPaste()
        {
            System.Windows.Forms.SendKeys.SendWait("^v");
        }

        protected IDialogService CustomPODialogService
        {
            get { return this.GetRequiredService<IDialogService>("CustomPODialogService"); }
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "PROJECTPOForecastViewModelWrapper"; }
        }

        protected override void OnClose(CancelEventArgs e)
        {
            GlobalMethods.SetAccordionExpandedState?.Invoke(true);
            base.OnClose(e);
        }
        #endregion

        public class POLine
        {
            public string PONumber { get; set; }
            public List<ExoDataPoint> DataPoints { get; set; }
        }
    }
}