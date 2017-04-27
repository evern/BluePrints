namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class DR_INVLINES_SERIALS
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(50)]
        public string SERIALNO { get; set; }

        public int? INVLINEID { get; set; }

        public DateTime? POSTTIME { get; set; }
    }
}