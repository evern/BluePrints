using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.DataModel;
using BluePrints.Common.ViewModel;
using BluePrints.Data;
using BluePrints.Data.Attributes;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    [ConstraintAttributes("ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_GROUP_DIRECT")]
    [RequiredAttributes("ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_GROUP_DIRECT")]
    public class ESTIMATION_DIRECT_ITEMProjection
    {
        public ESTIMATION_DIRECT_ITEMProjection()
        {
            ESTIMATION_DIRECT_ITEM = new ESTIMATION_DIRECT_ITEM();
            CHILD_ESTIMATION_DIRECT_ITEM = new ObservableCollection<ESTIMATION_DIRECT_ITEMProjection>();
        }

        [Key]
        public Guid GUID { get; set; }
        public ESTIMATION_DIRECT_ITEM ESTIMATION_DIRECT_ITEM { get; set; }

        public ObservableCollection<ESTIMATION_DIRECT_ITEMProjection> CHILD_ESTIMATION_DIRECT_ITEM { get; set; }

        public COMMODITY_GROUP_DIRECT MANUAL_COMMODITY_GROUP_DIRECT { get; set; }

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

        public decimal TOTAL_INSTALL_COSTS
        {
            get
            {
                if (ESTIMATION_DIRECT_ITEM == null || MANUAL_COMMODITY_GROUP_DIRECT == null || MANUAL_COMMODITY_GROUP_DIRECT.HOURS_INSTALL == null)
                    return 0;

                if (RATE == null || RATE.RATE1 == null)
                    return 0;

                return (decimal)MANUAL_COMMODITY_GROUP_DIRECT.HOURS_INSTALL * (decimal)RATE.RATE1 * ESTIMATION_DIRECT_ITEM.TOTAL_QUANTITY;
            }
        }

        public decimal TOTAL_FREIGHT_COSTS
        {
            get
            {
                if (ESTIMATION_DIRECT_ITEM == null || MANUAL_COMMODITY_GROUP_DIRECT == null || MANUAL_COMMODITY_GROUP_DIRECT.RATE_FREIGHT == null)
                    return 0;

                return (decimal)MANUAL_COMMODITY_GROUP_DIRECT.RATE_FREIGHT * ESTIMATION_DIRECT_ITEM.TOTAL_QUANTITY;
            }
        }

        public decimal TOTAL_SUPPLY_COSTS
        {
            get
            {
                if (ESTIMATION_DIRECT_ITEM == null || MANUAL_COMMODITY_GROUP_DIRECT == null || MANUAL_COMMODITY_GROUP_DIRECT.RATE_SUPPLY == null)
                    return 0;

                return (decimal)MANUAL_COMMODITY_GROUP_DIRECT.RATE_SUPPLY * ESTIMATION_DIRECT_ITEM.TOTAL_QUANTITY;
            }
        }

        public bool HAS_CHILDREN
        {
            get { return CHILD_ESTIMATION_DIRECT_ITEM.Count > 0; }
        }

        public bool ISEXPANDED { get; set; }
    }

    public static class ESTIMATION_DIRECT_ITEMProjectionQueries
    {
        public static IQueryable<ESTIMATION_DIRECT_ITEMProjection> JoinRATESOnESTIMATION_DIRECT_ITEMS(IQueryable<ESTIMATION_DIRECT_ITEM> ESTIMATION_DIRECT_ITEMS, Func<ESTIMATION_DIRECT> getESTIMATION_DIRECTFunc, Func<IQueryable<DEPARTMENT>> getDEPARTMENTFunc, Func<IQueryable<RATE>> getRATES_ByProjectFunc = null, bool isESTIMATION_DIRECTQueryProcessed = false)
        {
            ESTIMATION_DIRECT ESTIMATION_DIRECT = getESTIMATION_DIRECTFunc();
            IQueryable<ESTIMATION_DIRECT_ITEM> contextESTIMATION_DIRECT_ITEMS;
            if (ESTIMATION_DIRECT == null)
                contextESTIMATION_DIRECT_ITEMS = ESTIMATION_DIRECT_ITEMS.Where(x => x.GUID == Guid.Empty);
            else
            {
                if(isESTIMATION_DIRECTQueryProcessed)
                    contextESTIMATION_DIRECT_ITEMS = ESTIMATION_DIRECT_ITEMS;
                else
                    contextESTIMATION_DIRECT_ITEMS = ESTIMATION_DIRECT_ITEMS.Where(x => x.GUID_ESTIMATION_DIRECT == ESTIMATION_DIRECT.GUID);
            }


            IQueryable<RATE> RATES = getRATES_ByProjectFunc();
            IQueryable<DEPARTMENT> DEPARTMENTS = getDEPARTMENTFunc();
            DEPARTMENT constructionDEPARTMENT = DEPARTMENTS.FirstOrDefault(x => x.NAME.ToUpper() == CommonResources.DefaultConstructionDepartment);
            Guid searchDEPARTMENTGuid;
            if (constructionDEPARTMENT == null)
                searchDEPARTMENTGuid = Guid.Empty;
            else
                searchDEPARTMENTGuid = constructionDEPARTMENT.GUID;

            return contextESTIMATION_DIRECT_ITEMS.ToArray().AsQueryable().Select(x => new ESTIMATION_DIRECT_ITEMProjection() { GUID = x.GUID, ESTIMATION_DIRECT_ITEM = x, RATE = RATES.FirstOrDefault(y => y.GUID_DEPARTMENT == searchDEPARTMENTGuid && y.GUID_DISCIPLINE == x.GUID_DISCIPLINE) });
        }
    }
}
