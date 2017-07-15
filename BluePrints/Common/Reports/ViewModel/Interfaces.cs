using BaseModel.Misc;
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
    }

    public interface IReportable_Quantity : IReportable, IHaveQuantity, ICanTrack, ICanProgressByQuantity
    {

    }

    public interface IReportable_Group : IReportable
    {
        IEnumerable<IReportable> Reportables { get; }
    }

    public interface IReportable : IDeliverable_Rates, IHaveStats, IHaveProgresses, ICanSetProgresses, ICanUpdate
    {
        SingleObjectSummarizer StatSummarizer { get; }
    }

    public interface IDeliverable_Quantity_Group : IDeliverable_Quantity
    {
        IEnumerable<IDeliverable_Quantity> Deliverables { get; }
    }

    public interface IDeliverable_Quantity : IDeliverable_Rates, IHaveQuantity, ICanTrack
    {

    }

    public interface IDeliverable_Rates_Group : IDeliverable_Rates
    {
        IEnumerable<IDeliverable_Rates> DeliverableRates { get; }
    }

    public interface IDeliverable_Rates : IDeliverable, IHaveCosts
    {

    }

    public interface IDeliverable : IGuidEntityKey, IOriginalGuidEntityKey, IHaveCommodity_Code, IHaveHours
    {
        string Discipline_Code { get; }
        string Deliverable_Name { get; }
        Guid? Workpack_Guid { get; }
    }

    #region Ability Specification Interfaces
    public interface ICanProgressByQuantity : IHaveQuantity
    {
        decimal QuantityPerHour { get; }
        decimal PastInstalledQuantity { get; }
        decimal CurrentPeriodInstalledQuantity { get; set; }
        decimal MaxCurrentQuantity { get; }
        decimal TotalInstalledQuantity { get; }
        decimal getCurrentPeriodEarnedUnits(decimal newPercentage);
    }
    #endregion

    #region Property Specification Interfaces
    public interface ICanAssignP6 : IDeliverable, ICanUpdate
    {
        List<P6_ASSIGNMENT> P6_Assignments { get; set; }
        decimal Assigned_Percentage { get; }
        decimal Remaining_Percentage { get; }
    }

    public interface ICanSetProgresses
    {
        decimal Earned_Units_Total { get; }
        decimal Earned_Costs_Total { get; }
        decimal Earned_Units_BeforeDataDate { get; }
        decimal Earned_Units_OnDataDate { get; }
        decimal Earned_Costs_OnDataDate { get; }
        decimal Earned_Units_ToDate { get; }
        decimal Earned_Costs_ToDate { get; }
        decimal Earned_Units_AfterDataDate { get; }
        decimal Total_Earned_Percentage { get; set; }
        decimal Total_Percentage { get; }
        decimal Total_Percentage_ToDate { get; }
        decimal Baseline_Percentage { get; }
        decimal SchedulePercentage { get; }
        decimal MinPercentage { get; }
        decimal MaxPercentage { get; }
        bool ShouldSaveProgress { get; }

        IEnumerable<PROGRESS_ITEM> GetExistingOrNewEditedProgresses(Func<Expression<Func<PROGRESS_ITEM, bool>>, PROGRESS_ITEM> repository_find_actual_func);
        void SetReportingDataDate(DateTime dataDate);
        void SetProgressItems(List<PROGRESS_ITEM> progresses);
        void AppendProgressItem(PROGRESS_ITEM currentProgress);
    }

    public interface ICanTrack
    {
        bool? Track { get; }
    }

    public interface IHaveCosts
    {
        decimal ItemRate { get; }
        decimal Estimated_Costs { get; }
        decimal Variation_Costs { get; }
        decimal Total_Costs { get; }
    }

    public interface IHaveStockCode
    {
        string UOM { get; }
        string Stock_Code_Type { get; }
        string Stock_Code_Spec { get; }
        string Stock_Code_Desription { get; }
    }

    public interface IHaveCommodity_Code
    {
        //must use string because commodity code is not actual entity in design
        string Commodity_Code { get; }
        Guid? Area_Guid { get; }
        Guid? SubArea_Guid { get; }
    }

    public interface ISupportByDuration
    {
        bool IsByDuration { get; }
    }

    public interface IHaveDeliverableStatus
    {
        DELIVERABLES_STATUS Deliverable_Status { get; }
    }

    public interface IHaveP6Baselines
    {
        string P6_Baseline_Name { get; }
        string P6_Mod_Baseline_Name { get; }
    }

    public interface IHaveHours
    {
        decimal Estimated_Units { get; }
        decimal Total_Units { get; }
        decimal Variation_Units { get; }
    }

    public interface IHaveQuantity
    {
        decimal Estimated_Quantity { get; }
        decimal Total_Quantity { get; }
        string UOM { get; }
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
    #endregion
}
