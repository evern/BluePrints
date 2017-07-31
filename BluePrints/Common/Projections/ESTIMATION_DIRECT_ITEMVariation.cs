using BluePrints.Common.Base;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.Common.Projections
{
    public class ESTIMATION_DIRECT_ITEMVariation : BluePrintsQuantityVariationBase<ESTIMATION_DIRECT_ITEMProgress>, ISupportVariationSummary
    {
        public Guid? Baseline_Guid { get => Entity.Baseline_Guid; set => Entity.Entity.Baseline_Guid = value; }
        public Guid? Variation_Guid { get => Entity.Variation_Guid; set => Entity.Entity.Variation_Guid = value; }
        public decimal Estimated_Value { get => Entity.Estimated_Value; set => Entity.Estimated_Value = value; }
        public decimal DC_Value { get => Entity.DC_Value; set => Entity.DC_Value = value; }
    }

    public static class Estimation_Direct_ItemVariationQuery
    {
        public static IQueryable<ESTIMATION_DIRECT_ITEMVariation> SiteDirectVariationItemTransformation(
            IEnumerable<ESTIMATION_DIRECT_ITEMProgress> ESTIMATION_DIRECT_ITEMProgress, VARIATION VARIATION,
            IEnumerable<VARIATION_ITEM> VARIATION_ITEMS)
        {
            return
                ESTIMATION_DIRECT_ITEMProgress.OrderBy(x => x.Entity.Entity.CREATED).ToArray()
                    .Select(x => new ESTIMATION_DIRECT_ITEMVariation()
                    {
                        Entity = x,
                        VARIATION_ITEM = VARIATION_ITEMS.Where(y => y.GUID_ORIBASEITEM == x.Entity.Entity.GUID_ORIGINAL).FirstOrDefault(),
                        SubmittedDate = VARIATION == null ? null : VARIATION.SUBMITTED,
                        ApprovedDate = VARIATION == null ? null : VARIATION.APPROVED
                    }).AsQueryable();
        }
    }
}