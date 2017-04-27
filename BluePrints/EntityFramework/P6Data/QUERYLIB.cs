namespace BluePrints.P6Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("QUERYLIB")]
    public partial class QUERYLIB
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int query_lib_id { get; set; }

        [Required]
        [StringLength(25)]
        public string app_name { get; set; }

        [Required]
        [StringLength(1)]
        public string core_flag { get; set; }

        [Required]
        [StringLength(4000)]
        public string match_sql { get; set; }

        [StringLength(4000)]
        public string hints { get; set; }

        [StringLength(4000)]
        public string replacement_sql { get; set; }

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