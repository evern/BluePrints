namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PCATVAL")]
    public partial class PCATVAL
    {
        public PCATVAL()
        {
            PCATUSER = new HashSet<PCATUSER>();
            PROJPCAT = new HashSet<PROJPCAT>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int proj_catg_id { get; set; }

        public int proj_catg_type_id { get; set; }

        public int seq_num { get; set; }

        [Required]
        [StringLength(32)]
        public string proj_catg_short_name { get; set; }

        public int? parent_proj_catg_id { get; set; }

        [StringLength(100)]
        public string proj_catg_name { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? proj_catg_wt { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual PCATTYPE PCATTYPE { get; set; }

        public virtual ICollection<PCATUSER> PCATUSER { get; set; }

        public virtual ICollection<PROJPCAT> PROJPCAT { get; set; }
    }
}