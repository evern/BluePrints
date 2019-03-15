namespace BluePrints.Data
{
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using BluePrints.Common.Base;
    using BluePrints.P6Data;
    using DevExpress.Mvvm;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class P6_ASSIGNMENT : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate
    {
        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        public string Office => this.PROJECT.NUMBER + " " + this.PROJECT.OfficeName;
    }
}