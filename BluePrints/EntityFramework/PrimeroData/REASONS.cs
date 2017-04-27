namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class REASONS
    {
        [Key]
        public int SEQNO { get; set; }

        public int CLASSNO { get; set; }

        [StringLength(40)]
        public string REASONNAME { get; set; }

        [Required]
        [StringLength(200)]
        public string DESCRIPTION { get; set; }

        [Required]
        [StringLength(5)]
        public string REPORTCODE { get; set; }

        public int MUSTFILL_REF1 { get; set; }
    }
}