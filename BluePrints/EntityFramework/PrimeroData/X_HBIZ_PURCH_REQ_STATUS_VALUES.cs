namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class X_HBIZ_PURCH_REQ_STATUS_VALUES
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(255)]
        public string STATE { get; set; }
    }
}
