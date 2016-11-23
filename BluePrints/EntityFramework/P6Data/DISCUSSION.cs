namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DISCUSSION")]
    public partial class DISCUSSION
    {
        public DISCUSSION()
        {
            DISCUSSION_READ = new HashSet<DISCUSSION_READ>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int discussion_id { get; set; }

        public int task_id { get; set; }

        [StringLength(4000)]
        public string discussion_value { get; set; }

        public DateTime? discussion_date { get; set; }

        public int user_id { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ICollection<DISCUSSION_READ> DISCUSSION_READ { get; set; }

        public virtual TASK TASK { get; set; }
    }
}
