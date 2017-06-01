using BaseModel.Misc;
using BluePrints.Common.Projections;
using BluePrints.ViewModels;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace BluePrints.Views
{
    public partial class BASELINE_ITEMCollectionView : ViewStateRestoreUserControl
    {
        bool isBASELINELocked;
        public BASELINE_ITEMCollectionView()
        {
            InitializeComponent();
            InitializeViewControl(GridControl, tableView);
            ((BASELINE_ITEMSCollectionViewModelWrapper)DataContext).GetGridVisibleRows =
                GetGridVisibleRows;
            ((BASELINE_ITEMSCollectionViewModelWrapper)DataContext).SetBaselineLockUnlock =
                SetBaselineLockUnlock;
        }

        public IEnumerable<PROGRESS_ITEMProjection> GetGridVisibleRows()
        {
            List<PROGRESS_ITEMProjection> visibleBaselineItemProjections = new List<PROGRESS_ITEMProjection>();
            for (int i = 0; i < GridControl.VisibleRowCount; i++)
            {
                PROGRESS_ITEMProjection dataRow = (PROGRESS_ITEMProjection)GridControl.GetRow(GridControl.GetRowHandleByVisibleIndex(i));
                visibleBaselineItemProjections.Add(dataRow);
            }
            return visibleBaselineItemProjections;
        }

        public void SetBaselineLockUnlock(bool isLock)
        {
            isBASELINELocked = isLock;
        }

        private void tableView_ShowGridMenu(object sender, GridMenuEventArgs e)
        {
            var menuInfo = tableView.GridMenu.MenuInfo as GridCellMenuInfo;
            if (menuInfo != null && menuInfo.Row != null && menuInfo.Column != null)
            {
                contextMenuFillUp.IsVisible = true;
                contextMenuFillDown.IsVisible = true;

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

                if (menuInfo.Column == colWORKPACKInternalName1)
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
                    contextMenuBulkEdit.IsVisible = !isBASELINELocked;
                    contextMenuFillUp.IsVisible = !isBASELINELocked;
                    contextMenuFillDown.IsVisible = !isBASELINELocked;

                    contextMenuPopulate.IsVisible = false;
                }
                else if (menuInfo.Column == colINTERNAL_NUM)
                {
                    contextMenuPopulate.IsVisible = true;
                    contextMenuFillUp.IsVisible = false;
                    contextMenuFillDown.IsVisible = false;
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