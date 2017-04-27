namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("MANREP")]
    public partial class MANREP
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SEQNO { get; set; }

        [StringLength(20)]
        public string PERIOD_NAME { get; set; }

        public int? VERSION { get; set; }

        public int? PERIOD_DAYS { get; set; }

        public int? DAYS_USED { get; set; }

        public int? DAY_ORDERQTY { get; set; }

        public DateTime? CALCTIME { get; set; }

        public DateTime? CALCDATE { get; set; }

        public double? DAY_ORDERVAL { get; set; }

        public double? PD_BUDGET { get; set; }

        public double? YR_BUDGET { get; set; }

        public double? DAY_SALES { get; set; }

        public double? PD_SALES { get; set; }

        public double? YR_SALES { get; set; }

        public double? DEBAGEDBAL3 { get; set; }

        public double? DEBAGEDBAL2 { get; set; }

        public double? DEBAGEDBAL1 { get; set; }

        public double? DEBAGEDBAL0 { get; set; }

        public double? CREDAGEDBAL3 { get; set; }

        public double? CREDAGEDBAL2 { get; set; }

        public double? CREDAGEDBAL1 { get; set; }

        public double? CREDAGEDBAL0 { get; set; }

        public double? TOTALSTOCKCOST { get; set; }

        public double? DAY_COST { get; set; }

        public double? PD_COST { get; set; }

        public double? YR_COST { get; set; }

        public double? AVG_DRTRANS_AGE { get; set; }

        public double? AVG_CRTRANS_AGE { get; set; }

        public int? PERIOD_SEQNO { get; set; }

        public double? TOTALSTOCKCOST_AVECOST { get; set; }

        public double? TOTALSTOCKCOST_LATESTCOST { get; set; }

        public double? TOTALSTOCKCOST_STDCOST { get; set; }

        public int? DAY_ORDERQTY_EXCLUDEQUOTES { get; set; }

        public double? DAY_ORDERVAL_EXCLUDEQUOTES { get; set; }

        public double? DAY_SALES_POSTTIME { get; set; }

        public double? PD_SALES_POSTTIME { get; set; }

        public double? YR_SALES_POSTTIME { get; set; }

        public double? DAY_COST_POSTTIME { get; set; }

        public double? PD_COST_POSTTIME { get; set; }

        public double? YR_COST_POSTTIME { get; set; }

        [Required]
        [StringLength(1)]
        public string SCHEDULED { get; set; }

        public int? PARENT_PERIOD_SEQNO { get; set; }

        public double? BUDGET_MARGINPERCENT { get; set; }

        public DateTime? PERIOD_STARTDATE { get; set; }

        public DateTime? PERIOD_ENDDATE { get; set; }

        public int? BUDGET_SEQNO { get; set; }

        public double? RUNRATE_SO_FAR { get; set; }

        public double? RUNRATE_THIS_PERIOD { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? RUNRATE_ACTUAL { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? RUNRATE_ACTUAL_POSTTIME { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? RUNRATE_REQUIRED { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? RUNRATE_REQUIRED_POSTTIME { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? GP_PERIOD { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? GP_PERIOD_POSTTIME { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? GP_DAY { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? GP_DAY_POSTTIME { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? GP_YEAR { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? GP_YEAR_POSTTIME { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? PD_SALES_ESTIMATED { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? PD_SALES_ESTIMATED_POSTTIME { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? STOCKTURN_YEAR_LATESTCOST { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? STOCKTURN_YEAR_STDCOST { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? STOCKTURN_YEAR_AVECOST { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? STOCKTURN_YEAR_LATESTCOST_POSTTIME { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? STOCKTURN_YEAR_STDCOST_POSTTIME { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? STOCKTURN_YEAR_AVECOST_POSTTIME { get; set; }
    }
}