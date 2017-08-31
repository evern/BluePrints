using BaseModel.Attributes;
using BaseModel.Misc;
using BluePrints.Common.Base;
using BluePrints.Data;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace BluePrints.Common.Projections
{
    //[ConstraintAttributes("Entity.GUID_PARENT, Entity.INTERNAL_NUM")]
    public class MINUTE_AGENDAMasterDetailProjection : BluePrintsProjectionMasterDetailBase<MINUTE_AGENDA, MINUTE_AGENDAMasterDetailProjection>
    {

    }

    public static class MINUTE_AGENDAMasterDetailProjectionQueries
    {
        public static IQueryable<MINUTE_AGENDAMasterDetailProjection> MINUTE_AGENDA_Master_Detail_Transformation(
            IQueryable<MINUTE_AGENDA> MINUTE_AGENDAS, Guid projectGuid)
        {
            return MINUTE_AGENDAS.Where(x => x.GUID_PROJECT == projectGuid).ToArray().Select(x => new MINUTE_AGENDAMasterDetailProjection() { Entity = x }).AsQueryable();
        }
    }
}