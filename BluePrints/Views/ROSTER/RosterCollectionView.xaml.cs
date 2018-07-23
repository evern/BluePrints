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

    public class RosterCellTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            EditGridCellData cellData = item as EditGridCellData;
            if (cellData != null)
            {
            }

            return ((FrameworkElement)container).FindResource("ColumnTemplate") as DataTemplate;
        }
    }
}