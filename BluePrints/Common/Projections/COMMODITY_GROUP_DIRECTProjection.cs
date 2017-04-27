using BluePrints.Data;
using BluePrints.Data.Attributes;
using System;
using System.Linq;

namespace BluePrints.Common.Projections
{
    [ConstraintAttributes("Entity.GUID_PARENT, Entity.GUID_COMMODITYCODE")]
    [RequiredAttributes("Entity.DESCRIPTION")]
    public class COMMODITY_GROUP_DIRECTProjection : ProjectionMasterDetailBase<COMMODITY_GROUP_DIRECT, COMMODITY_GROUP_DIRECTProjection>
    {
        public Guid? GUID_PROJECT { get; set; }

        public override string ToString()
        {
            return Entity.DESCRIPTION;
        }

        public bool ISEXPANDED { get; set; }
    }

    public static class COMMODITY_GROUP_DIRECTProjectionQueries
    {
        public static IQueryable<COMMODITY_GROUP_DIRECTProjection> ConvertToProjectionCOMMODITY_GROUP_DIRECT(
            IQueryable<COMMODITY_GROUP_DIRECT> COMMODITY_GROUP_DIRECTS)
        {
            var allCOMMODITY_GROUP_DIRECTS =
                COMMODITY_GROUP_DIRECTS.OrderBy(x => x.DESCRIPTION);
            return
                allCOMMODITY_GROUP_DIRECTS.Select(
                    x => new COMMODITY_GROUP_DIRECTProjection() {GUID = x.GUID, Entity = x});
        }
    }
}