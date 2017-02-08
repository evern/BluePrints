namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class NARRATIVES
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(4096)]
        public string NARRATIVE { get; set; }
    }
}