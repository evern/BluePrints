using System;
using System.Linq;
using System.Windows.Controls;
using System.Collections.Generic;
using BluePrints.ViewModels;

namespace BluePrints.Views
{
    public partial class COMMODITY_GROUP_DIRECTCollectionView : UserControl
    {
        public COMMODITY_GROUP_DIRECTCollectionView()
        {
            InitializeComponent();
        }

        private void dragDropManager_Drop(object sender, DevExpress.Xpf.Grid.DragDrop.GridDropEventArgs e)
        {
            ((COMMODITY_GROUP_DIRECTCollectionViewModelWrapper)this.DataContext).dragDropManager_Drop(sender, e);
        }
    }
}
