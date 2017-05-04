using BaseModel.Misc;

namespace BluePrints.Views
{
    public partial class USERCollectionView : ViewStateRestoreUserControl
    {
        public USERCollectionView()
        {
            InitializeComponent();
            InitializeViewControl(GridControl, tableView);
        }
    }
}