namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.Misc;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [ConstraintAttributes("NUMBER")]
    public partial class REGISTER_CHANGE : IGuidEntityKey
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
    }
}