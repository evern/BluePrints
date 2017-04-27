namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class PRINT_LOG
    {
        [Key]
        public int SEQNO { get; set; }

        public int DOC_TYPE { get; set; }

        public int HDR_SEQNO { get; set; }

        public DateTime PRINT_TIME { get; set; }

        [StringLength(15)]
        public string REFERENCE { get; set; }

        [StringLength(20)]
        public string SENT_TO { get; set; }
    }
}