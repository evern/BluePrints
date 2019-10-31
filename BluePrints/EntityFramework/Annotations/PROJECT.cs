namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using BluePrints.Common.Base;
    using BluePrints.Common.Resources;
    using BluePrints.Common.ViewModel.Reporting;
    using Common;
    using DevExpress.Mvvm;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    [ConstraintAttributes("NUMBER")]
    public partial class PROJECT : EntityBase, IHaveP6Baselines, ICanSync, IHaveCreatedDate
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PROJECT()
        {
            AREA = new HashSet<AREA>();
            BASELINE = new HashSet<BASELINE>();
            BASELINE_ITEM_WORK = new HashSet<BASELINE_ITEM_WORK>();
            CLIENT_PROJECT = new HashSet<CLIENT_PROJECT>();
            STOCK_GROUP = new HashSet<STOCK_GROUP>();
            DELIVERABLES_STATUS = new HashSet<DELIVERABLES_STATUS>();
            ESTIMATE = new HashSet<ESTIMATE>();
            FORECAST_JOB = new HashSet<FORECAST_JOB>();
            FORECAST_JOB_SETTING = new HashSet<FORECAST_JOB_SETTING>();
            FORECAST = new HashSet<FORECAST>();
            FORECAST_PO = new HashSet<FORECAST_PO>();
            FORECAST_PO_SETTING = new HashSet<FORECAST_PO_SETTING>();
            HOLIDAY = new HashSet<HOLIDAY>();
            HSE = new HashSet<HSE>();
            MEETING = new HashSet<MEETING>();
            MEETING_TYPE = new HashSet<MEETING_TYPE>();
            MINUTE_AGENDA = new HashSet<MINUTE_AGENDA>();
            PROGRESS = new HashSet<PROGRESS>();
            PROJECT_SUMMARY = new HashSet<PROJECT_SUMMARY>();
            PROJECT_SUMMARY_SETTINGS = new HashSet<PROJECT_SUMMARY_SETTING>();
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
            ROSTER_STAFF = new HashSet<ROSTER_STAFF>();
            VARIATION = new HashSet<VARIATION>();
            SUBJOB = new HashSet<SUBJOB>();
            P6_ASSIGNMENT = new HashSet<P6_ASSIGNMENT>();
            STATUS = ProjectStatus.Active;
            TENDER_PROFILE = new HashSet<TENDER_PROFILE>();
            VARIATION_REGISTER = new HashSet<VARIATION_REGISTER>();
            CONTRACTTYPE = ContractType.LumpSum;
            CURRENCYCONVERSION = 1;
            REVIEWPERCENTAGE = 0.7M;
            REVIEWPERIOD = 5;
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

        [NotMapped]
        public string OfficeName
        {
            get
            {
                if (OFFICE == null)
                    return BluePrintsResources.GlobalOffice;

                return OFFICE.NAME;
            }
        }

        public string OfficeNameForExo
        {
            get
            {
                if (OFFICE == null)
                    return BluePrintsResources.OfficeMontreal;

                return OFFICE.NAME.ToUpper();
            }
        }

        [NotMapped]
        private IEnumerable<object> disciplines;

        [NotMapped]
        public object Disciplines
        {
            get { return disciplines; }
            set
            {
                if (value != disciplines)
                {
                    disciplines = value as IEnumerable<object>;
                }
            }
        }

        [NotMapped]
        public IEnumerable<DISCIPLINE> Project_Disciplines
        {
            get
            {
                if (disciplines == null)
                    return null;

                return disciplines.Select(x => (DISCIPLINE)x);
            }
        }

        [NotMapped]
        public ProjectStatus Status
        {
            get
            {
                return STATUS;
            }
            set
            {
                STATUS = value;
                this.RaisePropertiesChanged();
            }
        }

        [NotMapped]
        public bool IsTender
        {
            get
            {
                return STATUS == ProjectStatus.Tender || STATUS == ProjectStatus.TenderSubmitted;
            }
        }

        public string Office => this.NUMBER + " " + this.OfficeName;

        public Guid project_guid => GUID;

        public string P6_Baseline_Name => P6FORECAST_NAME;

        public string P6_Mod_Baseline_Name => string.Empty;
    }
}