namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.Misc;
    using BluePrints.Common.ViewModel.Reporting;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using BluePrints.Common;
    using DevExpress.Mvvm;
    using BluePrints.Common.Base;
    using BaseModel.DataModel;

    public partial class PROJECT_SUMMARY_SETTING : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate
    {
        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        public string Office => this.PROJECT == null ? string.Empty : this.PROJECT.NUMBER + " " + this.PROJECT.OfficeName;
    }
}