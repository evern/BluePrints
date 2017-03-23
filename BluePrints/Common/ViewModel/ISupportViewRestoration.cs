using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.ViewModel
{
    public interface ISupportViewRestoration
    {
        Action StoreActiveCell { get; set; }
        Action RestoreActiveCell { get; set; }

        //Raise Properties changed doesn't refresh column data, call this method instead
        Action ForceGridRefresh { get; set; }
    }
}
