namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TSDELEGATE")]
    public partial class TSDELEGATE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ts_delegate_id { get; set; }

        public int user_id { get; set; }

        public int ts_delegate_user_id { get; set; }

        public int? proj_id { get; set; }

        [StringLength(1)]
        public string active_flag { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        public virtual USERS USERS { get; set; }

        public virtual USERS USERS1 { get; set; }
    }
}