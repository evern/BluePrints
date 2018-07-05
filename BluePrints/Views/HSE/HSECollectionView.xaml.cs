using BaseModel.Misc;
using System.Linq;
using BluePrints.Common.Projections;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.PivotGrid;
using DevExpress.Xpf.PivotGrid.Internal;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace BluePrints.Views
{
    public partial class HSECollectionView : UserControl
    {
        public HSECollectionView()
        {
            InitializeComponent();
        }
    }

    public class CellTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            CellsAreaItem cell = (CellsAreaItem)item;
            //// Applies the Default template to the Row Grand Total cells.
            //if (cell.ColumnValue == null)
            //    return ((FrameworkElement)container).FindResource("NormalCellTemplate") as DataTemplate;

            PivotGridControl pivotGridControl = (PivotGridControl)cell.Field.Parent;
            IEnumerable<HSEReportProjection> dataSource = (IEnumerable<HSEReportProjection>)pivotGridControl.DataSource;
            HSEReportProjection rowData = cell.RowValue == null ? null : dataSource.FirstOrDefault(x => x.StatsName == cell.RowValue.ToString());
            if(rowData == null)
                return ((FrameworkElement)container).FindResource("NormalCellTemplate") as DataTemplate;
            else
            {
                if(rowData.StatsMask == "P0")
                    return ((FrameworkElement)container).FindResource("PercentageCellTemplate") as DataTemplate;
                else
                    return ((FrameworkElement)container).FindResource("NormalCellTemplate") as DataTemplate;
            }
            //PivotDrillDownDataSource pivotDrillDownDataSource = pivotGridControl.CreateDrillDownDataSource(cell.ColumnIndex, cell.RowIndex);
            //PivotDrillDownDataRow pivotDrillDownDataRow = pivotDrillDownDataSource[0];
            //string switchValue = pivotDrillDownDataRow["StatsMask"] == null ? "N0" : pivotDrillDownDataRow["StatsMask"].ToString();

            //if(switchValue == "P0")
            //{
            //    return ((FrameworkElement)container).FindResource("PercentageCellTemplate") as DataTemplate;
            //}
            //else if(switchValue == "N0")
            //    return ((FrameworkElement)container).FindResource("NormalCellTemplate") as DataTemplate;
            //else
            //return ((FrameworkElement)container).FindResource("NormalCellTemplate") as DataTemplate;
        }
    }
}