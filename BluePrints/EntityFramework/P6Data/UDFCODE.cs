namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UDFCODE")]
    public partial class UDFCODE
    {
        public UDFCODE()
        {
            UDFVALUE = new HashSet<UDFVALUE>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int udf_code_id { get; set; }

        public int udf_type_id { get; set; }

        public int seq_num { get; set; }

        [Required]
        [StringLength(60)]
        public string short_name { get; set; }

        [Required]
        [StringLength(120)]
        public string udf_code_name { get; set; }

        public int? parent_udf_code_id { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual UDFTYPE UDFTYPE { get; set; }

        public virtual ICollection<UDFVALUE> UDFVALUE { get; set; }
    }
}