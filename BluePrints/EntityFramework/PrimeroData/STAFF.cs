namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("STAFF")]
    public partial class STAFF
    {
        public STAFF()
        {
            CRM_BUDGET_LINE = new HashSet<CRM_BUDGET_LINE>();
            CRM_BUDGET = new HashSet<CRM_BUDGET>();
        }

        [Key]
        public int STAFFNO { get; set; }

        [StringLength(30)]
        public string NAME { get; set; }

        [StringLength(30)]
        public string JOBTITLE { get; set; }

        [StringLength(12)]
        public string EXTENSION { get; set; }

        [StringLength(30)]
        public string PHONE { get; set; }

        [StringLength(30)]
        public string HOMEPHONE { get; set; }

        [StringLength(1)]
        public string ISACTIVE { get; set; }

        [StringLength(30)]
        public string APP_PASSWORD { get; set; }

        public int? MENU_NO { get; set; }

        public double? AUTH_AMT { get; set; }

        public double? STOCK_AUTH_AMT { get; set; }

        public double? NON_STOCK_AUTH_AMT { get; set; }

        public int SECURITYPROFILEID { get; set; }

        public int USERPROFILEID { get; set; }

        [Required]
        [StringLength(30)]
        public string LOGINID { get; set; }

        public DateTime PASSWORD_CHANGED { get; set; }

        public DateTime? LAST_BAD_LOGIN { get; set; }

        public int BAD_LOGIN_COUNT { get; set; }

        public DateTime? LAST_LOGIN { get; set; }

        public int ACCOUNT_STATUS { get; set; }

        [StringLength(50)]
        public string EMAIL_ADDRESS { get; set; }

        public double DISCOUNTRATE { get; set; }

        [StringLength(15)]
        public string PAYROLL_ID { get; set; }

        [Required]
        [StringLength(1)]
        public string IS_SUPERVISOR { get; set; }

        [StringLength(15)]
        public string NICKNAME { get; set; }

        [Required]
        [StringLength(1)]
        public string ABSENT { get; set; }

        public int EMPLOYEE_CODE { get; set; }

        public int? SMTP_SEQNO { get; set; }

        [StringLength(1)]
        public string HAS_BUDGETS { get; set; }

        public int? REPORTS_TO_STAFFNO { get; set; }

        public string FACEBOOK_ACCESS_TOKEN { get; set; }

        public string LINKEDIN_TOKEN_KEY { get; set; }

        public string LINKEDIN_TOKEN_SECRET { get; set; }

        public string TWITTER_TOKEN_KEY { get; set; }

        public string TWITTER_TOKEN_SECRET { get; set; }

        public virtual ICollection<CRM_BUDGET_LINE> CRM_BUDGET_LINE { get; set; }

        public virtual ICollection<CRM_BUDGET> CRM_BUDGET { get; set; }
    }
}