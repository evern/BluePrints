namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using BluePrints.Common.Base;
    using DevExpress.Mvvm;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [ConstraintAttributes("INTERNAL_NUM")]
    public partial class AREA : EntityBase, IGuidEntityKey, ICanSync, IGuidParentEntityKey, IHaveCreatedDate
    {
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

        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        public override string ToString()
        {
            return INTERNAL_NUM;
        }

        public string Office => this.PROJECT == null ? string.Empty : this.PROJECT.NUMBER + " " + this.PROJECT.OfficeName;
    }
}