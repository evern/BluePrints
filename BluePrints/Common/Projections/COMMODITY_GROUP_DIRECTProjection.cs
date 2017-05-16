using BaseModel.Attributes;
using BluePrints.Common.Base;
using BluePrints.Data;
using System;
using System.Linq;

namespace BluePrints.Common.Projections
{
    [ConstraintAttributes("Entity.GUID_PARENT, Entity.GUID_COMMODITYCODE")]
    [RequiredAttributes("Entity.DESCRIPTION")]
    public class COMMODITY_GROUP_DIRECTProjection : BluePrintsProjectionMasterDetailBase<COMMODITY_GROUP_DIRECT, COMMODITY_GROUP_DIRECTProjection>
    {
    }

    public static class COMMODITY_GROUP_DIRECTProjectionQueries
    {
        public static IQueryable<COMMODITY_GROUP_DIRECTProjection> ConvertToProjectionCOMMODITY_GROUP_DIRECT(
            IQueryable<COMMODITY_GROUP_DIRECT> COMMODITY_GROUP_DIRECTS)
        {
            return COMMODITY_GROUP_DIRECTS.ToArray().OrderBy(x => x.CREATED).Select(x => new COMMODITY_GROUP_DIRECTProjection() {EntityKey = x.GUID, Entity = x}).AsQueryable();
        }
    }
}