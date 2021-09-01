namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class X_EARNED_QUERY
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(100)]
        public string ProjectNumber { get; set; }

        [StringLength(200)]
        public string SubJobCode { get; set; }

        [StringLength(56)]
        public string DisciplineCode { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string CommodityCode { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(1)]
        public string VariationCode { get; set; }

        public double? RATE { get; set; }

        [Key]
        [Column(Order = 3)]
        public decimal EARNED_UNITS { get; set; }

        [Key]
        [Column(Order = 4)]
        public DateTime EARNED_DATE { get; set; }
    }
}
