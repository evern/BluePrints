using BaseModel.Misc;
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
    public class ReportablesDisplay : BindableBase, IGuidEntityKey
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

        public void Update()
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
        }
    }

    public class DisplayQuantityReportableGroup : DisplayQuantityReportable
    {
        public IEnumerable<DisplayQuantityReportable> ChildReportables;
        public DisplayQuantityReportableGroup(IReportable_Quantity_Group reportableGroup)
            : base(reportableGroup)
        {
            this.ChildReportables = reportableGroup.Reportables.Select(x => new DisplayQuantityReportable(x));
        }

        public override void Update()
        {
            foreach (DisplayQuantityReportable child_reportable in ChildReportables)
                child_reportable.Update();

            base.Update();
        }
    }

    public class DisplayQuantityReportable : BindableBase, IReportable_Quantity
    {
        readonly IReportable_Quantity reportable;
        private SingleObjectSummarizer statsSummarizer;
        public SingleObjectSummarizer StatSummarizer => statsSummarizer;

        //For bindableBase property name usage only
        public DisplayQuantityReportable()
        {

        }

        public DisplayQuantityReportable(IReportable_Quantity deliverable)
        {
            this.reportable = deliverable;
        }

        public string Deliverable_Name => reportable.Deliverable_Name;

        public string Commodity_Code => reportable.Commodity_Code;

        public Guid? Workpack_Guid => reportable.Workpack_Guid;

        public Guid OriginalEntityKey => reportable.OriginalEntityKey;

        public void SetOriginalEntityKey(Guid newGuid) { }

        public Guid? Area_Guid => reportable.Area_Guid;

        public Guid? SubArea_Guid => reportable.SubArea_Guid;

        public decimal Estimated_Units => reportable.Estimated_Units;

        public decimal Total_Units => reportable.Total_Units;

        public decimal ItemRate => reportable.ItemRate;

        public decimal Estimated_Costs => reportable.Estimated_Costs;

        public decimal TotalCosts => reportable.Total_Costs;

        public bool? Track => reportable.Track;

        public decimal Estimated_Quantity => reportable.Estimated_Quantity;

        public decimal Total_Quantity => reportable.Total_Quantity;

        public string UOM => reportable.UOM;

        public Guid EntityKey { get => reportable.EntityKey; set => reportable.EntityKey = value; }

        public decimal QuantityPerHour => reportable.QuantityPerHour;

        public decimal PastInstalledQuantity => reportable.PastInstalledQuantity;

        public DateTime ReportingDataDate => reportable.ReportingDataDate;

        public List<PROGRESS_ITEM> PROGRESS_ITEMS => reportable.PROGRESS_ITEMS;

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_BeforeDataDate => reportable.PROGRESS_ITEM_BeforeDataDate;

        public PROGRESS_ITEM PROGRESS_ITEM_Current => reportable.PROGRESS_ITEM_Current;

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_UpToCurrentDataDate => reportable.PROGRESS_ITEM_UpToCurrentDataDate;

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_AfterDataDate => reportable.PROGRESS_ITEM_AfterDataDate;

        public ProgressStats Stats { get => reportable.Stats; set => reportable.Stats = value; }

        public decimal Variation_Units => reportable.Variation_Units;

        public decimal Total_Percentage => reportable.Total_Percentage;

        public string Discipline_Code => reportable.Discipline_Code;

        public decimal Variation_Costs => reportable.Variation_Costs;

        public decimal Total_Costs => reportable.Total_Costs;

        public decimal Earned_Units_Total => reportable.Earned_Units_Total;

        public decimal Earned_Costs_Total => reportable.Earned_Costs_Total;

        public decimal Earned_Units_BeforeDataDate => reportable.Earned_Units_BeforeDataDate;

        public decimal Earned_Units_OnDataDate => reportable.Earned_Units_OnDataDate;

        public decimal Earned_Units_ToDate => reportable.Earned_Units_ToDate;

        public decimal Earned_Costs_ToDate => reportable.Earned_Costs_ToDate;

        public decimal Earned_Costs_OnDataDate => reportable.Earned_Costs_OnDataDate;

        public decimal Earned_Units_AfterDataDate => reportable.Earned_Units_AfterDataDate;

        public decimal Total_Earned_Percentage { get => reportable.Total_Earned_Percentage; set => reportable.Total_Earned_Percentage = value; }

        public decimal Total_Percentage_ToDate => reportable.Total_Percentage_ToDate;

        public decimal TotalInstalledQuantity => reportable.TotalInstalledQuantity;

        public decimal Baseline_Percentage => reportable.Baseline_Percentage;

        public decimal SchedulePercentage => reportable.SchedulePercentage;

        public decimal MinPercentage => reportable.MinPercentage;

        public decimal MaxPercentage => reportable.MaxPercentage;

        public decimal CurrentPeriodInstalledQuantity { get => reportable.CurrentPeriodInstalledQuantity; set => reportable.CurrentPeriodInstalledQuantity = value; }

        public decimal MaxCurrentQuantity => reportable.MaxCurrentQuantity;

        public bool ShouldSaveProgress => reportable.ShouldSaveProgress;

        public void SetReportingDataDate(DateTime dataDate)
        {
            reportable.SetReportingDataDate(dataDate);
        }

        public void SetProgressItems(List<PROGRESS_ITEM> progresses)
        {
            reportable.SetProgressItems(progresses);
        }

        public void AppendProgressItem(PROGRESS_ITEM currentProgress)
        {
            reportable.AppendProgressItem(currentProgress);
        }

        public virtual void Update()
        {
            reportable.Update();
        }

        public decimal getCurrentPeriodEarnedUnits(decimal newPercentage)
        {
            return reportable.getCurrentPeriodEarnedUnits(newPercentage);
        }

        public IEnumerable<PROGRESS_ITEM> GetExistingOrNewEditedProgresses(Func<Expression<Func<PROGRESS_ITEM, bool>>, PROGRESS_ITEM> repository_find_actual_func)
        {
            return reportable.GetExistingOrNewEditedProgresses(repository_find_actual_func);
        }
    }
}
