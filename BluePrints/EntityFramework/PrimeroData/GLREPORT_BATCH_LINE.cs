namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class GLREPORT_BATCH_LINE
    {
        [Key]
        public int SEQNO { get; set; }

        public int BATCHNO { get; set; }

        public int LISTSEQ { get; set; }

        public int REPORT_SEQNO { get; set; }

        public int AGE { get; set; }

        [StringLength(8)]
        public string BUDGETCODE { get; set; }

        [StringLength(100)]
        public string BRANCHNOS { get; set; }

        [Required]
        [StringLength(1)]
        public string SUPPRESSZEROS { get; set; }

        [Required]
        [StringLength(1)]
        public string LISTSUBACCOUNTS { get; set; }

        [Required]
        [StringLength(1)]
        public string USECLARITY { get; set; }

        [Required]
        [StringLength(1)]
        public string SAVETOFILE { get; set; }

        [StringLength(50)]
        public string PRINTERNAME { get; set; }

        public int COPIES { get; set; }
    }
}