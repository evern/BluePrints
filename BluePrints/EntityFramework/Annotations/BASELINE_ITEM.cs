namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using BluePrints.Common;
    using BluePrints.Common.Base;
    using BluePrints.Common.Resources;
    using BluePrints.Common.ViewModel.Reporting;
    using DevExpress.Data.Filtering;
    using DevExpress.Mvvm;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    [ConstraintAttributes("GUID_BASELINE, INTERNAL_NUM")]
    public partial class BASELINE_ITEM : EntityBase, ICanSync, IDeliverable, IEntityNumber, IHaveCreatedDate, IHaveDBProductivityOverride, IHaveDeliverableStatus, IOriginalGuidEntityKey, ISupportVariationRevision, IGuidEntityKey

    {
        public BASELINE_ITEM()
        {
            DISCIPLINE_NUM = 1;
            DELIVERABLE_TYPE = DeliverableType.Deliverable;
            ExperimentalSubAreaGuid = new TokenProperty<Guid?>(() => GUID_SUBAREA, x => this.GUID_SUBAREA = x, () => AREA == null ? null : this.AREA.AREA1.Select(x => (Guid?)x.GUID));
        }

        [NotMapped]
        TokenProperty<Guid?> ExperimentalSubAreaGuid;

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

        [NotMapped]
        public Guid? DeliverableStatusGuid
        {
            get
            {
                return GUID_STATUS;
            }
            set
            {
                Guid? setValue = (Guid?)value;
                if (setValue == null)
                    GUID_STATUS = null;
                else if (IsDeliverableStatusValid(setValue))
                    GUID_STATUS = setValue;
            }
        }

        public bool SetDeliverableStatusByName(string statusName)
        {
            if (DeliverableStatusCollection == null)
                return false;

            DELIVERABLES_STATUS deliverable_status_by_name = DeliverableStatusCollection.FirstOrDefault(x => x.NAME.ToUpper() == statusName.ToUpper());
            if (deliverable_status_by_name != null)
            {
                GUID_STATUS = deliverable_status_by_name.GUID;
                return true;
            }

            return false;
        }

        public bool IsDeliverableStatusValid(Guid? DeliverableStatusGuid)
        {
            if (DeliverableStatusGuid == null)
                return false;

            if (DeliverableStatusCollection == null)
                return false;

            return DeliverableStatusCollection.Any(x => x.GUID == DeliverableStatusGuid);
        }

        [NotMapped]
        public CriteriaOperator ResourceFilterCriteria
        {
            get
            {
                if (GUID_DEPARTMENT != null && GUID_DISCIPLINE != null)
                    return CriteriaOperator.Parse("[GUID_DEPARTMENT] In ({" + GUID_DEPARTMENT.ToString() + "}) And [GUID_DISCIPLINE] In ({" + GUID_DISCIPLINE.ToString() + "})");
                else if (GUID_DISCIPLINE != null)
                    return CriteriaOperator.Parse("[GUID_DISCIPLINE] In ({" + GUID_DISCIPLINE.ToString() + "})");
                else if (GUID_DEPARTMENT != null)
                    return CriteriaOperator.Parse("[GUID_DEPARTMENT] In ({" + GUID_DEPARTMENT.ToString() + "})");

                return null;
            }
        }

        [NotMapped]
        public IEnumerable<DELIVERABLES_STATUS> DeliverableStatusCollection { get; set; }

        [NotMapped]
        public string Holds { get; set; }

        public void SetHolds(IEnumerable<REGISTER_HOLD_REF> holds)
        {
            List<string> deliverable_holds = holds.Where(x => x.GUID_BASELINE_ITEM == this.GUID_ORIGINAL && x.REGISTER_HOLD != null).Select(x => x.REGISTER_HOLD.NUMBER).ToList();
            if (deliverable_holds.Count == 0)
                return;

            string holdStr = string.Empty;
            foreach (string hold in deliverable_holds)
            {
                holdStr += hold + ", ";
            }

            Holds = holdStr.Substring(0, holdStr.Length - 2);
        }

        public string Office
        {
            get
            {
                if (this.BASELINE != null && this.BASELINE.PROJECT != null)
                    return "Baseline " + this.BASELINE.PROJECT.NUMBER + " " + this.BASELINE.PROJECT.OfficeName;
                else if (this.VARIATION != null && this.VARIATION.PROJECT != null)
                    return "Variation " + this.VARIATION.PROJECT.NUMBER + " " + this.VARIATION.PROJECT.OfficeName;

                return BluePrintsResources.GlobalOffice;
            }
        }

        public string Project_Number
        {
            get
            {
                if (this.BASELINE != null)
                    return this.BASELINE.PROJECT.NUMBER;
                else if (this.VARIATION != null)
                    return this.VARIATION.PROJECT.NUMBER;

                return string.Empty;
            }
        }

        public string Subjob_Name
        {
            get
            {
                if (SUBJOB == null)
                    return string.Empty;

                return SUBJOB.INTERNAL_NAME1;
            }
        }

        public PhaseType? Phase => PHASE == null ? null : PHASE.PHASE_TYPE;

        public ChargeType? Charge => PHASE == null ? null : PHASE.CHARGE_TYPE;

        public string Phase_Code => PHASE == null ? string.Empty : PHASE.INTERNAL_NUM;

        public string Department_Code => DEPARTMENT == null ? string.Empty : DEPARTMENT.CODE;

        public string Discipline_Code
        {
            get
            {
                if (DISCIPLINE == null)
                    return string.Empty;

                return DISCIPLINE.CODE + DISCIPLINE_NUM.ToString("00");
            }
        }

        public string Deliverable_Name => INTERNAL_NUM;

        [NotMapped]
        public Guid? Phase_Guid { get => GUID_PHASE; set => GUID_PHASE = value; }

        [NotMapped]
        public Guid? Subjob_Guid { get => GUID_SUBJOB; set => GUID_SUBJOB = value; }

        public Guid? Area_Guid => GUID_AREA;

        public Guid? SubArea_Guid => GUID_SUBAREA;

        public Guid? Discipline_Guid => GUID_DISCIPLINE;

        public decimal Discipline_Number => DISCIPLINE_NUM;

        [NotMapped]
        public Guid? Workpack_Guid { get => GUID_WORKPACK; set => GUID_WORKPACK = value; }

        [NotMapped]
        public bool IsByDuration { get => BY_DURATION; set => BY_DURATION = value; }

        public Guid OriginalEntityKey => GUID_ORIGINAL;

        public string Commodity_Code => DOCTYPE == null ? string.Empty : DOCTYPE.CODE;

        public decimal Budget_Units => BUDGET_HOURS;

        public decimal Budget_Quantity => BUDGET_HOURS;

        public decimal Total_Quantity => BUDGET_HOURS;

        [NotMapped]
        public string EntityNumber { get => INTERNAL_NUM; set => INTERNAL_NUM = value; }

        public string EntityGroup => string.Empty;

        [NotMapped]
        public DateTime EntityCreatedDate { get => CREATED; set => CREATED = value; }

        [NotMapped]
        public decimal? DB_Productivity_Override { get => PRODUCTIVITY_OVERRIDE; set => PRODUCTIVITY_OVERRIDE = value; }

        public DELIVERABLES_STATUS Deliverable_Status => DELIVERABLES_STATUS;

        [NotMapped]
        public Guid? Variation_Guid { get => GUID_VARIATION; set => GUID_VARIATION = value; }

        public void SetOriginalEntityKey(Guid newGuid)
        {
            GUID_ORIGINAL = newGuid;
        }
    }
}