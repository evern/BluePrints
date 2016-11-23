namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("CALENDAR")]
    public partial class CALENDAR
    {
        public CALENDAR()
        {
            RSRC = new HashSet<RSRC>();
            TASK = new HashSet<TASK>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int clndr_id { get; set; }

        [Required]
        [StringLength(1)]
        public string default_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string rsrc_private { get; set; }

        [Required]
        [StringLength(255)]
        public string clndr_name { get; set; }

        public int? proj_id { get; set; }

        public int? base_clndr_id { get; set; }

        public DateTime? last_chng_date { get; set; }

        [StringLength(16)]
        public string clndr_type { get; set; }

        [Column(TypeName = "text")]
        public string clndr_data { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? day_hr_cnt { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? week_hr_cnt { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? year_hr_cnt { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? month_hr_cnt { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        public virtual ICollection<RSRC> RSRC { get; set; }

        public virtual ICollection<TASK> TASK { get; set; }
    }
}
