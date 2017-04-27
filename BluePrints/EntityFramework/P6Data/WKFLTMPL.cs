namespace BluePrints.P6Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("WKFLTMPL")]
    public partial class WKFLTMPL
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int wkfl_tmpl_id { get; set; }

        [Required]
        [StringLength(255)]
        public string template_name { get; set; }

        [Required]
        [StringLength(1)]
        public string project_flag { get; set; }

        [StringLength(255)]
        public string wk_external_key { get; set; }

        public int? tmplcatg_catg_id { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual TMPLCATG TMPLCATG { get; set; }
    }
}