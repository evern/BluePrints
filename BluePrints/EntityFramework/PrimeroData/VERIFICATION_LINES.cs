namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class VERIFICATION_LINES
    {
        [Key]
        public int SEQNO { get; set; }

        public int HDR_SEQNO { get; set; }

        [StringLength(1000)]
        public string MESSAGE { get; set; }

        [StringLength(5)]
        public string V_CLASS { get; set; }

        [Required]
        [StringLength(50)]
        public string SOURCE_SEQ { get; set; }

        [StringLength(5)]
        public string LINETYPE { get; set; }
    }
}