namespace BluePrints.P6Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("PROJISSU")]
    public partial class PROJISSU
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int issue_id { get; set; }

        public int proj_id { get; set; }

        public int obs_id { get; set; }

        [Required]
        [StringLength(12)]
        public string priority_type { get; set; }

        [Required]
        [StringLength(12)]
        public string status_code { get; set; }

        [Required]
        [StringLength(100)]
        public string issue_name { get; set; }

        public int? thresh_id { get; set; }

        public int? track_view_id { get; set; }

        public int? wbs_id { get; set; }

        public int? task_id { get; set; }

        public int? rsrc_id { get; set; }

        public int? thresh_parm_id { get; set; }

        public int? base_proj_id { get; set; }

        public int? workspace_id { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? issue_value { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? lo_parm_value { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? hi_parm_value { get; set; }

        [StringLength(255)]
        public string add_by_name { get; set; }

        public DateTime? resolv_date { get; set; }

        public DateTime? add_date { get; set; }

        [Column(TypeName = "text")]
        public string issue_notes { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ISSUHIST ISSUHIST { get; set; }

        public virtual OBS OBS { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        public virtual PROJTHRS PROJTHRS { get; set; }

        public virtual PROJWBS PROJWBS { get; set; }

        public virtual RSRC RSRC { get; set; }

        public virtual TASK TASK { get; set; }

        public virtual THRSPARM THRSPARM { get; set; }

        public virtual TRAKVIEW TRAKVIEW { get; set; }

        public virtual WORKSPACE WORKSPACE { get; set; }
    }
}