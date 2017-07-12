using BaseModel.Misc;

namespace BluePrints.Views
{
    public partial class OffsiteDirectProgressCollectionView : ViewStateRestoreUserControl
    {
        public OffsiteDirectProgressCollectionView()
        {
            InitializeComponent();
            InitializeViewControl(GridControl, tableView);
        }
    }
}