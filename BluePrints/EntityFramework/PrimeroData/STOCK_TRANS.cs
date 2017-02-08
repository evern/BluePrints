namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STOCK_TRANS
    {
        [Key]
        public int SEQNO { get; set; }

        public DateTime? POSTTIME { get; set; }

        public DateTime? TRANSDATE { get; set; }

        [StringLength(23)]
        public string STOCKCODE { get; set; }

        public int? TRANSTYPE { get; set; }

        [StringLength(30)]
        public string REF1 { get; set; }

        [StringLength(30)]
        public string REF2 { get; set; }

        public double? QUANTITY { get; set; }

        public double? UNITPRICE { get; set; }

        public int? LOCATION { get; set; }

        public int? TOLOCATION { get; set; }

        [StringLength(1)]
        public string FROM_LEDGER { get; set; }

        public int? FROM_HDR { get; set; }

        [StringLength(20)]
        public string BATCHCODE { get; set; }

        public int? ACCNO { get; set; }

        public int? LINE_SEQNO { get; set; }

        public int? JOBNO { get; set; }

        public int? RECEIPT_NO { get; set; }

        [StringLength(1)]
        public string GLPOSTED { get; set; }

        public int? GLACC { get; set; }

        public int? GLSUBACC { get; set; }

        public int? GLBRANCH { get; set; }

        public double? UNITCOST { get; set; }

        [Required]
        [StringLength(1)]
        public string UPDATEITEM_FLAG { get; set; }

        [StringLength(1)]
        public string INC_FIFO_STOCKTAKE { get; set; }

        [StringLength(1)]
        public string REVERSED { get; set; }

        public double? ON_COSTS { get; set; }

        [StringLength(1)]
        public string POST_LOOKUP_TO_GL { get; set; }

        public DateTime? EXPIRY_DATE { get; set; }

        public int? GLBATCHNO { get; set; }

        public int KITSEQNO { get; set; }

        [StringLength(23)]
        public string KITCODE { get; set; }

        [StringLength(23)]
        public string PLU { get; set; }

        [StringLength(1)]
        public string POST_TO_GL { get; set; }

        public double? PREV_QUANTITY { get; set; }

        public double? PREV_AVECOST { get; set; }

        public double? ALT_QUANTITY { get; set; }

        public double? ALT_AVECOST { get; set; }

        public int? ALT_SEQNO { get; set; }

        public int? SESSION_ID { get; set; }

        public int PERIOD_SEQNO { get; set; }

        public int? AGE_STAMP { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int? AGE { get; set; }

        public double? QTY_TRADED_IN_NEG { get; set; }

        public double? VALUE_TRADED_IN_NEG { get; set; }

        [StringLength(1)]
        public string PERIOD_TRADED_IN_SEQ { get; set; }

        public double? NEW_AVECOST { get; set; }

        public double? NEW_QUANTITY { get; set; }

        public double? NEW_LOC_QTY { get; set; }

        public int? NEW_SEQORDER { get; set; }
    }
}