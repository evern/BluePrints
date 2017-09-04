namespace BluePrints.Data
{
    using BaseModel.Misc;
    using BluePrints.Common.Base;
    using DevExpress.Mvvm;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class USER : BluePrintsEntityBase, IGuidEntityKey, IHaveCreatedDate
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

        [NotMapped]
        public string Full_Name
        {
            get
            {
                if (FIRST_NAME == null)
                    return LAST_NAME;

                if (LAST_NAME == null)
                    return FIRST_NAME;

                return FIRST_NAME + " " + LAST_NAME;
            }
        }
    }
}