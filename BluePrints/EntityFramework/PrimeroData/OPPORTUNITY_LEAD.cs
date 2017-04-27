namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class OPPORTUNITY_LEAD
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(60)]
        public string DESCRIPTION { get; set; }
    }
}