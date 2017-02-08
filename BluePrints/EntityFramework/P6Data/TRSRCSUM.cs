namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TRSRCSUM")]
    public partial class TRSRCSUM
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int taskrsrc_sum_id { get; set; }

        public int? proj_id { get; set; }

        public int? rsrc_id { get; set; }

        public int? role_id { get; set; }

        public int? skill_level { get; set; }

        public DateTime? overalloc_date { get; set; }

        [Column(TypeName = "text")]
        public string spread_data { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }
    }
}