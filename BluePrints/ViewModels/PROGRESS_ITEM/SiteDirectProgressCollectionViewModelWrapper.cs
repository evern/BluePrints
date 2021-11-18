using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Filtering;
using BluePrints.Common.Helpers;
using BluePrints.Common.Projections;
using BluePrints.Common.Reports;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Misc;
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
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
        private ScoreCardDiscipline scoreCardDiscipline;
        private DispatcherTimer gridRefreshDispatcherTimer;
        protected override void resolveParameters(object parameter)
        {
            skipExoDataLoading = true;
            is_load_p6_task = true;
            isUseReportDate = LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Permission_DesignDeliverables_ProgressPreviousWeeksDate)) != LoginCredentials.PermissionStatus.None;
            canDateBackwardForward = LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Permission_DesignDeliverables_CanDateBackwardForward)) != LoginCredentials.PermissionStatus.None;

            delayedPROGRESSSavingDispatcher = new DispatcherTimer();
            delayedPROGRESSSavingDispatcher.Interval = new TimeSpan(0, 0, 0, 1);
            delayedPROGRESSSavingDispatcher.Tick += delayedPROGRESSSavingDispatcher_Tick;
            var receiveParameter = (TripleEntitiesParameter<Data.PROJECT, PROGRESS, object>)parameter;
            loadPROJECT = receiveParameter.GetFirstEntity();
            loadPROGRESS = receiveParameter.GetSecondEntity();
            scoreCardDiscipline = (ScoreCardDiscipline)receiveParameter.GetThirdEntity();

            primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(BluePrintsResources.OfficeMontreal).CreateUnitOfWork();

            gridRefreshDispatcherTimer = new DispatcherTimer();
            gridRefreshDispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);

            if (loadPROJECT != null)
                isQueryForLiveStatus = true;
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
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.CONSTRUCTION_STAGES, CONSTRUCTION_STAGEProjectionFunc);

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

        private Func<IRepositoryQuery<CONSTRUCTION_STAGE>, IQueryable<CONSTRUCTION_STAGE>> CONSTRUCTION_STAGEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.SCORE_CARD_DISCIPLINE == scoreCardDiscipline);
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
        
        protected override Func<IRepositoryQuery<ESTIMATE_ITEM>, IQueryable<ESTIMATE_ITEMProgress>> specifyMainViewModelProjection()
        {
            return query => ESTIMATE_ITEMProjectionQueries.IDeliverable_Progress_Transformation(query.Where(x => x.ESTIMATE.GUID_PROJECT == loadPROJECT.GUID && x.ESTIMATE.STATUS == BaselineStatus.Live).Where(x => x.DISCIPLINE.SCORE_CARD_DISCIPLINE == scoreCardDiscipline), loadPROJECT, loaderCollection.GetCollection<RATE>(), loadPROGRESS, PROGRESS_ITEMCollection, false, null, false, P6_ASSIGNMENTCollection, false, COMMODITY_CODECollection, false);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<ESTIMATE_ITEMProgress> entities)
        {
            MainViewModel.DisableEntitiesPauseUnpause = true;
            PROGRESS_ITEMSCollectionViewModel.DisableEntitiesPauseUnpause = true;
            MainViewModel.ValidateFillDownCallBack = ValidateFillDownCallBack;
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            loadDataPointsTable();
            skipExoDataLoading = true;
            base.OnAfterAssignedCallbackAndRaisePropertyChanged();
        }

        public override void OnAfterAuxiliaryEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender, Guid senderKey, bool isBulkRefresh)
        {
            if (changedType == typeof(PROGRESS_ITEM))
            {
                PROGRESS_ITEM progressItem = PROGRESS_ITEMCollection.FirstOrDefault(x => x.GUID.ToString() == key.ToString());
                if(progressItem != null)
                {
                    ESTIMATE_ITEMProgress deliverable = Entities.FirstOrDefault(x => x.OriginalEntityKey.ToString() == progressItem.GUID_ORIBASEITEM.ToString());
                    if (deliverable != null)
                    {
                        IEnumerable<PROGRESS_ITEM> progressItems = PROGRESS_ITEMCollection.Where(x => x.GUID_ORIBASEITEM == progressItem.GUID_ORIBASEITEM);
                        deliverable.SetProgressItems(progressItems.ToList());
                        List<StatsCalculationType> calcTypes = new List<StatsCalculationType>();
                        calcTypes.Add(StatsCalculationType.Earned);
                        deliverable.BuildStats(1, calcTypes);
                        populateRow(deliverable, true);
                        deliverable.Update();

                        gridRefreshDispatcherTimer.Tick -= gridRefreshDispatcherTimer_Tick;
                        gridRefreshDispatcherTimer.Tick += gridRefreshDispatcherTimer_Tick;
                        gridRefreshDispatcherTimer.Start();
                    }
                }

            }

            base.OnAfterAuxiliaryEntitiesChanged(key, changedType, messageType, sender, senderKey, isBulkRefresh);
        }

        private void gridRefreshDispatcherTimer_Tick(object sender, EventArgs e)
        {
            gridRefreshDispatcherTimer.Stop();
            GridControlService.RefreshData();   
        }

        public override void PastingFromClipboard(PastingFromClipboardEventArgs e)
        {
            GridControl gridControl = (GridControl)e.Source;
            TableView gridTableView = (TableView)gridControl.View;
            string newValueString = Clipboard.GetText().ToString();

            List<ErrorMessage> errorMessages = new List<ErrorMessage>();
            //remove tab in front
            if (newValueString != string.Empty)
            {
                if (newValueString.Substring(0, 1) == "\t")
                {
                    newValueString = newValueString.Substring(1, newValueString.Length - 1);
                }

                string[] RowData = DataUtils.ExcelSplit(newValueString).ToArray();
                pasteCellData(gridControl, gridTableView, RowData, out errorMessages);

                e.Handled = true;
            }

            ShowErrorMessage("Errors", errorMessages);
        }

        private void pasteCellData(GridControl gridControl, TableView gridTableView, string[] RowData, out List<ErrorMessage> errorMessages)
        {
            undoRedoClear();
            undoRedoPause();
            List<DataRow> editedRows = GridControlHelpers.PasteCellData(gridControl, gridTableView, RowData, basePasteData, out errorMessages, true);
            LoadingScreenManager.CloseLoadingScreen();
            undoRedoUnpause();
        }

        private bool basePasteData(DataRow newRow, ColumnBase copyColumn, string pasteData, bool isLastRow, out List<ErrorMessage> errorMessages)
        {
            errorMessages = new List<ErrorMessage>();
            if (copyColumn.FieldType == typeof(decimal))
            {
                ESTIMATE_ITEMProgress entity = (ESTIMATE_ITEMProgress)newRow[columnEntity];
                var rgx = new Regex(BluePrintsResources.Regex_NumbersOnly);
                var cleanColumnString = rgx.Replace(pasteData, string.Empty);
                decimal decimal_value;
                if (decimal.TryParse(cleanColumnString, out decimal_value))
                {
                    decimal oldValue = (decimal)newRow[copyColumn.FieldName];
                    ErrorMessage errorMessage;
                    updatePercentage(entity, copyColumn.FieldName, oldValue, decimal_value, out errorMessage);
                    if (errorMessage != null)
                    {
                        errorMessages.Add(errorMessage);
                        return false;
                    }
                }
            }

            return true;
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

        #region Grid Dependencies
        DataTable dataPointsTable = null;
        public DataTable DataPointsTable
        {
            get
            {
                return dataPointsTable;
            }
        }

        DataRowView selectedDataRow { get; set; }
        public DataRowView SelectedDataRow
        {
            get
            {
                return selectedDataRow;
            }
            set
            {
                if (dataPointsTable == null || value == null)
                    return;

                int rowIndex = dataPointsTable.Rows.IndexOf(value.Row);
                if (rowIndex == -1)
                    return;

                DataRowView dataRowView = dataPointsTable.DefaultView[rowIndex];
                selectedDataRow = dataRowView;
            }
        }

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

        private string stageFieldNamePercentageAffix = "Percentage";
        private string stageFieldNameQuantityAffix = "Quantity";
        private string currentPeriodPercentage = "CurrentPeriodPercentage";
        private string cumulativePercentage = "TotalPercentage";
        private string cumulativeEarnedUnits = "TotalUnits";
        private string currentPeriodEarnedUnits = "CurrentPeriodUnits";
        private void InitializeColumnSource(ObservableCollection<ColumnDescriptor> columns, ObservableCollection<SummaryDescriptor> summaries)
        {
            columns.Clear();
            summaries.Clear();

            columns.Add(new ColumnDescriptor() { FieldName = columnEntity + ".Entity.Entity.GUID_PHASE", ReadOnly = true, Header = "Phase", Fixed = FixedStyle.Left, Width = 50, DisplayMember = "INTERNAL_NUM", ValueMember = "GUID", ItemsSource = PHASECollection, Settings = SettingsType.Collection });
            summaries.Add(new SummaryDescriptor() { FieldName = columnEntity + ".Entity.Entity.GUID_PHASE", DisplayFormat = "{0} Record(s)", Type = SummaryItemType.Count });
            columns.Add(new ColumnDescriptor() { FieldName = columnEntity + ".Entity.Entity.GUID_AREA", ReadOnly = true, Header = "Area", Fixed = FixedStyle.Left, Width = 50, DisplayMember = "INTERNAL_NUM", ValueMember = "GUID", ItemsSource = AREACollection, Settings = SettingsType.Collection });
            columns.Add(new ColumnDescriptor() { FieldName = columnEntity + ".Entity.Entity.SubAreaGuid", ReadOnly = true, Header = "Sub-Area", Fixed = FixedStyle.Left, Width = 50, DisplayMember = "INTERNAL_NUM", ValueMember = "GUID", ItemsSource = AREACollection, Settings = SettingsType.Collection });
            columns.Add(new ColumnDescriptor() { FieldName = columnEntity + ".Entity.Entity.GUID_DISCIPLINE", ReadOnly = true, Header = "Discipline", Fixed = FixedStyle.Left, Width = 80, DisplayMember = "CODE", ValueMember = "GUID", ItemsSource = DISCIPLINECollection, Settings = SettingsType.Collection });
            columns.Add(new ColumnDescriptor() { FieldName = columnEntity + ".Entity.Entity.DISCIPLINE_NUM", ReadOnly = true, Mask = "n0", Header = "Discipline Num", Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Number });
            columns.Add(new ColumnDescriptor() { FieldName = columnEntity + ".Entity.Entity.COMMODITY_CODE", ReadOnly = true, Header = "Commodity Code", Fixed = FixedStyle.Left, Width = 80, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = columnEntity + ".Entity.Entity.STOCK_CODE", ReadOnly = true, Header = "Stock Code", Fixed = FixedStyle.Left, Width = 80, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = columnEntity + ".Entity.Entity.VARIATION_CODE", ReadOnly = true, Header = "Variation Code", Fixed = FixedStyle.Left, Width = 100, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = columnEntity + ".Entity.Entity.NAME", ReadOnly = true, Header = "Name", Fixed = FixedStyle.Left, Width = 150, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = columnEntity + ".Entity.Entity.CLIENT_NAME", ReadOnly = true, Header = "Client Name", Fixed = FixedStyle.Left, Width = 150, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = columnEntity + ".Entity.Total_Quantity", ReadOnly = true, Header = "Total Quantity", Fixed = FixedStyle.Left, Width = 75, MinValue = 0, Settings = SettingsType.Number, Mask = "n0" });
            columns.Add(new ColumnDescriptor() { FieldName = columnEntity + ".Entity.UOM", ReadOnly = true, Header = "UOM", Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Default });

            //unbound columns
            columns.Add(new ColumnDescriptor() { FieldName = currentPeriodPercentage, ReadOnly = true, Header = "Period Progress", HeaderToolTip = "Current period progress", Fixed = FixedStyle.Left, Width = 60, Mask = "p2", Settings = SettingsType.Number });
            columns.Add(new ColumnDescriptor() { FieldName = cumulativePercentage, ReadOnly = true, Header = "Progress", HeaderToolTip = "Cumulative progress", Fixed = FixedStyle.Left, Width = 60, Mask = "p2", Settings = SettingsType.Number });
            columns.Add(new ColumnDescriptor() { FieldName = currentPeriodEarnedUnits, ReadOnly = true, Header = "Period Earned Hrs", HeaderToolTip = "Cumulative progress", Fixed = FixedStyle.Left, Width = 60, Mask = "n2", Settings = SettingsType.Number });
            columns.Add(new ColumnDescriptor() { FieldName = cumulativeEarnedUnits, ReadOnly = true, Header = "Total Earned Hrs", HeaderToolTip = "Current period progress", Fixed = FixedStyle.Left, Width = 60, Mask = "n2", Settings = SettingsType.Number });

            foreach (CONSTRUCTION_STAGE CONSTRUCTION_STAGE in CONSTRUCTION_STAGECollection)
            {
                columns.Add(new ColumnDescriptor() { FieldName = CONSTRUCTION_STAGE.SORT_ORDER.ToString() + stageFieldNameQuantityAffix, Mask = "n2", Increment = 1m, Header = CONSTRUCTION_STAGE.NAME + " By Quantity", Fixed = FixedStyle.Right, Width = 50, Settings = SettingsType.Custom1 });
                columns.Add(new ColumnDescriptor() { FieldName = CONSTRUCTION_STAGE.SORT_ORDER.ToString() + stageFieldNamePercentageAffix, Mask = "p2", Increment = 0.1m, Header = CONSTRUCTION_STAGE.NAME + " " + CONSTRUCTION_STAGE.WEIGHT_PERCENTAGE.ToString("P0"), MaxValue = 1, Fixed = FixedStyle.Right, Width = 50, Settings = SettingsType.Number });
            }
        }
        
        protected override bool loadDataPointsTable()
        {
            IsLoading = true;
            this.RaisePropertyChanged(x => x.IsLoading);

            dataPointsTable = null;

            updateDataPointsTable();
            this.RaisePropertyChanged(x => x.DataPointsTable);

            IsLoading = false;
            this.RaisePropertyChanged(x => x.IsLoading);
            CommonMethods.AddSaveLayoutHandler(GridControlService.GetGridColumns());
            return true;
        }

        string columnEntity = "Entity";
        private void updateDataPointsTable()
        {
            GridControlService.BeginDataUpdate();
            dataPointsTable = new DataTable();

            InitializeColumnSource(ColumnDescriptors, SummaryDescriptors);
            dataPointsTable.Columns.Add(columnEntity, typeof(ESTIMATE_ITEMProgress));
            foreach (CONSTRUCTION_STAGE CONSTRUCTION_STAGE in CONSTRUCTION_STAGECollection)
            {
                dataPointsTable.Columns.Add(CONSTRUCTION_STAGE.SORT_ORDER.ToString() + stageFieldNameQuantityAffix, typeof(decimal));
                dataPointsTable.Columns.Add(CONSTRUCTION_STAGE.SORT_ORDER.ToString() + stageFieldNamePercentageAffix, typeof(decimal));

            }

            dataPointsTable.Columns.Add(currentPeriodPercentage, typeof(decimal));
            dataPointsTable.Columns.Add(cumulativePercentage, typeof(decimal));
            dataPointsTable.Columns.Add(currentPeriodEarnedUnits, typeof(decimal));
            dataPointsTable.Columns.Add(cumulativeEarnedUnits, typeof(decimal));

            //when it's disposed
            if(Entities != null)
                foreach (ESTIMATE_ITEMProgress entity in Entities)
                {
                    populateRow(entity, false);
                }

            GridControlService.EndDataUpdate();
        }

        private void populateRow(ESTIMATE_ITEMProgress entity, bool isUpdate)
        {
            if (dataPointsTable == null)
                return;

            DataRow newDataRow;
            if (!isUpdate)
                newDataRow = dataPointsTable.NewRow();
            else
            {
                newDataRow = (from DataRow dr in dataPointsTable.Rows
                              where ((ESTIMATE_ITEMProgress)dr[columnEntity]).GUID == entity.GUID
                              select dr).FirstOrDefault();
            }

            if (newDataRow == null)
                return;

            newDataRow[columnEntity] = entity;

            decimal cumulativeProgress = 0;
            decimal currentPeriodProgress = 0;
            //set progress to zero
            foreach(CONSTRUCTION_STAGE CONSTRUCTION_STAGE in CONSTRUCTION_STAGECollection)
            {
                IEnumerable<PROGRESS_ITEM> cumulativeDeliverableProgressesToDate = PROGRESS_ITEMCollection.Where(x => x.GUID_ORIBASEITEM == entity.Entity.Entity.GUID_ORIGINAL).Where(x => x.STAGE_ORDER == CONSTRUCTION_STAGE.SORT_ORDER && x.EARNED_DATE <= DataDate);
                IEnumerable<PROGRESS_ITEM> currentPeriodDeliverableProgresses = PROGRESS_ITEMCollection.Where(x => x.GUID_ORIBASEITEM == entity.Entity.Entity.GUID_ORIGINAL).Where(x => x.STAGE_ORDER == CONSTRUCTION_STAGE.SORT_ORDER && x.EARNED_DATE == DataDate);
                if (cumulativeDeliverableProgressesToDate.Count() == 0 || entity.Total_Quantity == 0)
                {
                    newDataRow[CONSTRUCTION_STAGE.SORT_ORDER.ToString() + stageFieldNameQuantityAffix] = 0;
                    newDataRow[CONSTRUCTION_STAGE.SORT_ORDER.ToString() + stageFieldNamePercentageAffix] = 0;
                }
                else
                {
                    decimal cumulativeEarnedQuantitiesToDate = cumulativeDeliverableProgressesToDate.Sum(x => x.EARNED_UNITS);
                    decimal stageEarnedPercentage = cumulativeEarnedQuantitiesToDate / entity.Total_Quantity;
                    decimal stageEarnedQuantity = cumulativeEarnedQuantitiesToDate;
                    newDataRow[CONSTRUCTION_STAGE.SORT_ORDER.ToString() + stageFieldNameQuantityAffix] = stageEarnedQuantity;
                    newDataRow[CONSTRUCTION_STAGE.SORT_ORDER.ToString() + stageFieldNamePercentageAffix] = stageEarnedPercentage;
                    decimal cumulativeStageProgress = CONSTRUCTION_STAGE.WEIGHT_PERCENTAGE * stageEarnedPercentage;

                    decimal currentPeriodEarnedQuantity = currentPeriodDeliverableProgresses.Sum(x => x.EARNED_UNITS);
                    currentPeriodProgress += CONSTRUCTION_STAGE.WEIGHT_PERCENTAGE * (currentPeriodEarnedQuantity / entity.Total_Quantity);
                    cumulativeProgress += cumulativeStageProgress;
                }
            }

            newDataRow[cumulativePercentage] = cumulativeProgress;
            newDataRow[currentPeriodPercentage] = currentPeriodProgress;

            entity.resetEarnedUnits();
            newDataRow[currentPeriodEarnedUnits] = entity.Earned_Units_OnDataDate;
            newDataRow[cumulativeEarnedUnits] = entity.Earned_Units_ToDate;

            if (!isUpdate)
                dataPointsTable.Rows.Add(newDataRow);
        }
        #endregion

        #endregion

        #region View Helpers
        private void showErrorMessage(ErrorMessage errorMessage)
        {
            if (errorMessage == null)
                return;

            List<ErrorMessage> errorMessages = new List<ErrorMessage>();
            errorMessages.Add(errorMessage);
            MainViewModel.ShowErrorMessage("Error", errorMessages);
        }

        public override bool CanUndo()
        {
            if (!IsCalculationCompleted)
                return false;

            if (PROGRESS_ITEMSCollectionViewModel == null || MainViewModel == null)
                return false;

            return PROGRESS_ITEMSCollectionViewModel.EntitiesUndoRedoManager.CanUndo() || MainViewModel.EntitiesUndoRedoManager.CanUndo();
        }

        public override bool CanRedo()
        {
            if (!IsCalculationCompleted)
                return false;

            if (PROGRESS_ITEMSCollectionViewModel == null || MainViewModel == null)
                return false;

            return PROGRESS_ITEMSCollectionViewModel.EntitiesUndoRedoManager.CanRedo() || MainViewModel.EntitiesUndoRedoManager.CanRedo();
        }


        public override void Undo()
        {
            PROGRESS_ITEMSCollectionViewModel.EntitiesUndoRedoManager.Undo();

            //mainviewmodel must be undone last so that auxiliary message event can handle progress_item changes
            MainViewModel.EntitiesUndoRedoManager.Undo();
        }

        public override void Redo()
        {
            PROGRESS_ITEMSCollectionViewModel.EntitiesUndoRedoManager.Redo();

            //mainviewmodel must be redone last so that auxiliary message event can handle progress_item changes
            MainViewModel.EntitiesUndoRedoManager.Redo();
        }

        private void undoRedoPause()
        {
            PROGRESS_ITEMSCollectionViewModel.EntitiesUndoRedoManager.PauseActionId();
            MainViewModel.EntitiesUndoRedoManager.PauseActionId();
        }

        private void undoRedoUnpause()
        {
            PROGRESS_ITEMSCollectionViewModel.EntitiesUndoRedoManager.UnpauseActionId();
            MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
        }

        private void undoRedoClear()
        {
            PROGRESS_ITEMSCollectionViewModel.EntitiesUndoRedoManager.Clear();
            MainViewModel.EntitiesUndoRedoManager.Clear();
        }

        #endregion

        #region Data Entry
        /// <summary>
        /// Influence column(s) when changes happens in other column
        /// </summary>
        public void CellValueChangedProgressUpdate(DevExpress.Xpf.Grid.CellValueChangedEventArgs e)
        {
            if (e.RowHandle == GridControl.AutoFilterRowHandle)
                return;

            DataRowView dataRowView = (DataRowView)e.Row;
            ESTIMATE_ITEMProgress entity = (ESTIMATE_ITEMProgress)dataRowView.Row[columnEntity];

            if (e.Column.FieldName.Contains(stageFieldNamePercentageAffix) || e.Column.FieldName.Contains(stageFieldNameQuantityAffix))
            {
                //only clear undo redo before update percentage here because this is the only event called from grid
                undoRedoClear();
                undoRedoPause();
                ErrorMessage errorMessage;
                updatePercentage(entity, e.Column.FieldName, e.OldValue, e.Value, out errorMessage);
                showErrorMessage(errorMessage);
                undoRedoUnpause();
            }

            e.Handled = true;
        }

        private void updatePercentage(ESTIMATE_ITEMProgress entity, string fieldName, object oldValue, object newValue, out ErrorMessage errorMessage)
        {
            if (entity.Total_Units == 0)
            {
                errorMessage = new ErrorMessage(entity.Deliverable_Name, "Deliverable doesn't have any units to progress");
                return;
            }

            errorMessage = null;
            if(fieldName.Contains(stageFieldNamePercentageAffix) || fieldName.Contains(stageFieldNameQuantityAffix))
            {
                bool isPercentage = fieldName.Contains(stageFieldNamePercentageAffix);
                string orderIdStr = Regex.Match(fieldName, @"\d+").Value;

                CONSTRUCTION_STAGE constructionSTAGE = CONSTRUCTION_STAGECollection.FirstOrDefault(x => x.SORT_ORDER.ToString() == orderIdStr);
                if (constructionSTAGE != null)
                {
                    List<PROGRESS_ITEM> progressToSave = new List<PROGRESS_ITEM>();

                    //quantity or percentages conversion depending on field name
                    decimal cumulativeEarnedQuantity = 0;
                    decimal cumulativeEarnedPercentage = 0;
                    if(isPercentage)
                    {
                        cumulativeEarnedPercentage = (decimal)newValue;
                        cumulativeEarnedQuantity = cumulativeEarnedPercentage * entity.Total_Quantity;
                    }
                    else
                    {
                        cumulativeEarnedQuantity = (decimal)newValue;
                        cumulativeEarnedPercentage = cumulativeEarnedQuantity / entity.Total_Quantity;
                    }

                    PROGRESS_ITEM currentPeriodPROGRESS_ITEM = entity.PROGRESS_ITEMS.FirstOrDefault(x => x.STAGE_ORDER == constructionSTAGE.SORT_ORDER && x.EARNED_DATE == DataDate);


                    IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMToDate = entity.PROGRESS_ITEMS.Where(x => x.STAGE_ORDER == constructionSTAGE.SORT_ORDER && x.EARNED_DATE < DataDate);
                    decimal dbCumulativeEarnedQuantity = PROGRESS_ITEMToDate.Sum(x => x.EARNED_UNITS);
                    decimal currentPeriodEarnedQuantity = cumulativeEarnedQuantity - dbCumulativeEarnedQuantity;
                    decimal currentPeriodEarnedPercentage = currentPeriodEarnedQuantity / entity.Total_Units;
                    //maximum and minimum is controlled here by the spinedit ability to set max as 100% and min as 0%, and that includes variation validatation, so there is no need to validate here
                    if (currentPeriodPROGRESS_ITEM == null)
                    {
                        currentPeriodPROGRESS_ITEM = new PROGRESS_ITEM();
                        currentPeriodPROGRESS_ITEM.GUID_ORIBASEITEM = entity.OriginalEntityKey;
                        currentPeriodPROGRESS_ITEM.GUID_PROGRESS = loadPROGRESS.GUID;
                        currentPeriodPROGRESS_ITEM.CREATED = DateTime.Now;
                        currentPeriodPROGRESS_ITEM.CREATEDBY = LoginCredentials.CurrentUserGuid;


                        PROGRESS_ITEMSCollectionViewModel.EntitiesUndoRedoManager.AddUndo(currentPeriodPROGRESS_ITEM, null, null, null, EntityMessageType.Added);
                    }
                    else
                    {
                        PROGRESS_ITEMSCollectionViewModel.EntitiesUndoRedoManager.AddUndo(currentPeriodPROGRESS_ITEM, BindableBase.GetPropertyName(() => new PROGRESS_ITEM().EARNED_UNITS), currentPeriodPROGRESS_ITEM.EARNED_UNITS, cumulativeEarnedQuantity, EntityMessageType.Changed);
                    }

                    currentPeriodPROGRESS_ITEM.EARNED_UNITS = currentPeriodEarnedQuantity;
                    currentPeriodPROGRESS_ITEM.EARNED_DATE = DataDate;

                    //audit history
                    PROGRESS_ITEMSCollectionViewModel.EntitiesUndoRedoManager.AddUndo(currentPeriodPROGRESS_ITEM, BindableBase.GetPropertyName(() => new PROGRESS_ITEM().BUDGET_HOURS), currentPeriodPROGRESS_ITEM.BUDGET_HOURS, entity.Total_Units, EntityMessageType.Changed);
                    currentPeriodPROGRESS_ITEM.BUDGET_HOURS = entity.Total_Units;
                    PROGRESS_ITEMSCollectionViewModel.EntitiesUndoRedoManager.AddUndo(currentPeriodPROGRESS_ITEM, BindableBase.GetPropertyName(() => new PROGRESS_ITEM().BUDGET_INSTALL_HOURS_PER_QTY), currentPeriodPROGRESS_ITEM.BUDGET_INSTALL_HOURS_PER_QTY, entity.UnitsPerQuantity, EntityMessageType.Changed);
                    currentPeriodPROGRESS_ITEM.BUDGET_INSTALL_HOURS_PER_QTY = entity.UnitsPerQuantity;
                    PROGRESS_ITEMSCollectionViewModel.EntitiesUndoRedoManager.AddUndo(currentPeriodPROGRESS_ITEM, BindableBase.GetPropertyName(() => new PROGRESS_ITEM().TOTAL_QUANTITY), currentPeriodPROGRESS_ITEM.TOTAL_QUANTITY, entity.Total_Quantity, EntityMessageType.Changed);
                    currentPeriodPROGRESS_ITEM.TOTAL_QUANTITY = entity.Total_Quantity;
                    PROGRESS_ITEMSCollectionViewModel.EntitiesUndoRedoManager.AddUndo(currentPeriodPROGRESS_ITEM, BindableBase.GetPropertyName(() => new PROGRESS_ITEM().EARNED_QUANTITY), currentPeriodPROGRESS_ITEM.EARNED_QUANTITY, cumulativeEarnedQuantity, EntityMessageType.Changed);
                    currentPeriodPROGRESS_ITEM.EARNED_QUANTITY = currentPeriodEarnedQuantity;
                    PROGRESS_ITEMSCollectionViewModel.EntitiesUndoRedoManager.AddUndo(currentPeriodPROGRESS_ITEM, BindableBase.GetPropertyName(() => new PROGRESS_ITEM().EARNED_PERCENTAGE), currentPeriodPROGRESS_ITEM.EARNED_PERCENTAGE, cumulativeEarnedPercentage, EntityMessageType.Changed);
                    currentPeriodPROGRESS_ITEM.EARNED_PERCENTAGE = currentPeriodEarnedPercentage;
                    currentPeriodPROGRESS_ITEM.STAGE_NAME = constructionSTAGE.NAME;
                    currentPeriodPROGRESS_ITEM.STAGE_ORDER = constructionSTAGE.SORT_ORDER;
                    currentPeriodPROGRESS_ITEM.STAGE_WEIGHT = constructionSTAGE.WEIGHT_PERCENTAGE;

                    PROGRESS_ITEMSCollectionViewModel.Save(currentPeriodPROGRESS_ITEM);

                    //add a dummy undo so that during undo/redo operation a baseline item message will be sent
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(entity, BindableBase.GetPropertyName(() => new ESTIMATE_ITEM().GUID), entity.GUID, entity.GUID, EntityMessageType.Changed, true);

                    //save baseline_item here so that auxiliary message can respond to progress item changes
                    MainViewModel.Save(entity);
                }
            }
        }
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

        public IEnumerable<CONSTRUCTION_STAGE> CONSTRUCTION_STAGECollection
        {
            get
            {
                var collection = GetEntities<CONSTRUCTION_STAGE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.SORT_ORDER);

                return collection;
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

        protected override PhaseType progress_type => PhaseType.Construct;

        protected override bool manuallySaveProgressOnAfterBaselineItemSaved => true;

        protected override bool isSingleProjectAndUserLocale => true;

        public override IEnumerable<IReportable> ReportingEntities => Entities;
        #endregion

        #region Reporting
        public override bool CanEditReport()
        {
            return !IsLoading;
        }

        public override void EditReport()
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

        public override bool CanViewReport()
        {
            return !IsLoading;
        }

        public override void ViewReport()
        {
        }
        #endregion
    }
}