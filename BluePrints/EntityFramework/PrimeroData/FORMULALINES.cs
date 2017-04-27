namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class FORMULALINES
    {
        [Key]
        public int SEQNO { get; set; }

        public int HDR_SEQNO { get; set; }

        public int PERIODNO { get; set; }

        public double? SPREAD { get; set; }
    }
}