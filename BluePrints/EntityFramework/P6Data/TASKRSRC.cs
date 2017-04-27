namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("TASKRSRC")]
    public partial class TASKRSRC
    {
        public TASKRSRC()
        {
            RSRCHOUR = new HashSet<RSRCHOUR>();
            TRSRCFIN = new HashSet<TRSRCFIN>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int taskrsrc_id { get; set; }

        public int task_id { get; set; }

        public int proj_id { get; set; }

        [Required]
        [StringLength(10)]
        public string rsrc_type { get; set; }

        [Required]
        [StringLength(1)]
        public string cost_qty_link_flag { get; set; }

        public int? role_id { get; set; }

        public int? acct_id { get; set; }

        public int? rsrc_id { get; set; }

        public int? pobs_id { get; set; }

        public int? skill_level { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? pend_complete_pct { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? remain_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? pend_remain_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? target_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? remain_qty_per_hr { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? pend_act_reg_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? target_lag_drtn_hr_cnt { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? target_qty_per_hr { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_ot_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? pend_act_ot_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_reg_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? relag_drtn_hr_cnt { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? ot_factor { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? cost_per_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? target_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_reg_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_ot_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? remain_cost { get; set; }

        public DateTime? act_start_date { get; set; }

        public DateTime? act_end_date { get; set; }

        public DateTime? restart_date { get; set; }

        public DateTime? reend_date { get; set; }

        public DateTime? target_start_date { get; set; }

        public DateTime? target_end_date { get; set; }

        public DateTime? rem_late_start_date { get; set; }

        public DateTime? rem_late_end_date { get; set; }

        [StringLength(22)]
        public string guid { get; set; }

        [StringLength(14)]
        public string rate_type { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_this_per_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_this_per_qty { get; set; }

        public int? curv_id { get; set; }

        [Required]
        [StringLength(1)]
        public string rollup_dates_flag { get; set; }

        [Required]
        [StringLength(24)]
        public string cost_per_qty_source_type { get; set; }

        [StringLength(4000)]
        public string remain_crv { get; set; }

        [StringLength(4000)]
        public string target_crv { get; set; }

        [StringLength(4000)]
        public string actual_crv { get; set; }

        [Column(TypeName = "text")]
        public string rsrc_request_data { get; set; }

        [Required]
        [StringLength(1)]
        public string ts_pend_act_end_flag { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? prior_ts_act_reg_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? prior_ts_act_ot_qty { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ACCOUNT ACCOUNT { get; set; }

        public virtual POBS POBS { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        public virtual ROLES ROLES { get; set; }

        public virtual RSRC RSRC { get; set; }

        public virtual RSRCCURV RSRCCURV { get; set; }

        public virtual ICollection<RSRCHOUR> RSRCHOUR { get; set; }

        public virtual TASK TASK { get; set; }

        public virtual ICollection<TRSRCFIN> TRSRCFIN { get; set; }
    }
}