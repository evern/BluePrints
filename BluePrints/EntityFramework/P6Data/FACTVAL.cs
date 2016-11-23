namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FACTVAL")]
    public partial class FACTVAL
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int fact_val_id { get; set; }

        public int fact_id { get; set; }

        [Required]
        [StringLength(24)]
        public string fact_value { get; set; }

        [StringLength(255)]
        public string fact_value_descr { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual FACTOR FACTOR { get; set; }
    }
}
