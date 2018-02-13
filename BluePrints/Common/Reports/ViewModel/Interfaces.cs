using BaseModel.Misc;
using BluePrints.Common.Projections;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.ViewModel.Reporting
{
    public interface IReportable_Quantity_Group : IReportable_Quantity
    {
        IEnumerable<IReportable_Quantity> Reportables { get; }
        decimal Trackable_Total_Quantity { get; }
        decimal Trackable_Installed_Quantity { get; }
        decimal Trackable_QuantityPerUnit { get; }
        decimal Trackable_Total_Units { get; }
    }

    //This interface is only used in variation to describe adjustment hours and costs
    //Variation_Install_Hours here is uncommitted hours wherelse Variation_Install_Hours in IHaveQuantity is coming from committed DC_Quantity
    public interface IVariation_Quantity
    {
        decimal Variation_Install_Cost { get; }
        decimal Variation_Supply_Cost { get; }
        decimal Variation_Freight_Cost { get; }
        decimal Variation_Total_Cost { get; }
        decimal Variation_Install_Hours { get; }
    }

    public interface IReportable_Quantity : IReportable, IHaveQuantity, ICanTrack, IHaveStock_Group, ICanProgressByQuantity
    {
        decimal Schedule_Estimated_Quantity { get; }
        decimal Schedule_Estimated_Current_Period_Quantity { get; }
        decimal Remaining_Hours_To_Completion { get; }
        decimal MinEstimateQuantity { get; }
        decimal Earned_Install_Costs_OnDataDate { get; }
        decimal Earned_Supply_Costs_OnDataDate { get; }
        decimal Earned_Total_Costs_OnDataDate { get; }
    }

    public interface IReportable_Group : IReportable
    {
        IEnumerable<IReportable> Reportables { get; }
    }

    public interface IReportable : IDeliverable_Rates, IHaveStats, IHaveProgresses, ICanSetProgresses, ICanUpdate
    {
        SingleObjectSummarizer StatSummarizer { get; }
        decimal Current_Productivity { get; }
        decimal? Override_Productivity { get; set; }
        decimal? Remaining_Productivity { get; }
    }

    public interface IDeliverable_Quantity_Group : IDeliverable_Quantity
    {
        IEnumerable<IDeliverable_Quantity> Deliverables { get; }
    }

    public interface IDeliverable_Quantity : IDeliverable_Rates, IHaveStock_Group, IHaveQuantity, ICanTrack
    {
        
    }

    public interface IDeliverable_Rates_Group : IDeliverable_Rates
    {
        IEnumerable<IDeliverable_Rates> DeliverableRates { get; }
    }

    public interface IDeliverable_Rates : IDeliverable, IHaveCosts
    {
        IEnumerable<User_Weight> AssignedUsers { get; }
    }

    public interface IDeliverable : IGuidEntityKey, IOriginalGuidEntityKey, IHaveCommodity_Code, IHaveHours
    {
        string Subjob_Name { get; }
        PhaseType? Phase { get; }
        string Phase_Code { get; }
        string Department_Code { get; }
        string Discipline_Code { get; }
        string Deliverable_Name { get; }
        Guid? Phase_Guid { get; set; }
        Guid? Subjob_Guid { get; set; }
        Guid? Area_Guid { get; }
        Guid? SubArea_Guid { get; }
        Guid? Discipline_Guid { get; }
        decimal Discipline_Number { get; }
        Guid? Workpack_Guid { get; set; }
    }

    public interface IHaveProcurementSubjob
    {
        Guid? Procurement_Subjob_Guid { get; set; }
    }

    #region Ability Specification Interfaces
    public interface ICanProgressByQuantity : IHaveQuantity
    {
        decimal QuantityPerUnit { get; }
        decimal UnitsPerQuantity { get; }
        decimal PastInstalledQuantity { get; }
        decimal CurrentPeriodInstalledQuantity { get; set; }
        decimal MaxCurrentQuantity { get; }
        decimal TotalInstalledQuantity { get; }
        decimal AbsoluteTotalInstalledQuantity { get; }
        decimal getCurrentPeriodEarnedUnits(decimal newPercentage);
    }

    public interface ICanAssignP6 : ICanUpdate, IGuidEntityKey, IOriginalGuidEntityKey, IHaveHours
    {
        List<P6_ASSIGNMENT> P6_Assignments { get; }
        Guid DeliverableKey { get; }
        string P6AssignmentName { get; }
        string P6AssignmentDescription { get; }
        decimal Assigned_Percentage { get; }
        decimal Remaining_Percentage { get; }
        decimal P6_Assignment_Total_Quantity { get; }
        string P6_Assignment_UOM { get; }
        Guid? P6_WorkpackGuid { get; }
    }

    public interface ICanSetProgresses
    {
        decimal Earned_Units_Total { get; }
        decimal Earned_Costs_Total { get; }
        decimal Earned_Units_BeforeDataDate { get; }
        decimal Earned_Units_OnDataDate { get; }
        decimal Earned_Units_ToDate { get; }
        decimal Earned_Costs_ToDate { get; }
        decimal Earned_Costs_OnDataDate { get; }
        decimal Earned_Units_AfterDataDate { get; }
        decimal Total_Earned_Percentage { get; set; }
        decimal Total_Percentage { get; }
        decimal Total_Percentage_ToDate { get; }
        decimal Baseline_Percentage { get; }
        decimal SchedulePercentage { get; }
        decimal ScheduleCurrentPeriodPercentage { get; }
        decimal MinPercentage { get; }
        decimal MaxPercentage { get; }
        bool ShouldSaveProgress { get; }
        decimal MinEstimateUnits { get; }

        IEnumerable<PROGRESS_ITEM> GetExistingOrNewEditedProgresses(Func<Expression<Func<PROGRESS_ITEM, bool>>, PROGRESS_ITEM> repository_find_actual_func);
        void SetReportingDataDate(DateTime dataDate);
        void SetProgressItems(List<PROGRESS_ITEM> progresses);
        void AppendProgressItem(PROGRESS_ITEM currentProgress);
    }
    #endregion

    #region Property Specification Interfaces
    public interface ISupportVariationSummary : ISupportVariation
    {
        decimal Forecast_Units { get; }
        decimal Variation_Cost { get; }
    }

    public interface ISupportVariation
    {
        Guid? Baseline_Guid { get; set; }
        Guid? Variation_Guid { get; set; }
        decimal Estimated_Value { get; set; }
        decimal DC_Value { get; set; }
    }

    public interface ICanTrack
    {
        EstimateProgressType Progress_Type { get; }
    }

    public interface IHaveCosts
    {
        decimal Budget_ItemRate { get; }
        decimal Budget_Costs { get; }
        decimal Variation_Costs { get; }
        decimal Total_Costs { get; }
    }

    public interface IHaveStockCode
    {
        string Estimate_UOM { get; }
        string Estimate_Stock_Code_Type { get; }
        string Estimate_Stock_Code_Spec { get; }
        string Estimate_Stock_Code_Description { get; }
    }

    public interface IHaveDBProductivityOverride
    {
        decimal? DB_Productivity_Override { get; set; }
    }

    public interface IHaveStock_Group
    {
        Guid? Stock_Group_Guid { get; }
    }

    public interface IHaveCommodity_Code
    {
        string Commodity_Code { get; }
    }

    public interface ISupportByDuration
    {
        bool IsByDuration { get; }
    }

    public interface IHaveDeliverableStatus
    {
        DELIVERABLES_STATUS Deliverable_Status { get; }
    }

    public interface IHaveP6Baselines : IGuidEntityKey
    {
        Guid project_guid { get; }
        string P6_Baseline_Name { get; }
        string P6_Mod_Baseline_Name { get; }
    }

    public interface IHaveHours
    {
        decimal Budget_Units { get; }
        decimal Total_Units { get; }
        decimal Variation_Units { get; }
    }

    public interface IHaveQuantity
    {
        decimal Estimate_Units { get; }
        decimal Estimate_ItemRate { get; }
        decimal Estimate_Stock_Code_Install_Hours { get; }
        decimal Budget_Stock_Code_Install_Hours { get; }
        decimal Estimate_Stock_Code_Supply_Rate { get; }
        decimal Budget_Stock_Code_Supply_Rate { get; }
        decimal Estimate_Quantity { get; }
        decimal Budget_Quantity { get; }
        decimal Variation_Quantity { get; }
        decimal Total_Quantity { get; }
        decimal Estimate_Install_Hours { get; }
        decimal Budget_Install_Hours { get; }
        decimal Variation_Install_Hours { get; }
        decimal Estimate_Install_Cost { get; }
        decimal Budget_Install_Cost { get; }
        decimal Variation_Install_Cost { get; }
        decimal Budget_FreightRate { get; }
        decimal Estimate_FreightRate { get; }
        decimal Estimate_Freight_Cost { get; }
        decimal Budget_Freight_Cost { get; }
        decimal Variation_Freight_Cost { get; }
        decimal Estimate_Supply_Cost { get; }
        decimal Budget_Supply_Cost { get; }
        decimal Variation_Supply_Cost { get; }
        decimal Total_Budget_Install_Cost { get; }
        decimal Total_Budget_Freight_Cost { get; }
        decimal Total_Budget_Supply_Cost { get; }
        decimal Total_Estimate_Cost { get; }
        decimal Total_Budget_Cost { get; }
        string Estimate_UOM { get; }
        string Budget_UOM { get; }
    }

    public interface IHaveProgresses
    {
        IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_BeforeDataDate { get; }
        PROGRESS_ITEM PROGRESS_ITEM_Current { get; }
        IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_UpToCurrentDataDate { get; }
        IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_AfterDataDate { get; }
        DateTime ReportingDataDate { get; }
        List<PROGRESS_ITEM> PROGRESS_ITEMS { get; }
    } 

    public interface IAmBaseline : IGuidEntityKey
    {
        BaselineStatus Baseline_Status { get; set; }
        string Revision { get; set; }
    }
    #endregion
}
