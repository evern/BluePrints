namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.Misc;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [ConstraintAttributes("INTERNAL_NUM")]
    public partial class AREA : IGuidEntityKey, IGuidParentEntityKey
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
    }
}