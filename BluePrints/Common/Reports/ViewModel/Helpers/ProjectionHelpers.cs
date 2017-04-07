using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Mvvm.POCO;
using BluePrints.Common.Projections;

namespace BluePrints.Common.ViewModel.Reporting
{
    /// <summary>
    /// Provides a set of extension methods to perform commonly used operations with ISupportProgressReporting.
    /// </summary>
    public static class ProjectionHelpers
    {
        public static void InitializePROGRESS_ITEMStats(IEnumerable<PROGRESS_ITEMProjection> PROGRESS_ITEMS, IEnumerable<VariationAdjustment> variationAdjustments, PROGRESS livePROGRESS, bool progressHaveStats)
        {
            foreach (PROGRESS_ITEMProjection progressItem in PROGRESS_ITEMS)
            {
                List<VariationAdjustment> currentProgressItemAdjustments = variationAdjustments.Where(x => x.DeliverableOriginalGuid == progressItem.Entity.Entity.GUID_ORIGINAL).ToList();
                if(!progressHaveStats)
                    progressItem.Stats = new ProgressStats(livePROGRESS, progressItem.Entity.Entity.ESTIMATED_HOURS, progressItem.Entity .Entity.TOTAL_HOURS, progressItem.Entity.ESTIMATED_COSTS, progressItem.Entity.TOTAL_COSTS, currentProgressItemAdjustments);
            }
        }

        public static List<VariationAdjustment> BuildProjectVariationAdjustments(IQueryable<VARIATION> VARIATION, IEnumerable<BASELINE_ITEMProjection> BASELINE_ITEM)
        {
            List<VariationAdjustment> variationAdjustments = new List<VariationAdjustment>();
            IQueryable<VARIATION> ApprovedVARIATION = VARIATION.Where(x => x.APPROVED != null && x.TYPE == VariationType.External);
            foreach (VARIATION variation in ApprovedVARIATION)
            {
                IEnumerable<VARIATION_ITEM> applicableVariation_Item = variation.VARIATION_ITEM.Where(x => x.ACTION == VariationAction.Add || x.ACTION == VariationAction.Append);
                foreach (VARIATION_ITEM variation_item in applicableVariation_Item)
                {
                    BASELINE_ITEMProjection findBASELINE_ITEM = BASELINE_ITEM.FirstOrDefault(x => x.Entity.GUID_ORIGINAL == variation_item.GUID_ORIBASEITEM);
                    if (findBASELINE_ITEM != null)
                    {
                        variationAdjustments.Add(new VariationAdjustment(variation_item.GUID_ORIBASEITEM) { AdjustmentDate = (DateTime)variation.APPROVED, AdjustmentUnits = variation_item.VARIATION_UNITS, AdjustmentRate = findBASELINE_ITEM.ITEMRATE });
                    }
                }
            }

            return variationAdjustments;
        }

        public static void SetWorkpackAssignmentStartUnit(IEnumerable<PROGRESS_ITEMProjection> progressStats)
        {
            Dictionary<Guid, decimal> workpackP6AssignedUnits = new Dictionary<Guid, decimal>();
            progressStats = progressStats.OrderBy(x => x.Entity.Entity.INTERNAL_NUM != null);
            foreach(PROGRESS_ITEMProjection progressStat in progressStats)
            {
                Guid? currentWORKPACKGuid = progressStat.Entity.Entity.GUID_WORKPACK;
                if (currentWORKPACKGuid == null)
                    continue;

                var assignedWorkpack = workpackP6AssignedUnits.Where(x => x.Key == currentWORKPACKGuid)
                    .Select(e => (KeyValuePair<Guid, decimal>?)e).FirstOrDefault();

                decimal workpackAssignmentStartUnit = 1;
                if (assignedWorkpack != null)
                {
                    workpackAssignmentStartUnit = ((KeyValuePair<Guid, decimal>)assignedWorkpack).Value;
                    workpackP6AssignedUnits.Remove(((KeyValuePair<Guid, decimal>)assignedWorkpack).Key);
                }

                progressStat.SetWorkpackAssignmentStartUnit(workpackAssignmentStartUnit);
                //move assignment start unit by total hours for next start unit assignment
                workpackAssignmentStartUnit += progressStat.Entity.Entity.TOTAL_HOURS;
                workpackP6AssignedUnits.Add((Guid)currentWORKPACKGuid, workpackAssignmentStartUnit);
            }
        }
    }
}