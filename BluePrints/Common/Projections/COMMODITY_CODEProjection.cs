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
    public class COMMODITY_CODEProjection : BluePrintsProjectionBase<COMMODITY_CODE>, IQuantityDeliverableGroupProjection, ICanUpdate
    {
        public COMMODITY_CODEProjection()
            : base()
        {
        }

        public IEnumerable<IQuantityReportable> Reportables { get; set; }

        public string Commodity_Code => Entity.CODE;

        public Guid? Area_Guid => Entity.GUID_AREA;

        public Guid? SubArea_Guid => Entity.GUID_SUBAREA;

        public decimal Estimated_Units => Reportables.Sum(x => x.Estimated_Units);

        public decimal Total_Units => Reportables.Where(x => (bool)x.Track).Sum(x => x.Total_Units);

        public decimal ItemRate => Reportables.Sum(x => ((ISortableDeliverableProjection)x.Deliverable).ItemRate);

        public decimal EstimatedCosts => Reportables.Sum(x => ((ISortableDeliverableProjection)x.Deliverable).EstimatedCosts);

        public decimal Total_Costs => Reportables.Sum(x => ((ISortableDeliverableProjection)x.Deliverable).Total_Costs);

        public decimal Estimated_Quantity => Reportables.Sum(x => x.Estimated_Quantity);

        public decimal Total_Quantity => Reportables.Where(x => (bool)x.Track).Sum(x => x.Total_Quantity);

        public string UOM => Entity.UOM;

        public decimal VariationUnits => Reportables.Sum(x => x.VariationUnits);

        public decimal VariationCosts => Reportables.Sum(x => ((ISortableDeliverableProjection)x.Deliverable).VariationCosts);
    }

    public static class COMMODITY_CODEProjectionQueries
    {
        public static IQueryable<COMMODITY_CODEProjection> COMMODITY_CODEProjectionQuery(
            IQueryable<COMMODITY_CODE> COMMODITY_CODES)
        {
            return
                COMMODITY_CODES.OrderBy(x => x.CODE).ToArray()
                    .Select(
                        commodity_code =>
                            new COMMODITY_CODEProjection()
                            {
                                Entity = commodity_code,
                            }).AsQueryable();
        }
    }
}