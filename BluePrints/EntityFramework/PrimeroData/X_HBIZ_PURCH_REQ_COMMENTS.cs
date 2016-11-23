namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class X_HBIZ_PURCH_REQ_COMMENTS
    {
        [Key]
        public int SEQNO { get; set; }

        public int? PARENT_SEQNO { get; set; }

        public int STAFFNO { get; set; }

        public DateTime DATE_CREATED { get; set; }

        [Column(TypeName = "text")]
        [Required]
        public string COMMENT { get; set; }

        public int PURCH_REQ_SEQNO { get; set; }
    }
}
