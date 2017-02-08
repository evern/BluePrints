namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class BILLOMAT_LINES
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(23)]
        public string BILLCODE { get; set; }

        [StringLength(23)]
        public string STOCKCODE { get; set; }

        public double? QUANTITY { get; set; }

        [StringLength(40)]
        public string DESCRIPTION { get; set; }

        [StringLength(1)]
        public string VARIANTLINE { get; set; }
    }
}