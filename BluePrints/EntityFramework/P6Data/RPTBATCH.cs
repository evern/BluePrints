namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("RPTBATCH")]
    public partial class RPTBATCH
    {
        public RPTBATCH()
        {
            RPTLIST = new HashSet<RPTLIST>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int rpt_batch_id { get; set; }

        [Required]
        [StringLength(80)]
        public string rpt_batch_name { get; set; }

        public int? proj_id { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        public virtual ICollection<RPTLIST> RPTLIST { get; set; }
    }
}
