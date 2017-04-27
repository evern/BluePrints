namespace BluePrints.Views
{
    /// <summary>
    /// Interaction logic for PHASECollectionView.xaml
    /// </summary>
    public partial class PHASECollectionView : ViewStateRestoreUserControl
    {
        public PHASECollectionView()
        {
            InitializeComponent();
            InitializeViewControl(GridControl, tableView);
        }
    }
}