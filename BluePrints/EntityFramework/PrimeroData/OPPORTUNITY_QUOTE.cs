namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class OPPORTUNITY_QUOTE
    {
        [Key]
        public int SEQNO { get; set; }

        public double? QUOTE_QTY { get; set; }

        public double? QUOTE_UNITPR { get; set; }

        public double? ACTUAL_UNITCOST { get; set; }

        public DateTime? TRANSDATE { get; set; }

        public double? EXCHRATE { get; set; }

        public double? DISCOUNT { get; set; }

        public double? UNITPRICE_INCTAX { get; set; }

        public int HDR_SEQNO { get; set; }

        public int? SECTION { get; set; }

        [StringLength(23)]
        public string STOCKCODE { get; set; }

        [StringLength(60)]
        public string DESCRIPTION { get; set; }

        [StringLength(1)]
        public string SHOW_ON_INVOICE { get; set; }

        public int? COST_CENTRE { get; set; }

        [StringLength(30)]
        public string LINE_STATUS { get; set; }

        public int? COST_CENTRE2 { get; set; }

        [StringLength(1)]
        public string NARRATIVE { get; set; }

        public int? TAXNO { get; set; }

        public int? BRANCHNO { get; set; }

        public int? SUBCODE { get; set; }

        public int? ANALYSIS { get; set; }

        public int? CURRENCYNO { get; set; }

        public int? ALINENO { get; set; }

        public int? GLCODE { get; set; }

        public int? NARRATIVE_SEQNO { get; set; }

        public double DIM_LENGTH { get; set; }

        public double DIM_WIDTH { get; set; }

        public double DIM_DEPTH { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? TOTAL_QUANTITY { get; set; }

        [Required]
        [StringLength(1)]
        public string PRICE_OVERRIDDEN { get; set; }

        [StringLength(1)]
        public string BOMTYPE { get; set; }

        [StringLength(1)]
        public string BOMPRICING { get; set; }

        [StringLength(1)]
        public string SHOWLINE { get; set; }

        [StringLength(1)]
        public string LINKEDSTATUS { get; set; }

        public double? LISTPRICE { get; set; }

        public int LINETYPE { get; set; }

        public int KITSEQNO { get; set; }

        [StringLength(23)]
        public string KITCODE { get; set; }

        [StringLength(23)]
        public string LINKED_STOCKCODE { get; set; }

        public double? LINKED_QTY { get; set; }

        public double HIDDEN_COST { get; set; }

        public double HIDDEN_SELL { get; set; }

        public int? SUPPLIERNO { get; set; }

        public int? FROMLOC { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? LINETOTAL { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? NUNITPR { get; set; }

        public int OPTION_NO { get; set; }

        [Required]
        [StringLength(1)]
        public string SPREADVALUE { get; set; }

        public double TAXRATE { get; set; }

        public double LINETOTAL_TAX { get; set; }

        public double LINE_TAX { get; set; }

        public double HIDDEN_LINETOTAL { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? LINETOTAL_INCTAX { get; set; }

        public int? OPPLINEID { get; set; }

        public double LINETAX_OVERRIDE { get; set; }

        [Required]
        [StringLength(1)]
        public string LINETAX_OVERRIDDEN { get; set; }

        public int? OPTION_NO_SEQNO { get; set; }
    }
}
