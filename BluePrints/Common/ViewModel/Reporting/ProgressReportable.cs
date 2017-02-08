using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Data;
using BluePrints.P6EntitiesDataModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Mvvm.POCO;
using BluePrints.Common.Projections;
using BluePrints.Data.Attributes;

namespace BluePrints.Common.ViewModel.Reporting
{
    public class PROJECTSummary : SummarizableObject
    {
        public static PROJECTSummary Create()
        {
            return ViewModelSource.Create(() => new PROJECTSummary());
        }

        public decimal GrossProfit { get; set; }
        public decimal EfficiencyRatio { get; set; }
        public decimal ProgressRatio { get; set; }

        public override void RecalculateStats(bool isCost = false)
        {
            if (ReportableObjects == null)
                return;

            if (Summary_CumulativeEarned != null && Summary_CumulativeActual != null)
            {
                var totalEarnedCost = Summary_CumulativeEarned.Costs;
                var totalActualCost = Summary_CumulativeActual.Costs;

                GrossProfit = totalEarnedCost == 0 || totalActualCost == 0
                    ? 0
                    : (totalEarnedCost - totalActualCost) / totalEarnedCost;
            }

            decimal totalEarnedUOM = 0;
            decimal totalBurnedUOM = 0;
            decimal totalPlannedUOM = 0;

            if (isCost)
            {
                if (Summary_CumulativeEarned != null)
                    totalEarnedUOM = Summary_CumulativeEarned.Costs;
                if (Summary_CumulativeBurned != null)
                    totalBurnedUOM = Summary_CumulativeBurned.Costs;
                if (Summary_CumulativePlanned != null)
                    totalPlannedUOM = Summary_CumulativePlanned.Costs;
            }
            else
            {
                if (Summary_CumulativeEarned != null)
                    totalEarnedUOM = Summary_CumulativeEarned.Units;
                if (Summary_CumulativeBurned != null)
                    totalBurnedUOM = Summary_CumulativeBurned.Units;
                if (Summary_CumulativePlanned != null)
                    totalPlannedUOM = Summary_CumulativePlanned.Units;
            }

            EfficiencyRatio = totalEarnedUOM == 0 || totalBurnedUOM == 0
                ? 0
                : (totalEarnedUOM - totalBurnedUOM) / totalBurnedUOM;
            ProgressRatio = totalEarnedUOM == 0 || totalPlannedUOM == 0
                ? 0
                : (totalEarnedUOM - totalPlannedUOM) / totalPlannedUOM;
        }
    }

    public abstract class SummarizableObject : ProgressReportable
    {
        #region Calculation Parameters

        public DateTime FirstAlignedDataDate { get; set; }
        public TimeSpan IntervalPeriod { get; set; }

        #endregion

        public IEnumerable<ReportableObject> ReportableObjects { get; set; }
        public IEnumerable<RATE> RATES { get; set; }
        public IEnumerable<VARIATION> VARIATIONS { get; set; }
        public BASELINE LiveBASELINE { get; set; }
        public PROGRESS LivePROGRESS { get; set; }
        public IBluePrintsEntitiesUnitOfWork BluePrintsUnitOfWork { get; set; }
        public IP6EntitiesUnitOfWork P6UnitOfWork { get; set; }

        public virtual void RecalculateStats(bool isCosts)
        {
        }

        private decimal? final_budgetedunits { get; set; }

        public decimal Final_BudgetedUnits
        {
            get
            {
                if (final_budgetedunits == null)
                    if (ReportableObjects == null)
                        final_budgetedunits = 0;
                    else
                        final_budgetedunits =
                            ReportableObjects.Sum(x => x.BASELINE_ITEMJoinRATE.BASELINE_ITEM.TOTAL_HOURS);

                return (decimal) final_budgetedunits;
            }
        }

        private decimal? final_budgetedcosts { get; set; }

        public decimal Final_BudgetedCosts
        {
            get
            {
                if (final_budgetedcosts == null)
                    if (ReportableObjects == null)
                        final_budgetedcosts = 0;
                    else
                        final_budgetedcosts = ReportableObjects.Sum(x => x.BASELINE_ITEMJoinRATE.TOTAL_COSTS);

                return (decimal) final_budgetedcosts;
            }
        }

        private decimal? total_budgetedunits { get; set; }

