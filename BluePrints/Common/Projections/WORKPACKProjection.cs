using BaseModel.Attributes;
using BluePrints.Common.Base;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.Common.Projections
{
    [ConstraintAttributes("Entity.GUID_PROJECT, Entity.INTERNAL_NAME1")]
    [RequiredAttributes("Entity.GUID_DDEPARTMENT, Entity.GUID_DDISCIPLINE")]
    public class WORKPACKProjection : BluePrintsProjectionBase<WORKPACK>
    {
        public decimal TOTAL_UNITS { get; set; }

        public decimal TOTAL_COSTS { get; set; }

        public void Update()
        {
            RaisePropertiesChanged();
        }
    }

    public static class WORKPACKProjectionQueries
    {
        public static IQueryable<WORKPACKProjection> JoinPROGRESSProjectionOnWORKPACKS(
            IQueryable<WORKPACK> WORKPACKS, IEnumerable<BASELINE_ITEM> BASELINE_ITEMS, Func<PROGRESS> getPROGRESSFunc, Func<BASELINE> getBASELINEFunc,
            Func<IEnumerable<PROGRESS_ITEM>> getPROGRESS_ITEMSFunc, Func<IEnumerable<RATE>> getRATESFunc,
            Func<IEnumerable<DELIVERABLES_STATUS>> getDELIVERABLES_STATUSESFunc,
            Func<IEnumerable<AREA>> getSUBAREAFunc = null,
            bool isBASELINEQueryProcessed = false)
        {
            var PROGRESS = getPROGRESSFunc();

            //IQueryable<PROGRESS_ITEM> LoadPROGRESS_ITEMS;
            //LoadPROGRESS_ITEMS = PROGRESS == null ? getPROGRESS_ITEMSFunc().Where(x => x.GUID_PROGRESS == Guid.Empty).ToArray().AsQueryable() : getPROGRESS_ITEMSFunc().ToArray().AsQueryable();
            IQueryable<BASELINE_ITEMProjection> AllBaselineItems;
            if (PROGRESS == null)
                AllBaselineItems = new List<BASELINE_ITEMProjection>().AsQueryable();
            else
                AllBaselineItems = BASELINE_ITEMProjectionQueries.BASELINE_ITEMProjectionQuery(BASELINE_ITEMS.AsQueryable(),
                    getBASELINEFunc, getRATESFunc, getDELIVERABLES_STATUSESFunc, getSUBAREAFunc, isBASELINEQueryProcessed);

            IEnumerable<AREA> SUBAREAS;
            if (getSUBAREAFunc == null)
                SUBAREAS = new List<AREA>();
            else
                SUBAREAS = getSUBAREAFunc();

            //IQueryable<PROGRESS_ITEMProjection> reportableItems =
            //    PROGRESS_ITEMProjectionQueries.JoinRATESAndPROGRESS_ITEMSOnBASELINE_ITEMS(BASELINE_ITEMS,
            //        getPROGRESSFunc, getBASELINEFunc, getPROGRESS_ITEMSFunc, getRATESFunc);

            var reportingDate = PROGRESS == null ? new DateTime() : PROGRESS.DATA_DATE;
            return
                WORKPACKS.ToArray().Select(x => new WORKPACKProjection()
                {
                    EntityKey = x.GUID,
                    Entity = x,
                    TOTAL_COSTS = AllBaselineItems.Where(y => y.Entity.GUID_WORKPACK == x.GUID).Sum(z => z.TOTAL_COSTS),
                    TOTAL_UNITS = AllBaselineItems.Where(y => y.Entity.GUID_WORKPACK == x.GUID).Sum(z => z.Entity.TOTAL_HOURS)
                    //ReportableObjects = reportableItems.Where(y => y.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_WORKPACK == x.GUID)
                    //.ToArray()
                    //.AsEnumerable()
                }).AsQueryable();
        }
    }
}
