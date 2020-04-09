using BaseModel.Misc;
using BluePrints.ViewModels;
using DevExpress.Xpf.Grid;
using System.Windows.Controls;

namespace BluePrints.Views
{
    public partial class OffsiteDirectVariationCollectionView : UserControl
    {
        public OffsiteDirectVariationCollectionView()
        {
            InitializeComponent();
            ((OffsiteDirectVariationCollectionViewModelWrapper)DataContext).SetViewCanEdit = SetViewCanEdit;
        }

        bool canEditView;
        private void SetViewCanEdit(bool canRead)
        {
            canEditView = canRead;
        }

        private void tableView_ShowGridMenu(object sender, GridMenuEventArgs e)
        {
            var menuInfo = tableView.GridMenu.MenuInfo as GridCellMenuInfo;
            if (menuInfo != null && menuInfo.Row != null && menuInfo.Column != null)
            {
                if(!canEditView)
                {
                    contextMenuFillUp.IsVisible = false;
                    contextMenuFillDown.IsVisible = false;
                    contextMenuBulkEdit.IsVisible = false;
                    contextMenuDuplicate.IsVisible = false;
                    contextMenuDuplicateMulti.IsVisible = false;
                    contextMenuInsert.IsVisible = false;
                    contextMenuInsertMulti.IsVisible = false;
                    contextMenuPopulate.IsVisible = false;
                    return;
                }

                contextMenuFillUp.IsVisible = true;
                contextMenuFillDown.IsVisible = true;
                contextMenuBulkEdit.IsVisible = true;

                if (menuInfo.Row.ControllerVisibleIndex == DataControlBase.NewItemRowHandle)
                {
                    contextMenuDuplicate.IsEnabled = false;
                    contextMenuDuplicateMulti.IsEnabled = false;
                    contextMenuInsert.IsEnabled = false;
                    contextMenuInsertMulti.IsEnabled = false;
                }
                else
                {
                    contextMenuDuplicate.IsEnabled = true;
                    contextMenuDuplicateMulti.IsEnabled = true;
                    contextMenuInsert.IsEnabled = true;
                    contextMenuInsertMulti.IsEnabled = true;
                }

                string[] context_menu_specification = null;
                if (menuInfo.Column.Tag != null)
                    context_menu_specification = menuInfo.Column.Tag.ToString().Split(';');

                if (context_menu_specification != null && context_menu_specification[0] != string.Empty)
                {
                    bool isPopulateVisible = bool.Parse(context_menu_specification[0]);
                    contextMenuPopulate.IsVisible = isPopulateVisible;

                    bool isFillVisible = bool.Parse(context_menu_specification[1]);
                    contextMenuFillUp.IsVisible = isFillVisible;
                    contextMenuFillDown.IsVisible = isFillVisible;
                    contextMenuBulkEdit.IsVisible = isFillVisible;
                    contextMenuPopulate.Content = context_menu_specification[2];
                }
                else
                    contextMenuPopulate.IsVisible = false;
            }
        }
    }
}