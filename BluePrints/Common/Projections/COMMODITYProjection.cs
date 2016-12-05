using BluePrints.Data;
using BluePrints.Data.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    [ConstraintAttributes("COMMODITY.GUID_PROJECT, COMMODITY.ITEM_DESC")]
    public class COMMODITYProjection
    {
        public COMMODITYProjection()
        {
            COMMODITY = new COMMODITY();
        }

        [Key]
        public Guid GUID { get; set; }

        public COMMODITY COMMODITY { get; set; }

        public Guid GUID_PARENT
        {
            get
            {
                if (COMMODITY == null || COMMODITY.GUID_PARENT == null)
                    return Guid.Empty;
                else
                    return (Guid)COMMODITY.GUID_PARENT;
            }
        }

        public bool ISREADONLY { get; set; }

        public bool ISEXPANDED { get; set; }
    }

    public static class COMMODITYProjectionQueries
    {
        public static IQueryable<COMMODITYProjection> ConvertToProjectionCOMMODITIES(IQueryable<COMMODITY> COMMODITIES)
        {
            IEnumerable<COMMODITY> readCOMMODITIES = COMMODITIES.ToArray().AsEnumerable();
            return readCOMMODITIES.Select(x => new COMMODITYProjection() { GUID = x.GUID, COMMODITY = x, ISREADONLY = readCOMMODITIES.Any(y => y.GUID_PARENT == x.GUID), ISEXPANDED = true }).AsQueryable();
        }
    }
}
