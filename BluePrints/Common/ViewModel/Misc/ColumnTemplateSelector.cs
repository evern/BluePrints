using DevExpress.Data;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using System;
using System.Windows;
using System.Windows.Controls;

namespace BluePrints.Common.ViewModel.Misc
{
    public class SummaryDescriptor
    {
        public SummaryItemType Type { get; set; }
        public string FieldName { get; set; }
        public string DisplayFormat { get; set; }
    }

    public class ColumnDescriptor
    {
        public ColumnDescriptor()
        {
            Increment = 1;
            Visible = true;
        }

        public string FieldName { get; set; }
        public string Header { get; set; }
        public double Width { get; set; }
        public FixedStyle Fixed { get; set; }
        public object HeaderToolTip { get; set; }
        public string Mask { get; set; }
        public decimal Increment { get; set; }
        public SettingsType Settings { get; set; }
        public bool ReadOnly { get; set; }
        public bool Visible { get; set; }
        public object DataContext { get; set; }
        public string DisplayMember { get; set; }
        public string ValueMember { get; set; }
        public object ItemsSource { get; set; }
        public string NullText { get; set; }
        public UnboundColumnType UnboundType { get; set; }
    }

    public enum SettingsType { Default, Unbound, Collection, DisplayMemberCollection, Text, POError, Budget, Number, Date, ForecastPast, ForecastFuture, ForecastChild, FullCode, StockItem, CommodityCode, JobError, Percent, Cost, Units, Gates, Unforecasted }
    public class ColumnTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            ColumnDescriptor column = (ColumnDescriptor)item;
            return (DataTemplate)((Control)container).FindResource(column.Settings + "ColumnTemplate");
        }
    }
}
