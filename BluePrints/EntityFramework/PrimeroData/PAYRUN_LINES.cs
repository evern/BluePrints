namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class PAYRUN_LINES
    {
        [Key]
        public int SEQNO { get; set; }

        public int HDRSEQNO { get; set; }

        public int? ACCNO { get; set; }

        public int? SUBACCNO { get; set; }

        public int? BRANCHNO { get; set; }

        public double? AMOUNT { get; set; }

        [StringLength(30)]
        public string REFERENCE { get; set; }

        public DateTime? PAYRUN_DATE { get; set; }

        [StringLength(50)]
        public string DESCRIPTION { get; set; }

        public int? PAYROLL_ID { get; set; }

        public int? COSTCENTRE { get; set; }

        public int? UNITS { get; set; }

        public int? ALLOWANCE { get; set; }

        public int? SESSION_ID { get; set; }
    }
}