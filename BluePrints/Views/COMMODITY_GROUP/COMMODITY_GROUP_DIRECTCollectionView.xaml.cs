using System;
using System.Linq;
using System.Windows.Controls;
using System.Collections.Generic;
using BluePrints.ViewModels;
using BluePrints.Common.Projections;
using DevExpress.Xpf.Grid;
using System.Windows;
using DevExpress.Xpf.Grid.TreeList;
using BluePrints.Data;

namespace BluePrints.Views
{
    public partial class COMMODITY_GROUP_DIRECTCollectionView : UserControl
    {
        public COMMODITY_GROUP_DIRECTCollectionView()
        {
            InitializeComponent();
            ((COMMODITY_GROUP_DIRECTCollectionViewModelWrapper) DataContext).Redraw = Redraw;
            ((COMMODITY_GROUP_DIRECTCollectionViewModelWrapper) DataContext).SetIsRowExpanded =
                SetIsRowExpanded;
        }

        private void dragDropManager_Drop(object sender, DevExpress.Xpf.Grid.DragDrop.GridDropEventArgs e)
        {
            ((COMMODITY_GROUP_DIRECTCollectionViewModelWrapper) DataContext).dragDropManager_Drop(sender, e);
        }

        public void Redraw()
        {
            gridControl.RefreshData();
        }

        public void SetIsRowExpanded(COMMODITY_GROUP_DIRECTProjection row)
        {
            if (row == null)
                return;

            var rowHandle = gridControl.DataController.FindRowByRowValue(row);
            gridControl.SetMasterRowExpanded(rowHandle, row.ISEXPANDED);
        }

        //private void treeListView1_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        //{
        //    TreeListView view = sender as TreeListView;
        //    TreeListViewHitInfo hi = view.CalcHitInfo(e.OriginalSource as DependencyObject);
        //    if (hi.InRowCell)
        //    {
        //        COMMODITY_CODE selectingCOMMODITY_CODE = (COMMODITY_CODE)gridControl.GetRow(hi.RowHandle);
        //        e.Handled = ((COMMODITY_GROUP_DIRECTCollectionViewModelWrapper)this.DataContext).IsCOMMODITY_CODENotSelectable(selectingCOMMODITY_CODE);
        //    }
        //}
    }
}