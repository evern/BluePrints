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
                else if (ESTIMATION_ITEM.COMMODITY.COMMODITY_CODE == null)
                    return Guid.Empty;
                else
                    return ESTIMATION_ITEM.COMMODITY.COMMODITY_CODE.GUID;
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
        public static IQueryable<ESTIMATION_ITEMProjection> JoinCOMMODITYAndRATESOnESTIMATION_ITEMS(IQueryable<ESTIMATION_ITEM> ESTIMATION_ITEMS, Func<ESTIMATION> getESTIMATIONFunc, Func<IQueryable<DEPARTMENT>> getDEPARTMENTSFunc, Func<IQueryable<RATE>> getRATESFunc = null, bool isESTIMATIONQueryProcessed = false)
        {
            ESTIMATION ESTIMATION = getESTIMATIONFunc();
            IQueryable<DEPARTMENT> DEPARTMENTS = getDEPARTMENTSFunc();
            DEPARTMENT constructionDEPARTMENT = null;
            Guid constructionDEPARTMENTGuid = Guid.Empty;
            IQueryable<ESTIMATION_ITEM> contextESTIMATION_ITEMS;
            if (ESTIMATION == null || DEPARTMENTS == null)
                contextESTIMATION_ITEMS = ESTIMATION_ITEMS.Where(x => x.GUID == Guid.Empty);
            else
            {
                constructionDEPARTMENT = DEPARTMENTS.FirstOrDefault(x => x.NAME.ToUpper() == CommonResources.DefaultConstructionDepartment);
                if(constructionDEPARTMENT != null)
                    constructionDEPARTMENTGuid = constructionDEPARTMENT.GUID;

                if (isESTIMATIONQueryProcessed)
                    contextESTIMATION_ITEMS = ESTIMATION_ITEMS;
                else
                    contextESTIMATION_ITEMS = ESTIMATION_ITEMS.Where(x => x.GUID_ESTIMATION == ESTIMATION.GUID);
            }

            IQueryable<RATE> RATES = getRATESFunc();
            return contextESTIMATION_ITEMS.ToArray().AsQueryable().Select(x => new ESTIMATION_ITEMProjection() { GUID = x.GUID, ESTIMATION_ITEM = x, 
                                                                                                                 RATE = RATES.FirstOrDefault(y => y.GUID_DEPARTMENT == constructionDEPARTMENTGuid && y.GUID_DISCIPLINE == x.GUID_DISCIPLINE),
                                                                                                                 ESTIMATION_MARGIN = ESTIMATION.MARGIN,
                                                                                                                 ESTIMATION_CONTINGENCY = ESTIMATION.CONTINGENCY

            });
        }
    }
}
