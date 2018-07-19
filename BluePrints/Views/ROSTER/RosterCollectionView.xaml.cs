using BaseModel.Misc;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.PivotGrid;
using DevExpress.Xpf.PivotGrid.Internal;
using System;
using System.Windows;
using System.Windows.Controls;

namespace BluePrints.Views
{
    public partial class RosterCollectionView : UserControl
    {
        public RosterCollectionView()
        {
            InitializeComponent();
        }
    }

    public class ColumnTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return (DataTemplate)((Control)container).FindResource("ColumnTemplate");
        }
    }
}