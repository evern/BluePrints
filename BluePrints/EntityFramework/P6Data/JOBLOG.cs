namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("JOBLOG")]
    public partial class JOBLOG
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int job_id { get; set; }

        [Column(TypeName = "text")]
        public string job_log_data { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual JOBSVC JOBSVC { get; set; }
    }
}
