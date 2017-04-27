namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class DR_PRICES
    {
        [Key]
        public int SEQNO { get; set; }

        public int? ACCNO { get; set; }

        [StringLength(23)]
        public string STOCKCODE { get; set; }

        public double? PRICE { get; set; }

        public DateTime? STARTDATE { get; set; }

        public DateTime? STOPDATE { get; set; }

        public double? MINQTY { get; set; }

        public int? ACCGROUP { get; set; }

        public int? STOCKPRICEGROUP { get; set; }

        public double? DISCOUNT { get; set; }

        [Required]
        [StringLength(1)]
        public string FREIGHT_FREE { get; set; }

        public int? POLICY_HDR { get; set; }

        public int SELL_PRICE_BANDNO { get; set; }

        public int? MASTER_JOBNO { get; set; }

        public int? JOBNO { get; set; }

        public int? CAMPAIGN_WAVE_SEQNO { get; set; }
    }
}