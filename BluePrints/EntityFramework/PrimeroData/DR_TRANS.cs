namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class DR_TRANS
    {
        [Key]
        public int SEQNO { get; set; }

        public DateTime? POSTTIME { get; set; }

        public DateTime? TRANSDATE { get; set; }

        public int? ACCNO { get; set; }

        public int? TRANSTYPE { get; set; }

        [StringLength(20)]
        public string INVNO { get; set; }

        [StringLength(20)]
        public string REF1 { get; set; }

        [StringLength(20)]
        public string REF2 { get; set; }

        [StringLength(30)]
        public string REF3 { get; set; }

        [StringLength(70)]
        public string NAME { get; set; }

        public int? SALESNO { get; set; }

        public double? SUBTOTAL { get; set; }

        public double? TAXTOTAL { get; set; }

        [StringLength(1)]
        public string TAXINC { get; set; }

        [StringLength(12)]
        public string ANALYSIS { get; set; }

        public double? ALLOCATEDBAL { get; set; }

        [StringLength(1)]
        public string ALLOCATED { get; set; }

        public int? ALLOCAGE { get; set; }

        [StringLength(1)]
        public string GLPOSTED { get; set; }

        public int? GLCODE { get; set; }

        public DateTime? DUEDATE { get; set; }

        public int? BRANCH_ACCNO { get; set; }

        [StringLength(30)]
        public string DELIVADDR1 { get; set; }

        [StringLength(30)]
        public string DELIVADDR2 { get; set; }

        [StringLength(30)]
        public string DELIVADDR3 { get; set; }

        [StringLength(30)]
        public string DELIVADDR4 { get; set; }

        public int? CONTACT_SEQNO { get; set; }

        public int? CURRENCYNO { get; set; }

        public double? EXCHRATE { get; set; }

        public int? BATCHNO { get; set; }

        public int? SHIFTNO { get; set; }

        public int? GLSUBCODE { get; set; }

        public int? BRANCHNO { get; set; }

        [StringLength(20)]
        public string ORD_REF { get; set; }

        [StringLength(70)]
        public string DISPATCH_INFO { get; set; }

        public int? SO_SEQNO { get; set; }

        public double? TAXRATE { get; set; }

        public int? TAXRATE_NO { get; set; }

        [StringLength(30)]
        public string DELIVADDR5 { get; set; }

        [StringLength(30)]
        public string DELIVADDR6 { get; set; }

        public double? PREV_PERIOD_OPEN { get; set; }

        [StringLength(30)]
        public string TERMINAL_ID { get; set; }

        public int? DEPOSIT_STATUS { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? AMOUNT { get; set; }

        public int UNREALISED_GAINS_GL_BATCH { get; set; }

        [StringLength(15)]
        public string TAXRETCODE { get; set; }

        public int? WEEK_NO { get; set; }

        public int? DDNO { get; set; }

        public double RELEASEDAMT { get; set; }

        public int? SALES_ACCNO { get; set; }

        [Required]
        [StringLength(1)]
        public string FREIGHT_FREE { get; set; }

        public int CONTRACT_HDR { get; set; }

        [StringLength(20)]
        public string BANKACCNO { get; set; }

        [StringLength(20)]
        public string BANKACCNAME { get; set; }

        public int? GLBATCHNO { get; set; }

        public int? NARRATIVE_SEQNO { get; set; }

        public int? TOAGEDBAL { get; set; }

        [StringLength(15)]
        public string EFTCAID { get; set; }

        public int? EFTSTAN { get; set; }

        public int? PAY_STATUS { get; set; }

        public int? PHYS_BRANCH { get; set; }

        public int? PHYS_STAFF { get; set; }

        public int? SESSION_ID { get; set; }

        public double? PREV_PERIOD_CLOSE { get; set; }

        [StringLength(30)]
        public string EFTAUTH { get; set; }

        public int? GATEWAYNO { get; set; }

        public int SOURCEINV_SEQNO { get; set; }

        [MaxLength(256)]
        public byte[] TXID { get; set; }

        public int? PTNO { get; set; }

        [StringLength(20)]
        public string CUSTORDERNO { get; set; }

        public int PERIOD_SEQNO { get; set; }

        public int? AGE_STAMP { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int? AGE { get; set; }

        public double TAXROUNDING { get; set; }

        public int? CAMPAIGN_WAVE_SEQNO { get; set; }

        public int JOBNO { get; set; }

        public int? OPPORTUNITY_SEQNO { get; set; }
    }
}