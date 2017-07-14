using BluePrints.Common.Base;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    public class P6_ASSIGNMENTProjection : BluePrintsProjectionBase<P6_ASSIGNMENT>
    {
        public P6_ASSIGNMENTProjection()
            : base()
        {

        }

        public string Deliverable_Name { get; set; }
        public Guid Deliverable_OriginalEntityKey { get; set; }
    }
}
