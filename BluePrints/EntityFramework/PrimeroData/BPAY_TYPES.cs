namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class BPAY_TYPES
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(20)]
        public string CODE { get; set; }

        [StringLength(40)]
        public string NAME { get; set; }
    }
}