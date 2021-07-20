using BaseModel.Attributes;
using BaseModel.Data.Helpers;
using BluePrints.Common.Base;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BluePrints.Common.Projections
{
    public class ESTIMATE_ITEMProjection : BluePrintsProjectionBase<ESTIMATE_ITEM>, IDeliverable_Rates, IHaveDBProductivityOverride, IHaveProcurementSubjob
    {
        public ESTIMATE_ITEMProjection()
            : base()
        {
        }

        public bool IsStockCodeValid(Guid? commodityCodeGuid)
        {
            return true;
        }

        public RATE RATE { get; set; }

        public RATE INTERNAL_RATE { get; set; }

        //public string Deliverable_Name => BUDGET_STOCK_CODE == null ? string.Empty : BUDGET_STOCK_CODE.CODE;
        public string Deliverable_Name => Entity.Deliverable_Name;

        public string Phase_Code => BluePrintsResources.Default_Construction_Phase;

        public string Variation_Code => Entity.Variation_Code;

        //public string Commodity_Code => Entity.STOCK_GROUP == null ? string.Empty : Entity.STOCK_GROUP.CODE;
        //temporarily removed for forecast phase 1 implementation so that schedule hours can be visualized in schedule mapping
        //public string Commodity_Code => BUDGET_STOCK_CODE == null ? string.Empty : BUDGET_STOCK_CODE.CODE;
        public string Commodity_Code => Entity.Commodity_Code;

        public Guid? Subjob_Guid => Entity.GUID_SUBJOB;

        public Guid? Area_Guid => Entity.GUID_AREA;

        public Guid? SubArea_Guid => Entity.GUID_SUBAREA;

        public decimal Total_Units_IncludingByDuration => Budget_Units;

        public List<VariationAdjustment> ApprovedVariations => new List<VariationAdjustment>();

        public string UOM => Entity.UOM;

        public decimal Budget_Units => Entity.Budget_Units;

        public decimal Total_Units => Entity.Total_Units;

        public Guid OriginalEntityKey { get => Entity.GUID_ORIGINAL; }

        public void SetOriginalEntityKey(Guid newGuid) { }

        public decimal Budget_ItemRate
        {
            get
            {
                if (Entity.CHARGE_RATE != null)
                    return (decimal)Entity.CHARGE_RATE;

                if (RATE == null || RATE.RATE1 == null)
                    return 0;

                return (decimal)RATE.RATE1;
            }
        }

        public decimal Budget_ItemInternalRate
        {
            get
            {
                if (Entity.INSTALL_RATE != null)
                    return (decimal)Entity.INSTALL_RATE;

                if (INTERNAL_RATE == null || INTERNAL_RATE.RATE1 == null)
                    return 0;

                return (decimal)INTERNAL_RATE.RATE1;
            }
        }

        public decimal Variation_InternalCosts => Variation_Units * Budget_ItemInternalRate;

        public decimal Total_InternalCosts => Budget_InternalCost + Variation_InternalCosts;

        //always use budget hours for tracking budget internal costs (not including budget adjustments)
        public decimal Budget_InternalCost => Entity.BUDGET_HOURS == null ? 0 : (decimal)Entity.BUDGET_HOURS * Budget_ItemInternalRate;

        public decimal Budget_Costs => Entity.BUDGET_HOURS == null  ? 0 : (decimal)Entity.BUDGET_HOURS * Budget_ItemRate;

        public decimal Total_Costs => Budget_Costs;

        public decimal Budget_Quantity => Entity.BUDGET_QUANTITY == null ? 0 : (decimal)Entity.BUDGET_QUANTITY;

        public decimal Total_Quantity => Entity.BUDGET_QUANTITY == null ? 0 : (decimal)Entity.BUDGET_QUANTITY;

        public decimal Install_Cost => Total_Units * Budget_ItemRate;

        public string Discipline_Code => Entity.Discipline_Code;

        public decimal Variation_Units => Entity.Variation_Units;

        public decimal Variation_Costs => Variation_Units * Budget_ItemRate;
        
        public decimal? DB_Productivity_Override { get => Entity.DB_Productivity_Override; set => Entity.DB_Productivity_Override = value; }

        public Guid? Baseline_Guid { get => Entity.Baseline_Guid; set => Entity.Baseline_Guid = value; }

        public Guid? Variation_Guid { get => Entity.Variation_Guid; set => Entity.Variation_Guid = value; }

        public decimal Estimated_Value { get => Entity.Estimated_Value; set => Entity.Estimated_Value = value; }

        public decimal Variation_Quantity => Entity.Variation_Quantity;

        public decimal Variation_Install_Cost => Variation_Units * Budget_ItemRate;

        public decimal Variation_Install_Hours => Entity.Variation_Units;

        public decimal Total_Install_Hours => Budget_Install_Hours + Variation_Install_Hours;

        public string Subjob_Name => Entity.Subjob_Name;

        public string Department_Code => Entity.Department_Code;

        public Guid? Department_Guid => Entity.Department_Guid;

        public Guid? Phase_Guid { get => Entity.Phase_Guid; set => Entity.Phase_Guid = value; }

        Guid? IDeliverable.Subjob_Guid { get => Entity.Subjob_Guid; set => Entity.Subjob_Guid = value; }

        public Guid? Procurement_Subjob_Guid { get => Entity.GUID_PSUBJOB; set => Entity.GUID_PSUBJOB = value; }

        public Guid? Discipline_Guid => Entity.GUID_DISCIPLINE;

        public decimal Discipline_Number => Entity.DISCIPLINE_NUM;

        public Guid? Workpack_Guid { get => Entity.GUID_WORKPACK; set => Entity.GUID_WORKPACK = value; }

        public decimal Budget_Install_Hours =>0;

        public decimal Budget_Install_Cost => Budget_Install_Hours * Budget_ItemRate;

        public decimal Total_Budget_Install_Cost => Budget_Install_Cost + Variation_Install_Cost;
        
        //fallback to forecast phase 1 implementation because user's aren't ready to put full budget in
        public decimal Total_Budget_Cost => Budget_ItemRate;

        public PhaseType? Phase => Entity.Phase;

        public ChargeType? Charge => Entity.Charge;

        public bool IsByDuration => Total_Units == 0;

        public IEnumerable<User_Weight> AssignedUsers => new List<User_Weight>();

        public string Project_Number => Entity.Project_Number;

        public decimal Budget_Adjustment_Units => 0;

        public decimal Budget_Adjustment_Costs => 0;

        public decimal Unadjusted_Budget_Units => Budget_Units;

        public decimal UnitsPerQuantity => Total_Units / Total_Quantity;
    }

    public static class ESTIMATE_ITEMProjectionQueries
    {
        public static IQueryable<ESTIMATE_ITEMProgress> IDeliverable_Progress_Transformation(
            IQueryable<ESTIMATE_ITEM> ESTIMATE_ITEMS, PROJECT PROJECT,
            IEnumerable<RATE> RATES = null, PROGRESS PROGRESS = null, IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS = null, bool useReportDate = false,
            IEnumerable<VARIATION> VARIATIONS = null, bool buildStats = false, IEnumerable<P6_ASSIGNMENT> P6_ASSIGNMENTS = null, bool showLoadingScreen = false, IEnumerable<COMMODITY_CODE> COMMODITY_CODES = null, bool forceRetrieveRemainingDataPoints = false)
        {
            var PROGRESS_ITEMSByOriginalGuid = PROGRESS_ITEMS == null ? null : PROGRESS_ITEMS.GroupBy(x => x.GUID_ORIBASEITEM).Select(group => new { OriginalGuid = group.Key, Progresses = group.ToList() });
            List<ESTIMATE_ITEM> estimate_items = ESTIMATE_ITEMS.ToList();
            List<ESTIMATE_ITEMProjection> estimation_direct_item_rates = ESTIMATE_ITEM_Rates_Transformation(estimate_items.AsQueryable(), RATES, showLoadingScreen).ToList();

            List<VariationAdjustment> projectVariationAdjustments;
            //VARIATIONS are only necessary if front-end requires percentages
            if (VARIATIONS != null)
                projectVariationAdjustments = ProjectionHelpers.BuildProjectVariationAdjustments(VARIATIONS.AsQueryable(), estimation_direct_item_rates);
            else
                projectVariationAdjustments = new List<VariationAdjustment>();

            List<ESTIMATE_ITEMProgress> estimation_direct_item_progresses = new List<ESTIMATE_ITEMProgress>();
            foreach (ESTIMATE_ITEMProjection estimation_direct_item_rate in estimation_direct_item_rates)
            {
                ESTIMATE_ITEMProgress newEstimation_Direct_itemProgress = new ESTIMATE_ITEMProgress(PROJECT, PROGRESS, estimation_direct_item_rate, projectVariationAdjustments, forceRetrieveRemainingDataPoints)
                {
                    P6_Assignments = P6_ASSIGNMENTS == null ? null : P6_ASSIGNMENTS.Where(assignment => assignment.GUID_ORIGINAL == estimation_direct_item_rate.OriginalEntityKey).ToList(),
                    Live_PROGRESS = PROGRESS,
                    Entity = estimation_direct_item_rate
                };

                newEstimation_Direct_itemProgress.Entity.Entity.FullCOMMODITY_CODECollection = COMMODITY_CODES;
                DateTime reportDateToUse = useReportDate ? PROGRESS.REPORT_DATE != null ? (DateTime)PROGRESS.REPORT_DATE : PROGRESS.DATA_DATE : PROGRESS.DATA_DATE;

                if(PROGRESS_ITEMSByOriginalGuid != null)
                    ProgressQueries.SetReportablePROGRESS_ITEM(newEstimation_Direct_itemProgress, PROGRESS_ITEMSByOriginalGuid);

                if (PROGRESS != null)
                    newEstimation_Direct_itemProgress.SetReportingDataDate(reportDateToUse);

                if (buildStats)
                    newEstimation_Direct_itemProgress.BuildStats();

                estimation_direct_item_progresses.Add(newEstimation_Direct_itemProgress);
                LoadingScreenManager.Progress();
            }

            //LoadingScreenManager.CloseLoadingScreen();

            return estimation_direct_item_progresses.AsQueryable();
        }

        public static IQueryable<ESTIMATE_ITEMProjection> ESTIMATE_ITEM_Rates_Transformation(
        IQueryable<ESTIMATE_ITEM> ESTIMATE_ITEMS,
        IEnumerable<RATE> RATES = null, bool showLoadingScreen = false)
        {
            //easier to debug doing it this way
            IEnumerable<ESTIMATE_ITEM> estimate_itemArr = ESTIMATE_ITEMS.ToArray();
            List<ESTIMATE_ITEMProjection> returnProjections = new List<ESTIMATE_ITEMProjection>();

            if (showLoadingScreen)
            {
                LoadingScreenManager.ShowLoadingScreen(estimate_itemArr.Count());
                LoadingScreenManager.SetMessage("Loading Construction Deliverables...");
            }

            foreach (ESTIMATE_ITEM estimate_item in estimate_itemArr)
            {
                ESTIMATE_ITEMProjection newESTIMATE_ITEM = new ESTIMATE_ITEMProjection();
                newESTIMATE_ITEM.Entity = estimate_item;

                if(RATES != null)
                {
                    if (estimate_item.PHASE.PHASE_TYPE != null)
                    {
                        RATE findRATE = BluePrintsDataUtils.CascadeRateSearch(estimate_item.GUID_AREA, estimate_item.GUID_SUBAREA, estimate_item.GUID_DISCIPLINE, estimate_item.GUID_DEPARTMENT, estimate_item.Commodity_Code, string.Empty, RATES, CostType.Charge, (PhaseType)estimate_item.PHASE.PHASE_TYPE);
                        if (findRATE != null)
                            newESTIMATE_ITEM.RATE = findRATE;

                        //Not available at this moment
                        RATE findInternalRate = BluePrintsDataUtils.CascadeRateSearch(estimate_item.GUID_AREA, estimate_item.GUID_SUBAREA, estimate_item.GUID_DISCIPLINE, estimate_item.GUID_DEPARTMENT, estimate_item.Commodity_Code, string.Empty, RATES, CostType.Cost, (PhaseType)estimate_item.PHASE.PHASE_TYPE);
                        if (findInternalRate != null)
                            newESTIMATE_ITEM.INTERNAL_RATE = findInternalRate;
                    }
                }

                returnProjections.Add(newESTIMATE_ITEM);

                LoadingScreenManager.Progress();
            }

            return returnProjections.AsQueryable();
        }
    }
}