using BaseModel.Misc;
using BluePrints.ViewModels;

namespace BluePrints.Views
{
    public partial class SiteDirectVariationCollectionView : ViewStateRestoreUserControl
    {
        public SiteDirectVariationCollectionView()
        {
            InitializeComponent();
            InitializeViewControl(GridControl, tableView);
        }
    }
}