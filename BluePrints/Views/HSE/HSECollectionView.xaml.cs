using BaseModel.Misc;
using BluePrints.Common.Projections;
using DevExpress.Xpf.Grid;
using System.Windows;
using System.Windows.Controls;

namespace BluePrints.Views
{
    public partial class HSECollectionView : UserControl
    {
        public HSECollectionView()
        {
            InitializeComponent();
            sortModeList_SelectionChanged();
        }

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.RegisterAttached("IsSelected", typeof(bool), typeof(HSECollectionView), new PropertyMetadata(false));
        public static void SetIsSelected(DependencyObject element, bool value)
        {
            element.SetValue(IsSelectedProperty, value);
        }
        public static int GetIsSelected(DependencyObject element)
        {
            return (int)element.GetValue(IsSelectedProperty);
        }

        private void sortModeList_SelectionChanged()
        {
            GridControl.GroupSummarySortInfo.Clear();
            for (int i = 0; i < GridControl.GroupSummary.Count; i++)
            {
                DevExpress.Xpf.Grid.GridSummaryItem item = GridControl.GroupSummary[i];
                SetIsSelected(item, true);
                GridControl.GroupSummarySortInfo.Add(new GridGroupSummarySortInfo(item, "StatsValue", System.ComponentModel.ListSortDirection.Ascending));
            }
        }

        private void GridControl_CustomGroupDisplayText(object sender, CustomGroupDisplayTextEventArgs e)
        {
            //HSEReportProjection rowData = (HSEReportProjection)e.Row;
            //if(e.Column.FieldName == "StatsName" && rowData.StatsName == "Daily Pre-Start Meetings")
            //{
            //    e.DisplayText = ((int)e.Value).ToString("P0");
            //}
        }
    }
}