using BaseModel.Misc;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.P6EntitiesDataModel;
using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.ViewModel.Reporting
{
    public class Commodity_CodeProgress : BluePrintsProgressableByQuantityProjectionBase<COMMODITY_CODEProjection>, IQuantityReportableGroup
    {
        public IEnumerable<IQuantityReportable> Deliverables { get; set; }

        public override PROGRESS_ITEM PROGRESS_ITEM_Current
        {
            get
            {
                PROGRESS_ITEM newPROGRESS_ITEM = new PROGRESS_ITEM();
                decimal totalCurrentUnits = Deliverables.Where(x => (bool)x.Track).Where(x => x.PROGRESS_ITEM_Current != null).Sum(x => x.PROGRESS_ITEM_Current.EARNED_UNITS);
                newPROGRESS_ITEM.EARNED_UNITS = totalCurrentUnits;
                newPROGRESS_ITEM.EARNED_DATE = ReportingDataDate;
                return newPROGRESS_ITEM;
            }
        }

        public override IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_BeforeDataDate
        {
            get
            {
                return Deliverables.Where(x => (bool)x.Track).SelectMany(x => x.PROGRESS_ITEM_BeforeDataDate);
            }
        }

        public override IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_AfterDataDate
        {
            get
            {
                return Deliverables.Where(x => (bool)x.Track).SelectMany(x => x.PROGRESS_ITEM_AfterDataDate);
            }
        }

        public override IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_UpToCurrentDataDate
        {
            get
            {
                return Deliverables.Where(x => (bool)x.Track).SelectMany(x => x.PROGRESS_ITEM_UpToCurrentDataDate);
            }
        }

        public override List<PROGRESS_ITEM> PROGRESS_ITEMS
        {
            get { return Deliverables.SelectMany(x => x.PROGRESS_ITEMS).ToList(); }
        }

        public override decimal GetCurrentPeriodPercentage(decimal newTotalQuantity)
        {
            return base.GetCurrentPeriodPercentage(newTotalQuantity);
        }
    }

    public class Estimation_Direct_ItemProgress : BluePrintsProgressableByQuantityProjectionBase<ESTIMATION_DIRECT_ITEMProjection>
    {

    }

    public class Baseline_ItemProgress : BluePrintsProgressableProjectionBase<BASELINE_ITEMProjection>
    {
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
    }

    public abstract class BluePrintsProgressableByQuantityProjectionBase<TEntity> : BluePrintsProgressableProjectionBase<TEntity>, IQuantityReportable
        where TEntity : class, IDeliverable, IHaveCosts, IHaveQuantity, new()
    {
        public decimal QuantityPerHour
        {
            get
            {
                if (TotalUnits == 0)
                    return 0;

                return Total_Quantity / TotalUnits;
            }
        }

        public decimal TotalPercentage
        {
            get
            {
                if (PROGRESS_ITEM_UpToCurrentDataDate.Count() == 0)
                    return 0;

                return PROGRESS_ITEM_UpToCurrentDataDate.Sum(x => x.EARNED_UNITS) / Entity.TotalUnits;
            }
        }

        public decimal CurrentTotalInstalledQuantity
        {
            get
            {
                return PROGRESS_ITEM_CurrentUnits * QuantityPerHour;
            }
        }

        public decimal PastInstalledQuantity
        {
            get
            {
                if (PROGRESS_ITEM_BeforeDataDate.Count() == 0 || QuantityPerHour == 0)
                    return 0;

                return PROGRESS_ITEM_BeforeDataDate.Sum(x => x.EARNED_UNITS) * QuantityPerHour;
            }
        }

        public decimal Estimated_Quantity => Entity.Estimated_Quantity;

        public decimal Total_Quantity => Entity.Total_Quantity;

        public string UOM => Entity.UOM;

        public bool? Track
        {
            get
            {
                ICanTrack trackableEntity = Entity as ICanTrack;
                if (trackableEntity != null)
                    return trackableEntity.Track;

                return null;
            }
        }

        public virtual decimal GetCurrentPeriodPercentage(decimal newTotalQuantity)
        {
            if (Total_Quantity == 0)
                return 0;

            return newTotalQuantity / Total_Quantity;
        }

        public decimal GetCurrentPeriodHours(decimal currentPeriodPercentage)
        {
            return currentPeriodPercentage * TotalUnits;
        }
    }

    public abstract class BluePrintsProgressableProjectionBase<TEntity> : BluePrintsProjectionBase<TEntity>, IReportableStats, ICanSetProgresses
        where TEntity : class, IDeliverable, IHaveCosts, new()
    {
        #region Stats Parameters
        readonly SingleObjectSummarizer statsSummarizer;
        public SingleObjectSummarizer StatSummarizer => statsSummarizer;
        public ProgressStats Stats { get; set; }

        public BluePrintsProgressableProjectionBase()
        {
            //Initialization without stats
        }

        public BluePrintsProgressableProjectionBase(PROJECT PROJECT, PROGRESS Live_PROGRESS, IEnumerable<VariationAdjustment> variation_adjustments)
        {
            SetReportingDataDate(Live_PROGRESS.DATA_DATE);
            ISortableDeliverableProjection deliverable = Entity as ISortableDeliverableProjection;
            if(deliverable != null)
            {
                List<VariationAdjustment> currentProgressItemAdjustments = variation_adjustments.Where(x => x.DeliverableOriginalGuid == deliverable.OriginalEntityKey).ToList();

                PartialStatsBuilder partialStatsBuilder = new PartialStatsBuilder(PROJECT.CURRENCYCONVERSION);
                Stats = new ProgressStats(Live_PROGRESS, deliverable.EstimatedUnits, deliverable.TotalUnits, deliverable.EstimatedCosts, deliverable.TotalCosts, variation_adjustments.Where(x => x.DeliverableOriginalGuid == deliverable.OriginalEntityKey).ToList());
                statsSummarizer = new SingleObjectSummarizer(this, partialStatsBuilder);
            }
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

            StatSummarizer.SetBudgetDataPoints();
        }
        #endregion

        #region For User Dashboard and Deliverables
        public PROGRESS loadPROGRESS { get; set; }
        #endregion

        public IDeliverable Deliverable => Entity;

        public string Commodity_Code => Entity.Commodity_Code;

        public Guid? Area_Guid => Entity.Area_Guid;

        public Guid? SubArea_Guid => Entity.SubArea_Guid;

        public decimal TotalUnitsIncludeByDuration => Entity.TotalUnitsIncludeByDuration;

        public decimal EstimatedUnits => Entity.EstimatedUnits;

        public decimal TotalUnits => Entity.TotalUnits;

        public virtual IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_BeforeDataDate => PROGRESS_ITEMS.Where(y => y.EARNED_DATE < ReportingDataDate);

        public virtual PROGRESS_ITEM PROGRESS_ITEM_Current => PROGRESS_ITEMS.FirstOrDefault(y => y.EARNED_DATE == ReportingDataDate);
    
        public virtual IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_UpToCurrentDataDate => PROGRESS_ITEMS.Where(y => y.EARNED_DATE <= ReportingDataDate);

        public virtual IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_AfterDataDate => PROGRESS_ITEMS.Where(y => y.EARNED_DATE > ReportingDataDate);


        private decimal? progress_item_pastunits;
        public decimal PROGRESS_ITEM_PastUnits
        {
            get
            {
                if (progress_item_pastunits == null)
                    if (PROGRESS_ITEM_BeforeDataDate == null)
                        progress_item_pastunits = 0;
                    else
                        progress_item_pastunits =
                            PROGRESS_ITEM_BeforeDataDate.Sum(progress => progress.EARNED_UNITS);

                return (decimal)progress_item_pastunits;
            }
        }

        public virtual decimal PROGRESS_ITEM_CurrentUnits
        {
            get
            {
                ISupportByDuration byDurationProjection = Entity as ISupportByDuration;
                if(byDurationProjection != null && byDurationProjection.IsByDuration)
                    return BluePrintsConstants.DurationBasedDisplayUnits;

                decimal currentUnits = PROGRESS_ITEM_Current == null ? 0 : PROGRESS_ITEM_Current.EARNED_UNITS;
                return PROGRESS_ITEM_PastUnits + currentUnits;
            }
        }
        
        private decimal? progress_item_futureunits;
        public decimal PROGRESS_ITEM_FutureUnits
        {
            get
            {
                if (progress_item_futureunits == null)
                    if (PROGRESS_ITEM_AfterDataDate == null)
                        progress_item_futureunits = 0;
                    else
                        progress_item_futureunits = PROGRESS_ITEM_AfterDataDate.Sum(x => x.EARNED_UNITS);

                return (decimal)progress_item_futureunits;
            }
        }

        public decimal Current_Total_Percentage
        {
            get
            {
                return PROGRESS_ITEM_CurrentUnits / TotalUnits;
            }
        }

        DateTime reportingDataDate { get; set; }
        public DateTime ReportingDataDate { get { return reportingDataDate; } }
        public void SetReportingDataDate(DateTime dataDate)
        {
            reportingDataDate = dataDate;
        }

        List<PROGRESS_ITEM> progress_items { get; set; }
        public virtual List<PROGRESS_ITEM> PROGRESS_ITEMS { get { return progress_items; } }

        public decimal VariationUnits => Entity.VariationUnits;

        public void SetProgressItems(List<PROGRESS_ITEM> progresses)
        {
            progress_items = progresses;
        }

        public void AppendCurrentProgressItem(PROGRESS_ITEM currentProgress)
        {
            if (progress_items == null)
                progress_items = new List<PROGRESS_ITEM>();

            progress_items.Add(currentProgress);
        }
    }

    public static class ProgressItemQueries
    {
        public static IQueryable<ProgressDisplay> SiteDirectProgressItemTransformation(
            IQueryable<PROGRESS_ITEM> PROGRESS_ITEMS, IEnumerable<COMMODITY_CODE> projectCOMMODITY_CODES, IEnumerable<STOCK_CODE> projectSTOCK_CODES, IEnumerable<ESTIMATION_DIRECT_ITEM> projectESTIMATION_DIRECT_ITEMS, IEnumerable<RATE> projectRATES, DateTime reportingDataDate)
        {
            IEnumerable<PROGRESS_ITEM> arrPROGRESS_ITEMS = PROGRESS_ITEMS.ToArray();
            List<ProgressDisplay> progressItems = new List<ProgressDisplay>();
            var PROGRESS_ITEMSByOriginalGuid = PROGRESS_ITEMS.GroupBy(x => x.GUID_ORIBASEITEM).Select(group => new { OriginalGuid = group.Key, Progresses = group.ToList() });
            
            IEnumerable<ESTIMATION_DIRECT_ITEMProjection> ESTIMATION_DIRECT_ITEMProjection = 
                ESTIMATION_DIRECT_ITEMProjectionQueries.ESTIMATION_DIRECT_ITEMProjectionQuery(projectESTIMATION_DIRECT_ITEMS.AsQueryable(), 
                                                                                                projectRATES,
                                                                                                projectSTOCK_CODES,
                                                                                                projectCOMMODITY_CODES).AsEnumerable();

            List<Estimation_Direct_ItemProgress> estimationDirectItemProgress = new List<Estimation_Direct_ItemProgress>();
            foreach (ESTIMATION_DIRECT_ITEMProjection ESTIMATION_DIRECT_ITEM in ESTIMATION_DIRECT_ITEMProjection)
            {
                Estimation_Direct_ItemProgress newEstimation_Direct_itemProgress = new Estimation_Direct_ItemProgress();
                newEstimation_Direct_itemProgress.Entity = ESTIMATION_DIRECT_ITEM;
                newEstimation_Direct_itemProgress.SetReportingDataDate(reportingDataDate);
                SetReportablePROGRESS_ITEM(newEstimation_Direct_itemProgress, PROGRESS_ITEMSByOriginalGuid);
                estimationDirectItemProgress.Add(newEstimation_Direct_itemProgress);
            }

            var estimationDirectProgressByCommodityCode = estimationDirectItemProgress.Where(x => !x.Entity.Entity.STANDALONE)
                .GroupBy(x => x.Entity.Entity.GUID_COMMODITY_CODE).Select(group => new { CommodityCodeGuid = group.Key, Estimation_Direct_ItemProgress = group.ToList() });

            foreach (COMMODITY_CODE COMMODITY_CODE in projectCOMMODITY_CODES)
            {
                Commodity_CodeProgress newCommodity_CodeProgress = new Commodity_CodeProgress();
                newCommodity_CodeProgress.Entity.Entity = COMMODITY_CODE;
                
                var currentCommodity_CodeProgresses = estimationDirectProgressByCommodityCode.FirstOrDefault(x => x.CommodityCodeGuid == COMMODITY_CODE.GUID);
                if(currentCommodity_CodeProgresses != null)
                {
                    newCommodity_CodeProgress.Entity.Reportables = currentCommodity_CodeProgresses.Estimation_Direct_ItemProgress;
                    newCommodity_CodeProgress.Deliverables = currentCommodity_CodeProgresses.Estimation_Direct_ItemProgress.ToList();
                    newCommodity_CodeProgress.SetReportingDataDate(reportingDataDate);
                    ProgressDisplay newProgressDisplay = new ProgressDisplay();
                    newProgressDisplay.ProgressItem = new GroupDisplayReportable(newCommodity_CodeProgress);
                    progressItems.Add(newProgressDisplay);
                }
            }

            progressItems.AddRange(estimationDirectItemProgress.Where(x => x.Entity.Entity.STANDALONE).Select(x => new ProgressDisplay() { ProgressItem = new StandaloneDisplayReportable(x) }));

            return progressItems.AsQueryable();
        }

        private static void SetReportablePROGRESS_ITEM(IReportable reportable, IQueryable<dynamic> PROGRESS_ITEMSByOriginalGuid)
        {
            ICanSetProgresses setProgressesProjection = reportable as ICanSetProgresses;
            if (setProgressesProjection == null)
                return;

            foreach(dynamic item in PROGRESS_ITEMSByOriginalGuid)
            {
                ISortableDeliverable basicDeliverable = reportable.Deliverable as ISortableDeliverable;
                if (basicDeliverable == null)
                    break;

                if (item.OriginalGuid == basicDeliverable.OriginalEntityKey)
                {
                    setProgressesProjection.SetProgressItems(item.Progresses);
                    return;
                }
            }

            setProgressesProjection.SetProgressItems(new List<PROGRESS_ITEM>());
        }
    }
}