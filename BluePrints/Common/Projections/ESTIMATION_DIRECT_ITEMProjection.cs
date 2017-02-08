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
    //[ConstraintAttributes("ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_GROUP_DIRECT")]
    //[RequiredAttributes("ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_GROUP_DIRECT")]
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

        public RATE RATE { get; set; }

        public string COMMODITY_GROUP_CODE_SELECTION
        {
            get
            {
                return (ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_CODE == null
                           ? Guid.Empty.ToString()
                           : ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_CODE.ToString()) +
                       (ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_GROUP_DIRECT == null
                           ? Guid.Empty.ToString()
                           : ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_GROUP_DIRECT.ToString()) +
                       ESTIMATION_DIRECT_ITEM.COMMODITY_GROUP_DIRECT_ID.ToString();
            }
            set
            {
                var guidLength = Guid.Empty.ToString().Length;
                var doubleGuidLength = guidLength * 2;

                var CommodityCodeGuidString = value.Substring(0, guidLength);
                var CommodityGroupGuidString = value.Substring(guidLength, guidLength);
                var CommodityGroupIdString = value.Substring(doubleGuidLength, value.Length - doubleGuidLength);

                if (CommodityCodeGuidString != Guid.Empty.ToString())
                {
                    ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_GROUP_DIRECT = null;
                    ESTIMATION_DIRECT_ITEM.COMMODITY_GROUP_DIRECT_ID = null;
                    ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_CODE = new Guid(CommodityCodeGuidString);
                }
                else
                {
                    ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_CODE = null;
                    ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_GROUP_DIRECT = new Guid(CommodityGroupGuidString);
                    if (CommodityGroupIdString != string.Empty)
                        ESTIMATION_DIRECT_ITEM.COMMODITY_GROUP_DIRECT_ID = int.Parse(CommodityGroupIdString);
                }
            }
        }

        public bool ISQUANTIFIABLE
        {
            get
            {
                return GUID != null && ESTIMATION_DIRECT_ITEM != null &&
                       ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_CODE != null;
            }
        }

        public decimal RATE_INSTALL
        {
            get
            {
                if (RATE == null || RATE.RATE1 == null)
                    return 0;

                return (decimal) RATE.RATE1;
            }
        }

        public decimal TOTAL_INSTALL_COSTS
        {
            get
            {
                if (ESTIMATION_DIRECT_ITEM == null || ESTIMATION_DIRECT_ITEM.HOURS_INSTALL == null)
                    return 0;

                if (RATE == null || RATE.RATE1 == null)
                    return 0;

                return (decimal) ESTIMATION_DIRECT_ITEM.HOURS_INSTALL * (decimal) RATE.RATE1 *
                       ESTIMATION_DIRECT_ITEM.TOTAL_QUANTITY;
            }
        }

        public decimal TOTAL_FREIGHT_COSTS
        {
            get
            {
                if (ESTIMATION_DIRECT_ITEM == null || ESTIMATION_DIRECT_ITEM.RATE_FREIGHT == null)
                    return 0;

                return (decimal) ESTIMATION_DIRECT_ITEM.RATE_FREIGHT * ESTIMATION_DIRECT_ITEM.TOTAL_QUANTITY;
            }
        }

        public decimal TOTAL_SUPPLY_COSTS
        {
            get
            {
                if (ESTIMATION_DIRECT_ITEM == null || ESTIMATION_DIRECT_ITEM.RATE_SUPPLY == null)
                    return 0;

                return (decimal) ESTIMATION_DIRECT_ITEM.RATE_SUPPLY * ESTIMATION_DIRECT_ITEM.TOTAL_QUANTITY;
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
        public static IQueryable<ESTIMATION_DIRECT_ITEMProjection> JoinRATESOnESTIMATION_DIRECT_ITEMS(
            IQueryable<ESTIMATION_DIRECT_ITEM> ESTIMATION_DIRECT_ITEMS, Func<ESTIMATION_DIRECT> getESTIMATION_DIRECTFunc,
            Func<IQueryable<DEPARTMENT>> getDEPARTMENTFunc, Func<IQueryable<RATE>> getRATES_ByProjectFunc = null,
            bool isESTIMATION_DIRECTQueryProcessed = false)
        {
            var ESTIMATION_DIRECT = getESTIMATION_DIRECTFunc();
            IQueryable<ESTIMATION_DIRECT_ITEM> contextESTIMATION_DIRECT_ITEMS;
            if (ESTIMATION_DIRECT == null)
            {
                contextESTIMATION_DIRECT_ITEMS = ESTIMATION_DIRECT_ITEMS.Where(x => x.GUID == Guid.Empty);
            }
            else
            {
                if (isESTIMATION_DIRECTQueryProcessed)
                    contextESTIMATION_DIRECT_ITEMS = ESTIMATION_DIRECT_ITEMS;
                else
                    contextESTIMATION_DIRECT_ITEMS =
                        ESTIMATION_DIRECT_ITEMS.Where(x => x.GUID_ESTIMATION_DIRECT == ESTIMATION_DIRECT.GUID);
            }


            var RATES = getRATES_ByProjectFunc();
            var DEPARTMENTS = getDEPARTMENTFunc();
            var constructionDEPARTMENT =
                DEPARTMENTS.FirstOrDefault(x => x.NAME.ToUpper() == CommonResources.DefaultConstructionDepartment);
            Guid searchDEPARTMENTGuid;
            if (constructionDEPARTMENT == null)
                searchDEPARTMENTGuid = Guid.Empty;
            else
                searchDEPARTMENTGuid = constructionDEPARTMENT.GUID;

            return
                contextESTIMATION_DIRECT_ITEMS.ToArray()
                    .AsQueryable()
                    .Select(
                        x =>
                            new ESTIMATION_DIRECT_ITEMProjection()
                            {
                                GUID = x.GUID,
                                ESTIMATION_DIRECT_ITEM = x,
                                RATE =
                                    RATES.FirstOrDefault(
                                        y =>
                                            y.GUID_DEPARTMENT == searchDEPARTMENTGuid &&
                                            y.GUID_DISCIPLINE == x.GUID_DISCIPLINE)
                            });
        }
    }
}