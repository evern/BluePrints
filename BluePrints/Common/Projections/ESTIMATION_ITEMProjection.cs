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
    public class ESTIMATE_ITEMProjection : BluePrintsProjectionBase<ESTIMATE_ITEM>, IDeliverable_Quantity, IHaveStockCode, IHaveDBProductivityOverride, ISupportVariation, ISupportByDuration, IHaveProcurementSubjob
    {
        public ESTIMATE_ITEMProjection()
            : base()
        {
            //need to initialize commodity code here so that copy/paste is able to get property info within STOCK_GROUP
            estimate_stock_code = new STOCK_CODE();
            budget_stock_code = new STOCK_CODE();
        }

        public RATE FREIGHT_RATE { get; set; }

        public decimal Rate_Per_Truck => FREIGHT_RATE == null ? 0 : FREIGHT_RATE.RATE1 == null ? 0 : ((Decimal)FREIGHT_RATE.RATE1);

        public decimal Budget_FreightRate => Entity.BUDGET_TRUCK_PERCENTAGE == null ? 0 : FREIGHT_RATE == null || FREIGHT_RATE.RATE1 == null ? 0 : ((Decimal)FREIGHT_RATE.RATE1) * (decimal)Entity.BUDGET_TRUCK_PERCENTAGE;

        public decimal Estimate_FreightRate => Entity.ESTIMATE_TRUCK_PERCENTAGE == null ? 0 : FREIGHT_RATE == null || FREIGHT_RATE.RATE1 == null ? 0 : ((Decimal)FREIGHT_RATE.RATE1) * (decimal)Entity.ESTIMATE_TRUCK_PERCENTAGE;

        public IEnumerable<STOCK_CODE> StockCodeCollection { get; set; }
        private STOCK_CODE estimate_stock_code;
        public STOCK_CODE ESTIMATE_STOCK_CODE
        {
            get
            {
                return estimate_stock_code;
            }
            set
            {
                //Always go by value so that changes can be identified in view
                if (value != null)
                    DataUtils.ShallowCopy(estimate_stock_code, value);
            }
        }

        private STOCK_CODE budget_stock_code;
        public STOCK_CODE BUDGET_STOCK_CODE
        {
            get
            {
                return budget_stock_code;
            }
            set
            {
                //Always go by value so that changes can be identified in view
                if (value != null)
                    DataUtils.ShallowCopy(budget_stock_code, value);
            }

        }
        //Used for direct property access validation in fill/undo-redo
        public Guid? Estimate_StockCodeGuid
        {
            get
            {
                return Entity.GUID_ESTIMATE_STOCK_CODE;
            }
            set
            {
                Guid? setValue = (Guid?)value;
                if (setValue == null)
                {
                    Entity.GUID_ESTIMATE_STOCK_CODE = null;
                    ESTIMATE_STOCK_CODE = null;
                }
                else if (IsStockCodeValid(setValue))
                {
                    Entity.GUID_ESTIMATE_STOCK_CODE = setValue;
                    if (StockCodeCollection != null)
                        ESTIMATE_STOCK_CODE = StockCodeCollection.FirstOrDefault(x => x.GUID == setValue);
                }
            }
        }

        //Used for direct property access validation in fill/undo-redo
        public Guid? Budget_StockCodeGuid
        {
            get
            {
                return Entity.GUID_BUDGET_STOCK_CODE;
            }
            set
            {
                Guid? setValue = (Guid?)value;
                if (setValue == null)
                {
                    Entity.GUID_BUDGET_STOCK_CODE = null;
                    BUDGET_STOCK_CODE = null;
                }
                else if (IsStockCodeValid(setValue))
                {
                    Entity.GUID_BUDGET_STOCK_CODE = setValue;
                    if (StockCodeCollection != null)
                        BUDGET_STOCK_CODE = StockCodeCollection.FirstOrDefault(x => x.GUID == setValue);
                }
            }
        }

        public EstimateProgressType PROGRESS_TYPE
        {
            get
            {
                return Entity.PROGRESS_TYPE;
            }
            set
            {
                if (value == EstimateProgressType.Trackable)
                {
                    if (BUDGET_STOCK_CODE == null)
                        return;

                    if (Entity.STOCK_GROUP == null)
                        return;

                    if (BUDGET_STOCK_CODE.UOM != Entity.STOCK_GROUP.UOM)
                        return;
                }

                if (value == EstimateProgressType.Auto)
                {
                    if (BUDGET_STOCK_CODE == null)
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

        //public string Deliverable_Name => BUDGET_STOCK_CODE == null ? string.Empty : BUDGET_STOCK_CODE.CODE;
        public string Deliverable_Name => Entity.Deliverable_Name;

        public string Phase_Code => BluePrintsResources.Default_Construction_Phase;

        //public string Commodity_Code => Entity.STOCK_GROUP == null ? string.Empty : Entity.STOCK_GROUP.CODE;
        public string Commodity_Code => BUDGET_STOCK_CODE == null ? string.Empty : BUDGET_STOCK_CODE.CODE;

        public Guid? Subjob_Guid => Entity.GUID_SUBJOB;

        public Guid? Area_Guid => Entity.GUID_AREA;

        public Guid? SubArea_Guid => Entity.GUID_SUBAREA;

        public decimal Total_Units_IncludingByDuration => Budget_Units;

        public decimal Estimate_Units => Entity.STOCK_CODE == null ? ESTIMATE_STOCK_CODE == null ? 0 : ESTIMATE_STOCK_CODE.HOURS_INSTALL * Entity.ESTIMATE_QUANTITY : Entity.STOCK_CODE.HOURS_INSTALL * Entity.ESTIMATE_QUANTITY;

        public decimal Budget_Units
        {
            get
            {
                if (Entity.BUDGET_QUANTITY == null)
                    return 0;

                if (Entity.STOCK_CODE != null)
                    return Entity.STOCK_CODE.HOURS_INSTALL * (decimal)Entity.BUDGET_QUANTITY;

                if (BUDGET_STOCK_CODE != null)
                    return BUDGET_STOCK_CODE.HOURS_INSTALL * (decimal)Entity.BUDGET_QUANTITY;

                return 0;
            }
        }
            
        public decimal Total_Units => Entity.Total_Units;

        public Guid OriginalEntityKey { get => Entity.GUID_ORIGINAL; }

        public void SetOriginalEntityKey(Guid newGuid) { }

        public decimal Budget_ItemRate => Entity.BUDGET_INSTALL_RATE;

        public decimal Estimate_ItemRate => Entity.ESTIMATE_INSTALL_RATE;

        public decimal Budget_Costs => Budget_Units * Budget_ItemRate;

        public decimal Total_Costs => Total_Budget_Install_Cost + Total_Budget_Freight_Cost + Total_Budget_Supply_Cost;

        public decimal Budget_Quantity => Entity.BUDGET_QUANTITY == null ? 0 : (decimal)Entity.BUDGET_QUANTITY;

        public decimal Total_Quantity => Entity.BUDGET_QUANTITY == null ? 0 + Entity.DC_QUANTITY : (decimal)Entity.BUDGET_QUANTITY + Entity.DC_QUANTITY;

        public string Estimate_UOM => ESTIMATE_STOCK_CODE == null ? string.Empty : ESTIMATE_STOCK_CODE.UOM;

        public string Budget_UOM => BUDGET_STOCK_CODE == null ? string.Empty : BUDGET_STOCK_CODE.UOM;

        public EstimateProgressType Progress_Type => Entity.PROGRESS_TYPE;

        public decimal Estimate_Supply_Cost => Estimate_Stock_Code_Supply_Rate * Estimate_Quantity;

        public decimal Budget_Supply_Cost => Budget_Stock_Code_Supply_Rate * Budget_Quantity;

        public decimal Install_Cost => Total_Units * Budget_ItemRate;

        public string Discipline_Code => Entity.Discipline_Code;

        public decimal Variation_Units => Entity.Variation_Units;

        public decimal Variation_Costs => Variation_Units * Budget_ItemRate;

        public decimal Estimate_Stock_Code_Supply_Rate => ESTIMATE_STOCK_CODE == null ? 0 : ESTIMATE_STOCK_CODE.RATE_SUPPLY;
        
        public decimal Budget_Stock_Code_Supply_Rate => BUDGET_STOCK_CODE == null ? 0 : BUDGET_STOCK_CODE.RATE_SUPPLY;

        public string Estimate_Stock_Code_Type => ESTIMATE_STOCK_CODE == null ? string.Empty : ESTIMATE_STOCK_CODE.TYPE;

        public string Estimate_Stock_Code_Spec => ESTIMATE_STOCK_CODE == null ? string.Empty : ESTIMATE_STOCK_CODE.SPEC;

        public string Estimate_Stock_Code_Description => ESTIMATE_STOCK_CODE == null ? string.Empty : ESTIMATE_STOCK_CODE.DESCRIPTION;

        public decimal? DB_Productivity_Override { get => Entity.DB_Productivity_Override; set => Entity.DB_Productivity_Override = value; }

        public Guid? Baseline_Guid { get => Entity.Baseline_Guid; set => Entity.Baseline_Guid = value; }

        public Guid? Variation_Guid { get => Entity.Variation_Guid; set => Entity.Variation_Guid = value; }

        public decimal Estimated_Value { get => Entity.Estimated_Value; set => Entity.Estimated_Value = value; }

        public decimal DC_Value { get => Entity.DC_Value; set => Entity.DC_Value = value; }

        public decimal Estimate_Stock_Code_Install_Hours => ESTIMATE_STOCK_CODE == null ? 0 : ESTIMATE_STOCK_CODE.HOURS_INSTALL;
        
        public decimal Budget_Stock_Code_Install_Hours => BUDGET_STOCK_CODE == null ? 0 : BUDGET_STOCK_CODE.HOURS_INSTALL;

        public decimal Variation_Quantity => Entity.DC_QUANTITY;

        public Guid? Stock_Group_Guid => Entity.STOCK_GROUP == null ? Guid.Empty : Entity.STOCK_GROUP.GUID;

        public decimal Estimate_Install_Cost => Estimate_Units * Estimate_ItemRate;

        public decimal Variation_Install_Cost => Variation_Units * Budget_ItemRate;

        public decimal Estimate_Freight_Cost => Estimate_Quantity * Estimate_FreightRate;

        public decimal Variation_Freight_Cost => Variation_Quantity * Budget_FreightRate;

        public decimal Estimate_Install_Hours => ESTIMATE_STOCK_CODE == null ? 0 : Entity.ESTIMATE_QUANTITY * ESTIMATE_STOCK_CODE.HOURS_INSTALL;

        public decimal Variation_Install_Hours => Entity.Variation_Units;

        public decimal Total_Install_Hours => Budget_Install_Hours + Variation_Install_Hours;

        public decimal Variation_Supply_Cost => Variation_Quantity * Estimate_Stock_Code_Supply_Rate;

        public decimal Total_Estimate_Cost => Estimate_Install_Cost + Estimate_Freight_Cost + Estimate_Supply_Cost;

        public string Subjob_Name => Entity.Subjob_Name;

        public string Department_Code => Entity.Department_Code;

        public Guid? Phase_Guid { get => Entity.Phase_Guid; set => Entity.Phase_Guid = value; }

        Guid? IDeliverable.Subjob_Guid { get => Entity.Subjob_Guid; set => Entity.Subjob_Guid = value; }

        public Guid? Procurement_Subjob_Guid { get => Entity.GUID_PSUBJOB; set => Entity.GUID_PSUBJOB = value; }

        public Guid? Discipline_Guid => Entity.GUID_DISCIPLINE;

        public decimal Discipline_Number => Entity.DISCIPLINE_NUM;

        public Guid? Workpack_Guid { get => Entity.GUID_WORKPACK; set => Entity.GUID_WORKPACK = value; }

        public decimal Estimate_Quantity => Entity.ESTIMATE_QUANTITY;

        public decimal Budget_Install_Hours => BUDGET_STOCK_CODE == null ? 0 : Entity.BUDGET_QUANTITY == null ? 0 : (decimal)Entity.BUDGET_QUANTITY * BUDGET_STOCK_CODE.HOURS_INSTALL;

        public decimal Budget_Install_Cost => Budget_Install_Hours * Budget_ItemRate;

        public decimal Total_Budget_Install_Cost => Budget_Install_Cost + Variation_Install_Cost;

        public decimal Total_Budget_Freight_Cost => Budget_Freight_Cost + Variation_Freight_Cost;

        public decimal Total_Budget_Supply_Cost => Budget_Quantity * Estimate_Stock_Code_Supply_Rate;

        public decimal Total_Budget_Cost => Total_Budget_Install_Cost + Total_Budget_Supply_Cost + Total_Budget_Freight_Cost;

        public decimal Budget_Freight_Cost => Budget_Quantity * Budget_FreightRate;

        public PhaseType? Phase => Entity.Phase;

        public ChargeType? Charge => Entity.Charge;

        public bool IsByDuration { get => Entity.IsByDuration; set => Entity.IsByDuration = value; }

        public IEnumerable<User_Weight> AssignedUsers => new List<User_Weight>();
    }

    public static class ESTIMATE_ITEMProjectionQueries
    {
        public static IQueryable<ESTIMATE_ITEMProgress> IDeliverable_Progress_Transformation(
            IQueryable<ESTIMATE_ITEM> ESTIMATE_ITEMS, PROJECT PROJECT, 
            IEnumerable<RATE> RATES, PROGRESS PROGRESS, IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS, bool useReportDate, IEnumerable<STOCK_CODE> STOCK_CODES, IEnumerable<STOCK_GROUP> STOCK_GROUPS = null, 
            IEnumerable<VARIATION> VARIATIONS = null, bool buildStats = false, IEnumerable<P6_ASSIGNMENT> P6_ASSIGNMENTS = null)
        {
            var PROGRESS_ITEMSByOriginalGuid = PROGRESS_ITEMS.GroupBy(x => x.GUID_ORIBASEITEM).Select(group => new { OriginalGuid = group.Key, Progresses = group.ToList() });
            List<ESTIMATE_ITEM> estimate_items = ESTIMATE_ITEMS.ToList();
            List<ESTIMATE_ITEMProjection> estimation_direct_item_rates = IDeliverable_Rates_Transformation(estimate_items.AsQueryable(), RATES, STOCK_CODES, STOCK_GROUPS).ToList();

            List<VariationAdjustment> projectVariationAdjustments;
            //VARIATIONS are only necessary if front-end requires percentages
            if (VARIATIONS != null)
                projectVariationAdjustments = ProjectionHelpers.BuildProjectVariationAdjustments(VARIATIONS.AsQueryable(), estimation_direct_item_rates);
            else
                projectVariationAdjustments = new List<VariationAdjustment>();

            List<ESTIMATE_ITEMProgress> estimation_direct_item_progresses = new List<ESTIMATE_ITEMProgress>();
            //LoadingScreenManager.ShowLoadingScreen(estimation_direct_item_rates.Count);
            //LoadingScreenManager.SetMessage("Building budget stats");
            foreach (ESTIMATE_ITEMProjection estimation_direct_item_rate in estimation_direct_item_rates)
            {
                ESTIMATE_ITEMProgress newEstimation_Direct_itemProgress = new ESTIMATE_ITEMProgress(PROJECT, PROGRESS, estimation_direct_item_rate, projectVariationAdjustments);
                newEstimation_Direct_itemProgress.P6_Assignments = P6_ASSIGNMENTS == null ? null : P6_ASSIGNMENTS.Where(assignment => assignment.GUID_ORIGINAL == estimation_direct_item_rate.OriginalEntityKey).ToList();
                newEstimation_Direct_itemProgress.Live_PROGRESS = PROGRESS;
                newEstimation_Direct_itemProgress.Entity = estimation_direct_item_rate;
                DateTime reportDateToUse = useReportDate ? PROGRESS.REPORT_DATE != null ? (DateTime)PROGRESS.REPORT_DATE : PROGRESS.DATA_DATE : PROGRESS.DATA_DATE;

                ProgressQueries.SetReportablePROGRESS_ITEM(newEstimation_Direct_itemProgress, PROGRESS_ITEMSByOriginalGuid);
                if(PROGRESS != null)
                    newEstimation_Direct_itemProgress.SetReportingDataDate(reportDateToUse);

                if (buildStats)
                    newEstimation_Direct_itemProgress.BuildStats();

                estimation_direct_item_progresses.Add(newEstimation_Direct_itemProgress);
                LoadingScreenManager.Progress();
            }

            //LoadingScreenManager.CloseLoadingScreen();

            return estimation_direct_item_progresses.AsQueryable();
        }

        public static IQueryable<ESTIMATE_ITEMProjection> IDeliverable_Rates_Transformation(
            IQueryable<ESTIMATE_ITEM> ESTIMATE_ITEMS, 
            IEnumerable<RATE> RATES, IEnumerable<STOCK_CODE> STOCK_CODES, IEnumerable<STOCK_GROUP> STOCK_GROUPS = null)
        {
            IEnumerable<RATE> INSTALL_RATES = RATES.Where(x => x.Phase_Type == PhaseType.Construct);
            IEnumerable<RATE> FREIGHT_RATES = RATES.Where(x => x.Phase_Type == PhaseType.Procurement);

            List<ESTIMATE_ITEMProjection> estimate_items = new List<ESTIMATE_ITEMProjection>();
            foreach(ESTIMATE_ITEM estimate_item in ESTIMATE_ITEMS)
            {
                ESTIMATE_ITEMProjection newEstimateItem = new ESTIMATE_ITEMProjection();
                newEstimateItem.Entity = estimate_item;
                if (STOCK_CODES != null)
                {
                    if(estimate_item.STOCK_CODE1 != null)
                        newEstimateItem.ESTIMATE_STOCK_CODE = estimate_item.STOCK_CODE1;
                    else if (estimate_item.GUID_ESTIMATE_STOCK_CODE != null)
                        newEstimateItem.ESTIMATE_STOCK_CODE = STOCK_CODES.FirstOrDefault(stockcode => stockcode.GUID == estimate_item.GUID_ESTIMATE_STOCK_CODE);

                    if (estimate_item.STOCK_CODE != null)
                        newEstimateItem.BUDGET_STOCK_CODE = estimate_item.STOCK_CODE;
                    else if(estimate_item.GUID_BUDGET_STOCK_CODE != null)
                        newEstimateItem.BUDGET_STOCK_CODE = STOCK_CODES.FirstOrDefault(stockcode => stockcode.GUID == estimate_item.GUID_BUDGET_STOCK_CODE);

                    //use entity framework navigational model to speed up process
                    //newEstimateItem.ESTIMATE_STOCK_CODE = STOCK_CODES.FirstOrDefault(stockcode => stockcode.GUID == estimate_item.GUID_ESTIMATE_STOCK_CODE);
                    //newEstimateItem.BUDGET_STOCK_CODE = STOCK_CODES.FirstOrDefault(stockcode => stockcode.GUID == estimate_item.GUID_BUDGET_STOCK_CODE);
                    //newEstimateItem.StockCodeCollection = STOCK_CODES;
                }

                //use entity framework navigational model to speed up process
                //newEstimateItem.RATE = INSTALL_RATES.FirstOrDefault(rate => (rate.PHASE_TYPE == estimate_item.Phase) && (rate.CHARGE_TYPE == estimate_item.PHASE.CHARGE_TYPE) && (rate.GUID_DISCIPLINE == estimate_item.GUID_DISCIPLINE || rate.GUID_DISCIPLINE == null) && (rate.GUID_COMMODITY == estimate_item.GUID_COMMODITY_CODE || rate.GUID_COMMODITY == null));
                newEstimateItem.FREIGHT_RATE = FREIGHT_RATES.FirstOrDefault(rate => (rate.GUID_DISCIPLINE == null) && (rate.GUID_DISCIPLINE == null) && (rate.GUID_COMMODITY == null));

                estimate_items.Add(newEstimateItem);
            }

            return estimate_items.AsQueryable();
            //return
            //    ESTIMATE_ITEMS.ToList()
            //        .Select(
            //            estimate_item =>
            //                new ESTIMATE_ITEMProjection()
            //                {
            //                    Entity = estimate_item,
            //                    ESTIMATE_STOCK_CODE = STOCK_CODES == null ? null : STOCK_CODES.FirstOrDefault(stockcode => stockcode.GUID == estimate_item.GUID_ESTIMATE_STOCK_CODE),
            //                    BUDGET_STOCK_CODE = STOCK_CODES == null ? null : STOCK_CODES.FirstOrDefault(stockcode => stockcode.GUID == estimate_item.GUID_BUDGET_STOCK_CODE),
            //                    RATE = INSTALL_RATES.FirstOrDefault(rate => (rate.PHASE_TYPE == estimate_item.Phase) && (rate.CHARGE_TYPE == estimate_item.PHASE.CHARGE_TYPE) && (rate.GUID_DISCIPLINE == estimate_item.GUID_DISCIPLINE || rate.GUID_DISCIPLINE == null) && (rate.GUID_COMMODITY == estimate_item.GUID_COMMODITY_CODE || rate.GUID_COMMODITY == null)),
            //                    FREIGHT_RATE = FREIGHT_RATES.FirstOrDefault(rate => (rate.GUID_DISCIPLINE == null) && (rate.GUID_DISCIPLINE == null) && (rate.GUID_COMMODITY == null)),
            //                    StockCodeCollection = STOCK_CODES == null ? null : STOCK_CODES
            //                }).AsQueryable();
        }
    }
}