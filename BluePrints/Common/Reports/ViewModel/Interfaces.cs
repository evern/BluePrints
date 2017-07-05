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

    public interface IReportable : IDeliverable, IHaveStats, IHaveProgresses, ICanUpdate
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
        string ReportableItem_Name { get; }
        string Commodity_Code { get; }
        Guid? Workpack_Guid { get; }
    }

    public interface IDeliverable : IGuidEntityKey, IHaveStockCode, IHaveHours
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

    public interface ICanUpdate
    {
        void Update();
    } 
    #endregion

    #region Property Specification Interfaces
    public interface ICanSetProgresses
    {
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
        decimal EstimatedCosts { get; }
        decimal TotalCosts { get; }
    }

    public interface IHaveStockCode
    {
        //must use string because stock code is not actual entity in design
        string Stock_Code { get; }
        Guid? Area_Guid { get; }
        Guid? SubArea_Guid { get; }
    }

    public interface IHaveHours
    {
        decimal TotalHoursIncludeByDuration { get; }
        decimal EstimatedHours { get; }
        decimal TotalHours { get; }
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
