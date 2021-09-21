namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class X_PL_SUMMARY
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        [StringLength(15)]
        public string JOBCODE { get; set; }

        [StringLength(100)]
        public string NUMBER { get; set; }

        public decimal? ApprovedRevenue { get; set; }

        public decimal? Submitted { get; set; }

        public decimal? Unapproved { get; set; }

        public decimal? ForecastRevenue { get; set; }

        public double? TotalTimeCosts { get; set; }

        public double? TotalMaterialCost { get; set; }

        public double? TotalInvoiced { get; set; }

        public double? TotalOutstanding { get; set; }
    }
}