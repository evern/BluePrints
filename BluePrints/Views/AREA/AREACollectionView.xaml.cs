using BaseModel.Misc;
using BluePrints.Common.Projections;
using BluePrints.ViewModels;

namespace BluePrints.Views
{
    /// <summary>
    /// Interaction logic for AREACollectionView.xaml
    /// </summary>
    public partial class AREACollectionView : ViewStateRestoreUserControl
    {
        public AREACollectionView()
        {
            InitializeComponent();
            InitializeViewControl(GridControl, tableView);
            ((AREACollectionViewModelWrapper)DataContext).SetIsRowExpanded = SetIsRowExpanded;
        }

        public void SetIsRowExpanded(AREAMasterDetailProjection row)
        {
            if (row == null)
                return;

            var rowHandle = GridControl.DataController.FindRowByValue("Entity.GUID", row.Entity.GUID);
            if (rowHandle >= 0)
                GridControl.SetMasterRowExpanded(rowHandle, row.IsExpanded);
        }
    }
}