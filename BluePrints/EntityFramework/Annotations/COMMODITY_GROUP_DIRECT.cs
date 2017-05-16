namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.Misc;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [ConstraintAttributes("GUID_PARENT, GUID_COMMODITYCODE")]
    public partial class COMMODITY_GROUP_DIRECT : IGuidEntityKey, IGuidParentEntityKey, IHaveCreatedDate
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
                return GUID_PARENT;
            }
            set
            {
                GUID_PARENT = value;
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
    }
}