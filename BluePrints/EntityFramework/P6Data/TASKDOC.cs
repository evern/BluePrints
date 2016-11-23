namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TASKDOC")]
    public partial class TASKDOC
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int taskdoc_id { get; set; }

        public int doc_id { get; set; }

        public int proj_id { get; set; }

        public int wbs_id { get; set; }

        [Required]
        [StringLength(1)]
        public string wp_flag { get; set; }

        public int? task_id { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual DOCUMENT DOCUMENT { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        public virtual PROJWBS PROJWBS { get; set; }

        public virtual TASK TASK { get; set; }
    }
}
