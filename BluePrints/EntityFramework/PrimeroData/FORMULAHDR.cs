namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FORMULAHDR")]
    public partial class FORMULAHDR
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(15)]
        public string NAME { get; set; }

        [StringLength(30)]
        public string DESCRIPTION { get; set; }
    }
}
