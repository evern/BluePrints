namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PURCHORD_HDR
    {
        [Key]
        public int SEQNO { get; set; }

        public int? STATUS { get; set; }

        public int? ACCNO { get; set; }

        public DateTime? ORDERDATE { get; set; }

        public DateTime? DUEDATE { get; set; }

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

        [StringLength(1)]
        public string TAXINC { get; set; }

        public DateTime? LAST_UPDATED { get; set; }

        public int? BRANCHNO { get; set; }

        public int? AUTH_STAFFNO { get; set; }

        public DateTime? AUTH_DATE { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? ORDTOTAL { get; set; }

        [StringLength(1)]
        public string ISCONFIRMED { get; set; }

        public int? LEADTIMEUSED { get; set; }

        [StringLength(30)]
        public string ADDRESS5 { get; set; }

        [StringLength(30)]
        public string ADDRESS6 { get; set; }

        public int? NARRATIVE_SEQNO { get; set; }

        public int? DEFLOCNO { get; set; }

        public int? PROCESSFINALISATION { get; set; }

        [MaxLength(256)]
        public byte[] TXID { get; set; }

        [StringLength(20)]
        public string SO_SOURCE_REF { get; set; }

        public int? HSTATUS { get; set; }

        public DateTime? CREATE_DATE { get; set; }

        public DateTime? ACTIVATION_DATE { get; set; }

        public DateTime? FINALISATION_DATE { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? LOCALVALUE { get; set; }

        public int? CAMPAIGN_WAVE_SEQNO { get; set; }

        [StringLength(100)]
        public string X_CONTACT { get; set; }

        public int? X_AUTH_REQ_USER { get; set; }

        public int? X_REQUISITION_BY { get; set; }
    }
}