namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("RCATTYPE")]
    public partial class RCATTYPE
    {
        public RCATTYPE()
        {
            RCATVAL = new HashSet<RCATVAL>();
            RSRCRCAT = new HashSet<RSRCRCAT>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int rsrc_catg_type_id { get; set; }

        public int seq_num { get; set; }

        public int rsrc_catg_short_len { get; set; }

        [Required]
        [StringLength(40)]
        public string rsrc_catg_type { get; set; }

        [Required]
        [StringLength(1)]
        public string super_flag { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ICollection<RCATVAL> RCATVAL { get; set; }

        public virtual ICollection<RSRCRCAT> RSRCRCAT { get; set; }
    }
}