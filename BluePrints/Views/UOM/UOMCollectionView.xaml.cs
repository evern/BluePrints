using System;
using System.Linq;
using System.Windows.Controls;
using System.Collections.Generic;

namespace BluePrints.Views
{
    public partial class UOMCollectionView : ViewStateRestoreUserControl
    {
        public UOMCollectionView()
        {
            InitializeComponent();
            InitializeViewControl(gridControl, tableView);
        }
    }
}