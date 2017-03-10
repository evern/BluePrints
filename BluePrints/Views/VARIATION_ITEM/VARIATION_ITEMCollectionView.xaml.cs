using System;
using System.Linq;
using System.Windows.Controls;
using System.Collections.Generic;
using BluePrints.ViewModels;
using BluePrints.Common;

namespace BluePrints.Views
{
    public partial class VARIATION_ITEMCollectionView : ViewStateRestoreUserControl
    {
        public VARIATION_ITEMCollectionView()
        {
            InitializeComponent();
            InitializeViewControl(gridControl, tableView);
            ((VARIATION_ITEMSViewModelWrapper) DataContext).ShowWORKPACKInternalName1 =
                ShowWorkpackInternalName1;
            ((VARIATION_ITEMSViewModelWrapper) DataContext).ShowWORKPACKInternalName2 =
                ShowWorkpackInternalName2;
        }

        public void ShowWorkpackInternalName1()
        {
            colWORKPACKInternalName1.Visible = true;
        }

        public void ShowWorkpackInternalName2()
        {
            colWORKPACKInternalName2.Visible = true;
        }
    }
}