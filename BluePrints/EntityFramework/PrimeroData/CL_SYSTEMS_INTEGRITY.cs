namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CL_SYSTEMS_INTEGRITY
    {
        [Key]
        public int SEQNO { get; set; }

        public int WIDGETID { get; set; }

        public int? CTX_SEQNO { get; set; }

        public int? UTILITIES { get; set; }

        public int? LEDGER_RECONCILIATION { get; set; }

        public int? DATA_VERIFICATION { get; set; }

        public int? TAX_RATE_EXCEPTIONS { get; set; }
    }
}
