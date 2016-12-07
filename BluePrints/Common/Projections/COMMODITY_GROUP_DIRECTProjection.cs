using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    public class COMMODITY_GROUP_DIRECTProjection
    {
        public COMMODITY_GROUP_DIRECTProjection()
        {
            COMMODITY_GROUP = new COMMODITY_GROUP_DIRECT();
            CHILD_COMMODITY_GROUP = new ObservableCollection<COMMODITY_GROUP_DIRECTProjection>();
        }

        public Guid GUID { get; set; }

        public COMMODITY_GROUP_DIRECT COMMODITY_GROUP { get; set; }

        public ObservableCollection<COMMODITY_GROUP_DIRECTProjection> CHILD_COMMODITY_GROUP { get; set; }

        public bool ISQUANTIFIABLE
        {
            get
            {
                return COMMODITY_GROUP.GUID_COMMODITYCODE == null;
            }
        }
    }

    public static class COMMODITY_GROUP_DIRECTProjectionQueries
    {
        public static IQueryable<COMMODITY_GROUP_DIRECTProjection> ConvertToProjectionCOMMODITY_GROUP_DIRECT(IQueryable<COMMODITY_GROUP_DIRECT> COMMODITY_GROUP_DIRECTS)
        {
            IEnumerable<COMMODITY_GROUP_DIRECT> allCOMMODITY_GROUP_DIRECTS = COMMODITY_GROUP_DIRECTS.ToArray().AsEnumerable();
            return allCOMMODITY_GROUP_DIRECTS.Select(x => new COMMODITY_GROUP_DIRECTProjection() { GUID = x.GUID, COMMODITY_GROUP = x }).AsQueryable();
        }
    }
}
