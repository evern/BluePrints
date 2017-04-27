namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("USESSION")]
    public partial class USESSION
    {
        public USESSION()
        {
            HQDATA = new HashSet<HQDATA>();
            HQUERY = new HashSet<HQUERY>();
            PROJSHAR = new HashSet<PROJSHAR>();
            UPKLIST = new HashSet<UPKLIST>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int session_id { get; set; }

        public DateTime login_time { get; set; }

        public DateTime last_active_time { get; set; }

        [Required]
        [StringLength(50)]
        public string host_name { get; set; }

        public int? user_id { get; set; }

        public int? process_num { get; set; }

        [StringLength(25)]
        public string app_name { get; set; }

        [StringLength(40)]
        public string hard_drive_code { get; set; }

        [StringLength(20)]
        public string db_engine_type { get; set; }

        [StringLength(255)]
        public string os_user_name { get; set; }

        [StringLength(22)]
        public string vdb_instance_guid { get; set; }

        public int? spid { get; set; }

        [StringLength(255)]
        public string operation_name { get; set; }

        public DateTime? operation_start_date { get; set; }

        [StringLength(1)]
        public string long_operation_flag { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ICollection<HQDATA> HQDATA { get; set; }

        public virtual ICollection<HQUERY> HQUERY { get; set; }

        public virtual ICollection<PROJSHAR> PROJSHAR { get; set; }

        public virtual USERS USERS { get; set; }

        public virtual ICollection<UPKLIST> UPKLIST { get; set; }
    }
}