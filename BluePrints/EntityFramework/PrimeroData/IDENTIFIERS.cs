namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class IDENTIFIERS
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(50)]
        public string TABLENAME { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(50)]
        public string FIELDNAME { get; set; }

        public int ID { get; set; }
    }
}