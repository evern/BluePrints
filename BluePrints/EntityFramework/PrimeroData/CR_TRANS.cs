namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CR_TRANS
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

        public int? SALESNO { get; set; }

        [StringLength(1)]
        public string GLPOSTED { get; set; }

        public int? GLCODE { get; set; }

        public DateTime? DUEDATE { get; set; }

        public int? BRANCH_ACCNO { get; set; }

        public int? CONTACT_SEQNO { get; set; }

        public int? CURRENCYNO { get; set; }

        public double? EXCHRATE { get; set; }

        public int? GLSUBCODE { get; set; }

        public int? BRANCHNO { get; set; }

        public int? PO_SEQNO { get; set; }

        public double? TAXRATE { get; set; }

        public int? TAXRATE_NO { get; set; }

        public double? PREV_PERIOD_OPEN { get; set; }

        public int? PAYSTATUS { get; set; }

        [StringLength(1)]
        public string AUTHORISED { get; set; }

        public int? AUTHORISEDBY { get; set; }

        public DateTime? AUTH_DATE { get; set; }

        public int UNREALISED_GAINS_GL_BATCH { get; set; }

        [StringLength(15)]
        public string TAXRETCODE { get; set; }

        public double? N_TOTVENDISC { get; set; }

        public double? RELEASEDAMT { get; set; }

        public int? PURCH_ACCNO { get; set; }

        public int? RECEIPTNO { get; set; }

        public double? N_TOTVENDISC_EXCLTAX { get; set; }

        public int? GLBATCHNO { get; set; }

        public int? TOAGEDBAL { get; set; }

        public int? NARRATIVE_SEQNO { get; set; }

        [StringLength(80)]
        public string IMAGE_URL { get; set; }

        public double MANUAL_ROUNDING { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? AMOUNT { get; set; }

        public double? PREV_PERIOD_CLOSE { get; set; }

        [MaxLength(256)]
        public byte[] TXID { get; set; }

        public int? PTNO { get; set; }

        public int? SESSION_ID { get; set; }

        public int PERIOD_SEQNO { get; set; }

        public int? AGE_STAMP { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int? AGE { get; set; }

        public int? BANKNO { get; set; }

        [StringLength(20)]
        public string PP_BATCHREF { get; set; }
    }
}