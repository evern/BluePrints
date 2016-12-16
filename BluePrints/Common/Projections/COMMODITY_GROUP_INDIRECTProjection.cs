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
    public class COMMODITY_GROUP_INDIRECTProjection
    {
        public COMMODITY_GROUP_INDIRECTProjection()
        {
            COMMODITY_GROUP = ViewModelSource.Create(() => new COMMODITY_GROUP_INDIRECT());
            CHILD_COMMODITY_GROUP = new ObservableCollection<COMMODITY_GROUP_INDIRECTProjection>();
        }

        [Key]
        public Guid GUID { get; set; }

        public COMMODITY_GROUP_INDIRECT COMMODITY_GROUP { get; set; }

        public ObservableCollection<COMMODITY_GROUP_INDIRECTProjection> CHILD_COMMODITY_GROUP { get; set; }

        public RATE RATE { get; set; }

        public decimal ITEMRATE
        {
            get
            {
                if (RATE == null || RATE.RATE1 == null)
                    return 0;

                return (decimal)RATE.RATE1;
            }
        }

        public decimal OPERATOR_COSTS
        {
            get
            {
                if (COMMODITY_GROUP == null || COMMODITY_GROUP.OPERATOR_RATE == null)
                    return 0;

                if (RATE == null || RATE.RATE1 == null)
                    return 0;

                return (decimal)COMMODITY_GROUP.OPERATOR_RATE;
            }
        }

        public override string ToString()
        {
            return COMMODITY_GROUP.DESCRIPTION;
        }
    }

    public static class COMMODITY_GROUP_INDIRECTProjectionQueries
    {
        public static IQueryable<COMMODITY_GROUP_INDIRECTProjection> ConvertToProjectionCOMMODITY_GROUP_INDIRECT(IQueryable<COMMODITY_GROUP_INDIRECT> COMMODITY_GROUP_INDIRECTS)
        {
            IEnumerable<COMMODITY_GROUP_INDIRECT> allCOMMODITY_GROUP_INDIRECTS = COMMODITY_GROUP_INDIRECTS.ToArray().OrderBy(x => x.DESCRIPTION).AsEnumerable();
            return allCOMMODITY_GROUP_INDIRECTS.Select(x => new COMMODITY_GROUP_INDIRECTProjection() { GUID = x.GUID, COMMODITY_GROUP = x }).AsQueryable();
        }
    }
}
