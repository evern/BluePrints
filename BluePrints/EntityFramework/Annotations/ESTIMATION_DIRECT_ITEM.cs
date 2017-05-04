namespace BluePrints.Data
{
    using BaseModel.Misc;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class ESTIMATION_DIRECT_ITEM : IGuidEntityKey
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

        public ESTIMATION_DIRECT_ITEM()
        {
            ESTIMATED_QUANTITY = 1;
        }

        public decimal TOTAL_QUANTITY
        {
            get { return ESTIMATED_QUANTITY + VAR_QUANTITY; }
        }
    }
}