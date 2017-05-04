using BaseModel.Misc;

namespace BluePrints.Views
{
    public partial class PROGRESS_ITEMCollectionView : ViewStateRestoreUserControl
    {
        public PROGRESS_ITEMCollectionView()
        {
            InitializeComponent();
            InitializeViewControl(GridControl, tableView);
        }
    }
}