using BaseModel.Misc;
using BluePrints.Common.Projections;
using BluePrints.ViewModels;
using System.Windows.Controls;

namespace BluePrints.Views
{
    /// <summary>
    /// Interaction logic for ESTIMATION_DIRECT_ITEMCollectionView.xaml
    /// </summary>
    public partial class ESTIMATION_DIRECT_ITEMCollectionViewOld : UserControl
    {
        public ESTIMATION_DIRECT_ITEMCollectionViewOld()
        {
            InitializeComponent();
            ((ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper) DataContext).SetIsRowExpanded = SetIsRowExpanded;
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