namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UDFTYPE")]
    public partial class UDFTYPE
    {
        public UDFTYPE()
        {
            UDFCODE = new HashSet<UDFCODE>();
            UDFVALUE = new HashSet<UDFVALUE>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int udf_type_id { get; set; }

        [Required]
        [StringLength(30)]
        public string table_name { get; set; }

        [Required]
        [StringLength(32)]
        public string udf_type_name { get; set; }

        [Required]
        [StringLength(40)]
        public string udf_type_label { get; set; }

        [Required]
        [StringLength(20)]
        public string logical_data_type { get; set; }

        [Required]
        [StringLength(1)]
        public string super_flag { get; set; }

        public int? udf_code_short_len { get; set; }

        [StringLength(4000)]
        public string formula { get; set; }

        [StringLength(4000)]
        public string indicator_expression { get; set; }

        [StringLength(1)]
        public string disp_data_flag { get; set; }

        [StringLength(1)]
        public string disp_indicator_flag { get; set; }

        [StringLength(4000)]
        public string summary_indicator_expression { get; set; }

        [StringLength(20)]
        public string summary_method { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ICollection<UDFCODE> UDFCODE { get; set; }

        public virtual ICollection<UDFVALUE> UDFVALUE { get; set; }
    }
}