using BaseModel.Misc;
using BluePrints.Common.Base;
using BluePrints.Data;
using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BluePrints.Common.Projections;
using BaseModel.DataModel;

namespace BluePrints.Common.ViewModel.Reporting
{
    public class ReportablesDisplay : EntityBase, ICanAssignP6, IGuidEntityKey, IReportable, IBookable
    {
        public Guid GUID { get => ProgressItem.GUID; set => ProgressItem.GUID = value; }
        public DisplayQuantityReportable ProgressItem { get; set; }

        public IEnumerable<DisplayQuantityReportable> Reportables
        {
            get
            {
                if (isSetNull)
                    return null;

                DisplayQuantityReportableGroup reportable = ProgressItem as DisplayQuantityReportableGroup;
                if (reportable != null)
                    return reportable.ChildReportables;

                return null;
            }
        }

        public bool IsExpandable
        {
            get
            {
                DisplayQuantityReportableGroup reportable = ProgressItem as DisplayQuantityReportableGroup;
                return reportable != null;
            }
        }

        public string Name
        {
            get
            {
                STOCK_GROUPProgress stockGroup = ProgressItem.Reportable as STOCK_GROUPProgress;
                if (stockGroup != null && stockGroup.Entity.TrackableEstimateItem != null)
                    return stockGroup.Entity.TrackableEstimateItem.Entity.NAME;
                else
                {
                    IEstimateItem estimate = ProgressItem.Reportable as IEstimateItem;
                    if (estimate != null)
                        return estimate.ReadOnlyEstimate.Entity.Entity.NAME;
                    else
                        return string.Empty;
                }
            }
        }

        public string Comments
        {
            get
            {
                STOCK_GROUPProgress stockGroup = ProgressItem.Reportable as STOCK_GROUPProgress;
                if (stockGroup != null && stockGroup.Entity.TrackableEstimateItem != null)
                    return stockGroup.Entity.TrackableEstimateItem.Entity.COMMENTS;
                else
                {
                    IEstimateItem estimate = ProgressItem.Reportable as IEstimateItem;
                    if (estimate != null)
                        return estimate.ReadOnlyEstimate.Entity.Entity.COMMENTS;
                    else
                        return string.Empty;
                }
            }
        }
        
        public string Description
        {
            get
            {
                STOCK_GROUPProgress stockGroup = ProgressItem.Reportable as STOCK_GROUPProgress;
                if (stockGroup != null && stockGroup.Entity.TrackableEstimateItem != null)
                    return stockGroup.Entity.TrackableEstimateItem.Entity.DESCRIPTION;
                else
                {
                    IEstimateItem estimate = ProgressItem.Reportable as IEstimateItem;
                    if (estimate != null)
                        return estimate.ReadOnlyEstimate.Entity.Entity.DESCRIPTION;
                    else
                        return string.Empty;
                }
            }
        }

        public string SeqNo
        {
            get
            {
                STOCK_GROUPProgress stockGroup = ProgressItem.Reportable as STOCK_GROUPProgress;
                if (stockGroup != null && stockGroup.Entity.TrackableEstimateItem != null)
                    return stockGroup.Entity.TrackableEstimateItem.Entity.SEQNO;
                else
                {
                    IEstimateItem estimate = ProgressItem.Reportable as IEstimateItem;
                    if (estimate != null)
                        return estimate.ReadOnlyEstimate.Entity.Entity.SEQNO;
                    else
                        return string.Empty;
                }
            }
        }

        public SingleObjectSummarizer StatSummarizer => ((IReportable)ProgressItem).StatSummarizer;

        public string Discipline_Code => ((IReportable)ProgressItem).Discipline_Code;

        public string Deliverable_Name => ((IReportable)ProgressItem).Deliverable_Name;

        public Guid? Subjob_Guid => ((IReportable)ProgressItem).Subjob_Guid;

        public Guid OriginalEntityKey => ((IReportable)ProgressItem).OriginalEntityKey;

        public string Phase_Code => ((IReportable)ProgressItem).Phase_Code;

        public string Commodity_Code => ((IReportable)ProgressItem).Commodity_Code;

