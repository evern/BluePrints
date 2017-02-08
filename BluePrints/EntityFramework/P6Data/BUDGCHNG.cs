namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("BUDGCHNG")]
    public partial class BUDGCHNG
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int budg_chng_id { get; set; }

        public int proj_id { get; set; }

        public int wbs_id { get; set; }

        public DateTime chng_date { get; set; }

        [Required]
        [StringLength(32)]
        public string chng_short_name { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? chng_cost { get; set; }

        [StringLength(255)]
        public string chng_by_name { get; set; }

        [StringLength(30)]
        public string status_code { get; set; }

        [StringLength(130)]
        public string chng_descr { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        public virtual PROJWBS PROJWBS { get; set; }
    }
}