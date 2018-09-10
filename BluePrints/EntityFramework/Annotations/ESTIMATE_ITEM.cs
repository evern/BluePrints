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

namespace BluePrints.Data
{
    public partial class ESTIMATE_ITEM : EntityBase, IGuidEntityKey, IOriginalGuidEntityKey, IHaveCreatedDate, IDeliverable, ISupportByDuration, IHaveDBProductivityOverride, ISupportVariation, IHaveProcurementSubjob
    {
        public ESTIMATE_ITEM()
        {
            DISCIPLINE_NUM = 1;
            PROGRESS_TYPE = 0;
        }

        [NotMapped]
        public Guid EntityKey
        {
            get
            {
                return GUID;
            }

            set
            {
                GUID = value;
            }
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
        public IEnumerable<AREA> SubAreaCollection
        {
            get
            {
                if (AREA == null)
                    return null;

                return AREA.AREA1;
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

        //public string Deliverable_Name => STOCK_CODE == null ? string.Empty : STOCK_CODE.CODE;
        public string Deliverable_Name => NAME;

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
                if (DISCIPLINE == null)
                    return string.Empty;

                return DISCIPLINE.CODE + DISCIPLINE_NUM;
            }
        }

        public string Phase_Code => BluePrintsResources.Default_Construction_Phase;

        public string Commodity_Code => COMMODITY_CODE == null ? string.Empty : COMMODITY_CODE.CODE;

        public Guid? Area_Guid => GUID_AREA;

        public Guid? SubArea_Guid => GUID_SUBAREA;

        public decimal Total_Units_IncludingByDuration => Budget_Units;

        public decimal Budget_Units
        {
            get
            {
                //if (IsByDuration)
                //    return BluePrintsConstants.DurationBasedTotalUnits;

                if (STOCK_CODE == null)
                    return 0;

                if (BUDGET_QUANTITY == null)
                    return 0;

                return (decimal)BUDGET_QUANTITY * STOCK_CODE.HOURS_INSTALL;
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
                if (STOCK_CODE == null)
                    return 0;
                else
                    return DC_QUANTITY * STOCK_CODE.HOURS_INSTALL;
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
        public decimal DC_Value { get => DC_QUANTITY; set => DC_QUANTITY = value; }

        [NotMapped]
        public string Subjob_Name
        {
            get
            {
                if (SUBJOB == null)
                    return string.Empty;

                return SUBJOB.INTERNAL_NAME1;
            }
        }

        [NotMapped]
        public string Department_Code
        {
            get
            {
                return "CN";
            }
        }

        [NotMapped]
        public Guid? Phase_Guid { get => GUID_PHASE; set => GUID_PHASE = value; }

        [NotMapped]
        public Guid? Procurement_Subjob_Guid { get => GUID_PSUBJOB; set => GUID_PSUBJOB = value; }

        public Guid? Discipline_Guid => GUID_DISCIPLINE;

        public decimal Discipline_Number => DISCIPLINE_NUM;

        [NotMapped]
        public Guid? Workpack_Guid { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public PhaseType? Phase => PHASE == null ? null : PHASE.PHASE_TYPE;

        [NotMapped]
        public bool IsByDuration { get => BY_DURATION; set => BY_DURATION = value; }

        [NotMapped]
        public ChargeType? Charge => PHASE == null ? null : PHASE.CHARGE_TYPE;

        [NotMapped]
        public decimal Budget_Quantity => BUDGET_QUANTITY == null ? 0 : (decimal)BUDGET_QUANTITY;

        [NotMapped]
        public decimal Total_Quantity => DC_QUANTITY + Budget_Quantity;

        public string Variation_Code => string.Empty;
    }
}
