namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class BANKBATCH_HDR
    {
        [Key]
        public int BATCHNO { get; set; }

        public int? BRANCHNO { get; set; }

        public DateTime? BATCHDATE { get; set; }

        public double? AMOUNT { get; set; }

        [StringLength(30)]
        public string NARRATIVE { get; set; }

        public int? PAYMENT_GROUP { get; set; }

        public int? PAYMENT_TYPE { get; set; }

        public int? GLCODE { get; set; }

        public int? GLSUBCODE { get; set; }

        public int? FEE_GLCODE { get; set; }

        public int? FEE_GLSUBCODE { get; set; }

        public double FEE_AMOUNT { get; set; }

        public double FEE_TAXRATE { get; set; }

        public int? FEE_TAXRATE_NO { get; set; }

        [Required]
        [StringLength(1)]
        public string GLPOSTED { get; set; }

        [StringLength(30)]
        public string TERMINAL_ID { get; set; }

        public int? SHIFTNO { get; set; }

        [Required]
        [StringLength(1)]
        public string BATCH_LOCKED { get; set; }

        [Required]
        [StringLength(1)]
        public string READY_TO_POST { get; set; }
    }
}
