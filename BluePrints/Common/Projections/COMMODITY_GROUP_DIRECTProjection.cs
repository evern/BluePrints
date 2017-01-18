using BluePrints.Data;
using BluePrints.Data.Attributes;
using BluePrints.Data.Helpers;
using DevExpress.Mvvm.POCO;
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
    [ConstraintAttributes("COMMODITY_GROUP.GUID_PARENT, COMMODITY_GROUP.GUID_COMMODITYCODE")]
    [RequiredAttributes("COMMODITY_GROUP.DESCRIPTION")]
    public class COMMODITY_GROUP_DIRECTProjection
    {
        public COMMODITY_GROUP_DIRECTProjection()
        {
            COMMODITY_GROUP = ViewModelSource.Create(() => new COMMODITY_GROUP_DIRECT());
            CHILD_COMMODITY_GROUP = new ObservableCollection<COMMODITY_GROUP_DIRECTProjection>();
        }

        [Key]
        public Guid GUID { get; set; }

        public Guid? GUID_PROJECT { get; set; }

        public COMMODITY_GROUP_DIRECT COMMODITY_GROUP { get; set; }

        public ObservableCollection<COMMODITY_GROUP_DIRECTProjection> CHILD_COMMODITY_GROUP { get; set; }

        public override string ToString()
        {
            return COMMODITY_GROUP.DESCRIPTION;
        }

        public string ConcatenatedGUID
        {
            get { return COMMODITY_GROUP.GUID.ToString() + (COMMODITY_GROUP.GUID_COMMODITYCODE == null ? Guid.Empty.ToString() : COMMODITY_GROUP.GUID_COMMODITYCODE.ToString()); }
        }

        public bool ISEXPANDED { get; set; }
    }

    public static class COMMODITY_GROUP_DIRECTProjectionQueries
    {
        public static IQueryable<COMMODITY_GROUP_DIRECTProjection> ConvertToProjectionCOMMODITY_GROUP_DIRECT(IQueryable<COMMODITY_GROUP_DIRECT> COMMODITY_GROUP_DIRECTS)
        {
            IEnumerable<COMMODITY_GROUP_DIRECT> allCOMMODITY_GROUP_DIRECTS = COMMODITY_GROUP_DIRECTS.ToArray().OrderBy(x => x.DESCRIPTION).AsEnumerable();
            return allCOMMODITY_GROUP_DIRECTS.Select(x => new COMMODITY_GROUP_DIRECTProjection() { GUID = x.GUID, COMMODITY_GROUP = x }).AsQueryable();
        }
    }
}
