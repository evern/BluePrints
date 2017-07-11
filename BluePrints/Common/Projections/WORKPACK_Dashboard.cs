using BluePrints.Common.Base;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.Common.Projections
{
    public class DisciplineCode_Dashboard : IHaveStats
    {
        public string Discipline_Code { get; set; }
        public List<CommodityCode_Dashboard> Commodity_Codes { get; set; }
        public ProgressStats Stats { get; set; }
    }

    public class CommodityCode_Dashboard : IHaveStats
    {
        public string Commodity_Code { get; set; }
        public ProgressStats Stats { get; set; }
    }

    public class WORKPACK_Dashboard : BluePrintsProjectionBase<WORKPACK>, IHaveStats
    {
        public ProgressStats Stats { get; set; }
        public IEnumerable<AREA> AvailableSubAreas { get; set; }

        public List<DisciplineCode_Dashboard> DisciplineCodes { get; set; }

        public bool HaveDisciplineCodes
        {
            get
            {
                return DisciplineCodes.Count() > 0;
            }
        }

        public void GroupProjectStats(ProjectSummaryStats projectStats, bool isLegacyProject)
        {
            Stats = projectStats.GroupStatsByWorkpack(this.Entity);
            if (!isLegacyProject)
            {
                IEnumerable<ExoDataPoint> burnedDataPoints = projectStats.GetBurnedDataPoints();
                DisciplineCodes = constructAllPossibleDisciplineCodes((SummaryStats)Stats, burnedDataPoints);
                List<DisciplineCode_Dashboard> removeDisciplineCodes = new List<DisciplineCode_Dashboard>();

                foreach(DisciplineCode_Dashboard disciplineCode in DisciplineCodes)
                {
                    disciplineCode.Stats = projectStats.GroupStatsByDisciplineCode((SummaryStats)Stats, disciplineCode.Discipline_Code);
                    if (disciplineCode.Stats == null)
                    {
                        removeDisciplineCodes.Add(disciplineCode);
                        continue;
                    }

                    List<CommodityCode_Dashboard> removeCommodityCodes = new List<CommodityCode_Dashboard>();
                    foreach (CommodityCode_Dashboard commodityCode in disciplineCode.Commodity_Codes)
                    {
                        commodityCode.Stats = projectStats.GroupStatsByCommodityCode((SummaryStats)disciplineCode.Stats, commodityCode.Commodity_Code);
                        if(commodityCode.Stats == null)
                        {
                            removeCommodityCodes.Add(commodityCode);
                        }
                    }

                    //omit stock codes without any stats
                    foreach (CommodityCode_Dashboard removeCommodityCode in removeCommodityCodes)
                    {
                        disciplineCode.Commodity_Codes.Remove(removeCommodityCode);
                    }
                }

                //omit stock codes without any stats
                foreach(DisciplineCode_Dashboard removeDisciplineCode in removeDisciplineCodes)
                {
                    DisciplineCodes.Remove(removeDisciplineCode);
                }
            }
        }

        private List<DisciplineCode_Dashboard> constructAllPossibleDisciplineCodes(SummaryStats workpackSummaryStats, IEnumerable<ExoDataPoint> burnedDataPoints)
        {
            List<DisciplineCode_Dashboard> disciplineCodeDashboard = new List<DisciplineCode_Dashboard>();
            foreach(IReportable reportable in workpackSummaryStats.Reportables)
            {
                ISortableDeliverableProjection sortableDeliverable = reportable.Deliverable as ISortableDeliverableProjection;
                if(sortableDeliverable != null)
                {
                    string discipline_code = sortableDeliverable.Discipline_Code;
                    if (!disciplineCodeDashboard.Any(x => x.Discipline_Code == discipline_code))
                    {
                        DisciplineCode_Dashboard newDisciplineCode = new DisciplineCode_Dashboard() { Discipline_Code = discipline_code };
                        assignAllPossibleCommodityCodes(newDisciplineCode, workpackSummaryStats.Reportables, burnedDataPoints);
                        disciplineCodeDashboard.Add(newDisciplineCode);
                    }
                }
            }

            foreach(ExoDataPoint burnedDataPoint in burnedDataPoints)
            {
                string disciplineCode = burnedDataPoint.DisciplineCode;
                if (!disciplineCodeDashboard.Any(x => x.Discipline_Code == disciplineCode))
                {
                    DisciplineCode_Dashboard newDisciplineCode = new DisciplineCode_Dashboard() { Discipline_Code = disciplineCode };
                    assignAllPossibleCommodityCodes(newDisciplineCode, workpackSummaryStats.Reportables, burnedDataPoints);
                    disciplineCodeDashboard.Add(newDisciplineCode);
                }
            }

            return disciplineCodeDashboard;
        }

        private void assignAllPossibleCommodityCodes(DisciplineCode_Dashboard stockCodeDashboards, IEnumerable<IReportable> workpackReportables, IEnumerable<ExoDataPoint> burnedDataPoints)
        {
            List<CommodityCode_Dashboard> commodityCodes = new List<CommodityCode_Dashboard>();
            foreach (IReportable reportable in workpackReportables)
            {
                string commodityCode = ((ISortableDeliverableProjection)reportable.Deliverable).Commodity_Code;
                if(!commodityCodes.Any(x => x.Commodity_Code == commodityCode))
                {
                    commodityCodes.Add(new CommodityCode_Dashboard() { Commodity_Code = commodityCode });
                }
            }

            foreach (ExoDataPoint burnedDataPoint in burnedDataPoints)
            {
                string commodityCode = burnedDataPoint.CommodityCode;
                if (!commodityCodes.Any(x => x.Commodity_Code == commodityCode))
                {
                    commodityCodes.Add(new CommodityCode_Dashboard() { Commodity_Code = commodityCode });
                }
            }

            stockCodeDashboards.Commodity_Codes = commodityCodes;
        }

        public void SetAvailableSubAreas(IEnumerable<AREA> SUBAREACollection)
        {
            AvailableSubAreas = SUBAREACollection.Where(x => x.GUID_PARENT == Entity.GUID_DAREA);
            this.RaisePropertyChanged();
        }

        #region WORKPACK Mapping
        public bool IsGetModifiedWORKPACK_ASSIGNMENTS { get; set; }
        public ICollection<WORKPACK_ASSIGNMENT> ObservableWORKPACK_ASSIGNMENTS
        {
            get
            {
                return
                    Entity.WORKPACK_ASSIGNMENT.Where(x => x.ISMODIFIEDBASELINE == IsGetModifiedWORKPACK_ASSIGNMENTS).OrderBy(x => x.LOW_VALUE)
                        .ToList();
            }
        }

        public virtual ICollection<BASELINE_ITEM> BASELINE_ITEMS { get; set; }

        public decimal ASSIGNED_UNITS
        {
            get
            {
                return Stats.totalUnits * ObservableWORKPACK_ASSIGNMENTS.Sum(x => (x.HIGH_VALUE - x.LOW_VALUE) + 0.01m);
            }
        }

        public decimal ASSIGNED_PERCENTAGE
        {
            get
            {
                return ObservableWORKPACK_ASSIGNMENTS.Sum(x => (x.HIGH_VALUE - x.LOW_VALUE) + 0.01m);
            }
        }
        #endregion
    }

    public static class WORKPACK_DashboardQueries
    {
        public static IQueryable<WORKPACK_Dashboard> SummarizeWORKPACKDashboard(IQueryable<WORKPACK> WORKPACKS,
            PROJECT_Dashboard projectDashboard, IEnumerable<AREA> subAreaCollection = null)
        {
            IEnumerable<WORKPACK_Dashboard> projectWORKPACKDashboards =
                WORKPACKS.Where(x => x.GUID_PROJECT == projectDashboard.EntityKey)
                    .Select(x => new WORKPACK_Dashboard() {EntityKey = x.GUID, Entity = x});
            List<WORKPACK_Dashboard> newWORKPACKDashboards = projectWORKPACKDashboards.ToList();
            newWORKPACKDashboards.ForEach(x => x.GroupProjectStats((ProjectSummaryStats)projectDashboard.Stats, projectDashboard.Entity.USELEGACYWORKPACK));
            if(subAreaCollection != null)
                newWORKPACKDashboards.ForEach(x => x.SetAvailableSubAreas(subAreaCollection));

            return newWORKPACKDashboards.AsQueryable();
        }

        public static IQueryable<WORKPACK_Dashboard> SummarizeWORKPACKDashboard(IQueryable<WORKPACK> WORKPACKS,
            PROGRESS PROGRESS, BASELINE BASELINE,
            IEnumerable<BASELINE_ITEM> BASELINE_ITEMS,
            IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS,
            IEnumerable<RATE> RATES,
            IEnumerable<DELIVERABLES_STATUS> DELIVERABLES_STATUSES)
        {
            var returnWORKPACK_Dashboard = new List<WORKPACK_Dashboard>();

            var projectDashboard =
                PROJECT_DashboardQueries.SummarizeSinglePROJECTDashboard(BASELINE.PROJECT, PROGRESS,
                    PROGRESS_ITEMS, BASELINE_ITEMS, BASELINE, RATES, DELIVERABLES_STATUSES, true);

            return SummarizeWORKPACKDashboard(WORKPACKS, projectDashboard);
        }
    }
}