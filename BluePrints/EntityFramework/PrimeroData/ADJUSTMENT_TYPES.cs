namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ADJUSTMENT_TYPES
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ATNO { get; set; }

        [StringLength(12)]
        public string ATDESC { get; set; }
    }
}