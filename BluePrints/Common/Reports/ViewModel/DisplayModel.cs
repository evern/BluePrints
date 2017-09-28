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

namespace BluePrints.Common.ViewModel.Reporting
{
    public class ReportablesDisplay : BluePrintsEntityBase, IGuidEntityKey, IReportable
    {
        public Guid GUID { get => ProgressItem.EntityKey; set => ProgressItem.EntityKey = value; }
        public Guid EntityKey { get => ProgressItem.EntityKey; set => ProgressItem.EntityKey = value; }
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

        public SingleObjectSummarizer StatSummarizer => ((IReportable)ProgressItem).StatSummarizer;

        public string Discipline_Code => ((IReportable)ProgressItem).Discipline_Code;

        public string Deliverable_Name => ((IReportable)ProgressItem).Deliverable_Name;

        public Guid? Workpack_Guid => ((IReportable)ProgressItem).Workpack_Guid;

        public Guid OriginalEntityKey => ((IReportable)ProgressItem).OriginalEntityKey;

        public string Phase_Code => ((IReportable)ProgressItem).Phase_Code;

        public string Commodity_Code => ((IReportable)ProgressItem).Commodity_Code;

        public Guid? Area_Guid => ((IReportable)ProgressItem).Area_Guid;

        public Guid? SubArea_Guid => ((IReportable)ProgressItem).SubArea_Guid;

        public decimal Estimated_Units => ((IReportable)ProgressItem).Estimated_Units;

        public decimal Total_Units => ((IReportable)ProgressItem).Total_Units;

        public decimal Variation_Units => ((IReportable)ProgressItem).Variation_Units;

        public decimal ItemRate => ((IReportable)ProgressItem).ItemRate;

        public decimal Estimated_Costs => ((IReportable)ProgressItem).Estimated_Costs;

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

        public decimal Total_Percentage => ((IReportable)ProgressItem).Total_Percentage;

        public decimal Total_Percentage_ToDate => ((IReportable)ProgressItem).Total_Percentage_ToDate;

        public decimal Baseline_Percentage => ((IReportable)ProgressItem).Baseline_Percentage;

        public decimal SchedulePercentage => ((IReportable)ProgressItem).SchedulePercentage;

        public decimal MinPercentage => ((IReportable)ProgressItem).MinPercentage;

        public decimal MaxPercentage => ((IReportable)ProgressItem).MaxPercentage;

        public bool ShouldSaveProgress => ((IReportable)ProgressItem).ShouldSaveProgress;

        public decimal Current_Productivity => ((IReportable)ProgressItem).Current_Productivity;

        public decimal? Override_Productivity { get => ((IReportable)ProgressItem).Override_Productivity; set => ((IReportable)ProgressItem).Override_Productivity = value; }

        public decimal MinEstimateUnits => ((IReportable)ProgressItem).MinEstimateUnits;

        public decimal? Remaining_Productivity => ((IReportable)ProgressItem).Remaining_Productivity;

        public string Workpack_Name => ((IReportable)ProgressItem).Workpack_Name;

        public string Department_Code => ((IReportable)ProgressItem).Department_Code;

        public decimal ScheduleCurrentPeriodPercentage => ((IReportable)ProgressItem).ScheduleCurrentPeriodPercentage;

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

        public void SetReportingDataDate(DateTime dataDate)
        {
            ((IReportable)ProgressItem).SetReportingDataDate(dataDate);
        }

        public void SetProgressItems(List<PROGRESS_ITEM> progresses)
        {
            ((IReportable)ProgressItem).SetProgressItems(progresses);
        }

        public void AppendProgressItem(PROGRESS_ITEM currentProgress)
        {
            ((IReportable)ProgressItem).AppendProgressItem(currentProgress);
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

    public class DisplayQuantityReportable : BluePrintsEntityBase, IReportable_Quantity
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
                if (deliverable.Progress_Type == Estimation_DirectProgressType.Trackable)
                    ColorIndex = 1;
                else
                    ColorIndex = 2;
            }
        }

        public string Deliverable_Name => Reportable.Deliverable_Name;

        public string Phase_Code => Reportable.Phase_Code;

        public string Commodity_Code => Reportable.Commodity_Code;

        public Guid? Workpack_Guid => Reportable.Workpack_Guid;

        public Guid OriginalEntityKey => Reportable.OriginalEntityKey;

        public void SetOriginalEntityKey(Guid newGuid) { }

        public Guid? Area_Guid => Reportable.Area_Guid;

        public Guid? SubArea_Guid => Reportable.SubArea_Guid;

        public decimal Estimated_Units => Reportable.Estimated_Units;

        public decimal Total_Units => Reportable.Total_Units;

        public decimal ItemRate => Reportable.ItemRate;

        public decimal Estimated_Costs => Reportable.Estimated_Costs;

        public decimal TotalCosts => Reportable.Total_Costs;

        public Estimation_DirectProgressType Progress_Type => Reportable.Progress_Type;

        public decimal Estimated_Quantity => Reportable.Estimated_Quantity;

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

        public string UOM => Reportable.UOM;

        public Guid EntityKey { get => Reportable.EntityKey; set => Reportable.EntityKey = value; }

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

        public DateTime ReportingDataDate => Reportable.ReportingDataDate;

        public List<PROGRESS_ITEM> PROGRESS_ITEMS => Reportable.PROGRESS_ITEMS;

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_BeforeDataDate => Reportable.PROGRESS_ITEM_BeforeDataDate;

