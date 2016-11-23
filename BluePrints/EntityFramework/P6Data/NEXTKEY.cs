namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("NEXTKEY")]
    public partial class NEXTKEY
    {
        [Key]
        [StringLength(30)]
        public string key_name { get; set; }

        public int key_seq_num { get; set; }
    }
}
