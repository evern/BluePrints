namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class OBJECT_LOCK
    {
        [Key]
        public int SEQNO { get; set; }

        [Required]
        [StringLength(40)]
        public string OBJECT_ID { get; set; }

        public int RECORDID { get; set; }

        [Required]
        [StringLength(40)]
        public string COMPUTERID { get; set; }

        public int? PONO { get; set; }
    }
}
