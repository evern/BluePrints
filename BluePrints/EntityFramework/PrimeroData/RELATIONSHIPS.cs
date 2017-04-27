namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class RELATIONSHIPS
    {
        [Key]
        public int SEQNO { get; set; }

        public int REL_SEQNO { get; set; }

        public int SOURCE_ENT_SEQNO { get; set; }

        public int DEST_ENT_SEQNO { get; set; }

        public int RELSETSEQNO { get; set; }
    }
}