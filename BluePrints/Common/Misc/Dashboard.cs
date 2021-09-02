using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BluePrints.Data.BluePrintsEntities;

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

        public string PhaseCodeFromSubJobCode
        {
            get
            {
                return BluePrintsDataUtils.GetPhaseCodeFromSubJobCode(Code);
            }
        }

        public List<DashboardTreeStructure> Child_Dashboards { get; set; }

        public bool IHave_ChildDashboard => Child_Dashboards != null && Child_Dashboards.Count > 0;

        //deprecated
        public SummaryStats Summary { get; set; }

        public ProgressStats Stats { get; set; }
    }

    /// <summary>
    /// Dashboard subjob for splitting subjob into department, discipline and commodity specific for reporting
    /// </summary>
    public class DashboardFlatStructure : DashboardTreeStructure, IHaveStats
    {
        public PhaseType? Phase { get; set; }
        public string PhaseCode { get; set; }
        public string SubjobCode { get; set; }
        public string AreaCode { get; set; }
        public string SubAreaCode { get; set; }
        public string DepartmentCode { get; set; }
        public string DisciplineCode { get; set; }
        public string CommodityCode { get; set; }
        public string Variation_Code { get; set; }

        //This is for dashboard that only have material and po stats
        public bool ShouldHide { get; set; }
        public bool IsManaged { get; set; }
    }

    public class Dashboard_Export_Data_Point
    {
        public string User { get; set; }
        public string Role { get; set; }
        public string Subjob_Name { get; set; }
        public string Discipline_Name { get; set; }
        public string Commodity_Code { get; set; }
        public string Department_Code { get; set; }
        public string Internal_Name { get; set; }
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
        PlannedLate, 
        Earned,
        Burned,
        Actual,
        Current,
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

        public static List<Dashboard_Export_Data_Point> BuildExportDataByType(StatsType statsType, string projectNumber, PROJECT_Dashboard dashboard)
        {
            List<Dashboard_Export_Data_Point> exports = new List<Dashboard_Export_Data_Point>();
            IEnumerable<IReportable> reportables = ((SummaryStats)dashboard.Stats).Reportables;
            using (BluePrintsEntities bluePrintDataContext = new BluePrintsEntities())
            {
                if(statsType == StatsType.Remaining)
                {
                    List<Data.DataPoint> remainingDataPoints = bluePrintDataContext.QueryDeliverableRemainingDataPointsByProject(projectNumber);
                    foreach (Data.DataPoint dataPoint in remainingDataPoints)
                    {
                        exports.Add(createExportDataPoint(reportables, statsType, dataPoint.Original_Guid, dataPoint.UniversalPeriodEndDate, dataPoint.PeriodUnits, dataPoint.PeriodPrice));
                    }
                }
                else if(statsType == StatsType.Earned)
                {
                    foreach(IReportable reportable in reportables)
                    {
                        exports.Add(createExportDataPoint(reportables, statsType, reportable.OriginalEntityKey, dashboard.Stats.ReportingDataDate, reportable.Earned_Units_ToDate, reportable.Earned_Costs_ToDate));
                    }
                }
                else if(statsType == StatsType.Planned)
                {
                    List<Data.DataPoint> plannedDataPoints = bluePrintDataContext.QueryDeliverablePlannedDataPointsByProject(projectNumber);
                    foreach (Data.DataPoint dataPoint in plannedDataPoints)
                    {
                        exports.Add(createExportDataPoint(reportables, statsType, dataPoint.Original_Guid, dataPoint.UniversalPeriodEndDate, dataPoint.PeriodUnits, dataPoint.PeriodPrice));
                    }
                }
                else if (statsType == StatsType.Current)
                {
                    List<Data.DataPoint> currentDataPoints = bluePrintDataContext.QueryDeliverableCurrentDataPointsByProject(projectNumber);
                    foreach (Data.DataPoint dataPoint in currentDataPoints)
                    {
                        exports.Add(createExportDataPoint(reportables, statsType, dataPoint.Original_Guid, dataPoint.UniversalPeriodEndDate, dataPoint.PeriodUnits, dataPoint.PeriodPrice));
                    }
                }
            }

            return exports;
        }

        private static Dashboard_Export_Data_Point createExportDataPoint(IEnumerable<IReportable> reportables, StatsType statsType, Guid originalGuid, DateTime dataDate, object units, object price)
        {
            Dashboard_Export_Data_Point new_export = new Dashboard_Export_Data_Point();
            IReportable reportable = reportables.FirstOrDefault(x => x.OriginalEntityKey == originalGuid);
            if (reportable != null)
            {
                new_export.Commodity_Code = reportable.Commodity_Code;
                new_export.Discipline_Name = reportable.Discipline_Code;
                new_export.Subjob_Name = reportable.Subjob_Name;
                new_export.Internal_Name = reportable.Deliverable_Name;
            }
            else
            {
                string s = string.Empty;
            }

            new_export.Type = statsType;
            new_export.Data_Date = dataDate;

            if(units.GetType() == typeof(double))
            {
                new_export.Units = Convert.ToDecimal(units);
                new_export.Costs = Convert.ToDecimal(price);
            }
            else if(units.GetType() == typeof(decimal))
            {
                new_export.Units = (decimal)units;
                new_export.Costs = (decimal)price;
            }

            return new_export;
        }

        public static List<Dashboard_Export_Data_Point> BuildExportData(IEnumerable<WBSReportable> WBSReportables, IEnumerable<DOCTYPE> DOCTYPECollection = null)
        {
            List<Dashboard_Export_Data_Point> export_data = new List<Dashboard_Export_Data_Point>();

            foreach (WBSReportable WBSReportable in WBSReportables)
            {
                if (WBSReportable != null)
                {
                    if (WBSReportable.Budgeted != null)
                        export_data.AddRange(buildExportDataByType(WBSReportable, StatsType.Planned, WBSReportable.Budgeted.DataPoints, null, DOCTYPECollection));

                    if (WBSReportable.BudgetedLate != null)
                        export_data.AddRange(buildExportDataByType(WBSReportable, StatsType.PlannedLate, WBSReportable.BudgetedLate.DataPoints, null, DOCTYPECollection));

                    if (WBSReportable.Earned != null)
                        export_data.AddRange(buildExportDataByType(WBSReportable, StatsType.Earned, WBSReportable.Earned.DataPoints, null, DOCTYPECollection));

                    if (WBSReportable.Current != null)
                        export_data.AddRange(buildExportDataByType(WBSReportable, StatsType.Current, WBSReportable.Current.DataPoints, null, DOCTYPECollection));

                    if (WBSReportable.Remaining != null)
                        export_data.AddRange(buildExportDataByType(WBSReportable, StatsType.Remaining, WBSReportable.Remaining.DataPoints, null, DOCTYPECollection));

                    if (WBSReportable.RemainingActual != null)
                        export_data.AddRange(buildExportDataByType(WBSReportable, StatsType.RemainingActual, WBSReportable.RemainingActual.DataPoints, null, DOCTYPECollection));

                    if (WBSReportable.Burned != null)
                        export_data.AddRange(buildExportDataByType(WBSReportable, StatsType.Burned, WBSReportable.Burned.DataPoints, null, DOCTYPECollection));
                }
            }

            return export_data;
        }

        private static List<Dashboard_Export_Data_Point> buildExportDataByType(WBSReportable WBSReportable, StatsType stats_type, IEnumerable<ViewModel.Reporting.DataPoint> data_points, IEnumerable<ViewModel.Reporting.DataPoint> dataPoints = null, IEnumerable<DOCTYPE> DOCTYPECollection = null)
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
                new_export_data.Commodity_Code = WBSReportable.COMMODITY_CODE;
                new_export_data.Discipline_Name = WBSReportable.DISCIPLINE_CODE;

                if (DOCTYPECollection != null)
                {
                    DOCTYPE findDOCTYPE = DOCTYPECollection.FirstOrDefault(x => x.CODE == WBSReportable.COMMODITY_CODE);
                    if (findDOCTYPE != null)
                        new_export_data.Department_Code = findDOCTYPE.DEPARTMENT.CODE;
                }

                new_export_data.Subjob_Name = WBSReportable.SUBJOB_CODE;

                if (dataPoints != null)
                {
                    ViewModel.Reporting.DataPoint current_period_actual = dataPoints.FirstOrDefault(x => x.ProgressDate == data_point.ProgressDate);
                    if (current_period_actual != null)
                        new_export_data.Actual_Costs = current_period_actual.Costs;
                }

                export_data_by_type.Add(new_export_data);
            }

            return export_data_by_type;
        }

        private static List<Dashboard_Export_Data_Point> buildExportDataByType(BASELINE_ITEMProgress deliverable, StatsType stats_type, IEnumerable<ViewModel.Reporting.DataPoint> data_points, IEnumerable<ViewModel.Reporting.DataPoint> actual_data_points = null, IEnumerable<DOCTYPE> DOCTYPECollection = null)
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

        private static DashboardTreeStructure create_dashboard(IDashboard parent_dashboard, string code, SummaryStats summary_stats, Func<IReportable, bool> reportable_predicate, Func<ExoDataPoint, bool> exo_predicate, bool forceRetrieveRemainingDataPoints = false)
        {
            DashboardTreeStructure dashboard = new DashboardTreeStructure() { Code = code, Parent_Dashboard = parent_dashboard };
            SummaryStats stats;
            if (summary_stats != null)
                stats = SummaryStatsHelpers.Group_Summary_Stats(summary_stats, reportable_predicate, exo_predicate, forceRetrieveRemainingDataPoints);
            else
                stats = null;

            if (stats != null)
                dashboard.Stats = stats;
            else
                return null;
            
            return dashboard;
        }

        public static List<DashboardFlatStructure> ProjectDashboardSummaryBuilder(ProjectSummaryStats projectSummaryStats, IEnumerable<SUBJOB> SUBJOBCollection, bool showLoadingScreen, bool isVariationSeparated = false, bool forceRetrieveRemainingDataPoints = false, IEnumerable<DOCTYPE> DOCTYPECollection = null)
        {
            List<DashboardFlatStructure> flatDashboards = new List<DashboardFlatStructure>();

            IEnumerable<SUBJOB> designSubjobs = SUBJOBCollection == null ? new List<SUBJOB>() : SUBJOBCollection.Where(x => x.PHASE != null && x.PHASE.PHASE_TYPE == PhaseType.Design);
            IEnumerable<SUBJOB> constructionSubjobs = SUBJOBCollection == null ? new List<SUBJOB>() : SUBJOBCollection.Where(x => x.PHASE != null && x.PHASE.PHASE_TYPE == PhaseType.Construct);
            IEnumerable<SUBJOB> all_subjobs = SUBJOBCollection == null ? new List<SUBJOB>() : SUBJOBCollection.ToList();

            IEnumerable<ExoDataPoint> burnedDataPoints = projectSummaryStats.Burned.GetData().Select(x => (ExoDataPoint)x);
            IEnumerable<ExoDataPoint> actualDataPoints = projectSummaryStats.Actual.GetData().Select(x => (ExoDataPoint)x);
            IEnumerable<ExoDataPoint> materialDataPoints = projectSummaryStats.Material.GetData().Select(x => (ExoDataPoint)x);
            IEnumerable<ExoDataPoint> poDataPoints = projectSummaryStats.PO.GetData().Select(x => (ExoDataPoint)x);
            IEnumerable<ExoDataPoint> previousPODataPoints = projectSummaryStats.PreviousPO.GetData().Select(x => (ExoDataPoint)x);
            List<ExoDataPoint> allDataPoints = new List<ExoDataPoint>();
            allDataPoints.AddRange(burnedDataPoints);

            //actual is a duplicate of burned
            //allDataPoints.AddRange(actualDataPoints);
            allDataPoints.AddRange(materialDataPoints);
            allDataPoints.AddRange(poDataPoints);
            allDataPoints.AddRange(previousPODataPoints);

            List<ExoDataPointsGroup> allActualsPointsGroups;
            List<ExoDataPointsGroup> groupedBurnedDataPoints;
            List<ExoDataPointsGroup> groupedActualDataPoints;
            List<ExoDataPointsGroup> groupedMaterialDataPoints;
            List<ExoDataPointsGroup> groupedPODataPoints;
            List<ExoDataPointsGroup> groupedPreviousPODataPoints;

            Func<IEnumerable<ExoDataPoint>, List<ExoDataPointsGroup>> groupFunc;
            if(isVariationSeparated)
                groupFunc = new Func<IEnumerable<ExoDataPoint>, List<ExoDataPointsGroup>>(y => y.GroupBy(x => new { x.Subjob_Name, x.DisciplineCode, x.Commodity_Code, x.Variation_Code }).Select(g => new ExoDataPointsGroup(g.Key.Subjob_Name, g.Key.DisciplineCode, g.Key.Commodity_Code, g.Key.Variation_Code, g)).ToList());
            else
                groupFunc = new Func<IEnumerable<ExoDataPoint>, List<ExoDataPointsGroup>>(y => y.GroupBy(x => new { x.Subjob_Name, x.DisciplineCode, x.Commodity_Code }).Select(g => new ExoDataPointsGroup(g.Key.Subjob_Name, g.Key.DisciplineCode, g.Key.Commodity_Code, "", g)).ToList());

            //group different actuals data points
            allActualsPointsGroups = groupFunc(allDataPoints);

            foreach (ExoDataPointsGroup exoDataPointsGroup in allActualsPointsGroups)
            {
                WBSReportable findWBSReportable;
                if (isVariationSeparated)
                    findWBSReportable = projectSummaryStats.WBSReportables.FirstOrDefault(x => x.SUBJOB_CODE == exoDataPointsGroup.SubJobCode && x.DISCIPLINE_CODE == exoDataPointsGroup.DisciplineCode && x.COMMODITY_CODE == exoDataPointsGroup.CommodityCode && x.VARIATION_CODE == exoDataPointsGroup.VariationCode);
                else
                    findWBSReportable = projectSummaryStats.WBSReportables.FirstOrDefault(x => x.SUBJOB_CODE == exoDataPointsGroup.SubJobCode && x.DISCIPLINE_CODE == exoDataPointsGroup.DisciplineCode && x.COMMODITY_CODE == exoDataPointsGroup.CommodityCode);

                //when full WBS code breakdown only exists in EXO
                if (findWBSReportable == null)
                {
                    if (isVariationSeparated)
                        projectSummaryStats.AddMissingActualsWBSReportables(exoDataPointsGroup.SubJobCode, exoDataPointsGroup.DisciplineCode, exoDataPointsGroup.CommodityCode, exoDataPointsGroup.VariationCode);
                    else
                        projectSummaryStats.AddMissingActualsWBSReportables(exoDataPointsGroup.SubJobCode, exoDataPointsGroup.DisciplineCode, exoDataPointsGroup.CommodityCode, "");
                }
            }

            groupedBurnedDataPoints = groupFunc(burnedDataPoints);
            groupedActualDataPoints = groupFunc(actualDataPoints);
            groupedMaterialDataPoints = groupFunc(materialDataPoints);
            groupedPODataPoints = groupFunc(poDataPoints);
            groupedPreviousPODataPoints = groupFunc(previousPODataPoints);

            foreach (WBSReportable wbsReportable in projectSummaryStats.WBSReportables)
            {
                //assign actuals to reportables
                wbsReportable.AssignWBSReportableData(x => x.Burned.SetData, groupedBurnedDataPoints, isVariationSeparated);
                wbsReportable.AssignWBSReportableData(x => x.Actual.SetData, groupedActualDataPoints, isVariationSeparated);
                wbsReportable.AssignWBSReportableData(x => x.Material.SetData, groupedMaterialDataPoints, isVariationSeparated);
                wbsReportable.AssignWBSReportableData(x => x.PO.SetData, groupedPODataPoints, isVariationSeparated);
                wbsReportable.AssignWBSReportableData(x => x.PreviousPO.SetData, groupedPreviousPODataPoints, isVariationSeparated);
                wbsReportable.SummariseRemainingActualData();
                populateFlatDashboards(flatDashboards, wbsReportable.SUBJOB_CODE, wbsReportable.DISCIPLINE_CODE, wbsReportable.COMMODITY_CODE, wbsReportable.VARIATION_CODE, wbsReportable, designSubjobs, constructionSubjobs, DOCTYPECollection);
            }
            
            return flatDashboards;
        }

        private static void populateFlatDashboards(List<DashboardFlatStructure> masterDashboardFlat, string subJobCode, string disciplineCode, string commodityCode, string variationCode, WBSReportable stats, IEnumerable<SUBJOB> designSubJobs, IEnumerable<SUBJOB> constructSubJobs, IEnumerable<DOCTYPE> DOCTYPECollection = null)
        {
            string s;
            if (subJobCode == "00151-009-00-D1" && disciplineCode == "ST01")
                s = string.Empty;

            DashboardFlatStructure newDashboard = new DashboardFlatStructure();
            newDashboard.SubjobCode = subJobCode;
            newDashboard.PhaseCode = newDashboard.SubjobCode.Length > 14 ? newDashboard.SubjobCode.Substring(13, 2) : string.Empty;
            newDashboard.AreaCode = newDashboard.SubjobCode.Length > 9 ? newDashboard.SubjobCode.Substring(6, 3) : string.Empty;
            newDashboard.SubAreaCode = newDashboard.SubjobCode.Length > 12 ? newDashboard.SubjobCode.Substring(10, 2) : string.Empty;

            string extractSubJobCode = BluePrintsDataUtils.GetPhaseCodeFromSubJobCode(subJobCode);
            newDashboard.Phase = designSubJobs.Any(x => x.PHASE.INTERNAL_NUM == extractSubJobCode) ? PhaseType.Design : constructSubJobs.Any(x => x.PHASE.INTERNAL_NUM == extractSubJobCode) ? PhaseType.Construct : (PhaseType?)null;
            newDashboard.DisciplineCode = disciplineCode;
            newDashboard.CommodityCode = commodityCode;

            DOCTYPE findDOCTYPE = DOCTYPECollection.FirstOrDefault(x => x.CODE == commodityCode);
            if (findDOCTYPE != null)
                newDashboard.DepartmentCode = findDOCTYPE.DEPARTMENT.CODE;

            newDashboard.Variation_Code = variationCode;
            newDashboard.Stats = stats;
            newDashboard.ShouldHide = shouldHideSubjobDashboard(stats);
            masterDashboardFlat.Add(newDashboard);
        }

        private static bool shouldHideSubjobDashboard(ProgressStats stats)
        {
            if (stats == null)
                return true;

            SummaryStats summaryStats = stats as SummaryStats;
            if (summaryStats == null)
                return true;

            if (summaryStats.Budgeted == null && summaryStats.Burned == null && summaryStats.Current == null && summaryStats.Earned == null)
                return true;

            if (summaryStats.Budgeted.DataPoints == null && summaryStats.Burned.DataPoints == null && summaryStats.Current.DataPoints == null && summaryStats.Earned.DataPoints == null)
                return true;

            if ((summaryStats.Budgeted.DataPoints != null && summaryStats.Budgeted.DataPoints.Count == 0) &&
                (summaryStats.Burned.DataPoints != null && summaryStats.Burned.DataPoints.Count == 0) && 
                (summaryStats.Current.DataPoints != null && summaryStats.Current.DataPoints.Count == 0) &&
                (summaryStats.Earned.DataPoints != null && summaryStats.Earned.DataPoints.Count == 0))
                return true;

            return false;
        }
    }
}
