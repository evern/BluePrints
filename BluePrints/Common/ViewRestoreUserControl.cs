using BluePrints.Common;
using BluePrints.Common.ViewModel;
using DevExpress.Xpf.Core.Serialization;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BluePrints.Views
{
    public class ViewStateRestoreUserControl : UserControl
    {
        int focusedRowHandle;
        ColumnBase currentColumn;
        bool onBeforeRefreshIsActive;

        GridControl gridControl;
        TableViewEx tableView;

        public void InitializeViewControl(GridControl gridControl, TableViewEx tableView)
        {
            this.gridControl = gridControl;
            this.tableView = tableView;

            ISupportViewRestoration viewRestoration = DataContext as ISupportViewRestoration;
            if(viewRestoration != null)
            {
                viewRestoration.StoreActiveCell = this.StoreFocusedCell;
                viewRestoration.RestoreActiveCell = this.RestoreFocusedCell;
                viewRestoration.ForceGridRefresh = this.ForceGridRefresh;
            }

            this.gridControl.AddHandler(DXSerializer.AllowPropertyEvent,
                    new AllowPropertyEventHandler(grid_AllowedProperty));

            foreach(GridColumn gridColumn in gridControl.Columns)
            {
                gridColumn.FilterPopupMode = FilterPopupMode.CheckedList;
            }
        }

        void grid_AllowedProperty(object sender, AllowPropertyEventArgs e)
        {
            e.Allow = e.DependencyProperty != GridControl.FilterStringProperty;
        }

        protected virtual void ForceGridRefresh()
        {
            gridControl.RefreshData();
        }

        protected virtual void StoreFocusedCell()
        {
            this.focusedRowHandle = tableView.FocusedRowHandle;
            this.currentColumn = gridControl.CurrentColumn;
            this.onBeforeRefreshIsActive = tableView.IsEditing;
        }

        protected virtual void RestoreFocusedCell()
        {
            gridControl.CurrentColumn = this.currentColumn;
            tableView.FocusedRowHandle = focusedRowHandle;
            gridControl.Focus();
            //Allows for previous value to be restored 
            //Because active editor have latest value but cannot revert to old value when esc is pressed
            //GridColumn setValueColumn = gridControl.Columns[gridControl.CurrentColumn.FieldName];
            //gridControl.SetFocusedRowCellValue(setValueColumn, currentValue);

            if (this.onBeforeRefreshIsActive && !tableView.IsEditing)
                tableView.ShowEditor();
        }
    }
}
