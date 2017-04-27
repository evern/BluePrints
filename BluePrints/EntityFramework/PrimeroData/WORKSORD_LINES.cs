namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class WORKSORD_LINES
    {
        [Key]
        public int SEQNO { get; set; }

        public int? HDR_SEQNO { get; set; }

        [StringLength(23)]
        public string STOCKCODE { get; set; }

        [StringLength(60)]
        public string DESCRIPTION { get; set; }

        public double? QTYREQD { get; set; }

        public double? QTYUSED { get; set; }

        [StringLength(20)]
        public string BATCHCODE { get; set; }
    }
}