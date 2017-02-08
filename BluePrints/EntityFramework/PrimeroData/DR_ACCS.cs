namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class DR_ACCS
    {
        public DR_ACCS()
        {
            CRM_BUDGET_LINE = new HashSet<CRM_BUDGET_LINE>();
            CRM_BUDGET = new HashSet<CRM_BUDGET>();
        }

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

        [StringLength(30)]
        public string PHONE { get; set; }

        [StringLength(30)]
        public string FAX { get; set; }

        [StringLength(60)]
        public string EMAIL { get; set; }

        public double? CREDLIMIT { get; set; }

        public int? ACCGROUP { get; set; }

        public int? SALESNO { get; set; }

        public double? LASTMONTH { get; set; }

        public double? LASTYEAR { get; set; }

        public double? AGEDBAL0 { get; set; }

        public double? AGEDBAL1 { get; set; }

        public double? AGEDBAL2 { get; set; }

        public double? AGEDBAL3 { get; set; }

        public int? CREDITSTATUS { get; set; }

        public int? DISCOUNTLEVEL { get; set; }

        [StringLength(1)]
        public string OPENITEM { get; set; }

        public int? INVOICETYPE { get; set; }

        [StringLength(4096)]
        public string NOTES { get; set; }

        public double? MONTHVAL { get; set; }

        public double? YEARVAL { get; set; }

        public DateTime? STARTDATE { get; set; }

        [StringLength(12)]
        public string SORTCODE { get; set; }

        [StringLength(20)]
        public string BANK { get; set; }

        [StringLength(40)]
        public string BANK_ACCOUNT { get; set; }

        [StringLength(40)]
        public string BANK_ACC_NAME { get; set; }

        [StringLength(40)]
        public string BSBNO { get; set; }

        [StringLength(1)]
        public string D_DEBIT_FAX { get; set; }

        [StringLength(1)]
        public string D_DEBIT_PRINT { get; set; }

        [StringLength(1)]
        public string D_DEBIT_EMAIL { get; set; }

        public int? PAY_TYPE { get; set; }

        [StringLength(30)]
        public string BRANCH { get; set; }

        [StringLength(30)]
        public string DRAWER { get; set; }

        public int? TAXSTATUS { get; set; }

        public int? PRICENO { get; set; }

        [StringLength(23)]
        public string AUTOBILLCODE { get; set; }

        [StringLength(15)]
        public string ALPHACODE { get; set; }

        public int HEAD_ACCNO { get; set; }

        [StringLength(30)]
        public string PASS_WORD { get; set; }

        public int CURRENCYNO { get; set; }

        [StringLength(60)]
        public string ALERT { get; set; }

        [StringLength(1)]
        public string STATEMENT { get; set; }

        public int? INVFILENO { get; set; }

        public double? PROMPTPAY_PC { get; set; }

        public double? PROMPTPAY_AMT { get; set; }

        [StringLength(1)]
        public string ISACTIVE { get; set; }

        [StringLength(1)]
        public string BAD_CHEQUE { get; set; }

        public int? BRANCHNO { get; set; }

        public DateTime? LAST_UPDATED { get; set; }

        [StringLength(30)]
        public string TAXREG { get; set; }

        [StringLength(1)]
        public string STOPCREDIT { get; set; }

        [StringLength(12)]
        public string POST_CODE { get; set; }

        public int? GLCONTROLACC { get; set; }

        public int? GLCONTROLSUBACC { get; set; }

        public double? PRIOR_AGEDBAL0 { get; set; }

        public double? PRIOR_AGEDBAL1 { get; set; }

        public double? PRIOR_AGEDBAL2 { get; set; }

        public double? PRIOR_AGEDBAL3 { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? BALANCE { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? PRIOR_BALANCE { get; set; }

        public int? ACCGROUP2 { get; set; }

        [Required]
        [StringLength(1)]
        public string FREIGHT_FREE { get; set; }

        public int? COURIER_DEPOT_SEQNO { get; set; }

        [Required]
        [StringLength(1)]
        public string KEEPTRANSACTIONS { get; set; }

        [Required]
        [StringLength(1)]
        public string NEED_ORDERNO { get; set; }

        public int? PRICEGROUP { get; set; }

        [Required]
        [StringLength(1)]
        public string ALLOW_RESTRICTED_STOCK { get; set; }

        [Required]
        [StringLength(1)]
        public string PRIVATE_ACC { get; set; }

        [Required]
        [StringLength(1)]
        public string ISTEMPLATE { get; set; }

        [StringLength(50)]
        public string WEBSITE { get; set; }

        public int AVE_DAYS_TO_PAY { get; set; }

        [StringLength(20)]
        public string INVOICE_TYPE { get; set; }

        public int? STATEMENT_CONTACT_SEQNO { get; set; }

        [StringLength(20)]
        public string LINKEDIN { get; set; }

        [StringLength(500)]
        public string TWITTER { get; set; }

        [StringLength(500)]
        public string FACEBOOK { get; set; }

        [StringLength(30)]
        public string X_PHONE2 { get; set; }

        [StringLength(50)]
        public string X_INDUSTRY { get; set; }

        public virtual ICollection<CRM_BUDGET_LINE> CRM_BUDGET_LINE { get; set; }

        public virtual ICollection<CRM_BUDGET> CRM_BUDGET { get; set; }
    }
}