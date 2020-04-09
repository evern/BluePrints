using BaseModel.Misc;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.ViewModels;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace BluePrints.Views
{
    public partial class BASELINE_ITEMCollectionView : UserControl
    {
        public BASELINE_ITEMCollectionView()
        {
            InitializeComponent();
        }

        private void tableView_ShowGridMenu(object sender, GridMenuEventArgs e)
        {
            var menuInfo = tableView.GridMenu.MenuInfo as GridCellMenuInfo;
            if (menuInfo != null && menuInfo.Row != null && menuInfo.Column != null)
            {
                contextMenuFillUp.IsVisible = true;
                contextMenuFillDown.IsVisible = true;
                contextMenuBulkEdit.IsVisible = true;
                contextMenuFindReplace.IsVisible = true;

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
                    contextMenuFindReplace.IsVisible = isFillVisible;
                    contextMenuPopulate.Content = context_menu_specification[2];
                }
                else
                    contextMenuPopulate.IsVisible = false;
            }
        }
    }
}