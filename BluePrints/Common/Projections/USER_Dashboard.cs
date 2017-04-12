using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.Data.Helpers;
using BluePrints.P6EntitiesDataModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    public class USER_Dashboard : ProjectionBase<PROJECT>, IHaveStats
    {
        public PROGRESS_ITEMProjection PROGRESS_ITEMProjection { get; set; }
        public PartialStatsBuilder DataPointsBuilder { get; set; }

        /// <summary>
        /// Used for summarizing selection of multiple user dashboard
        /// </summary>
        ProgressStats aggregateProgressStats { get; set; }
        public ProgressStats Stats
        {
            get
            {
                if (PROGRESS_ITEMProjection != null)
                    return PROGRESS_ITEMProjection.Stats;
                else if (aggregateProgressStats != null)
                    return aggregateProgressStats;

                return null;
            }
            set
            {
                aggregateProgressStats = value;
            }
        }

        public decimal UnitsProgressRatio
        {
            get
            {
                if (Stats.Earned.CurrentPeriodCumulativeDataPoint != null && Stats.Budgeted.CurrentPeriodCumulativeDataPoint != null)
                {
                    decimal earnedunits = Stats.Earned.CurrentPeriodCumulativeDataPoint.Units;
                    decimal plannedunits = Stats.Budgeted.CurrentPeriodCumulativeDataPoint.Units;

                    if (plannedunits > 0 && earnedunits > 0)
                        return earnedunits / plannedunits;
                    else
                        return 0;
                }
                else
                    return 0;
            }
        }

        public decimal CostsProgressRatio
        {
            get
            {
                if (Stats.Earned.CurrentPeriodCumulativeDataPoint != null && Stats.Budgeted.CurrentPeriodCumulativeDataPoint != null)
                {
                    decimal earnedcosts = Stats.Earned.CurrentPeriodCumulativeDataPoint.Costs;
                    decimal plannedcosts = Stats.Budgeted.CurrentPeriodCumulativeDataPoint.Costs;

                    if (plannedcosts > 0 && earnedcosts > 0)
                        return earnedcosts / plannedcosts;
                    else
                        return 0;
                }
                else
                    return 0;
            }
        }
    }

    public static class USER_DashboardQueries
    {
        public static IQueryable<USER_Dashboard> SummarizeUserDashboard(IQueryable<PROJECT> activePROJECTS, Func<IEnumerable<DELIVERABLES_STATUS>> getDELIVERABLES_STATUSESFunc, USER user, Action raisePropertyChanged = null)
        {
            List<USER_Dashboard> USER_Dashboards = new List<USER_Dashboard>();
            IP6EntitiesUnitOfWork p6UnitOfWork = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            Dictionary<PROJECT, IEnumerable<BASELINE_ITEM>> projectUSER_DELIVERABLES = new Dictionary<PROJECT, IEnumerable<BASELINE_ITEM>>();

            foreach (PROJECT activePROJECT in activePROJECTS)
            {
                BASELINE liveBASELINE = activePROJECT.BASELINE.FirstOrDefault(x => x.STATUS == BaselineStatus.Live);
                if (liveBASELINE == null)
                    continue;
    
                IQueryable<BASELINE_ITEM> userBASELINE_ITEMS = liveBASELINE.BASELINE_ITEM.Where(x => x.GUID_USER != null && x.GUID_USER == user.GUID).AsQueryable();
                if (userBASELINE_ITEMS.Count() == 0)
                    continue;

                projectUSER_DELIVERABLES.Add(activePROJECT, userBASELINE_ITEMS);
            }

            LoadingScreenManager.ShowLoadingScreen(projectUSER_DELIVERABLES.Count());
            foreach (KeyValuePair<PROJECT, IEnumerable<BASELINE_ITEM>> projectUSER_DELIVERABLE in projectUSER_DELIVERABLES)
            {
                PROJECT activePROJECT = projectUSER_DELIVERABLE.Key;
                IEnumerable<BASELINE_ITEM> userBASELINE_ITEMS = projectUSER_DELIVERABLE.Value;

                BASELINE liveBASELINE = activePROJECT.BASELINE.FirstOrDefault(x => x.STATUS == BaselineStatus.Live);
                IEnumerable<BASELINE_ITEM> projectBASELINE_ITEMS = liveBASELINE.BASELINE_ITEM.ToArray();

                IEnumerable<WORKPACK> projectWORKPACK = activePROJECT.WORKPACK;

                PROGRESS livePROGRESS = activePROJECT.PROGRESS.FirstOrDefault(x => x.STATUS == ProgressStatus.Live);
                if (livePROGRESS == null)
                    continue;
                IEnumerable<VARIATION> approvedVARIATION = activePROJECT.VARIATION.Where(x => x.APPROVED != null);

                IEnumerable<PROGRESS_ITEM> livePROGRESS_ITEMS = livePROGRESS.PROGRESS_ITEM;

                IEnumerable<RATE> projectRATES = activePROJECT.RATE;

                LoadingScreenManager.Progress();

                List<PROGRESS_ITEMProjection> PROJECT_PROGRESS_ITEMS = PROGRESS_ITEMProjectionQueries.JoinRATESAndPROGRESS_ITEMSOnBASELINE_ITEMSWithStats(userBASELINE_ITEMS.AsQueryable(), () => activePROJECT, () => livePROGRESS, () => liveBASELINE, () => projectWORKPACK, () => livePROGRESS_ITEMS, () => projectRATES, getDELIVERABLES_STATUSESFunc, () => approvedVARIATION, p6UnitOfWork).ToList();

                PROJECT_PROGRESS_ITEMS.ForEach(x => x.BuildStats());
                List<USER_Dashboard> userDashboard = PROJECT_PROGRESS_ITEMS.Select(x => new USER_Dashboard() { GUID = x.GUID, PROGRESS_ITEMProjection = x, Entity = activePROJECT }).ToList();
                USER_Dashboards.AddRange(userDashboard);
            }

            return USER_Dashboards.AsQueryable();
        }
    }
}