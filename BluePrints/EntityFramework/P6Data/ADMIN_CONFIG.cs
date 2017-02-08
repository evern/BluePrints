namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ADMIN_CONFIG
    {
        [Key]
        [StringLength(255)]
        public string config_name { get; set; }

        [StringLength(10)]
        public string config_type { get; set; }

        [StringLength(10)]
        public string factory_version { get; set; }

        public DateTime last_change_date { get; set; }

        [StringLength(255)]
        public string config_value { get; set; }

        [Column(TypeName = "text")]
        public string config_data { get; set; }

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