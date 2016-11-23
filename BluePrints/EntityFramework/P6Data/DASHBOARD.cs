namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DASHBOARD")]
    public partial class DASHBOARD
    {
        public DASHBOARD()
        {
            DASHUSER = new HashSet<DASHUSER>();
            RSRCANDASH = new HashSet<RSRCANDASH>();
            VWPREFDASH = new HashSet<VWPREFDASH>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int dashboard_id { get; set; }

        [Required]
        [StringLength(255)]
        public string dashboard_name { get; set; }

        public int? user_id { get; set; }

        [StringLength(30)]
        public string table_name { get; set; }

        public int? fk_id { get; set; }

        [Required]
        [StringLength(1)]
        public string lock_filter_flag { get; set; }

        public int? max_rows_per_portlet { get; set; }

        [StringLength(4000)]
        public string dashboard_layout_data { get; set; }

        [Column(TypeName = "text")]
        public string portlet_settings_data { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual USERS USERS { get; set; }

        public virtual ICollection<DASHUSER> DASHUSER { get; set; }

        public virtual ICollection<RSRCANDASH> RSRCANDASH { get; set; }

        public virtual ICollection<VWPREFDASH> VWPREFDASH { get; set; }
    }
}