        public Guid? Area_Guid => ((IReportable)ProgressItem).Area_Guid;

        public Guid? SubArea_Guid => ((IReportable)ProgressItem).SubArea_Guid;

        public decimal Budget_Units => ((IReportable)ProgressItem).Budget_Units;

        public decimal Total_Units => ((IReportable)ProgressItem).Total_Units;

        public decimal Budget_ItemRate => ((IReportable)ProgressItem).Budget_ItemRate;

        public decimal Budget_Costs => ((IReportable)ProgressItem).Budget_Costs;

        public decimal Variation_Costs => ((IReportable)ProgressItem).Variation_Costs;

        public decimal Total_Costs => ((IReportable)ProgressItem).Total_Costs;

        public ProgressStats Stats { get => ((IReportable)ProgressItem).Stats; set => ((IReportable)ProgressItem).Stats = value; }

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_BeforeDataDate => ((IReportable)ProgressItem).PROGRESS_ITEM_BeforeDataDate;

        public PROGRESS_ITEM PROGRESS_ITEM_Current => ((IReportable)ProgressItem).PROGRESS_ITEM_Current;

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_UpToCurrentDataDate => ((IReportable)ProgressItem).PROGRESS_ITEM_UpToCurrentDataDate;

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_AfterDataDate => ((IReportable)ProgressItem).PROGRESS_ITEM_AfterDataDate;

        public DateTime ReportingDataDate => ((IReportable)ProgressItem).ReportingDataDate;

        public List<PROGRESS_ITEM> PROGRESS_ITEMS => ((IReportable)ProgressItem).PROGRESS_ITEMS;

        public decimal Earned_Units_Total => ((IReportable)ProgressItem).Earned_Units_Total;

        public decimal Earned_Costs_Total => ((IReportable)ProgressItem).Earned_Costs_Total;

        public decimal Earned_Units_BeforeDataDate => ((IReportable)ProgressItem).Earned_Units_BeforeDataDate;

        public decimal Earned_Units_OnDataDate => ((IReportable)ProgressItem).Earned_Units_OnDataDate;

        public decimal Earned_Units_ToDate => ((IReportable)ProgressItem).Earned_Units_ToDate;

        public decimal Earned_Costs_ToDate => ((IReportable)ProgressItem).Earned_Costs_ToDate;

        public decimal Earned_Units_AfterDataDate => ((IReportable)ProgressItem).Earned_Units_AfterDataDate;

        public decimal Total_Earned_Percentage { get => ((IReportable)ProgressItem).Total_Earned_Percentage; set => ((IReportable)ProgressItem).Total_Earned_Percentage = value; }

        public decimal ProgressETC { get => ((IReportable)ProgressItem).ProgressETC; set => ((IReportable)ProgressItem).ProgressETC = value; }

        public decimal Total_Percentage => ((IReportable)ProgressItem).Total_Percentage;

        public decimal Total_Percentage_ToDate => ((IReportable)ProgressItem).Total_Percentage_ToDate;

        public decimal Baseline_Percentage => ((IReportable)ProgressItem).Baseline_Percentage;

        public decimal SchedulePercentage => ((IReportable)ProgressItem).SchedulePercentage;

        public decimal MinPercentage => ((IReportable)ProgressItem).MinPercentage;

        public decimal MaxPercentage => ((IReportable)ProgressItem).MaxPercentage;

        public bool ShouldSaveProgress => ((IReportable)ProgressItem).ShouldSaveProgress;

        public bool ShouldSaveProgressETC => ((IReportable)ProgressItem).ShouldSaveProgressETC;

        public decimal Current_Productivity => ((IReportable)ProgressItem).Current_Productivity;

        public decimal? Override_Productivity { get => ((IReportable)ProgressItem).Override_Productivity; set => ((IReportable)ProgressItem).Override_Productivity = value; }

        public decimal MinEstimateUnits => ((IReportable)ProgressItem).MinEstimateUnits;

        public decimal? Remaining_Productivity => ((IReportable)ProgressItem).Remaining_Productivity;

