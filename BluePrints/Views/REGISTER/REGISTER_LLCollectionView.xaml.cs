using BaseModel.Misc;

namespace BluePrints.Views
{
    /// <summary>
    /// Interaction logic for REGISTERCollectionView.xaml
    /// </summary>
    public partial class REGISTER_LLCollectionView : ViewStateRestoreUserControl
    {
        public REGISTER_LLCollectionView()
        {
            InitializeComponent();
            InitializeViewControl(GridControl, tableView);
        }
    }
}