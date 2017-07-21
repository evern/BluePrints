using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Base;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.P6EntitiesDataModel;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace BluePrints.Common.Projections
{
    public class PROJECT_Dashboard : BluePrintsProjectionBase<PROJECT>, IHaveSummary
    {
        public int WBSLevel => 0;
        public PROJECT_Dashboard()
        {
        }

        public PROJECT_Dashboard(IEnumerable<IReportable> reportableItems, IEnumerable<PROGRESS> PROGRESSES, IEnumerable<WORKPACK> WORKPACKS, IEnumerable<VARIATION> VARIATIONS, string project_number, decimal currency_conversion, IPrimeroEntitiesUnitOfWork PrimeroUOW = null)
        {
            TimeSpan reporting_interval = ChronologicalHelpers.GetDefaultIntervalTimeSpan();
            DateTime? earliest_first_aligned_data_date = ChronologicalHelpers.GetEarliestFirstAlignedDataDate(PROGRESSES);
            DateTime? latest_data_date = ChronologicalHelpers.GetLastDataDate(PROGRESSES);
            
            List<VariationAdjustment> projectVariationAdjustments = ProjectionHelpers.BuildProjectVariationAdjustments(VARIATIONS.AsQueryable(), reportableItems);
            FullStatsBuilder fullStatsBuilder = null;
            if (earliest_first_aligned_data_date != null)
                fullStatsBuilder = new FullStatsBuilder(project_number, currency_conversion, reporting_interval, (DateTime)earliest_first_aligned_data_date, WORKPACKS, PrimeroUOW);

            if(latest_data_date != null && earliest_first_aligned_data_date != null)
            {
                Stats = new ProjectSummaryStats(reportableItems, (DateTime)latest_data_date, reporting_interval, (DateTime)earliest_first_aligned_data_date, projectVariationAdjustments);
                projectSummarizer = new FullSummarizer((ProjectSummaryStats)Stats, fullStatsBuilder, project_number);
            }
        }

        FullSummarizer projectSummarizer { get; set; }
        public ProgressStats Stats
        {
            get { return GetProperty(() => Stats); }
            set { SetProperty(() => Stats, value); }
        }

        public void BuildStats(bool showLoadingScreen = true, bool isCosts = false)
        {
            if (projectSummarizer == null)
                return;

            projectSummarizer.BuildBurnedDataPoints();
            projectSummarizer.Build(showLoadingScreen, isCosts);
            this.RaisePropertiesChanged();
        }

        public void RecalculateStats(bool isCosts)
        {
            if (projectSummarizer == null)
                return;

            projectSummarizer.RecalculateStats(isCosts);
        }

        public List<Phase_Dashboard> Phase_Dashboards { get; set; }
        public bool IHavePhase_Dashboards { get { return Phase_Dashboards != null && Phase_Dashboards.Count() > 0; } }
    }

    public class Phase_Dashboard : IHaveStats
    {
        public int WBSLevel => 1;
        public string Code { get; set; }
        public List<Discipline_Dashboard> Discipline_Dashboards { get; set; }
        public bool IHaveDiscipline_Dashboards { get { return Discipline_Dashboards != null && Discipline_Dashboards.Count() > 0; } }
        public ProgressStats Stats { get; set; }
    }

    public class Discipline_Dashboard : IHaveStats
    {
        public int WBSLevel => 2;
        public string Code { get; set; }
        public List<Commodity_Dashboard> Commodity_Dashboards { get; set; }
        public bool IHaveCommodity_Dashboards { get { return Commodity_Dashboards != null && Commodity_Dashboards.Count() > 0; } }
        public ProgressStats Stats { get; set; }
    }

    public class Commodity_Dashboard : IHaveStats
    {
        public int WBSLevel => 3;
        public string Display_Code { get; set; }
        public string Code { get; set; }
        public ProgressStats Stats { get; set; }
    }

    public static class DashboardQueries
    {
        public static PROJECT_Dashboard Single_Project_DashboardTransformation(PROJECT PROJECT, BASELINE BASELINE, ESTIMATION_DIRECT ESTIMATION_DIRECT, IEnumerable<PROGRESS> PROGRESS, IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS, IEnumerable<RATE> RATES,
            IEnumerable<VARIATION> VARIATIONS = null, bool buildStats = false)
        {
            List<PROJECT> PROJECTS = new List<PROJECT>();
            List<BASELINE> BASELINES = new List<BASELINE>();
            List<ESTIMATION_DIRECT> ESTIMATION_DIRECTS = new List<ESTIMATION_DIRECT>();

            PROJECTS.Add(PROJECT);
            if (BASELINE != null)
                BASELINES.Add(BASELINE);

            if (ESTIMATION_DIRECT != null)
                ESTIMATION_DIRECTS.Add(ESTIMATION_DIRECT);

            var project_dashboard = DashboardQueries.Multiple_Project_DashboardTransformation(PROJECTS.AsQueryable(), BASELINES, ESTIMATION_DIRECTS, PROGRESS, PROGRESS_ITEMS, RATES, VARIATIONS, PROJECT.GUID, buildStats);

            if (project_dashboard.Count() == 0)
                return null;

            return project_dashboard.First();
        }

        public static IQueryable<PROJECT_Dashboard> Multiple_Project_DashboardTransformation(IQueryable<PROJECT> PROJECTS,
            IEnumerable<BASELINE> BASELINES, IEnumerable<ESTIMATION_DIRECT> ESTIMATION_DIRECTS, IEnumerable<PROGRESS> PROGRESSES,
            IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS, IEnumerable<RATE> RATES, 
            IEnumerable<VARIATION> VARIATIONS = null,
            Guid? project_guid = null, bool buildStats = false)
        {
            IQueryable<PROJECT> project_single_or_active_selection;
            if (project_guid != null)
                project_single_or_active_selection = PROJECTS.Where(x => x.GUID == project_guid);
            else
                project_single_or_active_selection = PROJECTS.Where(x => x.STATUS == ProjectStatus.Active);

            List<PROJECT_Dashboard> project_dashboard = new List<PROJECT_Dashboard>();
            var primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();

            foreach (var current_project in project_single_or_active_selection)
            {
                BASELINE live_baseline = BASELINES.FirstOrDefault(x => x.GUID_PROJECT == current_project.GUID);
                ESTIMATION_DIRECT live_estimation_direct = ESTIMATION_DIRECTS.FirstOrDefault(x => x.GUID_PROJECT == current_project.GUID);
                PROGRESS live_baseline_progress = PROGRESSES.FirstOrDefault(x => x.GUID_PROJECT == current_project.GUID && x.STATUS == ProgressStatus.Live && x.TYPE == ProgressType.Design);
                PROGRESS live_estimation_direct_progress = PROGRESSES.FirstOrDefault(x => x.GUID_PROJECT == current_project.GUID && x.STATUS == ProgressStatus.Live && x.TYPE == ProgressType.Construct);

                IEnumerable<PROGRESS_ITEM> live_baseline_progresses;
                if (live_baseline_progress != null)
                    live_baseline_progresses = PROGRESS_ITEMS.Where(x => x.GUID_PROGRESS == live_baseline_progress.GUID);
                else
                    live_baseline_progresses = null;

                IEnumerable<PROGRESS_ITEM> live_estimation_direct_progresses;
                if (live_estimation_direct_progress != null)
                    live_estimation_direct_progresses = PROGRESS_ITEMS.Where(x => x.GUID_PROGRESS == live_estimation_direct_progress.GUID);
                else
                    live_estimation_direct_progresses = null;

                IEnumerable<RATE> project_rates = RATES.Where(x => x.GUID_PROJECT == current_project.GUID);
                IEnumerable<VARIATION> approved_project_variations = VARIATIONS.Where(x => x.GUID_PROJECT == current_project.GUID);
                List<IReportable> reportables = new List<IReportable>();

                List<PROGRESS> current_project_progresses = new List<PROGRESS>();
                if (live_baseline != null && live_baseline_progresses != null)
                {
                    IEnumerable<BASELINE_ITEM> live_baseline_items = live_baseline.BASELINE_ITEM.Where(x => !x.BY_DURATION);
                    IEnumerable<BASELINE_ITEMProgress> project_baseline_item_progresses = ProgressQueries.OffsiteDirectProgressItemTransformation(
                    live_baseline_items.AsQueryable(), current_project, live_baseline_progress, project_rates, live_baseline_progresses, approved_project_variations).ToArray().AsEnumerable();
                    reportables.AddRange(project_baseline_item_progresses);
                    current_project_progresses.Add(live_baseline_progress);
                }

                if (live_estimation_direct != null && live_estimation_direct_progress != null)
                {
                    IEnumerable<ESTIMATION_DIRECT_ITEM> live_estimation_direct_items = live_estimation_direct.ESTIMATION_DIRECT_ITEM;
                    IEnumerable<ESTIMATION_DIRECT_ITEMProgress> project_estimation_direct_item_progresses =
                    ESTIMATION_DIRECT_ITEMProjectionQueries.IDeliverable_Progress_Transformation(live_estimation_direct_items.AsQueryable(), project_rates, live_estimation_direct_progress, live_estimation_direct_progresses);
                    reportables.AddRange(project_estimation_direct_item_progresses);
                    current_project_progresses.Add(live_estimation_direct_progress);
                }

                var current_project_dashboard = new PROJECT_Dashboard(reportables, current_project_progresses, current_project.WORKPACK, approved_project_variations, current_project.NUMBER, current_project.CURRENCYCONVERSION)
                {
                    EntityKey = current_project.GUID,
                    Entity = current_project
                };

                if (buildStats)
                    current_project_dashboard.BuildStats();

                project_dashboard.Add(current_project_dashboard);
            }

            return project_dashboard.AsQueryable();
        }

        public static List<Phase_Dashboard> Construct_Phase_Dashboards(ProjectSummaryStats project_summary_stats)
        {
            List<string> phases = new List<string>();
            phases.Add(BluePrintsResources.Default_Design_Phase);
            phases.Add(BluePrintsResources.Alternate_Design_Phase);
            phases.Add(BluePrintsResources.Default_Construction_Phase);

            List<Phase_Dashboard> phase_dashboards = new List<Phase_Dashboard>();
            foreach (string phase in phases)
            {
                Phase_Dashboard new_phase_dashboard = new Phase_Dashboard() { Code = phase };
                if(project_summary_stats != null)
                    new_phase_dashboard.Stats = SummaryStatsHelpers.Group_Summary_Stats(project_summary_stats, x => x.Phase_Code == phase, x => x.PhaseCode == phase);

                if (new_phase_dashboard.Stats != null)
                {
                    new_phase_dashboard.Discipline_Dashboards = construct_discipline_dashboards(new_phase_dashboard, project_summary_stats.GetBurnedDataPoints());
                    phase_dashboards.Add(new_phase_dashboard);
                }
            }

            return phase_dashboards;
        }

        private static List<Discipline_Dashboard> construct_discipline_dashboards(Phase_Dashboard phase_dashboard, IEnumerable<ExoDataPoint> burned_data_points)
        {
            List<Discipline_Dashboard> preliminary_discipline_dashboards = new List<Discipline_Dashboard>();
            SummaryStats phase_summary_stats = (SummaryStats)phase_dashboard.Stats;

            if(phase_summary_stats != null && phase_summary_stats.Reportables != null)
                foreach (IReportable reportable in phase_summary_stats.Reportables)
                {
                    string discipline_code = reportable.Discipline_Code;
                    if (!preliminary_discipline_dashboards.Any(x => x.Code == discipline_code))
                        preliminary_discipline_dashboards.Add(create_discipline_dashboard(discipline_code, (SummaryStats)phase_dashboard.Stats, burned_data_points));
                }

            foreach (ExoDataPoint burnedDataPoint in burned_data_points)
            {
                string discipline_code = burnedDataPoint.DisciplineCode;
                if (!preliminary_discipline_dashboards.Any(x => x.Code == discipline_code))
                    preliminary_discipline_dashboards.Add(create_discipline_dashboard(discipline_code, (SummaryStats)phase_dashboard.Stats, burned_data_points));
            }

            return preliminary_discipline_dashboards.Where(x => x.Stats != null).ToList();
        }

        private static Discipline_Dashboard create_discipline_dashboard(string discipline_code, SummaryStats summary_stats, IEnumerable<ExoDataPoint> burned_data_points)
        {
            Discipline_Dashboard discipline_dashboard = new Discipline_Dashboard() { Code = discipline_code };
            if(summary_stats != null)
                discipline_dashboard.Stats = SummaryStatsHelpers.Group_Summary_Stats(summary_stats, x => x.Discipline_Code == discipline_code, x => x.DisciplineCode == discipline_code);
            discipline_dashboard.Commodity_Dashboards = construct_commodity_dashboards(discipline_dashboard, burned_data_points);

            return discipline_dashboard;
        }

        private static List<Commodity_Dashboard> construct_commodity_dashboards(Discipline_Dashboard discipline_dashboard, IEnumerable<ExoDataPoint> burned_data_points)
        {
            List<Commodity_Dashboard> preliminary_commodity_codes = new List<Commodity_Dashboard>();
            SummaryStats phase_summary_stats = (SummaryStats)discipline_dashboard.Stats;

            if(phase_summary_stats != null && phase_summary_stats.Reportables != null)
                foreach (IReportable reportable in phase_summary_stats.Reportables)
                {
                    string commodityCode = reportable.Commodity_Code;
                    if (!preliminary_commodity_codes.Any(x => x.Code == commodityCode))
                        preliminary_commodity_codes.Add(create_commodity_dashboard(commodityCode, commodityCode, (SummaryStats)discipline_dashboard.Stats, burned_data_points));
                }

            foreach (ExoDataPoint burnedDataPoint in burned_data_points)
            {
                string commodityCode = burnedDataPoint.CommodityCode;
                if (!preliminary_commodity_codes.Any(x => x.Code == commodityCode))
                    preliminary_commodity_codes.Add(create_commodity_dashboard(commodityCode, commodityCode, (SummaryStats)discipline_dashboard.Stats, burned_data_points));
            }

            return preliminary_commodity_codes.Where(x => x.Stats != null).ToList();
        }

        private static Commodity_Dashboard create_commodity_dashboard(string commodity_code, string commodity_display_code, SummaryStats summary_stats, IEnumerable<ExoDataPoint> burned_data_points)
        {
            Commodity_Dashboard Commodity_dashboard = new Commodity_Dashboard() { Code = commodity_code, Display_Code = commodity_display_code };
            if(summary_stats != null)
                Commodity_dashboard.Stats = SummaryStatsHelpers.Group_Summary_Stats(summary_stats, x => x.Commodity_Code == commodity_code, x => x.CommodityCode == commodity_code);
            return Commodity_dashboard;
        }
    }
}