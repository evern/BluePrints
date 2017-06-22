using BaseModel.Misc;

namespace BluePrints.Views
{
    public partial class USER_PROGRESS_ITEMCollectionView : ViewStateRestoreUserControl
    {
        public USER_PROGRESS_ITEMCollectionView()
        {
            InitializeComponent();
            InitializeViewControl(GridControl, tableView);
        }
    }
}