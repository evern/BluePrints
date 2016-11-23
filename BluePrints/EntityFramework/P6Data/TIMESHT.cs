namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TIMESHT")]
    public partial class TIMESHT
    {
        public TIMESHT()
        {
            RSRCHOUR = new HashSet<RSRCHOUR>();
        }

        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ts_id { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int rsrc_id { get; set; }

        [Required]
        [StringLength(1)]
        public string daily_flag { get; set; }

        [Required]
        [StringLength(20)]
        public string status_code { get; set; }

        public int? user_id { get; set; }

        public DateTime? last_recv_date { get; set; }

        public DateTime? status_date { get; set; }

        [Column(TypeName = "text")]
        public string ts_notes { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ICollection<RSRCHOUR> RSRCHOUR { get; set; }
    }
}
