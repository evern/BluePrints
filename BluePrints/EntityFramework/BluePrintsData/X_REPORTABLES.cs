namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class X_REPORTABLES
    {
        public string SubJobCode { get; set; }

        public string DisciplineCode { get; set; }

        public string CommodityCode { get; set; }

        public string VariationCode { get; set; }

        [Key]
        [Column(Order = 0)]
        public double? BUDGET_UNITS { get; set; }

        [Key]
        [Column(Order = 1)]
        public double? TOTAL_UNITS { get; set; }

        [Key]
        [Column(Order = 2)]
        public double? RATE { get; set; }

        [Key]
        [Column(Order = 3)]
        public double? BUDGET_COSTS { get; set; }

        [Key]
        [Column(Order = 4)]
        public double? TOTAL_COSTS { get; set; }
    }
}
