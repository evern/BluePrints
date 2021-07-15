namespace BluePrints.Data
{
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using BluePrints.Common;
    using BluePrints.Common.Base;
    using DevExpress.Mvvm;
    using DevExpress.Mvvm.POCO;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class PROGRESS_ITEM : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate
    {
        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        public decimal ReportingEarnedUnits => EARNED_UNITS <= BluePrintsConstants.DurationBasedTotalUnits ? 0 : EARNED_UNITS;
        public string Office => this.PROGRESS.PROJECT.NUMBER + " " + this.PROGRESS.PROJECT.OfficeName;
    }
}