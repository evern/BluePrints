using BaseModel.Attributes;
using BaseModel.Data.Helpers;
using BluePrints.Common.Base;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BluePrints.Common.Projections
{
    public class ESTIMATION_DIRECT_ITEMProjection : BluePrintsProjectionBase<ESTIMATION_DIRECT_ITEM>, ITrackableQuantityDeliverableProjection, ICanUpdate
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
                    Entity.GUID_STOCK_CODE = null;
                else if (IsStockCodeValid(setValue))
                    Entity.GUID_STOCK_CODE = setValue;
            }
        }

        public bool IsStockCodeValid(Guid? commodityCodeGuid)
        {
            if (commodityCodeGuid == null)
                return false;

            if (StockCodeCollection == null)
                return false;

            return StockCodeCollection.Any(x => x.GUID == commodityCodeGuid);
        }

        public RATE RATE { get; set; }

        public string ReportableItem_Name => Entity.COMMODITY_CODE == null ? string.Empty : Entity.COMMODITY_CODE.CODE;

        public string Commodity_Code => Entity.COMMODITY_CODE == null ? string.Empty : Entity.COMMODITY_CODE.CODE;

        public Guid? Workpack_Guid => Entity.GUID_WORKPACK;

        public string Stock_Code => Entity.STOCK_CODE == null ? string.Empty : Entity.STOCK_CODE.CODE;

        public Guid? Area_Guid => Entity.GUID_AREA;

        public Guid? SubArea_Guid => Entity.GUID_SUBAREA;

        public decimal TotalHoursIncludeByDuration => EstimatedHours;

        public decimal EstimatedHours => Entity.STOCK_CODE == null ? 0 : Entity.ESTIMATED_QUANTITY * STOCK_CODE.HOURS_INSTALL;

        public decimal TotalHours => EstimatedHours;

        public Guid OriginalEntityKey { get => Entity.GUID_ORIGINAL; }
        public void SetOriginalEntityKey(Guid newGuid) { }

        public decimal ItemRate => RATE == null || RATE.RATE1 == null ? 0 : (decimal)RATE.RATE1;

        public decimal EstimatedCosts => EstimatedHours * ItemRate;

        public decimal TotalCosts => EstimatedCosts;

        public decimal Estimated_Quantity => Entity.ESTIMATED_QUANTITY;

        public decimal Total_Quantity => Estimated_Quantity;

        public string UOM => Entity.COMMODITY_CODE == null ? string.Empty : Entity.COMMODITY_CODE.UOM;

        public bool? Track => Entity.TRACK;

        public decimal Supply_Cost => Entity.COMMODITY_CODE == null ? 0 : STOCK_CODE.RATE_SUPPLY * Estimated_Quantity;

        public decimal Install_Cost => TotalHours * ItemRate;

        public ICollection<BASELINE_ITEM_ASSIGNMENT> ObservableBASELINE_ITEM_ASSIGNMENT { get; set; }

        private List<BASELINE_ITEM_ASSIGNMENT> p6assignments;
        public List<BASELINE_ITEM_ASSIGNMENT> P6Assignments
        {
            get
            {
                return p6assignments;
            }
            set
            {
                if (p6assignments == null)
                    p6assignments = new List<BASELINE_ITEM_ASSIGNMENT>();

                p6assignments = value;
            }
        }

        public decimal Remaining_Percentage
        {
            get
            {
                return 1 - AssignedPercentage;
            }
        }

        public decimal AssignedPercentage
        {
            get
            {
                return P6Assignments.Sum(x => (x.HIGH_VALUE - (x.LOW_VALUE - 0.01m)));
            }
        }

        /// <summary>
        /// Refreshes current row
        /// </summary>
        public void Update()
        {
            RaisePropertyChanged();
        }
    }

    public static class ESTIMATION_DIRECT_ITEMProjectionQueries
    {
        public static IQueryable<ESTIMATION_DIRECT_ITEMProjection> ESTIMATION_DIRECT_ITEMProjectionQuery(
            IQueryable<ESTIMATION_DIRECT_ITEM> ESTIMATION_DIRECT_ITEMS, 
            IEnumerable<RATE> RATES, IEnumerable<STOCK_CODE> projectSTOCK_CODES, IEnumerable<COMMODITY_CODE> projectCOMMODITY_CODES)
        {
            return
                ESTIMATION_DIRECT_ITEMS.OrderBy(x => x.CREATED).ToArray()
                    .Select(
                        estimate_direct_item =>
                            new ESTIMATION_DIRECT_ITEMProjection()
                            {
                                Entity = estimate_direct_item,
                                STOCK_CODE = projectSTOCK_CODES.FirstOrDefault(commoditycode => commoditycode.GUID == estimate_direct_item.GUID_COMMODITY_CODE),
                                RATE = RATES.FirstOrDefault(rate => rate.GUID_DISCIPLINE == estimate_direct_item.GUID_DISCIPLINE),
                                StockCodeCollection = projectSTOCK_CODES
                                .Where(commoditycode => 
                                commoditycode.GUID_DISCIPLINE == estimate_direct_item.GUID_DISCIPLINE),
                                CommodityCodeCollection = projectCOMMODITY_CODES
                                .Where(commodity_code =>
                                commodity_code.GUID_AREA == estimate_direct_item.GUID_AREA 
                                && commodity_code.GUID_SUBAREA == estimate_direct_item.GUID_SUBAREA 
                                && commodity_code.GUID_DISCIPLINE == estimate_direct_item.GUID_DISCIPLINE)
                            }).AsQueryable();
        }
    }
}