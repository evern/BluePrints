using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    public class COMMODITY_CODE_ProjectSpecificProjection
    {
        public COMMODITY_CODE_ProjectSpecificProjection()
        {
            COMMODITY_CODE = new COMMODITY_CODE();
            CHILD_COMMODITY_CODES = new ObservableCollection<COMMODITY_CODE_ProjectSpecificProjection>();
        }

        public Guid GUID { get; set; }

        public COMMODITY_CODE COMMODITY_CODE { get; set; }

        public ObservableCollection<COMMODITY_CODE_ProjectSpecificProjection> CHILD_COMMODITY_CODES { get; set; }

        public bool ISEXPANDED { get; set; }

        public bool ISGENERATED { get; set; }

        public string GROUP_ID
        {
            get
            {
                if (this.COMMODITY_CODE.GUID_COMMODITY_GROUP_DIRECT == null || this.COMMODITY_CODE.COMMODITY_GROUP_DIRECT_ID == null)
                    return string.Empty;
                else
                    return COMMODITY_CODE.GUID_COMMODITY_GROUP_DIRECT.ToString() + COMMODITY_CODE.COMMODITY_GROUP_DIRECT_ID.ToString();
            }
        }

        public bool ISQUANTIFIABLE
        {
            get { return !ISGENERATED; }
        }

        public bool HAS_CHILDREN
        {
            get { return CHILD_COMMODITY_CODES.Count > 0; }
        }
    }

    public static class COMMODITY_CODE_ProjectSpecific_ProjectionQueries
    {
        public static IQueryable<COMMODITY_CODE_ProjectSpecificProjection> transformCOMMODITY_CODE(IQueryable<COMMODITY_CODE> COMMODITY_CODES)
        {
            return COMMODITY_CODES.ToArray().AsQueryable().Select(x => new COMMODITY_CODE_ProjectSpecificProjection() { GUID = x.GUID, COMMODITY_CODE = x });
        }
    }
}
