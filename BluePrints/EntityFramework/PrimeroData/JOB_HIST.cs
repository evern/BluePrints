namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class JOB_HIST
    {
        [Key]
        public int SEQNO { get; set; }

        public int? JOBNO { get; set; }

        [StringLength(80)]
        public string SUBJECT { get; set; }

        [StringLength(4096)]
        public string NOTE { get; set; }

        public DateTime? POSTTIME { get; set; }

        public DateTime? TRANSDATE { get; set; }

        public int? SALESNO { get; set; }

        [StringLength(40)]
        public string OUTLOOK_LINK { get; set; }

        [StringLength(23)]
        public string X_INVNO { get; set; }
    }
}