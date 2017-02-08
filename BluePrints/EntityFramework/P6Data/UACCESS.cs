namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UACCESS")]
    public partial class UACCESS
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int user_id { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int proj_id { get; set; }

        [Required]
        [StringLength(1)]
        public string access_flag { get; set; }

        public int? wbs_id { get; set; }

        public int? parent_wbs_id { get; set; }

        public virtual USERS USERS { get; set; }
    }
}