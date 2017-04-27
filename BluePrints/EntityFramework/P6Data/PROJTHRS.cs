namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class PROJTHRS
    {
        public PROJTHRS()
        {
            PROJISSU = new HashSet<PROJISSU>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int thresh_id { get; set; }

        public int proj_id { get; set; }

        public int obs_id { get; set; }

        public int thresh_parm_id { get; set; }

        [Required]
        [StringLength(12)]
        public string status_code { get; set; }

        [Required]
        [StringLength(12)]
        public string priority_type { get; set; }

        [Required]
        [StringLength(12)]
        public string thresh_type { get; set; }

        public int? wbs_id { get; set; }

        public int? track_view_id { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? lo_parm_value { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? hi_parm_value { get; set; }

        [StringLength(50)]
        public string window_start { get; set; }

        [StringLength(50)]
        public string window_end { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual OBS OBS { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        public virtual ICollection<PROJISSU> PROJISSU { get; set; }

        public virtual PROJWBS PROJWBS { get; set; }

        public virtual THRSPARM THRSPARM { get; set; }

        public virtual TRAKVIEW TRAKVIEW { get; set; }
    }
}