namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PROFILE")]
    public partial class PROFILE
    {
        public PROFILE()
        {
            PROFPRIV = new HashSet<PROFPRIV>();
            USEROBS = new HashSet<USEROBS>();
            USERS = new HashSet<USERS>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int prof_id { get; set; }

        [Required]
        [StringLength(1)]
        public string default_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string superuser_flag { get; set; }

        [Required]
        [StringLength(12)]
        public string scope_type { get; set; }

        [Required]
        [StringLength(100)]
        public string prof_name { get; set; }

        [StringLength(255)]
        public string prof_descr { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ICollection<PROFPRIV> PROFPRIV { get; set; }

        public virtual ICollection<USEROBS> USEROBS { get; set; }

        public virtual ICollection<USERS> USERS { get; set; }
    }
}