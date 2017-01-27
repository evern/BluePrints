using BluePrints.Common.Projections;
using BluePrints.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            ((COMMODITY_CODEMasterDetailViewModelWrapper)this.DataContext).SetIsRowExpanded = this.SetIsRowExpanded;
            ((COMMODITY_CODEMasterDetailViewModelWrapper)this.DataContext).ShowDISCIPLINE = this.ShowDISCIPLINE;
            ((COMMODITY_CODEMasterDetailViewModelWrapper)this.DataContext).ShowDEPARTMENT = this.ShowDEPARTMENT;
            ((COMMODITY_CODEMasterDetailViewModelWrapper)this.DataContext).ShowINDIRECT_TYPE = this.ShowINDIRECT_TYPE;
            ((COMMODITY_CODEMasterDetailViewModelWrapper)this.DataContext).ShowDIRECT_RATES = this.ShowDIRECT_RATES;
            ((COMMODITY_CODEMasterDetailViewModelWrapper)this.DataContext).ShowINDIRECT_RATES = this.ShowINDIRECT_RATES;
        }

        public void SetIsRowExpanded(COMMODITY_CODEMasterDetailProjection row)
        {
            if (row == null)
                return;

            int rowHandle = gridControl.DataController.FindRowByRowValue(row);
            gridControl.SetMasterRowExpanded(rowHandle, row.ISEXPANDED);
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
