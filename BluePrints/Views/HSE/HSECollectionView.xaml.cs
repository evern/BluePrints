using BaseModel.Misc;
using System.Linq;
using BluePrints.Common.Projections;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.PivotGrid;
using DevExpress.Xpf.PivotGrid.Internal;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using DevExpress.Xpf.Editors;
using System.Windows.Data;
using System;
using System.Drawing;
using System.Windows.Media;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;

namespace BluePrints.Views
{
    public partial class HSECollectionView : UserControl
    {
        public HSECollectionView()
        {
            InitializeComponent();
        }

        private void pivotGridControl1_CustomSummary(object sender, PivotCustomSummaryEventArgs e)
        {
            if (e.DataField != fieldStatsValue && e.DataField != fieldStatsTarget) return;

            //if(e.ColumnFieldValue != null)
            //    Debug.Print(e.ColumnFieldValue.ToString());

            if (e.RowFieldValue != null)
            {
                PivotGridControl pivotGridControl = (PivotGridControl)sender;
                IEnumerable<HSEReportProjection> dataSource = (IEnumerable<HSEReportProjection>)pivotGridControl.DataSource;
                HSEReportProjection rowData = dataSource.FirstOrDefault(x => x.StatsName == e.RowFieldValue.ToString());
                if(rowData != null)
                {
                    if (rowData.StatsMask == "P0")
                        e.CustomValue = e.SummaryValue.Average;
                    else
                        e.CustomValue = e.SummaryValue.Summary;
                }
            }
        }
    }

    public class MyConv1 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // ((DevExpress.Xpf.Editors.RangeBaseEdit)(value)).Name
            RangeBaseEdit control = (RangeBaseEdit)value;
            if (control == null) return Colors.White;

            System.Windows.Media.Color returnColor;
            if (control.Name == "INJURIES_REC_LTI" || control.Name == "INJURIES_REC_RWI" || control.Name == "INJURIES_REC_MTI" || control.Name == "INCIDENT_FIRE" || control.Name == "INCIDENT_MAJOR_ENV" || control.Name == "INCIDENT_NOTICE")
                returnColor = (control.Value == 0) ? Colors.LightGreen : Colors.LightSalmon;
            else if (control.Name == "Total_Recordable_Injuries" || control.Name == "INCIDENT_DAM" || control.Name == "INCIDENT_ENV")
                returnColor = control.Value < control.Maximum ? Colors.LightGreen : Colors.LightSalmon;
            else
                returnColor = control.Value >= control.Maximum ? Colors.LightGreen : Colors.LightSalmon;

            return returnColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MyConv2 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // ((DevExpress.Xpf.Editors.RangeBaseEdit)(value)).Name
            RangeBaseEdit control = (RangeBaseEdit)value;
            if (control == null) return Colors.White;

            System.Windows.Media.Color returnColor;
            if (control.Name == "INJURIES_REC_LTI" || control.Name == "INJURIES_REC_RWI" || control.Name == "INJURIES_REC_MTI" || control.Name == "INCIDENT_FIRE" || control.Name == "INCIDENT_MAJOR_ENV" || control.Name == "INCIDENT_NOTICE")
                returnColor = (control.Value == 0) ? Colors.Green : Colors.Salmon;
            else if (control.Name == "Total_Recordable_Injuries" || control.Name == "INCIDENT_DAM" || control.Name == "INCIDENT_ENV")
                returnColor = control.Value < control.Maximum ? Colors.Green : Colors.Salmon;
            else
                returnColor = control.Value >= control.Maximum ? Colors.Green : Colors.Salmon;

            return returnColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CellTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            CellsAreaItem cell = (CellsAreaItem)item;
            if (cell.Value == null)
                return createSpinEdit(false);
            PivotGridControl pivotGridControl = (PivotGridControl)cell.Field.Parent;
            PivotDrillDownDataSource pivotDrillDownDataSource = pivotGridControl.CreateDrillDownDataSource(cell.ColumnIndex, cell.RowIndex);
            PivotDrillDownDataRow pivotDrillDownDataRow = pivotDrillDownDataSource[0];
            string statsMask = pivotDrillDownDataRow["StatsMask"].ToString();
            HSEStatsType statsType = (HSEStatsType)pivotDrillDownDataRow["StatsType"];
            decimal statsTarget = (decimal)pivotDrillDownDataRow["StatsTarget"];

