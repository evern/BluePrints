namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.Misc;
    using Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [ConstraintAttributes("NUMBER")]
    public partial class PROJECT : IGuidEntityKey
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PROJECT()
        {
            AREA = new HashSet<AREA>();
            BASELINE = new HashSet<BASELINE>();
            BASELINE_ITEM_ASSIGNMENT = new HashSet<BASELINE_ITEM_ASSIGNMENT>();
            COMMODITY_CODE = new HashSet<COMMODITY_CODE>();
            ESTIMATION_DIRECT = new HashSet<ESTIMATION_DIRECT>();
            ESTIMATION_INDIRECT = new HashSet<ESTIMATION_INDIRECT>();
            ESTIMATION_SETTING = new HashSet<ESTIMATION_SETTING>();
            PHASE = new HashSet<PHASE>();
            PROGRESS = new HashSet<PROGRESS>();
            REGISTER = new HashSet<REGISTER>();
            REGISTER_CHANGE = new HashSet<REGISTER_CHANGE>();
            REGISTER_HOLD = new HashSet<REGISTER_HOLD>();
            REGISTER_ISSUE = new HashSet<REGISTER_ISSUE>();
            REGISTER_RISK = new HashSet<REGISTER_RISK>();
            PROJECT_REPORT = new HashSet<PROJECT_REPORT>();
            RATE = new HashSet<RATE>();
            VARIATION = new HashSet<VARIATION>();
            WORKPACK = new HashSet<WORKPACK>();

            STATUS = ProjectStatus.Active;
            CONTRACTTYPE = ContractType.LumpSum;
            USELEGACYWORKPACK = true;
            CURRENCYCONVERSION = 1;
            REVIEWPERCENTAGE = 0.7M;
            REVIEWPERIOD = 5;
        }

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
    }
}