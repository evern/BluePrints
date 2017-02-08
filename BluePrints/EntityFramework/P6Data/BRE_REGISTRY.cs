namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class BRE_REGISTRY
    {
        [Key]
        [StringLength(50)]
        public string bre_registry_id { get; set; }

        [Required]
        [StringLength(30)]
        public string ip_address { get; set; }

        public DateTime start_time { get; set; }

        public DateTime? last_time { get; set; }

        [Required]
        [StringLength(1)]
        public string status_code { get; set; }

        public DateTime? torched_time { get; set; }

        [Required]
        [StringLength(30)]
        public string config_name { get; set; }

        [StringLength(1)]
        public string config_changed_flag { get; set; }

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