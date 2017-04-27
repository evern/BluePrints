namespace BluePrints.Views
{
    /// <summary>
    /// Interaction logic for PROJECTWORKPACKDetailsCollectionView.xaml
    /// </summary>
    public partial class PROGRESSCollectionView : ViewStateRestoreUserControl
    {
        public PROGRESSCollectionView()
        {
            InitializeComponent();
            InitializeViewControl(GridControl, tableView);
        }
    }
}