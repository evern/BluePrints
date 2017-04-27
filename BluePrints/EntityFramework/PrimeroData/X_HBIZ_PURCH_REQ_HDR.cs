namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class X_HBIZ_PURCH_REQ_HDR
    {
        [Key]
        public int SEQNO { get; set; }

        public int STAFFNO { get; set; }

        [Required]
        [StringLength(255)]
        public string TITLE { get; set; }

        public DateTime DATE_CREATED { get; set; }

        public DateTime? DATE_REQUIRED { get; set; }

        public int? REQUEST_TO_STAFFNO { get; set; }

        public bool? ISACTIVE { get; set; }

        [Column(TypeName = "text")]
        public string DESCRIPTION { get; set; }

        public int? JOBNO { get; set; }

        public int BRANCHNO { get; set; }

        public int LOCNO { get; set; }

        public bool? READY_FOR_REVIEW { get; set; }

        [StringLength(255)]
        public string DELADDR_1 { get; set; }

        [StringLength(255)]
        public string DELADDR_2 { get; set; }

        [StringLength(255)]
        public string DELADDR_3 { get; set; }

        [StringLength(255)]
        public string DELADDR_4 { get; set; }

        [StringLength(255)]
        public string DELADDR_5 { get; set; }

        [StringLength(255)]
        public string DELADDR_6 { get; set; }
    }
}