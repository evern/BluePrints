namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class TERRITORYELEMENTS
    {
        [Key]
        public int SEQNO { get; set; }

        public int HDR_SEQNO { get; set; }

        public int DR_ACCGROUP_SEQNO { get; set; }

        public int STOCK_GROUP_SEQNO { get; set; }
    }
}