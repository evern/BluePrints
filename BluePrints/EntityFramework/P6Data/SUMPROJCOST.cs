namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SUMPROJCOST")]
    public partial class SUMPROJCOST
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int proj_id { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int wbs_id { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int cost_type_id { get; set; }

        public DateTime? start_date { get; set; }

        public DateTime? end_date { get; set; }

        [StringLength(20)]
        public string spread_type { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? act_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? remain_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? target_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? total_cost { get; set; }

        public virtual COSTTYPE COSTTYPE { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        public virtual PROJWBS PROJWBS { get; set; }
    }
}