        public decimal Total_BudgetedUnits
        {
            get
            {
                if (total_budgetedunits == null)
                    if (ReportableObjects == null)
                        total_budgetedunits = 0;
                    else
                        total_budgetedunits =
                            ReportableObjects.Sum(x => x.BASELINE_ITEMJoinRATE.BASELINE_ITEM.ESTIMATED_HOURS);

                return (decimal) total_budgetedunits;
            }
        }

        private decimal? total_budgetedcosts { get; set; }

        public decimal Total_BudgetedCosts
        {
            get
            {
                if (total_budgetedcosts == null)
                    if (ReportableObjects == null)
                        total_budgetedcosts = 0;
                    else
                        total_budgetedcosts = ReportableObjects.Sum(x => x.BASELINE_ITEMJoinRATE.ESTIMATED_COSTS);

                return (decimal) total_budgetedcosts;
            }
        }
    }

    public abstract class ReportableObject : ProgressReportable
    {
        public BASELINE_ITEMProjection BASELINE_ITEMJoinRATE { get; set; }

        private IEnumerable<VARIATION_ITEM> VARIATION_ITEMS { get; set; }

        private IEnumerable<PROGRESS_ITEM> progress_items;

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS
        {
            get { return progress_items; }
            set
            {
                if (value == null)
                {
                    PROGRESS_ITEMcurrent = new PROGRESS_ITEM();
                    PROGRESS_ITEMSafterreportingdate = new List<PROGRESS_ITEM>();
                    PROGRESS_ITEMSbeforereportingdate = new List<PROGRESS_ITEM>();
                    PROGRESS_ITEMSuptocurrentdate = new List<PROGRESS_ITEM>();
                }
                else
                {
                    if (PROGRESS_ITEMcurrent == null)
                        PROGRESS_ITEMcurrent =
                            value.Where(
                                y =>
                                    y.GUID_ORIBASEITEM == BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_ORIGINAL &&
                                    y.EARNED_DATE == ReportingDataDate).OrderBy(x => x.EARNED_UNITS).FirstOrDefault();
                    if (PROGRESS_ITEMSafterreportingdate == null)
                        PROGRESS_ITEMSafterreportingdate =
                            value.Where(
                                y =>
                                    y.GUID_ORIBASEITEM == BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_ORIGINAL &&
                                    y.EARNED_DATE > ReportingDataDate).ToList();
                    if (PROGRESS_ITEMSbeforereportingdate == null)
                        PROGRESS_ITEMSbeforereportingdate =
                            value.Where(
                                y =>
                                    y.GUID_ORIBASEITEM == BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_ORIGINAL &&
                                    y.EARNED_DATE < ReportingDataDate).ToList();
                    if (PROGRESS_ITEMSuptocurrentdate == null)
                        PROGRESS_ITEMSuptocurrentdate =
                            value.Where(
                                y =>
                                    y.GUID_ORIBASEITEM == BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_ORIGINAL &&
                                    y.EARNED_DATE <= ReportingDataDate).ToList();

                    progress_items = value;
                }
            }
        }

        private PROGRESS_ITEM PROGRESS_ITEMcurrent;

        public PROGRESS_ITEM PROGRESS_ITEMCurrent
        {
            get { return PROGRESS_ITEMcurrent; }
            set { PROGRESS_ITEMcurrent = value; }
        }

        private List<PROGRESS_ITEM> PROGRESS_ITEMSafterreportingdate;

        public List<PROGRESS_ITEM> PROGRESS_ITEMSAfterReportingDate
        {
            get { return PROGRESS_ITEMSafterreportingdate; }
        }

        private List<PROGRESS_ITEM> PROGRESS_ITEMSbeforereportingdate;

        public List<PROGRESS_ITEM> PROGRESS_ITEMSBeforeReportingDate
        {
            get { return PROGRESS_ITEMSbeforereportingdate; }
        }

        private List<PROGRESS_ITEM> PROGRESS_ITEMSuptocurrentdate;

        public List<PROGRESS_ITEM> PROGRESS_ITEMSUpToCurrentDate
        {
            get { return PROGRESS_ITEMSuptocurrentdate; }
        }

        public decimal VariationProductivity { get; set; }
        public decimal BaselineProductivity { get; set; }
        public decimal ActualProductivity { get; set; }
        public bool isDataPointsGeneratedFromP6 { get; set; }

        private decimal? pastPROGRESS_ITEMS_UNITS;

