namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class MANREP_BRANCH
    {
        [Key]
        public int SEQNO { get; set; }

        public int MANREP_SEQNO { get; set; }

        public int BRANCHNO { get; set; }

        public int? DAY_ORDERQTY { get; set; }

        public double? DAY_ORDERVAL { get; set; }

        public double? DAY_SALES { get; set; }

        public double? PD_SALES { get; set; }

        public double? YR_SALES { get; set; }

        public double? DAY_COST { get; set; }

        public double? PD_COST { get; set; }

        public double? YR_COST { get; set; }

        public double? AVG_DRTRANS_AGE { get; set; }

        public double? AVG_CRTRANS_AGE { get; set; }

        public int? DAY_ORDERQTY_EXCLUDEQUOTES { get; set; }

        public double? DAY_ORDERVAL_EXCLUDEQUOTES { get; set; }

        public double? DAY_SALES_POSTTIME { get; set; }

        public double? PD_SALES_POSTTIME { get; set; }

        public double? YR_SALES_POSTTIME { get; set; }

        public double? DAY_COST_POSTTIME { get; set; }

        public double? PD_COST_POSTTIME { get; set; }

        public double? YR_COST_POSTTIME { get; set; }

        [StringLength(1)]
        public string SCHEDULED { get; set; }

        public int? PERIOD_DAYS { get; set; }

        public int? DAYS_USED { get; set; }

        public double? PD_BUDGET { get; set; }

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
    }
}
