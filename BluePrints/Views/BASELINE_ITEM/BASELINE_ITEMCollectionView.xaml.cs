using System;
using System.Linq;
using System.Windows.Controls;
using System.Collections.Generic;
using BluePrints.ViewModels;
using DevExpress.Xpf.Grid;
using System.Windows.Input;
using BluePrints.Common.Projections;
using DevExpress.Xpf.Editors;
using DevExpress.Data.Filtering;
using DevExpress.Data.Helpers;

namespace BluePrints.Views
{
    public partial class BASELINE_ITEMCollectionView : ViewStateRestoreUserControl
    {
        public BASELINE_ITEMCollectionView()
        {
            InitializeComponent();
            InitializeViewControl(gridControl, tableView);
            ((BASELINE_ITEMSViewModelWrapper)DataContext).ShowWORKPACKInternalName1 =
                ShowWorkpackInternalName1;
            ((BASELINE_ITEMSViewModelWrapper)DataContext).ShowWORKPACKInternalName2 =
                ShowWorkpackInternalName2;
            ((BASELINE_ITEMSViewModelWrapper)DataContext).GetGridVisibleRows =
                GetGridVisibleRows;
        }

        public IEnumerable<BASELINE_ITEMProjection> GetGridVisibleRows()
        {
            List<BASELINE_ITEMProjection> visibleBaselineItemProjections = new List<BASELINE_ITEMProjection>();
            for (int i = 0; i < gridControl.VisibleRowCount; i++)
            {
                BASELINE_ITEMProjection dataRow = (BASELINE_ITEMProjection)gridControl.GetRow(gridControl.GetRowHandleByVisibleIndex(i));
                visibleBaselineItemProjections.Add(dataRow);
            }
            return visibleBaselineItemProjections;
        }

        public void ShowWorkpackInternalName1()
        {
            colWORKPACKInternalName1.Visible = true;
        }

        public void ShowWorkpackInternalName2()
        {
            colWORKPACKInternalName2.Visible = true;
        }

        private void tableView_ShowGridMenu(object sender, GridMenuEventArgs e)
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

        private void gridControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    tableView.CommitEditing();
                    tableView.MoveNextRow();
                    gridControl.SelectedItem = gridControl.GetRow(tableView.FocusedRowHandle);
                }));
            }
        }
    }
}