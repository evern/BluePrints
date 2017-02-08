namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("RSRCRATE")]
    public partial class RSRCRATE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int rsrc_rate_id { get; set; }

        public int rsrc_id { get; set; }

        public DateTime start_date { get; set; }

        public int? shift_period_id { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? max_qty_per_hr { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? cost_per_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? cost_per_qty2 { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? cost_per_qty3 { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? cost_per_qty4 { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? cost_per_qty5 { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual RSRC RSRC { get; set; }

        public virtual SHIFTPER SHIFTPER { get; set; }
    }
}