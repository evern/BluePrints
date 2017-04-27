namespace BluePrints.P6Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("RSRCHOUR")]
    public partial class RSRCHOUR
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int rsrc_hr_id { get; set; }

        public int rsrc_id { get; set; }

        public int ts_id { get; set; }

        [Required]
        [StringLength(1)]
        public string task_ts_flag { get; set; }

        public int? taskrsrc_id { get; set; }

        public int? nonwork_type_id { get; set; }

        public int? proj_id { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? pend_hr_cnt { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? hr_cnt { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? pend_ot_hr_cnt { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? ot_hr_cnt { get; set; }

        public DateTime? work_date { get; set; }

        [StringLength(20)]
        public string status_code { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual NONWORK NONWORK { get; set; }

        public virtual RSRC RSRC { get; set; }

        public virtual TASKRSRC TASKRSRC { get; set; }

        public virtual TIMESHT TIMESHT { get; set; }
    }
}