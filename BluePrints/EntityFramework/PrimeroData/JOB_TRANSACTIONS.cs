namespace BluePrints.PrimeroData
{
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using DevExpress.Mvvm.POCO;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class JOB_TRANSACTIONS
    {
        [Key]
        public int SEQNO { get; set; }

        public double? EXCHRATE { get; set; }

        public double? DISCOUNT { get; set; }

        public double? UNITPRICE_INCTAX { get; set; }

        public double? FX_PRICE { get; set; }

        public double? RETENTION_RATE { get; set; }

        public double? RETENTION_AMOUNT { get; set; }

        public double? OVERHEAD { get; set; }

        public double? QUANTITY { get; set; }

        public double? UNITPRICE { get; set; }

        public double? UNITCOST { get; set; }

        public DateTime? ENDDATE { get; set; }

        public DateTime? TRANSDATE { get; set; }

        public int SOURCE_SEQNO { get; set; }

        public int? JOBNO { get; set; }

        public int? MASTER_JOBNO { get; set; }

        [StringLength(1)]
        public string TRANSTYPE { get; set; }

        [StringLength(30)]
        public string LINE_STATUS { get; set; }

        [StringLength(23)]
        public string STOCKCODE { get; set; }

        [StringLength(60)]
        public string DESCRIPTION { get; set; }

        public int? COST_TYPE { get; set; }

        public int? COST_GROUP { get; set; }

        [StringLength(10)]
        public string LINE_SOURCE { get; set; }

        public int? SOURCE_ID { get; set; }

        [StringLength(1)]
        public string NARRATIVE { get; set; }

        public int? STAFFNO { get; set; }

        [StringLength(15)]
        public string STARTTIME { get; set; }

        [StringLength(15)]
        public string ENDTIME { get; set; }

        public int? FROMLOC { get; set; }

        public int? LOCATION { get; set; }

        public int? GLCODE { get; set; }

        public int? BRANCHNO { get; set; }

        public int? SUBCODE { get; set; }

        public int? LINESORT { get; set; }

        public int? TAXNO { get; set; }

        public int? ANALYSIS { get; set; }

        public int? CURRENCYNO { get; set; }

        public int? BILLING_ID { get; set; }

        [StringLength(20)]
        public string BILLING_REF { get; set; }

        public double INVOICED { get; set; }

        public double INVOICEDQTY { get; set; }

        public int ALINENO { get; set; }

        public DateTime? INVOICEDATE { get; set; }

        public int? NARRATIVE_SEQNO { get; set; }

        public double DIM_LENGTH { get; set; }

        public double DIM_WIDTH { get; set; }

        public double DIM_DEPTH { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? TOTAL_QUANTITY { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? NUNITPR { get; set; }

        [Required]
        [StringLength(1)]
        public string PRICE_OVERRIDDEN { get; set; }

        public int LINETYPE { get; set; }

        public int KITSEQNO { get; set; }

        [StringLength(23)]
        public string KITCODE { get; set; }

        [StringLength(23)]
        public string LINKED_STOCKCODE { get; set; }

        public double? LINKED_QTY { get; set; }

        public double HIDDEN_COST { get; set; }

        public double HIDDEN_SELL { get; set; }

        public int JOBLINEID { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? LINECOST { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? LINECHARGE { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? LINETOTAL { get; set; }

        [Required]
        [StringLength(1)]
        public string BOMTYPE { get; set; }

        [Required]
        [StringLength(1)]
        public string BOMPRICING { get; set; }

        [Required]
        [StringLength(1)]
        public string SHOWLINE { get; set; }

        [Required]
        [StringLength(1)]
        public string LINKEDSTATUS { get; set; }

        public double? LISTPRICE { get; set; }

        [StringLength(20)]
        public string SOURCE_REF { get; set; }

        [StringLength(20)]
        public string BATCHCODE { get; set; }

        [Required]
        [StringLength(1)]
        public string SPREADVALUE { get; set; }

        public double TAXRATE { get; set; }

        public double LINETOTAL_TAX { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? LINETOTAL_INCTAX { get; set; }

        public double LINE_TAX { get; set; }

        public double HIDDEN_LINETOTAL { get; set; }

        public int PO_LINESEQNO { get; set; }

        public int INVSEQNO { get; set; }

        public int INVLINE_SEQNO { get; set; }

        public int SCHEDULE_SEQNO { get; set; }

        public DateTime? WIP_OUT_DATE { get; set; }

        public double ALLOWANCE { get; set; }

        [Required]
        [StringLength(1)]
        public string PROGRESSINVOICE { get; set; }

        public int? RATE_SEQNO { get; set; }

        [Required]
        [StringLength(1)]
        public string PAYROLL_STATUS { get; set; }

        public double PAYROLL_HOURS { get; set; }

        public int PAYRATE_NO { get; set; }

        public double PAYRATE { get; set; }

        public double PAYRATE_OVERRIDEN { get; set; }

        public int WAGE_CODE { get; set; }

        public long COST_CENTRE { get; set; }

        [Required]
        [StringLength(1)]
        public string ISSUPPLIED { get; set; }

        public int? SNTYPE { get; set; }

        public int? SNEXPDAYS { get; set; }

        [StringLength(50)]
        public string X_WAGE_TYPE { get; set; }

        public int CREDIT_SCHEDULE_SEQNO { get; set; }

        public double? STDCOST_IN { get; set; }

        public double? STDCOST_OUT { get; set; }

        public double? AVECOST_IN { get; set; }

        public double? AVECOST_OUT { get; set; }

        public double? LATESTCOST_IN { get; set; }

        public double? LATESTCOST_OUT { get; set; }

        [StringLength(1)]
        public string LOOKUP_RECOVERABLE { get; set; }

        public int COST_LINENO { get; set; }

        public int STOCK_TRANS_SEQ_IN { get; set; }

        public double? LINECHARGE_WRITEOFF { get; set; }

        public int WIP_IN_PERIOD_SEQNO { get; set; }

        public int WIP_OUT_PERIOD_SEQNO { get; set; }

        public int SU_SEQNO { get; set; }

        [StringLength(50)]
        public string X_VARIATIONCODE { get; set; }

        [StringLength(50)]
        public string X_PONUMBER { get; set; }

        [StringLength(50)]
        public string X_INVOICENO { get; set; }

        [StringLength(50)]
        public string X_SUPPLIERNAME { get; set; }

        public int X_WEEKNO { get; set; }

        public int X_YEAR { get; set; }
    }
}