        public string Subjob_Name => ((IReportable)ProgressItem).Subjob_Name;

        public string Department_Code => ((IReportable)ProgressItem).Department_Code;

        public Guid? Department_Guid => ((IReportable)ProgressItem).Department_Guid;

        public decimal ScheduleCurrentPeriodPercentage => ((IReportable)ProgressItem).ScheduleCurrentPeriodPercentage;

        public Guid? Phase_Guid { get => ((IReportable)ProgressItem).Phase_Guid; set => ((IReportable)ProgressItem).Phase_Guid = value; }

        Guid? IDeliverable.Subjob_Guid { get => ((IReportable)ProgressItem).Subjob_Guid; set => ((IReportable)ProgressItem).Subjob_Guid = value; }

        public decimal Earned_Costs_OnDataDate => ((IReportable)ProgressItem).Earned_Costs_OnDataDate;

        public Guid? Discipline_Guid => ((IReportable)ProgressItem).Discipline_Guid;

        public decimal Discipline_Number => ((IReportable)ProgressItem).Discipline_Number;

        public Guid? Workpack_Guid { get => ((IReportable)ProgressItem).Workpack_Guid; set => ((IReportable)ProgressItem).Workpack_Guid = value; }

        public PhaseType? Phase => ((IReportable)ProgressItem).Phase;

        public ChargeType? Charge => ((IReportable)ProgressItem).Charge;

        public IEnumerable<User_Weight> AssignedUsers => ((IReportable)ProgressItem).AssignedUsers;

        public List<P6_ASSIGNMENT> P6_Assignments => throw new NotImplementedException();

        public IEnumerable<PROGRESS_ITEM> Progresses => throw new NotImplementedException();

        public Guid DeliverableKey => ((IReportable)ProgressItem).GUID;

        public bool IsByDuration { get => ((IReportable)ProgressItem).IsByDuration; set => ((IReportable)ProgressItem).IsByDuration = value; }

        public DateTime? TaskAssignmentStartDate { get; set; }
        public decimal EarnedUnitsAccountedFor { get; set; }

        public decimal Budget_Quantity => ((IReportable)ProgressItem).Budget_Quantity;

        public decimal Total_Quantity => ((IReportable)ProgressItem).Total_Quantity;

        public string Project_Number => ((IReportable)ProgressItem).Project_Number;

        public decimal Variation_Units => ((IReportable)ProgressItem).Variation_Units;

        public string Variation_Code => ((IReportable)ProgressItem).Variation_Code;

        public bool CanBook
        {
            get
            {
                IBookable bookableProjection = ProgressItem as IBookable;
                if (bookableProjection != null)
                    return bookableProjection.CanBook;

                return false;
            }
            set
            {
                IBookable bookableProjection = ProgressItem as IBookable;
                if (bookableProjection != null)
                    bookableProjection.CanBook = value;

            }
        }

        public decimal Budget_Adjustment_Units => ((IReportable)ProgressItem).Budget_Adjustment_Units;

        public decimal Budget_Adjustment_Costs => ((IReportable)ProgressItem).Budget_Adjustment_Costs;

        public decimal Budget_ItemInternalRate => ((IReportable)ProgressItem).Budget_ItemInternalRate;

        public decimal Budget_InternalCost => ((IReportable)ProgressItem).Budget_InternalCost;

        public decimal Variation_InternalCosts => ((IReportable)ProgressItem).Variation_InternalCosts;

        public decimal Total_InternalCosts => ((IReportable)ProgressItem).Total_InternalCosts;

        public decimal Unadjusted_Budget_Units => ((IReportable)ProgressItem).Unadjusted_Budget_Units;

        public List<VariationAdjustment> ApprovedVariations => ((IReportable)ProgressItem).ApprovedVariations;

        public string P6AssignmentName => throw new NotImplementedException();

        public string P6AssignmentDescription => throw new NotImplementedException();

        public decimal Assigned_Percentage => throw new NotImplementedException();

        public decimal Remaining_Percentage => throw new NotImplementedException();

        public decimal P6_Assignment_Total_Quantity => throw new NotImplementedException();

