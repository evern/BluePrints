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
    public class STOCK_GROUPProjection : BluePrintsProjectionBase<STOCK_GROUP>, IDeliverable_Quantity_Group, ICanUpdate
    {
        public STOCK_GROUPProjection()
            : base()
        {
        }

        public IEnumerable<IDeliverable_Quantity> Deliverables { get; set; }

        public string Commodity_Code => Entity.CODE;

        public decimal Estimated_Units => Deliverables == null? 0 : Deliverables.Sum(x => x.Estimated_Units);

        public decimal Total_Units => Deliverables == null ? 0 : Deliverables.Sum(x => x.Total_Units);

        public decimal ItemRate => Deliverables == null ? 0 : Deliverables.Sum(x => x.ItemRate);

        public decimal Stock_Code_Supply_Rate => Deliverables == null ? 0 : Deliverables.Sum(x => x.Stock_Code_Supply_Rate);

        public decimal Estimated_Costs => Deliverables == null ? 0 : Deliverables.Sum(x => x.Estimated_Costs);

        public decimal Total_Costs => Deliverables == null ? 0 : Deliverables.Sum(x => x.Total_Costs);

        public decimal Estimated_Quantity => Deliverables == null ? 0 : Deliverables.Sum(x => x.Estimated_Quantity);

        public decimal Total_Quantity => Deliverables == null ? 0 : Deliverables.Sum(x => x.Total_Quantity);

        public string UOM => Entity.UOM;

        public decimal Variation_Units => Deliverables == null ? 0 : Deliverables.Sum(x => x.Variation_Units);

        public decimal Variation_Costs => Deliverables == null ? 0 : Deliverables.Sum(x => x.Variation_Costs);

        public string Discipline_Code => string.Empty;

        public string Deliverable_Name => string.Empty;

        public Guid? Workpack_Guid => Guid.Empty;

        public Guid OriginalEntityKey => Guid.Empty;

        public Estimation_DirectProgressType Progress_Type => Estimation_DirectProgressType.Standalone;

        public string Phase_Code => string.Empty;

        public string Commodity_Display_Code => Entity.CODE;

        public decimal Stock_Code_Install_Hours => Deliverables == null ? 0 : Deliverables.Sum(x => x.Stock_Code_Install_Hours);

        public decimal Variation_Quantity => Deliverables == null ? 0 : Deliverables.Sum(x => x.Variation_Quantity);

        public Guid? Area_Guid => Deliverables == null || Deliverables.Count() == 0 ? Guid.Empty : Deliverables.First().Area_Guid;

        public Guid? SubArea_Guid => Deliverables == null || Deliverables.Count() == 0 ? Guid.Empty : Deliverables.First().SubArea_Guid;

        public Guid? Stock_Group_Guid => Entity.GUID;

        public decimal FreightRate => Deliverables == null || Deliverables.Count() == 0 ? 0 : Deliverables.Sum(x => x.FreightRate);

        public decimal Estimated_Install_Cost => Deliverables == null || Deliverables.Count() == 0 ? 0 : Deliverables.Sum(x => x.Estimated_Install_Cost);

        public decimal Variation_Install_Cost => Deliverables == null || Deliverables.Count() == 0 ? 0 : Deliverables.Sum(x => x.Variation_Install_Cost);

        public decimal Total_Install_Cost => Deliverables == null || Deliverables.Count() == 0 ? 0 : Deliverables.Sum(x => x.Total_Install_Cost);

        public decimal Estimated_Freight_Cost => Deliverables == null || Deliverables.Count() == 0 ? 0 : Deliverables.Sum(x => x.Estimated_Freight_Cost);

        public decimal Variation_Freight_Cost => Deliverables == null || Deliverables.Count() == 0 ? 0 : Deliverables.Sum(x => x.Variation_Freight_Cost);

        public decimal Total_Freight_Cost => Deliverables == null || Deliverables.Count() == 0 ? 0 : Deliverables.Sum(x => x.Total_Freight_Cost);

        public decimal Estimated_Supply_Cost => Deliverables == null || Deliverables.Count() == 0 ? 0 : Deliverables.Sum(x => x.Estimated_Supply_Cost);

        public decimal Variation_Supply_Cost => Deliverables == null || Deliverables.Count() == 0 ? 0 : Deliverables.Sum(x => x.Variation_Supply_Cost);

        public decimal Total_Supply_Cost => Deliverables == null || Deliverables.Count() == 0 ? 0 : Deliverables.Sum(x => x.Total_Supply_Cost);

        public decimal Estimated_Install_Hours => Estimated_Units;

        public decimal Variation_Install_Hours => Variation_Units;

        public decimal Total_Install_Hours => Estimated_Install_Hours + Variation_Install_Hours;

        public decimal Total_Cost => throw new NotImplementedException();

        public void SetOriginalEntityKey(Guid newGuid)
        {
            
        }
    }

    public static class STOCK_GROUPProjectionQueries
    {
        public static IQueryable<STOCK_GROUPProjection> Stock_Group_Transformation(
            IQueryable<STOCK_GROUP> STOCK_GROUPS)
        {
            return
                STOCK_GROUPS.OrderBy(x => x.CODE).ToArray()
                    .Select(
                        stock_group =>
                            new STOCK_GROUPProjection()
                            {
                                Entity = stock_group
                            }).AsQueryable();
        }
    }
}