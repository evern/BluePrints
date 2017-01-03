using System;
using System.Linq;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Threading;
using BluePrints.ViewModels;

namespace BluePrints.Views
{
    public partial class COMMODITY_CODECollectionView : UserControl
    {
        DispatcherTimer sortTimer;
        public COMMODITY_CODECollectionView()
        {
            InitializeComponent();
            sortTimer = new DispatcherTimer();
            sortTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            sortTimer.Tick += sortTimer_Tick;
            sortTimer.Start();
            ((COMMODITY_CODESViewModelWrapper)this.DataContext).ShowDISCIPLINE = this.ShowDISCIPLINE;
            ((COMMODITY_CODESViewModelWrapper)this.DataContext).ShowDEPARTMENT = this.ShowDEPARTMENT;
            ((COMMODITY_CODESViewModelWrapper)this.DataContext).ShowINDIRECT_TYPE = this.ShowINDIRECT_TYPE;
            ((COMMODITY_CODESViewModelWrapper)this.DataContext).ShowHOURSAWEEK = this.ShowHOURSAWEEK;
        }

        public void ShowDISCIPLINE()
        {
            colDISCIPLINE.Visible = true;
        }

        public void ShowDEPARTMENT()
        {
            colDEPARTMENT.Visible = true;
        }

        public void ShowINDIRECT_TYPE()
        {
            colINDIRECT_TYPE.Visible = true;
        }

        public void ShowHOURSAWEEK()
        {
            colHOURSAWEEK.Visible = true;
        }

        void sortTimer_Tick(object sender, EventArgs e)
        {
            sortTimer.Stop();
            treeListControl.Columns["SORTORDER"].SortOrder = DevExpress.Data.ColumnSortOrder.Ascending;
        }

        private void dragDropManager_Dropped(object sender, DevExpress.Xpf.Grid.DragDrop.TreeListDroppedEventArgs e)
        {
            ((COMMODITY_CODESViewModelWrapper)this.DataContext).dragDropManager_Dropped(sender, e);
        }

        private void dragDropManager_Drop(object sender, DevExpress.Xpf.Grid.DragDrop.TreeListDropEventArgs e)
        {
            ((COMMODITY_CODESViewModelWrapper)this.DataContext).dragDropManager_Drop(sender, e);
        }
    }
}
