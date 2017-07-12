using BluePrints.Common.Projections;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.Common.ViewModel.Reporting
{
    /// <summary>
    /// Provides a set of extension methods to perform commonly used operations with ISupportProgressReporting.
    /// </summary>
    public static class ProjectionHelpers
    {
        public static void InitializePROGRESS_ITEMStats(IEnumerable<IReportable> reportableItems, IEnumerable<VariationAdjustment> variationAdjustments, PROGRESS livePROGRESS, bool progressHaveStats)
        {
            foreach (IReportable reportableItem in reportableItems)
            {
                List<VariationAdjustment> currentProgressItemAdjustments = variationAdjustments.Where(x => x.DeliverableOriginalGuid == reportableItem.OriginalEntityKey).ToList();
                if(!progressHaveStats)
                    reportableItem.Stats = new ProgressStats(livePROGRESS, reportableItem.Estimated_Units, reportableItem.Total_Units, reportableItem.Estimated_Costs, reportableItem.Total_Costs, currentProgressItemAdjustments);
            }
        }

        public static List<VariationAdjustment> BuildProjectVariationAdjustments(IQueryable<VARIATION> VARIATION, IEnumerable<IDeliverable_Rates> deliverables)
        {
            List<VariationAdjustment> variationAdjustments = new List<VariationAdjustment>();
            if (VARIATION.Count() == 0)
                return variationAdjustments;

            IQueryable<VARIATION> ApprovedVARIATION = VARIATION.Where(x => x.APPROVED != null && x.TYPE == VariationType.External);
            foreach (VARIATION variation in ApprovedVARIATION)
            {
                IEnumerable<VARIATION_ITEM> applicableVariation_Item = variation.VARIATION_ITEM.Where(x => x.ACTION == VariationAction.Add || x.ACTION == VariationAction.Append);
                foreach (VARIATION_ITEM variation_item in applicableVariation_Item)
                {
                    IDeliverable_Rates lookUpDeliverable = deliverables.FirstOrDefault(x => x.OriginalEntityKey == variation_item.GUID_ORIBASEITEM);
                    if (lookUpDeliverable != null)
                    {
                        variationAdjustments.Add(new VariationAdjustment(variation_item.GUID_ORIBASEITEM) { AdjustmentDate = (DateTime)variation.APPROVED, AdjustmentUnits = variation_item.VARIATION_UNITS, AdjustmentRate = lookUpDeliverable.ItemRate });
                    }
                }
            }

            return variationAdjustments;
        }
    }
}