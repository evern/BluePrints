using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.DataModel;
using BluePrints.Common.ViewModel;
using BluePrints.Data;
using BluePrints.Data.Attributes;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    [ConstraintAttributes("ESTIMATION_ITEM.GUID_COMMODITY")]
    public class ESTIMATION_ITEMProjection
    {
        public ESTIMATION_ITEMProjection()
        {
            ESTIMATION_ITEM = new ESTIMATION_ITEM();
            ESTIMATION_ITEM.COMMODITY = new COMMODITY();
        }

        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_ORIGINAL
        {
            get
            {
                if (ESTIMATION_ITEM == null || ESTIMATION_ITEM.GUID_ORIGINAL == null)
                    return Guid.Empty;
                else
                    return (Guid)ESTIMATION_ITEM.GUID_ORIGINAL;
            }
        }

        public Guid GUID_PARENT
        {
            get
            {
                if (ESTIMATION_ITEM == null || ESTIMATION_ITEM.GUID_PARENT == null)
                    return Guid.Empty;
                else
                    return (Guid)ESTIMATION_ITEM.GUID_PARENT;
            }
        }

        public bool ISREADONLY { get; set; }

        public bool ISEXPANDED { get; set; }

        public Guid GUID_COMMODITY
        {
            get
            {
                if (ESTIMATION_ITEM == null)
                    return Guid.Empty;
                else if (ESTIMATION_ITEM.COMMODITY == null)
                    return Guid.Empty;
                else
                    return ESTIMATION_ITEM.GUID_COMMODITY;
            }
        }

        public Guid GUID_COMMODITYCODE
        {
            get
            {
                if (ESTIMATION_ITEM == null)
                    return Guid.Empty;
                else if (ESTIMATION_ITEM.COMMODITY == null)
                    return Guid.Empty;
                else
                    return ESTIMATION_ITEM.COMMODITY.GUID_COMMODITYCODE;
            }
        }

        public ESTIMATION_ITEM ESTIMATION_ITEM { get; set; }
        public decimal ESTIMATION_MARGIN { get; set; }
        public decimal ESTIMATION_CONTINGENCY { get; set; }
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

        public decimal INSTALL_COST
        {
            get 
            {
                return ITEMRATE * ESTIMATION_ITEM.TOTAL_QUANTITY;
            }
        }

        public decimal SUB_TOTAL
        {
            get
            {
                return INSTALL_COST + ESTIMATION_ITEM.SUPPLY_COST + ESTIMATION_ITEM.FREIGHT_COST;
            }
        }

        public decimal TOTAL_COST
        {
            get
            {
                return SUB_TOTAL * (1 + ESTIMATION_MARGIN + ESTIMATION_CONTINGENCY);
            }
        }
    }

    public static class ESTIMATION_ITEMProjectionQueries
    {
        public static IQueryable<ESTIMATION_ITEMProjection> JoinCOMMODITYAndRATESOnESTIMATION_ITEMS(IQueryable<ESTIMATION_ITEM> ESTIMATION_ITEMS, Func<ESTIMATION> getESTIMATIONFunc, Func<DEPARTMENT> getDEPARTMENTFunc, Func<IQueryable<RATE>> getRATESFunc = null, bool isESTIMATIONQueryProcessed = false)
        {
            ESTIMATION ESTIMATION = getESTIMATIONFunc();
            DEPARTMENT DEPARTMENT = getDEPARTMENTFunc();
            IQueryable<ESTIMATION_ITEM> contextESTIMATION_ITEMS;
            if (ESTIMATION == null || DEPARTMENT == null)
                contextESTIMATION_ITEMS = ESTIMATION_ITEMS.Where(x => x.GUID == Guid.Empty);
            else
            {
                if (isESTIMATIONQueryProcessed)
                    contextESTIMATION_ITEMS = ESTIMATION_ITEMS;
                else
                    contextESTIMATION_ITEMS = ESTIMATION_ITEMS.Where(x => x.GUID_ESTIMATION == ESTIMATION.GUID);
            }

            IQueryable<RATE> RATES = getRATESFunc();
            return contextESTIMATION_ITEMS.ToArray().AsQueryable().Select(x => new ESTIMATION_ITEMProjection() { GUID = x.GUID, ESTIMATION_ITEM = x, 
                                                                                                                 RATE = RATES.FirstOrDefault(y => y.GUID_DEPARTMENT == DEPARTMENT.GUID && y.GUID_DISCIPLINE == x.GUID_DISCIPLINE),
                                                                                                                 ESTIMATION_MARGIN = ESTIMATION.MARGIN,
                                                                                                                 ESTIMATION_CONTINGENCY = ESTIMATION.CONTINGENCY,
                                                                                                                 ISEXPANDED = true,
                                                                                                                 ISREADONLY = contextESTIMATION_ITEMS.Any(y => y.GUID_PARENT == x.GUID_ORIGINAL)

            });
        }
    }
}
