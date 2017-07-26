using BaseModel.Misc;
using System.Diagnostics;

namespace BluePrints.Views
{
    /// <summary>
    /// Interaction logic for REGISTERCollectionView.xaml
    /// </summary>
    public partial class REGISTER_ISSUECollectionView : ViewStateRestoreUserControl
    {
        public REGISTER_ISSUECollectionView()
        {
            InitializeComponent();
            InitializeViewControl(GridControl, tableView);
        }
    }
}