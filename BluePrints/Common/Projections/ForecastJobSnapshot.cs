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

        public string AreaCode => BluePrintsDataUtils.GetAreaCode(SubJobCode);

        public string SubAreaCode => BluePrintsDataUtils.GetSubAreaCode(SubJobCode);

        public string SubJobCode { get; set; }
        public string DisciplineCode { get; set; }
        public string DisciplineDesc { get; set; }
        public string CommodityCode { get; set; }
        public string VariationCode { get; set; }
        public string DropDownPhase { get; set; }
        public decimal TenderBudget { get; set; }
        public decimal Budget { get; set; }
        public decimal ActualCosts { get; set; }
        public decimal ActualUnits { get; set; }
        public string CompareMask { get; set; }

        #region IForecastViewModel
        //used by detailed rows so that only P6 hour row can be edited
        public bool IsP6HoursRow { get; set; }
        public decimal DropDownIndirectBudget { get; set; }

        #region Rate Members
        public decimal P6RemainingUnits { get; set; }
        public decimal P6RemainingCosts { get; set; }
        public RATE FallBackRate { get; set; }
        public decimal P6NominalRate => P6RemainingUnits == 0 ? FallBackRate == null ? 0 : FallBackRate.RATE1 == null ? 0 : (decimal)FallBackRate.RATE1 : P6RemainingCosts / P6RemainingUnits;
        #endregion


        public decimal Productivity { get; set; }
        //store P6 units either native or from override
        public decimal? P6RemainingUnitsOverride { get; set; }
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
        public readonly DateTime MonthStartDate;
        public readonly DateTime MonthEndDate;
        private readonly DateTime firstViewDate;
        private readonly DateTime firstForecastDate;
        readonly IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> byDateForecastJobHourSnapshots;
        public ForecastDateSnapshot(IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> byDataDateForecastJobHourSnapshots, DateTime firstViewDate, DateTime date, DateTime dataDate)
        {
            QueryDate = date; 
            
            this.firstViewDate = firstViewDate;
            this.firstForecastDate = dataDate;

            firstForecastDate = new DateTime(dataDate.Date.Year, dataDate.Date.Month, 1).AddMonths(2).AddDays(-1);
            MonthStartDate = new DateTime(date.Date.Year, date.Date.Month, 1);
            MonthEndDate = MonthStartDate.AddMonths(1).AddDays(-1);

            this.byDateForecastJobHourSnapshots = byDataDateForecastJobHourSnapshots.Where(x => x.FORECAST_DATE != null &&  ((DateTime)x.FORECAST_DATE).Month == QueryDate.Month && ((DateTime)x.FORECAST_DATE).Year == QueryDate.Year);
        }

        public IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> POForecastSnapshots => byDateForecastJobHourSnapshots.Where(x => x.SNAPSHOT_TYPE == ForecastSnapshotValueType.POForecast);
        public IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> IndirectForecastSnapshots => byDateForecastJobHourSnapshots.Where(x => x.SNAPSHOT_TYPE == ForecastSnapshotValueType.IndirectForecast);
        public IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> MaterialForecastSnapshots => byDateForecastJobHourSnapshots.Where(x => x.SNAPSHOT_TYPE == ForecastSnapshotValueType.Material);
        public IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> ActualForecastSnapshots => byDateForecastJobHourSnapshots.Where(x => x.SNAPSHOT_TYPE == ForecastSnapshotValueType.Actual);
        public IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> P6Snapshots => byDateForecastJobHourSnapshots.Where(x => x.SNAPSHOT_TYPE == ForecastSnapshotValueType.P6Original);

        public DateTime QueryDate { get; set; }
        public decimal POForecastCosts => POForecastSnapshots.Sum(x => x.FORECAST_COST);
        public decimal IndirectForecastCosts => IndirectForecastSnapshots.Sum(x => x.FORECAST_COST);
        public decimal MaterialCosts => MaterialForecastSnapshots.Sum(x => x.FORECAST_COST);
        public decimal ActualCosts => ActualForecastSnapshots.Sum(x => x.FORECAST_COST);
        public decimal P6Hours
        {
            get
            {
                if (P6Snapshots.Count() == 0)
                    return 0;

                return P6Snapshots.Sum(x => x.FORECAST_QTY);
            }

        }

        public decimal P6Costs
        {
            get
            {
                if (P6Snapshots.Count() == 0)
                    return 0;

                return P6Snapshots.Sum(x => x.FORECAST_COST);
            }
        }

        public decimal P6Quantities => P6Snapshots.Sum(x => x.FORECAST_QTY);

        public decimal TotalCosts
        {
            get
            {
                return ActualCosts + MaterialCosts + POForecastCosts + IndirectForecastCosts + P6Costs;
            }
        }
    }

}
