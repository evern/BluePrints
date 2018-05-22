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
        private readonly IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private readonly IPrimeroEntitiesUnitOfWork primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
        string columnResourceSeqNo = "Resource_SeqNo";
        string columnJobNo = "JobNo";
        string columnCostGroup = "CostGroup";
        string columnCostType = "CostType";
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<Data.PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
            GetDateRange();

            defaultColumnFieldNames.Add(columnResourceSeqNo);
            defaultColumnFieldNames.Add(columnJobNo);
            defaultColumnFieldNames.Add(columnCostGroup);
            defaultColumnFieldNames.Add(columnCostType);

            systemColumnFieldNames.Add(columnResourceSeqNo);
            systemColumnFieldNames.Add(columnJobNo);
            systemColumnFieldNames.Add(columnCostGroup);
            systemColumnFieldNames.Add(columnCostType);
        }

        public FilterTreeViewModel<BASELINE_ITEMProgress, Guid> FilterTreeViewModel { get; set; }
        protected override void initializeEntitiesLoadersDescription()
        {
            MainViewModel = null;
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription<JOBCOST_RESOURCE, JOBCOST_RESOURCE, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOBCOST_RESOURCE);
            loaderCollection.AddLoaderDescription<JOB_COSTGROUPS, JOB_COSTGROUPS, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOB_COSTGROUPS);
            loaderCollection.AddLoaderDescription<JOB_COSTTYPES, JOB_COSTTYPES, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOB_COSTTYPES);
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
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEM>> specifyMainViewModelProjection()
        {
            return query => new List<BASELINE_ITEM>().AsQueryable();
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<BASELINE_ITEM> entities)
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

        public void AutoGeneratingPercentageColumns(AutoGeneratingColumnEventArgs e)
        {
            if (!defaultColumnFieldNames.Any(x => x == e.Column.FieldName))
            {
                SpinEditSettings spinEdit = new SpinEditSettings();
                spinEdit.MaskType = MaskType.Numeric;
                spinEdit.Mask = "n";
                spinEdit.MaskUseAsDisplayFormat = true;
                spinEdit.MinValue = 0;
                e.Column.EditSettings = spinEdit;
            }
            else
            {
                if (hiddenColumnFieldNames.Any(x => x == e.Column.FieldName) || systemColumnFieldNames.Any(x => x == e.Column.FieldName))
                {
                    e.Column.Visible = false;
                }

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

                    dataPointsTable.Columns.Add(columnResourceSeqNo, typeof(int));
                    dataPointsTable.Columns.Add(columnJobNo, typeof(int));
                    dataPointsTable.Columns.Add(columnCostGroup, typeof(int));
                    dataPointsTable.Columns.Add(columnCostType, typeof(int));

                    foreach (DateTime alignedDataDate in alignedDataDateCollection)
                    {
                        string columnFieldName = alignedDataDate.Date.ToShortDateString();
                        dataPointsTable.Columns.Add(columnFieldName, typeof(decimal));
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

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            //get { return "OffsiteDirectProgressViewModelWrapper" + view_project_specific_affix; }
            get { return "TimesheetEntryViewModelWrapper_v1" + view_project_specific_affix; }
        }

        private DevExpress.Mvvm.IDialogService DateFromToDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("DateFromToDialogService"); }
        }

        public void GetDateRange()
        {
            var dateFromToViewModel = DateFromToDialogViewModel.Create();
            if (DateFromToDialogService.ShowDialog(MessageButton.OKCancel, "Select Date Range to Query", "DateFromTo", dateFromToViewModel) == MessageResult.OK)
            {
                DateFrom = dateFromToViewModel.DateFrom;
                DateTo = dateFromToViewModel.DateTo.AddDays(-1);
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
                    if (!basePasteData(editing_row, editing_column, selected_cell.Column, string.Empty))
                        continue;

                    validateUserAuth(editing_row);
                }
            }
            //if copied only a row
            else if (grouped_results.All(x => x.Count == 1) && (grouped_results.Count == 1 || (selected_cells_groupby_columns.Count() == grouped_results.Count)))
            {
                int column_offset = 0;
                foreach (var selected_column in selected_cells_groupby_columns)
                {
                    int validated_column_offset = column_offset > (grouped_results.Count - 1) ? grouped_results.Count - 1 : column_offset;
                    var paste_value = grouped_results[validated_column_offset];
                    column_offset += 1;
                    //since we've already verified that each column group only has a row
                    string paste_data = paste_value.First();
                    List<GridColumn> visible_columns = gridTableView.VisibleColumns.ToList();

                    foreach (var selected_cell in selected_column.Cells)
                    {
                        //int column_visible_index = selected_cell.Column.VisibleIndex;
                        int row_handle = selected_cell.RowHandle;
                        //GridColumn current_column = visible_columns[column_visible_index];
                        GridColumn current_column = selected_cell.Column;
                        DataRowView editing_row_view = (DataRowView)gridControl.GetRow(row_handle);
                        DataRow editing_row = editing_row_view.Row;
                        DataColumn editing_column = editing_row.Table.Columns[selected_cell.Column.VisibleIndex];
                        if (!basePasteData(editing_row, editing_column, selected_cell.Column, paste_data))
                            continue;

                        validateUserAuth(editing_row);
                    }
                }
            }
            else
            {
                GridCell first_selected_cell = selected_cells.First();
                int first_row_handle = first_selected_cell.RowHandle;
                int first_row_visible_index = gridControl.GetRowVisibleIndexByHandle(first_row_handle);

                List<GridColumn> visible_columns = gridTableView.VisibleColumns.ToList();
                //commented out because not accurate during banded view
                //int first_column_visible_index = first_selected_cell.Column.VisibleIndex;
                int first_column_visible_index = visible_columns.First(x => x.FieldName == first_selected_cell.Column.FieldName).VisibleIndex;

                for (int i = 0; i < grouped_results.Count; i++)
                {
                    GridColumn current_column = visible_columns[first_column_visible_index + i];
                    string column_name = current_column.FieldName;
                    int row_visible_index_offset = 0;

                    foreach (string rowValue in grouped_results[i])
                    {
                        int current_row_visible_index = first_row_visible_index + row_visible_index_offset;
                        int current_row_handle = gridControl.GetRowHandleByVisibleIndex(current_row_visible_index);
                        DataRowView editing_row_view = (DataRowView)gridControl.GetRow(current_row_handle);
                        DataRow editing_row = editing_row_view.Row;
                        DataColumn editing_column = editing_row.Table.Columns[current_column.VisibleIndex];
                        if (editing_row == null)
                        {
                            MessageBoxService.ShowMessage("Please remove all line break from paste data or double click into cell to paste your data with line breaks");
                            break;
                        }

                        row_visible_index_offset += 1;
                        if (!basePasteData(editing_row, editing_column, current_column, rowValue))
                            continue;

                        validateUserAuth(editing_row);
                    }
                }
            }
        }

        private void pasteRowData(TableView gridTableView, string[] RowData)
        {
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
                    if (!basePasteData(newRow, newRow.Table.Columns[i], copyColumn, pasteData))
                        continue;
                }

                validateUserAuth(newRow);
                DataPointsTable.Rows.Add(newRow);
            }
        }

        private bool basePasteData(DataRow newRow, DataColumn dataColumn, ColumnBase copyColumn, string pasteData)
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
                        newRow[dataColumn] = int_value;
                    else
                    {
                        newRow.SetColumnError(dataColumn, "Value not found");
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
                    newRow[dataColumn] = decimal_value;
                }
                else
                {
                    newRow.SetColumnError(dataColumn, "Invalid value");
                    return false;
                }
            }

            return true;
        }

        public bool CanBulkDelete()
        {
            return GridControlService.GetSelectedRows().Count > 0;
        }

        public void BulkDelete()
        {
            GridControlService.RemoveSelectedRows();
        }

        public void ValidateCell(GridCellValidationEventArgs e)
        {
            string fieldName = e.Column.FieldName;
            DataRowView dataRowView = (DataRowView)e.Row;
            DataColumn dataColumn = dataRowView.Row.Table.Columns[fieldName];
            if (e.Value != null)
                dataRowView.Row.SetColumnError(dataColumn, string.Empty);

            //value is not set yet at this stage
            dataRowView.Row[dataColumn] = e.Value;

            DataRow validateRow = dataRowView.Row;
            validateUserAuth(validateRow);
        }

        public virtual void ValidateRow(GridRowValidationEventArgs e)
        {
            DataRowView dataRowView = (DataRowView)e.Row;
            DataRow validateRow = dataRowView.Row;
            validateUserAuth(validateRow);
        }

        private void validateUserAuth(DataRow validateRow)
        {
            if (validateRow[columnResourceSeqNo].ToString() != string.Empty && validateRow[columnJobNo].ToString() != string.Empty && validateRow[columnCostGroup].ToString() != string.Empty && validateRow[columnCostType].ToString() != string.Empty)
            {
                ExoTimeAuthorisation findAuthorisation = exoAuthorisations.Where(x => x.ResourceSeqNo == (int)validateRow[columnResourceSeqNo]).FirstOrDefault(x => x.SubJobNo == (int)validateRow[columnJobNo] && x.DisciplineId == (int)validateRow[columnCostGroup] && x.CommodityId == (int)validateRow[columnCostType]);
                if (findAuthorisation == null)
                    validateRow.SetColumnError(0, "User is not authorised to book");
                else
                    validateRow.SetColumnError(0, string.Empty);

                findAuthorisation = exoAuthorisations.FirstOrDefault(x => x.SubJobNo == (int)validateRow[columnJobNo] && x.DisciplineId == (int)validateRow[columnCostGroup] && x.CommodityId == (int)validateRow[columnCostType]);
                if (findAuthorisation == null)
                    validateRow.SetColumnError(0, "User is not authorised to book");
                else
                    validateRow.SetColumnError(0, string.Empty);
            }

            if (validateRow[columnJobNo].ToString() != string.Empty && validateRow[columnCostGroup].ToString() != string.Empty && validateRow[columnCostType].ToString() != string.Empty)
            {
                ExoTimeAuthorisation findAuthorisation = exoAuthorisations.FirstOrDefault(x => x.SubJobNo == (int)validateRow[columnJobNo] && x.DisciplineId == (int)validateRow[columnCostGroup] && x.CommodityId == (int)validateRow[columnCostType]);
                if (findAuthorisation == null)
                    validateRow.SetColumnError(1, "Current job line doesn't exists");
                else
                    validateRow.SetColumnError(1, string.Empty);
            }
        }

        public override string UnifiedValueValidation(BASELINE_ITEM projection, string field_name, object new_value)
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
