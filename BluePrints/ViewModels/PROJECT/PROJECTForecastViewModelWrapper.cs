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

        IEnumerable<ExoTimeAuthorisation> jobLines { get; set; }
        IPrimeroEntitiesUnitOfWork primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
        IEnumerable<ExoSubJobProjection> exoSubJobs;
        List<string> defaultColumnFieldNames = new List<string>();
        List<string> hiddenColumnFieldNames = new List<string>();
        protected override void resolveParameters(object parameter)
        {
            base.resolveParameters(parameter);
            defaultColumnFieldNames.Add(columnEntity);
            jobLines = ExoQueries.GetProjectLines(primeroUnitOfWork, loadPROJECT.NUMBER);
            exoSubJobs = ExoQueries.GetNativeExoSubJobProjection(primeroUnitOfWork, loadPROJECT);
            SelectedDataRows = new ObservableCollection<DataRowView>();
        }

        protected override void onSummaryCalculateComplete()
        {
            this.RaisePropertyChanged(x => x.DataPointsTable);
        }

        public override void FullRefresh()
        {
            dataPointsTable = null;
            base.FullRefresh();
        }

        #region Data Points Table
        string columnEntity = "Entity";
        DataTable dataPointsTable = null;
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
                    DateTime firstAlignedDataDate = ChronologicalHelpers.GenerateFirstAlignedDataDate(liveDesignProgress);

                    IEnumerable<Stats> remainingStats = SingleProjectDashboards.Where(x => x.Stats != null && x.Stats.Remaining != null).Select(x => x.Stats.Remaining);
                    DateTime lastDataDate = DateTime.Now;
                    if(remainingStats.Count() > 0)
                        lastDataDate = remainingStats.Max(x => x.EndDate);

                    lastDataDate = lastDataDate.AddDays(10 * interval.Days);
                    IEnumerable<DateTime> alignedDataDateCollection = ChronologicalHelpers.GenerateAlignedDatesCollection(firstAlignedDataDate, lastDataDate, interval);
                    dataPointsTable.Columns.Add(columnEntity, typeof(ExoSubJobProjection));

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

                    foreach (ExoSubJobProjection entity in exoSubJobs)
                    {
                        BuildRowStats(entity, false);
                    }

                    TableViewService.ScrollToLast();
                }

                return dataPointsTable;
            }
        }

        private void BuildRowStats(ExoSubJobProjection entity, bool isAuxiliary)
        {
            if (dataPointsTable == null)
                return;

            DataRow newDataRow = (from DataRow dr in dataPointsTable.Rows
                              where ((ExoSubJobProjection)dr[columnEntity]).SubJob.Code == entity.SubJob.Code && ((ExoSubJobProjection)dr[columnEntity]).Discipline.Code == entity.Discipline.Code
                              select dr).FirstOrDefault();

            bool isUpdate = true;
            if (newDataRow == null)
            {
                isUpdate = false;
                newDataRow = dataPointsTable.NewRow();
            }

            ExoTimeAuthorisation jobLine = jobLines.FirstOrDefault(x => x.SubJobCode == entity.SubJob.Code && x.DisciplineCode == entity.Discipline.Code);
            if (!isAuxiliary && jobLine != null)
            {
                entity.ExoBudgetQty += jobLine.BudgetQty;
                entity.ExoBudgetCosts += jobLine.BudgetCosts;
            }
            newDataRow[columnEntity] = entity;

            for (int i = 0; i < newDataRow.ItemArray.Count(); i++)
            {
                string columnName = dataPointsTable.Columns[i].ColumnName;
                if (!defaultColumnFieldNames.Any(x => x == columnName))
                    newDataRow[columnName] = 0.00m;
            }

            DashboardFlatStructure findDashboardEntity = SingleProjectDashboards.FirstOrDefault(x => x.SubjobCode == entity.SubJob.Code && x.DisciplineCode == entity.Discipline.Code);

            if(findDashboardEntity != null)
            {
                if (findDashboardEntity.Stats.Remaining != null && findDashboardEntity.Stats.Remaining.CumulativeDataPoints != null)
                    foreach (Common.ViewModel.Reporting.DataPoint progress in findDashboardEntity.Stats.Remaining.DataPoints)
                    {
                        string dateField = progress.ProgressDate.Date.ToShortDateString();
                        if (dataPointsTable.Columns.Contains(dateField))
                        {
                            decimal currentCosts = (decimal)newDataRow[dateField];
                            newDataRow[dateField] = currentCosts + progress.Costs;
                        }
                    }

                SummaryStats summaryStats = (SummaryStats)findDashboardEntity.Stats;
                if (summaryStats.Actual != null && summaryStats.Actual.CumulativeDataPoints != null)
                    foreach (Common.ViewModel.Reporting.DataPoint progress in summaryStats.Actual.DataPoints)
                    {
                        string dateField = progress.ProgressDate.Date.ToShortDateString();
                        if (dataPointsTable.Columns.Contains(dateField))
                        {
                            decimal currentCosts = (decimal)newDataRow[dateField];
                            newDataRow[dateField] = currentCosts + progress.Costs;
                        }
                    }
            }

            //effectively override remaining
            IEnumerable<FORECAST> currentRowFORECASTS = FORECASTCollectionViewModel.Entities.Where(x => x.SUBJOB_CODE == entity.SubJob.Code && x.DISCIPLINE_CODE == entity.Discipline.Code);
            foreach(FORECAST currentRowFORECAST in currentRowFORECASTS)
            {
                string dateField = currentRowFORECAST.FORECAST_DATE.ToShortDateString();
                if (dataPointsTable.Columns.Contains(dateField))
                {
                    if(currentRowFORECAST.FORECAST_UNITS != null)
                        newDataRow[dateField] = currentRowFORECAST.FORECAST_UNITS;
                }
            }
            
            if (!isUpdate)
                dataPointsTable.Rows.Add(newDataRow);
        }
        #endregion

        #region View Events
        public void AutoGeneratingPercentageColumns(AutoGeneratingColumnEventArgs e)
        {
            if (!defaultColumnFieldNames.Any(x => x == e.Column.FieldName))
            {
                DateTime parsedate;
                if (DateTime.TryParse(e.Column.FieldName, out parsedate))
                {
                    if(parsedate < liveDesignProgress.DATA_DATE)
                    {
                        e.Column.CellTemplate = Application.Current.Resources["forecastTemplatePast"] as DataTemplate;
                        e.Column.ReadOnly = true;
                    }
                    else
                        e.Column.CellTemplate = Application.Current.Resources["forecastTemplateFuture"] as DataTemplate;

                    e.Column.FilterPopupMode = FilterPopupMode.CheckedList;
                }
                else
                {
                    e.Column.Fixed = FixedStyle.Left;
                }
            }
            else
                e.Column.Visible = false;
        }

        public virtual void PastingFromClipboard(PastingFromClipboardEventArgs e)
        {
            GridControl gridControl = (GridControl)e.Source;
            TableView gridTableView = (TableView)gridControl.View;

            var selected_cells = gridTableView.GetSelectedCells();
            if (selected_cells.Count == 0)
                return;

            string newValueString = Clipboard.GetText().ToString().Replace("%", "");
            List<string> newValueArr = newValueString.Split('\r').ToList();
            if(newValueString.Contains("\t") || newValueArr.Where(x => x == "\n").Count() > 1)
            {
                MessageBoxService.ShowMessage("Grid doesn't support pasting from multiple cells, sorry for the inconvenience");
                return;
            }

            newValueString = newValueArr[0];
            decimal newValueDecimal = 0;
            if (decimal.TryParse(newValueString, out newValueDecimal))
            {
                foreach (var selected_cell in selected_cells)
                {
                    DataRowView editing_row = (DataRowView)gridControl.GetRow(selected_cell.RowHandle);
                    ExoSubJobProjection entity = (ExoSubJobProjection)editing_row.Row[columnEntity];

                    DateTime pasteCellDate;
                    if(DateTime.TryParse(selected_cell.Column.FieldName, out pasteCellDate))
                    {
                        decimal oldValue = (decimal)editing_row[selected_cell.Column.FieldName];
                        findExistingOrAddNewForecast(entity, pasteCellDate, newValueDecimal);
                    }
                }
            }

            e.Handled = true;
        }

        public void DeleteCellContent(object parameter)
        {
            GridControl gridControl = (GridControl)parameter;
            TableView tableView = gridControl.View as TableView;

            var selected_cells = tableView.GetSelectedCells();
            foreach (var selected_cell in selected_cells)
            {
                int row_handle = selected_cell.RowHandle;
                DataRowView editing_row_view = (DataRowView)gridControl.GetRow(row_handle);
                DataRow editing_row = editing_row_view.Row;
                DataColumn editing_column = editing_row.Table.Columns[selected_cell.Column.VisibleIndex];
                ExoSubJobProjection entity = (ExoSubJobProjection)editing_row[columnEntity];

                DateTime deleteCellDate;
                if(DateTime.TryParse(selected_cell.Column.FieldName, out deleteCellDate))
                {
                    findExistingOrAddNewForecast(entity, deleteCellDate, null);
                }
            }
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
                        mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertyChanged(x => x.DataPointsTable)));
                    }
                }
            }

            base.OnAfterAuxiliaryEntitiesChanged(key, changedType, messageType, sender, isBulkRefresh);
        }

        /// <summary>
        /// Influence column(s) when changes happens in other column
        /// </summary>
        public void CellValueChangedProgressUpdate(CellValueChangedEventArgs e)
        {
            if (e.RowHandle == GridControl.AutoFilterRowHandle)
                return;

            DataRowView dataRowView = (DataRowView)e.Row;
            ExoSubJobProjection entity = (ExoSubJobProjection)dataRowView.Row[columnEntity];

            if (e.Column.FieldName.ToUpper().Contains("ENTITY"))
            {
                //entity.Entity.Entity.PRIMARY_TITLE = e.Value.ToString();
                ///MainViewModel.EntitiesUndoRedoManager.AddUndo(entity, columnPrimaryTitle, e.OldValue, e.Value, EntityMessageType.Changed);
                //MainViewModel.Save(entity);
            }
            else
            {
                DateTime dateTime;
                if(DateTime.TryParse(e.Column.FieldName, out dateTime))
                {
                    decimal? forecastUnits = null;
                    decimal convertUnits = 0;
                    if (e.Value != null && decimal.TryParse(e.Value.ToString(), out convertUnits))
                        forecastUnits = convertUnits;

                    findExistingOrAddNewForecast(entity, dateTime, forecastUnits);
                }
            }

            e.Handled = true;
        }

        private void findExistingOrAddNewForecast(ExoSubJobProjection entity, DateTime forecastDate, decimal? forecastUnits)
        {
            FORECAST findFORECAST = FORECASTCollectionViewModel.Entities.FirstOrDefault(x => x.FORECAST_DATE == forecastDate.Date && x.SUBJOB_CODE == entity.SubJob.Code && x.DISCIPLINE_CODE == entity.Discipline.Code);
            if(findFORECAST == null && forecastUnits != null)
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
}