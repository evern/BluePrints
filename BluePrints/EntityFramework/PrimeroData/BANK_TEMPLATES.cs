namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class BANK_TEMPLATES
    {
        [Key]
        public int SEQNO { get; set; }

        [Required]
        [StringLength(20)]
        public string BANKNAME { get; set; }

        [Column(TypeName = "text")]
        [Required]
        public string BANKFILE { get; set; }
    }
}