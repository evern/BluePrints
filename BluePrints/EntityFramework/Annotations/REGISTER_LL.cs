namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.Misc;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [ConstraintAttributes("NUMBER")]
    public partial class REGISTER_LL : IGuidEntityKey, IEntityNumber, IHaveCreatedDate
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
        public string EntityNumber
        {
            get { return NUMBER; }
            set { NUMBER = value; }
        }

        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }
    }
}