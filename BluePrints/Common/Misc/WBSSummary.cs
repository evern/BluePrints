using BaseModel.Data.Helpers;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.Utils;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Misc
{
    public class WBSSummary : SummaryStats
    {
        public List<WBSReportable> WBSReportables { get; private set; }
        /// <summary>
        /// Initializes a standard summary from a collection of grouped WBS projection
        /// </summary>
        /// <param name="reportables">Projection with WBS and progress</param>
        /// <param name="livePROGRESS">Live progress for reporting data date, generating first aligned data date and interval</param>
        /// <param name="projectVariationAdjustments">Project variation adjustments that will be matched against each deliverable projection</param>
        /// <param name="progressItemHaveStats">Deliverable projection stats area already generated</param>
        public WBSSummary(IEnumerable<IReportable> reportables, DateTime reportingDataDate, TimeSpan reportingInterval, DateTime firstAlignedDataDate, bool forceRetrieveRemainingDataPoints = false, bool allowPercentageOnZeroTotalUnits = false)
            : base(reportables, reportingDataDate, reportingInterval, firstAlignedDataDate, forceRetrieveRemainingDataPoints, allowPercentageOnZeroTotalUnits)
        {
            WBSReportables = reportables.GroupBy(x => new { x.Subjob_Name, x.Discipline_Code, x.Commodity_Code, x.Variation_Code }).Select(g => new WBSReportable(g.Key.Subjob_Name, g.Key.Discipline_Code, g.Key.Commodity_Code, g.Key.Variation_Code, g.ToList(), reportingDataDate, reportingInterval, firstAlignedDataDate, forceRetrieveRemainingDataPoints, allowPercentageOnZeroTotalUnits)).ToList();
        }

        /// <summary>
        /// Initializes a standard summary from a collection of grouped WBS projection
        /// </summary>
        /// <param name="livePROGRESS">Live progress for reporting data date, generating first aligned data date and interval</param>
        /// <param name="projectVariationAdjustments">Project variation adjustments that will be matched against each deliverable projection</param>
        /// <param name="progressItemHaveStats">Deliverable projection stats area already generated</param>
        public WBSSummary(List<WBSReportable> WBSReportables, DateTime reportingDataDate, TimeSpan reportingInterval, DateTime firstAlignedDataDate, bool forceRetrieveRemainingDataPoints = false, bool allowPercentageOnZeroTotalUnits = false)
            : base(reportingDataDate, reportingInterval, firstAlignedDataDate, WBSReportables.Sum(x => x.BudgetedUnits), WBSReportables.Sum(x => x.BudgetedCosts), WBSReportables.Sum(x => x.BudgetedQty), WBSReportables.Sum(x => x.TotalQty), WBSReportables.Sum(x => x.BudgetedCosts), WBSReportables.Sum(x => x.TotalCosts), forceRetrieveRemainingDataPoints, allowPercentageOnZeroTotalUnits)
        {
            this.WBSReportables = WBSReportables;
        }

        public void AddMissingActualsWBSReportables(string SubJobCode, string DisciplineCode, string CommodityCode, string VariationCode)
        {
            WBSReportables.Add(new WBSReportable(SubJobCode, DisciplineCode, CommodityCode, VariationCode, new List<IReportable>(), this.ReportingDataDate, this.ReportingInterval, this.FirstAlignedDataDate, false, false));
        }
    }

    public class WBSReportable : SummaryStats, IHaveWBSCodeString
    {
        public WBSReportable(string SubJobCode, string DisciplineCode, string CommodityCode, string VariationCode, IEnumerable<IReportable> Reportables, DateTime reportingDataDate, TimeSpan reportingInterval, DateTime firstAlignedDataDate, bool forceRetrieveRemainingDataPoints = false, bool allowPercentageOnZeroTotalUnits = false)
            : base(Reportables, reportingDataDate, reportingInterval, firstAlignedDataDate, forceRetrieveRemainingDataPoints, allowPercentageOnZeroTotalUnits)
        {
            this.Reportables = Reportables;
            this.SUBJOB_CODE = SubJobCode;
            this.DISCIPLINE_CODE = DisciplineCode;
            this.COMMODITY_CODE = CommodityCode;
            this.VARIATION_CODE = VariationCode;
        }

        public WBSReportable(string SubJobCode, string DisciplineCode, string CommodityCode, string VariationCode, DateTime reportingDataDate, TimeSpan reportingInterval, DateTime firstAlignedDataDate, decimal budgetedUnits, decimal totalUnits, decimal budgetedQty, decimal totalQty, decimal budgetedCosts, decimal totalCosts, bool forceRetrieveRemainingDataPoints = false, bool allowPercentageOnZeroTotalUnits = false)
            : base(reportingDataDate, reportingInterval, firstAlignedDataDate, budgetedUnits, totalUnits, budgetedQty, totalQty, budgetedCosts, totalCosts, forceRetrieveRemainingDataPoints, allowPercentageOnZeroTotalUnits)
        {
            this.Reportables = new List<IReportable>();
            this.SUBJOB_CODE = SubJobCode;
            this.DISCIPLINE_CODE = DisciplineCode;
            this.COMMODITY_CODE = CommodityCode;
            this.VARIATION_CODE = VariationCode;
        }

        public string SUBJOB_CODE { get; set; }
        public string DISCIPLINE_CODE { get; set; }
        public string COMMODITY_CODE { get; set; }
        public string VARIATION_CODE { get; set; }

        public void AssignWBSReportableData(Func<WBSReportable, Action<IEnumerable<Data.DataPoint>>> setProgressStatsFunc, IEnumerable<DataPointsGroup> dataPointsGroups, bool isVariationSeparated)
        {
            IEnumerable<DataPointsGroup> filterDataPointsGroups = dataPointsGroups.Where(x => x.SubJobCode == this.SUBJOB_CODE && x.DisciplineCode == this.DISCIPLINE_CODE && x.CommodityCode == this.COMMODITY_CODE);
            if (isVariationSeparated)
                filterDataPointsGroups = filterDataPointsGroups.Where(x => x.VariationCode == this.VARIATION_CODE);

            DataPointsGroup dataPointsGroup = filterDataPointsGroups.FirstOrDefault();
            if (dataPointsGroup != null)
                setProgressStatsFunc(this)(dataPointsGroup.DataPoints);
        }

        public void AssignWBSReportableData(Func<WBSReportable, Action<IEnumerable<X_WBS_GROUPED_DATAPOINT>>> setProgressStatsFunc, IEnumerable<X_WBS_GROUPED_DATAPOINT> dataPointsGroups, bool isVariationSeparated)
        {
            IEnumerable<X_WBS_GROUPED_DATAPOINT> filterDataPointsGroups = dataPointsGroups.Where(x => x.SubJobCode == this.SUBJOB_CODE && x.DisciplineCode == this.DISCIPLINE_CODE && x.CommodityCode == this.COMMODITY_CODE);
            if (isVariationSeparated)
                filterDataPointsGroups = filterDataPointsGroups.Where(x => x.VariationCode == this.VARIATION_CODE);

            setProgressStatsFunc(this)(filterDataPointsGroups);
        }

        public void AssignWBSReportableData(Func<WBSReportable, Action<IEnumerable<ExoDataPoint>>> setProgressStatsFunc, IEnumerable<ExoDataPointsGroup> dataPointsGroups, bool isVariationSeparated)
        {
            IEnumerable<ExoDataPointsGroup> filterDataPointsGroups = dataPointsGroups.Where(x => x.SubJobCode == this.SUBJOB_CODE && x.DisciplineCode == this.DISCIPLINE_CODE && x.CommodityCode == this.COMMODITY_CODE);
            if (isVariationSeparated)
                filterDataPointsGroups = filterDataPointsGroups.Where(x => x.VariationCode == this.VARIATION_CODE);

            ExoDataPointsGroup dataPointsGroup = filterDataPointsGroups.FirstOrDefault();
            if (dataPointsGroup != null)
                setProgressStatsFunc(this)(dataPointsGroup.ExoDataPoints);
        }

        /// <summary>
        /// Build remaining actual data by combining burned and remaining datapoints, using burned / earned for productivity. Requires burned, earned and remaining to already exist on WBS summary
        /// </summary>
        public void SummariseRemainingActualData()
        {
            decimal defaultProductivity = decimal.Parse(BluePrintsResources.Default_Productivity);
            //establish remaining data points
            IEnumerable<ViewModel.Reporting.DataPoint> burnedDataPoints = this.Burned.GetData();
            IEnumerable<ViewModel.Reporting.DataPoint> earnedDataPoints = this.Earned.GetData();
            IEnumerable<ViewModel.Reporting.DataPoint> remainingDataPoints = this.Remaining.GetData().Where(x => x.IsRemaining).ToList();
            DateTime? lastBurnedDate = burnedDataPoints.Count() == 0 ? (DateTime?)null : burnedDataPoints.Max(x => x.ProgressDate);
            decimal totalEarnedUnits = earnedDataPoints.Count() == 0 ? 0 : earnedDataPoints.Sum(x => x.Units);
            decimal totalBurnedUnits = burnedDataPoints.Count() == 0 ? 0 : burnedDataPoints.Sum(x => x.Units);
            decimal productivity = BluePrintsDataUtils.GetProductivity(totalEarnedUnits, totalBurnedUnits);
            //adjust remaining data points by productivity
            if (lastBurnedDate != null)
                remainingDataPoints = remainingDataPoints.Where(x => x.ProgressDate > lastBurnedDate).ToList();

            List<ViewModel.Reporting.DataPoint> remainingAdjustDataPoints = new List<ViewModel.Reporting.DataPoint>();
            foreach (ViewModel.Reporting.DataPoint remainingDataPoint in remainingDataPoints.Where(x => !x.IsProductivityInflated))
            {
                ViewModel.Reporting.DataPoint remainingAdjustDataPoint = new ViewModel.Reporting.DataPoint();
                DataUtils.ShallowCopy(remainingAdjustDataPoint, remainingDataPoint);
                remainingAdjustDataPoint.Units = remainingAdjustDataPoint.Units / productivity;
                remainingAdjustDataPoint.Costs = remainingAdjustDataPoint.Costs / productivity;
                remainingAdjustDataPoint.IsProductivityInflated = true;
                remainingAdjustDataPoints.Add(remainingAdjustDataPoint);
            }

            //burned data points will be plotted before the data date
            if (burnedDataPoints != null && burnedDataPoints.Count() > 0)
                remainingAdjustDataPoints.AddRange(burnedDataPoints.ToList());

            if (remainingDataPoints.All(x => x.IsFromP6))
                this.RemainingActual.SetFromP6();

            this.RemainingActual.SetData(remainingAdjustDataPoints);
            this.RemainingActual.StatsBuilt = true;
        }
    }
}
