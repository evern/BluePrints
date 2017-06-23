using BaseModel.Misc;

namespace BluePrints.Views
{
    /// <summary>
    /// Interaction logic for AREADetailsCollectionView.xaml
    /// </summary>
    public partial class COMMODITY_CODECollectionView : ViewStateRestoreUserControl
    {
        public COMMODITY_CODECollectionView()
        {
            InitializeComponent();
            InitializeViewControl(GridControl, tableView);
        }
    }
}