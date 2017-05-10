using BaseModel.Misc;
using BluePrints.ViewModels;
using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace BluePrints.Views
{
    public partial class ROLECollectionView : ViewStateRestoreUserControl
    {
        private DispatcherTimer sortTimer;
        public ROLECollectionView()
        {
            InitializeComponent();
            sortTimer = new DispatcherTimer();
            sortTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            sortTimer.Tick += sortTimer_Tick;
            sortTimer.Start();
            ((ROLECollectionViewModelWrapper)this.DataContext).NativeTreeListRefresh = this.NativeTreeListRefresh;
        }

        private void sortTimer_Tick(object sender, EventArgs e)
        {
            sortTimer.Stop();
            treeListControl.Columns["Entity.SORTORDER"].SortOrder = DevExpress.Data.ColumnSortOrder.Ascending;
        }

        private void NativeTreeListRefresh()
        {
            treeListControl.RefreshData();
            treeListView.ExpandAllNodes();
        }

        private void dragDropManager_Dropped(object sender, DevExpress.Xpf.Grid.DragDrop.TreeListDroppedEventArgs e)
        {
            ((ROLECollectionViewModelWrapper)DataContext).dragDropManager_Dropped(sender, e);
        }

        private void dragDropManager_Drop(object sender, DevExpress.Xpf.Grid.DragDrop.TreeListDropEventArgs e)
        {
            ((ROLECollectionViewModelWrapper)DataContext).dragDropManager_Drop(sender, e);
        }
    }
}