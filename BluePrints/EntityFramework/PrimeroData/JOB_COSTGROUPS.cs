namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class JOB_COSTGROUPS
    {
        [Key]
        public int SEQNO { get; set; }

        public double? DEF_MARKUP { get; set; }

        public double? DEF_OVERHEAD { get; set; }

        [StringLength(50)]
        public string COSTDESC { get; set; }

        [StringLength(4)]
        public string SHORTCODE { get; set; }

        [StringLength(1)]
        public string SHOWONQUOTE { get; set; }

        [StringLength(1)]
        public string CONSOLIDATE { get; set; }

        [StringLength(1)]
        public string COPY_FROM_QUOTE { get; set; }
    }
}