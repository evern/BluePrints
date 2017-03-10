using System;
using System.Linq;
using System.Windows.Controls;
using System.Collections.Generic;

namespace BluePrints.Views
{
    public partial class DOCTYPECollectionView : ViewStateRestoreUserControl
    {
        public DOCTYPECollectionView()
        {
            InitializeComponent();
            InitializeViewControl(gridControl, tableView);
        }
    }
}