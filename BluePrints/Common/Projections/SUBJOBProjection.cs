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
    public class SUBJOBProjection : BluePrintsProjectionBase<SUBJOB>, IDeliverable_Rates_Group
    {
        public IEnumerable<IDeliverable_Rates> DeliverableRates { get; set; }

        public string Discipline_Code => string.Empty;

        public string Deliverable_Name => string.Empty;

        public Guid? Subjob_Guid => Guid.Empty;

        public Guid OriginalEntityKey => Guid.Empty;

        public string Phase_Code => string.Empty;

        public string Variation_Code => string.Empty;

        public string Commodity_Code => string.Empty;

        public Guid? Area_Guid => Guid.Empty;

        public Guid? SubArea_Guid => Guid.Empty;

        public decimal Budget_Units => DeliverableRates == null ? 0 : DeliverableRates.Sum(x => x.Budget_Units);

        public decimal Total_Units => DeliverableRates == null ? 0 : DeliverableRates.Sum(x => x.Total_Units);

        public decimal Variation_Units => DeliverableRates == null ? 0 : DeliverableRates.Sum(x => x.Variation_Units);

        public decimal Budget_ItemRate => DeliverableRates == null ? 0 : DeliverableRates.Sum(x => x.Budget_ItemRate);

        public decimal Budget_Costs => DeliverableRates == null ? 0 : DeliverableRates.Sum(x => x.Budget_Costs);

        public decimal Variation_Costs => DeliverableRates == null ? 0 : DeliverableRates.Sum(x => x.Variation_Costs);

        public decimal Total_Costs => DeliverableRates == null ? 0 : DeliverableRates.Sum(x => x.Total_Costs);

        public string Commodity_Display_Code => Commodity_Code;

        public string Subjob_Name => string.Empty;

        public string Department_Code => string.Empty;

        public Guid? Phase_Guid { get => Entity.GUID_DPHASE; set => Entity.GUID_DPHASE = value; }

        public Guid? Discipline_Guid => Guid.Empty;

        public decimal Discipline_Number => 0;

        public Guid? Workpack_Guid { get => Guid.Empty; set { } }

        public PhaseType? Phase => Entity.PHASE == null ? null : Entity.PHASE.PHASE_TYPE;

        public ChargeType? Charge => Entity.PHASE == null ? null : Entity.PHASE.CHARGE_TYPE;

        public IEnumerable<User_Weight> AssignedUsers => new List<User_Weight>();

        Guid? IDeliverable.Subjob_Guid { get => Guid.Empty; set { } }

        public bool IsByDuration { get; set; }

        public decimal Budget_Quantity => DeliverableRates == null ? 0 : DeliverableRates.Sum(x => x.Budget_Quantity);

        public decimal Total_Quantity => DeliverableRates == null ? 0 : DeliverableRates.Sum(x => x.Total_Quantity);

        public string Project_Number => Entity.PROJECT.NUMBER;

        public void SetOriginalEntityKey(Guid newGuid)
        {
            throw new NotImplementedException();
        }
    }

    public static class SUBJOBProjectionQueries
    {
        public static IQueryable<SUBJOBProjection> IDeliverable_Rates_Group_Transformation(
            IQueryable<SUBJOB> SUBJOBS, IEnumerable<BASELINE_ITEM> BASELINE_ITEMS, PROGRESS PROGRESS, BASELINE BASELINE,
            IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS, IEnumerable<RATE> RATES)
        {
            IQueryable<BASELINE_ITEMProjection> baseline_rateProjection;
            if (PROGRESS == null)
                baseline_rateProjection = new List<BASELINE_ITEMProjection>().AsQueryable();
            else
                baseline_rateProjection = BASELINE_ITEMProjectionQueries.IDeliverable_Rates_Transformation(BASELINE_ITEMS.AsQueryable(), RATES);

            var reportingDate = PROGRESS == null ? new DateTime() : PROGRESS.DATA_DATE;
            return
                SUBJOBS.ToArray().Select(x => new SUBJOBProjection()
                {
                    Entity = x,
                    DeliverableRates = baseline_rateProjection.Where(rateProjection => rateProjection.Subjob_Guid == x.GUID)
                }).AsQueryable();
        }
    }
}
