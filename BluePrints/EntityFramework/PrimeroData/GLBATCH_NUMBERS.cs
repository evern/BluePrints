namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GLBATCH_NUMBERS
    {
        [Key]
        [Column(Order = 0)]
        public int BATCHNO { get; set; }

        [Key]
        [Column(Order = 1)]
        public DateTime DATE_ISSUED { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(30)]
        public string USERID { get; set; }
    }
}