        public decimal PastPROGRESS_ITEMS_UNITS
        {
            get
            {
                if (pastPROGRESS_ITEMS_UNITS == null)
                    if (PROGRESS_ITEMSBeforeReportingDate == null)
                        pastPROGRESS_ITEMS_UNITS = 0;
                    else
                        pastPROGRESS_ITEMS_UNITS =
                            PROGRESS_ITEMSBeforeReportingDate.Sum(progress => progress.EARNED_UNITS);

                return (decimal) pastPROGRESS_ITEMS_UNITS;
            }
        }

        private decimal? futurePROGRESS_ITEMS_UNITS;

        public decimal FuturePROGRESS_ITEMS_UNITS
        {
            get
            {
                if (futurePROGRESS_ITEMS_UNITS == null)
                    if (PROGRESS_ITEMSAfterReportingDate == null)
                        futurePROGRESS_ITEMS_UNITS = 0;
                    else
                        futurePROGRESS_ITEMS_UNITS = PROGRESS_ITEMSAfterReportingDate.Sum(x => x.EARNED_UNITS);

                return (decimal) futurePROGRESS_ITEMS_UNITS;
            }
        }

        public decimal RemainingUnitsAfterDataDate
        {
            get { return BASELINE_ITEMJoinRATE.BASELINE_ITEM.TOTAL_HOURS - TOTAL_EARNED_UNITS; }
        }

        public decimal MinPercentage
        {
            get
            {
                if (PROGRESS_ITEMSBeforeReportingDate == null || BASELINE_ITEMJoinRATE == null ||
                    BASELINE_ITEMJoinRATE.BASELINE_ITEM.TOTAL_HOURS == 0)
                    return 0;
                else
                    return PastPROGRESS_ITEMS_UNITS / BASELINE_ITEMJoinRATE.BASELINE_ITEM.TOTAL_HOURS;
            }
        }

        public decimal MaxPercentage
        {
            get
            {
                if (BASELINE_ITEMJoinRATE == null)
                    return 0;
                else if (PROGRESS_ITEMSBeforeReportingDate == null || BASELINE_ITEMJoinRATE.BASELINE_ITEM.TOTAL_HOURS == 0)
                    return 1;
                else
                    return (BASELINE_ITEMJoinRATE.BASELINE_ITEM.TOTAL_HOURS - FuturePROGRESS_ITEMS_UNITS) /
                           BASELINE_ITEMJoinRATE.BASELINE_ITEM.TOTAL_HOURS;
            }
        }

        public decimal BASELINE_PERCENTAGE
        {
            get
            {
                if (BASELINE_ITEMJoinRATE == null || BASELINE_ITEMJoinRATE.BASELINE_ITEM == null ||
                    BASELINE_ITEMJoinRATE.BASELINE_ITEM.ESTIMATED_HOURS == 0)
                    return 0;

                return TOTAL_EARNED_UNITS / BASELINE_ITEMJoinRATE.BASELINE_ITEM.ESTIMATED_HOURS;
            }
        }

        public decimal TOTAL_PERCENTAGE
        {
            get
            {
                if (BASELINE_ITEMJoinRATE == null || BASELINE_ITEMJoinRATE.BASELINE_ITEM == null ||
                    BASELINE_ITEMJoinRATE.BASELINE_ITEM.ESTIMATED_HOURS == 0)
                    return 0;

                return TOTAL_EARNED_UNITS / BASELINE_ITEMJoinRATE.BASELINE_ITEM.TOTAL_HOURS;
            }
        }

        public decimal PERIOD_EARNED_PERCENTAGE
        {
            get
            {
                if (BASELINE_ITEMJoinRATE == null || PROGRESS_ITEMCurrent == null ||
                    BASELINE_ITEMJoinRATE.BASELINE_ITEM.TOTAL_HOURS == 0)
                    return 0;

                return PROGRESS_ITEMCurrent.EARNED_UNITS / BASELINE_ITEMJoinRATE.BASELINE_ITEM.TOTAL_HOURS;
            }
        }

        public decimal PERIOD_EARNED_UNITS
        {
            get
            {
                if (PROGRESS_ITEMCurrent == null)
                    return 0;

                return PROGRESS_ITEMCurrent.EARNED_UNITS;
            }
        }

        public decimal PERIOD_EARNED_COSTS
        {
            get
            {
                if (PROGRESS_ITEMCurrent == null || BASELINE_ITEMJoinRATE == null ||
                    BASELINE_ITEMJoinRATE.RATE == null || BASELINE_ITEMJoinRATE.RATE.RATE1 == null)
                    return 0;

                return PROGRESS_ITEMCurrent.EARNED_UNITS * (decimal) BASELINE_ITEMJoinRATE.RATE.RATE1;
            }
        }


