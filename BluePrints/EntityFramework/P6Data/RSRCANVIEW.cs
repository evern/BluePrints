namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("RSRCANVIEW")]
    public partial class RSRCANVIEW
    {
        public RSRCANVIEW()
        {
            RSRCANDASH = new HashSet<RSRCANDASH>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int rsrcan_view_id { get; set; }

        [Required]
        [StringLength(255)]
        public string rsrcan_view_name { get; set; }

        public int? user_id { get; set; }

        [StringLength(20)]
        public string rsrcan_view_type { get; set; }

        [StringLength(4000)]
        public string rsrcan_view_data { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ICollection<RSRCANDASH> RSRCANDASH { get; set; }

        public virtual USERS USERS { get; set; }
    }
}