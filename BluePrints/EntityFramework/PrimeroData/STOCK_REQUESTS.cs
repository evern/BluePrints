namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STOCK_REQUESTS
    {
        [Key]
        public int SEQNO { get; set; }

        public int? FROM_LOC { get; set; }

        public int? TO_LOC { get; set; }

        public DateTime? REQUEST_DATE { get; set; }

        public DateTime? REQUIRE_DATE { get; set; }

        public int? STAFFNO { get; set; }

        public int? STATUS { get; set; }

        public int? TRANSTYPE { get; set; }

        [StringLength(20)]
        public string CUSTORDERNO { get; set; }

        public int? NARRATIVE_SEQNO { get; set; }
    }
}