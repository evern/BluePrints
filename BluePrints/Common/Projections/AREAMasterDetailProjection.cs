using BaseModel.Attributes;
using BluePrints.Common.Base;
using BluePrints.Data;
using System;
using System.Linq;

namespace BluePrints.Common.Projections
{
    [ConstraintAttributes("Entity.INTERNAL_NUM")]
    [RequiredAttributes("Entity.TITLE, Entity.INTERNAL_NUM")]
    //[ConstraintAttributes("Entity.GUID_PARENT, Entity.INTERNAL_NUM")]
    public class AREAMasterDetailProjection : BluePrintsProjectionMasterDetailBase<AREA, AREAMasterDetailProjection>
    {

    }

    public static class AREAMasterDetailProjectionQueries
    {
        public static IQueryable<AREAMasterDetailProjection> Area_Master_Detail_Transformation(
            IQueryable<AREA> AREAS, Guid projectGuid)
        {
            int i = AREAS.Where(x => x.GUID_PROJECT == projectGuid).Count();
            return AREAS.Where(x => x.GUID_PROJECT == projectGuid).ToArray().Select(x => new AREAMasterDetailProjection() { Entity = x}).AsQueryable();
        }
    }
}