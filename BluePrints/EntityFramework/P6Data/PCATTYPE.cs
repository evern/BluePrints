namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PCATTYPE")]
    public partial class PCATTYPE
    {
        public PCATTYPE()
        {
            PCATVAL = new HashSet<PCATVAL>();
            PROJPCAT = new HashSet<PROJPCAT>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int proj_catg_type_id { get; set; }

        public int seq_num { get; set; }

        public int proj_catg_short_len { get; set; }

        [Required]
        [StringLength(40)]
        public string proj_catg_type { get; set; }

        [Required]
        [StringLength(1)]
        public string super_flag { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? proj_catg_type_wt { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? max_proj_catg_wt { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ICollection<PCATVAL> PCATVAL { get; set; }

        public virtual ICollection<PROJPCAT> PROJPCAT { get; set; }
    }
}
