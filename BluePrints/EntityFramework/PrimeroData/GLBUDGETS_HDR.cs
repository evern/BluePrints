namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GLBUDGETS_HDR
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(8)]
        public string BUDGETCODE { get; set; }
    }
}
