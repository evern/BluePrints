using BluePrints.Common.Base;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.Common.Projections
{
    public class StockCode_Dashboard : IHaveStats
    {
        public string Stock_Code { get; set; }
        public IEnumerable<CommodityCode_Dashboard> Commodity_Codes { get; set; }
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

        public IEnumerable<StockCode_Dashboard> StockCodes { get; set; }

        public bool HaveStockCodes
        {
            get
            {
                return StockCodes.Count() > 0;
            }
        }

        public void GroupProjectStats(ProjectSummaryStats projectStats, bool isLegacyProject)
        {
            Stats = projectStats.GroupStatsByWorkpack(this.Entity);
            if (!isLegacyProject)
            {
                StockCodes = constructStockCodes((SummaryStats)Stats);
                foreach(StockCode_Dashboard stockCode in StockCodes)
                {
                    stockCode.Stats = projectStats.GroupStatsByStockCode((SummaryStats)Stats, stockCode.Stock_Code);
                    foreach(CommodityCode_Dashboard commodityCode in stockCode.Commodity_Codes)
                    {
                        commodityCode.Stats = projectStats.GroupStatsByCommodityCode((SummaryStats)stockCode.Stats, commodityCode.Commodity_Code);
                    }
                }
            }
        }

        private IEnumerable<StockCode_Dashboard> constructStockCodes(SummaryStats workpackSummaryStats)
        {
            List<StockCode_Dashboard> stockCodeDashboards = new List<StockCode_Dashboard>();
            foreach(PROGRESS_ITEMProjection deliverable in workpackSummaryStats.Deliverable)
            {
                string stockCode = deliverable.Entity.Entity.StockCode;
                if (!stockCodeDashboards.Any(x => x.Stock_Code == stockCode))
                {
                    StockCode_Dashboard newStockCode = new StockCode_Dashboard() { Stock_Code = stockCode };
                    assignCommodityCodes(newStockCode, workpackSummaryStats.Deliverable);
                    stockCodeDashboards.Add(newStockCode);
                }
            }

            return stockCodeDashboards;
        }

        private void assignCommodityCodes(StockCode_Dashboard stockCodeDashboards, IEnumerable<PROGRESS_ITEMProjection> workpackDeliverables)
        {
            List<CommodityCode_Dashboard> commodityCodes = new List<CommodityCode_Dashboard>();
            foreach (PROGRESS_ITEMProjection deliverable in workpackDeliverables)
            {
                string commodityCode = deliverable.Entity.Entity.DOCTYPE.CODE;
                if(!commodityCodes.Any(x => x.Commodity_Code == commodityCode))
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
            Func<PROGRESS> getPROGRESSFunc, Func<BASELINE> getBASELINEFunc,
            Func<IEnumerable<BASELINE_ITEM>> getBASELINE_ITEMFunc,
            Func<IEnumerable<PROGRESS_ITEM>> getPROGRESS_ITEMSFunc,
            Func<IEnumerable<RATE>> getRATESFunc,
            Func<IEnumerable<DELIVERABLES_STATUS>> getDELIVERABLES_STATUSESFunc,
            bool getModifiedWORKPACK_ASSIGNMENT)
        {
            var returnWORKPACK_Dashboard = new List<WORKPACK_Dashboard>();

            var projectDashboard =
                PROJECT_DashboardQueries.SummarizeSinglePROJECTDashboard(getBASELINEFunc().PROJECT, getPROGRESSFunc,
                    getPROGRESS_ITEMSFunc, getBASELINE_ITEMFunc, getBASELINEFunc, getRATESFunc, getDELIVERABLES_STATUSESFunc, true);

            return SummarizeWORKPACKDashboard(WORKPACKS, projectDashboard);
        }
    }
}