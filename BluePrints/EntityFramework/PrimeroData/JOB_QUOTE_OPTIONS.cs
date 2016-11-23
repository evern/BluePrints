namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class JOB_QUOTE_OPTIONS
    {
        [Key]
        public int SEQNO { get; set; }

        public int? JOBNO { get; set; }

        public int? OPTION_NO { get; set; }

        [StringLength(30)]
        public string OPTION_NAME { get; set; }

        [Required]
        [StringLength(1)]
        public string OPTION_SELECTED { get; set; }
    }
}
