using DevExpress.Xpf.Grid;
using System;
using System.Linq;
using System.Windows;

namespace BluePrints.Common
{
    public class CustomTreeViewDragDropManager : TreeListDragDropManager
    {
        protected override TableDragIndicatorPosition GetDragIndicatorPositionForRowElement(FrameworkElement rowElement)
        {
            if (rowElement == null)
                return TableDragIndicatorPosition.None;
            return TableDragIndicatorPosition.InRow;
        }
    }

    public class CustomTableViewDragDropManager : GridDragDropManager
    {
        protected override TableDragIndicatorPosition GetDragIndicatorPositionForRowElement(FrameworkElement rowElement)
        {
            if (rowElement == null)
                return TableDragIndicatorPosition.None;
            return TableDragIndicatorPosition.InRow;
        }
    }
}
