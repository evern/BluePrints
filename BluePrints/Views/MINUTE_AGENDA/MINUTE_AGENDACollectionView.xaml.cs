using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace BluePrints.Views
{
    /// <summary>
    /// Interaction logic for AREACollectionView.xaml
    /// </summary>
    public partial class MINUTE_AGENDACollectionView : UserControl
    {
        private DispatcherTimer sortTimer;
        public MINUTE_AGENDACollectionView()
        {
            InitializeComponent();
            sortTimer = new DispatcherTimer();
            sortTimer.Interval = new TimeSpan(0, 0, 0, 1);
            sortTimer.Tick += sortTimer_Tick;
            sortTimer.Start();
        }

        private void sortTimer_Tick(object sender, EventArgs e)
        {
            sortTimer.Stop();
            GridControlTree.Columns["DisplayNumber"].SortOrder = DevExpress.Data.ColumnSortOrder.Ascending;
        }

        private void PART_Editor_ProcessNewValue(System.Windows.DependencyObject sender, DevExpress.Xpf.Editors.ProcessNewValueEventArgs e)
        {

        }
    }
}