        public string P6_Assignment_UOM => throw new NotImplementedException();

        public Guid? P6_WorkpackGuid => throw new NotImplementedException();

        public string P6AssignmentDescription2 => throw new NotImplementedException();

        public override void Update()
        {
            ProgressItem.Update();
            RefreshChild();
        }

        bool isSetNull;
        private void RefreshChild()
        {
            isSetNull = true;
            DisplayQuantityReportableGroup reportable = ProgressItem as DisplayQuantityReportableGroup;
            if (reportable != null)
            {
                RaisePropertyChanged(() => Reportables);
                isSetNull = false;
                RaisePropertyChanged(() => Reportables);
            }

            RaisePropertyChanged(() => ProgressItem);
        }

        public void SetOriginalEntityKey(Guid newGuid)
        {
            ((IReportable)ProgressItem).SetOriginalEntityKey(newGuid);
        }

        public IEnumerable<PROGRESS_ITEM> GetExistingOrNewEditedProgresses(Func<Expression<Func<PROGRESS_ITEM, bool>>, PROGRESS_ITEM> repository_find_actual_func)
        {
            return ((IReportable)ProgressItem).GetExistingOrNewEditedProgresses(repository_find_actual_func);
        }

        public IEnumerable<PROGRESS_ETC> GetExistingOrNewEditedProgressETCs(Func<Expression<Func<PROGRESS_ETC, bool>>, PROGRESS_ETC> repository_find_actual_func)
        {
            return ((IReportable)ProgressItem).GetExistingOrNewEditedProgressETCs(repository_find_actual_func);
        }

        public void SetReportingDataDate(DateTime dataDate)
        {
            ((IReportable)ProgressItem).SetReportingDataDate(dataDate);
        }

        public void SetProgressItems(List<PROGRESS_ITEM> progresses)
        {
            ((IReportable)ProgressItem).SetProgressItems(progresses);
        }

        public void SetProgressETCs(List<PROGRESS_ETC> progresETCs)
        {
            ((IReportable)ProgressItem).SetProgressETCs(progresETCs);
        }

        public void AppendProgressItem(PROGRESS_ITEM currentProgress)
        {
            ((IReportable)ProgressItem).AppendProgressItem(currentProgress);
        }

        public void BuildStats(decimal weightingPortion = 1, List<StatsCalculationType> calcTypes = null)
        {
        }
    }

    public class DisplayQuantityReportableGroup : DisplayQuantityReportable, IReportable_Group
    {
        public IEnumerable<DisplayQuantityReportable> ChildReportables;
        public DisplayQuantityReportableGroup(IReportable_Quantity_Group reportableGroup)
            : base(reportableGroup, false)
        {
            this.ChildReportables = reportableGroup.Reportables.Select(x => new DisplayQuantityReportable(x, true));
        }

        public IEnumerable<IReportable> Reportables => ChildReportables;

        public override void Update()
        {
            foreach (DisplayQuantityReportable child_reportable in ChildReportables)
                child_reportable.Update();

            base.Update();
        }
    }

    public class DisplayQuantityReportable : EntityBase, IReportable_Quantity, IEstimateItem
    {
        public IReportable_Quantity Reportable { get; }
        public int ColorIndex { get; private set; }

        //For bindableBase property name usage only
        public DisplayQuantityReportable()
        {

        }

        public DisplayQuantityReportable(IReportable_Quantity deliverable, bool is_nested)
        {
            this.Reportable = deliverable;
            if(is_nested)
            {
                if (deliverable.Progress_Type == EstimateProgressType.Trackable)
                    ColorIndex = 1;
                else
                    ColorIndex = 2;
            }
        }

        public string Deliverable_Name => Reportable.Deliverable_Name;

        public string Phase_Code => Reportable.Phase_Code;

        public string Commodity_Code => Reportable.Commodity_Code;

        public Guid? Subjob_Guid => Reportable.Subjob_Guid;

        public Guid OriginalEntityKey => Reportable.OriginalEntityKey;

        public void SetOriginalEntityKey(Guid newGuid) { }

        public Guid? Area_Guid => Reportable.Area_Guid;

