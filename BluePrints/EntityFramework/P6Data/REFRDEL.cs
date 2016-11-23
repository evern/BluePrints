namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("REFRDEL")]
    public partial class REFRDEL
    {
        [Key]
        [Column(Order = 0)]
        public DateTime delete_date { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(30)]
        public string table_name { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(255)]
        public string pk1 { get; set; }

        [StringLength(255)]
        public string pk2 { get; set; }

        [StringLength(255)]
        public string pk3 { get; set; }

        [StringLength(255)]
        public string pk4 { get; set; }

        public int? proj_id { get; set; }
    }
}
