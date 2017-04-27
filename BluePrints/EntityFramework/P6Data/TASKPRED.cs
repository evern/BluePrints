namespace BluePrints.P6Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("TASKPRED")]
    public partial class TASKPRED
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int task_pred_id { get; set; }

        public int task_id { get; set; }

        public int pred_task_id { get; set; }

        public int proj_id { get; set; }

        public int pred_proj_id { get; set; }

        [Required]
        [StringLength(12)]
        public string pred_type { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? lag_hr_cnt { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        public virtual PROJECT PROJECT1 { get; set; }

        public virtual TASK TASK { get; set; }

        public virtual TASK TASK1 { get; set; }
    }
}