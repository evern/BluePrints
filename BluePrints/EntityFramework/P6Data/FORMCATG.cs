namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("FORMCATG")]
    public partial class FORMCATG
    {
        public FORMCATG()
        {
            FORMTMPL = new HashSet<FORMTMPL>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int form_catg_id { get; set; }

        [Required]
        [StringLength(255)]
        public string form_catg_name { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ICollection<FORMTMPL> FORMTMPL { get; set; }
    }
}