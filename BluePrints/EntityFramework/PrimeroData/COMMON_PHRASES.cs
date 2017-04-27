namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class COMMON_PHRASES
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(200)]
        public string PHRASETEXT { get; set; }
    }
}