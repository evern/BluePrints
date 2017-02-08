namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POBS
    {
        public POBS()
        {
            PROJCOST = new HashSet<PROJCOST>();
            ROLES = new HashSet<ROLES>();
            RSRC = new HashSet<RSRC>();
            TASKRSRC = new HashSet<TASKRSRC>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int pobs_id { get; set; }

        public int seq_num { get; set; }

        [Required]
        [StringLength(255)]
        public string pobs_name { get; set; }

        public int? pobs_parent_id { get; set; }

        [StringLength(255)]
        public string pobs_descr { get; set; }

        [StringLength(255)]
        public string pobs_manager { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ICollection<PROJCOST> PROJCOST { get; set; }

        public virtual ICollection<ROLES> ROLES { get; set; }

        public virtual ICollection<RSRC> RSRC { get; set; }

        public virtual ICollection<TASKRSRC> TASKRSRC { get; set; }
    }
}