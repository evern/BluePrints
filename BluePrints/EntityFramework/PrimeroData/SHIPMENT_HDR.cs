namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class SHIPMENT_HDR
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(50)]
        public string INT_SHIPREF { get; set; }

        [StringLength(50)]
        public string EXT_SHIPREF { get; set; }

        [StringLength(50)]
        public string WEIGHT { get; set; }

        [StringLength(50)]
        public string VESSEL { get; set; }

        [StringLength(50)]
        public string CARRIER { get; set; }

        public int? CARRIER_NO { get; set; }

        public int? SHIP_METHOD { get; set; }

        [StringLength(500)]
        public string NOTES { get; set; }

        public int? STATUS { get; set; }

        public DateTime? ETA_DATE { get; set; }

        public DateTime? DEP_DATE { get; set; }

        public int? SESSION_ID { get; set; }
    }
}