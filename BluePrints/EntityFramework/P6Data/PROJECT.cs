namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("PROJECT")]
    public partial class PROJECT
    {
        public PROJECT()
        {
            ACTVTYPE = new HashSet<ACTVTYPE>();
            BUDGCHNG = new HashSet<BUDGCHNG>();
            CALENDAR = new HashSet<CALENDAR>();
            DOCUMENT = new HashSet<DOCUMENT>();
            EXTAPP = new HashSet<EXTAPP>();
            FORMPROJ = new HashSet<FORMPROJ>();
            ISSUHIST = new HashSet<ISSUHIST>();
            ITERDAYS = new HashSet<ITERDAYS>();
            OBSPROJ = new HashSet<OBSPROJ>();
            PROJCOST = new HashSet<PROJCOST>();
            PROJEST = new HashSet<PROJEST>();
            PROJFUND = new HashSet<PROJFUND>();
            PROJISSU = new HashSet<PROJISSU>();
            PROJPCAT = new HashSet<PROJPCAT>();
            PROJPROP = new HashSet<PROJPROP>();
            PROJSHAR = new HashSet<PROJSHAR>();
            PROJTHRS = new HashSet<PROJTHRS>();
            PROJWBS = new HashSet<PROJWBS>();
            PROJWSRPT = new HashSet<PROJWSRPT>();
            RELEASE = new HashSet<RELEASE>();
            RELITEMS = new HashSet<RELITEMS>();
            RPT = new HashSet<RPT>();
            RPTBATCH = new HashSet<RPTBATCH>();
            SCENPROJ = new HashSet<SCENPROJ>();
            SUMPROJCOST = new HashSet<SUMPROJCOST>();
            SUMTASK = new HashSet<SUMTASK>();
            SUMTASKSPREAD = new HashSet<SUMTASKSPREAD>();
            SUMTRSRC = new HashSet<SUMTRSRC>();
            TASK = new HashSet<TASK>();
            TASKACTV = new HashSet<TASKACTV>();
            TASKDOC = new HashSet<TASKDOC>();
            TASKFDBK = new HashSet<TASKFDBK>();
            TASKFIN = new HashSet<TASKFIN>();
            TASKMEMO = new HashSet<TASKMEMO>();
            TASKNOTE = new HashSet<TASKNOTE>();
            TASKPRED = new HashSet<TASKPRED>();
            TASKPRED1 = new HashSet<TASKPRED>();
            TASKPROC = new HashSet<TASKPROC>();
            TASKRISK = new HashSet<TASKRISK>();
            TASKRSRC = new HashSet<TASKRSRC>();
            TASKUSER = new HashSet<TASKUSER>();
            TASKWKSP = new HashSet<TASKWKSP>();
            TRSRCFIN = new HashSet<TRSRCFIN>();
            TSDELEGATE = new HashSet<TSDELEGATE>();
            UDFVALUE = new HashSet<UDFVALUE>();
            USERWKSP = new HashSet<USERWKSP>();
            VIEWPROP = new HashSet<VIEWPROP>();
            WBSBUDG = new HashSet<WBSBUDG>();
            WBSMEMO = new HashSet<WBSMEMO>();
            WBSRSRC = new HashSet<WBSRSRC>();
            WBSSTEP = new HashSet<WBSSTEP>();
            WORKFLOW = new HashSet<WORKFLOW>();
            WORKSPACE = new HashSet<WORKSPACE>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int proj_id { get; set; }

        public int fy_start_month_num { get; set; }

        [Required]
        [StringLength(1)]
        public string chng_eff_cmp_pct_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string rsrc_self_add_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string allow_complete_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string rsrc_multi_assign_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string checkout_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string project_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string step_complete_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string cost_qty_recalc_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string sum_only_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string batch_sum_flag { get; set; }

        [Required]
        [StringLength(2)]
        public string name_sep_char { get; set; }

        [Required]
        [StringLength(10)]
        public string def_complete_pct_type { get; set; }

        [Required]
        [StringLength(40)]
        public string proj_short_name { get; set; }

        public int? acct_id { get; set; }

        public int? orig_proj_id { get; set; }

        public int? source_proj_id { get; set; }

        public int? base_type_id { get; set; }

        public int? clndr_id { get; set; }

        public int? sum_base_proj_id { get; set; }

        public int? task_code_base { get; set; }

        public int? task_code_step { get; set; }

        public int? priority_num { get; set; }

        public int? wbs_max_sum_level { get; set; }

        public int? risk_level { get; set; }

        public int? strgy_priority_num { get; set; }

        public int? last_checksum { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? critical_drtn_hr_cnt { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? def_cost_per_qty { get; set; }

        public DateTime? last_recalc_date { get; set; }

        public DateTime? plan_start_date { get; set; }

        public DateTime? plan_end_date { get; set; }

        public DateTime? scd_end_date { get; set; }

        public DateTime add_date { get; set; }

        public DateTime? sum_data_date { get; set; }

        public DateTime? last_tasksum_date { get; set; }

        public DateTime? fcst_start_date { get; set; }

        [StringLength(12)]
        public string def_duration_type { get; set; }

        [StringLength(20)]
        public string task_code_prefix { get; set; }

        [StringLength(22)]
        public string guid { get; set; }

        [StringLength(24)]
        public string def_qty_type { get; set; }

        [StringLength(255)]
        public string add_by_name { get; set; }

        [StringLength(120)]
        public string web_local_root_path { get; set; }

        [StringLength(200)]
        public string proj_url { get; set; }

        [StringLength(14)]
        public string def_rate_type { get; set; }

        [Required]
        [StringLength(1)]
        public string act_this_per_link_flag { get; set; }

        [Required]
        [StringLength(12)]
        public string def_task_type { get; set; }

        [Required]
        [StringLength(1)]
        public string act_pct_link_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string add_act_remain_flag { get; set; }

        [Required]
        [StringLength(12)]
        public string critical_path_type { get; set; }

        [Required]
        [StringLength(1)]
        public string task_code_prefix_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string def_rollup_dates_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string rem_target_link_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string reset_planned_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string allow_neg_act_flag { get; set; }

        public int? rsrc_id { get; set; }

        [Required]
        [StringLength(1)]
        public string msp_managed_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string msp_update_actuals_flag { get; set; }

        public DateTime? checkout_date { get; set; }

        public int? checkout_user_id { get; set; }

        [StringLength(12)]
        public string sum_assign_level { get; set; }

        public int? last_fin_dates_id { get; set; }

        [Required]
        [StringLength(1)]
        public string use_project_baseline_flag { get; set; }

        public DateTime? last_baseline_update_date { get; set; }

        [Required]
        [StringLength(1)]
        public string ts_rsrc_vw_compl_asgn_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string ts_rsrc_mark_act_finish_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string ts_rsrc_vw_inact_actv_flag { get; set; }

        [StringLength(4000)]
        public string cr_external_key { get; set; }

        public DateTime? apply_actuals_date { get; set; }

        [StringLength(500)]
        public string description { get; set; }

        [StringLength(7)]
        public string intg_proj_type { get; set; }

        public int? matrix_id { get; set; }

        public int? location_id { get; set; }

        [Required]
        [StringLength(1)]
        public string control_updates_flag { get; set; }

        [Required]
        [StringLength(25)]
        public string hist_interval { get; set; }

        [Required]
        [StringLength(10)]
        public string hist_level { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ACCOUNT ACCOUNT { get; set; }

        public virtual ICollection<ACTVTYPE> ACTVTYPE { get; set; }

        public virtual BASETYPE BASETYPE { get; set; }

        public virtual ICollection<BUDGCHNG> BUDGCHNG { get; set; }

        public virtual ICollection<CALENDAR> CALENDAR { get; set; }

        public virtual ICollection<DOCUMENT> DOCUMENT { get; set; }

        public virtual EXPPROJ EXPPROJ { get; set; }

        public virtual ICollection<EXTAPP> EXTAPP { get; set; }

        public virtual FINDATES FINDATES { get; set; }

        public virtual ICollection<FORMPROJ> FORMPROJ { get; set; }

        public virtual ICollection<ISSUHIST> ISSUHIST { get; set; }

        public virtual ICollection<ITERDAYS> ITERDAYS { get; set; }

        public virtual ICollection<OBSPROJ> OBSPROJ { get; set; }

        public virtual ICollection<PROJCOST> PROJCOST { get; set; }

        public virtual USERS USERS { get; set; }

        public virtual ICollection<PROJEST> PROJEST { get; set; }

        public virtual ICollection<PROJFUND> PROJFUND { get; set; }

        public virtual ICollection<PROJISSU> PROJISSU { get; set; }

        public virtual ICollection<PROJPCAT> PROJPCAT { get; set; }

        public virtual ICollection<PROJPROP> PROJPROP { get; set; }

        public virtual ICollection<PROJSHAR> PROJSHAR { get; set; }

        public virtual ICollection<PROJTHRS> PROJTHRS { get; set; }

        public virtual ICollection<PROJWBS> PROJWBS { get; set; }

        public virtual ICollection<PROJWSRPT> PROJWSRPT { get; set; }

        public virtual ICollection<RELEASE> RELEASE { get; set; }

        public virtual ICollection<RELITEMS> RELITEMS { get; set; }

        public virtual ICollection<RPT> RPT { get; set; }

        public virtual ICollection<RPTBATCH> RPTBATCH { get; set; }

        public virtual ICollection<SCENPROJ> SCENPROJ { get; set; }

        public virtual ICollection<SUMPROJCOST> SUMPROJCOST { get; set; }

        public virtual ICollection<SUMTASK> SUMTASK { get; set; }

        public virtual ICollection<SUMTASKSPREAD> SUMTASKSPREAD { get; set; }

        public virtual ICollection<SUMTRSRC> SUMTRSRC { get; set; }

        public virtual ICollection<TASK> TASK { get; set; }

        public virtual ICollection<TASKACTV> TASKACTV { get; set; }

        public virtual ICollection<TASKDOC> TASKDOC { get; set; }

        public virtual ICollection<TASKFDBK> TASKFDBK { get; set; }

        public virtual ICollection<TASKFIN> TASKFIN { get; set; }

        public virtual ICollection<TASKMEMO> TASKMEMO { get; set; }

        public virtual ICollection<TASKNOTE> TASKNOTE { get; set; }

        public virtual ICollection<TASKPRED> TASKPRED { get; set; }

        public virtual ICollection<TASKPRED> TASKPRED1 { get; set; }

        public virtual ICollection<TASKPROC> TASKPROC { get; set; }

        public virtual ICollection<TASKRISK> TASKRISK { get; set; }

        public virtual ICollection<TASKRSRC> TASKRSRC { get; set; }

        public virtual ICollection<TASKUSER> TASKUSER { get; set; }

        public virtual ICollection<TASKWKSP> TASKWKSP { get; set; }

        public virtual ICollection<TRSRCFIN> TRSRCFIN { get; set; }

        public virtual ICollection<TSDELEGATE> TSDELEGATE { get; set; }

        public virtual ICollection<UDFVALUE> UDFVALUE { get; set; }

        public virtual ICollection<USERWKSP> USERWKSP { get; set; }

        public virtual ICollection<VIEWPROP> VIEWPROP { get; set; }

        public virtual ICollection<WBSBUDG> WBSBUDG { get; set; }

        public virtual ICollection<WBSMEMO> WBSMEMO { get; set; }

        public virtual ICollection<WBSRSRC> WBSRSRC { get; set; }

        public virtual ICollection<WBSSTEP> WBSSTEP { get; set; }

        public virtual ICollection<WORKFLOW> WORKFLOW { get; set; }

        public virtual ICollection<WORKSPACE> WORKSPACE { get; set; }
    }
}