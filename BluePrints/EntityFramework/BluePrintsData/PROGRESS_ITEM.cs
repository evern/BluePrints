namespace BluePrints.Data
{
    using BluePrints.Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PROGRESS_ITEM
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PROGRESS { get; set; }

        public Guid GUID_ORIBASEITEM { get; set; }

        public decimal EARNED_UNITS { get; set; }

        public DateTime EARNED_DATE { get; set; }

        [StringLength(200)]
        public string STAGE_NAME { get; set; }

        public int? STAGE_ORDER { get; set; }

        public decimal? STAGE_WEIGHT { get; set; }

        public decimal? BUDGET_INSTALL_HOURS_PER_QTY { get; set; }

        public decimal? EARNED_PERCENTAGE { get; set; }

        public decimal? TOTAL_QUANTITY { get; set; }

        public decimal? EARNED_QUANTITY { get; set; }

        public decimal? BUDGET_HOURS { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual PROGRESS PROGRESS { get; set; }
    }
}
