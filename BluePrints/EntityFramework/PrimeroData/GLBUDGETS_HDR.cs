namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class GLBUDGETS_HDR
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(8)]
        public string BUDGETCODE { get; set; }
    }
}