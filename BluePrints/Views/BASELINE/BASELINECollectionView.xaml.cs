using BaseModel.Misc;

namespace BluePrints.Views
{
    /// <summary>
    /// Interaction logic for AREADetailsCollectionView.xaml
    /// </summary>
    public partial class BASELINECollectionView : ViewStateRestoreUserControl
    {
        public BASELINECollectionView()
        {
            InitializeComponent();
            InitializeViewControl(GridControl, tableView);
        }
    }
}