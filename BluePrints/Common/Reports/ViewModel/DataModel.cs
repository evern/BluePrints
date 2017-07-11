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

        public override decimal GetCurrentPeriodPercentageByQuantity(decimal newTotalQuantity)
        {
            return base.GetCurrentPeriodPercentageByQuantity(newTotalQuantity);
        }
    }

    public class Estimation_Direct_ItemProgress : BluePrintsProgressableByQuantityProjectionBase<ESTIMATION_DIRECT_ITEMProjection>
    {

    }

    public class Baseline_ItemProgress : BluePrintsProgressableProjectionBase<BASELINE_ITEMProjection>
    {
        public Baseline_ItemProgress()
        {

        }

        public Baseline_ItemProgress(PROJECT PROJECT, PROGRESS LivePROGRESS, IEnumerable<VariationAdjustment> projectVariationAdjustments)
            : base(PROJECT, LivePROGRESS, projectVariationAdjustments)
        {

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
    }

    public abstract class BluePrintsProgressableByQuantityProjectionBase<TEntity> : BluePrintsProgressableProjectionBase<TEntity>, IQuantityReportable
        where TEntity : class, IDeliverable, IHaveCosts, IHaveQuantity, new()
    {
        public decimal QuantityPerHour
        {
            get
            {
                if (Total_Units == 0)
                    return 0;

                return Total_Quantity / Total_Units;
            }
        }

        public decimal CurrentTotalInstalledQuantity
        {
            get
            {
                return Earned_Units_ToDate * QuantityPerHour;
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

        public virtual decimal GetCurrentPeriodPercentageByQuantity(decimal newTotalQuantity)
        {
            if (Total_Quantity == 0)
                return 0;

            return newTotalQuantity / Total_Quantity;
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
            this.Live_PROGRESS = Live_PROGRESS;
            SetReportingDataDate(Live_PROGRESS.DATA_DATE);
            ISortableDeliverableProjection deliverable = Entity as ISortableDeliverableProjection;
            if(deliverable != null)
            {
                List<VariationAdjustment> currentProgressItemAdjustments = variation_adjustments.Where(x => x.DeliverableOriginalGuid == deliverable.OriginalEntityKey).ToList();

                PartialStatsBuilder partialStatsBuilder = new PartialStatsBuilder(PROJECT.CURRENCYCONVERSION);
                Stats = new ProgressStats(Live_PROGRESS, deliverable.Estimated_Units, deliverable.Total_Units, deliverable.EstimatedCosts, deliverable.Total_Costs, variation_adjustments.Where(x => x.DeliverableOriginalGuid == deliverable.OriginalEntityKey).ToList());
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
        public PROGRESS Live_PROGRESS { get; set; }
        #endregion

        public IDeliverable Deliverable => Entity;

        public string Commodity_Code => Entity.Commodity_Code;

        public Guid? Area_Guid => Entity.Area_Guid;

        public Guid? SubArea_Guid => Entity.SubArea_Guid;

        public decimal Estimated_Units => Entity.Estimated_Units;

        public decimal Total_Units => Entity.Total_Units;

        public virtual IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_BeforeDataDate => PROGRESS_ITEMS.Where(y => y.EARNED_DATE < ReportingDataDate);

        public virtual PROGRESS_ITEM PROGRESS_ITEM_Current => PROGRESS_ITEMS.FirstOrDefault(y => y.EARNED_DATE == ReportingDataDate);
    
        public virtual IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_UpToCurrentDataDate => PROGRESS_ITEMS.Where(y => y.EARNED_DATE <= ReportingDataDate);

        public virtual IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_AfterDataDate => PROGRESS_ITEMS.Where(y => y.EARNED_DATE > ReportingDataDate);

        public decimal GetCurrentPeriodHours(decimal currentPeriodPercentage)
        {
            return currentPeriodPercentage * Total_Units;
        }

        public decimal Baseline_Percentage => Estimated_Units == 0 ? 0 : (Earned_Units_ToDate / Estimated_Units);

        public decimal Total_Percentage => Total_Units == 0 ? 0 : (Earned_Units_ToDate / Total_Units);

        private decimal? total_earned_percentage;
        public decimal Total_Earned_Percentage
        {
            get
            {
                if (total_earned_percentage == null)
                {
                    ISupportByDuration supportByDurationProjection = Deliverable as ISupportByDuration;
                    if(supportByDurationProjection != null && supportByDurationProjection.IsByDuration)
                    {
                        total_earned_percentage = Earned_Units_ToDate / BluePrintsConstants.DurationBasedDisplayUnits;
                    }
                    else if (Total_Units > 0)
                    {
                        total_earned_percentage = Earned_Units_ToDate / Total_Units;
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
                if (Total_Units > 0)
                {
                    IOriginalGuidEntityKey originalEntityKeyProjection = Deliverable as IOriginalGuidEntityKey;
                    if(originalEntityKeyProjection != null && Live_PROGRESS != null)
                    {
                        decimal earnedUnits = value * Total_Units;
                        earnedUnits -= Earned_Units_BeforeDataDate;

                        PROGRESS_ITEM pendingSaveProgress;

                        if (PROGRESS_ITEM_Current == null)
                            pendingSaveProgress = new PROGRESS_ITEM();
                        else
                            pendingSaveProgress = PROGRESS_ITEM_Current;

                        //workaround for created because Save() only sets the projection primary key, this is used for property redo where the interceptor only tampers with UPDATED and CREATED is left as null
                        if (pendingSaveProgress.CREATED.Date.Year == 1)
                            pendingSaveProgress.CREATED = DateTime.Now;

                        pendingSaveProgress.GUID_PROGRESS = Live_PROGRESS.GUID;
                        pendingSaveProgress.GUID_ORIBASEITEM = originalEntityKeyProjection.OriginalEntityKey;
                        pendingSaveProgress.EARNED_DATE = ReportingDataDate;
                        pendingSaveProgress.EARNED_UNITS = earnedUnits;
                        AppendCurrentProgressItem(pendingSaveProgress);
                        //from here we leave it up to the view to save PROGRESS_ITEM_Current
                    }
                }

                total_earned_percentage = value;
            }
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

        public decimal MinPercentage => Total_Units == 0 ? 0 : (Earned_Units_BeforeDataDate / Total_Units);

        public decimal MaxPercentage => Total_Units == 0 ? 0 : ((Total_Units - Earned_Units_AfterDataDate) / Total_Units);
        
        public decimal DeliverableStatusMaxPercentage
        {
            get
            {
                IHaveDeliverableStatus deliverableStatusProjection = Deliverable as IHaveDeliverableStatus;
                if (deliverableStatusProjection != null && deliverableStatusProjection.Deliverable_Status != null)
                {
                    if (MaxPercentage < deliverableStatusProjection.Deliverable_Status.MAX_PERCENTAGE)
                        return MaxPercentage;

                    return deliverableStatusProjection.Deliverable_Status.MAX_PERCENTAGE;
                }

                return MaxPercentage;
            }
        }

        private decimal? earned_units_beforedatadate;
        public decimal Earned_Units_BeforeDataDate
        {
            get
            {
                if (earned_units_beforedatadate == null)
                    if (PROGRESS_ITEM_BeforeDataDate == null)
                        earned_units_beforedatadate = 0;
                    else
                        earned_units_beforedatadate = PROGRESS_ITEM_BeforeDataDate.Sum(progress => progress.EARNED_UNITS);

                return (decimal)earned_units_beforedatadate;
            }
        }

        public decimal Earned_Percentage_OnDataDate
        {
            get { return Total_Units == 0 ? 0 : (Earned_Units_OnDataDate / Total_Units); }
        }

        public decimal Earned_Units_OnDataDate
        {
            get
            {
                return PROGRESS_ITEM_Current == null ? 0 : PROGRESS_ITEM_Current.EARNED_UNITS;
            }
        }

        public decimal Earned_Costs_OnDataDate
        {
            get
            {
                return Earned_Units_OnDataDate * Entity.ItemRate;
            }
        }

        public virtual decimal Earned_Units_ToDate
        {
            get
            {
                return Earned_Units_BeforeDataDate + Earned_Units_OnDataDate;
            }
        }

        public decimal Earned_Cost_ToDate
        {
            get
            {
                return Earned_Units_ToDate * Entity.ItemRate;
            }
        }
        
        private decimal? earned_units_afterdatadate;
        public decimal Earned_Units_AfterDataDate
        {
            get
            {
                if (earned_units_afterdatadate == null)
                    if (PROGRESS_ITEM_AfterDataDate == null)
                        earned_units_afterdatadate = 0;
                    else
                        earned_units_afterdatadate = PROGRESS_ITEM_AfterDataDate.Sum(x => x.EARNED_UNITS);

                return (decimal)earned_units_afterdatadate;
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
}