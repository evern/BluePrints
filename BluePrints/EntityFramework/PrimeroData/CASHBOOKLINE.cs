namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("CASHBOOKLINE")]
    public partial class CASHBOOKLINE
    {
        [Key]
        public int SEQNO { get; set; }

        public int HDR_SEQNO { get; set; }

        public int LINENUM { get; set; }

        [StringLength(1)]
        public string LEDGER { get; set; }

        public int? BRANCHNO { get; set; }

        public int? ACCNO { get; set; }

        public int? SUBACCNO { get; set; }

        [StringLength(70)]
        public string NAME_DETAILS { get; set; }

        [StringLength(30)]
        public string CHQ_REF { get; set; }

        public DateTime? TRANSDATE { get; set; }

        public double? EXPENSE { get; set; }

        public double? INCOME { get; set; }

        public int? TAXRATE_NO { get; set; }

        public double? EXCHRATE { get; set; }

        public int? ALLOCAGE { get; set; }

        public double? TAX_OVERRIDE { get; set; }

        [StringLength(12)]
        public string TENDER_TYPE { get; set; }
    }
}