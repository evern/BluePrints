namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class OPPORTUNITY_LEAD
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(60)]
        public string DESCRIPTION { get; set; }
    }
}