using BaseModel.Attributes;
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
    public class BASELINE_ITEMProjection : BluePrintsProjectionBase<BASELINE_ITEM>, IDeliverable_Rates, ISupportByDuration, IHaveDeliverableStatus
    {
        public BASELINE_ITEMProjection()
            : base()
        {

        }

        public RATE RATE { get; set; }
        
        public ICollection<P6_ASSIGNMENT> ObservableBASELINE_ITEM_ASSIGNMENT { get; set; }

        private List<P6_ASSIGNMENT> baseline_item_assignments;
        public List<P6_ASSIGNMENT> BASELINE_ITEM_ASSIGNMENTS
        {
            get
            {
                return baseline_item_assignments;
            }
            set
            {
                if (baseline_item_assignments == null)
                    baseline_item_assignments = new List<P6_ASSIGNMENT>();

                baseline_item_assignments = value;
            }
        }

        public decimal Remaining_Percentage
        {
            get
            {
                return 1 - ASSIGNED_PERCENTAGE;
            }
        }

        public decimal ASSIGNED_PERCENTAGE
        {
            get
            {
                return BASELINE_ITEM_ASSIGNMENTS.Sum(x => (x.HIGH_VALUE - (x.LOW_VALUE - 0.01m)));
            }
        }

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

        public string ReportableItem_Name => Entity.ReportableItem_Name;

        public string Commodity_Code => Entity.Commodity_Code;

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
    }

    public static class BASELINE_ITEMProjectionQueries
    {
        public static IQueryable<BASELINE_ITEMProjection> BASELINE_ITEMProjectionQuery(
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