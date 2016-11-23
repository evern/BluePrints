using System;
using System.Linq;
using System.Windows.Controls;
using System.Collections.Generic;
using BluePrints.ViewModels;

namespace BluePrints.Views
{
    public partial class COMMODITYCollectionView : UserControl
    {
        public COMMODITYCollectionView()
        {
            InitializeComponent();
        }

        private void dragDropManager_Drop(object sender, DevExpress.Xpf.Grid.DragDrop.TreeListDropEventArgs e)
        {
            ((COMMODITYCollectionViewModelWrapper)this.DataContext).dragDropManager_Drop(sender, e);
        }
    }
}
