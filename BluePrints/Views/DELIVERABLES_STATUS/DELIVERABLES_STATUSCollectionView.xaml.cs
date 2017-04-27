namespace BluePrints.Views
{
    public partial class DELIVERABLES_STATUSCollectionView : ViewStateRestoreUserControl
    {
        public DELIVERABLES_STATUSCollectionView()
        {
            InitializeComponent();
            InitializeViewControl(GridControl, tableView);
        }
    }
}