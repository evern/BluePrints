using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.ViewModel.Misc
{
    public class EarnedDataDateRealignModel
    {
        public Guid Guid { get; set; }
        public string InternalNumber { get; set; }
        public decimal EarnedUnits { get; set; }
        public DateTime CurrentEarnedDate { get; set; }
        public DateTime BackwardEarnedDate { get; set; }
        public DateTime ForwardEarnedDate { get; set; }
    }
}
