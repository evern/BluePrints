using BaseModel.Misc;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.ViewModel.Reporting
{
    public interface IReportableGroup : IReportable
    {
        List<IReportable> Reportables { get; set; }
    }

    public interface IReportable : IDisplayDeliverable, IHaveQuantity, IHaveStats, IHaveProgresses, ICanUpdate
    {

    }

    public interface ICanUpdate
    {
        void Update();
    }

    public interface IHaveProgresses
    {
        DateTime ReportingDataDate { get; set; }
        List<PROGRESS_ITEM> PROGRESS_ITEMS { get; set; }
    }

    public interface ISortedProgresses : IHaveProgresses
    {
        IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_BeforeDataDate { get; }
        PROGRESS_ITEM PROGRESS_ITEM_Current { get; }
        IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_UpToCurrentDataDate { get; }
        IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_AfterDataDate { get; }
    }

    public interface IProgressProjection : ISortedProgresses, IGuidEntityKey, IHaveStats
    {

    }

    public interface IDisplayDeliverable : IDeliverable, IGuidEntityKey, IHaveQuantity, IHaveCosts
    {

    }

    public interface IHaveCosts
    {
        decimal ItemRate { get; }
        decimal EstimatedCosts { get; }
        decimal TotalCosts { get; }
    }

    public class GroupDisplayReportable : DisplayReportable
    {
        public IEnumerable<DisplayReportable> ChildDeliverables;
        public GroupDisplayReportable(IReportableGroup reportableGroup)
            : base(reportableGroup)
        {
            this.ChildDeliverables = reportableGroup.Reportables.Select(x => new DisplayReportable(x));
        }
    }

    public class DisplayReportable : IDisplayDeliverable
    {
        readonly IDisplayDeliverable deliverable;
        public DisplayReportable(IDisplayDeliverable deliverable)
        {
            this.deliverable = deliverable;
        }

        public string ReportableItem_Name => deliverable.ReportableItem_Name;

        public string Commodity_Code => deliverable.Commodity_Code;

        public Guid? Workpack_Guid => deliverable.Workpack_Guid;

        public Guid OriginalEntityKey { get => deliverable.OriginalEntityKey; set => deliverable.OriginalEntityKey = value; }

        public string Stock_Code => deliverable.Stock_Code;

        public Guid? Area_Guid => deliverable.Area_Guid;

        public Guid? SubArea_Guid => deliverable.SubArea_Guid;

        public decimal TotalHoursIncludeByDuration => deliverable.TotalHoursIncludeByDuration;

        public decimal EstimatedHours => deliverable.EstimatedHours;

        public decimal TotalHours => deliverable.TotalHours;

        public decimal ItemRate => deliverable.ItemRate;

        public decimal EstimatedCosts => deliverable.EstimatedCosts;

        public decimal TotalCosts => deliverable.TotalCosts;

        public decimal Estimated_Quantity => deliverable.Estimated_Quantity;

        public decimal Total_Quantity => deliverable.Total_Quantity;

        public string UOM => deliverable.UOM;

        public Guid EntityKey { get => deliverable.EntityKey; set => deliverable.EntityKey = value; }
    }

    public interface IDeliverable : IOriginalGuidEntityKey, IHaveStockCode, IHaveHours
    {
        string ReportableItem_Name { get; }
        string Commodity_Code { get; }
        Guid? Workpack_Guid { get; }
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
}
