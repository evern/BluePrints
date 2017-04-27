namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class JOB_STATUS
    {
        [Key]
        [StringLength(1)]
        public string STATUSKEY { get; set; }

        [Required]
        [StringLength(30)]
        public string STATUSDESC { get; set; }

        [StringLength(1)]
        public string ADMIN_STAT { get; set; }

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

        public int? GANTTBAR_COLOUR { get; set; }
    }
}