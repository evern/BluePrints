using System;
using System.Linq;
using System.Windows.Controls;
using System.Collections.Generic;

namespace BluePrints.Views
{
    public partial class INDIRECT_TYPECollectionView : ViewStateRestoreUserControl
    {
        public INDIRECT_TYPECollectionView()
        {
            InitializeComponent();
            InitializeViewControl(GridControl, tableView);
        }
    }
}