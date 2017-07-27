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
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.ViewModel.Reporting
{
    public class COMMODITY_CODEProgress : BluePrintsProgressableByQuantityProjectionBase<COMMODITY_CODEProjection>, IReportable_Quantity_Group
    {
        public IEnumerable<IReportable_Quantity> Reportables { get; set; }

        public override PROGRESS_ITEM PROGRESS_ITEM_Current
        {
            get
            {
                PROGRESS_ITEM newPROGRESS_ITEM = new PROGRESS_ITEM();
                decimal totalCurrentUnits = Reportables.Where(x => x.PROGRESS_ITEM_Current != null).Sum(x => x.PROGRESS_ITEM_Current.EARNED_UNITS);
                newPROGRESS_ITEM.EARNED_UNITS = totalCurrentUnits;
                newPROGRESS_ITEM.EARNED_DATE = ReportingDataDate;
                return newPROGRESS_ITEM;
            }
        }

        public override IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_BeforeDataDate
        {
            get
            {
                return Reportables.SelectMany(x => x.PROGRESS_ITEM_BeforeDataDate);
            }
        }

        public override IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_AfterDataDate
        {
            get
            {
                return Reportables.SelectMany(x => x.PROGRESS_ITEM_AfterDataDate);
            }
        }

        public override IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_UpToCurrentDataDate
        {
            get
            {
                return Reportables.SelectMany(x => x.PROGRESS_ITEM_UpToCurrentDataDate);
            }
        }

        public override List<PROGRESS_ITEM> PROGRESS_ITEMS
        {
            get { return Reportables.SelectMany(x => x.PROGRESS_ITEMS).ToList(); }
        }

        protected override decimal getNewPercentage()
        {
            decimal aggregate_total_quantity = Reportables.Where(x => x.Progress_Type == Estimation_DirectProgressType.Trackable).Sum(x => x.Total_Quantity);
            if (aggregate_total_quantity == 0)
                return 0;

            return set_current_period_quantity == null ? 0 : (decimal)set_current_period_quantity / aggregate_total_quantity;
        }

        private IEnumerable<IReportable_Quantity> trackable_reportables => Reportables.Where(x => x.Progress_Type == Estimation_DirectProgressType.Trackable);

        public decimal Trackable_Total_Units => trackable_reportables.Sum(x => x.Total_Units);

        public decimal Trackable_Total_Quantity => trackable_reportables.Sum(x => x.Total_Quantity);

        public decimal Trackable_Installed_Quantity => trackable_reportables.Sum(x => x.TotalInstalledQuantity);

        public override decimal MaxCurrentQuantity => Trackable_Total_Quantity - Trackable_Installed_Quantity;

        public override decimal Earned_Costs_Total => trackable_reportables.Sum(x => x.Earned_Costs_Total);

        public override decimal Remaining_Hours_To_Completion => Reportables.Sum(x => x.Remaining_Hours_To_Completion);

        public override decimal QuantityPerUnit => Reportables.Sum(x => x.QuantityPerUnit);

        public override decimal UnitsPerQuantity => Reportables.Sum(x => x.UnitsPerQuantity);

        public decimal Trackable_QuantityPerUnit
        {
            get
            {
                if (Trackable_Total_Units == 0)
                    return 0;

                return Trackable_Total_Quantity / Trackable_Total_Units;
            }
        }

        public override decimal get_actual_earned_quantity()
        {
            decimal trackable_earned_units_ondatadate = trackable_reportables.Sum(x => x.Earned_Units_OnDataDate);
            return trackable_earned_units_ondatadate * Trackable_QuantityPerUnit;
        }

        public override IEnumerable<PROGRESS_ITEM> GetExistingOrNewEditedProgresses(Func<Expression<Func<PROGRESS_ITEM, bool>>, PROGRESS_ITEM> repository_find_actual_func)
        {
            List<PROGRESS_ITEM> editPROGRESS_ITEMS = new List<PROGRESS_ITEM>();
            decimal newPercentage = getNewPercentage();
            foreach (IReportable_Quantity quantityReportable in Reportables)
            {
                IEnumerable<PROGRESS_ITEM> savePROGRESS_ITEMS = quantityReportable.GetExistingOrNewEditedProgresses(repository_find_actual_func);
                if(savePROGRESS_ITEMS.Count() > 0)
                {
                    PROGRESS_ITEM savePROGRESS_ITEM = savePROGRESS_ITEMS.First();
                    savePROGRESS_ITEM.EARNED_UNITS = quantityReportable.getCurrentPeriodEarnedUnits(newPercentage);
                    editPROGRESS_ITEMS.Add(savePROGRESS_ITEM);
                }
            }

            return editPROGRESS_ITEMS;
        }

        public override decimal Override_Productivity
        {
            get
            {
                if (set_override_productivity == null)
                {
                    //Group won't have DBProductivityOverride because it's value is derived from childrens
                    //IHaveDBProductivityOverride dbProductivityOverride = Entity as IHaveDBProductivityOverride;
                    set_override_productivity = Reportables.Count() == 0 ? 0 : Reportables.Max(x => x.Override_Productivity);
                }

                return (decimal)set_override_productivity;
            }
            set
            {
                foreach(IReportable_Quantity reportable in Reportables)
                {
                    reportable.Override_Productivity = value;
                    reportable.Update();
                }

                set_override_productivity = value;
            }
        }
    }

    public class ESTIMATION_DIRECT_ITEMProgress : BluePrintsProgressableByQuantityProjectionBase<ESTIMATION_DIRECT_ITEMProjection>, IHaveDBProductivityOverride
    {
        public ESTIMATION_DIRECT_ITEMProgress()
        {

        }

        public ESTIMATION_DIRECT_ITEMProgress(PROJECT PROJECT, PROGRESS LivePROGRESS, IDeliverable_Rates entity, IEnumerable<VariationAdjustment> projectVariationAdjustments)
            : base(PROJECT, LivePROGRESS, entity, projectVariationAdjustments)
        {

        }

        public decimal? DB_Productivity_Override { get => Entity.DB_Productivity_Override; set => Entity.DB_Productivity_Override = value; }
    }

    public class BASELINE_ITEMProgress : BluePrintsProgressableProjectionBase<BASELINE_ITEMProjection>, ISupportByDuration, ICanAssignP6
    {
        public BASELINE_ITEMProgress()
        {

        }

        public BASELINE_ITEMProgress(PROJECT PROJECT, PROGRESS LivePROGRESS, IDeliverable_Rates entity, IEnumerable<VariationAdjustment> projectVariationAdjustments)
            : base(PROJECT, LivePROGRESS, entity, projectVariationAdjustments)
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

        public decimal DeliverableStatusMaxPercentage
        {
            get
            {
                IHaveDeliverableStatus deliverableStatusProjection = Entity as IHaveDeliverableStatus;
                if (deliverableStatusProjection != null && deliverableStatusProjection.Deliverable_Status != null)
                {
                    if (MaxPercentage < deliverableStatusProjection.Deliverable_Status.MAX_PERCENTAGE)
                        return MaxPercentage;

                    return deliverableStatusProjection.Deliverable_Status.MAX_PERCENTAGE;
                }

                return MaxPercentage;
            }
        }

        public bool IsByDuration => Entity.IsByDuration;
    }

    public abstract class BluePrintsProgressableByQuantityProjectionBase<TEntity> : BluePrintsProgressableProjectionBase<TEntity>, IReportable_Quantity
        where TEntity : class, IDeliverable, IHaveCosts, IHaveQuantity, new()
    {
        public BluePrintsProgressableByQuantityProjectionBase()
        {

        }

        public BluePrintsProgressableByQuantityProjectionBase(PROJECT PROJECT, PROGRESS LivePROGRESS, IDeliverable_Rates entity, IEnumerable<VariationAdjustment> projectVariationAdjustments)
            : base(PROJECT, LivePROGRESS, entity, projectVariationAdjustments)
        {

        }

        public decimal Estimated_Quantity => Entity.Estimated_Quantity;
        public decimal Total_Quantity => Entity.Total_Quantity;
        public string UOM => Entity.UOM;

        public virtual decimal QuantityPerUnit
        {
            get
            {
                if (Total_Units == 0)
                    return 0;

                return Total_Quantity / Total_Units;
            }
        }

        public virtual decimal UnitsPerQuantity
        {
            get
            {
                if (Total_Quantity == 0)
                    return 0;

                return Total_Units / Total_Quantity;
            }
        }

        public virtual decimal Remaining_Hours_To_Completion
        {
            get
            {
                return (Total_Quantity - AbsoluteTotalInstalledQuantity) * UnitsPerQuantity;
            }
        }

        protected decimal? set_current_period_quantity { get; set; }
        public virtual decimal CurrentPeriodInstalledQuantity
        {
            get
            {
                if (set_current_period_quantity == null)
                    set_current_period_quantity = get_actual_earned_quantity();

                return (decimal)set_current_period_quantity;
            }
            set => set_current_period_quantity = value;
        }

        public override void Update()
        {
            set_current_period_quantity = null;
            base.Update();
        }

        public override bool ShouldSaveProgress => get_actual_earned_quantity() != set_current_period_quantity;

        
        public virtual decimal get_actual_earned_quantity()
        {
            return Earned_Units_OnDataDate * QuantityPerUnit;
        }

        public decimal PastInstalledQuantity
        {
            get
            {
                if (PROGRESS_ITEM_BeforeDataDate.Count() == 0 || QuantityPerUnit == 0)
                    return 0;

                return PROGRESS_ITEM_BeforeDataDate.Sum(x => x.EARNED_UNITS) * QuantityPerUnit;
            }
        }

        public decimal AbsoluteTotalInstalledQuantity => PastInstalledQuantity + CurrentPeriodInstalledQuantity + FutureInstalledQuantity;

        public decimal TotalInstalledQuantity => PastInstalledQuantity + FutureInstalledQuantity;

        public decimal FutureInstalledQuantity
        {
            get
            {
                if (PROGRESS_ITEM_AfterDataDate.Count() == 0 || QuantityPerUnit == 0)
                    return 0;

                return PROGRESS_ITEM_AfterDataDate.Sum(x => x.EARNED_UNITS) * QuantityPerUnit;
            }
        }

        public virtual decimal MaxCurrentQuantity => Total_Quantity - TotalInstalledQuantity;

        public Estimation_DirectProgressType Progress_Type
        {
            get
            {
                ICanTrack trackableEntity = Entity as ICanTrack;
                if (trackableEntity != null)
                    return trackableEntity.Progress_Type;

                return Estimation_DirectProgressType.Standalone;
            }
        }

        protected override decimal getNewPercentage()
        {
            if (Total_Quantity == 0)
                return 0;

            return set_current_period_quantity == null ? 0 : (decimal)set_current_period_quantity / Total_Quantity;
        }

        public override decimal getCurrentPeriodEarnedUnits(decimal newPercentage)
        {
            return newPercentage * Total_Units;
        }
    }

    public abstract class BluePrintsProgressableProjectionBase<TEntity> : BluePrintsProjectionBase<TEntity>, IReportable, ICanSetProgresses, ICanAssignP6
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

        public BluePrintsProgressableProjectionBase(PROJECT PROJECT, PROGRESS Live_PROGRESS, IDeliverable_Rates entity, IEnumerable<VariationAdjustment> variation_adjustments)
        {
            this.Live_PROGRESS = Live_PROGRESS;
            DateTime reporting_data_date = Live_PROGRESS.DATA_DATE;
            TimeSpan reporting_interval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(Live_PROGRESS);
            DateTime first_aligned_data_date = ChronologicalHelpers.GenerateFirstAlignedDataDate(Live_PROGRESS);
            SetReportingDataDate(reporting_data_date);
            List<VariationAdjustment> currentProgressItemAdjustments = variation_adjustments.Where(x => x.DeliverableOriginalGuid == entity.OriginalEntityKey).ToList();

            PartialStatsBuilder partialStatsBuilder = new PartialStatsBuilder(PROJECT.CURRENCYCONVERSION);
            Stats = new ProgressStats(reporting_data_date, reporting_interval, first_aligned_data_date, entity.Estimated_Units, entity.Total_Units, entity.Estimated_Costs, entity.Total_Costs, variation_adjustments.Where(x => x.DeliverableOriginalGuid == entity.OriginalEntityKey).ToList());
            statsSummarizer = new SingleObjectSummarizer(this, partialStatsBuilder);
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

        public string Phase_Code => Entity.Phase_Code;

        public string Commodity_Code => Entity.Commodity_Code;

        public string Commodity_Display_Code => Entity.Commodity_Display_Code;

        public Guid? Area_Guid => Entity.Area_Guid;

        public Guid? SubArea_Guid => Entity.SubArea_Guid;

        public decimal Estimated_Units => Entity.Estimated_Units;

        public virtual decimal Total_Units => Entity.Total_Units;

        public virtual IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_BeforeDataDate => PROGRESS_ITEMS.Where(y => y.EARNED_DATE < ReportingDataDate);

        public virtual PROGRESS_ITEM PROGRESS_ITEM_Current => PROGRESS_ITEMS.FirstOrDefault(y => y.EARNED_DATE == ReportingDataDate);
    
        public virtual IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_UpToCurrentDataDate => PROGRESS_ITEMS.Where(y => y.EARNED_DATE <= ReportingDataDate);

        public virtual IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_AfterDataDate => PROGRESS_ITEMS.Where(y => y.EARNED_DATE > ReportingDataDate);

        public decimal Baseline_Percentage => Estimated_Units == 0 ? 0 : (Earned_Units_ToDate / Estimated_Units);

        public decimal Total_Percentage_ToDate => Total_Units == 0 ? 0 : (Earned_Units_ToDate / Total_Units);

        public decimal Total_Percentage => Total_Units == 0 ? 0 : (Earned_Units_Total / Total_Units);

        private decimal? set_total_earned_percentage;
        public decimal Total_Earned_Percentage
        {
            get
            {
                if(set_total_earned_percentage == null)
                    set_total_earned_percentage = get_actual_total_earned_percentage();

                return (decimal)set_total_earned_percentage;
            }
            set
            {
                set_total_earned_percentage = value;
            }
        }

        public override void Update()
        {
            set_total_earned_percentage = null;
            set_override_productivity = null;
            base.Update();
        }

        //because entity goes through repository.reload() process this will be null since it's not meddled in the query
        public virtual bool ShouldSaveProgress => get_actual_total_earned_percentage(true) != set_total_earned_percentage;

        public virtual decimal? get_actual_total_earned_percentage(bool can_return_null = false)
        {
            //this happens during undo when first PROGRESS_ITEM is created in the same session
            if (Earned_Units_OnDataDate == 0 && PROGRESS_ITEM_Current == null && can_return_null)
                return null;

            ISupportByDuration supportByDurationProjection = Entity as ISupportByDuration;

            if (supportByDurationProjection != null && supportByDurationProjection.IsByDuration)
                return Earned_Units_ToDate / BluePrintsConstants.DurationBasedDisplayUnits;
            else if (Total_Units > 0)
                return Earned_Units_ToDate / Total_Units;
            else
                return 1;
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

        public decimal Earned_Percentage_OnDataDate => Total_Units == 0 ? 0 : (Earned_Units_OnDataDate / Total_Units);

        public virtual decimal Earned_Units_OnDataDate => PROGRESS_ITEM_Current == null ? 0 : PROGRESS_ITEM_Current.EARNED_UNITS;

        public decimal Earned_Costs_OnDataDate => Earned_Units_OnDataDate * Entity.ItemRate;

        public virtual decimal Earned_Units_ToDate => Earned_Units_BeforeDataDate + Earned_Units_OnDataDate;

        public decimal Earned_Units_Total => Earned_Units_ToDate + Earned_Units_AfterDataDate;

        public virtual decimal Earned_Costs_Total => Earned_Units_Total * Entity.ItemRate;

        public decimal Earned_Costs_ToDate => Earned_Units_ToDate * Entity.ItemRate;
        
        DateTime reportingDataDate { get; set; }
        public DateTime ReportingDataDate { get { return reportingDataDate; } }
        public void SetReportingDataDate(DateTime dataDate)
        {
            reportingDataDate = dataDate;
        }

        List<PROGRESS_ITEM> progress_items { get; set; }
        public virtual List<PROGRESS_ITEM> PROGRESS_ITEMS
        {
            get
            {
                if (progress_items == null)
                    progress_items = new List<PROGRESS_ITEM>();

                return progress_items;
            }
        }

        public decimal Current_Productivity => SchedulePercentage == 0 ? 0 : Total_Earned_Percentage / SchedulePercentage;

        protected decimal? set_override_productivity;
        public virtual decimal Override_Productivity
        {
            get
            {
                if (set_override_productivity == null)
                    set_override_productivity = get_db_or_current_productivity();

                return (decimal)set_override_productivity;
            }
            set
            {
                IHaveDBProductivityOverride dbProductivityOverride = Entity as IHaveDBProductivityOverride;
                if (dbProductivityOverride != null)
                    dbProductivityOverride.DB_Productivity_Override = value;

                set_override_productivity = value;
            }
        }

        public virtual bool ShouldSaveProductivity => set_override_productivity != get_db_or_current_productivity();

        protected virtual decimal? get_db_or_current_productivity()
        {
            IHaveDBProductivityOverride dbProductivityOverride = Entity as IHaveDBProductivityOverride;
            if (dbProductivityOverride != null && dbProductivityOverride.DB_Productivity_Override != null && dbProductivityOverride.DB_Productivity_Override > 0)
                return dbProductivityOverride.DB_Productivity_Override;
            else
                return Current_Productivity;
        }

        public decimal Variation_Units => Entity.Variation_Units;

        public string Discipline_Code => Entity.Discipline_Code;

        public string Deliverable_Name => Entity.Deliverable_Name;

        public Guid? Workpack_Guid => Entity.Workpack_Guid;

        public Guid OriginalEntityKey => Entity.OriginalEntityKey;

        public decimal ItemRate => Entity.ItemRate;

        public decimal Estimated_Costs => Entity.Estimated_Costs;

        public decimal Variation_Costs => Entity.Variation_Costs;

        public decimal Total_Costs => Entity.Total_Costs;

        public void SetProgressItems(List<PROGRESS_ITEM> progresses)
        {
            progress_items = progresses;
        }

        public void AppendProgressItem(PROGRESS_ITEM currentProgress)
        {
            progress_items.Add(currentProgress);
        }

        public void SetOriginalEntityKey(Guid newGuid)
        {
            Entity.SetOriginalEntityKey(newGuid);
        }

        public virtual IEnumerable<PROGRESS_ITEM> GetExistingOrNewEditedProgresses(Func<Expression<Func<PROGRESS_ITEM, bool>>, PROGRESS_ITEM> repository_find_actual_func)
        {
            PROGRESS_ITEM edit_PROGRESS_ITEM;
            if (PROGRESS_ITEM_Current != null)
                edit_PROGRESS_ITEM = PROGRESS_ITEM_Current;
            else
                edit_PROGRESS_ITEM = createNewProgress(repository_find_actual_func);


            edit_PROGRESS_ITEM.EARNED_UNITS = getCurrentPeriodEarnedUnits(getNewPercentage());

            //use list because overriding member will be a group
            List<PROGRESS_ITEM> editPROGRESS_ITEMS = new List<PROGRESS_ITEM>();
            editPROGRESS_ITEMS.Add(edit_PROGRESS_ITEM);

            return editPROGRESS_ITEMS;
        }

        protected virtual decimal getNewPercentage()
        {
            return set_total_earned_percentage == null ? 0 : (decimal)set_total_earned_percentage;
        }

        public virtual decimal getCurrentPeriodEarnedUnits(decimal newPercentage)
        {
            decimal total_earned_units = newPercentage * Total_Units;
            decimal current_period_earned_units = total_earned_units - Earned_Units_BeforeDataDate;
            return current_period_earned_units;
        }

        public PROGRESS_ITEM createNewProgress(Func<Expression<Func<PROGRESS_ITEM, bool>>, PROGRESS_ITEM> repository_find_actual_func)
        {
            PROGRESS_ITEM actual_PROGRESS_ITEM = repository_find_actual_func(x => x.EARNED_DATE == ReportingDataDate && x.GUID_ORIBASEITEM == OriginalEntityKey && x.GUID_PROGRESS == Live_PROGRESS.GUID);
            if (actual_PROGRESS_ITEM != null)
                return actual_PROGRESS_ITEM;

            PROGRESS_ITEM savePROGRESS_ITEM = new PROGRESS_ITEM();
            savePROGRESS_ITEM.GUID_ORIBASEITEM = Entity.OriginalEntityKey;
            savePROGRESS_ITEM.GUID_PROGRESS = Live_PROGRESS.GUID;
            savePROGRESS_ITEM.EARNED_DATE = Live_PROGRESS.DATA_DATE;
            savePROGRESS_ITEM.CREATED = DateTime.Now;

            return savePROGRESS_ITEM;
        }

        private List<P6_ASSIGNMENT> p6_assignments;
        public List<P6_ASSIGNMENT> P6_Assignments
        {
            get
            {
                if (p6_assignments == null)
                    p6_assignments = new List<P6_ASSIGNMENT>();

                return p6_assignments;
            }
            set
            {
                p6_assignments = value;
            }
        }

        public decimal Remaining_Percentage
        {
            get
            {
                return 1 - Assigned_Percentage;
            }
        }

        public decimal Assigned_Percentage
        {
            get
            {
                return P6_Assignments.Sum(x => (x.HIGH_VALUE - (x.LOW_VALUE - 0.01m)));
            }
        }
    }
}