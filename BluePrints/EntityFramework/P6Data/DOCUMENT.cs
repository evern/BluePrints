namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DOCUMENT")]
    public partial class DOCUMENT
    {
        public DOCUMENT()
        {
            DOCREVIEW = new HashSet<DOCREVIEW>();
            TASKDOC = new HashSet<TASKDOC>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int doc_id { get; set; }

        public int doc_seq_num { get; set; }

        [Required]
        [StringLength(1)]
        public string deliv_flag { get; set; }

        [Required]
        [StringLength(120)]
        public string doc_name { get; set; }

        public int? parent_doc_id { get; set; }

        public int? proj_id { get; set; }

        public int? doc_status_id { get; set; }

        public int? doc_catg_id { get; set; }

        public DateTime? doc_date { get; set; }

        [Required]
        [StringLength(20)]
        public string version_name { get; set; }

        [StringLength(22)]
        public string guid { get; set; }

        [StringLength(22)]
        public string tmpl_guid { get; set; }

        [StringLength(32)]
        public string doc_short_name { get; set; }

        [StringLength(255)]
        public string author_name { get; set; }

        [StringLength(255)]
        public string private_loc { get; set; }

        [StringLength(255)]
        public string public_loc { get; set; }

        [Column(TypeName = "text")]
        public string doc_content { get; set; }

        public int? rsrc_id { get; set; }

        [Required]
        [StringLength(10)]
        public string doc_mgmt_type { get; set; }

        [StringLength(4000)]
        public string external_doc_key { get; set; }

        [StringLength(4000)]
        public string cr_external_doc_key { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual DOCCATG DOCCATG { get; set; }

        public virtual ICollection<DOCREVIEW> DOCREVIEW { get; set; }

        public virtual DOCSTAT DOCSTAT { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        public virtual RSRC RSRC { get; set; }

        public virtual ICollection<TASKDOC> TASKDOC { get; set; }
    }
}