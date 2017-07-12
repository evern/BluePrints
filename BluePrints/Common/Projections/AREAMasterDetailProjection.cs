using BaseModel.Attributes;
using BluePrints.Common.Base;
using BluePrints.Data;
using System;
using System.Linq;

namespace BluePrints.Common.Projections
{
    [ConstraintAttributes("Entity.GUID_PARENT, Entity.INTERNAL_NUM")]
    public class AREAMasterDetailProjection : BluePrintsProjectionMasterDetailBase<AREA, AREAMasterDetailProjection>
    {

    }

    public static class AREAMasterDetailProjectionQueries
    {
        public static IQueryable<AREAMasterDetailProjection> transformAREA(
            IQueryable<AREA> AREAS, Guid projectGuid)
        {
            return AREAS.ToArray().Where(x => x.GUID_PROJECT == projectGuid).Select(x => new AREAMasterDetailProjection() { Entity = x}).AsQueryable();
        }
    }
}