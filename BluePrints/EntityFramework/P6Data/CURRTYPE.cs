namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("CURRTYPE")]
    public partial class CURRTYPE
    {
        public CURRTYPE()
        {
            PREFER = new HashSet<PREFER>();
            RSRC = new HashSet<RSRC>();
            USERS = new HashSet<USERS>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int curr_id { get; set; }

        public int decimal_digit_cnt { get; set; }

        [Required]
        [StringLength(6)]
        public string curr_symbol { get; set; }

        [Required]
        [StringLength(6)]
        public string decimal_symbol { get; set; }

        [Required]
        [StringLength(6)]
        public string digit_group_symbol { get; set; }

        [Required]
        [StringLength(20)]
        public string pos_curr_fmt_type { get; set; }

        [Required]
        [StringLength(20)]
        public string neg_curr_fmt_type { get; set; }

        [Required]
        [StringLength(40)]
        public string curr_type { get; set; }

        [Required]
        [StringLength(6)]
        public string curr_short_name { get; set; }

        public int group_digit_cnt { get; set; }

        [Column(TypeName = "numeric")]
        public decimal base_exch_rate { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ICollection<PREFER> PREFER { get; set; }

        public virtual ICollection<RSRC> RSRC { get; set; }

        public virtual ICollection<USERS> USERS { get; set; }
    }
}