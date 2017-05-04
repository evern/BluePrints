using BaseModel.Misc;

namespace BluePrints.Views
{
    /// <summary>
    /// Interaction logic for PROJECTVARIATIONDetailsCollectionView.xaml
    /// </summary>
    public partial class VARIATIONCollectionView : ViewStateRestoreUserControl
    {
        public VARIATIONCollectionView()
        {
            InitializeComponent();
            InitializeViewControl(GridControl, tableView);
        }
    }
}