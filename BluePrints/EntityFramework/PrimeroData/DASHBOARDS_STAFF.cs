namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class DASHBOARDS_STAFF
    {
        [Key]
        public int SEQNO { get; set; }

        public int? STAFFNO { get; set; }

        public int? DASHBOARDSEQNO { get; set; }
    }
}
