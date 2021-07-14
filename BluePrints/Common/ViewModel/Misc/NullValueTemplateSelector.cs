using BluePrints.Common.Filtering;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace BluePrints.Common.ViewModel.Misc
{
    public class NullValueTemplateSelector : DataTemplateSelector
    {
        public string DefaultEditorTemplateName { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            GridCellData data = (GridCellData)item;
            return data.Value == null ? (DataTemplate)((FrameworkElement)container).FindResource("ProgressEditor") : (DataTemplate)((FrameworkElement)container).FindResource(DefaultEditorTemplateName);
        }
    }
}
