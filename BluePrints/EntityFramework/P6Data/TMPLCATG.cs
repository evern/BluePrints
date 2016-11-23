namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TMPLCATG")]
    public partial class TMPLCATG
    {
        public TMPLCATG()
        {
            WKFLTMPL = new HashSet<WKFLTMPL>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int tmplcatg_catg_id { get; set; }

        [Required]
        [StringLength(255)]
        public string catg_name { get; set; }

        [Required]
        [StringLength(1)]
        public string project_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string process_flag { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ICollection<WKFLTMPL> WKFLTMPL { get; set; }
    }
}
