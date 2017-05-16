using BaseModel.Misc;
using BluePrints.Common.Projections;
using BluePrints.ViewModels;
using System.Windows.Controls;

namespace BluePrints.Views
{
    /// <summary>
    /// Interaction logic for ESTIMATION_DIRECT_ITEMCollectionView.xaml
    /// </summary>
    public partial class ESTIMATION_DIRECT_ITEMCollectionView : ViewStateRestoreUserControl
    {
        public ESTIMATION_DIRECT_ITEMCollectionView()
        {
            InitializeComponent();
            InitializeViewControl(GridControl, tableView);
            ((ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper)DataContext).SetIsRowExpanded =
                SetIsRowExpanded;
        }

        public void SetIsRowExpanded(ESTIMATION_DIRECT_ITEMProjection row)
        {
            if (row == null)
                return;

            var rowHandle = GridControl.DataController.FindRowByValue("Entity.GUID", row.Entity.GUID);
            if (rowHandle >= 0)
                GridControl.SetMasterRowExpanded(rowHandle, row.IsExpanded);
        }

        private void dragDropManager_Drop(object sender, DevExpress.Xpf.Grid.DragDrop.GridDropEventArgs e)
        {
            //((ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper)DataContext).dragDropManager_Drop(sender, e);
        }
    }
}