namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class DR_PRICE_POLICY_ACC
    {
        [Key]
        public int SEQNO { get; set; }

        public int POLICY_HDR { get; set; }

        public int? ACCNO { get; set; }

        public int? ACCGROUP { get; set; }
    }
}