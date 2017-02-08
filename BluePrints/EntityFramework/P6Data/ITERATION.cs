namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ITERATION")]
    public partial class ITERATION
    {
        public ITERATION()
        {
            ITERDAYS = new HashSet<ITERDAYS>();
            ITERGOAL = new HashSet<ITERGOAL>();
            PROJWBS = new HashSet<PROJWBS>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int iteration_id { get; set; }

        public int release_id { get; set; }

        [Required]
        [StringLength(50)]
        public string iteration_name { get; set; }

        public DateTime? start_date { get; set; }

        public DateTime? end_date { get; set; }

        public DateTime? termination_date { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? capacity_pct { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? remain_qty { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual RELEASE RELEASE { get; set; }

        public virtual ICollection<ITERDAYS> ITERDAYS { get; set; }

        public virtual ICollection<ITERGOAL> ITERGOAL { get; set; }

        public virtual ICollection<PROJWBS> PROJWBS { get; set; }
    }
}