        public Guid? SubArea_Guid => Reportable.SubArea_Guid;

        public decimal Budget_Units => Reportable.Budget_Units;

        public decimal Total_Units => Reportable.Total_Units;

        public decimal Budget_ItemRate => Reportable.Budget_ItemRate;

        public decimal Budget_Costs => Reportable.Budget_Costs;

        public decimal TotalCosts => Reportable.Total_Costs;

        public EstimateProgressType Progress_Type => Reportable.Progress_Type;

        public decimal Budget_Quantity => Reportable.Budget_Quantity;

        public decimal Trackable_Total_Quantity
        {
            get
            {
                IReportable_Quantity_Group reportable_group = Reportable as IReportable_Quantity_Group;
                if (reportable_group != null)
                    return reportable_group.Trackable_Total_Quantity;
                else
                    return Reportable.Total_Quantity;
            }
        }

        public decimal Total_Quantity => Reportable.Total_Quantity;

        public string Estimate_UOM => Reportable.Estimate_UOM;

        public Guid GUID { get => Reportable.GUID; set => Reportable.GUID = value; }

        public decimal QuantityPerUnit
        {
            get
            {
                IReportable_Quantity_Group reportable_group = Reportable as IReportable_Quantity_Group;
                if (reportable_group != null)
                    return reportable_group.Trackable_QuantityPerUnit;
                else
                    return Reportable.QuantityPerUnit;
            }
        }

        public decimal UnitsPerQuantity => Reportable.UnitsPerQuantity;

        public decimal PastInstalledQuantity => Reportable.PastInstalledQuantity;

        public decimal FutureInstalledQuantity => Reportable.FutureInstalledQuantity;

        public DateTime ReportingDataDate => Reportable.ReportingDataDate;

        public List<PROGRESS_ITEM> PROGRESS_ITEMS => Reportable.PROGRESS_ITEMS;

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_BeforeDataDate => Reportable.PROGRESS_ITEM_BeforeDataDate;

        public PROGRESS_ITEM PROGRESS_ITEM_Current => Reportable.PROGRESS_ITEM_Current;

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_UpToCurrentDataDate => Reportable.PROGRESS_ITEM_UpToCurrentDataDate;

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_AfterDataDate => Reportable.PROGRESS_ITEM_AfterDataDate;

        public ProgressStats Stats { get => Reportable.Stats; set => Reportable.Stats = value; }

        public string Variation_Code => Reportable.Variation_Code;

        public decimal Variation_Units => Reportable.Variation_Units;

        public decimal Total_Percentage => Reportable.Total_Percentage;

        public string Discipline_Code => Reportable.Discipline_Code;

        public decimal Variation_Costs => Reportable.Variation_Costs;

        public decimal Total_Costs => Reportable.Total_Costs;

        public decimal Earned_Units_Total => Reportable.Earned_Units_Total;

        public decimal Earned_Costs_Total => Reportable.Earned_Costs_Total;

        public decimal Earned_Units_BeforeDataDate => Reportable.Earned_Units_BeforeDataDate;

        public decimal Earned_Units_OnDataDate => Reportable.Earned_Units_OnDataDate;

        public decimal Earned_Costs_OnDataDate => Reportable.Earned_Costs_OnDataDate;

        public decimal Earned_Units_ToDate => Reportable.Earned_Units_ToDate;

        public decimal Earned_Costs_ToDate => Reportable.Earned_Costs_ToDate;

        public decimal Earned_Install_Costs_OnDataDate => Reportable.Earned_Install_Costs_OnDataDate;

        public decimal Earned_Supply_Costs_OnDataDate => Reportable.Earned_Supply_Costs_OnDataDate;

        public decimal Earned_Total_Costs_OnDataDate => Reportable.Earned_Total_Costs_OnDataDate;

        public decimal Earned_Units_AfterDataDate => Reportable.Earned_Units_AfterDataDate;

        public decimal Total_Earned_Percentage { get => Reportable.Total_Earned_Percentage; set => Reportable.Total_Earned_Percentage = value; }

