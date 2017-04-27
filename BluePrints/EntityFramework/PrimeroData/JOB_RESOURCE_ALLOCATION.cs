namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class JOB_RESOURCE_ALLOCATION
    {
        [Key]
        public int SEQNO { get; set; }

        public int RESOURCE_SEQNO { get; set; }

        public int JOBNO { get; set; }

        [StringLength(255)]
        public string SUBJECT_NOTES { get; set; }

        public int? LOCATION { get; set; }

        public DateTime? START_DATE { get; set; }

        public DateTime? START_TIME { get; set; }

        public DateTime? END_DATE { get; set; }

        public DateTime? END_TIME { get; set; }

        public double? TOTAL_HOURS { get; set; }

        [Required]
        [StringLength(1)]
        public string APPOINTMENT_SCHEDULED { get; set; }
    }
}