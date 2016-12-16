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
    public partial class COMMODITY_GROUP_INDIRECTCollectionView : UserControl
    {
        public COMMODITY_GROUP_INDIRECTCollectionView()
        {
            InitializeComponent();
            ((COMMODITY_GROUP_INDIRECTCollectionViewModelWrapper)this.DataContext).Redraw = this.Redraw;
        }

        private void dragDropManager_Drop(object sender, DevExpress.Xpf.Grid.DragDrop.GridDropEventArgs e)
        {
            ((COMMODITY_GROUP_INDIRECTCollectionViewModelWrapper)this.DataContext).dragDropManager_Drop(sender, e);
        }

        public void Redraw()
        {
            gridControl.RefreshData();
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
