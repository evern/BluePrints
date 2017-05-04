using BaseModel.Misc;

namespace BluePrints.Views
{
    public partial class DOCTYPECollectionView : ViewStateRestoreUserControl
    {
        public DOCTYPECollectionView()
        {
            InitializeComponent();
            InitializeViewControl(GridControl, tableView);
        }
    }
}