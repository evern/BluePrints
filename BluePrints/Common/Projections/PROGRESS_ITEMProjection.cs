using BluePrints.Common.Base;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.P6EntitiesDataModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.Common.Projections
{
    public class PROGRESS_ITEMProjection : BluePrintsProjectionBase<BASELINE_ITEMProjection>, IReportable
    {
        public DateTime ReportingDataDate { get; set; }

        #region Stats Parameters
        SingleObjectSummarizer StatSummarizer { get; set; }
        public ProgressStats Stats { get; set; }
        #endregion

        #region For User Dashboard and Deliverables
        public PROGRESS loadPROGRESS { get; set; }
        #endregion

        /// <summary>
        /// Refreshes current row when stats budgeted datapoints are set
        /// </summary>
        public void Update()
        {
            RaisePropertyChanged();
        }

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

        public DateTime? DueDate
        {
            get
            {
                if (Stats == null || Stats.Budgeted == null || Stats.Budgeted.CumulativeDataPoints == null || Stats.Budgeted.CumulativeDataPoints.Count == 0)
                    return null;

                return Stats.Budgeted.CumulativeDataPoints.Max(x => x.ProgressDate);
            }
        }

        public DateTime? ForecastDate
        {
            get
            {
                if (Stats == null || Stats.Budgeted == null || Stats.Remaining.CumulativeDataPoints == null || Stats.Remaining.CumulativeDataPoints.Count == 0)
                    return null;

                return Stats.Remaining.CumulativeDataPoints.Max(x => x.ProgressDate);
            }
        }

        public PROGRESS_ITEMProjection(DateTime reportingDataDate, TimeSpan reportInterval, DateTime firstAlignedDataDate, BASELINE_ITEMProjection baseline_itemProjection, PROJECT PROJECT, BASELINE LiveBASELINE, PROGRESS LivePROGRESS, IEnumerable<WORKPACK> WORKPACKS, IEnumerable<VariationAdjustment> projectVariationAdjustments, IP6EntitiesUnitOfWork P6UOW = null)
        {
            ReportingDataDate = reportingDataDate;
            Entity = baseline_itemProjection;

            List<VariationAdjustment> currentProgressItemAdjustments = projectVariationAdjustments.Where(x => x.DeliverableOriginalGuid == this.Entity.Entity.GUID_ORIGINAL).ToList();

            PartialStatsBuilder partialStatsBuilder = new PartialStatsBuilder(PROJECT, LiveBASELINE, LivePROGRESS, WORKPACKS, WORKPACKS.SelectMany(x => x.WORKPACK_ASSIGNMENT).ToList(), P6UOW);
            this.Stats = new ProgressStats(LivePROGRESS, this.Entity.Entity.ESTIMATED_HOURS, this.Entity.Entity.TOTAL_HOURS, this.Entity.ESTIMATED_COSTS, this.Entity.TOTAL_COSTS, projectVariationAdjustments.Where(x => x.DeliverableOriginalGuid == this.Entity.Entity.GUID_ORIGINAL).ToList());
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

        private IEnumerable<VARIATION_ITEM> VARIATION_ITEMS { get; set; }

        private List<PROGRESS_ITEM> progress_items;

        public List<PROGRESS_ITEM> PROGRESS_ITEMS
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
                                    y.GUID_ORIBASEITEM == Entity.Entity.GUID_ORIGINAL &&
                                    y.EARNED_DATE == ReportingDataDate).OrderBy(x => x.EARNED_UNITS).FirstOrDefault();
                    if (PROGRESS_ITEMSafterreportingdate == null)
                        PROGRESS_ITEMSafterreportingdate =
                            value.Where(
                                y =>
                                    y.GUID_ORIBASEITEM == Entity.Entity.GUID_ORIGINAL &&
                                    y.EARNED_DATE > ReportingDataDate).ToList();
                    if (PROGRESS_ITEMSbeforereportingdate == null)
                        PROGRESS_ITEMSbeforereportingdate =
                            value.Where(
                                y =>
                                    y.GUID_ORIBASEITEM == Entity.Entity.GUID_ORIGINAL &&
                                    y.EARNED_DATE < ReportingDataDate).ToList();
                    if (PROGRESS_ITEMSuptocurrentdate == null)
                        PROGRESS_ITEMSuptocurrentdate =
                            value.Where(
                                y =>
                                    y.GUID_ORIBASEITEM == Entity.Entity.GUID_ORIGINAL &&
                                    y.EARNED_DATE <= ReportingDataDate).ToList();

                    progress_items = value;
                }
            }
        }

        public IEnumerable<PROGRESS_ITEM> AllProgresses
        {
            get { return progress_items; }
        }

        private PROGRESS_ITEM progress_itemCurrent;

        public PROGRESS_ITEM PROGRESS_ITEMCurrent
        {
            get { return progress_itemCurrent; }
            set { progress_itemCurrent = value; }
        }

        private IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMSafterreportingdate;

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMSAfterReportingDate
        {
            get { return PROGRESS_ITEMSafterreportingdate; }
        }

        private IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMSbeforereportingdate;

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMSBeforeReportingDate
        {
            get { return PROGRESS_ITEMSbeforereportingdate; }
        }

        private IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMSuptocurrentdate;

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMSUpToCurrentDate
        {
            get { return PROGRESS_ITEMSuptocurrentdate; }
        }

        //xaml use only
        public decimal CurrentPROGRESS_ITEM_UNITS
        {
            get
            {
                if (Entity.Entity.BY_DURATION)
                    return BluePrintsConstants.DurationBasedDisplayUnits;

                decimal currentUnits = PROGRESS_ITEMCurrent == null ? 0 : PROGRESS_ITEMCurrent.EARNED_UNITS;
                return PastPROGRESS_ITEMS_UNITS + currentUnits - Entity.Entity.DC_HOURS;
            }
        }

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
            get
            {
                if (Entity == null || Entity.Entity == null)
                    return 0;

                return Entity.Entity.TOTAL_HOURS - TOTAL_EARNED_UNITS;
            }
        }

        public decimal MinPercentage
        {
            get
            {
                if (Entity == null || Entity.Entity == null || PROGRESS_ITEMSBeforeReportingDate == null)
                    return 0;

                if (Entity.Entity.BY_DURATION)
                    return PastPROGRESS_ITEMS_UNITS / BluePrintsConstants.DurationBasedTotalUnits;

                if (Entity.Entity.TOTAL_HOURS == 0)
                    return 0;

                return PastPROGRESS_ITEMS_UNITS / Entity.Entity.TOTAL_HOURS;
            }
        }

        public decimal MaxPercentage
        {
            get
            {
                if (Entity == null || Entity.Entity == null)
                    return 0;

                if (PROGRESS_ITEMSBeforeReportingDate == null)
                    return 1;

                if (Entity.Entity.BY_DURATION)
                    return (BluePrintsConstants.DurationBasedTotalUnits - FuturePROGRESS_ITEMS_UNITS) / BluePrintsConstants.DurationBasedTotalUnits;

                if (Entity.Entity.TOTAL_HOURS == 0)
                    return 1;

                return (Entity.Entity.TOTAL_HOURS - FuturePROGRESS_ITEMS_UNITS) / Entity.Entity.TOTAL_HOURS;
            }
        }

        public decimal BASELINE_PERCENTAGE
        {
            get
            {
                if (Entity == null || Entity.Entity == null)
                    return 0;

                if (Entity.Entity.BY_DURATION)
                    return TOTAL_EARNED_UNITS / BluePrintsConstants.DurationBasedTotalUnits;
                else if (Entity.Entity.ESTIMATED_HOURS == 0)
                    return 0;

                return TOTAL_EARNED_UNITS / Entity.Entity.ESTIMATED_HOURS;
            }
        }

        public decimal TOTAL_PERCENTAGE
        {
            get
            {
                if (Entity == null || Entity.Entity == null)
                    return 0;

                if (Entity.Entity.BY_DURATION)
                    return TOTAL_EARNED_UNITS / BluePrintsConstants.DurationBasedTotalUnits;
                else if (Entity.Entity.TOTAL_HOURS == 0)
                    return 0;

                return TOTAL_EARNED_UNITS / Entity.Entity.TOTAL_HOURS;
            }
        }

        //xaml use only
        public decimal PERIOD_EARNED_PERCENTAGE
        {
            get
            {
                if (Entity == null || Entity.Entity == null || PROGRESS_ITEMCurrent == null)
                    return 0;

                if (Entity.Entity.BY_DURATION)
                    return PROGRESS_ITEMCurrent.EARNED_UNITS / BluePrintsConstants.DurationBasedTotalUnits;
                else if (Entity.Entity.TOTAL_HOURS == 0)
                    return 0;

                return PROGRESS_ITEMCurrent.EARNED_UNITS / Entity.Entity.TOTAL_HOURS;
            }
        }

        //xaml use only
        public decimal DisplayPeriodEarnedUnits
        {
            get
            {
                if (Entity != null && Entity.Entity != null && Entity.Entity.BY_DURATION)
                    return BluePrintsConstants.DurationBasedDisplayUnits;

                if (PROGRESS_ITEMCurrent == null)
                    return 0;

                return PROGRESS_ITEMCurrent.EARNED_UNITS;
            }
        }

        //xaml use only
        public decimal DisplayPeriodEarnedCosts
        {
            get
            {
                if (Entity != null && Entity.Entity != null && Entity.Entity.BY_DURATION)
                    return BluePrintsConstants.DurationBasedDisplayUnits;

                if (PROGRESS_ITEMCurrent == null || Entity == null || Entity.RATE == null || Entity.RATE.RATE1 == null)
                    return 0;

                return PROGRESS_ITEMCurrent.EARNED_UNITS * (decimal)Entity.RATE.RATE1;
            }
        }


        private decimal? total_earned_percentage;

        public decimal TOTAL_EARNED_PERCENTAGE
        {
            get
            {
                if (total_earned_percentage == null)
                {
                    if (Entity == null || Entity.Entity == null)
                        return 0;

                    var totalUnits = Entity.Entity.TOTAL_HOURS;
                    if (Entity.Entity.BY_DURATION)
                    {
                        var earnedUnits = TOTAL_EARNED_UNITS;
                        total_earned_percentage = earnedUnits / BluePrintsConstants.DurationBasedTotalUnits;
                    }
                    else if(totalUnits > 0)
                    {
                        var earnedUnits = TOTAL_EARNED_UNITS;
                        total_earned_percentage = earnedUnits / totalUnits;
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
                if (Entity == null || Entity.Entity == null)
                    return;

                var totalUnits = Entity.Entity.TOTAL_HOURS;
                if (totalUnits > 0)
                {
                    decimal earnedUnits = value * Entity.Entity.TOTAL_HOURS;
                    earnedUnits -= PastPROGRESS_ITEMS_UNITS;

                    if (PROGRESS_ITEMCurrent == null)
                        PROGRESS_ITEMCurrent = new PROGRESS_ITEM();

                    PROGRESS_ITEMCurrent.EARNED_UNITS = earnedUnits;
                }
                else if(Entity.Entity.BY_DURATION)
                {
                    decimal earnedUnits = value * BluePrintsConstants.DurationBasedTotalUnits;
                    earnedUnits -= PastPROGRESS_ITEMS_UNITS;

                    if (PROGRESS_ITEMCurrent == null)
                        PROGRESS_ITEMCurrent = new PROGRESS_ITEM();

                    PROGRESS_ITEMCurrent.EARNED_UNITS = earnedUnits;
                }

                total_earned_percentage = value;
            }
        }

        //xaml use only
        public decimal DisplayTotalEarnedUnits
        {
            get
            {
                if (Entity != null && Entity.Entity != null && Entity.Entity.BY_DURATION)
                    return BluePrintsConstants.DurationBasedDisplayUnits;
                
                if (PROGRESS_ITEMCurrent == null)
                    return PastPROGRESS_ITEMS_UNITS;

                return PROGRESS_ITEMCurrent.EARNED_UNITS + PastPROGRESS_ITEMS_UNITS;
            }
        }

        //code use only
        public decimal TOTAL_EARNED_UNITS
        {
            get
            {
                if (PROGRESS_ITEMCurrent == null)
                    return PastPROGRESS_ITEMS_UNITS;

                return PROGRESS_ITEMCurrent.EARNED_UNITS + PastPROGRESS_ITEMS_UNITS;
            }
        }

        //xaml use only
        public decimal DisplayTotalEarnedCost
        {
            get
            {
                if (Entity != null && Entity.Entity != null && Entity.Entity.BY_DURATION)
                    return BluePrintsConstants.DurationBasedDisplayUnits;

                if (Entity == null || Entity.RATE == null)
                    return 0;

                return TOTAL_EARNED_UNITS * (decimal)Entity.RATE.RATE1;
            }
        }

        public decimal MAX_PERCENTAGE_WITH_DELIVERABLE_STATUS_LIMIT
        {
            get
            {
                if (Entity == null || Entity.Entity.DELIVERABLES_STATUS == null)
                {
                    return MaxPercentage;
                }
                else
                {
                    if (MaxPercentage < Entity.Entity.DELIVERABLES_STATUS.MAX_PERCENTAGE)
                        return MaxPercentage;
                    else
                        return Entity.Entity.DELIVERABLES_STATUS.MAX_PERCENTAGE;
                }
            }
        }

        public IDeliverable Deliverable
        {
            get { return Entity.Entity; }
        }

        public decimal ItemRate => Entity.ItemRate;

        public decimal EstimatedCosts => Entity.EstimatedCosts;

        public decimal TotalCosts => Entity.TotalCosts;

        public string ReportableItem_Name => Entity.ReportableItem_Name;

        public string Commodity_Code => Entity.Commodity_Code;

        public string Stock_Code => Entity.Stock_Code;

        public Guid? Workpack_Guid => Entity.Workpack_Guid;

        public decimal TotalHoursIncludeByDuration => Entity.TotalHoursIncludeByDuration;

        public decimal EstimatedHours => Entity.EstimatedHours;

        public decimal TotalHours => Entity.TotalHours;

        public Guid OriginalEntityKey
        {
            get { return Entity.OriginalEntityKey; }
            set { Entity.OriginalEntityKey = value; }
        }

        public Guid? Area_Guid { get => Entity.Entity.GUID_AREA; }
        public Guid? SubArea_Guid { get => Entity.Entity.GUID_SUBAREA; }

        public decimal Estimated_Quantity => throw new NotImplementedException();

        public decimal Total_Quantity => throw new NotImplementedException();

        public string UOM => throw new NotImplementedException();
    }

    public static class PROGRESS_ITEMProjectionQueries
    {
        public static IQueryable<PROGRESS_ITEMProjection> GetUserDeliverables(IQueryable<BASELINE_ITEM> query, Func<IEnumerable<DELIVERABLES_STATUS>> getDELIVERABLES_STATUSESFunc, USER user, bool buildStats = true)
        {
            IP6EntitiesUnitOfWork p6UnitOfWork = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            IQueryable<BASELINE_ITEM> projectUSER_DELIVERABLES = query.Where(x => x.GUID_USER == user.GUID && x.BASELINE.STATUS == BaselineStatus.Live && x.BASELINE.PROJECT.STATUS == ProjectStatus.Active);
            List<PROGRESS_ITEMProjection> USERDeliverables = new List<PROGRESS_ITEMProjection>();

            var deliverablesGroupByProject = projectUSER_DELIVERABLES.GroupBy(x => x.BASELINE.PROJECT).Select(group => new { Project = group.Key, Deliverables = group.ToList() });
            foreach(var deliverableGroupByProject in deliverablesGroupByProject)
            {
                PROJECT currentDeliverablePROJECT = deliverableGroupByProject.Project;

                PROGRESS livePROGRESS = currentDeliverablePROJECT.PROGRESS.FirstOrDefault(x => x.STATUS == ProgressStatus.Live);
                if (livePROGRESS == null)
                    continue;

                IEnumerable<BASELINE_ITEM> currentProjectDeliverables = deliverableGroupByProject.Deliverables;
                BASELINE liveBASELINE = currentDeliverablePROJECT.BASELINE.FirstOrDefault(x => x.STATUS == BaselineStatus.Live);
                IEnumerable<WORKPACK> projectWORKPACK = currentDeliverablePROJECT.WORKPACK;
                IEnumerable<VARIATION> approvedVARIATION = currentDeliverablePROJECT.VARIATION.Where(x => x.APPROVED != null);
                IEnumerable<PROGRESS_ITEM> livePROGRESS_ITEMS = livePROGRESS.PROGRESS_ITEM;
                IEnumerable<RATE> projectRATES = currentDeliverablePROJECT.RATE;
                Func<IEnumerable<DELIVERABLES_STATUS>> getProjectDELIVERABLE_STATUSFunc = () => getDELIVERABLES_STATUSESFunc().Where(x => x.GUID_PROJECT == currentDeliverablePROJECT.GUID);

                List<PROGRESS_ITEMProjection> activePROGRESS_ITEMS;
                if (buildStats)
                {
                    activePROGRESS_ITEMS = PROGRESS_ITEMProjectionQueries.JoinRATESAndPROGRESS_ITEMSOnBASELINE_ITEMSWithStats(currentProjectDeliverables.AsQueryable(), () => currentDeliverablePROJECT, () => livePROGRESS, () => liveBASELINE, () => projectWORKPACK, () => livePROGRESS_ITEMS, () => projectRATES, getProjectDELIVERABLE_STATUSFunc, () => approvedVARIATION, p6UnitOfWork).ToList();
                    activePROGRESS_ITEMS.ForEach(x => x.BuildStats());
                }
                else
                    activePROGRESS_ITEMS = PROGRESS_ITEMProjectionQueries.JoinRATESAndPROGRESS_ITEMSOnBASELINE_ITEMS(currentProjectDeliverables.AsQueryable(), () => livePROGRESS, () => liveBASELINE, () => livePROGRESS_ITEMS, () => projectRATES, getProjectDELIVERABLE_STATUSFunc).ToList();

                USERDeliverables.AddRange(activePROGRESS_ITEMS);
            }

            return USERDeliverables.AsQueryable();
        }

        public static IQueryable<PROGRESS_ITEMProjection> JoinRATESAndPROGRESS_ITEMSOnBASELINE_ITEMS(
            IQueryable<BASELINE_ITEM> BASELINE_ITEMS, Func<PROGRESS> getPROGRESSFunc, Func<BASELINE> getBASELINEFunc,
            Func<IEnumerable<PROGRESS_ITEM>> getPROGRESS_ITEMSFunc, Func<IEnumerable<RATE>> getRATESFunc,
            Func<IEnumerable<DELIVERABLES_STATUS>> getDELIVERABLES_STATUSESFunc,
            Func<IEnumerable<AREA>> getSUBAREAFunc = null,
            bool isBASELINEQueryProcessed = false)
        {
            var PROGRESS = getPROGRESSFunc();

            IEnumerable<PROGRESS_ITEM> LoadPROGRESS_ITEMS;
            if (PROGRESS == null)
                LoadPROGRESS_ITEMS = new List<PROGRESS_ITEM>();
            else
                LoadPROGRESS_ITEMS = getPROGRESS_ITEMSFunc();

            IQueryable<BASELINE_ITEMProjection> BASELINE_ITEMProjections;
            if (PROGRESS == null)
                BASELINE_ITEMProjections = new List<BASELINE_ITEMProjection>().AsQueryable();
            else
                BASELINE_ITEMProjections = BASELINE_ITEMProjectionQueries.BASELINE_ITEMProjectionQuery(BASELINE_ITEMS,
                    getBASELINEFunc, getRATESFunc, getDELIVERABLES_STATUSESFunc, getSUBAREAFunc, isBASELINEQueryProcessed);

            var reportingDate = PROGRESS == null ? new DateTime() : PROGRESS.DATA_DATE;

            return
                BASELINE_ITEMProjections.ToArray().Select(
                        x =>
                            new PROGRESS_ITEMProjection(reportingDate)
                            {
                                EntityKey = x.EntityKey,
                                Entity = x,
                                loadPROGRESS = PROGRESS,
                                PROGRESS_ITEMS = LoadPROGRESS_ITEMS.Where(y => y.GUID_ORIBASEITEM == x.Entity.OriginalEntityKey).ToList(), 
                            }).AsQueryable();
        }

        public static IQueryable<PROGRESS_ITEMProjection> JoinRATESAndPROGRESS_ITEMSOnBASELINE_ITEMSWithStats(
            IQueryable<BASELINE_ITEM> BASELINE_ITEMS, Func<PROJECT> getPROJECTFunc, Func<PROGRESS> getPROGRESSFunc, Func<BASELINE> getBASELINEFunc,
            Func<IEnumerable<WORKPACK>> getWORKPACKFunc, 
            Func<IEnumerable<PROGRESS_ITEM>> getPROGRESS_ITEMSFunc, Func<IEnumerable<RATE>> getRATESFunc,
            Func<IEnumerable<DELIVERABLES_STATUS>> getDELIVERABLES_STATUSESFunc, Func<IEnumerable<VARIATION>> getVARIATIONSFunc, IP6EntitiesUnitOfWork p6UOW,
            Func<IEnumerable<AREA>> getSUBAREAFunc = null,
            bool isBASELINEQueryProcessed = false, bool buildBudgetedOnly = false)
        {
            var PROGRESS = getPROGRESSFunc();

            IEnumerable<PROGRESS_ITEM> LoadPROGRESS_ITEMS;
            if (PROGRESS == null)
                LoadPROGRESS_ITEMS = new List<PROGRESS_ITEM>();
            else
                LoadPROGRESS_ITEMS = getPROGRESS_ITEMSFunc();

            IQueryable<BASELINE_ITEMProjection> BASELINE_ITEMProjections;
            if (PROGRESS == null)
                BASELINE_ITEMProjections = new List<BASELINE_ITEMProjection>().AsQueryable();
            else
                BASELINE_ITEMProjections = BASELINE_ITEMProjectionQueries.BASELINE_ITEMProjectionQuery(BASELINE_ITEMS,
                    getBASELINEFunc, getRATESFunc, getDELIVERABLES_STATUSESFunc, getSUBAREAFunc, isBASELINEQueryProcessed);

            var reportingDate = PROGRESS == null ? new DateTime() : PROGRESS.DATA_DATE;

            PROJECT project = getPROJECTFunc();
            IEnumerable<VARIATION> projectVARIATIONS = getVARIATIONSFunc();
            TimeSpan reportInterval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(PROGRESS);
            DateTime firstAlignedDataDate = ChronologicalHelpers.GenerateFirstAlignedDataDate(PROGRESS);
            List<VariationAdjustment> projectVariationAdjustments = ProjectionHelpers.BuildProjectVariationAdjustments(projectVARIATIONS.AsQueryable(), BASELINE_ITEMProjections);

            List<PROGRESS_ITEMProjection> progress_items = BASELINE_ITEMProjections
                .Select(
                        x =>
                        new PROGRESS_ITEMProjection(reportingDate, reportInterval, firstAlignedDataDate, x, project, getBASELINEFunc(), getPROGRESSFunc(), getWORKPACKFunc(), projectVariationAdjustments, p6UOW)
                        {
                            EntityKey = x.EntityKey,
                            loadPROGRESS = PROGRESS, 
                            PROGRESS_ITEMS = LoadPROGRESS_ITEMS.Where(y => y.GUID_ORIBASEITEM == x.Entity.OriginalEntityKey).ToList()
                        }).ToList();

            foreach(PROGRESS_ITEMProjection progress_item in progress_items)
            {
                if(progress_item.Stats == null)
                {
                    if (buildBudgetedOnly)
                        progress_item.BuildBudgetedStats();
                    else
                        progress_item.BuildStats();
                }
            }

            return progress_items.AsQueryable();
        }
    }
}