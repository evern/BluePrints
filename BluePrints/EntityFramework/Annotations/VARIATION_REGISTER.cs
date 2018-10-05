namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using BluePrints.Common.Base;
    using DevExpress.Mvvm;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class VARIATION_REGISTER : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate
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
        public decimal Margin => CURRENT_VALUE - COST;

        [NotMapped]
        public decimal Margin_Percentage => CURRENT_VALUE == 0 ? 0 : Margin / CURRENT_VALUE;

        public string Office => this.PROJECT.NUMBER + " " + this.PROJECT.OFFICE.NAME;
    }
}