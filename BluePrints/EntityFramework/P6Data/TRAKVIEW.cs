namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("TRAKVIEW")]
    public partial class TRAKVIEW
    {
        public TRAKVIEW()
        {
            PROJISSU = new HashSet<PROJISSU>();
            PROJTHRS = new HashSet<PROJTHRS>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int track_view_id { get; set; }

        [Required]
        [StringLength(12)]
        public string display_type { get; set; }

        [Required]
        [StringLength(80)]
        public string track_view_name { get; set; }

        [Required]
        [StringLength(1)]
        public string web_view_flag { get; set; }

        public int? user_id { get; set; }

        [Column(TypeName = "text")]
        public string track_view_data { get; set; }

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

        public virtual USERS USERS { get; set; }
    }
}