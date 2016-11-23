namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SALESORD_HDR_ARCHIVE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SEQNO { get; set; }

        public int? STATUS { get; set; }

        public int? ACCNO { get; set; }

        public DateTime? ORDERDATE { get; set; }

        public DateTime? DUEDATE { get; set; }

        [StringLength(20)]
        public string CUSTORDERNO { get; set; }

        [StringLength(20)]
        public string REFERENCE { get; set; }

        [StringLength(30)]
        public string ADDRESS1 { get; set; }

        [StringLength(30)]
        public string ADDRESS2 { get; set; }

        [StringLength(30)]
        public string ADDRESS3 { get; set; }

        [StringLength(30)]
        public string ADDRESS4 { get; set; }

        [StringLength(255)]
        public string INSTRUCTIONS { get; set; }

        public double? SUBTOTAL { get; set; }

        public double? TAXTOTAL { get; set; }

        public int? SALESNO { get; set; }

        public int? CONTACT_SEQNO { get; set; }

        public int? CURRENCYNO { get; set; }

        public double? EXCHRATE { get; set; }

        public int? CONSIGNTOLOC { get; set; }

        public int? BRANCHNO { get; set; }

        [StringLength(1)]
        public string TAXINC { get; set; }

        [StringLength(1)]
        public string BACKORDER { get; set; }

        public int? MANIFEST { get; set; }

        [StringLength(70)]
        public string DISPATCH_INFO { get; set; }

        public int? HSTATUS { get; set; }

        public DateTime? LAST_UPDATED { get; set; }

        [StringLength(30)]
        public string ADDRESS5 { get; set; }

        [StringLength(30)]
        public string ADDRESS6 { get; set; }

        public int? PAYMENT_STATUS { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? ORDTOTAL { get; set; }

        public int? DELIVERYCOUNT { get; set; }

        public int? INVOICECOUNT { get; set; }

        public int? NARRATIVE_SEQNO { get; set; }

        [StringLength(1)]
        public string HAS_UNRELEASED { get; set; }

        [StringLength(1)]
        public string HAS_BACKORDERS { get; set; }

        [StringLength(1)]
        public string HAS_UNSUPPLIED { get; set; }

        [StringLength(1)]
        public string HAS_UNINVOICED { get; set; }

        [StringLength(1)]
        public string HAS_UNPICKED { get; set; }

        public int? PICKEDCOUNT { get; set; }

        public int? RELEASECOUNT { get; set; }

        public int? ORDSTATUS { get; set; }

        public int? DEFLOCNO { get; set; }

        public int? PROCESSFINALISATION { get; set; }

        [MaxLength(256)]
        public byte[] TXID { get; set; }

        [StringLength(1)]
        public string ONHOLD { get; set; }

        [Required]
        [StringLength(1)]
        public string SHIP_COMPLETE { get; set; }

        public double TAXROUNDING { get; set; }
    }
}
