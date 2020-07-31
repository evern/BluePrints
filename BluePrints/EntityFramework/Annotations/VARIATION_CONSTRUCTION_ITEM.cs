namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using BluePrints.Common.Base;
    using DevExpress.Mvvm;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class VARIATION_CONSTRUCTION_ITEM : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate
    {
        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        public string Office => this.VARIATION_CONSTRUCTION.PROJECT.NUMBER + " " + this.VARIATION_CONSTRUCTION.PROJECT.OfficeName;
    }
}