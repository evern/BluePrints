namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.Misc;
    using BluePrints.Common;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [ConstraintAttributes("GUID_BASELINE, INTERNAL_NUM")]
    public partial class BASELINE_ITEM : IGuidEntityKey, IOriginalGuidEntityKey
    {
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
    }
}