namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.Misc;
    using BluePrints.Common;
    using BluePrints.Common.ViewModel.Reporting;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    [ConstraintAttributes("GUID_BASELINE, INTERNAL_NUM")]
    public partial class BASELINE_ITEM : IGuidEntityKey, IOriginalGuidEntityKey, IHaveCreatedDate, ISortableDeliverable
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

            set
            {
                GUID_ORIGINAL = value;
            }
        }

        [NotMapped]
        public decimal TOTAL_HOURS
        {
            get
            {
                return ESTIMATED_HOURS + DC_HOURS;
            }
        }

        [NotMapped]
        public decimal Total_HoursIncludeByDuration
        {
            get
            {
                if (BY_DURATION)
                    return BluePrintsConstants.DurationBasedTotalUnits;

                return ESTIMATED_HOURS + DC_HOURS;
            }
        }

        [NotMapped]
        public string StockCode
        {
            get
            {
                if (DISCIPLINE == null)
                    return string.Empty;

                return DISCIPLINE.CODE + DISCIPLINE_NUM;
            }
        }

        [NotMapped]
        public string CommodityCode
        {
            get
            {
                if (DOCTYPE == null)
                    return string.Empty;

                return DOCTYPE.CODE;
            }
        }

        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        [NotMapped]
        public string WORKPACK_NAME
        {
            get
            {
                if (WORKPACK == null)
                    return string.Empty;

                return WORKPACK.INTERNAL_NAME1;
            }
        }

        public string AREA_NAME
        {
            get
            {
                if (AREA == null)
                    return string.Empty;

                return AREA.INTERNAL_NUM;
            }
        }

        public string SUBAREA_NAME
        {
            get
            {
                if (AREA1 == null)
                    return string.Empty;

                return AREA1.INTERNAL_NUM;
            }
        }

        public string DOCTYPE_NAME
        {
            get
            {
                if (DOCTYPE == null)
                    return string.Empty;

                return DOCTYPE.NAME;
            }
        }

        public string DISCIPLINE_NAME
        {
            get
            {
                if (DISCIPLINE == null)
                    return string.Empty;

                return DISCIPLINE.NAME;
            }
        }

        public string DEPARTMENT_NAME
        {
            get
            {
                if (DEPARTMENT == null)
                    return string.Empty;

                return DEPARTMENT.NAME;
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

                return DOCTYPE.DELIVERABLES_STATUS
                    .Where(x => x.GUID_PROJECT == BASELINE.GUID_PROJECT)
                    .Where(x => 
                            (x.FOR_DELIVERABLE && DELIVERABLE_TYPE == DeliverableType.Deliverable) ||
                            (x.FOR_NCR && DELIVERABLE_TYPE == DeliverableType.DeliverableNCR) || 
                            (x.FOR_TASK && DELIVERABLE_TYPE == DeliverableType.Task)).OrderBy(x => x.AUTO_PERCENTAGE);
            }
        }

        [NotMapped]
        public string ReportableItem_Name
        {
            get { return INTERNAL_NUM; }
        }

        [NotMapped]
        public string Commodity_Code
        {
            get { return CommodityCode; }
        }

        [NotMapped]
        public string Stock_Code
        {
            get { return StockCode; }
        }

        [NotMapped]
        public Guid? Workpack_Guid
        {
            get { return GUID_WORKPACK; }
        }

        [NotMapped]
        public decimal TotalHoursIncludeByDuration
        {
            get { return Total_HoursIncludeByDuration; }
        }

        [NotMapped]
        public decimal EstimatedHours
        {
            get { return ESTIMATED_HOURS; }
        }

        [NotMapped]
        public decimal TotalHours
        {
            get { return TOTAL_HOURS; }
        }

        [NotMapped]
        public Guid? Area_Guid => GUID_AREA;

        [NotMapped]
        public Guid? SubArea_Guid => GUID_SUBAREA;

        [NotMapped]
        public decimal Estimated_Quantity => EstimatedHours;

        [NotMapped]
        public decimal Total_Quantity => TotalHours;
    }
}