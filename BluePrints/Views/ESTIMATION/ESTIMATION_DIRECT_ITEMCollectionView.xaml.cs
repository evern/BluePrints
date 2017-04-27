using BluePrints.Common.Projections;
using BluePrints.ViewModels;
using System.Windows.Controls;

namespace BluePrints.Views
{
    /// <summary>
    /// Interaction logic for ESTIMATION_DIRECT_ITEMCollectionView.xaml
    /// </summary>
    public partial class ESTIMATION_DIRECT_ITEMCollectionView : UserControl
    {
        public ESTIMATION_DIRECT_ITEMCollectionView()
        {
            InitializeComponent();
            ((ESTIMATION_DIRECT_ITEMSViewModelWrapper) DataContext).SetIsRowExpanded = SetIsRowExpanded;
        }

        public void SetIsRowExpanded(ESTIMATION_DIRECT_ITEMProjection row)
        {
            if (row == null)
                return;

            var rowHandle = GridControl.DataController.FindRowByRowValue(row);
            GridControl.SetMasterRowExpanded(rowHandle, row.ISEXPANDED);
        }
    }
}