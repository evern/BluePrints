namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("RSRC")]
    public partial class RSRC
    {
        public RSRC()
        {
            DOCUMENT = new HashSet<DOCUMENT>();
            PREFER = new HashSet<PREFER>();
            PROJEST = new HashSet<PROJEST>();
            PROJISSU = new HashSet<PROJISSU>();
            RSRCHOUR = new HashSet<RSRCHOUR>();
            RSRCPROP = new HashSet<RSRCPROP>();
            RSRCRATE = new HashSet<RSRCRATE>();
            RSRCRCAT = new HashSet<RSRCRCAT>();
            RSRCROLE = new HashSet<RSRCROLE>();
            RSRCSEC = new HashSet<RSRCSEC>();
            RSRFOLIO = new HashSet<RSRFOLIO>();
            TASK = new HashSet<TASK>();
            TASKRSRC = new HashSet<TASKRSRC>();
            WBSRSRC = new HashSet<WBSRSRC>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int rsrc_id { get; set; }

        public int clndr_id { get; set; }

        public int rsrc_seq_num { get; set; }

        [Required]
        [StringLength(1)]
        public string timesheet_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string active_flag { get; set; }

        [Required]
        [StringLength(10)]
        public string rsrc_type { get; set; }

        [Required]
        [StringLength(1)]
        public string auto_compute_act_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string ot_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string def_cost_qty_link_flag { get; set; }

        [Required]
        [StringLength(255)]
        public string rsrc_short_name { get; set; }

        [Required]
        [StringLength(255)]
        public string rsrc_name { get; set; }

        public int? parent_rsrc_id { get; set; }

        public int? xfer_complete_day_cnt { get; set; }

        public int? xfer_notstart_day_cnt { get; set; }

        public int? ts_approve_user_id { get; set; }

        public int? user_id { get; set; }

        public int? role_id { get; set; }

        public int curr_id { get; set; }

        public int? last_checksum { get; set; }

        public int? shift_id { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? ot_factor { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? def_qty_per_hr { get; set; }

        [StringLength(22)]
        public string guid { get; set; }

        [StringLength(24)]
        public string cost_qty_type { get; set; }

        [StringLength(32)]
        public string office_phone { get; set; }

        [StringLength(32)]
        public string other_phone { get; set; }

        [StringLength(40)]
        public string employee_code { get; set; }

        [StringLength(100)]
        public string rsrc_title_name { get; set; }

        [StringLength(120)]
        public string email_addr { get; set; }

        public int? unit_id { get; set; }

        public int? pobs_id { get; set; }

        [Column(TypeName = "text")]
        public string rsrc_notes { get; set; }

        [StringLength(7)]
        public string intg_type { get; set; }

        public DateTime? update_date { get; set; }

        public int? location_id { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual CALENDAR CALENDAR { get; set; }

        public virtual CURRTYPE CURRTYPE { get; set; }

        public virtual ICollection<DOCUMENT> DOCUMENT { get; set; }

        public virtual POBS POBS { get; set; }

        public virtual ICollection<PREFER> PREFER { get; set; }

        public virtual ICollection<PROJEST> PROJEST { get; set; }

        public virtual ICollection<PROJISSU> PROJISSU { get; set; }

        public virtual ROLES ROLES { get; set; }

        public virtual SHIFT SHIFT { get; set; }

        public virtual UMEASURE UMEASURE { get; set; }

        public virtual USERS USERS { get; set; }

        public virtual ICollection<RSRCHOUR> RSRCHOUR { get; set; }

        public virtual ICollection<RSRCPROP> RSRCPROP { get; set; }

        public virtual ICollection<RSRCRATE> RSRCRATE { get; set; }

        public virtual ICollection<RSRCRCAT> RSRCRCAT { get; set; }

        public virtual ICollection<RSRCROLE> RSRCROLE { get; set; }

        public virtual ICollection<RSRCSEC> RSRCSEC { get; set; }

        public virtual ICollection<RSRFOLIO> RSRFOLIO { get; set; }

        public virtual ICollection<TASK> TASK { get; set; }

        public virtual ICollection<TASKRSRC> TASKRSRC { get; set; }

        public virtual ICollection<WBSRSRC> WBSRSRC { get; set; }
    }
}