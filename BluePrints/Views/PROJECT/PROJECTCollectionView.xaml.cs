namespace BluePrints.Views
{
    public partial class PROJECTCollectionView : ViewStateRestoreUserControl
    {
        public PROJECTCollectionView()
        {
            InitializeComponent();
            InitializeViewControl(GridControl, tableView);
        }
    }
}