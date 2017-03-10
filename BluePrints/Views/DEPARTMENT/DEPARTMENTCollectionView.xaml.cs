using System;
using System.Linq;
using System.Windows.Controls;
using System.Collections.Generic;

namespace BluePrints.Views
{
    public partial class DEPARTMENTCollectionView : ViewStateRestoreUserControl
    {
        public DEPARTMENTCollectionView()
        {
            InitializeComponent();
            InitializeViewControl(gridControl, tableView);
        }
    }
}