namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SUMTASKSPREAD")]
    public partial class SUMTASKSPREAD
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int proj_id { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int wbs_id { get; set; }

        [Key]
        [Column(Order = 2)]
        public DateTime start_date { get; set; }

        [Key]
        [Column(Order = 3)]
        public DateTime end_date { get; set; }

        [Key]
        [Column(Order = 4)]
        [StringLength(20)]
        public string spread_type { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_equip_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_expense_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_mat_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_work_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_equip_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_work_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? base_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? base_equip_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? base_expense_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? base_mat_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? base_work_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? base_equip_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? base_work_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? late_remain_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? late_remain_equip_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? late_remain_expense_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? late_remain_mat_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? late_remain_work_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? late_remain_equip_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? late_remain_work_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? remain_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? remain_equip_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? remain_expense_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? remain_mat_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? remain_work_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? remain_equip_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? remain_work_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? target_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? target_equip_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? target_expense_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? target_mat_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? target_work_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? target_equip_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? target_work_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? total_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? total_equip_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? total_expense_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? total_mat_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? total_work_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? total_equip_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? total_work_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? bcwp { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? bcws { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? eac { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? eac_work_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? etc { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? etc_work_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? perfm_work_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? sched_work_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? fin_period_act_work_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? fin_period_act_work_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? fin_period_act_equip_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? fin_period_act_equip_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? fin_period_act_mat_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? fin_period_act_expense_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? fin_period_total_work_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? fin_period_total_work_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? fin_period_total_equip_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? fin_period_total_equip_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? fin_period_total_mat_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? fin_period_total_expense_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? fin_period_total_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? fin_period_act_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? fin_period_bcwp { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? fin_period_perfm_work_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? fin_period_eac { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? fin_period_eac_work_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? fin_period_bcws { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? fin_period_sched_work_qty { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        public virtual PROJWBS PROJWBS { get; set; }
    }
}
