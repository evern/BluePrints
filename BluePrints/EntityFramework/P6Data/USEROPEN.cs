namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("USEROPEN")]
    public partial class USEROPEN
    {
        public USEROPEN()
        {
            USROPNVAL = new HashSet<USROPNVAL>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int user_open_id { get; set; }

        [Required]
        [StringLength(255)]
        public string user_open_name { get; set; }

        public int? user_id { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual USERS USERS { get; set; }

        public virtual ICollection<USROPNVAL> USROPNVAL { get; set; }
    }
}