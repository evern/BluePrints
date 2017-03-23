using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.Data.Attributes;
using BluePrints.Common.ViewModel;

namespace BluePrints.Common.Projections
{
    [ConstraintAttributes("WORKPACK.GUID_PROJECT, WORKPACK.INTERNAL_NAME1, WORKPACK.INTERNAL_NAME2")]
    [RequiredAttributes("WORKPACK.GUID_DDEPARTMENT, WORKPACK.GUID_DDISCIPLINE")]
    public class WORKPACKProjection : PROJECTSummary, IHaveGUID
    {
        public WORKPACKProjection()
        {
            WORKPACK = new WORKPACK();
        }

        [Key]
        public Guid GUID { get; set; }

        public WORKPACK WORKPACK { get; set; }

        public decimal TOTAL_UNITS { get; set; }

        public decimal TOTAL_COSTS { get; set; }

        //public decimal TOTAL_UNITS
        //{
        //    get
        //    {
        //        if (!ReportableObjects.Any())
        //        {
        //            return 0;
        //        }
        //        else
        //        {
        //            IEnumerable<PROGRESS_ITEMProjection> progressItemProjections =
        //                (IEnumerable<PROGRESS_ITEMProjection>)ReportableObjects;

        //            return progressItemProjections.Sum(x => x.BASELINE_ITEMJoinRATE.BASELINE_ITEM.TOTAL_HOURS);
        //        }
        //    }
        //}

        //public decimal TOTAL_COSTS
        //{
        //    get
        //    {
        //        if (!ReportableObjects.Any())
        //        {
        //            return 0;
        //        }
        //        else
        //        {
        //            IEnumerable<PROGRESS_ITEMProjection> progressItemProjections =
        //                (IEnumerable<PROGRESS_ITEMProjection>)ReportableObjects;

        //            return progressItemProjections.Sum(x => x.BASELINE_ITEMJoinRATE.TOTAL_COSTS);
        //        }
        //    }
        //}
    }

    public static class WORKPACKProjectionQueries
    {
        public static IQueryable<WORKPACKProjection> JoinPROGRESSProjectionOnWORKPACKS(
            IQueryable<WORKPACK> WORKPACKS, IQueryable<BASELINE_ITEM> BASELINE_ITEMS, Func<PROGRESS> getPROGRESSFunc, Func<BASELINE> getBASELINEFunc,
            Func<IQueryable<PROGRESS_ITEM>> getPROGRESS_ITEMSFunc, Func<IQueryable<RATE>> getRATESFunc,
            Func<IQueryable<DELIVERABLES_STATUS>> getDELIVERABLES_STATUSESFunc = null,
            bool isBASELINEQueryProcessed = false)
        {
            var PROGRESS = getPROGRESSFunc();

            //IQueryable<PROGRESS_ITEM> LoadPROGRESS_ITEMS;
            //LoadPROGRESS_ITEMS = PROGRESS == null ? getPROGRESS_ITEMSFunc().Where(x => x.GUID_PROGRESS == Guid.Empty).ToArray().AsQueryable() : getPROGRESS_ITEMSFunc().ToArray().AsQueryable();
            IQueryable<BASELINE_ITEMProjection> AllBaselineItems;
            if (PROGRESS == null)
                AllBaselineItems = BASELINE_ITEMProjectionQueries.JoinRATESOnBASELINE_ITEMS(
                    BASELINE_ITEMS.Where(x => x.GUID == Guid.Empty), getBASELINEFunc, getRATESFunc,
                    getDELIVERABLES_STATUSESFunc, true);
            else
                AllBaselineItems = BASELINE_ITEMProjectionQueries.JoinRATESOnBASELINE_ITEMS(BASELINE_ITEMS,
                    getBASELINEFunc, getRATESFunc, getDELIVERABLES_STATUSESFunc, isBASELINEQueryProcessed);

            //IQueryable<PROGRESS_ITEMProjection> reportableItems =
            //    PROGRESS_ITEMProjectionQueries.JoinRATESAndPROGRESS_ITEMSOnBASELINE_ITEMS(BASELINE_ITEMS,
            //        getPROGRESSFunc, getBASELINEFunc, getPROGRESS_ITEMSFunc, getRATESFunc);

            var reportingDate = PROGRESS == null ? new DateTime() : PROGRESS.DATA_DATE;
            return
                WORKPACKS.Select(x => new WORKPACKProjection()
                {
                    GUID = x.GUID,
                    WORKPACK = x,
                    TOTAL_COSTS = AllBaselineItems.Where(y => y.BASELINE_ITEM.GUID_WORKPACK == x.GUID).Sum(z => z.TOTAL_COSTS),
                    TOTAL_UNITS = AllBaselineItems.Where(y => y.BASELINE_ITEM.GUID_WORKPACK == x.GUID).Sum(z => z.BASELINE_ITEM.TOTAL_HOURS)
                    //ReportableObjects = reportableItems.Where(y => y.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_WORKPACK == x.GUID)
                    //.ToArray()
                    //.AsEnumerable()
                });
        }
    }
}
