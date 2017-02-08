namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class FORMULALINES
    {
        [Key]
        public int SEQNO { get; set; }

        public int HDR_SEQNO { get; set; }

        public int PERIODNO { get; set; }

        public double? SPREAD { get; set; }
    }
}