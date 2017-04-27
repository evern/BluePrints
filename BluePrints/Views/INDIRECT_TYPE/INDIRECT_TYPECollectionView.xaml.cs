namespace BluePrints.Views
{
    public partial class INDIRECT_TYPECollectionView : ViewStateRestoreUserControl
    {
        public INDIRECT_TYPECollectionView()
        {
            InitializeComponent();
            InitializeViewControl(GridControl, tableView);
        }
    }
}