namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using BluePrints.Common;
    using BluePrints.Common.Base;
    using BluePrints.Common.Resources;
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

        public string ApprovedStr
        {
            get
            {
                return APPROVED == null ? BluePrintsResources.FalseConversionString : ((bool)APPROVED) ? BluePrintsResources.TrueConversionString : BluePrintsResources.FalseConversionString;
            }
        }

        public string InterdisciplinaryCheckStr
        {
            get
            {
                return INTERDISC_CHECK_COMPLETE ? BluePrintsResources.TrueConversionString : BluePrintsResources.FalseConversionString;
            }
        }

        public decimal EPCM_CostImpact
        {
            get
            {
                if (COST_IMPACT == null || COST_IMPACT == ScheduleImpact.No)
                    return 0;

                decimal epcmHoursImpact = EPCM_HOURS_IMPACT == null ? 0 : ((decimal)EPCM_HOURS_IMPACT);
                decimal avgHoursRate = AVG_HR_RATE == null ? 0 : ((decimal)AVG_HR_RATE);

                return epcmHoursImpact * avgHoursRate;
            }
        }

        public decimal TotalCostImpact
        {
            get
            {
                decimal capexImpact = CAPEX_IMPACT == null ? 0 : ((decimal)CAPEX_IMPACT);
                return EPCM_CostImpact + capexImpact;
            }
        }

        public string EntityGroup => string.Empty;

        public string Office => this.PROJECT.NUMBER + " " + this.PROJECT.OfficeName;
    }
}