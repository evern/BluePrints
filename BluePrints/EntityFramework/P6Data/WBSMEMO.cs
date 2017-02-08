namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("WBSMEMO")]
    public partial class WBSMEMO
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int wbs_memo_id { get; set; }

        public int proj_id { get; set; }

        public int wbs_id { get; set; }

        public int memo_type_id { get; set; }

        [Column(TypeName = "text")]
        public string wbs_memo { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual MEMOTYPE MEMOTYPE { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        public virtual PROJWBS PROJWBS { get; set; }
    }
}