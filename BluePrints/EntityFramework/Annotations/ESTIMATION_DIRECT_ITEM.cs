namespace BluePrints.Data
{
    using BaseModel.Misc;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class ESTIMATION_DIRECT_ITEM : IGuidEntityKey, IGuidParentEntityKey, IHaveCreatedDate
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
        public Guid? ParentEntityKey
        {
            get
            {
                return GUID_ORIGINAL_PARENT;
            }
            set
            {
                if (value != null)
                    GUID_ORIGINAL_PARENT = (Guid)value;
                else
                    GUID_ORIGINAL_PARENT = Guid.Empty;
            }
        }

        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get
            {
                return CREATED;
            }
            set
            {
                CREATED = value;
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