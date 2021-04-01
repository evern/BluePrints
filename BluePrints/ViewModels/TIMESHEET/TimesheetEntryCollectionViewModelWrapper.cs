using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Loader;
using BaseModel.ViewModel.UndoRedo;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Filtering;
using BluePrints.Common.Helpers;
using BluePrints.Common.Misc;
using BluePrints.Common.Projections;
using BluePrints.Common.Reports;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Misc;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using BluePrints.Reports;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Core.ConditionalFormatting;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Printing;
using DevExpress.XtraEditors.DXErrorProvider;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single PROGRESS object view model.
    /// </summary>
    public partial class TimesheetEntryCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <BASELINE_ITEM, BASELINE_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of PROGRESS_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static TimesheetEntryCollectionViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new TimesheetEntryCollectionViewModelWrapper());
        }

        #region Database Operation

        List<string> defaultColumnFieldNames = new List<string>();
        List<string> hiddenColumnFieldNames = new List<string>();
        List<string> systemColumnFieldNames = new List<string>();
        private Data.PROJECT loadPROJECT;
        private readonly IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory;
        private IPrimeroEntitiesUnitOfWork primeroUnitOfWork;
        string columnResourceSeqNo = "Resource_SeqNo";
        string columnJobNo = "JobNo";
        string columnCostGroup = "CostGroup";
        string columnCostType = "CostType";
        string columnStockCode = "StockCode";
        string columnVariationCode = "VariationCode";
        string columnNarrative = "Narrative";
        string columnUniqueCode = "UniqueCode";
        string columnDuplicate = "Duplicate";
        string valueNotFoundError = "Value not found";
        public bool IsTimesheetLoaded { get; set; }

        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public ObservableCollection<string> VariationCodes { get; set; }
        DispatcherTimer focusNewlyAddedProjectionTimer = new DispatcherTimer();

        protected override void resolveParameters(object parameter)
        {
            forceApplyBestFit = true;
            var PROJECTParameter = (EntitiesParameter<Data.PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
            primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(loadPROJECT.OfficeNameForExo == BluePrintsResources.OfficeMontreal);
            primeroUnitOfWork = primeroUnitOfWorkFactory.CreateUnitOfWork();
            GetDateRange();

            defaultColumnFieldNames.Add(columnResourceSeqNo);
            defaultColumnFieldNames.Add(columnJobNo);
            defaultColumnFieldNames.Add(columnCostGroup);
            defaultColumnFieldNames.Add(columnCostType);
            defaultColumnFieldNames.Add(columnStockCode);
            defaultColumnFieldNames.Add(columnVariationCode);
            defaultColumnFieldNames.Add(columnNarrative);
            defaultColumnFieldNames.Add(columnUniqueCode);

            systemColumnFieldNames.Add(columnResourceSeqNo);
            systemColumnFieldNames.Add(columnJobNo);
            systemColumnFieldNames.Add(columnCostGroup);
            systemColumnFieldNames.Add(columnCostType);
            systemColumnFieldNames.Add(columnStockCode);
            systemColumnFieldNames.Add(columnVariationCode);
            systemColumnFieldNames.Add(columnNarrative);
            systemColumnFieldNames.Add(columnUniqueCode);

            SelectedDataRows = new ObservableCollection<DataRowView>();
            focusNewlyAddedProjectionTimer = new DispatcherTimer();
            focusNewlyAddedProjectionTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
        }

        public FilterTreeViewModel<BASELINE_ITEMProgress, Guid> FilterTreeViewModel { get; set; }
        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(primeroUnitOfWorkFactory, x => x.JOBCOST_RESOURCE, JOBCOST_RESOURCEQuery);
            loaderCollection.AddLoaderDescription<JOB_COSTGROUPS, JOB_COSTGROUPS, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOB_COSTGROUPS);
            loaderCollection.AddLoaderDescription<JOB_COSTTYPES, JOB_COSTTYPES, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOB_COSTTYPES);
            loaderCollection.AddLoaderDescription(primeroUnitOfWorkFactory, x => x.JOBCOST_HDR, JOBCOST_HDRProjectionFunc);
        }

        private Func<IRepositoryQuery<JOBCOST_HDR>, IQueryable<JOBCOST_HDR>> JOBCOST_HDRProjectionFunc()
        {
            return query => query.Where(x => x.JOBCODE.Contains(loadPROJECT.NUMBER.ToString()));
        }

        List<ExoTimeAuthorisation> preloadedExoLines = new List<ExoTimeAuthorisation>();
        List<ExoTimeAuthorisation> preloadedExoLinesWithCostInfo = new List<ExoTimeAuthorisation>();
        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            preloadedExoLines = ExoQueries.GetProjectLinesWithoutCostInfo(primeroUnitOfWork, loadPROJECT.NUMBER);

            //used to validate cost type against cost group
            preloadedExoLinesWithCostInfo = ExoQueries.GetProjectLines(primeroUnitOfWork, loadPROJECT.NUMBER);
            VariationCodes = new ObservableCollection<string>(preloadedExoLines.Select(x => x.VariationCode).Distinct().OrderBy(x => x));
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS);
            IsTimesheetLoaded = true;
            this.RaisePropertyChanged(x => x.IsTimesheetLoaded);
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEM>> specifyMainViewModelProjection()
        {
            return query => new List<BASELINE_ITEM>().AsQueryable();
        }

        protected Func<IRepositoryQuery<JOBCOST_RESOURCE>, IQueryable<JOBCOST_RESOURCE>> JOBCOST_RESOURCEQuery()
        {
            return query => query.Where(x => x.ISACTIVE == "Y");
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<BASELINE_ITEM> entities)
        {
            MainViewModel.SetParentViewModel(this);
            MainViewModel.IsPasteCellLevel = false;
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        #region Collection Call Backs
        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            loadDataPointsTable();
            base.OnAfterAssignedCallbackAndRaisePropertyChanged();
        }

        public override void FullRefresh()
        {
            if (!CanFullRefresh())
                return;

            isReadingFromExo = true;
            IsTimesheetLoaded = false;
            base.FullRefresh();
        }
        #endregion

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

        private void InitializeColumnSource(ObservableCollection<ColumnDescriptor> columns, ObservableCollection<SummaryDescriptor> summaries, List<DateTime> alignedDates)
        {
            columns.Clear();
            summaries.Clear();

            columns.Add(new ColumnDescriptor() { FieldName = "Resource_SeqNo", Header = "Resource", Fixed = FixedStyle.Left, Width = 150, DisplayMember = "RESOURCENAME", ValueMember = "SEQNO", ItemsSource = JOBCOST_RESOURCECollection, Settings = SettingsType.DisplayMemberCollection });
            summaries.Add(new SummaryDescriptor() { FieldName = "Resource_SeqNo", DisplayFormat = "{0} Record(s)", Type = SummaryItemType.Count });
            columns.Add(new ColumnDescriptor() { FieldName = "JobNo", Header = "Job", Fixed = FixedStyle.Left, Width = 100, DisplayMember = "JOBCODE", ValueMember = "JOBNO", ItemsSource = JOBCOST_HDRCollection, Settings = SettingsType.DisplayMemberCollection });
            columns.Add(new ColumnDescriptor() { FieldName = "CostGroup", Header = "Discipline", Fixed = FixedStyle.Left, Width = 70, DisplayMember = "SHORTCODE", ValueMember = "SEQNO", ItemsSource = JOB_COSTGROUPSCollection, Settings = SettingsType.DisplayMemberCollection });
            columns.Add(new ColumnDescriptor() { FieldName = "CostType", Header = "Commodity", Fixed = FixedStyle.Left, Width = 70, DisplayMember = "SHORTCODE", ValueMember = "SEQNO", ItemsSource = JOB_COSTTYPESCollection, Settings = SettingsType.DisplayMemberCollection });
            columns.Add(new ColumnDescriptor() { FieldName = "StockCode", Header = "Stock Code", Fixed = FixedStyle.Left, Width = 100, NullText = "Leave empty to auto-populate with resource code during submission", Settings = SettingsType.Text });
            columns.Add(new ColumnDescriptor() { FieldName = "VariationCode", Header = "Variation", Fixed = FixedStyle.Left, Width = 70, ItemsSource = VariationCodes, Settings = SettingsType.Collection });
            columns.Add(new ColumnDescriptor() { FieldName = "Narrative", Header = "Narrative", Fixed = FixedStyle.Left, Width = 100, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = "Duplicate", Header = "Duplicate", ReadOnly = true, Fixed = FixedStyle.Right, Width = 50, UnboundType = UnboundColumnType.Boolean, Settings = SettingsType.Unbound });
            columns.Add(new ColumnDescriptor() { FieldName = "UniqueCode", Header = "UniqueCode", ReadOnly = true, Visible = false, Fixed = FixedStyle.Right, Width = 50, Settings = SettingsType.Default });

            foreach (DateTime alignedDate in alignedDates.OrderBy(x => x))
            {
                string columnFieldName = alignedDate.Date.ToShortDateString();
                columns.Add(new ColumnDescriptor() { FieldName = columnFieldName, ReadOnly = false, Header = columnFieldName, Mask = "n", Increment = 1, Fixed = FixedStyle.None, Width = 70, Settings = SettingsType.Number });
            }
        }

        private void refreshDataPointsTable()
        {
            dataPointsTable = null;
            mainThreadDispatcher.BeginInvoke(new Action(() => loadDataPointsTable()));
        }

        protected override void onAfterRefresh()
        {
            base.onAfterRefresh();
            refreshDataPointsTable();
        }

        public override void OnAfterAuxiliaryEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender, Guid senderKey, bool isBulkRefresh)
        {
            base.OnAfterAuxiliaryEntitiesChanged(key, changedType, messageType, sender, senderKey, isBulkRefresh);
        }
        
        private void reselectDeliverable()
        {
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertyChanged(x => x.SelectedEntity)));
        }

        DataRowView selectedDataRow;
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

        public ObservableCollection<DataRowView> SelectedDataRows { get; set; }

        List<DataRowView> newlyAddedRows;
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

        private void FocusNewlyAddedProjectionTimer_Tick(object sender, EventArgs e)
        {
            focusNewlyAddedProjectionTimer.Stop();
            if (Entities == null || newlyAddedRows.Count() == 0)
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

        private void loadDataPointsTable()
        {
            IsLoading = true;
            this.RaisePropertyChanged(x => x.IsLoading);

            dataPointsTable = null;

            updateDataPointsTable();
            this.RaisePropertyChanged(x => x.DataPointsTable);

            doNotPrompt = true;
            ReadFromExo();
            doNotPrompt = false;

            IsLoading = false;
            this.RaisePropertyChanged(x => x.IsLoading);
            TableViewService.ScrollToLast();
            CommonMethods.AddSaveLayoutHandler(GridControlService.GetGridColumns());
        }

        private void updateDataPointsTable()
        {
            GridControlService.BeginDataUpdate();

            dataPointsTable = new DataTable();
            dataPointsTable.RowChanged += DataPointsTable_RowChanged;
            TimeSpan interval = new TimeSpan(1, 0, 0, 0);
            List<DateTime> alignedDataDateCollection = ChronologicalHelpers.GenerateAlignedDatesCollection(DateFrom, DateTo, interval);
            InitializeColumnSource(ColumnDescriptors, SummaryDescriptors, alignedDataDateCollection);
            dataPointsTable.Columns.Add(columnResourceSeqNo, typeof(int));
            dataPointsTable.Columns.Add(columnJobNo, typeof(int));
            dataPointsTable.Columns.Add(columnCostGroup, typeof(int));
            dataPointsTable.Columns.Add(columnCostType, typeof(int));
            dataPointsTable.Columns.Add(columnStockCode, typeof(string));
            dataPointsTable.Columns.Add(columnVariationCode, typeof(string));
            dataPointsTable.Columns.Add(columnNarrative, typeof(string));

            foreach (DateTime alignedDataDate in alignedDataDateCollection)
            {
                string columnFieldName = alignedDataDate.Date.ToShortDateString();
                dataPointsTable.Columns.Add(columnFieldName, typeof(decimal));
            }

            //unique code must be added to the end, because it is invisible, so it won't interfere with copy paste
            dataPointsTable.Columns.Add(columnUniqueCode, typeof(string));

            GridControlService.EndDataUpdate();
        }

        DataTable dataPointsTable = null;
        //to prevent table new row from getting called when it's loading
        bool isReadingFromExo = true;
        public DataTable DataPointsTable
        {
            get
            {
                return dataPointsTable;
            }
        }

        public void ShowValidJobLines()
        {
            if (MessageBoxService.ShowMessage("Are you sure you want to show all valid job lines?\n\nThis will clear current table entries", "Confirmation", MessageButton.OKCancel) != MessageResult.OK)
                return;

            DataPointsTable.Clear();
            IEnumerable<ExoTimeAuthorisation> populateAuthorisations = preloadedExoLines.OrderBy(x => x.ResourceName).ThenBy(x => x.SubJobCode).ThenBy(x => x.DisciplineCode).ThenBy(x => x.CommodityCode);
            foreach(ExoTimeAuthorisation populateAuthorisation in populateAuthorisations)
            {
                DataRow newRow = DataPointsTable.NewRow();
                newRow[columnResourceSeqNo] = populateAuthorisation.ResourceSeqNo;
                newRow[columnJobNo] = populateAuthorisation.SubJobNo;
                newRow[columnCostGroup] = populateAuthorisation.DisciplineId;
                newRow[columnCostType] = populateAuthorisation.CommodityId;
                newRow[columnStockCode] = populateAuthorisation.StockCode;
                DataPointsTable.Rows.Add(newRow);
            }

            GridControlService.RefreshData();
        }

        public bool CanReadFromExo()
        {
            return dataPointsTable != null;
        }

        bool doNotPrompt = false;
        public void ReadFromExo()
        {
            if(!doNotPrompt)
                if (MessageBoxService.ShowMessage("Are you sure you want to read hours from exo?\n\nThis will clear the table and replace hours with hours from exo", "Confirmation", MessageButton.OKCancel) != MessageResult.OK)
                    return;

            isReadingFromExo = true;
            EntitiesUndoRedoManager.Clear();
            DataPointsTable.Clear();

            IEnumerable<JOBCOST_HDR> subJobs = ExoQueries.GetProjectSubJobs(primeroUnitOfWork, loadPROJECT.NUMBER);
            if(subJobs == null)
            {
                MessageBoxService.ShowMessage("There are no subjobs for this project", "Confirmation", MessageButton.OK);
                return;
            }

            List<TimesheetDate> weekStartDates = new List<TimesheetDate>();
            List<string> dateColumnNames = new List<string>();
            foreach (DataColumn dataColumn in DataPointsTable.Columns)
            {
                DateTime bookDate = DateTime.Now;
                if (DateTime.TryParse(dataColumn.ColumnName, out bookDate))
                {
                    TimesheetDate timeSheetDate = GetTimesheetDate(bookDate);
                    if (!weekStartDates.Any(x => x.WeekStartDate == timeSheetDate.WeekStartDate))
                        weekStartDates.Add(timeSheetDate);

                    dateColumnNames.Add(dataColumn.ColumnName);
                }
            }

            List<DataRow> newRows = new List<DataRow>();
            LoadingScreenManager.ShowLoadingScreen(subJobs.Count());
            foreach (JOBCOST_HDR subjob in subJobs)
            {
                List<JOB_TIMESHEETS> timeSheetAllDates = new List<JOB_TIMESHEETS>();
                foreach(TimesheetDate weekStartDate in weekStartDates)
                {
                    IQueryable<JOB_TIMESHEETS> timeSheets = primeroUnitOfWork.JOB_TIMESHEETS.Where(x => x.WEEK_START_DATE == weekStartDate.WeekStartDate && x.JOBNO == subjob.JOBNO);
                    timeSheetAllDates.AddRange(timeSheets.ToList());
                }

                if(timeSheetAllDates.Count > 0)
                {
                    foreach (JOB_TIMESHEETS timeSheet in timeSheetAllDates)
                    {
                        int findCostGroup;
                        if (timeSheet.COST_GROUP == null)
                            continue;
                        else
                            findCostGroup = (int)timeSheet.COST_GROUP;

                        int findCostType;
                        if (timeSheet.COST_TYPE == null)
                            continue;
                        else
                            findCostType = (int)timeSheet.COST_TYPE;

                        string findVariationCode = timeSheet.X_VARIATIONCODE;
                        string findNarrative = timeSheet.X_NARRATIVE;

                        DataRow newRow = newRows.FirstOrDefault(x => (int)x[columnJobNo] == timeSheet.JOBNO && (int)x[columnResourceSeqNo] == timeSheet.STAFFNO && (int)x[columnCostGroup] == findCostGroup && (int)x[columnCostType] == findCostType && x[columnVariationCode].ToString() == findVariationCode && x[columnNarrative].ToString() == findNarrative);
                        if(newRow == null)
                        {
                            newRow = DataPointsTable.NewRow();
                            newRow[columnResourceSeqNo] = timeSheet.STAFFNO;
                            newRow[columnJobNo] = timeSheet.JOBNO;
                            newRow[columnVariationCode] = timeSheet.X_VARIATIONCODE;
                            newRow[columnNarrative] = timeSheet.X_NARRATIVE;

                            if (timeSheet.COST_GROUP == null)
                                newRow[columnCostGroup] = DBNull.Value;
                            else
                                newRow[columnCostGroup] = (int)timeSheet.COST_GROUP;

                            if (timeSheet.COST_TYPE == null)
                                newRow[columnCostType] = DBNull.Value;
                            else
                                newRow[columnCostType] = (int)timeSheet.COST_TYPE;

                            newRow[columnStockCode] = timeSheet.STOCKCODE;

                            newRows.Add(newRow);
                            validateUserAuth(newRow);
                        }

                        foreach (string dateColumnName in dateColumnNames)
                        {
                            DateTime bookDate = DateTime.Parse(dateColumnName);
                            TimesheetDate timesheetDate = GetTimesheetDate(bookDate);

                            if (timeSheet.WEEK_START_DATE != timesheetDate.WeekStartDate)
                                continue;

                            bool isReadOnly = false;
                            double? exoHours = GetTimeSheetHours(timeSheet, timesheetDate, out isReadOnly);
                            if (exoHours == null)
                                newRow[dateColumnName] = DBNull.Value;
                            else
                            {
                                decimal exoHoursDecimal = Convert.ToDecimal((double)exoHours);
                                newRow[dateColumnName] = exoHoursDecimal;
                                if (isReadOnly)
                                    newRow.SetColumnError(dateColumnName, "Already Submitted to Exo");
                                else
                                    newRow.SetColumnError(dateColumnName, string.Empty);
                            }
                        }
                    }
                }

                LoadingScreenManager.Progress();
            }

            foreach (DataRow newRow in newRows)
            {
                validateUserAuth(newRow);
                DataPointsTable.Rows.Add(newRow);
            }
            
            //generate unique signature for each row
            isRowsUnique();

            GridControlService.RefreshData();

            if (!doNotPrompt)
                MessageBoxService.ShowMessage("Data retrieved from exo");

            isReadingFromExo = false;
        }

        public bool CanCommitToExo()
        {
            return IsTimesheetLoaded && DataPointsTable != null && DataPointsTable.Rows.Count > 0;
        }

        public void CommitToExo()
        {
            if(LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Permission_EXO_Timesheets_Commit)) == LoginCredentials.PermissionStatus.None)
            {
                MessageBoxService.ShowMessage("You do not have authority to commit to exo", "Unauthorised", MessageButton.OK);
                return;
            }

            if (MessageBoxService.ShowMessage("Are you sure you want to commit current table to exo?", "Confirmation", MessageButton.OKCancel) != MessageResult.OK)
                return;

            int committedRow = 0;
            if(!isRowsUnique())
            {
                MessageBoxService.ShowMessage("Rows that are not unique are highlighted in red, please delete the one's you don't want and try again later");
                return;
            }

            LoadingScreenManager.ShowLoadingScreen(DataPointsTable.Rows.Count);
            foreach (DataRow row in DataPointsTable.Rows)
            {
                if (row[columnResourceSeqNo].ToString() != string.Empty && row[columnJobNo].ToString() != string.Empty && row[columnCostGroup].ToString() != string.Empty && row[columnCostType].ToString() != string.Empty)
                {
                    int resourceSeqNo = (int)row[columnResourceSeqNo];
                    int subJobNo = (int)row[columnJobNo];
                    int costGroupNo = (int)row[columnCostGroup];
                    int costTypeNo = (int)row[columnCostType];
                    string variationCode = row[columnVariationCode].ToString();
                    if (variationCode.Length > 50)
                        variationCode = variationCode.Substring(0, 50);
                    else if (variationCode == string.Empty)
                        variationCode = null;
                    
                    string narrative = row[columnNarrative].ToString();

                    //not validating against cost group and cost type
                    //ExoTimeAuthorisation findExoLine = exoLines.FirstOrDefault(x => x.SubJobNo == subJobNo && x.DisciplineId == costGroupNo && x.CommodityId == costTypeNo && x.VariationCode == variationCode);

                    bool isVariationCodeNull = variationCode == string.Empty || variationCode == null;
                    bool isValidJobCode = IsValidJobCode(subJobNo, costGroupNo, costTypeNo, variationCode);
                    string subJobCode = string.Empty;
                    string subJobTitle = string.Empty;
                    string stockCode = row[columnStockCode].ToString();
                    string stockCodeDescription = string.Empty;
                    if (isValidJobCode)
                    {
                        JOBCOST_HDR subJob = primeroUnitOfWork.JOBCOST_HDR.FirstOrDefault(x => x.JOBNO == subJobNo);
                        if(subJob != null)
                        {
                            subJobCode = subJob.JOBCODE;
                            subJobTitle = subJob.TITLE;
                        }

                        if (stockCode.Trim() == string.Empty)
                        {
                            JOBCOST_RESOURCE resource = primeroUnitOfWork.JOBCOST_RESOURCE.FirstOrDefault(x => x.SEQNO == resourceSeqNo);
                            stockCode = resource.DEFAULT_STOCKCODE;
                        }

                        STOCK_ITEMS stockItem = primeroUnitOfWork.STOCK_ITEMS.FirstOrDefault(x => x.STOCKCODE == stockCode);
                        if (stockItem != null)
                        {
                            stockCode = stockItem.STOCKCODE;
                            stockCodeDescription = stockItem.DESCRIPTION;
                        }
                    }
                    
                    if(subJobCode != string.Empty && subJobTitle != string.Empty && stockCode != string.Empty && stockCodeDescription != string.Empty)
                    {
                        committedRow += 1;
                        foreach (DataColumn dataColumn in DataPointsTable.Columns)
                        {
                            DateTime bookDate = DateTime.Now;
                            if (DateTime.TryParse(dataColumn.ColumnName, out bookDate))
                            {
                                TimesheetDate timesheetDate = GetTimesheetDate(bookDate);
                                if (row[dataColumn] == DBNull.Value)
                                    continue;

                                decimal bookTime = (decimal)row[dataColumn];
                                bool isReadOnly = false;
                                JOB_TIMESHEETS timesheet = findNullOrEmptyVariationJobTimesheets(primeroUnitOfWork, resourceSeqNo, subJobNo, stockCode, costGroupNo, costTypeNo, variationCode, narrative, timesheetDate.WeekStartDate);
                                if (timesheet != null)
                                {
                                    AdjustTimeSheetHours(timesheet, timesheetDate, bookTime, out isReadOnly);
                                    if (isReadOnly)
                                        row.SetColumnError(dataColumn, "Already Submitted to Exo");
                                    else
                                        row.SetColumnError(dataColumn, string.Empty);

                                    primeroUnitOfWork.SaveChanges();
                                }
                                else
                                {
                                    JOB_TIMESHEETS newTimeSheet = new JOB_TIMESHEETS();
                                    newTimeSheet.STAFFNO = resourceSeqNo;
                                    newTimeSheet.JOBNO = subJobNo;
                                    newTimeSheet.TITLE = subJobCode + " : " + subJobTitle.Substring(0, subJobTitle.Length < 40 ? subJobTitle.Length : 40);
                                    newTimeSheet.STOCKCODE = stockCode;
                                    newTimeSheet.DESCRIPTION = stockCodeDescription;
                                    newTimeSheet.UNITPRICE = 0;
                                    newTimeSheet.WEEK_START_DATE = timesheetDate.WeekStartDate;
                                    AdjustTimeSheetHours(newTimeSheet, timesheetDate, bookTime, out isReadOnly);
                                    newTimeSheet.IS_OVERTIME = "N";
                                    newTimeSheet.DAY1_POSTED = "N";
                                    newTimeSheet.DAY2_POSTED = "N";
                                    newTimeSheet.DAY3_POSTED = "N";
                                    newTimeSheet.DAY4_POSTED = "N";
                                    newTimeSheet.DAY5_POSTED = "N";
                                    newTimeSheet.DAY6_POSTED = "N";
                                    newTimeSheet.DAY7_POSTED = "N";
                                    newTimeSheet.RATE_SEQNO = 0;
                                    newTimeSheet.RATE_FACTOR = 1;
                                    newTimeSheet.COST_GROUP = costGroupNo;
                                    newTimeSheet.COST_TYPE = costTypeNo;
                                    newTimeSheet.LABOUR_ALLOWANCE = 0;
                                    newTimeSheet.HAS_ALLOWANCE = "N";
                                    newTimeSheet.X_DECLINED = false;
                                    newTimeSheet.X_APPROVAL_MANAGER = -1;
                                    newTimeSheet.X_SUBMITTED = false;
                                    newTimeSheet.X_VARIATIONCODE = variationCode;
                                    newTimeSheet.X_NARRATIVE = narrative;
                                    primeroUnitOfWork.JOB_TIMESHEETS.Add(newTimeSheet);
                                    primeroUnitOfWork.SaveChanges();
                                }
                            }
                        }
                    }
                }

                LoadingScreenManager.Progress();
            }

            MessageBoxService.ShowMessage(committedRow.ToString() + " records committed to exo");
        }

        private bool IsValidJobCode(int subJobNo, int costGroupNo, int costTypeNo, string variationCode)
        {
            bool isVariationCodeNull = variationCode == string.Empty || variationCode == null;
            ExoTimeAuthorisation findExoLine = preloadedExoLinesWithCostInfo.FirstOrDefault(x => x.SubJobNo == subJobNo && x.DisciplineId == costGroupNo && x.CommodityId == costTypeNo);
            if (isVariationCodeNull)
            {
                if (findExoLine != null)
                    return true;
            }
            //for codes with variation, we want to validate against main job code first then against variation code
            else
            {
                if(findExoLine != null)
                {
                    ExoTimeAuthorisation findExoVariationLine;
                    if(isVariationCodeNull)
                        findExoVariationLine = preloadedExoLines.FirstOrDefault(x => x.SubJobNo == subJobNo && (x.VariationCode == null || x.VariationCode == string.Empty));
                    else
                        findExoVariationLine = preloadedExoLines.FirstOrDefault(x => x.SubJobNo == subJobNo && (x.VariationCode != null && x.VariationCode.ToUpper().Contains(variationCode.ToUpper())));

                    if (findExoVariationLine != null)
                        return true;
                }
            }

            return false;
        }

        //populate unique code for each row and checks whether all of them are unique
        private bool isRowsUnique()
        {
            List<string> scannedUniqueCode = new List<string>();
            foreach (DataRow row in DataPointsTable.Rows)
            {
                string uniqueCode = populateRowUniqueCode(row);
                if(uniqueCode != string.Empty)
                    scannedUniqueCode.Add(uniqueCode);
            }

            var uniqueCodeGrouped = scannedUniqueCode.GroupBy(x => x);
            if (uniqueCodeGrouped.Any(x => x.Count() > 1))
                return false;

            return true;
        }

        private string populateRowUniqueCode(DataRow row)
        {
            if (row[columnResourceSeqNo].ToString() != string.Empty && row[columnJobNo].ToString() != string.Empty && row[columnCostGroup].ToString() != string.Empty && row[columnCostType].ToString() != string.Empty)
            {
                int resourceSeqNo = (int)row[columnResourceSeqNo];
                int subJobNo = (int)row[columnJobNo];
                int costGroupNo = (int)row[columnCostGroup];
                int costTypeNo = (int)row[columnCostType];
                string stockCode = row[columnStockCode].ToString();
                string variationCode = row[columnVariationCode].ToString();
                string narrative = row[columnNarrative].ToString();

                //makes up the unique signature of each row by these attribute so we can see if other rows contain the same attribute and flag it as duplicate
                string uniqueCode = resourceSeqNo.ToString() + subJobNo.ToString() + costGroupNo.ToString() + costTypeNo.ToString() + stockCode + variationCode + narrative;
                row[columnUniqueCode] = uniqueCode;
                return uniqueCode;
            }
            else
                return string.Empty;
        }

        //because timesheet with variation code can be string.empty or null, do double checking here
        private JOB_TIMESHEETS findNullOrEmptyVariationJobTimesheets(IPrimeroEntitiesUnitOfWork primeroEntitiesUnitOfWork, int resourceSeqNo, int subJobNo, string stockCode, int costGroupNo, int costTypeNo, string variationCode, string narrative, DateTime timesheetWeekStartDate)
        {
            bool isVariationCodeNull = variationCode == null || variationCode == string.Empty;
            bool isNarrativeNull = narrative == null || narrative == string.Empty;

            if(!isNarrativeNull && !isVariationCodeNull)
                return primeroUnitOfWork.JOB_TIMESHEETS.FirstOrDefault(x => x.STAFFNO == resourceSeqNo && x.JOBNO == subJobNo && x.STOCKCODE == stockCode && x.COST_GROUP == costGroupNo && x.COST_TYPE == costTypeNo && x.WEEK_START_DATE == timesheetWeekStartDate && x.X_NARRATIVE == narrative && x.X_VARIATIONCODE == variationCode);
            else if(isNarrativeNull && !isVariationCodeNull)
                return primeroUnitOfWork.JOB_TIMESHEETS.FirstOrDefault(x => x.STAFFNO == resourceSeqNo && x.JOBNO == subJobNo && x.STOCKCODE == stockCode && x.COST_GROUP == costGroupNo && x.COST_TYPE == costTypeNo && x.WEEK_START_DATE == timesheetWeekStartDate && (x.X_NARRATIVE == string.Empty || x.X_NARRATIVE == null) && x.X_VARIATIONCODE == variationCode);
            else if (!isNarrativeNull && isVariationCodeNull)
                return primeroUnitOfWork.JOB_TIMESHEETS.FirstOrDefault(x => x.STAFFNO == resourceSeqNo && x.JOBNO == subJobNo && x.STOCKCODE == stockCode && x.COST_GROUP == costGroupNo && x.COST_TYPE == costTypeNo && x.WEEK_START_DATE == timesheetWeekStartDate && x.X_NARRATIVE == narrative && (x.X_VARIATIONCODE == null || x.X_VARIATIONCODE == string.Empty));
            //(isNarrativeNull && isVariationCodeNull)
            else
                return primeroUnitOfWork.JOB_TIMESHEETS.FirstOrDefault(x => x.STAFFNO == resourceSeqNo && x.JOBNO == subJobNo && x.STOCKCODE == stockCode && x.COST_GROUP == costGroupNo && x.COST_TYPE == costTypeNo && x.WEEK_START_DATE == timesheetWeekStartDate && (x.X_NARRATIVE == string.Empty || x.X_NARRATIVE == null) && (x.X_VARIATIONCODE == null || x.X_VARIATIONCODE == string.Empty));

        }

        public TimesheetDate GetTimesheetDate(DateTime bookDate)
        {
            DateTime startOfWeek = bookDate.StartOfWeek(DayOfWeek.Monday);
            int DayNum = (bookDate - startOfWeek).Days + 1;

            return new TimesheetDate() { WeekStartDate = startOfWeek, DayNumber = DayNum };
        }

        private void AdjustTimeSheetHours(JOB_TIMESHEETS timesheet, TimesheetDate bookDate, decimal bookTime, out bool isReadOnly)
        {
            Double dblTime = Convert.ToDouble(bookTime);
            switch (bookDate.DayNumber)
            {
                case 1:
                    if (timesheet.DAY1_POSTED == "Y")
                    {
                        isReadOnly = true;
                        break;
                    }

                    isReadOnly = false;
                    timesheet.DAY1 = dblTime;
                    timesheet.DAY1_NARRATIVE = BluePrintsUtils.FindExistingOrAddNewNarrative(timesheet.X_NARRATIVE, primeroUnitOfWork);
                    break;
                case 2:
                    if (timesheet.DAY2_POSTED == "Y")
                    {
                        isReadOnly = true;
                        break;
                    }

                    isReadOnly = false;
                    timesheet.DAY2 = dblTime;
                    timesheet.DAY2_NARRATIVE = BluePrintsUtils.FindExistingOrAddNewNarrative(timesheet.X_NARRATIVE, primeroUnitOfWork);
                    break;
                case 3:
                    if (timesheet.DAY3_POSTED == "Y")
                    {
                        isReadOnly = true;
                        break;
                    }

                    isReadOnly = false;
                    timesheet.DAY3 = dblTime;
                    timesheet.DAY3_NARRATIVE = BluePrintsUtils.FindExistingOrAddNewNarrative(timesheet.X_NARRATIVE, primeroUnitOfWork);
                    break;
                case 4:
                    if (timesheet.DAY4_POSTED == "Y")
                    {
                        isReadOnly = true;
                        break;
                    }

                    isReadOnly = false;
                    timesheet.DAY4 = dblTime;
                    timesheet.DAY4_NARRATIVE = BluePrintsUtils.FindExistingOrAddNewNarrative(timesheet.X_NARRATIVE, primeroUnitOfWork);
                    break;
                case 5:
                    if (timesheet.DAY5_POSTED == "Y")
                    {
                        isReadOnly = true;
                        break;
                    }

                    isReadOnly = false;
                    timesheet.DAY5 = dblTime;
                    timesheet.DAY5_NARRATIVE = BluePrintsUtils.FindExistingOrAddNewNarrative(timesheet.X_NARRATIVE, primeroUnitOfWork);
                    break;
                case 6:
                    if (timesheet.DAY6_POSTED == "Y")
                    {
                        isReadOnly = true;
                        break;
                    }

                    isReadOnly = false;
                    timesheet.DAY6 = dblTime;
                    timesheet.DAY6_NARRATIVE = BluePrintsUtils.FindExistingOrAddNewNarrative(timesheet.X_NARRATIVE, primeroUnitOfWork);
                    break;
                case 7:
                    if (timesheet.DAY7_POSTED == "Y")
                    {
                        isReadOnly = true;
                        break;
                    }

                    isReadOnly = false;
                    timesheet.DAY7 = dblTime;
                    timesheet.DAY7_NARRATIVE = BluePrintsUtils.FindExistingOrAddNewNarrative(timesheet.X_NARRATIVE, primeroUnitOfWork);
                    break;
                default:
                    isReadOnly = false;
                    break;
            }
        }

        private double? GetTimeSheetHours(JOB_TIMESHEETS timesheet, TimesheetDate bookDate, out bool isReadOnly)
        {
            switch (bookDate.DayNumber)
            {
                case 1:
                    isReadOnly = timesheet.DAY1_POSTED == "Y" ? true : false;
                    return timesheet.DAY1;
                case 2:
                    isReadOnly = timesheet.DAY1_POSTED == "Y" ? true : false;
                    return timesheet.DAY2;
                case 3:
                    isReadOnly = timesheet.DAY1_POSTED == "Y" ? true : false;
                    return timesheet.DAY3;
                case 4:
                    isReadOnly = timesheet.DAY1_POSTED == "Y" ? true : false;
                    return timesheet.DAY4;
                case 5:
                    isReadOnly = timesheet.DAY1_POSTED == "Y" ? true : false;
                    return timesheet.DAY5;
                case 6:
                    isReadOnly = timesheet.DAY1_POSTED == "Y" ? true : false;
                    return timesheet.DAY6;
                case 7:
                    isReadOnly = timesheet.DAY1_POSTED == "Y" ? true : false;
                    return timesheet.DAY7;
                default:
                    isReadOnly = false;
                    return null;
            }
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            //get { return "OffsiteDirectProgressViewModelWrapper" + view_project_specific_affix; }
            get { return "TimesheetEntryViewModelWrapper_v2" + view_project_specific_affix; }
        }

        private DevExpress.Mvvm.IDialogService DateFromToDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("DateFromToDialogService"); }
        }

        public void GetDateRange()
        {
            UICommand okCommand = new UICommand()
            {
                Id = TimesheetDateDialogAction.Ok,
                Caption = "Ok",
                IsCancel = true,
                IsDefault = false,
            };

            UICommand currentCommand = new UICommand()
            {
                Id = TimesheetDateDialogAction.UseWeekStart,
                Caption = "Use previous week start",
                IsCancel = true,
                IsDefault = false,
            };

            var dateFromToViewModel = DateFromToDialogViewModel.Create();
            UICommand result = DateFromToDialogService.ShowDialog(new List<UICommand>() { okCommand, currentCommand }, "Select Date Range to Query", "DateFromTo", dateFromToViewModel);

            if(result == okCommand)
            {
                DateFrom = dateFromToViewModel.DateFrom;
                DateTo = dateFromToViewModel.DateTo;
            }
            else
            {
                DateFrom = DateTime.Now.StartOfWeek(DayOfWeek.Monday).AddDays(-7);
                DateTo = DateFrom.AddDays(5);
            }
        }

        private string view_project_specific_affix
        {
            get
            {
                if (loadPROJECT == null)
                    return string.Empty;
                return loadPROJECT.GUID.ToString();
            }
        }
        #endregion


        public override void PastingFromClipboard(PastingFromClipboardEventArgs e)
        {
            GridControl gridControl = (GridControl)e.Source;
            TableView gridTableView = (TableView)gridControl.View;
            string newValueString = Clipboard.GetText().ToString();
            string[] RowData = DataUtils.ExcelSplit(newValueString).ToArray();

            if (MainViewModel.SelectMode == MultiSelectMode.Row)
                pasteRowData(gridTableView, RowData);
            else
                pasteCellData(gridControl, gridTableView, RowData);

            isRowsUnique();
            gridControl.RefreshData();
            e.Handled = true;
        }

        private void pasteCellData(GridControl gridControl, TableView gridTableView, string[] RowData)
        {
            EntitiesUndoRedoManager.PauseActionId();
            var selected_cells = gridTableView.GetSelectedCells();
            if (selected_cells.Count == 0)
                return;

            List<List<string>> row_data = new List<List<string>>();
            foreach (var row in RowData)
            {
                List<string> column_data = row.Split('\t').ToList();
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
                    DataColumn editing_column = editing_row.Table.Columns[selected_cell.Column.VisibleIndex];
                    basePasteData(editing_row, editing_column, selected_cell.Column, string.Empty, false);
                    tryFixCommodityCode(editing_row);
                    validateUserAuth(editing_row);
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
                        if (first_column_visible_index + columnOffset >= visible_columns.Count)
                            continue;

                        GridColumn current_column = visible_columns[first_column_visible_index + columnOffset];
                        string columnValue = grouped_results[pasteValueColumnOffset][pasteValueRowOffset];

                        int current_row_visible_index = first_row_visible_index + rowOffset;
                        int current_row_handle = gridControl.GetRowHandleByVisibleIndex(current_row_visible_index);

                        object rowObject = gridControl.GetRow(current_row_handle);
                        if (rowObject == null)
                            continue;

                        DataRowView editing_row_view = (DataRowView)rowObject;
                        DataRow editing_row = editing_row_view.Row;
                        DataColumn editing_column = editing_row.Table.Columns[current_column.VisibleIndex];
                        if (editing_row == null)
                        {
                            MessageBoxService.ShowMessage("Please remove all line break from paste data or double click into cell to paste your data with line breaks");
                            break;
                        }
                        
                        pasteValueColumnOffset += 1;
                        if (pasteValueColumnOffset >= grouped_results.Count)
                            pasteValueColumnOffset = 0;

                        basePasteData(editing_row, editing_column, current_column, columnValue, false);
                        tryFixCommodityCode(editing_row);
                        validateUserAuth(editing_row);
                    }

                    pasteValueRowOffset += 1;
                    if (pasteValueRowOffset >= grouped_results[pasteValueColumnOffset].Count)
                        pasteValueRowOffset = 0;
                }
            }

            EntitiesUndoRedoManager.UnpauseActionId();
        }

        private List<DataRowView> pasteRowData(TableView gridTableView, string[] RowData)
        {
            EntitiesUndoRedoManager.PauseActionId();
            List<DataRowView> newlyAddedRows = new List<DataRowView>();
            foreach (var Row in RowData)
            {
                DataRow newRow = DataPointsTable.NewRow();
                var ColumnStrings = Row.Split('\t');
                for (var i = 0; i < ColumnStrings.Count(); i++)
                {
                    if (i > gridTableView.VisibleColumns.Count - 1)
                        continue;

                    string pasteData = ColumnStrings[i];
                    ColumnBase copyColumn = gridTableView.VisibleColumns[i];
                    basePasteData(newRow, newRow.Table.Columns[i], copyColumn, pasteData, true);
                }

                validateUserAuth(newRow);
                DataPointsTable.Rows.Add(newRow);
                DataRowView dataRowView = DataPointsTable.DefaultView[DataPointsTable.Rows.IndexOf(newRow)];
                newlyAddedRows.Add(dataRowView);
                EntitiesUndoRedoManager.AddUndo(newRow, null, null, null, EntityMessageType.Added);
            }
            EntitiesUndoRedoManager.UnpauseActionId();

            return newlyAddedRows;
        }

        private void DataPointsTable_RowChanged(object sender, DataRowChangeEventArgs e)
        {
            if (e.Action == DataRowAction.Add)
            {
                if (isReadingFromExo)
                    return;

                int rowIndex = DataPointsTable.Rows.IndexOf(e.Row);
                if (rowIndex >= 0)
                {
                    DataRowView dataRowView = DataPointsTable.DefaultView[DataPointsTable.Rows.IndexOf(e.Row)];
                    OnAfterNewProjectionsAdded(dataRowView);
                }
            }
        }

        private bool basePasteData(DataRow newRow, DataColumn dataColumn, ColumnBase copyColumn, string pasteData, bool isNewRow)
        {
            ComboBoxEditSettings editSettings = copyColumn.ActualEditSettings as ComboBoxEditSettings;
            if (editSettings != null)
            {
                var copyColumnValueMember = (string)editSettings.GetType().GetProperty("ValueMember").GetValue(editSettings);
                var copyColumnDisplayMember = (string)editSettings.GetType().GetProperty("DisplayMember").GetValue(editSettings);
                var copyColumnItemsSource = (IEnumerable<object>)editSettings.GetType().GetProperty("ItemsSource").GetValue(editSettings);
                int? int_value = null;

                if (copyColumnItemsSource == null || copyColumnValueMember == null || copyColumnDisplayMember == null)
                    return false;

                foreach (var copyColumnItem in copyColumnItemsSource)
                {
                    if (copyColumnItem == null)
                        continue;

                    if(copyColumn.FieldType == typeof(string))
                    {
                        if (copyColumnItem.ToString().ToUpper().Contains(pasteData.ToUpper()))
                        {
                            //used for variation code column
                            newRow[dataColumn] = copyColumnItem.ToString();
                            EntitiesUndoRedoManager.AddUndo(newRow, dataColumn.ColumnName, newRow[dataColumn], int_value, EntityMessageType.Changed);
                            return true;
                        }
                    }
                    else
                    {
                        var itemDisplayMemberPropertyInfo = copyColumnItem.GetType().GetProperty(copyColumnDisplayMember);
                        var itemValueMemberPropertyInfo = copyColumnItem.GetType().GetProperty(copyColumnValueMember);

                        if (itemDisplayMemberPropertyInfo.GetValue(copyColumnItem).ToString().ToUpper() == pasteData.ToUpper())
                        {
                            int_value = (int)itemValueMemberPropertyInfo.GetValue(copyColumnItem);
                            break;
                        }
                    }
                }

                if (int_value != null)
                {
                    if(!isNewRow)
                        EntitiesUndoRedoManager.AddUndo(newRow, dataColumn.ColumnName, newRow[dataColumn], int_value, EntityMessageType.Changed);

                    newRow[dataColumn] = int_value;
                    newRow.SetColumnError(dataColumn, string.Empty);
                }
                else
                {
                    if(!isNewRow)
                        EntitiesUndoRedoManager.AddUndo(newRow, dataColumn.ColumnName, newRow[dataColumn], DBNull.Value, EntityMessageType.Changed);

                    newRow[dataColumn] = DBNull.Value;
                    newRow.SetColumnError(dataColumn, valueNotFoundError);
                    return false;
                }
            }
            else if (copyColumn.FieldType == typeof(decimal))
            {
                var rgx = new Regex(BluePrintsResources.Regex_NumbersOnly);
                var cleanColumnString = rgx.Replace(pasteData, string.Empty);
                decimal decimal_value;
                if (decimal.TryParse(cleanColumnString, out decimal_value))
                {
                    if (!isNewRow)
                        EntitiesUndoRedoManager.AddUndo(newRow, dataColumn.ColumnName, newRow[dataColumn], decimal_value, EntityMessageType.Changed);

                    newRow[dataColumn] = decimal_value;
                }
                else
                {
                    if (!isNewRow)
                        EntitiesUndoRedoManager.AddUndo(newRow, dataColumn.ColumnName, newRow[dataColumn], DBNull.Value, EntityMessageType.Changed);

                    newRow[dataColumn] = DBNull.Value;
                    return false;
                }
            }
            else if(copyColumn.FieldType == typeof(string))
            {
                newRow[dataColumn] = pasteData;
            }

            return true;
        }

        public override void BulkDelete()
        {
            GridControlService.BeginDataUpdate();
            int[] selectedRowHandles = GridControlService.GetSelectedRowHandles();
            EntitiesUndoRedoManager.PauseActionId();
            foreach (int selectedRowHandle in selectedRowHandles.OrderByDescending(x => x))
            {
                int listIndex = GridControlService.GetListIndexByRowHandle(selectedRowHandle);
                DataRow deleteRow = DataPointsTable.Rows[listIndex];

                foreach(DataColumn column in DataPointsTable.Columns)
                {
                    EntitiesUndoRedoManager.AddUndo(deleteRow, column.ColumnName, deleteRow[column], deleteRow[column], EntityMessageType.Changed);
                }
                EntitiesUndoRedoManager.AddUndo(deleteRow, null, null, null, EntityMessageType.Deleted);

                //GridControlService.RemoveSelectedRows(GridControlService.GetSelectedRowHandles());
                dataPointsTable.Rows.Remove(deleteRow);
            }

            EntitiesUndoRedoManager.UnpauseActionId();

            //devexpress issue
            try
            {
                GridControlService.EndDataUpdate();
            }
            catch
            {

            }
        }

        public void CustomUnboundColumnData(GridColumnDataEventArgs e)
        {
            if(dataPointsTable != null && dataPointsTable.Rows.Count > 0)
            {
                if (e.Column.FieldName == columnDuplicate)
                {
                    if(e.ListSourceRowIndex < dataPointsTable.Rows.Count)
                    {
                        DataRow row = dataPointsTable.Rows[e.ListSourceRowIndex];
                        object uniqueCodeObj = row[columnUniqueCode];
                        string uniqueCodeStr = uniqueCodeObj == null ? string.Empty : uniqueCodeObj.ToString();

                        IEnumerable<DataRow> dataRowCollection = from DataRow dr in dataPointsTable.Rows
                                                                 select dr;

                        e.Value = (dataRowCollection.Where(x => dataPointsTable.Rows.IndexOf(x) != e.ListSourceRowIndex).Any(x => x[columnUniqueCode] != null && x[columnUniqueCode].ToString() == uniqueCodeStr));
                    }
                }
            }
        }

        public virtual void TimeSheetCellValueChanged(CellValueChangedEventArgs e)
        {
            if (e.RowHandle == GridControl.AutoFilterRowHandle)
                return;

            if (!e.Handled)
            {
                DataRowView dataRowView = (DataRowView)e.Row;
                populateRowUniqueCode(dataRowView.Row);
            }
        }

        public override void ValidateCell(GridCellValidationEventArgs e)
        {
            string fieldName = e.Column.FieldName;
            DataRowView dataRowView = (DataRowView)e.Row;
            DataColumn dataColumn = dataRowView.Row.Table.Columns[fieldName];
            if (e.Value != null)
                dataRowView.Row.SetColumnError(dataColumn, string.Empty);

            EntitiesUndoRedoManager.AddUndo(dataRowView.Row, e.Column.FieldName, dataRowView.Row[dataColumn], e.Value, EntityMessageType.Changed);
            //value is not set yet at this stage
            dataRowView.Row[dataColumn] = e.Value;

            DataRow validateRow = dataRowView.Row;
            validateUserAuth(validateRow);
        }

        public override void ValidateRow(GridRowValidationEventArgs e)
        {
            DataRowView dataRowView = (DataRowView)e.Row;
            DataRow validateRow = dataRowView.Row;

            if(dataRowView.Row.RowState == DataRowState.Detached)
                EntitiesUndoRedoManager.AddUndo(dataRowView.Row, null, null, null, EntityMessageType.Added);

            validateUserAuth(validateRow);
        }

        public bool CanShowValidJobLines()
        {
            return IsTimesheetLoaded && DataPointsTable != null && !isReadingFromExo;
        }

        public override bool CanKeyboardCopy()
        {
            return IsTimesheetLoaded && DataPointsTable != null && !isReadingFromExo;
        }

        public override bool CanKeyboardPaste()
        {
            return IsTimesheetLoaded && DataPointsTable != null && !isReadingFromExo;
        }

        public override bool CanCopyWithHeader()
        {
            return IsTimesheetLoaded && DataPointsTable != null && !isReadingFromExo;
        }

        public override bool CanFullRefresh()
        {
            return IsTimesheetLoaded && DataPointsTable != null && !isReadingFromExo;
        }

        public override bool CanBulkDelete()
        {
            return IsTimesheetLoaded && DataPointsTable != null && !isReadingFromExo;
        }

        public override bool CanSaveLayout()
        {
            if (!IsTimesheetLoaded || DataPointsTable == null || isReadingFromExo)
                return false;

            return base.CanSaveLayout();
        }

        public override bool CanUndo()
        {
            if (EntitiesUndoRedoManager == null || DataPointsTable == null || !IsTimesheetLoaded || isReadingFromExo)
                return false;

            return EntitiesUndoRedoManager.CanUndo();
        }

        public override bool CanRedo()
        {
            if (EntitiesUndoRedoManager == null || DataPointsTable == null || !IsTimesheetLoaded || isReadingFromExo)
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
        /// Function to redo the entity changes
        /// Must be used in conjunction of EntitiesUndoManager
        /// </summary>
        /// <param name="entityProperty">Entity passed over from EntitiesUndoRedo</param>
        public virtual void BulkPropertyRedo(IEnumerable<UndoRedoEntityInfo<DataRow>> entityProperties)
        {
            IEnumerable<UndoRedoEntityInfo<DataRow>> bulkSaveProperties = entityProperties.Where(x => x.MessageType == EntityMessageType.Changed);
            IEnumerable<UndoRedoEntityInfo<DataRow>> bulkAddProperties = entityProperties.Where(x => x.MessageType == EntityMessageType.Added);
            IEnumerable<UndoRedoEntityInfo<DataRow>> bulkDeleteProperties = entityProperties.Where(x => x.MessageType == EntityMessageType.Deleted);

            //use ignore refresh here because it'll be refreshed in basebulksave

            EntitiesUndoRedoManager.PauseActionId();
            foreach (var bulkDeleteProperty in bulkDeleteProperties)
            {
                if(!entityProperties.Any(x => x.ActionId == bulkDeleteProperty.ActionId && x.MessageType == EntityMessageType.Changed))
                {
                    foreach (DataColumn column in DataPointsTable.Columns)
                    {
                        EntitiesUndoRedoManager.AddRedo(bulkDeleteProperty.ChangedEntity, column.ColumnName, bulkDeleteProperty.ChangedEntity[column], bulkDeleteProperty.ChangedEntity[column], EntityMessageType.Changed);
                    }
                }

                DataPointsTable.Rows.Remove(bulkDeleteProperty.ChangedEntity);
            }
            EntitiesUndoRedoManager.UnpauseActionId();

            foreach (var bulkAddProperty in bulkAddProperties)
            {
                DataPointsTable.Rows.Add(bulkAddProperty.ChangedEntity);
            }

            foreach (UndoRedoEntityInfo<DataRow> entityProperty in bulkSaveProperties)
            {
                entityProperty.ChangedEntity[entityProperty.PropertyName] = entityProperty.NewValue;
                if (entityProperty.NewValue != DBNull.Value || entityProperty.OldValue.ToString() == string.Empty)
                    entityProperty.ChangedEntity.SetColumnError(entityProperty.PropertyName, string.Empty);
                else
                    entityProperty.ChangedEntity.SetColumnError(entityProperty.PropertyName, valueNotFoundError);

                validateUserAuth(entityProperty.ChangedEntity);
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
            IEnumerable<UndoRedoEntityInfo<DataRow>> bulkDeleteProperties = entityProperties.Where(x => x.MessageType == EntityMessageType.Added);
            IEnumerable<UndoRedoEntityInfo<DataRow>> bulkAddProperties = entityProperties.Where(x => x.MessageType == EntityMessageType.Deleted);

            //use ignore refresh here because it'll be refreshed in basebulksave

            EntitiesUndoRedoManager.PauseActionId();
            foreach (var bulkDeleteProperty in bulkDeleteProperties)
            {
                if (!entityProperties.Any(x => x.ActionId == bulkDeleteProperty.ActionId && x.MessageType == EntityMessageType.Changed))
                {
                    foreach (DataColumn column in DataPointsTable.Columns)
                    {
                        EntitiesUndoRedoManager.AddRedo(bulkDeleteProperty.ChangedEntity, column.ColumnName, bulkDeleteProperty.ChangedEntity[column], bulkDeleteProperty.ChangedEntity[column], EntityMessageType.Changed);
                    }
                }

                DataPointsTable.Rows.Remove(bulkDeleteProperty.ChangedEntity);
            }

            EntitiesUndoRedoManager.UnpauseActionId();

            foreach (var bulkAddProperty in bulkAddProperties)
            {
                DataPointsTable.Rows.Add(bulkAddProperty.ChangedEntity);
            }

            foreach (UndoRedoEntityInfo<DataRow> entityProperty in bulkSaveProperties)
            {
                entityProperty.ChangedEntity[entityProperty.PropertyName] = entityProperty.OldValue;
                if (entityProperty.OldValue != DBNull.Value || entityProperty.OldValue.ToString() == string.Empty)
                    entityProperty.ChangedEntity.SetColumnError(entityProperty.PropertyName, string.Empty);
                else
                    entityProperty.ChangedEntity.SetColumnError(entityProperty.PropertyName, valueNotFoundError);

                validateUserAuth(entityProperty.ChangedEntity);
            }
        }

        /// <summary>
        /// some commodity code can be associated to multiple cost group so the commodity code assigned should be the same cost group
        /// </summary>
        private void tryFixCommodityCode(DataRow dataRow)
        {
            int? costTypeSeqNo = dataRow[columnCostType] == DBNull.Value ? null : (int?)dataRow[columnCostType];
            int? costGroupSeqNo = dataRow[columnCostGroup] == DBNull.Value ? null : (int?)dataRow[columnCostGroup];

            bool isCostTypeValidToCostGroup = preloadedExoLinesWithCostInfo.Any(x => x.DisciplineId == costGroupSeqNo && x.CommodityId == costTypeSeqNo);
            if(!isCostTypeValidToCostGroup)
            {
                JOB_COSTTYPES findCOSTTYPE = JOB_COSTTYPESCollection.FirstOrDefault(x => x.SEQNO == costTypeSeqNo);
                if (findCOSTTYPE != null)
                {
                    //when we cannot find the cost type to cost group association, attempt to find id on the same code
                    //because the same code can have different id in the database, dictated by the assigned cost group
                    string costTypeCode = findCOSTTYPE.SHORTCODE;
                    ExoTimeAuthorisation findExoLineWithGroupSeqNoAndCostTypeCode = preloadedExoLinesWithCostInfo.FirstOrDefault(x => x.DisciplineId == costGroupSeqNo && x.CommodityCode == costTypeCode);
                    if(findExoLineWithGroupSeqNoAndCostTypeCode != null)
                    {
                        dataRow[columnCostType] = findExoLineWithGroupSeqNoAndCostTypeCode.CommodityId;
                    }
                }
            }
        }

        private void validateUserAuth(DataRow validateRow)
        {
            //if (validateRow[columnResourceSeqNo].ToString() != string.Empty && validateRow[columnJobNo].ToString() != string.Empty && validateRow[columnCostGroup].ToString() != string.Empty && validateRow[columnCostType].ToString() != string.Empty)
            //{
            //    ExoTimeAuthorisation findAuthorisation = exoAuthorisations.Where(x => x.ResourceSeqNo == (int)validateRow[columnResourceSeqNo]).FirstOrDefault(x => x.SubJobNo == (int)validateRow[columnJobNo] && x.DisciplineId == (int)validateRow[columnCostGroup] && x.CommodityId == (int)validateRow[columnCostType]);
            //    if (findAuthorisation == null)
            //        validateRow.SetColumnError(columnResourceSeqNo, "User is not authorised to book");
            //    else
            //        validateRow.SetColumnError(columnResourceSeqNo, string.Empty);
            //}


            bool isValidStockCode = true;
            if (validateRow[columnStockCode].ToString() != string.Empty)
            {
                string stockCode = validateRow[columnStockCode].ToString();
                STOCK_ITEMS stockItem = primeroUnitOfWork.STOCK_ITEMS.FirstOrDefault(x => x.STOCKCODE == stockCode);
                if (stockItem == null)
                {
                    isValidStockCode = false;
                    validateRow.SetColumnError(columnStockCode, "Invalid stock code");
                }
            }

            if (isValidStockCode && validateRow[columnJobNo].ToString() != string.Empty && validateRow[columnCostGroup].ToString() != string.Empty && validateRow[columnCostType].ToString() != string.Empty)
            {
                IEnumerable<ExoTimeAuthorisation> findJobLine = preloadedExoLines.Where(x => x.SubJobNo == (int)validateRow[columnJobNo]);
                
                if (findJobLine.Count() == 0)
                    validateRow.SetColumnError(columnJobNo, "Invalid, please check whether the job exists");
                else
                {
                    //validate variation coe
                    if (validateRow[columnVariationCode].ToString() != string.Empty)
                    {
                        string variationCode = validateRow[columnVariationCode].ToString();
                        if (variationCode != string.Empty)
                        {
                            List<ExoTimeAuthorisation> jobWithVariationCode = findJobLine.Where(x => x.VariationCode != null).Where(x => x.VariationCode.Length > 0).ToList();
                            IEnumerable<ExoTimeAuthorisation> findAuthorisationByVariationCode = findJobLine.Where(x => x.VariationCode != null).Where(x => x.VariationCode.ToUpper().Contains(variationCode.ToUpper()));
                            if (findAuthorisationByVariationCode.Count() == 0)
                                validateRow.SetColumnError(columnVariationCode, "Invalid, please check whether variation code exists on job");
                            else
                                validateRow.SetColumnError(columnVariationCode, string.Empty);
                        }
                        else
                            validateRow.SetColumnError(columnVariationCode, string.Empty);
                    }

                    //validate subjob, cost group and cost type
                    int subJobNo = (int)validateRow[columnJobNo];
                    int costGroupNo = (int)validateRow[columnCostGroup];
                    int costTypeNo = (int)validateRow[columnCostType];
                    IEnumerable<ExoTimeAuthorisation> findJobLineByCostGroupType = preloadedExoLinesWithCostInfo.Where(x => x.SubJobNo == subJobNo && x.DisciplineId == costGroupNo && x.CommodityId == costTypeNo);
                    if (findJobLineByCostGroupType.Count() == 0)
                        validateRow.SetColumnError(columnCostGroup, "Invalid, please check whether cost group and cost type exists on job");
                    else
                    {
                        validateRow.SetColumnError(columnJobNo, string.Empty);
                        validateRow.SetColumnError(columnCostGroup, string.Empty);
                        validateRow.SetColumnError(columnResourceSeqNo, string.Empty);
                    }


                    //IEnumerable<ExoTimeAuthorisation> findAuthorisationByDisciplineId = findJobLine.Where(x => x.DisciplineId == (int)validateRow[columnCostGroup]);
                    //if (findAuthorisationByDisciplineId.Count() == 0)
                    //    validateRow.SetColumnError(columnJobNo, "Invalid, please check whether discipline code exists on job no");
                    //else
                    //{
                    //    IEnumerable<ExoTimeAuthorisation> findAuthorisationByCommodityId = findAuthorisationByDisciplineId.Where(x => x.CommodityId == (int)validateRow[columnCostType]);
                    //    if (findAuthorisationByDisciplineId.Count() == 0)
                    //        validateRow.SetColumnError(columnJobNo, "Invalid, please check whether commodity code exists on discipline code and job no");
                    //    else if(validateRow[columnVariationCode].ToString() != string.Empty)
                    //    {
                    //        string variationCode = validateRow[columnVariationCode].ToString();
                    //        if (variationCode != string.Empty)
                    //        {
                    //            IEnumerable<ExoTimeAuthorisation> findAuthorisationByVariationCode = findAuthorisationByCommodityId.Where(x => x.VariationCode == variationCode);
                    //            if(findAuthorisationByVariationCode.Count() == 0)
                    //                validateRow.SetColumnError(columnJobNo, "Invalid, please check whether variation code exists on commodity code, discipline code and job no");
                    //            else
                    //                validateRow.SetColumnError(columnResourceSeqNo, string.Empty);
                    //        }
                    //        else
                    //            validateRow.SetColumnError(columnResourceSeqNo, string.Empty);
                    //    }
                    //}
                }
            }
        }

        public override string UnifiedValueValidation(BASELINE_ITEM projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }

        public override string UnifiedRowValidation(BASELINE_ITEM projection)
        {
            return string.Empty;
        }

        public IEnumerable<JOBCOST_HDR> JOBCOST_HDRCollection
        {
            get
            {
                var collection = GetEntities<JOBCOST_HDR>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.JOBCODE);
                return collection;
            }
        }

        public IEnumerable<JOBCOST_RESOURCE> JOBCOST_RESOURCECollection
        {
            get
            {
                var collection = GetEntities<JOBCOST_RESOURCE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.RESOURCENAME);
                return collection;
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

        public IEnumerable<JOB_COSTTYPES> JOB_COSTTYPESCollection
        {
            get
            {
                var collection = GetEntities<JOB_COSTTYPES>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.SHORTCODE);
                return collection;
            }
        }
    }
}

