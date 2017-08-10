using BaseModel.Attributes;
using BluePrints.Common.Base;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.Common.Projections
{
    [ConstraintAttributes("Entity.GUID_PROJECT, Entity.INTERNAL_NAME1")]
    [RequiredAttributes("Entity.GUID_DDEPARTMENT, Entity.GUID_DDISCIPLINE")]
    public class WORKPACKProjection : BluePrintsProjectionBase<WORKPACK>, IDeliverable_Rates_Group
    {
        public IEnumerable<IDeliverable_Rates> DeliverableRates { get; set; }

        public string Discipline_Code => string.Empty;

        public string Deliverable_Name => string.Empty;

        public Guid? Workpack_Guid => Guid.Empty;

        public Guid OriginalEntityKey => Guid.Empty;

        public string Phase_Code => string.Empty;

        public string Commodity_Code => string.Empty;

        public Guid? Area_Guid => Guid.Empty;

        public Guid? SubArea_Guid => Guid.Empty;

        public decimal Estimated_Units => DeliverableRates == null ? 0 : DeliverableRates.Sum(x => x.Estimated_Units);

        public decimal Total_Units => DeliverableRates == null ? 0 : DeliverableRates.Sum(x => x.Total_Units);

        public decimal Variation_Units => DeliverableRates == null ? 0 : DeliverableRates.Sum(x => x.Variation_Units);

        public decimal ItemRate => DeliverableRates == null ? 0 : DeliverableRates.Sum(x => x.ItemRate);

        public decimal Estimated_Costs => DeliverableRates == null ? 0 : DeliverableRates.Sum(x => x.Estimated_Costs);

        public decimal Variation_Costs => DeliverableRates == null ? 0 : DeliverableRates.Sum(x => x.Variation_Costs);

        public decimal Total_Costs => DeliverableRates == null ? 0 : DeliverableRates.Sum(x => x.Total_Costs);

        public string Commodity_Display_Code => Commodity_Code;

        public string Workpack_Name => string.Empty;

        public string Department_Code => string.Empty;

        public void SetOriginalEntityKey(Guid newGuid)
        {
            throw new NotImplementedException();
        }
    }

    public static class WORKPACKProjectionQueries
    {
        public static IQueryable<WORKPACKProjection> IDeliverable_Rates_Group_Transformation(
            IQueryable<WORKPACK> WORKPACKS, IEnumerable<BASELINE_ITEM> BASELINE_ITEMS, PROGRESS PROGRESS, BASELINE BASELINE,
            IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS, IEnumerable<RATE> RATES)
        {
            IQueryable<BASELINE_ITEMProjection> baseline_rateProjection;
            if (PROGRESS == null)
                baseline_rateProjection = new List<BASELINE_ITEMProjection>().AsQueryable();
            else
                baseline_rateProjection = BASELINE_ITEMProjectionQueries.IDeliverable_Rates_Transformation(BASELINE_ITEMS.AsQueryable(), RATES);

            var reportingDate = PROGRESS == null ? new DateTime() : PROGRESS.DATA_DATE;
            return
                WORKPACKS.ToArray().Select(x => new WORKPACKProjection()
                {
                    Entity = x,
                    DeliverableRates = baseline_rateProjection.Where(rateProjection => rateProjection.Workpack_Guid == x.GUID)
                }).AsQueryable();
        }
    }
}
