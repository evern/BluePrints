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
        public static void Initialize_Stats(IEnumerable<IReportable> reportableItems, IEnumerable<VariationAdjustment> variationAdjustments, DateTime reporting_data_date, TimeSpan reporting_interval, DateTime first_aligned_data_date, bool progressHaveStats)
        {
            if (progressHaveStats)
                return;

            foreach (IReportable reportableItem in reportableItems)
            {
                ReportablesDisplay reportablesDisplay = reportableItem as ReportablesDisplay;
                if (reportablesDisplay != null)
                {
                    IReportable_Group reportable_Group = reportablesDisplay.ProgressItem as IReportable_Group;
                    if (reportable_Group != null)
                    {
                        List<VariationAdjustment> group_variation_adjustments = new List<VariationAdjustment>();
                        foreach(IReportable reportable in reportable_Group.Reportables)
                        {
                            group_variation_adjustments.AddRange(variationAdjustments.Where(x => x.DeliverableOriginalGuid == reportable.OriginalEntityKey).ToList());
                            reportable.Stats = new ProgressStats(reporting_data_date, reporting_interval, first_aligned_data_date, reportable.Budget_Units, reportable.Total_Units, reportable.Budget_Costs, reportable.Total_Costs, variationAdjustments.Where(x => x.DeliverableOriginalGuid == reportable.OriginalEntityKey));
                        }

                        reportable_Group.Stats = new ProgressStats(reporting_data_date, reporting_interval, first_aligned_data_date, reportable_Group.Budget_Units, reportable_Group.Total_Units, reportable_Group.Budget_Costs, reportable_Group.Total_Costs, group_variation_adjustments);
                    }
                }
                else
                    reportableItem.Stats = new ProgressStats(reporting_data_date, reporting_interval, first_aligned_data_date, reportableItem.Budget_Units, reportableItem.Total_Units, reportableItem.Budget_Costs, reportableItem.Total_Costs, variationAdjustments.Where(x => x.DeliverableOriginalGuid == reportableItem.OriginalEntityKey));
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
                        ICanProgressByQuantity progressByQuantityDeliverable = lookUpDeliverable as ICanProgressByQuantity;
                        decimal variation_units;
                        if (progressByQuantityDeliverable == null)
                            variation_units = variation_item.VARIATION_UNITS;
                        else
                            variation_units = variation_item.VARIATION_UNITS * progressByQuantityDeliverable.UnitsPerQuantity;

                        variationAdjustments.Add(new VariationAdjustment(variation_item.GUID_ORIBASEITEM) { AdjustmentDate = (DateTime)variation.APPROVED, AdjustmentUnits = variation_units, AdjustmentRate = lookUpDeliverable.Budget_ItemRate });
                    }
                }
            }

            return variationAdjustments;
        }
    }
}