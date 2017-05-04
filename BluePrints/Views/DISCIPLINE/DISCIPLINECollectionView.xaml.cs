using BaseModel.Misc;

namespace BluePrints.Views
{
    public partial class DISCIPLINECollectionView : ViewStateRestoreUserControl
    {
        public DISCIPLINECollectionView()
        {
            InitializeComponent();
            InitializeViewControl(GridControl, tableView);
        }
    }
}