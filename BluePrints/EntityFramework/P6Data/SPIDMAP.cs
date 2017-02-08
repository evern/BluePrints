namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SPIDMAP")]
    public partial class SPIDMAP
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int spid { get; set; }

        public int? user_id { get; set; }

        [StringLength(255)]
        public string user_name { get; set; }

        [StringLength(25)]
        public string app_name { get; set; }

        public int? refrdel_project_bypass { get; set; }

        [Column(TypeName = "text")]
        public string audit_info_extended { get; set; }
    }
}