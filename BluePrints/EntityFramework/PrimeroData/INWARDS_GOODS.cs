namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class INWARDS_GOODS
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(70)]
        public string SUPPLIERNAME { get; set; }

        public int LOCNO { get; set; }

        [StringLength(50)]
        public string SUPPLIERREF { get; set; }

        [Required]
        [StringLength(1)]
        public string COMPLETE { get; set; }

        [StringLength(500)]
        public string NOTES { get; set; }

        public int NUM_CARTONS { get; set; }

        [Required]
        [StringLength(1)]
        public string INVOICED { get; set; }

        public int GLBATCHNO { get; set; }

        public int SHIPMENTNO { get; set; }

        public DateTime? TRANSDATE { get; set; }

        public int SUPPLIERNO { get; set; }

        [StringLength(50)]
        public string PACKSLIP { get; set; }

        public int? REVERSALSTATUS { get; set; }

        public int? RELATED_SEQNO { get; set; }

        public int? COSTED_PERIOD_SEQNO { get; set; }

        public int? SESSION_ID { get; set; }

        public int? LASTINVSEQNO { get; set; }
    }
}