        private decimal? total_earned_percentage;

        public decimal TOTAL_EARNED_PERCENTAGE
        {
            get
            {
                if (total_earned_percentage == null)
                {
                    var totalUnits = BASELINE_ITEMJoinRATE.BASELINE_ITEM.TOTAL_HOURS;
                    if (totalUnits > 0)
                    {
                        var earnedUnits = TOTAL_EARNED_UNITS;
                        total_earned_percentage = totalUnits == 0 ? 0 : earnedUnits / totalUnits;
                    }
                    else
                    {
                        total_earned_percentage = 1;
                    }
                }

                return (decimal) total_earned_percentage;
            }
            set
            {
                var totalUnits = BASELINE_ITEMJoinRATE.BASELINE_ITEM.TOTAL_HOURS;
                if (totalUnits > 0)
                {
                    var earnedUnits = value * BASELINE_ITEMJoinRATE.BASELINE_ITEM.TOTAL_HOURS;
                    earnedUnits -= PastPROGRESS_ITEMS_UNITS;

                    if (PROGRESS_ITEMcurrent == null)
                        PROGRESS_ITEMcurrent = new PROGRESS_ITEM();

                    PROGRESS_ITEMcurrent.EARNED_UNITS = earnedUnits;
                }

                total_earned_percentage = value;
            }
        }

        public decimal TOTAL_EARNED_UNITS
        {
            get
            {
                if (PROGRESS_ITEMcurrent == null)
                    return PastPROGRESS_ITEMS_UNITS;

                return PROGRESS_ITEMcurrent.EARNED_UNITS + PastPROGRESS_ITEMS_UNITS;
            }
        }

        public decimal TOTAL_EARNED_COSTS
        {
            get
            {
                if (BASELINE_ITEMJoinRATE == null || BASELINE_ITEMJoinRATE.RATE == null)
                    return 0;

                return TOTAL_EARNED_UNITS * (decimal) BASELINE_ITEMJoinRATE.RATE.RATE1;
            }
        }
    }

    public abstract class ProgressReportable : ISupportProgressReporting
    {
        public DateTime ReportingDataDate { get; set; }

        private ProgressInfo summary_cumulativeoriginal;

        public ProgressInfo Summary_CumulativeOriginal
        {
            get
            {
                if (summary_cumulativeoriginal == null && Summary_CumulativeOriginalDataPoints != null && Summary_CumulativeOriginalDataPoints.Count > 0 && ReportingDataDate != null)
                    summary_cumulativeoriginal =
                        ISupportProgressReportingExtensions.FindDataPointByDate(Summary_CumulativeOriginalDataPoints,
                            ReportingDataDate.Date);
                return summary_cumulativeoriginal;
            }
        }

        private ProgressInfo summary_cumulativeplanned;

        public ProgressInfo Summary_CumulativePlanned
        {
            get
            {
                if (summary_cumulativeplanned == null && Summary_CumulativePlannedDataPoints != null && Summary_CumulativePlannedDataPoints.Count > 0 && ReportingDataDate != null)
                    summary_cumulativeplanned =
                        ISupportProgressReportingExtensions.FindDataPointByDate(Summary_CumulativePlannedDataPoints,
                            ReportingDataDate.Date);
                return summary_cumulativeplanned;
            }
        }

        private ProgressInfo summary_cumulativeearned;

        public ProgressInfo Summary_CumulativeEarned
        {
            get
            {
                if (summary_cumulativeearned == null && Summary_CumulativeEarnedDataPoints != null && Summary_CumulativeEarnedDataPoints.Count > 0 && ReportingDataDate != null)
                    summary_cumulativeearned =
                        ISupportProgressReportingExtensions.FindDataPointByDate(Summary_CumulativeEarnedDataPoints,
                            ReportingDataDate.Date);
                return summary_cumulativeearned;
            }
        }

        private ProgressInfo summary_cumulativeburned;

        public ProgressInfo Summary_CumulativeBurned
        {
            get
            {
                if (summary_cumulativeburned == null && Summary_CumulativeBurnedDataPoints != null && Summary_CumulativeBurnedDataPoints.Count > 0 && ReportingDataDate != null)
                    summary_cumulativeburned =
                        ISupportProgressReportingExtensions.FindDataPointByDate(Summary_CumulativeBurnedDataPoints,
                            ReportingDataDate.Date);
                return summary_cumulativeburned;
            }
        }

