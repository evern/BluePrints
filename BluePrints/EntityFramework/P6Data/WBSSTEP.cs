namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("WBSSTEP")]
    public partial class WBSSTEP
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int wbs_step_id { get; set; }

        public int proj_id { get; set; }

        public int wbs_id { get; set; }

        public int seq_num { get; set; }

        [Required]
        [StringLength(1)]
        public string complete_flag { get; set; }

        [Required]
        [StringLength(120)]
        public string step_name { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? step_wt { get; set; }

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
