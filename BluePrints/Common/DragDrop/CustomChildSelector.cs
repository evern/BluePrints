using DevExpress.Xpf.Grid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.DragDrop
{
    public class CustomChildSelector : IChildNodesSelector
    {
        IEnumerable IChildNodesSelector.SelectChildren(object item)
        {
            //if (item is Project)
            //    return (item as Project).Stages;
            //if (item is Stage)
            //    return (item as Stage).Tasks;
            return null;
        }
    }
}
