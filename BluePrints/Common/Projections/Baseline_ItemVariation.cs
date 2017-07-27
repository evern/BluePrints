using BluePrints.Common.Base;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.Common.Projections
{
    public class BASELINE_ITEMVariation : BluePrintsVariationBase<BASELINE_ITEMProgress>
    {
        
    }

    public static class Baseline_ItemVariationQuery
    {
        public static IQueryable<BASELINE_ITEMVariation> OffsiteDirectVariationItemTransformation(
            IQueryable<BASELINE_ITEM> BASELINE_ITEMS, PROJECT PROJECT, PROGRESS PROGRESS, IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS, BASELINE BASELINE, VARIATION VARIATION,
            IEnumerable<VARIATION_ITEM> VARIATION_ITEMS, IEnumerable<RATE> RATES)
        {
            IQueryable<BASELINE_ITEM> contextBASELINE_ITEMS;

            if (VARIATION.APPROVED == null)
                //When variation is not approved, retrieve current live deliverables and variation deliverables
                contextBASELINE_ITEMS = BASELINE_ITEMS.Where(x => x.GUID_BASELINE == BASELINE.GUID || x.GUID_VARIATION == VARIATION.GUID && x.GUID_BASELINE == null);
            else
                //When variation is approved, retrieve deliverables from variation connected baseline
                contextBASELINE_ITEMS = BASELINE_ITEMS.Where(x => x.GUID_VARIATION == VARIATION.GUID && x.GUID_BASELINE == VARIATION.GUID_BASELINE);

            //when either live progress or variation doesn't exists don't return anything
            IQueryable<BASELINE_ITEMProgress> Baseline_ItemProgresses;
            if (PROGRESS == null || VARIATION == null)
                Baseline_ItemProgresses = new List<BASELINE_ITEMProgress>().AsQueryable();
            else
                Baseline_ItemProgresses = ProgressQueries.OffsiteDirectProgressItemTransformation(contextBASELINE_ITEMS, PROJECT, PROGRESS, RATES, PROGRESS_ITEMS);

            return
                Baseline_ItemProgresses.OrderBy(x => x.Entity.Entity.CREATED).ToArray()
                    .Select(x => new BASELINE_ITEMVariation()
                    {
                        Entity = x,
                        VARIATION_ITEM = VARIATION_ITEMS.Where(y => y.GUID_ORIBASEITEM == x.Entity.Entity.GUID_ORIGINAL).FirstOrDefault(),
                        SubmittedDate = VARIATION.SUBMITTED,
                        ApprovedDate = VARIATION.APPROVED
                    }).AsQueryable();
        }
    }
}