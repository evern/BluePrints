namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SUMTASK")]
    public partial class SUMTASK
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int proj_id { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int wbs_id { get; set; }

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

        public int? complete_cnt { get; set; }

        public int? active_cnt { get; set; }

        public int? notstarted_cnt { get; set; }

        public int? base_complete_cnt { get; set; }

        public int? base_active_cnt { get; set; }

        public int? base_notstarted_cnt { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_drtn_hr_cnt { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? total_drtn_hr_cnt { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? base_drtn_hr_cnt { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? remain_drtn_hr_cnt { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? total_float_hr_cnt { get; set; }

        public DateTime? act_end_date { get; set; }

        public DateTime? act_start_date { get; set; }

        public DateTime? base_end_date { get; set; }

        public DateTime? base_start_date { get; set; }

        public DateTime? reend_date { get; set; }

        public DateTime? restart_date { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_this_per_work_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_this_per_work_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_this_per_equip_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_this_per_equip_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_this_per_mat_cost { get; set; }

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

        public DateTime? target_start_date { get; set; }

        public DateTime? target_end_date { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? target_drtn_hr_cnt { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        public virtual PROJWBS PROJWBS { get; set; }
    }
}
