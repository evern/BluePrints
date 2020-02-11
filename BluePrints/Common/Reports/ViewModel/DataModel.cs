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
    public abstract class BluePrintsProgressableProjectionBase<TEntity> : BluePrintsProjectionBase<TEntity>, IReportable, ICanSetProgresses, ICanAssignP6
        where TEntity : class, IDeliverable_Rates, IHaveCosts, new()
    {
        #region Stats Parameters
        readonly SingleObjectSummarizer statsSummarizer;
        public SingleObjectSummarizer StatSummarizer => statsSummarizer;
        public ProgressStats Stats { get; set; }
        public List<VariationAdjustment> ApprovedVariations { get; set; }
        public BluePrintsProgressableProjectionBase()
        {
            //Initialization without stats
        }

        public BluePrintsProgressableProjectionBase(PROJECT PROJECT, PROGRESS Live_PROGRESS, IDeliverable_Rates entity, IEnumerable<VariationAdjustment> variation_adjustments, bool useReportDate, DateTime? extrapolateDate = null, bool forceRetrieveRemainingDataPoints = false)
        {
            this.Live_PROGRESS = Live_PROGRESS;
            //DateTime reporting_data_date = Live_PROGRESS.DATA_DATE;
            DateTime reporting_data_date = useReportDate ? Live_PROGRESS.REPORT_DATE == null ? Live_PROGRESS.DATA_DATE : (DateTime)Live_PROGRESS.REPORT_DATE : Live_PROGRESS.DATA_DATE;
            TimeSpan reporting_interval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(Live_PROGRESS);
            DateTime first_aligned_data_date = ChronologicalHelpers.GenerateFirstAlignedDataDate(Live_PROGRESS);
            SetReportingDataDate(reporting_data_date);
            ApprovedVariations = variation_adjustments.Where(x => x.DeliverableOriginalGuid == entity.OriginalEntityKey).ToList();
            decimal variationUnits = ApprovedVariations.Sum(x => x.AdjustmentUnits);
            decimal totalUnits = entity.Budget_Units + variationUnits;
            decimal costsPerUnit = entity.Budget_Costs == 0 ? 0 : entity.Budget_Units / entity.Budget_Costs;
            decimal totalCosts = totalUnits * costsPerUnit;

            PartialStatsBuilder partialStatsBuilder = new PartialStatsBuilder(PROJECT.CURRENCYCONVERSION);
            Stats = new ProgressStats(reporting_data_date, reporting_interval, first_aligned_data_date, entity.Budget_Units, totalUnits, entity.Budget_Costs, totalCosts, ApprovedVariations, extrapolateDate, forceRetrieveRemainingDataPoints);
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

        public virtual decimal Total_Units => Variation_Units + Budget_Units;

        public DateTime? TaskAssignmentStartDate { get; set; }

        public decimal EarnedUnitsAccountedFor { get; set; }

        public string Phase_Code => Entity.Phase_Code;

        public string Commodity_Code => Entity.Commodity_Code;

        public Guid? Area_Guid => Entity.Area_Guid;

        public Guid? SubArea_Guid => Entity.SubArea_Guid;

        public decimal Budget_Units => Entity.Budget_Units + Budget_Adjustment_Units;

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

        public string Variation_Code => Entity.Variation_Code;

        public decimal Variation_Units => ApprovedVariations == null ? 0 : ApprovedVariations.Where(x => !x.IsBudgetAdjustment).Sum(x => x.AdjustmentUnits);

        public decimal Budget_Adjustment_Units => ApprovedVariations == null ? 0 : ApprovedVariations.Where(x => x.IsBudgetAdjustment).Sum(x => x.AdjustmentUnits);

        public decimal Budget_Adjustment_Costs => Budget_Adjustment_Units * Entity.Budget_ItemRate;

        public string Discipline_Code => Entity.Discipline_Code;

        public string Deliverable_Name => Entity.Deliverable_Name;

        public Guid? Subjob_Guid { get => Entity.Subjob_Guid; set => Entity.Subjob_Guid = value; }

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
            savePROGRESS_ITEM.EARNED_DATE = Live_PROGRESS.DATA_DATE.Date.AddDays(1).AddSeconds(-1);
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

        public decimal MinEstimateUnits => Earned_Units_Total > Unadjusted_Budget_Units ? Unadjusted_Budget_Units : Earned_Units_Total - Variation_Units < 0 ? 0 : Earned_Units_Total - Variation_Units;

        public decimal Unadjusted_Budget_Units => Entity.Budget_Units;

        public string Subjob_Name => Entity.Subjob_Name;

        public string Department_Code => Entity.Department_Code;

        public virtual decimal P6_Assignment_Total_Quantity => Total_Units;

        public virtual string P6_Assignment_UOM => "Hrs";

        public Guid? Phase_Guid { get => Entity.Phase_Guid; set => Entity.Phase_Guid = value; }

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

        public string Project_Number => Entity.Project_Number;

        public decimal Budget_ItemInternalRate => Entity.Budget_ItemInternalRate;

        public decimal Budget_InternalCost => Entity.Budget_InternalCost;

        public decimal Variation_InternalCosts => Variation_Units * Budget_ItemInternalRate;

        public decimal Total_InternalCosts => Budget_InternalCost + Variation_InternalCosts;
    }

    public class DeliverableEarnedPercentages
    {
        public decimal EarnedPercentage { get; set; }
        public DateTime EarnedDate { get; set; }
    }
}