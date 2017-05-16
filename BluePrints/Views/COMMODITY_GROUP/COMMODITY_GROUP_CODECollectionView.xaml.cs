using BaseModel.Misc;
using BluePrints.Common.Projections;
using BluePrints.ViewModels;
using System.Windows.Controls;

namespace BluePrints.Views
{
    public partial class COMMODITY_GROUP_CODECollectionView : ViewStateRestoreUserControl
    {
        public COMMODITY_GROUP_CODECollectionView()
        {
            InitializeComponent();
            InitializeViewControl(GridControl, tableView);
            ((COMMODITY_GROUP_CODECollectionViewModelWrapper)DataContext).SetIsRowExpanded =
                SetIsRowExpanded;
        }

        private void dragDropManager_Drop(object sender, DevExpress.Xpf.Grid.DragDrop.GridDropEventArgs e)
        {
            //((COMMODITY_GROUP_CODECollectionViewModelWrapper) DataContext).dragDropManager_Drop(sender, e);
        }

        public void SetIsRowExpanded(COMMODITY_GROUP_CODEProjection row)
        {
            if (row == null)
                return;

            var rowHandle = GridControl.DataController.FindRowByValue("Entity.GUID", row.Entity.GUID);
            if(rowHandle >= 0)
                GridControl.SetMasterRowExpanded(rowHandle, row.IsExpanded);
        }
    }
}