        private ProgressInfo summary_cumulativeactual;

        public ProgressInfo Summary_CumulativeActual
        {
            get
            {
                if (summary_cumulativeactual == null && Summary_CumulativeActualDataPoints != null && Summary_CumulativeActualDataPoints.Count > 0 && ReportingDataDate != null)
                    summary_cumulativeactual =
                        ISupportProgressReportingExtensions.FindDataPointByDate(Summary_CumulativeActualDataPoints,
                            ReportingDataDate.Date);
                return summary_cumulativeactual;
            }
        }

        private ProgressInfo summary_periodoriginal;

        public ProgressInfo Summary_PeriodOriginal
        {
            get
            {
                if (summary_periodoriginal == null && ReportingDataDate != null)
                    summary_periodoriginal =
                        ISupportProgressReportingExtensions.GeneratePeriodDataPointFromCumulative(
                            Summary_CumulativeOriginalDataPoints, ReportingDataDate.Date);
                return summary_periodoriginal;
            }
        }

        private ProgressInfo summary_periodplanned;

        public ProgressInfo Summary_PeriodPlanned
        {
            get
            {
                if (summary_periodplanned == null && Summary_CumulativePlannedDataPoints != null && Summary_CumulativePlannedDataPoints.Count > 0 && ReportingDataDate != null)
                    summary_periodplanned =
                        ISupportProgressReportingExtensions.GeneratePeriodDataPointFromCumulative(
                            Summary_CumulativePlannedDataPoints, ReportingDataDate.Date);
                return summary_periodplanned;
            }
        }

        private ProgressInfo summary_periodearned;

        public ProgressInfo Summary_PeriodEarned
        {
            get
            {
                if (summary_periodearned == null && Summary_CumulativeEarnedDataPoints != null && Summary_CumulativeEarnedDataPoints.Count > 0 && ReportingDataDate != null)
                    summary_periodearned =
                        ISupportProgressReportingExtensions.GeneratePeriodDataPointFromCumulative(
                            Summary_CumulativeEarnedDataPoints, ReportingDataDate.Date);
                return summary_periodearned;
            }
        }

        private ProgressInfo summary_periodburned;

        public ProgressInfo Summary_PeriodBurned
        {
            get
            {
                if (summary_periodburned == null && Summary_CumulativeBurnedDataPoints != null && Summary_CumulativeBurnedDataPoints.Count > 0 && ReportingDataDate != null)
                    summary_periodburned =
                        ISupportProgressReportingExtensions.GeneratePeriodDataPointFromCumulative(
                            Summary_CumulativeBurnedDataPoints, ReportingDataDate.Date);
                return summary_periodburned;
            }
        }

        private ProgressInfo summary_periodactual;

        public ProgressInfo Summary_PeriodActual
        {
            get
            {
                if (summary_periodactual == null && Summary_CumulativeActualDataPoints != null && Summary_CumulativeActualDataPoints.Count > 0 && ReportingDataDate != null)
                    summary_periodactual =
                        ISupportProgressReportingExtensions.GeneratePeriodDataPointFromCumulative(
                            Summary_CumulativeActualDataPoints, ReportingDataDate.Date);
                return summary_periodactual;
            }
        }

        /// <summary>
        /// Non cumulative variation points with date and baseline item original guid
        /// </summary>
        private ObservableCollection<VariationAdjustment> noncumulative_variationadjustments =
            new ObservableCollection<VariationAdjustment>();

        public ObservableCollection<VariationAdjustment> NonCumulative_VariationAdjustments
        {
            get { return noncumulative_variationadjustments; }
            set { noncumulative_variationadjustments = value; }
        }

        /// <summary>
        /// Summary of the individual original for all baselineitem in the project
        /// </summary>
        private ObservableCollection<ProgressInfo> noncumulative_originaldatapoints =
            new ObservableCollection<ProgressInfo>();

        public ObservableCollection<ProgressInfo> NonCumulative_OriginalDataPoints
        {
            get { return noncumulative_originaldatapoints; }
            set { noncumulative_originaldatapoints = value; }
        }

        /// <summary>
        /// Summary of the individual planned for all baselineitem in the project
        /// </summary>
        private ObservableCollection<ProgressInfo> noncumulative_planneddatapoints =
            new ObservableCollection<ProgressInfo>();

