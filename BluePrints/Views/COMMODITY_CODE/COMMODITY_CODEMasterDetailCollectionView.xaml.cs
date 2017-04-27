using BluePrints.Common.Projections;
using BluePrints.ViewModels;
using System.Windows.Controls;

namespace BluePrints.Views
{
    /// <summary>
    /// Interaction logic for ESTIMATION_DIRECT_ITEMCollectionView.xaml
    /// </summary>
    public partial class COMMODITY_CODEMasterDetailCollectionView : UserControl
    {
        public COMMODITY_CODEMasterDetailCollectionView()
        {
            InitializeComponent();
            ((COMMODITY_CODEMasterDetailViewModelWrapper) DataContext).SetIsRowExpanded = SetIsRowExpanded;
            ((COMMODITY_CODEMasterDetailViewModelWrapper) DataContext).ShowDISCIPLINE = ShowDISCIPLINE;
            ((COMMODITY_CODEMasterDetailViewModelWrapper) DataContext).ShowDEPARTMENT = ShowDEPARTMENT;
            ((COMMODITY_CODEMasterDetailViewModelWrapper) DataContext).ShowINDIRECT_TYPE = ShowINDIRECT_TYPE;
            ((COMMODITY_CODEMasterDetailViewModelWrapper) DataContext).ShowDIRECT_RATES = ShowDIRECT_RATES;
            ((COMMODITY_CODEMasterDetailViewModelWrapper) DataContext).ShowINDIRECT_RATES = ShowINDIRECT_RATES;
        }

        public void SetIsRowExpanded(COMMODITY_CODEMasterDetailProjection row)
        {
            if (row == null)
                return;

            var rowHandle = GridControl.DataController.FindRowByRowValue(row);
            GridControl.SetMasterRowExpanded(rowHandle, row.ISEXPANDED);
        }

        public void ShowDISCIPLINE()
        {
            colDISCIPLINE.Visible = true;
            colChildDISCIPLINE.Visible = true;
        }

        public void ShowDEPARTMENT()
        {
            colDEPARTMENT.Visible = true;
            colChildDEPARTMENT.Visible = true;
        }

        public void ShowINDIRECT_TYPE()
        {
            colINDIRECT_TYPE.Visible = true;
            colChildINDIRECT_TYPE.Visible = true;
        }

        public void ShowDIRECT_RATES()
        {
            colRATE_SUPPLY.Visible = true;
            colRATE_FREIGHT.Visible = true;
            colHOURS_INSTALL.Visible = true;
            colChildRATE_SUPPLY.Visible = true;
            colChildRATE_FREIGHT.Visible = true;
            colChildHOURS_INSTALL.Visible = true;
        }

        public void ShowINDIRECT_RATES()
        {
            colRATE_PLANT.Visible = true;
            colChildRATE_PLANT.Visible = true;
        }
    }
}