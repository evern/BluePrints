namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class VERIFICATION_HDR
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(10)]
        public string CONFIG_FLAG { get; set; }

        public int CREATED_BY { get; set; }

        public DateTime CREATEDATE { get; set; }
    }
}