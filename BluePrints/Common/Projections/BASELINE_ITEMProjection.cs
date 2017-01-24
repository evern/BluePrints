using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.DataModel;
using BluePrints.Common.ViewModel;
using BluePrints.Data;
using BluePrints.Data.Attributes;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    [ConstraintAttributes("BASELINE_ITEM.GUID_BASELINE, BASELINE_ITEM.INTERNAL_NUM")]
    public class BASELINE_ITEMProjection
    {
        public BASELINE_ITEMProjection()
        {
            BASELINE_ITEM = new BASELINE_ITEM();
        }

        [Key]
        public Guid GUID { get; set; }
        public BASELINE_ITEM BASELINE_ITEM { get; set; }
        public RATE RATE { get; set; }
        public DELIVERABLES_STATUS DELIVERABLE_STATUS { get; set; }
        public decimal ITEMRATE
        {
            get
            {
                if (RATE == null || RATE.RATE1 == null)
                    return 0;

                return (decimal)RATE.RATE1;
            }
        }

        public decimal ESTIMATED_COSTS
        {
            get
            {
                if (BASELINE_ITEM == null)
                    return 0;

                if (RATE == null || RATE.RATE1 == null)
                    return 0;

                return BASELINE_ITEM.ESTIMATED_HOURS * (decimal)RATE.RATE1;
            }
        }

        public decimal TOTAL_COSTS { get { return BASELINE_ITEM.TOTAL_HOURS * ITEMRATE; } }
    }

    public static class BASELINE_ITEMProjectionQueries
    {
        public static IQueryable<BASELINE_ITEMProjection> JoinRATESOnBASELINE_ITEMS(IQueryable<BASELINE_ITEM> BASELINE_ITEMS, Func<BASELINE> getBASELINEFunc, Func<IQueryable<RATE>> getRATES_ByProjectFunc = null, Func<IQueryable<DELIVERABLES_STATUS>> getDELIVERABLES_STATUSESFunc = null, bool isBASELINEQueryProcessed = false)
        {
            BASELINE BASELINE = getBASELINEFunc();
            IQueryable<BASELINE_ITEM> contextBASELINE_ITEMS;
            if (BASELINE == null)
                contextBASELINE_ITEMS = BASELINE_ITEMS.Where(x => x.GUID == Guid.Empty);
            else
            {
                if(isBASELINEQueryProcessed)
                    contextBASELINE_ITEMS = BASELINE_ITEMS;
                else
                    contextBASELINE_ITEMS = BASELINE_ITEMS.Where(x => x.GUID_BASELINE == BASELINE.GUID);
            }

            List<RATE> RATES;
            if (getRATES_ByProjectFunc == null)
                RATES = new List<RATE>();
            else
                RATES = new List<RATE>(getRATES_ByProjectFunc());

            List<DELIVERABLES_STATUS> DELIVERABLES_STATUSES;
            if (getDELIVERABLES_STATUSESFunc == null)
                DELIVERABLES_STATUSES = new List<DELIVERABLES_STATUS>();
            else
                DELIVERABLES_STATUSES = new List<DELIVERABLES_STATUS>(getDELIVERABLES_STATUSESFunc());

            return contextBASELINE_ITEMS.ToArray().AsQueryable().Select(x => new BASELINE_ITEMProjection() { GUID = x.GUID, BASELINE_ITEM = x, DELIVERABLE_STATUS = x.GUID_STATUS == null ? null : DELIVERABLES_STATUSES.FirstOrDefault(z => z.GUID == x.GUID_STATUS), RATE = RATES.FirstOrDefault(y => y.GUID_DEPARTMENT == x.GUID_DEPARTMENT && y.GUID_DISCIPLINE == x.GUID_DISCIPLINE) });
        }
    }
}