        public PROGRESS_ITEM PROGRESS_ITEM_Current => Reportable.PROGRESS_ITEM_Current;

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_UpToCurrentDataDate => Reportable.PROGRESS_ITEM_UpToCurrentDataDate;

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_AfterDataDate => Reportable.PROGRESS_ITEM_AfterDataDate;

        public ProgressStats Stats { get => Reportable.Stats; set => Reportable.Stats = value; }

        public decimal Variation_Units => Reportable.Variation_Units;

        public decimal Total_Percentage => Reportable.Total_Percentage;

        public string Discipline_Code => Reportable.Discipline_Code;

        public decimal Variation_Costs => Reportable.Variation_Costs;

        public decimal Total_Costs => Reportable.Total_Costs;

        public decimal Earned_Units_Total => Reportable.Earned_Units_Total;

        public decimal Earned_Costs_Total => Reportable.Earned_Costs_Total;

        public decimal Earned_Units_BeforeDataDate => Reportable.Earned_Units_BeforeDataDate;

        public decimal Earned_Units_OnDataDate => Reportable.Earned_Units_OnDataDate;

        public decimal Earned_Units_ToDate => Reportable.Earned_Units_ToDate;

        public decimal Earned_Costs_ToDate => Reportable.Earned_Costs_ToDate;

        public decimal Earned_Install_Costs_OnDataDate => Reportable.Earned_Install_Costs_OnDataDate;

        public decimal Earned_Supply_Costs_OnDataDate => Reportable.Earned_Supply_Costs_OnDataDate;

        public decimal Earned_Total_Costs_OnDataDate => Reportable.Earned_Total_Costs_OnDataDate;

        public decimal Earned_Units_AfterDataDate => Reportable.Earned_Units_AfterDataDate;

        public decimal Total_Earned_Percentage { get => Reportable.Total_Earned_Percentage; set => Reportable.Total_Earned_Percentage = value; }

        public decimal Total_Percentage_ToDate => Reportable.Total_Percentage_ToDate;

        public decimal TotalInstalledQuantity => Reportable.TotalInstalledQuantity;

        public decimal AbsoluteTotalInstalledQuantity => Reportable.AbsoluteTotalInstalledQuantity;

        public decimal Baseline_Percentage => Reportable.Baseline_Percentage;

        public decimal SchedulePercentage => Reportable.SchedulePercentage;

        public decimal MinPercentage => Reportable.MinPercentage;

        public decimal MaxPercentage => Reportable.MaxPercentage;

        public decimal CurrentPeriodInstalledQuantity { get => Reportable.CurrentPeriodInstalledQuantity; set => Reportable.CurrentPeriodInstalledQuantity = value; }

        public decimal MaxCurrentQuantity => Reportable.MaxCurrentQuantity;

        public bool ShouldSaveProgress => Reportable.ShouldSaveProgress;

        public decimal Remaining_Hours_To_Completion => Reportable.Remaining_Hours_To_Completion;

        public decimal Current_Productivity => Reportable.Current_Productivity;

        public decimal? Override_Productivity { get => Reportable.Override_Productivity; set => Reportable.Override_Productivity = value; }

        public decimal Stock_Code_Supply_Rate => Reportable.Stock_Code_Supply_Rate;

        public decimal Stock_Code_Install_Hours => Reportable.Stock_Code_Install_Hours;

        public decimal Total_Install_Hours => Reportable.Total_Install_Hours;

        public decimal Total_Install_Cost => Reportable.Total_Install_Cost;

        public decimal Total_Supply_Cost => Reportable.Total_Supply_Cost;

        public decimal Total_Cost => Reportable.Total_Cost;

        public SingleObjectSummarizer StatSummarizer => Reportable.StatSummarizer;

        public decimal MinEstimateQuantity => Reportable.MinEstimateQuantity;

        public decimal Variation_Quantity => Reportable.Variation_Quantity;

        public decimal MinEstimateUnits => Reportable.MinEstimateUnits;

        public Guid? Stock_Group_Guid => Reportable.Stock_Group_Guid;

        public decimal? Remaining_Productivity => Reportable.Remaining_Productivity;

        public decimal FreightRate => Reportable.FreightRate;

        public decimal Estimated_Install_Cost => Reportable.Estimated_Install_Cost;

        public decimal Variation_Install_Cost => Reportable.Variation_Install_Cost;

        public decimal Estimated_Freight_Cost => Reportable.Estimated_Freight_Cost;

        public decimal Variation_Freight_Cost => Reportable.Variation_Freight_Cost;

        public decimal Total_Freight_Cost => Reportable.Total_Freight_Cost;

        public decimal Estimated_Install_Hours => Reportable.Estimated_Install_Hours;

        public decimal Variation_Install_Hours => Reportable.Variation_Install_Hours;

        public decimal Estimated_Supply_Cost => Reportable.Estimated_Supply_Cost;

        public decimal Variation_Supply_Cost => Reportable.Variation_Supply_Cost;

        public string Workpack_Name => Reportable.Workpack_Name;

        public string Department_Code => Reportable.Department_Code;

        public decimal Schedule_Estimated_Quantity => Reportable.Schedule_Estimated_Quantity;

        public decimal Schedule_Estimated_Current_Period_Quantity => Reportable.Schedule_Estimated_Current_Period_Quantity;

        public decimal ScheduleCurrentPeriodPercentage => Reportable.ScheduleCurrentPeriodPercentage;

        public void SetReportingDataDate(DateTime dataDate)
        {
            Reportable.SetReportingDataDate(dataDate);
        }

        public void SetProgressItems(List<PROGRESS_ITEM> progresses)
        {
            Reportable.SetProgressItems(progresses);
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
    }
}
