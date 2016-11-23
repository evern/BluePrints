namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class DR_INVLINES
    {
        [Key]
        public int SEQNO { get; set; }

        public int? ACCNO { get; set; }

        [StringLength(20)]
        public string INVNO { get; set; }

        public int? HDR_SEQNO { get; set; }

        [StringLength(23)]
        public string STOCKCODE { get; set; }

        [StringLength(40)]
        public string DESCRIPTION { get; set; }

        public double? QUANTITY { get; set; }

        public double? UNITPRICE { get; set; }

        public double? DISCOUNT { get; set; }

        public int? ANALYSIS { get; set; }

        public int? LOCATION { get; set; }

        public double? UNITPRICE_INCTAX { get; set; }

        [StringLength(30)]
        public string CUSTOMFIELD { get; set; }

        [StringLength(1)]
        public string UPDATE_STOCK { get; set; }

        public double? UNITCOST { get; set; }

        [StringLength(15)]
        public string JOBCODE { get; set; }

        public int? CURRENCYNO { get; set; }

        public double? EXCHRATE { get; set; }

        public double? ORDERQTY { get; set; }

        public double? BKORDQTY { get; set; }

        [StringLength(20)]
        public string BATCHCODE { get; set; }

        [StringLength(1)]
        public string HIDDEN { get; set; }

        public int? SUBCODE { get; set; }

        public int? BRANCHNO { get; set; }

        public double? TAXRATE { get; set; }

        public int? TAXRATE_NO { get; set; }

        public double? LINETOTAL_TAX { get; set; }

        [StringLength(1)]
        public string LINETAX_OVERRIDDEN { get; set; }

        public int? LINE_SOURCE { get; set; }

        public int? JOBNO { get; set; }

        [StringLength(50)]
        public string SERIALNO { get; set; }

        public DateTime? TRANSDATE { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? LINETOTAL { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? LINETOTAL_INCTAX { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? NUNITPR { get; set; }

        public double? LISTPRICE { get; set; }

        [Required]
        [StringLength(1)]
        public string FREIGHT_FREE { get; set; }

        public int CONTRACT_HDR { get; set; }

        public int? DRINVLINEID { get; set; }

        public int? NARRATIVE_SEQNO { get; set; }

        [Required]
        [StringLength(1)]
        public string PRICE_OVERRIDDEN { get; set; }

        [StringLength(23)]
        public string KITCODE { get; set; }

        public int LINETYPE { get; set; }

        public int? KITSEQNO { get; set; }

        [StringLength(23)]
        public string LINKED_STOCKCODE { get; set; }

        public double? LINKED_QTY { get; set; }

        public double? HIDDEN_COST { get; set; }

        public double? HIDDEN_SELL { get; set; }

        [StringLength(1)]
        public string BOMTYPE { get; set; }

        [StringLength(1)]
        public string SHOWLINE { get; set; }

        [StringLength(1)]
        public string LINKEDSTATUS { get; set; }

        [StringLength(1)]
        public string BOMPRICING { get; set; }

        public int CREDITQTY { get; set; }
    }
}
