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
        bool isBASELINELocked;
        public BASELINE_ITEMCollectionView()
        {
            InitializeComponent();
            //InitializeViewControl(GridControl, tableView);
            //((BASELINE_ITEMCollectionViewModelWrapper)DataContext).GetGridVisibleRows = GetGridVisibleRows;
            //((BASELINE_ITEMCollectionViewModelWrapper)DataContext).SetBaselineLockUnlock =
            //    SetBaselineLockUnlock;
        }

        //public IEnumerable<BASELINE_ITEMProgress> GetGridVisibleRows()
        //{
        //    List<BASELINE_ITEMProgress> visibleBaselineItemProjections = new List<BASELINE_ITEMProgress>();
        //    for (int i = 0; i < GridControl.VisibleRowCount; i++)
        //    {
        //        BASELINE_ITEMProgress dataRow = (BASELINE_ITEMProgress)GridControl.GetRow(GridControl.GetRowHandleByVisibleIndex(i));
        //        visibleBaselineItemProjections.Add(dataRow);
        //    }
        //    return visibleBaselineItemProjections;
        //}

        public void SetBaselineLockUnlock(bool isLock)
        {
            isBASELINELocked = isLock;
        }

        private void tableView_ShowGridMenu(object sender, GridMenuEventArgs e)
        {
            //var menuInfo = tableView.GridMenu.MenuInfo as GridCellMenuInfo;
            //if (menuInfo != null && menuInfo.Row != null && menuInfo.Column != null)
            //{
            //    contextMenuFillUp.IsVisible = true;
            //    contextMenuFillDown.IsVisible = true;

            //    if (menuInfo.Row.ControllerVisibleIndex == DataControlBase.NewItemRowHandle)
            //    {
            //        contextMenuDuplicate.IsEnabled = false;
            //        contextMenuDuplicateMulti.IsEnabled = false;
            //    }
            //    else
            //    {
            //        contextMenuDuplicate.IsEnabled = true;
            //        contextMenuDuplicateMulti.IsEnabled = true;
            //    }

            //    if (menuInfo.Column == colBUDGET_HOURS)
            //    {
            //        contextMenuBulkEdit.IsVisible = !isBASELINELocked;
            //        contextMenuFillUp.IsVisible = !isBASELINELocked;
            //        contextMenuFillDown.IsVisible = !isBASELINELocked;

            //        contextMenuPopulate.IsVisible = false;
            //    }
            //    else
            //    {
            //        string[] context_menu_specification = null;
            //        if (menuInfo.Column.Tag != null)
            //            context_menu_specification = menuInfo.Column.Tag.ToString().Split(';');

            //        if (context_menu_specification != null)
            //        {
            //            bool isPopulateVisible = bool.Parse(context_menu_specification[0]);
            //            contextMenuPopulate.IsVisible = isPopulateVisible;

            //            bool isFillVisible = bool.Parse(context_menu_specification[1]);
            //            contextMenuFillUp.IsVisible = isFillVisible;
            //            contextMenuFillDown.IsVisible = isFillVisible;

            //            contextMenuPopulate.Content = context_menu_specification[2];
            //        }
            //        else
            //            contextMenuPopulate.IsVisible = false;
            //    }
            //}
        }
    }
}