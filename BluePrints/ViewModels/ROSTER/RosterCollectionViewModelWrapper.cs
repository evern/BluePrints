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
using BluePrints.Common.Misc;
using BluePrints.Common.Projections;
using BluePrints.Common.Reports;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
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
    public partial class RosterCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <ROSTER_STAFF, ROSTER_STAFF, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of PROGRESS_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static RosterCollectionViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new RosterCollectionViewModelWrapper());
        }

        #region Database Operation

        List<string> defaultColumnFieldNames = new List<string>();
        private Data.PROJECT loadPROJECT;
        private readonly IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private readonly IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private readonly IPrimeroEntitiesUnitOfWork primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();

        public List<string> predefinedStatusName = new List<string>();
        string columnStaffNo = "EXO_STAFFNO";
        string columnFirstName = "FIRST_NAME";
        string columnLastName = "LAST_NAME";
        string columnTitle = "TITLE";
        string columnDepartmentName = "DEPARTMENT_NAME";
        string columnEmployeeType = "EMPLOYEE_TYPE";

        string valueNotFoundError = "Value not found";

        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<Data.PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
            GetDateRange();

            defaultColumnFieldNames.Add(columnStaffNo);
            defaultColumnFieldNames.Add(columnFirstName);
            defaultColumnFieldNames.Add(columnLastName);
            defaultColumnFieldNames.Add(columnTitle);
            defaultColumnFieldNames.Add(columnDepartmentName);
            defaultColumnFieldNames.Add(columnEmployeeType);

            predefinedStatusName.Add("DIA");
            predefinedStatusName.Add("FIA");
            predefinedStatusName.Add("DOP");
            predefinedStatusName.Add("FOP");
            predefinedStatusName.Add("PER");
            predefinedStatusName.Add("R&R");
        }

        protected override void initializeEntitiesLoadersDescription()
        {
            MainViewModel = null;
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription<JOBCOST_RESOURCE, JOBCOST_RESOURCE, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOBCOST_RESOURCE);
            loaderCollection.AddLoaderDescription<JOB_COSTGROUPS, JOB_COSTGROUPS, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOB_COSTGROUPS);
            loaderCollection.AddLoaderDescription<JOB_COSTTYPES, JOB_COSTTYPES, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOB_COSTTYPES);
            loaderCollection.AddLoaderDescription<STAFF, STAFF, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.STAFF);
            loaderCollection.AddLoaderDescription(primeroUnitOfWorkFactory, x => x.JOBCOST_HDR, JOBCOST_HDRProjectionFunc);
        }

        private Func<IRepositoryQuery<JOBCOST_HDR>, IQueryable<JOBCOST_HDR>> JOBCOST_HDRProjectionFunc()
        {
            return query => query.Where(x => x.JOBCODE.Contains(loadPROJECT.NUMBER.ToString()));
        }

        List<ExoTimeAuthorisation> exoAuthorisations = new List<ExoTimeAuthorisation>();
        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            exoAuthorisations = ExoQueries.GetExoLinesAuthorisations(primeroUnitOfWork, loadPROJECT.NUMBER, false);
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.ROSTER_STAFFS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<ROSTER_STAFF>, IQueryable<ROSTER_STAFF>> specifyMainViewModelProjection()
        {
            return query => new List<ROSTER_STAFF>().AsQueryable();
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<ROSTER_STAFF> entities)
        {
            MainViewModel.SetParentViewModel(this);
            MainViewModel.IsPasteCellLevel = false;
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }
        #region Collection Call Backs

        public override void FullRefresh()
        {
            base.FullRefresh();
            refreshDataPointsTable();
        }
        #endregion

        #endregion

        #region View Properties
        private void refreshDataPointsTable()
        {
            dataPointsTable = null;
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertyChanged(x => x.DataPointsTable)));
        }

        protected override void onAfterRefresh()
        {
            base.onAfterRefresh();
            refreshDataPointsTable();
        }

        public override void OnAfterAuxiliaryEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
            base.OnAfterAuxiliaryEntitiesChanged(key, changedType, messageType, sender, isBulkRefresh);
        }

        public void GenerateJobCodeColumns(AutoGeneratingColumnEventArgs e)
        {
            if (!defaultColumnFieldNames.Any(x => x == e.Column.FieldName))
            {
                ComboBoxEditSettings comboBoxEdit = new ComboBoxEditSettings();
                comboBoxEdit.ItemsSource = SUBJOBCODECollection;
                e.Column.EditSettings = comboBoxEdit;
            }
            else
            {
                e.Column.Fixed = FixedStyle.Left;
            }
        }

        private void reselectDeliverable()
        {
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertyChanged(x => x.DisplaySelectedEntity)));
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

        DataTable dataPointsTable = null;
        public DataTable DataPointsTable
        {
            get
            {
                if (MainViewModel == null || DisplayEntities == null)
                    return null;

                if(dataPointsTable == null)
                {
                    dataPointsTable = new DataTable();
                    TimeSpan interval = new TimeSpan(1, 0, 0, 0);
                    IEnumerable<DateTime> alignedDataDateCollection = ChronologicalHelpers.GenerateAlignedDatesCollection(DateFrom, DateTo, interval);

                    dataPointsTable.Columns.Add(columnStaffNo, typeof(int));
                    dataPointsTable.Columns.Add(columnFirstName, typeof(string));
                    dataPointsTable.Columns.Add(columnLastName, typeof(string));
                    dataPointsTable.Columns.Add(columnTitle, typeof(string));
                    dataPointsTable.Columns.Add(columnDepartmentName, typeof(string));
                    dataPointsTable.Columns.Add(columnEmployeeType, typeof(string));

                    foreach (DateTime alignedDataDate in alignedDataDateCollection)
                    {
                        string columnFieldName = alignedDataDate.Date.ToShortDateString();
                        dataPointsTable.Columns.Add(columnFieldName, typeof(string));
                    }

                    TableViewService.ScrollToLast();
                }

                return dataPointsTable;
            }
            set
            {
                dataPointsTable = value;
            }
        }

        public bool CanReadFromExo()
        {
            return dataPointsTable != null;
        }

        public void ReadFromExo()
        {
            if (MessageBoxService.ShowMessage("Are you sure you want to read hours from exo?\n\nThis will clear the table and replace hours with hours from exo", "Confirmation", MessageButton.OKCancel) != MessageResult.OK)
                return;

            EntitiesUndoRedoManager.Clear();
            DataPointsTable.Clear();

            var result = exoAuthorisations.GroupBy(x => x.SubJobNo)
                   .Select(grp => grp.First())
                   .ToList();

            List<int> jobNumbers = result.Select(x => x.SubJobNo).ToList();
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
            LoadingScreenManager.ShowLoadingScreen(jobNumbers.Count);
            foreach (int jobNumber in jobNumbers)
            {
                List<JOB_TIMESHEETS> timeSheetAllDates = new List<JOB_TIMESHEETS>();
                foreach(TimesheetDate weekStartDate in weekStartDates)
                {
                    IQueryable<JOB_TIMESHEETS> timeSheets = primeroUnitOfWork.JOB_TIMESHEETS.Where(x => x.WEEK_START_DATE == weekStartDate.WeekStartDate && x.JOBNO == jobNumber);
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

                        DataRow newRow = newRows.FirstOrDefault(x => (int)x[columnFirstName] == timeSheet.JOBNO && (int)x[columnStaffNo] == timeSheet.STAFFNO && (int)x[columnLastName] == findCostGroup && (int)x[columnTitle] == findCostType);
                        if(newRow == null)
                        {
                            newRow = DataPointsTable.NewRow();
                            newRow[columnStaffNo] = timeSheet.STAFFNO;
                            newRow[columnFirstName] = timeSheet.JOBNO;
                            if (timeSheet.COST_GROUP == null)
                                newRow[columnLastName] = DBNull.Value;
                            else
                                newRow[columnLastName] = (int)timeSheet.COST_GROUP;

                            if (timeSheet.COST_TYPE == null)
                                newRow[columnTitle] = DBNull.Value;
                            else
                                newRow[columnTitle] = (int)timeSheet.COST_TYPE;

                            newRows.Add(newRow);
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
                                    newRow.SetColumnError(dateColumnName, "Already posted");
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

            GridControlService.RefreshData();
            MessageBoxService.ShowMessage("Data retrieved from exo");
        }

        public TimesheetDate GetTimesheetDate(DateTime bookDate)
        {
            DateTime startOfWeek = bookDate.StartOfWeek(DayOfWeek.Monday);
            int DayNum = (bookDate - startOfWeek).Days + 1;

            return new TimesheetDate() { WeekStartDate = startOfWeek, DayNumber = DayNum };
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
        protected override string ViewName
        {
            //get { return "OffsiteDirectProgressViewModelWrapper" + view_project_specific_affix; }
            get { return "RosterCollectionViewModelWrapper_v1" + view_project_specific_affix; }
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
                DateTo = dateFromToViewModel.DateTo.AddDays(-1);
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


        public virtual void PastingFromClipboard(PastingFromClipboardEventArgs e)
        {
            GridControl gridControl = (GridControl)e.Source;
            TableView gridTableView = (TableView)gridControl.View;
            string newValueString = Clipboard.GetText().ToString();
            string[] RowData = DataUtils.ExcelSplit(newValueString).ToArray();

            if (MainViewModel.SelectMode == MultiSelectMode.Row)
                pasteRowData(gridTableView, RowData);
            else
                pasteCellData(gridControl, gridTableView, RowData);

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
                        validateUserAuth(editing_row);
                    }

                    pasteValueRowOffset += 1;
                    if (pasteValueRowOffset >= grouped_results[pasteValueColumnOffset].Count)
                        pasteValueRowOffset = 0;
                }
            }

            EntitiesUndoRedoManager.UnpauseActionId();
        }

        private void pasteRowData(TableView gridTableView, string[] RowData)
        {
            EntitiesUndoRedoManager.PauseActionId();
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
                EntitiesUndoRedoManager.AddUndo(newRow, null, null, null, EntityMessageType.Added);
            }
            EntitiesUndoRedoManager.UnpauseActionId();
        }

        private bool basePasteData(DataRow newRow, DataColumn dataColumn, ColumnBase copyColumn, string pasteData, bool isNewRow)
        {
            if (copyColumn.FieldType == typeof(int))
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
                        var itemDisplayMemberPropertyInfo =
                            copyColumnItem.GetType().GetProperty(copyColumnDisplayMember);
                        var itemValueMemberPropertyInfo =
                            copyColumnItem.GetType().GetProperty(copyColumnValueMember);
                        if (itemDisplayMemberPropertyInfo.GetValue(copyColumnItem).ToString().ToUpper() == pasteData.ToUpper())
                        {
                            int_value = (int)itemValueMemberPropertyInfo.GetValue(copyColumnItem);
                            break;
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
            }
            else if (copyColumn.FieldType == typeof(decimal))
            {
                var rgx = new Regex("[^0-9a-z\\.]");
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

            return true;
        }

        public bool CanBulkDelete()
        {
            return GridControlService.GetSelectedRowHandles().Count() > 0;
        }

        public void BulkDelete()
        {
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

                DataPointsTable.Rows.Remove(deleteRow);
            }
            EntitiesUndoRedoManager.UnpauseActionId();
            //GridControlService.RemoveSelectedRows(GridControlService.GetSelectedRowHandles());
        }

        public void ValidateCell(GridCellValidationEventArgs e)
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

        public virtual void ValidateRow(GridRowValidationEventArgs e)
        {
            DataRowView dataRowView = (DataRowView)e.Row;
            DataRow validateRow = dataRowView.Row;

            if(dataRowView.Row.RowState == DataRowState.Detached)
                EntitiesUndoRedoManager.AddUndo(dataRowView.Row, null, null, null, EntityMessageType.Added);

            validateUserAuth(validateRow);
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

        bool isBackgroundEdit = false;
        /// <summary>
        /// Function to redo the entity changes
        /// Must be used in conjunction of EntitiesUndoManager
        /// </summary>
        /// <param name="entityProperty">Entity passed over from EntitiesUndoRedo</param>
        public virtual void BulkPropertyRedo(IEnumerable<UndoRedoEntityInfo<DataRow>> entityProperties)
        {
            isBackgroundEdit = true;
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
                //bulkDeleteProperty.ChangedEntity.Delete();
            }
            EntitiesUndoRedoManager.UnpauseActionId();

            foreach (var bulkAddProperty in bulkAddProperties)
            {
                DataPointsTable.Rows.Add(bulkAddProperty.ChangedEntity);
            }

            foreach (UndoRedoEntityInfo<DataRow> entityProperty in bulkSaveProperties)
            {
                entityProperty.ChangedEntity[entityProperty.PropertyName] = entityProperty.NewValue;
                if (entityProperty.NewValue != DBNull.Value)
                    entityProperty.ChangedEntity.SetColumnError(entityProperty.PropertyName, string.Empty);
                else
                    entityProperty.ChangedEntity.SetColumnError(entityProperty.PropertyName, valueNotFoundError);

                validateUserAuth(entityProperty.ChangedEntity);
                //DataUtils.SetNestedValue(entityProperty.PropertyName, entityProperty.ChangedEntity, entityProperty.NewValue);
            }

            isBackgroundEdit = false;
        }

        /// <summary>
        /// Function to undo the entity changes
        /// Must be used in conjunction of EntitiesUndoManager
        /// </summary>
        /// <param name="entityProperty">Entity passed over from EntitiesUndoRedo</param>
        public virtual void BulkPropertyUndo(IEnumerable<UndoRedoEntityInfo<DataRow>> entityProperties)
        {
            isBackgroundEdit = true;
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
                if (entityProperty.OldValue != DBNull.Value)
                    entityProperty.ChangedEntity.SetColumnError(entityProperty.PropertyName, string.Empty);
                else
                    entityProperty.ChangedEntity.SetColumnError(entityProperty.PropertyName, valueNotFoundError);

                validateUserAuth(entityProperty.ChangedEntity);
                //DataUtils.SetNestedValue(entityProperty.PropertyName, entityProperty.ChangedEntity, entityProperty.OldValue);
            }

            isBackgroundEdit = false;
        }

        private void validateUserAuth(DataRow validateRow)
        {
            if (validateRow[columnStaffNo].ToString() != string.Empty && validateRow[columnFirstName].ToString() != string.Empty && validateRow[columnLastName].ToString() != string.Empty && validateRow[columnTitle].ToString() != string.Empty)
            {
                ExoTimeAuthorisation findAuthorisation = exoAuthorisations.Where(x => x.ResourceSeqNo == (int)validateRow[columnStaffNo]).FirstOrDefault(x => x.SubJobNo == (int)validateRow[columnFirstName] && x.DisciplineId == (int)validateRow[columnLastName] && x.CommodityId == (int)validateRow[columnTitle]);
                if (findAuthorisation == null)
                    validateRow.SetColumnError(0, "User is not authorised to book");
                else
                    validateRow.SetColumnError(0, string.Empty);

                findAuthorisation = exoAuthorisations.FirstOrDefault(x => x.SubJobNo == (int)validateRow[columnFirstName] && x.DisciplineId == (int)validateRow[columnLastName] && x.CommodityId == (int)validateRow[columnTitle]);
                if (findAuthorisation == null)
                    validateRow.SetColumnError(0, "User is not authorised to book");
                else
                    validateRow.SetColumnError(0, string.Empty);
            }

            if (validateRow[columnFirstName].ToString() != string.Empty && validateRow[columnLastName].ToString() != string.Empty && validateRow[columnTitle].ToString() != string.Empty)
            {
                ExoTimeAuthorisation findAuthorisation = exoAuthorisations.FirstOrDefault(x => x.SubJobNo == (int)validateRow[columnFirstName] && x.DisciplineId == (int)validateRow[columnLastName] && x.CommodityId == (int)validateRow[columnTitle]);
                if (findAuthorisation == null)
                    validateRow.SetColumnError(1, "Current job line doesn't exists");
                else
                    validateRow.SetColumnError(1, string.Empty);
            }
        }

        public override string UnifiedValueValidation(ROSTER_STAFF projection, string field_name, object new_value)
        {
            return string.Empty;
        }

        public override string UnifiedRowValidation(ROSTER_STAFF projection)
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

        public IEnumerable<STAFF> STAFFCollection
        {
            get
            {
                var collection = GetEntities<STAFF>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
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

        public List<string> subJobCodeCollection;
        public IEnumerable<string> SUBJOBCODECollection
        {
            get
            {
                if (exoAuthorisations == null)
                    return null;

                if (exoAuthorisations.Count == 0)
                    return new List<string>();

                if(subJobCodeCollection == null)
                {
                    subJobCodeCollection = new List<string>();
                    subJobCodeCollection.AddRange(predefinedStatusName);
                    subJobCodeCollection.AddRange(exoAuthorisations.Select(x => x.SubJobCode + "-" + x.DisciplineCode + "-" + x.CommodityCode).Distinct().ToList());
                }

                return subJobCodeCollection;
            }
        }
    }
}

public class RosterCellData
{
    public string Code { get; set; }
    public int SubJobNo { get; set; }
    public int CostGroupNo { get; set; }
    public int CostTypeNo { get; set; }
    public string Comments { get; set; }
}