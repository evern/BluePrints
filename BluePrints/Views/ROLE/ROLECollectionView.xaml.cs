using BaseModel.Misc;
using BluePrints.ViewModels;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Settings;
using System;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Threading;

namespace BluePrints.Views
{
    public partial class ROLECollectionView : System.Windows.Controls.UserControl
    {
        private DispatcherTimer sortTimer;
        public ROLECollectionView()
        {
            InitializeComponent();
            sortTimer = new DispatcherTimer();
            sortTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            sortTimer.Tick += sortTimer_Tick;
            sortTimer.Start();
        }

        private void sortTimer_Tick(object sender, EventArgs e)
        {
            sortTimer.Stop();
            GridControl.Columns["Entity.SORTORDER"].SortOrder = DevExpress.Data.ColumnSortOrder.Ascending;
        }

        /// <summary>
        /// Circumvent an issue with checkedit not responsive on treelist because of drag drop manager and cellvaluechanged being implemented
        /// </summary>
        private void treeListControl_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            InplaceBaseEdit inplaceEdit = e.OriginalSource as InplaceBaseEdit;
            if (inplaceEdit != null)
            {
                CheckEditSettings checkEdit = inplaceEdit.Settings as CheckEditSettings;
                if (checkEdit != null)
                    SendKeys.SendWait(" ");
            }
        }
    }
}