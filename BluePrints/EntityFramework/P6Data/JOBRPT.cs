namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("JOBRPT")]
    public partial class JOBRPT
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int job_id { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int rpt_id { get; set; }

        [StringLength(20)]
        public string table_name { get; set; }

        public int? fk_id { get; set; }

        [Column(TypeName = "text")]
        public string jobrpt_data { get; set; }

        [Required]
        [StringLength(1)]
        public string personal_portal_flag { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual JOBSVC JOBSVC { get; set; }

        public virtual RPT RPT { get; set; }
    }
}