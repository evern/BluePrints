namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class JOB_RETENTION_LEVELS
    {
        [Key]
        public int SEQNO { get; set; }

        public double? RTN_2_MIN { get; set; }

        public double? RTN_2_RATE { get; set; }

        public double? RTN_3_MIN { get; set; }

        public double? RTN_3_RATE { get; set; }
    }
}