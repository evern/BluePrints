using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
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
        List<DashboardTreeStructure> Child_Dashboards { get; set; }
        bool IHave_ChildDashboard { get; }
        SummaryStats Summary { get; set; }
    }

    public class DashboardTreeStructure : IDashboard, IHaveStats
    {
        public IDashboard Parent_Dashboard { get; set; }

        public int WBSLevel { get; }

        public string Code { get; set; }

        public List<DashboardTreeStructure> Child_Dashboards { get; set; }

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

    /// <summary>
    /// Dashboard workpack for splitting workpack into department, discipline and commodity specific for reporting
    /// </summary>
    public class DashboardFlatStructure : DashboardTreeStructure, IHaveStats
    {
        public string WorkpackCode { get; set; }
        public string AreaCode { get; set; }
        public string SubAreaCode { get; set; }
        public string DepartmentCode { get; set; }
        public string DisciplineCode { get; set; }
        public string CommodityCode { get; set; }
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
        public decimal Actual_Costs { get; set; }
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
        public static List<Dashboard_Export_Data_Point> BuildExportData(List<DashboardTreeStructure> Workpack_Dashboards)
        {
            List<Dashboard_Export_Data_Point> export_data = new List<Dashboard_Export_Data_Point>();
            IEnumerable<DashboardTreeStructure> commodity_code_dashboards = Workpack_Dashboards.SelectMany(x => x.Child_Dashboards.SelectMany(department => department.Child_Dashboards.SelectMany(discipline => discipline.Child_Dashboards)));
            foreach (DashboardTreeStructure commodity_code_dashboard in commodity_code_dashboards)
            {
                if (commodity_code_dashboard.Stats != null)
                {
                    SummaryStats summary = (SummaryStats)commodity_code_dashboard.Stats;
                    if (summary.Budgeted != null)
                        export_data.AddRange(buildExportDataByType(commodity_code_dashboard, StatsType.Planned, summary.Budgeted.DataPoints));

                    if (summary.Earned != null)
                        export_data.AddRange(buildExportDataByType(commodity_code_dashboard, StatsType.Earned, summary.Earned.DataPoints));

                    if (summary.Burned != null)
                        export_data.AddRange(buildExportDataByType(commodity_code_dashboard, StatsType.Burned, summary.Burned.DataPoints, summary.Actual.DataPoints));

                    //if (summary.Actual != null)
                    //    export_data.AddRange(buildExportDataByType(commodity_code_dashboard, StatsType.Actual, summary.Actual.DataPoints));

                    if (summary.Remaining != null)
                        export_data.AddRange(buildExportDataByType(commodity_code_dashboard, StatsType.Remaining, summary.Remaining.DataPoints));
                }
            }

            return export_data;
        }


        private static List<Dashboard_Export_Data_Point> buildExportDataByType(DashboardTreeStructure commodity_code_dashboard, StatsType stats_type, IEnumerable<ViewModel.Reporting.DataPoint> data_points, IEnumerable<ViewModel.Reporting.DataPoint> actual_data_points = null)
        {
            List<Dashboard_Export_Data_Point> export_data_by_type = new List<Dashboard_Export_Data_Point>();
            if (data_points == null)
                return export_data_by_type;

            foreach (ViewModel.Reporting.DataPoint data_point in data_points)
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

                if (actual_data_points != null)
                {
                    ViewModel.Reporting.DataPoint current_period_actual = actual_data_points.FirstOrDefault(x => x.ProgressDate == data_point.ProgressDate);
                    if (current_period_actual != null)
                        new_export_data.Actual_Costs = current_period_actual.Costs;
                }

                export_data_by_type.Add(new_export_data);
            }

            return export_data_by_type;
        }

        public static int getSubDivideMaxProgress(this IDashboard dashboard, IEnumerable<ExoDataPoint> burned_data_points, Func<IReportable, string> reportable_selector, Func<ExoDataPoint, string> exo_selector)
        {
            IEnumerable<string> reportable_codes = dashboard.Summary.Reportables.Select(reportable_selector);
            IEnumerable<string> exo_codes = burned_data_points.Select(exo_selector);
            List<string> loop_codes = new List<string>(reportable_codes);
            loop_codes.AddRange(exo_codes);
            HashSet<string> unique_codes = new HashSet<string>(loop_codes);

            return unique_codes.Count;
        }

        public static void SubDivideDashboardStats(this IDashboard dashboard, IEnumerable<ExoDataPoint> burned_data_points, Func<IReportable, string> reportable_selector, Func<ExoDataPoint, string> exo_selector)
        {
            IEnumerable<string> reportable_codes = dashboard.Summary.Reportables.Select(reportable_selector);
            IEnumerable<string> exo_codes = burned_data_points.Select(exo_selector);
            List<string> loop_codes = new List<string>(reportable_codes);
            loop_codes.AddRange(exo_codes);
            HashSet<string> unique_codes = new HashSet<string>(loop_codes);

            List<DashboardTreeStructure> child_dashboard = new List<DashboardTreeStructure>();
            foreach (string unique_code in unique_codes.OrderBy(x => x))
            {
                DashboardTreeStructure new_dashboard = create_dashboard(dashboard, unique_code, dashboard.Summary, burned_data_points, x => reportable_selector(x) == unique_code, x => exo_selector(x) == unique_code);
                if(new_dashboard != null)
                    child_dashboard.Add(new_dashboard);
            }

            dashboard.Child_Dashboards = child_dashboard.Where(x => x.Stats != null).ToList();
        }

        private static DashboardTreeStructure create_dashboard(IDashboard parent_dashboard, string code, SummaryStats summary_stats, IEnumerable<ExoDataPoint> burned_data_points, Func<IReportable, bool> reportable_predicate, Func<ExoDataPoint, bool> exo_predicate)
        {
            DashboardTreeStructure dashboard = new DashboardTreeStructure() { Code = code, Parent_Dashboard = parent_dashboard };
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

        public static List<DashboardTreeStructure> ProjectDashboardHierarchicalBuilder(ProjectSummaryStats project_summary_stats)
        {
            DashboardTreeStructure project_dashboard = new DashboardTreeStructure();
            project_dashboard.Summary = project_summary_stats;

            IEnumerable<ExoDataPoint> burned_data_points = project_summary_stats.GetBurnedDataPoints();
            int maxProgress = project_dashboard.getSubDivideMaxProgress(burned_data_points, x => x.Workpack_Name, x => x.Workpack_Name);

            project_dashboard.SubDivideDashboardStats(burned_data_points, x => x.Workpack_Name, x => x.Workpack_Name);

            LoadingScreenManager.ShowLoadingScreen(maxProgress);
            //child dashboards are now subdivided into workpack dashboard
            foreach (DashboardTreeStructure workpack_dashboard in project_dashboard.Child_Dashboards)
            {
                workpack_dashboard.SubDivideDashboardStats(burned_data_points, x => x.Department_Code, x => x.Department_Code);

                foreach (DashboardTreeStructure department_dashboard in workpack_dashboard.Child_Dashboards)
                {
                    department_dashboard.SubDivideDashboardStats(burned_data_points, x => x.Discipline_Code, x => x.Discipline_Code);

                    //child dashboards are now subdivided into discipline dashboard
                    foreach (DashboardTreeStructure discipline_dashboard in department_dashboard.Child_Dashboards)
                    {
                        discipline_dashboard.SubDivideDashboardStats(burned_data_points, x => x.Commodity_Code, x => x.Commodity_Code);
                    }
                }

                LoadingScreenManager.Progress();
            }

            LoadingScreenManager.CloseLoadingScreen();
            return project_dashboard.Child_Dashboards;
        }

        public static List<DashboardFlatStructure> ProjectDashboardSummaryBuilder(ProjectSummaryStats project_summary_stats, out List<DashboardTreeStructure> hierarchicalDashboards, IEnumerable<WORKPACK> WORKPACKCollection)
        {
            List<DashboardFlatStructure> flatDashboards = new List<DashboardFlatStructure>();
            hierarchicalDashboards = ProjectDashboardHierarchicalBuilder(project_summary_stats);

            foreach(DashboardTreeStructure workpack_dashboard in hierarchicalDashboards.OrderBy(x => x.Code))
            {
                foreach (DashboardTreeStructure department_dashboard in workpack_dashboard.Child_Dashboards.OrderBy(x => x.Code))
                {
                    WORKPACK workpack = WORKPACKCollection.FirstOrDefault(x => x.INTERNAL_NAME1 == workpack_dashboard.Code);
                    string areaCode = string.Empty;

                    if(workpack!= null)
                        areaCode = workpack.AREA == null ? string.Empty : workpack.AREA.INTERNAL_NUM;
                    if(department_dashboard.Child_Dashboards.Count == 0)
                    {
                        DashboardFlatStructure departmentLevelDashboard = new DashboardFlatStructure();
                        departmentLevelDashboard.WorkpackCode = workpack_dashboard.Code;
                        departmentLevelDashboard.AreaCode = areaCode;
                        departmentLevelDashboard.DepartmentCode = department_dashboard.Code;
                        departmentLevelDashboard.DisciplineCode = string.Empty;
                        departmentLevelDashboard.Stats = department_dashboard.Stats;
                        flatDashboards.Add(departmentLevelDashboard);
                    }
                    else
                    {
                        foreach (DashboardTreeStructure discipline_dashboard in department_dashboard.Child_Dashboards.OrderBy(x => x.Code))
                        {
                            DashboardFlatStructure commodityLevelDashboard = new DashboardFlatStructure();
                            commodityLevelDashboard.WorkpackCode = workpack_dashboard.Code;
                            commodityLevelDashboard.AreaCode = areaCode;
                            commodityLevelDashboard.DepartmentCode = department_dashboard.Code;
                            commodityLevelDashboard.DisciplineCode = discipline_dashboard.Code;
                            commodityLevelDashboard.Stats = discipline_dashboard.Stats;
                            flatDashboards.Add(commodityLevelDashboard);

                            //foreach(DashboardTreeStructure commodity_dashboard in discipline_dashboard.Child_Dashboards.OrderBy(x => x.Code))
                            //{

                            //}
                        }
                    }
                }
            }

            return flatDashboards;
        }
    }
}
