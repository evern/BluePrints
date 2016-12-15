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

        public COMMODITY_GROUP_DIRECT COMMODITY_GROUP { get; set; }

        public ObservableCollection<COMMODITY_GROUP_DIRECTProjection> CHILD_COMMODITY_GROUP { get; set; }

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

        public decimal INSTALL_COSTS
        {
            get
            {
                if (COMMODITY_GROUP == null || COMMODITY_GROUP.HOURS_INSTALL == null)
                    return 0;

                if (RATE == null || RATE.RATE1 == null)
                    return 0;

                return (decimal)COMMODITY_GROUP.HOURS_INSTALL * (decimal)RATE.RATE1;
            }
        }

        public override string ToString()
        {
            return COMMODITY_GROUP.DESCRIPTION;
        }
    }

    public static class COMMODITY_GROUP_DIRECTProjectionQueries
    {
        public static IQueryable<COMMODITY_GROUP_DIRECTProjection> ConvertToProjectionCOMMODITY_GROUP_DIRECT(IQueryable<COMMODITY_GROUP_DIRECT> COMMODITY_GROUP_DIRECTS, Func<IQueryable<RATE>> getRATES_ByProjectFunc = null)
        {
            IEnumerable<COMMODITY_GROUP_DIRECT> allCOMMODITY_GROUP_DIRECTS = COMMODITY_GROUP_DIRECTS.ToArray().OrderBy(x => x.DESCRIPTION).AsEnumerable();
            IQueryable<RATE> RATES = getRATES_ByProjectFunc();
            return allCOMMODITY_GROUP_DIRECTS.Select(x => new COMMODITY_GROUP_DIRECTProjection() { GUID = x.GUID, COMMODITY_GROUP = x, RATE = RATES.FirstOrDefault(y => y.GUID_DEPARTMENT.ToString() == CommonResources.DefaultConstructionDepartment && y.GUID_DISCIPLINE == x.GUID_DISCIPLINE) }).AsQueryable();
        }
    }
}
