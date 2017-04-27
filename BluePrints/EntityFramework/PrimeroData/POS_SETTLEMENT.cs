namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class POS_SETTLEMENT
    {
        [Key]
        public int SEQNO { get; set; }

        [Required]
        [StringLength(15)]
        public string CAID { get; set; }

        public int BRANCHNO { get; set; }

        [Required]
        [StringLength(30)]
        public string TERMINALID { get; set; }

        public int STAFFNO { get; set; }

        public int STANFIRST { get; set; }

        public int STANLAST { get; set; }

        public DateTime TIMESETTLED { get; set; }

        public int BATCHNO { get; set; }
    }
}