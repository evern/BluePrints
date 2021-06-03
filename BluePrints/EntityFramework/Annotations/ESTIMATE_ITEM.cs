using BaseModel.Misc;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.Resources;
using DevExpress.Mvvm;
using BluePrints.Common.Base;
using BluePrints.Common;
using BaseModel.DataModel;
using DevExpress.XtraEditors.DXErrorProvider;

namespace BluePrints.Data
{
    public partial class ESTIMATE_ITEM : EntityBase, IGuidEntityKey, ICanSync, IOriginalGuidEntityKey, IHaveCreatedDate, IDeliverable, IHaveDBProductivityOverride, IHaveProcurementSubjob
    {
        public ESTIMATE_ITEM()
        {
            DISCIPLINE_NUM = 1;
        }

        [NotMapped]
        public Guid OriginalEntityKey
        {
            get
            {
                return GUID_ORIGINAL;
            }
        }

        public void SetOriginalEntityKey(Guid newGuid) { GUID_ORIGINAL = newGuid; }

        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        //Used for direct property access validation in fill/undo-redo
        [NotMapped]
        public Guid? SubAreaGuid
        {
            get
            {
                return GUID_SUBAREA;
            }
            set
            {
                Guid? setValue = (Guid?)value;
                if (setValue == null)
                    GUID_SUBAREA = null;
                else if (IsSubAreaValid(setValue))
                    GUID_SUBAREA = setValue;
            }
        }

        [NotMapped]
        public IEnumerable<AREA> NewItemRowSubAREACollection { get; set; }

        [NotMapped]
        public IEnumerable<AREA> SubAreaCollection
        {
            get
            {
                //when it's in read only mode we can use navigational properties to get sub areas
                if (AREA != null)
                    return AREA.AREA1;

                if (GUID_AREA == null || NewItemRowSubAREACollection == null)
                    return null;

                return NewItemRowSubAREACollection.Where(x => x.GUID_PARENT == GUID_AREA);
            }
        }

        public bool IsSubAreaValid(Guid? subAreaGuid)
        {
            if (subAreaGuid == null)
                return false;

            if (SubAreaCollection == null)
                return false;

            return SubAreaCollection.Any(x => x.GUID == subAreaGuid);
        }

        [NotMapped]
        public PHASE CachedPHASE { get; set; }

        [NotMapped]
        public PhaseType? PhaseType =>  CachedPHASE != null ? CachedPHASE.PHASE_TYPE : PHASE != null ? PHASE.PHASE_TYPE : null;

        [NotMapped]
        public IEnumerable<ExoTimeAuthorisation> ExoLines { get; set; }

        [NotMapped]
        public IEnumerable<COMMODITY_CODE> FullCOMMODITY_CODECollection;
        public IEnumerable<COMMODITY_CODE> CommodityCodeCollection
        {
            get
            {
                if (PhaseType == null || FullCOMMODITY_CODECollection == null || GUID_DISCIPLINE == null)
                    return null;

                return FullCOMMODITY_CODECollection.Where(x => x.PHASE_TYPE == PhaseType && x.GUID_DISCIPLINE == GUID_DISCIPLINE).OrderBy(x => x.CODE);
            }
        }

        public IEnumerable<COMMODITY_CODE> StockCodeCollection
        {
            get
            {
                if (PhaseType == null || FullCOMMODITY_CODECollection == null || GUID_DISCIPLINE == null || (COMMODITY_CODE == null || COMMODITY_CODE == string.Empty))
                    return null;

                return FullCOMMODITY_CODECollection.Where(x => x.PHASE_TYPE == PhaseType && x.GUID_DISCIPLINE == GUID_DISCIPLINE).Where(x => x.CODE == COMMODITY_CODE).OrderBy(x => x.DEFAULT_STOCKCODE);
            }
        }

        [NotMapped]
        public IEnumerable<DISCIPLINE> DisciplineCollection
        {
            get
            {
                if (PhaseType == null || FullCOMMODITY_CODECollection == null)
                    return null;

                return FullCOMMODITY_CODECollection.Where(x => x.PHASE_TYPE == PhaseType).Where(x => x.DISCIPLINE != null).Select(x => x.DISCIPLINE).Distinct().OrderBy(x => x.CODE);
            }
        }

        public bool IsDisciplineCodeValid
        {
            get
            {
                if (PhaseType != null)
                {
                    //when commodity code collection is not set, don't show any error
                    if (CommodityCodeCollection == null)
                        return true;

                    return CommodityCodeCollection.Any(x => x.GUID_DISCIPLINE == GUID_DISCIPLINE);
                }
                //when phase type is not set, don't show any error
                else
                {
                    return true;
                }
            }
        }

        public bool IsCommodityCodeValid
        {
            get
            {
                if(PhaseType != null && COMMODITY_CODE != null)
                {
                    //when commodity code collection is not set, don't show any error
                    if (CommodityCodeCollection == null)
                        return true;

                    return CommodityCodeCollection.Any(x => x.CODE == COMMODITY_CODE);
                }
                //when phase type is not set, don't show any error
                else
                {
                    return true;
                }
            }
        }

        //public string Deliverable_Name => STOCK_CODE == null ? string.Empty : STOCK_CODE.CODE;
        //for scheduling view use
        public string Deliverable_Name
        {
            get
            {
                string name = string.Empty;
                name += Subjob_Name + "-";
                name += Discipline_Code + "-";
                name += Commodity_Code;
                return name;
            }
        }