        public ObservableCollection<ProgressInfo> NonCumulative_PlannedDataPoints
        {
            get { return noncumulative_planneddatapoints; }
            set { noncumulative_planneddatapoints = value; }
        }

        /// <summary>
        /// Summary of the individual earned for all baselineitem in the project
        /// </summary>
        private ObservableCollection<ProgressInfo> noncumulative_earneddatapoints =
            new ObservableCollection<ProgressInfo>();

        public ObservableCollection<ProgressInfo> NonCumulative_EarnedDataPoints
        {
            get { return noncumulative_earneddatapoints; }
            set { noncumulative_earneddatapoints = value; }
        }

        /// <summary>
        /// Summary of the individual burned for all baselineitem in the project
        /// </summary>
        private ObservableCollection<ProgressInfo> noncumulative_burneddatapoints =
            new ObservableCollection<ProgressInfo>();

        public ObservableCollection<ProgressInfo> NonCumulative_BurnedDataPoints
        {
            get { return noncumulative_burneddatapoints; }
            set { noncumulative_burneddatapoints = value; }
        }

        /// <summary>
        /// Summary of the individual actual for all baselineitem in the project
        /// </summary>
        private ObservableCollection<ProgressInfo> noncumulative_actualdatapoints =
            new ObservableCollection<ProgressInfo>();

        public ObservableCollection<ProgressInfo> NonCumulative_ActualDataPoints
        {
            get { return noncumulative_actualdatapoints; }
            set { noncumulative_actualdatapoints = value; }
        }


        /// <summary>
        /// Summary of the burned for principal project
        /// </summary>
        private ObservableCollection<ProgressInfo> summary_burneddatapoints { get; set; }

        public ObservableCollection<ProgressInfo> Summary_CumulativeBurnedDataPoints
        {
            get { return summary_burneddatapoints; }
            set { summary_burneddatapoints = value; }
        }

        /// <summary>
        /// Summary of the burned by period for all baselineitem in the project
        /// </summary>
        private ObservableCollection<ProgressInfo> summary_periodburneddatapoints;

        public ObservableCollection<ProgressInfo> Summary_PeriodBurnedDataPoints
        {
            get
            {
                if (summary_periodburneddatapoints == null && summary_burneddatapoints != null && summary_burneddatapoints.Count > 0)
                    summary_periodburneddatapoints =
                        ISupportProgressReportingExtensions.ConvertCumulativeToPeriodDataPoint(summary_burneddatapoints);

                return summary_periodburneddatapoints;
            }
        }

        /// <summary>
        /// Summary of the actual by period for all baselineitem in the project
        /// </summary>
        private ObservableCollection<ProgressInfo> summary_periodactualdatapoints;

        public ObservableCollection<ProgressInfo> Summary_PeriodActualDataPoints
        {
            get
            {
                if (summary_periodactualdatapoints == null && summary_actualdatapoints != null && summary_actualdatapoints.Count > 0)
                    summary_periodactualdatapoints =
                        ISupportProgressReportingExtensions.ConvertCumulativeToPeriodDataPoint(summary_actualdatapoints);

                return summary_periodactualdatapoints;
            }
        }

        /// <summary>
        /// Summary of the earned by period for all baselineitem in the project
        /// </summary>
        private ObservableCollection<ProgressInfo> summary_periodearneddatapoints;

        public ObservableCollection<ProgressInfo> Summary_PeriodEarnedDataPoints
        {
            get
            {
                if (summary_periodearneddatapoints == null && summary_earneddatapoints != null && summary_earneddatapoints.Count > 0)
                    summary_periodearneddatapoints =
                        ISupportProgressReportingExtensions.ConvertCumulativeToPeriodDataPoint(summary_earneddatapoints);

                return summary_periodearneddatapoints;
            }
        }

        /// <summary>
        /// Summary of the original by period for all baselineitem in the project
        /// </summary>
        private ObservableCollection<ProgressInfo> summary_periodoriginaldatapoints;

        public ObservableCollection<ProgressInfo> Summary_PeriodOriginalDataPoints
        {
            get
            {
                if (summary_periodoriginaldatapoints == null && summary_originaldatapoints != null && summary_originaldatapoints.Count > 0)
                    summary_periodoriginaldatapoints =
                        ISupportProgressReportingExtensions.ConvertCumulativeToPeriodDataPoint(
                            summary_originaldatapoints);

                return summary_periodoriginaldatapoints;
            }
        }

