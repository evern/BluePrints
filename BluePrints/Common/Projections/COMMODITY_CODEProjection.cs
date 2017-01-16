using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    public class COMMODITY_CODEProjection
    {
        public Guid GUID { get; set; }
        public COMMODITY_CODE COMMODITY_CODE { get; set; }
        public bool ISQUANTIFIABLE { get; set; }
    }

    public static class COMMODITY_CODEProjectionQueries
    {
        public static IQueryable<COMMODITY_CODEProjection> transformCOMMODITY_CODE(IQueryable<COMMODITY_CODE> COMMODITY_CODES)
        {
            return COMMODITY_CODES.ToArray().AsQueryable().Select(x => new COMMODITY_CODEProjection() { GUID = x.GUID, COMMODITY_CODE = x });
        }
    }
}
