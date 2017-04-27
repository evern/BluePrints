namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class OPPORTUNITY_LINES
    {
        [Key]
        public int SEQNO { get; set; }

        public int HDR_SEQNO { get; set; }

        [Required]
        [StringLength(23)]
        public string STOCKCODE { get; set; }

        [Required]
        [StringLength(40)]
        public string DESCRIPTION { get; set; }

        public double? QUANTITY { get; set; }

        public double? UNITPRICE { get; set; }

        public double? DISCOUNT { get; set; }

        public int? TAXRATE_NO { get; set; }

        public double? TAXRATE { get; set; }

        public int? NARRATIVE_SEQNO { get; set; }
    }
}