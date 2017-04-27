namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class CUSTOM_VIEWS
    {
        [Key]
        public int SEQNO { get; set; }

        public int STAFFNO { get; set; }

        public int ENTITY_TYPE { get; set; }

        [StringLength(40)]
        public string VIEW_NAME { get; set; }

        [StringLength(4096)]
        public string VIEW_DETAILS { get; set; }
    }
}