using BluePrints.Common.Base;
using BluePrints.Data;
using System;
using System.Linq;

namespace BluePrints.Common.Projections
{
    public class COMMODITY_CODEMasterDetailProjection : BluePrintsProjectionMasterDetailBase<COMMODITY_CODE, COMMODITY_CODEMasterDetailProjection>
    {
        public bool ISEXPANDED { get; set; }

        public COMMODITY_CODEProjectionType ProjectionType { get; set; }

        public string GROUP_ID { get; }
        //public string GROUP_ID
        //{
        //    get
        //    {
        //        if (Entity.GUID_COMMODITY_GROUP_DIRECT == null ||
        //            Entity.COMMODITY_GROUP_DIRECT_ID == null)
        //            return string.Empty;
        //        else
        //            return Entity.GUID_COMMODITY_GROUP_DIRECT.ToString() +
        //                   Entity.COMMODITY_GROUP_DIRECT_ID.ToString();
        //    }
        //}

        public bool IsEditable { get; set; }

        public bool HAS_CHILDREN
        {
            get { return DetailEntities.Count > 0; }
        }

        public string COMMODITY_GROUP_CODE_SELECTION { get; }
        //public string COMMODITY_GROUP_CODE_SELECTION
        //{
        //    get
        //    {
        //        return Entity.GUID.ToString() +
        //               (Entity.GUID_COMMODITY_GROUP_DIRECT == null
        //                   ? Guid.Empty.ToString()
        //                   : Entity.GUID_COMMODITY_GROUP_DIRECT.ToString()) +
        //               Entity.COMMODITY_GROUP_DIRECT_ID.ToString();
        //    }
        //}
    }

    public static class COMMODITY_CODEMasterDetailProjectionQueries
    {
        public static IQueryable<COMMODITY_CODEMasterDetailProjection> transformCOMMODITY_CODE(
            IQueryable<COMMODITY_CODE> COMMODITY_CODES)
        {
            return
                COMMODITY_CODES
                    .Select(x => new COMMODITY_CODEMasterDetailProjection() { EntityKey = x.GUID, Entity = x});
        }
    }
}