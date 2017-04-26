using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.DataModel;
using BluePrints.Common.ViewModel;
using BluePrints.Data;
using BluePrints.Data.Attributes;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    [ConstraintAttributes("Entity.GUID_BASELINE, Entity.INTERNAL_NUM")]
    public class BASELINE_ITEMProjection : ProjectionBase<BASELINE_ITEM>
    {
        public BASELINE_ITEMProjection()
            : base()
        {

        }

        public RATE RATE { get; set; }
        public DELIVERABLES_STATUS DELIVERABLE_STATUS { get; set; }

        public decimal ITEMRATE
        {
            get
            {
                if (RATE == null || RATE.RATE1 == null)
                    return 0;

                return (decimal) RATE.RATE1;
            }
        }

        public decimal ESTIMATED_COSTS
        {
            get
            {
                if (Entity == null)
                    return 0;

                if (RATE == null || RATE.RATE1 == null)
                    return 0;

                return Entity.ESTIMATED_HOURS * (decimal) RATE.RATE1;
            }
        }

        public decimal DC_COSTS
        {
            get
            {
                if (Entity == null)
                    return 0;

                if (RATE == null || RATE.RATE1 == null)
                    return 0;

                return Entity.DC_HOURS * (decimal)RATE.RATE1;
            }
        }

        public decimal TOTAL_COSTS
        {
            get
            {
                if (Entity == null)
                    return 0;

                if (RATE == null || RATE.RATE1 == null)
                    return 0;

                return Entity.TOTAL_HOURS * ITEMRATE;
            }
        }

        private List<BASELINE_ITEM_ASSIGNMENT> baseline_item_assignments;
        public List<BASELINE_ITEM_ASSIGNMENT> BASELINE_ITEM_ASSIGNMENTS
        {
            get
            {
                return baseline_item_assignments;
            }
            set
            {
                if (baseline_item_assignments == null)
                    baseline_item_assignments = new List<BASELINE_ITEM_ASSIGNMENT>();

                baseline_item_assignments = value;
            }
        }
    }

    public static class BASELINE_ITEMProjectionQueries
    {
        public static IQueryable<BASELINE_ITEMProjection> BASELINE_ITEMProjectionQuery(
            IQueryable<BASELINE_ITEM> BASELINE_ITEMS, Func<BASELINE> getBASELINEFunc,
            Func<IEnumerable<RATE>> getRATES_ByProjectFunc,
            Func<IEnumerable<DELIVERABLES_STATUS>> getDELIVERABLES_STATUSESFunc,
            bool isBASELINEQueryProcessed = false)
        {
            var BASELINE = getBASELINEFunc();
            IQueryable<BASELINE_ITEM> contextBASELINE_ITEMS;
            if (BASELINE == null)
                contextBASELINE_ITEMS = new List<BASELINE_ITEM>().AsQueryable();
            else
            {
                if (isBASELINEQueryProcessed)
                    contextBASELINE_ITEMS = BASELINE_ITEMS;
                else
                    contextBASELINE_ITEMS = BASELINE_ITEMS.Where(x => x.GUID_BASELINE == BASELINE.GUID);
            }

            IEnumerable<RATE> RATES = getRATES_ByProjectFunc();
            IEnumerable<DELIVERABLES_STATUS> DELIVERABLES_STATUSES = getDELIVERABLES_STATUSESFunc();

            return
                contextBASELINE_ITEMS.ToArray()
                    .Select(
                        x =>
                            new BASELINE_ITEMProjection()
                            {
                                GUID = x.GUID,
                                Entity = x,
                                DELIVERABLE_STATUS =
                                    (x.GUID_STATUS == null)
                                        ? null
                                        : DELIVERABLES_STATUSES.FirstOrDefault(z => z.GUID == x.GUID_STATUS),
                                RATE = 
                                    RATES.FirstOrDefault(
                                        y =>
                                            y.GUID_DEPARTMENT == x.GUID_DEPARTMENT &&
                                            y.GUID_DISCIPLINE == x.GUID_DISCIPLINE)
                            }).AsQueryable();
        }
    }
}