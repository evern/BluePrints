using BaseModel.Misc;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.ViewModel.Reporting
{
    public interface IQuantityReportableGroup : IQuantityReportable
    {
        IEnumerable<IQuantityReportable> Deliverables { get; }
    }

    public interface IQuantityReportable : IReportable, ICanProgressByQuantity, ICanTrack
    {

    }

    public interface IReportableStats : IReportable
    {
        SingleObjectSummarizer StatSummarizer { get; }
    }

    public interface IReportable : IDeliverable, IHaveStats, IHaveProgresses
    {
        IDeliverable Deliverable { get; }
    }

    public interface IQuantityDeliverableGroupProjection : IDeliverable, IHaveCosts, IHaveQuantity
    {
        IEnumerable<IQuantityReportable> Reportables { get; }
    }

    public interface IGroupDeliverableProjection : IDeliverable, IHaveQuantity
    {

    }

    /// <summary>
    /// Deliverables with Rates and Quantity
    /// </summary>
    public interface ITrackableQuantityDeliverableProjection : IQuantityDeliverableProjection, ICanTrack
    {

    }

    /// <summary>
    /// Deliverables with Rates and Quantity
    /// </summary>
    public interface IQuantityDeliverableProjection : ISortableDeliverableProjection, IHaveQuantity
    {

    }

    /// <summary>
    /// Deliverables with Rates
    /// </summary>
    public interface ISortableDeliverableProjection : ISortableDeliverable, IHaveCosts
    {

    }

    public interface ISortableDeliverable : IDeliverable, IOriginalGuidEntityKey
    {
        string Discipline_Code { get; }
        string ReportableItem_Name { get; }
        Guid? Workpack_Guid { get; }
    }

    public interface IDeliverable : IGuidEntityKey, IHaveCommodity_Code, IHaveHours
    {

    }

    #region Ability Specification Interfaces
    public interface ICanProgressByQuantity : IHaveQuantity
    {
        decimal QuantityPerHour { get; }
        decimal TotalPercentage { get; }
        decimal PastInstalledQuantity { get; }
        decimal CurrentTotalInstalledQuantity { get; }
        decimal GetCurrentPeriodPercentage(decimal newTotalQuantity);
        decimal GetCurrentPeriodHours(decimal newPeriodPercentage);
    }
    #endregion

    #region Property Specification Interfaces
    public interface ICanSetProgresses
    {
        decimal Current_Total_Percentage { get; }
        void SetReportingDataDate(DateTime dataDate);
        void SetProgressItems(List<PROGRESS_ITEM> progresses);
        void AppendCurrentProgressItem(PROGRESS_ITEM currentProgress);
    }

    public interface ICanTrack
    {
        bool? Track { get; }
    }

    public interface IHaveCosts
    {
        decimal ItemRate { get; }
        decimal EstimatedCosts { get; }
        decimal VariationCosts { get; }
        decimal TotalCosts { get; }
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

    public interface IHaveHours
    {
        decimal TotalUnitsIncludeByDuration { get; }
        decimal EstimatedUnits { get; }
        decimal TotalUnits { get; }
        decimal VariationUnits { get; }
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
