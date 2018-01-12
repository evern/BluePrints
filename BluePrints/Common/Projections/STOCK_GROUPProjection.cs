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

        public decimal Budget_Units => Deliverables == null? 0 : Deliverables.Sum(x => x.Budget_Units);

        public decimal Total_Units => Deliverables == null ? 0 : Deliverables.Sum(x => x.Total_Units);

        public decimal Budget_ItemRate => Deliverables == null ? 0 : Deliverables.Sum(x => x.Budget_ItemRate);

        public decimal Estimate_ItemRate => Deliverables == null ? 0 : Deliverables.Sum(x => x.Estimate_ItemRate);

        public decimal Estimate_FreightRate => Deliverables == null || Deliverables.Count() == 0 ? 0 : Deliverables.Sum(x => x.Estimate_FreightRate);

        public decimal Estimate_Stock_Code_Supply_Rate => Deliverables == null ? 0 : Deliverables.Sum(x => x.Estimate_Stock_Code_Supply_Rate);

        public decimal Budget_Costs => Deliverables == null ? 0 : Deliverables.Sum(x => x.Budget_Costs);

        public decimal Total_Costs => Deliverables == null ? 0 : Deliverables.Sum(x => x.Total_Costs);

        public decimal Budget_Quantity => Deliverables == null ? 0 : Deliverables.Sum(x => x.Budget_Quantity);

        public decimal Total_Quantity => Deliverables == null ? 0 : Deliverables.Sum(x => x.Total_Quantity);

        public string Estimate_UOM => Entity.UOM;

        public decimal Estimate_Units => Deliverables == null ? 0 : Deliverables.Sum(x => x.Estimate_Units);

        public decimal Variation_Units => Deliverables == null ? 0 : Deliverables.Sum(x => x.Variation_Units);

        public decimal Variation_Costs => Deliverables == null ? 0 : Deliverables.Sum(x => x.Variation_Costs);

        public string Discipline_Code => string.Empty;

        public string Deliverable_Name => string.Empty;

        public Guid? Subjob_Guid => Guid.Empty;

        public Guid OriginalEntityKey => Guid.Empty;

        public EstimateProgressType Progress_Type => EstimateProgressType.Standalone;

        public string Phase_Code => string.Empty;

        public string Commodity_Display_Code => Entity.CODE;

        public decimal Estimate_Stock_Code_Install_Hours => Deliverables == null ? 0 : Deliverables.Sum(x => x.Estimate_Stock_Code_Install_Hours);

        public decimal Variation_Quantity => Deliverables == null ? 0 : Deliverables.Sum(x => x.Variation_Quantity);

        public Guid? Area_Guid => Deliverables == null || Deliverables.Count() == 0 ? Guid.Empty : Deliverables.First().Area_Guid;

        public Guid? SubArea_Guid => Deliverables == null || Deliverables.Count() == 0 ? Guid.Empty : Deliverables.First().SubArea_Guid;

        public Guid? Stock_Group_Guid => Entity.GUID;

        public decimal Budget_FreightRate => Deliverables == null || Deliverables.Count() == 0 ? 0 : Deliverables.Sum(x => x.Budget_FreightRate);

        public decimal Estimate_Install_Cost => Deliverables == null || Deliverables.Count() == 0 ? 0 : Deliverables.Sum(x => x.Estimate_Install_Cost);

        public decimal Variation_Install_Cost => Deliverables == null || Deliverables.Count() == 0 ? 0 : Deliverables.Sum(x => x.Variation_Install_Cost);

        public decimal Estimate_Freight_Cost => Deliverables == null || Deliverables.Count() == 0 ? 0 : Deliverables.Sum(x => x.Estimate_Freight_Cost);

        public decimal Variation_Freight_Cost => Deliverables == null || Deliverables.Count() == 0 ? 0 : Deliverables.Sum(x => x.Variation_Freight_Cost);

        public decimal Estimate_Supply_Cost => Deliverables == null || Deliverables.Count() == 0 ? 0 : Deliverables.Sum(x => x.Estimate_Supply_Cost);

        public decimal Variation_Supply_Cost => Deliverables == null || Deliverables.Count() == 0 ? 0 : Deliverables.Sum(x => x.Variation_Supply_Cost);

        public decimal Estimate_Install_Hours => Deliverables == null ? 0 : Deliverables.Sum(x => x.Estimate_Install_Hours);

        public decimal Variation_Install_Hours => Variation_Units;

        public decimal Total_Install_Hours => Estimate_Install_Hours + Variation_Install_Hours;

        public decimal Total_Estimate_Cost => Estimate_Install_Cost + Estimate_Supply_Cost + Estimate_Freight_Cost;

        public string Subjob_Name => string.Empty;

        public string Department_Code => string.Empty;

        public Guid? Phase_Guid { get; set; }

        public Guid? Discipline_Guid => throw new NotImplementedException();

        public decimal Discipline_Number => throw new NotImplementedException();

        public Guid? Workpack_Guid { get => null; set => throw new NotImplementedException(); }

        public decimal Estimate_Quantity => Deliverables == null ? 0 : Deliverables.Sum(x => x.Estimate_Quantity);

        public decimal Budget_Install_Hours => Deliverables == null ? 0 : Deliverables.Sum(x => x.Budget_Install_Hours);

        public decimal Budget_Install_Cost => Deliverables == null ? 0 : Deliverables.Sum(x => x.Budget_Install_Cost);

        public decimal Total_Budget_Install_Cost => Deliverables == null || Deliverables.Count() == 0 ? 0 : Deliverables.Sum(x => x.Total_Budget_Install_Cost);

        public decimal Total_Budget_Freight_Cost => Deliverables == null || Deliverables.Count() == 0 ? 0 : Deliverables.Sum(x => x.Total_Budget_Freight_Cost);

        public decimal Total_Budget_Supply_Cost => Deliverables == null || Deliverables.Count() == 0 ? 0 : Deliverables.Sum(x => x.Total_Budget_Supply_Cost);

        public decimal Total_Budget_Cost => Total_Budget_Install_Cost + Total_Budget_Supply_Cost + Total_Budget_Freight_Cost;

        public decimal Budget_Freight_Cost => Deliverables == null || Deliverables.Count() == 0 ? 0 : Deliverables.Sum(x => x.Budget_Freight_Cost);

        public decimal Budget_Stock_Code_Install_Hours => Deliverables == null || Deliverables.Count() == 0 ? 0 : Deliverables.Sum(x => x.Budget_Stock_Code_Install_Hours);

        public decimal Budget_Stock_Code_Supply_Rate => Deliverables == null || Deliverables.Count() == 0 ? 0 : Deliverables.Sum(x => x.Budget_Stock_Code_Install_Hours);

        public decimal Budget_Supply_Cost => Deliverables == null || Deliverables.Count() == 0 ? 0 : Deliverables.Sum(x => x.Budget_Stock_Code_Install_Hours);

        public string Budget_UOM => Entity.UOM;

        Guid? IDeliverable.Subjob_Guid { get; set; }

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