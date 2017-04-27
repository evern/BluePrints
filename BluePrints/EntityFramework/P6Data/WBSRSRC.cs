namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("WBSRSRC")]
    public partial class WBSRSRC
    {
        public WBSRSRC()
        {
            WBSRSRC_QTY = new HashSet<WBSRSRC_QTY>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int wbsrsrc_id { get; set; }

        public int wbs_id { get; set; }

        public int? rsrc_id { get; set; }

        public int proj_id { get; set; }

        [Required]
        [StringLength(1)]
        public string committed_flag { get; set; }

        public DateTime? start_date { get; set; }

        public DateTime? end_date { get; set; }

        [Required]
        [StringLength(1)]
        public string auto_compute_dates_flag { get; set; }

        public int? role_id { get; set; }

        [StringLength(4000)]
        public string rsrc_request_data { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? allocation_pct { get; set; }

        public int? wbrs_cat_id { get; set; }

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

        public virtual ROLES ROLES { get; set; }

        public virtual RSRC RSRC { get; set; }

        public virtual WBRSCAT WBRSCAT { get; set; }

        public virtual ICollection<WBSRSRC_QTY> WBSRSRC_QTY { get; set; }
    }
}