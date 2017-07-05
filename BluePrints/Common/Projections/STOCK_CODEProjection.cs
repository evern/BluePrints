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

        public IEnumerable<IQuantityReportable> Reportables { get; set; }

        public string Stock_Code => Entity.CODE;

        public Guid? Area_Guid => Entity.GUID_AREA;

        public Guid? SubArea_Guid => Entity.GUID_SUBAREA;

        public decimal TotalHoursIncludeByDuration => Reportables.Sum(x => x.TotalHoursIncludeByDuration);

        public decimal EstimatedHours => Reportables.Sum(x => x.EstimatedHours);

        public decimal TotalHours => Reportables.Where(x => (bool)x.Track).Sum(x => x.TotalHours);

        public decimal ItemRate => Reportables.Sum(x => ((ISortableDeliverableProjection)x.Deliverable).ItemRate);

        public decimal EstimatedCosts => Reportables.Sum(x => ((ISortableDeliverableProjection)x.Deliverable).EstimatedCosts);

        public decimal TotalCosts => Reportables.Sum(x => ((ISortableDeliverableProjection)x.Deliverable).TotalCosts);

        public decimal Estimated_Quantity => Reportables.Sum(x => x.Estimated_Quantity);

        public decimal Total_Quantity => Reportables.Where(x => (bool)x.Track).Sum(x => x.Total_Quantity);

        public string UOM => Entity.UOM;

        /// <summary>
        /// Refreshes current row
        /// </summary>
        public void Update()
        {
            RaisePropertyChanged();
        }
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