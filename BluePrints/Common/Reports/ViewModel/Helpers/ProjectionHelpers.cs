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
        public static void Initialize_Stats(IEnumerable<IReportable> reportableItems, DateTime reporting_data_date, TimeSpan reporting_interval, DateTime first_aligned_data_date, bool progressHaveStats, DateTime? overrideLastProgressDate = null, bool forceRetrieveRemainingDataPoints = false, bool allowPercentageOnZeroTotalUnits = false)
        {
            if (progressHaveStats)
                return;

            foreach (IReportable reportableItem in reportableItems)
            {
                reportableItem.Stats = new ProgressStats(reporting_data_date, reporting_interval, first_aligned_data_date, reportableItem.Budget_Units, reportableItem.Total_Units, reportableItem.Budget_Quantity, reportableItem.Total_Quantity, reportableItem.Budget_Costs, reportableItem.Total_Costs, overrideLastProgressDate, forceRetrieveRemainingDataPoints, allowPercentageOnZeroTotalUnits);
            }
        }

        public static List<VariationAdjustment> BuildProjectVariationAdjustments(IQueryable<VARIATION> VARIATION, IEnumerable<IDeliverable_Rates> deliverables)
        {
            List<VariationAdjustment> variationAdjustments = new List<VariationAdjustment>();
            if (VARIATION.Count() == 0)
                return variationAdjustments;

            IQueryable<VARIATION> ApprovedVARIATION = VARIATION.Where(x => x.APPROVED != null);
            foreach (VARIATION variation in ApprovedVARIATION)
            {
                IEnumerable<VARIATION_ITEM> applicableVariation_Item = variation.VARIATION_ITEM.Where(x => x.ACTION == VariationAction.Add || x.ACTION == VariationAction.Append || x.ACTION == VariationAction.Cancel);
                foreach (VARIATION_ITEM variation_item in applicableVariation_Item)
                {
                    IDeliverable_Rates lookUpDeliverable = deliverables.FirstOrDefault(x => x.OriginalEntityKey == variation_item.GUID_ORIBASEITEM);
                    if (lookUpDeliverable != null)
                    {
                        variationAdjustments.Add(new VariationAdjustment(variation_item.GUID_ORIBASEITEM) { VariationName = variation.NAME, AdjustmentDate = (DateTime)variation.APPROVED, AdjustmentUnits = variation_item.VARIATION_UNITS, AdjustmentRate = lookUpDeliverable.Budget_ItemRate, AdjustmentInternalRate = lookUpDeliverable.Budget_ItemInternalRate, IsBudgetAdjustment = variation.ADJUSTMENT_TO_BUDGET });
                    }
                }
            }

            return variationAdjustments;
        }
    }
}