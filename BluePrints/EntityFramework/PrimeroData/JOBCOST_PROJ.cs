namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class JOBCOST_PROJ
    {
        [Key]
        public int SEQNO { get; set; }

        [Required]
        [StringLength(15)]
        public string PROJ_CODE { get; set; }

        [StringLength(60)]
        public string PROJ_TITLE { get; set; }

        [Required]
        [StringLength(1)]
        public string ISACTIVE { get; set; }

        [Column(TypeName = "text")]
        public string DESCRIPTION { get; set; }
    }
}