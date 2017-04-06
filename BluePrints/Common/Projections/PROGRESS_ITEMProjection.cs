using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.DataModel;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.Data.Attributes;
using BluePrints.P6EntitiesDataModel;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    public class PROGRESS_ITEMProjection : IHaveGUID, IHaveStats
    {
        readonly DateTime ReportingDataDate;

        SingleObjectSummarizer StatSummarizer { get; set; }
        public ProgressStats Stats { get; set; }
        #region Runtime Parameters
        public decimal WorkpackAssignmentStartUnit { get; private set; }
        #endregion
        public void SetWorkpackAssignmentStartUnit(decimal workpackAssignmentStartUnit)
        {
            WorkpackAssignmentStartUnit = workpackAssignmentStartUnit;
        }
        public Guid GUID { get; set; }
        public PROGRESS_ITEMProjection()
        {
        }

        public PROGRESS_ITEMProjection(DateTime reportingDataDate)
        {
            ReportingDataDate = reportingDataDate;
        }

        public decimal SchedulePercentage
        {
            get
            {
                if (Stats == null || Stats.Budgeted == null || Stats.Budgeted.CurrentPeriodCumulativeDataPoint == null)
                    return 0;

                return Stats.Budgeted.CurrentPeriodCumulativeDataPoint.UnitsPercentage;
            }
        }

        public PROGRESS_ITEMProjection(DateTime reportingDataDate, TimeSpan reportInterval, DateTime firstAlignedDataDate, BASELINE_ITEMProjection baseline_itemProjection, PROJECT PROJECT, BASELINE LiveBASELINE, PROGRESS LivePROGRESS, IEnumerable<WORKPACK> WORKPACKS, IEnumerable<VariationAdjustment> projectVariationAdjustments, IP6EntitiesUnitOfWork P6UOW = null)
        {
            ReportingDataDate = reportingDataDate;
            BASELINE_ITEMJoinRATE = baseline_itemProjection;

            List<VariationAdjustment> currentProgressItemAdjustments = projectVariationAdjustments.Where(x => x.DeliverableOriginalGuid == this.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_ORIGINAL).ToList();

            PartialStatsBuilder partialStatsBuilder = new PartialStatsBuilder(PROJECT, LiveBASELINE, LivePROGRESS, WORKPACKS, WORKPACKS.SelectMany(x => x.WORKPACK_ASSIGNMENT).ToList(), P6UOW);
            this.Stats = new ProgressStats(LivePROGRESS, this.BASELINE_ITEMJoinRATE.BASELINE_ITEM.ESTIMATED_HOURS, this.BASELINE_ITEMJoinRATE.BASELINE_ITEM.TOTAL_HOURS, this.BASELINE_ITEMJoinRATE.ESTIMATED_COSTS, this.BASELINE_ITEMJoinRATE.TOTAL_COSTS, projectVariationAdjustments.Where(x => x.DeliverableOriginalGuid == this.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_ORIGINAL).ToList());
            StatSummarizer = new SingleObjectSummarizer(this, partialStatsBuilder);
        }

        public void BuildStats()
        {
            if (StatSummarizer == null || Stats == null)
                return;

            StatSummarizer.Build(false);
        }

        public void BuildBudgetedStats()
        {
            if (StatSummarizer == null || Stats == null)
                return;

            StatSummarizer.BuildBudgetedOnly();
        }

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
                    PROGRESS_ITEMCurrent = new PROGRESS_ITEM();
                    PROGRESS_ITEMSafterreportingdate = new List<PROGRESS_ITEM>();
                    PROGRESS_ITEMSbeforereportingdate = new List<PROGRESS_ITEM>();
                    PROGRESS_ITEMSuptocurrentdate = new List<PROGRESS_ITEM>();
                }
                else
                {
                    if (PROGRESS_ITEMCurrent == null)
                        PROGRESS_ITEMCurrent =
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

        private PROGRESS_ITEM progress_itemCurrent;

        public PROGRESS_ITEM PROGRESS_ITEMCurrent
        {
            get { return progress_itemCurrent; }
            set { progress_itemCurrent = value; }
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

        public bool isEarnedDataPointsFromP6 { get; set; }
        public bool isPlannedDataPointsFromP6 { get; set; }
        public bool isRemainingDataPointsFromP6 { get; set; }

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

                return (decimal)pastPROGRESS_ITEMS_UNITS;
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

                return (decimal)futurePROGRESS_ITEMS_UNITS;
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
                    BASELINE_ITEMJoinRATE.BASELINE_ITEM.TOTAL_HOURS == 0)
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

                return PROGRESS_ITEMCurrent.EARNED_UNITS * (decimal)BASELINE_ITEMJoinRATE.RATE.RATE1;
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

                return (decimal)total_earned_percentage;
            }
            set
            {
                var totalUnits = BASELINE_ITEMJoinRATE.BASELINE_ITEM.TOTAL_HOURS;
                if (totalUnits > 0)
                {
                    var earnedUnits = value * BASELINE_ITEMJoinRATE.BASELINE_ITEM.TOTAL_HOURS;
                    earnedUnits -= PastPROGRESS_ITEMS_UNITS;

                    if (PROGRESS_ITEMCurrent == null)
                        PROGRESS_ITEMCurrent = new PROGRESS_ITEM();

                    PROGRESS_ITEMCurrent.EARNED_UNITS = earnedUnits;
                }

                total_earned_percentage = value;
            }
        }

        public decimal TOTAL_EARNED_UNITS
        {
            get
            {
                if (PROGRESS_ITEMCurrent == null)
                    return PastPROGRESS_ITEMS_UNITS;

                return PROGRESS_ITEMCurrent.EARNED_UNITS + PastPROGRESS_ITEMS_UNITS;
            }
        }

        public decimal TOTAL_EARNED_COSTS
        {
            get
            {
                if (BASELINE_ITEMJoinRATE == null || BASELINE_ITEMJoinRATE.RATE == null)
                    return 0;

                return TOTAL_EARNED_UNITS * (decimal)BASELINE_ITEMJoinRATE.RATE.RATE1;
            }
        }

        public decimal MAX_PERCENTAGE_WITH_DELIVERABLE_STATUS_LIMIT
        {
            get
            {
                if (BASELINE_ITEMJoinRATE.DELIVERABLE_STATUS == null)
                {
                    return MaxPercentage;
                }
                else
                {
                    if (MaxPercentage < BASELINE_ITEMJoinRATE.DELIVERABLE_STATUS.MAX_PERCENTAGE)
                        return MaxPercentage;
                    else
                        return BASELINE_ITEMJoinRATE.DELIVERABLE_STATUS.MAX_PERCENTAGE;
                }
            }
        }

    }

    public static class PROGRESS_ITEMProjectionQueries
    {
        public static IQueryable<PROGRESS_ITEMProjection> JoinRATESAndPROGRESS_ITEMSOnBASELINE_ITEMS(
            IQueryable<BASELINE_ITEM> BASELINE_ITEMS, Func<PROGRESS> getPROGRESSFunc, Func<BASELINE> getBASELINEFunc,
            Func<IEnumerable<PROGRESS_ITEM>> getPROGRESS_ITEMSFunc, Func<IEnumerable<RATE>> getRATESFunc,
            Func<IEnumerable<DELIVERABLES_STATUS>> getDELIVERABLES_STATUSESFunc,
            bool isBASELINEQueryProcessed = false)
        {
            var PROGRESS = getPROGRESSFunc();

            IEnumerable<PROGRESS_ITEM> LoadPROGRESS_ITEMS;
            if (PROGRESS == null)
                LoadPROGRESS_ITEMS = new List<PROGRESS_ITEM>();
            else
                LoadPROGRESS_ITEMS = getPROGRESS_ITEMSFunc();

            IQueryable<BASELINE_ITEMProjection> BASELINE_ITEMJoinRATES;
            if (PROGRESS == null)
                BASELINE_ITEMJoinRATES = new List<BASELINE_ITEMProjection>().AsQueryable();
            else
                BASELINE_ITEMJoinRATES = BASELINE_ITEMProjectionQueries.JoinRATESOnBASELINE_ITEMS(BASELINE_ITEMS,
                    getBASELINEFunc, getRATESFunc, getDELIVERABLES_STATUSESFunc, isBASELINEQueryProcessed);

            var reportingDate = PROGRESS == null ? new DateTime() : PROGRESS.DATA_DATE;

            return
                BASELINE_ITEMJoinRATES.ToArray().Select(
                        x =>
                            new PROGRESS_ITEMProjection(reportingDate)
                            {
                                GUID = x.GUID,
                                BASELINE_ITEMJoinRATE = x, 
                                PROGRESS_ITEMS = LoadPROGRESS_ITEMS.Where(y => y.GUID_ORIBASEITEM == x.BASELINE_ITEM.GUID_ORIGINAL)
                            }).AsQueryable();
        }

        public static IQueryable<PROGRESS_ITEMProjection> JoinRATESAndPROGRESS_ITEMSOnBASELINE_ITEMSWithStats(
            IQueryable<BASELINE_ITEM> BASELINE_ITEMS, Func<PROJECT> getPROJECTFunc, Func<PROGRESS> getPROGRESSFunc, Func<BASELINE> getBASELINEFunc,
            Func<IEnumerable<WORKPACK>> getWORKPACKFunc, 
            Func<IEnumerable<PROGRESS_ITEM>> getPROGRESS_ITEMSFunc, Func<IEnumerable<RATE>> getRATESFunc,
            Func<IEnumerable<DELIVERABLES_STATUS>> getDELIVERABLES_STATUSESFunc, Func<IEnumerable<VARIATION>> getVARIATIONSFunc, IP6EntitiesUnitOfWork p6UOW,
            bool isBASELINEQueryProcessed = false)
        {
            var PROGRESS = getPROGRESSFunc();

            IEnumerable<PROGRESS_ITEM> LoadPROGRESS_ITEMS;
            if (PROGRESS == null)
                LoadPROGRESS_ITEMS = new List<PROGRESS_ITEM>();
            else
                LoadPROGRESS_ITEMS = getPROGRESS_ITEMSFunc();

            IQueryable<BASELINE_ITEMProjection> BASELINE_ITEMJoinRATES;
            if (PROGRESS == null)
                BASELINE_ITEMJoinRATES = new List<BASELINE_ITEMProjection>().AsQueryable();
            else
                BASELINE_ITEMJoinRATES = BASELINE_ITEMProjectionQueries.JoinRATESOnBASELINE_ITEMS(BASELINE_ITEMS,
                    getBASELINEFunc, getRATESFunc, getDELIVERABLES_STATUSESFunc, isBASELINEQueryProcessed);

            var reportingDate = PROGRESS == null ? new DateTime() : PROGRESS.DATA_DATE;

            PROJECT project = getPROJECTFunc();
            IEnumerable<VARIATION> projectVARIATIONS = getVARIATIONSFunc();
            TimeSpan reportInterval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(PROGRESS);
            DateTime firstAlignedDataDate = ChronologicalHelpers.GenerateFirstAlignedDataDate(PROGRESS);
            List<VariationAdjustment> projectVariationAdjustments = ProjectionHelpers.BuildProjectVariationAdjustments(projectVARIATIONS.AsQueryable(), BASELINE_ITEMJoinRATES);

            return
                BASELINE_ITEMJoinRATES.ToArray().Select(
                        x =>
                            new PROGRESS_ITEMProjection(reportingDate, reportInterval, firstAlignedDataDate, x, project, getBASELINEFunc(), getPROGRESSFunc(), getWORKPACKFunc(), projectVariationAdjustments, p6UOW)
                            {
                                GUID = x.GUID,
                                PROGRESS_ITEMS = LoadPROGRESS_ITEMS.Where(y => y.GUID_ORIBASEITEM == x.BASELINE_ITEM.GUID_ORIGINAL)
                            }).AsQueryable();
        }
    }
}