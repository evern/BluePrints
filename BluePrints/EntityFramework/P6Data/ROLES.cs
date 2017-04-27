namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class ROLES
    {
        public ROLES()
        {
            ROLELIMIT = new HashSet<ROLELIMIT>();
            ROLERATE = new HashSet<ROLERATE>();
            ROLFOLIO = new HashSet<ROLFOLIO>();
            RSRC = new HashSet<RSRC>();
            RSRCROLE = new HashSet<RSRCROLE>();
            SCENROLE = new HashSet<SCENROLE>();
            TASKRSRC = new HashSet<TASKRSRC>();
            WBSRSRC = new HashSet<WBSRSRC>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int role_id { get; set; }

        [Required]
        [StringLength(40)]
        public string role_short_name { get; set; }

        [Required]
        [StringLength(100)]
        public string role_name { get; set; }

        public int seq_num { get; set; }

        public int? parent_role_id { get; set; }

        [Required]
        [StringLength(1)]
        public string def_cost_qty_link_flag { get; set; }

        [Required]
        [StringLength(24)]
        public string cost_qty_type { get; set; }

        public int? pobs_id { get; set; }

        [Column(TypeName = "text")]
        public string role_descr { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual POBS POBS { get; set; }

        public virtual ICollection<ROLELIMIT> ROLELIMIT { get; set; }

        public virtual ICollection<ROLERATE> ROLERATE { get; set; }

        public virtual ICollection<ROLFOLIO> ROLFOLIO { get; set; }

        public virtual ICollection<RSRC> RSRC { get; set; }

        public virtual ICollection<RSRCROLE> RSRCROLE { get; set; }

        public virtual ICollection<SCENROLE> SCENROLE { get; set; }

        public virtual ICollection<TASKRSRC> TASKRSRC { get; set; }

        public virtual ICollection<WBSRSRC> WBSRSRC { get; set; }
    }
}