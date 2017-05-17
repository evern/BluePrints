using BaseModel.Misc;

namespace BluePrints.Views
{
    /// <summary>
    /// Interaction logic for REGISTERCollectionView.xaml
    /// </summary>
    public partial class REGISTER_CHANGECollectionView : ViewStateRestoreUserControl
    {
        public REGISTER_CHANGECollectionView()
        {
            InitializeComponent();
            InitializeViewControl(GridControl, tableView);
        }
    }
}