namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GLBUDGETS
    {
        [Key]
        public int SEQNO { get; set; }

        public int? ACCNO { get; set; }

        [StringLength(8)]
        public string BUDGETCODE { get; set; }

        public double? BUDGETVAL { get; set; }

        public int? PERIODNO { get; set; }

        public int? BRANCHNO { get; set; }

        public int? SUBACCNO { get; set; }

        public int? HDR_SEQNO { get; set; }
    }
}