        [NotMapped]
        public Guid? Subjob_Guid
        {
            get { return GUID_SUBJOB; }
            set { GUID_SUBJOB = value; }
        }

        [NotMapped]
        public string Discipline_Code
        {
            get
            {
                if (DISCIPLINE != null)
                    return DISCIPLINE.CODE + DISCIPLINE_NUM.ToString("00");
                else if (CachedDISCIPLINE != null)
                    return CachedDISCIPLINE.CODE + DISCIPLINE_NUM.ToString("00");
                else
                    return string.Empty;
            }
        }

        //used for storing newly added entity so that we don't change the context of the existing DISCIPLINE for newly added rows for Discipline_Code
        [NotMapped]
        public DISCIPLINE CachedDISCIPLINE { get; set; }

        public string Phase_Code => BluePrintsResources.Default_Construction_Phase;

        public string Commodity_Code
        {
            get
            {
                return COMMODITY_CODE;
            }
        }

        //used for storing newly added entity so that we don't change the context of the existing COMMODITY_CODE for newly added rows for Commodity_Code
        [NotMapped]
        public COMMODITY_CODE CachedCOMMODITY_CODE { get; set; }

        public Guid? Area_Guid => GUID_AREA;

        public Guid? SubArea_Guid => GUID_SUBAREA;

        public decimal Total_Units_IncludingByDuration => Budget_Units;

        public decimal Budget_Units
        {
            get
            {
                if (BUDGET_HOURS == null)
                    return 0;

                return (decimal)BUDGET_HOURS;
            }
        }

        [NotMapped]
        public decimal Total_Units
        {
            get
            {
                if (IsByDuration)
                    return BluePrintsConstants.DurationBasedTotalUnits;

                return Budget_Units + Variation_Units;
            }
        }

        public decimal Variation_Units
        {
            get
            {
                return 0;
            }
        }

        [NotMapped]
        public decimal? DB_Productivity_Override { get => PRODUCTIVITY_OVERRIDE; set => PRODUCTIVITY_OVERRIDE = value; }

        [NotMapped]
        public Guid? Variation_Guid { get => GUID_VARIATION; set => GUID_VARIATION = value; }

        [NotMapped]
        public Guid? Baseline_Guid { get => GUID_ESTIMATE; set => GUID_ESTIMATE = value; }

        [NotMapped]
        public decimal Estimated_Value { get => BUDGET_QUANTITY == null ? 0 : (decimal)BUDGET_QUANTITY; set => BUDGET_QUANTITY = value; }

        [NotMapped]
        public string Subjob_Name
        {
            get
            {
                if (SUBJOB != null)
                    return SUBJOB.INTERNAL_NAME1;
                else if (CachedSUBJOB != null)
                    return CachedSUBJOB.INTERNAL_NAME1;
                else
                    return string.Empty;
            }
        }

        //used for storing newly added SUBJOB so that we don't change the context of the existing SUBJOB for newly added rows for Subjob_Name
        [NotMapped]
        public SUBJOB CachedSUBJOB { get; set; }

        [NotMapped]
        public string Department_Code
        {
            get
            {
                return "CN";
            }
        }

        public Guid? Department_Guid => (Guid?)null;

        [NotMapped]
        public Guid? Phase_Guid { get => GUID_PHASE; set => GUID_PHASE = value; }

        [NotMapped]
        public Guid? Procurement_Subjob_Guid { get => GUID_PSUBJOB; set => GUID_PSUBJOB = value; }

        public Guid? Discipline_Guid => GUID_DISCIPLINE;

        public decimal Discipline_Number => DISCIPLINE_NUM;

        [NotMapped]
        public Guid? Workpack_Guid { get => GUID_WORKPACK; set => GUID_WORKPACK = value; }

        public PhaseType? Phase => PHASE == null ? null : PHASE.PHASE_TYPE;

        [NotMapped]
        public bool IsByDuration { get => BY_DURATION; set => BY_DURATION = value; }

        [NotMapped]
        public ChargeType? Charge => PHASE == null ? null : PHASE.CHARGE_TYPE;

        [NotMapped]
        public decimal Budget_Quantity => BUDGET_QUANTITY == null ? 0 : (decimal)BUDGET_QUANTITY;

        [NotMapped]
        public decimal Total_Quantity => Budget_Quantity;

        public string Variation_Code
        {
            get
            {
                if (VARIATION_CODE == null)
                    return string.Empty;

                return VARIATION_CODE;
            }
        }

        public string Office
        {
            get
            {
                if (this.ESTIMATE != null)
                    return this.ESTIMATE.PROJECT.NUMBER + " " + this.ESTIMATE.PROJECT.OfficeName;
                else if (this.VARIATION != null)
                    return this.VARIATION.PROJECT.NUMBER + " " + this.VARIATION.PROJECT.OfficeName;

                return string.Empty;
            }
        }

        public string Project_Number
        {
            get
            {
                if (this.ESTIMATE != null)
                    return this.ESTIMATE.PROJECT.NUMBER;
                else if (this.VARIATION != null)
                    return this.VARIATION.PROJECT.NUMBER;

                return string.Empty;
            }
        }

        public decimal Variation_Quantity => Variation_Units / UnitsPerQuantity;

        public decimal Unadjusted_Budget_Units => Budget_Units;

        public decimal UnitsPerQuantity => Total_Units / Total_Quantity;
    }
}
