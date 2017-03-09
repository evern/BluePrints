using BluePrints.Common.ViewModel;
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

        GridControl gridControl;
        TableView tableView;

        public void InitializeViewRestoration(GridControl gridControl, TableView tableView)
        {
            this.gridControl = gridControl;
            this.tableView = tableView;

            ISupportViewRestoration viewRestoration = DataContext as ISupportViewRestoration;
            if(viewRestoration != null)
            {
                viewRestoration.StoreActiveCell = this.StoreFocusedCell;
                viewRestoration.RestoreActiveCell = this.RestoreFocusedCell;
            }
        }

        protected void StoreFocusedCell()
        {
            this.focusedRowHandle = tableView.FocusedRowHandle;
            this.currentColumn = gridControl.CurrentColumn;
        }

        protected void RestoreFocusedCell()
        {
            gridControl.CurrentColumn = this.currentColumn;
            tableView.FocusedRowHandle = focusedRowHandle;
            gridControl.Focus();
            //GridColumn setValueColumn = gridControl.Columns[gridControl.CurrentColumn.FieldName];
            //gridControl.SetFocusedRowCellValue(setValueColumn, currentValue);
            tableView.ShowEditor();
        }
    }
}
