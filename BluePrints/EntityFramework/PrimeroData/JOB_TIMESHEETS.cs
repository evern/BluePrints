namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class JOB_TIMESHEETS
    {
        [Key]
        public int SEQNO { get; set; }

        public int? STAFFNO { get; set; }

        public int? JOBNO { get; set; }

        [StringLength(60)]
        public string TITLE { get; set; }

        [Required]
        [StringLength(23)]
        public string STOCKCODE { get; set; }

        [StringLength(60)]
        public string DESCRIPTION { get; set; }

        public double? UNITPRICE { get; set; }

        public DateTime? WEEK_START_DATE { get; set; }

        public double? DAY1 { get; set; }

        [Required]
        [StringLength(1)]
        public string DAY1_POSTED { get; set; }

        public int? DAY1_NARRATIVE { get; set; }

        public double? DAY2 { get; set; }

        [Required]
        [StringLength(1)]
        public string DAY2_POSTED { get; set; }

        public int? DAY2_NARRATIVE { get; set; }

        public double? DAY3 { get; set; }

        [Required]
        [StringLength(1)]
        public string DAY3_POSTED { get; set; }

        public int? DAY3_NARRATIVE { get; set; }

        public double? DAY4 { get; set; }

        [Required]
        [StringLength(1)]
        public string DAY4_POSTED { get; set; }

        public int? DAY4_NARRATIVE { get; set; }

        public double? DAY5 { get; set; }

        [Required]
        [StringLength(1)]
        public string DAY5_POSTED { get; set; }

        public int? DAY5_NARRATIVE { get; set; }

        public double? DAY6 { get; set; }

        [Required]
        [StringLength(1)]
        public string DAY6_POSTED { get; set; }

        public int? DAY6_NARRATIVE { get; set; }

        public double? DAY7 { get; set; }

        [Required]
        [StringLength(1)]
        public string DAY7_POSTED { get; set; }

        public int? DAY7_NARRATIVE { get; set; }

        [StringLength(1)]
        public string IS_OVERTIME { get; set; }

        public int? LINE_ID { get; set; }

        public int? RATE_SEQNO { get; set; }

        public double? RATE_FACTOR { get; set; }

        public int? COST_GROUP { get; set; }

        public int? COST_TYPE { get; set; }

        public double? LABOUR_ALLOWANCE { get; set; }

        [StringLength(1)]
        public string HAS_ALLOWANCE { get; set; }

        [StringLength(20)]
        public string SOURCE_REF { get; set; }

        public bool? X_DECLINED { get; set; }

        [StringLength(255)]
        public string X_DECLINE_REASON { get; set; }

        public int? X_APPROVAL_MANAGER { get; set; }

        public bool? X_SUBMITTED { get; set; }

        [StringLength(50)]
        public string X_VARIATIONCODE { get; set; }
    }
}