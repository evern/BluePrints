using BluePrints.Common.Base;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    public class COMMODITY_GROUP_CODEProjection : BluePrintsProjectionMasterDetailBase<COMMODITY_CODE, COMMODITY_GROUP_CODEProjection>
    {
    }

    public static class COMMODITY_GROUP_CODEProjectionQueries
    {
        public static IQueryable<COMMODITY_GROUP_CODEProjection> ConvertToProjectionCOMMODITY_GROUP_CODE(
            IQueryable<COMMODITY_CODE> COMMODITY_CODES)
        {
            List<COMMODITY_GROUP_CODEProjection> commodityGroupCodes = COMMODITY_CODES.ToArray().OrderBy(x => x.CREATED).Select(x => new COMMODITY_GROUP_CODEProjection() { EntityKey = x.GUID, Entity = x }).ToList();
            commodityGroupCodes.ForEach(x => x.Entity.SetUseGroupParent());
            return commodityGroupCodes.AsQueryable();
        }
    }
}
