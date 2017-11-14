namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.Misc;
    using BluePrints.Common;
    using BluePrints.Common.Base;
    using BluePrints.Common.ViewModel.Reporting;
    using DevExpress.Mvvm;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    [ConstraintAttributes("GUID_BASELINE, INTERNAL_NUM")]
    public partial class BASELINE_ITEM : BluePrintsEntityBase, IGuidEntityKey, IOriginalGuidEntityKey, IHaveCreatedDate, IDeliverable, ISupportByDuration, IHaveDBProductivityOverride, ISupportVariation, IEntityNumber
    {
        public BASELINE_ITEM()
        {
            DISCIPLINE_NUM = 1;
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

        public void SetOriginalEntityKey(Guid newGuid)
        {
            GUID_ORIGINAL = newGuid;
        }
       
        [NotMapped]
        public string Discipline_Code
        {
            get
            {
                if (DISCIPLINE == null)
                    return string.Empty;

                if (DISCIPLINE_NUM < 10)
                    return DISCIPLINE.CODE + "0" + DISCIPLINE_NUM.ToString();
                else
                    return DISCIPLINE.CODE + DISCIPLINE_NUM.ToString();
            }
        }

        [NotMapped]
        public string Department_Code
        {
            get
            {
                if (DEPARTMENT == null)
                    return string.Empty;

                return DEPARTMENT.CODE;
            }
        }

        [NotMapped]
        public string Phase_Code => PHASE == null ? string.Empty : PHASE.INTERNAL_NUM;

        [NotMapped]
        public string Commodity_Code
        {
            get
            {
                if (DOCTYPE == null)
                    return string.Empty;

                return DOCTYPE.CODE;
            }
        }
        
        public string Commodity_Display_Code => Commodity_Code;

        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        [NotMapped]
        public string Workpack_Name
        {
            get
            {
                if (WORKPACK == null)
                    return string.Empty;

                return WORKPACK.INTERNAL_NAME1;
            }
        }

        public string Area_Name
        {
            get
            {
                if (AREA == null)
                    return string.Empty;

                return AREA.INTERNAL_NUM;
            }
        }

        public string SubArea_Name
        {
            get
            {
                if (AREA1 == null)
                    return string.Empty;

                return AREA1.INTERNAL_NUM;
            }
        }

        public string DocType_Name
        {
            get
            {
                if (DOCTYPE == null)
                    return string.Empty;

                return DOCTYPE.NAME;
            }
        }

        public string Discipline_Name
        {
            get
            {
                if (DISCIPLINE == null)
                    return string.Empty;

                return DISCIPLINE.NAME;
            }
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

        public bool IsDeliverableStatusValid(Guid? DeliverableStatusGuid)
        {
            if (DeliverableStatusGuid == null)
                return false;

            if (DeliverableStatusCollection == null)
                return false;

            return DeliverableStatusCollection.Any(x => x.GUID == DeliverableStatusGuid);
        }

        [NotMapped]
        public IEnumerable<DELIVERABLES_STATUS> DeliverableStatusCollection
        {
            get
            {
                if (DOCTYPE == null || DOCTYPE.DELIVERABLES_STATUS == null)
                    return null;

                if (BASELINE == null && VARIATION == null)
                    return null;

                PROJECT PROJECT;
                if (BASELINE != null)
                    PROJECT = BASELINE.PROJECT;
                else
                    PROJECT = VARIATION.PROJECT;

                return DOCTYPE.DELIVERABLES_STATUS
                    .Where(x => x.GUID_PROJECT == PROJECT.GUID)
                    .Where(x => 
                            (x.FOR_DELIVERABLE && DELIVERABLE_TYPE == DeliverableType.Deliverable) ||
                            (x.FOR_NCR && DELIVERABLE_TYPE == DeliverableType.DeliverableNCR) || 
                            (x.FOR_TASK && DELIVERABLE_TYPE == DeliverableType.Task)).OrderBy(x => x.AUTO_PERCENTAGE);
            }
        }

        [NotMapped]
        public string Deliverable_Name
        {
            get { return INTERNAL_NUM; }
        }

        [NotMapped]
        public Guid? Workpack_Guid
        {
            get { return GUID_WORKPACK; }
            set { GUID_WORKPACK = value; }
        }

        [NotMapped]
        public decimal Estimated_Units
        {
            get
            {
                if (IsByDuration)
                    return BluePrintsConstants.DurationBasedTotalUnits;

                return ESTIMATED_HOURS;
            }
        }

        [NotMapped]
        public decimal Total_Units
        {
            get
            {
                if (IsByDuration)
                    return BluePrintsConstants.DurationBasedTotalUnits;

                return ESTIMATED_HOURS + DC_HOURS;
            }
        }

        [NotMapped]
        public Guid? Area_Guid => GUID_AREA;

        [NotMapped]
        public Guid? SubArea_Guid => GUID_SUBAREA;

        [NotMapped]
        public decimal Variation_Units => DC_HOURS;

        [NotMapped]
        public bool IsByDuration => BY_DURATION;

        [NotMapped]
        public decimal? DB_Productivity_Override { get => PRODUCTIVITY_OVERRIDE; set => PRODUCTIVITY_OVERRIDE = value; }

        [NotMapped]
        public Guid? Variation_Guid { get => GUID_VARIATION; set => GUID_VARIATION = value; }

        [NotMapped]
        public Guid? Baseline_Guid { get => GUID_BASELINE; set => GUID_BASELINE = value; } 

        [NotMapped]
        public decimal Estimated_Value { get => ESTIMATED_HOURS; set => ESTIMATED_HOURS = value; }

        [NotMapped]
        public decimal DC_Value { get => DC_HOURS; set => DC_HOURS = value; }

        [NotMapped]
        public string EntityNumber { get => INTERNAL_NUM; set => INTERNAL_NUM = value; }

        [NotMapped]
        public string EntityGroup => string.Empty;

        [NotMapped]
        public Guid? Phase_Guid { get => GUID_PHASE; set => GUID_PHASE = value; }
    }
}