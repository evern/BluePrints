using BaseModel.Misc;
using System.Windows.Controls;

namespace BluePrints.Views
{
    public partial class ESTIMATION_DIRECTCollectionView : ViewStateRestoreUserControl
    {
        public ESTIMATION_DIRECTCollectionView()
        {
            InitializeComponent();
            InitializeViewControl(GridControl, tableView);
        }
    }
}