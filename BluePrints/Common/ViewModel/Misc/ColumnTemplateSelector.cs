using DevExpress.Xpf.Grid;
using System.Windows;
using System.Windows.Controls;

namespace BluePrints.Common.ViewModel.Misc
{

    public class ColumnDescriptor
    {
        public string FieldName { get; set; }
        public string Header { get; set; }
        public double Width { get; set; }
        public FixedStyle Fixed { get; set; }
        public object HeaderToolTip { get; set; }
        public string Mask { get; set; }
        public SettingsType Settings { get; set; }
    }

    public enum SettingsType { Default, POError, Budget, Number, ForecastPast, ForecastFuture, ForecastChild }
    public class ColumnTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            ColumnDescriptor column = (ColumnDescriptor)item;
            return (DataTemplate)((Control)container).FindResource(column.Settings + "ColumnTemplate");
        }
    }
}
