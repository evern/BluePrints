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
    public class BASELINE_ITEMProjection : BluePrintsProjectionBase<BASELINE_ITEM>, IDeliverable
    {
        public BASELINE_ITEMProjection()
            : base()
        {

        }

        public RATE RATE { get; set; }


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

        public decimal TotalUnitsIncludeByDuration
        {
            get
            {
                if (Entity == null)
                    return 0;

                if (Entity.BY_DURATION)
                    return BluePrintsConstants.DurationBasedTotalUnits;

                return Entity.TOTAL_HOURS + Entity.DC_HOURS;
            }
        }

        public decimal TOTAL_UNITS
        {
            get
            {
                if (Entity == null)
                    return 0;

                return Entity.TOTAL_HOURS + Entity.DC_HOURS;
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


        public ICollection<BASELINE_ITEM_ASSIGNMENT> ObservableBASELINE_ITEM_ASSIGNMENT { get; set; }

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


        public Guid Original_Guid
        {
            get { return Entity.GUID_ORIGINAL; }
        }

        public string ReportableItem_Name
        {
            get { return Entity.INTERNAL_NUM; }
        }

        public string Commodity_Code
        {
            get { return Entity.CommodityCode; }
        }

        public string Stock_Code
        {
            get { return Entity.StockCode; }
        }

        public Guid? Workpack_Guid
        {
            get { return Entity.GUID_WORKPACK; }
        }

        public decimal TotalHoursIncludeByDuration
        {
            get { return Entity.Total_HoursIncludeByDuration; }
        }

        public decimal EstimatedHours
        {
            get { return Entity.ESTIMATED_HOURS; }
        }

        public decimal TotalHours
        {
            get { return Entity.TOTAL_HOURS; }
        }

        public decimal EstimatedCosts
        {
            get { return ESTIMATED_COSTS; }
        }

        public decimal TotalCosts
        {
            get { return TOTAL_COSTS; }
        }

        public decimal ItemRate
        {
            get { return ITEMRATE; }
        }
    }

    public static class BASELINE_ITEMProjectionQueries
    {
        public static IQueryable<BASELINE_ITEMProjection> BASELINE_ITEMProjectionQuery(
            IQueryable<BASELINE_ITEM> BASELINE_ITEMS, Func<BASELINE> getBASELINEFunc,
            Func<IEnumerable<RATE>> getRATES_ByProjectFunc,
            Func<IEnumerable<DELIVERABLES_STATUS>> getDELIVERABLES_STATUSESFunc,
            Func<IEnumerable<AREA>> getSUBAREAFunc = null, 
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
            IEnumerable<AREA> SUBAREAS;
            if (getSUBAREAFunc == null)
                SUBAREAS = new List<AREA>();
            else
                SUBAREAS = getSUBAREAFunc();

            return
                contextBASELINE_ITEMS.ToArray()
                    .Select(
                        x =>
                            new BASELINE_ITEMProjection()
                            {
                                EntityKey = x.GUID,
                                Entity = x,
                                RATE = 
                                    RATES.FirstOrDefault(
                                        y =>
                                            y.GUID_DEPARTMENT == x.GUID_DEPARTMENT &&
                                            y.GUID_DISCIPLINE == x.GUID_DISCIPLINE)
                            }).AsQueryable();
        }

        public static IQueryable<BASELINE_ITEMProjection> BASELINE_ITEMProjectionQuery(
            IQueryable<BASELINE_ITEM> BASELINE_ITEMS, BASELINE BASELINE,
            IEnumerable<RATE> RATES,
            IEnumerable<DELIVERABLES_STATUS> DELIVERABLES_STATUSES, IEnumerable<BASELINE_ITEM_ASSIGNMENT> BASELINE_ITEM_ASSIGNMENTS)
        {
            IQueryable<BASELINE_ITEM> contextBASELINE_ITEMS;
            if (BASELINE == null)
                contextBASELINE_ITEMS = new List<BASELINE_ITEM>().AsQueryable();
            else
            {
                contextBASELINE_ITEMS = BASELINE_ITEMS.Where(x => x.GUID_BASELINE == BASELINE.GUID);
            }

            return
                contextBASELINE_ITEMS.ToArray()
                    .Select(
                        x =>
                            new BASELINE_ITEMProjection()
                            {
                                EntityKey = x.GUID,
                                Entity = x,
                                RATE =
                                    RATES.FirstOrDefault(
                                        y =>
                                            y.GUID_DEPARTMENT == x.GUID_DEPARTMENT &&
                                            y.GUID_DISCIPLINE == x.GUID_DISCIPLINE),
                                BASELINE_ITEM_ASSIGNMENTS = BASELINE_ITEM_ASSIGNMENTS.Where(y => y.GUID_ORIGINAL == x.GUID_ORIGINAL).ToList()
                            }).AsQueryable();
        }
    }
}