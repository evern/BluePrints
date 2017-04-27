namespace BluePrints.P6Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("FILTPROP")]
    public partial class FILTPROP
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int filter_id { get; set; }

        [Required]
        [StringLength(16)]
        public string table_name { get; set; }

        [Required]
        [StringLength(20)]
        public string filter_type { get; set; }

        [Required]
        [StringLength(40)]
        public string filter_name { get; set; }

        public int? user_id { get; set; }

        [Column(TypeName = "text")]
        public string filter_data { get; set; }

        public int? rpt_id { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual RPT RPT { get; set; }

        public virtual USERS USERS { get; set; }
    }
}