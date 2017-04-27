namespace BluePrints.P6Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("TRSRCFIN")]
    public partial class TRSRCFIN
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int fin_dates_id { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int taskrsrc_id { get; set; }

        public int task_id { get; set; }

        public int proj_id { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_cost { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual FINDATES FINDATES { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        public virtual TASK TASK { get; set; }

        public virtual TASKRSRC TASKRSRC { get; set; }
    }
}