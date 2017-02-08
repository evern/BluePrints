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
        private DispatcherTimer sortTimer;

        public COMMODITY_CODECollectionView()
        {
            InitializeComponent();
            sortTimer = new DispatcherTimer();
            sortTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            sortTimer.Tick += sortTimer_Tick;
            sortTimer.Start();
            ((COMMODITY_CODESViewModelWrapper) DataContext).ShowDISCIPLINE = ShowDISCIPLINE;
            ((COMMODITY_CODESViewModelWrapper) DataContext).ShowDEPARTMENT = ShowDEPARTMENT;
            ((COMMODITY_CODESViewModelWrapper) DataContext).ShowINDIRECT_TYPE = ShowINDIRECT_TYPE;
            ((COMMODITY_CODESViewModelWrapper) DataContext).ShowDIRECT_RATES = ShowDIRECT_RATES;
            ((COMMODITY_CODESViewModelWrapper) DataContext).ShowINDIRECT_RATES = ShowINDIRECT_RATES;
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

        public void ShowDIRECT_RATES()
        {
            colRATE_SUPPLY.Visible = true;
            colRATE_FREIGHT.Visible = true;
            colHOURS_INSTALL.Visible = true;
        }

        public void ShowINDIRECT_RATES()
        {
            colRATE_PLANT.Visible = true;
        }

        private void sortTimer_Tick(object sender, EventArgs e)
        {
            sortTimer.Stop();
            treeListControl.Columns["SORTORDER"].SortOrder = DevExpress.Data.ColumnSortOrder.Ascending;
        }

        private void dragDropManager_Dropped(object sender, DevExpress.Xpf.Grid.DragDrop.TreeListDroppedEventArgs e)
        {
            ((COMMODITY_CODESViewModelWrapper) DataContext).dragDropManager_Dropped(sender, e);
        }

        private void dragDropManager_Drop(object sender, DevExpress.Xpf.Grid.DragDrop.TreeListDropEventArgs e)
        {
            ((COMMODITY_CODESViewModelWrapper) DataContext).dragDropManager_Drop(sender, e);
        }
    }
}