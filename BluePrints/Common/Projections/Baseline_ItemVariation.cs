using BluePrints.Common.Base;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.Common.Projections
{
    public class BASELINE_ITEMVariation : BluePrintsVariationBase<BASELINE_ITEMProgress>, ISupportVariationSummary
    {
        public Guid? Baseline_Guid { get => Entity.Baseline_Guid; set => Entity.Entity.Baseline_Guid = value; }
        public Guid? Variation_Guid { get => Entity.Variation_Guid; set => Entity.Entity.Variation_Guid = value; }
        public decimal Estimated_Value { get => Entity.Estimated_Value; set => Entity.Estimated_Value = value; }
        public decimal DC_Value { get => Entity.DC_Value; set => Entity.DC_Value = value; }
    }

    public static class Baseline_ItemVariationQuery
    {
        public static IQueryable<BASELINE_ITEMProgress> OffsiteDirectVariationItemTransformation(IQueryable<BASELINE_ITEM> BASELINE_ITEMS, PROJECT PROJECT, PROGRESS PROGRESS, IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS, BASELINE BASELINE, VARIATION VARIATION, IEnumerable<VARIATION_ITEM> VARIATION_ITEMS, IEnumerable<RATE> RATES)
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

            foreach(var baseline_item in Baseline_ItemProgresses)
            {
                baseline_item.VARIATION_ITEM = VARIATION_ITEMS.Where(y => y.GUID_ORIBASEITEM == baseline_item.Entity.Entity.GUID_ORIGINAL).FirstOrDefault();
                baseline_item.SubmittedDate = VARIATION.SUBMITTED;
                baseline_item.ApprovedDate = VARIATION.APPROVED;
            }

            return Baseline_ItemProgresses;
        }
    }
}