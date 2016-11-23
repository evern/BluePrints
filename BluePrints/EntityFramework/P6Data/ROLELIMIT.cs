namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ROLELIMIT")]
    public partial class ROLELIMIT
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int rolelimit_id { get; set; }

        public int role_id { get; set; }

        public DateTime start_date { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? max_qty_per_hr { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ROLES ROLES { get; set; }
    }
}
