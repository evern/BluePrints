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

        public ProgressStats Stats
        {
            get { return GetProperty(() => Stats); }
            set { SetProperty(() => Stats, value); }
        }

        FullSummarizer projectSummarizer { get; set; }

        public void InitializeSummarizer(IEnumerable<IReportable> reportableItems, PROGRESS LivePROGRESS, IEnumerable<WORKPACK> WORKPACKS, IEnumerable<VARIATION> VARIATIONS, IPrimeroEntitiesUnitOfWork PrimeroUOW = null, string projectNumber = "")
        {
            TimeSpan reportInterval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(LivePROGRESS);
            DateTime firstAlignedDataDate = ChronologicalHelpers.GenerateFirstAlignedDataDate(LivePROGRESS);
            List<VariationAdjustment> projectVariationAdjustments = ProjectionHelpers.BuildProjectVariationAdjustments(VARIATIONS.AsQueryable(), reportableItems.Select(x => (ISortableDeliverableProjection)x.Deliverable));

            FullStatsBuilder fullStatsBuilder = new FullStatsBuilder(Entity, LivePROGRESS, WORKPACKS, PrimeroUOW);

            Stats = new ProjectSummaryStats(reportableItems, LivePROGRESS, projectVariationAdjustments);
            projectSummarizer = new FullSummarizer((ProjectSummaryStats)Stats, fullStatsBuilder, projectNumber);
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

    public static class PROJECT_DashboardQueries
    {
        public static IQueryable<PROJECT_Dashboard> SummarizePROJECTDashboard(IQueryable<PROJECT> PROJECTS,
            IEnumerable<PROGRESS> PROGRESSES, IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS,
            IEnumerable<BASELINE> BASELINES, IEnumerable<RATE> RATES, IEnumerable<DELIVERABLES_STATUS> DELIVERABLES_STATUSES, 
            IEnumerable<VARIATION> VARIATIONS = null, Action raisePropertyChanged = null,
            Guid? SinglePROJECTGuid = null, bool IsShowProgress = true)
        {
            IQueryable<PROJECT> singleOrActivePROJECT;

            if (SinglePROJECTGuid != null)
                singleOrActivePROJECT = PROJECTS.Where(x => x.GUID == SinglePROJECTGuid);
                    //process only active PROJECTS
            else
                singleOrActivePROJECT = PROJECTS.Where(x => x.STATUS == ProjectStatus.Active);
            //process only active PROJECTS

            List<PROJECT_Dashboard> PROJECTDashboard = new List<PROJECT_Dashboard>();
            var bluePrintsUnitOfWork =
                BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            var p6UnitOfWork = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            //Cannot use primeroUnitOfWork because same context cannot be used on multithreaded environment
            //var primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            
            foreach (var localPROJECT in singleOrActivePROJECT)
            {
                BASELINE liveBASELINE =
                    BASELINES.FirstOrDefault(x => x.GUID_PROJECT == localPROJECT.GUID);
                if (liveBASELINE == null)
                    continue;

                PROGRESS livePROGRESS =
                    PROGRESSES.FirstOrDefault(x => x.GUID_PROJECT == localPROJECT.GUID && x.STATUS == ProgressStatus.Live);

                if (livePROGRESS == null)
                    continue;

                IEnumerable<PROGRESS_ITEM> livePROGRESS_ITEM = PROGRESS_ITEMS.Where(x => x.PROGRESS.GUID == livePROGRESS.GUID);

                IEnumerable<BASELINE_ITEM> liveBASELINE_ITEM = liveBASELINE.BASELINE_ITEM.Where(x => !x.BY_DURATION);
                IEnumerable<RATE> RATESByProject = RATES.Where(x => x.GUID_PROJECT == localPROJECT.GUID);
                IEnumerable<VARIATION> ApprovedVARIATIONSByProject = VARIATIONS.Where(x => x.GUID_PROJECT == localPROJECT.GUID);

                IEnumerable<PROGRESS_ITEMProjection> projectProgress_Items =
                    PROGRESS_ITEMProjectionQueries.JoinRATESAndPROGRESS_ITEMSOnBASELINE_ITEMS(
                        liveBASELINE_ITEM.AsQueryable(), livePROGRESS, livePROGRESS_ITEM, RATESByProject).ToArray().AsEnumerable();

                var currentPROJECT_Dashboard = new PROJECT_Dashboard()
                {
                    EntityKey = localPROJECT.GUID,
                    Entity = localPROJECT
                };
                
                currentPROJECT_Dashboard.InitializeSummarizer(projectProgress_Items, livePROGRESS, localPROJECT.WORKPACK, ApprovedVARIATIONSByProject, null, localPROJECT.NUMBER);
                PROJECTDashboard.Add(currentPROJECT_Dashboard);
            }

            return PROJECTDashboard.AsQueryable();
        }

        public static PROJECT_Dashboard SummarizeSinglePROJECTDashboard(PROJECT PROJECT, PROGRESS PROGRESS,
            IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS, IEnumerable<BASELINE_ITEM> BASELINE_ITEMS,
            BASELINE BASELINE, IEnumerable<RATE> RATES, IEnumerable<DELIVERABLES_STATUS> DELIVERABLES_STATUSES, bool buildStatsImmediately = false)
        {
            var bluePrintsUnitOfWork =
                BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            var p6UnitOfWork = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            var primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();

            IEnumerable<AREA> SubAREAS = PROJECT.AREA.Where(x => x.ParentEntityKey != null);

            IEnumerable<PROGRESS_ITEMProjection> progress_item =
                PROGRESS_ITEMProjectionQueries.JoinRATESAndPROGRESS_ITEMSOnBASELINE_ITEMS(
                        BASELINE_ITEMS.AsQueryable(), PROGRESS, PROGRESS_ITEMS, RATES)
                    .ToArray()
                    .AsEnumerable();

            var currentPROJECT_Dashboard = new PROJECT_Dashboard()
            {
                EntityKey = PROJECT.GUID,
                Entity = PROJECT
            };

            currentPROJECT_Dashboard.InitializeSummarizer(progress_item, PROGRESS, PROJECT.WORKPACK, PROJECT.VARIATION, primeroUnitOfWork, PROJECT.NUMBER);

            if (buildStatsImmediately)
                currentPROJECT_Dashboard.BuildStats(false);

            return currentPROJECT_Dashboard;
        }
    }
}