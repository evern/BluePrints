using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.Common.Projections
{
    public class VARIATION_ITEMProjection : PROGRESS_ITEMProjection
    {
        public VARIATION_ITEMProjection()
            : base()
        {
            variation_item = new VARIATION_ITEM();
            VARIATION_ITEM.ACTION = VariationAction.NoAction;
        }

        public VARIATION_ITEMProjection(DateTime reportingDataDate)
            : base(reportingDataDate)
        {
            variation_item = new VARIATION_ITEM();
            VARIATION_ITEM.ACTION = VariationAction.NoAction;
        }

        private VARIATION_ITEM variation_item { get; set; }

        public VARIATION_ITEM VARIATION_ITEM
        {
            get { return variation_item; }
            set
            {
                if (value == null)
                    return;
                else
                    variation_item = value;
            }
        }

        public decimal FORECAST_UNITS
        {
            get
            {
                //When variation item is approved minunits will be 0 because there will be no more value to contra in progress
                if (ISAPPROVED)
                    return VARIATION_ITEM.VARIATION_UNITS;

                if (VARIATION_ITEM.ACTION == VariationAction.Cancel)
                {
                    return MINUNITS;
                }

                return VARIATION_ITEM.VARIATION_UNITS;
            }
        }

        public DateTime? SUBMITTED { get; set; }

        public DateTime? APPROVED { get; set; }

        public bool AdjustUnitsReadOnly
        {
            get { return SUBMITTED != null || Entity.Entity.BY_DURATION; }
        }

        public bool ISSUBMITTED
        {
            get { return SUBMITTED != null; }
        }

        public bool ISAPPROVED
        {
            get { return APPROVED != null; }
        }

        public bool ISREADONLY
        {
            get
            {
                if (ISSUBMITTED)
                    return true;

                if (GUID == Guid.Empty)
                    return false;

                if (VARIATION_ITEM.ACTION != VariationAction.Add)
                    return true;

                return false;
            }
        }

        public bool ISCANCELLABLE
        {
            get
            {
                if (ISSUBMITTED || ISAPPROVED)
                    return false;

                if (VARIATION_ITEM.ACTION != VariationAction.Add)
                    return true;

                return false;
            }
        }

        public bool ISENABLED
        {
            get
            {
                return !ISREADONLY;
            }
        }

        public decimal MINUNITSFORVALIDATION
        {
            get
            {
                //when variation is apporved MINUNITS should not cause a warning
                if (ISSUBMITTED)
                    return -100000;

                return MINUNITS;
            }
        }

        public decimal MINUNITS
        {
            get
            {
                if (PROGRESS_ITEMSBeforeReportingDate == null ||
                    Entity == null || Entity.Entity.TOTAL_HOURS == 0)
                    return 0;
                if (PROGRESS_ITEMCurrent == null)
                    return -1 * Entity.Entity.TOTAL_HOURS;
                else
                    return -1 * (Entity.Entity.TOTAL_HOURS - (PROGRESS_ITEMCurrent.EARNED_UNITS + PastPROGRESS_ITEMS_UNITS));
            }
        }

        public bool CANTOGGLECANCELLATION
        {
            get { return !ISSUBMITTED && VARIATION_ITEM.ACTION != VariationAction.Add; }
        }

        public decimal TOTAL_COST
        {
            get
            {
                return (Entity.Entity.TOTAL_HOURS + VARIATION_ITEM.VARIATION_UNITS) *
                       Entity.ItemRate;
            }
        }

        public decimal VARIATION_COST
        {
            get
            {
                return FORECAST_UNITS * Entity.ItemRate;
            }
        }
    }

    public static class VARIATION_ITEMProjectionQuery
    {
        public static IQueryable<VARIATION_ITEMProjection> JoinRATESAndPROGRESS_ITEMSAndVARIATION_ITEMSOnBASELINE_ITEMS(
            IQueryable<BASELINE_ITEM> BASELINE_ITEMS, PROGRESS PROGRESS, BASELINE BASELINE,
            VARIATION VARIATION, IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS,
            IEnumerable<VARIATION_ITEM> VARIATION_ITEMS, IEnumerable<RATE> RATES, IEnumerable<DELIVERABLES_STATUS> DELIVERABLES_STATUSES, IEnumerable<AREA> SUBAREA, DateTime? submittedDate, DateTime? approvedDate)
        {
            IQueryable<PROGRESS_ITEMProjection> BASELINE_ITEMJoinRATESJoinPROGRESS_ITEMS;
            if (PROGRESS == null || VARIATION == null)
                BASELINE_ITEMJoinRATESJoinPROGRESS_ITEMS = new List<PROGRESS_ITEMProjection>().AsQueryable();
            else
            {
                if (VARIATION.APPROVED != null)
                    BASELINE_ITEMJoinRATESJoinPROGRESS_ITEMS =
                        PROGRESS_ITEMProjectionQueries.JoinRATESAndPROGRESS_ITEMSOnBASELINE_ITEMS(
                            BASELINE_ITEMS.Where(x => x.GUID_VARIATION == VARIATION.GUID && x.GUID_BASELINE == VARIATION.GUID_BASELINE),
                            PROGRESS, PROGRESS_ITEMS, RATES, DELIVERABLES_STATUSES, SUBAREA);
                else
                    BASELINE_ITEMJoinRATESJoinPROGRESS_ITEMS =
                        PROGRESS_ITEMProjectionQueries.JoinRATESAndPROGRESS_ITEMSOnBASELINE_ITEMS(
                            BASELINE_ITEMS.Where(x => x.GUID_BASELINE == BASELINE.GUID || x.GUID_VARIATION == VARIATION.GUID && x.GUID_BASELINE == null), PROGRESS, PROGRESS_ITEMS, RATES, DELIVERABLES_STATUSES, SUBAREA);
            }

            var reportingDate = PROGRESS == null ? new DateTime() : PROGRESS.DATA_DATE;
            return
                BASELINE_ITEMJoinRATESJoinPROGRESS_ITEMS.OrderBy(x => x.Entity.Entity.CREATED).ToArray()
                    .Select(x => new VARIATION_ITEMProjection(reportingDate)
                    {
                        EntityKey = x.EntityKey,
                        VARIATION_ITEM =
                            VARIATION_ITEMS.Where(
                                    y => y.GUID_ORIBASEITEM == x.Entity.Entity.GUID_ORIGINAL)
                                .FirstOrDefault(),
                        Entity = x.Entity,
                        SUBMITTED = submittedDate,
                        APPROVED = approvedDate,
                        PROGRESS_ITEMS =
                            PROGRESS_ITEMS.Where(
                                    y => y.GUID_ORIBASEITEM == x.Entity.Entity.GUID_ORIGINAL).ToList()
                    }).AsQueryable();
        }
    }
}