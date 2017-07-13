using BaseModel.Misc;

namespace BluePrints.Views
{
    public partial class User_OffisteDirectProgressCollectionView : ViewStateRestoreUserControl
    {
        public User_OffisteDirectProgressCollectionView()
        {
            InitializeComponent();
            InitializeViewControl(GridControl, tableView);
        }
    }
}