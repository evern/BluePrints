using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using BluePrints.Common.Misc;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel.Reporting;
using DevExpress.Data;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;

namespace BaseModel.ViewModel.Dialogs
{
    public class DistributionSelectViewModel<T>
        where T : IHaveWBSCodeString
    {
        public static DistributionSelectViewModel<T> Create(GridControl gridControl, IList<GridCell> selectedCells, string entityColumn)
        {
            return ViewModelSource.Create(() => new DistributionSelectViewModel<T>(gridControl, selectedCells, entityColumn));
        }

        List<string> hiddenColumnFieldNames = new List<string>();
        decimal curveBeginPercentage = 0;
        public decimal CurveBeginPercentage
        {
            get => curveBeginPercentage;
            set
            {
                curveBeginPercentage = value;
                if(isCompletelyLoaded)
                    distributeUnits(lastMethod);
            }
        }

        public void SpinEditValueChanging(EditValueChangingEventArgs e)
        {
            CurveBeginPercentage = (decimal)e.NewValue;
        }

        bool isCompletelyLoaded = false;
        public DataTable SimulationTable { get; set; }
        string entityColumnName { get; set; }
        GridControl gridControl;

        protected DistributionSelectViewModel(GridControl gridControl, IList<GridCell> selectedCells, string entityColumnName)
        {
            this.entityColumnName = entityColumnName;
            this.gridControl = gridControl;
            curveBeginPercentage = 0;
            hiddenColumnFieldNames.Add(this.entityColumnName);
            var selected_cells_groupby_rows = selectedCells.GroupBy(x => x.RowHandle).Select(group => new { RowIndex = group.Key, Cells = group.ToList() });
            IEnumerable<string> columnNames = selected_cells_groupby_rows.SelectMany(x => x.Cells.Select(y => y.Column.FieldName));
            List<DateTime> columnDates = new List<DateTime>();
            foreach(string columnName in columnNames)
            {
                DateTime parseDateTime;
                if (DateTime.TryParse(columnName, out parseDateTime))
                {
                    columnDates.Add(parseDateTime);
                }
                else
                    return;
            }

            SimulationTable = new DataTable();
            SimulationTable.Columns.Add(entityColumnName, typeof(T));
            columnDates = columnDates.OrderBy(x => x).ToList();
            foreach(DateTime columnDate in columnDates)
            {
                string columnFieldName = columnDate.Date.ToShortDateString();
                if(!SimulationTable.Columns.Contains(columnFieldName))
                    SimulationTable.Columns.Add(columnFieldName, typeof(decimal));
            }

            foreach(var groupedCells in selected_cells_groupby_rows)
            {
                DataRowView editing_row_view = (DataRowView)gridControl.GetRow(groupedCells.RowIndex);
                DataRow editing_row = editing_row_view.Row;
                T job = (T)editing_row[this.entityColumnName];

                foreach(var column in groupedCells.Cells)
                {
                    BuildRowStats(job, column.Column.FieldName, editing_row[column.Column.FieldName]);
                }
            }

            distributeUnits("Equal");
            isCompletelyLoaded = true;
        }

        private void distributeUnits(string method)
        {
            Dictionary<int, string> decimalColumns = getDecimalColumns();
            foreach (DataRow dataRow in SimulationTable.Rows)
            {
                decimal totalUnits = 0;
                foreach (var decimalColumn in decimalColumns)
                {
                    var columnValue = dataRow[decimalColumn.Key];
                    if (columnValue != DBNull.Value)
                        totalUnits += (decimal)dataRow[decimalColumn.Key];
                }

                if(totalUnits > 0)
                {
                    decimal profileA = 0;
                    decimal profileB = 0;
                    decimal periodUnits = 0;
                    if (method == "Equal")
                    {
                        periodUnits = totalUnits / decimalColumns.Count;
                        foreach (var decimalColumn in decimalColumns)
                        {
                            dataRow[decimalColumn.Key] = periodUnits;
                        }
                    }
                    else 
                    {
                        if (method == "Front")
                        {
                            profileA = 0.75m;
                            profileB = 0.25m;
                        }
                        else if(method == "Balanced")
                        {
                            profileA = 0.5m;
                            profileB = 0;
                        }
                        else if (method == "Back")
                        {
                            profileA = 0;
                            profileB = 0.25m;
                        }

                        if(profileA != 0 || profileB != 0)
                        {
                            for(int i = 0;i < decimalColumns.Count;i++)
                            {
                                decimal bellProRate = BellProRata.BetaPer(profileA, profileB, i + 1, decimalColumns.Count, CurveBeginPercentage);
                                var column = decimalColumns.ElementAt(i);
                                dataRow[column.Value] = totalUnits * bellProRate;
                            }
                        }
                    }
                }

            }

            gridControl.RefreshData();
        }

        public string ConvertToPasteData()
        {
            string pasteString = string.Empty;
            Dictionary<int, string> decimalColumns = getDecimalColumns();
            foreach (DataRow dataRow in SimulationTable.Rows)
            {
                foreach (var decimalColumn in decimalColumns)
                {
                    var columnValue = dataRow[decimalColumn.Key];
                    if (columnValue != DBNull.Value)
                        pasteString += ((decimal)columnValue).ToString() + "\t";
                }
                
                //remove \t
                if(pasteString.Length > 2)
                    pasteString = pasteString.Substring(0, pasteString.Length - 2);

                pasteString += "\r\n";
            }

            //remove \r\n
            if(pasteString.Length > 4)
                return pasteString.Substring(0, pasteString.Length - 4);
            return pasteString;
        }

        private Dictionary<int, string> getDecimalColumns()
        {
            Dictionary<int, string> columnDictionary = new Dictionary<int, string>();
            for (int i = 0; i < SimulationTable.Columns.Count; i++)
            {
                DateTime parseDateTime;
                if(DateTime.TryParse(SimulationTable.Columns[i].ColumnName, out parseDateTime))
                {
                    columnDictionary.Add(i, SimulationTable.Columns[i].ColumnName);
                }
            }

            return columnDictionary;
        }

        private DataRow BuildRowStats(T job, string fieldName, object value)
        {
            if (SimulationTable == null)
                return null;

            DataRow findExistingOrNewDataRow = (from DataRow dr in SimulationTable.Rows
                                                where (((T)dr[entityColumnName])).SUBJOB_CODE == job.SUBJOB_CODE && ((T)dr[entityColumnName]).DISCIPLINE_CODE == job.DISCIPLINE_CODE
                                                select dr).FirstOrDefault();
            
            if (findExistingOrNewDataRow == null)
            {
                findExistingOrNewDataRow = SimulationTable.NewRow();
                findExistingOrNewDataRow[entityColumnName] = job;

                SimulationTable.Rows.Add(findExistingOrNewDataRow);
            }

            if(value != DBNull.Value)
                findExistingOrNewDataRow[fieldName] = (decimal)value;

            return findExistingOrNewDataRow;
        }

        string lastMethod = "Equal";
        public void RadioChecked(RoutedEventArgs e)
        {
            lastMethod = ((DevExpress.Xpf.Editors.ListBoxRadioButton)e.Source).Name;
            distributeUnits(lastMethod);
        }

        public void AutoGeneratingPercentageColumns(AutoGeneratingColumnEventArgs e)
        {
            if (!hiddenColumnFieldNames.Any(x => x == e.Column.FieldName))
            {
                DateTime parsedate;
                if (DateTime.TryParse(e.Column.FieldName, out parsedate))
                {
                    e.Column.CellTemplate = Application.Current.Resources["forecastTemplateFuture"] as DataTemplate;
                    e.Column.FilterPopupMode = FilterPopupMode.Excel;
                }
                else
                {
                    e.Column.ReadOnly = true;
                    e.Column.Fixed = FixedStyle.Left;
                }
            }
            else
            {
                e.Cancel = true;
            }
        }
    }
}