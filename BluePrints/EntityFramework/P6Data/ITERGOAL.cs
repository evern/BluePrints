namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ITERGOAL")]
    public partial class ITERGOAL
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int iter_goal_id { get; set; }

        public int iteration_id { get; set; }

        [Required]
        [StringLength(255)]
        public string goal_name { get; set; }

        [Required]
        [StringLength(4000)]
        public string goal_descr { get; set; }

        [Required]
        [StringLength(12)]
        public string status_code { get; set; }

        public int rfolio_id { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ITERATION ITERATION { get; set; }

        public virtual RFOLIO RFOLIO { get; set; }
    }
}