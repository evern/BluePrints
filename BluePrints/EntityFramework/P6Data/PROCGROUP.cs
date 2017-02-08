namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PROCGROUP")]
    public partial class PROCGROUP
    {
        public PROCGROUP()
        {
            PROCITEM = new HashSet<PROCITEM>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int proc_group_id { get; set; }

        public int seq_num { get; set; }

        [Required]
        [StringLength(120)]
        public string proc_group_name { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ICollection<PROCITEM> PROCITEM { get; set; }
    }
}