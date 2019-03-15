namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using BluePrints.Common.Base;
    using DevExpress.Mvvm;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [ConstraintAttributes("NUMBER")]
    public partial class REGISTER_CHANGE : EntityBase, IGuidEntityKey, ICanSync, IEntityNumber, IHaveCreatedDate
    {
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

        public string EntityGroup => string.Empty;

        public string Office => this.PROJECT.NUMBER + " " + this.PROJECT.OfficeName;
    }
}