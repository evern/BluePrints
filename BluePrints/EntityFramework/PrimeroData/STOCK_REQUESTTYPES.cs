namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STOCK_REQUESTTYPES
    {
        [Key]
        public int SEQNO { get; set; }

        [Required]
        [StringLength(40)]
        public string DISPLAY_NAME { get; set; }
    }
}
