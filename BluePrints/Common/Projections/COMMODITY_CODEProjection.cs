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
    public class COMMODITY_CODEProjection : BluePrintsProjectionBase<COMMODITY_CODE>, IDeliverable_Quantity_Group, ICanUpdate
    {
        public COMMODITY_CODEProjection()
            : base()
        {
        }

        public IEnumerable<IDeliverable_Quantity> Deliverables { get; set; }

        public string Commodity_Code => Entity.CODE;

        public Guid? Area_Guid => Entity.GUID_AREA;

        public Guid? SubArea_Guid => Entity.GUID_SUBAREA;

        public decimal Estimated_Units => Deliverables.Where(x => x.Progress_Type == Estimation_DirectProgressType.Trackable).Sum(x => x.Estimated_Units);

        public decimal Total_Units => Deliverables.Where(x => x.Progress_Type == Estimation_DirectProgressType.Trackable).Sum(x => x.Total_Units);

        public decimal ItemRate => Deliverables.Sum(x => x.ItemRate);

        public decimal Estimated_Costs => Deliverables.Sum(x => x.Estimated_Costs);

        public decimal Total_Costs => Deliverables.Sum(x => x.Total_Costs);

        public decimal Estimated_Quantity => Deliverables.Where(x => x.Progress_Type == Estimation_DirectProgressType.Trackable).Sum(x => x.Estimated_Quantity);

        public decimal Total_Quantity => Deliverables.Where(x => x.Progress_Type == Estimation_DirectProgressType.Trackable).Sum(x => x.Total_Quantity);

        public string UOM => Entity.UOM;

        public decimal Variation_Units => Deliverables.Sum(x => x.Variation_Units);

        public decimal Variation_Costs => Deliverables.Sum(x => x.Variation_Costs);

        public string Discipline_Code => string.Empty;

        public string Deliverable_Name => string.Empty;

        public Guid? Workpack_Guid => Guid.Empty;

        public Guid OriginalEntityKey => throw new NotImplementedException();

        public Estimation_DirectProgressType Progress_Type => Estimation_DirectProgressType.Standalone;

        public void SetOriginalEntityKey(Guid newGuid)
        {
            throw new NotImplementedException();
        }
    }

    public static class COMMODITY_CODEProjectionQueries
    {
        public static IQueryable<COMMODITY_CODEProjection> Commodity_Code_Group_Transformation(
            IQueryable<COMMODITY_CODE> COMMODITY_CODES)
        {
            return
                COMMODITY_CODES.OrderBy(x => x.CODE).ToArray()
                    .Select(
                        commodity_code =>
                            new COMMODITY_CODEProjection()
                            {
                                Entity = commodity_code
                            }).AsQueryable();
        }
    }
}