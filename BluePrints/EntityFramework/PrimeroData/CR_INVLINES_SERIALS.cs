namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CR_INVLINES_SERIALS
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(50)]
        public string SERIALNO { get; set; }

        public int? INVLINEID { get; set; }

        public DateTime? POSTTIME { get; set; }
    }
}