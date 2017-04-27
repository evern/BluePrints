namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("TASK")]
    public partial class TASK
    {
        public TASK()
        {
            DISCUSSION = new HashSet<DISCUSSION>();
            ITERDAYS = new HashSet<ITERDAYS>();
            PROJCOST = new HashSet<PROJCOST>();
            PROJISSU = new HashSet<PROJISSU>();
            TASKRISK = new HashSet<TASKRISK>();
            TASKACTV = new HashSet<TASKACTV>();
            TASKDOC = new HashSet<TASKDOC>();
            TASKFIN = new HashSet<TASKFIN>();
            TASKMEMO = new HashSet<TASKMEMO>();
            TASKPRED = new HashSet<TASKPRED>();
            TASKPRED1 = new HashSet<TASKPRED>();
            TASKPROC = new HashSet<TASKPROC>();
            TASKRSRC = new HashSet<TASKRSRC>();
            TASKUSER = new HashSet<TASKUSER>();
            TASKWKSP = new HashSet<TASKWKSP>();
            TRSRCFIN = new HashSet<TRSRCFIN>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int task_id { get; set; }

        public int proj_id { get; set; }

        public int wbs_id { get; set; }

        public int clndr_id { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? est_wt { get; set; }

        [Column(TypeName = "numeric")]
        public decimal phys_complete_pct { get; set; }

        [Required]
        [StringLength(1)]
        public string rev_fdbk_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string lock_plan_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string auto_compute_act_flag { get; set; }

        [Required]
        [StringLength(10)]
        public string complete_pct_type { get; set; }

        [Required]
        [StringLength(10)]
        public string task_type { get; set; }

        [Required]
        [StringLength(12)]
        public string duration_type { get; set; }

        [Required]
        [StringLength(12)]
        public string review_type { get; set; }

        [Required]
        [StringLength(12)]
        public string status_code { get; set; }

        [Required]
        [StringLength(40)]
        public string task_code { get; set; }

        [Required]
        [StringLength(120)]
        public string task_name { get; set; }

        public int? rsrc_id { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? total_float_hr_cnt { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? free_float_hr_cnt { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? remain_drtn_hr_cnt { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_work_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? remain_work_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? target_work_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? target_drtn_hr_cnt { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? target_equip_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_equip_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? remain_equip_qty { get; set; }

        public DateTime? cstr_date { get; set; }

        public DateTime? act_start_date { get; set; }

        public DateTime? act_end_date { get; set; }

        public DateTime? late_start_date { get; set; }

        public DateTime? late_end_date { get; set; }

        public DateTime? expect_end_date { get; set; }

        public DateTime? early_start_date { get; set; }

        public DateTime? early_end_date { get; set; }

        public DateTime? restart_date { get; set; }

        public DateTime? reend_date { get; set; }

        public DateTime? target_start_date { get; set; }

        public DateTime? target_end_date { get; set; }

        public DateTime? review_end_date { get; set; }

        public DateTime? rem_late_start_date { get; set; }

        public DateTime? rem_late_end_date { get; set; }

        [StringLength(12)]
        public string cstr_type { get; set; }

        [StringLength(12)]
        public string priority_type { get; set; }

        [StringLength(22)]
        public string guid { get; set; }

        [StringLength(22)]
        public string tmpl_guid { get; set; }

        public DateTime? cstr_date2 { get; set; }

        [StringLength(12)]
        public string cstr_type2 { get; set; }

        public int? float_path { get; set; }

        public int? float_path_order { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_this_per_work_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_this_per_equip_qty { get; set; }

        [Required]
        [StringLength(1)]
        public string driving_path_flag { get; set; }

        public DateTime? suspend_date { get; set; }

        public DateTime? resume_date { get; set; }

        public DateTime? external_early_start_date { get; set; }

        public DateTime? external_late_end_date { get; set; }

        public int? location_id { get; set; }

        [StringLength(1)]
        public string control_updates_flag { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual CALENDAR CALENDAR { get; set; }

        public virtual ICollection<DISCUSSION> DISCUSSION { get; set; }

        public virtual ICollection<ITERDAYS> ITERDAYS { get; set; }

        public virtual ICollection<PROJCOST> PROJCOST { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        public virtual ICollection<PROJISSU> PROJISSU { get; set; }

        public virtual PROJWBS PROJWBS { get; set; }

        public virtual RSRC RSRC { get; set; }

        public virtual ICollection<TASKRISK> TASKRISK { get; set; }

        public virtual ICollection<TASKACTV> TASKACTV { get; set; }

        public virtual ICollection<TASKDOC> TASKDOC { get; set; }

        public virtual TASKFDBK TASKFDBK { get; set; }

        public virtual ICollection<TASKFIN> TASKFIN { get; set; }

        public virtual ICollection<TASKMEMO> TASKMEMO { get; set; }

        public virtual TASKNOTE TASKNOTE { get; set; }

        public virtual ICollection<TASKPRED> TASKPRED { get; set; }

        public virtual ICollection<TASKPRED> TASKPRED1 { get; set; }

        public virtual ICollection<TASKPROC> TASKPROC { get; set; }

        public virtual ICollection<TASKRSRC> TASKRSRC { get; set; }

        public virtual ICollection<TASKUSER> TASKUSER { get; set; }

        public virtual ICollection<TASKWKSP> TASKWKSP { get; set; }

        public virtual ICollection<TRSRCFIN> TRSRCFIN { get; set; }
    }
}