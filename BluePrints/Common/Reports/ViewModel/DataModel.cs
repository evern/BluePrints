using BaseModel.Attributes;
using BaseModel.Misc;
using BluePrints.Common.Base;
using BluePrints.Common.Misc;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.XtraEditors.DXErrorProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace BluePrints.Common.ViewModel.Reporting
{
    public class STOCK_GROUPProgress : BluePrintsProgressableByQuantityProjectionBase<STOCK_GROUPProjection>, IReportable_Quantity_Group
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
            decimal aggregate_total_quantity = Reportables.Where(x => x.Progress_Type == EstimateProgressType.Trackable).Sum(x => x.Total_Quantity);
            if (aggregate_total_quantity == 0)
                return 0;

            return set_current_period_quantity == null ? 0 : (decimal)set_current_period_quantity / aggregate_total_quantity;
        }

        private IEnumerable<IReportable_Quantity> trackable_reportables => Reportables.Where(x => x.Progress_Type == EstimateProgressType.Trackable);

        public decimal Trackable_Total_Units => trackable_reportables.Sum(x => x.Total_Units);

        public decimal Trackable_Total_Quantity => trackable_reportables.Sum(x => x.Total_Quantity);

        public override decimal Trackable_Installed_Quantity => trackable_reportables.Sum(x => x.AbsoluteTotalInstalledQuantity);

        public override decimal AbsoluteTotalInstalledQuantity => Reportables.Sum(x => x.AbsoluteTotalInstalledQuantity);

        public override decimal MaxCurrentQuantity => trackable_reportables.Sum(x => x.MaxCurrentQuantity);

        public override decimal Earned_Costs_Total => trackable_reportables.Sum(x => x.Earned_Costs_Total);

        public override decimal Remaining_Hours_To_Completion => Reportables.Sum(x => x.Remaining_Hours_To_Completion);

        public override decimal QuantityPerUnit
        {
            get
            {
                decimal totalUnits = Reportables.Sum(x => x.Total_Units);
                if (totalUnits == 0)
                    return 0;

                decimal totalQuantity = Reportables.Sum(x => x.Total_Quantity);
                if (totalQuantity == 0)
                    return 0;

                return totalQuantity / totalUnits;
            }
        }

        public override decimal UnitsPerQuantity => Reportables.Sum(x => x.UnitsPerQuantity);

        public override decimal Earned_Install_Costs_OnDataDate => Reportables.Sum(x => x.Earned_Install_Costs_OnDataDate);

        public override decimal Earned_Supply_Costs_OnDataDate => Reportables.Sum(x => x.Earned_Supply_Costs_OnDataDate);

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

        public override decimal? Override_Productivity
        {
            get
            {
                if (set_override_productivity == null)
                {
                    //Group won't have DBProductivityOverride because it's value is derived from childrens
                    //IHaveDBProductivityOverride dbProductivityOverride = Entity as IHaveDBProductivityOverride;
                    set_override_productivity = Reportables.Count() == 0 ? 0 : Reportables.Max(x => x.Override_Productivity);
                }

                return set_override_productivity;
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

    public class ESTIMATE_ITEMProgress : BluePrintsProgressableByQuantityProjectionBase<ESTIMATE_ITEMProjection>, IHaveDBProductivityOverride, IHaveProcurementSubjob, IEstimateItem, IDXDataErrorInfo
    {
        public ESTIMATE_ITEMProgress()
        {

        }

        public ESTIMATE_ITEMProgress(PROJECT PROJECT, PROGRESS LivePROGRESS, IDeliverable_Rates entity, IEnumerable<VariationAdjustment> projectVariationAdjustments)
            : base(PROJECT, LivePROGRESS, entity, projectVariationAdjustments)
        {

        }

        public decimal? DB_Productivity_Override { get => Entity.DB_Productivity_Override; set => Entity.DB_Productivity_Override = value; }

        public Guid? Baseline_Guid { get => Entity.Baseline_Guid; set => Entity.Baseline_Guid = value; }

        public Guid? GUID_VARIATION { get => Entity.Variation_Guid; set => Entity.Variation_Guid = value; }

        public decimal Estimated_Value { get => Entity.Estimated_Value; set => Entity.Estimated_Value = value; }

        public decimal DC_Value { get => Entity.DC_Value; set => Entity.DC_Value = value; }

        public Guid? Procurement_Subjob_Guid { get => Entity.Procurement_Subjob_Guid; set => Entity.Procurement_Subjob_Guid = value; }

        public decimal Variance_Quantity => Budget_Quantity - Estimate_Quantity;

        public decimal Variance_Stock_Code_Install_Hours => Budget_Stock_Code_Install_Hours - Estimate_Stock_Code_Install_Hours;

        public decimal Variance_Stock_Code_Supply_Rate => Budget_Stock_Code_Supply_Rate - Estimate_Stock_Code_Supply_Rate;

        public decimal Variance_Supply_Cost => Budget_Supply_Cost - Estimate_Supply_Cost;

        public decimal Variance_Install_Cost => Budget_Install_Cost - Estimate_Install_Cost;

        public decimal Variance_Install_Hours => Budget_Install_Hours - Estimate_Install_Hours;

        public decimal Variance_FreightRate => Budget_FreightRate - Estimate_FreightRate;

        public decimal Variance_Freight_Cost => Budget_Freight_Cost - Estimate_Freight_Cost;

        public ESTIMATE_ITEMProgress ReadOnlyEstimate => this;

        public void GetError(ErrorInfo info)
        {
        }

        public void GetPropertyError(string propertyName, ErrorInfo info)
        {
            if (propertyName.Contains(BindableBase.GetPropertyName(() => new ESTIMATE_ITEMProgress().Entity.Entity.GUID_COMMODITY_CODE)) && !Entity.Entity.IsCommodityCodeValid)
            {
                info.ErrorText = "Invalid commodity code, please check discipline";
            }
            else if (propertyName.Contains(BindableBase.GetPropertyName(() => new ESTIMATE_ITEMProgress().Entity.Entity.GUID_DISCIPLINE)) && !Entity.Entity.IsDisciplineCodeValid)
            {
                info.ErrorText = "Invalid discipline code, please check phase";
            }
        }
    }


    public abstract class BluePrintsProgressableByQuantityProjectionBase<TEntity> : BluePrintsProgressableProjectionBase<TEntity>, IReportable_Quantity, ICanAssignP6
        where TEntity : class, IDeliverable_Rates, IHaveCosts, IHaveStock_Group, IHaveQuantity, new()
    {
        public BluePrintsProgressableByQuantityProjectionBase()
        {

        }

        public BluePrintsProgressableByQuantityProjectionBase(PROJECT PROJECT, PROGRESS LivePROGRESS, IDeliverable_Rates entity, IEnumerable<VariationAdjustment> projectVariationAdjustments)
            : base(PROJECT, LivePROGRESS, entity, projectVariationAdjustments, false)
        {
            
        }

        public string Estimate_UOM => Entity.Estimate_UOM;

        public string Budget_UOM => Entity.Budget_UOM;

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

        public virtual decimal Schedule_Estimated_Quantity
        {
            get
            {
                return SchedulePercentage * Budget_Quantity;
            }
        }

        public virtual decimal Schedule_Estimated_Current_Period_Quantity
        {
            get
            {
                return ScheduleCurrentPeriodPercentage * Budget_Quantity;
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

                decimal earnedUnits = PROGRESS_ITEM_BeforeDataDate.Sum(x => x.EARNED_UNITS) * QuantityPerUnit;

                return earnedUnits;
            }
        }

        public virtual decimal AbsoluteTotalInstalledQuantity => PastInstalledQuantity + CurrentPeriodInstalledQuantity + FutureInstalledQuantity;

        public decimal MinEstimateQuantity => (AbsoluteTotalInstalledQuantity - Entity.Variation_Quantity) < 0 ? 0 : AbsoluteTotalInstalledQuantity - Entity.Variation_Quantity;

        public decimal TotalInstalledQuantity => PastInstalledQuantity + CurrentPeriodInstalledQuantity;

        public virtual decimal Trackable_Installed_Quantity => this.Progress_Type == EstimateProgressType.Trackable ? TotalInstalledQuantity : 0;

        public decimal FutureInstalledQuantity
        {
            get
            {
                if (PROGRESS_ITEM_AfterDataDate.Count() == 0 || QuantityPerUnit == 0)
                    return 0;

                return PROGRESS_ITEM_AfterDataDate.Sum(x => x.EARNED_UNITS) * QuantityPerUnit;
            }
        }

        public virtual decimal MaxCurrentQuantity => Math.Round(Total_Quantity - PastInstalledQuantity - FutureInstalledQuantity, 2);

        public EstimateProgressType Progress_Type
        {
            get
            {
                ICanTrack trackableEntity = Entity as ICanTrack;
                if (trackableEntity != null)
                    return trackableEntity.Progress_Type;

                return EstimateProgressType.Standalone;
            }
        }

        public decimal Total_Install_Hours => QuantityPerUnit == 0 ? 0 : AbsoluteTotalInstalledQuantity / QuantityPerUnit;

        public decimal Total_Estimate_Cost => Estimate_Install_Cost + Estimate_Freight_Cost + Estimate_Supply_Cost;

        public decimal Estimate_Stock_Code_Install_Hours => Entity.Estimate_Stock_Code_Install_Hours;

        public decimal Budget_Stock_Code_Install_Hours => Entity.Budget_Stock_Code_Install_Hours;

        public decimal Estimate_Stock_Code_Supply_Rate => Entity.Estimate_Stock_Code_Supply_Rate;

        public decimal Budget_Stock_Code_Supply_Rate => Entity.Budget_Stock_Code_Supply_Rate;

        public decimal Variation_Quantity => Entity.Variation_Quantity;

        public virtual decimal Earned_Install_Costs_OnDataDate => Earned_Units_OnDataDate * Budget_ItemRate;

        public decimal Estimate_ItemRate => Entity.Estimate_ItemRate;

        public virtual decimal Earned_Supply_Costs_OnDataDate => Earned_Units_OnDataDate * Entity.Estimate_Stock_Code_Supply_Rate;

        public decimal Earned_Total_Costs_OnDataDate => Earned_Install_Costs_OnDataDate + Earned_Supply_Costs_OnDataDate;

        public Guid? Stock_Group_Guid => Entity.Stock_Group_Guid;

        public decimal Budget_FreightRate => Entity.Budget_FreightRate;

        public decimal Estimate_Install_Cost => Entity.Estimate_Install_Cost;

        public decimal Variation_Install_Cost => Entity.Variation_Install_Cost;

        public decimal Estimate_Freight_Cost => Entity.Estimate_Freight_Cost;

        public decimal Variation_Freight_Cost => Entity.Variation_Freight_Cost;

        public decimal Estimate_Install_Hours => Entity.Estimate_Install_Hours;

        public decimal Variation_Install_Hours => Entity.Variation_Install_Hours;

        public decimal Estimate_Supply_Cost => Entity.Estimate_Supply_Cost;

        public decimal Budget_Supply_Cost => Entity.Budget_Supply_Cost;

        public decimal Variation_Supply_Cost => Entity.Variation_Supply_Cost;

        public override decimal P6_Assignment_Total_Quantity => Entity.Total_Quantity;

        public override string P6_Assignment_UOM => Entity.Estimate_UOM;

        public decimal Estimate_Quantity => Entity.Estimate_Quantity;

        public decimal Budget_Install_Hours => Entity.Budget_Install_Hours;

        public decimal Budget_Install_Cost => Entity.Budget_Install_Cost;

        public decimal Total_Budget_Install_Cost => Entity.Total_Budget_Install_Cost;

        public decimal Total_Budget_Freight_Cost => Entity.Total_Budget_Freight_Cost;

        public decimal Total_Budget_Supply_Cost => Entity.Total_Budget_Supply_Cost;

        public decimal Total_Budget_Cost => Entity.Total_Budget_Cost;

        public decimal Budget_Freight_Cost => Entity.Budget_Freight_Cost;

        public decimal Estimate_Units => Entity.Estimate_Units;

        public decimal Estimate_FreightRate => Entity.Estimate_FreightRate;

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
        where TEntity : class, IDeliverable_Rates, IHaveCosts, new()
    {
        #region Stats Parameters
        readonly SingleObjectSummarizer statsSummarizer;
        public SingleObjectSummarizer StatSummarizer => statsSummarizer;
        public ProgressStats Stats { get; set; }
        public List<VariationAdjustment> ApprovedVariations;
        public BluePrintsProgressableProjectionBase()
        {
            //Initialization without stats
        }

        public BluePrintsProgressableProjectionBase(PROJECT PROJECT, PROGRESS Live_PROGRESS, IDeliverable_Rates entity, IEnumerable<VariationAdjustment> variation_adjustments, bool useReportDate, DateTime? extrapolateDate = null)
        {
            this.Live_PROGRESS = Live_PROGRESS;
            //DateTime reporting_data_date = Live_PROGRESS.DATA_DATE;
            DateTime reporting_data_date = useReportDate ? Live_PROGRESS.REPORT_DATE == null ? Live_PROGRESS.DATA_DATE : (DateTime)Live_PROGRESS.REPORT_DATE : Live_PROGRESS.DATA_DATE;
            TimeSpan reporting_interval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(Live_PROGRESS);
            DateTime first_aligned_data_date = ChronologicalHelpers.GenerateFirstAlignedDataDate(Live_PROGRESS);
            SetReportingDataDate(reporting_data_date);
            ApprovedVariations = variation_adjustments.Where(x => x.DeliverableOriginalGuid == entity.OriginalEntityKey).ToList();
            decimal variationUnits = ApprovedVariations.Sum(x => x.AdjustmentUnits);
            decimal totalUnits = variationUnits + variationUnits;
            decimal unitsPerQuantity = entity.Budget_Quantity == 0 ? 0 : entity.Budget_Units / entity.Budget_Quantity;
            decimal totalQuantity = unitsPerQuantity == 0 ? 0 : totalUnits / unitsPerQuantity;
            decimal costsPerUnit = entity.Budget_Costs == 0 ? 0 : entity.Budget_Units / entity.Budget_Costs;
            decimal totalCosts = totalUnits * costsPerUnit;

            PartialStatsBuilder partialStatsBuilder = new PartialStatsBuilder(PROJECT.CURRENCYCONVERSION);
            Stats = new ProgressStats(reporting_data_date, reporting_interval, first_aligned_data_date, entity.Budget_Units, totalUnits, entity.Budget_Quantity, totalQuantity, entity.Budget_Costs, totalCosts, ApprovedVariations, extrapolateDate);
            statsSummarizer = new SingleObjectSummarizer(this, partialStatsBuilder);
        }

        public void BuildStats(decimal weightingPortion = 1, List<StatsCalculationType> calcTypes = null)
        {
            if (StatSummarizer == null || Stats == null)
                return;

            if(calcTypes == null)
            {
                calcTypes = BluePrintsDataUtils.AllCalcTypes;
            }

            StatSummarizer.Build(false, false, weightingPortion, calcTypes);
        }

        public void BuildBudgetedStats(decimal weightingPortion = 1)
        {
            if (StatSummarizer == null || Stats == null)
                return;

            StatSummarizer.SetBudgetDataPoints(weightingPortion);
            StatSummarizer.SetCurrentDataPoints(weightingPortion);
        }
        #endregion

        #region For User Dashboard and Deliverables
        public PROGRESS Live_PROGRESS { get; set; }
        #endregion

        public decimal Total_Units => Variation_Units + Budget_Units;

        public DateTime? TaskAssignmentStartDate { get; set; }

        public decimal EarnedUnitsAccountedFor { get; set; }

        public string Phase_Code => Entity.Phase_Code;

        public string Commodity_Code => Entity.Commodity_Code;

        public Guid? Area_Guid => Entity.Area_Guid;

        public Guid? SubArea_Guid => Entity.SubArea_Guid;

        public decimal Budget_Units => Entity.Budget_Units;

        public virtual IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_BeforeDataDate => PROGRESS_ITEMS.Where(y => y.EARNED_DATE.Date < ReportingDataDate.Date);

        public virtual PROGRESS_ITEM PROGRESS_ITEM_Current => PROGRESS_ITEMS.FirstOrDefault(y => y.EARNED_DATE.Date == ReportingDataDate.Date);
    
        public virtual IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_UpToCurrentDataDate => PROGRESS_ITEMS.Where(y => y.EARNED_DATE.Date <= ReportingDataDate.Date);

        public virtual IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_AfterDataDate => PROGRESS_ITEMS.Where(y => y.EARNED_DATE.Date > ReportingDataDate.Date);

        public decimal Baseline_Percentage => Budget_Units == 0 ? 0 : (Earned_Units_ToDate / Budget_Units);

        public decimal Total_Percentage_ToDate => Total_Units == 0 ? 0 : (Earned_Units_ToDate / Total_Units);

        public decimal Total_Percentage => Total_Units == 0 ? 0 : (Earned_Units_Total / Total_Units);

        public DateTime? FirstDataDate => PROGRESS_ITEMS.Count() == 0 ? (DateTime?)null : PROGRESS_ITEMS.Min(x => x.EARNED_DATE);

        public DateTime? LastDataDate => PROGRESS_ITEMS.Count() == 0 ? (DateTime?)null : PROGRESS_ITEMS.Max(x => x.EARNED_DATE);

        public IEnumerable<DeliverableEarnedPercentages> EarnedPercentages => Stats == null || Stats.Earned == null || Stats.Earned.CumulativeDataPoints == null || Stats.Earned.CumulativeDataPoints.Count == 0 ? null : Stats.Earned.CumulativeDataPoints.Where(x => Stats.Earned.DataPoints.Any(z => z.ProgressDate == x.ProgressDate)).Select(x => new DeliverableEarnedPercentages() { EarnedDate = x.ProgressDate, EarnedPercentage = x.UnitsPercentage });

        public bool IsByDuration { get => Entity.IsByDuration; set => Entity.IsByDuration = value; }
        #region local non-interface variables
        public Guid? GuidCurrent
        {
            get
            {
                if (PROGRESS_ITEMS.Count == 0)
                    return null;

                if (PROGRESS_ITEM_Current == null)
                    return null;

                return PROGRESS_ITEM_Current.GUID;
            }
        }

        public DateTime? Last_Updated
        {
            get
            {
                if (PROGRESS_ITEMS.Count == 0)
                    return null;

                if (PROGRESS_ITEM_Current != null)
                    return PROGRESS_ITEM_Current.UPDATED;

                return PROGRESS_ITEMS.OrderBy(x => x.UPDATED).Last().UPDATED;
            }
        }

        public Guid? Last_UpdatedBy
        {
            get
            {
                if (PROGRESS_ITEMS.Count == 0)
                    return null;

                if (PROGRESS_ITEM_Current != null)
                    return PROGRESS_ITEM_Current.UPDATEDBY;

                return PROGRESS_ITEMS.OrderBy(x => x.UPDATED).Last().UPDATEDBY;
            }
        }

        public DateTime? Last_Created
        {
            get
            {
                if (PROGRESS_ITEMS.Count == 0)
                    return null;

                if (PROGRESS_ITEM_Current != null)
                    return PROGRESS_ITEM_Current.CREATED;

                return PROGRESS_ITEMS.OrderBy(x => x.CREATED).Last().CREATED;
            }
        }

        public Guid? Last_CreatedBy
        {
            get
            {
                if (PROGRESS_ITEMS.Count == 0)
                    return null;

                if (PROGRESS_ITEM_Current != null)
                    return PROGRESS_ITEM_Current.CREATEDBY;

                return PROGRESS_ITEMS.OrderBy(x => x.CREATED).Last().CREATEDBY;
            }
        }
        #endregion

        public IEnumerable<PROGRESS_ITEM> Progresses => PROGRESS_ITEMS;
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
            remaining_productivity = null;
            deliverable_milestones = null;
            base.Update();
        }

        //because entity goes through repository.reload() process this will be null since it's not meddled in the query
        public virtual bool ShouldSaveProgress => get_actual_total_earned_percentage(true) != set_total_earned_percentage;

        public virtual decimal? get_actual_total_earned_percentage(bool can_return_null = false)
        {
            //this happens during undo when first PROGRESS_ITEM is created in the same session
            if (Earned_Units_OnDataDate == 0 && PROGRESS_ITEM_Current == null && can_return_null)
                return null;

            IDeliverable deliverable = Entity as IDeliverable;

            if (deliverable != null && deliverable.IsByDuration)
                return Earned_Units_ToDate / BluePrintsConstants.DurationBasedTotalUnits;
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

        public decimal CurrentUnits
        {
            get
            {
                if (Stats == null || Stats.Current == null || Stats.Current.CurrentPeriodCumulativeDataPoint == null)
                    return 0;

                return Stats.Current.CurrentPeriodCumulativeDataPoint.Units;
            }
        }

        public decimal ScheduleCurrentPeriodPercentage
        {
            get
            {
                if (Stats == null || Stats.Budgeted == null || Stats.Budgeted.CurrentPeriodDataPoint == null)
                    return 0;

                return Stats.Budgeted.CurrentPeriodDataPoint.UnitsPercentage;
            }
        }

        public decimal Schedule_Remaining_Units
        {
            get
            {
                if (Stats == null || Stats.Budgeted == null || Stats.Budgeted.CurrentPeriodCumulativeDataPoint == null)
                    return 0;

                return Stats.Remaining.CumulativeDataPoints.Last().Units;
            }
        }

        public decimal MinPercentage => Total_Units == 0 ? 1 : (Earned_Units_BeforeDataDate / Total_Units);

        public virtual decimal MaxPercentage => Total_Units == 0 ? 1 : ((Total_Units - Earned_Units_AfterDataDate) / Total_Units);

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

        public virtual decimal Earned_Costs_OnDataDate => Earned_Units_OnDataDate * Entity.Budget_ItemRate;

        public virtual decimal Earned_Units_ToDate => Earned_Units_BeforeDataDate + Earned_Units_OnDataDate;

        public decimal Earned_Units_Total => Earned_Units_ToDate + Earned_Units_AfterDataDate;

        public virtual decimal Earned_Costs_Total => Earned_Units_Total * Entity.Budget_ItemRate;

        public decimal Earned_Costs_ToDate => Earned_Units_ToDate * Entity.Budget_ItemRate;
        
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

        public decimal Current_Productivity => CurrentUnits == 0 ? 1 : Earned_Units_ToDate / CurrentUnits;

        decimal? remaining_productivity { get; set; }
        public decimal? Remaining_Productivity
        {
            get
            {
                if (Stats == null)
                    return 0;

                if(remaining_productivity == null)
                    remaining_productivity = Stats.RemainingProductivity <= 0 || Stats.BaselineProductivity <= 0 ? (decimal?)null : Stats.RemainingProductivity / Stats.BaselineProductivity;

                return remaining_productivity;
            }
        }

        protected decimal? set_override_productivity;
        public virtual decimal? Override_Productivity
        {
            get
            {
                if (set_override_productivity == null)
                    set_override_productivity = get_db_or_current_productivity();

                return set_override_productivity;
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

        public string Variation_Code => string.Empty;

        public decimal Variation_Units => ApprovedVariations == null ? 0 : ApprovedVariations.Sum(x => x.AdjustmentUnits);

        public string Discipline_Code => Entity.Discipline_Code;

        public string Deliverable_Name => Entity.Deliverable_Name;

        public Guid? Subjob_Guid => Entity.Subjob_Guid;

        public Guid OriginalEntityKey => Entity.OriginalEntityKey;

        public decimal Budget_ItemRate => Entity.Budget_ItemRate;

        public decimal Budget_Costs => Entity.Budget_Costs;

        public decimal Variation_Costs => Variation_Units * Entity.Budget_ItemRate;

        public decimal Total_Costs => Budget_Costs + Variation_Costs;

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

        public bool IHaveMilestones
        {
            get
            {
                if (Milestones == null || Milestones.Count() == 0)
                    return false;

                return true;
                //if (P6_Assignments == null || P6TASKCollection == null)
                //    return false;

                ////when deliverable exceeds this percentage the milestones won't be displayed
                //    decimal minAssignmentPercentage = 0;
                //if (this.Stats != null && this.Stats.Earned != null && this.Stats.Earned.CurrentPeriodCumulativeDataPoint != null)
                //    minAssignmentPercentage = this.Stats.Earned.CurrentPeriodCumulativeDataPoint.UnitsPercentage;

                //return P6_Assignments.Where(x => x.HIGH_VALUE > minAssignmentPercentage).Count() > 0;
            }
        }

        public bool IAmCritical
        {
            get
            {
                //overdue will override current color
                if (IAmOverdue)
                    return false;

                if (Milestones == null || Milestones.Count() == 0)
                    return false;

                Deliverable_Milestone nextCriticalMilestone = deliverable_milestones.FirstOrDefault(x => x.DueDate <= ReportingDataDate.AddDays(7));
                if (nextCriticalMilestone != null)
                    return true;

                return false;
            }
        }

        public bool IAmOverdue
        {
            get
            {
                if (Milestones == null || Milestones.Count() == 0)
                    return false;

                Deliverable_Milestone nextCriticalMilestone = deliverable_milestones.FirstOrDefault(x => x.DueDate <= ReportingDataDate);
                if (nextCriticalMilestone != null)
                    return true;

                return false;
            }
        }

        public IEnumerable<P6Data.TASK> P6TASKCollection { get; set; }

        List<Deliverable_Milestone> deliverable_milestones;
        public IEnumerable<Deliverable_Milestone> Milestones
        {
            get
            {
                if(deliverable_milestones == null)
                {
                    deliverable_milestones = new List<Deliverable_Milestone>();
                    if (P6_Assignments == null || P6TASKCollection == null)
                        return new List<Deliverable_Milestone>();

                    //when deliverable exceeds this percentage the milestones won't be displayed
                    decimal minAssignmentPercentage = 0;
                    minAssignmentPercentage = Total_Percentage_ToDate;

                    foreach (P6_ASSIGNMENT p6assignment in P6_Assignments.Where(x => x.HIGH_VALUE > minAssignmentPercentage).OrderBy(x => x.HIGH_VALUE))
                    {
                        P6Data.TASK P6TASK = P6TASKCollection.FirstOrDefault(x => x.task_code == p6assignment.P6_ACTIVITYID);
                        if (P6TASK != null && P6TASK.early_end_date != null)
                        {
                            Deliverable_Milestone milestone = new Deliverable_Milestone();
                            milestone.Milestone = P6TASK.task_name;
                            milestone.Percentage = p6assignment.HIGH_VALUE;
                            milestone.DueDate = (DateTime)P6TASK.early_end_date;
                            deliverable_milestones.Add(milestone);
                        }
                    }

                    return deliverable_milestones;
                }
                else
                {
                    return deliverable_milestones;
                }
            }
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

        public decimal MinEstimateUnits => Earned_Units_Total - Variation_Units < 0 ? 0 : Earned_Units_Total - Variation_Units;

        public string Subjob_Name => Entity.Subjob_Name;

        public string Department_Code => Entity.Department_Code;

        public virtual decimal P6_Assignment_Total_Quantity => Total_Units;

        public virtual string P6_Assignment_UOM => "Hrs";

        public Guid? Phase_Guid { get => Entity.Phase_Guid; set => Entity.Phase_Guid = value; }

        Guid? IDeliverable.Subjob_Guid { get => Entity.Subjob_Guid; set => Entity.Subjob_Guid = value; }

        public string P6AssignmentName => Entity.Deliverable_Name;

        public Guid? Discipline_Guid => Entity.Discipline_Guid;

        public decimal Discipline_Number => Entity.Discipline_Number;

        public Guid? Workpack_Guid { get => Entity.Workpack_Guid; set => Entity.Workpack_Guid = value; }

        public Guid? P6_WorkpackGuid => Workpack_Guid;

        public string P6AssignmentDescription => string.Empty;

        public string P6AssignmentDescription2 => string.Empty;

        public PhaseType? Phase => Entity.Phase;

        public ChargeType? Charge => Entity.Charge;

        public IEnumerable<User_Weight> AssignedUsers => Entity.AssignedUsers;

        public Guid DeliverableKey => Entity.GUID;

        public decimal Budget_Quantity => Entity.Budget_Quantity;

        public decimal Total_Quantity => Entity.Total_Quantity;

        public string Project_Number => Entity.Project_Number;
    }

    public class DeliverableEarnedPercentages
    {
        public decimal EarnedPercentage { get; set; }
        public DateTime EarnedDate { get; set; }
    }
}