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
        public PROJECT PROJECT { get; set; }
    }

    //public static class USER_DashboardQueries
    //{
    //    public static IQueryable<USER_Dashboard> SummarizeUserDashboard(IQueryable<PROJECT> PROJECTS,
    //        Func<IEnumerable<PROGRESS>> getLivePROGRESSESFunc, 
    //        Func<IEnumerable<BASELINE>> getLiveBASELINESFunc)
    //    {
            
    //        return newWORKPACKDashboards.AsQueryable();
    //    }
    //}
}