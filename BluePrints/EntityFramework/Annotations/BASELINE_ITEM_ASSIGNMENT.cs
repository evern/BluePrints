namespace BluePrints.Data
{
    using BaseModel.Misc;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class BASELINE_ITEM_ASSIGNMENT : IGuidEntityKey, IHaveCreatedDate
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
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }
    }
}