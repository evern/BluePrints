using System;
using System.Linq;
using System.Windows.Controls;
using System.Collections.Generic;
using BluePrints.ViewModels;
using BluePrints.Common;

namespace BluePrints.Views
{
    public partial class VARIATION_ITEMCollectionView : UserControl
    {
        public VARIATION_ITEMCollectionView()
        {
            InitializeComponent();
            ((VARIATION_ITEMSViewModelWrapper)this.DataContext).ShowWORKPACKInternalName1 = this.ShowWorkpackInternalName1;
            ((VARIATION_ITEMSViewModelWrapper)this.DataContext).ShowWORKPACKInternalName2 = this.ShowWorkpackInternalName2;
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
