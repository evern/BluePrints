using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BaseModel.ViewModel.Services;
using BaseModel.ViewModel.UndoRedo;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
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
using DevExpress.Data.Filtering;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
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
    public class PROJECTPOSnapshotForecastViewModelWrapper : BluePrintsEntitiesCollectionWrapper<FORECAST_PO, FORECAST_PO, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of PROJECTPOSnapshotForecastViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static PROJECTPOSnapshotForecastViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new PROJECTPOSnapshotForecastViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the PROJECTPOSnapshotForecastViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTPOSnapshotForecastViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected PROJECTPOSnapshotForecastViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private IBluePrintsEntitiesUnitOfWork bluePrintsUnitOfWork;
        protected IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory;
        protected IPrimeroEntitiesUnitOfWork primeroUnitOfWork;

        protected PROJECT loadPROJECT;
        protected List<DateTime> alignedDataDateCollection;
        protected List<string> hiddenColumnFieldNames = new List<string>();
        protected string columnEntity = "Entity";
        DispatcherTimer selectedItemsChangedDispatcher;
        DispatcherTimer closeEditorDispatcher;
        public CriteriaOperator POFilterCriteria { get; set; }
        public CriteriaOperator ActualFilterCriteria { get; set; }
        DispatcherTimer delayedProjectSaveTimer;
        public bool IsWeeks => false; //used by POForecastHeaderTemplate
        BackgroundWorker exoLoadingBackgroundWorker = new BackgroundWorker();
        BackgroundWorker projectSavingBackgroundWorker = new BackgroundWorker();

        protected List<X_PURCHORD_LINE_DETAIL> X_PURCHORD_LINE_DETAILS;
        protected InstantFeedbackActualDetailsCollectionViewModelWrapper instantFeedbackActualDetailViewModel = InstantFeedbackActualDetailsCollectionViewModelWrapper.Create();
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
            delayedProjectSaveTimer = new DispatcherTimer();
            delayedProjectSaveTimer.Interval = new TimeSpan(0, 0, 0, 1);
            projectSavingBackgroundWorker.DoWork += ProjectSavingBackgroundWorker_DoWork;
            projectSavingBackgroundWorker.WorkerSupportsCancellation = true;

            setProject(loadPROJECT);

            primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(loadPROJECT.OfficeNameForExo);
            primeroUnitOfWork = primeroUnitOfWorkFactory.CreateUnitOfWork();

            hiddenColumnFieldNames.Add(columnEntity);

            selectedItemsChangedDispatcher = new DispatcherTimer();
            selectedItemsChangedDispatcher.Interval = new TimeSpan(0, 0, 0, 0, 1);

            closeEditorDispatcher = new DispatcherTimer();
            closeEditorDispatcher.Interval = new TimeSpan(0, 0, 0, 0, 1);
            closeEditorDispatcher.Tick += CloseEditorDispatcher_Tick;

            bluePrintsUnitOfWork = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
            GlobalMethods.SetAccordionExpandedState?.Invoke(false);

            exoLoadingBackgroundWorker.DoWork += exoLoadingBackgroundWorker_DoWork;
            exoLoadingBackgroundWorker.RunWorkerCompleted += ExoLoadingBackgroundWorker_RunWorkerCompleted;
            exoLoadingBackgroundWorker.WorkerSupportsCancellation = true;
            IsLoading = true;
            isHandleLoadedGridRows = true;
            this.RaisePropertyChanged(x => x.IsLoading);
        }

        bool isRefreshedAllData;
        protected override bool loadDataPointsTable()
        {            
            //Auto refresh forecast data on load
            if (!isRefreshedAllData && CurrentPO_FORECAST_JOB_HOUR_SNAPSHOTCollection.Count() == 0 && LoadDataDate != null)
            {
                RefreshAllForecastData();
                isRefreshedAllData = true;
                return false;
            }

            IsLoading = true;
            this.RaisePropertyChanged(x => x.IsLoading);

            dataPointsTable = null;

            updateDataPointsTable();
            this.RaisePropertyChanged(x => x.DataPointsTable);

            IsLoading = false;
            this.RaisePropertyChanged(x => x.IsLoading);

            //so filters will show transactions, as it is not shown during load, RaisePropertyChanged on ActualDetails will allow the grid to start showing data
            instantFeedbackActualDetailViewModel.OnParameterChange(loadPROJECT);
            CommonMethods.AddSaveLayoutHandler(GridControlService.GetGridColumns());
            return true;
        }

        private void ExoLoadingBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            isExoDataLoaded = true;
            BackgroundRefresh();
        }

        protected override void OnClose(CancelEventArgs e)
        {
            if(shouldPromptForSavingSnapshot)
            {
                if (MessageBoxService.ShowMessage("Forecast edited, do you wish to save a snapshot?", "Save", MessageButton.YesNo) == MessageResult.Yes)
                    SaveSnapshot();
            }

            exoLoadingBackgroundWorker.CancelAsync();
            GlobalMethods.SetAccordionExpandedState?.Invoke(true);
            base.OnClose(e);
        }

        private void exoLoadingBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (exoLoadingBackgroundWorker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            loadExoData(primeroUnitOfWork);
        }

        protected override void BackgroundRefresh(bool loadLayout = false)
        {
            if(isExoDataLoaded)
                base.BackgroundRefresh(true);
        }

        protected bool isExoDataLoaded = false;
        protected virtual void loadExoData(IPrimeroEntitiesUnitOfWork primeroUOW)
        {
            isExoDataLoaded = false;

            Common.LoadingScreenManager.SetMessage("Loading PO Details...");
            X_PURCHORD_LINE_DETAILS = PrimeroEntities.GetPurchaseOrdersDetail(primeroUnitOfWork, loadPROJECT.NUMBER, ActualsCutOffDate);
            Common.LoadingScreenManager.CloseLoadingScreen();

            generateAlignedDataDates();
            isExoDataLoaded = true;
        }

        private void CloseEditorDispatcher_Tick(object sender, EventArgs e)
        {
            closeEditorDispatcher.Stop();
            GridControlService.GridControl.View.CloseEditor();
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => setProject(x));
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.FORECASTS, FORECASTProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.FORECAST_PO_SETTINGS, FORECAST_PO_SETTINGProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.FORECAST_EACS, FORECAST_EACProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.FORECAST_JOB_HOUR_SNAPSHOTS, FORECAST_JOB_HOUR_SNAPSHOTProjectionFunc);
            loaderCollection.AddLoaderDescription<JOB_COSTGROUPS, JOB_COSTGROUPS, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOB_COSTGROUPS);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINE_DESCS, DISCIPLINE_DESCProjectionFunc);
            loaderCollection.AddLoaderDescription<STOCK_GROUPS, STOCK_GROUPS, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.STOCK_GROUPS);
            loaderCollection.AddLoaderDescription<STOCK_GROUP2S, STOCK_GROUP2S, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.STOCK_GROUP2S);
        }

        private Func<IRepositoryQuery<DISCIPLINE_DESC>, IQueryable<DISCIPLINE_DESC>> DISCIPLINE_DESCProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<FORECAST>, IQueryable<FORECAST>> FORECASTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<FORECAST_PO_SETTING>, IQueryable<FORECAST_PO_SETTING>> FORECAST_PO_SETTINGProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected virtual Func<IRepositoryQuery<FORECAST_EAC>, IQueryable<FORECAST_EAC>> FORECAST_EACProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected virtual Func<IRepositoryQuery<FORECAST_JOB_HOUR_SNAPSHOT>, IQueryable<FORECAST_JOB_HOUR_SNAPSHOT>> FORECAST_JOB_HOUR_SNAPSHOTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && (x.SNAPSHOT_TYPE == ForecastSnapshotValueType.Actual || x.SNAPSHOT_TYPE == ForecastSnapshotValueType.CurrentOutstandingPO));
        }

        public DateTime? LoadDataDate { get; set; }
        private void setProject(Data.PROJECT project)
        {
            loadPROJECT = project;

            DateTime dataDate;
            if (loadPROJECT.FORECAST_DATA_DATE != null)
            {
                dataDate = (DateTime)loadPROJECT.FORECAST_DATA_DATE;
                LoadDataDate = new DateTime(dataDate.Year, dataDate.Month, 1).AddMonths(1).AddSeconds(-1);
            }
            else
            {
                DateTime endOfCurrentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).AddSeconds(-1);

                loadPROJECT.FORECAST_DATA_DATE = endOfCurrentMonth;
                dataDate = endOfCurrentMonth;
                LoadDataDate = dataDate;
                savePROJECT();
            }

            ForecastStartDate = new DateTime(dataDate.Year, dataDate.Month, 1).AddMonths(2).AddDays(-1);

            DateTime endDate;
            if (loadPROJECT.FORECAST_END_DATE != null)
                endDate = (DateTime)loadPROJECT.FORECAST_END_DATE;
            else
                endDate = DateTime.Now;

            ForecastEndDate = endDate;

            this.RaisePropertiesChanged();
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
                mainThreadDispatcher.BeginInvoke(new Action(() => PROJECTCollectionViewModel.Save(loadPROJECT)));
        }

        bool shownMessage;
        private void showDataDateErrorMessage()
        {
            if(!shownMessage)
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage("Data date not set, please set data date from in forecast")));
                shownMessage = true;
            }
        }

        private Func<IRepositoryQuery<Data.PROJECT>, IQueryable<Data.PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.FORECAST_POS);
        }

        protected override Func<IRepositoryQuery<FORECAST_PO>, IQueryable<FORECAST_PO>> specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        //List<ExoDataPoint> materialDataPoints;
        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<FORECAST_PO> entities)
        {
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

        public override void PastingFromClipboard(PastingFromClipboardEventArgs e)
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

            }

            string[] RowData = DataUtils.ExcelSplit(newValueString).ToArray();
            List<ErrorMessage> errorMessages;
            pasteCellData(gridControl, gridTableView, RowData, out errorMessages);

            GridControlService.RefreshData();
            e.Handled = true;

            ShowErrorMessage("Error", errorMessages);
        }

        private void pasteCellData(GridControl gridControl, TableView gridTableView, string[] RowData, out List<ErrorMessage> errorMessages)
        {
            EntitiesUndoRedoManager.PauseActionId();
            GridControlHelpers.PasteCellData(gridControl, gridTableView, RowData, basePasteData, out errorMessages);
            EntitiesUndoRedoManager.UnpauseActionId();
        }

        private bool basePasteData(DataRow newRow, ColumnBase copyColumn, string pasteData, bool isLastRow, out List<ErrorMessage> errorMessages)
        {
            errorMessages = new List<ErrorMessage>();
            if (copyColumn.FieldType == typeof(decimal))
            {
                var rgx = new Regex(BluePrintsResources.Regex_NumbersOnly);
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
            else if(copyColumn.FieldName.Contains(BindableBase.GetPropertyName(() => new POForecastSnapshotProjection().Comments)))
            {
                if (pasteData != null)
                    findExistingOrAddNewFORECAST_JOB_SETTING(newRow, pasteData);
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
                else if (entityProperty.PropertyName.Contains(BindableBase.GetPropertyName(() => new POForecastSnapshotProjection().Comments)))
                {
                    string oldValueString = entityProperty.OldValue == null ? string.Empty : entityProperty.OldValue.ToString();
                    findExistingOrAddNewFORECAST_JOB_SETTING(entityProperty.ChangedEntity, oldValueString);
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
                else if (entityProperty.PropertyName.Contains(BindableBase.GetPropertyName(() => new POForecastSnapshotProjection().Comments)))
                {
                    string newValueString = entityProperty.NewValue == null ? string.Empty : entityProperty.NewValue.ToString();
                    findExistingOrAddNewFORECAST_JOB_SETTING(entityProperty.ChangedEntity, newValueString);
                }
            }

            GridControlService.RefreshData();
        }
        #endregion

        #region View Properties
        protected ObservableCollection<ColumnDescriptor> columnDescriptors;
        public ObservableCollection<ColumnDescriptor> ColumnDescriptors
        {
            get
            {
                if (columnDescriptors == null)
                {
                    columnDescriptors = new ObservableCollection<ColumnDescriptor>();
                }
                return columnDescriptors;
            }
        }

        protected ObservableCollection<SummaryDescriptor> summaryDescriptors;
        public ObservableCollection<SummaryDescriptor> SummaryDescriptors
        {
            get
            {
                if (summaryDescriptors == null)
                {
                    summaryDescriptors = new ObservableCollection<SummaryDescriptor>();
                }
                return summaryDescriptors;
            }
        }

        protected virtual void updateDataPointsTable()
        {
            GridControlService.BeginDataUpdate();

            //generate aligned dates
            if (alignedDataDateCollection == null || alignedDataDateCollection.Count == 0)
                return;

            //initialize view source
            InitializeColumnSource(ColumnDescriptors, SummaryDescriptors, alignedDataDateCollection);

            //initialize datatable schema
            dataPointsTable = new DataTable();
            dataPointsTable.Columns.Add(columnEntity, typeof(POForecastSnapshotProjection));

            foreach (DateTime alignedDataDate in alignedDataDateCollection)
            {
                string columnFieldName = alignedDataDate.Date.ToString(BluePrintsResources.ColumnDateFormat);
                dataPointsTable.Columns.Add(columnFieldName, typeof(decimal));
            }

            //construction projection from grouped po lines
            List<POSnapshotLine> poLines = getPOLines();
            LoadingScreenManager.ShowLoadingScreen(poLines.Count() + 1);
            LoadingScreenManager.SetMessage("Loading PO Snapshot...");
            List<POForecastSnapshotProjection> projections = new List<POForecastSnapshotProjection>();
            foreach (var poLine in poLines.OrderBy(x => x.PONumber))
            {
                POForecastSnapshotProjection newForecast = ViewModelSource.Create(() => new POForecastSnapshotProjection());
                newForecast.PONO = poLine.PONumber;
                //since it's a group it'll always contain at least a single element
                newForecast.CurrentPOSnapshots = poLine.DataPoints;
                newForecast.VariationCode = poLine.VariationCode;

                //populate comment
                FORECAST_PO_SETTING forecastPOSetting = FORECAST_PO_SETTINGCollection.FirstOrDefault(x => x.PONO == poLine.PONumber && x.VARIATION_CODE == poLine.VariationCode && (x.STOCK_CODE == null || x.STOCK_CODE == string.Empty));
                if (forecastPOSetting != null)
                    newForecast.Comments = forecastPOSetting.PO_COMMENTS;

                IEnumerable<X_PURCHORD_LINE_DETAIL> purchaseOrderDetails = PODetail.Where(x => x.PO_NUMBER.ToString() == poLine.PONumber);
                if(purchaseOrderDetails.Count() > 0)
                {
                    newForecast.Supplier = purchaseOrderDetails.First().SUPPLIER_NAME;
                    newForecast.FirstActualDate = purchaseOrderDetails.Min(x => x.ORDERDATE);
                }

                projections.Add(newForecast);
                LoadingScreenManager.Progress();
            }

            LoadingScreenManager.ResetCurrentProgress();
            LoadingScreenManager.SetMaxProgress(projections.Count());
            LoadingScreenManager.SetMessage("Loading PO Forecast...");
            //gets the forecasted data into dates bucket in the row and adds to datatable
            foreach (POForecastSnapshotProjection projection in projections)
            {
                DataRow newRow = DataPointsTable.NewRow();
                newRow[columnEntity] = projection;
                updateRowPOForecast(alignedDataDateCollection, Entities, CutoffActual_FORECAST_JOB_HOUR_SNAPSHOTCollection, ActualsCutOffDate, projection.PONO, projection.VariationCode, newRow);
                dataPointsTable.Rows.Add(newRow);
                LoadingScreenManager.Progress();
            }

            LoadingScreenManager.CloseLoadingScreen();
            //TableViewService.ScrollToLast();

            GridControlService.EndDataUpdate();
        }

        protected DataTable dataPointsTable = null;
        public virtual DataTable DataPointsTable
        {
            get
            {
                return dataPointsTable;
            }
        }

        protected virtual void InitializeColumnSource(ObservableCollection<ColumnDescriptor> columns, ObservableCollection<SummaryDescriptor> summaries, List<DateTime> alignedDates)
        {
            columns.Clear();
            summaries.Clear();

            columns.Add(new ColumnDescriptor() { FieldName = "Entity.PONO", Header = "PO Number", ReadOnly = true, Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Default });
            summaries.Add(new SummaryDescriptor() { FieldName = "Entity.PONO", DisplayFormat = "{0} Record(s)", Type = SummaryItemType.Count });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.VariationCode", Header = "Variation", ReadOnly = true, Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.Supplier", Header = "Supplier", ReadOnly = true, Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.FirstActualDate", Header = "First Raised", ReadOnly = true, Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Date });
            //columns.Add(new ColumnDescriptor() { FieldName = "Entity.FirstInvoiceDate", Header = "First Invoiced", ReadOnly = true, Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Date });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.PO_TotalPrice", Header = "Total", Mask = "c", ReadOnly = true, Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.PO_Invoiced", Header = "Cut Off Invoiced", Mask = "c", ReadOnly = true, Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.PO_RemainingPrice", Header = "Cut Off Outstanding", Mask = "c", ReadOnly = true, Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number });
            summaries.Add(new SummaryDescriptor() { FieldName = "Entity.PO_RemainingPrice", DisplayFormat = "c", Type = SummaryItemType.Sum });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.TotalForecast", Header = "Forecasted", Mask = "c", ReadOnly = true, Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number });
            summaries.Add(new SummaryDescriptor() { FieldName = "Entity.TotalForecast", DisplayFormat = "c", Type = SummaryItemType.Sum });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.Unforecasted", Header = "Not Forecasted", Mask = "c", ReadOnly = true, Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Unforecasted });
            summaries.Add(new SummaryDescriptor() { FieldName = "Entity.Unforecasted", DisplayFormat = "c", Type = SummaryItemType.Sum });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.Comments", Header = "Comments", ReadOnly = false, Fixed = FixedStyle.Left, Width = 200, Settings = SettingsType.Default });

            foreach (DateTime alignedDate in alignedDates.OrderBy(x => x))
            {
                string columnFieldName = alignedDate.Date.ToString(BluePrintsResources.ColumnDateFormat);
                columns.Add(new ColumnDescriptor() { FieldName = columnFieldName, ReadOnly = false, Header = columnFieldName, Mask = "c0", Increment = 1, Fixed = FixedStyle.None, Width = 60, Settings = SettingsType.ForecastFuture });
                summaries.Add(new SummaryDescriptor() { FieldName = columnFieldName, DisplayFormat = "c0", Type = SummaryItemType.Sum });
            }
        }

        protected virtual void findExistingOrAddNewFORECAST_JOB_SETTING(DataRow updateRow, string comments)
        {
            POForecastSnapshotProjection forecast = ((POForecastSnapshotProjection)updateRow[columnEntity]);    
            FORECAST_PO_SETTING relevantFORECAST_PO_SETTING = FORECAST_PO_SETTINGCollection.FirstOrDefault(x => x.PONO == forecast.PONO && x.VARIATION_CODE == forecast.VariationCode);
            if (relevantFORECAST_PO_SETTING == null)
            {
                FORECAST_PO_SETTING newFORECAST_PO_SETTING = new FORECAST_PO_SETTING();
                newFORECAST_PO_SETTING.GUID_PROJECT = loadPROJECT.GUID;
                newFORECAST_PO_SETTING.PONO = forecast.PONO;
                newFORECAST_PO_SETTING.VARIATION_CODE = forecast.VariationCode;
                newFORECAST_PO_SETTING.STOCK_CODE = string.Empty;

                if (forecast.VariationCode != null && forecast.VariationCode != string.Empty)
                    newFORECAST_PO_SETTING.VARIATION_CODE = forecast.VariationCode;
                else
                    newFORECAST_PO_SETTING.VARIATION_CODE = string.Empty;

                relevantFORECAST_PO_SETTING = newFORECAST_PO_SETTING;
            }

            relevantFORECAST_PO_SETTING.PO_COMMENTS = comments;
            FORECAST_PO_SETTINGCollectionViewModel.Save(relevantFORECAST_PO_SETTING);
            forecast.Comments = comments;
        }

        private bool generateAlignedDataDates()
        {
            if (MainViewModel == null || ForecastStartDate == null)
                return false;

            //since displayentities comes from mainviewmodel it should be populated by now
            DateTime latestDate = Entities.Count == 0 ? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).AddDays(-1) : Entities.Max(x => x.FORECAST_DATE);
            if (latestDate > ForecastEndDate)
                ForecastEndDate = latestDate;

            DateTime earliestDateBeginningOfMonth = new DateTime(((DateTime)ForecastStartDate).Year, ((DateTime)ForecastStartDate).Month, 1);
            alignedDataDateCollection = ChronologicalHelpers.GenerateEndDatesCollection(earliestDateBeginningOfMonth, ForecastEndDate);

            return true;
        }

        private POSnapshotLine getPOLine(string poNo)
        {
            return getPOLines().FirstOrDefault(x => x.PONumber == poNo);
        }

        private List<POSnapshotLine> getPOLines()
        {
            if (CurrentPO_FORECAST_JOB_HOUR_SNAPSHOTCollection == null)
                return new List<POSnapshotLine>();

            return CurrentPO_FORECAST_JOB_HOUR_SNAPSHOTCollection.GroupBy(x => new { x.PO_NUMBER, x.VARIATION_CODE }).Select(group => new POSnapshotLine { PONumber = group.Key.PO_NUMBER, VariationCode = group.Key.VARIATION_CODE, DataPoints = group.ToList() }).ToList();
        }

        public bool CanSaveSnapshot()
        {
            return !IsLoading && LoadDataDate != null;
        }

        public bool CanLoadSnapshot()
        {
            return !IsLoading && LoadDataDate != null;
        }

        public void LoadSnapshot()
        {
            Common.LoadingScreenManager.ShowLoadingScreen(1);
            Common.LoadingScreenManager.SetMessage("Load PO and Actual Snapshot...");
            BluePrintsContextHelper.RefreshForecastActualsByProject(loadPROJECT.NUMBER, (DateTime)LoadDataDate);
            BluePrintsContextHelper.RefreshPOByProject(loadPROJECT.NUMBER, (DateTime)LoadDataDate);
            Common.LoadingScreenManager.CloseLoadingScreen();

            resetIsLoading();
            FullRefresh();
        }


        public void RefreshAllForecastData()
        {
            IsLoading = true;
            Common.LoadingScreenManager.ShowLoadingScreen(1);
            //Common.LoadingScreenManager.SetMessage("Fetching P6 remaining data...");
            //await BluePrintsContextHelper.RefreshDeliverablesRemainingDataPointsByProject(LoadPROJECT.NUMBER, true);

            //Common.LoadingScreenManager.SetMessage("Fetching P6 planned data...");
            //await BluePrintsContextHelper.RefreshDeliverablesPlannedDataPointsByProject(LoadPROJECT.NUMBER, true);

            Common.LoadingScreenManager.SetMessage("Updating actuals, indirect, P6 and PO data...");
            BluePrintsContextHelper.RefreshAllForecastData(loadPROJECT.NUMBER, (DateTime)LoadDataDate);
            Common.LoadingScreenManager.CloseLoadingScreen();

            resetIsLoading();
            FullRefresh();
        }

        protected bool shouldPromptForSavingSnapshot;
        public void SaveSnapshot()
        {
            Common.LoadingScreenManager.ShowLoadingScreen(1);
            Common.LoadingScreenManager.SetMessage("Saving PO Outstanding Snapshot...");
            BluePrintsContextHelper.SavePOForecastData(loadPROJECT.NUMBER, (DateTime)LoadDataDate);
            Common.LoadingScreenManager.CloseLoadingScreen();

            shouldPromptForSavingSnapshot = false;
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
                    e.Column.FilterPopupMode = FilterPopupMode.Excel;
                    e.Column.ReadOnly = false;
                    e.Column.FixedWidth = true;
                    e.Column.Width = 60;
                }
            }
        }

        public void DatesCellValueChanged(CellValueChangedEventArgs e)
        {
            DateTime parseDateTime;
            if(DateTime.TryParse(e.Column.FieldName, out parseDateTime) && e.Value != null)
            {
                decimal newValue = (decimal)e.Value;
                DataRowView dataRowView = (DataRowView)e.Row;
                findExistingOrAddNewFORECAST_PO(dataRowView.Row, parseDateTime, newValue, true);

                updateRowPOForecast(alignedDataDateCollection, Entities, CutoffActual_FORECAST_JOB_HOUR_SNAPSHOTCollection, ActualsCutOffDate, string.Empty, string.Empty, dataRowView.Row);
                addUndo(dataRowView.Row, e.Column.FieldName, e.OldValue, newValue, EntityMessageType.Changed);
            }
            else if (e.Column.FieldName.Contains(BindableBase.GetPropertyName(() => new POForecastSnapshotProjection().Comments)))
            {
                DataRowView dataRowView = (DataRowView)e.Row;
                string commeentsValue = e.Value == null ? string.Empty : e.Value.ToString();
                findExistingOrAddNewFORECAST_JOB_SETTING(dataRowView.Row, commeentsValue);
            }
        }

        protected virtual void clearPOForecast(string poNo, string stockCode, string variationCode)
        {
            List<FORECAST_PO> removePOForecasts = Entities.Where(x => x.PONO == poNo && x.VARIATION_CODE == variationCode).ToList();
            MainViewModel.BaseBulkDelete(removePOForecasts);
            shouldPromptForSavingSnapshot = true;
        }

        protected virtual void findExistingOrAddNewFORECAST_PO(DataRow dataRow, DateTime forecastDate, decimal? viewCosts, bool skipUpdating = false)
        {
            POForecastSnapshotProjection entity = (POForecastSnapshotProjection)dataRow[columnEntity];

            //each PO have multiple items, so we need to store the pro-rated value per PO items in the database
            decimal proRateOnPOItem = 1;
            if (entity.PO_RemainingPrice > 0)
                proRateOnPOItem = (decimal)viewCosts / entity.PO_RemainingPrice;

            var groupByCodesPOItems = entity.CurrentPOSnapshots.GroupBy(g => new { PONumber = g.PO_NUMBER, JobCode = g.SUBJOB_CODE, DisciplineCode = g.DISCIPLINE_CODE, CommodityCode = g.COMMODITY_CODE, StockCode = g.STOCK_CODE, VariationCode = g.VARIATION_CODE }).Select(g => new { g.Key.PONumber, g.Key.JobCode, g.Key.DisciplineCode, g.Key.CommodityCode, g.Key.StockCode, g.Key.VariationCode, RemainingCosts = g.Sum(x => x.FORECAST_COST) }).ToList();
            decimal cumulativeTrueProRateValue = 0;

            for(int i = 0;i < groupByCodesPOItems.Count;i++)
            {
                var groupByCodesPOItem = groupByCodesPOItems[i];
                FORECAST_PO findFORECAST_PO = Entities.FirstOrDefault(x => x.FORECAST_DATE == forecastDate.Date && x.PONO == groupByCodesPOItem.PONumber && x.COMMODITY_CODE == groupByCodesPOItem.CommodityCode && x.DISCIPLINE_CODE == groupByCodesPOItem.DisciplineCode && x.STOCK_CODE == groupByCodesPOItem.StockCode && x.VARIATION_CODE == groupByCodesPOItem.VariationCode && x.JOB_CODE == groupByCodesPOItem.JobCode);

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
                findFORECAST_PO.STOCK_CODE = groupByCodesPOItem.StockCode == null ? "" : groupByCodesPOItem.StockCode;
                findFORECAST_PO.VARIATION_CODE = groupByCodesPOItem.VariationCode;
                findFORECAST_PO.FORECAST_DATE = new DateTime(forecastDate.Year, forecastDate.Month, forecastDate.Day);
                if (viewCosts == null || ((decimal)viewCosts) == 0.00m)
                    findFORECAST_PO.FORECAST_VALUE = null;
                else
                {
                    decimal trueProRateValue = groupByCodesPOItem.RemainingCosts * proRateOnPOItem;
                    decimal viewCostDecimal = (decimal)viewCosts;
                    cumulativeTrueProRateValue += trueProRateValue;
                    //when it's the last item but the figures doesn't match what user's has keyed in
                    if (i == groupByCodesPOItems.Count - 1)
                    {
                        if (cumulativeTrueProRateValue < viewCostDecimal)
                            trueProRateValue += (viewCostDecimal - cumulativeTrueProRateValue);
                    }

                    findFORECAST_PO.FORECAST_VALUE = trueProRateValue;
                }

                MainViewModel.Save(findFORECAST_PO);
                shouldPromptForSavingSnapshot = true;
            }

            if(!skipUpdating)
                updateRowPOForecast(alignedDataDateCollection, Entities, CutoffActual_FORECAST_JOB_HOUR_SNAPSHOTCollection, ActualsCutOffDate, string.Empty, string.Empty, dataRow);
        }

        public bool CanShowCustomPaymentDialog
        {
            get
            {
                return SelectedDataRow != null;
            }
        }

        public bool CanPaymentSpread(object parameter)
        {
            return !IsLoading;
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
            try
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

                    LoadingScreenManager.ShowLoadingScreen(numberOfSelectedRows);
                    for (int rowOffset = 0; rowOffset < rowOffsetSelection; rowOffset++)
                    {
                        LoadingScreenManager.Progress();
                        int current_row_visible_index = first_row_visible_index + rowOffset;
                        int current_row_handle = gridControl.GetRowHandleByVisibleIndex(current_row_visible_index);
                        object rowObject = gridControl.GetRow(current_row_handle);
                        if (rowObject == null)
                            continue;

                        DataRowView editing_row_view = (DataRowView)rowObject;
                        DataRow editing_row = editing_row_view.Row;
                        POForecastSnapshotProjection projection = (POForecastSnapshotProjection)editing_row[columnEntity];

                        string stockCode = string.Empty;
                        if (projection.GetType() == typeof(POFlatForecastSnapshotProjection))
                            stockCode = ((POFlatForecastSnapshotProjection)projection).StockCode;

                        clearPOForecast(projection.PONO, stockCode, projection.VariationCode);
                        decimal costPerPeriod = projection.PO_RemainingPrice / (decimal)spreadPeriod;

                        //decimal remainingPrice = projection.PO_RemainingPrice < 0 ? 0 : projection.PO_RemainingPrice;
                        //decimal costPerPeriod = remainingPrice / (decimal)spreadPeriod;
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
                                if (dataPointsTable.Columns.Contains(parseFieldName))
                                    oldValue = editing_row[parseFieldName];

                                addUndo(editing_row, parseFieldName, oldValue, costPerPeriod, EntityMessageType.Changed);
                                findExistingOrAddNewFORECAST_PO(editing_row, parseDateTime, costPerPeriod, true);
                                lastProcessedDate = parseDateTime;
                            }
                        }

                        if (!forceRefreshDataTable)
                        {
                            updateRowPOForecast(alignedDataDateCollection, Entities, CutoffActual_FORECAST_JOB_HOUR_SNAPSHOTCollection, ActualsCutOffDate, string.Empty, string.Empty, editing_row);

                            //because grid doesn't refresh totals
                            GridControlService.RefreshData();
                        }

                        pasteValueRowOffset += 1;
                    }
                    LoadingScreenManager.CloseLoadingScreen();

                    if (forceRefreshDataTable)
                        refreshDataTable();

                    EntitiesUndoRedoManager.UnpauseActionId();
                }
            }
            catch
            {
                MessageBoxService.ShowMessage("Exception: Please contact " + BluePrintsResources.ITEmail + " to troubleshoot", "Error", MessageButton.OK);
            }
        }

        private void updateRowPOForecast(List<DateTime> alignedDates, IEnumerable<FORECAST_PO> FORECAST_POCollection, IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> cutOffActuals, DateTime cutOffDate, string POno = "", string variationCode = "", DataRow PORow = null)
        {
            if(PORow == null && POno != string.Empty)
                PORow = findPORow(POno, variationCode);

            if (PORow != null)
            {
                POForecastSnapshotProjection forecast = (POForecastSnapshotProjection)PORow[columnEntity];
                forecast.UpdateForecastPayments(FORECAST_POCollection, cutOffActuals, cutOffDate);

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

        public bool CanAlignPOsWithActuals()
        {
            return !IsLoading;
        }

        public virtual void AlignPOsWithActuals()
        {
            EntitiesUndoRedoManager.Clear();
            IEnumerable<POForecastSnapshotProjection> projections = from DataRow dr in dataPointsTable.Rows
                                                            select (POForecastSnapshotProjection)dr[columnEntity];

            List<FORECAST_PO> saveFORECAST_POs = new List<FORECAST_PO>();
            //fix codes mis-alignment
            LoadingScreenManager.ShowLoadingScreen(projections.Count());
            LoadingScreenManager.SetMessage("Aligning Actuals...");
            //fix dates mis-alignment
            foreach (POForecastSnapshotProjection projection in projections)
            {
                LoadingScreenManager.Progress();
                DataRow editing_row = findPORow(projection.PONO, projection.VariationCode);
                decimal totalForecastValue = projection.FORECAST_POs.Where(x => x.FORECAST_VALUE != null).Sum(x => (decimal)x.FORECAST_VALUE);
                if(totalForecastValue == 0)
                {
                    if(editing_row != null)
                    {
                        findExistingOrAddNewFORECAST_PO(editing_row, (DateTime)ForecastStartDate, projection.PO_RemainingPrice);
                    }
                }
                else
                {
                    foreach (FORECAST_PO FORECAST_PO in projection.FORECAST_POs.OrderBy(x => x.FORECAST_DATE))
                    {
                        //need to pro-rate costs by WBS
                        decimal wbsRemainingCosts = projection.CurrentPOSnapshots.Where(x => x.SUBJOB_CODE == FORECAST_PO.JOB_CODE && x.DISCIPLINE_CODE == FORECAST_PO.DISCIPLINE_CODE && x.COMMODITY_CODE == FORECAST_PO.COMMODITY_CODE && x.STOCK_CODE == FORECAST_PO.STOCK_CODE).Sum(x => x.FORECAST_COST);
                        //forecast POs already filtered by variation code
                        decimal wbsForecastCosts = projection.FORECAST_POs.Where(x => x.JOB_CODE == FORECAST_PO.JOB_CODE && x.DISCIPLINE_CODE == FORECAST_PO.DISCIPLINE_CODE && x.COMMODITY_CODE == FORECAST_PO.COMMODITY_CODE && x.STOCK_CODE == FORECAST_PO.STOCK_CODE).Where(x => x.FORECAST_DATE.Date > ActualsCutOffDate.Date && x.FORECAST_VALUE != null).Sum(x => (decimal)x.FORECAST_VALUE);
                        decimal wbsCostDifference = wbsRemainingCosts - wbsForecastCosts;

                        if (FORECAST_PO.FORECAST_DATE.Date <= ActualsCutOffDate.Date)
                        {
                            //store as 0 so that when we rewind and adjust actuals again this point will actually be used
                            FORECAST_PO.FORECAST_VALUE = 0.00m;
                            saveFORECAST_POs.Add(FORECAST_PO);

                            //when the previous date is adjusted as 0 and no existing record to move unforecasted amount anymore, default to adding forecast amount to forecast start date
                            if(projection.FORECAST_POs.Where(x => x.FORECAST_VALUE != null).Sum(x => x.FORECAST_VALUE) == 0)
                            {
                                findExistingOrAddNewFORECAST_PO(editing_row, (DateTime)ForecastStartDate, projection.PO_RemainingPrice);
                                //no point to continue since the rest will be zero
                                break;
                            }

                            continue;
                        }
                        
                        //cost adjustment
                        if (wbsCostDifference > 0)
                        {
                            FORECAST_PO.FORECAST_VALUE += wbsCostDifference;
                            saveFORECAST_POs.Add(FORECAST_PO);
                        }
                        else if (wbsCostDifference < 0)
                        {
                            decimal forecastValue = FORECAST_PO.FORECAST_VALUE == null ? 0 : (decimal)FORECAST_PO.FORECAST_VALUE;
                            decimal postAdjustmentCosts = forecastValue + wbsCostDifference;
                            FORECAST_PO.FORECAST_VALUE += wbsCostDifference;
                            saveFORECAST_POs.Add(FORECAST_PO);
                        }
                    }
                }
            }

            LoadingScreenManager.CloseLoadingScreen();
            LastAlignedDate = DateTime.Now;
            MainViewModel.BaseBulkSave(saveFORECAST_POs);
            SaveSnapshot();
            shouldPromptForSavingSnapshot = false;
            refreshDataTable();
        }

        protected void refreshDataTable()
        {
            generateAlignedDataDates();
            dataPointsTable = null;
            updateDataPointsTable();
            this.RaisePropertyChanged(x => x.DataPointsTable);
        }

        private DataRow findPORow(string PONumber, string variationCode)
        {
            return (from DataRow dr in dataPointsTable.Rows
                    where ((POForecastSnapshotProjection)dr[columnEntity]).PONO == PONumber && ((POForecastSnapshotProjection)dr[columnEntity]).VariationCode == variationCode
                    select dr).FirstOrDefault();
        }
        
        public override void FullRefresh()
        {
            if (!CanFullRefresh())
                return;

            EntitiesUndoRedoManager.Clear();
            dataPointsTable = null;
            //loadExoData();
            IsLoading = true;
            this.RaisePropertyChanged(x => x.IsLoading);
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
                    return new DateTime(forecastStartDate.Year, forecastStartDate.Month, 1).AddSeconds(-1);
                }
            }
        }

        public DateTime? ForecastStartDate { get; set; }
        public DateTime ForecastEndDate { get; set; }

        public bool CanSaveDateAndRefresh()
        {
            return !IsLoading;
        }

        public void SaveDateAndRefresh()
        {
            DateTime? changedDate = ForecastStartDate;
            BluePrintsDataUtils.SaveDateAndRefresh(loadPROJECT, LoadDataDate, ref changedDate, ForecastEndDate, FORECAST_EACCollection, PROJECTCollectionViewModel, MessageBoxService);

            EntitiesUndoRedoManager.Clear();
            refreshDataTable();

            ForecastStartDate = changedDate;
            this.RaisePropertyChanged(x => x.ForecastStartDate);
            this.RaisePropertyChanged(x => x.ForecastEndDate);
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
                if (!IsLoading)
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

        public IListSource ActualsDetail => instantFeedbackActualDetailViewModel.InstantFeedbackEntities;
        public List<X_PURCHORD_LINE_DETAIL> PODetail => X_PURCHORD_LINE_DETAILS;
        public Visibility ActualDetailsVisibility => !IsPoDetailsVisible ? Visibility.Visible : Visibility.Collapsed;
        public Visibility PODetailsVisibility => IsPoDetailsVisible ? Visibility.Visible : Visibility.Collapsed;
        public bool IsPoDetailsVisible { get; set; }
        public bool IsHidden { get; set; }
        public int DateSortIndex => 1;
        private void setFilter(DataRowView dataRowView, GridColumn gridColumn)
        {
            if (gridColumn == null || SelectedDataRows == null || SelectedDataRows.Count == 0)
                return;

            if (gridColumn.FieldName.ToUpper().Contains("PO_REMAININGPRICE"))
            {
                POForecastSnapshotProjection entity = (POForecastSnapshotProjection)dataRowView[columnEntity];
                POFilterCriteria = CriteriaOperator.Parse("[PO_NUMBER] = '" + entity.PONO + "' AND [VARIATION_CODE] = '" + entity.VariationCode + "'"); 
                IsHidden = false;
                IsPoDetailsVisible = true;

                this.RaisePropertyChanged(x => x.PODetail);
                this.RaisePropertyChanged(x => x.POFilterCriteria);
            }
            else if (gridColumn.FieldName.ToUpper().Contains("PO_TOTALPRICE"))
            {
                POForecastSnapshotProjection entity = (POForecastSnapshotProjection)dataRowView[columnEntity];
                POFilterCriteria = CriteriaOperator.Parse("[PO_NUMBER] = '" + entity.PONO + "' AND [VARIATION_CODE] = '" + entity.VariationCode + "'");
                IsHidden = false;
                IsPoDetailsVisible = true;

                this.RaisePropertyChanged(x => x.PODetail);
                this.RaisePropertyChanged(x => x.POFilterCriteria);
            }
            else if (gridColumn.FieldName.ToUpper().Contains("PO_INVOICED"))
            {
                POForecastSnapshotProjection entity = (POForecastSnapshotProjection)dataRowView[columnEntity];
                ActualFilterCriteria = CriteriaOperator.Parse("[PO_NUMBER] = " + entity.PONO + " AND [VARIATION_CODE] = '" + entity.VariationCode + "' AND [TRANSDATE] <= #" + ActualsCutOffDate.Year + "-" + ActualsCutOffDate.Month + "-" + ActualsCutOffDate.Day + "#");
                IsHidden = false;
                IsPoDetailsVisible = false;

                this.RaisePropertyChanged(x => x.ActualsDetail);
                this.RaisePropertyChanged(x => x.ActualFilterCriteria);
            }

            this.RaisePropertyChanged(x => x.ActualDetailsVisibility);
            this.RaisePropertyChanged(x => x.PODetailsVisibility);
            this.RaisePropertyChanged(x => x.DateSortIndex);
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

            //workaround for when detail grid doesn't show anything when it's first loaded, bug on devexpress
            ActualFilterCriteria = CriteriaOperator.Parse("[SUB_JOBCODE] = 'x'");
            POFilterCriteria = CriteriaOperator.Parse("[SUB_JOBCODE] = 'x'");
            this.RaisePropertyChanged(x => x.ActualFilterCriteria);
            this.RaisePropertyChanged(x => x.POFilterCriteria);

            ActualFilterCriteria = CriteriaOperator.Parse("");
            POFilterCriteria = CriteriaOperator.Parse("");
            this.RaisePropertyChanged(x => x.IsHidden);
            this.RaisePropertyChanged(x => x.ActualFilterCriteria);
            this.RaisePropertyChanged(x => x.POFilterCriteria);
            this.RaisePropertyChanged(x => x.ActualsDetail);
            this.RaisePropertyChanged(x => x.PODetail);
        }

        public override bool CanExportToExcel()
        {
            return !IsLoading;
        }

        public override bool CanExportToPDF()
        {
            return !IsLoading;
        }

        public override bool CanResetLayout()
        {
            if (IsLoading)
                return false;

            return base.CanResetLayout();
        }

        public override bool CanSaveLayout()
        {
            return !IsLoading;
        }

        public override bool CanKeyboardCopy()
        {
            return !IsLoading;
        }

        public override bool CanKeyboardPaste()
        {
            return !IsLoading;
        }

        public override bool CanUndo()
        {
            if (EntitiesUndoRedoManager == null || IsLoading)
                return false;

            return EntitiesUndoRedoManager.CanUndo();
        }

        public override bool CanRedo()
        {
            if (EntitiesUndoRedoManager == null || IsLoading)
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

        [ServiceProperty(Key = "DetailTableViewService")]
        protected virtual ITableViewService DetailTableViewService { get { return null; } }
        public void ExportDetailToExcel()
        {
            string ResultPath = string.Empty;
            if (FolderBrowserDialogService.ShowDialog())
            {
                ResultPath = FolderBrowserDialogService.ResultPath;
                bool result = DetailTableViewService.ExportToXls(ResultPath + "\\" + loadPROJECT.NUMBER + "_PO_Detail.xlsx", isExcelExportDataAware);

                if (!result)
                    MessageBoxService.ShowMessage("Export failed because the file is in use", "Warning", MessageButton.OK, MessageIcon.Warning);
            }
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

        public override void KeyboardCopy()
        {
            System.Windows.Forms.SendKeys.SendWait("^c");
        }

        public override void KeyboardPaste()
        {
            System.Windows.Forms.SendKeys.SendWait("^v");
        }

        protected IDialogService CustomPODialogService
        {
            get { return this.GetRequiredService<IDialogService>("CustomPODialogService"); }
        }

        public IEnumerable<FORECAST> FORECASTCollection
        {
            get
            {
                return GetEntities<FORECAST>();
            }
        }

        public IEnumerable<FORECAST_EAC> FORECAST_EACCollection
        {
            get
            {
                return GetEntities<FORECAST_EAC>();
            }
        }

        public IEnumerable<FORECAST_PO_SETTING> FORECAST_PO_SETTINGCollection
        {
            get
            {
                return GetEntities<FORECAST_PO_SETTING>();
            }
        }

        public CollectionViewModel<FORECAST_PO_SETTING, FORECAST_PO_SETTING, Guid, IBluePrintsEntitiesUnitOfWork> FORECAST_PO_SETTINGCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<FORECAST_PO_SETTING, FORECAST_PO_SETTING, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<FORECAST_PO_SETTING>();
            }
        }
        public IEnumerable<JOB_COSTGROUPS> JOB_COSTGROUPSCollection
        {
            get
            {
                var collection = GetEntities<JOB_COSTGROUPS>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.SHORTCODE);
                return collection;
            }
        }

        public IEnumerable<DISCIPLINE_DESC> DISCIPLINE_DESCCollection
        {
            get
            {
                var collection = GetEntities<DISCIPLINE_DESC>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        public IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> CurrentPO_FORECAST_JOB_HOUR_SNAPSHOTCollection
        {
            get
            {
                var collection = GetEntities<FORECAST_JOB_HOUR_SNAPSHOT>();
                if (collection != null)
                    collection = collection.Where(x => x.SNAPSHOT_TYPE == ForecastSnapshotValueType.CurrentOutstandingPO);

                return collection;
            }
        }

        public IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> CutoffActual_FORECAST_JOB_HOUR_SNAPSHOTCollection
        {
            get
            {
                var collection = GetEntities<FORECAST_JOB_HOUR_SNAPSHOT>();
                if (collection != null)
                    collection = collection.Where(x => x.SNAPSHOT_TYPE == ForecastSnapshotValueType.Actual).Where(x => x.FORECAST_DATE <= ActualsCutOffDate);

                return collection;
            }
        }

        public CollectionViewModel<DISCIPLINE_DESC, DISCIPLINE_DESC, Guid, IBluePrintsEntitiesUnitOfWork> DISCIPLINE_DESCCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<DISCIPLINE_DESC, DISCIPLINE_DESC, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<DISCIPLINE_DESC>();
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

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "PROJECTPOSnapshotForecastViewModelWrapper_v2"; }
        }
        #endregion
    }

    public class POFlatSnapshotLine : POSnapshotLine
    {
        public string StockCode { get; set; }
    }

    public class POSnapshotLine
    {
        public string PONumber { get; set; }
        public string VariationCode { get; set; }
        public List<FORECAST_JOB_HOUR_SNAPSHOT> DataPoints { get; set; }
    }
}