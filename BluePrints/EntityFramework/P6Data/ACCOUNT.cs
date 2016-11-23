namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ACCOUNT")]
    public partial class ACCOUNT
    {
        public ACCOUNT()
        {
            PROJCOST = new HashSet<PROJCOST>();
            PROJECT = new HashSet<PROJECT>();
            TASKRSRC = new HashSet<TASKRSRC>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int acct_id { get; set; }

        public int acct_seq_num { get; set; }

        [Required]
        [StringLength(40)]
        public string acct_short_name { get; set; }

        [Required]
        [StringLength(100)]
        public string acct_name { get; set; }

        public int? parent_acct_id { get; set; }

        [Column(TypeName = "text")]
        public string acct_descr { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ICollection<PROJCOST> PROJCOST { get; set; }

        public virtual ICollection<PROJECT> PROJECT { get; set; }

        public virtual ICollection<TASKRSRC> TASKRSRC { get; set; }
    }
}
