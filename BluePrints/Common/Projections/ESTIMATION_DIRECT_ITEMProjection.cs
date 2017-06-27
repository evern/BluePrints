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
                        x =>
                            new ESTIMATION_DIRECT_ITEMProjection()
                            {
                                Entity = x
                            }).AsQueryable();
        }
    }
}