namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class JOB_COSTGROUPS
    {
        [Key]
        public int SEQNO { get; set; }

        public double? DEF_MARKUP { get; set; }

        public double? DEF_OVERHEAD { get; set; }

        [StringLength(50)]
        public string COSTDESC { get; set; }

        [StringLength(3)]
        public string SHORTCODE { get; set; }

        [StringLength(1)]
        public string SHOWONQUOTE { get; set; }

        [StringLength(1)]
        public string CONSOLIDATE { get; set; }

        [StringLength(1)]
        public string COPY_FROM_QUOTE { get; set; }
    }
}
