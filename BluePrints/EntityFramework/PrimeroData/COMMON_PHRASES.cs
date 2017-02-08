namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class COMMON_PHRASES
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(200)]
        public string PHRASETEXT { get; set; }
    }
}