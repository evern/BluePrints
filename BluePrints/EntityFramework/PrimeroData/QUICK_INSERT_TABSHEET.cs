namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class QUICK_INSERT_TABSHEET
    {
        [Key]
        public int SEQNO { get; set; }

        [Required]
        [StringLength(40)]
        public string TABSHEETNAME { get; set; }

        [StringLength(40)]
        public string TEMPLATENAME { get; set; }

        [StringLength(100)]
        public string DESCRIPTION { get; set; }

        [StringLength(1)]
        public string ISACTIVE { get; set; }
    }
}
