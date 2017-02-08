namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("JOBSVC")]
    public partial class JOBSVC
    {
        public JOBSVC()
        {
            JOBRPT = new HashSet<JOBRPT>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int job_id { get; set; }

        public int? parent_job_id { get; set; }

        public int seq_num { get; set; }

        [Required]
        [StringLength(1)]
        public string audit_flag { get; set; }

        [Required]
        [StringLength(20)]
        public string job_type { get; set; }

        [Required]
        [StringLength(100)]
        public string job_name { get; set; }

        public int user_id { get; set; }

        public DateTime? last_run_date { get; set; }

        [Required]
        [StringLength(20)]
        public string status_code { get; set; }

        [StringLength(255)]
        public string recur_data { get; set; }

        [StringLength(20)]
        public string recur_type { get; set; }

        public DateTime? submitted_date { get; set; }

        [StringLength(255)]
        public string last_error_descr { get; set; }

        [StringLength(255)]
        public string audit_file_path { get; set; }

        [Column(TypeName = "text")]
        public string job_data { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual JOBLOG JOBLOG { get; set; }

        public virtual ICollection<JOBRPT> JOBRPT { get; set; }

        public virtual USERS USERS { get; set; }
    }
}