using BluePrints.Common.Base;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.Common.Projections
{
    public class ESTIMATE_ITEMVariation : BluePrintsQuantityVariationBase<ESTIMATE_ITEMProgress>, ISupportVariationSummary
    {
        public Guid? Baseline_Guid { get => Entity.Baseline_Guid; set => Entity.Entity.Baseline_Guid = value; }
        public Guid? Variation_Guid { get => Entity.Variation_Guid; set => Entity.Entity.Variation_Guid = value; }
        public decimal Estimated_Value { get => Entity.Estimated_Value; set => Entity.Estimated_Value = value; }
        public decimal DC_Value { get => Entity.DC_Value; set => Entity.DC_Value = value; }
    }

    public static class Estimation_Direct_ItemVariationQuery
    {
        public static IQueryable<ESTIMATE_ITEMVariation> SiteDirectVariationItemTransformation(
            IEnumerable<ESTIMATE_ITEMProgress> ESTIMATE_ITEMProgress, VARIATION VARIATION,
            IEnumerable<VARIATION_ITEM> VARIATION_ITEMS)
        {
            return
                ESTIMATE_ITEMProgress.OrderBy(x => x.Entity.Entity.CREATED).ToArray()
                    .Select(x => new ESTIMATE_ITEMVariation()
                    {
                        Entity = x,
                        VARIATION_ITEM = VARIATION_ITEMS.Where(y => y.GUID_ORIBASEITEM == x.Entity.Entity.GUID_ORIGINAL).FirstOrDefault(),
                        SubmittedDate = VARIATION == null ? null : VARIATION.SUBMITTED,
                        ApprovedDate = VARIATION == null ? null : VARIATION.APPROVED
                    }).AsQueryable();
        }
    }
}