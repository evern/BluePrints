using BaseModel.Attributes;
using BaseModel.Data.Helpers;
using BluePrints.Common.Base;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BluePrints.Common.Projections
{
    public class ESTIMATION_DIRECT_ITEMProjection : BluePrintsProjectionBase<ESTIMATION_DIRECT_ITEM>
    {
        public ESTIMATION_DIRECT_ITEMProjection()
            : base()
        {
            //need to initialize commodity code here so that copy/paste is able to get property info within COMMODITY_CODE
            commodity_code = new COMMODITY_CODE();
        }

        public RATE RATE { get; set; }

        private COMMODITY_CODE commodity_code;
        public COMMODITY_CODE COMMODITY_CODE
        {
            get
            {
                return commodity_code;
            }
            set
            {
                //Always go by value
                DataUtils.ShallowCopy(commodity_code, value);
            }
        }

        public decimal ItemRate
        {
            get
            {
                if (RATE == null || RATE.RATE1 == null)
                    return 0;

                return (decimal)RATE.RATE1;
            }
        }

        public decimal Total_Install_Hours
        {
            get
            {
                if (COMMODITY_CODE == null || COMMODITY_CODE.HOURS_INSTALL == 0)
                    return 0;

                return COMMODITY_CODE.HOURS_INSTALL;
            }
        }

        public decimal Supply_Cost
        {
            get
            {
                if (COMMODITY_CODE == null || COMMODITY_CODE.RATE_SUPPLY == 0)
                    return 0;

                return COMMODITY_CODE.RATE_SUPPLY * Entity.ESTIMATED_QUANTITY;
            }
        }

        public decimal Install_Cost
        {
            get
            {
                if (Entity == null)
                    return 0;


                return Entity.ESTIMATED_QUANTITY * Total_Install_Hours * ItemRate;
            }
        }

        public IEnumerable<COMMODITY_CODE> CommodityCodeCollection { get; set; }

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

        public IEnumerable<STOCK_CODE> StockCodeCollection { get; set; }

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
            IQueryable<ESTIMATION_DIRECT_ITEM> ESTIMATION_DIRECT_ITEMS, ESTIMATION_DIRECT ESTIMATION_DIRECT,
            IEnumerable<RATE> RATES, IEnumerable<COMMODITY_CODE> COMMODITY_CODES, IEnumerable<STOCK_CODE> STOCK_CODES)
        {
            return
                ESTIMATION_DIRECT_ITEMS.OrderBy(x => x.CREATED).ToArray()
                    .Select(
                        estimate_direct_item =>
                            new ESTIMATION_DIRECT_ITEMProjection()
                            {
                                Entity = estimate_direct_item,
                                COMMODITY_CODE = COMMODITY_CODES.FirstOrDefault(commoditycode => commoditycode.GUID == estimate_direct_item.GUID_COMMODITY_CODE),
                                RATE = RATES.FirstOrDefault(rate => rate.GUID_DISCIPLINE == estimate_direct_item.GUID_DISCIPLINE),
                                CommodityCodeCollection = COMMODITY_CODES.Where(commoditycode => commoditycode.GUID_DISCIPLINE == estimate_direct_item.GUID_DISCIPLINE),
                                StockCodeCollection = STOCK_CODES
                                .Where(stockcode =>
                                stockcode.GUID_AREA == estimate_direct_item.GUID_AREA 
                                && stockcode.GUID_SUBAREA == estimate_direct_item.GUID_SUBAREA 
                                && stockcode.GUID_DISCIPLINE == estimate_direct_item.GUID_DISCIPLINE)
                            }).AsQueryable();
        }
    }
}