namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class OPPORTUNITY_STAGE
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(60)]
        public string DESCRIPTION { get; set; }

        public int? DEF_PROBABILITY { get; set; }

        [StringLength(1)]
        public string STATUSKEY { get; set; }

        [Required]
        [StringLength(1)]
        public string ADMIN_STAT { get; set; }

        [Required]
        [StringLength(1)]
        public string LOCK_JOB { get; set; }

        [Required]
        [StringLength(1)]
        public string ISARCHIVED { get; set; }

        [Required]
        [StringLength(1)]
        public string ISCOMPLETE { get; set; }

        [Required]
        [StringLength(1)]
        public string ISACTIVE { get; set; }

        [Required]
        [StringLength(1)]
        public string ISINVOICEREADY { get; set; }

        [Required]
        [StringLength(1)]
        public string ISLOCKQUOTE { get; set; }

        [Required]
        [StringLength(1)]
        public string WORKFLOW_CONSTRAINED { get; set; }
    }
}