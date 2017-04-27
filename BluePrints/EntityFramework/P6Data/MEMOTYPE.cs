namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("MEMOTYPE")]
    public partial class MEMOTYPE
    {
        public MEMOTYPE()
        {
            TASKMEMO = new HashSet<TASKMEMO>();
            WBSMEMO = new HashSet<WBSMEMO>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int memo_type_id { get; set; }

        public int seq_num { get; set; }

        [Required]
        [StringLength(1)]
        public string eps_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string proj_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string wbs_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string task_flag { get; set; }

        [Required]
        [StringLength(40)]
        public string memo_type { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ICollection<TASKMEMO> TASKMEMO { get; set; }

        public virtual ICollection<WBSMEMO> WBSMEMO { get; set; }
    }
}