using BluePrints.Common.Projections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Misc
{
    public class ContextBASELINE_ITEMProjectionsMessage
    {
        public ContextBASELINE_ITEMProjectionsMessage(IEnumerable<BASELINE_ITEMProjection> contextCollection)
        {
            BASELINE_ITEMProjections = contextCollection;
        }

        public IEnumerable<BASELINE_ITEMProjection> BASELINE_ITEMProjections { get; private set; }
    }

    public class SelectIntIdMessage
    {
        public SelectIntIdMessage(int selectedId)
        {
            SelectedId = selectedId;
        }

        public int SelectedId { get; private set; }
    }
}