        /// <summary>
        /// Summary of the planned by period for all baselineitem in the project
        /// </summary>
        private ObservableCollection<ProgressInfo> summary_periodplanneddatapoints;

        public ObservableCollection<ProgressInfo> Summary_PeriodPlannedDataPoints
        {
            get
            {
                if (summary_periodplanneddatapoints == null && summary_planneddatapoints != null && summary_planneddatapoints.Count > 0)
                    summary_periodplanneddatapoints =
                        ISupportProgressReportingExtensions.ConvertCumulativeToPeriodDataPoint(summary_planneddatapoints);

                return summary_periodplanneddatapoints;
            }
        }

        /// <summary>
        /// Summary of the variation adjustment units by date
        /// </summary>
        private ObservableCollection<VariationAdjustment> cumulative_variationadjustments { get; set; }

        public ObservableCollection<VariationAdjustment> Cumulative_VariationAdjustments
        {
            get { return cumulative_variationadjustments; }
            set { cumulative_variationadjustments = value; }
        }

        private ObservableCollection<ProgressInfo> summary_actualdatapoints { get; set; }

        public ObservableCollection<ProgressInfo> Summary_CumulativeActualDataPoints
        {
            get { return summary_actualdatapoints; }
            set { summary_actualdatapoints = value; }
        }


        /// <summary>
        /// Summary of the earned for all baselineitem in the project
        /// </summary>
        private ObservableCollection<ProgressInfo> summary_earneddatapoints { get; set; }

        public ObservableCollection<ProgressInfo> Summary_CumulativeEarnedDataPoints
        {
            get { return summary_earneddatapoints; }
            set { summary_earneddatapoints = value; }
        }

        /// <summary>
        /// Summary of the original for all baselineitem in the project
        /// </summary>
        private ObservableCollection<ProgressInfo> summary_originaldatapoints { get; set; }

        public ObservableCollection<ProgressInfo> Summary_CumulativeOriginalDataPoints
        {
            get { return summary_originaldatapoints; }
            set { summary_originaldatapoints = value; }
        }

        /// <summary>
        /// Summary of the planned for all baselineitem in the project
        /// </summary>
        private ObservableCollection<ProgressInfo> summary_planneddatapoints { get; set; }

        public ObservableCollection<ProgressInfo> Summary_CumulativePlannedDataPoints
        {
            get { return summary_planneddatapoints; }
            set { summary_planneddatapoints = value; }
        }

        /// <summary>
        /// Summary of the individual remaining based on original productivity in the project
        /// </summary>
        private ObservableCollection<ProgressInfo> noncumulative_remainingoriginaldatapoints =
            new ObservableCollection<ProgressInfo>();

        public ObservableCollection<ProgressInfo> NonCumulative_RemainingOriginalDataPoints
        {
            get { return noncumulative_remainingoriginaldatapoints; }
            set { noncumulative_remainingoriginaldatapoints = value; }
        }

        /// <summary>
        /// Summary of the individual remaining based on planned productivity in the project
        /// </summary>
        private ObservableCollection<ProgressInfo> noncumulative_remainingplanneddatapoints =
            new ObservableCollection<ProgressInfo>();

        public ObservableCollection<ProgressInfo> NonCumulative_RemainingPlannedDataPoints
        {
            get { return noncumulative_remainingplanneddatapoints; }
            set { noncumulative_remainingplanneddatapoints = value; }
        }

        /// <summary>
        /// Summary of the individual remaining based on current productivity in the project
        /// </summary>
        private ObservableCollection<ProgressInfo> noncumulative_remainingcurrentdatapoints =
            new ObservableCollection<ProgressInfo>();

        public ObservableCollection<ProgressInfo> NonCumulative_RemainingCurrentDataPoints
        {
            get { return noncumulative_remainingcurrentdatapoints; }
            set { noncumulative_remainingcurrentdatapoints = value; }
        }

        /// <summary>
        /// Summary of the individual remaining based on variation productivity in the project
        /// </summary>
        private ObservableCollection<ProgressInfo> noncumulative_remainingvariationdatapoints =
            new ObservableCollection<ProgressInfo>();

        public ObservableCollection<ProgressInfo> NonCumulative_RemainingVariationDataPoints
        {
            get { return noncumulative_remainingvariationdatapoints; }
            set { noncumulative_remainingvariationdatapoints = value; }
        }

