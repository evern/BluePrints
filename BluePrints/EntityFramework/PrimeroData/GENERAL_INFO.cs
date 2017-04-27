namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class GENERAL_INFO
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SEQNO { get; set; }

        [StringLength(40)]
        public string USERNAME { get; set; }

        [StringLength(40)]
        public string ADDRESS1 { get; set; }

        [StringLength(40)]
        public string ADDRESS2 { get; set; }

        [StringLength(40)]
        public string ADDRESS3 { get; set; }

        [StringLength(40)]
        public string PHONE { get; set; }

        [StringLength(40)]
        public string FAX { get; set; }

        [StringLength(60)]
        public string EMAIL { get; set; }

        public int? SECURITYCODE { get; set; }

        public int? SOFTWARE_VERSION { get; set; }

        [StringLength(30)]
        public string TAXREGNO { get; set; }

        [StringLength(30)]
        public string GL_PASSWORD { get; set; }

        [StringLength(20)]
        public string SUB1_NAME { get; set; }

        [StringLength(1)]
        public string SUB1_DEF { get; set; }

        [StringLength(20)]
        public string SUB2_NAME { get; set; }

        [StringLength(1)]
        public string SUB2_DEF { get; set; }

        [StringLength(20)]
        public string SUB3_NAME { get; set; }

        [StringLength(1)]
        public string SUB3_DEF { get; set; }

        [StringLength(20)]
        public string SUB4_NAME { get; set; }

        [StringLength(1)]
        public string SUB4_DEF { get; set; }

        [StringLength(30)]
        public string DELIVADDR1 { get; set; }

        [StringLength(30)]
        public string DELIVADDR2 { get; set; }

        [StringLength(30)]
        public string DELIVADDR3 { get; set; }

        [StringLength(30)]
        public string DELIVADDR4 { get; set; }

        [StringLength(1)]
        public string JOBCODE_ON_TRANS { get; set; }

        [StringLength(1)]
        public string STOCKDISCTABLE { get; set; }

        [StringLength(1)]
        public string STOCK_WARNING { get; set; }

        public int? MAXSTOCKLOC { get; set; }

        [StringLength(1)]
        public string USE_WORKSORDS { get; set; }

        public int? NO_SELLPRICES { get; set; }

        [StringLength(1)]
        public string CUSTOMCONFIG_CACHE { get; set; }

        public int? REGISTERED_USERS { get; set; }

        [StringLength(40)]
        public string BANK_ACC_NAME { get; set; }

        [StringLength(40)]
        public string BANK_ACC_NO { get; set; }

        [StringLength(3)]
        public string COUNTRY { get; set; }

        public int? TERMINAL_ID_GEN { get; set; }

        public int? ANALYSISCODES { get; set; }

        [StringLength(1)]
        public string VALIDATE_ANALYSISCODES { get; set; }

        public DateTime? EXPDATE { get; set; }

        [Required]
        [StringLength(1)]
        public string SQL_COMPAT { get; set; }

        [StringLength(1)]
        public string OPENSYS { get; set; }

        public int COMPANYPROFILEID { get; set; }

        [StringLength(30)]
        public string CURR_YEAR_LABEL { get; set; }

        [StringLength(30)]
        public string LAST_YEAR_LABEL { get; set; }

        [StringLength(30)]
        public string NEXT_YEAR_LABEL { get; set; }

        [StringLength(250)]
        public string CHQ_FILE_FORMAT { get; set; }

        public DateTime? STOCKCAL_LASTRUN { get; set; }

        public int? COMPANYNO { get; set; }

        public int? FIRST_PERIOD_OF_YEAR { get; set; }

        public int? AUDITORVERSION { get; set; }

        [Required]
        [StringLength(1)]
        public string RECAL_STOCK_TRANS { get; set; }

        [StringLength(1)]
        public string EXOCFG_VERIFY { get; set; }

        public int NARRATIVE_SEQNO { get; set; }

        [StringLength(1)]
        public string DBUPDATE_GLMOVEMENTS { get; set; }

        [StringLength(255)]
        public string MERCHANTPWD { get; set; }

        [StringLength(255)]
        public string ENCRYPTKEY { get; set; }

        [StringLength(255)]
        public string DECRYPTKEY { get; set; }

        [StringLength(100)]
        public string WEBSITE { get; set; }

        [StringLength(60)]
        public string SYSADMIN_NAME { get; set; }

        [StringLength(40)]
        public string SYSADMIN_PHONE { get; set; }

        [StringLength(60)]
        public string SYSADMIN_EMAIL { get; set; }

        [StringLength(60)]
        public string PARTNER_NAME { get; set; }

        [StringLength(60)]
        public string PARTNER_COMPANYNAME { get; set; }

        [StringLength(40)]
        public string PARTNER_PHONE { get; set; }

        [StringLength(60)]
        public string PARTNER_EMAIL { get; set; }

        public int BPAY_BILLER { get; set; }

        [StringLength(20)]
        public string MPOWERED_PAYEE { get; set; }

        [StringLength(40)]
        public string PARENTUSERNAME { get; set; }

        public int BUSINESS_START_PERIODNO { get; set; }

        [Required]
        [StringLength(1)]
        public string POSTAL_AS_DELIVARY { get; set; }

        public DateTime? AVE_DAYS_SP_LASTRUN { get; set; }

        [Required]
        [StringLength(1)]
        public string MPOWERED_SERVICES { get; set; }

        public double? MPOWERED_MAXPAYMENTS { get; set; }

        [Required]
        [StringLength(1)]
        public string MPOWERED_SIGNUP_PROMPT { get; set; }

        [StringLength(40)]
        public string MSC_DATABASE_ID { get; set; }

        [StringLength(40)]
        public string MSC_DATABASE_NAME { get; set; }

        [Required]
        [StringLength(1)]
        public string MPOWERED_PAYMENTS { get; set; }

        [StringLength(255)]
        public string FACEBOOK_APP_KEY { get; set; }

        [StringLength(255)]
        public string FACEBOOK_APP_SECRET { get; set; }

        [StringLength(255)]
        public string LINKEDIN_APP_KEY { get; set; }

        [StringLength(255)]
        public string LINKEDIN_APP_SECRET { get; set; }

        [StringLength(255)]
        public string TWITTER_APP_KEY { get; set; }

        [StringLength(255)]
        public string TWITTER_APP_SECRET { get; set; }

        public string FACEBOOK_ACCESS_TOKEN { get; set; }

        public string LINKEDIN_TOKEN_KEY { get; set; }

        public string LINKEDIN_TOKEN_SECRET { get; set; }

        public string TWITTER_TOKEN_KEY { get; set; }

        public string TWITTER_TOKEN_SECRET { get; set; }

        [StringLength(500)]
        public string FACEBOOK_USER_ID { get; set; }

        [StringLength(500)]
        public string FACEBOOK_PAGE_ID { get; set; }

        public int COMPANY_FACEBOOK_ACCOUNT_TYPE { get; set; }

        public int FACEBOOK_POST_ACCOUNT_TYPE { get; set; }

        [StringLength(20)]
        public string LINKEDIN_USER_ID { get; set; }

        [StringLength(20)]
        public string LINKEDIN_COMPANY_ID { get; set; }

        public int COMPANY_LINKEDIN_ACCOUNT_TYPE { get; set; }

        [StringLength(500)]
        public string TWITTER_USER_ID { get; set; }

        [StringLength(60)]
        public string DIAGNOSTICS_EMAIL { get; set; }

        [StringLength(60)]
        public string DETAILS_UPDATE_EMAIL { get; set; }

        public DateTime? CRM_SALES_BUDGETS_RECALC_LASTRUN { get; set; }

        public int STARTOFWEEKDAY { get; set; }

        [Required]
        [StringLength(1)]
        public string UPDATED_CURRENCY_RATECHANGES { get; set; }
    }
}