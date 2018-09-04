using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Document;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Misc;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using BluePrints.Reports;
using System.IO;
using BluePrints.Common.Reports;
using BaseModel.ViewModel.Dialogs;
using BluePrints.Common.Resources;
using BaseModel.ViewModel.Services;
using DevExpress.Mvvm.DataAnnotations;
using BluePrints.P6EntitiesDataModel;
using DevExpress.Xpf.Core.ConditionalFormatting;
using System.Data;
using System.Windows.Media;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Editors;
using DevExpress.Utils.Filtering;
using System.ComponentModel.DataAnnotations;
using DevExpress.Data.Filtering;
using DevExpress.Xpf.Docking.Base;
using BaseModel.ViewModel.UndoRedo;
using System.Text.RegularExpressions;
using System.Diagnostics;

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

        }

        protected override void addEntitiesLoader()
        {
            base.addEntitiesLoader();
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.FORECASTS, FORECASTProjectionFunc);
        }

        private Func<IRepositoryQuery<FORECAST>, IQueryable<FORECAST>> FORECASTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        public bool IsHidden { get; set; }
        public CriteriaOperator FilterCriteria { get; set; }
        public virtual DateTime EndSelectionDate { get; set; }
        public virtual DateTime StartSelectionDate { get; set; }
        public virtual IEnumerable<string> Subjobs { get; set; }
        IEnumerable<ExoTimeAuthorisation> jobLines { get; set; }
        IPrimeroEntitiesUnitOfWork primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
        IEnumerable<ExoSubJobProjection> exoSubJobs;
        List<string> hiddenColumnFieldNames = new List<string>();
        protected override void resolveParameters(object parameter)
        {
            base.resolveParameters(parameter);
            hiddenColumnFieldNames.Add(columnEntity);
            jobLines = ExoQueries.GetProjectLines(primeroUnitOfWork, loadPROJECT.NUMBER);
            exoSubJobs = ExoQueries.GetNativeExoSubJobProjection(primeroUnitOfWork, loadPROJECT);
            SelectedDataRows = new ObservableCollection<DataRowView>();
            StartSelectionDate = DateTime.Now;
            DetailedData = new List<ExoDataPoint>();
            IsHidden = true;
            doNotApplyBestFit = true;
        }

        protected override void onSummaryCalculateComplete()
        {
            FORECASTCollectionViewModel.SetParentViewModel(this);
            this.RaisePropertyChanged(x => x.DataPointsTable);
        }

        public override void FullRefresh()
        {
            dataPointsTable = null;
            base.FullRefresh();
        }

        private void refreshDataTable()
        {
            dataPointsTable = null;
            this.RaisePropertyChanged(x => x.DataPointsTable);
        }

        #region Data Points Table
        string columnEntity = "Entity";
        string columnOutstanding = "Outstanding";
        DataTable dataPointsTable = null;
        DateTime firstAlignedDataDate;

        public DataTable DataPointsTable
        {
            get
            {
                if (MainViewModel == null || SingleProjectDashboards == null)
                    return null;

                if (dataPointsTable == null)
                {
                    dataPointsTable = new DataTable();
                    TimeSpan interval = new TimeSpan(7, 0, 0, 0);
                    firstAlignedDataDate = ChronologicalHelpers.GenerateFirstAlignedDataDate(liveDesignProgress);

                    IEnumerable<Stats> actualStats = SingleProjectDashboards.Where(x => x.Stats != null && ((SummaryStats)x.Stats).Actual != null).Select(x => ((SummaryStats)x.Stats).Actual);
                    IEnumerable<Stats> materialStats = SingleProjectDashboards.Where(x => x.Stats != null && ((SummaryStats)x.Stats).Material != null).Select(x => ((SummaryStats)x.Stats).Material);
                    IEnumerable<Stats> poStats = SingleProjectDashboards.Where(x => x.Stats != null && ((SummaryStats)x.Stats).PO != null).Select(x => ((SummaryStats)x.Stats).PO);

                    IEnumerable<Stats> remainingStats = SingleProjectDashboards.Where(x => x.Stats != null && x.Stats.Remaining != null).Select(x => x.Stats.Remaining);
                    DateTime lastDataDate = DateTime.Now;
                    if(remainingStats.Count() > 0)
                        lastDataDate = remainingStats.Max(x => x.EndDate);

                    List<ExoSubJobProjection> combinedSubJobs = new List<ExoSubJobProjection>();
                    combinedSubJobs.AddRange(exoSubJobs.Select(x => new ExoSubJobProjection() { SubJob = new PrimeroSubJob() { Code = x.SubJob.Code }, Discipline = new PrimeroDiscipline() { Code = x.Discipline.Code }, Commodity = new PrimeroCommodity() { Code = x.Commodity.Code } }));

                    List<ExoDataPoint> allData = new List<ExoDataPoint>();
                    DetailedData.AddRange(actualStats.SelectMany(x => x.ExoDataPoints));
                    DetailedData.AddRange(materialStats.SelectMany(x => x.ExoDataPoints));

                    allData.AddRange(DetailedData);
                    allData.AddRange(poStats.SelectMany(x => x.ExoDataPoints));

                    List<string> uniqueSubjobs = allData.Select(x => x.Subjob_Name + ";" + x.Discipline_Code + ";" + x.Commodity_Code).Distinct().ToList();
                    foreach(string uniqueSubjob in uniqueSubjobs)
                    {
                        List<string> delimited = uniqueSubjob.Split(';').ToList();
                        string subjobName = delimited[0];
                        string disciplineName = delimited[1];
                        string commodityName = delimited[2];

                        if(!combinedSubJobs.Any(x => x.SubJob.Code == subjobName && x.Discipline.Code == disciplineName && x.Commodity.Code == commodityName))
                        {
                            combinedSubJobs.Add(new ExoSubJobProjection() { SubJob = new PrimeroSubJob() { Code = subjobName }, Discipline = new PrimeroDiscipline() { Code = disciplineName }, Commodity = new PrimeroCommodity() { Code = commodityName } });
                        }
                    }

                    lastDataDate = lastDataDate.AddDays(10 * interval.Days);
                    IEnumerable<DateTime> alignedDataDateCollection = ChronologicalHelpers.GenerateAlignedDatesCollection(firstAlignedDataDate, lastDataDate, interval);
                    dataPointsTable.Columns.Add(columnEntity, typeof(ExoSubJobProjection));
                    dataPointsTable.Columns.Add(columnOutstanding, typeof(decimal));

                    foreach (DateTime alignedDataDate in alignedDataDateCollection)
                    {
                        string columnFieldName = alignedDataDate.Date.ToShortDateString();

                        if (alignedDataDate == liveDesignProgress.DATA_DATE)
                        {
                            DataColumn lastColumn = new DataColumn();
                            lastColumn.ColumnName = columnFieldName;
                            lastColumn.DataType = typeof(decimal);
                            dataPointsTable.Columns.Add(lastColumn);
                        }
                        else
                            dataPointsTable.Columns.Add(columnFieldName, typeof(decimal));
                    }

                    foreach (ExoSubJobProjection entity in combinedSubJobs)
                    {
                        BuildRowStats(entity, false);
                    }

                    TableViewService.ScrollToLast();
                }

                return dataPointsTable;
            }
        }


        public List<ExoDataPoint> DetailedData { get; set; }
        private void BuildRowStats(ExoSubJobProjection entity, bool isUpdate)
        {
            if (dataPointsTable == null)
                return;

            DataRow findExistingOrNewDataRow = (from DataRow dr in dataPointsTable.Rows
                              where ((ExoSubJobProjection)dr[columnEntity]).SubJob.Code == entity.SubJob.Code && ((ExoSubJobProjection)dr[columnEntity]).Discipline.Code == entity.Discipline.Code
                              select dr).FirstOrDefault();

            if (findExistingOrNewDataRow == null)
            {
                //during update the entity must be found
                if (isUpdate)
                    return;

                findExistingOrNewDataRow = dataPointsTable.NewRow();
                IEnumerable<ExoTimeAuthorisation> relevantJobLines = jobLines.Where(x => x.SubJobCode == entity.SubJob.Code && x.DisciplineCode == entity.Discipline.Code);
                entity.ExoBudgetQty = relevantJobLines.Sum(x => x.BudgetQty);
                entity.ExoBudgetCosts = relevantJobLines.Sum(x => x.BudgetCosts);

                findExistingOrNewDataRow[columnEntity] = entity;
                for (int i = 0; i < findExistingOrNewDataRow.ItemArray.Count(); i++)
                {
                    string columnName = dataPointsTable.Columns[i].ColumnName;
                    DateTime parseDateTime;
                    if ((columnName == columnOutstanding) || DateTime.TryParse(columnName, out parseDateTime))
                        findExistingOrNewDataRow[columnName] = 0.00m;
                }

                dataPointsTable.Rows.Add(findExistingOrNewDataRow);
            }
            else if (isUpdate)
            {
                updateForecast(findExistingOrNewDataRow, entity);
                return;
            }

            IEnumerable<DashboardFlatStructure> relevantDashboards = SingleProjectDashboards.Where(x => x.SubjobCode == entity.SubJob.Code && x.DisciplineCode == entity.Discipline.Code && x.CommodityCode == entity.Commodity.Code);

            DashboardFlatStructure findDashboardEntity = null;
            if (relevantDashboards.Count() > 0)
                findDashboardEntity = relevantDashboards.First();
            else
                findDashboardEntity = null;

            if (findDashboardEntity != null)
            {
                if (findDashboardEntity.Stats.Remaining != null && findDashboardEntity.Stats.Remaining.DataPoints != null)
                    foreach (Common.ViewModel.Reporting.DataPoint progress in findDashboardEntity.Stats.Remaining.DataPoints)
                    {
                        string dateField = progress.ProgressDate.Date.ToShortDateString();
                        if (dataPointsTable.Columns.Contains(dateField))
                        {
                            decimal currentCosts = (decimal)findExistingOrNewDataRow[dateField];
                            findExistingOrNewDataRow[dateField] = currentCosts + progress.Costs;
                        }
                    }

                SummaryStats summaryStats = (SummaryStats)findDashboardEntity.Stats;
                if (summaryStats.Actual != null && summaryStats.Actual.DataPoints != null)
                {
                    foreach (Common.ViewModel.Reporting.DataPoint progress in summaryStats.Actual.DataPoints)
                    {
                        string dateField = progress.ProgressDate.Date.ToShortDateString();
                        if (dataPointsTable.Columns.Contains(dateField))
                        {
                            decimal currentCosts = (decimal)findExistingOrNewDataRow[dateField];
                            findExistingOrNewDataRow[dateField] = currentCosts + progress.Costs;
                        }
                    }
                }

                if (summaryStats.Material != null && summaryStats.Material.DataPoints != null)
                {
                    foreach (Common.ViewModel.Reporting.DataPoint progress in summaryStats.Material.DataPoints)
                    {
                        string dateField = progress.ProgressDate.Date.ToShortDateString();
                        if (dataPointsTable.Columns.Contains(dateField))
                        {
                            DateTime parseStartDate = DateTime.Parse(dateField);
                            if (parseStartDate < StartSelectionDate)
                                StartSelectionDate = parseStartDate;

                            DateTime parseEndDate = DateTime.Parse(dateField);
                            if (parseEndDate > EndSelectionDate)
                                EndSelectionDate = parseEndDate;

                            decimal currentCosts = (decimal)findExistingOrNewDataRow[dateField];
                            findExistingOrNewDataRow[dateField] = currentCosts + progress.Costs;
                        }
                    }
                }

                if (summaryStats.PO != null && summaryStats.PO.DataPoints != null)
                {
                    foreach (Common.ViewModel.Reporting.DataPoint progress in summaryStats.PO.DataPoints)
                    {
                        decimal currentCosts = (decimal)findExistingOrNewDataRow[columnOutstanding];
                        findExistingOrNewDataRow[columnOutstanding] = currentCosts + progress.Costs;
                    }
                }
                //newDataRow[breakDownEntity] = exoDataPoints;
            }

            //effectively override remaining
            updateForecast(findExistingOrNewDataRow, entity);
        }

        private void setForecastCellNull(DataRow updateRow, ExoSubJobProjection entity, string fieldName)
        {
            DateTime dateTime;
            if(DateTime.TryParse(fieldName, out dateTime))
            {
                IEnumerable<DashboardFlatStructure> relevantDashboards = SingleProjectDashboards.Where(x => x.SubjobCode == entity.SubJob.Code && x.DisciplineCode == entity.Discipline.Code);
                IEnumerable<Common.ViewModel.Reporting.DataPoint> dataPoints = relevantDashboards.Where(x => x.Stats != null && x.Stats.Remaining != null && x.Stats.Remaining.DataPoints != null).SelectMany(x => x.Stats.Remaining.DataPoints);
                IEnumerable<Common.ViewModel.Reporting.DataPoint> dateSpecificDataPoints = dataPoints.Where(x => x.ProgressDate.Date == dateTime);

                if(DataPointsTable.Columns.Contains(fieldName))
                    updateRow[fieldName] = dateSpecificDataPoints.Sum(x => x.Costs);
            }
        }

        private void updateForecast(DataRow dataRow, ExoSubJobProjection entity)
        {
            IEnumerable<FORECAST> currentRowFORECASTS = FORECASTCollectionViewModel.Entities.Where(x => x.SUBJOB_CODE == entity.SubJob.Code && x.DISCIPLINE_CODE == entity.Discipline.Code);
            foreach (FORECAST currentRowFORECAST in currentRowFORECASTS)
            {
                string dateField = currentRowFORECAST.FORECAST_DATE.ToShortDateString();
                if (dataPointsTable.Columns.Contains(dateField))
                {
                    if (currentRowFORECAST.FORECAST_UNITS != null)
                        dataRow[dateField] = currentRowFORECAST.FORECAST_UNITS;
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

        private void setFilter(DataRowView dataRowView, GridColumn gridColumn)
        {
            if (gridColumn.ReadOnly)
            {
                DateTime parseEndDate;
                if (DateTime.TryParse(gridColumn.ActualColumnChooserHeaderCaption.ToString(), out parseEndDate))
                {
                    ExoSubJobProjection entity = (ExoSubJobProjection)dataRowView[columnEntity];
                    parseEndDate = parseEndDate.AddDays(1).AddSeconds(-1);
                    EndSelectionDate = parseEndDate;
                    StartSelectionDate = parseEndDate.AddDays(-7);
                    if(parseEndDate == firstAlignedDataDate)
                        FilterCriteria = CriteriaOperator.Parse("[Subjob_Name] = '" + entity.SubJob.Code + "' And [Discipline_Code] = '" + entity.Discipline.Code +"' And [ActualDate] <= #" + EndSelectionDate.Year + "-" + EndSelectionDate.Month + "-" + EndSelectionDate.Day + "#");
                    else
                        FilterCriteria = CriteriaOperator.Parse("[Subjob_Name] = '" + entity.SubJob.Code + "' And [Discipline_Code] = '" + entity.Discipline.Code + "' And [ActualDate] > #" + StartSelectionDate.Year + "-" + StartSelectionDate.Month + "-" + StartSelectionDate.Day + "# And [ActualDate] <= #" + EndSelectionDate.Year + "-" + EndSelectionDate.Month + "-" + EndSelectionDate.Day + "#");

                    IsHidden = false;

                    this.RaisePropertyChanged(x => x.FilterCriteria);
                }
                else
                {
                    IsHidden = true;
                }
            }
            else
                IsHidden = true;

            this.RaisePropertyChanged(x => x.IsHidden);
        }

        public void AutoGeneratingPercentageColumns(AutoGeneratingColumnEventArgs e)
        {
            if (!hiddenColumnFieldNames.Any(x => x == e.Column.FieldName))
            {
                DateTime parsedate;
                if (DateTime.TryParse(e.Column.FieldName, out parsedate))
                {
                    if(parsedate < liveDesignProgress.DATA_DATE)
                    {
                        e.Column.CellTemplate = Application.Current.Resources["forecastTemplatePast"] as DataTemplate;
                        e.Column.AllowEditing = DevExpress.Utils.DefaultBoolean.False;
                        e.Column.ReadOnly = true;
                    }
                    else
                        e.Column.CellTemplate = Application.Current.Resources["forecastTemplateFuture"] as DataTemplate;

                    e.Column.FilterPopupMode = FilterPopupMode.CheckedList;
                }
                else
                {
                    e.Column.ReadOnly = true;
                    e.Column.Fixed = FixedStyle.Left;
                }
            }
            else
            {
                e.Column.Visible = false;
            }
        }

        public virtual void PastingFromClipboard(PastingFromClipboardEventArgs e)
        {
            GridControl gridControl = (GridControl)e.Source;
            TableView gridTableView = (TableView)gridControl.View;
            string newValueString = Clipboard.GetText().ToString();
            string[] RowData = DataUtils.ExcelSplit(newValueString).ToArray();
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
                    basePasteData(editing_row, selected_cell.Column, string.Empty, false);
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

                        GridColumn current_column = visible_columns.First(x => x.VisibleIndex == (first_column_visible_index + columnOffset));
                        string columnValue = grouped_results[pasteValueColumnOffset][pasteValueRowOffset];

                        int current_row_visible_index = first_row_visible_index + rowOffset;
                        int current_row_handle = gridControl.GetRowHandleByVisibleIndex(current_row_visible_index);

                        object rowObject = gridControl.GetRow(current_row_handle);
                        if (rowObject == null)
                            continue;

                        DataRowView editing_row_view = (DataRowView)rowObject;
                        DataRow editing_row = editing_row_view.Row;
                        var gg = from c in editing_row.Table.Columns.Cast<DataColumn>()
                                select c.ColumnName;
                        
                        pasteValueColumnOffset += 1;
                        if (pasteValueColumnOffset >= grouped_results.Count)
                            pasteValueColumnOffset = 0;

                        basePasteData(editing_row, current_column, columnValue, false);
                    }

                    pasteValueRowOffset += 1;
                    if (pasteValueRowOffset >= grouped_results[pasteValueColumnOffset].Count)
                        pasteValueRowOffset = 0;
                }
            }

            EntitiesUndoRedoManager.UnpauseActionId();
        }


        private bool basePasteData(DataRow newRow, ColumnBase copyColumn, string pasteData, bool isNewRow)
        {
            ExoSubJobProjection entity = (ExoSubJobProjection)newRow[columnEntity];
            
            if (copyColumn.FieldType == typeof(decimal))
            {
                var rgx = new Regex("[^0-9a-z\\.]");
                var cleanColumnString = rgx.Replace(pasteData, string.Empty);
                decimal decimal_value;
                if (decimal.TryParse(cleanColumnString, out decimal_value))
                {
                    if (!isNewRow)
                        EntitiesUndoRedoManager.AddUndo(newRow, copyColumn.FieldName, newRow[copyColumn.FieldName], decimal_value, EntityMessageType.Changed);

                    newRow[copyColumn.FieldName] = decimal_value;
                    DateTime columnDateTime;
                    if(DateTime.TryParse(copyColumn.FieldName, out columnDateTime))
                    {
                        findExistingOrAddNewForecast(entity, columnDateTime, decimal_value);
                    }
                }
                else
                {
                    if (!isNewRow)
                        EntitiesUndoRedoManager.AddUndo(newRow, copyColumn.FieldName, newRow[copyColumn.FieldName], DBNull.Value, EntityMessageType.Changed);

                    setForecastCellNull(newRow, entity, copyColumn.FieldName);
                    //newRow[dataColumn] = DBNull.Value;
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
            GridControl gridControl = (GridControl)parameter;
            TableView tableView = gridControl.View as TableView;
            EntitiesUndoRedoManager.PauseActionId();
            var selected_cells = tableView.GetSelectedCells();
            foreach (var selected_cell in selected_cells)
            {
                int row_handle = selected_cell.RowHandle;
                DataRowView editing_row_view = (DataRowView)gridControl.GetRow(row_handle);
                DataRow editing_row = editing_row_view.Row;
                DataColumn editing_column = editing_row.Table.Columns[selected_cell.Column.VisibleIndex];
                ExoSubJobProjection entity = (ExoSubJobProjection)editing_row[columnEntity];

                string columnFieldName = selected_cell.Column.FieldName;
                DateTime deleteCellDate;
                if(DateTime.TryParse(columnFieldName, out deleteCellDate))
                {
                    EntitiesUndoRedoManager.AddUndo(editing_row, columnFieldName, editing_row[columnFieldName], null, EntityMessageType.Changed);
                    findExistingOrAddNewForecast(entity, deleteCellDate, null);
                    setForecastCellNull(editing_row, entity, columnFieldName);
                    //editing_row[columnFieldName] = 0.00m;
                }
            }
            EntitiesUndoRedoManager.UnpauseActionId();
        }

        public override void OnAfterAuxiliaryEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
             if (changedType == typeof(FORECAST))
            {
                FORECAST changedFORECAST = FORECASTCollectionViewModel.Entities.FirstOrDefault(x => x.GUID == (Guid)key);
                if (changedFORECAST != null)
                {
                    ExoSubJobProjection findUpdatedEntity = exoSubJobs.FirstOrDefault(x => x.SubJob.Code == changedFORECAST.SUBJOB_CODE && x.Discipline.Code == changedFORECAST.DISCIPLINE_CODE);
                    if(findUpdatedEntity != null)
                    {
                        BuildRowStats(findUpdatedEntity, true);
                    }
                }

                mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertyChanged(x => x.DataPointsTable)));
            }

            base.OnAfterAuxiliaryEntitiesChanged(key, changedType, messageType, sender, isBulkRefresh);
        }

        /// <summary>
        /// Influence column(s) when changes happens in other column
        /// </summary>
        public void CellValueChangedUpdate(CellValueChangedEventArgs e)
        {
            if (e.RowHandle == GridControl.AutoFilterRowHandle)
                return;

            DataRowView dataRowView = (DataRowView)e.Row;
            ExoSubJobProjection entity = (ExoSubJobProjection)dataRowView.Row[columnEntity];
            EntitiesUndoRedoManager.PauseActionId();
            if (e.Column.FieldName.ToUpper().Contains("ENTITY"))
            {
                //entity.Entity.Entity.PRIMARY_TITLE = e.Value.ToString();
                ///MainViewModel.EntitiesUndoRedoManager.AddUndo(entity, columnPrimaryTitle, e.OldValue, e.Value, EntityMessageType.Changed);
                //MainViewModel.Save(entity);
            }
            else
            {
                string fieldName = e.Column.FieldName;
                DateTime dateTime;
                if(DateTime.TryParse(fieldName, out dateTime))
                {
                    decimal? forecastUnits = null;
                    decimal convertUnits = 0;
                    if (e.Value != null && decimal.TryParse(e.Value.ToString(), out convertUnits))
                        forecastUnits = convertUnits;

                    EntitiesUndoRedoManager.AddUndo(dataRowView.Row, fieldName, e.OldValue, forecastUnits, EntityMessageType.Changed);
                    findExistingOrAddNewForecast(entity, dateTime, forecastUnits);
                }
            }

            EntitiesUndoRedoManager.UnpauseActionId();
            e.Handled = true;
        }

        private void findExistingOrAddNewForecast(ExoSubJobProjection entity, DateTime forecastDate, decimal? forecastUnits)
        {
            FORECAST findFORECAST = FORECASTCollectionViewModel.Entities.FirstOrDefault(x => x.FORECAST_DATE == forecastDate.Date && x.SUBJOB_CODE == entity.SubJob.Code && x.DISCIPLINE_CODE == entity.Discipline.Code);
            if(findFORECAST == null)
            {
                FORECAST newFORECAST = new FORECAST();
                newFORECAST.GUID = Guid.Empty;
                newFORECAST.GUID_PROJECT = loadPROJECT.GUID;
                newFORECAST.SUBJOB_CODE = entity.SubJob.Code;
                newFORECAST.DISCIPLINE_CODE = entity.Discipline.Code;
                newFORECAST.COMMODITY_CODE = string.Empty;
                newFORECAST.FORECAST_DATE = forecastDate.Date;
                newFORECAST.FORECAST_UNITS = forecastUnits;
                FORECASTCollectionViewModel.Save(newFORECAST);
            }
            else
            {
                findFORECAST.FORECAST_UNITS = forecastUnits;
                FORECASTCollectionViewModel.Save(findFORECAST);
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
                object oldValue = entityProperty.OldValue;
                ExoSubJobProjection exoSubJob = (ExoSubJobProjection)entityProperty.ChangedEntity[columnEntity];
                if (oldValue == null)
                {
                    setForecastCellNull(entityProperty.ChangedEntity, exoSubJob, entityProperty.PropertyName);
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
                    findExistingOrAddNewForecast(exoSubJob, parseDateTime, oldValueDecimal);
                }
            }

            isBackgroundEdit = false;
        }

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
                if (!entityProperties.Any(x => x.ActionId == bulkDeleteProperty.ActionId && x.MessageType == EntityMessageType.Changed))
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
                object newValue = entityProperty.NewValue;
                ExoSubJobProjection exoSubJob = (ExoSubJobProjection)entityProperty.ChangedEntity[columnEntity];
                if (newValue == null)
                {
                    setForecastCellNull(entityProperty.ChangedEntity, exoSubJob, entityProperty.PropertyName);
                    //newValue = 0.00m;
                }
                else
                    entityProperty.ChangedEntity[entityProperty.PropertyName] = newValue;

                DateTime parseDateTime;
                if (DateTime.TryParse(entityProperty.PropertyName, out parseDateTime))
                {
                    decimal? newValueDecimal = null;
                    if (entityProperty.NewValue != null)
                        newValueDecimal = (decimal)entityProperty.NewValue;
                    findExistingOrAddNewForecast(exoSubJob, parseDateTime, newValueDecimal);
                }
            }

            isBackgroundEdit = false;
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
        #endregion

        #region Entity Wrapper Properties
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
}