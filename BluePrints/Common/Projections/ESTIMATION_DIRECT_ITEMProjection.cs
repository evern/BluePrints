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
                if (commodity_code == null)
                    commodity_code = new COMMODITY_CODE();

                //Always go by value
                DataUtils.ShallowCopy(commodity_code, value);
            }
        }

        public decimal ITEMRATE
        {
            get
            {
                if (RATE == null || RATE.RATE1 == null)
                    return 0;

                return (decimal)RATE.RATE1;
            }
        }

        public decimal ESTIMATED_COSTS
        {
            get
            {
                if (Entity == null)
                    return 0;

                if (RATE == null || RATE.RATE1 == null)
                    return 0;

                return Entity.ESTIMATED_QUANTITY * (decimal)RATE.RATE1;
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
        public static IQueryable<ESTIMATION_DIRECT_ITEMProjection> BASELINE_ITEMProjectionQuery(
            IQueryable<ESTIMATION_DIRECT_ITEM> ESTIMATION_DIRECT_ITEMS, ESTIMATION_DIRECT ESTIMATION_DIRECT,
            IEnumerable<RATE> RATES, IEnumerable<COMMODITY_CODE> COMMODITY_CODES)
        {
            return
                ESTIMATION_DIRECT_ITEMS.ToArray()
                    .Select(
                        estimate_direct_item =>
                            new ESTIMATION_DIRECT_ITEMProjection()
                            {
                                Entity = estimate_direct_item,
                                COMMODITY_CODE = COMMODITY_CODES.FirstOrDefault(commoditycode => commoditycode.GUID == estimate_direct_item.GUID_COMMODITY_CODE),
                                RATE = RATES.FirstOrDefault(rate => rate.GUID_DISCIPLINE == estimate_direct_item.GUID_DISCIPLINE)
                            }).AsQueryable();
        }
    }
}