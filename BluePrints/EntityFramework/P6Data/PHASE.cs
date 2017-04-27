namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("PHASE")]
    public partial class PHASE
    {
        public PHASE()
        {
            PROJWBS = new HashSet<PROJWBS>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int phase_id { get; set; }

        public int seq_num { get; set; }

        [Required]
        [StringLength(100)]
        public string phase_name { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ICollection<PROJWBS> PROJWBS { get; set; }
    }
}