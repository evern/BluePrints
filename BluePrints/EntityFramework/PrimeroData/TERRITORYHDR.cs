namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("TERRITORYHDR")]
    public partial class TERRITORYHDR
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(15)]
        public string REPORTCODE { get; set; }

        [StringLength(30)]
        public string DESCRIPTION { get; set; }

        public int? STAFFNO { get; set; }

        [StringLength(255)]
        public string NOTES { get; set; }
    }
}