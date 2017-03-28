using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.DataModel;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.Data.Attributes;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    public class PROGRESS_ITEMProjection : ReportableObject, IHaveGUID
    {
        public PROGRESS_ITEMProjection()
        {
        }

        public decimal MAX_PERCENTAGE_WITH_DELIVERABLE_STATUS_LIMIT
        {
            get
            {
                if (BASELINE_ITEMJoinRATE.DELIVERABLE_STATUS == null)
                {
                    return MaxPercentage;
                }
                else
                {
                    if (MaxPercentage < BASELINE_ITEMJoinRATE.DELIVERABLE_STATUS.MAX_PERCENTAGE)
                        return MaxPercentage;
                    else
                        return BASELINE_ITEMJoinRATE.DELIVERABLE_STATUS.MAX_PERCENTAGE;
                }
            }
        }

        public Guid GUID { get; set; }
    }

    public static class PROGRESS_ITEMProjectionQueries
    {
        public static IQueryable<PROGRESS_ITEMProjection> JoinRATESAndPROGRESS_ITEMSOnBASELINE_ITEMS(
            IQueryable<BASELINE_ITEM> BASELINE_ITEMS, Func<PROGRESS> getPROGRESSFunc, Func<BASELINE> getBASELINEFunc,
            Func<IEnumerable<PROGRESS_ITEM>> getPROGRESS_ITEMSFunc, Func<IEnumerable<RATE>> getRATESFunc,
            Func<IEnumerable<DELIVERABLES_STATUS>> getDELIVERABLES_STATUSESFunc,
            bool isBASELINEQueryProcessed = false)
        {
            var PROGRESS = getPROGRESSFunc();

            IEnumerable<PROGRESS_ITEM> LoadPROGRESS_ITEMS;
            if (PROGRESS == null)
                LoadPROGRESS_ITEMS = new List<PROGRESS_ITEM>();
            else
                LoadPROGRESS_ITEMS = getPROGRESS_ITEMSFunc();

            IQueryable<BASELINE_ITEMProjection> BASELINE_ITEMJoinRATES;
            if (PROGRESS == null)
                BASELINE_ITEMJoinRATES = new List<BASELINE_ITEMProjection>().AsQueryable();
            else
                BASELINE_ITEMJoinRATES = BASELINE_ITEMProjectionQueries.JoinRATESOnBASELINE_ITEMS(BASELINE_ITEMS,
                    getBASELINEFunc, getRATESFunc, getDELIVERABLES_STATUSESFunc, isBASELINEQueryProcessed);

            var reportingDate = PROGRESS == null ? new DateTime() : PROGRESS.DATA_DATE;

            return
                BASELINE_ITEMJoinRATES.ToArray().Select(
                        x =>
                            new PROGRESS_ITEMProjection()
                            {
                                GUID = x.GUID,
                                BASELINE_ITEMJoinRATE = x,
                                ReportingDataDate = reportingDate,
                                PROGRESS_ITEMS = LoadPROGRESS_ITEMS.Where(y => y.GUID_ORIBASEITEM == x.BASELINE_ITEM.GUID_ORIGINAL)
                            }).AsQueryable();
        }
    }
}