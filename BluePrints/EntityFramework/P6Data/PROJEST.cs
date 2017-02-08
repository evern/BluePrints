namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PROJEST")]
    public partial class PROJEST
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int proj_est_id { get; set; }

        public int proj_id { get; set; }

        public int wbs_id { get; set; }

        [Required]
        [StringLength(1)]
        public string applied_flag { get; set; }

        [Required]
        [StringLength(10)]
        public string rsrc_type { get; set; }

        [Required]
        [StringLength(20)]
        public string est_type { get; set; }

        [Required]
        [StringLength(120)]
        public string est_name { get; set; }

        public int? rsrc_id { get; set; }

        public int? bu_cmplx_value { get; set; }

        public int? adj_mult_pct { get; set; }

        public int? fp_cnt { get; set; }

        public int? fp_cmplx_value { get; set; }

        public int? fp_unadj_cnt { get; set; }

        public int? est_task_cnt { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? fp_prod_avg_value { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? est_qty { get; set; }

        public DateTime? est_date { get; set; }

        [Column(TypeName = "text")]
        public string est_notes { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        public virtual PROJWBS PROJWBS { get; set; }

        public virtual RSRC RSRC { get; set; }
    }
}