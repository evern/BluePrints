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

        public void InitializeSummarizer(IEnumerable<PROGRESS_ITEMProjection> progress_items, BASELINE LiveBASELINE, PROGRESS LivePROGRESS, IEnumerable<WORKPACK> WORKPACKS, IEnumerable<WORKPACK_ASSIGNMENT> WORKPACK_ASSIGNMENTS, IEnumerable<VARIATION> VARIATIONS, IBluePrintsEntitiesUnitOfWork BluePrintsUOW = null, IP6EntitiesUnitOfWork P6UOW = null, IPrimeroEntitiesUnitOfWork PrimeroUOW = null, string projectNumber = "")
        {
            TimeSpan reportInterval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(LivePROGRESS);
            DateTime firstAlignedDataDate = ChronologicalHelpers.GenerateFirstAlignedDataDate(LivePROGRESS);
            List<VariationAdjustment> projectVariationAdjustments = ProjectionHelpers.BuildProjectVariationAdjustments(VARIATIONS.AsQueryable(), progress_items.Select(x => x.Entity));

            FullStatsBuilder fullStatsBuilder = new FullStatsBuilder(Entity, LiveBASELINE, LivePROGRESS, WORKPACKS, WORKPACK_ASSIGNMENTS, P6UOW, PrimeroUOW);

            Stats = new ProjectSummaryStats(progress_items, LivePROGRESS, projectVariationAdjustments);
            projectSummarizer = new FullSummarizer((ProjectSummaryStats)Stats, fullStatsBuilder, projectNumber);
        }

        public void BuildStats(bool showLoadingScreen = true, bool isCosts = false)
        {
            if (projectSummarizer == null)
                return;

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
            Func<IEnumerable<PROGRESS>> getLivePROGRESSESFunc, Func<IEnumerable<PROGRESS_ITEM>> getLivePROGRESS_ITEMFunc,
            Func<IEnumerable<BASELINE>> getLiveBASELINESFunc, Func<IEnumerable<RATE>> getRATESFunc, Func<IEnumerable<DELIVERABLES_STATUS>> getDELIVERABLES_STATUSESFunc, 
            Func<IEnumerable<VARIATION>> getApprovedVARIATIONFunc = null, Action raisePropertyChanged = null,
            Guid? SinglePROJECTGuid = null, bool IsShowProgress = true)
        {
            var LiveBASELINES = getLiveBASELINESFunc();
            var LivePROGRESSES = getLivePROGRESSESFunc();

            IEnumerable<VARIATION> ApprovedVARIATIONS = ApprovedVARIATIONS = getApprovedVARIATIONFunc();
            IEnumerable<RATE> AllRATES = getRATESFunc();
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
            var primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();

            foreach (var localPROJECT in singleOrActivePROJECT)
            {
                BASELINE liveBASELINE =
                    LiveBASELINES.FirstOrDefault(x => x.GUID_PROJECT == localPROJECT.GUID);
                if (liveBASELINE == null)
                    continue;

                PROGRESS livePROGRESS =
                    LivePROGRESSES.FirstOrDefault(x => x.GUID_PROJECT == localPROJECT.GUID && x.STATUS == ProgressStatus.Live);

                if (livePROGRESS == null)
                    continue;

                IEnumerable<PROGRESS_ITEM> livePROGRESS_ITEM =
                    getLivePROGRESS_ITEMFunc()
                        .Where(x => x.PROGRESS.GUID == livePROGRESS.GUID);

                IEnumerable<BASELINE_ITEM> liveBASELINE_ITEM = liveBASELINE.BASELINE_ITEM.Where(x => !x.BY_DURATION);
                IEnumerable<RATE> RATESByProject = AllRATES.Where(x => x.GUID_PROJECT == localPROJECT.GUID);
                IEnumerable<VARIATION> ApprovedVARIATIONSByProject =
                    ApprovedVARIATIONS.Where(x => x.GUID_PROJECT == localPROJECT.GUID);

                IEnumerable<PROGRESS_ITEMProjection> projectProgress_Items =
                    PROGRESS_ITEMProjectionQueries.JoinRATESAndPROGRESS_ITEMSOnBASELINE_ITEMS(
                        liveBASELINE_ITEM.AsQueryable(), () => livePROGRESS, () => liveBASELINE,
                        () => livePROGRESS_ITEM, () => RATESByProject, () => getDELIVERABLES_STATUSESFunc()).ToArray().AsEnumerable();

                var currentPROJECT_Dashboard = new PROJECT_Dashboard()
                {
                    EntityKey = localPROJECT.GUID,
                    Entity = localPROJECT
                    //VARIATIONS = ApprovedVARIATIONSByProject
                };

                currentPROJECT_Dashboard.InitializeSummarizer(projectProgress_Items, liveBASELINE, livePROGRESS, localPROJECT.WORKPACK, localPROJECT.WORKPACK.SelectMany(x => x.WORKPACK_ASSIGNMENT), ApprovedVARIATIONSByProject, bluePrintsUnitOfWork, p6UnitOfWork, primeroUnitOfWork, localPROJECT.NUMBER);

                PROJECTDashboard.Add(currentPROJECT_Dashboard);
            }

            return PROJECTDashboard.AsQueryable();
        }

        public static PROJECT_Dashboard SummarizeSinglePROJECTDashboard(PROJECT PROJECT, Func<PROGRESS> getPROGRESSFunc,
            Func<IEnumerable<PROGRESS_ITEM>> getPROGRESS_ITEMSFunc, Func<IEnumerable<BASELINE_ITEM>> getBASELINE_ITEMSFunc,
            Func<BASELINE> getBASELINEFunc, Func<IEnumerable<RATE>> getRATESFunc, Func<IEnumerable<DELIVERABLES_STATUS>> getDELIVERABLES_STATUSESFunc, bool buildStatsImmediately = false)
        {
            var bluePrintsUnitOfWork =
                BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            var p6UnitOfWork = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            var primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();

            IEnumerable<PROGRESS_ITEMProjection> progress_item =
                PROGRESS_ITEMProjectionQueries.JoinRATESAndPROGRESS_ITEMSOnBASELINE_ITEMS(
                        getBASELINE_ITEMSFunc().AsQueryable(), getPROGRESSFunc, getBASELINEFunc, getPROGRESS_ITEMSFunc, getRATESFunc, getDELIVERABLES_STATUSESFunc)
                    .ToArray()
                    .AsEnumerable();

            var currentPROJECT_Dashboard = new PROJECT_Dashboard()
            {
                EntityKey = PROJECT.GUID,
                Entity = PROJECT
            };

            currentPROJECT_Dashboard.InitializeSummarizer(progress_item, getBASELINEFunc(), getPROGRESSFunc(), PROJECT.WORKPACK, PROJECT.WORKPACK.SelectMany(x => x.WORKPACK_ASSIGNMENT), PROJECT.VARIATION, bluePrintsUnitOfWork, p6UnitOfWork, primeroUnitOfWork, PROJECT.NUMBER);

            if (buildStatsImmediately)
                currentPROJECT_Dashboard.BuildStats(false);

            return currentPROJECT_Dashboard;
        }
    }
}