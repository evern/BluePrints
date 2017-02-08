namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PROFILE_CONSTRAINT_VALUES
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CONSTRAINTID { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(1)]
        public string VALUETYPE { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(100)]
        public string CONSTRAINTVALUE { get; set; }

        [StringLength(255)]
        public string DISPLAYTEXT { get; set; }
    }
}