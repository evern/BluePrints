using System;
using System.Linq;
using System.Windows.Controls;
using System.Collections.Generic;
using BluePrints.ViewModels;

namespace BluePrints.Views
{
    public partial class ESTIMATION_ITEMCollectionView : UserControl
    {
        public ESTIMATION_ITEMCollectionView()
        {
            InitializeComponent();
        }

        private void dragDropManager_Drop(object sender, DevExpress.Xpf.Grid.DragDrop.TreeListDropEventArgs e)
        {
            ((ESTIMATION_ITEMSViewModelWrapper)this.DataContext).dragDropManager_Drop(sender, e);
        }
    }
}
