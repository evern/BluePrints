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
        public Guid DummyId { get; set; }

        [Column(Order = 0)]
        [StringLength(100)]
        public string ProjectNumber { get; set; }

        [StringLength(200)]
        public string SubJobCode { get; set; }

        [StringLength(56)]
        public string DisciplineCode { get; set; }

        [StringLength(50)]
        public string CommodityCode { get; set; }

        [StringLength(100)]
        public string VariationCode { get; set; }

        public decimal? RATE { get; set; }

        public Guid GUID { get; set; }

        public Guid GUID_PROGRESS { get; set; }

        public Guid GUID_ORIBASEITEM { get; set; }

        public decimal EARNED_UNITS { get; set; }

        public DateTime EARNED_DATE { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public int? STAGE_ORDER { get; set; }

        [StringLength(200)]
        public string STAGE_NAME { get; set; }

        public decimal? STAGE_WEIGHT { get; set; }

        public decimal? BUDGET_INSTALL_HOURS_PER_QTY { get; set; }

        public decimal? EARNED_PERCENTAGE { get; set; }

        public decimal? TOTAL_QUANTITY { get; set; }

        public decimal? EARNED_QUANTITY { get; set; }

        public decimal? BUDGET_HOURS { get; set; }
    }
}
