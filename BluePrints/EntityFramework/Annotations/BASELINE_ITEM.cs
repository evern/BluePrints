namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.Misc;
    using BluePrints.Common;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [ConstraintAttributes("GUID_BASELINE, INTERNAL_NUM")]
    public partial class BASELINE_ITEM : IGuidEntityKey, IOriginalGuidEntityKey, IHaveCreatedDate
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
    }
}