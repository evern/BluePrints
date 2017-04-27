namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class JOB_TIMESHEET_ALLOWANCE
    {
        [Key]
        public int SEQNO { get; set; }

        public int? STAFFNO { get; set; }

        public DateTime? WEEK_START_DATE { get; set; }

        [StringLength(5)]
        public string ALLOWANCE_ID { get; set; }

        [StringLength(50)]
        public string DESCRIPTION { get; set; }

        public double? UNIT { get; set; }
    }
}