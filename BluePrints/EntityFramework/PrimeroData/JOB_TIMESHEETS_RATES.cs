namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class JOB_TIMESHEETS_RATES
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(5)]
        public string SHORTCODE { get; set; }

        [StringLength(30)]
        public string RATENAME { get; set; }

        public double? COSTRATE { get; set; }

        public double? SELLRATE { get; set; }

        public double? PAYROLLRATE { get; set; }

        public int? RATECOLOR { get; set; }

        [StringLength(5)]
        public string PAYROLL_ALLOWANCE_ID { get; set; }
    }
}