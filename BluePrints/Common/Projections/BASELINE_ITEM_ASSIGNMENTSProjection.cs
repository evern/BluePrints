using BluePrints.Common.Base;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    public class BASELINE_ITEM_ASSIGNMENTSProjection : BluePrintsProjectionBase<BASELINE_ITEM_ASSIGNMENT>
    {
        public BASELINE_ITEM_ASSIGNMENTSProjection()
            : base()
        {

        }

        public string INTERNAL_NUM { get; set; }
        public Guid GUID_ORIGINAL { get; set; }
    }
}
