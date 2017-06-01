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
        }
    }
}