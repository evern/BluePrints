namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class USERS
    {
        public USERS()
        {
            DASHBOARD = new HashSet<DASHBOARD>();
            DASHUSER = new HashSet<DASHUSER>();
            DISCUSSION_READ = new HashSet<DISCUSSION_READ>();
            DOCREVIEW = new HashSet<DOCREVIEW>();
            DOCREVIEWTASK = new HashSet<DOCREVIEWTASK>();
            FILTPROP = new HashSet<FILTPROP>();
            GCHANGE = new HashSet<GCHANGE>();
            JOBSVC = new HashSet<JOBSVC>();
            PCATUSER = new HashSet<PCATUSER>();
            PFOLIO = new HashSet<PFOLIO>();
            PREFER = new HashSet<PREFER>();
            PROJECT = new HashSet<PROJECT>();
            RFOLIO = new HashSet<RFOLIO>();
            RLFOLIO = new HashSet<RLFOLIO>();
            RPT = new HashSet<RPT>();
            RSRC = new HashSet<RSRC>();
            RSRCANVIEW = new HashSet<RSRCANVIEW>();
            RSRCSEC = new HashSet<RSRCSEC>();
            SCENARIO = new HashSet<SCENARIO>();
            SCENUSER = new HashSet<SCENUSER>();
            TASKUSER = new HashSet<TASKUSER>();
            TRAKVIEW = new HashSet<TRAKVIEW>();
            TSDELEGATE = new HashSet<TSDELEGATE>();
            TSDELEGATE1 = new HashSet<TSDELEGATE>();
            UACCESS = new HashSet<UACCESS>();
            UEVNTREG = new HashSet<UEVNTREG>();
            USERDATA = new HashSet<USERDATA>();
            USERENG = new HashSet<USERENG>();
            USEROBS = new HashSet<USEROBS>();
            USEROPEN = new HashSet<USEROPEN>();
            USERSET = new HashSet<USERSET>();
            USERWKSP = new HashSet<USERWKSP>();
            USESSION = new HashSet<USESSION>();
            VIEWPREF = new HashSet<VIEWPREF>();
            VIEWPROP = new HashSet<VIEWPROP>();
            VWPREFUSER = new HashSet<VWPREFUSER>();
            WORKFLOW = new HashSet<WORKFLOW>();
            WKFLUSER = new HashSet<WKFLUSER>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int user_id { get; set; }

        [Required]
        [StringLength(1)]
        public string global_flag { get; set; }

        [Required]
        [StringLength(16)]
        public string email_type { get; set; }

        [Required]
        [StringLength(255)]
        public string user_name { get; set; }

        public int? prof_id { get; set; }

        public int curr_id { get; set; }

        [Required]
        [StringLength(1)]
        public string all_rsrc_access_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string report_user_flag { get; set; }

        [StringLength(22)]
        public string guid { get; set; }

        [StringLength(32)]
        public string email_srv_user_name { get; set; }

        [StringLength(32)]
        public string office_phone { get; set; }

        [StringLength(255)]
        public string actual_name { get; set; }

        [StringLength(120)]
        public string email_send_server { get; set; }

        [StringLength(120)]
        public string email_addr { get; set; }

        [StringLength(255)]
        public string email_srv_passwd { get; set; }

        [StringLength(255)]
        public string passwd { get; set; }

        [StringLength(255)]
        public string notify_prefs { get; set; }

        public int? navi_view_id { get; set; }

        [StringLength(1)]
        public string override_naviview_flag { get; set; }

        public int? ui_view_pref_id { get; set; }

        [StringLength(4000)]
        public string cr_external_key { get; set; }

        [StringLength(255)]
        public string cr_user_name { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public int? failed_login_attempts_cnt { get; set; }

        public virtual CURRTYPE CURRTYPE { get; set; }

        public virtual ICollection<DASHBOARD> DASHBOARD { get; set; }

        public virtual ICollection<DASHUSER> DASHUSER { get; set; }

        public virtual ICollection<DISCUSSION_READ> DISCUSSION_READ { get; set; }

        public virtual ICollection<DOCREVIEW> DOCREVIEW { get; set; }

        public virtual ICollection<DOCREVIEWTASK> DOCREVIEWTASK { get; set; }

        public virtual ICollection<FILTPROP> FILTPROP { get; set; }

        public virtual ICollection<GCHANGE> GCHANGE { get; set; }

        public virtual ICollection<JOBSVC> JOBSVC { get; set; }

        public virtual ICollection<PCATUSER> PCATUSER { get; set; }

        public virtual ICollection<PFOLIO> PFOLIO { get; set; }

        public virtual ICollection<PREFER> PREFER { get; set; }

        public virtual PROFILE PROFILE { get; set; }

        public virtual ICollection<PROJECT> PROJECT { get; set; }

        public virtual ICollection<RFOLIO> RFOLIO { get; set; }

        public virtual ICollection<RLFOLIO> RLFOLIO { get; set; }

        public virtual ICollection<RPT> RPT { get; set; }

        public virtual ICollection<RSRC> RSRC { get; set; }

        public virtual ICollection<RSRCANVIEW> RSRCANVIEW { get; set; }

        public virtual ICollection<RSRCSEC> RSRCSEC { get; set; }

        public virtual ICollection<SCENARIO> SCENARIO { get; set; }

        public virtual ICollection<SCENUSER> SCENUSER { get; set; }

        public virtual ICollection<TASKUSER> TASKUSER { get; set; }

        public virtual ICollection<TRAKVIEW> TRAKVIEW { get; set; }

        public virtual ICollection<TSDELEGATE> TSDELEGATE { get; set; }

        public virtual ICollection<TSDELEGATE> TSDELEGATE1 { get; set; }

        public virtual ICollection<UACCESS> UACCESS { get; set; }

        public virtual ICollection<UEVNTREG> UEVNTREG { get; set; }

        public virtual ICollection<USERDATA> USERDATA { get; set; }

        public virtual ICollection<USERENG> USERENG { get; set; }

        public virtual ICollection<USEROBS> USEROBS { get; set; }

        public virtual ICollection<USEROPEN> USEROPEN { get; set; }

        public virtual ICollection<USERSET> USERSET { get; set; }

        public virtual ICollection<USERWKSP> USERWKSP { get; set; }

        public virtual ICollection<USESSION> USESSION { get; set; }

        public virtual ICollection<VIEWPREF> VIEWPREF { get; set; }

        public virtual ICollection<VIEWPROP> VIEWPROP { get; set; }

        public virtual ICollection<VWPREFUSER> VWPREFUSER { get; set; }

        public virtual ICollection<WORKFLOW> WORKFLOW { get; set; }

        public virtual ICollection<WKFLUSER> WKFLUSER { get; set; }
    }
}