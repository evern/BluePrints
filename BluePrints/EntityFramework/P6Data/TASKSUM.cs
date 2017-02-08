namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TASKSUM")]
    public partial class TASKSUM
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int task_sum_id { get; set; }

        public int wbs_id { get; set; }

        public int proj_id { get; set; }

        public int? complete_cnt { get; set; }

        public int? active_cnt { get; set; }

        public int? notstarted_cnt { get; set; }

        public int? base_complete_cnt { get; set; }

        public int? base_active_cnt { get; set; }

        public int? base_notstarted_cnt { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_drtn_hr_cnt { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_equip_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_work_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? total_drtn_hr_cnt { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? base_drtn_hr_cnt { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? base_work_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? base_equip_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? etc_work_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? remain_drtn_hr_cnt { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? remain_work_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? remain_equip_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? total_float_hr_cnt { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_expense_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_work_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_equip_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? base_expense_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? base_work_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? base_equip_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? bcwp { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? etc { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? bcws { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? remain_expense_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? remain_work_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? remain_equip_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? perfm_work_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? sched_work_qty { get; set; }

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
        public decimal? base_mat_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? remain_mat_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_mat_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_this_per_mat_cost { get; set; }

        [Column(TypeName = "text")]
        public string spread_data { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }
    }
}