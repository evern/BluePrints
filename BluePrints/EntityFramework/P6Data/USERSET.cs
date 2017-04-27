namespace BluePrints.P6Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("USERSET")]
    public partial class USERSET
    {
        [Key]
        [Column("namespace", Order = 0)]
        [StringLength(255)]
        public string _namespace { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(255)]
        public string setting_name { get; set; }

        [StringLength(4000)]
        public string setting_value { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int user_id { get; set; }

        [Column(TypeName = "text")]
        public string userset_blob { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual USERS USERS { get; set; }
    }
}