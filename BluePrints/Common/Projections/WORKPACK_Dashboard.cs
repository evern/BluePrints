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
    public class WORKPACK_Dashboard : ProjectionBase<WORKPACK>, IHaveStats
    {
        public ProgressStats Stats { get; set; }

        public void GroupProjectStats(ProjectSummaryStats projectStats)
        {
            Stats = projectStats.GroupBurnedStatsByWorkpack(this.Entity);
        }
        #region WORKPACK Mapping

        public bool IsGetModifiedWORKPACK_ASSIGNMENTS { get; set; }
        public ICollection<WORKPACK_ASSIGNMENT> ObservableWORKPACK_ASSIGNMENTS
        {
            get
            {
                return
                    Entity.WORKPACK_ASSIGNMENT.Where(x => x.ISMODIFIEDBASELINE == IsGetModifiedWORKPACK_ASSIGNMENTS).OrderBy(x => x.LOW_VALUE)
                        .ToList();
            }
        }

        public virtual ICollection<BASELINE_ITEM> BASELINE_ITEMS { get; set; }

        public decimal ASSIGNED_UNITS
        {
            get
            {
                return Stats.totalUnits * ObservableWORKPACK_ASSIGNMENTS.Sum(x => (x.HIGH_VALUE - x.LOW_VALUE) + 0.01m);
            }
        }

        public decimal ASSIGNED_PERCENTAGE
        {
            get
            {
                return ObservableWORKPACK_ASSIGNMENTS.Sum(x => (x.HIGH_VALUE - x.LOW_VALUE) + 0.01m);
            }
        }
        #endregion
    }

    public static class WORKPACK_DashboardQueries
    {
        public static IQueryable<WORKPACK_Dashboard> SummarizeWORKPACKDashboard(IQueryable<WORKPACK> WORKPACKS,
            PROJECT_Dashboard projectDashboard)
        {
            IEnumerable<WORKPACK_Dashboard> projectWORKPACKDashboards =
                WORKPACKS.Where(x => x.GUID_PROJECT == projectDashboard.GUID)
                    .Select(x => new WORKPACK_Dashboard() {GUID = x.GUID, Entity = x});
            List<WORKPACK_Dashboard> newWORKPACKDashboards = projectWORKPACKDashboards.ToList();
            newWORKPACKDashboards.ForEach(x => x.GroupProjectStats((ProjectSummaryStats)projectDashboard.Stats));

            return newWORKPACKDashboards.AsQueryable();
        }

        public static IQueryable<WORKPACK_Dashboard> SummarizeWORKPACKDashboard(IQueryable<WORKPACK> WORKPACKS,
            Func<PROGRESS> getPROGRESSFunc, Func<BASELINE> getBASELINEFunc,
            Func<IEnumerable<BASELINE_ITEM>> getBASELINE_ITEMFunc,
            Func<IEnumerable<PROGRESS_ITEM>> getPROGRESS_ITEMSFunc,
            Func<IEnumerable<RATE>> getRATESFunc,
            Func<IEnumerable<DELIVERABLES_STATUS>> getDELIVERABLES_STATUSESFunc,
            bool getModifiedWORKPACK_ASSIGNMENT)
        {
            var returnWORKPACK_Dashboard = new List<WORKPACK_Dashboard>();

            var projectDashboard =
                PROJECT_DashboardQueries.SummarizeSinglePROJECTDashboard(getBASELINEFunc().PROJECT, getPROGRESSFunc,
                    getPROGRESS_ITEMSFunc, getBASELINE_ITEMFunc, getBASELINEFunc, getRATESFunc, getDELIVERABLES_STATUSESFunc, true);

            return SummarizeWORKPACKDashboard(WORKPACKS, projectDashboard);
        }
    }
}