namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class D_DEBIT_HDR
    {
        [Key]
        public int SEQNO { get; set; }

        public int? BRANCHNO { get; set; }

        public int? STATUS { get; set; }

        [StringLength(40)]
        public string REF1 { get; set; }

        [StringLength(40)]
        public string REF2 { get; set; }

        public DateTime? CREATEDATE { get; set; }

        public DateTime? EXPDATE { get; set; }

        public DateTime? PROCESSDATE { get; set; }
    }
}