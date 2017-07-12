using BaseModel.Misc;
using BluePrints.ViewModels;

namespace BluePrints.Views
{
    public partial class OffsiteDirectVariationCollectionView : ViewStateRestoreUserControl
    {
        public OffsiteDirectVariationCollectionView()
        {
            InitializeComponent();
            InitializeViewControl(GridControl, tableView);
        }
    }
}