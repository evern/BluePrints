namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.Misc;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [ConstraintAttributes("CODE")]
    public partial class COMMODITY_CODE : IGuidEntityKey, IHaveCreatedDate
    {
        public COMMODITY_CODE()
        {
            RATE_SUPPLY = 0;
            HOURS_INSTALL = 0;
        }

        [NotMapped]
        public Guid EntityKey
        {
            get { return GUID; }
            set { GUID = value; }
        }

        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }
    }
}