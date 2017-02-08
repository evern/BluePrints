namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class JOB_STATUS_CONSTRAINT
    {
        [Key]
        public int SEQNO { get; set; }

        [Required]
        [StringLength(1)]
        public string ISACTIVE { get; set; }

        [StringLength(1)]
        public string FROM_STATUS { get; set; }

        [StringLength(1)]
        public string TO_STATUS { get; set; }

        [Required]
        [StringLength(50)]
        public string DESCRIPTION { get; set; }

        [Required]
        [StringLength(5)]
        public string SHORTDESC { get; set; }

        [StringLength(1)]
        public string TRACKEVENT { get; set; }
    }
}