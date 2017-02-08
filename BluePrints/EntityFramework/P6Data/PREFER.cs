namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PREFER")]
    public partial class PREFER
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int prefer_id { get; set; }

        public int hr_decimal_cnt { get; set; }

        public int xfer_complete_day_cnt { get; set; }

        public int xfer_notstart_day_cnt { get; set; }

        public int max_wbs_level_cnt { get; set; }

        public int max_rsrc_level_cnt { get; set; }

        public int max_acct_level_cnt { get; set; }

        public int max_task_actv_type_cnt { get; set; }

        public int future_ts_cnt { get; set; }

        public int max_obs_level_cnt { get; set; }

        public int week_start_day_num { get; set; }

        public int ts_approval_level { get; set; }

        public int task_code_len { get; set; }

        public int proj_short_len { get; set; }

        public int wbs_short_len { get; set; }

        public int rsrc_short_len { get; set; }

        public int acct_short_len { get; set; }

        public int role_short_len { get; set; }

        public int max_base_cnt { get; set; }

        [Column(TypeName = "numeric")]
        public decimal def_target_drtn_hr_cnt { get; set; }

        [Column(TypeName = "numeric")]
        public decimal day_hr_cnt { get; set; }

        [Column(TypeName = "numeric")]
        public decimal week_hr_cnt { get; set; }

        [Column(TypeName = "numeric")]
        public decimal year_hr_cnt { get; set; }

        [Column(TypeName = "numeric")]
        public decimal month_hr_cnt { get; set; }

        public int max_actv_level_cnt { get; set; }

        public int max_eps_level_cnt { get; set; }

        public int max_pcat_level_cnt { get; set; }

        public int max_rcat_level_cnt { get; set; }

        [Required]
        [StringLength(1)]
        public string complete_task_hrs_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string default_timesheet_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string future_ts_hrs_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string ev_fix_cost_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string ts_daily_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string notstart_task_hrs_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string prestart_task_hrs_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string postend_task_hrs_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string ermm_enabled_flag { get; set; }

        [Required]
        [StringLength(2)]
        public string name_sep_char { get; set; }

        [Required]
        [StringLength(4)]
        public string year_char { get; set; }

        [Required]
        [StringLength(4)]
        public string month_char { get; set; }

        [Required]
        [StringLength(4)]
        public string week_char { get; set; }

        [Required]
        [StringLength(4)]
        public string day_char { get; set; }

        [Required]
        [StringLength(4)]
        public string hour_char { get; set; }

        [Required]
        [StringLength(4)]
        public string minute_char { get; set; }

        [Required]
        [StringLength(20)]
        public string ts_approval_type { get; set; }

        [Required]
        [StringLength(20)]
        public string db_name { get; set; }

        [Required]
        [StringLength(20)]
        public string tasksum_period_type { get; set; }

        [Required]
        [StringLength(20)]
        public string trsrcsum_period_type { get; set; }

        [Required]
        [StringLength(30)]
        public string database_version { get; set; }

        [Required]
        [StringLength(40)]
        public string phase_label { get; set; }

        public int? ts_approve_user_id { get; set; }

        public int curr_id { get; set; }

        public int? ev_user_pct { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? ev_etc_user_value { get; set; }

        [StringLength(20)]
        public string ev_compute_type { get; set; }

        [StringLength(20)]
        public string ev_etc_compute_type { get; set; }

        [StringLength(255)]
        public string rpt_header_1 { get; set; }

        [StringLength(255)]
        public string rpt_header_2 { get; set; }

        [StringLength(255)]
        public string rpt_header_3 { get; set; }

        [StringLength(255)]
        public string rpt_footer_1 { get; set; }

        [StringLength(255)]
        public string rpt_footer_2 { get; set; }

        [StringLength(255)]
        public string rpt_footer_3 { get; set; }

        [StringLength(255)]
        public string rpt_user_1 { get; set; }

        [StringLength(255)]
        public string rpt_user_2 { get; set; }

        [StringLength(255)]
        public string rpt_user_3 { get; set; }

        [Column(TypeName = "text")]
        public string license_data { get; set; }

        public int max_role_level_cnt { get; set; }

        public int past_ts_cnt { get; set; }

        public int projinit_admin_user_id { get; set; }

        public int? default_rsrc_sec_id { get; set; }

        [Required]
        [StringLength(1)]
        public string allow_user_time_period_flag { get; set; }

        [StringLength(255)]
        public string exp_root_url { get; set; }

        [Required]
        [StringLength(1)]
        public string ts_rsrc_def_asgn_actv_flag { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual CURRTYPE CURRTYPE { get; set; }

        public virtual RSRC RSRC { get; set; }

        public virtual USERS USERS { get; set; }
    }
}