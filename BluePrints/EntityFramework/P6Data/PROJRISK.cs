namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PROJRISK")]
    public partial class PROJRISK
    {
        public PROJRISK()
        {
            TASKRISK = new HashSet<TASKRISK>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int risk_id { get; set; }

        public int proj_id { get; set; }

        public DateTime? add_date { get; set; }

        [Required]
        [StringLength(12)]
        public string status_code { get; set; }

        [Required]
        [StringLength(200)]
        public string risk_name { get; set; }

        public int? risk_type_id { get; set; }

        public int? rsrc_id { get; set; }

        [Column(TypeName = "text")]
        public string risk_descr { get; set; }

        [Required]
        [StringLength(12)]
        public string risk_to_type { get; set; }

        public int? identified_by_id { get; set; }

        [StringLength(12)]
        public string response_type { get; set; }

        [StringLength(255)]
        public string response_text { get; set; }

        [StringLength(2)]
        public string pre_rsp_prblty { get; set; }

        [StringLength(2)]
        public string pre_rsp_schd_prblty { get; set; }

        [StringLength(2)]
        public string pre_rsp_cost_prblty { get; set; }

        [StringLength(2)]
        public string post_rsp_prblty { get; set; }

        [StringLength(2)]
        public string post_rsp_schd_prblty { get; set; }

        [StringLength(2)]
        public string post_rsp_cost_prblty { get; set; }

        [StringLength(4000)]
        public string risk_cause { get; set; }

        [StringLength(4000)]
        public string risk_effect { get; set; }

        [StringLength(4000)]
        public string notes { get; set; }

        [Required]
        [StringLength(40)]
        public string risk_code { get; set; }

        [StringLength(4000)]
        public string risk_desc { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ICollection<TASKRISK> TASKRISK { get; set; }
    }
}
