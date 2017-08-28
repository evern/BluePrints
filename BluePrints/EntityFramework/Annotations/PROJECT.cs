namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.Misc;
    using Common;
    using DevExpress.Mvvm;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [ConstraintAttributes("NUMBER")]
    public partial class PROJECT : BindableBase, IGuidEntityKey, IHaveCreatedDate, ICanUpdate
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PROJECT()
        {
            AREA = new HashSet<AREA>();
            BASELINE = new HashSet<BASELINE>();
            CLIENT_PROJECT = new HashSet<CLIENT_PROJECT>();
            STOCK_GROUP = new HashSet<STOCK_GROUP>();
            DELIVERABLES_STATUS = new HashSet<DELIVERABLES_STATUS>();
            ESTIMATION_DIRECT = new HashSet<ESTIMATION_DIRECT>();
            MEETING = new HashSet<MEETING>();
            PROGRESS = new HashSet<PROGRESS>();
            STOCK_CODE = new HashSet<STOCK_CODE>();
            REGISTER = new HashSet<REGISTER>();
            PROJECT_REPORT = new HashSet<PROJECT_REPORT>();
            RATE = new HashSet<RATE>();
            REGISTER_CHANGE = new HashSet<REGISTER_CHANGE>();
            REGISTER_HOLD = new HashSet<REGISTER_HOLD>();
            REGISTER_ISSUE = new HashSet<REGISTER_ISSUE>();
            REGISTER_LL = new HashSet<REGISTER_LL>();
            REGISTER_NC = new HashSet<REGISTER_NC>();
            REGISTER_RISK = new HashSet<REGISTER_RISK>();
            VARIATION = new HashSet<VARIATION>();
            WORKPACK = new HashSet<WORKPACK>();
            P6_ASSIGNMENT = new HashSet<P6_ASSIGNMENT>();
            STATUS = ProjectStatus.Active;
            CONTRACTTYPE = ContractType.LumpSum;
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

        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get
            {
                return CREATED;
            }
            set
            {
                CREATED = value;
            }
        }

        public void Update()
        {
            RaisePropertiesChanged();
        }
    }
}