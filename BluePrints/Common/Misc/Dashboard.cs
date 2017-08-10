using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Misc
{
    public interface IDashboard
    {
        int WBSLevel { get; }
        string Code { get; set; }
        List<Dashboard> Child_Dashboard { get; set; }
        bool IHave_ChildDashboard { get; }
        SummaryStats Summary { get; set; }
    }

    public class Dashboard : IDashboard, IHaveStats
    {
        public int WBSLevel { get; }

        public string Code { get; set; }

        public List<Dashboard> Child_Dashboard { get; set; }

        public bool IHave_ChildDashboard { get; }

        public SummaryStats Summary { get; set; }

        public ProgressStats Stats
        {
            get => Summary;
            set
            {
                if (value.GetType() == typeof(SummaryStats))
                    Summary = (SummaryStats)value;
            }
        }
    }

    public static class DashboardExtensions
    {
        public static void SubDivideDashboardStats(this IDashboard dashboard, IEnumerable<ExoDataPoint> burned_data_points, Func<IReportable, string> reportable_selector, Func<ExoDataPoint, string> exo_selector)
        {
            IEnumerable<string> reportable_codes = dashboard.Summary.Reportables.Select(reportable_selector);
            IEnumerable<string> exo_codes = burned_data_points.Select(exo_selector);
            List<string> loop_codes = new List<string>(reportable_codes);
            loop_codes.AddRange(exo_codes);
            HashSet<string> unique_codes = new HashSet<string>(loop_codes);

            List<Dashboard> child_dashboard = new List<Dashboard>();
            foreach (string unique_code in unique_codes)
            {
                child_dashboard.Add(create_dashboard(unique_code, dashboard.Summary, burned_data_points, x => reportable_selector(x) == unique_code, x => exo_selector(x) == unique_code));
            }

            dashboard.Child_Dashboard = child_dashboard.Where(x => x.Stats != null).ToList();
        }

        private static Dashboard create_dashboard(string code, SummaryStats summary_stats, IEnumerable<ExoDataPoint> burned_data_points, Func<IReportable, bool> reportable_predicate, Func<ExoDataPoint, bool> exo_predicate)
        {
            Dashboard dashboard = new Dashboard() { Code = code };
            if (summary_stats != null)
                dashboard.Stats = SummaryStatsHelpers.Group_Summary_Stats(summary_stats, reportable_predicate, exo_predicate);
            return dashboard;
        }

        public static List<Dashboard> ProjectDashboardHierarchicalBuilder(ProjectSummaryStats project_summary_stats)
        {
            List<Dashboard> phase_dashboards = Construct_Phase_Dashboards(project_summary_stats);
            IEnumerable<ExoDataPoint> burned_data_points = project_summary_stats.GetBurnedDataPoints();

            foreach(Dashboard phase_dashboard in phase_dashboards)
            {
                phase_dashboard.SubDivideDashboardStats(burned_data_points, x => x.Workpack_Name, x => x.Workpack_Name);
            }

            List<Dashboard> workpack_dashboards = new List<Dashboard>();
            //child dashboards are now subdivided into department dashboard
            foreach(Dashboard workpack_dashboard in phase_dashboards.SelectMany(x => x.Child_Dashboard))
            {
                workpack_dashboard.SubDivideDashboardStats(burned_data_points, x => x.Department_Code, x => x.Department_Code);
                workpack_dashboards.Add(workpack_dashboard);
            }

            List<Dashboard> department_dashboards = new List<Dashboard>();
            foreach(Dashboard department_dashboard in workpack_dashboards.SelectMany(x => x.Child_Dashboard))
            {
                department_dashboard.SubDivideDashboardStats(burned_data_points, x => x.Discipline_Code, x => x.Discipline_Code);
                department_dashboards.Add(department_dashboard);
            }

            foreach(Dashboard discipline_dashboard in department_dashboards.SelectMany(x => x.Child_Dashboard))
            {
                discipline_dashboard.SubDivideDashboardStats(burned_data_points, x => x.Commodity_Code, x => x.Commodity_Code);
            }

            return phase_dashboards;
        }

        public static List<Dashboard> Construct_Phase_Dashboards(ProjectSummaryStats project_summary_stats)
        {
            List<string> phases = new List<string>();
            phases.Add(BluePrintsResources.Default_Design_Phase);
            phases.Add(BluePrintsResources.Alternate_Design_Phase);
            phases.Add(BluePrintsResources.Default_Construction_Phase);

            List<Dashboard> phase_dashboards = new List<Dashboard>();
            foreach (string phase in phases)
            {
                Dashboard new_phase_dashboard = new Dashboard() { Code = phase };
                if (project_summary_stats != null)
                    new_phase_dashboard.Stats = SummaryStatsHelpers.Group_Summary_Stats(project_summary_stats, x => x.Phase_Code == phase, x => x.PhaseCode == phase);

                if (new_phase_dashboard.Stats != null)
                {
                    phase_dashboards.Add(new_phase_dashboard);
                }
            }

            return phase_dashboards;
        }
    }
}
