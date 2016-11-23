namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("EXTAPP")]
    public partial class EXTAPP
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int app_id { get; set; }

        [Required]
        [StringLength(1)]
        public string login_supply_flag { get; set; }

        [Required]
        [StringLength(100)]
        public string app_name { get; set; }

        [Required]
        [StringLength(100)]
        public string app_exe_name { get; set; }

        public int? proj_id { get; set; }

        [StringLength(40)]
        public string app_user_name { get; set; }

        [StringLength(100)]
        public string app_passwd { get; set; }

        [StringLength(100)]
        public string app_data_name { get; set; }

        [StringLength(255)]
        public string app_parm_string { get; set; }

        [StringLength(255)]
        public string app_data_loc { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual PROJECT PROJECT { get; set; }
    }
}
