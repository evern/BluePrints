using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.ViewModel.Reporting
{
    public class VariationAdjustment
    {
        public DateTime AdjustmentDate { get; set; }
        public decimal AdjustmentUnits { get; set; }
        public decimal? AdjustmentRate { get; set; }

        public readonly Guid DeliverableOriginalGuid;

        public VariationAdjustment(Guid deliverableOriginalGuid)
        {
            DeliverableOriginalGuid = deliverableOriginalGuid;
        }

        public decimal AdjustmentNativeCosts
        {
            get { return AdjustmentUnits * AdjustmentRate == null ? 0 : (decimal)AdjustmentRate; }
        }

        public decimal AdjustmentCumulativeCosts { get; set; }
    }
}