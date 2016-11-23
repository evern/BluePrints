namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UDFVALUE")]
    public partial class UDFVALUE
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int udf_type_id { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int fk_id { get; set; }

        public int? proj_id { get; set; }

        public DateTime? udf_date { get; set; }

        [StringLength(255)]
        public string udf_text { get; set; }

        public decimal? udf_number { get; set; }

        public int? udf_code_id { get; set; }

        [StringLength(30)]
        public string table_name { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        public virtual UDFCODE UDFCODE { get; set; }

        public virtual UDFTYPE UDFTYPE { get; set; }
    }
}
