namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class DR_PRICE_POLICY
    {
        [Key]
        public int POLICY_HDR { get; set; }

        [StringLength(30)]
        public string CUSTOMER_REF { get; set; }

        [StringLength(30)]
        public string POLICY_REF { get; set; }

        public DateTime START_DATE { get; set; }

        public DateTime END_DATE { get; set; }

        [Required]
        [StringLength(1)]
        public string PRICE_MODE { get; set; }

        [Required]
        [StringLength(1)]
        public string IS_ACTIVE { get; set; }

        [StringLength(4096)]
        public string NOTES { get; set; }

        [Required]
        [StringLength(1)]
        public string FREIGHT_FREE { get; set; }

        [Required]
        [StringLength(1)]
        public string FIXED { get; set; }

        public int? CAMPAIGN_WAVE_SEQNO { get; set; }
    }
}