namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class X_HBIZ_PURCH_REQ_HDR_HISTORY
    {
        [Key]
        public int SEQNO { get; set; }

        public int? STAFFNO { get; set; }

        [StringLength(255)]
        public string REASON { get; set; }

        public DateTime? TIME { get; set; }

        public bool? IS_APPROVED { get; set; }

        public int REQ_HDR_SEQNO { get; set; }

        public int? STATUS { get; set; }
    }
}
