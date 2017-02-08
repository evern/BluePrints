namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("EXPPROJ")]
    public partial class EXPPROJ
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int proj_id { get; set; }

        [Required]
        [StringLength(255)]
        public string exp_group { get; set; }

        [Required]
        [StringLength(24)]
        public string exp_proj_name { get; set; }

        [Required]
        [StringLength(1)]
        public string login_supplied_flag { get; set; }

        [StringLength(40)]
        public string exp_user_name { get; set; }

        [StringLength(60)]
        public string exp_passwd { get; set; }

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