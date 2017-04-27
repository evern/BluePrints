namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class JOB_TIMESHEET_ALLOWANCE_TYPES
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(5)]
        public string ALLOWANCE_ID { get; set; }

        public int? ALLOWANCE_FACTOR { get; set; }

        [StringLength(50)]
        public string DESCRIPTION { get; set; }

        [StringLength(50)]
        public string UNIT_OF_MEASURE { get; set; }

        [StringLength(5)]
        public string PAYROLL_ALLOWANCE_ID { get; set; }
    }
}