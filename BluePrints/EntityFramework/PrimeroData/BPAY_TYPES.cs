namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class BPAY_TYPES
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(20)]
        public string CODE { get; set; }

        [StringLength(40)]
        public string NAME { get; set; }
    }
}