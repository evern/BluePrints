namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class JOB_CONTRACT_BILLINGS
    {
        [Key]
        public int SEQNO { get; set; }

        public int? JOBNO { get; set; }

        public int? MASTER_JOBNO { get; set; }

        [Required]
        [StringLength(23)]
        public string STOCKCODE { get; set; }

        [Required]
        [StringLength(60)]
        public string DESCRIPTION { get; set; }

        public DateTime? DUE_DATE { get; set; }

        public double? SUBTOTAL { get; set; }

        public double? TAXTOTAL { get; set; }

        public int? CURRENCYNO { get; set; }

        public double EXCHRATE { get; set; }

        public double? RETENTION { get; set; }

        public double? RETENTION_RATE { get; set; }

        public double? RETENTION2 { get; set; }

        public double? RETENTION2_RATE { get; set; }

        public double? RETENTION3 { get; set; }

        public double? RETENTION3_RATE { get; set; }

        public DateTime? RETENTION_DUE_DATE { get; set; }

        [StringLength(20)]
        public string PROFORMA_NO { get; set; }

        [StringLength(4096)]
        public string NOTES { get; set; }

        public int? INV_SEQNO { get; set; }

        [StringLength(20)]
        public string INVNO { get; set; }

        public int? RETENTION_INV_SEQNO { get; set; }

        [StringLength(20)]
        public string RETENTION_INVNO { get; set; }

        public int? GLCODE { get; set; }

        public int? GLSUBCODE { get; set; }

        public double? PERCENTAGE_COMPLETE { get; set; }

        [Required]
        [StringLength(1)]
        public string ALLOW_ALLOCATION { get; set; }

        public double RETENTION_REALISED { get; set; }
    }
}
