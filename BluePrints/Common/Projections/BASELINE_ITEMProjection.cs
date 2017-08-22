using BaseModel.Attributes;
using BaseModel.Misc;
using BluePrints.Common.Base;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BluePrints.Common.Projections
{
    [ConstraintAttributes("Entity.GUID_BASELINE, Entity.INTERNAL_NUM")]
    public class BASELINE_ITEMProjection : BluePrintsProjectionBase<BASELINE_ITEM>, IDeliverable_Rates, ISupportByDuration, IHaveDeliverableStatus, IHaveDBProductivityOverride, ISupportVariation, IEntityNumber
    {
        public BASELINE_ITEMProjection()
            : base()
        {

        }

        public RATE RATE { get; set; }

        public decimal ItemRate
        {
            get
            {
                if (RATE == null || RATE.RATE1 == null)
                    return 0;

                return (decimal)RATE.RATE1;
            }
        }

        public decimal Estimated_Costs => Estimated_Units * ItemRate;

        public decimal Total_Costs => Total_Units * ItemRate;

        public string Deliverable_Name => Entity.Deliverable_Name;

        public string Commodity_Code => Entity.Commodity_Code;

        public string Commodity_Display_Code => Commodity_Code;

        public Guid? Workpack_Guid => Entity.Workpack_Guid;

        public decimal Estimated_Units => Entity.Estimated_Units;

        public decimal Total_Units => Entity.Total_Units;

        public Guid OriginalEntityKey => Entity.OriginalEntityKey;

        public void SetOriginalEntityKey(Guid newGuid) => Entity.SetOriginalEntityKey(newGuid);

        public Guid? Area_Guid => Entity.GUID_AREA;

        public Guid? SubArea_Guid => Entity.GUID_SUBAREA;

        public string Discipline_Code => Entity.Discipline_Code;

        public decimal Variation_Units => Entity.Variation_Units;

        public decimal Variation_Costs => Entity.Variation_Units * ItemRate;

        public bool IsByDuration => Entity.IsByDuration;

        public DELIVERABLES_STATUS Deliverable_Status => Entity.DELIVERABLES_STATUS;

        public string Phase_Code => Entity.PHASE == null ? string.Empty : Entity.PHASE.INTERNAL_NUM;

        public decimal? DB_Productivity_Override { get => Entity.DB_Productivity_Override; set => Entity.DB_Productivity_Override = value; }

        public Guid? Baseline_Guid { get => Entity.Baseline_Guid; set => Entity.Baseline_Guid = value; }

        public Guid? Variation_Guid { get => Entity.Variation_Guid; set => Entity.Variation_Guid = value; }
        public decimal Estimated_Value { get => Entity.Estimated_Value; set => Entity.Estimated_Value = value; }
        public decimal DC_Value { get => Entity.DC_Value; set => Entity.DC_Value = value; }

        public string Workpack_Name => Entity.Workpack_Name;

        public string Department_Code => Entity.Department_Code;

        public string EntityNumber { get => Entity.EntityNumber; set => Entity.EntityNumber = value; }
    }

    public static class BASELINE_ITEMProjectionQueries
    {
        public static IQueryable<BASELINE_ITEMProjection> IDeliverable_Rates_Transformation(
            IQueryable<BASELINE_ITEM> BASELINE_ITEMS, 
            IEnumerable<RATE> RATES)
        {
            return
                BASELINE_ITEMS.ToArray()
                    .Select(x => new BASELINE_ITEMProjection()
                            {
                                EntityKey = x.GUID,
                                Entity = x,
                                RATE = RATES.FirstOrDefault(y => y.GUID_DEPARTMENT == x.GUID_DEPARTMENT && y.GUID_DISCIPLINE == x.GUID_DISCIPLINE)
                            }).AsQueryable();
        }
    }
}