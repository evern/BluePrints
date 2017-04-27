namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class INWARDS_GOODS_LINES
    {
        [Key]
        public int SEQNO { get; set; }

        public int HDR_SEQNO { get; set; }

        public int PO_NUMBER { get; set; }

        public int PO_LINE_NUM { get; set; }

        public int SHIPMENTNO { get; set; }

        public int LOCNO { get; set; }

        public int BATCHNO { get; set; }

        public int BRANCHNO { get; set; }

        public int JOBNO { get; set; }

        public int COST_TYPE { get; set; }

        public int COST_GROUP { get; set; }

        [StringLength(23)]
        public string STOCKCODE { get; set; }

        [StringLength(40)]
        public string DESCRIPTION { get; set; }

        public double? QUANTITY { get; set; }

        public double? UNITPRICE { get; set; }

        public double? DUTYCOST { get; set; }

        public double? FIXEDCOST { get; set; }

        public double LCOST { get; set; }

        public double FXCOST { get; set; }

        public double INV_QUANT { get; set; }

        [StringLength(20)]
        public string BATCHCODE { get; set; }

        public DateTime? EXPIRY_DATE { get; set; }

        [StringLength(15)]
        public string SUPPLIERCODE { get; set; }

        public double PURCHPACKQUANT { get; set; }

        public double PURCHPACKPRICE { get; set; }

        public double? EXCHRATE { get; set; }

        public int? LINETYPE { get; set; }

        public int? KITSEQNO { get; set; }

        [StringLength(23)]
        public string KITCODE { get; set; }

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

        [StringLength(1)]
        public string COMPLETE { get; set; }

        public double? DISCOUNT { get; set; }

        public double? INV_FC_COST { get; set; }

        public double? INV_EXCHRATE { get; set; }

        public double? INV_COST { get; set; }

        [StringLength(1)]
        public string INV_COMPLETE { get; set; }

        [StringLength(1)]
        public string VAR_GLPOSTED { get; set; }

        public double? FX_VAR { get; set; }

        public DateTime? INV_TRANSDATE { get; set; }

        public int? INWGLID { get; set; }

        public int PERIOD_SEQNO { get; set; }

        public int? AGE_STAMP { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int? INV_AGE { get; set; }
    }
}