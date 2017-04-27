namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class BANK_REC_UPLOADS
    {
        [Key]
        public int SEQNO { get; set; }

        public int? RECONCILENO { get; set; }

        public int? ACCNO { get; set; }

        public int? SUBACCNO { get; set; }

        [StringLength(50)]
        public string CHQNO { get; set; }

        [StringLength(15)]
        public string DATESTR { get; set; }

        [StringLength(255)]
        public string DETAILS { get; set; }

        public double? AMOUNT { get; set; }

        public int? RECONCILED { get; set; }

        public int? GLTRANS_SEQ { get; set; }

        [StringLength(5)]
        public string MTS { get; set; }

        [StringLength(40)]
        public string REFERENCE { get; set; }

        [StringLength(12)]
        public string PARTICULARS { get; set; }
    }
}