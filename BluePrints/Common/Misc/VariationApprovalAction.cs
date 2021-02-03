using BluePrints.Common.ViewModel.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Misc
{
    public class VariationApprovalAction<TEntity>
        where TEntity : class, IDeliverable, ISupportVariationRevision, new()
    {
        public VariationApprovalAction(ISupportVariation<TEntity> deliverable)
        {
            Deliverable = deliverable;
        }

        public ISupportVariation<TEntity> Deliverable { get; set; }
        public bool ReduceEarned { get; set; }
        public decimal MaximumReducibleUnits => -1 * (Deliverable.Total_Units - Deliverable.Earned_Units_Total);
    }
}
