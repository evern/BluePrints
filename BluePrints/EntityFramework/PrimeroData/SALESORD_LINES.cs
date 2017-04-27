namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class SALESORD_LINES
    {
        [Key]
        public int SEQNO { get; set; }

        public int? ACCNO { get; set; }

        public int? HDR_SEQNO { get; set; }

        [StringLength(23)]
        public string STOCKCODE { get; set; }

        [StringLength(40)]
        public string DESCRIPTION { get; set; }

        public double? ORD_QUANT { get; set; }

        public double? SUP_QUANT { get; set; }

        public double? INV_QUANT { get; set; }

        public double? UNITPRICE { get; set; }

        public double? DISCOUNT { get; set; }

        public int? ANALYSIS { get; set; }

        public int? LOCATION { get; set; }

        public double? SUPPLY_NOW { get; set; }

        public double? INVOICE_NOW { get; set; }

        [StringLength(15)]
        public string JOBCODE { get; set; }

        [StringLength(20)]
        public string BATCHCODE { get; set; }

        public int? SUBCODE { get; set; }

        public int? BRANCHNO { get; set; }

        public double? LAST_SUP { get; set; }

        public double? LAST_INV { get; set; }

        public double? TAXRATE { get; set; }

        public int? TAXRATE_NO { get; set; }

        public double? LINETAX_OVERRIDE { get; set; }

        [StringLength(1)]
        public string LINETAX_OVERRIDDEN { get; set; }

        [StringLength(50)]
        public string SERIALNO { get; set; }

        public double? RELEASE_QUANT { get; set; }

        [StringLength(12)]
        public string BINCODE { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? LINETOTAL { get; set; }

        public int? LSTATUS { get; set; }

        public double? LISTPRICE { get; set; }

        public int? SOLINEID { get; set; }

        public int? CONTRACT_HDR { get; set; }

        [StringLength(40)]
        public string LINKED_STOCKCODE { get; set; }

        public double? LINKED_QTY { get; set; }

        public double BKORD_QUANT { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? UNINV_QUANT { get; set; }

        public double PICK_NOW { get; set; }

        public double PICKED_QUANT { get; set; }

        public double LAST_PICKED { get; set; }

        public double RELEASE_NOW { get; set; }

        public double LAST_RELEASED { get; set; }

        public int? NARRATIVE_SEQNO { get; set; }

        [Required]
        [StringLength(1)]
        public string PRICE_OVERRIDDEN { get; set; }

        [StringLength(23)]
        public string KITCODE { get; set; }

        public int HDR_STATUS { get; set; }

        public int LINETYPE { get; set; }

        public int KITSEQNO { get; set; }

        [StringLength(1)]
        public string BOMTYPE { get; set; }

        [StringLength(1)]
        public string SHOWLINE { get; set; }

        [StringLength(1)]
        public string LINKEDSTATUS { get; set; }

        [StringLength(1)]
        public string BOMPRICING { get; set; }

        public double? HIDDEN_SELL { get; set; }

        public double? CORRECTION_QUANT { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? CORRECTED_QUANT { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? UNSUP_QUANT { get; set; }

        [StringLength(20)]
        public string CUSTORDERNO { get; set; }

        public DateTime? DUEDATE { get; set; }

        public int? SUPPLIERNO { get; set; }

        public int? PURCHORDNO { get; set; }
    }
}