namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PUBUSER")]
    public partial class PUBUSER
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int public_group_id { get; set; }

        [Required]
        [StringLength(255)]
        public string private_db_user_name { get; set; }

        [StringLength(255)]
        public string private_db_passwd { get; set; }
    }
}