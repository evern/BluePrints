using BaseModel.Misc;
using DevExpress.Xpf.Grid;
using System.Windows.Controls;

namespace BluePrints.Views
{
    public partial class OffsiteDirectDistributionCollectionView : UserControl
    {
        public OffsiteDirectDistributionCollectionView()
        {
            InitializeComponent();
        }

        private void tableView_ShowGridMenu(object sender, GridMenuEventArgs e)
        {
            var menuInfo = tableView.GridMenu.MenuInfo as GridCellMenuInfo;
            if (menuInfo != null && menuInfo.Row != null && menuInfo.Column != null)
            {
                if(menuInfo.Column.FieldName.Contains("ProgressId"))
                {
                    biFillUp.IsVisible = true;
                    biFillDown.IsVisible = true;
                }
                else
                {
                    biFillUp.IsVisible = false;
                    biFillDown.IsVisible = false;
                }
            }
        }
    }
}