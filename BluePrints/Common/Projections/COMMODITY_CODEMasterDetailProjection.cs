using BluePrints.Common.ViewModel;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    public class COMMODITY_CODEMasterDetailProjection : IHaveGUID
    {
        public COMMODITY_CODEMasterDetailProjection()
        {
            COMMODITY_CODE = new COMMODITY_CODE();
            CHILD_COMMODITY_CODES = new ObservableCollection<COMMODITY_CODEMasterDetailProjection>();
        }

        public Guid GUID { get; set; }

        public COMMODITY_CODE COMMODITY_CODE { get; set; }

        public ObservableCollection<COMMODITY_CODEMasterDetailProjection> CHILD_COMMODITY_CODES { get; set; }

        public bool ISEXPANDED { get; set; }

        public COMMODITY_CODEProjectionType ProjectionType { get; set; }

        public string GROUP_ID
        {
            get
            {
                if (COMMODITY_CODE.GUID_COMMODITY_GROUP_DIRECT == null ||
                    COMMODITY_CODE.COMMODITY_GROUP_DIRECT_ID == null)
                    return string.Empty;
                else
                    return COMMODITY_CODE.GUID_COMMODITY_GROUP_DIRECT.ToString() +
                           COMMODITY_CODE.COMMODITY_GROUP_DIRECT_ID.ToString();
            }
        }

        public bool IsEditable { get; set; }

        public bool HAS_CHILDREN
        {
            get { return CHILD_COMMODITY_CODES.Count > 0; }
        }

        public string COMMODITY_GROUP_CODE_SELECTION
        {
            get
            {
                return COMMODITY_CODE.GUID.ToString() +
                       (COMMODITY_CODE.GUID_COMMODITY_GROUP_DIRECT == null
                           ? Guid.Empty.ToString()
                           : COMMODITY_CODE.GUID_COMMODITY_GROUP_DIRECT.ToString()) +
                       COMMODITY_CODE.COMMODITY_GROUP_DIRECT_ID.ToString();
            }
        }
    }

    public static class COMMODITY_CODEMasterDetailProjectionQueries
    {
        public static IQueryable<COMMODITY_CODEMasterDetailProjection> transformCOMMODITY_CODE(
            IQueryable<COMMODITY_CODE> COMMODITY_CODES)
        {
            return
                COMMODITY_CODES.ToArray()
                    .AsQueryable()
                    .Select(x => new COMMODITY_CODEMasterDetailProjection() {GUID = x.GUID, COMMODITY_CODE = x});
        }
    }
}