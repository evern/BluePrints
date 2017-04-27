namespace BluePrints.P6Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("NOTE")]
    public partial class NOTE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int note_id { get; set; }

        [Required]
        [StringLength(30)]
        public string table_name { get; set; }

        [Required]
        [StringLength(30)]
        public string type_name { get; set; }

        public int fk_id { get; set; }

        public DateTime note_date { get; set; }

        [StringLength(4000)]
        public string note_value { get; set; }

        [Required]
        [StringLength(255)]
        public string user_name { get; set; }

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