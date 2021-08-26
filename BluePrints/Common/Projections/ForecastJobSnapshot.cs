using BaseModel.DataModel;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Misc;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.ViewModels;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    public class ForecastJobSnapshot : EntityBase, IHaveDisciplineDesc, IForecastViewModel
    {
        public List<ForecastDateSnapshot> DateCosts { get; set; }
        public ForecastJobSnapshot()
        {
            DateCosts = new List<ForecastDateSnapshot>();
        }
        public string PhaseCode => BluePrintsDataUtils.GetPhaseCode(SubJobCode);

        public string AreaCode
        {
            get
            {
                if (SubJobCode == string.Empty)
                    return string.Empty;
                else if (SubJobCode.Length < 15)
                    return string.Empty;

                return SubJobCode.Substring(6, 3);
            }
        }

        public string SubAreaCode
        {
            get
            {
                if (SubJobCode == string.Empty)
                    return string.Empty;
                else if (SubJobCode.Length < 15)
                    return string.Empty;

                return SubJobCode.Substring(10, 2);
            }
        }

        public string SubJobCode { get; set; }

        public string DisciplineCode { get; set; }

        public string DisciplineDesc { get; set; }

        public string CommodityCode { get; set; }

        public string DropDownPhase { get; set; }

        public decimal TenderBudget { get; set; }

        public decimal Budget { get; set; }

        public string CompareMask { get; set; }

        #region IForecastViewModel
        //used by detailed rows so that only P6 hour row can be edited
        public bool IsP6HoursRow { get; set; }
        public decimal DropDownIndirectBudget { get; set; }
        public IEnumerable<IForecastDateCostViewModel> ForecastDateCosts => DateCosts; 
        #endregion

        Dictionary<string, decimal> poStockCodeAttributes = null;
        public Dictionary<string, decimal> POStockCodeAttributes
        {
            get
            {
                if (poStockCodeAttributes == null)
                {
                    poStockCodeAttributes = new Dictionary<string, decimal>();
                    var groupByStockCodeSnapshots = DateCosts.SelectMany(x => x.POForecastSnapshots).GroupBy(x => x.STOCK_CODE).Select(group => new { StockCode = group.Key, Budget = group.First().PROJECT_BUDGET });
                    foreach (var groupByStockCodeSnapshot in groupByStockCodeSnapshots)
                    {
                        poStockCodeAttributes.Add(groupByStockCodeSnapshot.StockCode, groupByStockCodeSnapshot.Budget);
                    }
                }

                return poStockCodeAttributes;
            }
        }


        Dictionary<string, decimal> indirectStockCodeAttributes = null;
        public Dictionary<string, decimal> IndirectStockCodeAttributes
        {
            get
            {
                if (indirectStockCodeAttributes == null)
                {
                    indirectStockCodeAttributes = new Dictionary<string, decimal>();
                    var groupByStockCodeSnapshots = DateCosts.SelectMany(x => x.IndirectForecastSnapshots).GroupBy(x => x.STOCK_CODE).Select(group => new { StockCode = group.Key, Budget = group.First().PROJECT_BUDGET });
                    foreach (var groupByStockCodeSnapshot in groupByStockCodeSnapshots)
                    {
                        indirectStockCodeAttributes.Add(groupByStockCodeSnapshot.StockCode, groupByStockCodeSnapshot.Budget);
                    }
                }

                return indirectStockCodeAttributes;
            }
        }


        Dictionary<string, decimal> materialStockCodeAttributes = null;
        public Dictionary<string, decimal> MaterialStockCodeAttributes
        {
            get
            {
                if (materialStockCodeAttributes == null)
                {
                    materialStockCodeAttributes = new Dictionary<string, decimal>();
                    var groupByStockCodeSnapshots = DateCosts.SelectMany(x => x.MaterialForecastSnapshots).GroupBy(x => x.STOCK_CODE).Select(group => new { StockCode = group.Key, Budget = group.First().PROJECT_BUDGET });
                    foreach (var groupByStockCodeSnapshot in groupByStockCodeSnapshots)
                    {
                        materialStockCodeAttributes.Add(groupByStockCodeSnapshot.StockCode, groupByStockCodeSnapshot.Budget);
                    }
                }

                return materialStockCodeAttributes;
            }
        }


        Dictionary<string, decimal> actualStockCodeAttributes = null;
        public Dictionary<string, decimal> ActualStockCodeAttributes
        {
            get
            {
                if (actualStockCodeAttributes == null)
                {
                    actualStockCodeAttributes = new Dictionary<string, decimal>();
                    var groupByStockCodeSnapshots = DateCosts.SelectMany(x => x.ActualForecastSnapshots).GroupBy(x => x.STOCK_CODE).Select(group => new { StockCode = group.Key, Budget = group.First().PROJECT_BUDGET });
                    foreach (var groupByStockCodeSnapshot in groupByStockCodeSnapshots)
                    {
                        actualStockCodeAttributes.Add(groupByStockCodeSnapshot.StockCode, groupByStockCodeSnapshot.Budget);
                    }
                }

                return actualStockCodeAttributes;
            }
        }
    }

    public class ForecastDateSnapshot : IForecastDateCostViewModel
    {
        readonly IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> byDateForecastJobHourSnapshots;
        public ForecastDateSnapshot(IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> byDateForecastJobHourSnapshots, DateTime date)
        {
            Date = date;
            this.byDateForecastJobHourSnapshots = byDateForecastJobHourSnapshots.Where(x => x.FORECAST_DATE.Date == Date.Date);
        }

        public IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> POForecastSnapshots => byDateForecastJobHourSnapshots.Where(x => x.SNAPSHOT_TYPE == Common.ForecastSnapshotValueType.POForecast);
        public IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> IndirectForecastSnapshots => byDateForecastJobHourSnapshots.Where(x => x.SNAPSHOT_TYPE == Common.ForecastSnapshotValueType.IndirectForecast);
        public IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> MaterialForecastSnapshots => byDateForecastJobHourSnapshots.Where(x => x.SNAPSHOT_TYPE == Common.ForecastSnapshotValueType.Material);
        public IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> ActualForecastSnapshots => byDateForecastJobHourSnapshots.Where(x => x.SNAPSHOT_TYPE == Common.ForecastSnapshotValueType.Actual);
        public IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> P6Snapshots => byDateForecastJobHourSnapshots.Where(x => x.SNAPSHOT_TYPE == Common.ForecastSnapshotValueType.P6Original);
        public IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> P6OverrideSnapshots => byDateForecastJobHourSnapshots.Where(x => x.SNAPSHOT_TYPE == Common.ForecastSnapshotValueType.P6Override);
        public IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> ViewOverrideSnapshots => byDateForecastJobHourSnapshots.Where(x => x.SNAPSHOT_TYPE == Common.ForecastSnapshotValueType.DiscretionaryTotal);

        public DateTime Date { get; set; }
        public decimal POForecastCosts => POForecastSnapshots.Sum(x => x.FORECAST_COST);
        public decimal IndirectForecastCosts => IndirectForecastSnapshots.Sum(x => x.FORECAST_COST);
        public decimal MaterialCosts => MaterialForecastSnapshots.Sum(x => x.FORECAST_COST);
        public decimal ActualCosts => ActualForecastSnapshots.Sum(x => x.FORECAST_COST);
        public decimal P6Costs => P6Snapshots.Sum(x => x.FORECAST_COST);
        public decimal P6Quantities => P6Snapshots.Sum(x => x.FORECAST_QTY);
        public decimal? P6OverrideCost
        {
            get
            {
                if (P6OverrideSnapshots.Count() == 0)
                    return null;

                return P6OverrideSnapshots.Sum(x => x.FORECAST_COST);
            }
        }

        public decimal? P6OverrideQuantity
        {
            get
            {
                if (P6OverrideSnapshots.Count() == 0)
                    return null;

                return P6OverrideSnapshots.Sum(x => x.FORECAST_QTY);
            }
        }

        public decimal? ViewOverrideCost
        {
            get
            {
                if (ViewOverrideSnapshots.Count() == 0)
                    return null;

                return ViewOverrideSnapshots.Sum(x => x.FORECAST_COST);
            }
        }

        public decimal P6ViewCost
        {
            get
            {
                if (P6OverrideCost != null)
                    return (decimal)P6OverrideCost;

                return P6Costs;
            }
        }

        public decimal TotalCosts
        {
            get
            {
                if (ViewOverrideCost != null)
                    return (decimal)ViewOverrideCost;

                return ActualCosts + MaterialCosts + P6ViewCost + POForecastCosts + IndirectForecastCosts;
            }
        }
    }

}
