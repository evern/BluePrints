namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("RLFOLIO")]
    public partial class RLFOLIO
    {
        public RLFOLIO()
        {
            ROLFOLIO = new HashSet<ROLFOLIO>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int rlfolio_id { get; set; }

        public int? user_id { get; set; }

        [Required]
        [StringLength(40)]
        public string rlfolio_name { get; set; }

        [Required]
        [StringLength(20)]
        public string rlfolio_type { get; set; }

        [StringLength(255)]
        public string rlfolio_descr { get; set; }

        [Column(TypeName = "text")]
        public string rlfolio_data { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual USERS USERS { get; set; }

        public virtual ICollection<ROLFOLIO> ROLFOLIO { get; set; }
    }
}