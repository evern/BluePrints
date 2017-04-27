namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class OPPORTUNITY_TYPE
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(60)]
        public string DESCRIPTION { get; set; }
    }
}