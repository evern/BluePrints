namespace BluePrints.Data
{
    using BaseModel.Misc;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class COMMODITY_CODE : IGuidEntityKey
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

        public override string ToString()
        {
            return FULLCODE;
        }
    }
}