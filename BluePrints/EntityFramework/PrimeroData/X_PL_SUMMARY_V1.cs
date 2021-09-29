namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class X_PL_SUMMARY_V1
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int JOBNO { get; set; }

        [StringLength(15)]
        public string JOBCODE { get; set; }

        [StringLength(60)]
        public string TITLE { get; set; }

        public Guid? PROJECT_GUID { get; set; }

        [StringLength(100)]
        public string NUMBER { get; set; }

        public decimal? ORI_REVENUE { get; set; }

        public decimal? VAR_REVENUE { get; set; }

        public decimal? UNAPPROVED_VAR_REVENUE { get; set; }

        public double? TotalTimeCosts { get; set; }

        public double? TotalMaterialCosts { get; set; }

        public double? TotalInvoiced { get; set; }

        public double? TotalOutstanding { get; set; }

        public double? TotalCosts { get; set; }
    }
}