namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TASKSUMFIN")]
    public partial class TASKSUMFIN
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int task_sum_fin_id { get; set; }

        public int fin_dates_id { get; set; }

        public int wbs_id { get; set; }

        public int proj_id { get; set; }

        public int task_sum_id { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_work_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_work_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_equip_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_equip_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_mat_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_expense_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? total_work_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? total_work_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? total_equip_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? total_equip_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? total_mat_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? total_expense_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? total_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? bcwp { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? perfm_work_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? etc { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? etc_work_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? eac { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? eac_work { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? bcws { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? acwp { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? sched_work_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? base_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? base_work_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? base_work_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? base_mat_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? base_equip_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? base_equip_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? base_expense_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? remain_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? remain_work_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? remain_work_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? remain_mat_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? remain_equip_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? remain_equip_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? remain_expense_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? target_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? target_equip_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? target_equip_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? target_expense_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? target_mat_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? target_work_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? target_work_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? late_remain_work_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? late_remain_work_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? late_remain_equip_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? late_remain_equip_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? late_remain_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? late_remain_mat_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? late_remain_expense_cost { get; set; }

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
