using System;
using System.Linq;
using System.Windows.Controls;
using System.Collections.Generic;
using DevExpress.Xpf.Grid;
using BluePrints.ViewModels;

namespace BluePrints.Views
{
    public partial class ROLECollectionView : UserControl
    {
        public ROLECollectionView()
        {
            InitializeComponent();
        }

        private void dragDropManager_Dropped(object sender, DevExpress.Xpf.Grid.DragDrop.TreeListDroppedEventArgs e)
        {
            ((ROLECollectionViewModelWrapper)this.DataContext).dragDropManager_Dropped(sender, e);
        }
    }
}