        public decimal ProgressETC { get => Reportable.ProgressETC; set => Reportable.ProgressETC = value; }

        public decimal Total_Percentage_ToDate => Reportable.Total_Percentage_ToDate;

        public decimal TotalInstalledQuantity => Reportable.TotalInstalledQuantity;

        public decimal AbsoluteTotalInstalledQuantity => Reportable.AbsoluteTotalInstalledQuantity;

        public decimal Trackable_Installed_Quantity => Reportable.Trackable_Installed_Quantity;

        public decimal Baseline_Percentage => Reportable.Baseline_Percentage;

        public decimal SchedulePercentage => Reportable.SchedulePercentage;

        public decimal MinPercentage => Reportable.MinPercentage;

        public decimal MaxPercentage => Reportable.MaxPercentage;

        public decimal CurrentPeriodInstalledQuantity { get => Reportable.CurrentPeriodInstalledQuantity; set => Reportable.CurrentPeriodInstalledQuantity = value; }

        public decimal MaxCurrentQuantity => Reportable.MaxCurrentQuantity;

        public bool ShouldSaveProgress => Reportable.ShouldSaveProgress;

        public bool ShouldSaveProgressETC => Reportable.ShouldSaveProgressETC;

        public decimal Remaining_Hours_To_Completion => Reportable.Remaining_Hours_To_Completion;

        public decimal Current_Productivity => Reportable.Current_Productivity;

        public decimal? Override_Productivity { get => Reportable.Override_Productivity; set => Reportable.Override_Productivity = value; }

        public decimal Estimate_Stock_Code_Supply_Rate => Reportable.Estimate_Stock_Code_Supply_Rate;

        public decimal Estimate_Stock_Code_Install_Hours => Reportable.Estimate_Stock_Code_Install_Hours;

        public decimal Total_Estimate_Cost => Reportable.Total_Estimate_Cost;

        public SingleObjectSummarizer StatSummarizer => Reportable.StatSummarizer;

        public decimal MinEstimateQuantity => Reportable.MinEstimateQuantity;

        public decimal Variation_Quantity => Reportable.Variation_Quantity;

        public decimal MinEstimateUnits => Reportable.MinEstimateUnits;

        public Guid? Stock_Group_Guid => Reportable.Stock_Group_Guid;

        public decimal? Remaining_Productivity => Reportable.Remaining_Productivity;

        public decimal Budget_FreightRate => Reportable.Budget_FreightRate;

        public decimal Estimate_Install_Cost => Reportable.Estimate_Install_Cost;

        public decimal Variation_Install_Cost => Reportable.Variation_Install_Cost;

        public decimal Estimate_Freight_Cost => Reportable.Estimate_Freight_Cost;

        public decimal Variation_Freight_Cost => Reportable.Variation_Freight_Cost;

        public decimal Estimate_Install_Hours => Reportable.Estimate_Install_Hours;

        public decimal Variation_Install_Hours => Reportable.Variation_Install_Hours;

        public decimal Estimate_Supply_Cost => Reportable.Estimate_Supply_Cost;

        public decimal Variation_Supply_Cost => Reportable.Variation_Supply_Cost;

        public string Subjob_Name => Reportable.Subjob_Name;

        public string Department_Code => Reportable.Department_Code;

        public Guid? Department_Guid => Reportable.Department_Guid;

        public decimal Schedule_Estimated_Quantity => Reportable.Schedule_Estimated_Quantity;

        public decimal Schedule_Estimated_Current_Period_Quantity => Reportable.Schedule_Estimated_Current_Period_Quantity;

        public decimal ScheduleCurrentPeriodPercentage => Reportable.ScheduleCurrentPeriodPercentage;

        public Guid? Phase_Guid { get => Reportable.Phase_Guid; set => Reportable.Phase_Guid = value; }

        Guid? IDeliverable.Subjob_Guid { get => Reportable.Subjob_Guid; set => Reportable.Subjob_Guid = value; }

        public Guid? Discipline_Guid => Reportable.Discipline_Guid;

        public decimal Discipline_Number => Reportable.Discipline_Number;

