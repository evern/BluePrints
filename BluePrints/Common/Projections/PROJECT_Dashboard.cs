using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Base;
using BluePrints.Common.Misc;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.P6EntitiesDataModel;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace BluePrints.Common.Projections
{
    public class PROJECT_Dashboard : BluePrintsProjectionBase<PROJECT>, IHaveSummary, IHaveStats
    {
        public int WBSLevel => 0;
        public PROJECT_Dashboard()
        {
        }

        public PROJECT_Dashboard(IEnumerable<IReportable> reportableItems, IEnumerable<PROGRESS> PROGRESSES, IEnumerable<SUBJOB> SUBJOBS, IEnumerable<VARIATION> VARIATIONS, string project_number, decimal currency_conversion, IPrimeroEntitiesUnitOfWork PrimeroUOW, DateTime? fixedStartDate = null, DateTime? fixedDataDate = null, bool forceRetrieveRemainingDataPoints = false)
        {
            TimeSpan reporting_interval = ChronologicalHelpers.GetDefaultIntervalTimeSpan();
            DateTime? earliest_first_aligned_data_date;

            if (fixedStartDate != null && fixedDataDate != null)
                earliest_first_aligned_data_date = ChronologicalHelpers.RewindDataDate((DateTime)fixedStartDate, (DateTime)fixedDataDate, new TimeSpan(7, 0, 0, 0));
            else
                earliest_first_aligned_data_date = ChronologicalHelpers.GetEarliestFirstAlignedDataDate(PROGRESSES);

            DateTime? latest_data_date = fixedDataDate == null ? ChronologicalHelpers.GetReportLastDataDate(PROGRESSES) : fixedDataDate;
            
            List<VariationAdjustment> projectVariationAdjustments = ProjectionHelpers.BuildProjectVariationAdjustments(VARIATIONS.AsQueryable(), reportableItems);
            FullStatsBuilder fullStatsBuilder = null;
            if(latest_data_date != null && earliest_first_aligned_data_date != null)
            {
                fullStatsBuilder = new FullStatsBuilder(project_number, currency_conversion, reporting_interval, (DateTime)earliest_first_aligned_data_date, SUBJOBS, (DateTime)latest_data_date, PrimeroUOW);
                Stats = new ProjectSummaryStats(reportableItems, (DateTime)latest_data_date, reporting_interval, (DateTime)earliest_first_aligned_data_date, projectVariationAdjustments, null, forceRetrieveRemainingDataPoints);
                projectSummarizer = new FullSummarizer((ProjectSummaryStats)Stats, fullStatsBuilder, project_number);
            }
        }

        public List<TENDER_PROFILE_ITEM> TenderProfileItems { get; set; }

        FullSummarizer projectSummarizer { get; set; }
        public ProgressStats Stats
        {
            get { return GetProperty(() => Stats); }
            set { SetProperty(() => Stats, value); }
        }

        public void BuildStats(bool showLoadingScreen = true, bool isCosts = false, decimal weightingPortion = 1, bool forceRetrieveAllJobs = false, bool forceRetrieveAllUnits = false, bool forceRetrieveAllPOs = false, List<StatsCalculationType> calcTypes = null, bool useProductivityFactorOnRemaining = false)
        {
            if (projectSummarizer == null)
                return;

            if (calcTypes == null)
                calcTypes = BluePrintsDataUtils.AllCalcTypes;

            projectSummarizer.Build(showLoadingScreen, isCosts, weightingPortion, calcTypes, useProductivityFactorOnRemaining);

            if (calcTypes.Contains(StatsCalculationType.Burned))
                //Build burned must come after build so that remaining can be retrieved for remaining actual
                projectSummarizer.BuildBurnedDataPoints(forceRetrieveAllJobs, forceRetrieveAllUnits, forceRetrieveAllPOs, showLoadingScreen);

            this.RaisePropertiesChanged();
        }

        public void RecalculateStats(bool isCosts, bool showLoadingScreen = true)
        {
            if (projectSummarizer == null)
                return;

            if(showLoadingScreen)
            {
                LoadingScreenManager.ShowLoadingScreen(0);
                LoadingScreenManager.SetMessage("Recalculating Summary...");
            }

            projectSummarizer.RecalculateStats(isCosts);

            if (showLoadingScreen)
                LoadingScreenManager.CloseLoadingScreen();
        }

        public List<DashboardTreeStructure> Subjob_TreeDashboards { get; set; }
        public List<DashboardFlatStructure> Subjob_Dashboards { get; set; }
        public List<Dashboard_Export_Data_Point> Export_Data { get; set; }
        public bool IHaveSubjob_Dashboards { get { return Subjob_TreeDashboards != null && Subjob_TreeDashboards.Count > 0; } }

        //for project plan tender profile saving
        public IBluePrintsEntitiesUnitOfWork BluePrintsEntitiesUnitOfWork { get; set; }
    }

    public static class DashboardQueries
    {
        public static PROJECT_Dashboard Single_Project_DashboardTransformation(PROJECT PROJECT, BASELINE BASELINE, ESTIMATE ESTIMATE, IEnumerable<PROGRESS> PROGRESS, IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS, IEnumerable<RATE> RATES,
            IEnumerable<VARIATION> VARIATIONS = null, bool buildStats = false, IEnumerable<USER> USERCollection = null, IEnumerable<BASELINE_ITEM_WORK> BASELINE_ITEM_WORKCollection = null, DateTime? fixedStartDate = null, DateTime? fixedDataDate = null, bool forceRetrieveRemainingDataPoints = false, bool showLoadingScreen = true)
        {
            List<PROJECT> PROJECTS = new List<PROJECT>();
            List<BASELINE> BASELINES = new List<BASELINE>();
            List<ESTIMATE> ESTIMATES = new List<ESTIMATE>();

            PROJECTS.Add(PROJECT);
            if (BASELINE != null)
                BASELINES.Add(BASELINE);

            if (ESTIMATE != null)
                ESTIMATES.Add(ESTIMATE);

            var project_dashboard = DashboardQueries.Multiple_Project_DashboardTransformation(PROJECTS.AsQueryable(), BASELINES, ESTIMATES, PROGRESS, PROGRESS_ITEMS, RATES, VARIATIONS, PROJECT.GUID, buildStats, USERCollection, BASELINE_ITEM_WORKCollection, fixedStartDate, fixedDataDate, showLoadingScreen, forceRetrieveRemainingDataPoints);

            if (project_dashboard.Count() == 0)
                return null;

            return project_dashboard.First();
        }

        public static IQueryable<PROJECT_Dashboard> Multiple_Project_DashboardTransformation(IQueryable<PROJECT> PROJECTS,
            IEnumerable<BASELINE> BASELINES, IEnumerable<ESTIMATE> ESTIMATES, IEnumerable<PROGRESS> PROGRESSES,
            IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS, IEnumerable<RATE> RATES, 
            IEnumerable<VARIATION> VARIATIONS = null,
            Guid? project_guid = null, bool buildStats = false, IEnumerable<USER> USERCollection = null, IEnumerable<BASELINE_ITEM_WORK> BASELINE_ITEM_WORKCollection = null, DateTime? fixedStartDate = null, DateTime? fixedDataDate = null, bool showLoadingScreen = false, bool forceRetrieveRemainingDataPoints = false)
        {
            IQueryable<PROJECT> project_single_or_active_selection;
            if (project_guid != null)
                project_single_or_active_selection = PROJECTS.Where(x => x.GUID == project_guid);
            else
                project_single_or_active_selection = PROJECTS.Where(x => x.STATUS == ProjectStatus.Active);

            List<PROJECT_Dashboard> project_dashboard = new List<PROJECT_Dashboard>();
            foreach (var current_project in project_single_or_active_selection)
            {
                BASELINE live_baseline = BASELINES.FirstOrDefault(x => x.GUID_PROJECT == current_project.GUID);
                ESTIMATE live_estimation_direct = ESTIMATES.FirstOrDefault(x => x.GUID_PROJECT == current_project.GUID);
                PROGRESS live_baseline_progress = PROGRESSES.FirstOrDefault(x => x.GUID_PROJECT == current_project.GUID && x.STATUS == ProgressStatus.Live && x.TYPE == PhaseType.Design);
                PROGRESS live_estimation_direct_progress = PROGRESSES.FirstOrDefault(x => x.GUID_PROJECT == current_project.GUID && x.STATUS == ProgressStatus.Live && x.TYPE == PhaseType.Construct);

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
                    //IEnumerable<BASELINE_ITEM> live_baseline_items = live_baseline.BASELINE_ITEM.Where(x => !x.BY_DURATION);
                    IEnumerable<BASELINE_ITEM> live_baseline_items = live_baseline.BASELINE_ITEM;

                    //prefetch attributes so that parallel operation won't attempt to retrieve from a db with closed connection for navigational properties
                    string preloadCode = string.Empty;
                    foreach (BASELINE_ITEM liveBaselineItem in live_baseline_items)
                    {
                        preloadCode = liveBaselineItem.Discipline_Code;
                        preloadCode = liveBaselineItem.Subjob_Name;
                        preloadCode = liveBaselineItem.Commodity_Code;
                        preloadCode = liveBaselineItem.Phase_Code;
                    }

                    IEnumerable<BASELINE_ITEMProgress> project_baseline_item_progresses = ProgressQueries.OffsiteDirectProgressItemTransformation(
                    live_baseline_items.AsQueryable(), current_project, live_baseline_progress, project_rates, live_baseline_progresses, approved_project_variations, false, null, DeliverableInternalNumberMode.Default, true, null, USERCollection, BASELINE_ITEM_WORKCollection, null, null, null, null, null, null, null, null, showLoadingScreen).ToArray().AsEnumerable();
                    reportables.AddRange(project_baseline_item_progresses);
                    current_project_progresses.Add(live_baseline_progress);
                }

                if (live_estimation_direct != null && live_estimation_direct_progress != null)
                {
                    IEnumerable<ESTIMATE_ITEM> live_estimation_direct_items = live_estimation_direct.ESTIMATE_ITEM;

                    //prefetch attributes so that parallel operation won't attempt to retrieve from a db with closed connection for navigational properties
                    string preloadCode = string.Empty;
                    foreach (ESTIMATE_ITEM liveEstimateItem in live_estimation_direct_items)
                    {
                        preloadCode = liveEstimateItem.Discipline_Code;
                        preloadCode = liveEstimateItem.Subjob_Name;
                        preloadCode = liveEstimateItem.Commodity_Code;
                        preloadCode = liveEstimateItem.Phase_Code;
                    }

                    IEnumerable<ESTIMATE_ITEMProgress> project_estimation_direct_item_progresses =
                    ESTIMATE_ITEMProjectionQueries.IDeliverable_Progress_Transformation(live_estimation_direct_items.AsQueryable(), current_project, project_rates, live_estimation_direct_progress, live_estimation_direct_progresses, true, approved_project_variations, false, null, showLoadingScreen, null, forceRetrieveRemainingDataPoints);
                    reportables.AddRange(project_estimation_direct_item_progresses);
                    current_project_progresses.Add(live_estimation_direct_progress);
                }

                IPrimeroEntitiesUnitOfWork primeroUOW = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(current_project.OfficeNameForExo == BluePrintsResources.OfficeMontreal).CreateUnitOfWork();
                var current_project_dashboard = new PROJECT_Dashboard(reportables, current_project_progresses, current_project.SUBJOB, approved_project_variations, current_project.NUMBER, current_project.CURRENCYCONVERSION, primeroUOW, fixedStartDate, fixedDataDate, forceRetrieveRemainingDataPoints)
                {
                    GUID = current_project.GUID,
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