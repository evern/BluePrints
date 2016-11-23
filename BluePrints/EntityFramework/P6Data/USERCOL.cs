namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("USERCOL")]
    public partial class USERCOL
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int user_col_id { get; set; }

        [Required]
        [StringLength(16)]
        public string table_name { get; set; }

        [Required]
        [StringLength(20)]
        public string logical_data_type { get; set; }

        [Required]
        [StringLength(32)]
        public string user_col_name { get; set; }

        [Required]
        [StringLength(40)]
        public string user_col_label { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }
    }
}
