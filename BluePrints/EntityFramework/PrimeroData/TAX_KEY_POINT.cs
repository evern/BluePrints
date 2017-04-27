namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class TAX_KEY_POINT
    {
        [Key]
        public int SEQNO { get; set; }

        [Required]
        [StringLength(3)]
        public string COUNTRY { get; set; }

        [Required]
        [StringLength(5)]
        public string KEY_POINT { get; set; }

        public int LISTSEQ { get; set; }

        [Required]
        [StringLength(1)]
        public string DR_LEDGER { get; set; }

        [Required]
        [StringLength(1)]
        public string CR_LEDGER { get; set; }

        [StringLength(255)]
        public string KP_DESCRIPTION { get; set; }
    }
}