namespace BluePrints.Views
{
    /// <summary>
    /// Interaction logic for AREACollectionView.xaml
    /// </summary>
    public partial class AREACollectionView : ViewStateRestoreUserControl
    {
        public AREACollectionView()
        {
            InitializeComponent();
            InitializeViewControl(GridControl, tableView);
        }
    }
}