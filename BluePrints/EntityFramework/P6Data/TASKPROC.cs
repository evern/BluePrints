namespace BluePrints.P6Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("TASKPROC")]
    public partial class TASKPROC
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int proc_id { get; set; }

        public int task_id { get; set; }

        public int seq_num { get; set; }

        public int proj_id { get; set; }

        [Required]
        [StringLength(1)]
        public string complete_flag { get; set; }

        [Required]
        [StringLength(120)]
        public string proc_name { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? proc_wt { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? complete_pct { get; set; }

        [Column(TypeName = "text")]
        public string proc_descr { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        public virtual TASK TASK { get; set; }
    }
}