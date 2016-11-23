namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CAMPAIGN_STAGE
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(60)]
        public string DESCRIPT { get; set; }

        [StringLength(1)]
        public string STATUSKEY { get; set; }

        [Required]
        [StringLength(1)]
        public string ADMIN_STAT { get; set; }

        [Required]
        [StringLength(1)]
        public string LOCK_CAMPAIGN { get; set; }

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
        public string WORKFLOW_CONSTRAINED { get; set; }
    }
}