        public Guid? Workpack_Guid { get => Reportable.Workpack_Guid; set => Reportable.Workpack_Guid = value; }

        public decimal Estimate_Quantity => Reportable.Estimate_Quantity;

        public decimal Budget_Install_Hours => Reportable.Budget_Install_Hours;

        public decimal Budget_Install_Cost => Reportable.Budget_Install_Cost;

        public decimal Total_Budget_Install_Cost => Reportable.Total_Budget_Install_Cost;

        public decimal Total_Budget_Freight_Cost => Reportable.Total_Budget_Freight_Cost;

        public decimal Total_Budget_Supply_Cost => Reportable.Total_Budget_Supply_Cost;

        public decimal Total_Budget_Cost => Reportable.Total_Budget_Cost;

        public decimal Budget_Freight_Cost => Reportable.Budget_Freight_Cost;

        public decimal Estimate_Units => Reportable.Estimate_Units;

        public decimal Estimate_ItemRate => Reportable.Estimate_ItemRate;

        public decimal Estimate_FreightRate => Reportable.Estimate_FreightRate;

        public decimal Budget_Stock_Code_Install_Hours => Reportable.Budget_Stock_Code_Install_Hours;

        public decimal Budget_Stock_Code_Supply_Rate => Reportable.Budget_Stock_Code_Supply_Rate;

        public decimal Budget_Supply_Cost => Reportable.Budget_Supply_Cost;

        public string Budget_UOM => Reportable.Budget_UOM;

        public PhaseType? Phase => Reportable.Phase;

        public IEnumerable<User_Weight> AssignedUsers => Reportable.AssignedUsers;

        public ESTIMATE_ITEMProgress ReadOnlyEstimate => Reportable as ESTIMATE_ITEMProgress;

        public bool IsByDuration { get => Reportable.IsByDuration; set => Reportable.IsByDuration = value; }

        public ChargeType? Charge => Reportable.Charge;

        public string Project_Number => Reportable.Project_Number;

        public decimal Budget_Adjustment_Units => Reportable.Budget_Adjustment_Units;

        public decimal Budget_Adjustment_Costs => Reportable.Budget_Adjustment_Costs;

        public decimal Budget_ItemInternalRate => Reportable.Budget_ItemInternalRate;

        public decimal Budget_InternalCost => Reportable.Budget_InternalCost;

        public decimal Variation_InternalCosts => Reportable.Variation_InternalCosts;

        public decimal Total_InternalCosts => Reportable.Total_InternalCosts;

        public decimal Unadjusted_Budget_Units => Reportable.Unadjusted_Budget_Units;

        public List<VariationAdjustment> ApprovedVariations => Reportable.ApprovedVariations;

        public void SetReportingDataDate(DateTime dataDate)
        {
            Reportable.SetReportingDataDate(dataDate);
        }

        public void SetProgressItems(List<PROGRESS_ITEM> progresses)
        {
            Reportable.SetProgressItems(progresses);
        }

        public void SetProgressETCs(List<PROGRESS_ETC> progresETCs)
        {
            Reportable.SetProgressETCs(progresETCs);
        }

        public void AppendProgressItem(PROGRESS_ITEM currentProgress)
        {
            Reportable.AppendProgressItem(currentProgress);
        }

        public override void Update()
        {
            Reportable.Update();
        }

        public decimal getCurrentPeriodEarnedUnits(decimal newPercentage)
        {
            return Reportable.getCurrentPeriodEarnedUnits(newPercentage);
        }

        public IEnumerable<PROGRESS_ITEM> GetExistingOrNewEditedProgresses(Func<Expression<Func<PROGRESS_ITEM, bool>>, PROGRESS_ITEM> repository_find_actual_func)
        {
            return Reportable.GetExistingOrNewEditedProgresses(repository_find_actual_func);
        }

        public IEnumerable<PROGRESS_ETC> GetExistingOrNewEditedProgressETCs(Func<Expression<Func<PROGRESS_ETC, bool>>, PROGRESS_ETC> repository_find_actual_func)
        {
            return Reportable.GetExistingOrNewEditedProgressETCs(repository_find_actual_func);
        }
    }
}
