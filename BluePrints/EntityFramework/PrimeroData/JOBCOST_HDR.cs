namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class JOBCOST_HDR
    {
        [Key]
        public int JOBNO { get; set; }

        public DateTime? QUOTEDATE { get; set; }

        public DateTime? STARTDATE { get; set; }

        public DateTime? DUEDATE { get; set; }

        public DateTime? COMPLETED { get; set; }

        public double? ESTIMATE { get; set; }

        public double? INVOICED { get; set; }

        public double? THETIME { get; set; }

        public double? MATERIALS { get; set; }

        public double? DEF_OVERHEAD { get; set; }

        public double? MATERIALSCOST { get; set; }

        public double? ESTIMATECOST { get; set; }

        public double? THETIMECOST { get; set; }

        public double? INVOICEDCOST { get; set; }

        [StringLength(15)]
        public string JOBCODE { get; set; }

        public int? ACCNO { get; set; }

        [StringLength(15)]
        public string CUSTORDNO { get; set; }

        [StringLength(1)]
        public string STATUS { get; set; }

        [StringLength(60)]
        public string TITLE { get; set; }

        public int? CATEGORY { get; set; }

        public int? JOBTYPE { get; set; }

        public int? STAFFNO { get; set; }

        public int? ACTIONBY { get; set; }

        public int MASTER_JOBNO { get; set; }

        public int? COSTGL { get; set; }

        public int? SALESGL { get; set; }

        [StringLength(50)]
        public string SERIALNO { get; set; }

        [StringLength(50)]
        public string CONTACT { get; set; }

        [StringLength(1000)]
        public string PRIVATE_NOTE { get; set; }

        public int? COSTSUBGL { get; set; }

        public int? SALESSUBGL { get; set; }

        public int? DEL_ADDR { get; set; }

        public int? CONTACTNO { get; set; }

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

        public double? WRITE_OFF_COST { get; set; }

        public double? TOTAL_HOURS { get; set; }

        public double? EST_HOURS { get; set; }

        public double? ASSET_COST { get; set; }

        public double? ASSET_VALUE { get; set; }

        public int? BRANCHNO { get; set; }

        [Required]
        [StringLength(1)]
        public string ISACTIVE { get; set; }

        [Required]
        [StringLength(1)]
        public string HASUNBILLED { get; set; }

        [Required]
        [StringLength(1)]
        public string INVOICEREADY { get; set; }

        public DateTime? CALLBACKDATE { get; set; }

        public DateTime? ENTRYDATE { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? TOTALVALUE { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? TOTALCOST { get; set; }

        public int? WIPLOC { get; set; }

        public double EXCHRATE { get; set; }

        public double RETENTION_RATE { get; set; }

        public double RETENTION2_RATE { get; set; }

        public double RETENTION2_MIN { get; set; }

        public double RETENTION3_RATE { get; set; }

        public double RETENTION3_MIN { get; set; }

        public double ALLOWANCE { get; set; }

        public int? BILLINGMODE { get; set; }

        public int? PROJ_SEQNO { get; set; }

        [StringLength(5000)]
        public string DESCRIPTION { get; set; }

        public int? CAMPAIGN_WAVE_SEQNO { get; set; }

        public int? OPPORTUNITY_SEQNO { get; set; }
    }
}