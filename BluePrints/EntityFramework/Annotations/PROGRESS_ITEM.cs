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

        //because EARNED_UNITS can be quantity
        [NotMapped]
        public decimal EarnedUnits
        {
            get
            {
                if (BUDGET_INSTALL_HOURS_PER_QTY == null || STAGE_WEIGHT == null)
                    return EARNED_UNITS;

                return EARNED_UNITS * (decimal)STAGE_WEIGHT * (decimal)BUDGET_INSTALL_HOURS_PER_QTY;
            }
            set
            {
                if (BUDGET_INSTALL_HOURS_PER_QTY == null || STAGE_WEIGHT == null)
                    EARNED_UNITS = value;
                else
                    //convert to qty
                    EARNED_UNITS = value / (decimal)STAGE_WEIGHT / (decimal)BUDGET_INSTALL_HOURS_PER_QTY;
            }
        }

        public decimal ReportingEarnedUnits => EARNED_UNITS <= BluePrintsConstants.DurationBasedTotalUnits ? 0 : EARNED_UNITS;
        public string Office => this.PROGRESS.PROJECT.NUMBER + " " + this.PROGRESS.PROJECT.OfficeName;
    }
}