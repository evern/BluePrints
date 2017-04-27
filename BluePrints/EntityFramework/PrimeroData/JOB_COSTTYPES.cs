namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class JOB_COSTTYPES
    {
        [Key]
        public int SEQNO { get; set; }

        public double? DEF_MARKUP { get; set; }

        public double? DEF_OVERHEAD { get; set; }

        [StringLength(50)]
        public string COSTDESC { get; set; }

        public int? GLCODE { get; set; }

        public int? GLSUBCODE { get; set; }

        [StringLength(1)]
        public string SHOWONQUOTE { get; set; }

        [Required]
        [StringLength(3)]
        public string SHORTCODE { get; set; }

        public int? DEF_COSTGROUP { get; set; }

        public int? DEF_PURCH_GLCODE { get; set; }

        public int? DEF_PURCH_GLSUBCODE { get; set; }

        [StringLength(1)]
        public string CONSOLIDATE { get; set; }

        [StringLength(1)]
        public string COPY_FROM_QUOTE { get; set; }
    }
}