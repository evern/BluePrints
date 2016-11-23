namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("BASETYPE")]
    public partial class BASETYPE
    {
        public BASETYPE()
        {
            PROJECT = new HashSet<PROJECT>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int base_type_id { get; set; }

        public int base_type_seq_num { get; set; }

        [Required]
        [StringLength(40)]
        public string base_type { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ICollection<PROJECT> PROJECT { get; set; }
    }
}
