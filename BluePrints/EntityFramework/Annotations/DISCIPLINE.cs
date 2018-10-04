namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using BluePrints.Common.Base;
    using DevExpress.Mvvm;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [ConstraintAttributes("CODE")]
    public partial class DISCIPLINE : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate
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

        //used for collection mapping which allows null values
        [NotMapped]
        public Guid? MappingEntityKey
        {
            get
            {
                if (EntityKey == Guid.Empty)
                    return null;

                return EntityKey;
            }
        }
    }
}