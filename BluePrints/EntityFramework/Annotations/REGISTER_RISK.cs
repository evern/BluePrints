namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.Misc;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [ConstraintAttributes("NUMBER")]
    public partial class REGISTER_RISK : IGuidEntityKey, IEntityNumber
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

        public string EntityNumber
        {
            get { return NUMBER; }
            set { NUMBER = value; }
        }
    }
}