namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class STOCK_TRANS_SERIALS
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(50)]
        public string SERIALNO { get; set; }

        public int? STOCKTRANSSEQNO { get; set; }
    }
}