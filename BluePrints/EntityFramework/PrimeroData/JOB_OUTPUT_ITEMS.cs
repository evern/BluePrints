namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class JOB_OUTPUT_ITEMS
    {
        [Key]
        public int SEQNO { get; set; }

        public int? JOBNO { get; set; }

        [Required]
        [StringLength(23)]
        public string STOCKCODE { get; set; }

        [Required]
        [StringLength(60)]
        public string DESCRIPTION { get; set; }

        public int? LOCATION { get; set; }

        public double? QUANTITY { get; set; }
    }
}