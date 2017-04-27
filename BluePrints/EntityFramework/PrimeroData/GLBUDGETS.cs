namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class GLBUDGETS
    {
        [Key]
        public int SEQNO { get; set; }

        public int? ACCNO { get; set; }

        [StringLength(8)]
        public string BUDGETCODE { get; set; }

        public double? BUDGETVAL { get; set; }

        public int? PERIODNO { get; set; }

        public int? BRANCHNO { get; set; }

        public int? SUBACCNO { get; set; }

        public int? HDR_SEQNO { get; set; }
    }
}