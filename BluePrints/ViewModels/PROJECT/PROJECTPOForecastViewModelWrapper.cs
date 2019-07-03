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
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
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
        protected PROJECT loadPROJECT;
        List<DateTime> alignedDataDateCollection;
        List<ExoDataPoint> exoPOs = new List<ExoDataPoint>();
        //List<ExoDataPoint> exoMaterials = new List<ExoDataPoint>();
        List<string> hiddenColumnFieldNames = new List<string>();
        protected string columnEntity = "Entity";
        protected string columnTotalForecast = "TotalForecast";
        protected string columnUnforecasted = "Unforecasted";
        DispatcherTimer selectedItemsChangedDispatcher;
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
            hiddenColumnFieldNames.Add(columnEntity);

            selectedItemsChangedDispatcher = new DispatcherTimer();
            selectedItemsChangedDispatcher.Interval = new TimeSpan(0, 0, 0, 0, 1);
            bluePrintsUnitOfWork = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
            GlobalMethods.SetAccordionExpandedState?.Invoke(false);
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
        }

        private Func<IRepositoryQuery<Data.PROJECT>, IQueryable<Data.PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<FORECAST_PO_DATE>, IQueryable<FORECAST_PO_DATE>> FORECAST_PO_DATEProjectionFunc()
        {
            return query => query.Where(x => x.FORECAST_PO.GUID_PROJECT == loadPROJECT.GUID);
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

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<FORECAST_PO> entities)
        {
            exoPOs = BluePrintsDataUtils.GetEXOPO(loadPROJECT.NUMBER);
            //exoMaterials = BluePrintsDataUtils.GetMaterials(loadPROJECT.NUMBER);
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        public override string UnifiedValueValidation(FORECAST_PO projection, string field_name, object new_value)
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
            string newValueString = Clipboard.GetText().ToString();

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

        private bool basePasteData(DataRow newRow, ColumnBase copyColumn, string pasteData)
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

                    findExistingOrAddNewFORECAST_PO(newRow, columnDateTime, viewDecimalValue);
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
                if (MainViewModel == null || exoPOs == null)
                    return null;

                if (dataPointsTable == null)
                {
                    //generate aligned dates
                    if (!generateAlignedDataDates())
                        return null;

                    //initialize datatable schema
                    dataPointsTable = new DataTable();
                    dataPointsTable.Columns.Add(columnEntity, typeof(POForecastProjection));
                    dataPointsTable.Columns.Add(columnTotalForecast, typeof(decimal));
                    dataPointsTable.Columns.Add(columnUnforecasted, typeof(decimal));

                    foreach (DateTime alignedDataDate in alignedDataDateCollection)
                    {
                        string columnFieldName = alignedDataDate.Date.ToShortDateString();
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
                        updateRowPOForecast(alignedDataDateCollection, DisplayEntities, projection.PONO, newRow);
                        dataPointsTable.Rows.Add(newRow);
                    }

                    TableViewService.ScrollToLast();
                }

                return dataPointsTable;
            }
        }

        private bool generateAlignedDataDates()
        {
            if (MainViewModel == null)
                return false;

            //since displayentities comes from mainviewmodel it should be populated by now
            DateTime latestDate = DisplayEntities.Count == 0 ? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).AddDays(-1) : DisplayEntities.Max(x => x.FORECAST_DATE);
            if (latestDate > ForecastEndDate)
                ForecastEndDate = latestDate;

            DateTime earliestDateBeginningOfMonth = new DateTime(ForecastStartDate.Year, ForecastStartDate.Month, 1);
            alignedDataDateCollection = ChronologicalHelpers.GenerateMonthEndDatesCollection(earliestDateBeginningOfMonth, ForecastEndDate);

            return true;
        }

        private POLine getPOLine(string poNo)
        {
            return getPOLines().FirstOrDefault(x => x.PONumber == poNo);
        }

        private List<POLine> getPOLines()
        {
            if (exoPOs == null)
                return new List<POLine>();

            return exoPOs.GroupBy(x => x.PONumber).Select(group => new POLine() { PONumber = group.Key, DataPoints = group.ToList() }).ToList();
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
                    e.Column.CellTemplate = Application.Current.Resources["POForecastTemplate"] as DataTemplate;
                    GridControlService.AddSummary(e.Column.FieldName, SummaryItemType.Sum, "c0");
                    e.Column.FilterPopupMode = FilterPopupMode.CheckedList;
                    e.Column.ReadOnly = false;
                    e.Column.FixedWidth = true;
                    e.Column.Width = 75;
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
                findExistingOrAddNewFORECAST_PO(dataRowView.Row, parseDateTime, newValue);

                updateRowPOForecast(alignedDataDateCollection, DisplayEntities, string.Empty, dataRowView.Row);
                addUndo(dataRowView.Row, e.Column.FieldName, e.OldValue, newValue, EntityMessageType.Changed);
            }
        }

        private void findExistingOrAddNewFORECAST_PO(DataRow dataRow, DateTime forecastDate, decimal? viewCosts)
        {
            POForecastProjection entity = (POForecastProjection)dataRow[columnEntity];
            FORECAST_PO findFORECAST_PO = DisplayEntities.FirstOrDefault(x => x.FORECAST_DATE == forecastDate.Date && x.PONO == entity.PONO);

            if(findFORECAST_PO == null)
            {
                findFORECAST_PO = new FORECAST_PO();
                findFORECAST_PO.GUID = Guid.Empty;
            }

            findFORECAST_PO.GUID_PROJECT = loadPROJECT.GUID;
            findFORECAST_PO.PONO = entity.PONO;
            findFORECAST_PO.FORECAST_DATE = new DateTime(forecastDate.Year, forecastDate.Month, forecastDate.Day);
            if (viewCosts != null && ((decimal)viewCosts) == 0.00m)
                findFORECAST_PO.FORECAST_PERCENT = null;
            else
                findFORECAST_PO.FORECAST_PERCENT = viewCosts == null ? (double?)null : Convert.ToDouble(viewCosts / Math.Round(entity.PO_TotalPrice));

            MainViewModel.Save(findFORECAST_PO);

            updateRowPOForecast(alignedDataDateCollection, DisplayEntities, string.Empty, dataRow);
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

        public void ShowCustomPaymentDialog()
        {
            //POForecastProjection projection = (POForecastProjection)SelectedDataRow[columnEntity];
            //initializeForecastConfig(projection);
            //if (showCustomPaymentDialog(projection))
            //{
            //    projection.ForecastConfig.MODE = POPaymentTerms.Custom;
            //    projection.PaymentTerms = POPaymentTerms.Custom;
            //    MainViewModel.Save(projection.ForecastConfig);
            //    generatePOForecast(projection, alignedDataDateCollection);

            //    GridControlService.RefreshData();
            //}
        }

        /// <summary>
        /// Show dialog to allow user to input custom dates and percentage for a PO
        /// </summary>
        /// <param name="projection">Custom dates</param>
        /// <returns>User clicks ok</returns>
        private bool showCustomPaymentDialog(POForecastProjection projection)
        {
            //FORECAST_PO_DATECollectionViewModelWrapper POCustomDateViewModel = FORECAST_PO_DATECollectionViewModelWrapper.Create();
            //POCustomDateViewModel.OnParameterChange(projection.ForecastConfig);
            //if (CustomPODialogService.ShowDialog(MessageButton.OKCancel, "Assign payment dates and percentages", "FORECAST_PO_DATECollectionView", POCustomDateViewModel) == MessageResult.OK)
            //{
            //    projection.FORECAST_POs = FORECAST_PO_DATECollection.Where(x => x.GUID_FORECAST_PO == projection.ForecastConfig.GUID).ToList();
            //    decimal totalPercent = projection.FORECAST_POs.Sum(x => x.PAYMENT_PERCENT);
            //    if (totalPercent < 1 || totalPercent > 1)
            //    {
            //        MessageBoxService.ShowMessage("Please make sure % is 100%");
            //        return showCustomPaymentDialog(projection);
            //    }
            //    else
            //    {
            //        POCustomDateViewModel.Dispose();
            //        return true;
            //    }
            //}
            //else
            //{
            //    POCustomDateViewModel.Dispose();
            //    return false;
            //}

            return false;
        }

        private void updateRowPOForecast(List<DateTime> alignedDates, IEnumerable<FORECAST_PO> FORECAST_POCollection, string POno = "", DataRow PORow = null, bool refreshRow = false)
        {
            if(PORow == null && POno != string.Empty)
                PORow = findPORow(POno);

            if (PORow != null)
            {
                POForecastProjection forecast = (POForecastProjection)PORow[columnEntity];
                forecast.UpdateForecastPayments(FORECAST_POCollection);

                //reset datarow dates
                foreach (DateTime alignedDate in alignedDataDateCollection)
                {
                    PORow[alignedDate.ToShortDateString()] = 0;
                }

                decimal totalForecast = 0;
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
                        string alignedDateField = ((DateTime)alignedDataDate).ToShortDateString();
                        PORow[alignedDateField] = forecastPayment.Costs;
                        totalForecast += forecastPayment.Costs;
                    }
                }

                PORow[columnUnforecasted] = forecast.PO_RemainingPrice - totalForecast;
                PORow[columnTotalForecast] = totalForecast;
            }
        }

        private void refreshDataTable()
        {
            dataPointsTable = null;
            this.RaisePropertyChanged(x => x.DataPointsTable);
        }

        private DataRow findPORow(string PONumber)
        {
            return (from DataRow dr in dataPointsTable.Rows
                    where ((POForecastProjection)dr[columnEntity]).PONO == PONumber
                    select dr).FirstOrDefault();
        }

        public IEnumerable<FORECAST_PO_DATE> FORECAST_PO_DATECollection
        {
            get
            {
                var collection = GetEntities<FORECAST_PO_DATE>();
                if (collection != null)
                {
                    collection = collection.OrderBy(x => x.PAYMENT_DATE);
                }

                return collection;
            }
        }

        public override void FullRefresh()
        {
            EntitiesUndoRedoManager.Clear();

            dataPointsTable = null;
            base.FullRefresh();
        }

        public DateTime ForecastStartDate
        {
            get
            {
                DateTime dataDate = DateTime.Now;

                //do this to prevent binding errors
                if (loadPROJECT != null && loadPROJECT.FORECAST_DATA_DATE != null)
                    dataDate = (DateTime)loadPROJECT.FORECAST_DATA_DATE;

                return new DateTime(((DateTime)dataDate).Year, ((DateTime)dataDate).Month, 1).AddMonths(2).AddDays(-1);
            }
            set
            {
                if (!IsLoading)
                {
                    DateTime saveDateTime = value;
                    loadPROJECT.FORECAST_DATA_DATE = new DateTime(((DateTime)saveDateTime).Year, ((DateTime)saveDateTime).Month, 1).AddDays(-1);
                    PROJECTCollectionViewModel.Save(loadPROJECT);
                    refreshDataTable();
                }
            }
        }

        public DateTime ForecastEndDate
        {
            get
            {
                //do this to prevent binding errors
                if (loadPROJECT == null || loadPROJECT.FORECAST_END_DATE == null)
                    return DateTime.Now;

                return (DateTime)loadPROJECT.FORECAST_END_DATE;
            }
            set
            {
                if (!IsLoading)
                {
                    loadPROJECT.FORECAST_END_DATE = value;
                    PROJECTCollectionViewModel.Save(loadPROJECT);
                    refreshDataTable();
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
            this.RaisePropertyChanged(x => x.PODetails);
            //selectedItemsChangedDispatcher.Tick -= SelectedItemsChangedDispatcher_Tick;
            //selectedItemsChangedDispatcher.Tick += SelectedItemsChangedDispatcher_Tick;
            //selectedItemsChangedDispatcher.Start();
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
            this.RaisePropertyChanged(x => x.PODetails);
        }

        public IEnumerable<ExoDataPoint> PODetails
        {
            get
            {
                foreach(var selectedDataRow in SelectedDataRows)
                {
                    POForecastProjection projection = (POForecastProjection)selectedDataRow[columnEntity];
                    foreach(var po in projection.ExoPOs)
                    {
                        yield return po;
                    }
                }
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