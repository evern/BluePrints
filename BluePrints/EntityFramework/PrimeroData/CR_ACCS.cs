namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CR_ACCS
    {
        [Key]
        public int ACCNO { get; set; }

        [StringLength(60)]
        public string NAME { get; set; }

        [StringLength(30)]
        public string ADDRESS1 { get; set; }

        [StringLength(30)]
        public string ADDRESS2 { get; set; }

        [StringLength(30)]
        public string ADDRESS3 { get; set; }

        [StringLength(30)]
        public string ADDRESS4 { get; set; }

        [StringLength(30)]
        public string ADDRESS5 { get; set; }

        [StringLength(30)]
        public string PHONE { get; set; }

        [StringLength(30)]
        public string FAX { get; set; }

        [StringLength(60)]
        public string EMAIL { get; set; }

        public double? CREDLIMIT { get; set; }

        public int? ACCGROUP { get; set; }

        public double? LASTMONTH { get; set; }

        public double? LASTYEAR { get; set; }

        public double? AGEDBAL0 { get; set; }

        public double? AGEDBAL1 { get; set; }

        public double? AGEDBAL2 { get; set; }

        public double? AGEDBAL3 { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? BALANCE { get; set; }

        public int? CREDITSTATUS { get; set; }

        [StringLength(1)]
        public string OPENITEM { get; set; }

        [StringLength(30)]
        public string DELADDR1 { get; set; }

        [StringLength(30)]
        public string DELADDR2 { get; set; }

        [StringLength(30)]
        public string DELADDR3 { get; set; }

        [StringLength(30)]
        public string DELADDR4 { get; set; }

        [StringLength(30)]
        public string DELADDR5 { get; set; }

        [StringLength(30)]
        public string DELADDR6 { get; set; }

        public int? SALESNO { get; set; }

        public int? DISCOUNTLEVEL { get; set; }

        public int? INVOICETYPE { get; set; }

        public double? AUTOFREIGHT { get; set; }

        [StringLength(4096)]
        public string NOTES { get; set; }

        public double? MONTHVAL { get; set; }

        public double? YEARVAL { get; set; }

        [StringLength(15)]
        public string ALPHACODE { get; set; }

        public int? TAXSTATUS { get; set; }

        public int HEAD_ACCNO { get; set; }

        public int CURRENCYNO { get; set; }

        [StringLength(60)]
        public string ALERT { get; set; }

        [StringLength(1)]
        public string ISACTIVE { get; set; }

        [StringLength(40)]
        public string BANK_ACCOUNT { get; set; }

        [StringLength(15)]
        public string DEFAULT_CODE { get; set; }

        [StringLength(40)]
        public string BANK_ACC_NAME { get; set; }

        public DateTime? LAST_UPDATED { get; set; }

        public int? LEADTIME { get; set; }

        [StringLength(30)]
        public string TAXREG { get; set; }

        [StringLength(12)]
        public string POST_CODE { get; set; }

        public double? N_CR_DISC { get; set; }

        public int? GLCONTROLACC { get; set; }

        public int? GLCONTROLSUBACC { get; set; }

        public int? BRANCHNO { get; set; }

        public double? PRIOR_AGEDBAL0 { get; set; }

        public double? PRIOR_AGEDBAL1 { get; set; }

        public double? PRIOR_AGEDBAL2 { get; set; }

        public double? PRIOR_AGEDBAL3 { get; set; }

        public double? PROMPT_PAY_DISC { get; set; }

        [StringLength(40)]
        public string BSBNO { get; set; }

        public double? AUTO_AUTH_AMT { get; set; }

        public int? PAY_TYPE { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? PRIOR_BALANCE { get; set; }

        public int? ACCGROUP2 { get; set; }

        public int? LEADTIME2 { get; set; }

        public double? N_LAND_COST_PROVN { get; set; }

        [StringLength(1)]
        public string PP_TOPAY { get; set; }

        [StringLength(1)]
        public string STOPCREDIT { get; set; }

        public int DEF_INVMODE { get; set; }

        [Required]
        [StringLength(1)]
        public string PRIVATE_ACC { get; set; }

        [StringLength(30)]
        public string WEBSITE { get; set; }

        public int AVE_DAYS_TO_PAY { get; set; }

        [StringLength(256)]
        public string STATEMENT_TEXT { get; set; }

        [StringLength(20)]
        public string REMITTANCE_METHOD { get; set; }

        [Required]
        [StringLength(1)]
        public string SEND_PAYMENT_REMITTANCE { get; set; }

        public int? STATEMENT_CONTACT_SEQNO { get; set; }

        [StringLength(20)]
        public string LINKEDIN { get; set; }

        [StringLength(500)]
        public string TWITTER { get; set; }

        [StringLength(500)]
        public string FACEBOOK { get; set; }

        [StringLength(30)]
        public string X_PHONE2 { get; set; }
    }
}