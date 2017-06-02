namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.Misc;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    [ConstraintAttributes("NUMBER")]
    public partial class REGISTER_ISSUE : IGuidEntityKey, IEntityNumber
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
        public bool IsClosed
        {
            get
            {
                return DATE_CLOSED != null;
            }
        }

        [NotMapped]
        public REGISTER_CHANGE RegisterChange { get; set; }

        public void SetRegisterChange(IEnumerable<REGISTER_CHANGE> REGISTER_CHANGECollection)
        {
            RegisterChange = REGISTER_CHANGECollection.FirstOrDefault(x => x.GUID == GUID_CHANGE);
        }

        [NotMapped]
        public REGISTER_HOLD RegisterHold { get; set; }

        public void SetRegisterHold(IEnumerable<REGISTER_HOLD> REGISTER_HoldCollection)
        {
            RegisterHold = REGISTER_HoldCollection.FirstOrDefault(x => x.GUID == GUID_HOLD);
        }
    }
}