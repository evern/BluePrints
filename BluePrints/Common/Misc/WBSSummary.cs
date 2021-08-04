using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel.Reporting;
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
        public WBSSummary(IEnumerable<IReportable> reportables, DateTime reportingDataDate, TimeSpan reportingInterval, DateTime firstAlignedDataDate, IEnumerable<VariationAdjustment> projectVariationAdjustments, bool forceRetrieveRemainingDataPoints = false, bool allowPercentageOnZeroTotalUnits = false)
            : base(reportables, reportingDataDate, reportingInterval, firstAlignedDataDate, forceRetrieveRemainingDataPoints, allowPercentageOnZeroTotalUnits)
        {
            WBSReportables = reportables.GroupBy(x => new { x.Subjob_Name, x.Discipline_Code, x.Commodity_Code, x.Variation_Code }).Select(g => new WBSReportable(g.Key.Subjob_Name, g.Key.Discipline_Code, g.Key.Commodity_Code, g.Key.Variation_Code, g.ToList(), reportingDataDate, reportingInterval, firstAlignedDataDate, projectVariationAdjustments, forceRetrieveRemainingDataPoints, allowPercentageOnZeroTotalUnits)).ToList();
        }

        public void AddMissingActualsWBSReportables(string SubJobCode, string DisciplineCode, string CommodityCode, string VariationCode)
        {
            WBSReportables.Add(new WBSReportable(SubJobCode, DisciplineCode, CommodityCode, VariationCode, new List<IReportable>(), this.ReportingDataDate, this.ReportingInterval, this.FirstAlignedDataDate, new List<VariationAdjustment>(), false, false));
        }
    }

    public class WBSReportable : SummaryStats, IHaveWBSCodeString
    {
        public WBSReportable(string SubJobCode, string DisciplineCode, string CommodityCode, string VariationCode, IEnumerable<IReportable> Reportables, DateTime reportingDataDate, TimeSpan reportingInterval, DateTime firstAlignedDataDate, IEnumerable<VariationAdjustment> projectVariationAdjustments, bool forceRetrieveRemainingDataPoints = false, bool allowPercentageOnZeroTotalUnits = false)
            : base(Reportables, reportingDataDate, reportingInterval, firstAlignedDataDate, forceRetrieveRemainingDataPoints, allowPercentageOnZeroTotalUnits)
        {
            this.Reportables = Reportables;
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
            DataPointsGroup dataPointsGroup;
            if(isVariationSeparated)
                dataPointsGroup = dataPointsGroups.FirstOrDefault(x => x.SubJobCode == this.SUBJOB_CODE && x.DisciplineCode == this.DISCIPLINE_CODE && x.CommodityCode == this.COMMODITY_CODE && x.VariationCode == this.VARIATION_CODE);
            else
                dataPointsGroup = dataPointsGroups.FirstOrDefault(x => x.SubJobCode == this.SUBJOB_CODE && x.DisciplineCode == this.DISCIPLINE_CODE && x.CommodityCode == this.COMMODITY_CODE);

            if (dataPointsGroup != null)
                setProgressStatsFunc(this)(dataPointsGroup.DataPoints);
        }

        public void AssignWBSReportableData(Func<WBSReportable, Action<IEnumerable<ExoDataPoint>>> setProgressStatsFunc, IEnumerable<ExoDataPointsGroup> dataPointsGroups, bool isVariationSeparated)
        {
            ExoDataPointsGroup dataPointsGroup;
            if (isVariationSeparated)
                dataPointsGroup = dataPointsGroups.FirstOrDefault(x => x.SubJobCode == this.SUBJOB_CODE && x.DisciplineCode == this.DISCIPLINE_CODE && x.CommodityCode == this.COMMODITY_CODE && x.VariationCode == this.VARIATION_CODE);
            else
                dataPointsGroup = dataPointsGroups.FirstOrDefault(x => x.SubJobCode == this.SUBJOB_CODE && x.DisciplineCode == this.DISCIPLINE_CODE && x.CommodityCode == this.COMMODITY_CODE);

            if (dataPointsGroup != null)
                setProgressStatsFunc(this)(dataPointsGroup.ExoDataPoints);
        }
    }
}
