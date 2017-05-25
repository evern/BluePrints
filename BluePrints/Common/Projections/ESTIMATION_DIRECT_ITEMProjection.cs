using BaseModel.Data.Helpers;
using BluePrints.Common.Base;
using BluePrints.Common.Resources;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.Common.Projections
{
    //[ConstraintAttributes("ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_GROUP_DIRECT")]
    //[RequiredAttributes("ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_GROUP_DIRECT")]
    public class ESTIMATION_DIRECT_ITEMProjection : BluePrintsProjectionMasterDetailBase<ESTIMATION_DIRECT_ITEM, ESTIMATION_DIRECT_ITEMProjection>
    {
        public RATE RATE { get; set; }

        COMMODITY_CODE commodity_code;
        public COMMODITY_CODE COMMODITY_CODE
        {
            get { return commodity_code; }
            set
            {
                if (value == null)
                    commodity_code = null;
                else
                {
                    commodity_code = new COMMODITY_CODE();
                    DataUtils.ShallowCopy(commodity_code, value);
                }
            }
        }

        /// <summary>
        /// Used to update the estimation code when collection changes
        /// </summary>
        public void UpdateCommodityCode(IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection)
        {
            COMMODITY_CODE lookupCommodityCode = COMMODITY_CODECollection.FirstOrDefault(x => x.GUID == Entity.GUID_COMMODITY_CODE);
            COMMODITY_CODE = lookupCommodityCode;
        }

        public string COMMODITY_GROUP_CODE_SELECTION
        {
            get
            {
                return (Entity.GUID_COMMODITY_CODE == null
                           ? Guid.Empty.ToString()
                           : Entity.GUID_COMMODITY_CODE.ToString()) +
                       (Entity.GUID_COMMODITY_GROUP_DIRECT == null
                           ? Guid.Empty.ToString()
                           : Entity.GUID_COMMODITY_GROUP_DIRECT.ToString()) +
                       Entity.COMMODITY_GROUP_DIRECT_ID.ToString();
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
                    Entity.GUID_COMMODITY_GROUP_DIRECT = null;
                    Entity.COMMODITY_GROUP_DIRECT_ID = null;
                    Entity.GUID_COMMODITY_CODE = new Guid(CommodityCodeGuidString);
                }
                else
                {
                    Entity.GUID_COMMODITY_CODE = null;
                    Entity.GUID_COMMODITY_GROUP_DIRECT = new Guid(CommodityGroupGuidString);
                    if (CommodityGroupIdString != string.Empty)
                        Entity.COMMODITY_GROUP_DIRECT_ID = int.Parse(CommodityGroupIdString);
                }
            }
        }

        public bool ISQUANTIFIABLE
        {
            get
            {
                return EntityKey != null && Entity != null &&
                       Entity.GUID_COMMODITY_CODE != null;
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
                if (Entity == null || COMMODITY_CODE == null)
                    return 0;

                if (RATE == null || RATE.RATE1 == null)
                    return 0;

                return (decimal) COMMODITY_CODE.HOURS_INSTALL * (decimal) RATE.RATE1 *
                       Entity.TOTAL_QUANTITY;
            }
        }

        public decimal TOTAL_FREIGHT_COSTS
        {
            get
            {
                if (Entity == null || COMMODITY_CODE == null)
                    return 0;

                return (decimal) COMMODITY_CODE.RATE_FREIGHT * Entity.TOTAL_QUANTITY;
            }
        }

        public decimal TOTAL_SUPPLY_COSTS
        {
            get
            {
                if (Entity == null || COMMODITY_CODE == null)
                    return 0;

                return (decimal) COMMODITY_CODE.RATE_SUPPLY * Entity.TOTAL_QUANTITY;
            }
        }

        public bool HAS_CHILDREN
        {
            get { return DetailEntities.Count > 0; }
        }

        public bool ISEXPANDED { get; set; }
    }

    public static class ESTIMATION_DIRECT_ITEMProjectionQueries
    {
        public static IQueryable<ESTIMATION_DIRECT_ITEMProjection> JoinRATESOnESTIMATION_DIRECT_ITEMS(
            IQueryable<ESTIMATION_DIRECT_ITEM> ESTIMATION_DIRECT_ITEMS, Func<ESTIMATION_DIRECT> getESTIMATION_DIRECTFunc,
            Func<IEnumerable<DEPARTMENT>> getDEPARTMENTFunc, Func<IEnumerable<RATE>> getRATES_ByProjectFunc = null,
            bool isESTIMATION_DIRECTQueryProcessed = false)
        {
            var ESTIMATION_DIRECT = getESTIMATION_DIRECTFunc();
            IQueryable<ESTIMATION_DIRECT_ITEM> contextESTIMATION_DIRECT_ITEMS;
            if (ESTIMATION_DIRECT == null)
                contextESTIMATION_DIRECT_ITEMS = new List<ESTIMATION_DIRECT_ITEM>().AsQueryable();
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
                DEPARTMENTS.FirstOrDefault(x => x.NAME.ToUpper() == BluePrintsResources.DefaultConstructionDepartment);
            Guid searchDEPARTMENTGuid;
            if (constructionDEPARTMENT == null)
                searchDEPARTMENTGuid = Guid.Empty;
            else
                searchDEPARTMENTGuid = constructionDEPARTMENT.GUID;

            return
                contextESTIMATION_DIRECT_ITEMS.ToArray()
                    .Select(
                        x =>
                            new ESTIMATION_DIRECT_ITEMProjection()
                            {
                                EntityKey = x.GUID,
                                Entity = x,
                                RATE = RATES == null ? null :
                                    RATES.FirstOrDefault(
                                        y =>
                                            y.GUID_DEPARTMENT == searchDEPARTMENTGuid &&
                                            y.GUID_DISCIPLINE == x.GUID_DISCIPLINE)
                            }).AsQueryable();
        }

        public static IQueryable<ESTIMATION_DIRECT_ITEMProjection> JoinRATESOnESTIMATION_DIRECT_ITEMS(
            IQueryable<ESTIMATION_DIRECT_ITEM> ESTIMATION_DIRECT_ITEMS, Func<ESTIMATION_DIRECT> getESTIMATION_DIRECTFunc,
            Func<IEnumerable<DEPARTMENT>> getDEPARTMENTFunc, Func<IEnumerable<COMMODITY_CODE>> getCOMMODITY_CODESFunc, Func<IEnumerable<RATE>> getRATES_ByProjectFunc = null,
            bool isESTIMATION_DIRECTQueryProcessed = false)
        {
            var ESTIMATION_DIRECT = getESTIMATION_DIRECTFunc();
            IQueryable<ESTIMATION_DIRECT_ITEM> contextESTIMATION_DIRECT_ITEMS;
            if (ESTIMATION_DIRECT == null)
                contextESTIMATION_DIRECT_ITEMS = new List<ESTIMATION_DIRECT_ITEM>().AsQueryable();
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
            var COMMODITY_CODES = getCOMMODITY_CODESFunc();

            var constructionDEPARTMENT =
                DEPARTMENTS.FirstOrDefault(x => x.NAME.ToUpper() == BluePrintsResources.DefaultConstructionDepartment);
            Guid searchDEPARTMENTGuid;
            if (constructionDEPARTMENT == null)
                searchDEPARTMENTGuid = Guid.Empty;
            else
                searchDEPARTMENTGuid = constructionDEPARTMENT.GUID;

            return
                contextESTIMATION_DIRECT_ITEMS.ToArray()
                    .Select(
                        x =>
                            new ESTIMATION_DIRECT_ITEMProjection()
                            {
                                EntityKey = x.GUID,
                                Entity = x,
                                RATE = RATES == null ? null :
                                    RATES.FirstOrDefault(
                                        y =>
                                            y.GUID_DEPARTMENT == searchDEPARTMENTGuid &&
                                            y.GUID_DISCIPLINE == x.GUID_DISCIPLINE), 
                                COMMODITY_CODE = COMMODITY_CODES.FirstOrDefault(z => z.GUID == x.GUID_COMMODITY_CODE)
                            }).AsQueryable();
        }
    }
}