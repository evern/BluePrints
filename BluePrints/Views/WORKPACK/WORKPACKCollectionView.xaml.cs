using BaseModel.Misc;
using DevExpress.Xpf.Grid;
using System.Windows.Controls;

namespace BluePrints.Views
{
    /// <summary>
    /// Interaction logic for WORKPACKCollectionView.xaml
    /// </summary>
    public partial class WORKPACKCollectionView : UserControl
    {
        public WORKPACKCollectionView()
        {
            InitializeComponent();
        }

        private void tableView_ShowGridMenu(object sender, DevExpress.Xpf.Grid.GridMenuEventArgs e)
        {
            var menuInfo = tableView.GridMenu.MenuInfo as GridCellMenuInfo;
            if (menuInfo != null && menuInfo.Row != null && menuInfo.Column != null)
            {
                if (menuInfo.Row.ControllerVisibleIndex == DataControlBase.NewItemRowHandle)
                {
                    contextMenuDuplicate.IsEnabled = false;
                    contextMenuDuplicateMulti.IsEnabled = false;
                }
                else
                {
                    contextMenuDuplicate.IsEnabled = true;
                    contextMenuDuplicateMulti.IsEnabled = true;
                }

                if (menuInfo.Column == colINTERNAL_NAME)
                {
                    contextMenuPopulate.IsEnabled = true;
                    contextMenuPopulate.Content = "Autofill from Area/DocType/Disc";
                }
                else
                {
                    contextMenuPopulate.IsEnabled = false;
                    contextMenuPopulate.Content = "Autofill";
                }
            }
        }
    }
}