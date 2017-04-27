namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class D_DEBIT_LINES
    {
        [Key]
        public int SEQNO { get; set; }

        public int HDR_SEQNO { get; set; }

        public int? TRANS_SEQNO { get; set; }

        public int? ACCNO { get; set; }

        public double? INVAMT { get; set; }

        public double? INVPAY { get; set; }

        public double? INVPAID { get; set; }

        [StringLength(20)]
        public string INVNO { get; set; }

        [StringLength(40)]
        public string REF1 { get; set; }

        [StringLength(40)]
        public string REF2 { get; set; }
    }
}