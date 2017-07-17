using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Base;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.P6EntitiesDataModel;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.Common.Projections
{
    public class PROJECT_Dashboard : BluePrintsProjectionBase<PROJECT>, IHaveSummary
    {
        public PROJECT_Dashboard()
        {
        }

        public PROJECT_Dashboard(IEnumerable<IReportable> reportableItems, IEnumerable<PROGRESS> PROGRESSES, IEnumerable<WORKPACK> WORKPACKS, IEnumerable<VARIATION> VARIATIONS, string project_number, IPrimeroEntitiesUnitOfWork PrimeroUOW = null)
        {
            TimeSpan reporting_interval = ChronologicalHelpers.GetDefaultIntervalTimeSpan();
            DateTime? earliest_first_aligned_data_date = ChronologicalHelpers.GetEarliestFirstAlignedDataDate(PROGRESSES);
            DateTime? latest_data_date = ChronologicalHelpers.GetLastDataDate(PROGRESSES);
            
            List<VariationAdjustment> projectVariationAdjustments = ProjectionHelpers.BuildProjectVariationAdjustments(VARIATIONS.AsQueryable(), reportableItems);
            FullStatsBuilder fullStatsBuilder = null;
            if (earliest_first_aligned_data_date != null)
                fullStatsBuilder = new FullStatsBuilder(Entity, reporting_interval, (DateTime)earliest_first_aligned_data_date, WORKPACKS, PrimeroUOW);

            if(latest_data_date != null && earliest_first_aligned_data_date != null)
                Stats = new ProjectSummaryStats(reportableItems, (DateTime)latest_data_date, reporting_interval, (DateTime)earliest_first_aligned_data_date, projectVariationAdjustments);

            projectSummarizer = new FullSummarizer((ProjectSummaryStats)Stats, fullStatsBuilder, project_number);
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
    }

    public static class DashboardQueries
    {
        public static PROJECT_Dashboard Single_Project_DashboardTransformation(PROJECT PROJECT, BASELINE BASELINE, ESTIMATION_DIRECT ESTIMATION_DIRECT, IEnumerable<PROGRESS> PROGRESS, IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS, IEnumerable<RATE> RATES,
            IEnumerable<VARIATION> VARIATIONS = null, bool buildStats = false)
        {
            List<PROJECT> PROJECTS = new List<PROJECT>();
            List<BASELINE> BASELINES = new List<BASELINE>();
            List<ESTIMATION_DIRECT> ESTIMATION_DIRECTS = new List<ESTIMATION_DIRECT>();

            if (BASELINE != null)
            {
                BASELINES.Add(BASELINE);
                PROJECTS.Add(BASELINE.PROJECT);
            }

            if(ESTIMATION_DIRECT != null)
                ESTIMATION_DIRECTS.Add(ESTIMATION_DIRECT);

            var project_dashboard = DashboardQueries.Multiple_Project_DashboardTransformation(PROJECTS.AsQueryable(), BASELINES, ESTIMATION_DIRECTS, PROGRESS, PROGRESS_ITEMS, RATES, VARIATIONS, BASELINE.GUID_PROJECT, buildStats);

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

                IEnumerable<PROGRESS_ITEM> live_baseline_progresses = PROGRESS_ITEMS.Where(x => x.GUID_PROGRESS == live_baseline_progress.GUID);
                IEnumerable<PROGRESS_ITEM> live_estimation_direct_progresses = PROGRESS_ITEMS.Where(x => x.GUID_PROGRESS == live_estimation_direct_progress.GUID);

                IEnumerable<RATE> project_rates = RATES.Where(x => x.GUID_PROJECT == current_project.GUID);
                IEnumerable<VARIATION> approved_project_variations = VARIATIONS.Where(x => x.GUID_PROJECT == current_project.GUID);
                List<IReportable> reportables = new List<IReportable>();

                if (live_baseline != null && live_baseline_progresses != null)
                {
                    IEnumerable<BASELINE_ITEM> live_baseline_items = live_baseline.BASELINE_ITEM.Where(x => !x.BY_DURATION);
                    IEnumerable<BASELINE_ITEMProgress> project_baseline_item_progresses = ProgressQueries.OffsiteDirectProgressItemTransformation(
                    live_baseline_items.AsQueryable(), current_project, live_baseline_progress, project_rates, live_baseline_progresses, approved_project_variations).ToArray().AsEnumerable();
                    reportables.AddRange(project_baseline_item_progresses);
                }

                if (live_estimation_direct != null && live_estimation_direct_progress != null)
                {
                    IEnumerable<ESTIMATION_DIRECT_ITEM> live_estimation_direct_items = live_estimation_direct.ESTIMATION_DIRECT_ITEM;
                    IEnumerable<ESTIMATION_DIRECT_ITEMProgress> project_estimation_direct_item_progresses =
                    ESTIMATION_DIRECT_ITEMProjectionQueries.IDeliverable_Progress_Transformation(live_estimation_direct_items.AsQueryable(), project_rates, live_baseline_progress, live_estimation_direct_progresses);
                    reportables.AddRange(project_estimation_direct_item_progresses);
                }

                var current_project_dashboard = new PROJECT_Dashboard(reportables, PROGRESSES, current_project.WORKPACK, approved_project_variations, current_project.NUMBER)
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
    }
}