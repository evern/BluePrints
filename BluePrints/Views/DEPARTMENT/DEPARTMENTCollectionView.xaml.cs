namespace BluePrints.Views
{
    public partial class DEPARTMENTCollectionView : ViewStateRestoreUserControl
    {
        public DEPARTMENTCollectionView()
        {
            InitializeComponent();
            InitializeViewControl(GridControl, tableView);
        }
    }
}