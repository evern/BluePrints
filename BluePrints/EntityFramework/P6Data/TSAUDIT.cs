namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TSAUDIT")]
    public partial class TSAUDIT
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ts_audit_id { get; set; }

        public DateTime? audit_date { get; set; }

        public int? rsrc_id { get; set; }

        [StringLength(255)]
        public string rsrc_short_name { get; set; }

        [StringLength(255)]
        public string rsrc_name { get; set; }

        [StringLength(20)]
        public string ts_status_code { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? reg_hrs { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? reg_ot_hrs { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? pend_reg_hrs { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? pend_reg_ot_hrs { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? oh_hrs { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? oh_ot_hrs { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? pend_oh_hrs { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? pend_oh_ot_hrs { get; set; }

        public int? ts_id { get; set; }

        public DateTime? ts_start_date { get; set; }

        public DateTime? ts_end_date { get; set; }

        public int? proj_id { get; set; }

        [StringLength(40)]
        public string proj_short_name { get; set; }

        [StringLength(20)]
        public string ts_task_status { get; set; }

        public int? user_id { get; set; }

        [StringLength(255)]
        public string user_name { get; set; }

        [StringLength(50)]
        public string approving_as { get; set; }
    }
}