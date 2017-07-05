using BaseModel.Attributes;
using BaseModel.Data.Helpers;
using BaseModel.Misc;
using BluePrints.Common.Base;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BluePrints.Common.Projections
{
    public class STOCK_CODEProjection : BluePrintsProjectionBase<STOCK_CODE>, IQuantityDeliverableGroupProjection, ICanUpdate
    {
        public STOCK_CODEProjection()
            : base()
        {
        }

        public Estimation_Direct_ItemProgress Estimation_Direct_Items { get; set; }

        public IEnumerable<IQuantityReportable> Reportables { get; set; }

        public string ReportableItem_Name
        {
            get { return string.Empty; }
        }

        public string Stock_Code
        {
            get { return Entity.CODE; }
        }

        public Guid? Area_Guid => Entity.GUID_AREA;

        public Guid? SubArea_Guid => Entity.GUID_SUBAREA;

        public decimal TotalHoursIncludeByDuration
        {
            get { return Reportables.Sum(x => x.TotalHoursIncludeByDuration); }
        }

        public decimal EstimatedHours
        {
            get { return Reportables.Sum(x => x.EstimatedHours); }
        }

        public decimal TotalHours
        {
            get { return Reportables.Sum(x => x.TotalHours); }
        }

        public decimal EstimatedCosts
        {
            get { return Reportables.Sum(x => ((ISortableDeliverableProjection)x.Deliverable).EstimatedCosts); }
        }

        public decimal TotalCosts
        {
            get { return Reportables.Sum(x => ((ISortableDeliverableProjection)x.Deliverable).TotalCosts); }
        }

        public decimal ItemRate => Reportables.Sum(x => ((ISortableDeliverableProjection)x.Deliverable).ItemRate);

        public DateTime ReportingDataDate { get; set; }

        public IEnumerable<ISortableDeliverableProjection> Deliverables { get; set; }

        /// <summary>
        /// Refreshes current row
        /// </summary>
        public void Update()
        {
            RaisePropertyChanged();
        }

        public decimal Estimated_Quantity => Reportables.Sum(x => x.Estimated_Quantity);

        public decimal Total_Quantity => Reportables.Sum(x => x.Total_Quantity);

        public string UOM => Entity.UOM;

        #region Access Violation Properties Due to IBasicDeliverable
        public string Commodity_Code => string.Empty;

        public Guid? Workpack_Guid => null;

        public Guid OriginalEntityKey { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        #endregion
    }

    public static class STOCK_CODEProjectionQueries
    {
        public static IQueryable<STOCK_CODEProjection> STOCK_CODEProjectionQuery(
            IQueryable<STOCK_CODE> STOCK_CODES)
        {
            return
                STOCK_CODES.OrderBy(x => x.CODE).ToArray()
                    .Select(
                        stock_code =>
                            new STOCK_CODEProjection()
                            {
                                Entity = stock_code,
                            }).AsQueryable();
        }
    }
}