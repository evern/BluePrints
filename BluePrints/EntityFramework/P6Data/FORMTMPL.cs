namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FORMTMPL")]
    public partial class FORMTMPL
    {
        public FORMTMPL()
        {
            FORMPROJ = new HashSet<FORMPROJ>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int form_tmpl_id { get; set; }

        [Required]
        [StringLength(255)]
        public string form_tmpl_name { get; set; }

        [StringLength(4000)]
        public string form_tmpl_desc { get; set; }

        public int? form_catg_id { get; set; }

        [Column(TypeName = "text")]
        public string form_tmpl_data { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual FORMCATG FORMCATG { get; set; }

        public virtual ICollection<FORMPROJ> FORMPROJ { get; set; }
    }
}
