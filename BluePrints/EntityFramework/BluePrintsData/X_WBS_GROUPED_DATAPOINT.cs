namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class X_WBS_GROUPED_DATAPOINT
    {
        public string SubJobCode { get; set; }

        public string DisciplineCode { get; set; }

        public string CommodityCode { get; set; }

        public string VariationCode { get; set; }

        public DateTime UniversalPeriodEndDate { get; set; }

        public double TotalUnits { get; set; }

        public double TotalCosts { get; set; }
    }
}
