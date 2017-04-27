namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class RELATIONSHIPS_DEFN
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(30)]
        public string RELNAME { get; set; }

        [StringLength(30)]
        public string RELNAME_INV { get; set; }

        public int ENT_SOURCE { get; set; }

        public int ENT_DEST { get; set; }

        public int? RELTYPE { get; set; }

        public int? IMAGEINDEX { get; set; }
    }
}