using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.DataModel;
using BluePrints.Common.ViewModel;
using BluePrints.Data;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    public class VARIATION_ITEMProjection : PROGRESS_ITEMProjection
    {
        public VARIATION_ITEMProjection()
        {
            VARIATION_ITEM = new VARIATION_ITEM();
            BASELINE_ITEMJoinRATE = new BASELINE_ITEMProjection();
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

        public bool ISSUBMITTED { get; set; }

        public bool ISAPPROVED { get; set; }

        public bool ISREADONLY
        {
            get
            {
                if (ISSUBMITTED == true)
                    return true;

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
                    BASELINE_ITEMJoinRATE == null || BASELINE_ITEMJoinRATE.BASELINE_ITEM.TOTAL_HOURS == 0)
                    return 0;
                if (PROGRESS_ITEMCurrent == null)
                    return -1 * BASELINE_ITEMJoinRATE.BASELINE_ITEM.TOTAL_HOURS;
                else
                    return -1 * (BASELINE_ITEMJoinRATE.BASELINE_ITEM.TOTAL_HOURS - (PROGRESS_ITEMCurrent.EARNED_UNITS + PastPROGRESS_ITEMS_UNITS));
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
                return (BASELINE_ITEMJoinRATE.BASELINE_ITEM.TOTAL_HOURS + VARIATION_ITEM.VARIATION_UNITS) *
                       BASELINE_ITEMJoinRATE.ITEMRATE;
            }
        }

        public decimal VARIATION_COST
        {
            get
            {
                return FORECAST_UNITS * BASELINE_ITEMJoinRATE.ITEMRATE;
            }
        }
    }

    public static class VARIATION_ITEMProjectionQuery
    {
        public static IQueryable<VARIATION_ITEMProjection> JoinRATESAndPROGRESS_ITEMSAndVARIATION_ITEMSOnBASELINE_ITEMS(
            IQueryable<BASELINE_ITEM> BASELINE_ITEMS, Func<PROGRESS> getPROGRESSFunc, Func<BASELINE> getBASELINEFunc,
            Func<VARIATION> getVARIATIONFunc, Func<IEnumerable<PROGRESS_ITEM>> getPROGRESS_ITEMSFunc,
            Func<IEnumerable<VARIATION_ITEM>> getVARIATION_ITEMSFunc, Func<IEnumerable<RATE>> getRATESFunc, Func<IEnumerable<DELIVERABLES_STATUS>> getDELIVERABLES_STATUSESFunc, bool IsSubmitted, bool IsApproved)
        {
            var BASELINE = getBASELINEFunc();
            var PROGRESS = getPROGRESSFunc();
            var VARIATION = getVARIATIONFunc();
            var RATES = getRATESFunc();

            IEnumerable<VARIATION_ITEM> LoadVARIATION_ITEMS;
            if (VARIATION == null)
                LoadVARIATION_ITEMS = new List<VARIATION_ITEM>();
            else
                LoadVARIATION_ITEMS = getVARIATION_ITEMSFunc();

            IEnumerable<PROGRESS_ITEM> LoadPROGRESS_ITEMS;
            if (PROGRESS == null)
                LoadPROGRESS_ITEMS = new List<PROGRESS_ITEM>();
            else
                LoadPROGRESS_ITEMS = getPROGRESS_ITEMSFunc();

            IQueryable<PROGRESS_ITEMProjection> BASELINE_ITEMJoinRATESJoinPROGRESS_ITEMS;
            if (PROGRESS == null || VARIATION == null)
                BASELINE_ITEMJoinRATESJoinPROGRESS_ITEMS = new List<PROGRESS_ITEMProjection>().AsQueryable();
            else
            {
                if (VARIATION.APPROVED != null)
                    BASELINE_ITEMJoinRATESJoinPROGRESS_ITEMS =
                        PROGRESS_ITEMProjectionQueries.JoinRATESAndPROGRESS_ITEMSOnBASELINE_ITEMS(
                            BASELINE_ITEMS.Where(x => x.GUID_VARIATION == VARIATION.GUID && x.GUID_BASELINE == VARIATION.GUID_BASELINE),
                            getPROGRESSFunc, getBASELINEFunc, getPROGRESS_ITEMSFunc, getRATESFunc, getDELIVERABLES_STATUSESFunc, true);
                else
                    BASELINE_ITEMJoinRATESJoinPROGRESS_ITEMS =
                        PROGRESS_ITEMProjectionQueries.JoinRATESAndPROGRESS_ITEMSOnBASELINE_ITEMS(
                            BASELINE_ITEMS.Where(
                                x => x.GUID_BASELINE == BASELINE.GUID || x.GUID_VARIATION == VARIATION.GUID && x.GUID_BASELINE == null), getPROGRESSFunc,
                            getBASELINEFunc, getPROGRESS_ITEMSFunc, getRATESFunc, getDELIVERABLES_STATUSESFunc, true);
            }

            var reportingDate = PROGRESS == null ? new DateTime() : PROGRESS.DATA_DATE;
            return
                BASELINE_ITEMJoinRATESJoinPROGRESS_ITEMS.ToArray()
                    .Select(x => new VARIATION_ITEMProjection()
                    {
                        GUID = x.GUID,
                        VARIATION_ITEM =
                            LoadVARIATION_ITEMS.Where(
                                    y => y.GUID_ORIBASEITEM == x.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_ORIGINAL)
                                .FirstOrDefault(),
                        BASELINE_ITEMJoinRATE = x.BASELINE_ITEMJoinRATE,
                        ISSUBMITTED = IsSubmitted,
                        ISAPPROVED = IsApproved,
                        ReportingDataDate = reportingDate,
                        PROGRESS_ITEMS =
                            LoadPROGRESS_ITEMS.Where(
                                    y => y.GUID_ORIBASEITEM == x.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_ORIGINAL)
                    }).AsQueryable();
        }
    }
}