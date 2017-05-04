namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.Misc;
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
            get { return ESTIMATED_HOURS + DC_HOURS; }
        }
    }
}