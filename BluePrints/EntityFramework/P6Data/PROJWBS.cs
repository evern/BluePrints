namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PROJWBS
    {
        public PROJWBS()
        {
            BUDGCHNG = new HashSet<BUDGCHNG>();
            PROJEST = new HashSet<PROJEST>();
            PROJISSU = new HashSet<PROJISSU>();
            PROJTHRS = new HashSet<PROJTHRS>();
            PRPFOLIO = new HashSet<PRPFOLIO>();
            SUMPROJCOST = new HashSet<SUMPROJCOST>();
            SUMTASK = new HashSet<SUMTASK>();
            SUMTASKSPREAD = new HashSet<SUMTASKSPREAD>();
            SUMTRSRC = new HashSet<SUMTRSRC>();
            TASK = new HashSet<TASK>();
            TASKDOC = new HashSet<TASKDOC>();
            WBSBUDG = new HashSet<WBSBUDG>();
            WBSMEMO = new HashSet<WBSMEMO>();
            WBSRSRC = new HashSet<WBSRSRC>();
            WBSSTEP = new HashSet<WBSSTEP>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int wbs_id { get; set; }

        public int proj_id { get; set; }

        public int obs_id { get; set; }

        public int seq_num { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? est_wt { get; set; }

        [Required]
        [StringLength(1)]
        public string proj_node_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string sum_data_flag { get; set; }

        [Required]
        [StringLength(20)]
        public string status_code { get; set; }

        [Required]
        [StringLength(40)]
        public string wbs_short_name { get; set; }

        [Required]
        [StringLength(100)]
        public string wbs_name { get; set; }

        public int? phase_id { get; set; }

        public int? parent_wbs_id { get; set; }

        public int? ev_user_pct { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? ev_etc_user_value { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? orig_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? indep_remain_total_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? ann_dscnt_rate_pct { get; set; }

        [StringLength(20)]
        public string dscnt_period_type { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? indep_remain_work_qty { get; set; }

        public DateTime? anticip_start_date { get; set; }

        public DateTime? anticip_end_date { get; set; }

        [StringLength(20)]
        public string ev_compute_type { get; set; }

        [StringLength(20)]
        public string ev_etc_compute_type { get; set; }

        public int? resp_team_id { get; set; }

        public int? iteration_id { get; set; }

        [StringLength(22)]
        public string guid { get; set; }

        [StringLength(22)]
        public string tmpl_guid { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? original_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? rqmt_rem_qty { get; set; }

        [StringLength(7)]
        public string intg_type { get; set; }

        public int? status_reviewer { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ICollection<BUDGCHNG> BUDGCHNG { get; set; }

        public virtual ITERATION ITERATION { get; set; }

        public virtual OBS OBS { get; set; }

        public virtual PHASE PHASE { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        public virtual ICollection<PROJEST> PROJEST { get; set; }

        public virtual ICollection<PROJISSU> PROJISSU { get; set; }

        public virtual ICollection<PROJTHRS> PROJTHRS { get; set; }

        public virtual RFOLIO RFOLIO { get; set; }

        public virtual ICollection<PRPFOLIO> PRPFOLIO { get; set; }

        public virtual ICollection<SUMPROJCOST> SUMPROJCOST { get; set; }

        public virtual ICollection<SUMTASK> SUMTASK { get; set; }

        public virtual ICollection<SUMTASKSPREAD> SUMTASKSPREAD { get; set; }

        public virtual ICollection<SUMTRSRC> SUMTRSRC { get; set; }

        public virtual ICollection<TASK> TASK { get; set; }

        public virtual ICollection<TASKDOC> TASKDOC { get; set; }

        public virtual ICollection<WBSBUDG> WBSBUDG { get; set; }

        public virtual ICollection<WBSMEMO> WBSMEMO { get; set; }

        public virtual ICollection<WBSRSRC> WBSRSRC { get; set; }

        public virtual ICollection<WBSSTEP> WBSSTEP { get; set; }
    }
}
