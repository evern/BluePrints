namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("RCATVAL")]
    public partial class RCATVAL
    {
        public RCATVAL()
        {
            RSRCRCAT = new HashSet<RSRCRCAT>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int rsrc_catg_id { get; set; }

        public int rsrc_catg_type_id { get; set; }

        public int seq_num { get; set; }

        [Required]
        [StringLength(32)]
        public string rsrc_catg_short_name { get; set; }

        [StringLength(100)]
        public string rsrc_catg_name { get; set; }

        public int? parent_rsrc_catg_id { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual RCATTYPE RCATTYPE { get; set; }

        public virtual ICollection<RSRCRCAT> RSRCRCAT { get; set; }
    }
}