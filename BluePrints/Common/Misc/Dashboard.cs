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
        IDashboard Parent_Dashboard { get; set; }
        int WBSLevel { get; }
        string Code { get; set; }
        List<Dashboard> Child_Dashboards { get; set; }
        bool IHave_ChildDashboard { get; }
        SummaryStats Summary { get; set; }
    }

    public class Dashboard : IDashboard, IHaveStats
    {
        public IDashboard Parent_Dashboard { get; set; }

        public int WBSLevel { get; }

        public string Code { get; set; }

        public List<Dashboard> Child_Dashboards { get; set; }

        public bool IHave_ChildDashboard => Child_Dashboards != null && Child_Dashboards.Count > 0;

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

    public class Dashboard_Export_Data_Point
    {
        public string Workpack_Name { get; set; }
        public string Department_Name { get; set; }
        public string Discipline_Name { get; set; }
        public string Commodity_Code { get; set; }
        public DateTime Data_Date { get; set; }
        public StatsType Type { get; set; }
        public decimal Units { get; set; }
        public decimal Costs { get; set; }
    }

    public enum StatsType
    {
        Planned, 
        Earned,
        Burned,
        Actual,
        Remaining
    }

    public static class DashboardHelpers
    {
        public static List<Dashboard_Export_Data_Point> BuildExportData(List<Dashboard> Workpack_Dashboards)
        {
            List<Dashboard_Export_Data_Point> export_data = new List<Dashboard_Export_Data_Point>();
            IEnumerable<Dashboard> commodity_code_dashboards = Workpack_Dashboards.SelectMany(x => x.Child_Dashboards.SelectMany(department => department.Child_Dashboards.SelectMany(discipline => discipline.Child_Dashboards)));
            foreach(Dashboard commodity_code_dashboard in commodity_code_dashboards)
            {
                if(commodity_code_dashboard.Stats != null)
                {
                    SummaryStats summary = (SummaryStats)commodity_code_dashboard.Stats;
                    if (summary.Budgeted != null)
                        export_data.AddRange(buildExportDataByType(commodity_code_dashboard, StatsType.Planned, summary.Budgeted.DataPoints));

                    if (summary.Earned != null)
                        export_data.AddRange(buildExportDataByType(commodity_code_dashboard, StatsType.Earned, summary.Earned.DataPoints));

                    if (summary.Burned != null)
                        export_data.AddRange(buildExportDataByType(commodity_code_dashboard, StatsType.Burned, summary.Burned.DataPoints));

                    if (summary.Actual != null)
                        export_data.AddRange(buildExportDataByType(commodity_code_dashboard, StatsType.Actual, summary.Actual.DataPoints));

                    if (summary.Remaining != null)
                        export_data.AddRange(buildExportDataByType(commodity_code_dashboard, StatsType.Remaining, summary.Remaining.DataPoints));
                }
            }

            return export_data;
        }

        private static List<Dashboard_Export_Data_Point> buildExportDataByType(Dashboard commodity_code_dashboard, StatsType stats_type, IEnumerable<DataPoint> data_points)
        {
            List<Dashboard_Export_Data_Point> export_data_by_type = new List<Dashboard_Export_Data_Point>();
            if (data_points == null)
                return export_data_by_type;

            foreach (DataPoint data_point in data_points)
            {
                Dashboard_Export_Data_Point new_export_data = new Dashboard_Export_Data_Point();
                new_export_data.Type = stats_type;
                new_export_data.Data_Date = data_point.ProgressDate;
                new_export_data.Units = data_point.Units;
                new_export_data.Costs = data_point.Costs;
                new_export_data.Commodity_Code = commodity_code_dashboard.Code;
                new_export_data.Discipline_Name = commodity_code_dashboard.Parent_Dashboard.Code;
                new_export_data.Department_Name = commodity_code_dashboard.Parent_Dashboard.Parent_Dashboard.Code;
                new_export_data.Workpack_Name = commodity_code_dashboard.Parent_Dashboard.Parent_Dashboard.Parent_Dashboard.Code;
                export_data_by_type.Add(new_export_data);
            }

            return export_data_by_type;
        }

        public static void SubDivideDashboardStats(this IDashboard dashboard, IEnumerable<ExoDataPoint> burned_data_points, Func<IReportable, string> reportable_selector, Func<ExoDataPoint, string> exo_selector)
        {
            IEnumerable<string> reportable_codes = dashboard.Summary.Reportables.Select(reportable_selector);
            IEnumerable<string> exo_codes = burned_data_points.Select(exo_selector);
            List<string> loop_codes = new List<string>(reportable_codes);
            loop_codes.AddRange(exo_codes);
            HashSet<string> unique_codes = new HashSet<string>(loop_codes);

            List<Dashboard> child_dashboard = new List<Dashboard>();
            foreach (string unique_code in unique_codes.OrderBy(x => x))
            {
                Dashboard new_dashboard = create_dashboard(dashboard, unique_code, dashboard.Summary, burned_data_points, x => reportable_selector(x) == unique_code, x => exo_selector(x) == unique_code);
                if(new_dashboard != null)
                    child_dashboard.Add(new_dashboard);
            }

            dashboard.Child_Dashboards = child_dashboard.Where(x => x.Stats != null).ToList();
        }

        private static Dashboard create_dashboard(IDashboard parent_dashboard, string code, SummaryStats summary_stats, IEnumerable<ExoDataPoint> burned_data_points, Func<IReportable, bool> reportable_predicate, Func<ExoDataPoint, bool> exo_predicate)
        {
            Dashboard dashboard = new Dashboard() { Code = code, Parent_Dashboard = parent_dashboard };
            SummaryStats stats;
            if (summary_stats != null)
                stats = SummaryStatsHelpers.Group_Summary_Stats(summary_stats, reportable_predicate, exo_predicate);
            else
                stats = null;

            if (stats != null)
                dashboard.Stats = stats;
            else
                return null;
            
            return dashboard;
        }

        public static List<Dashboard> ProjectDashboardHierarchicalBuilder(ProjectSummaryStats project_summary_stats)
        {
            Dashboard project_dashboard = new Dashboard();
            project_dashboard.Summary = project_summary_stats;

            IEnumerable<ExoDataPoint> burned_data_points = project_summary_stats.GetBurnedDataPoints();
            project_dashboard.SubDivideDashboardStats(burned_data_points, x => x.Workpack_Name, x => x.Workpack_Name);

            List<Dashboard> workpack_dashboards = new List<Dashboard>();
            //child dashboards are now subdivided into workpack dashboard
            foreach (Dashboard workpack_dashboard in project_dashboard.Child_Dashboards)
            {
                workpack_dashboard.SubDivideDashboardStats(burned_data_points, x => x.Department_Code, x => x.Department_Code);
                workpack_dashboards.Add(workpack_dashboard);
            }

            //child dashboards are now subdivided into department dashboard
            List<Dashboard> department_dashboards = new List<Dashboard>();
            foreach (Dashboard department_dashboard in workpack_dashboards.SelectMany(x => x.Child_Dashboards))
            {
                department_dashboard.SubDivideDashboardStats(burned_data_points, x => x.Discipline_Code, x => x.Discipline_Code);
                department_dashboards.Add(department_dashboard);
            }

            //child dashboards are now subdivided into discipline dashboard
            foreach (Dashboard discipline_dashboard in department_dashboards.SelectMany(x => x.Child_Dashboards))
            {
                discipline_dashboard.SubDivideDashboardStats(burned_data_points, x => x.Commodity_Code, x => x.Commodity_Code);
            }

            return project_dashboard.Child_Dashboards;
        }
    }
}
