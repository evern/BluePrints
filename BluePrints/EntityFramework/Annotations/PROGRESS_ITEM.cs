namespace BluePrints.Data
{
    using BaseModel.Misc;
    using DevExpress.Mvvm.POCO;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class PROGRESS_ITEM : IGuidEntityKey, IHaveCreatedDate
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