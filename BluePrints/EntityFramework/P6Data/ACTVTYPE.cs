namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ACTVTYPE")]
    public partial class ACTVTYPE
    {
        public ACTVTYPE()
        {
            ACTVCODE = new HashSet<ACTVCODE>();
            TASKACTV = new HashSet<TASKACTV>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int actv_code_type_id { get; set; }

        public int actv_short_len { get; set; }

        public int seq_num { get; set; }

        [Required]
        [StringLength(40)]
        public string actv_code_type { get; set; }

        [Required]
        [StringLength(10)]
        public string actv_code_type_scope { get; set; }

        public int? proj_id { get; set; }

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

        public virtual ICollection<ACTVCODE> ACTVCODE { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        public virtual ICollection<TASKACTV> TASKACTV { get; set; }
    }
}