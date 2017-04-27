namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("WORKSPACE")]
    public partial class WORKSPACE
    {
        public WORKSPACE()
        {
            PROJISSU = new HashSet<PROJISSU>();
            TASKWKSP = new HashSet<TASKWKSP>();
            USERWKSP = new HashSet<USERWKSP>();
            WORKFLOW = new HashSet<WORKFLOW>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int workspace_id { get; set; }

        [Required]
        [StringLength(12)]
        public string workspace_type { get; set; }

        public int proj_id { get; set; }

        [Required]
        [StringLength(255)]
        public string workspace_name { get; set; }

        [Column(TypeName = "text")]
        public string workspace_prefs { get; set; }

        [StringLength(4000)]
        public string cr_external_key { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        public virtual ICollection<PROJISSU> PROJISSU { get; set; }

        public virtual ICollection<TASKWKSP> TASKWKSP { get; set; }

        public virtual ICollection<USERWKSP> USERWKSP { get; set; }

        public virtual ICollection<WORKFLOW> WORKFLOW { get; set; }
    }
}