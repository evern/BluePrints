namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class CONTACT_LIST_ITEM
    {
        [Key]
        public int SEQNO { get; set; }

        public int HDR_SEQNO { get; set; }

        public int ITEM_TYPE { get; set; }

        public int? ITEM_SEQNO { get; set; }

        [StringLength(1)]
        public string QUERY_INSERTED { get; set; }
    }
}