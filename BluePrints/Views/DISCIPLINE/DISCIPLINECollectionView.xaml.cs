using System;
using System.Linq;
using System.Windows.Controls;
using System.Collections.Generic;

namespace BluePrints.Views
{
    public partial class DISCIPLINECollectionView : ViewStateRestoreUserControl
    {
        public DISCIPLINECollectionView()
        {
            InitializeComponent();
            InitializeViewRestoration(gridControl, tableView);
        }
    }
}