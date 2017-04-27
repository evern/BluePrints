namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("NONWORK")]
    public partial class NONWORK
    {
        public NONWORK()
        {
            RSRCHOUR = new HashSet<RSRCHOUR>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int nonwork_type_id { get; set; }

        public int seq_num { get; set; }

        [Required]
        [StringLength(32)]
        public string nonwork_code { get; set; }

        [Required]
        [StringLength(40)]
        public string nonwork_type { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ICollection<RSRCHOUR> RSRCHOUR { get; set; }
    }
}