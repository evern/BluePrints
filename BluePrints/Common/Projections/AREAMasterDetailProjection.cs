using BaseModel.Attributes;
using BluePrints.Common.Base;
using BluePrints.Data;
using System;
using System.Linq;

namespace BluePrints.Common.Projections
{
    [ConstraintAttributes("Entity.INTERNAL_NUM")]
    public class AREAMasterDetailProjection : BluePrintsProjectionMasterDetailBase<AREA, AREAMasterDetailProjection>
    {

    }

    public static class AREAMasterDetailProjectionQueries
    {
        public static IQueryable<AREAMasterDetailProjection> transformAREA(
            IQueryable<AREA> AREAS)
        {
            return
                AREAS
                    .Select(x => new AREAMasterDetailProjection() { Entity = x});
        }
    }
}