using BaseModel.Misc;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.ViewModel.Reporting
{
    public interface IReportableGroup : IQuantityReportable
    {
        IEnumerable<IQuantityReportable> Deliverables { get; }
    }

    public interface IQuantityReportable : IReportable, ICanProgressByQuantity
    {

    }

    public interface IReportable : IDeliverableProjection, IHaveStats, IHaveProgresses, ICanUpdate
    {

    }

    public interface IQuantityDeliverableGroupProjection : IQuantityDeliverableProjection, IHaveQuantity
    {
        IEnumerable<IQuantityReportable> Reportables { get; set; }
    }

    /// <summary>
    /// Deliverables with Rates and Quantity
    /// </summary>
    public interface IQuantityDeliverableProjection : IDeliverableProjection, IHaveQuantity
    {

    }

    /// <summary>
    /// Deliverables with Rates
    /// </summary>
    public interface IDeliverableProjection : IBasicDeliverable, IGuidEntityKey, IHaveCosts
    {

    }


    public interface IBasicDeliverable : IOriginalGuidEntityKey, IHaveStockCode, IHaveHours
    {
        string ReportableItem_Name { get; }
        string Commodity_Code { get; }
        Guid? Workpack_Guid { get; }
    }

    #region Ability Specification Interfaces
    public interface ICanProgressByQuantity : IHaveQuantity
    {
        decimal QuantityPerHour { get; }
        decimal TotalPercentage { get; }
        decimal PastInstalledQuantity { get; }
        decimal CurrentTotalInstalledQuantity { get; set; }
        decimal GetCurrentPeriodPercentage(decimal newTotalQuantity);
        decimal GetCurrentPeriodHours(decimal newPeriodPercentage);
    }

    public interface ICanUpdate
    {
        void Update();
    } 
    #endregion

    #region Property Specification Interfaces
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
        DateTime ReportingDataDate { get; set; }
        List<PROGRESS_ITEM> PROGRESS_ITEMS { get; set; }
    } 
    #endregion
}
