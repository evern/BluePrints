namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STOCK_REQUESTLINES
    {
        [Key]
        public int SEQNO { get; set; }

        public int HDR_SEQNO { get; set; }

        [Required]
        [StringLength(23)]
        public string STOCKCODE { get; set; }

        [Required]
        [StringLength(40)]
        public string DESCRIPTION { get; set; }

        public double? PACK_SIZE { get; set; }

        public double? REQ_QUANT { get; set; }

        public double? INTRANS_QUANT { get; set; }

        public double? SUP_QUANT { get; set; }

        [StringLength(30)]
        public string COMMENT { get; set; }

        [StringLength(20)]
        public string BATCHCODE { get; set; }

        public int? LINETYPE { get; set; }

        [StringLength(23)]
        public string LINKED_STOCKCODE { get; set; }

        public double? LINKED_QTY { get; set; }

        [StringLength(1)]
        public string BOMTYPE { get; set; }

        [StringLength(1)]
        public string SHOWLINE { get; set; }

        [StringLength(1)]
        public string LINKEDSTATUS { get; set; }

        [StringLength(1)]
        public string BOMPRICING { get; set; }

        public int? NARRATIVE_SEQNO { get; set; }
    }
}
