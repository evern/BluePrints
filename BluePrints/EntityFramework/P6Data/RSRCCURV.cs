namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("RSRCCURV")]
    public partial class RSRCCURV
    {
        public RSRCCURV()
        {
            TASKRSRC = new HashSet<TASKRSRC>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int curv_id { get; set; }

        [Required]
        [StringLength(60)]
        public string curv_name { get; set; }

        [Required]
        [StringLength(1)]
        public string default_flag { get; set; }

        [Column(TypeName = "text")]
        [Required]
        public string curv_data { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ICollection<TASKRSRC> TASKRSRC { get; set; }
    }
}