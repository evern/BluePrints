namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class NARRATIVES
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(4096)]
        public string NARRATIVE { get; set; }
    }
}