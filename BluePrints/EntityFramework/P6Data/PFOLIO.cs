namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PFOLIO")]
    public partial class PFOLIO
    {
        public PFOLIO()
        {
            PRPFOLIO = new HashSet<PRPFOLIO>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int pfolio_id { get; set; }

        [Required]
        [StringLength(1)]
        public string closed_proj_flag { get; set; }

        [Required]
        [StringLength(1)]
        public string whatif_proj_flag { get; set; }

        [Required]
        [StringLength(20)]
        public string pfolio_type { get; set; }

        [Required]
        [StringLength(40)]
        public string pfolio_name { get; set; }

        public int? user_id { get; set; }

        [StringLength(255)]
        public string pfolio_descr { get; set; }

        public DateTime? last_refresh_date { get; set; }

        [Column(TypeName = "text")]
        public string pfolio_data { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual USERS USERS { get; set; }

        public virtual ICollection<PRPFOLIO> PRPFOLIO { get; set; }
    }
}