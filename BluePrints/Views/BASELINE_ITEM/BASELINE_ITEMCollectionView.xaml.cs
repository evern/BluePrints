using System;
using System.Linq;
using System.Windows.Controls;
using System.Collections.Generic;
using BluePrints.ViewModels;
using DevExpress.Xpf.Grid;
using System.Windows.Input;
using DevExpress.Xpf.Editors;

namespace BluePrints.Views
{
    public partial class BASELINE_ITEMCollectionView : UserControl
    {
        public BASELINE_ITEMCollectionView()
        {
            InitializeComponent();
            ((BASELINE_ITEMSViewModelWrapper)this.DataContext).ShowWORKPACKInternalName1 = this.ShowWorkpackInternalName1;
            ((BASELINE_ITEMSViewModelWrapper)this.DataContext).ShowWORKPACKInternalName2 = this.ShowWorkpackInternalName2;
        }

        public void ShowWorkpackInternalName1()
        {
            colWORKPACKInternalName1.Visible = true;
        }

        public void ShowWorkpackInternalName2()
        {
            colWORKPACKInternalName2.Visible = true;
        }

        private void tableView_ShowGridMenu(object sender, DevExpress.Xpf.Grid.GridMenuEventArgs e)
        {
            GridCellMenuInfo menuInfo = tableView.GridMenu.MenuInfo as GridCellMenuInfo;
            if (menuInfo != null && menuInfo.Row != null && menuInfo.Column != null)
            {
                if (menuInfo.Row.ControllerVisibleIndex == GridControl.NewItemRowHandle)
                {
                    contextMenuDuplicate.IsEnabled = false;
                    contextMenuDuplicateMulti.IsEnabled = false;
                }
                else
                {
                    contextMenuDuplicate.IsEnabled = true;
                    contextMenuDuplicateMulti.IsEnabled = true;
                }

                if (menuInfo.Column == colWORKPACKInternalName1 || menuInfo.Column == colWORKPACKInternalName2)
                {
                    contextMenuPopulate.IsVisible = true;
                    contextMenuPopulate.Content = "Autofill from Area/DocType/Disc/Dept data";
                }
                else if (menuInfo.Column == colAREA)
                {
                    contextMenuPopulate.IsVisible = true;
                    contextMenuPopulate.Content = "Autofill From Workpack Data";
                }
                else if (menuInfo.Column == colDOCTYPE)
                {
                    contextMenuPopulate.IsVisible = true;
                    contextMenuPopulate.Content = "Autofill From Workpack Data";
                }
                else if (menuInfo.Column == colDISCIPLINE)
                {
                    contextMenuPopulate.IsVisible = true;
                    contextMenuPopulate.Content = "Autofill From Workpack Data";
                }
                else if (menuInfo.Column == colDEPARTMENT)
                {
                    contextMenuPopulate.IsVisible = true;
                    contextMenuPopulate.Content = "Autofill From Workpack Data";
                }
                else if (menuInfo.Column == colPRIMARY_TITLE)
                {
                    contextMenuPopulate.IsVisible = false;
                }
                else if (menuInfo.Column == colSECONDARY_TITLE)
                {
                    contextMenuPopulate.IsVisible = false;
                }
                else if (menuInfo.Column == colESTIMATED_HOURS)
                {
                    contextMenuPopulate.IsVisible = false;
                }
                else if (menuInfo.Column == colINTERNAL_NUM)
                {
                    contextMenuPopulate.IsVisible = true;
                    contextMenuPopulate.Content = "Autofill From Area/DocType/Disc/Dept Data";
                }
                else if (menuInfo.Column == colCLIENT_NUM)
                {
                    contextMenuPopulate.IsVisible = false;
                }
                else if (menuInfo.Column == colREVISION_NUMBER)
                {
                    contextMenuPopulate.IsVisible = false;
                }
                else if (menuInfo.Column == colTOTAL_COSTS)
                {
                    contextMenuPopulate.IsVisible = false;
                    //contextMenuBulkEdit.IsEnabled = false;
                }
                else if (menuInfo.Column == colDC_HOURS)
                {
                    contextMenuPopulate.IsVisible = false;
                    //contextMenuBulkEdit.IsEnabled = false;
                }
                else if (menuInfo.Column == colCOMMENTS)
                {
                    contextMenuPopulate.IsVisible = false;
                }
                else if (menuInfo.Column == colDELIVERABLE_TYPE)
                {
                    contextMenuPopulate.IsVisible = false;
                }
            }
        }
    }
}
