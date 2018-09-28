using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public string PhaseCode
        {
            get
            {
                if (Code == string.Empty)
                    return string.Empty;

                List<string> codePartition = Code.Split('-').ToList();
                if (codePartition.Count < 4)
                    return string.Empty;

                return codePartition[3];
            }
        }

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
    /// Dashboard subjob for splitting subjob into department, discipline and commodity specific for reporting
    /// </summary>
    public class DashboardFlatStructure : DashboardTreeStructure, IHaveStats
    {
        public PhaseType? Phase { get; set; }
        public string SubjobCode { get; set; }
        public string AreaCode { get; set; }
        public string SubAreaCode { get; set; }
        //public string DepartmentCode { get; set; }
        public string DisciplineCode { get; set; }
        public string CommodityCode { get; set; }
        public string Variation_Code { get; set; }

        //This is for dashboard that only have material and po stats
        public bool ShouldHide { get; set; }
    }

    public class Dashboard_Export_Data_Point
    {
        public string User { get; set; }
        public string Role { get; set; }
        public string Subjob_Name { get; set; }
        //public string Department_Name { get; set; }
        public string Discipline_Name { get; set; }
        public string Commodity_Code { get; set; }
        public DateTime Data_Date { get; set; }
        public StatsType Type { get; set; }
        public decimal Units { get; set; }
        public decimal Costs { get; set; }
        public decimal Actual_Costs { get; set; }
    }

    public class Deliverable_Export_Data_Point
    {

    }

    public enum StatsType
    {
        Planned, 
        Earned,
        Burned,
        Actual,
        Remaining,
        RemainingActual
    }

    public static class DashboardHelpers
    {
        public static List<Dashboard_Export_Data_Point> BuildExportData(IEnumerable<BASELINE_ITEMProgress> deliverables)
        {
            List<Dashboard_Export_Data_Point> export_data = new List<Dashboard_Export_Data_Point>();
            foreach (BASELINE_ITEMProgress deliverable in deliverables)
            {
                if (deliverable.Stats != null)
                {
                    ProgressStats summary = deliverable.Stats;
                    if (summary.Budgeted != null)
                        export_data.AddRange(buildExportDataByType(deliverable, StatsType.Planned, summary.Budgeted.DataPoints));

                    if (summary.Earned != null)
                        export_data.AddRange(buildExportDataByType(deliverable, StatsType.Earned, summary.Earned.DataPoints));

                    if (summary.Remaining != null)
                        export_data.AddRange(buildExportDataByType(deliverable, StatsType.Remaining, summary.Remaining.DataPoints));
                }
            }

            return export_data;
        }

        public static List<Dashboard_Export_Data_Point> BuildExportData(List<DashboardTreeStructure> Subjob_Dashboards)
        {
            List<Dashboard_Export_Data_Point> export_data = new List<Dashboard_Export_Data_Point>();
            IEnumerable<DashboardTreeStructure> commodity_code_dashboards = Subjob_Dashboards.SelectMany(x => x.Child_Dashboards.SelectMany(discipline => discipline.Child_Dashboards));

            bool isDisciplineDataPointsGathered = false;
            bool isSubjobDataPointsGathered = false;
            foreach (DashboardTreeStructure subjob_dashboard in Subjob_Dashboards)
            {
                isSubjobDataPointsGathered = false;
                foreach (DashboardTreeStructure discipline_dashboard in subjob_dashboard.Child_Dashboards)
                {
                    isDisciplineDataPointsGathered = false;
                    foreach (DashboardTreeStructure commodity_dashboard in discipline_dashboard.Child_Dashboards)
                    {
                        SummaryStats summaryCommodity = (SummaryStats)commodity_dashboard.Stats;
                        if (summaryCommodity.Burned != null)
                        {
                            isDisciplineDataPointsGathered = true;
                            isSubjobDataPointsGathered = true;

                            List<Dashboard_Export_Data_Point> burned_data = buildExportDataByType2(commodity_dashboard, commodity_dashboard.Parent_Dashboard.Parent_Dashboard.Code, commodity_dashboard.Parent_Dashboard.Code, summaryCommodity.Burned.DataPoints, StatsType.Burned, summaryCommodity.Actual.DataPoints);
                            if (burned_data.Count > 0)
                            {
                                export_data.AddRange(burned_data);
                            }
                        }
                    }

                    SummaryStats summaryDiscipline = (SummaryStats)discipline_dashboard.Stats;
                    if (!isDisciplineDataPointsGathered && summaryDiscipline.Burned != null)
                    {
                        isDisciplineDataPointsGathered = true;
                        isSubjobDataPointsGathered = true;

                        List<Dashboard_Export_Data_Point> burned_data = buildExportDataByType2(discipline_dashboard, discipline_dashboard.Parent_Dashboard.Code, discipline_dashboard.Code, summaryDiscipline.Burned.DataPoints, StatsType.Burned, summaryDiscipline.Actual.DataPoints);
                        if (burned_data.Count > 0)
                        {
                            export_data.AddRange(burned_data);
                        }
                    }
                }

                SummaryStats summarySubjob = (SummaryStats)subjob_dashboard.Stats;
                if (!isSubjobDataPointsGathered && summarySubjob.Burned != null)
                {
                    List<Dashboard_Export_Data_Point> burned_data = buildExportDataByType2(subjob_dashboard, subjob_dashboard.Code, string.Empty, summarySubjob.Burned.DataPoints, StatsType.Burned, summarySubjob.Actual.DataPoints);
                    if (burned_data.Count > 0)
                    {
                        export_data.AddRange(burned_data);
                    }
                }
            }

            foreach (DashboardTreeStructure commodity_code_dashboard in commodity_code_dashboards)
            {
                if (commodity_code_dashboard.Stats != null)
                {
                    SummaryStats summary = (SummaryStats)commodity_code_dashboard.Stats;
                    if (summary.Budgeted != null)
                        export_data.AddRange(buildExportDataByType(commodity_code_dashboard, StatsType.Planned, summary.Budgeted.DataPoints));

                    if (summary.Earned != null)
                        export_data.AddRange(buildExportDataByType(commodity_code_dashboard, StatsType.Earned, summary.Earned.DataPoints));

                    if (summary.Remaining != null)
                        export_data.AddRange(buildExportDataByType(commodity_code_dashboard, StatsType.Remaining, summary.Remaining.DataPoints));

                    if (summary.RemainingActual != null)
                        export_data.AddRange(buildExportDataByType(commodity_code_dashboard, StatsType.RemainingActual, summary.RemainingActual.DataPoints));
                }
            }

            return export_data;
        }

        private static List<Dashboard_Export_Data_Point> buildExportDataByType2(DashboardTreeStructure commodity_code_dashboard, string subJobName, string disciplineName, IEnumerable<ViewModel.Reporting.DataPoint> data_points, StatsType stats_type, IEnumerable<ViewModel.Reporting.DataPoint> actual_data_points = null)
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
                new_export_data.Subjob_Name = subJobName;
                new_export_data.Discipline_Name = disciplineName;

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
                //new_export_data.Department_Name = commodity_code_dashboard.Parent_Dashboard.Parent_Dashboard.Code;
                new_export_data.Subjob_Name = commodity_code_dashboard.Parent_Dashboard.Parent_Dashboard.Code;

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

        private static List<Dashboard_Export_Data_Point> buildExportDataByType(BASELINE_ITEMProgress deliverable, StatsType stats_type, IEnumerable<ViewModel.Reporting.DataPoint> data_points, IEnumerable<ViewModel.Reporting.DataPoint> actual_data_points = null)
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
                new_export_data.Commodity_Code = deliverable.Entity.Entity.INTERNAL_NUM;
                new_export_data.Discipline_Name = deliverable.Discipline_Code;
                //new_export_data.Department_Name = deliverable.Department_Code;
                new_export_data.Subjob_Name = deliverable.Subjob_Name;
                new_export_data.User = deliverable.User_Name;
                new_export_data.Role = deliverable.User_Role;

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

        public static int getSubDivideMaxProgress(this IDashboard dashboard, IEnumerable<ExoDataPoint> burned_data_points, IEnumerable<ExoDataPoint> material_data_point, IEnumerable<ExoDataPoint> po_data_point, Func<IReportable, string> reportable_selector, Func<ExoDataPoint, string> exo_selector)
        {
            IEnumerable<string> reportable_codes = dashboard.Summary.Reportables.Select(reportable_selector);
            IEnumerable<string> exo_codes = burned_data_points.Select(exo_selector);
            IEnumerable<string> material_codes = material_data_point.Select(exo_selector);
            IEnumerable<string> po_codes = po_data_point.Select(exo_selector);
            List<string> loop_codes = new List<string>(reportable_codes);
            loop_codes.AddRange(exo_codes);
            loop_codes.AddRange(material_codes);
            loop_codes.AddRange(po_codes);
            HashSet<string> unique_codes = new HashSet<string>(loop_codes);

            return unique_codes.Count;
        }

        public static void SubDivideDashboardStats(this IDashboard dashboard, IEnumerable<ExoDataPoint> burned_data_points, IEnumerable<ExoDataPoint> material_data_point, IEnumerable<ExoDataPoint> po_data_point, Func<IReportable, string> reportable_selector, Func<ExoDataPoint, string> exo_selector)
        {
            IEnumerable<string> reportable_codes = dashboard.Summary.Reportables.Select(reportable_selector);
            IEnumerable<string> exo_codes = burned_data_points.Select(exo_selector);
            IEnumerable<string> material_codes = material_data_point.Select(exo_selector);
            IEnumerable<string> po_codes = po_data_point.Select(exo_selector);
            List<string> loop_codes = new List<string>(reportable_codes);
            loop_codes.AddRange(exo_codes);
            loop_codes.AddRange(material_codes);
            loop_codes.AddRange(po_codes);
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
            if (project_summary_stats == null)
                return new List<DashboardTreeStructure>();

            DashboardTreeStructure project_dashboard = new DashboardTreeStructure();
            project_dashboard.Summary = project_summary_stats;

            IEnumerable<ExoDataPoint> burned_data_points = project_summary_stats.GetBurnedDataPoints();
            IEnumerable<ExoDataPoint> material_data_points = project_summary_stats.GetMaterialDataPoints();
            IEnumerable<ExoDataPoint> po_data_points = project_summary_stats.GetPODataPoints();
            int maxProgress = project_dashboard.getSubDivideMaxProgress(burned_data_points, material_data_points, po_data_points, x => x.Subjob_Name, x => x.Subjob_Name);

            project_dashboard.SubDivideDashboardStats(burned_data_points, material_data_points, po_data_points, x => x.Subjob_Name, x => x.Subjob_Name);

            LoadingScreenManager.ShowLoadingScreen(maxProgress);
            //child dashboards are now subdivided into subjob dashboard
            foreach (DashboardTreeStructure subjob_dashboard in project_dashboard.Child_Dashboards)
            {
                //subjob_dashboard.SubDivideDashboardStats(burned_data_points, x => x.Department_Code, x => x.Department_Code);
                subjob_dashboard.SubDivideDashboardStats(burned_data_points, material_data_points, po_data_points, x => x.Discipline_Code, x => x.Discipline_Code);

                foreach (DashboardTreeStructure discipline_dashboard in subjob_dashboard.Child_Dashboards)
                {
                    //department_dashboard.SubDivideDashboardStats(burned_data_points, x => x.Discipline_Code, x => x.Discipline_Code);
                    discipline_dashboard.SubDivideDashboardStats(burned_data_points, material_data_points, po_data_points, x => x.Commodity_Code, x => x.Commodity_Code);
                    //child dashboards are now subdivided into variation dashboard
                    //foreach (DashboardTreeStructure variation_dashboard in discipline_dashboard.Child_Dashboards)
                    //{
                    //    variation_dashboard.SubDivideDashboardStats(burned_data_points, material_data_points, po_data_points, x => x.Variation_Code, x => x.Variation_Code);
                    //}
                }

                LoadingScreenManager.Progress();
            }

            LoadingScreenManager.CloseLoadingScreen();
            return project_dashboard.Child_Dashboards;
        }

        public static List<DashboardFlatStructure> ProjectDashboardSummaryBuilder(ProjectSummaryStats project_summary_stats, out List<DashboardTreeStructure> hierarchicalDashboards, IEnumerable<SUBJOB> SUBJOBCollection)
        {
            List<DashboardFlatStructure> flatDashboards = new List<DashboardFlatStructure>();
            hierarchicalDashboards = ProjectDashboardHierarchicalBuilder(project_summary_stats);

            IEnumerable<SUBJOB> design_subjobs = SUBJOBCollection == null ? new List<SUBJOB>() : SUBJOBCollection.Where(x => x.PHASE != null && x.PHASE.PHASE_TYPE == PhaseType.Design);
            IEnumerable<SUBJOB> construction_subjobs = SUBJOBCollection == null ? new List<SUBJOB>() : SUBJOBCollection.Where(x => x.PHASE != null && x.PHASE.PHASE_TYPE == PhaseType.Construct);
            IEnumerable<SUBJOB> all_subjobs = SUBJOBCollection == null ? new List<SUBJOB>() : SUBJOBCollection.ToList();

            foreach (DashboardTreeStructure subjob_dashboard in hierarchicalDashboards.OrderBy(x => x.Code))
            {
                if (subjob_dashboard.Child_Dashboards.Count == 0)
                {
                    DashboardFlatStructure subjobLevelDashboard = new DashboardFlatStructure();
                    subjobLevelDashboard.SubjobCode = subjob_dashboard.Code;
                    subjobLevelDashboard.Phase = design_subjobs.Any(x => x.PHASE.INTERNAL_NUM == subjob_dashboard.PhaseCode) ? PhaseType.Design : construction_subjobs.Any(x => x.PHASE.INTERNAL_NUM == subjob_dashboard.PhaseCode) ? PhaseType.Construct : (PhaseType?)null;
                    subjobLevelDashboard.AreaCode = string.Empty;
                    //subjobLevelDashboard.DepartmentCode = string.Empty;
                    subjobLevelDashboard.DisciplineCode = string.Empty;
                    subjobLevelDashboard.Stats = subjob_dashboard.Stats;
                    subjobLevelDashboard.ShouldHide = shouldHideSubjobDashboard(subjob_dashboard.Stats);
                    flatDashboards.Add(subjobLevelDashboard);
                }
                else
                {
                    foreach (DashboardTreeStructure discipline_dashboard in subjob_dashboard.Child_Dashboards.OrderBy(x => x.Code))
                    {
                        if(discipline_dashboard.Child_Dashboards.Count == 0)
                        {
                            SUBJOB subjob = all_subjobs.FirstOrDefault(x => x.INTERNAL_NAME1 == subjob_dashboard.Code);
                            string areaCode = string.Empty;

                            if (subjob != null)
                                areaCode = subjob.AREA == null ? string.Empty : subjob.AREA.INTERNAL_NUM;

                            DashboardFlatStructure disciplineLevelDashboard = new DashboardFlatStructure();
                            disciplineLevelDashboard.SubjobCode = subjob_dashboard.Code;
                            disciplineLevelDashboard.Phase = design_subjobs.Any(x => x.PHASE.INTERNAL_NUM == subjob_dashboard.PhaseCode) ? PhaseType.Design : construction_subjobs.Any(x => x.PHASE.INTERNAL_NUM == subjob_dashboard.PhaseCode) ? PhaseType.Construct : (PhaseType?)null;
                            disciplineLevelDashboard.AreaCode = areaCode;
                            //disciplineLevelDashboard.DepartmentCode = string.Empty;
                            disciplineLevelDashboard.DisciplineCode = discipline_dashboard.Code;
                            disciplineLevelDashboard.Stats = discipline_dashboard.Stats;
                            disciplineLevelDashboard.ShouldHide = shouldHideSubjobDashboard(discipline_dashboard.Stats);
                            flatDashboards.Add(disciplineLevelDashboard);
                        }
                        else
                        {
                            foreach(DashboardTreeStructure commodity_dashboard in discipline_dashboard.Child_Dashboards.OrderBy(x => x.Code))
                            {
                                //if (commodity_dashboard.Child_Dashboards.Count == 0)
                                //{
                                    SUBJOB subjob = all_subjobs.FirstOrDefault(x => x.INTERNAL_NAME1 == subjob_dashboard.Code);
                                    string areaCode = string.Empty;

                                    if (subjob != null)
                                        areaCode = subjob.AREA == null ? string.Empty : subjob.AREA.INTERNAL_NUM;

                                    DashboardFlatStructure commodityLevelDashboard = new DashboardFlatStructure();
                                    commodityLevelDashboard.SubjobCode = subjob_dashboard.Code;
                                    commodityLevelDashboard.Phase = design_subjobs.Any(x => x.PHASE.INTERNAL_NUM == subjob_dashboard.PhaseCode) ? PhaseType.Design : construction_subjobs.Any(x => x.PHASE.INTERNAL_NUM == subjob_dashboard.PhaseCode) ? PhaseType.Construct : (PhaseType?)null;
                                    commodityLevelDashboard.AreaCode = areaCode;
                                    //disciplineLevelDashboard.DepartmentCode = string.Empty;
                                    commodityLevelDashboard.DisciplineCode = discipline_dashboard.Code;
                                    commodityLevelDashboard.CommodityCode = commodity_dashboard.Code;
                                    commodityLevelDashboard.Stats = commodity_dashboard.Stats;
                                    commodityLevelDashboard.ShouldHide = shouldHideSubjobDashboard(commodity_dashboard.Stats);
                                    flatDashboards.Add(commodityLevelDashboard);
                                //}
                                //else
                                //{
                                //    foreach (DashboardTreeStructure variation_dashboard in commodity_dashboard.Child_Dashboards.OrderBy(x => x.Code))
                                //    {
                                //        SUBJOB subjob = all_subjobs.FirstOrDefault(x => x.INTERNAL_NAME1 == subjob_dashboard.Code);
                                //        string areaCode = string.Empty;

                                //        if (subjob != null)
                                //            areaCode = subjob.AREA == null ? string.Empty : subjob.AREA.INTERNAL_NUM;

                                //        DashboardFlatStructure variationLevelDashboard = new DashboardFlatStructure();
                                //        variationLevelDashboard.SubjobCode = subjob_dashboard.Code;
                                //        variationLevelDashboard.Phase = design_subjobs.Any(x => x.PHASE.INTERNAL_NUM == subjob_dashboard.PhaseCode) ? PhaseType.Design : construction_subjobs.Any(x => x.PHASE.INTERNAL_NUM == subjob_dashboard.PhaseCode) ? PhaseType.Construct : (PhaseType?)null;
                                //        variationLevelDashboard.AreaCode = areaCode;
                                //        //disciplineLevelDashboard.DepartmentCode = string.Empty;
                                //        variationLevelDashboard.DisciplineCode = discipline_dashboard.Code;
                                //        variationLevelDashboard.CommodityCode = commodity_dashboard.Code;
                                //        variationLevelDashboard.Variation_Code = variation_dashboard.Code;
                                //        variationLevelDashboard.Stats = variation_dashboard.Stats;
                                //        variationLevelDashboard.ShouldHide = shouldHideSubjobDashboard(variation_dashboard.Stats);
                                //        flatDashboards.Add(variationLevelDashboard);
                                //    }
                                //}

                            }
                        }
                    }
                }
            }

            return flatDashboards;
        }

        private static bool shouldHideSubjobDashboard(ProgressStats stats)
        {
            if (stats == null)
                return true;

            SummaryStats summaryStats = stats as SummaryStats;
            if (summaryStats == null)
                return true;

            if (summaryStats.Budgeted == null && summaryStats.Burned == null && summaryStats.Current == null)
                return true;

            if (summaryStats.Budgeted.DataPoints == null && summaryStats.Burned.DataPoints == null && summaryStats.Current.DataPoints == null)
                return true;

            if ((summaryStats.Budgeted.DataPoints != null && summaryStats.Budgeted.DataPoints.Count == 0) &&
                (summaryStats.Burned.DataPoints != null && summaryStats.Burned.DataPoints.Count == 0) && 
                (summaryStats.Current.DataPoints != null && summaryStats.Current.DataPoints.Count == 0))
                return true;

            return false;
        }
    }
}
