using BaseModel.Misc;
using BluePrints.ViewModels;

namespace BluePrints.Views
{
    public partial class VARIATION_ITEMCollectionView : ViewStateRestoreUserControl
    {
        public VARIATION_ITEMCollectionView()
        {
            InitializeComponent();
            InitializeViewControl(GridControl, tableView);
            ((VARIATION_ITEMSCollectionViewModelWrapper) DataContext).ShowWORKPACKInternalName1 =
                ShowWorkpackInternalName1;
            ((VARIATION_ITEMSCollectionViewModelWrapper) DataContext).ShowWORKPACKInternalName2 =
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