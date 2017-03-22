using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.P6EntitiesDataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    public class USER_Dashboard : PROJECTSummary, IHaveGUID
    {
        public Guid GUID { get; set; }
        public PROGRESS_ITEMProjection PROGRESS_ITEMProjection { get; set; }
    }

    //public static class USER_DashboardQueries
    //{
    //    public static IQueryable<USER_Dashboard> SummarizeUserDashboard(IQueryable<BASELINE_ITEM> BASELINE_ITEMS,
    //        Func<IQueryable<PROJECT_Dashboard>> getPROJECT_DashboardsFunc)
    //    {
    //        IQueryable<PROJECT_Dashboard> projectWORKPACKDashboards = getPROJECT_DashboardsFunc();

    //        //get all reportable objects within all projects, IQuerable<PROJECT_Dashboard> must have projection on retrieving user guid only progress_items
    //        IQueryable<ReportableObject> userReportableObjects = projectWORKPACKDashboards.SelectMany(x => x.ReportableObjects);
    //        IQueryable<PROGRESS_ITEMProjection> userPROGRESS_ITEMS = userReportableObjects.Select(x => (PROGRESS_ITEMProjection)x);

    //        return newWORKPACKDashboards.AsQueryable();
    //    }
    //}
}