            if (cell.ColumnValueDisplayText.ToString() == "Target")
            {
                if (statsMask == "P0")
                    return createSpinEdit(true);
                else
                    return createSpinEdit(false);
            }
            else
            {
                if (statsType == HSEStatsType.INJURIES_REC_LTI || statsType == HSEStatsType.INJURIES_REC_RWI || statsType == HSEStatsType.INJURIES_REC_MTI || statsType == HSEStatsType.INCIDENT_FIRE || statsType == HSEStatsType.INCIDENT_MAJOR_ENV || statsType == HSEStatsType.INCIDENT_NOTICE)
                    return createProgressBarMaximumEdit(statsType.ToString(), false);
                else if(statsType == HSEStatsType.KPI_NM || statsType == HSEStatsType.KPI_TAKE5)
                    return createSpinEdit(false);
                else
                {
                    //grand total column
                    if(cell.IsTotalAppearance)
                    {
                        object test = pivotGridControl.GetCellValue(cell.ColumnIndex + 1, cell.RowIndex);
                        decimal target = 0;
                        decimal.TryParse(test == null ? "" : test.ToString(), out target);
                        if (statsMask == "P0")
                            return createProgressBarEdit(statsType.ToString(), 0, target, true);
                        else
                            return createProgressBarEdit(statsType.ToString(), 0, target, false);
                    }
                    else
                    {
                        if (statsMask == "P0")
                            return createProgressBarEdit(statsType.ToString(), 0, statsTarget, true);
                        else
                            return createProgressBarEdit(statsType.ToString(), 0, statsTarget, false);
                    }
                }
            }
        }

        public DataTemplate createProgressBarMaximumEdit(string controlName, bool isPercentage)
        {
            var progressBarEdit = new FrameworkElementFactory(typeof(ProgressBarEdit));
            progressBarEdit.SetValue(ProgressBarEdit.NameProperty, controlName);
            progressBarEdit.SetBinding(SpinEdit.EditValueProperty, new Binding("Value") { Mode = BindingMode.OneWay });
            progressBarEdit.SetValue(ProgressBarEdit.MaximumProperty, Convert.ToDouble(1));
            progressBarEdit.SetValue(ProgressBarEdit.MinimumProperty, Convert.ToDouble(0));
            if (isPercentage)
                progressBarEdit.SetValue(ProgressBarEdit.DisplayFormatStringProperty, "p0");
            else
                progressBarEdit.SetValue(ProgressBarEdit.DisplayFormatStringProperty, "n0");
            progressBarEdit.SetValue(ProgressBarEdit.ContentDisplayModeProperty, ContentDisplayMode.Value);
            progressBarEdit.SetValue(ProgressBarEdit.EditModeProperty, EditMode.InplaceInactive);
            progressBarEdit.SetValue(ProgressBarEdit.ForegroundProperty, new SolidColorBrush(Colors.Black));
            progressBarEdit.SetValue(ProgressBarEdit.AdditionalForegroundProperty, new SolidColorBrush(Colors.Black));
            DataTemplate newDataTemplate = new DataTemplate() { VisualTree = progressBarEdit };
            return newDataTemplate;
        }

        public DataTemplate createProgressBarEdit(string controlName, decimal minValue, decimal maxValue, bool isPercentage)
        {
            var progressBarEdit = new FrameworkElementFactory(typeof(ProgressBarEdit));
            progressBarEdit.SetValue(ProgressBarEdit.NameProperty, controlName);
            progressBarEdit.SetBinding(SpinEdit.EditValueProperty, new Binding("Value") { Mode = BindingMode.OneWay });
            progressBarEdit.SetValue(ProgressBarEdit.MaximumProperty, Convert.ToDouble(maxValue));
            progressBarEdit.SetValue(ProgressBarEdit.MinimumProperty, Convert.ToDouble(minValue));
            if(isPercentage)
                progressBarEdit.SetValue(ProgressBarEdit.DisplayFormatStringProperty, "p0");
            else
                progressBarEdit.SetValue(ProgressBarEdit.DisplayFormatStringProperty, "n0");
            progressBarEdit.SetValue(ProgressBarEdit.ContentDisplayModeProperty, ContentDisplayMode.Value);
            progressBarEdit.SetValue(ProgressBarEdit.EditModeProperty, EditMode.InplaceInactive);
            progressBarEdit.SetValue(ProgressBarEdit.ForegroundProperty, new SolidColorBrush(Colors.Black));
            progressBarEdit.SetValue(ProgressBarEdit.AdditionalForegroundProperty, new SolidColorBrush(Colors.Black));
            DataTemplate newDataTemplate = new DataTemplate() { VisualTree = progressBarEdit };
            return newDataTemplate;
        }

        public DataTemplate createSpinEdit(bool isPercentage)
        {
            var spinEdit = new FrameworkElementFactory(typeof(SpinEdit));
            spinEdit.SetBinding(SpinEdit.EditValueProperty, new Binding("Value") { Mode = BindingMode.OneWay });
            if (isPercentage)
                spinEdit.SetValue(SpinEdit.MaskProperty, "p0");
            else
                spinEdit.SetValue(SpinEdit.MaskProperty, "n0");
            spinEdit.SetValue(SpinEdit.MaskUseAsDisplayFormatProperty, true);
            spinEdit.SetValue(SpinEdit.EditModeProperty, EditMode.InplaceInactive);
            spinEdit.SetValue(SpinEdit.ShowEditorButtonsProperty, false);
            spinEdit.SetValue(SpinEdit.BackgroundProperty, new SolidColorBrush(Colors.White));
            spinEdit.SetValue(SpinEdit.ForegroundProperty, new SolidColorBrush(Colors.Black));
            DataTemplate newDataTemplate = new DataTemplate() { VisualTree = spinEdit };
            return newDataTemplate;
        }
    }

}