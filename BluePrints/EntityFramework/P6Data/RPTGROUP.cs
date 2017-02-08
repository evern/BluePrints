namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("RPTGROUP")]
    public partial class RPTGROUP
    {
        public RPTGROUP()
        {
            RPT = new HashSet<RPT>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int rpt_group_id { get; set; }

        public int rpt_group_seq_num { get; set; }

        [Required]
        [StringLength(80)]
        public string rpt_group_name { get; set; }

        public int? parent_group_id { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ICollection<RPT> RPT { get; set; }
    }
}