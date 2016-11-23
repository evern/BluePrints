namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FACTOR")]
    public partial class FACTOR
    {
        public FACTOR()
        {
            FACTVAL = new HashSet<FACTVAL>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int fact_id { get; set; }

        public int fact_seq_num { get; set; }

        [Required]
        [StringLength(20)]
        public string fact_type { get; set; }

        [Required]
        [StringLength(100)]
        public string fact_name { get; set; }

        public int? def_fact_val_id { get; set; }

        [Column(TypeName = "text")]
        public string fact_descr { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ICollection<FACTVAL> FACTVAL { get; set; }
    }
}
