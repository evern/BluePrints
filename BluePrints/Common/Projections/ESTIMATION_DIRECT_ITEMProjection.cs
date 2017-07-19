using BaseModel.Attributes;
using BaseModel.Data.Helpers;
using BluePrints.Common.Base;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BluePrints.Common.Projections
{
    public class ESTIMATION_DIRECT_ITEMProjection : BluePrintsProjectionBase<ESTIMATION_DIRECT_ITEM>, IDeliverable_Quantity, ICanAssignP6, IHaveStockCode
    {
        public ESTIMATION_DIRECT_ITEMProjection()
            : base()
        {
            //need to initialize commodity code here so that copy/paste is able to get property info within COMMODITY_CODE
            stock_code = new STOCK_CODE();
        }

        public IEnumerable<STOCK_CODE> StockCodeCollection { get; set; }
        private STOCK_CODE stock_code;
        public STOCK_CODE STOCK_CODE
        {
            get
            {
                return stock_code;
            }
            set
            {
                //Always go by value so that changes can be identified in view
                if (value != null)
                    DataUtils.ShallowCopy(stock_code, value);
            }
        }

        //Used for direct property access validation in fill/undo-redo
        public Guid? CommodityCodeGuid
        {
            get
            {
                return Entity.GUID_COMMODITY_CODE;
            }
            set
            {
                Guid? setValue = (Guid?)value;
                if (setValue == null)
                    Entity.GUID_COMMODITY_CODE = null;
                else if (IsCommodityCodeValid(setValue))
                    Entity.GUID_COMMODITY_CODE = setValue;
            }
        }

        public bool IsCommodityCodeValid(Guid? commodityCodeGuid)
        {
            if (commodityCodeGuid == null)
                return false;

            if (CommodityCodeCollection == null)
                return false;

            return CommodityCodeCollection.Any(x => x.GUID == commodityCodeGuid);
        }

        public IEnumerable<COMMODITY_CODE> CommodityCodeCollection { get; set; }

        //Used for direct property access validation in fill/undo-redo
        public Guid? StockCodeGuid
        {
            get
            {
                return Entity.GUID_STOCK_CODE;
            }
            set
            {
                Guid? setValue = (Guid?)value;
                if (setValue == null)
                {
                    Entity.GUID_STOCK_CODE = null;
                    STOCK_CODE = null;
                }
                else if (IsStockCodeValid(setValue))
                {
                    Entity.GUID_STOCK_CODE = setValue;
                    if(StockCodeCollection != null)
                        STOCK_CODE = StockCodeCollection.FirstOrDefault(x => x.GUID == setValue);
                }
            }
        }

        public bool IsStockCodeValid(Guid? commodityCodeGuid)
        {
            return true;
            //if (commodityCodeGuid == null)
            //    return false;

            //if (StockCodeCollection == null)
            //    return false;

            //return StockCodeCollection.Any(x => x.GUID == commodityCodeGuid);
        }

        public RATE RATE { get; set; }

        public string Deliverable_Name => STOCK_CODE == null ? string.Empty : STOCK_CODE.CODE;

        public string Phase_Code => BluePrintsResources.Default_Construction_Phase;

        public string Commodity_Code => Entity.COMMODITY_CODE == null ? string.Empty : Entity.COMMODITY_CODE.CODE;

        public string Commodity_Display_Code => Entity.COMMODITY_CODE == null ? string.Empty : Entity.COMMODITY_CODE.Display_Code;

        public Guid? Workpack_Guid => Entity.GUID_WORKPACK;

        public Guid? Area_Guid => Entity.GUID_AREA;

        public Guid? SubArea_Guid => Entity.GUID_SUBAREA;

        public decimal Total_Units_IncludingByDuration => Estimated_Units;

        public decimal Estimated_Units => Entity.STOCK_CODE == null ? STOCK_CODE == null ? 0 : STOCK_CODE.HOURS_INSTALL * Entity.ESTIMATED_QUANTITY : Entity.ESTIMATED_QUANTITY * Entity.STOCK_CODE.HOURS_INSTALL;

        public decimal Total_Units => Estimated_Units;

        public Guid OriginalEntityKey { get => Entity.GUID_ORIGINAL; }
        public void SetOriginalEntityKey(Guid newGuid) { }

        public decimal ItemRate => RATE == null || RATE.RATE1 == null ? 0 : (decimal)RATE.RATE1;

        public decimal Estimated_Costs => Estimated_Units * ItemRate;

        public decimal Total_Costs => Estimated_Costs;

        public decimal Estimated_Quantity => Entity.ESTIMATED_QUANTITY;

        public decimal Total_Quantity => Estimated_Quantity;

        public string UOM => STOCK_CODE == null ? string.Empty : STOCK_CODE.UOM;

        public Estimation_DirectProgressType Progress_Type => Entity.PROGRESS_TYPE;

        public decimal Supply_Cost => STOCK_CODE == null ? 0 : STOCK_CODE.RATE_SUPPLY * Estimated_Quantity;

        public decimal Install_Cost => Total_Units * ItemRate;

        public ICollection<P6_ASSIGNMENT> ObservableBASELINE_ITEM_ASSIGNMENT { get; set; }

        private List<P6_ASSIGNMENT> p6_assignments;
        public List<P6_ASSIGNMENT> P6_Assignments
        {
            get
            {
                if (p6_assignments == null)
                    p6_assignments = new List<P6_ASSIGNMENT>();

                return p6_assignments;
            }
            set
            {
                p6_assignments = value;
            }
        }

        public decimal Remaining_Percentage
        {
            get
            {
                return 1 - Assigned_Percentage;
            }
        }

        public decimal Assigned_Percentage
        {
            get
            {
                return P6_Assignments.Sum(x => (x.HIGH_VALUE - (x.LOW_VALUE - 0.01m)));
            }
        }

        public string Discipline_Code => Entity.Discipline_Code;

        public decimal Variation_Units => Entity.Variation_Units;

        public decimal Variation_Costs => 0;

        public string Stock_Code_Type => STOCK_CODE == null ? string.Empty : STOCK_CODE.TYPE;

        public string Stock_Code_Spec => STOCK_CODE == null ? string.Empty : STOCK_CODE.SPEC;

        public string Stock_Code_Description => STOCK_CODE == null ? string.Empty : STOCK_CODE.DESCRIPTION;
    }

    public static class ESTIMATION_DIRECT_ITEMProjectionQueries
    {
        public static IQueryable<ESTIMATION_DIRECT_ITEMProgress> IDeliverable_Progress_Transformation(
            IQueryable<ESTIMATION_DIRECT_ITEM> ESTIMATION_DIRECT_ITEMS,
            IEnumerable<RATE> RATES, PROGRESS PROGRESS, IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS, IEnumerable<STOCK_CODE> STOCK_CODES = null, IEnumerable<COMMODITY_CODE> COMMODITY_CODES = null, IEnumerable<P6_ASSIGNMENT> P6_ASSIGNMENTS = null)
        {
            var PROGRESS_ITEMSByOriginalGuid = PROGRESS_ITEMS.GroupBy(x => x.GUID_ORIBASEITEM).Select(group => new { OriginalGuid = group.Key, Progresses = group.ToList() });
            List<ESTIMATION_DIRECT_ITEMProjection> estimation_direct_item_rates =
            ESTIMATION_DIRECT_ITEMProjectionQueries.IDeliverable_Rates_Transformation(ESTIMATION_DIRECT_ITEMS,
                                                                                            RATES,
                                                                                            STOCK_CODES,
                                                                                            COMMODITY_CODES).ToList();

            List<ESTIMATION_DIRECT_ITEMProgress> estimation_direct_item_progresses = new List<ESTIMATION_DIRECT_ITEMProgress>();
            foreach (ESTIMATION_DIRECT_ITEMProjection estimation_direct_item_rate in estimation_direct_item_rates)
            {
                ESTIMATION_DIRECT_ITEMProgress newEstimation_Direct_itemProgress = new ESTIMATION_DIRECT_ITEMProgress();
                newEstimation_Direct_itemProgress.P6_Assignments = P6_ASSIGNMENTS == null ? null : P6_ASSIGNMENTS.Where(assignment => assignment.GUID_ORIGINAL == estimation_direct_item_rate.OriginalEntityKey).ToList();
                newEstimation_Direct_itemProgress.Live_PROGRESS = PROGRESS;
                newEstimation_Direct_itemProgress.Entity = estimation_direct_item_rate;
                ProgressQueries.SetReportablePROGRESS_ITEM(newEstimation_Direct_itemProgress, PROGRESS_ITEMSByOriginalGuid);
                if(PROGRESS != null)
                    newEstimation_Direct_itemProgress.SetReportingDataDate(PROGRESS.DATA_DATE);
                estimation_direct_item_progresses.Add(newEstimation_Direct_itemProgress);
            }

            return estimation_direct_item_progresses.AsQueryable();
        }

        public static IQueryable<ESTIMATION_DIRECT_ITEMProjection> IDeliverable_Rates_Transformation(
            IQueryable<ESTIMATION_DIRECT_ITEM> ESTIMATION_DIRECT_ITEMS, 
            IEnumerable<RATE> RATES, IEnumerable<STOCK_CODE> STOCK_CODES = null, IEnumerable<COMMODITY_CODE> COMMODITY_CODES = null)
        {
            return
                ESTIMATION_DIRECT_ITEMS.OrderBy(x => x.CREATED).ToArray()
                    .Select(
                        estimate_direct_item =>
                            new ESTIMATION_DIRECT_ITEMProjection()
                            {
                                Entity = estimate_direct_item,
                                STOCK_CODE = STOCK_CODES == null ? null : STOCK_CODES.FirstOrDefault(stockcode => stockcode.GUID == estimate_direct_item.GUID_STOCK_CODE),
                                RATE = RATES.FirstOrDefault(rate => rate.GUID_DISCIPLINE == estimate_direct_item.GUID_DISCIPLINE),
                                StockCodeCollection = STOCK_CODES == null ? null : STOCK_CODES,
                                //.Where(stockcode => stockcode.GUID_DISCIPLINE == estimate_direct_item.GUID_DISCIPLINE),
                                CommodityCodeCollection = COMMODITY_CODES == null ? null : COMMODITY_CODES
                                .Where(commodity_code => commodity_code.GUID_AREA == estimate_direct_item.GUID_AREA 
                                && commodity_code.GUID_SUBAREA == estimate_direct_item.GUID_SUBAREA).OrderBy(x => x.CODE)
                            }).AsQueryable();
        }
    }
}