using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Base;
using BluePrints.Common.Misc;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.Utils;
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

        public PROJECT_Dashboard(IEnumerable<PROGRESS> PROGRESSES, IEnumerable<SUBJOB> SUBJOBS, string project_number, decimal currency_conversion, IPrimeroEntitiesUnitOfWork PrimeroUOW, DateTime? fixedStartDate = null, DateTime? fixedDataDate = null, bool forceRetrieveRemainingDataPoints = false, bool IsVariationSeparated = false)
        {
            TimeSpan reporting_interval = ChronologicalHelpers.GetDefaultIntervalTimeSpan();
            DateTime? earliest_first_aligned_data_date;

            if (fixedStartDate != null && fixedDataDate != null)
                earliest_first_aligned_data_date = ChronologicalHelpers.RewindDataDate((DateTime)fixedStartDate, (DateTime)fixedDataDate, new TimeSpan(7, 0, 0, 0));
            else
                earliest_first_aligned_data_date = ChronologicalHelpers.GetEarliestFirstAlignedDataDate(PROGRESSES);

            DateTime? latest_data_date = fixedDataDate == null ? ChronologicalHelpers.GetReportLastDataDate(PROGRESSES) : fixedDataDate;
            
            FullStatsBuilder fullStatsBuilder = null;
            if(latest_data_date != null && earliest_first_aligned_data_date != null)
            {
                fullStatsBuilder = new FullStatsBuilder(project_number, currency_conversion, reporting_interval, (DateTime)earliest_first_aligned_data_date, SUBJOBS, (DateTime)latest_data_date, PrimeroUOW);

                //only forecast will have variation separated
                List<X_REPORTABLES> reportables = BluePrintsContextHelper.GetReportablesSummary(project_number, IsVariationSeparated);
                List<WBSReportable> WBSSummaries;
                if (!IsVariationSeparated)
                    WBSSummaries = reportables.GroupBy(x => new { x.SubJobCode, x.DisciplineCode, x.CommodityCode }).Distinct().Select(x => new WBSReportable(x.Key.SubJobCode, x.Key.DisciplineCode, x.Key.CommodityCode, string.Empty, (DateTime)latest_data_date, reporting_interval, (DateTime)earliest_first_aligned_data_date, Convert.ToDecimal(x.Sum(r => r.BUDGET_UNITS)), Convert.ToDecimal(x.Sum(r => r.TOTAL_UNITS)), Convert.ToDecimal(x.Sum(r => r.BUDGET_UNITS)), Convert.ToDecimal(x.Sum(r => r.TOTAL_UNITS)), Convert.ToDecimal(x.Sum(r => r.BUDGET_COSTS)), Convert.ToDecimal(x.Sum(r => r.TOTAL_COSTS)), forceRetrieveRemainingDataPoints)).ToList();
                else
                    WBSSummaries = reportables.Select(x => new WBSReportable(x.SubJobCode, x.DisciplineCode, x.CommodityCode, x.VariationCode, (DateTime)latest_data_date, reporting_interval, (DateTime)earliest_first_aligned_data_date, Convert.ToDecimal(x.BUDGET_UNITS), Convert.ToDecimal(x.TOTAL_UNITS), Convert.ToDecimal(x.BUDGET_UNITS), Convert.ToDecimal(x.TOTAL_UNITS), Convert.ToDecimal(x.BUDGET_COSTS), Convert.ToDecimal(x.TOTAL_COSTS), forceRetrieveRemainingDataPoints)).ToList();

                //some jobs can exist both in design and construction, so group it up to make sure uniqueness
                var groupedWBSSummaries = WBSSummaries.GroupBy(x => new { x.SUBJOB_CODE, x.DISCIPLINE_CODE, x.COMMODITY_CODE, x.VARIATION_CODE }).Select(group => group.ToList());
                List<WBSReportable> uniqueWBSSummaries = groupedWBSSummaries.Select(x => x.First()).ToList();
                //For Debugging
                //List<WBSReportable> wbsReportables = uniqueWBSSummaries.Where(x => x.SUBJOB_CODE == "30202-000-00-I1" && x.DISCIPLINE_CODE == "PM01" && x.COMMODITY_CODE == "G02" && x.VARIATION_CODE == string.Empty).ToList();
                //string s = wbsReportables.Count.ToString();

                Stats = new ProjectSummaryStats(uniqueWBSSummaries, (DateTime)latest_data_date, reporting_interval, (DateTime)earliest_first_aligned_data_date, forceRetrieveRemainingDataPoints, false);
                projectSummarizer = new FullSummarizer((ProjectSummaryStats)Stats, fullStatsBuilder, project_number, true);
            }
        }

        public List<TENDER_PROFILE_ITEM> TenderProfileItems { get; set; }

        FullSummarizer projectSummarizer { get; set; }
        public ProgressStats Stats
        {
            get { return GetProperty(() => Stats); }
            set { SetProperty(() => Stats, value); }
        }

        public void BuildStats(DashboardEXOQueryType dashboardEXOQueryType = DashboardEXOQueryType.TimeAndMaterial, bool showLoadingScreen = true, decimal weightingPortion = 1, bool forceRetrieveAllJobs = false, bool forceRetrieveAllUnits = false, bool forceRetrieveAllPOs = false, List<StatsCalculationType> calcTypes = null, bool useProductivityFactorOnRemaining = false, bool isVariationSeparated = false, bool isByWeek = false)
        {
            if (projectSummarizer == null)
                return;

            if (calcTypes == null)
                calcTypes = BluePrintsDataUtils.AllCalcTypes;

            projectSummarizer.Build(showLoadingScreen, weightingPortion, calcTypes, isVariationSeparated, useProductivityFactorOnRemaining);

            if (calcTypes.Contains(StatsCalculationType.Burned))
                //Build burned must come after build so that remaining can be retrieved for remaining actual
                projectSummarizer.BuildBurnedDataPoints(dashboardEXOQueryType, true, showLoadingScreen, forceRetrieveAllJobs, forceRetrieveAllUnits, forceRetrieveAllPOs, isByWeek);

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

        public List<DashboardFlatStructure> Subjob_Dashboards { get; set; }
        public List<Dashboard_Export_Data_Point> Export_Data { get; set; }

        //for project plan tender profile saving
        public IBluePrintsEntitiesUnitOfWork BluePrintsEntitiesUnitOfWork { get; set; }
    }

    public static class DashboardQueries
    {
        public static PROJECT_Dashboard Single_Project_DashboardTransformation(PROJECT PROJECT, IEnumerable<PROGRESS> PROGRESS, DateTime? fixedStartDate = null, DateTime? fixedDataDate = null, bool forceRetrieveRemainingDataPoints = false, bool isVariationSeparated = false)
        {
            List<PROJECT> PROJECTS = new List<PROJECT>();

            PROJECTS.Add(PROJECT);
            var project_dashboard = DashboardQueries.Multiple_Project_DashboardTransformation(PROJECTS.AsQueryable(), PROGRESS, PROJECT.GUID, fixedStartDate, fixedDataDate, forceRetrieveRemainingDataPoints, isVariationSeparated);

            if (project_dashboard.Count() == 0)
                return null;

            return project_dashboard.First();
        }

        public static IQueryable<PROJECT_Dashboard> Multiple_Project_DashboardTransformation(IQueryable<PROJECT> PROJECTS, IEnumerable<PROGRESS> PROGRESSES, 
            Guid? project_guid = null, DateTime? fixedStartDate = null, DateTime? fixedDataDate = null, bool forceRetrieveRemainingDataPoints = false, bool isVariationSeparated = false)
        {
            IQueryable<PROJECT> project_single_or_active_selection;
            if (project_guid != null)
                project_single_or_active_selection = PROJECTS.Where(x => x.GUID == project_guid);
            else
                project_single_or_active_selection = PROJECTS.Where(x => x.STATUS == ProjectStatus.Active);

            List<PROJECT_Dashboard> project_dashboard = new List<PROJECT_Dashboard>();
            foreach (var current_project in project_single_or_active_selection)
            {
                PROGRESS live_baseline_progress = PROGRESSES.FirstOrDefault(x => x.GUID_PROJECT == current_project.GUID && x.STATUS == ProgressStatus.Live && x.TYPE == PhaseType.Design);
                PROGRESS live_estimation_direct_progress = PROGRESSES.FirstOrDefault(x => x.GUID_PROJECT == current_project.GUID && x.STATUS == ProgressStatus.Live && x.TYPE == PhaseType.Construct);

                List<PROGRESS> projectProgresses = new List<PROGRESS>();
                if (live_baseline_progress != null)
                    projectProgresses.Add(live_baseline_progress);

                if (live_estimation_direct_progress != null)
                    projectProgresses.Add(live_estimation_direct_progress);

                IPrimeroEntitiesUnitOfWork primeroUOW = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(current_project.OfficeNameForExo).CreateUnitOfWork();
                var current_project_dashboard = new PROJECT_Dashboard(projectProgresses, current_project.SUBJOB, current_project.NUMBER, current_project.CURRENCYCONVERSION, primeroUOW, fixedStartDate, fixedDataDate, forceRetrieveRemainingDataPoints, isVariationSeparated)
                {
                    GUID = current_project.GUID,
                    Entity = current_project
                };

                project_dashboard.Add(current_project_dashboard);
            }

            return project_dashboard.AsQueryable();
        }
    }
}