namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PROJCOST")]
    public partial class PROJCOST
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int cost_item_id { get; set; }

        public int proj_id { get; set; }

        public int task_id { get; set; }

        [Required]
        [StringLength(1)]
        public string auto_compute_act_flag { get; set; }

        [Required]
        [StringLength(12)]
        public string cost_load_type { get; set; }

        public int? acct_id { get; set; }

        public int? cost_type_id { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? target_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? remain_cost { get; set; }

        [StringLength(30)]
        public string qty_name { get; set; }

        [Column(TypeName = "numeric")]
        public decimal target_qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal cost_per_qty { get; set; }

        [StringLength(32)]
        public string po_number { get; set; }

        [StringLength(100)]
        public string vendor_name { get; set; }

        [StringLength(120)]
        public string cost_name { get; set; }

        [Column(TypeName = "text")]
        public string cost_descr { get; set; }

        public int? pobs_id { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ACCOUNT ACCOUNT { get; set; }

        public virtual COSTTYPE COSTTYPE { get; set; }

        public virtual POBS POBS { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        public virtual TASK TASK { get; set; }
    }
}
