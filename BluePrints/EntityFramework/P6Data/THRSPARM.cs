namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("THRSPARM")]
    public partial class THRSPARM
    {
        public THRSPARM()
        {
            PROJISSU = new HashSet<PROJISSU>();
            PROJTHRS = new HashSet<PROJTHRS>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int thresh_parm_id { get; set; }

        public int seq_num { get; set; }

        [Required]
        [StringLength(1)]
        public string wbs_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string task_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string rsrc_flag { get; set; }

        [Required]
        [StringLength(12)]
        public string thresh_parm_type { get; set; }

        [Required]
        [StringLength(80)]
        public string thresh_parm_name { get; set; }

        [StringLength(40)]
        public string thresh_field_name { get; set; }

        [StringLength(80)]
        public string thresh_short_name { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ICollection<PROJISSU> PROJISSU { get; set; }

        public virtual ICollection<PROJTHRS> PROJTHRS { get; set; }
    }
}