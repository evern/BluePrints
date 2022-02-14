using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Misc
{
    public class POFilterMessage
    {
        public POFilterMessage(string PONumber, string StockCode, string VariationCode)
        {
            this.PONumber = PONumber;
            this.StockCode = StockCode;
            this.VariationCode = VariationCode;
        }

        public string PONumber { get; private set; }
        public string StockCode { get; private set; }
        public string VariationCode { get; private set; }
    }

    public class ContextBASELINE_ITEMProjectionsMessage
    {
        public ContextBASELINE_ITEMProjectionsMessage(IEnumerable<BASELINE_ITEMProjection> contextCollection)
        {
            BASELINE_ITEMProjections = contextCollection;
        }

        public IEnumerable<BASELINE_ITEMProjection> BASELINE_ITEMProjections { get; private set; }
    }

    public class P6_Deliverable_Assignment_Message
    {
        public P6_Deliverable_Assignment_Message(P6_Activity selected_activity, IEnumerable<ICanAssignP6> selected_deliverables)
        {
            Selected_Activity = selected_activity;
            Selected_Deliverables = selected_deliverables;
        }

        public P6_Activity Selected_Activity { get; private set; }
        public IEnumerable<ICanAssignP6> Selected_Deliverables { get; private set; }
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
