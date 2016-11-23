namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("RFOLIO")]
    public partial class RFOLIO
    {
        public RFOLIO()
        {
            ITERGOAL = new HashSet<ITERGOAL>();
            PROJWBS = new HashSet<PROJWBS>();
            RELEASE = new HashSet<RELEASE>();
            RSRFOLIO = new HashSet<RSRFOLIO>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int rfolio_id { get; set; }

        public int? user_id { get; set; }

        [Required]
        [StringLength(40)]
        public string rfolio_name { get; set; }

        [Required]
        [StringLength(20)]
        public string rfolio_type { get; set; }

        [StringLength(255)]
        public string rfolio_descr { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? team_capacity_pct { get; set; }

        public int? parent_rfolio_id { get; set; }

        [Column(TypeName = "text")]
        public string rfolio_data { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ICollection<ITERGOAL> ITERGOAL { get; set; }

        public virtual ICollection<PROJWBS> PROJWBS { get; set; }

        public virtual ICollection<RELEASE> RELEASE { get; set; }

        public virtual USERS USERS { get; set; }

        public virtual ICollection<RSRFOLIO> RSRFOLIO { get; set; }
    }
}
