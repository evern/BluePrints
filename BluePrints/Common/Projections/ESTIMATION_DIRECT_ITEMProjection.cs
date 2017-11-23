using BaseModel.Attributes;
using BaseModel.Data.Helpers;
using BluePrints.Common.Base;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BluePrints.Common.Projections
{
    public class ESTIMATION_DIRECT_ITEMProjection : BluePrintsProjectionBase<ESTIMATION_DIRECT_ITEM>, IDeliverable_Quantity, IHaveStockCode, IHaveDBProductivityOverride, ISupportVariation, IHaveProcurementSubjob
    {
        public ESTIMATION_DIRECT_ITEMProjection()
            : base()
        {
            //need to initialize commodity code here so that copy/paste is able to get property info within STOCK_GROUP
            stock_code = new STOCK_CODE();
        }

        public RATE FREIGHT_RATE { get; set; }

        public decimal FreightRate => FREIGHT_RATE == null ? 0 : FREIGHT_RATE.RATE1 == null || Entity.TRUCK_PERCENTAGE == null ? 0 : (decimal)FREIGHT_RATE.RATE1 * (decimal)Entity.TRUCK_PERCENTAGE;

        public IEnumerable<STOCK_CODE> StockCodeCollection { get; set; }
        private STOCK_CODE stock_code;
        public STOCK_CODE STOCK_CODE
        {
            get
            {
                return stock_code;
            }
            set
            {
                //Always go by value so that changes can be identified in view
                if (value != null)
                    DataUtils.ShallowCopy(stock_code, value);
            }
        }

        //Used for direct property access validation in fill/undo-redo
        public Guid? StockCodeGuid
        {
            get
            {
                return Entity.GUID_STOCK_CODE;
            }
            set
            {
                Guid? setValue = (Guid?)value;
                if (setValue == null)
                {
                    Entity.GUID_STOCK_CODE = null;
                    STOCK_CODE = null;
                }
                else if (IsStockCodeValid(setValue))
                {
                    Entity.GUID_STOCK_CODE = setValue;
                    if (StockCodeCollection != null)
                        STOCK_CODE = StockCodeCollection.FirstOrDefault(x => x.GUID == setValue);
                }
            }
        }

        public Estimation_DirectProgressType PROGRESS_TYPE
        {
            get
            {
                return Entity.PROGRESS_TYPE;
            }
            set
            {
                if (value == Estimation_DirectProgressType.Trackable)
                {
                    if (STOCK_CODE == null)
                        return;

                    if (Entity.STOCK_GROUP == null)
                        return;

                    if (STOCK_CODE.UOM != Entity.STOCK_GROUP.UOM)
                        return;
                }

                if (value == Estimation_DirectProgressType.Auto)
                {
                    if (STOCK_CODE == null)
                        return;

                    if (Entity.STOCK_GROUP == null)
                        return;
                }

                Entity.PROGRESS_TYPE = value;
            }
        }

        public bool IsStockCodeValid(Guid? commodityCodeGuid)
        {
            return true;
            //if (commodityCodeGuid == null)
            //    return false;

            //if (StockCodeCollection == null)
            //    return false;

            //return StockCodeCollection.Any(x => x.GUID == commodityCodeGuid);
        }

        public RATE RATE { get; set; }

        public string Deliverable_Name => STOCK_CODE == null ? string.Empty : STOCK_CODE.CODE;

        public string Phase_Code => BluePrintsResources.Default_Construction_Phase;

        public string Commodity_Code => Entity.STOCK_GROUP == null ? string.Empty : Entity.STOCK_GROUP.CODE;

        public Guid? Subjob_Guid => Entity.GUID_SUBJOB;

        public Guid? Area_Guid => Entity.GUID_AREA;

        public Guid? SubArea_Guid => Entity.GUID_SUBAREA;

        public decimal Total_Units_IncludingByDuration => Estimated_Units;

        public decimal Estimated_Units => Entity.STOCK_CODE == null ? STOCK_CODE == null ? 0 : STOCK_CODE.HOURS_INSTALL * Entity.ESTIMATED_QUANTITY : Entity.ESTIMATED_QUANTITY * Entity.STOCK_CODE.HOURS_INSTALL;

        public decimal Total_Units => Entity.Total_Units;

        public Guid OriginalEntityKey { get => Entity.GUID_ORIGINAL; }

        public void SetOriginalEntityKey(Guid newGuid) { }

        public decimal ItemRate => Entity.RATE_OVERRIDE == null ? RATE == null || RATE.RATE1 == null ? 0 : (decimal)RATE.RATE1 : (decimal)Entity.RATE_OVERRIDE;

        public decimal Estimated_Costs => Estimated_Units * ItemRate;

        public decimal Total_Costs => Total_Install_Cost + Total_Freight_Cost + Total_Supply_Cost;

        public decimal Estimated_Quantity => Entity.ESTIMATED_QUANTITY;

        public decimal Total_Quantity => Entity.ESTIMATED_QUANTITY + Entity.DC_QUANTITY;

        public string UOM => STOCK_CODE == null ? string.Empty : STOCK_CODE.UOM;

        public Estimation_DirectProgressType Progress_Type => Entity.PROGRESS_TYPE;

        public decimal Supply_Cost => Stock_Code_Supply_Rate * Estimated_Quantity;

        public decimal Install_Cost => Total_Units * ItemRate;

        public string Discipline_Code => Entity.Discipline_Code;

        public decimal Variation_Units => Entity.Variation_Units;

        public decimal Variation_Costs => Variation_Units * ItemRate;

        public decimal Stock_Code_Supply_Rate => STOCK_CODE == null ? 0 : STOCK_CODE.RATE_SUPPLY;

        public string Stock_Code_Type => STOCK_CODE == null ? string.Empty : STOCK_CODE.TYPE;

        public string Stock_Code_Spec => STOCK_CODE == null ? string.Empty : STOCK_CODE.SPEC;

        public string Stock_Code_Description => STOCK_CODE == null ? string.Empty : STOCK_CODE.DESCRIPTION;

        public decimal? DB_Productivity_Override { get => Entity.DB_Productivity_Override; set => Entity.DB_Productivity_Override = value; }

        public Guid? Baseline_Guid { get => Entity.Baseline_Guid; set => Entity.Baseline_Guid = value; }

        public Guid? Variation_Guid { get => Entity.Variation_Guid; set => Entity.Variation_Guid = value; }

        public decimal Estimated_Value { get => Entity.Estimated_Value; set => Entity.Estimated_Value = value; }

        public decimal DC_Value { get => Entity.DC_Value; set => Entity.DC_Value = value; }

        public decimal Stock_Code_Install_Hours => STOCK_CODE == null ? 0 : STOCK_CODE.HOURS_INSTALL;

        public decimal Variation_Quantity => Entity.DC_QUANTITY;

        public Guid? Stock_Group_Guid => Entity.STOCK_GROUP == null ? Guid.Empty : Entity.STOCK_GROUP.GUID;

        public decimal Estimated_Install_Cost => Estimated_Units * ItemRate;

        public decimal Variation_Install_Cost => Variation_Units * ItemRate;

        public decimal Total_Install_Cost => Estimated_Install_Cost + Variation_Install_Cost;

        public decimal Estimated_Freight_Cost => Estimated_Quantity * FreightRate;

        public decimal Variation_Freight_Cost => Variation_Quantity * FreightRate;

        public decimal Total_Freight_Cost => Estimated_Freight_Cost + Variation_Freight_Cost;

        public decimal Estimated_Install_Hours => Entity.Estimated_Units;

        public decimal Variation_Install_Hours => Entity.Variation_Units;

        public decimal Total_Install_Hours => Entity.Estimated_Units;

        public decimal Estimated_Supply_Cost => Estimated_Quantity * Stock_Code_Supply_Rate;

        public decimal Variation_Supply_Cost => Variation_Quantity * Stock_Code_Supply_Rate;

        public decimal Total_Supply_Cost => Estimated_Supply_Cost + Variation_Supply_Cost;

        public decimal Total_Cost => Total_Install_Cost + Total_Supply_Cost + Total_Freight_Cost;

        public string Subjob_Name => Entity.Subjob_Name;

        public string Department_Code => Entity.Department_Code;

        public Guid? Phase_Guid { get => Entity.Phase_Guid; set => Entity.Phase_Guid = value; }

        Guid? IDeliverable.Subjob_Guid { get => Entity.Subjob_Guid; set => Entity.Subjob_Guid = value; }

        public Guid? Procurement_Subjob_Guid { get => Entity.GUID_PSUBJOB; set => Entity.GUID_PSUBJOB = value; }

        public Guid? Discipline_Guid => null;

        public decimal Discipline_Number => 0;

        public Guid? Workpack_Guid { get => Guid.Empty; set { } }
    }

    public static class ESTIMATION_DIRECT_ITEMProjectionQueries
    {
        public static IQueryable<ESTIMATION_DIRECT_ITEMProgress> IDeliverable_Progress_Transformation(
            IQueryable<ESTIMATION_DIRECT_ITEM> ESTIMATION_DIRECT_ITEMS, PROJECT PROJECT, 
            IEnumerable<RATE> RATES, PROGRESS PROGRESS, IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS, IEnumerable<STOCK_CODE> STOCK_CODES = null, IEnumerable<STOCK_GROUP> STOCK_GROUPS = null, 
            IEnumerable<VARIATION> VARIATIONS = null, bool buildStats = false, IEnumerable<P6_ASSIGNMENT> P6_ASSIGNMENTS = null)
        {
            var PROGRESS_ITEMSByOriginalGuid = PROGRESS_ITEMS.GroupBy(x => x.GUID_ORIBASEITEM).Select(group => new { OriginalGuid = group.Key, Progresses = group.ToList() });
            List<ESTIMATION_DIRECT_ITEMProjection> estimation_direct_item_rates =
            ESTIMATION_DIRECT_ITEMProjectionQueries.IDeliverable_Rates_Transformation(ESTIMATION_DIRECT_ITEMS,
                                                                                            RATES,
                                                                                            STOCK_CODES,
                                                                                            STOCK_GROUPS).ToList();

            List<VariationAdjustment> projectVariationAdjustments;
            //VARIATIONS are only necessary if front-end requires percentages
            if (VARIATIONS != null)
                projectVariationAdjustments = ProjectionHelpers.BuildProjectVariationAdjustments(VARIATIONS.AsQueryable(), estimation_direct_item_rates);
            else
                projectVariationAdjustments = new List<VariationAdjustment>();

            List<ESTIMATION_DIRECT_ITEMProgress> estimation_direct_item_progresses = new List<ESTIMATION_DIRECT_ITEMProgress>();
            foreach (ESTIMATION_DIRECT_ITEMProjection estimation_direct_item_rate in estimation_direct_item_rates)
            {
                ESTIMATION_DIRECT_ITEMProgress newEstimation_Direct_itemProgress = new ESTIMATION_DIRECT_ITEMProgress(PROJECT, PROGRESS, estimation_direct_item_rate, projectVariationAdjustments);
                newEstimation_Direct_itemProgress.P6_Assignments = P6_ASSIGNMENTS == null ? null : P6_ASSIGNMENTS.Where(assignment => assignment.GUID_ORIGINAL == estimation_direct_item_rate.OriginalEntityKey).ToList();
                newEstimation_Direct_itemProgress.Live_PROGRESS = PROGRESS;
                newEstimation_Direct_itemProgress.Entity = estimation_direct_item_rate;
                ProgressQueries.SetReportablePROGRESS_ITEM(newEstimation_Direct_itemProgress, PROGRESS_ITEMSByOriginalGuid);
                if(PROGRESS != null)
                    newEstimation_Direct_itemProgress.SetReportingDataDate(PROGRESS.DATA_DATE);

                if (buildStats)
                    newEstimation_Direct_itemProgress.BuildStats();

                estimation_direct_item_progresses.Add(newEstimation_Direct_itemProgress);
            }

            return estimation_direct_item_progresses.AsQueryable();
        }

        public static IQueryable<ESTIMATION_DIRECT_ITEMProjection> IDeliverable_Rates_Transformation(
            IQueryable<ESTIMATION_DIRECT_ITEM> ESTIMATION_DIRECT_ITEMS, 
            IEnumerable<RATE> RATES, IEnumerable<STOCK_CODE> STOCK_CODES = null, IEnumerable<STOCK_GROUP> STOCK_GROUPS = null)
        {
            IEnumerable<RATE> INSTALL_RATES = RATES.Where(x => x.DEPARTMENT.NAME.ToUpper() == BluePrintsResources.Default_Construction_Department);
            IEnumerable<RATE> FREIGHT_RATES = RATES.Where(x => x.DEPARTMENT.NAME.ToUpper() == BluePrintsResources.Default_Procurement_Department);

            return
                ESTIMATION_DIRECT_ITEMS.OrderBy(x => x.CREATED).ToArray()
                    .Select(
                        estimate_direct_item =>
                            new ESTIMATION_DIRECT_ITEMProjection()
                            {
                                Entity = estimate_direct_item,
                                STOCK_CODE = STOCK_CODES == null ? null : STOCK_CODES.FirstOrDefault(stockcode => stockcode.GUID == estimate_direct_item.GUID_STOCK_CODE),
                                RATE = INSTALL_RATES.FirstOrDefault(rate => rate.GUID_DISCIPLINE == estimate_direct_item.GUID_DISCIPLINE),
                                FREIGHT_RATE = FREIGHT_RATES.FirstOrDefault(rate => rate.GUID_DISCIPLINE == estimate_direct_item.GUID_DISCIPLINE),
                                StockCodeCollection = STOCK_CODES == null ? null : STOCK_CODES
                            }).AsQueryable();
        }
    }
}