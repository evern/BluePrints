namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TRSRCSUMFN")]
    public partial class TRSRCSUMFN
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int trsrc_sum_fin_id { get; set; }

        public int fin_dates_id { get; set; }

        public int? proj_id { get; set; }

        public int? rsrc_id { get; set; }

        public int? role_id { get; set; }

        public int taskrsrc_sum_id { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_ot_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_reg_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_ot_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_reg_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? total_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? total_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? remain_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? remain_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? target_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? target_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? staffed_remain_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? unstaffed_remain_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? staffed_remain_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? unstaffed_remain_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? staffed_late_remain_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? unstaffed_late_remain_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? staffed_late_remain_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? unstaffed_late_remain_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? late_remain_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? late_remain_cost { get; set; }

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