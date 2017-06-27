using BaseModel.Attributes;
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
            IEnumerable<RATE> RATES)
        {
            return
                ESTIMATION_DIRECT_ITEMS.ToArray()
                    .Select(
                        estimate_direct_item =>
                            new ESTIMATION_DIRECT_ITEMProjection()
                            {
                                Entity = estimate_direct_item,
                                RATE = RATES.FirstOrDefault(rate => rate.GUID_DISCIPLINE == estimate_direct_item.GUID_DISCIPLINE)
                            }).AsQueryable();
        }
    }
}