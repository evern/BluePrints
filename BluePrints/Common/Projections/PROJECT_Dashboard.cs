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

        public PROJECT_Dashboard(IEnumerable<IReportable> reportableItems, PROGRESS LivePROGRESS, IEnumerable<WORKPACK> WORKPACKS, IEnumerable<VARIATION> VARIATIONS, string project_number, IPrimeroEntitiesUnitOfWork PrimeroUOW = null)
        {
            TimeSpan reportInterval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(LivePROGRESS);
            DateTime firstAlignedDataDate = ChronologicalHelpers.GenerateFirstAlignedDataDate(LivePROGRESS);
            List<VariationAdjustment> projectVariationAdjustments = ProjectionHelpers.BuildProjectVariationAdjustments(VARIATIONS.AsQueryable(), reportableItems);

            FullStatsBuilder fullStatsBuilder = new FullStatsBuilder(Entity, LivePROGRESS, WORKPACKS, PrimeroUOW);

            Stats = new ProjectSummaryStats(reportableItems, LivePROGRESS, projectVariationAdjustments);
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
        public static PROJECT_Dashboard Single_Project_DashboardTransformation(PROJECT PROJECT, BASELINE BASELINE, PROGRESS PROGRESS, IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS, IEnumerable<RATE> RATES,
            IEnumerable<VARIATION> VARIATIONS = null, bool buildStats = false)
        {
            List<PROJECT> PROJECTS = new List<PROJECT>();
            List<BASELINE> BASELINES = new List<BASELINE>();
            List<PROGRESS> PROGRESSES = new List<PROGRESS>();

            PROJECTS.Add(BASELINE.PROJECT);
            BASELINES.Add(BASELINE);
            PROGRESSES.Add(PROGRESS);
            var project_dashboard = DashboardQueries.Multiple_Project_DashboardTransformation(PROJECTS.AsQueryable(), BASELINES, PROGRESSES, PROGRESS_ITEMS, RATES, VARIATIONS, BASELINE.GUID_PROJECT, buildStats);

            if (project_dashboard.Count() == 0)
                return null;

            return project_dashboard.First();
        }

        public static IQueryable<PROJECT_Dashboard> Multiple_Project_DashboardTransformation(IQueryable<PROJECT> PROJECTS,
            IEnumerable<BASELINE> BASELINES, IEnumerable<PROGRESS> PROGRESSES,
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
                if (live_baseline == null)
                    continue;

                PROGRESS live_progress = PROGRESSES.FirstOrDefault(x => x.GUID_PROJECT == current_project.GUID && x.STATUS == ProgressStatus.Live);
                if (live_progress == null)
                    continue;

                IEnumerable<PROGRESS_ITEM> live_progresses = PROGRESS_ITEMS.Where(x => x.PROGRESS.GUID == live_progress.GUID);
                IEnumerable<BASELINE_ITEM> live_baseline_items = live_baseline.BASELINE_ITEM.Where(x => !x.BY_DURATION);
                IEnumerable<RATE> project_rates = RATES.Where(x => x.GUID_PROJECT == current_project.GUID);
                IEnumerable<VARIATION> approved_project_variations = VARIATIONS.Where(x => x.GUID_PROJECT == current_project.GUID);

                IEnumerable<BASELINE_ITEMProgress> project_baseline_item_progresses = ProgressQueries.OffsiteDirectProgressItemTransformation(
                live_baseline_items.AsQueryable(), current_project, live_progress, project_rates, live_progresses, approved_project_variations).ToArray().AsEnumerable();

                var current_project_dashboard = new PROJECT_Dashboard(project_baseline_item_progresses, live_progress, current_project.WORKPACK, approved_project_variations, current_project.NUMBER)
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