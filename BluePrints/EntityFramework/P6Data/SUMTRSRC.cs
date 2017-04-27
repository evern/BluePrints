namespace BluePrints.P6Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SUMTRSRC")]
    public partial class SUMTRSRC
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int sumtrsrc_id { get; set; }

        public int? proj_id { get; set; }

        public int? wbs_id { get; set; }

        public DateTime? start_date { get; set; }

        public DateTime? end_date { get; set; }

        [StringLength(20)]
        public string spread_type { get; set; }

        public int? rsrc_id { get; set; }

        public int? role_id { get; set; }

        public DateTime? act_start_date { get; set; }

        public DateTime? act_end_date { get; set; }

        public DateTime? remain_start_date { get; set; }

        public DateTime? remain_end_date { get; set; }

        public DateTime? overalloc_date { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? staffed_act_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? unstaffed_act_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? staffed_act_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? unstaffed_act_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? staffed_act_ot_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? unstaffed_act_ot_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? staffed_act_ot_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? unstaffed_act_ot_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? staffed_act_reg_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? unstaffed_act_reg_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? staffed_act_reg_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? unstaffed_act_reg_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? staffed_late_remain_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? unstaffed_late_remain_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? staffed_late_remain_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? unstaffed_late_remain_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? staffed_remain_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? unstaffed_remain_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? staffed_remain_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? unstaffed_remain_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? staffed_target_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? unstaffed_target_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? staffed_target_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? unstaffed_target_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? staffed_total_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? unstaffed_total_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? staffed_total_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? unstaffed_total_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? fin_period_act_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? fin_period_act_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? fin_period_total_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? fin_period_total_cost { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        public virtual PROJWBS PROJWBS { get; set; }
    }
}