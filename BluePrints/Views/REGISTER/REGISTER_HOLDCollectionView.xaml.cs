using BaseModel.Misc;

namespace BluePrints.Views
{
    /// <summary>
    /// Interaction logic for REGISTERCollectionView.xaml
    /// </summary>
    public partial class REGISTER_HOLDCollectionView : ViewStateRestoreUserControl
    {
        public REGISTER_HOLDCollectionView()
        {
            InitializeComponent();
            InitializeViewControl(GridControl, tableView);
        }
    }
}