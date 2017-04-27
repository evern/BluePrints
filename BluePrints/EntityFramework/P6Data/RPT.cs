namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("RPT")]
    public partial class RPT
    {
        public RPT()
        {
            FILTPROP = new HashSet<FILTPROP>();
            JOBRPT = new HashSet<JOBRPT>();
            PROJWSRPT = new HashSet<PROJWSRPT>();
            RPTLIST = new HashSet<RPTLIST>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int rpt_id { get; set; }

        [Required]
        [StringLength(1)]
        public string global_flag { get; set; }

        [Required]
        [StringLength(12)]
        public string rpt_type { get; set; }

        [Required]
        [StringLength(80)]
        public string rpt_name { get; set; }

        [Required]
        [StringLength(32)]
        public string rpt_area { get; set; }

        public int? rpt_group_id { get; set; }

        public int? proj_id { get; set; }

        [StringLength(10)]
        public string rpt_state { get; set; }

        [Column(TypeName = "text")]
        public string rpt_data { get; set; }

        public DateTime? last_run_date { get; set; }

        public int? user_id { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ICollection<FILTPROP> FILTPROP { get; set; }

        public virtual ICollection<JOBRPT> JOBRPT { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        public virtual ICollection<PROJWSRPT> PROJWSRPT { get; set; }

        public virtual RPTGROUP RPTGROUP { get; set; }

        public virtual USERS USERS { get; set; }

        public virtual ICollection<RPTLIST> RPTLIST { get; set; }
    }
}