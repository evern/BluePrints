using BaseModel.Attributes;
using BluePrints.Common.Base;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.Common.Projections
{
    [ConstraintAttributes("Entity.GUID_PROJECT, Entity.INTERNAL_NAME1")]
    [RequiredAttributes("Entity.GUID_DDEPARTMENT, Entity.GUID_DDISCIPLINE")]
    public class WORKPACKProjection : BluePrintsProjectionBase<WORKPACK>
    {
        public decimal TOTAL_UNITS { get; set; }

        public decimal TOTAL_COSTS { get; set; }
    }

    public static class WORKPACKProjectionQueries
    {
        public static IQueryable<WORKPACKProjection> JoinPROGRESSProjectionOnWORKPACKS(
            IQueryable<WORKPACK> WORKPACKS, IEnumerable<BASELINE_ITEM> BASELINE_ITEMS, PROGRESS PROGRESS, BASELINE BASELINE,
            IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS, IEnumerable<RATE> RATES)
        {
            IQueryable<BASELINE_ITEMProjection> AllBaselineItems;
            if (PROGRESS == null)
                AllBaselineItems = new List<BASELINE_ITEMProjection>().AsQueryable();
            else
                AllBaselineItems = BASELINE_ITEMProjectionQueries.BASELINE_ITEMProjectionQuery(BASELINE_ITEMS.AsQueryable(), RATES);

            var reportingDate = PROGRESS == null ? new DateTime() : PROGRESS.DATA_DATE;
            return
                WORKPACKS.ToArray().Select(x => new WORKPACKProjection()
                {
                    EntityKey = x.GUID,
                    Entity = x,
                    TOTAL_COSTS = AllBaselineItems.Where(y => y.Entity.GUID_WORKPACK == x.GUID).Sum(z => z.Total_Costs),
                    TOTAL_UNITS = AllBaselineItems.Where(y => y.Entity.GUID_WORKPACK == x.GUID).Sum(z => z.Total_Units)
                }).AsQueryable();
        }
    }
}
