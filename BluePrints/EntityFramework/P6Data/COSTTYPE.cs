namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("COSTTYPE")]
    public partial class COSTTYPE
    {
        public COSTTYPE()
        {
            PROJCOST = new HashSet<PROJCOST>();
            SUMPROJCOST = new HashSet<SUMPROJCOST>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int cost_type_id { get; set; }

        public int seq_num { get; set; }

        [Required]
        [StringLength(36)]
        public string cost_type { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ICollection<PROJCOST> PROJCOST { get; set; }

        public virtual ICollection<SUMPROJCOST> SUMPROJCOST { get; set; }
    }
}