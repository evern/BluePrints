namespace BluePrints.Data
{
    using BaseModel.Misc;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class VARIATION_ITEM : IGuidEntityKey
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