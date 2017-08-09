using BluePrints.Common.Base;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.Common.Projections
{
    public class WORKPACK_Dashboard : BluePrintsProjectionBase<WORKPACK>, IHaveStats
    {
        public ProgressStats Stats { get; set; }
        public IEnumerable<AREA> AvailableSubAreas { get; set; }


        public void SetAvailableSubAreas(IEnumerable<AREA> SUBAREACollection)
        {
            AvailableSubAreas = SUBAREACollection.Where(x => x.GUID_PARENT == Entity.GUID_DAREA);
            this.RaisePropertyChanged();
        }

        public void GroupProjectStats(SummaryStats project_summary_stats)
        {
            Stats = SummaryStatsHelpers.Group_Summary_Stats(project_summary_stats, x => x.Workpack_Guid == Entity.EntityKey, x => x.WorkpackName == Entity.INTERNAL_NAME1);
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
        public static IQueryable<WORKPACK_Dashboard> Workpack_Dashboard(IQueryable<WORKPACK> WORKPACKS,
            IEnumerable<PROGRESS> PROGRESSES, BASELINE BASELINE, ESTIMATION_DIRECT ESTIMATION_DIRECT, 
            IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS,
            IEnumerable<RATE> RATES,
            IEnumerable<DELIVERABLES_STATUS> DELIVERABLES_STATUSES)
        {
            var projectDashboard = DashboardQueries.Single_Project_DashboardTransformation(BASELINE.PROJECT, BASELINE, ESTIMATION_DIRECT, PROGRESSES, PROGRESS_ITEMS, RATES, null, true);
            return Workpack_Dashboard_Summary(WORKPACKS, projectDashboard);
        }

        public static IQueryable<WORKPACK_Dashboard> Workpack_Dashboard_Summary(IQueryable<WORKPACK> WORKPACKS,
            PROJECT_Dashboard projectDashboard, IEnumerable<AREA> subAreaCollection = null)
        {
            IEnumerable<WORKPACK_Dashboard> workpack_dashboards = WORKPACKS.Where(x => x.GUID_PROJECT == projectDashboard.EntityKey).Select(x => new WORKPACK_Dashboard() {EntityKey = x.GUID, Entity = x});
            List<WORKPACK_Dashboard> newWORKPACKDashboards = workpack_dashboards.ToList();
            newWORKPACKDashboards.ForEach(x => x.GroupProjectStats((SummaryStats)projectDashboard.Stats));
            if (subAreaCollection != null)
                newWORKPACKDashboards.ForEach(x => x.SetAvailableSubAreas(subAreaCollection));

            return newWORKPACKDashboards.AsQueryable();
        }
    }
}