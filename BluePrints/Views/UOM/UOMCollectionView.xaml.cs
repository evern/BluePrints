namespace BluePrints.Views
{
    public partial class UOMCollectionView : ViewStateRestoreUserControl
    {
        public UOMCollectionView()
        {
            InitializeComponent();
            InitializeViewControl(GridControl, tableView);
        }
    }
}