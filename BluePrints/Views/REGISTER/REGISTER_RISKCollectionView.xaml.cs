using BaseModel.Misc;

namespace BluePrints.Views
{
    /// <summary>
    /// Interaction logic for REGISTERCollectionView.xaml
    /// </summary>
    public partial class REGISTER_RISKCollectionView : ViewStateRestoreUserControl
    {
        public REGISTER_RISKCollectionView()
        {
            InitializeComponent();
            InitializeViewControl(GridControl, tableView);
        }
    }
}