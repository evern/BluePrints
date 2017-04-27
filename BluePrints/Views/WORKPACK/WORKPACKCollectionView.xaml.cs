namespace BluePrints.Views
{
    /// <summary>
    /// Interaction logic for WORKPACKCollectionView.xaml
    /// </summary>
    public partial class WORKPACKCollectionView : ViewStateRestoreUserControl
    {
        public WORKPACKCollectionView()
        {
            InitializeComponent();
            InitializeViewControl(GridControl, tableView);
        }
    }
}