        /// <summary>
        /// Summary of the datapoints based on original productivity in the project
        /// </summary>
        private ObservableCollection<ProgressInfo> summary_remainingoriginaldatapoints { get; set; }

        public ObservableCollection<ProgressInfo> Summary_CumulativeRemainingOriginalDataPoints
        {
            get { return summary_remainingoriginaldatapoints; }
            set { summary_remainingoriginaldatapoints = value; }
        }

        /// <summary>
        /// Summary of the datapoints based on planned productivity in the project
        /// </summary>
        private ObservableCollection<ProgressInfo> summary_remainingplanneddatapoints { get; set; }

        public ObservableCollection<ProgressInfo> Summary_CumulativeRemainingPlannedDataPoints
        {
            get { return summary_remainingplanneddatapoints; }
            set { summary_remainingplanneddatapoints = value; }
        }

        /// <summary>
        /// Summary of the datapoints based on remaining productivity in the project
        /// </summary>
        private ObservableCollection<ProgressInfo> summary_remainingcurrentdatapoints { get; set; }

        public ObservableCollection<ProgressInfo> Summary_CumulativeRemainingCurrentDataPoints
        {
            get { return summary_remainingcurrentdatapoints; }
            set { summary_remainingcurrentdatapoints = value; }
        }

        /// <summary>
        /// Summary of the datapoints based on variation productivity in the project
        /// </summary>
        private ObservableCollection<ProgressInfo> summary_remainingvariationdatapoints { get; set; }

        public ObservableCollection<ProgressInfo> Summary_CumulativeRemainingVariationDataPoints
        {
            get { return summary_remainingvariationdatapoints; }
            set { summary_remainingvariationdatapoints = value; }
        }

        /// <summary>
        /// Summary of the earned by period for all baselineitem in the project
        /// </summary>
        private ObservableCollection<ProgressInfo> summary_periodremainingvariationdatapoints;

        public ObservableCollection<ProgressInfo> Summary_PeriodRemainingVariationDataPoints
        {
            get
            {
                if (summary_periodremainingvariationdatapoints == null && ReportingDataDate != null)
                    summary_periodremainingvariationdatapoints =
                        ISupportProgressReportingExtensions.ConvertCumulativeToPeriodDataPoint(
                            summary_remainingvariationdatapoints, ReportingDataDate.Date);

                return summary_periodremainingvariationdatapoints;
            }
        }

        /// <summary>
        /// Summary of the original by period for all baselineitem in the project
        /// </summary>
        private ObservableCollection<ProgressInfo> summary_periodremainingoriginaldatapoints;

        public ObservableCollection<ProgressInfo> Summary_PeriodRemainingOriginalDataPoints
        {
            get
            {
                if (summary_periodremainingoriginaldatapoints == null && ReportingDataDate != null)
                    summary_periodremainingoriginaldatapoints =
                        ISupportProgressReportingExtensions.ConvertCumulativeToPeriodDataPoint(
                            summary_remainingoriginaldatapoints, ReportingDataDate.Date);

                return summary_periodremainingoriginaldatapoints;
            }
        }

        /// <summary>
        /// Summary of the planned by period for all baselineitem in the project
        /// </summary>
        private ObservableCollection<ProgressInfo> summary_periodremainingplanneddatapoints;

        public ObservableCollection<ProgressInfo> Summary_PeriodRemainingPlannedDataPoints
        {
            get
            {
                if (summary_periodremainingplanneddatapoints == null && ReportingDataDate != null)
                    summary_periodremainingplanneddatapoints =
                        ISupportProgressReportingExtensions.ConvertCumulativeToPeriodDataPoint(
                            summary_remainingplanneddatapoints, ReportingDataDate.Date);

                return summary_periodremainingplanneddatapoints;
            }
        }

        /// <summary>
        /// Summary of the current by period for all baselineitem in the project
        /// </summary>
        private ObservableCollection<ProgressInfo> summary_periodremainingcurrentdatapoints;

        public ObservableCollection<ProgressInfo> Summary_PeriodRemainingCurrentDataPoints
        {
            get
            {
                if (summary_periodremainingcurrentdatapoints == null && ReportingDataDate != null)
                    summary_periodremainingcurrentdatapoints =
                        ISupportProgressReportingExtensions.ConvertCumulativeToPeriodDataPoint(
                            summary_remainingcurrentdatapoints, ReportingDataDate.Date);

                return summary_periodremainingcurrentdatapoints;
            }
        }
    }
}