namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("RELEASE")]
    public partial class RELEASE
    {
        public RELEASE()
        {
            ITERATION = new HashSet<ITERATION>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int release_id { get; set; }

        public int? rfolio_id { get; set; }

        [Required]
        [StringLength(60)]
        public string release_name { get; set; }

        public DateTime start_date { get; set; }

        public int proj_id { get; set; }

        [StringLength(255)]
        public string release_theme { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ICollection<ITERATION> ITERATION { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        public virtual RFOLIO RFOLIO { get; set; }
    }
}
