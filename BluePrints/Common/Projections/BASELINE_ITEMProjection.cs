using BaseModel.Attributes;
using BaseModel.DataModel;
using BaseModel.Misc;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Base;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.Utils;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace BluePrints.Common.Projections
{
    [ConstraintAttributes("Entity.GUID_BASELINE, Entity.INTERNAL_NUM")]
    public class BASELINE_ITEMProjection : BluePrintsProjectionBase<BASELINE_ITEM>, IDeliverable_Rates, IHaveDeliverableStatus, IHaveDBProductivityOverride, IEntityNumber, ISupportGangRate, ISupportVariationRevision
    {
        public BASELINE_ITEMProjection()
            : base()
        {
        }

        public RATE RATE { get; set; }

        public RATE INTERNAL_RATE { get; set; }

        private IEnumerable<object> assignUserObject;

        public object AssignUserObject
        {
            get { return assignUserObject; }
            set
            {
                if (value != assignUserObject)
                {
                    assignUserObject = value as IEnumerable<object>;
                }
            }
        }

        public IEnumerable<USER> AssignUsers => assignUserObject == null ? null : assignUserObject.Select(x => (USER)x);

        List<User_Weight> userweights { get; set; }
        public List<User_Weight> UserWeights
        {
            get
            {
                if (userweights == null)
                    userweights = new List<User_Weight>();

                return userweights;
            }
            set
            {
                userweights = value;
            }
        }
        
        public Guid? GUID_BASELINE { get => Entity.GUID_BASELINE; set => Entity.GUID_BASELINE = value; }

        public Guid? GUID_VARIATION { get => Entity.GUID_VARIATION; set => Entity.GUID_VARIATION = value; }

        public IEnumerable<User_Weight> AssignedUsers => UserWeights;

        public string Project_Number => Entity.Project_Number;

        public string Subjob_Name => Entity.Subjob_Name;

        public Guid? Subjob_Guid { get => Entity.Subjob_Guid; set => Entity.Subjob_Guid = value; }

        public PhaseType? Phase => Entity.Phase;

        public ChargeType? Charge => Entity.Charge;

        public string Phase_Code => Entity.Phase_Code;

        public string Department_Code => Entity.Department_Code;

        public Guid? Department_Guid => Entity.Department_Guid;

        public string Discipline_Code => Entity.Discipline_Code;

        public string Deliverable_Name => Entity.Deliverable_Name;

        public Guid? Phase_Guid { get => Entity.Phase_Guid; set => Entity.Phase_Guid = value; }

        public Guid? Area_Guid => Entity.Area_Guid;

        public Guid? SubArea_Guid => Entity.SubArea_Guid;

        public Guid? Discipline_Guid => Entity.Discipline_Guid;

        public decimal Discipline_Number => Entity.Discipline_Number;

        public Guid? Workpack_Guid { get => Entity.Workpack_Guid; set => Entity.Workpack_Guid = value; }

        public bool IsByDuration => Total_Units == 0;

        public string Commodity_Code => Entity.Commodity_Code;

        public decimal Budget_Units => Entity.Budget_Units;

        public decimal Budget_Quantity => Entity.Budget_Quantity;

        public decimal Total_Quantity => Entity.Total_Quantity;

        public decimal Budget_ItemRate
        {
            get
            {
                if (RATE == null || RATE.RATE1 == null)
                    return 0;

                return (decimal) RATE.RATE1;
            }
        }

        public decimal Budget_Costs => Entity.Budget_Units * this.Budget_ItemRate;

        public decimal Budget_ItemInternalRate
        {
            get
            {
                if (INTERNAL_RATE == null || INTERNAL_RATE.RATE1 == null)
                    return 0;

                return (decimal)INTERNAL_RATE.RATE1;
            }
        }

        //always use budget hours for tracking budget internal costs (not including budget adjustments)
        public decimal Budget_InternalCost => Entity.BUDGET_HOURS * this.Budget_ItemInternalRate;

        public decimal Variation_InternalCosts => 0;

        public decimal Total_InternalCosts => 0;

        public Guid OriginalEntityKey => Entity.OriginalEntityKey;

        public DELIVERABLES_STATUS Deliverable_Status => Entity.Deliverable_Status;

        public decimal? DB_Productivity_Override { get => Entity.DB_Productivity_Override; set => Entity.DB_Productivity_Override = value; }

        public string EntityNumber { get => Entity.EntityNumber; set => Entity.EntityNumber = value; }

        public string EntityGroup => Entity.EntityGroup;

        public RateRole RateRole { get; set; }

        public decimal SplitRate { get; set; }

        public decimal SplitHours { get; set; }

        public decimal RoleCost => SplitRate * SplitHours;

        public Guid GUID_ORIGINAL { get => Entity.GUID_ORIGINAL; set => Entity.GUID_ORIGINAL = value; }

        public string Variation_Code => string.Empty;

        public decimal Variation_Units => throw new NotImplementedException();

        public decimal Variation_Costs => 0;

        public decimal Total_Units => Budget_Units + Variation_Units;

        public decimal Total_Costs => 0;

        public decimal Budget_Adjustment_Units => 0;

        public decimal Budget_Adjustment_Costs => 0;

        public decimal Unadjusted_Budget_Units => Entity.Budget_Units;

        public List<VariationAdjustment> ApprovedVariations => new List<VariationAdjustment>();

        public decimal UnitsPerQuantity => 1;

        public string UOM => "h";

        public decimal Variation_Quantity => Variation_Units / UnitsPerQuantity;

        public string Deliverable_Title => Entity.Deliverable_Title;

        public string Area_Title => Entity.Area_Title;

        public void SetOriginalEntityKey(Guid newGuid)
        {
            Entity.SetOriginalEntityKey(newGuid);
        }
    }

    public class User_Weight
    {
        public USER User { get; set; }
        public decimal Weight { get; set; }
        public decimal AggregateWeight { get; set; }

        public double AggregateWeightDbl
        {
            get
            {
                if (AggregateWeight == 0)
                    return 0;

                return Convert.ToDouble(AggregateWeight);
            }
        }

        public string UserName => User == null ? string.Empty : User.Full_Name;
        public string UserRole => User == null ? string.Empty : User.ROLE == null ? string.Empty : User.ROLE.NAME;
    }

    public static class BASELINE_ITEMProjectionQueries
    {
        public static IQueryable<BASELINE_ITEMProjection> IDeliverable_Rates_Transformation(
            IQueryable<BASELINE_ITEM> BASELINE_ITEMS,
            IEnumerable<RATE> RATES = null, IEnumerable<DOCTYPE> DOCTYPES = null, bool showLoadingScreen = false)
        {
            //List<VARIATION_ITEM> variation_items = VARIATIONS.SelectMany(x => x.VARIATION_ITEM).ToList();
            //List<BASELINE_ITEM> baseline_items = BASELINE_ITEMS.ToList();

            ////deliverable that should exists are those that are added through the deliverable's list
            //var baselineItem = from baseline_item in baseline_items
            //           join variation_item in variation_items
            //           on baseline_item.GUID_ORIGINAL equals variation_item.GUID_ORIBASEITEM
            //           into bv
            //           from variation_defaultIfEmpty in bv.DefaultIfEmpty()
            //           where baseline_item.GUID_VARIATION == null || (baseline_item.GUID_VARIATION != null && variation_defaultIfEmpty != null)
            //           select baseline_item;

            //easier to debug doing it this way
            IEnumerable<BASELINE_ITEM> baseline_items = BASELINE_ITEMS.ToArray();
            List<BASELINE_ITEMProjection> returnBASELINE_ITEMProjection = new List<BASELINE_ITEMProjection>();

            if(showLoadingScreen)
            {
                LoadingScreenManager.ShowLoadingScreen(baseline_items.Count());
                LoadingScreenManager.SetMessage("Loading Design Deliverables...");
            }

            foreach(BASELINE_ITEM baseline_item in baseline_items)
            {
                BASELINE_ITEMProjection newBASELINE_ITEM = new BASELINE_ITEMProjection();
                newBASELINE_ITEM.Entity = baseline_item;

                if(RATES != null)
                {
                    string docTypeCode = baseline_item.DOCTYPE != null ? baseline_item.DOCTYPE.CODE : string.Empty;
                    if(docTypeCode == string.Empty && DOCTYPES != null)
                    {
                        DOCTYPE findDOCTYPE = DOCTYPES.FirstOrDefault(x => x.GUID == baseline_item.GUID_DOCTYPE);
                        if (findDOCTYPE != null)
                            docTypeCode = findDOCTYPE.CODE;
                    }

                    if (baseline_item.PHASE != null && baseline_item.PHASE.PHASE_TYPE != null)
                    {
                        RATE findRATE = BluePrintsDataUtils.CascadeRateSearch(baseline_item.GUID_AREA, baseline_item.GUID_SUBAREA, baseline_item.GUID_DISCIPLINE, baseline_item.DISCIPLINE_NUM, baseline_item.GUID_DEPARTMENT, docTypeCode, string.Empty, RATES, CostType.Charge, (PhaseType)baseline_item.PHASE.PHASE_TYPE);
                        if (findRATE != null)
                            newBASELINE_ITEM.RATE = findRATE;

                        RATE findInternalRate = BluePrintsDataUtils.CascadeRateSearch(baseline_item.GUID_AREA, baseline_item.GUID_SUBAREA, baseline_item.GUID_DISCIPLINE, baseline_item.DISCIPLINE_NUM, baseline_item.GUID_DEPARTMENT, docTypeCode, string.Empty, RATES, CostType.Cost, (PhaseType)baseline_item.PHASE.PHASE_TYPE);
                        if (findInternalRate != null)
                            newBASELINE_ITEM.INTERNAL_RATE = findInternalRate;
                    }
                }

                returnBASELINE_ITEMProjection.Add(newBASELINE_ITEM);

                LoadingScreenManager.Progress();
            }

            return returnBASELINE_ITEMProjection.AsQueryable();

            //return
            //    BASELINE_ITEMS.ToArray()
            //        .Select(x => new BASELINE_ITEMProjection()
            //        {
            //            GUID = x.GUID,
            //            Entity = x,
            //            RATE = RATES.FirstOrDefault(y => (y.PHASE_TYPE == x.Phase) && (y.CHARGE_TYPE == x.PHASE.CHARGE_TYPE) && (y.GUID_DEPARTMENT == x.GUID_DEPARTMENT || y.GUID_DEPARTMENT == null) && (y.GUID_DISCIPLINE == x.GUID_DISCIPLINE || y.GUID_DISCIPLINE == null) && (y.GUID_COMMODITY == x.GUID_DOCTYPE || y.GUID_COMMODITY == null))
            //        }).AsQueryable();
        }
    }
}