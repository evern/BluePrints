namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("GCHANGE")]
    public partial class GCHANGE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int gchange_id { get; set; }

        [Required]
        [StringLength(40)]
        public string gchange_name { get; set; }

        [Column(TypeName = "text")]
        public string gchange_data { get; set; }

        public int? user_id { get; set; }

        [Required]
        [StringLength(20)]
        public string table_name { get; set; }

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