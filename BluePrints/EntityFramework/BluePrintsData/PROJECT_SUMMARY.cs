namespace BluePrints.Data
{
    using Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PROJECT_SUMMARY")]
    public partial class PROJECT_SUMMARY
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PROJECT { get; set; }

        [Required]
        public StaticSummaryRowTypes PHASE_TYPE { get; set; }

        public decimal? ORI_CONTRACT { get; set; }

        public decimal? BUDGET_UNITS { get; set; }

        public decimal? FORECAST_UNITS { get; set; }

        public decimal? EARNED_UNITS { get; set; }

        public decimal? PLANNED_UNITS { get; set; }

        public decimal? APPROVED_VAR { get; set; }

        public decimal? UNAPPROVED_VAR { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }
    }
}
