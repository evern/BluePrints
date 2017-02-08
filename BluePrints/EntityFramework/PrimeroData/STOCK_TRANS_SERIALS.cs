namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STOCK_TRANS_SERIALS
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(50)]
        public string SERIALNO { get; set; }

        public int? STOCKTRANSSEQNO { get; set; }
    }
}